# Báo cáo kiểm tra BHT 1.0.34

Ngày 15/10/2026. Trước 1.0.33, sau 1.0.34. Nhánh development/incremental-confirmation; làm tại Git root sharing-0654/source, giữ dirty worktree. Không commit/push.

Vấn đề gốc: khóa đơn thuần không chứng minh thứ tự phiên bản; chốt cũ phải từ chối mọi import có lịch sử. RevisionMerge.Decide/Chain dùng chung cho preflight và writer riêng, kiểm tra snapshot SHA256, cha, counters, ownership và fork. Không tắt guard thường. RevisionTransfer.PrepareVersionedRolling kiểm tra trước khi ghi, lọc đúng đối tượng và hợp nhất lịch sử trong transaction. ObjectCorrection thêm tombstone, bằng chứng before_pose/CAD_STATE để tránh ghi đè bố trí đã sửa độc lập. Lisp symbol-sync-ex bỏ qua hồ sơ ngừng sử dụng, SignReportRecords loại khỏi báo cáo; Palette hiển thị trạng thái.

Kiểm tra tự động: 428 CoreTests, 50 hiệu chỉnh hồi quy, 101 đo bảng WinForms đạt. Core Console trên FAS/DLL protected đạt 17 bài merge/tombstone và 11 bài D10: 178 hồ sơ khác, 801 điểm, tim/đoạn và thực thể ngoài phạm vi giữ nguyên; lỗi sau cập nhật phục hồi, lưu/mở lại DWG và .bht giữ lịch sử. Bằng chứng ở D:/lisp/BHT/audit-1.0.34.

Các lần thử đầu phát hiện lớp CAD được đổi tên khi clone và bài thử gọi API thiếu tham số; đã sửa so sánh tên lớp và lời gọi test. Mô phỏng exception trong ObjectErased không đáng tin vì sự kiện bị trì hoãn; bài cuối gây lỗi đơn vị sau bước xóa hình trong transaction, xác minh canonical trước/sau bằng nhau. Bài debug dùng tên private bị obfuscator đổi tên; đã bỏ reflection khỏi kiểm thử runtime protected. Giữ các log thất bại trung gian.

Giới hạn: chưa kiểm thử GUI AutoCAD thực tế ở mọi DPI; font test không phải Windows DPI. Milestone 3 đạt phần ancestry/rollback/tombstone đã nêu, chưa đối chiếu legacy hoặc chuẩn hóa gói trên revision bằng UI. Bố trí khác không đủ bằng chứng sẽ báo xung đột; không tự chọn bản. Cọc Km/H và biển nhiều mặt/nội dung tùy chỉnh vẫn cần quy trình riêng. Chưa nghiệm thu toàn bộ Milestone 2-5.

Tiếp theo: Display Revision, lưu GLOBAL/LOCAL rõ ràng; rồi danh tính/trạng thái bình đồ và phát hành theo phiên bản. Không sử dụng timestamp làm thẩm quyền và không tự cập nhật bản đã bàn giao.
