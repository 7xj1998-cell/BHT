param([string]$BinDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop';$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v\qa\road106"
New-Item -ItemType Directory -Force $out | Out-Null
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
function LP($p){$p.Replace('\','/')}
$dwg="$out\Bo-tri-ngoai-duong-"+[guid]::NewGuid().ToString('N')+'.dwg'
$boot=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@(('(load "'+(LP "$PSScriptRoot\test_ROAD106.lsp")+'")'),
 '_.UNDO','_End','_.UNDO','_Auto','_Off',
 '(setq r106-before-undo (r106-snapshot) r106-old-picker (eval ''bht:kh-road-pick))',
 '(defun bht:kh-road-pick () (list r106-road -1))',
 '(bht:api-symbol-road "3" "8" "5" "THANG")',
 '(set ''bht:kh-road-pick r106-old-picker)',
 '(princ (strcat "\nROAD106-UNDOCTL=" (itoa (getvar "UNDOCTL"))))','_.U',
 '(princ "\nROAD106-AFTER-U")',
 '(princ (if (equal r106-before-undo (r106-snapshot)) "\nROAD106-UNDO-PASS" "\nROAD106-UNDO-FAIL"))',
 '_.QSAVE',('"'+(LP $dwg)+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTRoad106 "$out\profile" /s "$out\create.scr" /l en-US > "$out\create.log"
$log=Get-Content "$out\create.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) ROAD106|^ROAD106'} | Write-Output
if($log -match '; error:|(?m)^FAIL ROAD106' -or $log -notmatch '(?m)^ROAD106-CHECKS=\d+ FAIL=0' -or $log -notmatch '(?m)^ROAD106-UNDO-PASS'){throw 'Road display tests failed'}
$steps=@('(princ (if (and (= "ROAD" (bht:meta "symbol_layout_method" "")) (handent (bht:meta "symbol_road_handle" "")) (equal 8.0 (bht:num (bht:meta "symbol_road_left" "")) 1e-8) (= "1" (bht:meta "symbol_road_direction" "")) (= "GAP_KHUC" (bht:meta "symbol_leader_mode" ""))) "\nROAD106-REOPEN-PASS" "\nROAD106-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTRoad106 "$out\profile" /i $dwg /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=Get-Content "$out\reopen.log" -Encoding Unicode -Raw
if($log -notmatch '(?m)^ROAD106-REOPEN-PASS' -or $log -match '; error:'){throw 'Road display reopen failed'}
Write-Output "PASS ROAD106 save/reopen; DWG=$dwg"
