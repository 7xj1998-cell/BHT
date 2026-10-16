param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$Inputs,[Parameter(Mandatory=$true)][string]$RuntimePath,[Parameter(Mandatory=$true)][string]$OutDir)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
New-Item -ItemType Directory -Path $OutDir | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll","$BinDir\BHT.Palette.dll" -Destination $OutDir
$drawing=@(Get-ChildItem -LiteralPath $Inputs -Filter '*.dwg' -File)
if($drawing.Count -ne 1){throw 'Exactly one copied DWG fixture required'}
$inputHash=(Get-FileHash -LiteralPath $drawing[0].FullName).Hash
Copy-Item -LiteralPath $drawing[0].FullName -Destination "$OutDir\D15-copy.dwg"
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 /r:System.Windows.Forms.dll /r:System.Drawing.dll "/out:$OutDir\Import112Probe.dll" '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$OutDir\BHT.Core.dll" "/r:$OutDir\BHT.Bridge.dll" "/r:$OutDir\BHT.Palette.dll" "$PSScriptRoot\Import112Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Probe compile failed'}
function BhtPath112([string]$path){$path.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(BhtPath112 "$OutDir\BHT.Bridge.dll")+'"'),
 '(setq *bht-no-launch* T)',('(load "'+(BhtPath112 $RuntimePath)+'")'),'_.NETLOAD',('"'+(BhtPath112 "$OutDir\BHT.Palette.dll")+'"'),
 '_.NETLOAD',('"'+(BhtPath112 "$OutDir\Import112Probe.dll")+'"'))
foreach($arm in @('IMPORT112ARM','IMPORT112CANCEL','IMPORT112EXCEPTION','IMPORT112MISSING')){$lines+=@($arm,'BHTNAPTICHLUY','IMPORT112VERIFY')}
$lines+=@('IMPORT112UI','_.QUIT','_Y')
[IO.File]::WriteAllLines("$OutDir\command.scr",$lines,[Text.UTF8Encoding]::new($true))
$env:BHT111_INPUT=[IO.Path]::GetFullPath($Inputs)
. "$root\scripts\coreconsole.ps1"
$log=Invoke-BhtCoreConsole -Script "$OutDir\command.scr" -Drawing "$OutDir\D15-copy.dwg" -OutputDirectory $OutDir -TimeoutSeconds 120
$log -split "`n" | Where-Object {$_ -match '^PASS|^FAIL|^IMPORT112-'} | Write-Output
if($log -notmatch 'IMPORT112-TOTAL=\d+ FAIL=0' -or (Get-FileHash -LiteralPath $drawing[0].FullName).Hash -ne $inputHash){throw 'Import112 regression failed'}
