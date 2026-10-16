# Báo cáo kiểm tra BHT 1.1.1

## Phiên bản và phạm vi

1.1.0 → 1.1.1, nhánh development/incremental-confirmation, Git root D:/lisp/BHT/sharing-0654/source. Giữ toàn bộ dirty worktree trước đợt sửa tại audit-1.1.1/baseline/source, tracked.diff và source-hashes.csv. Không commit/push/main trong đợt này. Bộ cài 1.1.0 giữ nguyên nội dung.

## Vấn đề và nguyên nhân đã xác định

- Bảng quản lý bình đồ kế thừa ForeColor xanh nhạt của dialog trong khi ô có nền trắng; PaletteTheme chưa xử lý DataGridView. Bảng đoạn chưa có cơ chế xuống dòng/giãn chiều cao tiêu đề và dùng font nhỏ.
- 7 gói DOAN8–DOAN14 đúng cấu trúc/SHA256, đọc được 179 OBJ và 801 điểm mỗi gói. ValidateConfirmedPoints đạt trên cả 7 bản sao. Điều này chỉ xác minh tính nhất quán được kiểm tra, không chứng minh số liệu ngoài thực địa chính xác.
- D15 đã lưu có 1 OBJ, 790 điểm, 1 tim. OBJ-000001 là DEN_TH, điểm BOT19-R-000144; nguồn DOAN14 cùng mã là BIEN_BAO/P.127-80, điểm BOT1-R-000041. Xung đột nhom, ma_hieu, mat, pt, so_tru, so_mat. Backend 1.1.0 đã chặn đúng, trước bước clone; 1.1.1 giữ chính sách này.
- Phiên GUI người dùng PID26428 không phản hồi, CPU tăng và bộ nhớ khoảng 8,2 GB. Không dừng hoặc sửa phiên đó. Sau đó tiến trình không còn trong danh sách. Không có bằng chứng kết luận xung đột dữ liệu là nguyên nhân vòng CPU; không tái hiện được đứng trên DWG đã lưu. Công cụ Windows lỗi native pipe; đọc stack thụ động không attach được. Không suy diễn đã sửa triệt để.

## File và hàm thay đổi

- PaletteTheme: StyleGrid/ApplyTableDialog, màu ô/header/selection rõ ràng, padding, kiểu chữ.
- SegmentTableForm: tiêu đề hai dòng, cột có độ rộng tối thiểu, dòng tự giãn, font lớn, phía hiển thị tiếng Việt, giữ giá trị lưu, bảo vệ DataError index.
- DrawingProjectForm mới: cửa sổ dùng chung với TabDrawingProject.OpenDrawingProject, bố cục sáng, bảng/chi tiết/nút; giữ các đường xác nhận, fingerprint và dispatcher cũ.
- ImportConflictForm, PaletteHost.ShowImportConflicts: font/contrast, chiều cao tối thiểu, bảng chỉ tự đo dòng đang hiển thị; báo cáo modeless được tạo sau khi lệnh kết thúc tại Idle. Không ghi dữ liệu từ báo cáo.
- ProjectTransfer.Copy/CopyTransaction, ProgressTransfer.ImportRolling/ImportRollingCommand: callback bước xử lý, nhật ký gần nhất, xung đột ghi rõ nguồn/đích. Không đổi chính sách ProgressPolicy, revision, segment hoặc toạ độ.
- BhtStore.ReadXrecord/WriteRaw: dispose ResultBuffer, đọc Data một lần. Không đổi codec/ZIP.
- scripts/coreconsole, run_correction132: giữ handle tiến trình để Windows PowerShell 5 không trả ExitCode rỗng. run_regression110: tránh tên LP trùng alias hệ thống.
- run_import111, Import111Probe, Tables111Probe, Import111CommandProbe/run_import111_command: hồi quy mới; package.ps1 sao chép ảnh hướng dẫn.
- VERSION/DLL/Lisp/bundle/installer đồng bộ bằng set-version.ps1; CHANGELOG và hướng dẫn mới.

## Hành vi trước/sau

Trước: chữ bảng đồng bộ mờ, header đoạn bị cắt; lệnh nạp không có bước tiến độ, xung đột mở modal trong đường xử lý lệnh. Sau: chữ tối/nền sáng, header tự xuống dòng, chọn dòng đọc chi tiết; nạp có nhật ký và bảng xung đột độc lập để kiểm tra CAD. Dữ liệu trùng mã khác nội dung tiếp tục bị chặn; không đưa dữ liệu cũ đè lên hồ sơ khác.

DOAN01 và DOAN_01 cùng có trong dữ liệu, giữ nguyên 30 đoạn. Không tự đồng nhất mã đoạn/gói hoặc sửa tệp nguồn.

