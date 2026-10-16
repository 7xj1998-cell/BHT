param([Parameter(Mandatory=$true)][string]$BinDir,[string]$RuntimePath='',[string]$ProjectPackage='',[string]$Segments='',[string]$CompletePackage='',[string]$ReferenceDrawing='',[string]$AcadProfile='')
$profileArgs=@();if($AcadProfile){$profileArgs=@('/p',$AcadProfile)}
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$v\workflow122-"+[guid]::NewGuid().ToString('N'));New-Item -ItemType Directory $out | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll" -Destination $out
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 "/out:$out\Workflow122Probe.dll" '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$out\BHT.Core.dll" "/r:$out\BHT.Bridge.dll" "$PSScriptRoot\Workflow122Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Workflow122 probe build failed'}
$env:BHT_WORKFLOW_PACKAGE=$ProjectPackage
$env:BHT_WORKFLOW_SEGMENTS=$Segments
$env:BHT_WORKFLOW_COMPLETE=$CompletePackage
$env:BHT_WORKFLOW_REFERENCE=$ReferenceDrawing
function TestLispPath([string]$p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(TestLispPath "$out\BHT.Bridge.dll")+'"'),'_.NETLOAD',('"'+(TestLispPath "$out\Workflow122Probe.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(TestLispPath "$root\src\lisp")+'")'),('(load "'+(TestLispPath $RuntimePath)+'")'),('(load "'+(TestLispPath "$PSScriptRoot\test_WORKFLOW122.lsp")+'")'),'WORKFLOW122PROBE','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\test.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' @profileArgs /isolate BHTWorkflow122 "$out\profile" /s "$out\test.scr" /l en-US > "$out\console.log"
$log=[IO.File]::ReadAllText("$out\console.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) WORKFLOW122|^WORKFLOW122-'} | Write-Output
if($log -match '; error:|(?m)^FAIL WORKFLOW122|WORKFLOW122-ERROR' -or $log -notmatch 'WORKFLOW122-CHECKS=\d+ FAIL=0' -or $log -notmatch 'WORKFLOW122-DOTNET-CHECKS=\d+ FAIL=0'){throw "Workflow122 failed: $out"}
Write-Output "PASS WORKFLOW122: $out"
if($ReferenceDrawing){
 $real=@($lines | Select-Object -First 7)
 $real+=@('WORKFLOW122REALROUTE','(setq w122-n 0 w122-fail 0)',
  '(defun w122-check (name ok) (setq w122-n (1+ w122-n)) (if (not ok) (setq w122-fail (1+ w122-fail))) (princ (strcat "\n" (if ok "PASS WORKFLOW122 " "FAIL WORKFLOW122 ") name)))',
  '(setq w122-result (bht:route-recalculate "TIMRANHD8"))',
  '(w122-check "real-package-recalculates-with-confirmed-destination-TDT-controls" (not (eq (car w122-result) ''LOI)))',
  '(w122-check "real-package-nearest-post-offset-2.251" (= "2.251" (bht:get (bht:obj-read "OBJ-000120") "offset_m")))',
  '(w122-check "real-package-chainage-from-destination-48525.006" (= "48525.006" (bht:get (bht:obj-read "OBJ-000120") "ly_trinh_m")))',
  '(w122-check "real-package-selected-destination-route" (= "TIMRANHD8" (bht:get (bht:obj-read "OBJ-000120") "route_id")))',
  '(w122-check "real-package-other-sign-chainage-calculated" (= "48784.073" (bht:get (bht:obj-read "OBJ-000116") "ly_trinh_m")))',
  '(princ (strcat "\nWORKFLOW122-REAL-CHECKS=" (itoa w122-n) " FAIL=" (itoa w122-fail)))','_.QUIT','_Y')
 [IO.File]::WriteAllLines("$out\real.scr",$real,[Text.UTF8Encoding]::new($true))
 & 'D:\AutoCAD 2024\accoreconsole.exe' @profileArgs /isolate BHTWorkflow122 "$out\profile" /s "$out\real.scr" /l en-US > "$out\real.log"
 $log=[IO.File]::ReadAllText("$out\real.log",[Text.Encoding]::Unicode)
 $log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) WORKFLOW122|^WORKFLOW122-'} | Write-Output
 if($log -match '; error:|(?m)^FAIL WORKFLOW122|WORKFLOW122-ERROR' -or $log -notmatch 'WORKFLOW122-REAL-CHECKS=5 FAIL=0'){throw "Workflow122 real destination route failed: $out"}
}
