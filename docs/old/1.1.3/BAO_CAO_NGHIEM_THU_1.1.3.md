# Báo cáo kiểm tra BHT 1.1.3

Ngày 16/10/2026. Phiên bản 1.1.2 → 1.1.3. Git root D:/lisp/BHT/sharing-0654/source, nhánh development/incremental-confirmation. Giữ dirty worktree có sẵn; baseline và hash trong audit-1.1.3/baseline. Không commit/push.

## Vấn đề và nguyên nhân

DOAN15.bht có 30 SEG: DOAN01…15 và DOAN_01…15, ba tên gói với mã có/không dấu gạch dưới. Các cặp cùng phạm vi/phía/tên gói nhưng metadata nguồn khác. Hợp nhất cũ dùng khóa SEG chính xác nên thêm cả hai bộ; chính sách đổi cách viết gói chỉ hoạt động khi tập segment_id giống hệt. Tái hiện thực bằng DLL 1.1.2, bản sao DOAN9 và DOAN10: SEG_before=15, SEG_after=30, 175 thực thể được nạp.

DOAN15 có 179 OBJ, 801 RTK, 108 hồ sơ khóa. DOAN08/GOI03 có 15 OBJ khóa, tim DOAN_08; các đoạn 9–15 dùng mã có dấu gạch dưới. Xóa toàn bộ bộ mã cũ sẽ làm mất đoạn được hồ sơ khóa tham chiếu.

## Thay đổi mã và hành vi

| File / hàm | Thay đổi |
|---|---|
| BHT.Core/SegmentDuplicatePolicy: Plan, SameMeaning, NewAliasConflicts | Nhận ứng viên khác dấu gạch dưới, kiểm tra ý nghĩa và tham chiếu; chặn mã mới có thể nhân đôi |
| BHT.Bridge/SegmentDuplicates: Preview, Apply | Xem trước không ghi, kiểm tra lại toàn bộ dữ liệu/đúng DB, lưu journal và xóa SEG dư trong một transaction |
| ProjectTransfer.CopyTransaction | Cùng chính sách chặn trước rolling/survey-only, trước ghi và sao chép CAD |
| SegmentStore.Apply | Chặn thay bảng Excel/BHT tạo mã đoạn khác cách viết |
| BhtStore.Write/Delete và 04-objects.lsp progress-guard | SEG_ALIAS chỉ ghi thêm, không sửa/xóa nhật ký |
| SegmentTableForm, SegmentDuplicateReviewForm | Nút trong bảng hiện có; đối chiếu đầy đủ ý nghĩa giữ/bỏ, xác nhận riêng, tải lại bảng; chặn gom khi có ô chưa áp dụng |
| Segment113Tests, Segments113Probe, run_segments113 | Hồi quy chính sách, dữ liệu thật, giữ dữ liệu, journal, giao dịch, Undo và import lặp |

Trước: nạp hai cách viết thành 30 dòng. Sau: chặn nạp thêm mã ứng viên; gom có xác nhận những dòng không tham chiếu. Giữ mã đang dùng, không viết lại OBJ/RTK/ROUTE/CAD, không mở khóa và không tạo revision giả. Nhật ký SEG_ALIAS chứa snapshot đầy đủ của dòng bỏ/giữ. ZIP .bht không đổi.

## Kiểm tra đã thực chạy

| Mức / bộ kiểm tra | Kết quả |
|---|---|
| Build .NET 4.8 / CoreTests | 443 PASS, 0 FAIL, gồm 15 kiểm tra mới |
| Core Console / Segments113, DLL protected cuối | 78 PASS, 0 FAIL; thêm 2 kiểm tra lệnh/Undo và guard nhật ký Lisp đạt |
| Core Console / Import111, D14/D15 | 16 PASS, 0 FAIL |
| Core Console / Import112, picker/hủy/lỗi/FAS | 33 PASS, 0 FAIL |
| Core Console / bảng, font mô phỏng 100–200% | 316 PASS, 0 FAIL |
| Core Console / M1 bổ sung, Undo, rollback | 31 Lisp + 29 .NET, mở lại DWG đạt |
| Core Console / hiệu chỉnh từng hồ sơ | 50 PASS, 0 FAIL |
| Core Console / hợp nhất revision | 17 PASS, 0 FAIL |
| Core Console / quản lý bình đồ, phát hành/lưu trữ | 20 PASS, 0 FAIL |
| Core Console / bố trí GLOBAL và LOCAL | 21 PASS, 0 FAIL |
| Core Console / bố cục Palette | 3.102 kiểm tra đạt |
| Core Console / hồi quy báo cáo, điểm đầu và áp dụng xem trước | 10 PASS, 0 FAIL |
| Build runtime | FAS 1.1.3 và Core/Bridge bảo vệ Obfuscar 2.2.50 đạt |

