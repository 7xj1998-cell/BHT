# BHT 1.0.4 — Hiển thị báo hiệu và chuyển dự án

Ngày 12/10/2026. Dành cho AutoCAD 2024.

- Đã thêm **Điểm vị trí → Xoay nhãn RTK…**: chọn **Theo UCS hiện tại** hoặc **Chọn hướng A → B trên CAD…**.
- Tất cả tên, mã điểm, mô tả và cao độ RTK của BHT cùng xoay theo hướng đọc. Điểm đo, nội dung, phông và cỡ chữ được giữ nguyên.
- Đã giữ hướng nhãn khi cập nhật; hỗ trợ Undo cả lần xoay, bỏ qua cụm có lớp khóa hoặc khác mặt phẳng XY.
- Đã thu gọn hộp tỷ lệ RTK và bỏ đoạn ghi chú dài.
- Đã thay bốn mẫu đèn tín hiệu bằng DWG do người dùng cung cấp; giữ màu đèn, chuẩn hóa block lồng và lớp hiển thị khi nạp.
- Đã thêm khoảng cách đặt báo hiệu dùng chung, gom hình/nhãn/đường nối về BHT_KYHIEU và nối biển về chân trụ hoặc mép dưới block.
- Đã thêm xuất/nạp dự án DWG, giữ toàn bộ điểm, hồ sơ, mốc/tuyến và liên kết ảnh. Bản này nạp vào DWG chưa có BHT; không gộp hai dự án theo phạm vi đã chốt.

Lệnh tắt mới: **BHTXOAYNHAN**, **BHTKHOANGCACH**, **BHTCHONBAOHIEU**, **BHTXUATDUAN**, **BHTNAPDUAN**. Nếu vừa thêm điểm hoặc loại nhãn, bấm **Cập nhật nhãn RTK** trước rồi xoay. Xem **HUONG_DAN.md** để biết phạm vi từng lệnh và cách dùng.

Lưu bản vẽ, đóng AutoCAD rồi chạy **BHT-Setup-1.0.4.exe**. Không nạp chồng DLL mới vào phiên đang chạy bản cũ.

Thư viện đi kèm được giữ trong gói cài dùng tại máy này. Không công bố tài nguyên bên thứ ba lên GitHub. Chuyển mọi chữ trong DWG thành nét để bỏ phụ thuộc phông vẫn chưa triển khai.
