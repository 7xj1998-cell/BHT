param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$RuntimePath,[string]$Tag='after')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content (Join-Path $root 'VERSION') -Raw).Trim()
$out="$root\build\v$version\qa\fill-$Tag"
New-Item -ItemType Directory -Force $out | Out-Null
$inputFile='C:\Users\Le Bao\Desktop\Lisp đang build\BHT\Ho so\261011_test.dwg'
$hash=(Get-FileHash -LiteralPath $inputFile).Hash
$drawing="$out\261011_test-fill.dwg"
Copy-Item -LiteralPath $inputFile -Destination $drawing -Force
$env:BHT101_REPORT="$out\fill.txt"
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+"$BinDir\BHT.Bridge.dll".Replace('\','/')+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+"$root\src\lisp".Replace('\','/')+'")'),('(load "'+$RuntimePath.Replace('\','/')+'")'),('(load "'+"$PSScriptRoot\test_V101_FILL.lsp".Replace('\','/')+'")'),'_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\fill.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate "BHT101Fill$Tag" "$out\profile" /i $drawing /s "$out\fill.scr" /l en-US > "$out\console.log"
if($hash -ne (Get-FileHash -LiteralPath $inputFile).Hash){throw 'Original drawing hash changed'}
if(!(Test-Path "$out\fill.txt")){throw 'Fill probe did not finish'}
$report=Get-Content "$out\fill.txt" -Raw
Write-Output $report
if($report -match '(?m)^FAIL'){throw 'Fill regression failed'}
