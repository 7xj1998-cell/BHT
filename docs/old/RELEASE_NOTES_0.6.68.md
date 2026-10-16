# BHT 0.6.68 — Báo cáo và nút thao tác dễ hiểu hơn

Đã viết lại nội dung kiểm tra bản vẽ theo hướng dẫn của skill vietnamese-tech-writing. Thay đổi cách diễn đạt và trình bày; giữ nguyên cách tính lý trình, điều kiện phát hiện lỗi và mã lệnh CAD.

## Báo cáo kiểm tra bản vẽ

- Mở đầu bằng số lỗi cần xử lý và số mục cần kiểm tra. Phần **Việc cần làm** đưa các lỗi/cảnh báo lên trước, giữ mã điểm/hồ sơ để tra cứu, kèm hướng xử lý. Phần **Số liệu đã kiểm tra** tách từng chỉ số thành dòng riêng.
- Thay “mồ côi” bằng đúng ý nghĩa: **ký hiệu không tìm thấy hồ sơ đối tượng**, **ký hiệu không tìm thấy hồ sơ ảnh**, hoặc **nhãn không tìm thấy điểm gốc**. Số 0 nghĩa là chưa phát hiện trường hợp đó.
- Làm rõ “điểm bị dịch”: vị trí điểm trên bản vẽ khác tọa độ lưu lúc nhập; cần đối chiếu số liệu gốc trước khi sử dụng.
- Giải thích cảnh báo khoảng cách giữa các mốc trên tim tuyến; ghi rõ cảnh báo này áp dụng khi tính theo tim tuyến.
- Báo cáo tự xuống dòng, chỉ cuộn dọc, mở từ dòng đầu. Dùng phông Segoe UI; nút **Sao chép báo cáo** sao chép toàn bộ nội dung.
- Kiểm tra bản vẽ chỉ đọc dữ liệu. Những hướng dẫn có thể xóa ký hiệu không còn hồ sơ đều nói rõ tác dụng của lệnh; việc mở báo cáo không tự chạy các lệnh này.

## Tên nút và phần hướng dẫn

| Trước | Sau |
|---|---|
| Kiểm tra dữ liệu (BHTKT) | Kiểm tra bản vẽ |
| Chỉ thư mục ảnh | Chọn thư mục ảnh |
| Đồng bộ ký hiệu ảnh | Cập nhật ký hiệu ảnh |
| Xác nhận gắn / Bỏ gắn | Gắn ảnh vào hồ sơ / Gỡ ảnh khỏi hồ sơ |
| Chọn 2 cọc… | Chọn 2 mốc… |
| Theo cọc đã nhập… | Dùng mốc trong danh sách… |
| Tính lý trình các hồ sơ | Tính lý trình tất cả hồ sơ |
| Xác nhận cọc làm mốc | Xác nhận dùng mốc |
| Bỏ xác nhận mốc | Ngừng dùng mốc thủ công |
| Chèn / cập nhật | Vẽ / cập nhật ký hiệu |
| Xem / xóa Station Control | Xem / xóa mốc trên tim tuyến |

Các nút trên giữ nguyên tác dụng. Mã lệnh CAD vẫn dùng được. Phần hướng dẫn về tim tuyến diễn đạt “offset” thành “khoảng cách đến tim”.

## Cài đặt và kiểm tra

Bạn lưu công việc, đóng AutoCAD, cài **BHT-Setup-0.6.68.exe**, rồi mở lại AutoCAD 2024. Trong tab **Tổng quan** hoặc **Tuyến & báo cáo**, bấm **Kiểm tra bản vẽ** để đọc báo cáo mới.

Đã kiểm tra trên AutoCAD 2024: Core 233; giao diện 771, gồm báo cáo ở kích thước 620×420 và 900×680; kiểm tra dữ liệu 11 trường hợp, gồm điểm đã di chuyển, ký hiệu mất liên kết, số lỗi/cảnh báo và bảo toàn đối tượng. Nội dung tiếng Việt đã qua bộ kiểm tra của skill, không có lỗi hoặc cảnh báo. Chưa nghiệm thu bộ cài trên máy sạch khác.

Bộ cài cục bộ kèm thư viện biển/đèn/phông như bản trước. Không sửa hoặc lưu bản vẽ người dùng đang mở.
