param([string]$BinDir,[Parameter(Mandatory=$true)][string]$Inputs,[string]$OutDir,[string]$Probe='Import111Probe',[string]$Command='IMPORT111PROBE')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content "$root\VERSION" -Raw).Trim()
if(!$BinDir){$BinDir="$root\build\v$version\bin"}
if(!$OutDir){$OutDir="$root\build\v$version\import111-$([guid]::NewGuid().ToString('N'))"}
New-Item -ItemType Directory -Force $OutDir | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll","$BinDir\BHT.Palette.dll" -Destination $OutDir
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 /r:System.Windows.Forms.dll /r:System.Drawing.dll "/out:$OutDir\$Probe.dll" '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$OutDir\BHT.Core.dll" "/r:$OutDir\BHT.Bridge.dll" "/r:$OutDir\BHT.Palette.dll" "$PSScriptRoot\$Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Probe compile failed'}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+$OutDir.Replace('\','/')+'/BHT.Bridge.dll"'),'_.NETLOAD',('"'+$OutDir.Replace('\','/')+'/'+$Probe+'.dll"'),$Command,'_.QUIT','_Y')
[IO.File]::WriteAllLines("$OutDir\test.scr",$lines,[Text.UTF8Encoding]::new($true))
$env:BHT111_INPUT=[IO.Path]::GetFullPath($Inputs)
. "$root\scripts\coreconsole.ps1"
$log=Invoke-BhtCoreConsole -Script "$OutDir\test.scr" -OutputDirectory $OutDir -TimeoutSeconds 180
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) (IMPORT111|TABLES111)|^(IMPORT111|TABLES111)-'} | Write-Output
if($log -notmatch '(IMPORT111|TABLES111)-TOTAL=\d+ FAIL=0' -or $log -match '(IMPORT111|TABLES111)-ERROR'){throw "Probe failed: $OutDir"}
Write-Output "Evidence: $OutDir"
