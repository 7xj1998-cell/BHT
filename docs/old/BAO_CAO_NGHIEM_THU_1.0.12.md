# Kiểm chứng BHT 1.0.12

Ngày 13/10/2026. Môi trường: AutoCAD 2024 x64.

| Hạng mục | Kết quả |
| --- | --- |
| Build | Core, Bridge, Palette 1.0.12 |
| Core | 233 kiểm tra đạt |
| Hình học tim | 32 kiểm tra đạt trên DLL thường và DLL bảo vệ |
| Giao diện | 2.679 kiểm tra đạt với DLL bảo vệ/FAS |
| Copy biển | 34 kiểm tra đạt |
| Đọc nhãn cọc | 11 kiểm tra đạt |

Đã tái hiện lỗi cung đơn trên DLL 1.0.11 trước khi sửa: 3/4 kiểm tra thất bại; nửa đường tròn bán kính 10 m thiếu 0,0032549459 m chiều dài. Bản 1.0.12 đạt cùng phép thử, bổ sung cung ngược chiều, cung qua góc 0, bán kính 0,01 m và 5.000 m, nối cung/đoạn thẳng theo hai chiều, giữ nguyên dữ liệu nguồn và lưu/mở lại DWG. Sai số vị trí trong các mẫu cung nhỏ hơn 0,0000001 m.

Phép thử dùng hình học AutoCAD tạo trong cơ sở dữ liệu riêng, chưa thay thế kiểm chứng với tim native TDT. Đường cong khác vẫn lấy mẫu; không tuyên bố mọi loại đường cong đều chính xác tuyệt đối.

Người dùng xác nhận hiện không có khóa TDT; kiểm tra lý trình native trên bản vẽ thực tế để lại cho ngày 14/10/2026. Không đưa lời gọi native thử nghiệm vào bộ cài.

Nhật ký build/kiểm tra lưu ở `D:/lisp/BHT/old/maintenance-20261013`; phép thử mới tại `tests/IntegrationTests/run_tdt_geometry.ps1`. Kết quả kiểm tra bộ cài và runtime sau đóng gói được ghi riêng trong `installer112.log` và `runtime112.log`.
