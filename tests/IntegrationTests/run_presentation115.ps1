param([Parameter(Mandatory=$true)][string]$BinDir,[string]$RuntimePath='',[switch]$Legacy)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$v\marker114-"+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $out | Out-Null
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
function LP($p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),
  ('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@(('(load "'+(LP "$PSScriptRoot\test_MARKER114.lsp")+'")'),
 ('(load "'+(LP "$PSScriptRoot\test_PRESENTATION115.lsp")+'")'),'_.UNDO','_End','_.UNDO','_Auto','_Off','(setq m114-undo (m114-snapshot))',
 '(bht:api-symbol-center "R114" "18" "THANG")','_.U',
 '(princ (if (equal m114-undo (m114-snapshot)) "\nMARKER114-UNDO-PASS" "\nMARKER114-UNDO-FAIL"))',
 '(setq p115-ucs-head (m114-head "MC"))','_.UCS','_Z','37','(bht:api-sign-place "MC" "DIRECT" "ROUTE" "0")',
 '(princ (if (equal (m114-head "MC") p115-ucs-head 1e-7) "\nMARKER114-UCS-PASS" "\nMARKER114-UCS-FAIL"))',
 '_.UCS','_World','_.QSAVE',('"'+(LP "$out\roundtrip.dwg")+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTMarker114 "$out\profile" /s "$out\create.scr" /l en-US > "$out\create.log"
$log=[IO.File]::ReadAllText("$out\create.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) (MARKER114|PRESENTATION115)|^(MARKER114|PRESENTATION115)'}
if($Legacy){if($log -notmatch '(?m)^FAIL (MARKER114|PRESENTATION115) marker-at-origin-no-leader'){throw 'Legacy marker defect not reproduced'};Write-Host "LEGACY DEFECT REPRODUCED: $out";exit 0}
if($log -match '; error:|(?m)^FAIL (MARKER114|PRESENTATION115)' -or $log -notmatch 'PRESENTATION115-CHECKS=\d+ FAIL=0' -or $log -notmatch 'MARKER114-UNDO-PASS' -or $log -notmatch 'MARKER114-UCS-PASS'){throw "Marker tests failed: $out"}
$steps=@('(bht:symbol-sync ''("ML" "MR" "MN" "MK" "MC"))',
 '(setq ok114 T)',
 '(foreach id ''("ML" "MR" "MN" "MK" "MC") (setq rec (bht:obj-read id) e (cdr (assoc id (bht:tagged-pairs "INSERT" "BHT_KH" 1)))) (if (or (not (equal (bht:obj-position rec (bht:pt-all)) (cdr (assoc 10 (entget e))) 1e-7)) (assoc id (bht:tagged-pairs "LWPOLYLINE" "BHT_KH" 1)) (assoc id (bht:tagged-pairs "LINE" "BHT_KH" 1))) (setq ok114 nil)))',
 '(setq e (cdr (assoc "MN" (bht:tagged-pairs "INSERT" "BHT_KH" 1))))',
 '(princ (if (and ok114 (equal (sin (cdr (assoc 50 (entget e)))) 1.0 1e-7)) "\nMARKER114-REOPEN-PASS" "\nMARKER114-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTMarker114 "$out\profile" /i "$out\roundtrip.dwg" /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=[IO.File]::ReadAllText("$out\reopen.log",[Text.Encoding]::Unicode)
if($log -match '; error:|(?m)^MARKER114-REOPEN-FAIL' -or $log -notmatch '(?m)^MARKER114-REOPEN-PASS'){throw "Marker reopen failed: $out"}
Write-Host "PASS MARKER114 save/reopen; REPORT: $out"
