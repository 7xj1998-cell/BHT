# BHT 1.0.3 — Gộp nút trùng và làm rõ quy trình

Ngày 12/10/2026. Tiếp nối 1.0.2; giữ giao diện xanh, dữ liệu hồ sơ và thư viện hiện có.

- Chỉ còn một nút **Tính lý trình tất cả hồ sơ** ở đầu trang Tuyến & báo cáo. Nút giữ phương pháp hiện tại; nếu chưa đủ nguồn mốc, bảng kiểm tra mốc được mở trước khi tính.
- Nút tính riêng đổi thành **Tính lý trình hồ sơ này**. Chọn hai mốc và dùng danh sách mốc vẫn được giữ vì khác cách chọn nguồn.
- Tổng quan tập trung vào điều hướng. CSV nằm ở Điểm vị trí; KMZ/thống kê ảnh ở Ảnh hiện trường; kiểm tra bản vẽ và xuất dữ liệu ở Tuyến & báo cáo.
- Đã đổi tên thao tác block chung để phân biệt với block riêng của một hồ sơ; nút Tiếp tục dẫn sang tab nhập điểm khi chưa có dữ liệu.
- Đã cập nhật BHTHELP và viết lại hướng dẫn tiếng Việt, bảng lệnh tắt, danh mục nút và báo cáo kiểm thử.

## Kiểm thử và phạm vi

Core: 233 kiểm tra đạt. Giao diện và luồng sửa: 2.132 kiểm tra tự động đạt, cả DLL thường và runtime bảo vệ. Kiểm tra thư viện: 3.052 hình/nền/phông và 1.027 nội dung biển đạt. Các bộ thử mốc, tuyến, góc biển, đèn, tỷ lệ và lưu/mở lại đạt trong AutoCAD Core Console 2024.

Bản sao BOT830 có 177 hồ sơ, 801 điểm. Tính theo mốc hiện có trả về 73 hồ sơ có số/kết quả và 104 hồ sơ chưa được chấp nhận để cập nhật. Không tự sửa mốc hoặc di chuyển điểm nhằm làm hết cảnh báo. Xem báo cáo chi tiết để biết nguyên nhân.

Phiên AutoCAD đang mở dùng BHT 1.0.2; đã kiểm tra lệnh trên bản sao qua COM/MCP. Chưa nghiệm thu từng nút bằng chuột thật với 1.0.3, tuyến TDT proxy thực và máy sạch. Không coi số kiểm tra tự động là số nút đã bấm tay.

## Cài đặt

Lưu bản vẽ, đóng mọi AutoCAD, chạy BHT-Setup-1.0.3.exe rồi mở lại CAD 2024. Không cài đè hoặc thay DLL trong phiên đang làm việc.

Gói local có thư viện DWG/phông; chưa đưa gói tài nguyên bên thứ ba lên GitHub công khai. Mã nguồn trước khi sửa được giữ tại `old/pre-1.0.3` trong workspace.

Đọc **HUONG_DAN.md**, **DANH_MUC_LENH_NUT_1.0.3.md** và **BAO_CAO_NGHIEM_THU_1.0.3.md** trong thư mục docs đi kèm.
