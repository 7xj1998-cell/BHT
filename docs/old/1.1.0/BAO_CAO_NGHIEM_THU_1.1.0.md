# BHT 1.1.0 - Báo cáo kiểm chứng bản bàn giao thử

Ngày 15/10/2026. Phiên bản trước: 1.0.36; phiên bản mới: 1.1.0. Đây là build kiểm tra cục bộ, chưa nghiệm thu GUI hoặc toàn bộ lộ trình 15 đoạn. Không push GitHub, không commit thay công việc đang dở.

## Baseline và phạm vi

Git root: D:/lisp/BHT/sharing-0654/source. Nhánh development/incremental-confirmation. Giữ nguyên các thay đổi chưa commit từ M1-M5. Snapshot trước sửa: D:/lisp/BHT/audit-1.1.0/baseline-1.0.36-source.zip, SHA256 6F91334AF41817A8161F77E7A1C49579AB5F4C7E86FA140EAC9C7FC82AE71DB2. Danh sách Git trước sửa: audit-baseline-1.1.0-status.txt ở thư mục BHT.

D13 đầu vào là DWG đã nạp hồ sơ trong New/05 BV KS - BOT/Doan13(Km42+143_Km42+931), dung lượng 3.223.675 byte. D13 thô trước đó chưa có tim/hồ sơ không được tính là tái hiện lỗi bố trí. D10 dùng bản đã có 179 hồ sơ/801 điểm. Mỗi runner sao chép đầu vào và kiểm tra hash nguồn; không ghi DWG khảo sát gốc.

Đợt này sửa báo cáo, xem trước, chọn điểm đầu, hướng nhãn, phạm vi và tốc độ bố trí. Giữ nền tảng milestone hiện có, không mở rộng merge tùy ý hoặc tự sửa revision cũ.

## Nguyên nhân và hành vi trước/sau

| Vấn đề | Nguyên nhân đã xác minh | Sửa và hành vi mới |
|---|---|---|
| Không đọc đủ cọc | Bộ đệm chỉ giữ 40 thông báo cuối | Giữ đủ trong giới hạn công khai 20.000 mục/8 triệu ký tự; vượt giới hạn có cảnh báo |
| Bảng chặn zoom/pan | Báo cáo và xem trước dùng hộp modal | ShowModelessDialog; token gắn tài liệu, kiểm tra lại trước apply; phương án lỗi thời không ghi |
| Điểm đầu sai không báo | Chiếu điểm xa lên tim và nhận chiều không còn chiều dài | Từ chối lệch trên 1 m, chiều đi ra ngoài; chặn đổi hệ khoảng cách khi đã có mốc |
| Nhãn ngược chiều tuyến | Chọn nhánh góc dễ đọc theo trục WCS | Dùng chiều tăng Km và tiếp tuyến cục bộ; thiếu căn cứ thì giữ nhãn cũ/cảnh báo |
| Lag cập nhật hàng loạt | Quét thực thể lặp cho từng OBJ và từng lượt nhãn | Chỉ mục dùng trong một thao tác; giảm quét lại, chỉ xử lý đối tượng chưa đủ khoảng lùi |
| D13 nhích nhiều lần rồi lỗi | Nhãn giữa đường dẫn dịch ít hơn INSERT; dựng nhãn thay đổi bao hình; giới hạn lượt hết nhưng lỗi mất nội dung | Bù đáp ứng dịch chuyển, đo lại bao hình; giữ khoảng cách tối thiểu khi chạy lặp; lỗi chứa OBJ và nguyên nhân |
| Cọc ngoài phạm vi bị xử lý | Vòng bố trí cọc duyệt toàn DWG | Lọc phạm vi chiếu và tim đã gắn khi bố trí theo tim |

D13 có ba biển OBJ-000087/088/090 và ba cọc H OBJ-000089/098/175 cùng tuyến. Test ban đầu chỉ loại ba biển khỏi tập “ngoài phạm vi” nên báo sai khi ba cọc cùng tuyến xoay theo tim. Đã đối chiếu route_id/doan từng cọc, sửa định nghĩa phạm vi fixture và giữ kiểm tra nguyên trạng mọi ký hiệu ngoài sáu hồ sơ này. Không sửa sản phẩm để né lỗi test.

## File và hàm chính

