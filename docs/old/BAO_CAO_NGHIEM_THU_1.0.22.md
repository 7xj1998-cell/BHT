# Kiểm chứng BHT 1.0.22

Ngày 14/10/2026. AutoCAD 2024 x64, C# 5 / .NET Framework 4.8. Bản nguồn trước sửa: `D:/lisp/BHT/audit-1.0.22/source-before-1.0.22.zip`. Log kiểm tra và kết quả đóng gói: `D:/lisp/BHT/audit-1.0.22/`.

## Phát hiện từ dữ liệu cung cấp

`D:/lisp/BHT/filebaohieu.bht` có 179 hồ sơ, một tim nguồn TUYEN1 và **0 đoạn**. Cấu hình nguồn dùng FIELD_MARKERS. Gói này không chứa bảng phân chia, mặc dù định dạng `.bht` đã hỗ trợ xuất/nạp các bản ghi SEG.

Bản cũ có thể dùng nhãn Km do BHT tạo trong bộ đọc cọc và lấy Km/H hồ sơ nhập làm mốc dự phòng cho tim TDT chưa xác nhận mốc. Chữ ký hình học trong gói cũng còn handle nguồn sau sao chép, gây cảnh báo tim đã đổi dù mẫu hình học không đổi. Đã sửa các đường xử lý này và thêm luồng nạp báo hiệu theo tim đích.

Lỗi `bad argument type: numberp: nil` được tái hiện trên mã 1.0.21 bằng cách truyền raw-distance thiếu vào hàm đổi khoảng cách tuyến. Bản mới kiểm tra dữ liệu trước phép toán, từ chối cả lần quét khi có dòng cọc không hợp lệ và trả thông báo. Đây là tái hiện lỗi đầu vào; chưa đủ bằng chứng để kết luận chính xác nguồn phát sinh nil trong phiên đoạn 10 của người dùng.

## Kết quả kiểm tra

| Phạm vi | Kết quả |
|---|---|
| Core | 303 đạt; có đọc bảng XLSX thực tế |
| WORKFLOW122 Lisp | 11 đạt: thiếu raw-distance, phạm vi, cô lập mốc TDT, dữ liệu quét thiếu, không nhận một phần danh sách lỗi, giữ số cọc khảo sát |
| WORKFLOW122 .NET | 23 đạt trên DLL bảo vệ: nạp theo tim đích, giữ tọa độ và mặt phụ, bỏ nhãn lý trình nguồn, giữ bảng đoạn/Unicode/ảnh, bảo toàn cấu hình đích, không sao chép tim nguồn từ cấu hình trình bày, từ chối nạp trùng, sửa chữ ký handle, xuất/nạp bảng 15 đoạn |
| Gói thật và tim đoạn 8 đã có mốc xác nhận | 5 đạt trên FAS/DLL bảo vệ; nhập đủ 179 hồ sơ vào bản vẽ thử, tính lại bằng hình học và mốc đích |
| STATION117 | 22 đạt, Undo và lưu/mở lại |
| SEG119 | 19 đạt; áp dụng nguyên khối, hoàn tác lỗi, lưu/mở lại, truyền dự án; cả DLL thường và bảo vệ |
| OFFSET121 | 26 đạt; .NET kiểm tra DIM thật, hai đầu XY, Measurement, lớp khóa và lưu/mở lại; cả DLL thường và bảo vệ |
| FIELD / CAPTION118 / REPORT120 | 51 / 21 / 11 đạt, gồm các kiểm tra lưu/mở lại phù hợp; REPORT đọc DWG cung cấp |
| Palette | 2.897 đạt trên DLL thường và bảo vệ; kiểm tra bố cục sáu bước và trạng thái nút khi chưa chọn tim |

Trong ca thật đoạn 8, OBJ-000120 có hai trụ: chọn trụ gần tim, lệch 2,251 m và lý trình 48.525,006 m (Km48+525,01). OBJ-000116 tính được 48.784,073 m. Ca này dùng hình học tham chiếu và mốc đã xác nhận lưu sẵn; không thay thế kiểm chứng đọc cọc native TDT trực tiếp.

## Bảng phân chia trong .bht

Đã tạo riêng `D:/lisp/BHT/filebaohieu-kem-15-doan-1.0.22.bht` từ gói gốc và `D:/lisp/BHT/Ho so/Phan chia goi thau.bhtseg`. Xuất rồi nạp lại đối chiếu toàn bộ bản ghi chuẩn hóa: đủ 15 đoạn, giữ các trường. Luồng nạp báo hiệu theo tim đích cũng giữ đủ 15 đoạn, bỏ liên kết tim nguồn để gán tim mới. Những đoạn cùng ID đã có ở đích được giữ nguyên.

Bảng hiện có chưa gán tim. Mã GOI_01…03 được sinh từ tên nhóm khi chuyển XLSX ở bản 1.0.19; cần đối chiếu mã gói thực tế. `.bht` xuất dữ liệu đã lưu trong DWG, không tự tìm và ghép XLSX bên ngoài. Thông báo xuất/nạp hiển thị số hồ sơ, tim và đoạn; gói không có SEG có thông báo rõ.

## Giới hạn và cách dùng

DWG đoạn 10 đã lưu tìm thấy chưa có trạng thái hồ sơ BHT như ảnh. Các DWG trong `New` có dữ liệu làm việc thuộc đoạn 8. Core Console không đọc được proxy TDT như phiên AutoCAD có TDT đầy đủ; thử native trên bản sao chưa hoàn tất vì nhập DWG báo eBadDwgHeader. Không tính thử này là đạt. Cần kiểm tra lại **Đọc / kiểm tra cọc TDT** trong đúng profile TDT và DWG làm việc sau cập nhật.

Mở DWG đoạn chưa nhập hồ sơ BHT → **Nạp báo hiệu từ .bht để tính theo tim đích** → chọn tim đoạn, đúng loại tim → đọc/xác nhận ít nhất hai mốc bao quanh phạm vi tính → **Tính lại theo tim đang chọn** → kiểm tra/DIM → gán tim cho đoạn và phân đoạn → lưu DWG, xuất báo cáo hoặc `.bht`. Không suy mốc từ số Km/H cũ trong file tổng hợp. Tệp ảnh gốc vẫn ở ngoài gói.

Các thử nghiệm dùng bản sao hoặc đọc DWG nguồn; không ghi đè DWG và `.bht` gốc, không thay tài nguyên TDT, không tự cài bộ phát hành. Kết quả kiểm tra runtime và bộ cài cuối cùng ghi tại `D:/lisp/BHT/audit-1.0.22/BAO_CAO.md`.
