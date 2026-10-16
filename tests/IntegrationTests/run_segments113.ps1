param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$Inputs,[Parameter(Mandatory=$true)][string]$OutDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
New-Item -ItemType Directory -Path $OutDir | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll","$BinDir\BHT.Palette.dll" -Destination $OutDir
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 /langversion:5 /r:System.Windows.Forms.dll /r:System.Drawing.dll '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$OutDir\BHT.Core.dll" "/r:$OutDir\BHT.Bridge.dll" "/r:$OutDir\BHT.Palette.dll" "/out:$OutDir\Segments113Probe.dll" "$PSScriptRoot\Segments113Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Segment probe compile failed'}
$env:BHT113_INPUT=[IO.Path]::GetFullPath($Inputs)
$lines=@('_.NETLOAD',('"'+$OutDir.Replace('\','/')+'/Segments113Probe.dll"'),'SEGMENTS113PROBE','SEG113UNDOSEED','(if (/= 0 (logand 8 (getvar "UNDOCTL"))) (command-s "_.UNDO" "_End"))','_.UNDO','_Auto','_Off','SEG113UNDOAPPLY','_.U','SEG113UNDOVERIFY')
if($RuntimePath){$lines+=@('(setq *bht-no-launch* T)',('(load "'+$RuntimePath.Replace('\','/')+'")'),
 '(setq s113-old ''(("format" . "BHT_SEG_ALIAS/1") ("removed_id" . "QA113_OLD") ("kept_id" . "QA113_KEEP")))',
 '(bht:rec-write "SEG_ALIAS" "QA113" s113-old)',
 '(setq s113-write (vl-catch-all-apply ''bht:rec-write (list "SEG_ALIAS" "QA113" ''(("tamper" . "yes")))))',
 '(setq s113-delete (vl-catch-all-apply ''bht:rec-delete (list "SEG_ALIAS" "QA113")))',
 '(princ (if (and (vl-catch-all-error-p s113-write) (vl-catch-all-error-p s113-delete) (equal s113-old (bht:rec-read "SEG_ALIAS" "QA113"))) "\nSEG113_LISP=PASS" "\nSEG113_LISP=FAIL"))')}
$lines+=@('_.QUIT','_Y')
[IO.File]::WriteAllLines("$OutDir\segments.scr",$lines,[Text.UTF8Encoding]::new($true))
. "$root\scripts\coreconsole.ps1"
$log=Invoke-BhtCoreConsole -Script "$OutDir\segments.scr" -OutputDirectory $OutDir -TimeoutSeconds 180
$log -split "`n" | Where-Object {$_ -match '^PASS SEG113|^FAIL SEG113|^SEG113_'} | Write-Output
if($log -notmatch 'SEG113_TOTAL=\d+ FAIL=0' -or $log -notmatch 'SEG113_UNDO=PASS'){throw 'Segment113 regression failed'}
if($RuntimePath -and $log -notmatch '(?m)^SEG113_LISP=PASS'){throw 'Segment113 Lisp journal guard failed'}
