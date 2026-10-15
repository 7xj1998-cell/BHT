param([string]$BinDir, [string]$RuntimePath)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content (Join-Path $root 'VERSION') -Raw).Trim()
$out=Join-Path $root "build\v$version\layer-qa"
New-Item -ItemType Directory -Force -Path $out | Out-Null
$lines=@('(setvar "LISPSYS" 1)','(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+"$BinDir\BHT.Bridge.dll".Replace('\','/')+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+"$root\src\lisp".Replace('\','/')+'")'),('(load "'+$RuntimePath.Replace('\','/')+'")'),
 '(entmake ''((0 . "LAYER") (100 . "AcDbSymbolTableRecord") (100 . "AcDbLayerTableRecord") (2 . "QA_FROZEN") (70 . 0) (62 . 2) (6 . "Continuous")))',
 '(setq qa-ent (entmakex ''((0 . "LINE") (8 . "QA_FROZEN") (10 0.0 0.0 0.0) (11 10.0 0.0 0.0))))',
 '55','(command)',
 '_.-LAYER','_Freeze','QA_FROZEN','', 
 '(princ (strcat "\nFIXTURE-FROZEN=" (if (= 1 (logand 1 (cdr (assoc 70 (tblsearch "LAYER" "QA_FROZEN"))))) "PASS" "FAIL")))',
 '66',
 '(princ (strcat "\nALIAS66-THAW=" (if (= 0 (logand 1 (cdr (assoc 70 (tblsearch "LAYER" "QA_FROZEN"))))) "PASS" "FAIL")))',
 '(princ (strcat "\nNO-BHT44=" (if (bht:fn-defined-p ''c:44) "FAIL" "PASS")))',
 '(defun c:BHTLAYFRZ () (princ "\nALIAS55-ROUTES-FREEZE=PASS"))','55',
 '(defun c:44 () "KEEP44")','(defun c:55 () "KEEP55")','(defun c:66 () "KEEP66")',
 ('(load "'+"$root\src\lisp\modules\12-api.lsp".Replace('\','/')+'")'),
 '(princ (strcat "\nEXISTING=" (c:44) "/" (c:55) "/" (c:66)))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\commands.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHT102Layers "$out\profile" /s "$out\commands.scr" /l en-US > "$out\commands.log"
$log=Get-Content "$out\commands.log" -Encoding Unicode -Raw
foreach($expected in @('_.LAYFRZ','FIXTURE-FROZEN=PASS','ALIAS66-THAW=PASS','ALIAS55-ROUTES-FREEZE=PASS','NO-BHT44=PASS','EXISTING=KEEP44/KEEP55/KEEP66')) {
 if($log -notmatch [regex]::Escape($expected)){throw "Missing $expected"}
 Write-Output "PASS $expected"
}
