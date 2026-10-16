# BHT 1.1.5 — bộ tổng hợp bàn giao cho AI

Ngày16/10/2026. Mã nguồn releases/source-1.1.5. Tài liệu bổ sung không thay đổi bộ cài đã kiểm thử.

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


---

# 2. Diễn tiến dự án

Lịch sử dưới đây tổng hợp từ tài liệu còn lưu, không khẳng định mọi phiên bản đều có tag hoặc bộ cài còn tồn tại. Thứ tự trong CHANGELOG có vài mục trùng/đặt cuối tệp, ví dụ 0.6.9 và 1.0.33; phải đối chiếu số phiên bản và nội dung.

| Giai đoạn | Mục tiêu và kết quả chính | Nguồn kiểm tra |
| --- | --- | --- |
| 0.3.x và trước | Nền AutoLISP, dữ liệu trong DWG, hợp đồng BHT_V02. Chưa đủ tài liệu trong bộ hiện tại để xác định chính xác ngày bắt đầu hoặc mọi thay đổi trước 0.4.0. | DATA_CONTRACT, cuối CHANGELOG |
| 0.4.0–0.4.3, 27–28/09/2026 | Thêm Core/Bridge/Palette net48; BHT/BTH mở một Palette; Application Bundle; chọn RTK hai chiều; nhãn và quản lý ảnh; đồng bộ phiên bản DLL/Lisp. | CHANGELOG 0.4.x |
| 0.4.4–0.4.6, 28–29/09 | Thư viện biển, tuyến TDT đọc riêng rồi tạo Polyline tham chiếu, Excel, Unicode/TCVN3, sửa chữ/tab/thông báo lỗi. Giữ dữ liệu đo. | CHANGELOG và DATA_CONTRACT |
| 0.5.1–0.5.5, 01/10 | Quy tắc phiên bản/đóng gói, phông VNRomancUpdate, biển tự do, bố trí/hatch, bỏ DCL dự phòng và tiếp tục Palette. | CHANGELOG 0.5.x |
| 0.6.0–0.6.13, 01–05/10 | Tách 12 module Lisp; chuyển một phần xử lý sang .NET, fallback; kiểm tra bộ cài; chiều tuyến/mốc TDT, thư viện và nội dung biển. | MIGRATION_0.6.0, CHANGELOG |
| 0.6.14–0.6.29, 07–08/10 | Sửa nhập số, mất bản nháp/scroll, ghi hồ sơ tách chèn ký hiệu; đa mặt/trụ, tỷ lệ, đường dẫn, EXE cài đặt, chọn biển modeless, biển trạm thu phí. | CHANGELOG và docs/old |
| 0.6.30–0.6.54, 08–11/10 | Chất lượng vector/hatch/biển chữ, hiển thị và tỷ lệ nhiều nhóm; tối ưu luồng, nhận diện runtime; sửa lỗi khởi động stringp T do OR AutoLISP trả boolean. | CHANGELOG, báo cáo cũ |
| 0.6.57–0.6.68, 11/10 | Phát triển từ nền đầy đủ 0.6.54; bảng chữ tùy chỉnh, đèn tín hiệu/cần vươn, bố trí và đối chiếu thực tế. Không suy ra 0.6.55 là nền runtime hiện tại. | CHANGELOG |
| 1.0.0–1.0.13, 11–13/10 | Hoàn thiện vận hành hồ sơ, ký hiệu và tuyến; cải tiến từ các ca thực tế. Xem chỉ mục từng phiên bản thay vì coi cả dải là một lần nghiệm thu. | RELEASE_NOTES/BAO_CAO trong docs/old |
| 1.0.14–1.0.29, 14/10 | Sửa cọc, DIM/offset đúng trụ, báo cáo mỗi mặt biển/biển phụ, hướng nhãn/đường dẫn, quản lý xử lý theo đoạn và kế thừa; 1.0.29 sửa biến angle che hàm angle gây bad function. | CHANGELOG, AI_HANDOFF lịch sử |
| 1.0.30 | Nền đang phát triển tại lúc bắt đầu lộ trình integrity. Public main sau đó giữ nguồn nền 1.0.30; không đồng nghĩa runtime hiện hành. | Git nền 3084a4c và tài liệu baseline |
| 1.0.31 | Milestone 1: tính/xác nhận bổ sung, bỏ chặn toàn đoạn nhưng giữ guard từng đối tượng; kiểm tra tim/mốc đã xác nhận, no-op và rollback, chính sách gói thống nhất. | MILESTONE_1_ARCHITECTURE, báo cáo M1 |
| 1.0.32 | Milestone 2: hiệu chỉnh riêng hồ sơ khóa, preview/confirm, history, legacy baseline, journal/Undo, tính lại phụ thuộc theo loại thay đổi. | MILESTONE_2_ARCHITECTURE |
| 1.0.33 | Sửa chữ đè/cắt trên bảng hiệu chỉnh; AutoSize/wrap/padding, 101 kiểm tra layout đạt sau baseline có 30 lỗi. | CHANGELOG mục ở cuối, báo cáo cũ |
| 1.0.34 | Milestone 3: ancestry merge, fork conflict, identity validation, tombstone; không dùng timestamp. | RevisionMerge, RevisionTransfer, báo cáo 1.0.34 |
| 1.0.35 | Milestone 4: lưu thay đổi trình bày, Display Revision, GLOBAL/LOCAL; kiểm tra bố trí tay và không ghi đè LOCAL. | DrawingPresentation, báo cáo 1.0.35 |
| 1.0.36 | Milestone 5 nền tảng: drawing identity/state, liên kết chỉ đọc, snapshot preview, phát hành R01/R02, guard bản phát hành. | DrawingProject, báo cáo 1.0.36 |
| 1.1.0, 15/10 | Báo cáo modeless/đủ dòng; kiểm tra điểm đầu tuyến, hướng nhãn theo chiều tăng Km; giảm thao tác lặp trong bố trí/cập nhật; giữ lỗi baseline ký hiệu được ghi nhận. Công bố runtime thử nghiệm lên GitHub, chưa công bố nguồn của build này. | CHANGELOG, release v1.1.0 |
| 1.1.1, 16/10 | Thiết kế lại bảng đoạn/xung đột; chẩn đoán D14/D15. Phát hiện hai đối tượng khác dữ liệu trùng mã, không bỏ guard để ép nhập. | báo cáo 1.1.1 |
| 1.1.2 | Tách chọn tệp kế thừa khỏi kiểm tra/nhập, nhật ký, bỏ Lisp DRAIN sau lệnh .NET; chưa chứng minh sửa hết lỗi đứng GUI. | RollingImportFileForm, RollingImportLog |
| 1.1.3 | Kiểm tra/gom đoạn trùng có duyệt, SEG_ALIAS chỉ thêm. Một bản sao 30 SEG về 15; giữ dòng đang được OBJ tham chiếu và chặn trường hợp cả hai mã đang dùng. | SegmentDuplicates/SegmentDuplicatePolicy |
| 1.1.4 | Chặn phép chiếu bị kẹp đầu/cuối rồi tạo Km giả; kiểm tra khoảng/phía kể cả gán tay. Giải thích điều kiện còn thiếu trước xác nhận hiệu chỉnh. | route-projection-in-domain, progress-plan/finish |
| 1.1.5 | Layer tim theo mã tuyến, migration có duyệt và rollback, giữ nguồn TDT/hình học; tương thích Line cũ, chặn xung đột style/viewport. | RouteLayers và báo cáo 1.1.5 |

