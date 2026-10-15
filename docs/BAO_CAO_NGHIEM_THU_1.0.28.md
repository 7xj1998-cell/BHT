# Kiểm chứng BHT 1.0.28

AutoCAD 2024 Core Console, profile BHTResearch111; môi trường thử riêng.

- CoreTests: 305 đạt, 0 lỗi.
- PaletteLayoutProbe: 3.027 đạt. Bảng đối chiếu: lọc, đủ mặt biển, đọc nội dung, đóng bảng, nút ở hai kích thước. Kiểm tra giữ vị trí cuộn khi danh sách tuyến bị xóa/dựng lại, trả tiêu điểm và cuộn chủ động.
- Progress125 mở rộng: 13 kiểm tra Lisp, 25 kiểm tra .NET, 7 kiểm tra chuyển đoạn và nhãn kế thừa đạt trên source và DLL/FAS bảo vệ. Có trường hợp nhiều hồ sơ, hai trục tọa độ, bảng đoạn đồng thời xung đột; từ chối toàn bộ và giữ dữ liệu đích.
- DISPLAY127: 9 kiểm tra .NET gọi API tỷ lệ/hướng nhãn đạt trên DLL/FAS bảo vệ.
- Đối chiếu riêng ba tệp người dùng: đều có 179 hồ sơ, 801 điểm, 15 đoạn; không sửa hoặc lưu lại tệp gốc. Bảng gốc so với đoạn 8 có 9 khác biệt trên 5 bản ghi; với đoạn 9 có 1 khác biệt; đoạn 8 và 9 có 10 khác biệt trên 5 bản ghi. Báo cáo dự án nằm ngoài gói phát hành tại audit-1.0.28.

Giới hạn: kiểm tra UI bằng WinForms trong tiến trình thử, chưa cài và thao tác lại trong phiên AutoCAD đang mở của người dùng. Chưa tự hợp nhất các nội dung khảo sát khác nhau hoặc quy đổi DOAN08 sang DOAN_08. Kiểm tra tính toàn vẹn kết quả đã xác nhận vẫn có thể chặn tệp không hợp lệ trước bước so sánh giữa hai bản.

Kết quả bộ cài và runtime phát hành lưu ngoài gói bất biến tại D:\lisp\BHT\audit-1.0.28\BAO_CAO.md.
