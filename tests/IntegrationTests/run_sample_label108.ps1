param([string]$BinDir,[string]$RuntimePath,[Parameter(Mandatory=$true)][string]$SamplePath)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out="$root\build\v$v\qa\sample-label108"
New-Item -ItemType Directory -Force $out | Out-Null
$sourceHash=(Get-FileHash -LiteralPath $SamplePath).Hash
$copy="$out\BOT830-Nhan-bien-hai-dong-"+[guid]::NewGuid().ToString('N')+'.dwg'
Copy-Item -LiteralPath $SamplePath -Destination $copy
function LP($p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@(('(load "'+(LP "$PSScriptRoot\test_SAMPLE_LABEL108.lsp")+'")'),'_.QSAVE','_.QUIT')
[IO.File]::WriteAllLines("$out\run.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSampleLabel108 "$out\profile" /i $copy /s "$out\run.scr" /l en-US > "$out\run.log"
$log=Get-Content "$out\run.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^SAMPLE-LABEL108'} | Write-Output
if($log -match '; error:|(?m)^SAMPLE-LABEL108-[A-Z-]+-FAIL' -or $log -notmatch '(?m)^SAMPLE-LABEL108 checked=\d+ bad=0 ms=\d+'){throw 'Sample sign label layout failed'}
$steps=@('(princ (if (and (= *bht-version* (bht:meta "symbol_build" "")) (vl-some ''(lambda (pr) (bht:kh-station-label-p (cdr pr))) (bht:tagged-pairs "TEXT" "BHT_KH" 1))) "\nLABEL108-REOPEN-PASS" "\nLABEL108-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSampleLabel108 "$out\profile" /i $copy /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=Get-Content "$out\reopen.log" -Encoding Unicode -Raw
if($log -notmatch '(?m)^LABEL108-REOPEN-PASS' -or $log -match '; error:'){throw 'Sample label reopen failed'}
if((Get-FileHash -LiteralPath $SamplePath).Hash -ne $sourceHash){throw 'Original drawing changed'}
Write-Output "PASS LABEL108 reopen; original unchanged; DWG=$copy"
