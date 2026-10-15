# BHT 1.0.28 - Bảng đối chiếu khi nạp và giữ vị trí bảng công cụ

- Nạp dữ liệu kế thừa kiểm tra toàn bộ xung đột trước khi ghi: trùng mã điểm, tọa độ, nội dung khảo sát, kết quả xác nhận, bảng đoạn và bản ghi khác.
- Bảng đối chiếu có mã đối tượng, lý do, trường dữ liệu, giá trị bản vẽ hiện tại và tệp nạp; tìm kiếm, xem đầy đủ và sao chép toàn bộ. Nhiều mặt biển/ảnh/điểm được giữ đủ trong đối chiếu.
- Khi có xung đột, dừng toàn bộ lượt nạp và giữ bản vẽ đích. Bản này chưa tự quyết định lấy dữ liệu bên nào hoặc chuẩn hóa mã đoạn khác nhau.
- Bảng tuyến giữ vị trí cuộn khi làm mới, không ẩn/hiện tạm nhóm cọc TDT trong lúc dựng lại danh sách. Trả tiêu điểm từ lệnh/hộp thoại không tự cuộn; mở nhóm công cụ và điều hướng bàn phím vẫn có cuộn chủ động.

Đóng tất cả AutoCAD sau khi lưu công việc, chạy BHT-Setup-1.0.28.exe rồi kiểm tra BHTNETINFO. Không nạp chồng DLL vào phiên đang dùng bản cũ.
