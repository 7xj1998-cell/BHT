# BHT 1.0.29 - Nhãn theo đường dẫn và sửa bố trí hàng loạt

- Sửa lỗi `bad function: 1.51086`: biến cục bộ `angle` che hàm AutoLISP cùng tên khi bố trí hàng loạt gọi tính hướng nhãn. Đổi tên biến trong module ký hiệu.
- Nhãn biển báo lấy đoạn dài nhất của đường dẫn; mã biển chính và các biển phụ ở trên, lý trình ở dưới, căn giữa cùng trục. Biển hai trụ dùng đường dẫn phía trên theo chiều chữ dễ đọc. Giữ lựa chọn song song/vuông góc; chọn song song để trình bày theo mẫu DT.830.
- Thêm Tuyến > 5. Trình bày bản vẽ > Áp dụng mẫu nhãn báo hiệu… (`BHTMAUNHAN`). Chọn biển/nhãn, Enter áp dụng mọi biển đã vẽ; Esc hủy. Trả nhãn đã dịch chuyển thủ công về mẫu, giữ vị trí hình biển và số liệu khảo sát. Cập nhật thông thường vẫn giữ nhãn kéo tay.
- Nội dung biển khi đối chiếu được giải mã thành mặt biển, địa danh và lý trình trên biển. XML khác định dạng nhưng cùng nội dung, Unicode NFC/NFD tương đương không còn bị báo xung đột. HÒA và HOÀ vẫn là khác nội dung, không tự đổi dấu hay chọn bản đúng.

Đã tái hiện lỗi bố trí bằng FAS 1.0.28 trên bản sao gói đoạn 8; bản sửa source và FAS chạy thành công, giữ điểm/tim/lý trình. Không thay dữ liệu gốc hay tự cài vào phiên AutoCAD đang mở.

Lưu công việc, đóng tất cả AutoCAD rồi chạy BHT-Setup-1.0.29.exe. Kiểm tra BHTNETINFO; chọn nhãn song song, bấm Áp dụng mẫu nhãn báo hiệu nếu cần chuyển các nhãn cũ về mẫu.
