param([string]$RuntimePath='', [switch]$Legacy)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
$out=Join-Path $root ("build\v$v\review113-"+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $out | Out-Null
$lines=@('(setvar "SECURELOAD" 0)',('(setq *bht-no-launch* T *bht-module-root* "'+"$root\src\lisp".Replace('\','/')+'")'),('(load "'+$RuntimePath.Replace('\','/')+'")'),('(load "'+"$PSScriptRoot\test_REVIEW113.lsp".Replace('\','/')+'")'),'_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\test.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTReview113 "$out\profile" /s "$out\test.scr" /l en-US > "$out\console.log"
$result=[IO.File]::ReadAllText("$out\console.log",[Text.Encoding]::Unicode)
$result -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) REVIEW113|^REVIEW113-FAIL='}
if($Legacy){if($result -notmatch 'FAIL REVIEW113 duplicate-controls'){throw 'Duplicate control defect not reproduced'}}
elseif($result -notmatch 'REVIEW113-FAIL=0' -or $result -match '(?m)^FAIL REVIEW113|; error:'){throw "Review regression failed: $out"}
Write-Host "REPORT: $out"
