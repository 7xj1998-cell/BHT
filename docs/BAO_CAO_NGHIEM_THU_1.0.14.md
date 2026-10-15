# Kiểm chứng BHT 1.0.14

Thực hiện đêm 13/10 sang 14/10/2026 trên AutoCAD 2024 x64. Bản sửa tập trung vào cọc tiêu, đường tham chiếu bố trí và lỗi chọn thư mục runtime.

## Kết quả có bằng chứng

| Hạng mục | Kết quả |
| --- | --- |
| Build C# và phiên bản | Core, Bridge, Palette 1.0.14.0; build thành công |
| Core | 249 PASS, 0 FAIL trên DLL thường và DLL bảo vệ |
| Cọc tiêu MARKER114 | 21 PASS trên source; 21 PASS trên FAS/DLL bảo vệ; Undo, UCS xoay, lưu/mở lại đạt |
| Bản sao 261011_test.dwg | 52 cọc đúng điểm, 52 cọc đúng pháp tuyến, không có đường nối; điểm đo, tuyến và số liệu lý trình giữ nguyên |
| Bố trí ngoài đường ROAD106 | 44 PASS; Undo, lưu/mở lại đạt |
| Kiểu đường nối DISPLAY105 | 20 PASS; Undo, lưu/mở lại đạt |
| Hiển thị/xuất nhập dự án | 10 kiểm tra hiển thị và 12 kiểm tra chuyển dự án đạt, gồm ánh xạ đường tham chiếu cọc |
| Giao diện | 2.679 kiểm tra đạt trên DLL thường và bảo vệ |
| Hình học/liên kết tim | 37 PASS trên DLL bảo vệ |
| Lý trình/mốc của 1.0.13 | 10 PASS trên FAS mới |
| Mốc nhập MARKER649 | 16 PASS |
| Copy biển | 34 PASS; bộ đọc nhãn cọc đi kèm cũng đạt |
| Đèn | Bộ thư viện đèn và các ca Lisp bố trí/hướng đạt |
| Runtime độc lập | Tạo/chèn biển/lưu/mở lại đạt; thêm FAS cùng phiên bản vào đầu Support Path để xác nhận runtime vẫn chọn đúng thư mục Bridge |

Test mới tái hiện lỗi gấp khúc tại điểm gốc trên FAS 1.0.13. Các kiểm tra 1.0.14 bao gồm cọc hai bên tuyến thẳng, pháp tuyến trên cung, đảo chiều tuyến, cọc đã dịch tay, nhãn tự động, thiếu hướng, lớp khóa, API đặt riêng, bảo toàn POINT và mốc. Cố tình gây lỗi giữa thao tác để kiểm tra rollback bản ghi và hình vẽ. Không bỏ qua lỗi để tính PASS.

Log ở `build/v1.0.14/`: `marker114-*`, `sample-marker114-*`, `qa/road106`, `qa/display105`, `qa/display104`, `bin/project104`, `bin-protected/project104`, `bin-protected/ui-preview`, `geometry-*`, `review113-*`, `copy110`, `lights.log`. Các runner ghi thư mục cụ thể ở cuối output. Bản DWG mẫu kiểm chứng được lưu riêng; SHA256 bản gốc trước/sau giống nhau.

## Bộ thử rộng chưa đạt

`run_signs.ps1` mặc định dừng tại `BHTSIGNPROBE` trong Core Console; đã giữ `build/v1.0.14/legacy-sign-probe-crash.log`. Khi dùng thư viện ADSCivil hiện có, các phần sau chạy được nhưng bộ rộng chưa đạt.

Đối chiếu cùng môi trường với DLL/FAS 1.0.13: 47 thất bại so sánh thứ tự vẽ native của thư viện ADS trùng nhau giữa hai bản; các ca SIGN về tên block tốc độ, đường nối, vị trí nhãn, cảnh báo chân thiếu và nâng cấp hình cũng đã thất bại trên bản cũ. Không kết luận từng ca là lỗi sản phẩm mới hay kỳ vọng kiểm thử đã cũ khi chưa phân tích riêng. Log: `sign-wide-new.log`, `sign-wide-baseline113.log`, `sign-wide-new-ads.txt`, `sign-baseline113/ads-probe.txt`.

Khác biệt mới ở ca cọc cũ là kỳ vọng đặt chân tại điểm chọn tự do; yêu cầu 1.0.14 đổi sang điểm khảo sát. Đã cập nhật kiểm tra cọc trong `test_SIGN.lsp` cho yêu cầu này và bỏ tọa độ nhãn cố định đã lỗi thời, giữ kiểm tra nhãn nằm dưới thân cọc và theo góc. Bộ MARKER114 kiểm tra độc lập yêu cầu mới và đã đạt. **Không tính toàn bộ run_signs là PASS.**

## Giới hạn

Chưa thử chuỗi thao tác nhiều tài liệu bằng chuột trong AutoCAD đầy đủ, CAD hãng khác, máy nhận khác hoặc lý trình native TDT có khóa. Kiểm chứng hình học không thay cho kiểm chứng lý trình TDT.

Bộ cài được kiểm tra sau đóng gói; kết quả và SHA256 bàn giao ghi ở `D:/lisp/BHT/BHT-HIEN-TAI.md`. Báo cáo này không khẳng định không còn mọi lỗi trong phần mềm. Các đề xuất về phạm vi bố trí theo tuyến/loại được ghi riêng trong `RA_SOAT_KY_THUAT_1.0.14.md` và chưa triển khai.
