param([Parameter(Mandatory=$true)][string]$BinDir,[string]$AcadDir='D:\AutoCAD 2024',[string]$AcadProfile='BHTResearch111',[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}else{$RuntimePath=[IO.Path]::GetFullPath($RuntimePath)}
$out=Join-Path $root ("build\v$v\incremental131-"+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $out | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll","$BinDir\BHT.Palette.dll" -Destination $out
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 /r:System.Windows.Forms.dll /r:System.Drawing.dll "/out:$out\Incremental131Probe.dll" "/r:$AcadDir\acdbmgd.dll" "/r:$AcadDir\accoremgd.dll" "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "/r:$BinDir\BHT.Palette.dll" "$PSScriptRoot\Incremental131Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Incremental probe compile failed'}
function LispPath([string]$p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LispPath "$out\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(LispPath "$root\src\lisp")+'")'),
 ('(load "'+(LispPath $RuntimePath)+'")'))
$undo=@('(if (/= 0 (logand 8 (getvar "UNDOCTL"))) (command-s "_.UNDO" "_End"))','_.UNDO','_Auto','_Off',
 '(i131-obj "UNDO_ADD" 65.0)','(bht:obj-write "UNDO_ADD" (bht:set (bht:obj-read "UNDO_ADD") "doan" "DOAN_08"))',
 '(if (/= 0 (logand 8 (getvar "UNDOCTL"))) (command-s "_.UNDO" "_End"))',
 '(setq i131-before (i131-snapshot)) (princ)','(defun bht:palette-load () T)','(defun BHTPROGRESSFORM () ''("CALCULATE" "DOAN_08" "R08"))','(defun BHTPROGRESSPREVIEW (title rows) "OK")',
 'BHTXULYDOAN','(i131-check "public-mixed-segment-calculated" (= "1065.000" (bht:get (bht:obj-read "UNDO_ADD") "ly_trinh_m")))','_.U',
 '(i131-check "one-undo-restores-mixed-segment" (equal i131-before (i131-snapshot)))','BHTXULYDOAN',
 '(setq i131-before (i131-snapshot)) (princ)','(defun BHTPROGRESSFORM () ''("FINISH" "DOAN_08" "R08"))','BHTXULYDOAN',
 '(i131-check "public-mixed-segment-confirmed" (bht:progress-locked (bht:obj-read "UNDO_ADD")))','_.U',
 '(i131-check "one-undo-restores-incremental-confirmation" (equal i131-before (i131-snapshot)))','BHTXULYDOAN',
 '(princ (strcat "\nINCREMENTAL131-UNDO=" (itoa i131-n) " FAIL=" (itoa i131-fail)))')
$lines=$boot+@('_.NETLOAD',('"'+(LispPath "$out\BHT.Palette.dll")+'"'),'_.NETLOAD',('"'+(LispPath "$out\Incremental131Probe.dll")+'"'),('(load "'+(LispPath "$PSScriptRoot\test_INCREMENTAL131.lsp")+'")'))+$undo+@('INCREMENTAL131PROBE','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\test.scr",$lines,[Text.UTF8Encoding]::new($true))
. "$root\scripts\coreconsole.ps1"
$log=Invoke-BhtCoreConsole -AcadDir $AcadDir -Profile $AcadProfile -Script "$out\test.scr" -OutputDirectory $out
[IO.File]::WriteAllText("$out\decoded.log",$log,[Text.UTF8Encoding]::new($true))
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) INCREMENTAL131|^INCREMENTAL131-'} | Write-Output
Write-Output "Evidence: $out"
if($log -match '; error:|(?m)^FAIL INCREMENTAL131|INCREMENTAL131-ERROR' -or $log -notmatch 'INCREMENTAL131-UNDO=31 FAIL=0' -or $log -notmatch 'INCREMENTAL131-DOTNET=\d+ FAIL=0'){throw "Incremental131 failed: $out"}
$reopen=$boot+@('(setq i131-old (bht:rec-all "OBJ"))',
 '(setq i131-r (bht:progress-apply (bht:progress-plan "DOAN_08" "R08")))',
 '(if (and (eq (car i131-r) ''NO_CHANGE) (equal i131-old (bht:rec-all "OBJ"))) (princ "\nINCREMENTAL131-REOPEN-PASS") (princ "\nINCREMENTAL131-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",$reopen,[Text.UTF8Encoding]::new($true))
$reopenLog=Invoke-BhtCoreConsole -AcadDir $AcadDir -Profile $AcadProfile -Script "$out\reopen.scr" -Drawing "$out\confirmed.dwg" -OutputDirectory $out
if($reopenLog -match '; error:|(?m)^INCREMENTAL131-REOPEN-FAIL' -or $reopenLog -notmatch '(?m)^INCREMENTAL131-REOPEN-PASS'){throw "Reopen failed: $out"}
Write-Output 'PASS INCREMENTAL131 reopened DWG no-op'
