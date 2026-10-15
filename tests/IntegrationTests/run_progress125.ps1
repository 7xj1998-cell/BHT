param([Parameter(Mandatory=$true)][string]$BinDir,[string]$RuntimePath='',[string]$Drawing='',[string]$AcadProfile='BHTResearch111')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$v\progress125-"+[guid]::NewGuid().ToString('N'));New-Item -ItemType Directory $out | Out-Null
$BinDir=[IO.Path]::GetFullPath($BinDir);Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll","$BinDir\BHT.Palette.dll" -Destination $out
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}else{$RuntimePath=[IO.Path]::GetFullPath($RuntimePath)}
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 "/out:$out\Progress125Probe.dll" '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "$PSScriptRoot\Progress125Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Progress125 probe compile failed'}
function ProgressPath([string]$p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(ProgressPath "$out\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(ProgressPath "$root\src\lisp")+'")'),('(load "'+(ProgressPath $RuntimePath)+'")'),'_.NETLOAD',('"'+(ProgressPath "$out\Progress125Probe.dll")+'"'))
$lines=$boot+@(('(load "'+(ProgressPath "$PSScriptRoot\test_PROGRESS125.lsp")+'")'),'PROGRESS125PROBE','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\test.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /p $AcadProfile /isolate BHTProgress125 "$out\profile" /s "$out\test.scr" /l en-US > "$out\console.log"
$log=[IO.File]::ReadAllText("$out\console.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) PROGRESS125|^PROGRESS125-'} | Write-Output
if($log -match '; error:|(?m)^FAIL PROGRESS125|PROGRESS125-ERROR' -or $log -notmatch 'PROGRESS125-LISP=\d+ FAIL=0' -or $log -notmatch 'PROGRESS125-DOTNET=\d+ FAIL=0'){throw "Progress125 failed: $out"}
Write-Output "PASS PROGRESS125: $out"
$fixture=[IO.File]::ReadAllText("$PSScriptRoot\test_PROGRESS125.lsp");$fixture=$fixture.Substring(0,$fixture.IndexOf('(p125-route "R10"'))
[IO.File]::WriteAllText("$out\fixture.lsp",$fixture,[Text.UTF8Encoding]::new($true))
$stage=$boot+@(('(load "'+(ProgressPath "$out\fixture.lsp")+'")'),'(p125-route "R11" 100.0 1100.0)','(BHT125STAGEIMPORT)',
 '(setq p125-a (bht:obj-read "A10"))',
 '(p125-check "stage-import-keeps10-and-leaves11-pending" (and (bht:progress-locked p125-a) (= (bht:get p125-a "ly_trinh_m") "1050.000") (= (bht:get (bht:obj-read "B11") "ly_trinh_m") "")))',
 '(p125-check "unused-source-route-does-not-duplicate-local11" (equal (acad_strlsort (bht:rec-keys "ROUTE")) ''("R10" "R11")))',
 '(setq p125-before (bht:obj-read "B11") p125-seg (bht:rec-read "SEG" "D11") p125-original-write bht:obj-write)',
 '(defun bht:obj-write (id rec) (if (= id "B11") (vl-exit-with-error "QA forced write failure") (p125-original-write id rec)))',
 '(setq p125-result (bht:progress-apply (bht:progress-plan "D11" "R11")) bht:obj-write p125-original-write)',
 '(p125-check "failed-calculation-restores-record-and-segment" (and (eq (car p125-result) ''LOI) (equal p125-before (bht:obj-read "B11")) (equal p125-seg (bht:rec-read "SEG" "D11")) (equal p125-a (bht:obj-read "A10"))))',
 '(if (/= 0 (logand 8 (getvar "UNDOCTL"))) (command-s "_.UNDO" "_End"))','_.UNDO','_Auto','_Off',
 '(defun bht:palette-load () T)','(defun BHTPROGRESSFORM () ''("CALCULATE" "D11" "R11"))','(defun BHTPROGRESSPREVIEW (title rows) "CANCEL")','BHTXULYDOAN',
 '(p125-check "public-preview-cancel-preserves-data" (and (equal p125-before (bht:obj-read "B11")) (equal p125-seg (bht:rec-read "SEG" "D11"))))',
 '(defun BHTPROGRESSPREVIEW (title rows) "OK")','BHTXULYDOAN',
 '(p125-check "public-command-calculates-selected-segment" (= (bht:get (bht:obj-read "B11") "ly_trinh_m") "1150.000"))','_.U',
 '(p125-check "single-undo-restores-calculation-and-binding" (and (equal p125-before (bht:obj-read "B11")) (equal p125-seg (bht:rec-read "SEG" "D11")) (equal p125-a (bht:obj-read "A10"))))',
 '(bht:progress-apply (bht:progress-plan "D11" "R11"))','(bht:kh-display-transaction ''bht:progress-finish (list "D11"))',
 '(p125-check "next-drawing-computes11-and-preserves10" (and (equal p125-a (bht:obj-read "A10")) (bht:progress-locked (bht:obj-read "B11")) (= (bht:get (bht:obj-read "B11") "ly_trinh_m") "1150.000")))',
 'PROGRESS125STAGELABEL','(princ (strcat "\nPROGRESS125-STAGE=" (itoa p125-n) " FAIL=" (itoa p125-fail)))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\stage.scr",$stage,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /p $AcadProfile /isolate BHTProgress125 "$out\profile" /s "$out\stage.scr" /l en-US > "$out\stage.log"
$log=[IO.File]::ReadAllText("$out\stage.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) PROGRESS125|^PROGRESS125-'} | Write-Output
if($log -match '; error:|(?m)^FAIL PROGRESS125' -or $log -notmatch 'PROGRESS125-STAGE=7 FAIL=0' -or $log -notmatch 'PASS PROGRESS125 stage10-label-present'){throw "Stage transfer failed: $out"}
if($Drawing){
 Copy-Item -LiteralPath $Drawing -Destination "$out\real-input.dwg"
 $env:BHT_PROGRESS_REAL="$out\real-input.dwg"
 & "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 "/out:$out\Progress125UiProbe.dll" /r:System.Windows.Forms.dll /r:System.Drawing.dll '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "/r:$BinDir\BHT.Palette.dll" "$PSScriptRoot\Progress125UiProbe.cs"
 if($LASTEXITCODE -ne 0){throw 'UI probe compile failed'}
 $ui=$boot+@('_.NETLOAD',('"'+(ProgressPath "$out\BHT.Palette.dll")+'"'),'_.NETLOAD',('"'+(ProgressPath "$out\Progress125UiProbe.dll")+'"'),'PROGRESS125UI','_.QUIT','_Y')
 [IO.File]::WriteAllLines("$out\ui.scr",$ui,[Text.UTF8Encoding]::new($true))
 & 'D:\AutoCAD 2024\accoreconsole.exe' /p $AcadProfile /isolate BHTProgress125 "$out\profile" /s "$out\ui.scr" /l en-US > "$out\ui.log"
 $uiReport=Get-Content "$out\progress-ui.txt" -Raw;Write-Output $uiReport;if($uiReport -notmatch 'PASS PROGRESS125 UI 10'){throw "UI failed: $out"}
 $real=$boot+@(('(load "'+(ProgressPath "$PSScriptRoot\test_PROGRESS125_REAL.lsp")+'")'),'_.QUIT','_Y')
 [IO.File]::WriteAllLines("$out\real.scr",$real,[Text.UTF8Encoding]::new($true))
 & 'D:\AutoCAD 2024\accoreconsole.exe' /p $AcadProfile /isolate BHTProgress125 "$out\profile" /i "$out\real-input.dwg" /s "$out\real.scr" /l en-US > "$out\real.log"
 $log=[IO.File]::ReadAllText("$out\real.log",[Text.Encoding]::Unicode)
 $log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) PROGRESS125|^PROGRESS125-|^REAL125'} | Write-Output
 if($log -match '; error:|(?m)^FAIL PROGRESS125' -or $log -notmatch 'PROGRESS125-REAL=8 FAIL=0'){throw "Real drawing failed: $out"}
}
Write-Output "PASS PROGRESS125 COMPLETE: $out"
