# BHT 1.1.0 - Báo cáo, hướng nhãn và bố trí theo tuyến

Bản build kiểm tra cục bộ, nâng từ 1.0.36. Dùng bộ cài BHT-Setup-1.1.0.exe và bản sao trong D:/lisp/BHT/Test-1.1.0. Chưa coi các bài Core Console là nghiệm thu giao diện AutoCAD.

- Đã bỏ giới hạn âm thầm 40 dòng của báo cáo nhanh. Cửa sổ có cuộn, xuống dòng, tăng/giảm chữ, sao chép và kéo đổi kích thước.
- Đã chuyển báo cáo và bảng xem trước tính theo đoạn sang cửa sổ không chặn CAD. Trước khi áp dụng, BHT kiểm tra lại bản vẽ và dữ liệu; phương án cũ bị từ chối nếu dữ liệu đã đổi.
- Đã chặn điểm đầu cách tim trên 1 m, chiều đi ra ngoài đầu/cuối tuyến và đổi cơ sở khoảng cách khi đã có mốc. Chọn giữa tuyến có cảnh báo phần phía sau điểm đầu bị loại.
- Nhãn biển chọn chiều đọc theo chiều tăng Km và tiếp tuyến tại điểm khảo sát. UCS và góc nhìn không đổi kết quả. Hướng song song/vuông góc đường nối vẫn theo lựa chọn hiện có.
- Đã giảm quét toàn bản vẽ trong cập nhật hàng loạt, giữ chỉ mục trong từng thao tác và chỉ đo lại ký hiệu chưa đủ khoảng lùi.
- Đã sửa vòng bố trí không hội tụ trên D13: bù chuyển động nhãn giữa đường dẫn, kiểm tra lại khoảng lùi thực tế và giữ vị trí đã đủ khoảng cách khi chạy lặp. Lỗi có mã hồ sơ và nguyên nhân, kèm đối chiếu phục hồi.

Không đổi cấu trúc ZIP .bht, không tự khởi tạo revision khi mở bản vẽ. Hiệu chỉnh riêng, lịch sử, merge theo tổ tiên, GLOBAL/LOCAL và quản lý phát hành tiếp tục giữ phạm vi đã có của 1.0.36.

Đọc [hướng dẫn 1.1.0](HUONG_DAN_HIEU_CHINH_1.1.0.md), [bài kiểm tra toàn bộ](HUONG_DAN_TEST_TOAN_BO.md) và [báo cáo kiểm chứng](BAO_CAO_NGHIEM_THU_1.1.0.md). Các giới hạn legacy, giấy phép AutoCAD, kiểm thử GUI và những phần roadmap chưa hoàn tất được ghi riêng; không tự coi bản này là đã nghiệm thu toàn bộ công trình.
