param([Parameter(Mandatory=$true)][string]$BinDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$v\offset121-"+[guid]::NewGuid().ToString('N'));New-Item -ItemType Directory -Path $out | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll" -Destination $out
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
function LP([string]$p){$p.Replace('\','/')}
$csc="$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /target:library /platform:x64 /langversion:5 "/out:$out\Offset121Probe.dll" '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$out\BHT.Core.dll" "/r:$out\BHT.Bridge.dll" "$PSScriptRoot\Offset121Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Offset121 probe build failed'}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$out\BHT.Bridge.dll")+'"'),'_.NETLOAD',('"'+(LP "$out\Offset121Probe.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@(('(load "'+(LP "$PSScriptRoot\test_OFFSET121.lsp")+'")'),
 '(foreach p (bht:tagged-pairs "DIMENSION" "BHT_OFFSET_DIM" 1) (if (= (car p) "OTHER121") (entdel (cdr p))))',
 '_.UNDO','_End','_.UNDO','_Auto','_Off','BHTDIMTIM','Id','OTHER121',
 '(o121-check "public-command-creates-real-dimension" (= 2 (length (bht:tagged-pairs "DIMENSION" "BHT_OFFSET_DIM" 1))))',
 '_.U','(o121-check "public-command-one-step-undo-removes-only-created-dimension" (= 1 (length (bht:tagged-pairs "DIMENSION" "BHT_OFFSET_DIM" 1))))',
 'BHTDIMTIM','Id','OTHER121',
 '(princ (strcat "\nOFFSET121-FINAL-CHECKS=" (itoa o121-n) " FAIL=" (itoa o121-fail)))',
 'OFFSET121GEOMETRY','_.QSAVE',('"'+(LP "$out\roundtrip.dwg")+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTOffset121 "$out\profile" /s "$out\create.scr" /l en-US > "$out\create.log"
$log=[IO.File]::ReadAllText("$out\create.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) OFFSET121|^OFFSET121-'} | Write-Output
if($log -match '; error:|Autodesk.AutoCAD.Runtime.Exception:|(?m)^FAIL OFFSET121|OFFSET121-GEOMETRY-FAIL' -or $log -notmatch 'OFFSET121-FINAL-CHECKS=\d+ FAIL=0' -or $log -notmatch 'OFFSET121-GEOMETRY-PASS'){throw "Offset121 failed: $out"}
$steps=@('OFFSET121GEOMETRY','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTOffset121 "$out\profile" /i "$out\roundtrip.dwg" /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=[IO.File]::ReadAllText("$out\reopen.log",[Text.Encoding]::Unicode)
if($log -match '; error:|OFFSET121-GEOMETRY-FAIL' -or $log -notmatch 'OFFSET121-GEOMETRY-PASS'){throw "Offset121 reopen failed: $out"}
Write-Output "PASS OFFSET121 DIM geometry and save/reopen: $out"
