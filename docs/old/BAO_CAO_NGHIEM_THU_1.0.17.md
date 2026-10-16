# Nghiệm thu BHT 1.0.17

Ngày 14/10/2026. AutoCAD Core Console 2024, .NET Framework 4.8 x64.

## Phạm vi

Sửa phương pháp tính còn lưu FIELD_MARKERS sau nạp dự án; tách mốc hồ sơ cũ khỏi các mốc TDT đã xác nhận. Thêm lệnh tính lại toàn bộ theo đúng một tim đang chọn, xem kết quả dự kiến trước khi ghi. Không đọc/ghi bản vẽ gốc đang chỉnh sửa của người dùng.

Ảnh thể hiện route TUYENTDTD15, ba mốc TDT từ Km49+103.28 đến Km49+294.25, bốn mốc hồ sơ tự động và ngoại suy 50 m. Phép thử dùng tuyến/mốc giả lập tái hiện vấn đề ghép nguồn và giữ FIELD_MARKERS; không dùng số giả lập làm lý trình nghiệm thu của hồ sơ thực.

## Kết quả trước đóng gói

| Phạm vi | Kết quả |
| --- | --- |
| Core | 258 PASS |
| Giao diện trên DLL bảo vệ | 2.723 PASS |
| STATION117 | 22 PASS và lưu/mở lại PASS, cả Lisp nguồn và FAS/DLL bảo vệ |
| Mốc thực địa FIELD | 51 PASS, lưu/mở lại và một Undo PASS |
| Quản lý tuyến MANAGE | PASS, hủy/sửa/xóa/Undo và bảo toàn hình học |
| Điểm đầu/chiều tuyến | PASS |

STATION117 đối chiếu nội suy bằng giá trị tính độc lập; tuyến cũ gần điểm hơn không được dùng khi đã chọn tim mới. Kiểm tra cọc khảo sát không đổi, hồ sơ thiếu mốc/ngoài tuyến/thiếu điểm được bỏ lý trình cũ; bổ sung mốc đúng cho phép tính lại. Cung đảo chiều dùng chiều dài cung, không dùng dây cung. Từ chối tim thay hình học chưa xác nhận lại. Hủy giữ dữ liệu, lỗi ghi giữa chừng hoàn tác các bản ghi và phương pháp; một Undo trả toàn bộ trạng thái trước lệnh.

Đổi tên tim đang dùng cập nhật ID; tim bị xóa không tự chuyển sang tuyến cũ. Chẩn đoán hiển thị phương pháp và tim tính, chỉ ra hồ sơ trong phạm vi chưa tính theo tim này thay vì chỉ đếm hồ sơ đã gắn tuyến.

Kết quả giao diện, runtime độc lập và bộ cài sau đóng gói được lưu tại `D:/lisp/BHT/audit-1.0.17/BAO_CAO.md`.

## Giới hạn

Chưa có bản DWG lưu trạng thái đang thể hiện trong ảnh để nghiệm thu trực tiếp. Chưa kiểm chứng native TDT có khóa hoặc CAD hãng khác. Không khẳng định ba mốc trong ảnh phủ đủ toàn bộ tim; vị trí gần Km48+540 cần thêm mốc phù hợp. Số Km/H khảo sát của cọc giữ nguyên, không coi là kết quả chiếu lên tuyến mới. Bộ biển tổng hợp native lỗi đã ghi ở bản trước không nằm trong phạm vi lần này.
