param([string]$BinDir,[string]$RuntimePath,[string]$Drawing='',[string]$Fixture='test_REGRESSION110.lsp',[string]$OutDir='',[switch]$Baseline)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
if(!$BinDir){$BinDir="$root\build\v$v\bin"}
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
if(!$OutDir){$OutDir="$root\build\v$v\regression110-$([guid]::NewGuid().ToString('N'))"}
New-Item -ItemType Directory -Path $OutDir | Out-Null
function BhtLispPath110([string]$p){$p.Replace('\','/')}
$copy=''
if($Drawing){
 $hash=(Get-FileHash -LiteralPath $Drawing).Hash
 $copy=Join-Path $OutDir 'test-copy.dwg'
 Copy-Item -LiteralPath $Drawing -Destination $copy
 if($Fixture -eq 'test_REOPEN110.lsp'){Copy-Item -LiteralPath (Join-Path (Split-Path -Parent $Drawing) 'qa110-saved-state.lsp') -Destination $OutDir}
 if((Get-FileHash -LiteralPath $copy).Hash -ne $hash){throw 'Copy mismatch'}
 @{source=$Drawing;sha256=$hash;copy=$copy}|ConvertTo-Json|Set-Content "$OutDir\input-proof.json" -Encoding UTF8
}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(BhtLispPath110 "$BinDir\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(BhtLispPath110 "$root\src\lisp")+'" qa110-baseline '+$(if($Baseline){'T'}else{'nil'})+')'),
 ('(load "'+(BhtLispPath110 $RuntimePath)+'")'),('(load "'+(BhtLispPath110 "$PSScriptRoot\$Fixture")+'")'),'_.QUIT','_Y')
[IO.File]::WriteAllLines("$OutDir\test.scr",$lines,[Text.UTF8Encoding]::new($true))
$env:BHT_SIGN_ROOT="$env:APPDATA\Autodesk\ApplicationPlugins\BHT.bundle\Contents\Windows\SignLibrary\BHT"
$env:BHT_QA110_TESTS=BhtLispPath110 $PSScriptRoot
. "$root\scripts\coreconsole.ps1"
try {
 $log=Invoke-BhtCoreConsole -Script "$OutDir\test.scr" -Drawing $copy -OutputDirectory $OutDir -TimeoutSeconds 300
 $log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL|BASELINE|QA110)'} | Write-Output
 if($log -notmatch 'QA110-DONE'){throw 'Test did not complete'}
 if($Fixture -eq 'test_REOPEN110.lsp'){
  if((Get-FileHash -LiteralPath "$OutDir\qa110-saved-state.lsp").Hash -ne (Get-FileHash -LiteralPath "$OutDir\qa110-reopened-state.lsp").Hash){throw 'Reopened drawing differs from saved canonical snapshot'}
  'PASS QA110 saved-dwg-reopens-canonical-byte-for-byte'
 }
} finally {
 if($Drawing -and (Get-FileHash -LiteralPath $Drawing).Hash -ne $hash){throw 'Source drawing changed'}
}
Write-Output "Evidence: $OutDir"
