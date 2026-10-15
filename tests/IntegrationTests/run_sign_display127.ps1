param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$RuntimePath,[switch]$ExpectMissingRegistration)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$version\display127-"+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $out | Out-Null
$BinDir=[IO.Path]::GetFullPath($BinDir);$RuntimePath=[IO.Path]::GetFullPath($RuntimePath)
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll" -Destination $out
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 "/out:$out\SignDisplay127Probe.dll" '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$out\BHT.Core.dll" "/r:$out\BHT.Bridge.dll" "$PSScriptRoot\SignDisplay127Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Display probe compile failed'}
function DisplayPath([string]$path){$path.Replace('\','/')}
$moduleRoot=if([IO.Path]::GetExtension($RuntimePath) -eq '.lsp'){'"'+(DisplayPath (Split-Path -Parent $RuntimePath))+'"'}else{'nil'}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(DisplayPath "$out\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* '+$moduleRoot+')'),('(load "'+(DisplayPath $RuntimePath)+'")'),
 ('(load "'+(DisplayPath "$PSScriptRoot\test_SCALE652.lsp")+'")'),
 '_.NETLOAD',('"'+(DisplayPath "$out\SignDisplay127Probe.dll")+'"'),'BHTSIGNDISPLAY127','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\test.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /p BHTResearch111 /isolate BHTDisplay127 "$out\profile" /s "$out\test.scr" /l en-US > "$out\console.log"
$log=[IO.File]::ReadAllText("$out\console.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) DISPLAY127|^DISPLAY127-'} | Write-Output
if($ExpectMissingRegistration){
 if($log -notmatch 'FAIL DISPLAY127 invoke-PARALLEL' -or $log -notmatch 'eInvalidInput'){throw 'Expected registration defect not reproduced'}
 Write-Output "REPRODUCED missing registration: $out"
}elseif($log -notmatch 'DISPLAY127-CHECKS=9 FAIL=0' -or $log -match '; error:|(?m)^FAIL DISPLAY127'){
 throw "Display API regression failed: $out"
}else{Write-Output "PASS DISPLAY127: $out"}
