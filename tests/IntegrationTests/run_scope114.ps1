param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$OutDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
New-Item -ItemType Directory -Path $OutDir | Out-Null
function L114([string]$p){[IO.Path]::GetFullPath($p).Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(L114 "$BinDir\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(L114 "$root\src\lisp")+'")'),('(load "'+(L114 $RuntimePath)+'")'))
$steps=@(('(load "'+(L114 "$PSScriptRoot\test_SCOPE114.lsp")+'")'),
 '(if (/= 0 (logand 8 (getvar "UNDOCTL"))) (command-s "_.UNDO" "_End"))','_.UNDO','_Auto','_Off',
 '(s114-obj "BOUNDARY" 75.0 2.0)',
 '(if (/= 0 (logand 8 (getvar "UNDOCTL"))) (command-s "_.UNDO" "_End"))',
 '(defun bht:palette-load () T)','(defun BHTPROGRESSFORM () ''("CALCULATE" "DOAN_01" "R01"))','(defun BHTPROGRESSPREVIEW (title rows) "OK")',
 '(setq s114-before (s114-snapshot)) (princ)','BHTXULYDOAN',
 '(s114-check "boundary-calculated-via-public-command" (= "38697.000" (bht:get (bht:obj-read "BOUNDARY") "ly_trinh_m")))','_.U',
 '(s114-check "public-calculation-undo-restores-all" (equal s114-before (s114-snapshot)))','BHTXULYDOAN',
 '(setq s114-before (s114-snapshot)) (princ)','(defun BHTPROGRESSFORM () ''("FINISH" "DOAN_01" "R01"))','BHTXULYDOAN',
 '(s114-check "public-finish-confirms-boundary-not-outside" (and (bht:progress-locked (bht:obj-read "BOUNDARY")) (not (bht:progress-locked (bht:obj-read "OUT6")))))','_.U',
 '(s114-check "public-confirmation-undo-restores-all" (equal s114-before (s114-snapshot)))','BHTXULYDOAN',
 ('(setq s114-f (open "'+(L114 "$OutDir\before.lsp")+'" "w"))'),
 '(prin1 (list (bht:rec-all "OBJ") (bht:rec-all "SEG") (bht:rec-all "ROUTE")) s114-f) (close s114-f)',
 '(setvar "FILEDIA" 0)','_.QSAVE',(L114 "$OutDir\saved.dwg"),
 '(princ (strcat "\nSCOPE114-TOTAL=" (itoa s114-count) " FAIL=" (itoa s114-fails)))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$OutDir\test.scr",$boot+$steps,[Text.UTF8Encoding]::new($true))
. "$root\scripts\coreconsole.ps1"
$log=Invoke-BhtCoreConsole -Script "$OutDir\test.scr" -OutputDirectory $OutDir
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) SCOPE114|^SCOPE114-TOTAL'} | Write-Output
if($log -notmatch '(?m)^SCOPE114-TOTAL=28 FAIL=0'){throw 'Scope114 incomplete or failed'}
$reopen=@(('(setq s114-f (open "'+(L114 "$OutDir\before.lsp")+'" "r"))'),
 '(setq s114-before (read (read-line s114-f))) (close s114-f)',
 '(if (equal s114-before (list (bht:rec-all "OBJ") (bht:rec-all "SEG") (bht:rec-all "ROUTE"))) (princ "\nPASS SCOPE114 reopen-canonical") (princ "\nFAIL SCOPE114 reopen-canonical"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$OutDir\reopen.scr",$boot+$reopen,[Text.UTF8Encoding]::new($true))
$log=Invoke-BhtCoreConsole -Script "$OutDir\reopen.scr" -Drawing "$OutDir\saved.dwg" -OutputDirectory $OutDir
if($log -notmatch '(?m)^PASS SCOPE114 reopen-canonical'){throw 'Scope114 reopen incomplete'}
Write-Output 'PASS SCOPE114 reopen-canonical'