## Bài học kỹ thuật phải giữ

- Không dùng Wblock để tạo snapshot dự án đầy đủ: đã phát hiện mất NOD. DrawingProject dùng SaveAs(false) và mở lại snapshot.
- Không bọc transaction .NET quanh acedInvoke/Lisp tạo block rồi cho rằng rollback bao trùm mọi thứ: đã có eKeyNotFound và Access Violation. Hiệu chỉnh dùng journal + nhóm native Undo + đối chiếu trạng thái.
- MangleName bảo vệ block/style cùng tên nhưng khác nội dung. Sửa layer phải có phạm vi; đổi toàn bộ sang Ignore có thể làm sai hình biển.
- Trong AutoLISP, OR trả boolean; tên biến trùng hàm angle đã gây lỗi thật; tránh sửa theo suy luận C#/JavaScript.
- Bảng đẹp trong probe không chứng minh thao tác chuột, giấy phép, proxy/font và DPI máy người dùng đều đạt.
- Tên tim trong dòng OUT_OF_RANGE là dấu vết lần thử, không đủ để kết luận đối tượng đã được gán đoạn hoặc đã khóa.
- Hai đối tượng có cùng object_id ở hai nguồn không mặc nhiên là cùng đối tượng. Không sửa trùng mã mà bỏ qua XData/điểm/ảnh/block.

