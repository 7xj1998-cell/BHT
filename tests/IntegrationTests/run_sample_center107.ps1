param([string]$BinDir,[string]$RuntimePath,[Parameter(Mandatory=$true)][string]$SamplePath)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out="$root\build\v$v\qa\sample-center107"
New-Item -ItemType Directory -Force $out | Out-Null
$sourceHash=(Get-FileHash -LiteralPath $SamplePath).Hash
$copy="$out\BOT830-CachTim-TUYEN1-"+[guid]::NewGuid().ToString('N')+'.dwg'
Copy-Item -LiteralPath $SamplePath -Destination $copy
function LP($p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@(('(load "'+(LP "$PSScriptRoot\test_SAMPLE_CENTER107.lsp")+'")'),'_.QSAVE','_.QUIT')
[IO.File]::WriteAllLines("$out\run.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSampleCenter107 "$out\profile" /i $copy /s "$out\run.scr" /l en-US > "$out\run.log"
$log=Get-Content "$out\run.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^SAMPLE-CENTER107'} | Write-Output
if($log -match '; error:|(?m)^SAMPLE-CENTER107-[A-Z-]+-FAIL' -or $log -notmatch '(?m)^SAMPLE-CENTER107 points=\d+ records=\d+ moved=\d+ bad=0 ms=\d+ FAIL=0'){throw 'Sample center layout failed'}
$steps=@('(princ (if (and (= "CENTER" (bht:meta "symbol_layout_method" "")) (= "TUYEN1" (bht:meta "symbol_center_route" "")) (= "KHAC" (bht:get (bht:route-read "TUYEN1") "loai")) (equal 20.0 (bht:num (bht:meta "symbol_center_distance" "")) 1e-8)) "\nCENTER107-REOPEN-PASS" "\nCENTER107-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSampleCenter107 "$out\profile" /i $copy /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=Get-Content "$out\reopen.log" -Encoding Unicode -Raw
if($log -notmatch '(?m)^CENTER107-REOPEN-PASS' -or $log -match '; error:'){throw 'Sample center reopen failed'}
if((Get-FileHash -LiteralPath $SamplePath).Hash -ne $sourceHash){throw 'Original drawing changed'}
Write-Output "PASS CENTER107 reopen; original unchanged; DWG=$copy"
