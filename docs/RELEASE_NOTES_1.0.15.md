# BHT 1.0.15 — Cọc tiêu và bố trí báo hiệu

## Thay đổi

- Nhãn vàng H/Km nằm trong thân cọc; tự vừa chiều dài thân và xoay cùng cọc. Chân cọc vẫn ở điểm đo, vuông góc tuyến, không có đường nối.
- Bỏ nhãn tên màu trắng do BHT tạo cho điểm chỉ thuộc cọc đã vẽ. Giữ tên điểm dùng chung với nhóm khác, cao độ, ID và chữ người dùng vẽ riêng. Nhãn ở lớp khóa không bị xóa.
- Nút đặt cọc chuyển thành “Cọc đặt tự động”, nền xám và không bấm được. Dùng “Vẽ / cập nhật ký hiệu” hoặc chọn tim trong “Bố trí tất cả báo hiệu” để cập nhật cọc.
- Biển bố trí hàng loạt theo tiếp tuyến tại từng điểm, cùng quy ước với lựa chọn “Vuông góc với tuyến” trong đặt ký hiệu riêng. Bộ dựng hình, chân trụ, nhãn và đường nối được dùng chung. Hỗ trợ biển một trụ và hai trụ.
- Khoảng cách đã nhập vẫn tính tới toàn bộ hình và nhãn sau khi xoay. Trên đường cong có bước kiểm tra lại sau khi dựng nhãn để tránh lấn đường.

## Sử dụng

Đóng AutoCAD, cài BHT-Setup-1.0.15.exe rồi mở lại bản vẽ. Trong tab Tuyến, chọn Bố trí tất cả báo hiệu, chọn tim và khoảng cách, sau đó Áp dụng tất cả. Thao tác này cập nhật cọc và các báo hiệu hiện có. Với cọc mới, dùng Vẽ / cập nhật ký hiệu.

Không đổi tọa độ điểm khảo sát, liên kết lý trình hoặc mốc tuyến. Cột Km giữ cách bố trí hiện có. Ký hiệu ở lớp khóa được bỏ qua. Trường hợp không đủ tuyến để xác định hướng được báo riêng và giữ góc cũ. Chữ trắng tự vẽ không mang dữ liệu BHT được giữ lại.

## Kiểm chứng trước đóng gói

- Core: 249 kiểm tra đạt.
- Giao diện: 2.683 kiểm tra đạt, gồm nút cọc xám/vô hiệu hóa và bật lại khi chọn biển.
- Cọc: 21 kiểm tra; trình bày mới: 13 kiểm tra, gồm nhãn vừa thân, bỏ nhãn lặp, giữ nhãn điểm dùng chung, biển một/hai trụ có hình giống đặt riêng, hướng tiếp tuyến trên cung đảo chiều.
- Bố trí đường: 44 kiểm tra; kiểu đường nối: 20 kiểm tra. Undo, UCS, lỗi hoàn tác, lớp khóa và lưu/mở lại đạt.
- DWG mẫu: 52/52 cọc đặt và định hướng đúng; bản gốc không đổi. Có bản sao nghiệm thu tại `D:/lisp/BHT/audit-1.0.15/BHT-coc-tieu-1.0.15.dwg`.
- FAS và DLL bảo vệ đạt các bộ cọc/trình bày, bố trí đường, giao diện và đèn.

Bộ biển tổng hợp cũ dừng ở probe thư viện native trên cả 1.0.14 và 1.0.15; không tính bộ này là đạt. Kết quả kiểm tra bộ cài và runtime độc lập được lưu trong báo cáo nghiệm thu ngoài mã nguồn tại `D:/lisp/BHT/audit-1.0.15`. Chưa kiểm chứng lý trình native TDT có khóa.
