# 3. Kiến trúc và bản đồ xử lý

## Khởi động từ bộ cài đến Palette

1. INSTALL_BHT xác minh manifest/hash/version, chặn khi AutoCAD còn chạy, cài bundle theo người dùng và giữ cơ chế phục hồi bản cũ.
2. PackageContents.xml khai báo Bridge và Lisp nạp theo cấu hình bundle. Bản nguồn khai báo LSP; lúc đóng gói protected script đổi entry sang FAS. DLL Autodesk chỉ tham chiếu từ AutoCAD cài tại máy.
3. BHT-1.1.5.lsp tìm module root, nạp lần lượt 12 module. LispRuntimeLoader/PaletteHost kiểm tra runtime đúng phiên bản, không tự chấp nhận DLL/Lisp lệch phiên bản.
4. BHT hoặc BTH mở PaletteSet qua PaletteHost.Show; các tab quản lý tổng quan, RTK, ảnh, hồ sơ, tuyến/báo cáo. AcadDispatcher đưa thao tác cần CAD về đúng document/command context, tránh reentry và kết quả cũ sau khi đổi DWG.
5. Phần chuẩn hóa layer tuyến mới không tự chạy khi mở. Mã lịch sử có SymbolUpgradeStartup để nâng cấp trình bày trong những điều kiện riêng: phải đọc guard trước khi thay đổi cơ chế khởi động, không kết luận toàn bộ startup chỉ đọc dựa trên hành vi layer.

## Các lớp

| Thành phần | Trách nhiệm | Điểm cần đọc |
| --- | --- | --- |
| BHT.Core | Record/codec, validation, Unicode, policy, revision, báo cáo và xử lý không phụ thuộc AutoCAD | Models, ProgressPolicy, ObjectRevision, RevisionMerge, SegmentDuplicatePolicy, SignReportRecords |
| BHT.Bridge | Transaction DWG, XRecord/XData, CAD geometry/block, import/export, hiệu chỉnh, đồng bộ | BhtStore, ProjectTransfer, ProgressTransfer, ImportComparison, ObjectCorrection, RevisionTransfer, DrawingPresentation, DrawingProject, RouteLayers |
| BHT.Palette | WinForms/PaletteSet, biểu mẫu xem trước/đối chiếu, nhập liệu và điều phối | PaletteHost, BhtPaletteControl, TabObjectsRoutes, ObjectCorrectionForm, ProgressForm, TabDrawingProject, RouteDataControl |
| AutoLISP | Thuật toán hiện có, lệnh CAD tương tác, chiếu tuyến/tính Km, cập nhật ký hiệu/nhãn, API | modules/01–12 |
| Script/test | Phiên bản, build, FAS, bảo vệ DLL, bộ cài, Core/AutoCAD probe | scripts/, tests/ |

12 module Lisp: 01-foundation, 02-points, 03-labels, 04-objects, 05-photos, 06-routes, 07-segments, 08-signs, 09-draworder, 10-reports, 11-diagnostics, 12-api. Không tách lại module chỉ vì thấy lag; đo phần duyệt thực thể, dựng lại block, refresh UI và callback trước.

## Kho dữ liệu và danh tính

DWG là nguồn dữ liệu hiện hành. NOD `BHT_V02` chứa META/CONFIG, DATASET, OBJ, PHOTO, ROUTE, SEG và các dictionary bổ sung của revision/drawing. BhtRecord giữ cả khóa lặp và thứ tự. XRecord mã nhóm 1 lưu chuỗi key=value; chuỗi dài dùng key+=. Không được parse bằng cách mất khóa lặp `pt`, `mat`, `moc` hoặc xuống dòng.

POINT/RTK, ký hiệu và liên kết CAD dùng XData/handle theo hợp đồng hiện có. Handle chỉ có ý nghĩa trong database tương ứng; import phải remap, không dùng handle làm danh tính liên bản vẽ.

File `.bht` là gói ZIP hiện có chứa snapshot DWG và metadata của định dạng. Không tự thêm database ngoài, không thay ZIP để triển khai lịch sử. Đọc ProjectPackage và ProjectTransfer trước mọi thay đổi định dạng.

`object_id` là mã người dùng thấy. Khi baseline được duyệt, metadata mới dùng project_id/object_guid/revision_id/parent_revision cùng các counter survey/display/assignment. HISTORY/CHANGES lưu thêm, không thay phiên bản cũ. Snapshot có kiểm tra hash; revision_time chỉ phục vụ truy vết. Baseline LEGACY_UNVERIFIED không chứng minh tính đúng khảo sát cũ.

Các kho liên quan khác gồm CORRECTION (journal), VIEW_LOCAL/LOCAL_HISTORY (trình bày riêng), SEG_ALIAS (gom mã đoạn có lưu dấu vết), dữ liệu bình đồ/phát hành trong DrawingProject. Tên/chính sách chính xác phải đọc writer tương ứng; không ghi generic vào các kho có lịch sử.

## Bản đồ đường xử lý

