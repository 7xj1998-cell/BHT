# Cài BHT 1.0.30

## Bộ cài một file để gửi

Gửi một file **BHT-Setup-1.0.30.exe**. Người nhận đóng AutoCAD, mở file và bấm **Cài đặt BHT**. File tự giải nén vào thư mục tạm, kiểm tra dữ liệu, cài bundle và phông chữ cho tài khoản Windows hiện tại. Không cần quyền quản trị, không cần giữ file phụ cạnh EXE. Có thể giữ EXE để cài lại.

Máy nhận cần Windows 64-bit, .NET Framework 4.8 và AutoCAD tương thích. Bundle khai báo AutoCAD 2021–2024; bản này chỉ được kiểm tra trên AutoCAD 2024. AutoCAD dùng nền tảng .NET khác cần bản build phù hợp. Bộ cài chứa thư viện DWG/phông của BHT, không cài AutoCAD hoặc TDT. Chỉ khi dùng chức năng lấy thêm dữ liệu từ TDT mới cần phần mềm TDT tương ứng; các block đã lưu trong DWG vẫn được dùng lại.

Khi cài thành công, cửa sổ báo **Đã cài đặt BHT** và chỉ còn nút **Hoàn tất** để đóng. Nút cài đặt được ẩn và khóa để tránh cài lặp trong cùng cửa sổ. Nếu cài lỗi, nút cài đặt vẫn còn để thử lại.

Mở lại AutoCAD và gõ **BHT** hoặc **BTH**. Bộ cài giữ bản BHT cũ trong thư mục backup và từ chối cập nhật khi AutoCAD đang chạy. File EXE chưa có chữ ký Authenticode; không thay đổi thiết lập bảo vệ của Windows.

`BHT-Setup-1.0.30.exe --validate-only --report <đường-dẫn-tệp>` chỉ kiểm tra bộ cài, không cài vào máy. Nhật ký xác minh ghi vào tệp được chỉ định.

## Cách dùng gói ZIP: Application Bundle

1. Giải nén đầy đủ `BHT-1.0.30.zip` và đóng AutoCAD.
2. Chạy `INSTALL_BHT.cmd` trong thư mục đã giải nén. File CMD cần bundle, PowerShell và manifest đi kèm; không gửi riêng CMD.
3. Mở AutoCAD và gõ `BTH` hoặc `BHT`.

AutoCAD tự nhận `BHT.bundle`; không chạy `NETLOAD`. Palette mở bên trái và chỉ có một phiên bản.

## Cách APPLOAD

Trước khi cài, đóng tất cả cửa sổ AutoCAD. DLL .NET đang dùng không thể được thay thế trong phiên AutoCAD hiện tại.

Gói runtime dùng `BHT-1.0.30.fas`, không có LSP hoặc thư mục modules. Khi cần nạp thủ công, giữ nguyên cấu trúc bundle và dùng FAS trong `BHT.bundle/Contents/Windows`; xem hướng dẫn runtime trong `README.md` của gói cài. Cách cài bằng EXE được ưu tiên để nạp đủ DLL, thư viện và phông chữ.

Từ v0.5.5 đã bỏ DCL dự phòng. Nếu Palette không mở, gõ `BHTLOAD`, kiểm tra DLL hoặc cài lại bundle và mở lại AutoCAD.

## Phông chữ CAD

Chữ CAD mặc định dùng `VNRomancUpdate.shx` với bảng mã **Unicode**. Phông được kiểm tra trên máy là SHX Unicode, có glyph riêng cho `Ê`, `ê`, `Ế`, `Ệ`; không chuyển TCVN3 cho phông này. Khi gõ trực tiếp vào TEXT có kiểu `BHT_TCVN`/`BHT_BIENBAO`, chọn Unicode trong bộ gõ. Tên kiểu `BHT_TCVN` được giữ để tương thích bản vẽ cũ. Kiểu tùy chỉnh dùng `vnromanc.shx` vẫn dùng TCVN3; Palette, CSV/Excel và hồ sơ luôn giữ Unicode.

Từ v0.6.0 cần giữ đủ thư mục modules và SHA256SUMS.txt khi giải nén. Bộ cài từ chối bundle thiếu file, file đổi hash hoặc thêm file lạ. Bản cũ được giữ trong BHT-backup-<id> để khôi phục; thư mục backup không có đuôi .bundle nên không tự nạp. Đóng AutoCAD trước khi cài. `powershell -File INSTALL_BHT.ps1 -ValidateOnly` chỉ xác minh gói.

Gói mặc định không chứa source C#. Mã Lisp vẫn đọc được; DLL vẫn có thể bị dịch ngược. SHA256 phát hiện file lệch khỏi manifest, không thay chữ ký nhà phát hành. Bản này chưa có chữ ký Authenticode.

