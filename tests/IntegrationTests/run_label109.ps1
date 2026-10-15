param([string]$BinDir,[string]$RuntimePath)
$ErrorActionPreference='Stop';$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v\qa\label109"
New-Item -ItemType Directory -Force $out | Out-Null
function LP($p){$p.Replace('\','/')}
$dwg="$out\Nhan-gon-va-duong-noi-"+[guid]::NewGuid().ToString('N')+'.dwg'
$boot=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@(('(load "'+(LP "$PSScriptRoot\test_LABEL109.lsp")+'")'),'_.QSAVE',('"'+(LP $dwg)+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTLabel109 "$out\profile" /s "$out\create.scr" /l en-US > "$out\create.log"
$log=Get-Content "$out\create.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) LABEL109|^LABEL109'} | Write-Output
if($log -match '; error:|(?m)^FAIL LABEL109' -or $log -notmatch '(?m)^LABEL109-CHECKS=\d+ FAIL=0'){throw 'Sign label tests failed'}
$steps=@(('(load "'+(LP "$PSScriptRoot\label109_helpers.lsp")+'")'),'(princ (if (and (l109-cluster-p 0.4) (= 1 (length (bht:tagged-pairs "TEXT" "BHT_ANHTEN" 1)))) "\nLABEL109-REOPEN-PASS" "\nLABEL109-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTLabel109 "$out\profile" /i $dwg /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=Get-Content "$out\reopen.log" -Encoding Unicode -Raw
if($log -notmatch '(?m)^LABEL109-REOPEN-PASS' -or $log -match '; error:'){throw 'Sign label reopen failed'}
Write-Output "PASS LABEL109 save/reopen; DWG=$dwg"
