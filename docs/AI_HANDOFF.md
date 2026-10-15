## 1.0.29 - 2026-10-14

Sửa bad function: 1.51086 do biến angle che hàm AutoLISP trong bố trí hàng loạt. Nhãn báo hiệu theo đoạn dẫn dài: mã/biển phụ trên, lý trình dưới; hai trụ dùng đường dẫn phía trên. Thêm BHTMAUNHAN để áp dụng cho nhãn kéo tay mà giữ hình biển. Đối chiếu sign_content theo mô hình XML hợp lệ/NFC và hiển thị văn bản; HÒA và HOÀ vẫn khác.

08-signs: không dùng biến angle trong luồng gọi hàm angle; thay heading-angle. kh-caption-segments chọn đoạn dài nhất. kh-caption-leader-layout dùng đường dẫn trên với BIEN_BAO, khoảng cách 1.25h cho mã cuối và 0.25h tới đầu dòng Km. BHTMAUNHAN dùng kh-display-transaction và kh-layout-refresh; không dùng symbol-reset vì sẽ di chuyển hình biển. SurveyComparison chỉ chuẩn hóa sign_content schema v1 đã xác minh, giữ nguyên các trường khác và dữ liệu chưa hiểu. Audit-1.0.29 tái hiện old/new trên gói đoạn8, ảnh plot119/120. Chưa cài phiên CAD hiện tại.

## 1.0.28 - 2026-10-14

Kiểm tra toàn bộ khác biệt trước nạp kế thừa, bảng đối chiếu có tìm kiếm và chi tiết hai bản. Giữ tính nguyên tử khi từ chối nạp. Giữ vị trí cuộn bảng tuyến, chặn ẩn/hiện tạm nhóm cọc TDT và cuộn do trả tiêu điểm; giữ cuộn chủ động khi mở nhóm và dùng Tab.

ImportComparison.CheckRollingConflicts chạy trước mọi thay đổi đích; ImportConflictException mang danh sách chỉ đọc. Palette đăng ký sự kiện từ Bridge, ShowModalDialog; CoreConsole fallback đủ dòng lệnh. ResponsiveFlowPanel.RevealControl dành cho cuộn chủ động, ScrollToControl chặn cuộn ngầm. RefreshRouteData và RefreshCurrentPage giữ scroll, UpdateRouteTools bỏ qua refreshingRoutes. Tests Progress125 thêm 5 kiểm tra; Palette thêm15. Người dùng vừa gửi ba gói; dữ liệu khảo sát đã khác, mã đoạn khác, chưa có progress_locked=1. Xem audit-1.0.28/PHUONG_AN_DT830.md; chưa triển khai hợp nhất ba chiều hoặc chuẩn hóa mã đoạn.

## 1.0.27 - 2026-10-14

1.0.26 thiếu bht:api-sign-display trong *bht-api-functions*, nên Lisp trực tiếp đạt nhưng .NET Application.Invoke eInvalidInput. Đã đăng ký trong12-api. SignDisplay127Probe/run_sign_display127 gọi LispApi.Call thật, kiểm tra cả hai hướng, META, từ chối tham số và giữ OBJ; tái hiện gói cũ bằng -ExpectMissingRegistration. Không chỉ chạy Lisp trực tiếp cho API mới.

TabObjectsRoutes: khung _tdtRouteTools bỏ Padding, Margin ngang0/dưới0; cùng cạnh với nút mốc bên dưới trong ResponsiveFlowPanel. PaletteLayoutProbe thêm25 kiểm tra căn cạnh/kích thước, tổng3012. Core305, DISPLAY1279 source và9 DLL/FAS bảo vệ đạt. Đọc docs/RELEASE_NOTES_1.0.27.md và audit-1.0.27/BAO_CAO.md.
## 1.0.26 - 2026-10-14

Đọc RELEASE_NOTES_1.0.26, BAO_CAO_NGHIEM_THU_1.0.26 và NGHIEN_CUU_TDT_REA_1.0.26 trước. SignScaleForm/Control thêm orientation; 12-api api-sign-display dùng kh-display-transaction, giữ wrapper api-sign-scale hai tham số. META sign_label_orientation PARALLEL/PERPENDICULAR. 08-signs lấy đoạn cuối đường nối biển–trụ (bỏ @BHT_RAIL), căn cụm nhãn hai trụ giữa các đoạn, giữ hướng dễ đọc và vị trí tay. Tests test_LABEL126/run_scale652 và PaletteLayoutProbe.

