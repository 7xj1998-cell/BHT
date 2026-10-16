# Rà soát và kiểm chứng BHT 1.0.13

Ngày 13/10/2026. Môi trường: AutoCAD 2024 x64.

## Phạm vi rà soát

Đã quét 83 file C#, 13 file Lisp và 10 script PowerShell trong source/công cụ/cài đặt; build các assembly và biên dịch toàn bộ 12 module Lisp. Rà soát tập trung vào nhập số liệu, hồ sơ/XRecord, ảnh, tim và mốc, ký hiệu/nhãn, xuất nhập dự án, xuất Excel, điều phối lệnh và cài đặt. Các script PowerShell sản phẩm không có lỗi cú pháp qua parser.

Đây là rà soát tĩnh kết hợp hồi quy trên AutoCAD 2024, không phải chứng nhận không còn mọi lỗi trên mọi bản vẽ hoặc phiên bản CAD.

## Lỗi đã có bằng chứng và bản sửa

| Vấn đề | Kiểm chứng |
| --- | --- |
| Cung đơn bị chia thành dây cung | DLL 1.0.11 thiếu 3,25 mm trên nửa đường tròn bán kính 10 m; bản mới giữ cung, kiểm tra thuận/ngược chiều và lưu/mở lại đạt. |
| Nhập lý trình cực lớn | 1.0.12 nhận `1e100` rồi ném OverflowException; bản mới từ chối trước khi ghi. |
| Mốc trùng và số không hợp lệ | Lisp cũ lỗi khi tính với hai mốc cùng khoảng cách; Core cũ trả VALID cho truy vấn NaN. Bản mới trả trạng thái không xác định/lỗi đầu vào. |
| Nhập Km không nhất quán | Lisp cũ nhận `Km1+1000` và không nhận dấu phẩy thập phân. Đã thống nhất quy tắc nhập với Core. |
| Cập nhật nhầm đường tham chiếu | Trước đây chỉ cần cùng lớp BHT_TUYEN_TDT; nay phải có XData trỏ đúng tim nguồn. Kiểm tra không thay đổi hình học khi chỉ đọc quyền cập nhật. |
| Báo cáo Excel | Ký tự U+0001 trước đây tạo XML không hợp lệ. Đã mã hóa theo SpreadsheetML, giữ nguyên chuỗi escape có sẵn; thử lỗi dữ liệu/tệp khóa vẫn giữ báo cáo cũ. |

## Kết quả

| Hạng mục | Kết quả |
| --- | --- |
| Core | 249 kiểm tra đạt |
| Hình học và liên kết tim | 37 kiểm tra đạt trên DLL thường/bảo vệ |
| Lisp mới | 10 kiểm tra đạt trên source và FAS |
| Giao diện | 2.679 kiểm tra đạt |
| Copy biển / đọc nhãn cọc | 34 / 11 kiểm tra đạt |
| Hồi quy rộng | 16 bộ đạt: lý trình hai mốc, đầu/chiều tuyến, sửa/xóa tuyến/Undo, mốc thực địa, mốc nhập, mốc thủ công, báo cáo, xuất/nhập dự án, bố trí biển, nhãn, RTK, đèn, lỗi mới, hình học TDT, copy biển, giao diện. |
| Kiểm tra bổ sung | Hướng biển/Undo đạt; bootstrap FAS/DLL đạt; REVIEW đạt, S0 20 đạt, V5 22 đạt. |

Hai kỳ vọng cũ được cập nhật theo hành vi đã phát hành: cọc chưa có Km/H hiện “Cọc tiêu” từ 1.0.9; block biển chuyển sang tên portable từ 1.0.10. Vẫn giữ kiểm tra nội dung, phông, khung, vị trí và dữ liệu RTK, không bỏ qua lỗi hình học để làm bộ thử đạt.

## Giới hạn

Không có khóa TDT trong đợt này, nên chưa kiểm chứng lý trình native trên DWG thực tế. Ca TX cũ thiếu fixture proxy và được ghi BLOCKED; không tính là đạt. Chưa thử máy nhận khác, CAD hãng khác hay chuỗi thao tác nhiều tài liệu bằng chuột trong AutoCAD đầy đủ.

Các cung song song XY được giữ chính xác; các loại đường cong khác vẫn có phương pháp lấy mẫu. Các phép thử hình học không chứng minh đã đọc được lý trình gốc TDT.

Nhật ký ở `D:/lisp/BHT/old/maintenance-20261013`. Kiểm tra bộ cài và chạy độc lập sau đóng gói được ghi tại `installer113.log`, `runtime113.log`; tệp bàn giao gốc ở `D:/lisp/BHT/BHT-Setup-1.0.13.exe`.
