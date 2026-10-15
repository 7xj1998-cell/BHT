# Nghiệm thu BHT 1.0.19

Ngày 14/10/2026. Môi trường .NET Framework 4.8 x64 / C#5, AutoCAD Core Console 2024. Mã nguồn trước sửa được lưu tại D:/lisp/BHT/audit-1.0.19/source-before-1.0.19.zip.

## Kết quả

| Phạm vi | Kết quả |
| --- | --- |
| Core, gồm định dạng BHTSEG/1 và Excel | 282 PASS, 0 FAIL khi dùng workbook thực tế |
| SEG119: tim riêng, chồng phạm vi, cọc, hoàn tác, Undo | 19 PASS, 0 FAIL trên FAS/DLL bảo vệ |
| SEG119 Store | PASS: áp dụng atomic, từ chối tim thiếu, giữ gán tay không đổi, bỏ gán tay khi đổi tim, native roundtrip, chuyển cả dự án .bht |
| SEG119 lưu/mở lại | PASS: giữ bảng đoạn, mã tim rãnh và hình học tuyến |
| STATION117 | 22 PASS, Undo và mở lại PASS |
| FIELD | 51 PASS, Undo và mở lại PASS |
| CAPTION118 | 21 PASS, API và Layout PASS, mở lại PASS |
| MARKER114 | 21 PASS, Undo/UCS và mở lại PASS |
| DISPLAY104 / Project | 10 PASS hiển thị, 12 PASS chuyển dự án |
| Palette | 2.777 PASS trên DLL bảo vệ, gồm chọn tim rãnh cho nhiều dòng và chuyển đổi bắt buộc sang .bhtseg |

Workbook D:/lisp/BHT/Ho so/Phan chia goi thau.xlsx được đọc giữ nguyên: 15 dòng dữ liệu, 3 nhóm tên gói gộp ô M. Đầu/cuối từ B+C / E+F, không lấy chiều dài suy ngược. Mã gói GOI_01…03 là mã do BHT sinh vì file nguồn không có mã gói; hiện rõ trước khi áp dụng. Kiểm tra các lý trình 38622–38697 và cuối 52618, phía, tổng số đoạn và nhóm. Tệp XML giữ Unicode, tab/xuống dòng trong ghi chú; roundtrip tim rãnh; dữ liệu hỏng/checksum sai, trùng mã, sai phía, cuối <= đầu, Km không hợp lệ và Excel lỗi giữa bảng đều bị từ chối. Ghi tệp lỗi không làm mất bản cũ.

SEG119 dùng hai tim song song trong cùng khoảng Km, xác nhận chọn tim riêng gần nhất; tie offset và mốc biên không gán đoán. Gán tay chọn tim cụ thể, tính riêng/toàn bộ giữ tim đó, cả khi chế độ chung dùng cọc. Đổi tên cập nhật SEG; xóa tim giữ mã thiếu và chặn phân đoạn; thiếu mốc chặn ghi; gây lỗi ghi giữa lô hoàn tác toàn bộ OBJ và nhãn. Số Km/H của cọc được giữ và cập nhật route_id theo đoạn. Phía theo tim rãnh không bị phía đường khai báo ghi đè.

Bộ cài và runtime độc lập sẽ được xác minh tại bước phát hành; báo cáo đầu ra lưu ngoài mã nguồn ở audit-1.0.19. Không cài đè phiên bản đang dùng và không sửa bản vẽ/Excel nguồn của người dùng. Chưa thực hiện kiểm tra tương tác trên bản vẽ đúng ảnh chụp; thử nghiệm CAD dùng bản vẽ tạo riêng. Chưa đối chiếu native TDT với khóa bản quyền. Workbook khác cấu trúc hai mẫu cần đưa về mẫu cột của BHT.
