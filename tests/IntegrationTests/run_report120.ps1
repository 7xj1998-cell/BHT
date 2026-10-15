param([Parameter(Mandatory=$true)][string]$BinDir,[string[]]$InputDwgs=@(),[string]$ReportDir='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$version\report120-"+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $out | Out-Null
if(!$ReportDir){$ReportDir=Join-Path $out 'reports'}
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll" -Destination $out
$csc="$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /target:library /platform:x64 /langversion:5 "/out:$out\Report120Probe.dll" '/r:System.Xml.Linq.dll' '/r:System.IO.Compression.dll' '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$out\BHT.Core.dll" "/r:$out\BHT.Bridge.dll" "$PSScriptRoot\Report120Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Report120 probe build failed'}
$hashes=@{};foreach($file in $InputDwgs){$hashes[$file]=(Get-FileHash -LiteralPath $file).Hash}
$env:BHT_REPORT_OUTPUT=$ReportDir; $env:BHT_REPORT_INPUTS=$InputDwgs -join '|'
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+("$out\BHT.Bridge.dll").Replace('\','/')+'"'),'_.NETLOAD',('"'+("$out\Report120Probe.dll").Replace('\','/')+'"'),'REPORT120','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\report.scr",$boot,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTReport120 "$out\profile" /s "$out\report.scr" /l en-US > "$out\report.log"
$log=[IO.File]::ReadAllText("$out\report.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS REPORT120|REPORT120-)'} | Write-Output
if($log -match 'REPORT120-FAIL|; error:' -or $log -notmatch 'REPORT120-PASS CHECKS=\d+'){throw "Report120 failed: $out"}
foreach($file in $InputDwgs){if((Get-FileHash -LiteralPath $file).Hash -ne $hashes[$file]){throw "Input DWG changed: $file"}}
Write-Output "PASS REPORT120 source DWG SHA256 unchanged; reports=$ReportDir; audit=$out"
