# BHT 1.0.12

Ngày 13/10/2026.

Đã sửa sai số hình học khi tim TDT trả về một cung tròn đơn. BHT giữ cung trong Polyline tham chiếu bằng bulge, thay vì chia thành các đoạn thẳng. Đoạn thẳng đơn giữ hai đỉnh; chiều dài trả về khớp với đường tham chiếu thực tế.

Trường hợp tái hiện trên 1.0.11: nửa đường tròn bán kính 10 m bị ngắn hơn khoảng 3,25 mm, sai vị trí tối đa khoảng 3,11 mm tại các điểm thử. Cùng mẫu trên 1.0.12 không còn sai số này. Các cung có mặt phẳng song song XY được giữ chính xác; đường cong khác vẫn dùng phương pháp lấy mẫu hiện có.

Giữ bố cục gọn của 1.0.11 và bản sửa copy biển/hatch. Với biển cũ, chạy `BHTSUASAOCHEP` trong DWG nguồn đang hiển thị đúng trước khi copy.

## Cập nhật tuyến đã lấy từ TDT

Đóng AutoCAD trước khi cài. Khi có khóa và phiên TDT hoạt động, dùng `BHTTUYENTDT` lấy/cập nhật lại tim tham chiếu, kiểm tra mốc và tính lại lý trình. BHT không tự sửa tim TDT gốc hoặc cập nhật tuyến khi khởi động.

## Phạm vi TDT

Bản này sửa hình học tham chiếu, chưa đọc trực tiếp lý trình gốc TDT 9.1. `BHTDOCCOCTDT` vẫn đọc nhãn Km để duyệt mốc; `BHTMOCKM` dùng mốc đã biết. Chưa kiểm chứng native do hiện không có khóa TDT. Thử trên bản sao DWG trước khi áp dụng cho công việc.
