param([string]$BinDir,[string]$Tag='after',[string]$Drawing='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out="$root\build\v1.0.0\qa\$Tag"
New-Item -ItemType Directory -Force $out | Out-Null
if(!$Drawing){$Drawing="$root\build\v1.0.0\qa\261011_test-V1.dwg"}
$acad='D:\AutoCAD 2024'
$probe="$BinDir\V1SampleProbe.dll"
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /r:System.Windows.Forms.dll /r:System.Drawing.dll "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "/r:$BinDir\BHT.Palette.dll" "/r:$acad\acmgd.dll" "/r:$acad\accoremgd.dll" "/r:$acad\acdbmgd.dll" "/out:$probe" "$PSScriptRoot\V1SampleProbe.cs"
if($LASTEXITCODE -ne 0){throw 'Probe compilation failed'}
$env:BHT_V1_QA=$out
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+("$BinDir\BHT.Bridge.dll").Replace('\','/')+'"'),'_.NETLOAD',('"'+("$BinDir\BHT.Palette.dll").Replace('\','/')+'"'),'_.NETLOAD',('"'+$probe.Replace('\','/')+'"'),'BHTV1SAMPLE','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\sample.scr",$lines,[Text.UTF8Encoding]::new($true))
& "$acad\accoreconsole.exe" /isolate "BHTV1$Tag" "$out\profile" /i $Drawing /s "$out\sample.scr" /l en-US > "$out\console.log"
Get-Content "$out\sample-perf.txt"
if((Get-Content "$out\sample-perf.txt" -Raw) -match 'FAIL'){throw 'Sample probe failed'}


