param([string]$BinDir,[string]$RuntimePath)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out="$root\build\v1.0.0\qa"
$acad='D:\AutoCAD 2024';$probe="$BinDir\V1CacheProbe.dll"
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "/r:$acad\accoremgd.dll" "/r:$acad\acdbmgd.dll" "/out:$probe" "$PSScriptRoot\V1CacheProbe.cs"
if($LASTEXITCODE -ne 0){throw 'Cache probe compilation failed'}
$lines=@('(setvar "LISPSYS" 1)','(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+("$BinDir\BHT.Bridge.dll").Replace('\','/')+'"'),'_.NETLOAD',('"'+$probe.Replace('\','/')+'"'),'BHTV1CACHE',('(setq *bht-no-launch* T *bht-module-root* "'+("$root\src\lisp").Replace('\','/')+'")'),('(load "'+$RuntimePath.Replace('\','/')+'")'),('(load "'+("$PSScriptRoot\test_V1_INDEX.lsp").Replace('\','/')+'")'),'_.QUIT','_Y')
$lines=$lines[0..($lines.Length-3)]+@(
 '(bht:obj-write "UNDO-QA" ''(("object_id" . "UNDO-QA") ("mo_ta" . "before")))',
 '(princ (strcat "\nCACHE-WARM=" (BHTV1CACHEVALUE)))',
 '_.UNDO','_End',
 '(defun c:V1EDIT () (bht:obj-write "UNDO-QA" ''(("object_id" . "UNDO-QA") ("mo_ta" . "after"))))',
 'V1EDIT',
 '(princ (strcat "\nCACHE-LISP=" (BHTV1CACHEVALUE)))',
 '_.UNDO','1',
 '(princ (strcat "\nCACHE-UNDO=" (BHTV1CACHEVALUE)))',
 '(BHTV1CACHECLOSE)','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\cache-index.scr",$lines,[Text.UTF8Encoding]::new($true))
& "$acad\accoreconsole.exe" /isolate BHTV1cacheIndex "$out\cache-index-profile" /s "$out\cache-index.scr" /l en-US > "$out\cache-index-console.log"
$log=Get-Content "$out\cache-index-console.log" -Encoding Unicode -Raw
$report=Get-Content "$BinDir\cache-test.txt" -Raw
$report
($log -split "`n" | Where-Object {$_ -match '^INDEX-PASS'})
if($report -match 'FAIL' -or $log -notmatch 'INDEX-FAIL=0' -or $log -match '; error:' -or $log -notmatch 'CACHE-WARM=before' -or $log -notmatch 'CACHE-LISP=after' -or $log -notmatch 'CACHE-UNDO=before'){throw 'Cache/index/Undo regression failed'}
Write-Host 'PASS external Lisp edit and native AutoCAD Undo'

