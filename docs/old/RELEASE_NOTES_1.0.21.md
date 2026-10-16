# BHT 1.0.21 — Đo tới tim tại trụ gần nhất

Ngày 14/10/2026, AutoCAD 2024 x64.

- Bổ sung dấu chấm đặc đúng tâm X của từng điểm khảo sát cho nhóm Khác, bảng quảng cáo, bảng chỉ dẫn và nhóm chưa xác định. Giữ cách vẽ riêng cho cọc tiêu.
- Cụm biển nhiều trụ dùng trụ khảo sát gần tim nhất theo XY để tính lệch tim, lý trình và phía tuyến. Áp dụng khi tính một hồ sơ, hàng loạt, tính lại theo tim và phân đoạn có tim riêng. Tâm đại diện dùng trình bày không đổi.
- Thêm `BHTDIMTIM` và nút **DIM từ điểm trụ gần nhất tới tim**. Tạo DIM thật trên lớp vàng `BHT_DIM_KIEM_TRA`, đo tới hình chiếu trên tim, không tính chênh cao và không ghi đè số đo bằng chữ. Gọi lại cập nhật DIM cùng hồ sơ/tim; có Undo.
- Sửa lỗi `eInvalidInput` khi block bảng chỉ dẫn cũ không có vòng chân trụ.

Cài bản mới sau khi lưu và đóng AutoCAD. Trong DWG cũ, chạy tính lại lý trình, cập nhật ký hiệu để bổ sung dấu chấm, rồi xuất lại Excel. Xuất báo cáo không tự tính lại dữ liệu đã lưu.

DWG người dùng trong thư mục New: trụ 154 cách TIMRANHD8 **2,251304 m**, trụ 155 **3,072335 m**. Giá trị cũ **2,662 m** là khoảng cách trung điểm. Hồ sơ OBJ-000120 mới lấy trụ 154, lý trình **Km48+525,01**. Biển Khác OBJ-000116 cách tim **1,143088 m**, lý trình **Km48+784,07**. Bản có DIM lưu riêng, giữ nguyên DWG gốc.

DIM là hình học kiểm tra tại thời điểm chạy lệnh. Khi đổi điểm hoặc tim, gọi lại lệnh để cập nhật.
