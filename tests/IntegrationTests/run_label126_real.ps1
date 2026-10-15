param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$Drawing,[string]$AcadProfile='BHTResearch111')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$v\label126-real-"+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $out | Out-Null
$before=(Get-FileHash -LiteralPath $Drawing -Algorithm SHA256).Hash
Copy-Item -LiteralPath $Drawing -Destination "$out\label-check.dwg"
function LabelPath([string]$p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LabelPath "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LabelPath "$root\src\lisp")+'")'),('(load "'+(LabelPath "$root\src\lisp\BHT-$v.lsp")+'")'),('(load "'+(LabelPath "$PSScriptRoot\test_LABEL126_REAL.lsp")+'")'))
$env:BHT_LABEL126_SNAPSHOT="$out\snapshot.lsp"
foreach($phase in @('WRITE','REOPEN')){
 $env:BHT_LABEL126_PHASE=$phase
 $lines=$boot
 if($phase -eq 'WRITE'){$lines+=@('_.QSAVE')}
 $lines+=@('_.QUIT','_Y')
 [IO.File]::WriteAllLines("$out\$phase.scr",$lines,[Text.UTF8Encoding]::new($true))
 & 'D:\AutoCAD 2024\accoreconsole.exe' /p $AcadProfile /isolate BHTLabel126 "$out\profile" /i "$out\label-check.dwg" /s "$out\$phase.scr" /l en-US > "$out\$phase.log"
 $log=[IO.File]::ReadAllText("$out\$phase.log",[Text.Encoding]::Unicode)
 $log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) L126REAL|^L126REAL-'} | Write-Output
 if($log -match '; error:|(?m)^FAIL L126REAL' -or $log -notmatch 'L126REAL-CHECKS=\d+ FAIL=0'){throw "Label126 real drawing check failed: $out"}
}
if((Get-FileHash -LiteralPath $Drawing -Algorithm SHA256).Hash -ne $before){throw 'Original drawing changed during isolated checks'}
Write-Output "PASS real drawing original unchanged: $before"
Write-Output "Label126 artifacts: $out"
