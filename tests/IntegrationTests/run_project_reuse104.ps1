param([string]$BinDir,[string]$RuntimePath,[string]$ProjectPath)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out=Join-Path $root 'build/v1.0.4/qa/project-reuse104'
New-Item -ItemType Directory -Force $out | Out-Null
function LP($p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'),
 '_.UNDO','_End','_.UNDO','_Auto','_Off',
 'BHTNAPDUAN',('"'+(LP $ProjectPath)+'"'),
 '(princ (if (and (= 801 (length (bht:pt-all))) (= 177 (length (bht:rec-all "OBJ")))) "\nPASS PROJECTREUSE104 import-command" "\nFAIL PROJECTREUSE104 import-command"))',
 '_.U',
 '(princ (if (and (null (bht:pt-all)) (null (bht:rec-all "OBJ"))) "\nPASS PROJECTREUSE104 import-one-undo" "\nFAIL PROJECTREUSE104 import-one-undo"))',
 'BHTNAPDUAN',('"'+(LP $ProjectPath)+'"'),
 '(setq reuse-pts (bht:pt-all) reuse-symbols (length (bht:tagged-pairs "INSERT" "BHT_KH" 1)))',
 '(bht:symbol-sync nil)',
 '(princ (if (and (equal reuse-pts (bht:pt-all)) (= reuse-symbols (length (bht:tagged-pairs "INSERT" "BHT_KH" 1))) (= 177 (length (bht:rec-all "OBJ")))) "\nPASS PROJECTREUSE104 update-keeps-points-records-symbols" "\nFAIL PROJECTREUSE104 update-keeps-points-records-symbols"))',
 '(princ (if (vl-every ''(lambda (pr) (= (cdr (assoc 8 (entget (cdr pr)))) "BHT_KYHIEU")) (bht:tagged-pairs "*" "BHT_KH" 1)) "\nPASS PROJECTREUSE104 common-layer-after-update" "\nFAIL PROJECTREUSE104 common-layer-after-update"))',
 '(princ (if (= "OK" (car (bht:api-symbol-gap "7"))) "\nPASS PROJECTREUSE104 change-distance-after-import" "\nFAIL PROJECTREUSE104 change-distance-after-import"))',
 '_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\run.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTProjectReuse104 "$out\profile" /s "$out\run.scr" /l en-US > "$out\run.log"
$log=Get-Content "$out\run.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) PROJECTREUSE104'} | Write-Output
if($log -match '; error:|(?m)^FAIL PROJECTREUSE104' -or ([regex]::Matches($log,'(?m)^PASS PROJECTREUSE104').Count -ne 5)){throw 'Imported project reuse failed'}
