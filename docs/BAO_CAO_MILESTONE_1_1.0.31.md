# Báo cáo Milestone 1 — Safe Incremental Confirmation

Ngày 15/10/2026. Đã hoàn thành triển khai và các kiểm thử tự động nêu dưới đây. Bản phát triển cục bộ, chưa nghiệm thu AutoCAD GUI trên hồ sơ thực tế, chưa đóng gói bộ cài ổn định.

## Phiên bản và baseline

- Trước sửa: source 1.0.30; bản đã bàn giao gần nhất 1.0.29. Sau sửa: 1.0.31, assembly 1.0.31.0, đồng bộ bằng scripts/set-version.ps1 và kiểm tra bằng check-version.ps1.
- Source thực tế: `D:/lisp/BHT/sharing-0654/source`. Ban đầu thư mục này chưa có .git; đối chiếu mã tracked với checkout cục bộ tại commit 3084a4c rồi tạo Git tại source, nhánh `development/incremental-confirmation`. Không push, không thay nhánh main của checkout cũ, chưa commit thay đổi.
- Khác biệt có sẵn: .gitignore/README, thiếu COPYING_SCOPE/LICENSE/PUBLIC_SOURCE_VALIDATION so với checkout tham chiếu, cùng tài nguyên/HTML/RELEASE_STATE chưa tracked. Đã giữ nguyên khác biệt này; trạng thái và patch trước sửa ở `D:/lisp/BHT/audit-1.0.31/baseline-status.txt` và baseline.patch.
- Baseline build đạt 311 CoreTests. Dùng dữ liệu giả lập; không sửa hồ sơ thực tế để thử.

## Vấn đề và nguyên nhân gốc

| Vấn đề trước sửa | Nguyên nhân | Hành vi sau sửa |
|---|---|---|
| Không tính thêm OBJ026 vào đoạn có 25 hồ sơ khóa | progress-plan chặn cả đoạn trước khi tới logic bỏ qua từng hồ sơ khóa | Bỏ chặn cả đoạn; kiểm tra ngữ cảnh cũ, chỉ tính pending đủ điều kiện. |
| Xác nhận ghi lại hồ sơ đã khóa | progress-finish duyệt mọi hồ sơ của đoạn | Chỉ xác nhận pending sau kiểm tra kết quả; toàn bộ khóa thì NO_CHANGE. |
| Có thể áp dụng phương án đã cũ | Chưa đối chiếu snapshot trước lúc ghi | Tái lập phương án; dữ liệu đổi thì yêu cầu xem trước lại. |
| Cập nhật nhãn ngoài phạm vi khi danh sách trống | nil có nghĩa là mọi hồ sơ trong luồng nhãn hiện có | Trả NO_CHANGE trước khi gọi cập nhật nhãn. |
| Đối chiếu gói/tim chặn trường hợp kế thừa hợp lệ | So sánh tên gói thô và tim theo định danh bản vẽ; chính sách trước/trong nạp tách nhau | Chuẩn hóa có bằng chứng và dùng quy tắc chung; tái dùng tim khi đã đối chiếu. |
| Trường dữ liệu khóa chưa được bảo vệ đồng nhất | Thiếu sign_content/so_mat/station_point_method, phân biệt hoa thường tại cửa ghi | Thêm trường bảo vệ đồng bộ Lisp/C#, chuẩn hóa khóa kho. |
| Nhập hồ sơ mới nhưng dùng toàn bộ điểm cũ có lỗi native | Gọi WblockCloneObjects với danh sách rỗng | Bỏ qua clone khi không có thực thể cần thêm. |
| Abort không phục hồi dữ liệu ở side database | Native Undo recording mặc định tắt | Tạm bật trước giao dịch nạp/sao, trả chế độ cũ trong finally. |
| Snapshot NaN lọt kiểm tra xuất/nạp | TryParse thành công nhưng phép so sánh chênh lệch với NaN không báo khác | Yêu cầu số hữu hạn cho snapshot điểm và mẫu tuyến. |

## File và hàm thay đổi

