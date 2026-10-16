## Layer tuyến 1.1.5

Chạy run_layers115.ps1 với BinDir, RuntimePath, InputPackage và OutDir mới. Probe chỉ mở bản sao từ gói, so sánh canonical kho BHT/RTK/hình học/XData; xác minh hash nguồn không đổi. 4 Lisp + 18 .NET + Undo: tạo/đổi mã tuyến, chọn lại tim bị xung đột giữ kết quả cũ, viewport/khóa/style, Line cũ, save/reopen, real DOAN15, import atomic/idempotent. Không thay test tự động bằng thông báo thành công trên Palette.

## Kiểm tra phạm vi 1.1.4

run_scope114.ps1 -BinDir <DLL> -RuntimePath <FAS> -OutDir <thư-mục-mới>: 28 kiểm tra + reopen canonical. test_SCOPE114.lsp tái hiện clamp trên 1.1.3, baseline dùng run_regression110 -Baseline. test_REAL_SCOPE114.lsp chỉ đọc bản sao và xuất kết quả chiếu, không thay số cọc Km/H nhập tay. Correction114UiProbe kiểm tra 9 điều kiện preview/duyệt/invalidated, chạy qua run_correction132 -Probe Correction114UiProbe -Command CORRECTION114UI. Evidence D:/lisp/BHT/audit-1.1.4. Không suy diễn thành nghiệm thu chuột.

# Kiểm thử tích hợp — AutoCAD Core Console (accoreconsole.exe)

Chạy trên máy có AutoCAD 2024 (mặc định `D:\AutoCAD 2024\accoreconsole.exe`, `LISPSYS = 1`).

## Bảng và nạp tích luỹ 1.1.1

`run_import111.ps1 -BinDir <DLL> -Inputs <thư-mục-bản-sao-7-BHT-và-D15> -OutDir <mới>` kiểm tra gói, xung đột 6 trường OBJ-000001, canonical trước/sau, RTK/hình học, nhập lặp và lưu/mở lại. `-Probe Tables111Probe -Command TABLES111PROBE` kiểm tra 316 điều kiện về chữ, độ tương phản, tiêu đề, nút và bảng 1.000 xung đột; tạo PNG từ WinForms thật. Không thay GUI nghiệm thu.

`run_import111_command.ps1 -BinDir <DLL> -Inputs <bản-sao> -RuntimePath <FAS> -OutDir <mới>` gọi lệnh BHTNAPTICHLUY thật, kiểm tra xung đột nguyên tử và nhật ký cục bộ. Nguồn khảo sát không đưa vào Git/bộ cài. Bộ test M1–M5 và Palette chạy lại với DLL/FAS bảo vệ. Runner giữ native process handle để Windows PowerShell 5 đọc đúng exit code; run_regression110 dùng tên hàm đường dẫn riêng, tránh alias LP.

## Hiệu chỉnh 1.0.32

`run_correction132.ps1 -BinDir <DLL> -OutDir <audit>` chạy fixture 50 hồ sơ khóa và Correction132Probe. Có `-RuntimePath <FAS>` cho runtime đóng gói. Test gồm rollback lỗi sau CAD/history, U/REDO, guard C#/Lisp, RTK, phân đoạn, số trụ, lưu/mở lại và export/import lịch sử. `Correction132RealProbe` chỉ chạy trên bản sao D10; `Correction132UiProbe` kiểm tra WinForms trong Core Console, không thay nghiệm thu AutoCAD GUI. Fixture `correction132-api.lsp` hỗ trợ probe đọc trạng thái; không đưa vào bộ cài.

Runner chạy ẩn, timeout và chỉ dọn senddmp của test. Không chạy trên DWG nguồn. Không chạy tự động bộ signs rộng khi chưa xử lý lỗi native đã ghi trong báo cáo. Giữ log/snapshot cần thiết trước khi xóa profile/DWG sinh lại được.

## Milestone 1 — 1.0.31