Lộ trình mục tiêu vẫn là “một dự án logic — đối tượng có phiên bản — nhiều bình đồ”. Hiện kho có thẩm quyền vẫn nằm trong DWG; chưa chuyển sang SQLite. Mỗi milestone có phần triển khai và giới hạn riêng, không diễn giải bảng lịch sử thành chứng nhận hoàn tất toàn bộ.


---

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


---

# 4. Kiểm thử, lỗi và giới hạn

## Bằng chứng của đợt 1.1.5

| Kiểm tra | Kết quả ghi nhận | Cách đọc |
| --- | --- | --- |
| CoreTests | 443 PASS, 0 FAIL | Đã chạy lại trên bộ nguồn công bố; log bổ sung public-build sau bước chuẩn bị |
| Layer native/legacy/real-data | 4 Lisp + 18 .NET + 1 Undo PASS | Runtime FAS/DLL bảo vệ cuối; có compare canonical, source hash, save/reopen, import/no-op/conflict |
| Progress125 | 13 Lisp + 28 .NET + 7 stage, nhãn kế thừa PASS | Runtime cuối; gồm route collision, giữ khóa, Line cũ, layer theo mã remap |
| Correction132 | 50 PASS | Runtime cuối, hiệu chỉnh/history/rollback |
| Incremental131 | 31 Lisp + 29 .NET + reopen PASS | Cùng đợt 1.1.5, trước thay đổi cuối nhận diện Line/thứ tự chọn lại tim |
| Scope114 | 28 + reopen PASS | Kiểm tra giới hạn/phạm vi, trước các thay đổi cuối nói trên |
| Palette layout | 3.142 PASS | Probe bố cục/editor workflow; không phải thao tác chuột thực tế |
| AutoCAD GUI thực tế | Chưa nghiệm thu đầy đủ | Công cụ ảnh từng timeout; giấy phép/proxy/font từng chặn phiên chạy |

`evidence/` chứa các log chọn lọc từ bản gốc và build kiểm tra nguồn công bố. Đường dẫn máy phát triển trong log là dấu vết lịch sử; bản DWG/.bht/ảnh thực địa không được đưa lên Git. Không thể tái chạy ca real-data chỉ bằng clone công khai nếu thiếu fixture riêng.

Các lỗi trung gian 1.1.5 đã sửa: gán thuộc tính layer trước khi có database (eNoDatabase), gán plot style trong CTB (ePlotStyleInColorDependentMode), từ chối Line native trong hồ sơ cũ, vô hiệu hóa kết quả trước khi thay tim thành công. Log lỗi cũ vẫn giữ ở máy phát triển; không coi chúng là lỗi đang tái hiện sau bản sửa cuối.

## Mức bao phủ lộ trình A–O

