param([string]$BinDir,[string]$RuntimePath)
$ErrorActionPreference='Stop';$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v\qa\label108"
New-Item -ItemType Directory -Force $out | Out-Null
function LP($p){$p.Replace('\','/')}
$dwg="$out\Nhan-bien-hai-dong-"+[guid]::NewGuid().ToString('N')+'.dwg'
$boot=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@(('(load "'+(LP "$PSScriptRoot\test_LABEL108.lsp")+'")'),'_.QSAVE',('"'+(LP $dwg)+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTLabel108 "$out\profile" /s "$out\create.scr" /l en-US > "$out\create.log"
$log=Get-Content "$out\create.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) LABEL108|^LABEL108'} | Write-Output
if($log -match '; error:|(?m)^FAIL LABEL108' -or $log -notmatch '(?m)^LABEL108-CHECKS=\d+ FAIL=0'){throw 'Sign label tests failed'}
$steps=@('(setq lines108 (mapcar ''cdr (vl-remove-if-not ''(lambda (pr) (= (car pr) "MULTI108")) (bht:tagged-pairs "TEXT" "BHT_KH" 1))))','(princ (if (and (= (length lines108) 3) (= 1 (length (vl-remove-if-not ''bht:kh-station-label-p lines108)))) "\nLABEL108-REOPEN-PASS" "\nLABEL108-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTLabel108 "$out\profile" /i $dwg /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=Get-Content "$out\reopen.log" -Encoding Unicode -Raw
if($log -notmatch '(?m)^LABEL108-REOPEN-PASS' -or $log -match '; error:'){throw 'Sign label reopen failed'}
Write-Output "PASS LABEL108 save/reopen; DWG=$dwg"
