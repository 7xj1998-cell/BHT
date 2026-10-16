# Báo cáo kiểm tra cục bộ BHT 1.0.32

Ngày 15/10/2026. Phiên bản trước 1.0.31; phiên bản mới 1.0.32. Đây là mốc triển khai luồng hiệu chỉnh của Milestone 2 để kiểm tra trên bản sao, **chưa nghiệm thu toàn bộ Milestone 2 hoặc AutoCAD GUI**. Không push GitHub.

## Vấn đề và nguyên nhân

Hồ sơ chỉ có progress_locked; sửa kết quả thường phải mở lại đoạn, chưa giữ lịch sử theo đối tượng. Generic writer C#/Lisp chưa biết revision nên có thể mở lại hoặc ghi trường ngoài danh sách bảo vệ mà không lưu lịch sử. Rolling import cũ không có ancestry nên không đủ căn cứ xử lý phiên bản mới.

## Đã triển khai

- Palette: hiệu chỉnh riêng thuộc tính, liên kết RTK, tính lại lý trình, đổi phân đoạn; trước/sau, lý do, người sửa, duyệt baseline và chuyển đoạn, xem lịch sử.
- Core: snapshot giữ thứ tự/khóa lặp, SHA256, UUID, cha, counters theo khảo sát/phân đoạn; baseline cũ ghi rõ LEGACY_UNVERIFIED.
- Bridge/Lisp: writer riêng đúng đối tượng; kiểm tra tim/mốc/điểm/phạm vi, ngăn xem trước lỗi thời và ghi ngoài quy trình. Tính lại riêng khi đổi số trụ ảnh hưởng cách chọn điểm.
- Native Undo + journal cho dữ liệu/lịch sử/ký hiệu, đối chiếu rollback; giữ vị trí ký hiệu chỉnh tay và thực thể ngoài phạm vi.
- Full export/import giữ lịch sử, không đổi ZIP; rolling/survey-only có lịch sử bị chặn cả preflight lẫn thực thi. Mở lại cả đoạn có lịch sử bị chặn.

Trước: sửa mã OBJ001 đã khóa phải mở lại đoạn, không có bản trước. Sau: sửa riêng OBJ001, lưu bản cũ và revision mới; 49 hồ sơ khác giữ nguyên. Đổi liên kết không sửa RTK gốc; gói lấy theo đoạn được duyệt.

## File và hàm chính

