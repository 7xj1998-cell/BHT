param([string]$BinDir,[string]$RuntimePath="",[string]$AcadProfile='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content (Join-Path $root 'VERSION') -Raw).Trim();$out=Join-Path $root "build/v$version"
function TestLispPath($p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(TestLispPath "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(TestLispPath "$root\src\lisp")+'")'),('(load "'+(TestLispPath "$root\src\lisp\BHT-$version.lsp")+'")'),('(load "'+(TestLispPath "$PSScriptRoot\test_SCALE652.lsp")+'")'),('(load "'+(TestLispPath "$PSScriptRoot\test_LABEL126.lsp")+'")'),('(load "'+(TestLispPath "$PSScriptRoot\test_LABEL129.lsp")+'")'),'_.QUIT','_Y')
if($RuntimePath){$lines=@($lines | ForEach-Object {$_.Replace(("$root\src\lisp\BHT-$version.lsp").Replace('\','/'),$RuntimePath.Replace('\','/'))})}
$scr=Join-Path $out 'scale652.scr';[IO.File]::WriteAllLines($scr,$lines,[Text.UTF8Encoding]::new($true))
$log=Join-Path $out 'scale652.log'
$profileArgs=@();if($AcadProfile){$profileArgs=@('/p',$AcadProfile)}
& 'D:\AutoCAD 2024\accoreconsole.exe' @profileArgs /isolate BHTScale652 (Join-Path $out 'scale652-profile') /s $scr /l en-US > $log
$s=[IO.File]::ReadAllText($log,[Text.Encoding]::Unicode)
$s -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) (S652|L126|L129)|^(S652|L126|L129)-'} | ForEach-Object {Write-Host $_}
if($s -notmatch 'L129-FAIL=0' -or $s -notmatch 'L126-FAIL=0' -or $s -notmatch 'S652-FAIL=0' -or $s -match '(?m)^FAIL (S652|L126|L129)|; error:'){throw 'Sign scale/label checks failed'}
