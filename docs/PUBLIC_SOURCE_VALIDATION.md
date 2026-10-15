# Kiểm tra gói mã nguồn công khai

Ngày: 2026-10-15. Phiên bản mã nguồn: 1.0.30 (đang phát triển).

- scripts/check-version.ps1: đạt.
- scripts/build.ps1 -CoreOnly -UseCsc -Test -OutDir build/core: 311 PASS, 0 FAIL.
- Không có DWG, .bht, DLL/EXE/FAS, phông, bitmap hoặc dữ liệu Excel trong danh sách Git.
- Quét các mẫu token GitHub/AWS và khóa riêng phổ biến: không phát hiện trong mã công bố. Đây không phải kiểm toán bảo mật đầy đủ.
- Chưa kiểm chứng plugin/Palette trên gói công khai; thiếu thư viện hình học/ảnh/phông cục bộ. CI chỉ chạy Core.
- Các sửa nhãn mới nhất được yêu cầu vẫn chưa hoàn tất. Xem README để phân biệt trạng thái phát triển với bản phát hành đã kiểm chứng 1.0.29.
- Kiểm tra script tăng phiên bản trên clone tạm: 1.0.30 → 1.0.31 giữ NuGet 1.0.3, check-version đạt. Clone kiểm tra không được phát hành.
