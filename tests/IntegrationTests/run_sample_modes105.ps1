param([string]$BinDir,[string]$RuntimePath,[string]$SamplePath)
$ErrorActionPreference='Stop';$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v\qa\sample-modes105"
New-Item -ItemType Directory -Force $out | Out-Null
$copy="$out\BOT830-DuongThang-"+[guid]::NewGuid().ToString('N')+'.dwg';Copy-Item -LiteralPath $SamplePath -Destination $copy
$bent="$out\BOT830-GapKhuc-"+[guid]::NewGuid().ToString('N')+'.dwg'
function LP($p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'),
 '(progn (setq sample105-pts (bht:pt-all) sample105-objs (bht:rec-all "OBJ") sample105-angle-scale (mapcar ''(lambda (pr / d) (setq d (entget (cdr pr))) (list (car pr) (assoc 41 d) (assoc 50 d))) (bht:tagged-pairs "INSERT" "BHT_KH" 1))) (princ))',
 '_.UNDO','_End','_.UNDO','_Auto','_Off',
 '(progn (setq sample105-time (getvar "MILLISECS") sample105-r (bht:api-symbol-display "3" "THANG")) (princ sample105-r))',
 '(princ (strcat "\nSAMPLE105 straight_ms=" (itoa (- (getvar "MILLISECS") sample105-time))))',
 '(princ (if (and (= "OK" (car sample105-r)) (equal sample105-pts (bht:pt-all)) (= 177 (length (bht:rec-all "OBJ"))) (null (bht:tagged-pairs "LWPOLYLINE" "BHT_KH" 1))) "\nSAMPLE105-STRAIGHT-PASS" "\nSAMPLE105-STRAIGHT-FAIL"))',
 '(princ (if (equal sample105-angle-scale (mapcar ''(lambda (pr / d) (setq d (entget (cdr pr))) (list (car pr) (assoc 41 d) (assoc 50 d))) (bht:tagged-pairs "INSERT" "BHT_KH" 1)) 1e-8) "\nSAMPLE105-ANGLES-SCALES-PASS" "\nSAMPLE105-ANGLES-SCALES-FAIL"))',
 '_.U',
 '(princ (if (equal sample105-objs (bht:rec-all "OBJ")) "\nSAMPLE105-UNDO-PASS" "\nSAMPLE105-UNDO-FAIL"))',
 '(bht:api-symbol-display "3" "THANG")','_.QSAVE',
 '(progn (setq sample105-time (getvar "MILLISECS") sample105-r (bht:api-symbol-display "3" "GAP_KHUC")) (princ sample105-r))',
 '(princ (strcat "\nSAMPLE105 bent_ms=" (itoa (- (getvar "MILLISECS") sample105-time))))',
 '(princ (if (and (= "OK" (car sample105-r)) (equal sample105-pts (bht:pt-all)) (bht:tagged-pairs "LWPOLYLINE" "BHT_KH" 1)) "\nSAMPLE105-BENT-PASS" "\nSAMPLE105-BENT-FAIL"))',
 'BHTCHONBAOHIEU',
 '(princ (if (> (sslength (cadr (ssgetfirst))) 500) "\nSAMPLE105-SELECTION-PASS" "\nSAMPLE105-SELECTION-FAIL"))',
 '_.SAVEAS','2018',('"'+(LP $bent)+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\run.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSampleModes105 "$out\profile" /i $copy /s "$out\run.scr" /l en-US > "$out\run.log"
$log=Get-Content "$out\run.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^SAMPLE105'} | Write-Output
if($log -match '; error:|(?m)^SAMPLE105-[A-Z-]+-FAIL' -or $log -notmatch 'SAMPLE105-SELECTION-PASS' -or !(Test-Path -LiteralPath $bent)){throw 'Sample modes failed'}
Write-Output "STRAIGHT=$copy";Write-Output "BENT=$bent"
