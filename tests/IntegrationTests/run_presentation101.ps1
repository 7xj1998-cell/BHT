param([string]$BinDir)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out="$root\build\v1.0.1\qa";$acad='D:\AutoCAD 2024'
$probe="$BinDir\Presentation101Probe.dll"
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 "/r:$acad\acdbmgd.dll" "/r:$acad\accoremgd.dll" "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "/out:$probe" "$PSScriptRoot\Presentation101Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Presentation probe compilation failed'}
$env:BHT101_QA=$out
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+"$BinDir\BHT.Bridge.dll".Replace('\','/')+'"'),'_.NETLOAD',('"'+$probe.Replace('\','/')+'"'),'BHT101SMOKE','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\presentation.scr",$lines,[Text.UTF8Encoding]::new($true))
& "$acad\accoreconsole.exe" /isolate BHT101Presentation "$out\presentation-profile" /s "$out\presentation.scr" /l en-US > "$out\presentation.log"
if(!(Test-Path "$out\presentation.txt")){throw 'Presentation probe did not complete'}
$result=Get-Content "$out\presentation.txt" -Raw;Write-Output $result
if($result -match '(?m)^FAIL'){throw 'Presentation tests failed'}
