# BHT v1.1.5 — mã nguồn cho AutoCAD

> **Nguồn mở build 1.1.5 — đọc [START_HERE_AI.md](START_HERE_AI.md) trước.** Có [bộ tổng hợp cho AI](handoff/1.1.5/BHT_AI_HANDOFF_1.1.5.md), [hướng dẫn build](handoff/1.1.5/05_BUILD_VA_TIEP_TUC.md), lịch sử và giới hạn kiểm thử. Đây là bản thử nghiệm, không phải nghiệm thu toàn bộ milestone. Bộ cài1.1.5 giữ nguyên; chỉ bổ sung nguồn/tài liệu bàn giao. Mã C#/Lisp giữ nguyên so với snapshot cục bộ đã build.



BHT quản lý hiện trạng tuyến, điểm khảo sát, biển báo, cọc và đèn trong AutoCAD. Kho này chứa mã nguồn C# và AutoLISP để đọc, kiểm tra và tiếp tục phát triển.

**AI/người phát triển mới: bắt đầu tại [docs/AI_HANDOFF.md](docs/AI_HANDOFF.md), sau đó đọc [AGENTS.md](AGENTS.md).**

## Bản hiện tại

Bản 1.1.5 đặt layer tim theo mã tuyến; có nút đối chiếu và chuyển layer cũ. Giữ sửa lỗi 1.1.4: chặn điểm vượt đầu/cuối tim bị tính thành lý trình đầu mút, kiểm tra phạm vi cả hồ sơ gán tay và giải thích điều kiện xác nhận hiệu chỉnh. Giữ hồ sơ khóa, RTK, mốc và lịch sử. Xem [hướng dẫn mới](docs/HUONG_DAN_HIEU_CHINH_1.1.5.md) và [bài kiểm tra](docs/HUONG_DAN_TEST_TOAN_BO.md). Build cục bộ, chưa push.

- Mã nguồn **1.1.5**, assembly **1.1.5.0**; bộ cài 1.1.5 dành cho kiểm tra cục bộ trên bản sao. Chưa nghiệm thu toàn bộ Milestone 2-5 hoặc AutoCAD GUI. Bản ổn định đã bàn giao gần nhất là 1.0.29.
- [Hướng dẫn sử dụng](docs/HUONG_DAN.md), [kiểm chứng Milestone 1](docs/BAO_CAO_MILESTONE_1_1.0.31.md), [mục lục tài liệu cũ](docs/LICH_SU_TAI_LIEU.md).
- [Nghiên cứu TDT bằng REA](docs/NGHIEN_CUU_TDT_REA_1.0.26.md): đã phân tích dữ liệu tuyến/cọc; chưa bật đọc lý trình native. Cặp GetStationAtPoint/GetPointAtStation thuộc luồng trắc dọc.
- .bht giữ cả bảng phân chia đoạn đã lưu trong DWG. Nạp báo hiệu theo tim đích giữ bảng, bỏ liên kết tim nguồn; nạp nguyên dự án giữ liên kết.
- Giữ đủ mặt biển/biển phụ, tim riêng theo đoạn, dấu chấm tâm X và DIM tại trụ gần tim nhất.
- Filebaohieu.bht người dùng gửi có 179 hồ sơ, 1 tim và 0 đoạn. Cần nạp bảng vào DWG trước khi xuất hoặc dùng gói bổ sung bảng được bàn giao riêng.

## Cấu trúc

| Thư mục | Vai trò |
|---|---|
| src/lisp/modules | 12 module AutoLISP: điểm, hồ sơ, tuyến, ký hiệu, API |
| src/lisp/BHT-1.1.5.lsp | Loader mã nguồn |
| src/dotnet/BHT.Core | Mô hình, quy tắc nghiệp vụ; không phụ thuộc AutoCAD |
| src/dotnet/BHT.Bridge | Đọc/ghi DWG, thư viện block, điều phối lệnh và gọi Lisp |
| src/dotnet/BHT.Palette | Giao diện WinForms/Palette của AutoCAD |
| tests | Kiểm tra Core, tích hợp AutoCAD, bộ cài |
| scripts, packaging | Build, phiên bản, FAS, bảo vệ DLL, đóng gói/cài đặt |
| docs | Hướng dẫn, hợp đồng dữ liệu, lịch sử phát hành và QA |

## Build

Windows, .NET Framework 4.8; plugin cần các DLL tham chiếu từ AutoCAD cài hợp lệ. Tích hợp hiện được kiểm tra với AutoCAD 2024 x64.

Chỉ build/kiểm tra Core, không cần AutoCAD:

    ./scripts/check-version.ps1
    ./scripts/build.ps1 -CoreOnly -UseCsc -Test -OutDir 'build/core'

Build plugin trên máy có AutoCAD:

    ./scripts/build.ps1 -AcadDir 'D:\AutoCAD 2024' -UseCsc -Test -OutDir 'build/v1.0.36/bin'

Không có DLL AutoCAD thì script có thể bỏ qua plugin; đọc log để phân biệt build Core và build đầy đủ. Không NETLOAD DLL mới đè lên phiên CAD đã nạp DLL cũ; đóng CAD và cài lại hoặc dùng Core Console cô lập để thử.

## Tài nguyên và bộ cài

Thư mục làm việc cục bộ có tài nguyên phục vụ đóng gói đầy đủ. Khi công bố mã nguồn, phải loại DLL Autodesk, DWG/phông bên thứ ba chưa rõ quyền phân phối, dữ liệu khảo sát và ảnh thực tế. Gói mã nguồn công khai đủ để đọc mã và chạy CoreTests; tái tạo bộ cài đầy đủ cần tài nguyên cục bộ hợp lệ.

Xem [thư viện cục bộ](docs/ADSCIVIL_LIBRARY.md), [đóng gói bảo vệ](docs/PROTECTED_BUILD.md), [cài đặt](packaging/README_INSTALL.md), [hợp đồng dữ liệu](docs/DATA_CONTRACT.md) và [hướng dẫn sử dụng](docs/HUONG_DAN.md).

Bản kiểm tra 1.1.4 đóng gói FAS/DLL trong BHT-1.1.5.zip và BHT-Setup-1.1.5.exe; hướng dẫn cài/kiểm tra nằm trong ghi chú phiên bản. Source hiện là Git checkout cục bộ trên nhánh development/incremental-confirmation. Tài nguyên bên thứ ba tiếp tục tuân theo giấy phép của chủ sở hữu.

Bản 1.0.24: bảng chọn đoạn/gói có đối chiếu phía đường; tự cập nhật phân đoạn khi tính theo tuyến; I.449 hai dòng. Xem tài liệu phiên bản đó trong [kho tài liệu cũ](docs/LICH_SU_TAI_LIEU.md).
