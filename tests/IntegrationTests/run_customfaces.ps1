param([string]$BinDir,[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$v=(Get-Content "$root\VERSION" -Raw).Trim();$out="$root\build\v$v"
if(!$RuntimePath){$RuntimePath="$root\src\lisp\BHT-$v.lsp"}
function LP($p){$p.Replace('\','/')}
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:library /platform:x64 "/r:D:\AutoCAD 2024\acdbmgd.dll" "/r:D:\AutoCAD 2024\accoremgd.dll" "/r:$BinDir\BHT.Bridge.dll" "/r:$BinDir\BHT.Core.dll" "/out:$BinDir\CustomFacesProbe.dll" "$PSScriptRoot\CustomFacesProbe.cs"
if($LASTEXITCODE -ne 0){throw 'Compile probe failed'}
$boot=@('(setvar "SECURELOAD" 0)','_.NETLOAD',('"'+(LP "$BinDir\BHT.Bridge.dll")+'"'),'_.NETLOAD',('"'+(LP "$BinDir\CustomFacesProbe.dll")+'"'),('(setq *bht-no-launch* T *bht-module-root* "'+(LP "$root\src\lisp")+'")'),('(load "'+(LP $RuntimePath)+'")'))
$dwg=Join-Path $out ("custom-faces-"+[guid]::NewGuid().ToString("N")+".dwg")
foreach($phase in @('create','reopen')){
 $steps=@('CUSTOMFACECONTENT',('(setq cf (open "'+(LP "$BinDir\custom-content.xml")+'" "r") xml (read-line cf))'),'(close cf)')
 if($phase -eq 'create'){
  $steps+=@('(setq p (entmakex (quote ((0 . "POINT") (10 0.0 0.0 0.0)))))','(bht:pt-write-xdata p "CF-PT" "QA" "1" "1" 0.0 0.0 0.0 "" "" "" "")','(bht:obj-create "CF" (list (cons "nhom" "BIEN_BAO") (cons "ma_hieu" "R.122") (cons "mat" "R.122") (cons "mat" "BHT.TEXT.SAMPLE") (cons "so_tru" "1") (cons "so_mat" "2") (cons "sign_content" xml)) (quote ("CF-PT")) T)','(bht:symbol-sync (quote ("CF")))','CUSTOMFACECHECK','_.QSAVE',('"'+(LP $dwg)+'"'))
 }else{$steps+=@('(princ (if (= (bht:get (bht:obj-read "CF") "sign_content") xml) "\nCUSTOM-STORAGE-PASS" "\nCUSTOM-STORAGE-FAIL"))','CUSTOMFACECHECK')}
 $scr="$out\custom-$phase.scr";[IO.File]::WriteAllLines($scr,($boot+$steps+@('_.QUIT','_Y')),[Text.UTF8Encoding]::new($true))
 $args=@('/isolate','BHTCustom661',"$out\custom-profile",'/s',$scr,'/l','en-US');if($phase -eq 'reopen'){$args+=@('/i',$dwg)}
 & 'D:\AutoCAD 2024\accoreconsole.exe' @args > "$out\custom-$phase.log"
 $log=[IO.File]::ReadAllText("$out\custom-$phase.log",[Text.Encoding]::Unicode)
 if($log -notmatch '(?m)^CUSTOM-FACES-PASS' -or $log -match '; error:|(?m)^CUSTOM-STORAGE-FAIL'){throw "Custom face $phase failed"}
 if($phase -eq 'reopen' -and $log -notmatch '(?m)^CUSTOM-STORAGE-PASS'){throw 'Content not preserved'}
 Write-Output "PASS custom mixed faces $phase"
}
