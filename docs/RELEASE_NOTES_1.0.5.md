# BHT 1.0.5 — Sửa đường nối và lựa chọn báo hiệu

Ngày 12/10/2026. Dành cho AutoCAD 2024.

- Đã sửa mở Công cụ bổ sung và gói thầu ngay khi tick, không cần đổi tab rồi quay lại.
- Đã cho chọn Đường thẳng/Gấp khúc trong Khoảng cách đặt báo hiệu. Đặt tự do lưu đúng kiểu riêng của từng hồ sơ; cập nhật không tự đổi thẳng thành gấp khúc.
- Đã sửa chọn toàn bộ để giữ lựa chọn sau khi lệnh kết thúc, kể cả khi PICKFIRST trước đó bị tắt.
- Đã thêm Chọn báo hiệu cùng nhóm để chọn đủ hình, nhãn, đường nối và dấu gốc; thêm SELECTSIMILAR theo lớp để chọn các block khác tên.

Mở **Tuyến & báo cáo → Công cụ bổ sung** để dùng các nút mới. Chọn **Khoảng cách đặt báo hiệu… → Đường thẳng → Áp dụng tất cả** nếu muốn đổi toàn bộ biển/bảng/đèn đang có. Công cụ này thay kiểu riêng trước đó của các ký hiệu được cập nhật; điểm đo giữ nguyên.

Lệnh: **BHTKHOANGCACH**, **BHTCHONBAOHIEU**, **BHTCHONTUONGTU**, **BHTSELECTSIMILAR**. SELECTSIMILAR theo lớp đặt SELECTSIMILARMODE=2, áp dụng cho cả lệnh CAD gọi về sau; CAD vẫn phân biệt block, TEXT và LINE. Dùng BHTCHONTUONGTU hoặc BHTCHONBAOHIEU để COPY đủ thành phần.

Lưu công việc, đóng CAD rồi chạy BHT-Setup-1.0.5.exe. Bộ cài chưa tự được cài vào phiên CAD đang mở. Xem HUONG_DAN.md và BAO_CAO_NGHIEM_THU_1.0.5.md để biết giới hạn kiểm thử.
