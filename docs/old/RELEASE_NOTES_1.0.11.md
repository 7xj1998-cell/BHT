# BHT 1.0.11

Ngày 13/10/2026.

## Giao diện

- Điều hướng Chung, Điểm, Hồ sơ, Ảnh, Tuyến chuyển lên trên; không còn dải bên phải rộng 108 px.
- Trang Tổng quan có một nút Tiếp tục công việc; các chức năng vẫn ở tab chuyên trách.
- Giữ bảng màu xanh BHT hiện có. Đã kiểm tra bố cục và luồng sửa hồ sơ ở các kích thước bảng hẹp/rộng.

## Copy báo hiệu

Giữ bản sửa 1.0.10: thứ tự hatch/chữ được lưu ngay trong block riêng; màu ByLayer được chốt theo nguồn để tránh lớp trùng tên ở đích làm biển trắng. Với biển cũ, chạy `BHTSUASAOCHEP` trong DWG nguồn đang hiển thị đúng trước khi copy. Không dùng lệnh để đoán lại thứ tự trong bản đã copy hỏng.

## TDT 9.1

`BHTKIEMTRATDT` hiển thị thông tin module thực sự đang nạp trong tiến trình CAD và lớp gốc của proxy. Đọc [phân tích TDT 9.1](PHAN_TICH_TDT_9_1.md) để phân biệt hình học tim, lý trình từ nhãn/cọc và lý trình native.

Chưa xác nhận đọc đúng lý trình gốc TDT trên `doan1.dwg`; chưa xác định được đường dẫn bản vẽ này. Các hàm native đã tìm được trong thư viện nhưng chưa vượt qua kiểm thử trong phiên TDT đầy đủ, nên không đưa vào luồng tính hồ sơ. Vẫn cần kiểm tra/xác nhận mốc cho phương pháp hình học hiện tại.

## Cài đặt

Đóng AutoCAD, chạy bộ cài 1.0.11, mở lại bản vẽ nguồn và kiểm tra tên phiên bản BHT. Bundle trên máy lúc khảo sát khai báo 1.0.3; việc tạo bộ cài mới không thay thế bản đang dùng.
