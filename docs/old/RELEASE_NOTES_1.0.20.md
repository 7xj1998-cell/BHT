# BHT 1.0.20 — Báo cáo đủ mặt biển và biển phụ

Ngày 14/10/2026. AutoCAD 2024, Windows x64.

- Sửa Excel chỉ lấy mã biển chính của hồ sơ. Mỗi mặt biển có một dòng, kể cả biển phụ và các tấm trùng mã. Danh sách mặt được ưu tiên giống cách dựng ký hiệu CAD; hồ sơ cũ thiếu danh sách dùng mã chính và số mặt đã lưu.
- Mỗi mặt có tên, nhóm và nội dung riêng. Mã như S.509a@5 xuất S.509a, nội dung 5 m. Chữ/số XML gắn đúng chỉ số mặt; nội dung DESC_3 cũ của S.509a được ưu tiên theo cách dựng mặt thực tế. Không đưa XML thô vào tên biển.
- Tổng hợp đếm từng mặt, phân biệt mã/tên/nhóm/tình trạng/nội dung khác nhau. Tổng số hồ sơ/cụm và số mặt hiện riêng. Cùng ID, lý trình, tim, đoạn và gói giữa các mặt của một cụm. Trụ và tổng số mặt của cụm chỉ ghi ở dòng đầu để tránh cộng trùng.
- Cột Offset đổi thành “Lệch tim tuyến (m)”: khoảng cách mặt bằng từ vị trí hồ sơ tới tim dùng tính lý trình. Không lấy khoảng dịch ký hiệu trình bày.
- Xuất chỉ đọc dữ liệu đã lưu, không tính lại lý trình và không sửa DWG gốc. Giữ `.bhtseg`, tim riêng theo đoạn và các sửa CAD của 1.0.19.

Cài: lưu và đóng AutoCAD, chạy BHT-Setup-1.0.20.exe, mở lại rồi chọn Tuyến & báo cáo → Xuất báo cáo biển báo Excel. Báo cáo Excel đã xuất ở bản cũ cần xuất lại.

Kiểm tra bằng Core và AutoCAD Core Console với thư viện thực tế. Bản filebaohieu.dwg đã lưu trên Desktop có 118 cụm, 146 mặt (19 mã S). Không coi bản này là trạng thái đang sửa trong ảnh chụp: lý trình/tim có thể khác. Báo cáo mẫu tái hiện W.239a + S.509a (5 m), Km48+530.05, TIMRANHD8, lệch 1,080 m được kiểm tra riêng.
