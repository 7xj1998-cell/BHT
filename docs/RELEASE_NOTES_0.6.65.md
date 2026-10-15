# BHT 0.6.65 — Tên I.428 và mốc cọc bổ sung

## Biển I.428
Sửa mục I.428 hiện “Chưa có tên biển”: bản vẽ thư viện mang mã I.428, còn danh mục đặt tên ở I.428a. BHT dùng tên **Cửa hàng xăng dầu** từ chính danh mục đó cho mặt I.428. Tên hiển thị ở thẻ, xem trước và tìm kiếm không dấu. Không đổi hình/mã DWG và không sửa tài nguyên gốc. Không tự sửa mô tả hồ sơ cũ do người dùng nhập.

## Cọc tiêu bổ sung thủ công
Trong **Tuyến & báo cáo → Kiểm tra cọc đã nhập…**, cọc tiêu bổ sung thủ công xuất hiện cùng các cọc đo, kể cả khi chưa có Km/H. Dòng chưa dùng làm mốc có nền xanh nhạt; dòng dùng được/có cảnh báo ghi rõ trạng thái.

1. Chọn dòng cọc bổ sung đủ Km/H và vị trí.
2. Bấm **Xác nhận cọc làm mốc**, kiểm tra ID/lý trình trong hộp xác nhận rồi đồng ý.
3. Bảng mở lại với trạng thái **THỦ CÔNG — đã xác nhận làm mốc**. Bấm **Dùng các cọc này**, sau đó tính lý trình.
4. Muốn ngừng dùng cọc: chọn dòng và bấm **Bỏ xác nhận mốc**.

Xác nhận/bỏ xác nhận được lưu ngay vào DWG; Hủy bảng sau đó không hoàn tác thao tác đã lưu. Có thể dùng Undo hoặc bỏ xác nhận để thay đổi. Cọc thiếu Km/H hoặc thiếu điểm vị trí không được xác nhận. Sau khi sửa số Km/H, bộ điểm hoặc vị trí XY so với lúc xác nhận, cọc cần xác nhận lại.

Nguồn điểm/hồ sơ vẫn là thủ công, không được đổi thành RTK. Xác nhận chỉ áp dụng cho nội suy theo cọc tiêu (danh sách chung, nhóm cọc, cặp riêng), không đưa cọc thủ công vào cơ chế tự nhận mốc theo tim tuyến. Cột Km vẫn không tham gia luồng cọc tiêu. Hồ sơ khác dùng chung điểm thủ công phải xác nhận riêng.

Kết quả có dùng mốc thủ công giữ nguồn hai cọc, ghi rõ “Có mốc bổ sung thủ công đã xác nhận; kết quả tham khảo”, field_manual_control=1 và station_status=NEEDS_REVIEW. Cảnh báo hình học, trùng số, nhiều đoạn, đoạn dài/hệ số bất thường vẫn có hiệu lực. Xác nhận mốc không bỏ qua các kiểm tra này. Các kết quả liên quan được đánh dấu cần tính lại khi bỏ/đổi xác nhận.

Khoảng cách trong bảng tính từ **mốc được dùng trước đó**, bỏ qua các dòng thủ công chưa xác nhận; số cuối bảng phân biệt cọc được dùng và tổng cọc hiển thị.

## Kiểm thử
AutoCAD 2024: Core 233; giao diện 745 gồm tên/tìm I.428, chọn dòng/xác nhận/bỏ xác nhận, cọc thiếu dữ liệu; 39 kiểm tra danh sách/mốc thủ công, lưu/mở lại/API .NET; 51 kiểm tra tính theo cọc và hồi quy tạo cọc thủ công. Kiểm tra danh sách/mốc thủ công trên FAS/DLL bảo vệ. Chưa nghiệm thu trên máy sạch khác. Không sửa hoặc lưu đè bản vẽ người dùng đang mở.

Bộ cài cục bộ kèm thư viện biển/đèn/phông như trước; không công bố tài nguyên bên thứ ba lên GitHub.