| Nhóm | Bằng chứng có | Phần chưa nên khẳng định |
| --- | --- | --- |
| A bổ sung 25+1 | Incremental131, M1, Progress125 | Mọi cấu hình tim/mốc ngoài thực tế |
| B sửa mã, C đổi điểm, K đổi đoạn | Correction132, snapshot/history; có tính riêng và duyệt chuyển đoạn | Biển nhiều mặt/XML tùy biến, mốc Km/H ảnh hưởng nhiều OBJ |
| D/E/F trình bày GLOBAL/LOCAL | Presentation135: 21 ca ở build trước, 1.1.0 có hồi quy ghi nhận | Không chạy lại trọn bộ M4 trên runtime 1.1.5; LOCAL theo layout chưa có |
| G/H/L/N revision cũ/fork/tombstone/lặp | Revision134: 17 ca ở build trước; Core revision vẫn được chạy trong443 | Không có UI giải quyết mọi fork hoặc gắn identity legacy |
| I trùng mã | Import conflict và kiểm tra project/GUID | Không tự hợp nhất hai đối tượng khác nhau trùng mã |
| J đổi mã gói | Alias policy, Segment113 và gom SEG có bằng chứng | Không bao phủ chuẩn hóa mọi mã gói có lịch sử revision |
| M rollback | Transaction, native U/REDO, fault injection trên các luồng test | Không chứng minh chống mất điện/crash native giữa DWG và tệp |
| O legacy | Đọc legacy, baseline có duyệt, gói .bht không đổi | Không tự xem tất cả hồ sơ legacy là verified hoặc cùng dự án |

## Các vấn đề đang mở

1. **11 lỗi kiểm thử ký hiệu legacy**: ghi nhận trong run_signs ở các đợt 1.0.36/1.1.0. Chưa được sửa trong 1.1.5. Khi xử lý, tái chạy baseline và bản sửa trên cùng fixture, giữ tên từng ca thất bại và snapshot; không giảm kỳ vọng chỉ để xanh.
2. **Nạp kế thừa bị đứng theo phản ánh người dùng**: 1.1.2 tách chọn tệp và thêm nhật ký. Chưa có bằng chứng mọi lỗi đứng GUI đã được xử lý. Phân biệt hộp proxy/font/license checkout đang chặn với deadlock/clone chậm của BHT. Không tắt hoặc giết CAD chứa công việc chưa lưu.
3. **D14/D15 trùng OBJ-000001**: đã có bằng chứng hai loại hồ sơ/điểm khác nhau; cần quy trình xử lý danh tính, không bỏ xung đột để ép nhập. Do dữ liệu riêng không có trong repo, phải xin đúng bản sao được phép dùng khi cần tái hiện.
4. **Ảnh Đoạn 11 có10 khóa, file đã lưu có8**: chưa xác minh được hai hồ sơ tăng từ bản DWG mới. Không suy từ ảnh rằng8 hồ sơ trong gói đã sai; không tự mở khóa/đổi đoạn.
5. **Hiệu chỉnh nâng cao**: thiếu luồng đủ an toàn cho biển nhiều mặt/nội dung tùy biến và mốc làm đổi hàng loạt kết quả. Cần thiết kế plan dependency và duyệt rõ ràng.
6. **GLOBAL/LOCAL**: LOCAL theo DWG; nhận survey/assignment vào LOCAL bị chặn trong trường hợp chưa an toàn; chưa có bố trí riêng theo layout hoặc nguồn LOCAL xuất thành GLOBAL.
7. **Bình đồ/phát hành**: cần nghiệm thu đủ15 đoạn, crash recovery giữa tệp và DWG, đối chiếu tệp đã tồn tại; không tự đồng bộ các tệp đóng hoặc bản bàn giao.
8. **Layer**: chỉ tim tham chiếu BHT. Layer cũ không tự xóa; viewport override/freeze bị chặn; không áp quy tắc này cho mọi lớp RTK/biển/nhãn.
9. **Tốc độ**: sửa1.1.0 giảm thao tác lặp, nhưng chưa có benchmark bảo đảm mọi bản vẽ lớn. Đo thời gian clone, scan, rebuild, regen và refresh; chia module không tự làm code nhanh hơn.

## Thứ tự tiếp tục đề nghị

Trước hết xác minh bản cài/fixture và kiểm tra luồng người dùng báo lỗi trong bản sao; tiếp theo xử lý11 lỗi signs với baseline rõ; sau đó nghiệm thu A–O và15 đoạn trên GUI. Chỉ mở rộng sửa biển phức tạp/identity legacy/LOCAL khi các guard và rollback đã được test. Nếu đổi mã, tăng phiên bản sau1.1.5 và viết hướng dẫn mới.


