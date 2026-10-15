# Lệnh và nút BHT 1.0.25

| Chức năng | Lệnh / nút | Kết quả |
|---|---|---|
| Kế thừa kết quả giữa DWG | BHTNAPTICHLUY / Nạp dữ liệu kế thừa (.bht) | Giữ hồ sơ và lý trình; đối chiếu dữ liệu trùng trước khi nạp |
| Quản lý theo đoạn | BHTXULYDOAN | Bảng chọn đoạn, gói, tim, số hồ sơ đã xác nhận |
| Tính đoạn được chọn | Tính lý trình theo đoạn | Xem trước rồi tính các hồ sơ đủ điều kiện; một Undo khôi phục |
| Bảo vệ kết quả | Xác nhận kết quả đoạn | Kiểm tra số liệu và bảo vệ kết quả hiện có của đoạn |
| Chỉnh sửa kết quả | Mở lại kết quả đoạn | Bỏ bảo vệ đúng đoạn; giữ số liệu để đối chiếu |
| Chuyển sang đoạn kế tiếp | Xuất dữ liệu tích lũy (.bht) | Xuất toàn bộ hồ sơ và bảng phân đoạn hiện có |
| Bàn giao riêng một đoạn | Xuất hồ sơ đoạn (.bht) | Xuất hồ sơ đã xác nhận và tài nguyên liên quan của dòng đang chọn |
| Khởi tạo khảo sát theo tim đích | BHTNAPBAOHIEU / Nạp khảo sát mới (tính lại lý trình) | Đặt lại kết quả tính; không dùng để kế thừa kết quả đã xác nhận |
| Chọn đoạn cho một hồ sơ | BHTGANDOAN | Bảng chọn để gán/bỏ gán; cần mở lại đoạn nếu hồ sơ đã xác nhận |
| Nạp bảng phân đoạn | BHTGOITHAU | Đọc Excel hoặc .bhtseg; bảng được lưu trong DWG và gói .bht |
| Nạp tim TDT / Polyline | BHTTUYENTDT / BHTTUYEN | Khai báo loại tim, hình học, hướng tính |
| Mốc lý trình | BHTDSMOC | Bảng mốc: xem vị trí, đối chiếu và xóa mốc đã chọn |
| Kiểm tra khoảng cách tới tim | BHTDIMTIM | DIM từ điểm trụ tính lý trình tới hình chiếu trên tim |

Tại tab Tuyến, nhóm **QUẢN LÝ XỬ LÝ THEO ĐOẠN** chứa hai nút mở quy trình mới. Các kết quả đã xác nhận được bỏ qua khi tính hàng loạt. Phải mở lại đúng đoạn trước khi sửa tim, phân đoạn hoặc số liệu được bảo vệ.
