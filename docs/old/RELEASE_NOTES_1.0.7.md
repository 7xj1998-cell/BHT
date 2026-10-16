# BHT 1.0.7 — Nhận tuyến đã nạp khi bố trí theo tim

Đã sửa lỗi tuyến có trong bảng **Dữ liệu tuyến** nhưng không xuất hiện ở hộp bố trí **Cách tim**. Bản 1.0.6 chỉ nhận loại nội bộ `TIM_DUONG`; tuyến `TUYEN1` trong DWG đã lưu đang có loại **Khác**, dù hình học là Polyline 2D hợp lệ.

- Danh sách chọn tim đã hiển thị tất cả tuyến đã nạp cùng loại tuyến. Chọn đúng đường muốn dùng làm tim bố trí; không cần nạp lại `TUYEN1`.
- Bảng dữ liệu tuyến đã có cột **Loại** để dễ kiểm tra phân loại.
- Đã giữ nguyên loại tuyến, mốc, điểm khảo sát và lý trình khi dịch hình. Tim bị xóa hoặc hình học không phù hợp vẫn bị từ chối với hướng xử lý cụ thể.

Sau khi lưu công việc, đóng CAD, cài **BHT-Setup-1.0.7.exe** và mở lại AutoCAD 2024. Vào **Tuyến & báo cáo → Công cụ bổ sung → Bố trí tất cả báo hiệu… → Cách tim: chọn tuyến đã nạp → TUYEN1 — Khác**. Nhập khoảng cách phù hợp rồi bấm **Áp dụng tất cả**. Nhấn **F2** để xem kết quả; **U** hoàn tác lần vừa bố trí.

Đã kiểm thử 44 ca bố trí, 2.676 mục giao diện và 120 báo hiệu trên bản sao DWG đã lưu, dùng FAS/DLL bảo vệ. Chưa nghiệm thu thao tác chuột trên phiên CAD của người dùng hoặc cài trên máy sạch. Xem [báo cáo kiểm thử](BAO_CAO_NGHIEM_THU_1.0.7.md) và [hướng dẫn](HUONG_DAN.md).
