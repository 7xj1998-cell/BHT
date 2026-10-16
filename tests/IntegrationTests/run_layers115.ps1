param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$InputPackage,[Parameter(Mandatory=$true)][string]$OutDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$sourceHash=(Get-FileHash -LiteralPath $InputPackage).Hash
$env:BHT115_INPUT=[IO.Path]::GetFullPath($InputPackage)
& "$PSScriptRoot\run_correction132.ps1" -BinDir $BinDir -OutDir $OutDir -RuntimePath $RuntimePath -Probe Layers115Probe -Command LAYERS115PROBE -Fixture "$PSScriptRoot\test_LAYERS115.lsp"
$lines=[IO.File]::ReadAllLines("$OutDir\test.scr")
$undo=@();foreach($line in $lines){if($line -eq 'LAYERS115PROBE'){
 $undo+=@('L115SEED','L115APPLY','_.U','L115UNDO')
}else{$undo+=$line}}
[IO.File]::WriteAllLines("$OutDir\undo.scr",$undo,[Text.UTF8Encoding]::new($true))
. "$root\scripts\coreconsole.ps1"
$log=Invoke-BhtCoreConsole -Script "$OutDir\undo.scr" -OutputDirectory $OutDir
if($log -notmatch '(?m)^PASS LAYERS115 single-undo'){throw 'Layer Undo did not complete'}
if((Get-FileHash -LiteralPath $InputPackage).Hash -ne $sourceHash){throw 'Source BHT changed'}
Write-Output 'PASS LAYERS115 single-undo and source hash unchanged'
