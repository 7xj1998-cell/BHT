# BHT 1.0.24 — Chọn đoạn/gói và biển tên đường

**Chọn đoạn / gói cho hồ sơ…** (`BHTGANDOAN`) mở bảng gồm mã đoạn, mã gói, tên công trình, lý trình đầu/cuối, phía đường, tim và kết quả đối chiếu hồ sơ. Chọn dòng rồi Gán đoạn đã chọn; Hủy giữ nguyên; Bỏ gán đoạn xóa lựa chọn. Một lần Undo khôi phục thao tác. Bảng hiện cả đoạn ngoài phạm vi để người dùng chủ động gán thủ công.

Tính lý trình một hồ sơ, tất cả hồ sơ theo tuyến, hoặc tính lại theo tim đang chọn cập nhật đoạn từ kết quả mới. Giữ gán thủ công còn hợp lệ. Đoạn gắn tim khác không được tự nhận chỉ vì trùng Km; chung biên hoặc chồng khoảng vẫn yêu cầu chọn. Phía đường đã nhập dùng đối chiếu bảng đoạn; phía so với tim rãnh không thay thế phía đường. Hồ sơ đúng khoảng Km nhưng sai phía có trạng thái `KHAC_PHIA_DUONG`, hiển thị nguyên nhân trong thông tin hồ sơ và bảng chọn.

I.449 có hai ô nội dung: **Dòng trên** mặc định ĐƯỜNG và **Tên đường (dòng dưới)**. CAD vẽ hai dòng căn giữa trong mặt biển. Tên dài được thu nhỏ để nằm trong biển. Giữ giá trị NAME đã nhập trong dữ liệu cũ; bổ sung ĐƯỜNG khi chưa có HEADER. Xuất hồ sơ giữ cả hai dòng theo thứ tự. Mở Nội dung biển và Vẽ/cập nhật ký hiệu để cập nhật biển cũ.

## Đối chiếu DWG đoạn 10

Đã kiểm tra bản sao `D:/lisp/BHT/New/BD_TD_TN(Km50+140-Km51+634.24).dwg`. OBJ-000058 có Km50+871.60 nhưng lưu `phia_duong=PHAI`; DOAN_10 yêu cầu TRAI. Đây là lý do biển không tự vào đoạn. `phia_tuyen=TRAI` là phía so với tim đang tính. Chọn DOAN_10 thủ công hoặc kiểm tra và sửa Phía đường tại hồ sơ nếu số liệu nhập sai; không tự đảo dữ liệu khảo sát.

Tim tên TIMRANHD10 trong file đang khai báo TIM_DUONG, và cả 15 đoạn chưa gắn tim riêng. Nếu đây là tim rãnh, dùng Sửa tuyến chọn Tim rãnh, gán TIMRANHD10 cho DOAN_10 trong bảng đoạn rồi tính lại. Không suy loại tuyến chỉ từ tên ID.

Lưu bản vẽ, đóng AutoCAD, cài `BHT-Setup-1.0.24.exe`, mở lại và kiểm tra `BHTNETINFO`. Không nạp chồng DLL mới vào phiên đang dùng bản cũ. Kiểm tra này dùng hình học tham chiếu và mốc BHT đã lưu trong DWG; không thay thế nghiệm thu đọc lại native TDT.
