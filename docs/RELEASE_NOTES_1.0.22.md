# BHT 1.0.22 — Nạp báo hiệu và tính theo tim đoạn

Quy trình trong tab Tuyến: **Nạp báo hiệu → Chọn tim đoạn → Xác nhận mốc Km → Tính lại và kiểm tra → Trình bày → Xuất hồ sơ**.

Mở DWG đoạn rồi chọn **Nạp báo hiệu từ .bht để tính theo tim đích…** (`BHTNAPBAOHIEU`). Dùng được khi đích đã khai báo tim/bảng đoạn nhưng chưa có hồ sơ khảo sát BHT. Giữ điểm đo, các mặt biển, ảnh và bảng phân chia; bỏ nguồn tim/mốc/phương pháp tính cũ. Lý trình nguồn lưu đối chiếu, không dùng làm kết quả mới. Các đoạn mới cần gán lại tim; đoạn đã có ở đích giữ nguyên.

Nạp tim TDT, chọn đúng dòng tim, sửa loại thành Tim rãnh nếu cần. Đọc và xác nhận ít nhất hai mốc bao quanh phạm vi tính. Tim TDT không lấy số Km/H trên cọc tiêu nhập từ file tổng hợp làm mốc; bộ quét cũng không đọc lại nhãn/biển do BHT vẽ. Số Km/H quan sát trên cọc được giữ. Bấm **Tính lại theo tim đang chọn…**, kiểm tra số hồ sơ chưa xác định, rồi DIM kiểm chứng tại trụ gần nhất.

`BHTNAPDUAN` trong mục phương pháp bổ sung vẫn nạp nguyên dự án, gồm tim/mốc, bảng đoạn và liên kết tim của từng đoạn. `.bht` chỉ chứa bảng phân chia khi bảng đó đã được nạp vào DWG xuất. Thông báo sau xuất/nạp hiển thị số hồ sơ, số tim và số đoạn; không tự tìm một XLSX bên ngoài để ghép vào gói.

Sửa chữ ký hình học còn handle cũ sau xuất/nạp. Từ chối dữ liệu cọc thiếu số trước khi tính; không đổi dữ liệu thiếu thành 0. Tim TDT gốc cần profile TDT hoạt động để đọc native. Chưa tái hiện trọn vẹn phiên đoạn 10 trong ảnh vì DWG đã lưu được cung cấp chưa có hồ sơ BHT đó; lỗi thiếu raw-distance đã được tái hiện trên mã cũ bằng kiểm thử lỗi đầu vào và có kiểm tra hồi quy.

File người dùng `filebaohieu.bht` tại thời điểm kiểm tra: 179 hồ sơ, 1 tim, 0 đoạn. Bản bổ sung bảng đoạn được lưu riêng, dùng bảng .bhtseg đã có; nguồn gốc và các giới hạn ghi trong báo cáo nghiệm thu.
