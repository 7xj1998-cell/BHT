# Rà soát BHT ngày 13/10/2026

## Phát hiện và xử lý

1. **P1 - hatch che số khi chuyển DWG.** Tái hiện 6/8 mẫu trên source 1.0.9 bằng WblockCloneObjects, gồm P.127-80, P.127a/b/c/d và R.415. Màu vẫn tồn tại nhưng SORTENTSTABLE không giữ thứ tự qua clone: số 80 đứng trước nền trắng. Bản 1.0.10 tạo block riêng theo thứ tự hiển thị, cố định màu ByLayer và thuộc tính; xuất/nạp dự án phục hồi thứ tự. Đã thêm kiểm tra hồi quy.
2. **P1 - chưa đọc được lý trình TDT.** Code lấy hình học bằng Explode trong bộ nhớ và quét nhãn Km, chưa gọi API lý trình native. Tim proxy bị bỏ qua khi quét có thể dẫn đến danh sách cọc thiếu. Đã chặn nguồn mất/proxy, thêm chẩn đoán và làm rõ hướng dẫn. Chưa sửa được lỗi module vendor: nạp AlignmentDb.dbx thất bại; nạp TDTObjects.dbx gây lỗi thiếu lớp TdtDbPointObj trong Core Console riêng. Không thay đổi tài nguyên vendor hoặc bản vẽ gốc.
3. **P2 - bảng tuyến báo sẵn sàng chỉ dựa vào hình học.** Trạng thái chưa chứng minh đủ mốc lý trình. Đã đổi thành Có hình học, giải thích bước đọc/xác nhận cọc TDT.
4. **P2 - giao diện hẹp.** Ở chiều rộng 340 px, tên nút dài bị rút gọn và bảng hồ sơ phải cuộn ngang. Các phép kiểm tra bố cục đạt nhưng chưa thay thế nghiệm thu thao tác chuột, DPI và màn hình nhỏ. Giữ giao diện xanh hiện tại; nên dùng Palette rộng hơn khi nhập hồ sơ.

## Kiểm tra

- Build đủ Core, Bridge, Palette cho AutoCAD 2024; Core 233 đạt.
- Giao diện: 2.676 kiểm tra bố cục/quy trình đạt; đã xem ảnh bộ thử, gồm bảng hẹp.
- Copy: 32 kiểm tra trên 8 mẫu x 2 chế độ nền x clone/lưu-mở; thêm lớp đích màu trắng và xuất/nạp dự án.
- Bộ đọc cọc giả lập kiểm tra nhãn, block lồng, thuộc tính, trùng lặp, khoảng cách và nguồn bị mất. Đây không phải kiểm tra TDT native.
- Cập nhật bản sao 261011_test.dwg: giữ handle INSERT, vị trí, góc, tỷ lệ, điểm đo và hồ sơ; bật/tắt nền đạt. File gốc được so sánh SHA256.
- Mở bộ chọn biển: 234/165/157 ms; 24 ảnh, 4.478.976 byte dữ liệu pixel. Tìm kiếm theo chuỗi gõ 1-2 ms; chưa đo toàn bộ độ trễ timer tới khi danh sách ổn định.
- Tính lý trình 5 lần: 100 hồ sơ 62-94 ms, 1.000 hồ sơ 687-922 ms, 5.000 hồ sơ 3.687-5.000 ms; sai số lớn nhất 0,0005 m. Đo trong khi tiến trình lưu trữ đang chạy, không phải benchmark máy rảnh.

## Phạm vi còn thiếu

Chưa có đường dẫn DWG đang lỗi và CAD đích từ người dùng. Chưa nghiệm thu Ctrl+C/Ctrl+V bằng chuột giữa hai phiên CAD hoặc CAD hãng khác. Chưa đọc được lý trình native TDT trên DWG đang lỗi. Không công bố các trường hợp này đã hết lỗi.

Source trước sửa: D:/lisp/BHT/maintenance-20261013/source-before-1.0.10.7z. Chứng cứ kiểm tra: maintenance-20261013 và build/v1.0.10.