| File | Trách nhiệm |
|---|---|
| src/dotnet/BHT.Core/ObjectRevision.cs | Patch theo loại, Snapshot/Restore/Hash, Next/History |
| src/dotnet/BHT.Bridge/ObjectCorrection.cs | Preview/Confirm, CommitCorrection/ValidateHistory, journal/Undo |
| src/dotnet/BHT.Bridge/BhtStore.cs | Guard generic writer, private WriteRaw |
| src/lisp/modules/04-objects.lsp | revision-governed, progress-guard |
| src/lisp/modules/07-segments.lsp | correction-evaluate, chặn reopen có lịch sử |
| src/lisp/modules/08-signs.lsp | scoped symbol-sync không dọn thực thể ngoài phạm vi |
| src/lisp/modules/12-api.lsp | evaluate/refresh/state API |
| src/dotnet/BHT.Bridge/ProjectTransfer.cs, ImportComparison.cs, ProgressTransfer.cs | preflight/ghi nhập thống nhất, lọc lịch sử khi xuất đoạn |
| src/dotnet/BHT.Palette/ObjectCorrectionForm.cs, TabObjectCorrection.cs, TabObjectsRoutes.cs | cửa sổ modeless, đối chiếu, lịch sử |
| tests/CoreTests/Revision132Tests.cs, tests/IntegrationTests/*132* | unit, integration, UI và bản sao New |
| scripts/coreconsole.ps1 và runner liên quan | chạy ẩn, timeout, dọn đúng crash reporter thuộc test |

## Kiểm thử đã chạy

| Mức kiểm tra | Kết quả |
|---|---|
| Static review | Rà luồng tạo/gán/tính/xác nhận/khóa/ghi/xuất/nạp, writer ngoài progress; giới hạn còn thiếu ghi riêng |
| Build C# 5 / .NET 4.8 | Core, Bridge, Palette và test build được với AutoCAD 2024 |
| CoreTests | 410 PASS, 0 FAIL |
| Correction132, Core Console | 50 PASS trên DLL/FAS protected cuối: 50 hồ sơ khóa, chỉ đối tượng chọn đổi; fault sau CAD; U/REDO; lịch sử, stale, guard C#/Lisp, RTK, chuyển đoạn, số trụ, export/import/reopen |
| Bản sao D10 trong New | 11 PASS trên runtime cuối: OBJ-000044 W.207b -> W.207c, 178 hồ sơ khác và CAD ngoài phạm vi không đổi; 801 điểm/ROUTE/SEG không đổi; fault rollback, baseline, lưu/mở lại, .bht |
| Correction UI tự động | 19 PASS: bốn loại, xác nhận bị khóa trước preview, nhập liệu/bảng/nút ở hai kích thước, không nhập đè lý trình |
| Incremental131 hồi quy | 31 Lisp/Undo + 29 .NET/UI PASS, mở lại DWG no-op PASS |
| Progress125 hồi quy | 13 Lisp + 27 .NET + 7 stage/nhãn PASS |
| PaletteLayout | 3.062 PASS |
| AutoCAD GUI thực tế | Chưa nghiệm thu tương tác modeless trong AutoCAD GUI. Core Console không cung cấp cửa sổ có thể điều khiển để kiểm tra bằng computer-use |

Mã test giữ trong source/tests; log, snapshot và hashes tại D:/lisp/BHT/audit-1.0.32, bằng chứng cũ được nén trong old khi dọn sản phẩm tạo lại được. Kết quả các lần chạy trước không được dùng thay kiểm tra runtime cuối.

## Lần thử thất bại và xử lý

1. Transaction .NET bao quanh Lisp tạo block gây eKeyNotFound; thử OpenCloseTransaction phát sinh Access Violation trong tiến trình cô lập. Đã loại đường xử lý đó, dùng nhóm native Undo riêng và journal. Runtime cuối vượt qua lỗi cố ý sau cập nhật CAD mà khôi phục snapshot đúng.
2. Một assertion ban đầu giả định station_point_id luôn chứa mã điểm; thuật toán cũ dùng REPRESENTATIVE cho một trụ. Test được sửa theo hợp đồng thật: pt, tọa độ tính, lý trình, offset, phía tuyến; không sửa thuật toán để ép assertion.
3. Bài đối chiếu D10 bổ sung gom toàn bản vẽ thành một chuỗi Lisp gây timeout. Chuyển trả danh sách snapshot từng thực thể; runtime cuối đạt kiểm tra giữ CAD ngoài phạm vi.
4. Thử nhìn cửa sổ từ Core Console không có cửa sổ targetable; đã dừng tiến trình kiểm tra. Không tính đây là GUI PASS.

## Giới hạn và rủi ro

Chưa có hiệu chỉnh mã nhiều mặt/nội dung XML/custom block; chưa sửa liên kết/lý trình cọc Km/H tác động mốc; chưa có quy trình đối chiếu legacy thiếu bằng chứng; chưa có GLOBAL/LOCAL, Display Revision cho CAD trực tiếp, ancestry merge, tombstone hoặc quản lý bản phát hành nhiều DWG. Vì vậy không coi toàn bộ Milestone 2-5 hoàn thành.

Journal phát hiện thao tác dang dở, nhưng chưa bảo đảm tự chữa DWG sau native crash/mất điện; phục hồi bằng bản sao. BHT cũ không hiểu guard revision, nên không dùng để chỉnh dữ liệu đã hiệu chỉnh. Bộ biển báo rộng từng có lỗi native/draw-order ở bản trước chưa được nghiệm thu lại toàn bộ trong đợt này.

## Hướng tiếp theo

Hoàn thiện các loại hiệu chỉnh phức tạp còn thiếu và nghiệm thu GUI trên bản sao trước khi gọi Milestone 2 ổn định. Sau đó triển khai Milestone 3 theo project_id/object_guid/quan hệ cha, cùng chính sách preflight và ghi nhập; không bỏ chốt bảo vệ rolling trước khi đủ test stale, nhánh xung đột, tombstone, trùng mã và nhập lặp.

[Hướng dẫn thao tác và phục hồi](HUONG_DAN_HIEU_CHINH_1.0.32.md) · [Kiến trúc](MILESTONE_2_ARCHITECTURE.md).
