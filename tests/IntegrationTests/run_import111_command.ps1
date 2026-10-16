param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$Inputs,[Parameter(Mandatory=$true)][string]$RuntimePath,[Parameter(Mandatory=$true)][string]$OutDir)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
New-Item -ItemType Directory -Path $OutDir | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll","$BinDir\BHT.Palette.dll" -Destination $OutDir
$drawing=@(Get-ChildItem -LiteralPath $Inputs -Filter '*.dwg' -File)
if($drawing.Count -ne 1){throw 'Use copied D15 fixture directory with exactly one DWG'}
$inputHash=(Get-FileHash -LiteralPath $drawing[0].FullName).Hash
Copy-Item -LiteralPath $drawing[0].FullName -Destination "$OutDir\D15-copy.dwg"
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 "/out:$OutDir\Import111CommandProbe.dll" '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$OutDir\BHT.Core.dll" "/r:$OutDir\BHT.Bridge.dll" "$PSScriptRoot\Import111CommandProbe.cs"
if($LASTEXITCODE -ne 0){throw 'Probe compile failed'}
function BhtImport111Path([string]$path){$path.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','(setvar "FILEDIA" 0)','_.NETLOAD',('"'+(BhtImport111Path "$OutDir\BHT.Bridge.dll")+'"'),
 '(setq *bht-no-launch* T)',('(load "'+(BhtImport111Path $RuntimePath)+'")'),'_.NETLOAD',('"'+(BhtImport111Path "$OutDir\BHT.Palette.dll")+'"'),
 '_.NETLOAD',('"'+(BhtImport111Path "$OutDir\Import111CommandProbe.dll")+'"'),'IMPORT111ARM','BHTNAPTICHLUY',(BhtImport111Path "$Inputs\DOAN14.bht"),'IMPORT111VERIFY','_.QUIT','_Y')
[IO.File]::WriteAllLines("$OutDir\command.scr",$lines,[Text.UTF8Encoding]::new($true))
. "$root\scripts\coreconsole.ps1"
$log=Invoke-BhtCoreConsole -Script "$OutDir\command.scr" -Drawing "$OutDir\D15-copy.dwg" -OutputDirectory $OutDir -TimeoutSeconds 120
$log -split "`n" | Where-Object {$_ -match '^PASS|^FAIL|^IMPORT111-|^BHT:'} | Write-Output
if($log -notmatch 'IMPORT111-TOTAL=2 FAIL=0' -or (Get-FileHash -LiteralPath $drawing[0].FullName).Hash -ne $inputHash){throw 'Public command regression failed'}
