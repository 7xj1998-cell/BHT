# Nghiệm thu BHT 1.0.16

Ngày 14/10/2026, AutoCAD Core Console 2024 và .NET Framework 4.8 x64.

## Dữ liệu thật

Nguồn: `C:/Users/Le Bao/Desktop/filebaohieu.dwg`, chỉ đọc. Thử nghiệm thực hiện trên bản sao ở `D:/lisp/BHT/audit-1.0.16/filebaohieu-original.dwg`.

SHA256 nguồn: `EA27DC51A73213C26127BF2F0C0F32184E8D9EF8BDBFBB2DE174563E78A5B28A`.

Tái hiện bản 1.0.15: INSUNITS nguồn Millimeters, đích Undefined bị chặn; đặt đích Millimeters nạp thành công 4.235 đối tượng CAD. Không chứng minh DWG nguồn hỏng. Chưa có DWG đích thực tế để xác định các nguyên nhân khác nếu có.

## Kết quả

| Phạm vi | Kết quả |
| --- | --- |
| Core | 258 PASS |
| Giao diện trên DLL bảo vệ | 2.683 PASS |
| Gói .bht | Roundtrip byte; dọn tệp tạm; không ghi đè; từ chối sai SHA256, phiên bản lạ, dung lượng sai/quá lớn, thành phần lạ, DTD, gói cắt ngắn |
| PROJECT116, DLL thường và bảo vệ | 44 PASS mỗi lượt |
| Nạp gói nguồn thật | Giữ 801 điểm, 179 hồ sơ, 334 bản ghi ảnh, 2 bộ dữ liệu, 1 chuỗi cọc, 1 tuyến và mốc |
| Đối chiếu dữ liệu | Tọa độ XYZ và nội dung hồ sơ khớp; remap handle tuyến; lưu/mở lại giữ dữ liệu |
| Bản vẽ đích có hình | Từ chối khác đơn vị trước khi ghi; chọn giữ tọa độ giữ hình đích và INSUNITS đích |
| Dự án đã có / chỉ có cấu hình | Không nạp chồng; bản cấu hình rỗng không xuất thành dự án |
| PROJECT116FLOW, FAS/DLL bảo vệ | 7 PASS: nạp/đơn vị, một Undo, hủy, giữ tọa độ, cập nhật ký hiệu, xuất, mở lại |
| DISPLAY104 và Project104, bản bảo vệ | 10 + 12 PASS |

Bộ thử luồng lệnh kiểm tra cập nhật ký hiệu sau nạp không làm mất điểm, hồ sơ hay số ký hiệu. Bộ chuyển cũ kiểm tra cả đường tham chiếu cọc/mép đường, tên block remap và không mang hình ngoài dự án.

Kết quả xác minh bộ cài và runtime độc lập sau đóng gói được ghi ngoài source tại `D:/lisp/BHT/audit-1.0.16/BAO_CAO.md`.

## Giới hạn

Không nghiệm thu toàn bộ nghiệp vụ phần mềm. Lỗi bộ thử biển tổng hợp native đã ghi ở 1.0.15 không thuộc đợt sửa này. Chưa kiểm chứng CAD khác, lý trình native TDT có khóa, thư viện proxy/xref/phông tùy chỉnh ngoài gói. Ảnh gốc chưa được đóng gói; chỉ bảo toàn bản ghi và đường dẫn ảnh. Mã SHA256 là kiểm tra toàn vẹn, không xác thực tác giả. Không cài tự động và không sửa DWG gốc.
