param([string]$BinDir='', [string]$AcadDir='D:\AutoCAD 2024')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$version=(Get-Content "$root\VERSION" -Raw).Trim()
if(!$BinDir){$BinDir="$root\build\v$version\bin"}
$out="$root\build\v$version\copy110"
New-Item -ItemType Directory -Force $out | Out-Null
$env:BHT_SIGN_ROOT="$root\ads_library_local\TrafficSignal"
$env:BHT_REVIEW_OUT=$out
$env:BHT_QA_OUTPUT="$out\stakes.txt"
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 "/r:$AcadDir\acdbmgd.dll" "/r:$AcadDir\accoremgd.dll" "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "/out:$BinDir\Copy110Probe.dll" "$PSScriptRoot\Copy110Probe.cs" "$PSScriptRoot\TdtStakeProbe.cs"
if($LASTEXITCODE -ne 0){throw 'Copy probe compilation failed'}
$report="$out\copy-review.txt"
if(Test-Path -LiteralPath $report){Remove-Item -LiteralPath $report}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+"$BinDir\BHT.Bridge.dll".Replace('\','/')+'"'),'_.NETLOAD',('"'+"$BinDir\Copy110Probe.dll".Replace('\','/')+'"'),'BHTCOPYREVIEW','BHTSTAKEREGRESSION','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\test.scr",$lines,[Text.UTF8Encoding]::new($true))
& "$AcadDir\accoreconsole.exe" /isolate BHTCopy110 "$out\profile" /s "$out\test.scr" /l en-US > "$out\console.log"
$result=Get-Content -LiteralPath $report -Raw
$result
if($result -match '(?m)^(FAIL|ERROR|IMPORT_FAIL)' -or ([regex]::Matches($result,'(?m)^PASS ')).Count -lt 32){throw 'Copy regression failed'}
$stakes=Get-Content "$out\stakes.txt" -Raw
if($stakes -notmatch 'FAILURES=0'){throw 'Stake regression failed'}
