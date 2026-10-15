param([string]$BinDir,[string]$RuntimePath,[string]$Tag)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out="$root\build\v1.0.0\qa\$Tag"
New-Item -ItemType Directory -Force $out | Out-Null
$env:BHT_V1_QA=$out.Replace('\','/')
$lines=@('(setvar "LISPSYS" 1)','(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+("$BinDir\BHT.Bridge.dll").Replace('\','/')+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+("$root\src\lisp").Replace('\','/')+'")'),('(load "'+$RuntimePath.Replace('\','/')+'")'),('(load "'+("$PSScriptRoot\test_V1_SCALE.lsp").Replace('\','/')+'")'),'_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\scale.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate "BHTV1scale$Tag" "$out\scale-profile" /s "$out\scale.scr" /l en-US > "$out\scale-console.log"
$log=Get-Content "$out\scale-console.log" -Encoding Unicode -Raw
if($log -notmatch 'V1-SCALE-COMPLETE' -or $log -match '; error:'){throw 'Scale test did not complete'}
$report=Get-Content "$out\scale-perf.txt" -Raw
$report
if($report -notmatch 'FAIL=0'){throw 'Station accuracy failed'}