## Kiểm thử thực chạy

| Mức / bộ | Kết quả | Bằng chứng |
|---|---|---|
| Unit Core | 428 PASS, 0 FAIL | build-final-verified.txt |
| Bảng font/contrast/header/nút, 100–200% cỡ chữ | 316 PASS, 0 FAIL | protected-tables-final |
| 7 gói, D14→D15, snapshot, import lặp, reopen | 28 PASS, 0 FAIL | protected-import |
| Lệnh BHTNAPTICHLUY thật + FAS + log | 2 PASS, 0 FAIL | public-command |
| M1 | 31 Lisp + 29 .NET, reopen PASS | incremental.txt |
| M2 | 50 PASS | Correction132Probe |
| Merge | 17 PASS | Revision134Probe |
| GLOBAL/LOCAL | 21 PASS | Presentation135Probe |
| Project / released / archive | 20 PASS | Project136Probe |
| Bảng hiệu chỉnh | 101 PASS | CorrectionGrid133Probe |
| Palette tổng | 3.102 PASS | palette-layout-final.txt |
| Report / điểm đầu / preview | 10 PASS | regression110-final |

Bằng chứng tại D:/lisp/BHT/audit-1.1.1/evidence. DLL/FAS bảo vệ được dùng cho các bộ hồi quy. Thời gian đo protected: chặn D14/D15 255/186 ms; nạp DWG trống 408 ms, 4.257 thực thể CAD; bảng 1.000 xung đột tạo trong 66 ms. Đây là phép đo trên máy này, không phải cam kết cho mọi bản vẽ.

Canonical D15 trước/sau hai lần từ chối giống nhau: các bản ghi, điểm, danh sách thực thể, XData và thuộc tính hình học quan trọng; không chỉ kiểm tra thông báo. Nạp DOAN14 trên DWG trống giữ 179 OBJ/801 điểm, nhập lặp và lưu/mở lại không đổi. Hash nguồn đối chiếu cuối đợt; không chỉnh tệp New.

## Lần thử chưa đạt và sửa tiếp

Baseline 1.1.0 tái hiện header cắt/selection khó đọc. Bản sửa đầu còn 3 kiểm tra header tại 125% thất bại; đã tăng độ rộng tối thiểu và chạy lại 316/316. Palette hồi quy phát hiện nút Đóng bảng xung đột nằm ngoài cửa sổ nhỏ; đã sửa chiều cao tối thiểu/khung chi tiết và đạt 3.102. Không nới điều kiện kiểm thử để che lỗi.

Runner Windows PowerShell trả exit code rỗng dù 28 assertion đạt; đã giữ process handle, chạy lại thành công. Runner regression có LP alias làm đường dẫn rỗng; đã sửa tên helper và đạt 10 assertion. Một lần build vào thư mục đang giữ PDB bị file lock; chuyển output sạch, build cuối đạt. Giữ log các lần lỗi để truy vết.

## Rủi ro và giới hạn

Chưa nghiệm thu AutoCAD GUI/DPI thực tế hay xác minh phiên đứng tại máy người dùng. Deferred Idle/modeless cần kiểm tra trực tiếp sau cài; không coi WinForms trong Core Console là GUI AutoCAD. Chưa có xử lý trùng mã khác đối tượng, đối chiếu project legacy tương tác hoặc hợp nhất 30 mã đoạn. 11 FAIL signs đã ghi ở 1.1.0 chưa được giải quyết/kiểm tra lại; không mở rộng sửa các lỗi độc lập trong bản này. Giấy phép AutoCAD/native TDT và các giới hạn roadmap cũ còn nguyên.

## Sử dụng, migration và rollback

Không đổi ZIP, không tự tạo metadata khi mở, không sửa hồ sơ khoá ngoài giao dịch, không tự đổi RTK/hình học. Hướng dẫn thao tác có ảnh tại HUONG_DAN_HIEU_CHINH_1.1.1.md. Tài liệu 1.1.0 đã gom docs/old/1.1.0 và đối chiếu hash. Bộ cài cũ giữ dưới old để phục hồi khi CAD đóng. Chỉ kiểm tra trên bản sao, giữ tệp lịch sử.

## Bước tiếp theo

Thực hiện bài kiểm tra GUI trên bản sao D15; lưu bước cuối/nhật ký nếu còn đứng. Sau khi xác định đúng stack/sự kiện, sửa nguyên nhân bằng bản patch mới. Quy trình xử lý hai hồ sơ trùng mã cần preview toàn bộ tham chiếu và transaction riêng trước khi mở chức năng; không tự xoá hoặc rename để giúp import chạy qua.