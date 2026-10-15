param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$Drawing,[string]$RuntimePath='',[string]$Tag='sample')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content "$root\VERSION" -Raw).Trim()
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$version.lsp"}
if($Tag -notmatch '^[a-zA-Z0-9_-]+$'){throw 'Invalid tag'}
$out="$root\build\v$version\qa\$Tag"
New-Item -ItemType Directory -Force $out | Out-Null
$hash=(Get-FileHash -LiteralPath $Drawing).Hash
$copy="$out\BHT-$version-test.dwg"
Copy-Item -LiteralPath $Drawing -Destination $copy -Force
$env:BHT103_SAMPLE=$out.Replace('\','/')
function LP($p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
foreach($phase in @('create','reopen')){
 $env:BHT103_PHASE=$phase
 $steps=@('(load "'+(LP "$PSScriptRoot\test_SAMPLE103.lsp")+'")')
 if($phase -eq 'create'){$steps+='_.QSAVE'}
 [IO.File]::WriteAllLines("$out\$phase.scr",($boot+$steps+@('_.QUIT','_Y')),[Text.UTF8Encoding]::new($true))
 & 'D:\AutoCAD 2024\accoreconsole.exe' /isolate "BHT103$Tag" "$out\profile" /i $copy /s "$out\$phase.scr" /l en-US > "$out\$phase.log"
 $log=Get-Content "$out\$phase.log" -Encoding Unicode -Raw
 if($log -notmatch 'SAMPLE103-COMPLETE' -or $log -match '; error:'){throw "Sample $phase incomplete"}
 $result=Get-Content "$out\$phase.txt" -Raw
 Write-Output $result
 if($result -match '(?m)^FAIL' -or $result -notmatch '(?m)^PASS'){throw "Sample $phase failed"}
}
if($hash -ne (Get-FileHash -LiteralPath $Drawing).Hash){throw 'Source drawing changed'}
Write-Output 'PASS source drawing file unchanged'