REA đã xác định GetStationAtPoint/GetPointAtStation gọi TdtVerticalAlignmentManager, dùng X/tỷ lệ/đường dóng trắc dọc: KHÔNG gọi với điểm bình đồ. Dữ liệu phù hợp gồm hình học TdtGeAlignment, lý trình gốc, khoảng cách cọc và hiệu chỉnh (DisplayInformation cộng cả hai). Evidence34 bản ghi và snapshot nằm audit-1.0.26. DwgFiler .NET thử nghiệm bị access violation ngay tự kiểm polyline trong CoreConsole, đã loại source/DLL thử khỏi phát hành; chỉ giữ source nghiên cứu và log ngoài repo. Chưa có native runtime thành công; không tuyên bố đã bỏ DTST/BTST. DWG gốc SHA936A8CE...5075 giữ nguyên.
## 1.0.25 - 2026-10-14

ProgressForm.cs mở BHTPROGRESSFORM/BHTPROGRESSPREVIEW; 07-segments.lsp triển khai BHTXULYDOAN, progress-plan/apply/finish/reopen. Chỉ chọn hồ sơ đủ điều kiện của đoạn đang xử lý; giữ đoạn khác và hồ sơ progress_locked=1. Snapshot progress_point và progress_route_signature được kiểm tra khi xuất/nạp kế thừa. Không coi xác nhận hồ sơ là đủ khối lượng khảo sát. Từ ngữ giao diện: xử lý theo đoạn, kế thừa dữ liệu, xác nhận kết quả đoạn, xuất dữ liệu tích lũy.

ProgressPolicy.cs + BhtStore + rec-write/delete bảo vệ kết quả. ProjectTransfer partial với ProgressTransfer.cs: ImportRolling/BHTNAPTICHLUY, ExportSelection, chống trùng/xung đột, remap route_id và handle. Bỏ tuyến nguồn không được hồ sơ nhập sử dụng, giữ META đích và ghi chú khi nâng kết quả; không nhập FIELD_CHAIN. ResetStation của chế độ khảo sát mới phải xóa trạng thái xác nhận. Xuất riêng đoạn lọc điểm/ảnh/ký hiệu/tim, tệp ảnh vẫn ngoài gói. Bản cũ không hiểu cơ chế bảo vệ mới.

run_progress125.ps1 kiểm tra thường/bảo vệ, hai DWG nối tiếp, UI và bản sao đoạn 10. CoreConsole cần /p BHTResearch111 trên máy này vì startup TDT có lệnh menu gây ngắt script. Các runner cho phép AcadProfile tùy chọn; tên helper TestLispPath tránh alias lp trong Windows PowerShell. Không đổi cấu hình TDT. Đọc báo cáo 1.0.25 và audit ngoài nguồn. DWG thật có OBJ-000182/183/184 XA_TUYEN (24.520/21.077/20.903 m); xác nhận đoạn bị chặn đúng. Chỉ loại gán đoạn ba hồ sơ trong bản sao kiểm thử để kiểm tra phần hợp lệ, không sửa khảo sát gốc. OBJ-000058 khác phía đường, TIMRANHD10 còn loại TIM_DUONG; cuối bảng Km51+634.00 khác tên DWG Km51+634.24, cần đối chiếu tài liệu kỹ thuật.

## 1.0.24 - 2026-10-14

BHTGANDOAN mở bảng chọn mã đoạn/gói, tên công trình, khoảng Km, phía đường, tim và đối chiếu điều kiện. Hủy giữ nguyên; gán/bỏ gán có một Undo. Tính lại theo tuyến cập nhật phân đoạn tự động và giữ gán thủ công. Không dùng phía so với tim rãnh thay phía đường. Trạng thái KHAC_PHIA_DUONG giải thích đúng trường hợp nằm trong khoảng nhưng sai phía.

I.449 có HEADER (mặc định ĐƯỜNG) phía trên và NAME phía dưới, căn giữa trong mặt biển, giữ dữ liệu NAME cũ và xuất đủ hai dòng. Kiểm tra DWG đoạn 10: OBJ-000058 lưu PHAI trong khi DOAN_10 là TRAI; TIMRANHD10 còn khai báo TIM_DUONG, bảng đoạn chưa gắn tim. Giữ nguyên bản vẽ gốc, kiểm tra trên bản sao.
# Cập nhật 1.0.23 — 14/10/2026

