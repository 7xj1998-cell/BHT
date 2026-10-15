# Nghiệm thu BHT 1.0.20

Ngày 14/10/2026. Windows x64, .NET Framework 4.8, C#5, AutoCAD Core Console 2024. Bản nguồn trước sửa: D:/lisp/BHT/audit-1.0.20/source-before-1.0.20.zip.

## Kết quả trước đóng gói

- Core: 303 PASS, 0 FAIL trên DLL thường và DLL bảo vệ.
- REPORT120: 11 PASS trên DLL bảo vệ, gồm XRecord, danh mục thật, workbook thực và SHA256 DWG không đổi.
- Palette: 2.777 PASS trên FAS/DLL bảo vệ.
- Build đủ Core, Bridge, Palette; FAS và đồng bộ phiên bản 1.0.20 đạt.

## Nội dung kiểm tra

- Core C120: W.239a và S.509a, mặt trùng mã, fallback hồ sơ cũ, danh sách mat là nguồn ưu tiên, thiếu danh mục, biển tùy chỉnh Unicode, XML lỗi, XML sai chỉ số, nội dung riêng từng mặt. DESC_3 lưu chiều cao khác @ được giữ giống CAD.
- Workbook XLSX thực: số dòng chi tiết bằng tổng số mặt trong Tổng hợp; tách nội dung 5 m/6 m; số trụ và số mặt cụm không nhân theo các dòng phụ; ô số cho phép Excel SUM; giữ ID, tim, đoạn, gói, lý trình, offset; ghi đè atomic và báo cáo rỗng. Bảo toàn kiểm tra các nhóm bảng khác của C118.
- run_report120.ps1 đọc XRecord qua BhtDataService và danh mục biển thực. Mẫu W.239a + S.509a (5 m) ở Km48+530.05 / TIMRANHD8 / lệch 1,080 m xuất hai dòng, một hồ sơ, một trụ; tổng hợp khớp.
- Bản Desktop/filebaohieu.dwg: 118 hồ sơ/cụm, 146 mặt, 19 mặt mã S. Đọc bằng OpenForReadAndAllShare, so sánh snapshot hồ sơ và SHA256 đầu vào; không lưu lại DWG. Kiểm tra lại với DLL bảo vệ.
- Palette: bộ giao diện và quy trình sửa hồ sơ trên FAS/DLL bảo vệ. Không thay vị trí CAD hay thuật toán lý trình trong đợt này.

Kết quả Core cuối cùng, Palette, gói runtime độc lập và bộ cài xác minh được lưu ở D:/lisp/BHT/audit-1.0.20/BAO_CAO.md cùng log. Không cài tự động lên máy người dùng. Không sửa tài nguyên TDT gốc.

## Giới hạn dữ liệu kiểm chứng

Bản filebaohieu.dwg đã lưu có lý trình cũ, không phải toàn bộ trạng thái tim rãnh trong ảnh mới. Bản Doan8 dưới Ho so ngày22/09 không có hồ sơ BHT. Mẫu tích hợp tái hiện thông số ảnh để xác minh tính đầy đủ của báo cáo; không tự tính lại hay thay lý trình trong bản vẽ của người dùng. Các lý trình đang chỉnh cần lưu/xuất lại từ đúng DWG sau khi mở bản mới.

Hồ sơ thiếu mã mặt chỉ có thể dùng mã chính và số mặt đã lưu như CAD; phần mềm không suy đoán biển phụ từ ảnh hoặc từ hình vẽ rời không có hồ sơ.
