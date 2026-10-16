# Milestone 1 — Luồng dữ liệu và phạm vi bảo vệ

Ngày 15/10/2026, source 1.0.31. Đây là bản đồ triển khai hiện có, chưa phải kiến trúc dự án có revision theo đối tượng.

## Các đường xử lý đã rà soát

| Thao tác | Điểm vào và nơi ghi | Bảo vệ liên quan |
|---|---|---|
| Tạo hồ sơ | 04-objects.lsp, ObjectLogic, BhtDataService → BhtStore | Kiểm tra mã, liên kết điểm, ghi OBJ qua kho dữ liệu. |
| Gán đoạn | 07-segments.lsp, SegmentStore | Kiểm tra khoảng/phía; doan và goi vẫn được bảo vệ trên hồ sơ khóa. |
| Tính lý trình | progress-plan → station-object-route / seg-station-record | Chỉ lập cập nhật cho hồ sơ chưa khóa; kiểm tra tim và ngữ cảnh hồ sơ khóa theo chế độ đọc. |
| Áp dụng | progress-apply → kh-display-transaction → progress-write | Tái lập phương án để chặn dữ liệu thay đổi sau xem trước; phạm vi nhãn là danh sách ID cập nhật. |
| Xác nhận | progress-finish, gọi trong kh-display-transaction | Kiểm tra lại kết quả phụ thuộc; chỉ ghi pending, không ghi lại hồ sơ khóa. |
| Khóa dữ liệu | progress-guard và BhtStore.Write/Delete | Chuẩn hóa khóa kho sang chữ hoa; bảo vệ dữ liệu khảo sát, kết quả, đoạn/gói và tuyến liên quan. |
| Mở lại | progress-reopen | Luồng mở lại cả đoạn cũ vẫn tồn tại; bổ sung không dùng luồng này. |
| Xuất .bht | ExportSelection → ValidateConfirmedPoints → Copy → ProjectPackage | Đối chiếu điểm/tim, từ chối ghi đè, tạo DWG tạm, giữ cấu trúc ZIP. |
| Nạp kế thừa | ImportRolling → PrepareRolling → CopyTransaction | Kiểm tra nguồn; chuẩn hóa tên gói có bằng chứng; đối chiếu trước khi ghi; giao dịch đích có Undo recording. |
| So sánh xung đột | CheckRollingConflicts và ProgressPolicy.RollingConflicts | Cùng danh sách trường dùng ở bước trước nạp và bước thực thi; không tự giải quyết hai kết quả xác nhận khác nhau. |
| Ký hiệu và nhãn | symbol-sync-ex, kh-station-label-refresh, 08-signs.lsp | Không truyền phạm vi nil khi không có cập nhật; rollback chỉ ghi lại bản ghi khác snapshot. |
| Lưu/mở lại | NOD / XRecord / XData, Database.SaveAs / ReadDwgFile | Kiểm thử canonical và mở lại bằng phiên Core Console riêng. |

Các nơi ghi ngoài nhóm progress gồm chỉnh hồ sơ/điểm/ảnh trong BhtDataService, SegmentStore, sửa tuyến/mốc và cập nhật trình bày. Kho Lisp/.NET vẫn là chốt bảo vệ; không tắt toàn cục progress-guard. Di chuyển POINT/Polyline bằng lệnh CAD không đi qua kho, nên trước tính/xuất/nạp phải đối chiếu thực thể với snapshot.

## Chính sách trong Milestone 1

- `ProtectedFields` và progress-guard đồng bộ thêm so_mat, sign_content, station_point_method. Danh sách kết quả riêng giữ doan/goi, loại route_id có thể đổi tên khi chuyển DWG.
- `ResultKey` dùng canonical record, tránh ghép chuỗi mơ hồ. `SameSurvey` và `RollingConflicts` dùng SurveyComparison cho nội dung tương đương.
- `PackageAliasPolicy` chỉ nhận GOI01 ↔ GOI_01 khi toàn bộ tập đoạn, tên, khoảng và phía phù hợp; chuẩn hóa bản ghi nguồn đã tách trong bộ nhớ sang cách viết của đích. Không sửa tệp nguồn hoặc ghi lại OBJ khóa ở đích.
- Tim ở đích chỉ được tái dùng khi khai báo và hình học kiểm tra khớp; nếu không, chặn việc nhận kết quả cho đoạn đã gắn tim khác.
- Xác nhận mới bổ sung progress_context và progress_route_state. Đây là bằng chứng ngữ cảnh, không phải revision hay bằng chứng danh tính dự án.
- Giao dịch nạp tạm bật native Undo recording nếu database tắt, rồi trả về chế độ cũ. Kiểm thử lỗi sau xóa nhãn chứng minh khôi phục cả bản ghi và nhãn.

## Giới hạn và bước tiếp theo

Hình học tuyến hiện dùng chiều dài, trạng thái đóng và chín mẫu hình học theo cơ chế hiện có. Đây không phải hash toàn bộ định nghĩa đường cong; thay đổi đặc biệt nằm giữa mẫu có thể không được phát hiện. Cần đánh giá dấu vân tay hình học đầy đủ trước khi coi đây là hệ thống revision hình học.

Mốc tự động ở hồ sơ cũ không có snapshot ngữ cảnh bị chặn khi không đủ bằng chứng. Bản 1.0.31 chưa gắn project_id/object_guid và chưa giải quyết hai dự án trùng object_id. Vì vậy nạp tích lũy chỉ dùng với cùng hồ sơ dự án đã đối chiếu.

Milestone 2 cần tách đường ghi hiệu chỉnh với quyền giới hạn theo OBJ, snapshot trước/sau, lý do, xác nhận và lịch sử chỉ ghi thêm trong cùng giao dịch. Cần thiết kế danh tính/migration trước khi sửa hồ sơ khóa; không mở rộng ngoại lệ progress-guard để làm nhanh.
