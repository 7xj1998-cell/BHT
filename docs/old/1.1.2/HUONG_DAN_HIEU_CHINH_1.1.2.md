# BHT 1.1.2 — Nạp dữ liệu kế thừa

Ngày 16/10/2026, AutoCAD 2024. Bản này tách bước chọn tệp khỏi bước đọc dữ liệu, ghi nhật ký từ trước khi chọn tệp. Chưa có bằng chứng kết luận đã sửa triệt để hiện tượng đứng CAD của người dùng.

## Cài và chuẩn bị

1. Lưu công việc, đóng AutoCAD rồi chạy BHT-Setup-1.1.2.exe.
2. Mở lại AutoCAD, gõ BHT hoặc BTH; kiểm tra tiêu đề 1.1.2.
3. Thử trên bản sao D15-Truoc-Nap.dwg trong Test-1.1.2. Giữ nguyên DWG và .bht nguồn trong New.

## Thao tác nạp

1. Mở **Tuyến & báo cáo → Quản lý xử lý theo đoạn → Nạp dữ liệu kế thừa (.bht)…**.
2. Bấm **Chọn tệp…**, chọn DOAN14.bht. Bước này chỉ nhận đường dẫn, chưa đọc gói hoặc sửa bản vẽ.
3. Nếu cửa sổ chọn tệp gặp vấn đề, bạn có thể dán đường dẫn đầy đủ vào ô nhập:

   `D:\lisp\BHT\New\Dữ liệu BHT\DOAN14.bht`

4. Bấm **Kiểm tra và nạp**. BHT đóng bảng chọn rồi kiểm tra dữ liệu; chỉ ghi khi toàn bộ điều kiện hợp lệ.
5. Nếu có xung đột, BHT giữ nguyên bản vẽ và mở bảng đối chiếu. Có thể kéo bảng sang bên để kiểm tra CAD. Không bấm nạp lặp liên tục.
6. Chỉ lưu DWG làm việc sau khi kiểm tra kết quả. Bấm Hủy hoặc Esc tại bảng chọn để thoát mà chưa nạp.

Lệnh BHTNAPTICHLUY vẫn giữ nguyên. Với FILEDIA=0, lệnh nhận đường dẫn tại dòng lệnh để phục vụ script; không cần đổi biến này khi dùng Palette.

## Kết quả đúng với DOAN14 và đoạn 15

| Nội dung | Bản vẽ đoạn 15 đã lưu | DOAN14.bht |
|---|---|---|
| Mã hồ sơ | OBJ-000001 | OBJ-000001 |
| Loại | Đèn tín hiệu | Biển P.127-80 |
| Điểm | BOT19-R-000144 | BOT1-R-000041 |

Đây là hai hồ sơ khác nhau trùng mã. Kết quả mong đợi là **chặn nạp, hiện 6 khác biệt, bản vẽ giữ nguyên**. Không xóa RTK hoặc hồ sơ để ép nhập. Bản này chưa có quy trình duyệt đổi mã đồng bộ toàn bộ tham chiếu.

## Bài thử

| Bài | Thao tác | Kết quả mong đợi |
|---|---|---|
| 1 | Mở bảng nạp rồi Hủy | Bản vẽ giữ nguyên |
| 2 | Dán đường dẫn tệp không tồn tại | Báo không tìm thấy, bảng vẫn mở |
| 3 | Chọn DOAN14, chưa bấm Kiểm tra và nạp | Hiện đường dẫn, chưa đọc gói |
| 4 | Dán đường dẫn DOAN14 rồi nạp trên bản sao D15 | Hiện 6 khác biệt, CAD tiếp tục phản hồi |
| 5 | Nạp DOAN14 vào DWG trống rồi nạp lại | 179 hồ sơ/801 điểm, không nhân bản |
| 6 | Lưu DWG đã nạp, đóng/mở lại rồi nạp cùng gói | Giữ dữ liệu, không tạo trùng |

## Nếu CAD vẫn đứng

Nhật ký: `%LOCALAPPDATA%\BHT\Logs\rolling-import-last.log`. Sao chép nhật ký ngay sau lỗi, trước lần thử tiếp theo; mỗi lần bắt đầu lệnh sẽ thay nhật ký gần nhất. Có thể dán đường dẫn thư mục vào thanh địa chỉ File Explorer.

Ghi lại bước cuối: mở bảng BHT, mở chọn tệp Windows, đã chọn tệp nhưng chưa bấm nạp, đang đọc DWG, đối chiếu dữ liệu, hay hiển thị xung đột. Nhấn F2 để xem dòng lệnh nếu CAD còn phản hồi. Nhật ký ghi phiên bản và đường dẫn DLL để kiểm tra nạp nhầm bản cũ.

Nếu dùng bản sao và chưa lưu, đóng bản sao không lưu để quay về trước nạp. Muốn trở lại 1.1.1: đóng CAD, chạy bộ cài cũ trong old và dùng DWG sao lưu trước thử. Không sửa hay ghi đè tệp nguồn để xử lý xung đột.

## Giới hạn

CoreTests, Core Console với DLL bảo vệ/FAS và kiểm tra bảng đã đạt. AutoCAD đầy đủ 1.1.1 trả về bảng xung đột và tiếp tục phản hồi khi dùng đường dẫn trong script. Chưa coi đây là nghiệm thu thao tác chọn tệp thực tế hoặc toàn bộ Milestone. 11 lỗi signs từ bản trước chưa được xử lý trong đợt này.

Các chức năng còn lại xem [hướng dẫn 1.1.1 lưu trữ](old/1.1.1/HUONG_DAN_HIEU_CHINH_1.1.1.md) và [bài kiểm tra toàn bộ](HUONG_DAN_TEST_TOAN_BO.md).
