# Kiểm chứng BHT 1.0.11

Ngày 13/10/2026.

## Kết quả

| Hạng mục | Kết quả |
| --- | --- |
| Build AutoCAD 2024 x64 | Core, Bridge, Palette đúng 1.0.11 |
| Core | 233 đạt |
| Giao diện với DLL/FAS bảo vệ | 2.679 kiểm tra đạt; đã xem ảnh 340 px và 560 px |
| Copy biển | 34 kiểm tra đạt: DWG mới, thứ tự/màu, lưu-mở lại, lớp đích trắng, xuất/nạp dự án |
| Bộ đọc cọc | 11 kiểm tra đạt; không thay thế kiểm tra native TDT |

Thay đổi giao diện bỏ dải phải 108 px và các nút chuyển tab lặp ở Tổng quan. Tên đầy đủ của tab vẫn có trong tooltip; dữ liệu và chức năng được giữ. Bảng danh sách nhiều cột vẫn có thể cần cuộn ngang ở chiều rộng 340 px.

## Nghiên cứu TDT

Đã kiểm tra PE/import/export của bộ TDT 9.1 đang cài, xác nhận API GetStationAtPoint/GetPointAtStation tồn tại trong AlignmentDb.dbx và giải mã chữ ký hàm. Báo cáo PHAN_TICH_TDT_9_1.md phân tích phụ thuộc, lý trình đứt thước, giới hạn explode/quét nhãn và hướng kết nối native có kiểm chứng.

Phiên AutoCAD GUI nghiên cứu riêng chưa chạy tới lệnh thử native. Core Console thiếu đăng ký lớp TDT; không được dùng hiện tượng này để kết luận bản cài TDT của người dùng hỏng. Chưa có kết quả lý trình native trên đúng doan1.dwg, nên bộ cài không chứa NativeStationProbe hoặc lời gọi native thử nghiệm.

## Phạm vi sử dụng

Đã kiểm tra chuyển biển sang DWG mới trên AutoCAD 2024 theo yêu cầu người dùng. Chưa thử trên máy nhận thực tế. BHTSUASAOCHEP chỉ chuẩn bị biển trong nguồn đang hiển thị đúng. Bản cài hiện có trên máy khai báo 1.0.3; bộ cài mới chưa được cài đè.

Nhật ký kiểm chứng và ảnh: D:/lisp/BHT/maintenance-20261013 và build/v1.0.11. Báo cáo kiểm tra bộ cài/runtime độc lập được lưu trong maintenance-20261013 sau khi đóng gói.
