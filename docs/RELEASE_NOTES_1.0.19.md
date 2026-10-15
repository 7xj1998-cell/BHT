# BHT 1.0.19 — Bảng đoạn riêng và tim theo đoạn

Bản phát hành ngày 14/10/2026, dành cho AutoCAD 2024 trên Windows.

- Nạp Excel `.xlsx`, xem trước và chuyển thành `.bhtseg`. Bảng gốc không đổi. Không cần cài Excel để nạp.
- `.bhtseg` lưu Unicode, mã đoạn/gói, lý trình, phía, tên gói và mã tim; kiểm tra phiên bản, số đoạn và SHA256 trước khi nạp. Tệp này chứa bảng đoạn, không chứa hình học. Chuyển cả dự án và tim dùng `.bht`.
- Nhận bảng `Phan chia goi thau.xlsx` hiện có: 15 đoạn, 3 nhóm tên gói ở ô gộp M, Km ở B+C/E+F. Do bảng không có mã gói, BHT tạo GOI_01…03 và công khai ở phần xem trước; có thể sửa. Không lấy cột chiều dài để đổi lý trình.
- Có mẫu XLSX với SEGMENT_ID, PACKAGE_ID, KM_START_M, KM_END_M, SIDE, ROUTE_ID, PACKAGE_NAME, NOTE. Lý trình nhận mét hoặc Km48+469,10. SIDE là TRAI/PHAI/HAI_BEN.
- Mỗi đoạn chọn một tim đường/tim rãnh/tuyến khác đã khai báo. Chọn nhiều dòng rồi gán cùng tim. Tim cần hình học và ít nhất hai mốc hợp lệ để phân đoạn.
- Phân đoạn dùng lý trình theo tim từng đoạn. Chọn tim phù hợp gần nhất, chênh offset không quá 0,01 m hoặc trùng mốc biên thì báo nhiều đoạn và dùng BHTGANDOAN. Phía lấy theo chiều tăng lý trình tim đó. Đoạn không gán tim vẫn dùng lý trình/phía hiện có.
- Tính riêng và tính toàn bộ giữ tim của đoạn đã gán, kể cả khi chế độ chung dùng cọc. Cọc có số Km/H giữ số khảo sát. Tính lại theo một tim không ghi đè các đoạn đã gán tim khác.
- Thay bảng xóa gán tự động cũ để phân lại. Gán tay chỉ giữ nếu tim, phạm vi, phía và gói không đổi; thay liên kết tim đánh lại lý trình để cập nhật. Đổi tên tim cập nhật mã trong đoạn, mất tim giữ mã thiếu để yêu cầu chọn lại.

Cách dùng: Tuyến & báo cáo → Phân chia đoạn/gói thầu → Nạp Excel / bảng BHT → xem dữ liệu → lưu .bhtseg và nạp → Xem đoạn / gán tim đường, tim rãnh → áp dụng → Gán đoạn/gói theo tim của đoạn. Lưu DWG sau khi hoàn tất.

Đóng AutoCAD, cài BHT-Setup-1.0.19.exe rồi mở lại. Không NETLOAD chồng DLL vào phiên CAD cũ. TSV chỉ nhận khi chuyển đổi dữ liệu cũ. XLSX có cấu trúc khác hai mẫu trên cần bố trí lại theo mẫu; công thức cần có giá trị Excel đã tính/lưu.
