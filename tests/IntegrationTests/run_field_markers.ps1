param([string]$AcadDir='D:\AutoCAD 2024',[string]$BinDir='',[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$version"
if(!$BinDir){$BinDir="$out\bin"};if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$version.lsp"}
function LP([string]$p){$p.Replace('\','/')}
$dwg=Join-Path $out ('field-'+[guid]::NewGuid().ToString('N')+'.dwg')
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),
 ('(load "'+(LP $RuntimePath)+'")'),('(load "'+(LP "$PSScriptRoot\test_FIELD_MARKERS.lsp")+'")'), '_.QSAVE',('"'+(LP $dwg)+'"'),'_.QUIT','_Y')
$scr="$out\field.scr";$log="$out\field.log"
[IO.File]::WriteAllLines($scr,$lines,[Text.UTF8Encoding]::new($true))
& "$AcadDir\accoreconsole.exe" /isolate BHTField "$out\field-profile" /s $scr /l en-US > $log
$text=[IO.File]::ReadAllText($log,[Text.Encoding]::Unicode)
$text -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) FIELD|^FIELD-FINAL'} | ForEach-Object {Write-Host $_}
if($text -notmatch 'FIELD-FINAL-FAIL=0' -or $text -match 'FAIL FIELD|; error:'){throw 'Field-marker integration failed; see field.log'}

& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 "/r:$AcadDir\acdbmgd.dll" "/r:$AcadDir\accoremgd.dll" "/r:$BinDir\BHT.Bridge.dll" "/r:$BinDir\BHT.Core.dll" "/out:$BinDir\FieldApiProbe.dll" "$PSScriptRoot\FieldApiProbe.cs"
if($LASTEXITCODE -ne 0){throw 'Field API probe compile failed'}
$reopen=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'),
 '(princ (if (and (bht:field-mode-p) (= (bht:get (bht:obj-read "SIGN") "field_used_a") "FA") (= (car (bht:api-object-station "SIGN")) "OK") (equal (bht:get-all (bht:rec-read "FIELD_CHAIN" "FIELD1") "marker") (quote ("FA" "FB" "FC" "FD" "FE")))) "\nFIELD-REOPEN-PASS" "\nFIELD-REOPEN-FAIL"))',
 '_.UNDO','_End',
 '_.NETLOAD',('"'+(LP "$BinDir\FieldApiProbe.dll")+'"'),
 '(setq undo-before (bht:obj-read "SIGN"))','BHTFIELDAPIPROBE','_.UNDO','1',
 '(princ (if (equal undo-before (bht:obj-read "SIGN")) "\nFIELD-UNDO-PASS" "\nFIELD-UNDO-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\field-reopen.scr",$reopen,[Text.UTF8Encoding]::new($true))
& "$AcadDir\accoreconsole.exe" /isolate BHTField "$out\field-profile" /i $dwg /s "$out\field-reopen.scr" /l en-US > "$out\field-reopen.log"
$text=[IO.File]::ReadAllText("$out\field-reopen.log",[Text.Encoding]::Unicode)
if($text -notmatch '(?m)^FIELD-REOPEN-PASS' -or $text -notmatch '(?m)^FIELD-UNDO-PASS' -or $text -match '; error:'){throw 'Field-marker persistence/undo failed'}
Write-Host 'PASS FIELD save/reopen and one-step undo'
