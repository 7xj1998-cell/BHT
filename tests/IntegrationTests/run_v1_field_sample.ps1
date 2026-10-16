param([string]$BinDir,[string]$RuntimePath,[string]$Tag='after',[switch]$Save,[string]$Drawing='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out="$root\build\v1.0.0\qa\$Tag"
New-Item -ItemType Directory -Force $out | Out-Null
if(!$Drawing){$Drawing="$root\build\v1.0.0\qa\261011_test-V1.dwg"}
$env:BHT_V1_QA=$out.Replace('\','/')
$lines=@('(setvar "LISPSYS" 1)','(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+("$BinDir\BHT.Bridge.dll").Replace('\','/')+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+("$root\src\lisp").Replace('\','/')+'")'),('(load "'+$RuntimePath.Replace('\','/')+'")'),('(load "'+("$PSScriptRoot\test_V1_SAMPLE.lsp").Replace('\','/')+'")'))
if($Save){$lines+=@('_.QSAVE')}
$lines+=@('_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\field.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate "BHTV1field$Tag" "$out\field-profile" /i $Drawing /s "$out\field.scr" /l en-US > "$out\field-console.log"
$log=Get-Content "$out\field-console.log" -Encoding Unicode -Raw
if($log -notmatch 'V1-SAMPLE-COMPLETE' -or $log -match '; error:'){throw 'Field sample failed'}
Get-Content "$out\field-perf.txt"
Get-Content "$out\sample-check.txt" -TotalCount 4