Trên tám gói thực trong New: DOAN8/9 giữ 15 dòng; DOAN10/14/15 gom 30 → 15; DOAN11/12/13 bị chặn toàn bộ do các danh sách ứng viên còn tham chiếu cả hai mã. Không bỏ qua các tham chiếu này để ép mọi file đạt 15 dòng.

DOAN15 được đối chiếu canonical toàn bộ bản ghi ngoài SEG/SEG_ALIAS, XYZ RTK, thực thể mọi block, vị trí/chữ/góc/tỷ lệ/XData và hình học đường. Đường không hỗ trợ lấy mẫu báo trạng thái lỗi API trong snapshot để đối chiếu, không sửa đường. Lưu/mở DWG và đóng gói/mở lại .bht khớp snapshot. Nạp gói đã gom vào DWG trống giữ 179 OBJ/801 RTK/15 SEG/15 journal, nạp lại không nhân bản. Nạp gói 30 dòng cũ vào đích đã gom bị chặn và giữ nguyên canonical.

Rollback: lỗi sau khi cleanup trong transaction ngoài phục hồi đủ SEG/journal; lỗi chèn giữa ghi journal và xóa một SEG cũng phục hồi đủ. Một Undo sau lệnh gom khôi phục snapshot trước thao tác. Không dùng kiểm tra này để khẳng định khôi phục được khi mất điện hoặc crash native.

## Lần kiểm tra chưa đạt và cách xử lý

Probe ban đầu thiếu đúng delegate AutoCAD nên không compile; đã sửa loại delegate. Lấy mẫu Curve gặp eInvalidInput trên một đường không hỗ trợ tham số; bổ sung ghi trạng thái lỗi và tiếp tục đối chiếu các đặc tính hỗ trợ, không bỏ đối tượng khỏi snapshot. Kiểm tra ban đầu giả định mọi gói đều gom được; DOAN11 có doan_ung_vien tham chiếu cả hai mã nên bị chặn đúng. Giữ chính sách bảo vệ, bổ sung nhánh kiểm thử chặn và kiểm tra canonical.

Thử gây lỗi qua ObjectAppended không truyền exception về caller như dự kiến; không dùng kết quả đó làm bằng chứng rollback. Thay bằng lỗi trong native transaction sau ghi/xóa và transaction ngoài có cleanup, cả hai đạt. Thử mở lệnh bảng trong Core Console không thực hiện vì IsCoreConsole chủ động bỏ qua UI; giữ điều kiện sản phẩm và tách test backend/Undo khỏi nghiệm thu GUI. Các log thất bại giữ trong audit-1.1.3/evidence.

## Giới hạn và rủi ro còn lại

Ảnh là WinForms render. Chưa nghiệm thu chuột, DPI Windows hoặc cửa sổ bảng lồng nhau trong phiên AutoCAD người dùng; kết quả cài/GUI nếu thực hiện tiếp được ghi bổ sung ngoài gói trong audit, không sửa cùng phiên bản. Bộ kiểm tra rộng signs còn 11 lỗi từ bản trước, chưa sửa trong đợt này. Không kết luận đã sửa nguyên nhân đứng khi chọn tệp hoặc lỗi giấy phép/proxy.

Chưa chuyển đổi toàn bộ mã gói: DOAN08/GOI03 phải giữ để OBJ khóa khớp SEG, các đoạn khác có GOI_03. Journal mới phải được giữ khi tiếp tục lưu/xuất/nhập; không dùng bản cũ để chỉnh và đồng bộ tiếp dữ liệu mới. Bản có lịch sử HISTORY/CHANGES/LOCAL_HISTORY/RELEASES bị chặn gom để tránh mất tham chiếu ẩn; các cặp hai mã đang dùng cần migration riêng có duyệt.

Hướng tiếp theo: quy trình chuyển đổi mã đoạn/gói có xem trước mọi OBJ/SEG/tham chiếu/lịch sử, quyền ghi giới hạn và rollback; không đổi tên hàng loạt các hồ sơ khóa. Tiếp tục đối chiếu xung đột OBJ-000001 giữa D14/D15 độc lập với lỗi bảng đoạn. Không gộp các Milestone chưa nghiệm thu vào tuyên bố ổn định.

Hướng dẫn: HUONG_DAN_HIEU_CHINH_1.1.3.md. Bằng chứng: D:/lisp/BHT/audit-1.1.3/evidence. Bản gốc trong New không dùng làm đích sửa.
