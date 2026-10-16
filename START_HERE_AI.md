# BHT 1.1.5 — bắt đầu bàn giao cho AI

Đây là mã nguồn mở của build BHT **1.1.5** đã cài trên máy phát triển ngày 16/10/2026. Bản này vẫn là **bản thử nghiệm**, chưa nghiệm thu toàn bộ công trình 15 đoạn hoặc toàn bộ thao tác AutoCAD GUI.

Đọc [bộ tổng hợp liền mạch](handoff/1.1.5/BHT_AI_HANDOFF_1.1.5.md) trước. Bộ đó gồm lịch sử từ nền 0.3.x theo tài liệu còn lưu, khởi động phần mềm, kiến trúc, đường ghi dữ liệu, milestone, kiểm thử, lỗi còn lại và quy trình tiếp tục. Bản riêng từng phần ở [mục lục](handoff/1.1.5/00_README.md).

- `src/`, `scripts/`, `tests/`, `packaging/`: bản sao nguồn phát triển của build, có SHA256 đối chiếu trong `handoff/1.1.5/SOURCE_MANIFEST.json`.
- `docs/`: tài liệu và báo cáo theo thời điểm viết. Các câu “hiện hành”, “không push”, “chưa có M3” trong báo cáo cũ không mô tả trạng thái mới nhất; đọc ngày và phiên bản trước khi suy luận.
- `handoff/1.1.5/`: tài liệu bàn giao bổ sung sau khi build, không thay đổi hành vi hay bộ cài 1.1.5.
- `LICENSE`: MIT cho mã BHT; đọc `COPYING_SCOPE.md` cho tài nguyên bên ngoài. AutoCAD SDK và giấy phép chạy AutoCAD không nằm trong kho.

Kho phát hành: https://github.com/7xj1998-cell/BHT/tree/releases/source-1.1.5 . Bộ cài đã kiểm thử và gói bàn giao ở release v1.1.5. Nhánh main giữ lịch sử nền; không suy ra phiên bản mới nhất chỉ từ main.

Không sửa lại mã rồi phát hành bộ cài khác cùng số 1.1.5. `RELEASE_STATE.json` là dấu vết gói cục bộ đã đóng, không phải cấu hình đường dẫn cho máy khác. Muốn tạo bản phát hành có thay đổi, tăng phiên bản bằng công cụ có sẵn. Các tài liệu bàn giao/giấy phép bổ sung không phải một build runtime mới.