- `07-segments.lsp`: progress-plan/write/apply/finish; thêm progress-context, progress-route-state, progress-point-snapshot, progress-check-locked. Không dùng progress-reopen cho tính bổ sung.
- `04-objects.lsp`: progress-guard; `08-signs.lsp`: kh-display-transaction chỉ phục hồi bản ghi thay đổi.
- `ProgressPolicy.cs`: ProtectedFields, ResultFields, ResultKey, RollingConflicts, SameRoute, RouteState. `PackageAliasPolicy.cs`: chính sách cùng gói có điều kiện.
- `BhtStore.cs`: Write/Delete; `ImportComparison.cs`: CheckRollingConflicts; `ProgressTransfer.cs`: PrepareRolling, RollingRouteBindings, RouteGeometryMatches, ValidateConfirmedPoints/TryFiniteNumber.
- `ProjectTransfer.cs`: Copy/CopyTransaction, kiểm tra danh sách clone, ResetStation. `ProgressForm.cs`: điều kiện nút và thông báo cho tính/xác nhận bổ sung.
- Kiểm thử: CoreTests/Program.cs, Incremental131Probe.cs, test_INCREMENTAL131.lsp, run_incremental131.ps1; cập nhật kỳ vọng cũ trong Progress125 theo hành vi mới, không bỏ các kiểm tra xung đột.
- VERSION, loader, AssemblyInfo, manifest, tài liệu và tham chiếu phiên bản được đồng bộ bằng script hiện có.

## Bằng chứng kiểm thử

| Lớp kiểm thử | Kết quả cuối | Phạm vi |
|---|---|---|
| Static review | Đã rà luồng ghi/guard/import/rollback; git diff --check đạt | Không thay cho chạy AutoCAD. |
| Build .NET Framework 4.8 | Core, Bridge, Palette thành công; check-version đạt | Dùng tham chiếu AutoCAD 2024 trên máy. |
| Unit test CoreTests | 378 PASS, 0 FAIL | Chính sách nội dung/kết quả, gói tương đương và hồi quy Core hiện có. |
| AutoCAD Core Console Incremental131 Lisp | 31 PASS, 0 FAIL | 27 assertion Lisp + 4 assertion lệnh công khai/Undo; 25+1, no-op, lỗi giữa lần ghi, điểm/tim/mốc đổi, nhãn/block chỉnh tay. |
| AutoCAD Core Console Incremental131 .NET/UI | 29 PASS, 0 FAIL | Canonical export/save/reopen, guard, package alias, import idempotence, route reuse, lỗi sau xóa nhãn, NaN và điều kiện nút. |
| Core Console mở lại DWG ở phiên riêng | PASS | Tính lặp không ghi thay đổi sau lưu/mở lại. |
| Core Console Progress125 | 13 Lisp + 27 .NET + 7 stage PASS, 0 FAIL; kiểm tra nhãn stage đạt | Chuỗi xử lý hai đoạn, nạp/xuất .bht, UI và Undo liên quan. |
| PaletteLayout trong Core Console | 3.062 kiểm tra PASS | WinForms và luồng editor qua probe; không phải nghiệm thu AutoCAD GUI thực tế. |
| AutoCAD GUI thực tế / dữ liệu DT.830 | Chưa chạy trong đợt này | Chưa xác nhận thao tác người dùng, plugin TDT có khóa hoặc toàn bộ hồ sơ 15 đoạn. |

Log cuối ở `D:/lisp/BHT/audit-1.0.31`: build-final.txt, incremental-final.txt, progress-final.txt, palette-final.txt. Log chi tiết và mã/kịch bản thử phát sinh được nén có đối chiếu SHA256 ở `D:/lisp/BHT/old/local-20261015/build-evidence.zip`; manifest đi kèm ghi từng tệp. Mã kiểm thử chính vẫn ở tests.

Đã chạy kiểm thử trước sửa: baseline 1.0.30 thất bại ca lập phương án 25+1, giữ nguyên snapshot khi chỉ xem. Trong lần rà cuối, ba ca NaN thất bại trước bổ sung TryFiniteNumber (nonfinite-before.txt) rồi đạt sau sửa. Các lần phát triển trung gian phát hiện lỗi clone rỗng, rollback side database và fixture Undo bị gom nhóm; đã sửa tương ứng và chạy lại. Không tính các lần đó là đạt.

## Toàn vẹn dữ liệu và tương thích

