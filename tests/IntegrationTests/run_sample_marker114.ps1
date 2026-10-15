param([Parameter(Mandatory=$true)][string]$BinDir,[Parameter(Mandatory=$true)][string]$SamplePath,[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim()
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
$out=Join-Path $root ("build\v$v\sample-marker114-"+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $out | Out-Null
$hash=(Get-FileHash -LiteralPath $SamplePath).Hash
$copy=Join-Path $out 'BHT-coc-tieu-1.0.14.dwg'
Copy-Item -LiteralPath $SamplePath -Destination $copy
function LP($p){$p.Replace('\','/')}
$lines=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),
  ('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'),
  ('(load "'+(LP "$PSScriptRoot\test_SAMPLE_MARKER114.lsp")+'")'),'_.QSAVE','_.QUIT')
[IO.File]::WriteAllLines("$out\test.scr",$lines,[Text.UTF8Encoding]::new($true))
& 'D:\AutoCAD 2024\accoreconsole.exe' /isolate BHTSampleMarker114 "$out\profile" /i $copy /s "$out\test.scr" /l en-US > "$out\console.log"
$log=[IO.File]::ReadAllText("$out\console.log",[Text.Encoding]::Unicode)
$log -split "`n" | Where-Object {$_ -match '^SAMPLE-MARKER114|^\("OK"|^\("LOI"'}
if((Get-FileHash -LiteralPath $SamplePath).Hash -ne $hash){throw 'Original drawing changed'}
if($log -match '; error:' -or $log -notmatch '(?m)^SAMPLE-MARKER114 markers=[1-9]\d* oriented=[1-9]\d* ms=\d+ FAIL=0'){throw "Sample failed: $out"}
Write-Host "PASS sample markers; original unchanged; DWG: $copy"
