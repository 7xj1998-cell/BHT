# BHT 0.6.64 — Dùng trực tiếp cọc đã nhập lý trình

## Cách dùng cho một đường
1. Cọc tiêu đã lưu Km/H và gắn đủ điểm đo được lấy trực tiếp; không nhập lại số cọc, không đặt tên chuỗi hoặc thêm từng cọc.
2. Trong **Tuyến & báo cáo**, bấm **Kiểm tra cọc đã nhập…**. Bảng mở sẵn theo lý trình tăng, hiện ID cọc, lý trình, XY, khoảng cách từ cọc trước và cảnh báo đoạn.
3. Kiểm tra các cọc thuộc cùng một đường, bấm **Dùng các cọc này** rồi **Tính lý trình các hồ sơ**. Xác nhận danh sách không tự tính lại hoặc vẽ lại ký hiệu.
4. Tại từng hồ sơ, **Tính lý trình** dùng nguồn đã chọn; khi chưa có tim/nguồn cọc/cặp riêng sẽ mở bảng kiểm tra. **Theo cọc đã nhập…** chuyển riêng hồ sơ đó sang danh sách chung sau khi xem lại. **Chọn 2 cọc…** xử lý đoạn cần đối chiếu.

Không cần “Tạo chuỗi” cho một đường. **Nâng cao: nhóm đường / nhánh** chứa các thao tác tạo/sửa/xóa/dùng nhóm cho bản vẽ có nhiều đường và chuyển lại phương pháp tim tuyến. Chuỗi cũ vẫn đọc được. Nhóm/cặp riêng của từng hồ sơ vẫn có ưu tiên; đổi nguồn mặc định không xóa các gán riêng. Muốn đổi hồ sơ cũ sang danh sách chung, dùng **Theo cọc đã nhập…** tại hồ sơ đó.

## Kiểm soát dữ liệu
- Chỉ cọc tiêu đủ Km/H và tất cả điểm đo tồn tại; cột Km, cọc thiếu dữ liệu và cọc thủ công 0.6.63 không làm mốc.
- Trùng số Km/H không bị âm thầm gộp; chặn các đoạn liên quan. Góc đổi hướng từ 90 độ trở lên trong thứ tự dự kiến cũng yêu cầu đối chiếu.
- Đoạn dài hơn 500 m hoặc hệ số chênh lý trình/khoảng cách lệch quá 3% được cảnh báo và không tự ghi kết quả. Phạm vi lệch đường nối cọc mặc định 100 m. Có thể dùng cặp riêng đã kiểm tra, kết quả bất thường vẫn là tạm tính.
- Điểm thuộc nhiều đoạn vẫn phải chọn hai cọc; không chọn đoạn gần nhất rồi coi là đúng. Không ngoại suy.
- Khi thêm/xóa cọc, đổi số hoặc tọa độ XY, danh sách đã xác nhận hết hiệu lực; giữ lý trình cũ và yêu cầu kiểm tra lại. Không tự nối qua cọc bị xóa. Xem lại danh sách không thay đổi sẽ không đánh dấu kết quả cũ cần tính lại.
- Kết quả giữ ID hai cọc và nguồn nội suy để xem trong hồ sơ. Các phép kiểm tra chỉ giúp phát hiện bất thường; không xác định chắc chắn cọc cùng đường từ tọa độ. Người dùng cần kiểm tra danh sách, dùng nhóm khi có nhiều đường/nhánh.

## Lưu trữ và kiểm thử
Danh sách được lưu nội bộ dưới FIELD_CHAIN/BHT_IMPORTED_MARKERS, kind=IMPORTED_MARKERS, input_snapshot và blocked_pair. Đây là chi tiết lưu trữ; người dùng không phải tạo hoặc đặt tên chuỗi. Snapshot kiểm tra trực tiếp dữ liệu hiện tại và được dùng chung trong mỗi lần cập nhật hàng loạt/xoay biển.

AutoCAD 2024 trên máy phát triển: Core 233, giao diện 738; 25 kiểm tra danh sách tự lấy và lưu/mở lại/API .NET với snapshot dài; 51 kiểm tra cọc thực địa, lưu/mở lại và Undo API; hồi quy cọc thủ công 0.6.63. Luồng mới và luồng cọc cũ đạt trên FAS/DLL bảo vệ. Chưa nghiệm thu trên máy sạch khác; chưa thay DLL hoặc sửa bản vẽ người dùng đang mở.

Bộ cài cục bộ tiếp tục kèm thư viện biển, đèn và phông; không công bố tài nguyên bên thứ ba lên GitHub. Giới hạn bàn giao một DWG không phụ thuộc phông chưa thay đổi.
