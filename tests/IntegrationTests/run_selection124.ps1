param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$Drawing,[string]$RuntimePath='',[string]$AcadProfile='')
$profileArgs=@();if($AcadProfile){$profileArgs=@('/p',$AcadProfile)}
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$v\selection124-"+[guid]::NewGuid().ToString('N'));New-Item -ItemType Directory $out | Out-Null
$BinDir=[IO.Path]::GetFullPath($BinDir)
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll","$BinDir\BHT.Palette.dll" -Destination $out
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}else{$RuntimePath=[IO.Path]::GetFullPath($RuntimePath)}
Copy-Item -LiteralPath $Drawing -Destination "$out\input.dwg"
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 "/out:$out\Selection124Probe.dll" /r:System.Windows.Forms.dll /r:System.Drawing.dll '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "/r:$BinDir\BHT.Palette.dll" "$PSScriptRoot\Selection124Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Selection124 probe compile failed'}
function TestLispPath([string]$p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(TestLispPath "$BinDir\BHT.Bridge.dll")+'"'),'_.NETLOAD',('"'+(TestLispPath "$BinDir\BHT.Palette.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(TestLispPath "$root\src\lisp")+'")'),('(load "'+(TestLispPath $RuntimePath)+'")'),'_.NETLOAD',('"'+(TestLispPath "$out\Selection124Probe.dll")+'"'),'SELECTION124PROBE',('(load "'+(TestLispPath "$PSScriptRoot\test_SELECTION124.lsp")+'")'),'_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\test.scr",$lines,[Text.UTF8Encoding]::new($true))
$uiLines=@($lines | Select-Object -First 9)+@('SELECTION124PROBE','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\ui.scr",$uiLines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' @profileArgs /isolate BHTSelection124 "$out\profile" /s "$out\ui.scr" /l en-US > "$out\ui.log"
$realLines=@($lines | Where-Object {$_ -ne 'SELECTION124PROBE'} | Select-Object -SkipLast 2)
$realLines+=@('(if (/= 0 (logand 8 (getvar "UNDOCTL"))) (command-s "_.UNDO" "_End"))','_.UNDO','_Auto','_Off',
 '(setq q124-old-pick bht:pick-object q124-old-load bht:palette-load q124-old-ui BHTSEGMENTPICK q124-command-before (bht:obj-read "OBJ-000058"))',
 '(defun bht:pick-object (prompt) "OBJ-000058")','(defun bht:palette-load () T)','(defun BHTSEGMENTPICK (id) q124-ui-result)',
 '(setq q124-ui-result ''("CANCEL"))','BHTGANDOAN','(q124-check "public-picker-cancel-keeps-record" (equal q124-command-before (bht:obj-read "OBJ-000058")))',
 '(setq q124-ui-result ''("SELECT" "DOAN_10"))','BHTGANDOAN','(q124-check "public-picker-assigns-exact-selected-ID" (= (bht:get (bht:obj-read "OBJ-000058") "doan") "DOAN_10"))',
 '_.U','(q124-check "public-picker-single-undo-restores-record" (equal q124-command-before (bht:obj-read "OBJ-000058")))',
 '(setq q124-ui-result ''("CLEAR"))','BHTGANDOAN','(q124-check "public-picker-clear-removes-assignment" (= (bht:get (bht:obj-read "OBJ-000058") "gan_doan_pp") "CHUA_PHAN_DOAN"))',
 '_.U','(q124-check "public-picker-clear-undo-restores-record" (equal q124-command-before (bht:obj-read "OBJ-000058")))',
 '(setq bht:pick-object q124-old-pick bht:palette-load q124-old-load BHTSEGMENTPICK q124-old-ui)',
 '(princ (strcat "\nSELECTION124-FINAL=" (itoa q124-count) " FAIL=" (itoa q124-fail)))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\test.scr",$realLines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' @profileArgs /isolate BHTSelection124 "$out\profile" /i "$out\input.dwg" /s "$out\test.scr" /l en-US > "$out\console.log"
$log=[IO.File]::ReadAllText("$out\console.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) SELECTION124|^SELECTION124-'} | Write-Output
if($log -match '; error:|(?m)^FAIL SELECTION124' -or $log -notmatch 'SELECTION124-FINAL=\d+ FAIL=0'){throw "Selection124 failed: $out"}
$ui=Get-Content "$out\selection124-result.txt" -Raw;Write-Output $ui
if($ui -match 'FAIL' -or ([regex]::Matches($ui,'(?m)^PASS')).Count -lt 30){throw "Selection124 UI/CAD failed: $out"}
Write-Output "PASS SELECTION124: $out"
