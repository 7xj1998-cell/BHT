# BHT 1.0.14

Ngày 14/10/2026.

- Cọc tiêu giữ chân tại điểm khảo sát, đầu cọc vuông góc tiếp tuyến tại vị trí chiếu lên tim và hướng về tim. Không áp dụng khoảng lùi, đường nối thẳng hay gấp khúc của biển báo cho cọc tiêu.
- Bố trí tất cả đưa cả cọc đã dịch tay về điểm gốc và xóa đường nối cũ. Giữ tỷ lệ cọc và nhãn; không sửa POINT RTK, lý trình hoặc mốc. Cột Km giữ nguyên trong thao tác này.
- Chọn một cọc trong Hồ sơ rồi bấm **Đặt cọc tại điểm** để đặt riêng. Cọc phải có tuyến trong hồ sơ hoặc đường tham chiếu đã chọn khi bố trí. Nếu thiếu hướng, đặt riêng báo lỗi; bố trí hàng loạt giữ góc cũ và liệt kê cọc chưa xác định hướng. Cọc có thành phần trên lớp khóa được bỏ qua và báo ID.
- Giữ liên kết tim/PL dùng bố trí khi xuất và nạp dự án sang DWG khác.
- Sửa lỗi chọn nhầm thư mục runtime khi Support Path có BHT cùng phiên bản. Script kiểm thử bộ cài dùng được với Windows PowerShell 5.1; README đã đồng bộ phiên bản.

Đóng AutoCAD trước khi cài `BHT-Setup-1.0.14.exe`. Sau khi mở lại bản sao DWG, vào Tuyến > Bố trí tất cả báo hiệu, chọn tim tham chiếu rồi áp dụng để sửa cọc đã vẽ. Có thể Undo cả đợt bố trí. Bản này không tự sửa bản vẽ khi mở CAD.

Rà soát nghiệp vụ: [đánh giá theo quy trình kỹ sư](RA_SOAT_KY_THUAT_1.0.14.md). Kiểm chứng: [báo cáo nghiệm thu](BAO_CAO_NGHIEM_THU_1.0.14.md).

Chưa kiểm chứng lý trình native TDT có khóa hoặc thao tác nhiều tài liệu bằng chuột trong AutoCAD đầy đủ. Chân cọc là tâm điểm khảo sát, không chiếu tọa độ cọc về tim đường.
