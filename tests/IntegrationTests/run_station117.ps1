param([Parameter(Mandatory=$true)][string]$BinDir,[string]$RuntimePath='',[string]$AcadProfile='')
$profileArgs=@();if($AcadProfile){$profileArgs=@('/p',$AcadProfile)}
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$v\station117-"+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $out | Out-Null
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
function TestLispPath([string]$p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(TestLispPath "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(TestLispPath "$root\src\lisp")+'")'),('(load "'+(TestLispPath $RuntimePath)+'")'))
$steps=@(('(load "'+(TestLispPath "$PSScriptRoot\test_STATION117.lsp")+'")'),'_.UNDO','_End','_.UNDO','_Auto','_Off',
 '(setq *bht-route-selected* "NEW-TDT")','BHTTINHTHEOTUYEN','K',
 '(s117-check "cancel-no-mutation" (equal s117-final (s117-snapshot)))',
 '(bht:meta-set "station_method" "FIELD_MARKERS")','(bht:meta-set "station_active_route" "OLD")','(setq s117-undo-before (s117-snapshot) *bht-route-selected* "NEW-TDT")',
 '(if (/= 0 (logand 8 (getvar "UNDOCTL"))) (command-s "_.UNDO" "_End"))','BHTTINHTHEOTUYEN','C','_.U',
 '(s117-check "one-undo-restores-method-and-derived-stations" (equal s117-undo-before (s117-snapshot)))',
 '(setq *bht-route-selected* "NEW-TDT")','BHTTINHTHEOTUYEN','C',
 '(princ (strcat "\nSTATION117-FINAL-CHECKS=" (itoa s117-checks) " FAIL=" (itoa s117-fails)))',
 '_.QSAVE',('"'+(TestLispPath "$out\roundtrip.dwg")+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' @profileArgs /isolate BHTStation117 "$out\profile" /s "$out\create.scr" /l en-US > "$out\create.log"
$log=[IO.File]::ReadAllText("$out\create.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) STATION117|^STATION117-'} | Write-Output
if($log -match '; error:|(?m)^FAIL STATION117' -or $log -notmatch 'STATION117-FINAL-CHECKS=\d+ FAIL=0'){throw "Station117 failed: $out"}
$reopen=@('(princ (if (and (= "NEW-TDT" (bht:meta "station_active_route" "")) (= "ROUTE" (bht:meta "station_method" "")) (= 3 (length (car (bht:route-object-controls (bht:route-read "NEW-TDT")))))) "\nPASS STATION117 reopen-active-route-and-source" "\nFAIL STATION117 reopen-active-route-and-source"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$reopen),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' @profileArgs /isolate BHTStation117 "$out\profile" /i "$out\roundtrip.dwg" /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=[IO.File]::ReadAllText("$out\reopen.log",[Text.Encoding]::Unicode)
if($log -match '; error:|(?m)^FAIL STATION117' -or $log -notmatch '(?m)^PASS STATION117 reopen'){throw "Station117 reopen failed: $out"}
Write-Output "PASS STATION117 reopen: $out"
