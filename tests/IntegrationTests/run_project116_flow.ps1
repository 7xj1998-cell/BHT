param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$PackagePath,[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
$out=Join-Path $root ('build\v'+$v+'\project116-flow-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $out | Out-Null
function LispPath([string]$path){$path.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LispPath "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LispPath "$root\src\lisp")+'")'),('(load "'+(LispPath $RuntimePath)+'")'))
$steps=@('(setvar "INSUNITS" 0)','_.UNDO','_End','_.UNDO','_Auto','_Off',
 'BHTNAPDUAN',('"'+(LispPath $PackagePath)+'"'),
 '(princ (if (and (= 801 (length (bht:pt-all))) (= 179 (length (bht:rec-all "OBJ"))) (= 4 (getvar "INSUNITS"))) "\nPASS PROJECT116FLOW import-and-units" "\nFAIL PROJECT116FLOW import-and-units"))',
 '_.U',
 '(princ (if (and (null (bht:pt-all)) (null (bht:rec-all "OBJ")) (= 0 (getvar "INSUNITS"))) "\nPASS PROJECT116FLOW one-undo" "\nFAIL PROJECT116FLOW one-undo"))',
 '(setq unrelated116 (entmakex ''((0 . "LINE") (10 1.0 2.0 0.0) (11 3.0 4.0 0.0))))',
 'BHTNAPDUAN',('"'+(LispPath $PackagePath)+'"'),'Huy',
 '(princ (if (and (null (bht:pt-all)) (entget unrelated116) (= 0 (getvar "INSUNITS"))) "\nPASS PROJECT116FLOW cancel-unchanged" "\nFAIL PROJECT116FLOW cancel-unchanged"))',
 'BHTNAPDUAN',('"'+(LispPath $PackagePath)+'"'),'Giunguyen',
 '(princ (if (and (= 801 (length (bht:pt-all))) (= 179 (length (bht:rec-all "OBJ"))) (entget unrelated116) (= 0 (getvar "INSUNITS"))) "\nPASS PROJECT116FLOW explicit-coordinate-import" "\nFAIL PROJECT116FLOW explicit-coordinate-import"))',
 '(setq before116 (bht:pt-all) symbols116 (length (bht:tagged-pairs "INSERT" "BHT_KH" 1)))',
 '(bht:symbol-sync nil)',
 '(princ (if (and (equal before116 (bht:pt-all)) (= symbols116 (length (bht:tagged-pairs "INSERT" "BHT_KH" 1))) (= 179 (length (bht:rec-all "OBJ")))) "\nPASS PROJECT116FLOW symbol-update-preserves-data" "\nFAIL PROJECT116FLOW symbol-update-preserves-data"))',
 'BHTXUATDUAN',('"'+(LispPath "$out\exported.bht")+'"'),
 '(princ (if (findfile "'+(LispPath "$out\exported.bht")+'") "\nPASS PROJECT116FLOW export-command" "\nFAIL PROJECT116FLOW export-command"))',
 '_.QSAVE',('"'+(LispPath "$out\roundtrip.dwg")+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\run.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTProject116Flow "$out\profile" /s "$out\run.scr" /l en-US > "$out\run.log"
$text=[IO.File]::ReadAllText("$out\run.log",[Text.Encoding]::Unicode)
$text -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) PROJECT116FLOW'} | Write-Output
if($text -match '; error:|(?m)^FAIL PROJECT116FLOW' -or ([regex]::Matches($text,'(?m)^PASS PROJECT116FLOW').Count -ne 6)){throw "Project flow failed: $out"}
$reopen=@('(princ (if (and (= 801 (length (bht:pt-all))) (= 179 (length (bht:rec-all "OBJ")))) "\nPASS PROJECT116FLOW reopen" "\nFAIL PROJECT116FLOW reopen"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$reopen),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTProject116Flow "$out\profile" /i "$out\roundtrip.dwg" /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$text=[IO.File]::ReadAllText("$out\reopen.log",[Text.Encoding]::Unicode)
if($text -match '; error:|(?m)^FAIL PROJECT116FLOW' -or $text -notmatch '(?m)^PASS PROJECT116FLOW reopen'){throw "Reopen failed: $out"}
Write-Output "PASS PROJECT116FLOW reopen: $out"
