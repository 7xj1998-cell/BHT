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
