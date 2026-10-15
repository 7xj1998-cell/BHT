param([string]$BinDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop';$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v\qa\display105"
New-Item -ItemType Directory -Force $out | Out-Null
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
function LP($p){$p.Replace('\','/')}
$dwg="$out\Display-modes-"+[guid]::NewGuid().ToString('N')+'.dwg'
$boot=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@(('(load "'+(LP "$PSScriptRoot\test_DISPLAY104.lsp")+'")'),('(load "'+(LP "$PSScriptRoot\test_DISPLAY105.lsp")+'")'),
 '_.UNDO','_End','_.UNDO','_Auto','_Off',
 '(progn (setq d105-before (bht:rec-all "OBJ") d105-before-meta (bht:rec-read "META" "CONFIG") d105-before-entities (mapcar ''(lambda (pr) (entget (cdr pr) ''("*"))) (bht:tagged-pairs "*" "BHT_KH" 1))) (princ))',
 '(bht:api-symbol-display "9" "GAP_KHUC")','_.U',
 '(princ (if (and (equal d105-before (bht:rec-all "OBJ")) (equal d105-before-meta (bht:rec-read "META" "CONFIG")) (equal d105-before-entities (mapcar ''(lambda (pr) (entget (cdr pr) ''("*"))) (bht:tagged-pairs "*" "BHT_KH" 1)))) "\nDISPLAY105-UNDO-PASS" "\nDISPLAY105-UNDO-FAIL"))',
 '_.QSAVE',('"'+(LP $dwg)+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTDisplay105 "$out\profile" /s "$out\create.scr" /l en-US > "$out\create.log"
$log=Get-Content "$out\create.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) DISPLAY105|^DISPLAY105'} | Write-Output
if($log -match '; error:|(?m)^FAIL DISPLAY105' -or $log -notmatch 'DISPLAY105-CHECKS=\d+ FAIL=0' -or $log -notmatch 'DISPLAY105-UNDO-PASS'){throw 'Display modes failed'}
$steps=@('(princ (if (and (= "THANG" (bht:meta "symbol_leader_mode" "")) (null (bht:tagged-pairs "LWPOLYLINE" "BHT_KH" 1))) "\nDISPLAY105-REOPEN-PASS" "\nDISPLAY105-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTDisplay105 "$out\profile" /i $dwg /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=Get-Content "$out\reopen.log" -Encoding Unicode -Raw
if($log -notmatch 'DISPLAY105-REOPEN-PASS' -or $log -match '; error:'){throw 'Display modes reopen failed'}
Write-Output "PASS DISPLAY105 save/reopen; DWG=$dwg"