- 01-foundation.lsp: screen-message-add; 12-api.lsp: api-messages, api-progress-preview-apply, đăng ký API.
- 06-routes.lsp: route-start-error, route-set-start-dir, route-pick-start; không đổi dữ liệu RTK hoặc công thức lý trình đã xác nhận.
- 07-segments.lsp: progress-preview-open, progress-preview-apply, BHTXULYDOAN; giữ kiểm tra phương án và nhóm Undo.
- 08-signs.lsp: kh-caption-route-angle, kh-caption-direction-clear-p, kh-caption-angle, kh-caption-leader-layout, kh-text-angle-tag, kh-txt-state, symbol-sync-ex, kh-road-target, kh-layout-clearance, kh-marker-place-all, kh-display-transaction.
- BHT.Palette/ReportDialog.cs, BhtPaletteControl.cs, ProgressForm.cs: báo cáo đọc đủ, cửa sổ modeless, bảng tự giãn và apply qua dispatcher.
- tests/IntegrationTests: bộ REGRESSION110, CAPTION110, BULK110, REOPEN110, PERF110, Report110Probe; sửa runner signs/lights để kiểm soát timeout và chỉ dừng crash reporter của bài thử.

Không tắt progress-guard toàn cục. Không đổi ZIP .bht. Metadata ANGLE là dữ liệu trình bày, không tăng Survey Revision. Không ghi revision khi kéo chuột hoặc mở Palette.

## Kết quả kiểm tra

Bằng chứng gốc tại D:/lisp/BHT/audit-1.1.0. Runtime kiểm tra cuối dùng FAS 1.1.0 và Core/Bridge qua Obfuscar 2.2.50; Palette giữ tên vì ràng buộc UI.

| Loại | Bài / bằng chứng | Kết quả |
|---|---|---|
| Build và unit | build-final-2.txt, protected-core.txt | 428 PASS, 0 FAIL trên cả DLL thường và protected |
| Core Console | protected-regression | 10 PASS: 180 dòng, chọn điểm đầu, no-write preview, stale/fresh/single-use |
| Core Console | protected-caption | 269 PASS: 128 tổ hợp hướng/phía/chế độ, tim cong, mốc đổi chiều, UCS/VIEW, nhãn tay và giữ dữ liệu |
| Core Console DWG thật | protected-d13-final | 10 PASS: bố trí, giữ khảo sát, phạm vi, ba phép đo khoảng lùi độc lập, lặp và rollback |
| Lưu/mở lại | reopen-serialized | Hai snapshot canonical trước/sau giống từng byte; số thực hình học chuẩn hóa 8 số lẻ, bỏ ename tham chiếu phiên |
| M1 | protected-incremental.txt | 31 Lisp + 29 .NET, Undo và mở lại DWG đạt |
| M2 | protected-correction132 | 50 PASS: snapshot, baseline, lịch sử, RTK relink, 49 hồ sơ giữ nguyên, Undo/REDO và lỗi giữa ghi |
| M2 với D10 | protected-real-d10 | 11 PASS: 178 hồ sơ khác/801 điểm/tim/đoạn/CAD ngoài phạm vi giữ nguyên; .bht và DWG roundtrip |
| M3 | protected-revision134 | 17 PASS: tổ tiên, nhánh, idempotence, rollback, tombstone |
| M4 | protected-presentation135 | 21 PASS: GLOBAL/LOCAL, nhãn/đường dẫn tay, giữ khảo sát và mở lại |
| M5 | protected-project136 | 20 PASS: kiểm tra DWG chỉ đọc, snapshot preview, trạng thái, phát hành R01/R02 |
| UI lập trình trong Core Console | protected-report110 | 23 PASS; ảnh report110.png/preview110.png đã kiểm tra, chữ rõ và hàng nút đủ |
| UI lập trình | protected-grid133, protected-palette-final.txt | 101 kiểm tra bảng + 3.102 kiểm tra layout đạt; không thay DPI/GUI thật |
| API tỷ lệ | protected-display127.txt | 9 PASS qua Application.Invoke; fixture SCALE652 đi kèm |
| Thư viện đèn | protected-lights.txt | Các bài thư viện/đặt đèn/hướng đạt |
| Hiệu năng D10 | protected-perf-d10 | SYNC 36.282 ms/422 lượt quét; SCALE 5.312 ms/24 lượt quét; nguồn khảo sát giữ nguyên |
| GUI AutoCAD thật | Chưa hoàn tất | Công cụ đọc màn hình báo FrameArrived timed out; cần người dùng thử zoom/pan/DPI và xác nhận hướng đọc |

