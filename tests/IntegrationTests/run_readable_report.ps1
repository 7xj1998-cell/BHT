param([string]$AcadDir='D:\AutoCAD 2024',[string]$BinDir='',[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$version"
if(!$BinDir){$BinDir="$out\bin"};if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$version.lsp"}
function LP([string]$p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),
 ('(load "'+(LP $RuntimePath)+'")'),('(load "'+(LP "$PSScriptRoot\test_READABLE_REPORT.lsp")+'")'),'_.QUIT','_Y')
$scr="$out\readable-report.scr";$log="$out\readable-report.log"
[IO.File]::WriteAllLines($scr,$lines,[Text.UTF8Encoding]::new($true))
& "$AcadDir\accoreconsole.exe" /isolate BHTReadableReport "$out\report-profile" /s $scr /l en-US > $log
$text=[IO.File]::ReadAllText($log,[Text.Encoding]::Unicode)
$text -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) REPORT|^REPORT-PASS'} | ForEach-Object {Write-Host $_}
if($text -notmatch 'REPORT-FAIL=0' -or $text -match 'FAIL REPORT|; error:'){throw 'Readable-report integration failed; see readable-report.log'}
