param([string]$AcadDir='D:\AutoCAD 2024',[string]$BinDir='',[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$version"
if(!$BinDir){$BinDir="$out\bin"};if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$version.lsp"}
function LP([string]$p){$p.Replace('\','/')}
$dwg=Join-Path $out ('input-'+[guid]::NewGuid().ToString('N')+'.dwg')
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),
 ('(load "'+(LP $RuntimePath)+'")'),('(load "'+(LP "$PSScriptRoot\test_IMPORTED_MARKERS.lsp")+'")'), '_.QSAVE',('"'+(LP $dwg)+'"'),'_.QUIT','_Y')
$scr="$out\input.scr";$log="$out\input.log"
[IO.File]::WriteAllLines($scr,$lines,[Text.UTF8Encoding]::new($true))
& "$AcadDir\accoreconsole.exe" /isolate BHTInput "$out\input-profile" /s $scr /l en-US > $log
$text=[IO.File]::ReadAllText($log,[Text.Encoding]::Unicode)
$text -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) INPUT|^INPUT-FINAL'} | ForEach-Object {Write-Host $_}
if($text -notmatch 'INPUT-FINAL-FAIL=0' -or $text -match 'FAIL INPUT|; error:'){throw 'Imported-marker integration failed; see input.log'}

& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 "/r:$AcadDir\acdbmgd.dll" "/r:$AcadDir\accoremgd.dll" "/r:$BinDir\BHT.Bridge.dll" "/r:$BinDir\BHT.Core.dll" "/out:$BinDir\ImportedApiProbe.dll" "$PSScriptRoot\ImportedApiProbe.cs"
if($LASTEXITCODE -ne 0){throw 'Imported API probe compile failed'}
$reopen=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'),
 '(princ (if (and (= "BHT_IMPORTED_MARKERS" (bht:meta "field_active_chain" "")) (= "OK" (car (bht:api-object-station "SIGN"))) (bht:field-manual-approved (bht:obj-read "HAND") (bht:pt-all))) "\nINPUT-REOPEN-PASS" "\nINPUT-REOPEN-FAIL"))',
 '_.NETLOAD',('"'+(LP "$BinDir\ImportedApiProbe.dll")+'"'),'BHTIMPORTEDAPIPROBE','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\input-reopen.scr",$reopen,[Text.UTF8Encoding]::new($true))
& "$AcadDir\accoreconsole.exe" /isolate BHTInput "$out\input-profile" /i $dwg /s "$out\input-reopen.scr" /l en-US > "$out\input-reopen.log"
$text=[IO.File]::ReadAllText("$out\input-reopen.log",[Text.Encoding]::Unicode)
if($text -notmatch '(?m)^INPUT-REOPEN-PASS' -or $text -notmatch '(?m)^INPUT-API-PASS' -or $text -match '; error:'){throw 'Imported-marker reopen/API failed'}
Write-Host 'PASS INPUT save/reopen'