RouteForms.cs cung cấp BHTROUTEOPTIONS/BHTROUTEMARKS qua LispFunction: form chọn loại/numeric validation và bảng chọn mốc. 06-routes dùng chung route-options cho nạp Polyline/TDT và sửa tuyến; Core Console trả SKIP cho UI, có CLI dự phòng. BHTDSMOC trả mã raw-row để xóa đúng sau sắp xếp, kiểm tra snapshot, rollback và Undo; VIEW đổi dist theo start/direction rồi vẽ dấu cộng/circle tạm đúng tâm. Mốc tự động có index âm, không xóa tại bảng. Route123 31 Lisp/command +24 UI, Undo/reopen, fake TDT adapter để kiểm tra loại; không chứng minh native TDT trong ảnh. Đọc RELEASE_NOTES_1.0.23.md và BAO_CAO_NGHIEM_THU_1.0.23.md. Nguồn trước sửa ở audit-1.0.23/source-before-1.0.23.zip.

# Cập nhật 1.0.22 — 14/10/2026

Đọc RELEASE_NOTES_1.0.22.md và BAO_CAO_NGHIEM_THU_1.0.22.md trước. Workflow122 bảo vệ phân biệt nhập nguyên dự án và nhập khảo sát theo tim đích; TDT dùng ROUTE_ONLY từ khi mới tạo và không lấy số cọc khảo sát. Tdt91Stakes bỏ toàn bộ hiển thị BHT trước khi đọc TEXT/block. ProjectTransfer sửa identity trong geometry_signature, giữ thân signature để bắt thay đổi hình học; remap XData nguồn TDT. Nút bố trí theo sáu bước, phương pháp FIELD ở mục riêng. Filebaohieu.bht gốc: 179OBJ/1ROUTE/0SEG. Gói bổ sung bảng xuất riêng, giữ gốc.

# Cập nhật bàn giao 1.0.21 - 14/10/2026

06-routes/07-segments chọn điểm trụ gần tim theo XY cho các nhóm biển có so_tru>1, tính station/offset/side tại cùng điểm. Giữ obj-position centroid cho ký hiệu. Lưu station_point_* và station_projection_*; xóa khi đổi nguồn/invalid tuyến/mơ hồ nhiều tim. 08-signs origin-sync dùng kh-fillable-p cho cả biển Khác; BANG_CHI_DAN có nhiều đường nối. SignSupports.Feet trả null khi không có chân để tránh empty ResultBuffer marshal eInvalidInput. OffsetDimension.cs đăng ký CommandClass/LispFunction BHTOFFSETDIM; lệnh Lisp BHTDIMTIM có Undo, tạo/cập nhật AlignedDimension theo object+route, XY z0, không text override. Không tự tạo DIM khi mở/tính lại.

DWG mới đúng ảnh: D:/lisp/BHT/New/BD-TD-TN_Doan8(Km48+469.1+Km50+290.77).dwg. Tim TIMRANHD8 handle6E3A0. OBJ-000120 trụ154 offset2.251304 < trụ155 offset3.072335; cũ centroid2.662, mới Km48+525.006. OBJ-000116 nhóm KHAC điểm161 offset1.143088, Km48+784.073. Bản DIM lưu riêng BHT-Doan8-DIM-kiem-tra-1.0.21.dwg; gốc giữ SHA256. run_offset121 kiểm tra CoreConsole+save/reopen+nearest support+true Measurement. Audit ngoài source: D:/lisp/BHT/audit-1.0.21. Không tự cài bộ phát hành.

# Cập nhật bàn giao 1.0.20 - 14/10/2026

Sửa báo cáo Excel thiếu biển phụ tại SignReportRecords và SignReportWorkbook. Mỗi mặt một dòng, giữ trùng mã, mat ưu tiên so với ma_hieu, fallback so_mat <=20 giống bht:kh-sign-codes. Số trụ/FaceCount ghi dòng đầu, FacePosition và FaceContent bổ sung cuối danh sách. BaseCode loại tham số @ trong mã, giữ số thực ở nội dung; XML chỉ lấy đúng index+code, không lộ XML thô. DESC_3#0 của S.509a là override số mét thực sự ở CAD nên ưu tiên hơn @/mẫu. Tổng hợp nhóm thêm nội dung. Offset chỉ đổi tên/giải thích, không đổi công thức. Kiểm tra Core C120 và run_report120.ps1 với BhtDataService/XRecord/catalog thực tế. Bản DWG Desktop filebaohieu có118 cụm146 mặt19 mãS; Doan8 trong Ho so là bản cũ không có hồ sơ BHT, không dùng làm bằng chứng trạng thái screenshot. Không tự cài.

