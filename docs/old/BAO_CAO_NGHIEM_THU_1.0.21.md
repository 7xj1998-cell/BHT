# Nghiệm thu BHT 1.0.21

Ngày 14/10/2026. C#5, .NET Framework 4.8 x64, AutoCAD 2024. Bản nguồn trước sửa nằm tại D:/lisp/BHT/audit-1.0.21/source-before-1.0.21.zip.

- Core: 303 kiểm tra đạt, gồm báo cáo đủ mặt biển và đọc bảng XLSX thực tế.
- OFFSET121: 26 kiểm tra Lisp đạt, gồm lệnh công khai và Undo một bước; kiểm tra độc lập bằng .NET xác nhận DIM thật, Measurement, hai đầu tọa độ XY, không text override, lớp khóa không làm đổi DIM, gọi lại không nhân bản và lưu/mở lại giữ kết quả. Kiểm tra trên DLL bảo vệ và FAS.
- STATION117: 22 đạt, gồm tính lại theo tim, giữ số cọc, Undo và mở lại.
- SEG119: 19 đạt, gồm tim riêng, chọn đoạn, hoàn tác khi lỗi, đổi tên tim và truyền dự án.
- FIELD: 51 đạt, mở lại và Undo. CAPTION118: 21 đạt, API, thứ tự vẽ và mở lại. Palette: 2.817 đạt trên bộ thường; bộ FAS/DLL bảo vệ được xác minh riêng.
- REPORT120: 11 đạt trên DLL bảo vệ, đọc DWG mới do người dùng chỉ định; SHA256 đầu vào không đổi.

DWG thực tại D:/lisp/BHT/New/BD-TD-TN_Doan8(Km48+469.1+Km50+290.77).dwg có TIMRANHD8, OBJ-000120 (trụ154/155) và OBJ-000116 (biển Khác). Đo độc lập từ tọa độ từng trụ lên Curve tham chiếu: 2,251304 / 3,072335 / 1,143088 m. Hồ sơ 120 đổi từ trung điểm 2,662 m sang trụ154 2,251 m; lý trình Km48+525,006. Hồ sơ116 giữ Km48+784,073. Bản sao có ba AlignedDimension thật, dấu chấm tại điểm của nhóm Khác và view BHT_DIM_154_155 / BHT_DIM_BIEN_KHAC để mở kiểm tra.

Không tự cài bộ phát hành. Không thay tài nguyên TDT gốc. Tính theo hình học tham chiếu TIMRANHD8 đã lưu trong DWG; khi tim gốc đổi cần cập nhật tham chiếu và tính lại. Các phép kiểm trên máy phát triển không thay nghiệm thu toàn bộ hồ sơ ngoài hiện trường.

Log và kết quả cuối bộ cài/runtime: D:/lisp/BHT/audit-1.0.21/BAO_CAO.md.
