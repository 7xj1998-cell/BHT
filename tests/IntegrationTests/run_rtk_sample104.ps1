param([string]$BinDir,[string]$RuntimePath='', [string]$SamplePath, [string]$OutputName='rtk104-sample')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim(); $out="$root\build\v$v\qa\$OutputName"
New-Item -ItemType Directory -Force $out | Out-Null
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
function LP($p){$p.Replace('\','/')}
$dwg="$out\RTK-BOT830-"+[guid]::NewGuid().ToString('N')+'.dwg'
Copy-Item -LiteralPath $SamplePath -Destination $dwg
$boot=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@(('(load "'+(LP "$PSScriptRoot\test_RTK104_SAMPLE.lsp")+'")'),
 '_.UCS','_World','_.UCS','_Z','30','_.UNDO','_End','_.UNDO','_Auto','_Off',
 '(setq qa-ms (getvar "MILLISECS"))',
 '(bht:api-label-rotate "UCS")',
 '(princ (strcat "\nSAMPLE104 rotation_ms=" (itoa (- (getvar "MILLISECS") qa-ms))))',
 '(s104-check "all-labels-30-degrees" (vl-every ''(lambda (e) (equal (cdr (assoc 50 (entget e))) (/ pi 6.0) 1e-8)) (s104-labels)))',
 '(s104-check "points-records-symbols-preserved" (and (equal qa-pts (bht:pt-all)) (equal qa-objs (bht:rec-all "OBJ")) (equal qa-symbols (s104-symbols))))',
 '(s104-check "content-font-height-preserved" (equal qa-content (s104-content)))',
 '_.U',
 '(s104-check "one-undo-restores-all-labels" (equal qa-labels (s104-snapshot) 1e-8))',
 'BHTXOAYNHAN','Huong','0,0','10,0',
 '(s104-check "picked-direction-all-labels" (vl-every ''(lambda (e) (equal (cdr (assoc 50 (entget e))) (/ pi 6.0) 1e-8)) (s104-labels)))',
 '(setq qa-rotated (s104-snapshot))','(bht:api-label-sync "ALL")',
 '(setq qa-now (s104-snapshot))',
 '(princ (strcat "\nSAMPLE104 after_sync_labels=" (itoa (length qa-now))))',
 '(s104-check "update-keeps-existing-labels" (vl-every ''(lambda (d) (equal d (entget (cdr (assoc -1 d)) ''("*")) 1e-8)) qa-rotated))',
 '(s104-check "update-keeps-survey-and-objects" (and (equal qa-pts (bht:pt-all)) (equal qa-objs (bht:rec-all "OBJ"))))',
 '(princ (strcat "\nSAMPLE104-CHECKS=" (itoa qa-checks) " FAIL=" (itoa qa-fails)))',
 '(bht:api-label-rotate "UCS")','_.QSAVE','_.QUIT')
$steps=@($steps | ForEach-Object { if($_.StartsWith('(setq')) { '(progn '+$_+' (princ))' } else { $_ } })
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSample104 "$out\profile" /i $dwg /s "$out\create.scr" /l en-US > "$out\create.log"
$log=Get-Content "$out\create.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) SAMPLE104|^SAMPLE104'} | Write-Output
if($log -notmatch 'SAMPLE104-CHECKS=\d+ FAIL=0' -or $log -match '(?m)^FAIL SAMPLE104|; error:'){throw 'RTK sample failed'}
$check=@('(setq labels (mapcar ''cdr (bht:tagged-pairs "TEXT" "BHT_NHAN" 1)))','(princ (if (and (> (length labels) 100) (vl-every ''(lambda (e) (and (equal (cdr (assoc 50 (entget e))) (/ pi 6.0) 1e-8) (= (bht:lbl-state e) ''TAY))) labels)) "\nSAMPLE104-REOPEN-PASS" "\nSAMPLE104-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$check),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSample104 "$out\profile" /i $dwg /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=Get-Content "$out\reopen.log" -Encoding Unicode -Raw
if($log -notmatch '(?m)^SAMPLE104-REOPEN-PASS' -or $log -match '; error:'){throw 'RTK sample reopen failed'}
Write-Output "PASS SAMPLE104 save/reopen; DWG=$dwg"
