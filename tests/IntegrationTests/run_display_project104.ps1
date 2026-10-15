param([string]$BinDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop';$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v\qa\display104";New-Item -ItemType Directory -Force $out | Out-Null
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library "/r:D:\AutoCAD 2024\acdbmgd.dll" "/r:D:\AutoCAD 2024\accoremgd.dll" "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "/out:$BinDir\Project104Probe.dll" "$PSScriptRoot\Project104Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Probe compile failed'}
function LP($p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'),('(load "'+(LP "$PSScriptRoot\test_DISPLAY104.lsp")+'")'),'_.NETLOAD',('"'+(LP "$BinDir\Project104Probe.dll")+'"'),'BHTPROJECT104PROBE','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\run.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTDisplay104 "$out\profile" /s "$out\run.scr" /l en-US > "$out\run.log"
$log=Get-Content "$out\run.log" -Encoding Unicode -Raw;$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) DISPLAY104|^DISPLAY104-CHECKS='} | Write-Output
if($log -match '; error:|(?m)^FAIL DISPLAY104' -or $log -notmatch 'DISPLAY104-CHECKS=\d+ FAIL=0'){throw 'Display test failed'}
$result=Get-Content "$BinDir\project104\result.txt";$result | Write-Output;if($result -match '^FAIL'){throw 'Project transfer failed'}
