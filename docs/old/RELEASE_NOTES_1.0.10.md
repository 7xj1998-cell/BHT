# BHT 1.0.10 - sao chép biển và chẩn đoán TDT

Ngày 13/10/2026.

Biển tạo/cập nhật bằng bản này giữ thứ tự hatch và chữ ngay trong block riêng. Đã kiểm tra chuyển sang DWG trống, lưu/mở lại và trường hợp lớp cùng tên ở DWG đích có màu trắng. Xuất/nạp dự án cũng khôi phục thứ tự vẽ.

Với biển đã vẽ, chạy `BHTSUASAOCHEP` trong DWG nguồn khi hình đang đúng, chọn các block BHT cần chuyển rồi copy lại. Có thể dùng **Cập nhật mẫu biển đã vẽ** để cập nhật từ hồ sơ. Block cùng tên đã tồn tại tại đích có thể được CAD tái sử dụng. Lệnh không đoán lại thứ tự từ bản copy đã hỏng.

`BHTKIEMTRATDT` đọc loại đối tượng và handle tim, không sửa hình học. `BHTTUYENTDT` lấy hình học; sau đó cần `BHTDOCCOCTDT` để đọc nhãn Km và xác nhận mốc. Nếu không có nhãn Km, nhập mốc đã biết bằng `BHTMOCKM`.

Chưa xác nhận đã giải quyết trường hợp TDT của người dùng: phiên thử riêng không nạp được đầy đủ module TDT; cần DWG có lỗi và phiên CAD chạy TDT 9.1 bình thường. Chưa kiểm tra CAD hãng khác hoặc phiên bản khác AutoCAD 2024.

Đóng AutoCAD rồi cài BHT 1.0.10. Không NETLOAD DLL mới chồng lên DLL cũ trong cùng phiên.
