# BHT 1.1.3 — Gom đoạn trùng có đối chiếu

Đã thêm nút Kiểm tra / gom đoạn trùng trong bảng đoạn hiện có. Xem trước mã giữ/bỏ, gói, phạm vi, phía và tim; chỉ gom các dòng không tham chiếu khi đủ bằng chứng. Lưu bản ghi đầy đủ trong SEG_ALIAS, một giao dịch và Undo. Không mở khóa hoặc đổi dữ liệu OBJ/RTK/ROUTE/CAD.

Đã chặn nạp kế thừa, nạp khảo sát và thay bảng khi mã đoạn khác cách viết có thể tạo thêm dòng trùng. Không tự hợp nhất hai mã đang dùng, hai tim khác nhau hoặc bản có lịch sử cần chuyển đổi riêng. Gói .bht giữ định dạng ZIP hiện có.

DOAN15 đã kiểm tra trên bản sao: 30 → 15 SEG, giữ 179 OBJ/801 RTK/108 hồ sơ khóa. DOAN08/GOI03 giữ vì có 15 hồ sơ khóa. Chưa chuẩn hóa toàn bộ cách viết mã gói; một số file cũ còn ứng viên tham chiếu cả hai mã nên bị chặn.

Xem hướng dẫn HUONG_DAN_HIEU_CHINH_1.1.3 và báo cáo BAO_CAO_NGHIEM_THU_1.1.3. Phát triển cục bộ, không push. Không nghiệm thu toàn bộ roadmap hoặc khẳng định đã sửa lỗi đứng CAD từ kết quả gom đoạn.
