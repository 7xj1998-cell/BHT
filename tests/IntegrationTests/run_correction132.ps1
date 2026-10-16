param([string]$BinDir,[string]$OutDir,[string]$AcadDir='D:\AutoCAD 2024',[string]$RuntimePath='',[string]$Drawing='',[string]$Fixture='',[string]$Probe='Correction132Probe',[string]$Command='CORRECTION132PROBE')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
if(!$BinDir){$BinDir="$root\build\v$v\bin"}
if(!$OutDir){$OutDir="$root\build\v$v\correction132-$([guid]::NewGuid().ToString('N'))"}
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
if(!$Fixture){$Fixture="$PSScriptRoot\test_CORRECTION132.lsp"}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll","$BinDir\BHT.Palette.dll" -Destination $OutDir
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 /r:System.Windows.Forms.dll /r:System.Drawing.dll "/out:$OutDir\$Probe.dll" "/r:$AcadDir\acdbmgd.dll" "/r:$AcadDir\accoremgd.dll" "/r:$OutDir\BHT.Core.dll" "/r:$OutDir\BHT.Bridge.dll" "/r:$OutDir\BHT.Palette.dll" "$PSScriptRoot\$Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Probe compilation failed'}
function L([string]$path){$path.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(L "$OutDir\BHT.Bridge.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(L "$root\src\lisp")+'")'),('(load "'+(L $RuntimePath)+'")'),
 '_.NETLOAD',('"'+(L "$OutDir\$Probe.dll")+'"'),('(load "'+(L $Fixture)+'")'),
 '(if (/= 0 (logand 8 (getvar "UNDOCTL"))) (command-s "_.UNDO" "_End"))','_.UNDO','_Auto','_Off',$Command,'_.QUIT','_Y')
[IO.File]::WriteAllLines("$OutDir\test.scr",$lines,[Text.UTF8Encoding]::new($true))
$env:BHT_SIGN_ROOT="$env:APPDATA\Autodesk\ApplicationPlugins\BHT.bundle\Contents\Windows\SignLibrary\BHT"
$args=@('/p','BHTResearch111','/isolate','BHTCorrection132',('"'+$OutDir+'\profile"'),'/s',('"'+$OutDir+'\test.scr"'),'/l','en-US')
if($Drawing){
 $inputCopy=Join-Path $OutDir 'input-copy.dwg'
 if(Test-Path -LiteralPath $inputCopy){throw 'Use a fresh output directory for drawing tests'}
 $inputHash=(Get-FileHash -LiteralPath $Drawing).Hash
 Copy-Item -LiteralPath $Drawing -Destination $inputCopy
 if((Get-FileHash -LiteralPath $inputCopy).Hash -ne $inputHash){throw 'Drawing copy hash mismatch'}
 @{source=$Drawing;sha256=$inputHash;copy=$inputCopy}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $OutDir 'input-proof.json') -Encoding UTF8
 $args+=@('/i',('"'+$inputCopy+'"'))
}
$process=Start-Process -FilePath "$AcadDir\accoreconsole.exe" -ArgumentList $args -WindowStyle Hidden -PassThru -RedirectStandardOutput "$OutDir\console.log" -RedirectStandardError "$OutDir\stderr.log"
$processHandle=$process.Handle
$deadline=(Get-Date).AddSeconds(180)
try {
 while(!$process.HasExited){
  $reporters=Get-CimInstance Win32_Process -Filter "Name='senddmp.exe'" | Where-Object {$_.CommandLine -and $_.CommandLine.Contains($OutDir)}
  if($reporters){foreach($r in $reporters){Stop-Process -Id $r.ProcessId -Force};throw 'Owned AutoCAD crash detected; stopping test.'}
  if((Get-Date) -gt $deadline){throw 'AutoCAD timeout'}
  Start-Sleep -Milliseconds 750;$process.Refresh()
 }
 $process.WaitForExit()
 $log=[IO.File]::ReadAllText("$OutDir\console.log",[Text.Encoding]::Unicode).Replace([string][char]0,'')
 [IO.File]::WriteAllText("$OutDir\decoded.log",$log,[Text.UTF8Encoding]::new($true))
 $log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL|CORRECTION132-|; error:)'} | Write-Output
 if($process.ExitCode -ne 0 -or $log -match '; error:|(?m)^FAIL |CORRECTION132-ERROR|Exception:|FATAL ERROR' -or $log -notmatch 'CORRECTION132-TOTAL=\d+ FAIL=0'){throw "Correction test failed: $OutDir"}
} finally {
 if(!$process.HasExited){Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue}
 $ownedPath=[IO.Path]::GetFullPath($OutDir)
 Get-CimInstance Win32_Process -Filter "Name='senddmp.exe'" | Where-Object {$_.CommandLine -and $_.CommandLine.Contains($ownedPath)} | ForEach-Object {Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue}
}
Write-Output "Evidence: $OutDir"