# Bàn giao 1.0.19 — 14/10/2026

Ưu tiên RELEASE_NOTES_1.0.19 và BAO_CAO_NGHIEM_THU_1.0.19. SegmentTable.cs (Core) đọc XLSX/TSV cũ, kiểm tra và lưu BHTSEG/1 (.bhtseg ZIP XML/SHA256); SegmentStore.cs (Bridge) áp dụng atomic và bỏ gán cũ; SegmentTableForm.cs (Palette) xem trước/sửa/gán tim. 07-segments.lsp tính trên từng route_id trong SEG, độc lập station_active_route. 06-routes.lsp giữ tim riêng khi tính chung/riêng hoặc chế độ cọc, đổi tên cập nhật SEG. Thiếu tim/mốc chặn phân đoạn; không fallback. Kh-display-transaction hoàn tác cả kết quả LOI lẫn exception. Core C#5, AutoCAD2024; bảo toàn các sửa nhãn/bảng tổng hợp 1.0.18. Bộ thử mới: Segment119 Core, SEG119 Lisp+Store+Undo/reopen/.bht transfer, và 7 ca Palette mới. Không tự suy mã gói thực tế: GOI_01…03 là mã sinh từ tên nhóm Excel và hiện trước khi áp dụng.
# Cập nhật bàn giao 1.0.14 - 14/10/2026

Ưu tiên [ghi chú 1.0.14](RELEASE_NOTES_1.0.14.md), [kiểm chứng](BAO_CAO_NGHIEM_THU_1.0.14.md) và [rà soát nghiệp vụ](RA_SOAT_KY_THUAT_1.0.14.md). Cọc tiêu giữ chân tại điểm đo, vuông góc tuyến, không có đường nối; tách khỏi cách bố trí biển báo. Bộ rộng run_signs chưa đạt, có đối chiếu 1.0.13; xem giới hạn nghiệm thu. Nội dung bên dưới là lịch sử trước bản này.

# Bàn giao BHT 1.0.13 cho AI/người phát triển tiếp theo

## Đọc theo thứ tự

1. AGENTS.md — quy tắc phiên bản và bảo toàn công việc.
2. VERSION, CHANGELOG.md — trạng thái bản hiện tại.
3. docs/DATA_CONTRACT.md — Dictionary/XRecord/XData, nguồn dữ liệu trong DWG.
4. src/lisp/modules/12-api.lsp, src/dotnet/BHT.Bridge/LispApi.cs, AcadDispatcher.cs — hợp đồng giao tiếp.
5. src/dotnet/BHT.Palette/BhtPaletteControl.cs — gắn tài liệu, gọi API, trạng thái giao diện.
6. docs/RELEASE_NOTES_1.0.0.md; build/v1.0.0/qa chứa số đo và bản sao mẫu trên máy phát triển.

Bản 1.0.0 kế thừa 0.6.68. Thư mục source hiện tại không có Git metadata. Các đường dẫn máy cá nhân trong script/tài liệu lịch sử là cấu hình của máy phát triển, không phải đường dẫn bắt buộc trên máy mới.

## Bản đồ xử lý

- Điểm RTK, hồ sơ đối tượng: Lisp 02-points, 04-objects; BhtDataService ở Bridge.
- Tuyến/đầu tuyến/mốc Km và lý trình: 06-routes, 07-segments; RouteDataControl và TabObjectsRoutes.
- Sinh, nối, xoay và đổi tỷ lệ ký hiệu: 08-signs; SignSupports, SignLayout, SignContentBlocks.
- Thư viện/ảnh biển: BhtSignLibrary và TdtSignLibrary ở Bridge; SignPicker* ở Palette.
- Đèn: LightModels, LightPickerControl, scripts/LightLibraryBuilder.cs.
- Chữ: NativeTextService, quy tắc Unicode/phông trong AGENTS.md.
- Gói cài: scripts/package.ps1, compile-lisp.ps1, protect-dotnet.ps1, packaging/SingleFileInstaller.cs.

## Những quyết định cần giữ

