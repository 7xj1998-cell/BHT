# Milestone 2 - Giao dịch hiệu chỉnh và lịch sử đối tượng

1.0.31 -> 1.0.32, ngày 15/10/2026. Source/Git root: D:/lisp/BHT/sharing-0654/source, nhánh development/incremental-confirmation. Baseline status/patch/SHA256 tại D:/lisp/BHT/audit-1.0.32. Không commit/push tự động.

## Luồng

ObjectCorrectionForm -> AcadDispatcher.RunInCommandContext -> ObjectCorrection.Preview/Confirm -> bht:correction-evaluate -> BhtStore.CommitCorrection. Xem trước chỉ đọc; plan bất biến giữ Database, snapshot, patch whitelist, loại thay đổi và dấu kiểm tra dữ liệu phụ thuộc. Caller chỉ nhận bản sao. Xác nhận kiểm tra lại dữ liệu/tính toán và duyệt chuyển đoạn.

Attributes, PointLink, Station, Assignment có whitelist riêng. Mã đổi đồng bộ mặt đơn; biển phức tạp bị từ chối. Số trụ ảnh hưởng điểm đại diện nên tính lại. Đổi điểm/đoạn tính riêng một OBJ qua thuật toán station hiện có. Không tắt progress-guard, không ghi cả đoạn. Cọc Km/H có thể ảnh hưởng mốc nên chưa hỗ trợ đổi điểm/lý trình.

## Nguyên tử và phục hồi

Transaction .NET bao quanh acedInvoke tạo block đã gây eKeyNotFound và một lần Access Violation trong test cô lập; phương án đó bị loại. Bản cuối dùng nhóm native Undo riêng: journal IN_FLIGHT đã commit -> transaction dữ liệu/lịch sử -> Lisp cập nhật ký hiệu -> xóa journal -> kiểm tra lịch sử -> kết thúc nhóm. Lỗi thì U đúng nhóm và đối chiếu bản ghi/điểm/entget toàn bộ thực thể. Không tuyên bố phục hồi khi snapshot khác.

Journal bảo đảm nhóm không rỗng nếu transaction dữ liệu lỗi, đồng thời phát hiện thao tác dang dở. Chưa bảo đảm tự khôi phục DWG được tác nhân khác lưu đúng lúc tiến trình bị chấm dứt; trường hợp đó bị chặn để dùng bản sao trước thao tác. Cần Undo All. Nhóm Undo trước lệnh kết thúc trước nhóm riêng. Không sửa RTK/tim/mốc gốc.

## Dữ liệu

Giữ NOD BHT_V02, thêm HISTORY/CHANGES/CORRECTION. META/CONFIG có project_id khi baseline đầu tiên được duyệt. Chỉ OBJ được sửa nhận object_guid, revision_id, parent_revision, counters, reason/time/author/status. object_id không đổi, không gắn ID hàng loạt.

HISTORY khóa UUID, snapshot binary v1 (số cặp, chuỗi có độ dài, UTF-8/Base64), SHA256. Giữ thứ tự/khóa lặp/newline. Baseline LEGACY_UNVERIFIED giữ nguyên hồ sơ cũ, counters bắt đầu 0. Mỗi correction có UUID và cha rõ; survey/assignment tăng theo miền dữ liệu, display_revision giữ 0 ở giai đoạn này. Thời gian UTC chỉ để tra cứu.

Kiểm tra metadata khớp snapshot, project khớp META, GUID duy nhất, cha đầy đủ/không vòng lặp và counters liên tục. HISTORY/CHANGES chỉ thêm; generic .NET/Lisp writer chặn sửa khảo sát/khóa/revision ngoài luồng correction. Chỉ writer nội bộ nhận plan đã đối chiếu ghi đúng OBJ.

kh_*, custom_block, sign_layout/gap/clearance/fill, sign_heading_* và marker_display_* tách khỏi dữ liệu có thẩm quyền khảo sát; sua_luc không tham gia đối chiếu phần đó. Các đường trình bày cũ chưa có Display Revision (M4). Snapshot lịch sử vẫn lưu nguyên toàn bộ bản ghi tại lúc sửa.

CHANGES lưu object_guid, revision, đoạn cũ/mới, UPDATE_AVAILABLE. Chưa phải danh sách DWG hoặc bộ máy đồng bộ M5.

## Tương thích

Không thay ZIP .bht. Hồ sơ cũ vẫn đọc; baseline không tự coi verified. Full export/import giữ HISTORY/META; xuất đoạn lọc lịch sử theo OBJ được chọn. Lịch sử không remap handle, hình học hiện hành vẫn clone như cũ. Chặn rolling/survey-only có lịch sử ở cả preflight và thực thi đến M3. Trở lại 1.0.31 phải dùng bản sao dữ liệu trước hiệu chỉnh.

## Còn thiếu

Sửa mã biển nhiều mặt/nội dung XML qua trình chọn biển, sửa mốc tác động nhiều hồ sơ, đối chiếu hồ sơ thiếu bằng chứng, danh sách thay đổi trên bảng tiến độ, ancestry merge, tombstone và GLOBAL/LOCAL chưa hoàn tất. 1.0.32 là mốc kiểm tra cục bộ cho luồng đã triển khai, chưa nghiệm thu toàn bộ Milestone 2 hoặc lộ trình.
