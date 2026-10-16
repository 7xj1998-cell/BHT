# BHT 1.0.13

Ngày 13/10/2026.

## Các lỗi đã sửa

- Đã giữ cung tròn chính xác khi TDT trả về một cung đơn; giữ hai đỉnh cho đoạn thẳng đơn. Chiều dài trả về khớp với đường tham chiếu. Kế thừa bản sửa 1.0.12.
- Đã chặn lý trình quá lớn vượt khả năng biểu diễn của AutoLISP, tránh lỗi khi lưu/định dạng. Thống nhất nhập dấu phẩy thập phân giữa Lisp và bảng nhập liệu; từ chối số mét lẻ từ 1.000 trở lên trong dạng Km+M và lý trình âm nhập tay.
- Đã từ chối mốc trùng vị trí trước khi nội suy/ngoại suy, tránh chia cho 0. Phần tính bằng C# không trả trạng thái hợp lệ cho NaN/Infinity hoặc mốc lỗi.
- Đã chặn cập nhật Polyline tham chiếu thuộc tim TDT khác. Trùng tên lớp không còn đủ điều kiện để ghi đè hình học.
- Đã xử lý ký tự điều khiển, chuỗi dạng `_x0041_`, Unicode và xuống dòng khi xuất Excel. Báo cáo được ghi vào tệp tạm trước khi thay bản cũ; lỗi dữ liệu hoặc tệp đang khóa không làm mất báo cáo đã có.

Giữ các sửa lỗi copy biển, hatch che số và bố cục bảng từ 1.0.10–1.0.11.

## Cài và thử

Đóng AutoCAD trước khi chạy `BHT-Setup-1.0.13.exe`. Mở lại và kiểm tra phiên bản hiển thị. Bộ cài mới chưa tự thay bản đang cài trên máy.

Với biển cũ, chạy `BHTSUASAOCHEP` trong DWG nguồn đang hiển thị đúng trước khi copy. Khi có khóa TDT, mở bản sao DWG bằng TDT 9.1, chạy `BHTKIEMTRATDT`, lấy/cập nhật tim qua `BHTTUYENTDT`, kiểm tra mốc rồi tính lại lý trình.

Bản này chưa tích hợp đọc lý trình native TDT. Các kết quả hình học và đọc nhãn không thay thế kiểm chứng lý trình gốc; người dùng sẽ thử khi có khóa vào ngày 14/10/2026.
