param([Parameter(Mandatory=$true)][string]$BinDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$v\route123-"+[guid]::NewGuid().ToString('N'));New-Item -ItemType Directory $out | Out-Null
$BinDir=[IO.Path]::GetFullPath($BinDir)
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}else{$RuntimePath=[IO.Path]::GetFullPath($RuntimePath)}
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 "/out:$out\Route123UiProbe.dll" /r:System.Windows.Forms.dll /r:System.Drawing.dll '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$BinDir\BHT.Palette.dll" "$PSScriptRoot\Route123UiProbe.cs"
if($LASTEXITCODE -ne 0){throw 'Route123 probe compile failed'}
function LP([string]$p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),'_.NETLOAD',('"'+(LP "$BinDir\BHT.Palette.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@('_.NETLOAD',('"'+(LP "$out\Route123UiProbe.dll")+'"'),'ROUTE123UIPROBE',('(load "'+(LP "$PSScriptRoot\test_ROUTE123.lsp")+'")'),
 '(if (/= 0 (logand 8 (getvar "UNDOCTL"))) (command-s "_.UNDO" "_End"))','_.UNDO','_Auto','_Off',
 '(setq *bht-route-selected* "R123")','BHTDSMOC',
 '(q123-check "public-table-delete-applies-selected-row" (= 2 (length (bht:get-all (bht:route-read "R123") "moc"))))',
 '_.U','(q123-check "one-undo-restores-controls-and-derived-stations" (and (equal q123-current (bht:route-read "R123")) (equal q123-objs (bht:rec-all "OBJ"))))',
 '(setq q123-option-reply ''("OK" "R123" "KHAC" "20.5" "0") *bht-route-selected* "R123")','BHTSUATUYEN',
 '(q123-check "public-edit-uses-dialog-selection" (= "KHAC" (bht:get (bht:route-read "R123") "loai")))','_.U',
 '(q123-check "public-edit-undo-restores-type-and-marks" (equal q123-current (bht:route-read "R123")))',
 '(setq BHTROUTEOPTIONS q123-real-options BHTROUTEMARKS q123-real-marks)',
 '(princ (strcat "\nROUTE123-FINAL=" (itoa q123-count) " FAIL=" (itoa q123-fail)))',
 '_.QSAVE',('"'+(LP "$out\roundtrip.dwg")+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\test.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTRoute123 "$out\profile" /s "$out\test.scr" /l en-US > "$out\console.log"
$log=[IO.File]::ReadAllText("$out\console.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) ROUTE123|^ROUTE123-'} | Write-Output
if($log -match '; error:|(?m)^FAIL ROUTE123' -or $log -notmatch 'ROUTE123-FINAL=\d+ FAIL=0'){throw "Route123 failed: $out"}
$ui=Get-Content "$out\route-ui\result.txt" -Raw;Write-Output $ui;if($ui -notmatch '^PASS'){throw "Route123 UI failed: $out"}
$reopen=@('(princ (if (and (= "TIM_RANH" (bht:get (bht:route-read "R123") "loai")) (= 3 (length (bht:get-all (bht:route-read "R123") "moc")))) "\nPASS ROUTE123 reopen-type-and-controls" "\nFAIL ROUTE123 reopen-type-and-controls"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$reopen),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTRoute123 "$out\profile" /i "$out\roundtrip.dwg" /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=[IO.File]::ReadAllText("$out\reopen.log",[Text.Encoding]::Unicode)
if($log -match '; error:|(?m)^FAIL ROUTE123' -or $log -notmatch '(?m)^PASS ROUTE123 reopen'){throw "Route123 reopen failed: $out"}
Write-Output "PASS ROUTE123 reopen: $out"
