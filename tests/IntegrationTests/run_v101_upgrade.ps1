param([string]$BinDir,[string]$RuntimePath)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out="$root\build\v1.0.1\qa"
$source='C:\Users\Le Bao\Desktop\Lisp đang build\BHT\Ho so\261011_test.dwg'
$hash=(Get-FileHash -LiteralPath $source).Hash
$drawing="$out\261011_test-BHT-1.0.1.dwg"
Copy-Item -LiteralPath $source -Destination $drawing -Force
$env:BHT101_UPGRADE_REPORT="$out\upgrade.txt"
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+"$BinDir\BHT.Bridge.dll".Replace('\','/')+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+"$root\src\lisp".Replace('\','/')+'")'),('(load "'+$RuntimePath.Replace('\','/')+'")'),('(load "'+"$PSScriptRoot\test_V101_UPGRADE.lsp".Replace('\','/')+'")'),'_.QSAVE','_.QUIT')
[IO.File]::WriteAllLines("$out\upgrade.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHT101Upgrade "$out\upgrade-profile" /i $drawing /s "$out\upgrade.scr" /l en-US > "$out\upgrade.log"
if($hash -ne (Get-FileHash -LiteralPath $source).Hash){throw 'Original drawing changed'}
$report=Get-Content "$out\upgrade.txt" -Raw;Write-Output $report
if($report -match '(?m)^FAIL' -or $report -notmatch 'PASS upgraded sample fill on'){throw 'Upgrade regression failed'}
