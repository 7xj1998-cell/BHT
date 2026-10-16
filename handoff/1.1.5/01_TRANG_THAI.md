# 1. Trạng thái build hiện tại

## Nhận diện

| Mục | Giá trị |
| --- | --- |
| Phần mềm | BHT — quản lý hiện trạng/hồ sơ khảo sát tuyến trong AutoCAD |
| Runtime | 1.1.5; DLL 1.1.5.0; loader BHT-1.1.5.lsp; gói cài dùng FAS |
| Môi trường kiểm tra | AutoCAD 2024, Windows x64, .NET Framework 4.8, C# 5 và AutoLISP |
| Nguồn làm việc gốc | D:/lisp/BHT/sharing-0654/source |
| Nhánh phát triển tại máy | development/incremental-confirmation |
| Nhánh công bố nguồn | releases/source-1.1.5 |
| Trạng thái | Build thử nghiệm có kiểm thử; không phải nghiệm thu toàn bộ roadmap |
| Mức ổn định lịch sử | 1.0.29 được ghi nhận là bản kiểm chứng/bàn giao ổn định gần nhất trong tài liệu dự án; không suy ra các bản sau đã nghiệm thu tương đương |

Bộ cài hiện có được giữ nguyên byte; công việc công bố chỉ bổ sung nguồn mở và bộ bàn giao. Bộ cài vẫn dùng DLL bảo vệ/FAS, nhưng mã C#/Lisp tương ứng được công bố để đọc và build. Không bắt buộc người tiếp nhận phải dùng bản obfuscate để phát triển.

## Chức năng đang có

- Nhập/quản lý điểm RTK, gắn ảnh hiện trường, tạo hồ sơ đối tượng nhiều nhóm, quản lý tuyến tham chiếu và mốc Km.
- Chọn biển/mặt biển, dựng block/nhãn/leader, cọc và đèn, tỷ lệ/hatch/bố trí tay, báo cáo Excel Unicode.
- Bảng đoạn/gói, tính và xác nhận bổ sung hồ sơ chưa khóa, giữ các hồ sơ đã khóa.
- Hiệu chỉnh riêng một hồ sơ có xem trước, lý do/người sửa, kiểm tra dữ liệu phụ thuộc, lịch sử và phục hồi khi lỗi trong các luồng đã hỗ trợ.
- Revision theo đối tượng, hợp nhất theo quan hệ cha/tổ tiên, phát hiện nhánh sửa độc lập, ngừng sử dụng có tombstone.
- Lưu bố trí GLOBAL/LOCAL rõ ràng; kiểm tra bình đồ liên quan, đồng bộ có xem trước và phát hành tệp mới Rxx.
- Kiểm tra/gom đoạn trùng có duyệt, chặn điểm ngoài đầu/cuối tim và ngoài phạm vi đoạn kể cả gán tay.
- 1.1.5: layer tim tham chiếu theo mã tuyến; nút chuyển layer cũ, không đổi hình học hoặc dữ liệu khảo sát.

“Đang có” nghĩa có mã triển khai và phạm vi kiểm thử được ghi nhận. Không có nghĩa mọi biến thể hồ sơ thực địa đã được hỗ trợ.

## Phần mới nhất: 1.1.4 → 1.1.5

Nhiều tim kế thừa cùng dùng BHT_TUYEN_TDT. AutoCAD MangleName sinh $0$, rồi nạp qua nhiều bản vẽ làm tên dài dần. RouteLayers đưa đúng tim vào BHT_TUYEN_ kèm mã tuyến cuối sau remap. Ví dụ DOAN_11 → BHT_TUYEN_DOAN_11. Không đổi mã tuyến cũ để ép tên đẹp, không đổi tất cả block/style sang chế độ Ignore.

Palette: Tuyến & báo cáo → TIM ĐANG CHỌN → Đặt layer theo tên tuyến. Xem trước toàn bộ tim, xác nhận rồi ghi transaction. Layer khóa, khác thuộc tính, có viewport override/freeze hoặc liên kết không an toàn sẽ bị chặn. Giữ layer cũ và thực thể ngoài phạm vi. Chỉ mở bản vẽ không tự chuyển layer.

Kiểm chứng bản sao gói nhiều đoạn: 9 tim chuyển layer, hồ sơ/RTK/hình học/XData giữ nguyên; Undo, lưu/mở lại, xuất/nhập và nạp lặp đạt. Nguồn khảo sát thực tế không nằm trong gói công khai.

## Nguyên tắc không được phá

1. RTK gốc là dữ liệu đo; vị trí block/nhãn không thay thế RTK.
2. Khóa từng đối tượng; không mở khóa cả đoạn để sửa một biển.
3. Hồ sơ cũ chưa có metadata không tự trở thành dữ liệu đã xác minh. Baseline cần duyệt và mang trạng thái LEGACY_UNVERIFIED.
4. Không quyết định bản đúng bằng thời gian sửa; dùng danh tính dự án/đối tượng và quan hệ revision.
5. Preflight và bước ghi phải cùng chính sách. Có xung đột chưa xử lý thì không nhập một phần.
6. Không tự ghi đè hồ sơ nguồn hoặc bản phát hành. Chỉ thử trên bản sao.
7. Không coi CoreTests là bằng chứng thao tác AutoCAD GUI hay giấy phép hoạt động.

## Giới hạn tổng quát

Còn 11 lỗi kiểm thử ký hiệu legacy ghi nhận từ các đợt trước. Chưa nghiệm thu đầy đủ GUI/DPI/công trình 15 đoạn. Chưa có UI hoàn chỉnh giải quyết nhánh revision, đối chiếu danh tính legacy, chuẩn hóa gói có lịch sử, hiệu chỉnh mọi biển nhiều mặt/nội dung tùy biến hoặc mốc ảnh hưởng nhiều đối tượng. LOCAL theo DWG, chưa theo từng layout. Chưa có cơ chế giao dịch phân tán bảo đảm phục hồi khi mất điện giữa ghi DWG và phát hành tệp.