---

# 5. Build và tiếp tục phát triển

## Chuẩn bị

Clone nhánh releases/source-1.1.5. Đọc AGENTS.md, VERSION, START_HERE_AI và mục kiểm thử. Kiểm tra Git root/status trước sửa. Không cần quay lại bản1.0.30 trên main để làm nền cho bản1.1.5.

Windows/.NET Framework4.8, compiler C#5 hoặc .NET SDK có reference assemblies; AutoCAD2024 cần để build Bridge/Palette và chạy Core Console. Không chép acdbmgd/accoremgd/acmgd.dll vào Git. Các thư viện biển và phông không thuộc MIT; mã nguồn có sẵn tài nguyên ảnh/vector cần compile và mẫu đèn, còn đóng gói đầy đủ cần bản tài nguyên được phép phân phối tại máy. Đọc COPYING_SCOPE.md.

## Lệnh có sẵn

Chạy tại Git root. Sửa đường dẫn AutoCAD cho máy thực tế.

```powershell
# Core không cần AutoCAD; CI chỉ xác nhận mức này.
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -CoreOnly -UseCsc -Test -OutDir build/core-check/bin

# Build plugin bằng tham chiếu AutoCAD tại máy.
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -AcadDir 'D:\AutoCAD 2024' -UseCsc -Test -OutDir build/source-check/bin

# Kiểm tra đồng bộ version.
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/check-version.ps1
```

Không có DLL AutoCAD thì build script báo SKIP Bridge/Palette. Không diễn giải BUILD XONG của CoreOnly thành build plugin thành công. Dùng output mới để tránh DLL cũ gây nhầm. Xem BHT.sln, Directory.Build.props và BHT.CoreTests.csproj nếu dùng SDK.

```powershell
# Các runner AutoCAD thực tế; đọc tham số trước khi chạy.
Get-Help tests/IntegrationTests/run_progress125.ps1 -Full
Get-Help tests/IntegrationTests/run_incremental131.ps1 -Full
Get-Help tests/IntegrationTests/run_correction132.ps1 -Full
Get-Help tests/IntegrationTests/run_scope114.ps1 -Full
Get-Help tests/IntegrationTests/run_layers115.ps1 -Full
```

Probe mới cần BinDir đúng bản, RuntimePath đúng LSP/FAS, output riêng. Layers115 còn cần InputPackage riêng được phép thử. Runner dùng bản sao, profile cô lập và timeout; không dùng bản khảo sát gốc làm nơi ghi. `run_all` hoặc `run_signs` có thể cần ADS/TDT/fixture và có lỗi baseline đã biết; phải báo FAIL/BLOCKED cụ thể.

## Chuỗi đóng gói

1. Bump bằng scripts/set-version.ps1 khi thay chức năng; viết CHANGELOG/hướng dẫn và cập nhật tài liệu hiện hành.
2. Build private + CoreTests; compile-lisp.ps1 tạo FAS; protect-dotnet.ps1 cần Obfuscar2.2.50 nếu muốn runtime protected.
3. Chạy Core Console bằng chính DLL/FAS chuẩn bị đóng gói; đối chiếu record/RTK/CAD canonical, U/REDO, save/reopen, import/export lặp và lỗi giữa giao dịch.
4. scripts/package.ps1 kiểm version/hash và RELEASE_STATE, tạo ZIP+EXE. Không sửa nội dung đã phát hành dưới cùng số. `-IncludeSource` là tùy chọn; công bố nguồn lần này dùng snapshot riêng không tái đóng bộ cài.
5. Cài khi CAD đã đóng và so hash3DLL+FAS. Test GUI tách riêng khỏi Core Console.
6. Gom hướng dẫn/bộ cài cũ vào old, có manifest đối chiếu; giữ mã test và bằng chứng cần thiết. Không xóa hồ sơ hay test chỉ để giảm số lỗi.