| Công việc | Đường mã chính | Điều kiện bảo vệ |
| --- | --- | --- |
| Nhập RTK | 02-points + BhtStore/BhtDataService | Giữ tọa độ đo, ID/tham chiếu; không lấy vị trí block thay điểm |
| Tạo/lưu OBJ | 04-objects, Models/ObjectLogic, TabObjectsRoutes | Validation, progress-guard, BhtStore.Write |
| Gán đoạn/gói | 07-segments, SegmentStore, BHTGANDOAN | `doan` là assignment, `goi` là quản lý gói; không bỏ cả hai khỏi bảo vệ |
| Chọn tim/mốc/tính Km | 06-routes, route-project, route-projection-in-domain | Native reference, chiều tuyến, mốc, offset, phạm vi đầu/cuối, bằng chứng hình học |
| Tính bổ sung | 07-segments: progress-plan/apply/write | Chỉ pending; kiểm tra context tim/mốc/khóa; gán tay vẫn kiểm tra phạm vi/phía |
| Xác nhận | progress-finish | Tái lập plan, chỉ xác nhận hồ sơ đủ điều kiện; tất cả khóa là no-op |
| Mở lại | progress-reopen | Là hành vi có chủ ý; không phải đường mặc định sửa một OBJ |
| Hiệu chỉnh một OBJ | ObjectCorrectionForm → ObjectCorrection.Preview/Confirm → bht:correction-evaluate → BhtStore.CommitCorrection | Plan bất biến, whitelist theo loại, so sánh trạng thái, duyệt baseline/chuyển đoạn, history + journal/Undo |
| Xuất .bht | ProjectTransfer.Export/ExportSelection | Xác minh điểm/tim xác nhận; snapshot; xuất đoạn giữ lịch sử liên quan |
| Nạp kế thừa | BHTNAPTICHLUY, ProgressTransfer.ImportRolling, CheckRollingConflicts/PrepareRolling, ProjectTransfer | Preflight cùng policy với writer; không ghi từng phần khi có conflict |
| Nạp có revision | RevisionMerge.Decide/Chain, RevisionTransfer | Same revision/no-op, ancestor giữ bản mới, descendant upgrade hợp lệ, fork/identity sai bị chặn |
| Bố trí/nhãn | 08-signs + SignAssembly/SignLayout và các helper | Phân biệt tọa độ đo/vị trí trình bày; giữ bố trí tay theo policy, không tạo block trùng |
| Lưu GLOBAL/LOCAL | ObjectCorrection và DrawingPresentation | Lưu sau thao tác CAD, không revision liên tục trong drag; LOCAL không bị global ghi đè |
| Bình đồ/phát hành | DrawingProject, TabDrawingProject | Fingerprint preview, nguồn liên quan chỉ đọc, tệp Rxx mới; RequireWritable cho bản phát hành |
| Layer theo tuyến | bht:route-write → BHTROUTEWRITE; ProjectTransfer → ApplyOne; Palette → Preview/Apply | Layer + ROUTE trong transaction; giữ hình học/XData; native curve cho phép; không đổi TDT nguồn |
| Lưu/mở/rollback | SaveAs snapshot, native Undo, transaction, journal và probes | Snapshot canonical và hash; không coi message thành công là bằng chứng |

## Các quyết định phải nhất quán

ProgressPolicy.Changed kiểm tra trường khảo sát/kết quả/assignment được bảo vệ. ResultKey và SameSurvey phục vụ so sánh khác nhau; không dùng chúng thay nhau. Trường trình bày và thời gian cập nhật không được tạo xung đột khảo sát giả. PackageAliasPolicy chỉ chấp nhận trường hợp đã chứng minh, không tự quy đổi mọi GOI01 thành GOI_01 hoặc xóa DOAN khỏi guard.

CheckRollingConflicts chạy trước PrepareRolling, nên sửa policy phải sửa cả báo cáo xung đột và thực thi. CompareRolling hiện dùng đường nhập thật trên snapshot để tránh hai quy tắc khác nhau.

BhtStore.Write và bht:progress-guard chặn writer thông thường. Đường hiệu chỉnh/versioned merge có quyền ghi riêng theo plan đã kiểm tra, không tắt guard toàn cục. Side database cần Undo recording thích hợp để transaction abort phục hồi thực thể clone.

Drawing states gồm UP_TO_DATE, UPDATE_AVAILABLE, CONFLICT, RELEASED, READ_ONLY_ARCHIVE trong thiết kế/triển khai. Đây là trạng thái BHT, không phải cách khóa mọi lệnh AutoCAD gốc. Bản vẽ khác đang mở mà chưa lưu không được coi là dữ liệu đã biết khi đọc tệp trên đĩa.

## Phạm vi rollback thực tế

Import dùng transaction và kiểm tra toàn bộ trước ghi. Hiệu chỉnh có đoạn cập nhật CAD qua Lisp nên dùng nhóm Undo riêng/journal, rồi đối chiếu snapshot. Phát hành DWG + tệp không có giao dịch phân tán chống mất điện. Không được hứa “atomic tuyệt đối” cho crash giữa hai kho hoặc cho kiểu thực thể chưa hỗ trợ.
