param([string]$BinDir,[string]$RuntimePath)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out="$root\build\v1.0.1\qa"
$lines=@('(setvar "LISPSYS" 1)','(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+"$BinDir\BHT.Bridge.dll".Replace('\','/')+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+"$root\src\lisp".Replace('\','/')+'")'),('(load "'+$RuntimePath.Replace('\','/')+'")'),
 '(defun c:V101OFF () (bht:api-sign-fill "0") (princ))',
 '_.UNDO','_End','V101OFF',
 '(princ (strcat "\nOFF-META=" (bht:meta "sign_fill_all" "1")))',
 '_.UNDO','1',
 '(princ (strcat "\nUNDO-META=" (bht:meta "sign_fill_all" "1")))',
 '(princ (strcat "\nUNDO-NOFILL=" (itoa (length (vl-remove-if-not ''(lambda (p) (wcmatch (cdr (assoc 2 (entget (cdr p)))) "*_NOFILL_*")) (bht:tagged-pairs "INSERT" "BHT_KH" 1))))))',
 '(entmake ''((0 . "LAYER") (100 . "AcDbSymbolTableRecord") (100 . "AcDbLayerTableRecord") (2 . "QA_OFF") (70 . 0) (62 . -2) (6 . "Continuous")))',
 '11',
 '(princ (strcat "\nALIAS11-ON=" (if (> (cdr (assoc 62 (tblsearch "LAYER" "QA_OFF"))) 0) "PASS" "FAIL")))',
 '_.-LAYER','_Freeze','QA_OFF','',
 '55',
 '(princ (strcat "\nALIAS55-THAW=" (if (= 0 (logand 1 (cdr (assoc 70 (tblsearch "LAYER" "QA_OFF")))) ) "PASS" "FAIL")))',
 '(princ (strcat "\nNO-BHT44=" (if (bht:fn-defined-p ''c:44) "FAIL" "PASS")))',
 '(defun c:11 () "EXISTING-TDT")',
 '(defun c:44 () "EXISTING-TDT44")',
 ('(load "'+"$root\src\lisp\modules\12-api.lsp".Replace('\','/')+'")'),
 '(princ (strcat "\nEXISTING-11=" (c:11)))',
 '(princ (strcat "\nEXISTING-44=" (c:44)))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\commands.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHT101Commands "$out\commands-profile" /i "$out\BIEN_MAU_1.0.1.dwg" /s "$out\commands.scr" /l en-US > "$out\commands.log"
$log=Get-Content "$out\commands.log" -Encoding Unicode -Raw
foreach($expected in @('OFF-META=0','UNDO-META=1','UNDO-NOFILL=0','ALIAS11-ON=PASS','ALIAS55-THAW=PASS','EXISTING-11=EXISTING-TDT','NO-BHT44=PASS','EXISTING-44=EXISTING-TDT44')){if($log -notmatch [regex]::Escape($expected)){throw "Missing $expected"};Write-Output "PASS $expected"}
