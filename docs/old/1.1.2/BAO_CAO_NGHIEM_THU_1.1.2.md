# Báo cáo kiểm tra BHT 1.1.2

Ngày 16/10/2026. Phiên bản 1.1.1 → 1.1.2. Nhánh development/incremental-confirmation, Git root D:/lisp/BHT/sharing-0654/source. Không commit/push. Baseline đầy đủ của dirty worktree, danh sách tệp và hash ở D:/lisp/BHT/audit-1.1.2/baseline; giữ công việc có sẵn.

## Vấn đề và bằng chứng

Người dùng xác nhận Nạp dữ liệu kế thừa, chọn tệp rồi CAD đứng. Nhật ký 1.1.1 chỉ bắt đầu sau khi GetFileNameForOpen trả về, nên không quan sát được bước chọn/đóng tệp. Core Console kiểm tra lại cùng DLL/FAS đang cài: 16 kiểm tra dữ liệu và 2 kiểm tra lệnh đạt, phát hiện xung đột D14/D15 trong khoảng 0,2–0,3 giây. Tệp DOAN14 đúng cấu trúc/SHA256, 179 OBJ và 801 điểm.

Trong AutoCAD đầy đủ 1.1.1, probe mở Palette, chọn trang tuyến, theo dõi lệnh BHTNAPTICHLUY và nhập đường dẫn bằng script FILEDIA=0. Lệnh kết thúc, nhận 6 xung đột, bảng ImportConflictForm hiện và timer tiếp tục chạy 12 nhịp; fingerprint đích trước/sau giống nhau. Bằng chứng D:/lisp/BHT/audit-import-20261016-0805/evidence/gui-visible/gui-trace.log. Lúc đầu AutoCAD chờ trước khi chạy script, sau đó script đã chạy; không có bằng chứng xác định hộp thoại nào gây chờ. Công cụ Computer Use lỗi native pipe, không xác minh màn hình bằng công cụ đó.

Chưa tái hiện được vòng CPU/bộ nhớ của người dùng. **Nguyên nhân gốc chưa xác định**. Kết quả chỉ thu hẹp nghi vấn đến luồng cửa sổ chọn tệp hoặc trạng thái phiên thực tế; không kết luận lỗi gói dữ liệu hay giấy phép gây đứng.

## Thay đổi

- ProgressTransfer.ImportRollingCommand: bắt đầu nhật ký trước bước chọn tệp; picker riêng cho GUI, giữ script FILEDIA=0; ghi kết thúc chọn tệp, hủy, exception và kết thúc xử lý. Kiểm tra vẫn ở đúng Document trước nhập.
- RollingImportLog mới: nhật ký cục bộ, phiên bản/đường dẫn DLL/đích/bước xử lý. Không ghi dữ liệu DWG.
- RollingImportFileForm mới và PaletteHost.PickRollingFile: chọn/dán đường dẫn, bấm Kiểm tra và nạp; không mở ZIP/DWG trong cửa sổ chọn tệp; cửa sổ Windows AutoUpgradeEnabled=false để bỏ xem trước shell. Kiểm tra đường dẫn/đuôi/tồn tại; cho phép dán trực tiếp nếu cửa sổ chọn tệp có vấn đề.
- BhtPaletteControl.SendCmd/OnCommandFinished: riêng BHTNAPTICHLUY không gọi Lisp DRAIN trước/sau; làm mới Palette khi lệnh kết thúc. Đây là giảm thao tác tái nhập, chưa có bằng chứng nó là nguyên nhân gốc.
- PaletteHost.ShowImportConflicts: ghi nhật ký tạo/hiện/đóng bảng, bắt cả lỗi constructor sau khi lệnh kết thúc.
- Import112Probe/run_import112: hủy/lỗi picker/tệp thiếu/xung đột, canonical, thứ tự log, bảng chọn và xác nhận rõ ràng. Không sửa kiểm tra cũ để bỏ qua lỗi.
- VERSION đồng bộ bằng set-version.ps1; CHANGELOG, hướng dẫn mới và mục lục. Hướng dẫn 1.1.1 giữ nguyên nội dung trong old với nhật ký hash.

Không đổi ProgressPolicy, PrepareRolling, CheckRollingConflicts, RevisionMerge hay cấu trúc ZIP. Không đổi RTK, hình học tuyến hoặc hồ sơ đã khóa. Hai hồ sơ khác nhau trùng OBJ-000001 tiếp tục bị chặn: đích DEN_TH/BOT19-R-000144, nguồn BIEN_BAO/P.127-80/BOT1-R-000041.

## Kiểm tra thực chạy

| Mức | Kết quả |
|---|---|
| Build C#/.NET 4.8 và CoreTests | 428 PASS, 0 FAIL |
| DLL bảo vệ và FAS, chọn tệp/hủy/lỗi/bảng | 33 PASS, 0 FAIL |
| D14/D15 canonical, Undo, lặp, DWG trống và lưu/mở lại | 16 PASS, 0 FAIL |
| Lệnh BHTNAPTICHLUY thật với FILEDIA=0 | 2 PASS, 0 FAIL |
| Bảng 100–200% font mô phỏng | 316 PASS, 0 FAIL |
| Palette layout | 3.102 PASS |
| M1 bổ sung/Undo/import rollback | 31 Lisp + 29 .NET, mở lại DWG PASS |
| M2 hiệu chỉnh | 50 PASS, 0 FAIL |

Các hàng trên là CoreTests hoặc AutoCAD Core Console, không thay thế nghiệm thu chọn tệp Windows trong AutoCAD. Bằng chứng trong audit-1.1.2/evidence, dùng DLL protected cuối tại build/v1.1.2/final/bin-protected và FAS1.1.2.

Lỗi công cụ lần đầu protect-dotnet do giá trị mặc định PSScriptRoot không có trong thời điểm bind tham số; đã chạy lại với đường dẫn Obfuscar rõ ràng, đạt. Không thay đổi script để che lỗi. Hướng dẫn mới qua validator tiếng Việt: 0 lỗi, 0 cảnh báo. Ảnh WinForms là bằng chứng bố cục, không phải ảnh chụp AutoCAD GUI.

## Giới hạn, phục hồi và bước tiếp theo

Bản 1.1.2 là bản kiểm tra cục bộ cho luồng chọn tệp mới. Chưa nghiệm thu picker Windows và việc khắc phục hiện tượng đứng của người dùng. Chưa có giải quyết tương tác trùng mã legacy; không được xoá hoặc tự đổi mã để ép nạp. 11 lỗi signs của bản trước chưa sửa và chưa chạy lại bộ signs rộng trong đợt này. Không nghiệm thu các Milestone còn lại chỉ từ bản sửa này.

Nếu còn đứng, giữ nhật ký ngay sau lỗi và ghi bước cuối. Dùng lối dán đường dẫn để phân biệt chọn tệp với đọc dữ liệu. Chỉ thử trên bản sao; rollback bằng đóng CAD/cài lại 1.1.1 và mở bản DWG trước thử. Không migration dữ liệu hoặc tự tạo revision khi chỉ mở bảng.

Kết quả cài bộ cài và GUI sau đóng gói được lưu bổ sung ở audit-1.1.2/evidence, không thay mã dưới cùng phiên bản. Phát hành GitHub và xử lý trùng mã có kiểm soát nằm ngoài đợt này.
