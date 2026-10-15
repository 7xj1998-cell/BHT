param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$ProjectPath)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ('build\v'+$v+'\project116-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $out | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll" -Destination $out
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$out\BHT.Core.dll" "/r:$out\BHT.Bridge.dll" "/out:$out\Project116Probe.dll" "$PSScriptRoot\Project116Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Probe compile failed'}
$env:BHT_PROJECT116_OUT=$out
$env:BHT_PROJECT116_INPUT=$ProjectPath
$lp=$out.Replace('\','/')
$steps=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+$lp+'/BHT.Bridge.dll"'),'_.NETLOAD',('"'+$lp+'/Project116Probe.dll"'),'BHTPROJECT116PROBE','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\run.scr",$steps,[Text.UTF8Encoding]::new($true))
try { & 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTProject116 "$out\profile" /s "$out\run.scr" /l en-US > "$out\run.log" }
finally { Remove-Item Env:BHT_PROJECT116_OUT,Env:BHT_PROJECT116_INPUT -ErrorAction SilentlyContinue }
$results=Get-Content "$out\result.txt"
$results | Write-Output
if($results -match '^FAIL' -or @($results | Where-Object {$_ -match '^PASS'}).Count -lt 40){throw "Project transfer failed: $out"}
Write-Output "PASS PROJECT116: $out"
