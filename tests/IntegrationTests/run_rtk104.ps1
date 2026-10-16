param([string]$BinDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v\rtk104"
New-Item -ItemType Directory -Force $out | Out-Null
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
function LP($p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$dwg="$out\RTK-xoay-nhan-"+[guid]::NewGuid().ToString('N')+'.dwg'
$steps=@(('(load "'+(LP "$PSScriptRoot\test_RTK104.lsp")+'")'),
 '_.UNDO','_End','_.UNDO','_Auto','_Off',
 '(setq qa-before-undo (r104-snapshot))',
 '(bht:api-label-rotate "UCS")',
 '_.U',
 '(r104-check "api-one-undo-restores-all-labels" (equal qa-before-undo (r104-snapshot) 1e-9))',
 'BHTXOAYNHAN','Huong','0,0','10,0',
 '(r104-check "picked-direction-converted-from-UCS" (r104-angle-p (/ pi 6.0)))',
 '_.U',
 '(r104-check "command-one-undo-restores-all-labels" (equal qa-before-undo (r104-snapshot) 1e-9))',
 'BHTXOAYNHAN','Huong','0,0','10,0',
 '(setq qa-before-cancel (r104-snapshot))','(bht:api-label-rotate "PICK")','',
 '(r104-check "cancel-before-A-preserves-all" (equal qa-before-cancel (r104-snapshot)))',
 '(bht:api-label-rotate "PICK")','0,0','',
 '(r104-check "cancel-before-B-preserves-all" (equal qa-before-cancel (r104-snapshot)))',
 '(bht:api-label-rotate "PICK")','0,0','0,0',
 '(r104-check "zero-direction-preserves-all" (equal qa-before-cancel (r104-snapshot)))',
 '(r104-check "points-still-unchanged" (equal qa-points (bht:pt-all)))',
 '(princ (strcat "\nRTK104-CHECKS=" (itoa qa-checks) " FAIL=" (itoa qa-fails)))',
 '_.QSAVE',('"'+(LP $dwg)+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTRTK104 "$out\profile" /s "$out\create.scr" /l en-US > "$out\create.log"
$log=Get-Content "$out\create.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) RTK104|^RTK104-CHECKS='} | Write-Output
if($log -notmatch 'RTK104-CHECKS=\d+ FAIL=0' -or $log -match '(?m)^FAIL RTK104|; error:'){throw 'RTK rotation create failed'}
$check=@('(setq labels (mapcar ''cdr (bht:tagged-pairs "TEXT" "BHT_NHAN" 1)))','(princ (if (and (= (length labels) 8) (vl-every ''(lambda (e) (and (equal (cdr (assoc 50 (entget e))) (/ pi 6.0) 1e-8) (= (bht:lbl-state e) ''TAY))) labels)) "\nRTK104-REOPEN-PASS" "\nRTK104-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$check),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTRTK104 "$out\profile" /i $dwg /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=Get-Content "$out\reopen.log" -Encoding Unicode -Raw
if($log -notmatch '(?m)^RTK104-REOPEN-PASS' -or $log -match '; error:'){throw 'RTK reopen failed'}
Write-Output 'PASS RTK104 save/reopen'
