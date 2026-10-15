param([string]$BinDir,[string]$RuntimePath='',[string]$SamplePath)
$ErrorActionPreference='Stop';$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v\qa\sample-road106"
New-Item -ItemType Directory -Force $out | Out-Null
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
$copy="$out\BOT830-bo-tri-ngoai-duong-"+[guid]::NewGuid().ToString('N')+'.dwg';Copy-Item -LiteralPath $SamplePath -Destination $copy
function LP($p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','(setvar "LISPSYS" 1)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'),('(load "'+(LP "$PSScriptRoot\test_SAMPLE_ROAD106.lsp")+'")'),'_.QSAVE','_.QUIT')
[IO.File]::WriteAllLines("$out\run.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSampleRoad106 "$out\profile" /i $copy /s "$out\run.scr" /l en-US > "$out\run.log"
$log=Get-Content "$out\run.log" -Encoding Unicode -Raw
$log -split "`n" | Where-Object {$_ -match '^SAMPLE-ROAD106'} | Write-Output
if($log -match '; error:|(?m)^SAMPLE-ROAD106-[A-Z-]+-FAIL' -or $log -notmatch 'SAMPLE-ROAD106-CLEARANCE-PASS'){throw 'Sample road layout failed'}
Write-Output "DWG=$copy"