Nguồn công bố có thêm giấy phép và handoff sau khi bộ cài1.1.5 được đóng. Vì fingerprint đóng gói bao gồm docs, chạy package lại1.1.5 từ kho này có thể bị RELEASE_STATE chặn. Đây là bảo vệ đúng; không xóa/bỏ kiểm tra fingerprint để phát hành lại. Build để kiểm tra nguồn vẫn được; bản thay đổi phải lên phiên bản mới.

## Quy trình điều tra lỗi

Ghi nhận VERSION runtime và DLL/FAS thực tế; có thể máy còn nạp DLL cũ trong phiên CAD. Chụp/đọc hộp đang chặn, kiểm log trước/sau chọn tệp. Tạo fixture nhỏ nhất và snapshot trước. Tái hiện baseline rồi sửa phạm vi nhỏ, chạy lại cùng test. Khi import phải kiểm tra cả preflight và writer. Khi sửa survey phải kiểm cả Lisp và C# guard. Khi sửa presentation phải đối chiếu RTK/lý trình không đổi.

Đừng giả định .bht mới nhất theo tên hoặc ngày là đúng. Đối chiếu hash, số record, danh tính và ancestry. Không dùng dữ liệu đã sửa thử làm fixture nguồn cho lần sau. Ghi rõ test nào chạy trên runtime hiện tại, test nào chỉ có bằng chứng lịch sử.


---

# 6. Prompt giao cho AI tiếp nhận

Bạn tiếp nhận BHT1.1.5, phần mềm khảo sát tuyến trên AutoCAD2024, C#/.NET Framework4.8 và AutoLISP. Hãy đọc START_HERE_AI.md, handoff/1.1.5/BHT_AI_HANDOFF_1.1.5.md, AGENTS.md và mã thực tế trước khi sửa.

Mục tiêu: tiếp tục theo “một dự án logic — đối tượng có phiên bản — nhiều bình đồ”, ưu tiên tính toàn vẹn dữ liệu. Không viết lại BHT, không bỏ guard, không dùng thời gian sửa làm thẩm quyền. Giữ RTK, hình học tim, mốc, hồ sơ khóa ngoài phạm vi và bản phát hành. Mọi test thực địa dùng bản sao.

Bắt đầu bằng xác minh Git root/branch/status, VERSION và build đang cài; lập bản đồ chức năng liên quan yêu cầu mới. Đọc báo cáo hiện hành và lỗi chưa xử lý. Tạo test tái hiện, sửa mã, build và kiểm thử phù hợp. So sánh canonical trước/sau thay vì chỉ thông báo thành công. Báo bằng tiếng Việt: phiên bản, nguyên nhân, file/hàm, trước/sau, test PASS/FAIL/BLOCKED, giới hạn và hướng dẫn phục hồi.

Các phần M1–M5 có nền tảng triển khai, chưa nghiệm thu trọn lộ trình. Có11 lỗi ký hiệu legacy chưa sửa và GUI15đoạn chưa nghiệm thu. Không tuyên bố tất cả đã hoàn tất hoặc dùng CoreTests thay kiểm thử AutoCAD GUI. Không tự mở khóa cả đoạn, tự hợp nhất identity legacy hoặc tự ghi đè bình đồ bàn giao.

Bản công khai nguồn là releases/source-1.1.5, main hiện là nền lịch sử. Chỉ dẫn “không push” trong báo cáo cũ mô tả phiên trước; công bố1.1.5 đã được người dùng yêu cầu. Sau này không tự push/phát hành thay đổi mới nếu không có yêu cầu tương ứng. Khi đổi tính năng, dùng set-version và hướng dẫn mới. Không tái phát hành mã khác dưới1.1.5.

Trước khi thực hiện công việc mới, nêu ngắn gọn phần đã xác minh và phần thiếu bằng chứng. Nếu lỗi liên quan dữ liệu riêng không có trong repo, dùng fixture giả lập cho phần độc lập; chỉ yêu cầu bản sao cần thiết, không suy đoán dữ liệu nguồn. Tiếp tục thực thi phần đã được yêu cầu, không chỉ viết đề xuất.


---

