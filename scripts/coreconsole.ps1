function Invoke-BhtCoreConsole {
 param([string]$AcadDir='D:\AutoCAD 2024',[Parameter(Mandatory=$true)][string]$Script,[string]$Drawing='',
       [Parameter(Mandatory=$true)][string]$OutputDirectory,[string]$Profile='BHTResearch111',[int]$TimeoutSeconds=180)
 $ErrorActionPreference='Stop'
 $out=[IO.Path]::GetFullPath($OutputDirectory)
 New-Item -ItemType Directory -Force -Path $out | Out-Null
 $label=[IO.Path]::GetFileNameWithoutExtension($Script)
 $args=@('/p',$Profile,'/isolate','BHTLocalCheck',('"'+$out+'\profile"'),'/s',('"'+[IO.Path]::GetFullPath($Script)+'"'),'/l','en-US')
 if($Drawing){$args+=@('/i',('"'+[IO.Path]::GetFullPath($Drawing)+'"'))}
 $process=Start-Process -FilePath "$AcadDir\accoreconsole.exe" -ArgumentList $args -WindowStyle Hidden -PassThru -RedirectStandardOutput "$out\$label-console.log" -RedirectStandardError "$out\$label-stderr.log"
 # Windows PowerShell 5 can lose ExitCode after polling a process whose native
 # handle was never opened. Keep that handle before the process exits.
 $processHandle=$process.Handle
 $deadline=(Get-Date).AddSeconds($TimeoutSeconds)
 try {
  while(!$process.HasExited){
   $reporters=Get-CimInstance Win32_Process -Filter "Name='senddmp.exe'" | Where-Object {$_.CommandLine -and $_.CommandLine.Contains($out)}
   if($reporters){foreach($reporter in $reporters){Stop-Process -Id $reporter.ProcessId -Force};throw "AutoCAD crashed: $out"}
   if((Get-Date) -gt $deadline){throw "AutoCAD timed out: $out"}
   Start-Sleep -Milliseconds 750;$process.Refresh()
  }
  $process.WaitForExit()
  $log=[IO.File]::ReadAllText("$out\$label-console.log",[Text.Encoding]::Unicode).Replace([string][char]0,'')
  [IO.File]::WriteAllText("$out\$label-decoded.log",$log,[Text.UTF8Encoding]::new($true))
  [IO.File]::WriteAllText("$out\$label-exit.txt",[string]$process.ExitCode)
  if($process.ExitCode -ne 0 -or $log -match '; error:|FATAL ERROR|Exception:|(?m)^FAIL '){throw "AutoCAD check failed (exit $($process.ExitCode)): $out\$label-decoded.log"}
  return $log
 } finally {
  if(!$process.HasExited){Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue}
  Get-CimInstance Win32_Process -Filter "Name='senddmp.exe'" | Where-Object {$_.CommandLine -and $_.CommandLine.Contains($out)} | ForEach-Object {Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue}
 }
}
