param([Parameter(Mandatory=$true)][string]$BinDir, [string]$AcadDir='D:\AutoCAD 2024', [switch]$Legacy)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$version\geometry-"+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $out | Out-Null
$probe=Join-Path $out 'TdtGeometryProbe.dll'
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 "/r:$AcadDir\acdbmgd.dll" "/r:$AcadDir\accoremgd.dll" "/r:$BinDir\BHT.Bridge.dll" "/out:$probe" "$PSScriptRoot\TdtGeometryProbe.cs"
if($LASTEXITCODE -ne 0){throw 'Geometry probe compilation failed'}
$env:BHT_GEOMETRY_REPORT=Join-Path $out 'result.txt'
$env:BHT_GEOMETRY_LEGACY=if($Legacy){'1'}else{'0'}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+"$BinDir\BHT.Bridge.dll".Replace('\','/')+'"'),'_.NETLOAD',('"'+$probe.Replace('\','/')+'"'),'BHTGEOMETRYREGRESSION','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\test.scr",$lines,[Text.UTF8Encoding]::new($true))
& "$AcadDir\accoreconsole.exe" /isolate BHTGeometry112 "$out\profile" /s "$out\test.scr" /l en-US > "$out\console.log"
if(!(Test-Path $env:BHT_GEOMETRY_REPORT)){throw "Missing geometry report: $out"}
$result=Get-Content -LiteralPath $env:BHT_GEOMETRY_REPORT -Raw
$result
if($Legacy){
 if($result -notmatch 'FAIL semicircle-length' -or $result -match '(?m)^ERROR'){throw 'Expected legacy geometry defect not reproduced'}
}elseif($result -notmatch '(?m)^FAILURES=0' -or $result -match '(?m)^(FAIL |ERROR)'){throw "Geometry regression failed: $out"}
Write-Host "REPORT: $out"
