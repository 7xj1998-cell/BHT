param([string]$AcadDir='D:\AutoCAD 2024',[string]$BinDir='',[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$version"
if(!$BinDir){$BinDir="$out\bin"};if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$version.lsp"}
function LP([string]$p){$p.Replace('\','/')}
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 "/r:$AcadDir\acdbmgd.dll" "/r:$AcadDir\accoremgd.dll" "/r:$BinDir\BHT.Bridge.dll" "/r:$BinDir\BHT.Core.dll" "/out:$BinDir\ManualMarkerProbe.dll" "$PSScriptRoot\ManualMarkerProbe.cs"
if($LASTEXITCODE -ne 0){throw 'Manual probe compile failed'}
$dwg="$out\manual-$([guid]::NewGuid().ToString('N')).dwg"
$load=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'),
 '_.NETLOAD',('"'+(LP "$BinDir\ManualMarkerProbe.dll")+'"'))
$verify=@('(setq mi (bht:pt-all) mr (bht:obj-read "OBJ-000001"))',
 '(princ (if (and (bht:obj-manual-position-p mr mi) (null (bht:field-marker "OBJ-000001" mi))) "\nMANUAL-CONTROL-PASS" "\nMANUAL-FAIL-control"))',
 '(bht:obj-write "BORROWED" (bht:set (bht:set (bht:set mr "object_id" "BORROWED") "position_source" "") "station_control_eligible" ""))',
 '(princ (if (null (bht:field-marker "BORROWED" mi)) "\nMANUAL-SHARED-PASS" "\nMANUAL-FAIL-shared"))',
 '(bht:obj-delete "BORROWED")',
 '(setq ml (entmakex (quote ((0 . "LWPOLYLINE") (100 . "AcDbEntity") (100 . "AcDbPolyline") (90 . 2) (70 . 0) (10 0.0 0.0) (10 100.0 0.0)))))',
 '(bht:route-create "MANUAL-ROUTE" ml "TIM_DUONG" 4.0 0.0 nil "QA")',
 '(bht:route-write "MANUAL-ROUTE" (bht:set (bht:route-read "MANUAL-ROUTE") "max_offset" "100"))',
 '(princ (if (null (car (bht:route-object-controls (bht:route-read "MANUAL-ROUTE")))) "\nMANUAL-ROUTE-PASS" "\nMANUAL-FAIL-route"))',
 '(bht:api-symbol-sync "OBJ-000001 OBJ-000002 OBJ-000003")',
 '(princ (if (= 3 (length (bht:tagged-pairs "INSERT" "BHT_KH" 1))) "\nMANUAL-SYMBOL-PASS" "\nMANUAL-FAIL-symbol"))',
 '(setq mc (vl-catch-all-apply (quote bht:check) nil))',
 '(princ (if (and (not (vl-catch-all-error-p mc)) (= 0 (car mc))) "\nMANUAL-DIAGNOSTICS-PASS" "\nMANUAL-FAIL-diagnostics"))')
[IO.File]::WriteAllLines("$out\manual.scr",($load+@('BHTMANUALPROBE')+$verify+@('_.QSAVE',('"'+(LP $dwg)+'"'),'_.QUIT','_Y')),[Text.UTF8Encoding]::new($true))
& "$AcadDir\accoreconsole.exe" /isolate BHTManual "$out\manual-profile" /s "$out\manual.scr" /l en-US > "$out\manual.log"
$text=[IO.File]::ReadAllText("$out\manual.log",[Text.Encoding]::Unicode)
$text -split "`n" | Where-Object {$_ -match '^MANUAL-'} | Write-Host
foreach($required in @('MANUAL-FINAL-PASS','MANUAL-CONTROL-PASS','MANUAL-SHARED-PASS','MANUAL-ROUTE-PASS','MANUAL-SYMBOL-PASS','MANUAL-DIAGNOSTICS-PASS')) {if($text -notmatch "(?m)^$required"){throw "Missing $required"}}
if($text -match '(?m)^MANUAL-FAIL|; error:'){throw 'Manual-marker integration failed'}
[IO.File]::WriteAllLines("$out\manual-reopen.scr",($load+@('BHTMANUALREOPEN','_.UNDO','_End','BHTMANUALUNDO','_.UNDO','1',
 '(princ (if (and (= 3 (length (bht:obj-ids))) (= 3 (length (bht:pt-all)))) "\nMANUAL-UNDO-PASS" "\nMANUAL-FAIL-undo"))',
 '_.QUIT','_Y')),[Text.UTF8Encoding]::new($true))
& "$AcadDir\accoreconsole.exe" /isolate BHTManual "$out\manual-profile" /i $dwg /s "$out\manual-reopen.scr" /l en-US > "$out\manual-reopen.log"
$text=[IO.File]::ReadAllText("$out\manual-reopen.log",[Text.Encoding]::Unicode)
if($text -notmatch '(?m)^MANUAL-REOPEN-PASS' -or $text -notmatch '(?m)^MANUAL-UNDO-PASS' -or $text -match '(?m)^MANUAL-FAIL|; error:'){throw 'Manual-marker reopen/undo failed'}
Write-Host 'MANUAL-REOPEN-PASS; MANUAL-UNDO-PASS'
