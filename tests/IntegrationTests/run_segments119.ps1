param([Parameter(Mandatory=$true)][string]$BinDir,[string]$RuntimePath='',[string]$AcadProfile='')
$profileArgs=@();if($AcadProfile){$profileArgs=@('/p',$AcadProfile)}
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot); $v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$v\segments119-"+[guid]::NewGuid().ToString('N')); New-Item -ItemType Directory $out | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll" -Destination $out -Force
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
function TestLispPath([string]$p){$p.Replace('\','/')}
$csc="$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /target:library /platform:x64 /langversion:5 "/out:$out\Segment119Probe.dll" '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$BinDir\BHT.Bridge.dll" "/r:$BinDir\BHT.Core.dll" "$PSScriptRoot\Segment119Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Probe build failed'}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(TestLispPath "$out\BHT.Bridge.dll")+'"'),'_.NETLOAD',('"'+(TestLispPath "$out\Segment119Probe.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(TestLispPath "$root\src\lisp")+'")'),('(load "'+(TestLispPath $RuntimePath)+'")'))
$steps=@(('(load "'+(TestLispPath "$PSScriptRoot\test_SEG119.lsp")+'")'),'_.UNDO','_End','_.UNDO','_Auto','_Off',
 '(setq s119-undo-before (bht:rec-all "OBJ"))','BHTPHANDOAN','_.U',
 '(s119-check "partition-one-step-undo" (equal s119-undo-before (bht:rec-all "OBJ")))',
 'BHTPHANDOAN','(princ (strcat "\nSEG119-FINAL-CHECKS=" (itoa s119-n) " FAIL=" (itoa s119-fail)))',
 'SEG119STORE','_.QSAVE',('"'+(TestLispPath "$out\roundtrip.dwg")+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' @profileArgs /isolate BHTSeg119 "$out\profile" /s "$out\create.scr" /l en-US > "$out\create.log"
$log=[IO.File]::ReadAllText("$out\create.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) SEG119|^SEG119-'} | Write-Output
if($log -match '; error:|(?m)^FAIL SEG119' -or $log -notmatch 'SEG119-FINAL-CHECKS=\d+ FAIL=0' -or $log -notmatch 'SEG119-STORE-PASS'){throw "Segment119 failed: $out"}
$steps=@('(princ (if (and (= "DITCH-NEW" (bht:get (bht:rec-read "SEG" "DITCH-A") "route_id")) (= "TIM_RANH" (bht:get (bht:route-read "DITCH-NEW") "loai"))) "\nSEG119-REOPEN-PASS" "\nSEG119-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' @profileArgs /isolate BHTSeg119 "$out\profile" /i "$out\roundtrip.dwg" /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=[IO.File]::ReadAllText("$out\reopen.log",[Text.Encoding]::Unicode)
if($log -match '; error:|(?m)^SEG119-REOPEN-FAIL' -or $log -notmatch 'SEG119-REOPEN-PASS'){throw "Segment119 reopen failed: $out"}
Write-Output "PASS SEG119 save/reopen: $out"
