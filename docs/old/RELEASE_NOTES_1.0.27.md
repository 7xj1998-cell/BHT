# BHT 1.0.27 — Sửa Áp dụng tỷ lệ nhãn và căn nút cọc TDT

- Sửa lỗi `eInvalidInput` khi Áp dụng tỷ lệ/hướng nhãn: hàm `bht:api-sign-display` đã có ở 1.0.26 nhưng bị thiếu trong danh sách đăng ký `vl-acad-defun` cho .NET. Đã bổ sung đăng ký và kiểm tra qua đúng `LispApi.Call` → `Application.Invoke`.
- Căn nút **Đọc / kiểm tra cọc TDT** cùng lề trái/phải với **Thêm mốc bổ sung / điểm gãy Km** và **Bảng mốc: xem vị trí / xóa mốc…**. Bỏ lề thừa của khung con; giữ điều kiện hiện theo tim TDT đang chọn.

Đã tái hiện lỗi trên FAS phát hành 1.0.26; bản sửa đạt kiểm tra ở source và DLL/FAS bảo vệ. Giữ chức năng hướng nhãn, trình bày hai trụ và xử lý lý trình theo đoạn của 1.0.26. Lần này không thay thuật toán đọc TDT hoặc tính lý trình.

Lưu công việc, đóng tất cả AutoCAD, chạy **BHT-Setup-1.0.27.exe**, mở lại và kiểm tra **BHTNETINFO**. Không nạp chồng DLL mới vào phiên đang dùng bản cũ.
