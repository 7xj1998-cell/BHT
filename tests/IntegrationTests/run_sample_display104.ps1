param([string]$BinDir,[string]$RuntimePath='', [string]$SamplePath, [string]$OutputName='sample-display104')
$ErrorActionPreference='Stop';$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v\qa\$OutputName";New-Item -ItemType Directory -Force $out | Out-Null
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
$copy="$out\BOT830-hien-thi-"+[guid]::NewGuid().ToString('N')+'.dwg';Copy-Item -LiteralPath $SamplePath -Destination $copy
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library "/r:D:\AutoCAD 2024\acdbmgd.dll" "/r:D:\AutoCAD 2024\accoremgd.dll" "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "/out:$BinDir\SampleProject104Probe.dll" "$PSScriptRoot\SampleProject104Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Sample probe compile failed'}
$env:BHT_PROJECT104_OUTPUT=$out
function LP($p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'),
 '(progn (setq qa-pts (bht:pt-all) qa-before (mapcar ''(lambda (pr) (entget (cdr pr) ''("*"))) (bht:tagged-pairs "*" "BHT_KH" 1)) qa-objs (bht:rec-all "OBJ")) (princ))',
 '_.UNDO','_End','_.UNDO','_Auto','_Off',
 '(progn (setq qa-ms (getvar "MILLISECS") qa-result (bht:api-symbol-gap "5")) (princ qa-result))',
 '(princ (strcat "\nSAMPLEDISPLAY104 distance_ms=" (itoa (- (getvar "MILLISECS") qa-ms))))',
 '(princ (if (and (= (car qa-result) "OK") (equal qa-pts (bht:pt-all)) (vl-every ''(lambda (pr) (= (cdr (assoc 8 (entget (cdr pr)))) "BHT_KYHIEU")) (bht:tagged-pairs "*" "BHT_KH" 1))) "\nSAMPLEDISPLAY104-PRESERVE-PASS" "\nSAMPLEDISPLAY104-PRESERVE-FAIL"))',
 '_.U',
 '(princ (if (and (equal qa-objs (bht:rec-all "OBJ")) (vl-every ''(lambda (d) (equal d (entget (cdr (assoc -1 d)) ''("*")) 1e-8)) qa-before)) "\nSAMPLEDISPLAY104-UNDO-PASS" "\nSAMPLEDISPLAY104-UNDO-FAIL"))',
 '(bht:api-symbol-gap "5")','_.NETLOAD',('"'+(LP "$BinDir\SampleProject104Probe.dll")+'"'),'BHTSAMPLEPROJECT104PROBE','_.QSAVE','_.QUIT')
[IO.File]::WriteAllLines("$out\run.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSampleDisplay104 "$out\profile" /i $copy /s "$out\run.scr" /l en-US > "$out\run.log"
$log=Get-Content "$out\run.log" -Encoding Unicode -Raw;$log -split "`n" | Where-Object {$_ -match '^SAMPLEDISPLAY104'} | Write-Output
if($log -match '; error:|(?m)^SAMPLEDISPLAY104-[A-Z]+-FAIL' -or $log -notmatch '(?m)^SAMPLEDISPLAY104-UNDO-PASS'){throw 'Sample display failed'}
$result=Get-Content "$out\project-result.txt";$result | Write-Output;if($result -match '^FAIL'){throw 'Sample project transfer failed'}
Write-Output "DWG-display=$copy"
