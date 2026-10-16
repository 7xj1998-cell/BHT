# 5. Build và tiếp tục phát triển

## Chuẩn bị

Clone nhánh releases/source-1.1.5. Đọc AGENTS.md, VERSION, START_HERE_AI và mục kiểm thử. Kiểm tra Git root/status trước sửa. Không cần quay lại bản1.0.30 trên main để làm nền cho bản1.1.5.

Windows/.NET Framework4.8, compiler C#5 hoặc .NET SDK có reference assemblies; AutoCAD2024 cần để build Bridge/Palette và chạy Core Console. Không chép acdbmgd/accoremgd/acmgd.dll vào Git. Các thư viện biển và phông không thuộc MIT; mã nguồn có sẵn tài nguyên ảnh/vector cần compile và mẫu đèn, còn đóng gói đầy đủ cần bản tài nguyên được phép phân phối tại máy. Đọc COPYING_SCOPE.md.

## Lệnh có sẵn

Chạy tại Git root. Sửa đường dẫn AutoCAD cho máy thực tế.

```powershell
# Core không cần AutoCAD; CI chỉ xác nhận mức này.
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -CoreOnly -UseCsc -Test -OutDir build/core-check/bin

# Build plugin bằng tham chiếu AutoCAD tại máy.
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -AcadDir 'D:\AutoCAD 2024' -UseCsc -Test -OutDir build/source-check/bin

# Kiểm tra đồng bộ version.
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/check-version.ps1
```

Không có DLL AutoCAD thì build script báo SKIP Bridge/Palette. Không diễn giải BUILD XONG của CoreOnly thành build plugin thành công. Dùng output mới để tránh DLL cũ gây nhầm. Xem BHT.sln, Directory.Build.props và BHT.CoreTests.csproj nếu dùng SDK.

```powershell
# Các runner AutoCAD thực tế; đọc tham số trước khi chạy.
Get-Help tests/IntegrationTests/run_progress125.ps1 -Full
Get-Help tests/IntegrationTests/run_incremental131.ps1 -Full
Get-Help tests/IntegrationTests/run_correction132.ps1 -Full
Get-Help tests/IntegrationTests/run_scope114.ps1 -Full
Get-Help tests/IntegrationTests/run_layers115.ps1 -Full
```

Probe mới cần BinDir đúng bản, RuntimePath đúng LSP/FAS, output riêng. Layers115 còn cần InputPackage riêng được phép thử. Runner dùng bản sao, profile cô lập và timeout; không dùng bản khảo sát gốc làm nơi ghi. `run_all` hoặc `run_signs` có thể cần ADS/TDT/fixture và có lỗi baseline đã biết; phải báo FAIL/BLOCKED cụ thể.

## Chuỗi đóng gói

1. Bump bằng scripts/set-version.ps1 khi thay chức năng; viết CHANGELOG/hướng dẫn và cập nhật tài liệu hiện hành.
2. Build private + CoreTests; compile-lisp.ps1 tạo FAS; protect-dotnet.ps1 cần Obfuscar2.2.50 nếu muốn runtime protected.
3. Chạy Core Console bằng chính DLL/FAS chuẩn bị đóng gói; đối chiếu record/RTK/CAD canonical, U/REDO, save/reopen, import/export lặp và lỗi giữa giao dịch.
4. scripts/package.ps1 kiểm version/hash và RELEASE_STATE, tạo ZIP+EXE. Không sửa nội dung đã phát hành dưới cùng số. `-IncludeSource` là tùy chọn; công bố nguồn lần này dùng snapshot riêng không tái đóng bộ cài.
5. Cài khi CAD đã đóng và so hash3DLL+FAS. Test GUI tách riêng khỏi Core Console.
6. Gom hướng dẫn/bộ cài cũ vào old, có manifest đối chiếu; giữ mã test và bằng chứng cần thiết. Không xóa hồ sơ hay test chỉ để giảm số lỗi.

Nguồn công bố có thêm giấy phép và handoff sau khi bộ cài1.1.5 được đóng. Vì fingerprint đóng gói bao gồm docs, chạy package lại1.1.5 từ kho này có thể bị RELEASE_STATE chặn. Đây là bảo vệ đúng; không xóa/bỏ kiểm tra fingerprint để phát hành lại. Build để kiểm tra nguồn vẫn được; bản thay đổi phải lên phiên bản mới.

## Quy trình điều tra lỗi

Ghi nhận VERSION runtime và DLL/FAS thực tế; có thể máy còn nạp DLL cũ trong phiên CAD. Chụp/đọc hộp đang chặn, kiểm log trước/sau chọn tệp. Tạo fixture nhỏ nhất và snapshot trước. Tái hiện baseline rồi sửa phạm vi nhỏ, chạy lại cùng test. Khi import phải kiểm tra cả preflight và writer. Khi sửa survey phải kiểm cả Lisp và C# guard. Khi sửa presentation phải đối chiếu RTK/lý trình không đổi.

Đừng giả định .bht mới nhất theo tên hoặc ngày là đúng. Đối chiếu hash, số record, danh tính và ancestry. Không dùng dữ liệu đã sửa thử làm fixture nguồn cho lần sau. Ghi rõ test nào chạy trên runtime hiện tại, test nào chỉ có bằng chứng lịch sử.
