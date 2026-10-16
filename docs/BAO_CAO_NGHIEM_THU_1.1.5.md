# BHT 1.1.5 - Báo cáo kiểm chứng layer tuyến

Ngày 16/10/2026. Phiên bản trước: 1.1.4. Phiên bản mới: 1.1.5. Nhánh development/incremental-confirmation; build cục bộ, không push. Baseline và thay đổi chưa commit được giữ tại D:/lisp/BHT/audit-1.1.5/baseline.

## Nguyên nhân và thay đổi

Tim BHT cùng layer BHT_TUYEN_TDT, còn quá trình clone dùng MangleName để cô lập tài nguyên. Nạp lặp qua các bản vẽ làm tên layer tích lũy dấu $0$. Không thể thay toàn bộ MangleName thành Ignore vì block/style cùng tên có thể khác nội dung.

- RouteLayers.cs: Name, Preview, Apply, ApplyOne, WriteLisp. Đặt layer theo mã tuyến cuối cùng; transaction và Undo; preflight layer khóa, thuộc tính xung đột, viewport, hình học thiếu và bản vẽ phát hành. Chỉ nhận đường AutoCAD thường đã biết, không sửa đối tượng TDT tùy biến.
- 06-routes.lsp: bht:route-write gọi BHTROUTEWRITE sau progress-guard; ghi bản ghi và layer trong cùng giao dịch. Không tắt bảo vệ hồ sơ.
- ProjectTransfer.cs: sau remap handle và mã tuyến, gán layer semantic trong cùng giao dịch nhập; giữ cơ chế clone tài nguyên.
- RouteDataControl.cs: thêm Đặt layer theo tên tuyến, xem trước và xác nhận; không tự chạy khi mở DWG/Palette.

Trước: nhiều tim dùng tên layer dài có dấu $0$. Sau: tim DOAN_11 ở BHT_TUYEN_DOAN_11. Không sửa mã tuyến, OBJ, RTK, lý trình, mốc, hình học hoặc ZIP .bht. Layer cũ vẫn tồn tại nếu chưa được dọn thủ công; không xóa thực thể ngoài phạm vi.

## Bằng chứng kiểm thử

| Mức | Kết quả | Bằng chứng |
| --- | --- | --- |
| Tái hiện baseline 1.1.4 | 3/3 kiểm tra tên layer thất bại đúng lỗi | audit-1.1.5/baseline-layer |
| Unit | 443 PASS, 0 FAIL | build/v1.1.5/bin-private và log build |
| Core Console layer | 4 Lisp + 18 .NET + 1 Undo PASS | audit-1.1.5/layers-release |
| Core Console nạp/khóa | 13 Lisp + 28 .NET + 7 stage, nhãn kế thừa PASS | audit-1.1.5/progress-release.log |
| Core Console hiệu chỉnh | 50 PASS | audit-1.1.5/correction-release |
| Core Console bổ sung | 31 Lisp + 29 .NET, reopen PASS | audit-1.1.5/incremental-protected.log |
| Core Console phạm vi | 28 + reopen PASS | audit-1.1.5/scope-protected |
| Kiểm tra layout tự động | 3.142 PASS | build/v1.1.5/bin-protected/ui-preview/result.txt |
| GUI chuột thực tế | Chưa nghiệm thu | Cần thực hiện hướng dẫn trên bản sao |

Đối chiếu canonical toàn kho BHT, RTK, hình học, XData trước/sau trên bản sao DOAN15: 9 tim chuyển layer; không đổi dữ liệu khảo sát. Đã lưu/mở lại DWG, xuất/nhập .bht, nạp lặp, thử layer đích xung đột và Undo. SHA256 nguồn: 7D510BF3B9C37350CC770E446DE61B42124676CFE0F10819A3D19A60BC7FFFDC.

Các lỗi phát hiện trong quá trình phát triển đã giữ log: eNoDatabase khi gán thuộc tính trước khi thêm layer; ePlotStyleInColorDependentMode khi gán plot style trong CTB; Progress125 bị chặn vì tham chiếu Line cũ. Đã sửa nguyên nhân, bổ sung hồi quy Line và chạy lại Progress125 đạt. Không sửa test để bỏ yêu cầu tương thích. Khi chọn lại tim, chuyển bước vô hiệu hóa kết quả chưa khóa xuống sau khi ghi tuyến/layer thành công; kiểm thử xung đột layer giữ nguyên kết quả cũ đạt.

## Rủi ro và phần còn lại

Layer riêng theo viewport bị chặn có chủ đích; chưa tự chuyển override/freeze giữa các layer. Giữ layer cũ, không làm sạch toàn bộ Layer Manager. Tên tuyến cũ như TUYEN1_GOI1 giữ nguyên, không tự suy ra đoạn. Chưa xử lý mọi layer RTK/nhãn/biển theo tên tuyến.

11 lỗi signs legacy từ các đợt trước chưa được sửa, không coi bản này hoàn tất toàn bộ roadmap. Những kiểm tra M1/scope/layout chạy trước các thay đổi cuối về nhận diện Line và thứ tự chọn lại tim. Core/Palette không đổi; layer/Progress125/M2 được chạy lại với runtime cuối, gồm ca chọn lại tim thất bại giữ kết quả cũ. Bước tiếp theo: kiểm tra thao tác thực tế theo hướng dẫn, sau đó mới mở rộng chính sách layer cho các nhóm thực thể khác nếu cần.

Hướng dẫn mới: HUONG_DAN_HIEU_CHINH_1.1.5.md và HTML. Tài liệu 1.1.4 lưu tại docs/old/1.1.4, đối chiếu hash trong MANIFEST.json. Không sửa hồ sơ nguồn của người dùng.