So sánh D10 1.0.36: SYNC 55.094 ms/1.014 lượt; SCALE 21.953 ms/502 lượt. Một số lần đo có tiến trình khác chạy cùng máy, nên thời gian chỉ tham khảo; số lượt quét phản ánh thay đổi thuật toán. D13 cũ mất 7.094 ms/184 lượt quét/10 refresh rồi rollback; mới 4.062 ms/62 lượt/4 refresh và thành công. Không diễn giải lần thất bại cũ thành benchmark thành công tương đương.

## Các kiểm tra chưa đạt và sai lệch môi trường

run_signs với thư viện ADS đi kèm còn 11 FAIL SIGN ở cả 1.0.36 và 1.1.0; danh sách hai bản khớp nhau. Giữ nguyên log ở signs-current và signs-baseline136, không đổi test thành PASS. Các tên gồm description-import-80, sync-creates-speed80, free-replace-route-straight, free-reset-to-route, label-exact-sign-rotation-with-waypoints, label-bottom-center-alignment, delineator-label-below-symbol-and-rotates, support-missing-measured-foot-warning, origin-at-G-with-no-leader-and-deduplicated-source, upgrade-replaces-old-geometry-preserves-fill-off, quick-auto-elbow-angle. Một số kỳ vọng phản ánh mẫu/cache cũ; chưa kết luận mọi lỗi đều do test. Bộ này chưa đạt và cần rà soát riêng. Khi chạy FAS cũ với fixture hiện tại còn có FAIL TCVN version-current vì fixture yêu cầu 1.1.0; không tính là hồi quy.

Nhánh kiểm tra native TDT mặc định của SignLibraryProbe gây lỗi native trước khi hoàn tất. Không dùng kết quả đó làm chứng nhận thư viện phát hành; lần ADS dùng thư viện đi kèm đã chạy đến cuối, vẫn giữ 11 FAIL nêu trên. Đã dừng đúng crash reporter từ tiến trình kiểm thử; không dừng CAD của người dùng.

PaletteLayoutProbe ban đầu yêu cầu chỉ cuộn dọc; test được cập nhật theo tính năng mới có tùy chọn cuộn ngang và kiểm tra bật/tắt xuống dòng. Đây là thay đổi yêu cầu UI, không bỏ kiểm tra dữ liệu. Test reopen ban đầu đưa snapshot 1,5 MB qua AutoLISP read và gặp string too long on input; đổi sang so sánh byte file ngoài Lisp, không nới sai số hoặc bỏ trường để qua test.

Core Console có cảnh báo không xóa được tệp tạm errno=13 sau QUIT ở một số lần; log assertion hoàn tất và mã thoát được kiểm tra riêng. Lỗi “License checkout timed out” của phiên GUI chưa được khắc phục trong đợt này. Không sửa dịch vụ/giấy phép hoặc tuyên bố lỗi này do BHT.

## Giới hạn, phục hồi và bước tiếp theo

Nhãn legacy chưa có góc tham chiếu không thể xác định chắc một lần ROTATE tay trước nâng cấp. Khi đổi mốc/đầu tuyến, cần quy trình đối chiếu có kiểm soát; chưa tự remap mốc. Bố trí khoảng lùi dùng bao hình bảo thủ, chưa xử lý tránh mọi va chạm ký hiệu. Thao tác CAD vẫn chạy đồng bộ; chưa có ghi nền an toàn.

Rollback đã chứng minh OBJ/META và thực thể trình bày trong các tình huống lỗi chủ động, không chứng minh mọi định nghĩa block hay mất điện/crash native. Bản phát hành và READ_ONLY_ARCHIVE chặn đường ghi BHT, không chặn lệnh CAD gốc. Chưa tự đồng bộ tệp DWG đóng; chưa hoàn tất UI đối chiếu legacy/chọn nhánh, hiệu chỉnh biển phức tạp, mốc Km/H tác động nhiều hồ sơ, LOCAL theo layout.

Hướng dẫn mới có bài P-T cho lỗi vừa báo và A-O cho milestone. Cần nghiệm thu trên bản sao D13/D10 với AutoCAD GUI hoạt động ổn định, rồi rà soát 11 test biển còn tồn tại trước khi coi bản kế tiếp là ổn định. Giữ bản BEFORE và backup 1.0.36; rollback bằng bản dữ liệu trước nâng cấp, không mở rồi lưu đè dữ liệu 1.1 bằng DLL cũ.

Hướng dẫn cũ chuyển vào docs/old, có journal SHA256 ở docs-archive.json ngoài source. Bộ cài và bản thử được đặt ở thư mục BHT; hash bộ cài nằm trong BHT-SHA256.txt. Trạng thái cài vào máy được ghi riêng trong BHT-HIEN-TAI.md, không suy ra từ việc tạo được EXE.
