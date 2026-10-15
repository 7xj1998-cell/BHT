# BHT 1.0.23 — Chọn loại tim và bảng mốc tuyến

Nạp tim TDT hoặc Polyline mở hộp thông tin tuyến với danh sách **Tim đường / Tim rãnh / Tuyến khác**. Bản trước tạo TDT cố định TIM_DUONG. Tuyến TDT đã có khi cập nhật hình học giữ nguyên loại và mốc đã khai báo.

**Sửa tuyến…** dùng cùng hộp nhập, nhận số thập phân dấu phẩy hoặc dấu chấm. Thông số sai được báo tại trường tương ứng; không còn bắt nhập mã loại tim bằng chữ tại dòng lệnh. Hủy không thay đổi dữ liệu. Áp dụng thông số không đổi giữ revision và kết quả; đổi thông số yêu cầu tính lại hồ sơ. Đổi tên vẫn cập nhật liên kết đoạn và hồ sơ.

**Bảng mốc: xem vị trí / xóa mốc…** (`BHTDSMOC`) hiện khoảng cách dọc tim, lý trình sau/trước, hệ số đoạn trước, nguồn và ghi chú. Mốc xếp theo khoảng cách dọc tim; hệ số bất thường hoặc dữ liệu hỏng có cảnh báo. Điểm gãy Km giữ hai lý trình riêng.

- Chọn một dòng hoặc nhấp đúp để xem vị trí trên CAD: dấu cộng và vòng tròn vàng đặt đúng vị trí mốc trên tim. Enter trở lại bảng, Esc thoát; dấu kiểm tra được dọn sau thao tác.
- Ctrl/Shift chọn nhiều dòng rồi **Xóa mốc đã chọn…**. Xác nhận nêu số mốc; xóa theo bản ghi gốc, không dùng số hàng sau sắp xếp. Một lần Undo khôi phục lần xóa và dữ liệu hồ sơ liên quan.
- Mốc tự động từ hồ sơ cọc hiển thị riêng; sửa/xóa tại hồ sơ, không xóa bằng bảng mốc khai báo.
- Sau khi xóa mốc hoặc đổi thông số tim, dùng **Tính lại theo tim đang chọn…** trước khi xuất hồ sơ.

Lưu và đóng toàn bộ AutoCAD trước khi chạy BHT-Setup-1.0.23.exe, rồi mở lại. Không NETLOAD chồng DLL mới trong phiên đang dùng bản cũ. Gõ BHTNETINFO để kiểm tra phiên bản.

Nhánh chọn loại khi nạp TDT được thử với adapter giả lập; chưa nghiệm thu trực tiếp native TDT trên chính phiên đoạn 10 trong ảnh. Xem BAO_CAO_NGHIEM_THU_1.0.23.md.