- Không tự nâng cấp/sinh lại ký hiệu lúc khởi động. Cơ chế cũ từng gây lỗi native CAD; cập nhật hình học chỉ khi người dùng thực hiện thao tác tương ứng.
- Tỷ lệ biển chung áp dụng BIEN_BAO, BANG_CHI_DAN, BANG_QC, KHAC, CHUA_XAC_DINH; cọc và đèn có quy tắc riêng. Giữ tọa độ/góc INSERT khi đổi tỷ lệ.
- Tắt tô nền chỉ bỏ nền; giữ hatch hình vật thể và viền.
- Nhãn cọc đặt dưới ký hiệu, xoay theo INSERT; giữ H4/40. Hồ sơ Km/H đủ dữ liệu được dùng làm mốc theo tuyến phù hợp, không bắt nhập lại bằng BHTMOCKM.
- Đèn chiếu sáng/tín hiệu tách nhóm, không có nhãn ngoài dưới chân.
- Trụ mất mặt biển dùng khung có chữ “Trụ mất biển”, không nhãn ngoài; giữ mã gốc để khôi phục.
- Lưu nội dung Unicode; chuyển bảng mã cũ chỉ tại lớp hiển thị khi cần.
- Không sửa tài nguyên TDT gốc hoặc đưa thư viện vendor cục bộ vào Git.

## Sửa lỗi nạp Lisp 0.6.53

BhtPaletteControl.ProbeLisp trước đây quy mọi lỗi gọi API thành khác phiên bản. Bản mới phân biệt phản hồi phiên bản hợp lệ nhưng không tương thích với tình trạng chưa gọi được API.

LispRuntimeLoader tìm FAS (hoặc LSP khi phát triển) đúng tên phiên bản cạnh DLL Bridge. Gửi LOAD qua hàng đợi CAD; Lisp gọi BHTRUNTIMELOADED với token khi hoàn tất, sau đó Palette kiểm tra lại API. Mỗi lượt mở bảng thử một lần, chờ tối đa 30 giây; callback cũ bị bỏ khi đổi/đóng tài liệu.

Không thay SECURELOAD trong luồng sản phẩm. Test cô lập có thiết lập riêng. Không dùng NETLOAD chồng DLL mới để thử trong phiên CAD người dùng.

**Đã xác định ở 0.6.54:** OR trong AutoLISP trả T, không trả đường dẫn. Khi findfile tìm thấy file trong Support Path, vl-filename-directory nhận T và lỗi stringp. Đã thay bằng gán findfile + fallback riêng và tái hiện trước/sau sửa. Xem docs/QA_0.6.54.md. Hành vi đổi/đóng nhiều tài liệu và hủy LOAD vẫn cần kiểm thử tương tác AutoCAD đầy đủ.

## Kiểm thử đã thực hiện

- CoreTests: 233 đạt.
- Palette layout/editor workflow: 683 đạt.
- Bootstrap DLL đã bảo vệ + FAS, đường dẫn có dấu/khoảng trắng: thiếu API ban đầu -> nạp -> gọi API đúng phiên bản.
- Nhánh lỗi thiếu file, LOAD lỗi và phiên bản không tương thích.
- Runtime độc lập: chèn biển, lưu và mở lại DWG.
- Bộ cài một file: 39 kiểm tra, kiểm tra payload lỗi và validate-only không cài đè.

Các script chính: tests/IntegrationTests/run_bootstrap653.ps1, run_palette_layout.ps1; tests/ReleaseTests/protected-runtime.ps1, single-file-installer.ps1. Tests tích hợp cần AutoCAD và có thể cần tài nguyên local; không báo đạt nếu môi trường thiếu.

## Tiếp tục phát triển

- Đọc mã trước khi đề xuất; không dựa riêng vào README/hướng dẫn cũ vì chúng mô tả nhiều giai đoạn lịch sử.
- Thay đổi chức năng tiếp theo phải tăng phiên bản bằng scripts/set-version.ps1, tối thiểu 1.0.14.
- Hoàn tất code, test và tài liệu trước package: RELEASE_STATE.json chặn đóng gói lại cùng phiên bản với nội dung đã đổi.
- Không đưa kết quả build, DLL Autodesk, dữ liệu người dùng hoặc bản sao tài nguyên vendor lên repo.
- Kho hiện chưa có LICENSE riêng cho mã BHT; công khai để đọc không thay thế việc lựa chọn giấy phép phân phối/tái sử dụng.
