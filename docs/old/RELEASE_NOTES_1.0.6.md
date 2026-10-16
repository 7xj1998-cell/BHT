# BHT 1.0.6 — Bố trí báo hiệu ra ngoài đường

Lệnh khoảng cách trước đây chỉ dịch biển xa điểm đo theo hướng đã đặt, nên hình vẫn có thể lấn vào mặt đường. Bản 1.0.6 đã thêm bố trí theo tim tuyến hoặc ra ngoài mép đường bằng một PL giữa đường.

- **Có tim:** chọn tim đường đã nạp, nhập khoảng cách tối thiểu đến hình biển/nhãn.
- **Chưa có tim:** nhập bề rộng trái/phải và khoảng lùi, chọn PL tham chiếu và đầu PL trên CAD.
- Đã tính kích thước biển, nhãn, biển hai trụ và đèn; giữ tọa độ khảo sát, lý trình, góc và tỷ lệ.
- Đã dùng hướng vuông góc tại từng vị trí trên đoạn thẳng, cong hoặc gãy khúc. Đường cong/gãy khúc có thể cần khoảng lùi lớn hơn giá trị tối thiểu.
- Đã giữ lựa chọn đường nối thẳng/gấp khúc trong cùng công cụ, cùng khả năng Undo và phục hồi khi lỗi.

Vào **Tuyến & báo cáo → Công cụ bổ sung → Bố trí tất cả báo hiệu…**, hoặc gõ **BHTKHOANGCACH**. Nhấn F2 để đọc danh sách ID chưa bố trí. Xem [hướng dẫn](HUONG_DAN.md) và [báo cáo kiểm thử](BAO_CAO_NGHIEM_THU_1.0.6.md).

Đã kiểm thử bằng AutoCAD 2024 Core Console và giao diện WinForms được tạo từ bộ thử. Chưa nghiệm thu thao tác chọn PL trực tiếp trên phiên CAD/bản vẽ của người dùng và chưa thử cài trên máy sạch. Bộ cài vẫn dùng FAS và ba DLL như bản trước; không tự cài hoặc đóng CAD.