`run_incremental131.ps1 -BinDir <thư-mục-DLL>` tự tạo dữ liệu giả lập trong `build/v1.0.31`, dùng profile BHTResearch111. Bộ này kiểm tra 25 hồ sơ khóa + hồ sơ bổ sung, snapshot trước/sau, bảo vệ điểm/tim/nhãn tay, lỗi giữa lần ghi, một Undo mỗi lệnh, stale preview, legacy không tự điền metadata, gói tương đương, nạp lặp, snapshot NaN và lưu/mở lại DWG. Cần chạy `run_progress125.ps1` và `run_palette_layout.ps1 -AcadProfile BHTResearch111` để kiểm tra hồi quy liên quan. Các bộ này không cần chỉnh dữ liệu khảo sát thật.

Giữ các tệp .cs/.lsp/.ps1 của kiểm thử. Sau khi lưu log và báo cáo, có thể xóa thư mục kết quả/profile/DWG giả lập để giảm dung lượng; runner tạo lại ở lần chạy sau. Kết quả và giới hạn 1.0.31 ở `docs/BAO_CAO_MILESTONE_1_1.0.31.md`.

## Bộ kiểm thử lịch sử

Thư mục làm việc cố định trong kịch bản: `C:\Users\Le Bao\BHT_TEST_V044\` (đổi `*T-DIR*` trong `t_common.lsp`
và `$w` trong `run_session.ps1` / `run_all.ps1` nếu chạy ở máy khác).

**Không có dữ liệu thật trong repo.** Trước khi chạy cần tự chuẩn bị trong thư mục làm việc:
`thư mục có dấu\BHT-1.1.5.lsp` (bản Lisp cần kiểm, đặt trong đường dẫn có dấu), `bin\BHT.*.dll` (kết quả build),
`data\survey.csv` (+ các file trong data), `anh_da_doi\kmz_out\` (205 JPG + BHT_PHOTO.tsv), `irt\google_sat_18_x.jpg`,
`irt\anh_khac.jpg`, `run\route_src.dwg`, `run\legacy_032_src.dwg` (bản sao bản vẽ làm bằng 0.3.2),
`run\v033_A_out.dwg` (bản vẽ do bộ kiểm 0.3.3 tạo). Kịch bản chỉ làm việc trên **bản sao** bản vẽ.

| Phiên | Nội dung |
|---|---|
| S0 | nạp từ đường dẫn có dấu, BHTTEST, API Lisp, xác nhận bỏ DCL và BHTLOAD khi chưa có plugin |
| A, B, L, R, L2 | hồi quy bộ kiểm 0.3.3 trên BHT-1.1.5.lsp (nhãn, hồ sơ, ký hiệu, thứ tự, ảnh, bản vẽ 0.3.2, lưu / mở lại) |
| N, N2 | bản vẽ 0.3.3 + NETLOAD BHT.Bridge: C# đọc = Lisp, C# tạo/sửa hồ sơ Lisp đọc giống hệt, Application.Invoke, lưu / mở lại |
| NL | bản vẽ 0.3.2 + BHT.Bridge |
| P | NETLOAD BHT.Palette.dll trong Core Console, lệnh chính BTH |
| R5 | Route Model V5 A–O: StartPoint/direction/closed/revision/station break/left-right/offset/legacy + scanner cọc Km chỉ đọc |
| T | ảnh nền IRT dạng lưới 156 tile, layer khóa, tile trên layer 0 trong thư mục `IRT\` |

```powershell
powershell -ExecutionPolicy Bypass -Command "& .\run_all.ps1"            # tất cả
powershell -ExecutionPolicy Bypass -Command "& .\run_all.ps1 -Only N,N2" # một số phiên
```
Kết quả: `run\result_<phiên>.txt` (PASS / FAIL / SKIP / BLOCKED từng mục, dòng `TONG PHIEN`).

## Kiểm tra review từ v0.5.5

Chạy `run_review.ps1` sau khi build DLL vào `build/v<phiên-bản>/bin`. Runner đọc fixture qua `-FixtureRoot`, ghi script/log vào `build/v<phiên-bản>/review` và dùng profile Core Console riêng. Các phiên REVIEW, S0, V5 chạy trên bản vẽ mới; TX chỉ chạy khi có `tdt/tdt_copy.dwg` và không lưu bản vẽ.

`test_REVIEW.lsp` kiểm tra bỏ DCL, giữ tìm kiếm/tình trạng, đọc trạng thái, lệnh BHTTRANGTHAI, BHTTEST và hướng dẫn BHTLOAD khi DLL không nạp được. `run_signs.ps1` kiểm tra biển báo, phông, chèn tự do tương tác và WinForms picker.
## Kiểm tra cọc TDT từ v0.6.9

`TdtStakeProbe.cs` chạy lệnh `BHTSTAKEREGRESSION` trong AutoCAD/Core Console, với BHT.Bridge đã nạp. Ca thử tạo cơ sở dữ liệu riêng trong bộ nhớ và kiểm tra block lồng nhau, phép biến đổi, từng thuộc tính, MTEXT, lý trình sai, lọc offset và quét lặp. Đặt biến môi trường `BHT_QA_OUTPUT` để chỉ định tệp kết quả.

`test_TDT_ROUTE.lsp` chỉ chạy trên **bản sao** bản vẽ khi TDT và BHT đã nạp. Cấu hình `*qa-tdt-source*` (handle tuyến TDT), `*qa-tdt-count*` (số cọc đã đối chiếu) và `*qa-output*` (tệp kết quả). Ca thử kiểm tra tạo/cập nhật cùng handle, vị trí vạch cọc, nguồn không đổi, hình mũi tên qua bước xác nhận/REGEN, UCS xoay, hai đầu tuyến, dọn hình và nạp lại không trùng. Việc xác nhận nạp mốc trong ca thử được mô phỏng trên bản sao.

## Kiểm tra biển và số Km từ v0.6.10

`Sign0610Probe.cs` chạy BHTSIGN0610PROBE sau khi nạp Bridge và Lisp trên bản sao. Ca thử kiểm tra các biến thể R.415/W.239, hình đã sửa, biểu tượng có hatch, thứ tự nền/biểu tượng, chế độ không tô màu, chữ vừa khung I.439, màu trắng cố định và số Km trong block. BHT_QA_OUTPUT chỉ định tệp kết quả. BHTPLOT0610 xuất vùng hình mẫu sang PDF; BHT_QA_PDF chỉ định đường dẫn đích.

`test_SIGN0610.lsp` kiểm tra lưu và thể hiện H9/39, cọc Km, nội dung tên cầu và cập nhật lặp. Lưu bản sao rồi nạp `test_SIGN0610_REOPEN.lsp` trong phiên mới để kiểm tra nội dung và block còn nguyên. `PickerProbe.cs` kiểm tra ô nhập I.439 và giá trị trả về sau khi đóng bảng.


### Sửa hướng cọc và mẫu I.439 (0.6.11)

`test_SIGN0611.lsp` kiểm tra lý trình tự lấy từ Km/H, ẩn nhãn và điểm chèn cọc Km. `Sign0611Probe.cs` kiểm tra ba đường viền gốc, phông Giaothong1, màu trắng khi in, vị trí thanh đuôi và các góc quay. `sign0611-template.scr` chạy các thao tác chọn điểm lên/xuống/trái/phải, UCS xoay và hủy hướng có độ dài bằng không.

`run_sign0611.ps1 -Drawing <bản-vẽ> -ProfileDirectory <thư-mục-profile-test> -BinDir <DLL-build>` tạo bản sao trong thư mục build, dùng profile test đã được cấu hình để nạp DLL/Lisp và giữ nguyên thiết lập bảo mật. Gói phát hành không chứa bản vẽ khảo sát hoặc kết quả DWG thử.

### Tỷ lệ biển và bố cục Palette (0.6.21)

run_signs.ps1 kiểm tra tỷ lệ hình/nhãn độc lập, giữ vị trí tự động/tay, nhiều nhãn, bản ghi RTK, các nhóm khác, lặp cập nhật, biển chèn sau và giá trị sai không ghi dữ liệu. run_palette_layout.ps1 kiểm tra nút trên Palette rộng 340/480 px, nhóm hành động, mở/hủy hộp tỷ lệ giữ bản nháp và cuộn, mức tùy chỉnh, đặt lại và hủy. Kết quả 0.6.21: 111 kiểm tra form đạt, 171 kiểm thử Core đạt, hồi quy CAD/ký hiệu/Unicode/G/T đạt.
### Nhãn đèn và biển trạm thu phí (0.6.28)

SignLibraryProbe kiểm tra IE.472a/b native: màu hatch xanh, chữ Unicode trắng, khung có bulge bo góc, số mét riêng, outline không sửa bản tô màu, catalog có vector và hoạt động không có TDT. PickerProbe kiểm tra nhập/sửa mét, dữ liệu từng mặt, chặn 0 và nút Đóng đóng cả cửa sổ modeless. test_SIGN kiểm tra nhãn DEN bên dưới block quay, cập nhật lặp giữ RTK, nhãn IE.472a kèm số mét.

PaletteLayoutProbe kiểm tra một cửa sổ modeless, lệnh ZOOM khi thư viện mở vẫn giữ hồ sơ, nhận lựa chọn vào bản nháp, đổi hồ sơ hoặc Unbind đóng thư viện. 144 kiểm tra đạt trong Core Console/WinForms; thao tác chuột ở AutoCAD giao diện đầy đủ chưa được thử.

Từ 0.6.29, các ca IE.472a/b còn kiểm tra đúng hai trụ/circle chân, không giữ trụ giữa cũ và các mặt ghép không chứa trụ/foot riêng.

## Kiểm tra xoay đầu biển 0.6.35

Chạy `run_heading.ps1 -BinDir <thư-mục-DLL-đã-build>` bằng AutoCAD 2024 Core Console. Kiểm tra tiếp tuyến PL thẳng/cong, chiều tuyến, hướng chung trong UCS xoay, giữ RTK/điểm chèn/tỷ lệ/điểm gấp khúc, nối chân CAP1, lựa chọn/hủy và một Undo cho cả đợt xoay. Log tại `build/v<phiên-bản>/heading.log`; yêu cầu `HEADING-FINAL-FAIL=0` và không có lỗi Lisp.


## Hồi quy 1.1.0

run_regression110.ps1 chạy trên bản sao, ghi input-proof SHA256 và kiểm tra lại nguồn sau test. Dùng BinDir của DLL protected và RuntimePath trỏ FAS. Fixture mặc định REGRESSION110 kiểm tra báo cáo/điểm đầu/modeless; CAPTION110 kiểm tra 269 điều kiện hướng nhãn; BULK110 cần DWG D13 có hồ sơ đã nạp; REOPEN110 nhận DWG đầu ra cùng qa110-saved-state.lsp và đối chiếu byte; PERF110 đo SYNC/SCALE trên D10. Không dùng D13 thô chưa có hồ sơ làm fixture bố trí.

run_correction132.ps1 hỗ trợ Report110Probe/REPORT110PROBE với report110-api.lsp để kiểm tra API và WinForms. Các probe Correction132, Revision134, Presentation135, Project136 tiếp tục dùng test_CORRECTION132.lsp. Correction132RealProbe/CORRECTION132REAL dùng D10 đã sao chép và correction132-api.lsp.

Đặt BHT_SIGN_ROOT tới thư viện SignLibrary/BHT thực sự đi kèm bundle. run_signs cần BHT_TEST_ADS=1 để kiểm tra thư viện ADS đang phát hành; nhánh native TDT mặc định chưa được chứng nhận trong đợt này. Dùng coreconsole.ps1 cho timeout/crash isolation. 11 FAIL SIGN trùng baseline136 vẫn giữ nguyên, xem BAO_CAO_NGHIEM_THU_1.1.0.md; không đổi kỳ vọng cũ để che lỗi. WinForms render không chứng minh thao tác zoom/pan trong AutoCAD GUI.


## Gom đoạn trùng 1.1.3

run_segments113.ps1 nhận BinDir, Inputs (bản sao tám gói New) và OutDir mới. Kiểm tra SEG113_TOTAL và SEG113_UNDO; lưu kế hoạch, bản DWG/BHT đã gom và ảnh form. So sánh toàn bộ bản ghi ngoài SEG/journal, RTK và CAD; chặn tham chiếu hai mã, lịch sử, stale/khác DB; append-only, fault/Undo, save/reopen và nạp lặp. Core Console bỏ qua lệnh bảng có UI theo IsCoreConsole; probe chỉ gọi backend trong nhánh đó. Nhánh AutoCAD đầy đủ dùng timer instrumentation trên bản sao để mở bảng, xác nhận và đóng bảng; không kiểm chứng thao tác chuột Windows. Không chạy trên bản vẽ người dùng.
