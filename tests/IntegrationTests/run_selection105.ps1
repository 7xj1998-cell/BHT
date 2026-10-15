param([string]$BinDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop';$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v\qa\selection105"
New-Item -ItemType Directory -Force $out | Out-Null
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library "/r:D:\AutoCAD 2024\acdbmgd.dll" "/r:D:\AutoCAD 2024\accoremgd.dll" "/r:$BinDir\BHT.Core.dll" "/r:$BinDir\BHT.Bridge.dll" "/out:$BinDir\Selection105Probe.dll" "$PSScriptRoot\Selection105Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Selection probe compile failed'}
function LP($p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'),('(load "'+(LP "$PSScriptRoot\test_DISPLAY104.lsp")+'")'),
 '(bht:obj-create "L1" ''(("nhom" . "DEN_CS")) ''("P1") T)',
 '(bht:kh-free-apply "L1" ''(60.0 60.0 0.0) 0.0 nil)',
 '(setq sx-other (bht:insert "BHT_KH_COC_TIEU" ''(150.0 150.0 0.0) "0" 1.0))',
 '(setvar "SELECTSIMILARMODE" 130)','(setvar "PICKFIRST" 0)','_.NETLOAD',('"'+(LP "$BinDir\Selection105Probe.dll")+'"'),
 'BHTCHONBAOHIEU','BHTSELECTION105ALL','BHTCHONTUONGTU','BHTSELECTION105SIMILAR','BHTSELECTSIMILAR','BHTSELECTION105NATIVE','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\run.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSelection105 "$out\profile" /s "$out\run.scr" /l en-US > "$out\run.log"
$result=Get-Content "$BinDir\selection105-result.txt";$result | Write-Output
if($result -match '^FAIL' -or @($result | Where-Object {$_ -match '^PASS'}).Count -ne 12){throw 'Selection regression failed'}
