# BHT 1.1.5 — mã nguồn mở và bộ bàn giao AI

Công bố nguồn C#/AutoLISP của build1.1.5 cùng bộ tổng hợp lịch sử từ nền Lisp đến các milestone và sửa lỗi1.1.x. Bộ cài là đúng tệp1.1.5 đã kiểm thử/cài cục bộ, không build lại hoặc thay đổi dưới cùng số phiên bản.

## Nội dung

- Mã nguồn: nhánh `releases/source-1.1.5`; đọc `START_HERE_AI.md` trước. Mã BHT theo MIT; tài nguyên bên ngoài theo `COPYING_SCOPE.md`.
- `BHT-Source-1.1.5.zip`: source, scripts, tests, tài liệu lịch sử, bộ bàn giao và mẫu thư viện cần thiết. Không chứa DLL Autodesk hay hồ sơ khảo sát.
- `BHT-AI-Handoff-1.1.5.zip`: gói nhỏ gửi cho AI, gồm tổng hợp, prompt tiếp nhận, lịch sử, kiến trúc, build/test, lỗi còn lại, manifest và báo cáo chọn lọc.
- `BHT_AI_HANDOFF_1.1.5.md`: toàn bộ nội dung chính trong một tệp để đọc nhanh.
- `BHT-Setup-1.1.5.exe`: bộ cài hiện có cho AutoCAD2024. Đóng CAD trước khi cài, kiểm tra trên bản sao.
- `SHA256SUMS.txt`: đối chiếu các tệp tải về.

## Thay đổi gần nhất

Tim tham chiếu dùng BHT_TUYEN_ kèm mã tuyến, tránh tên layer lặp $0$. Bản vẽ cũ có nút đối chiếu và chuyển layer trong Tuyến & báo cáo. Giữ RTK, hồ sơ khóa, hình học và layer cũ; chặn xung đột thuộc tính/viewport. Giữ các sửa phạm vi lý trình của1.1.4.

## Kiểm chứng và giới hạn

Nguồn công bố đã build đủ Core/Bridge/Palette trên máy có AutoCAD2024; 443 CoreTests đạt. Đối chiếu858 file nguồn/script/test/tài nguyên khớp SHA256 snapshot phát triển. Runtime cuối1.1.5: layer4 Lisp +18.NET +Undo, Progress12513+28+7/nhãn, Correction50 đạt. M1/scope/layout và các milestone khác có bằng chứng theo phiên bản trong bộ bàn giao.

Đây là **prerelease**. Còn11 lỗi signs legacy; chưa nghiệm thu GUI/công trình15đoạn và mọi biến thể M2–M5. Không dùng kiểm tra Core để suy ra AutoCAD GUI đã đạt. Các ghi chú “không push/chưa có M3/hiện hành” trong tài liệu cũ là lịch sử. Nhánh main giữ nền cũ; dùng đúng nhánh/tag1.1.5.