param([Parameter(Mandatory=$true)][string]$BinDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot); $v=(Get-Content "$root\VERSION" -Raw).Trim()
$out=Join-Path $root ("build\v$v\caption118-"+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $out | Out-Null
Copy-Item -LiteralPath "$BinDir\BHT.Core.dll","$BinDir\BHT.Bridge.dll" -Destination $out
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
$csc="$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /target:library /platform:x64 /langversion:5 "/out:$out\Caption118Probe.dll" '/r:D:\AutoCAD 2024\acdbmgd.dll' '/r:D:\AutoCAD 2024\accoremgd.dll' "/r:$BinDir\BHT.Bridge.dll" "/r:$BinDir\BHT.Core.dll" "$PSScriptRoot\Caption118Probe.cs"
if($LASTEXITCODE -ne 0){throw 'Probe build failed'}
function LP($p){$p.Replace('\','/')}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$out\BHT.Bridge.dll")+'"'),'_.NETLOAD',('"'+(LP "$out\Caption118Probe.dll")+'"'),
 ('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$steps=@(('(load "'+(LP "$PSScriptRoot\test_CAPTION118.lsp")+'")'),'CAPTION118API','CAPTION118LAYOUT','_.QSAVE',('"'+(LP "$out\roundtrip.dwg")+'"'),'_.QUIT')
[IO.File]::WriteAllLines("$out\create.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTCaption118 "$out\profile" /s "$out\create.scr" /l en-US > "$out\create.log"
$log=[IO.File]::ReadAllText("$out\create.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^(PASS|FAIL) CAPTION118|^CAPTION118-'}
if($log -match '; error:|(?m)^FAIL CAPTION118|(?m)^CAPTION118-(API|LAYOUT)-FAIL' -or $log -notmatch 'CAPTION118-CHECKS=\d+ FAIL=0' -or $log -notmatch '(?m)^CAPTION118-API-PASS' -or $log -notmatch '(?m)^CAPTION118-LAYOUT-PASS'){throw "Caption118 failed: $out"}
$steps=@('(princ (if (> (CAPTION118ORDER) 0) "\nCAPTION118-REOPEN-PASS" "\nCAPTION118-REOPEN-FAIL"))','_.QUIT','_Y')
[IO.File]::WriteAllLines("$out\reopen.scr",($boot+$steps),[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTCaption118 "$out\profile" /i "$out\roundtrip.dwg" /s "$out\reopen.scr" /l en-US > "$out\reopen.log"
$log=[IO.File]::ReadAllText("$out\reopen.log",[Text.Encoding]::Unicode)
if($log -match '; error:|(?m)^CAPTION118-REOPEN-FAIL' -or $log -notmatch '(?m)^CAPTION118-REOPEN-PASS'){throw "Caption118 reopen failed: $out"}
Write-Host "PASS CAPTION118 save/reopen: $out"