Đã đối chiếu snapshot/canonical bản ghi, RTK, hình học và thực thể trình bày trong các ca thử; 25 hồ sơ cũ không bị ghi lại. Thử lỗi giữa ghi xác nhận/phép tính và lỗi nhập sau xóa nhãn đều khôi phục trạng thái đã so sánh. Không chỉ dựa trên dòng thông báo thành công.

.bht giữ cấu trúc ZIP hiện có. Hai trường snapshot mới ở OBJ/XRecord không phải revision; hồ sơ legacy không được tự điền. Legacy có mốc khai báo và bằng chứng đủ được thử tính bổ sung; legacy thiếu bằng chứng bị chặn. Chưa thử toàn bộ mọi file .bht lịch sử và mọi phiên bản phần mềm cũ.

Ma trận mục tiêu: A đạt trong dữ liệu giả lập; J đạt phạm vi đổi cách viết gói có bằng chứng; M/N đạt các ca rollback/nạp lặp được nêu. O được kiểm chứng ở fixture legacy và đọc/ghi .bht hiện hành, chưa bao phủ toàn bộ kho lịch sử. B/C/E/F/G/H/I/K/L cần nền tảng danh tính, hiệu chỉnh và revision ở các milestone sau. D mới có kiểm thử giữ trình bày cũ khi bổ sung/nạp; chưa có workflow Display Revision.

## Rủi ro còn lại và Milestone 2

- Hình học dùng chín mẫu và chiều dài theo cơ chế hiện có; chưa bảo đảm phát hiện mọi thay đổi nằm giữa mẫu.
- Không có project_id/object_guid, revision ancestry hay tombstone. Không dùng bản này để tự hợp nhất hai dự án trùng mã hoặc tự chọn bản mới theo thời gian.
- Chưa có hiệu chỉnh một đối tượng khóa, GLOBAL/LOCAL hay trạng thái đồng bộ bình đồ; không nghiệm thu toàn bộ lộ trình bằng kết quả Milestone 1.
- Chưa chạy bộ cài/FAS/DLL bảo vệ vì chưa phát hành. Các lỗi cũ ngoài phạm vi, gồm bộ run_signs rộng từng có thất bại được ghi trong tài liệu lịch sử, chưa được tuyên bố đã giải quyết.
- Milestone 2 chưa triển khai trong đợt này. Bước tiếp theo là thiết kế baseline danh tính và đường ghi hiệu chỉnh theo đối tượng, lịch sử chỉ ghi thêm, kiểm thử nguyên tử bản ghi/lịch sử/CAD, rồi mới bổ sung UI trước/sau.

## Hướng dẫn và quản lý tệp

Hướng dẫn mới: [RELEASE_NOTES_1.0.31.md](old/RELEASE_NOTES_1.0.31.md). Tài liệu tổng: [HUONG_DAN.md](HUONG_DAN.md). Bản đồ kỹ thuật: [MILESTONE_1_ARCHITECTURE.md](MILESTONE_1_ARCHITECTURE.md). Tài liệu cũ chuyển vào docs/old; HTML cũ nén vào old cấp dự án. [Mục lục](LICH_SU_TAI_LIEU.md) chỉ vị trí tra cứu.

Build để dùng cục bộ ở `build/v1.0.31/bin`; giữ ba DLL BHT và CoreTests.exe. Xóa bản build/profile/test DWG tạo lại được sau khi lưu bằng chứng; giữ nguồn hồi quy. Nhật ký cleanup-actions.jsonl, cleanup-result.json và protected-survey-before.csv ở audit-1.0.31 ghi phạm vi, dung lượng và đối chiếu SHA256 dữ liệu thật.

Kết quả dọn: đã xóa 139.129 tệp sinh từ build/kiểm thử, tổng 18.360.476.840 byte (17,10 GiB); kho bằng chứng/manifest mới khoảng 15 MB. Đã chuyển 178 tài liệu Markdown vào docs/old và nén 35 HTML cũ. Bộ cài 1.0.13–1.0.29 cùng audit/checkouts cũ nằm trong old/local-20261015. Đối chiếu SHA256 634 tệp khảo sát gốc trước/sau không thay đổi. Không xóa mã test hồi quy hoặc tài nguyên thư viện nguồn.
