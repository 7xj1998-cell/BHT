# BHT v1.0.30 — mã nguồn cho AutoCAD

BHT quản lý hiện trạng tuyến, biển báo, biển phụ, cọc tiêu, tuyến tham chiếu, lý trình và hồ sơ theo đoạn/gói thầu trong AutoCAD. Mã nguồn gồm AutoLISP và C# (.NET Framework 4.8).

## Trạng thái phiên bản

Đây là ảnh chụp mã nguồn đang phát triển 1.0.30. Bản phát hành đã kiểm chứng gần nhất là **1.0.29**. Số phiên bản 1.0.30 đã đồng bộ, nhưng các sửa đổi nhãn mới được yêu cầu chưa hoàn tất và chưa được nghiệm thu. Không coi kho này là bộ cài 1.0.30 đã phát hành.

Xem [lịch sử thay đổi](CHANGELOG.md), [ghi chú 1.0.29](docs/RELEASE_NOTES_1.0.29.md), [hướng dẫn sử dụng](docs/HUONG_DAN.md). Người phát triển bắt đầu với [AGENTS.md](AGENTS.md) và [bàn giao kỹ thuật](docs/AI_HANDOFF.md); thông tin trạng thái ở README này được ưu tiên khi tài liệu lịch sử khác phiên bản.

## Build và kiểm tra

Windows, PowerShell và .NET Framework 4.8. Build Core không cần AutoCAD:

```powershell
./scripts/check-version.ps1
./scripts/build.ps1 -CoreOnly -UseCsc -Test -OutDir build/core
```

Plugin Bridge/Palette cần DLL tham chiếu từ bản AutoCAD cài hợp lệ. Các thư viện hình học biển, bitmap và phông cục bộ không có trong kho công khai; cần cung cấp tài nguyên có quyền sử dụng trước khi build/đóng gói đầy đủ. Xem [thư viện cục bộ](docs/ADSCIVIL_LIBRARY.md) và [script build](scripts/build.ps1).

## Cấu trúc

- `src/lisp`: loader và 12 module nghiệp vụ AutoLISP.
- `src/dotnet`: Core, Bridge và Palette.
- `tests`: kiểm tra Core, tích hợp AutoCAD và bộ cài.
- `scripts`, `packaging`: build và đóng gói.
- `docs`: hợp đồng dữ liệu, hướng dẫn và lịch sử kiểm chứng.

CI chạy Core trên Windows. CI không kiểm chứng giao diện hoặc thao tác CAD thực tế. Một số tài liệu lịch sử tham chiếu đường dẫn/ảnh cục bộ không được công bố.

## Giấy phép và tài nguyên

Mã BHT được công bố theo [MIT](LICENSE) theo lựa chọn của chủ dự án. Xem [phạm vi giấy phép](COPYING_SCOPE.md). Kho này không chứa bản vẽ khảo sát, gói dữ liệu .bht, DLL Autodesk/TDT/ADSCivil, phông, ảnh chụp màn hình hoặc thư viện hình biển của nhà cung cấp.

Kho mã nguồn mới: https://github.com/7xj1998-cell/BHT

Bộ cài runtime như BHT-1.0.30.zip không phải gói mã nguồn và chưa được phát hành cho phiên bản này.
