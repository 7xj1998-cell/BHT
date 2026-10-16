# BHT 1.1.4 - Kiểm tra lý trình và hiệu chỉnh hồ sơ

Ngày 16/10/2026. Dùng AutoCAD 2024. Bản sửa thay thế 1.1.3 để kiểm tra cục bộ trên bản sao. Không tự sửa lại kết quả đã khóa khi mở bản vẽ.

## 1. Lỗi đã xác định

Phép lấy điểm gần nhất trên tim hữu hạn trả về đầu mút khi điểm khảo sát vượt đầu/cuối tim. Bản cũ dùng đầu mút đó để tính lý trình nếu khoảng cách vẫn trong giới hạn lệch tim. Vì vậy các điểm khác nhau có thể cùng nhận Km38+697.00. Đây là lỗi phần mềm đã tái hiện trên AutoCAD Core Console, không phải yêu cầu phải gán tay các biển vào đoạn.

Bản 1.1.4 loại điểm vượt đầu/cuối theo tiếp tuyến tại đầu mút. Điểm nằm đúng mặt cắt vuông góc ở đầu/cuối vẫn hợp lệ nếu đạt các điều kiện khác. Điểm nằm trên tim nhưng ngoài khoảng lý trình của đoạn cũng không được xác nhận, kể cả đã gán đoạn thủ công. Ngưỡng số học đầu mút là 0,000001 m; không phải dung sai khảo sát được tự nới rộng.

## 2. Vì sao báo cáo hiện tim Đoạn 11?

Phân biệt ba thông tin:

| Thông tin | Ý nghĩa |
| --- | --- |
| Tim / route_id | Tim đã dùng để thử tính, kể cả lần tính không thành công |
| Đoạn / doan | Đoạn thực sự được gán cho hồ sơ |
| OUT_OF_RANGE | Ngoài phạm vi, chưa có lý trình hợp lệ từ lần tính đó |

Trong ảnh báo cáo, dòng cách tim khoảng 12.500 m có trạng thái OUT_OF_RANGE và cột đoạn/gói trống. Chỉ thấy tên tim DOAN_11 chưa chứng minh hồ sơ được nhập vào Đoạn 11 hoặc đã khóa.

Đã đọc lại `D:\lisp\BHT\New\Dữ liệu BHT\DOAN15.bht` ngày 16/10: 179 hồ sơ, 801 điểm RTK, 108 hồ sơ khóa; Đoạn 11 có 8 hồ sơ khóa. File có thời điểm sửa 09:28:01 và SHA256 `7D510BF3B9C37350CC770E446DE61B42124676CFE0F10819A3D19A60BC7FFFDC`.

Tám hồ sơ của Đoạn 11: OBJ-000001, OBJ-000002, OBJ-000003, OBJ-000039, OBJ-000040, OBJ-000041, OBJ-000047, OBJ-000185. Lý trình tính lại để đối chiếu khớp số đã lưu đến 1 mm. Không ghi kết quả đối chiếu vào file nguồn.

Ảnh bảng ghi 10 khóa chưa khớp file này. Cần lưu DWG đang có 10 khóa thành một bản sao mới hoặc xuất hồ sơ với tên mới để đối chiếu hai hồ sơ tăng thêm. Không xuất đè DOAN15.bht chỉ để cung cấp dữ liệu kiểm tra.

## 3. Xử lý hồ sơ ngoài phạm vi

1. Dùng SAVEAS tạo bản DWG kiểm tra; giữ DWG/.bht đang làm và bản đã bàn giao.
2. Cài 1.1.4 khi AutoCAD đã đóng. Mở lại bản sao, gõ BHT hoặc BTH và kiểm tra phiên bản.
3. Trong quản lý xử lý theo đoạn, kiểm tra đúng đoạn, tim, Km đầu/cuối và phía đường. Đoạn 1 trong ảnh là Km38+622 đến Km38+697. Không kéo dài tim hoặc tăng phạm vi chỉ để nhận thêm biển chưa đối chiếu.
4. Bấm Tính bổ sung, đọc từng dòng lý do trước khi áp dụng. Điểm vượt đầu/cuối phải bị loại; không được dồn ba biển về Km38+697 chỉ vì gần điểm cuối.
5. Nếu hồ sơ chưa khóa đang gán nhầm đoạn: dùng Chọn đoạn / gói cho hồ sơ (BHTGANDOAN), chọn đúng hồ sơ rồi Bỏ gán đoạn hoặc chọn đoạn đúng đã có tim/mốc. Bỏ gán chỉ bỏ liên kết đoạn, không chứng minh lý trình cũ đúng. Tính lại bằng tim đúng và kiểm tra trước khi xác nhận.
6. Nếu hồ sơ đã khóa: dùng Hiệu chỉnh hồ sơ đã xác nhận cho riêng hồ sơ đó. Chọn Phân đoạn / gói thầu để chuyển sang đoạn đúng; chọn Liên kết điểm RTK nếu chọn nhầm điểm; chọn Tính lại lý trình riêng khi đoạn và điểm đã đúng. Xem trước phải tính lại và đạt phạm vi/phía đường. Duyệt chuyển đoạn chỉ khi bảng trước/sau đúng.
7. Nếu không có đoạn/tim phù hợp hoặc thiếu bằng chứng nguồn, giữ hồ sơ cũ để đối chiếu. Hiện chưa hỗ trợ bỏ hẳn liên kết đoạn của hồ sơ đã khóa qua hiệu chỉnh; không mở khóa cả đoạn để vượt điều kiện này.

Tính bổ sung bỏ qua hồ sơ không đủ điều kiện và giữ nguyên hồ sơ đã khóa. Nó không phải công cụ tự sửa kết quả cũ sai. Hồ sơ đã gán nhưng không đạt phạm vi sẽ chặn xác nhận bổ sung cho đến khi xử lý riêng.

## 4. Vì sao không bấm được Xác nhận hiệu chỉnh?

Trong ảnh, bảng Trước/Sau còn trống: chưa có phương án xem trước hợp lệ. Nhập lý do không tự làm thay đổi trường Mô tả.

1. Chọn Thuộc tính khảo sát khi chỉ sửa mã/mô tả. Nếu muốn xóa mô tả dư, sửa trực tiếp ô Mô tả trong cột Giá trị đề nghị rồi rời ô đó.
2. Nhập lý do và người hiệu chỉnh.
3. Bấm Xem trước / tính riêng. Nếu báo Không có thay đổi thì giá trị đề nghị vẫn giống bản đã lưu; không có phiên bản mới để xác nhận.
4. Kiểm tra bảng trước/sau. Nếu chỉ sửa mô tả, không được tự xuất hiện thay đổi điểm RTK, lý trình hoặc đoạn.
5. Với hồ sơ cũ chưa có lịch sử, chỉ đánh dấu đồng ý lưu bản gốc khi chấp nhận giữ nguyên bản cũ dưới dạng nguồn chưa được xác minh. Đây không phải xác nhận rằng dữ liệu cũ đã đúng.
6. Mục duyệt chuyển đoạn chỉ hiện khi đoạn thực sự đổi. Không đánh dấu chuyển đoạn để sửa mô tả.
7. Bấm Xác nhận hiệu chỉnh khi nút đã bật. Nếu sửa thêm sau xem trước, phải xem trước lại.

Bản 1.1.4 ẩn các mục duyệt chưa liên quan và hiện rõ điều kiện còn thiếu. Nếu xem trước báo lỗi điểm/tim/mốc/lớp khóa, cần xử lý đúng lỗi đó; đánh dấu các ô không bỏ qua kiểm tra dữ liệu.

## 5. Bài kiểm tra trước khi tiếp tục làm thật

| Bài | Thao tác trên bản sao | Kết quả cần thấy |
| --- | --- | --- |
| Ngoài cuối tim | Tính đoạn 1 với biển vượt Km cuối | Loại khỏi phương án; không nhận Km38+697 giả |
| Đúng ranh | Điểm có hình chiếu vuông góc đúng đầu/cuối | Được tính nếu đạt phạm vi lệch tim, mốc và phía đường |
| Gán tay sai | Gán điểm có lý trình ngoài đoạn rồi tính/xác nhận | Bị chặn; không khóa kết quả sai |
| Giữ hồ sơ cũ | Bổ sung một hồ sơ trong đoạn có hồ sơ khóa | Hồ sơ khóa giữ nguyên điểm, mã, lý trình, đoạn |
| Hiệu chỉnh | Sửa Mô tả, xem trước, duyệt bản gốc nếu được yêu cầu | Chỉ thay trường đã đề nghị; có lịch sử; hồ sơ khác không đổi |
| Hoàn tác | Sau thao tác thử, dùng Undo ngay; lưu/mở lại bản sao | Khôi phục bản ghi và trạng thái đúng; không nhân bản hồ sơ |

Đã kiểm tra tự động trên dữ liệu giả lập và đọc đối chiếu DOAN15.bht. Chưa có bản DWG mới chứa trạng thái 10 khóa của ảnh; chưa nghiệm thu thao tác chuột trên phiên AutoCAD của người dùng. Kiểm thử cũ về ký hiệu còn 11 lỗi đã ghi nhận trước đây, không được coi là đã sửa trong 1.1.4.

## 6. Phục hồi

Đóng bảng xem trước trước khi xác nhận sẽ không ghi dữ liệu. Sau thao tác đã ghi trên bản sao, dùng Undo ngay trước khi làm việc khác hoặc mở lại bản sao trước thao tác. Hiệu chỉnh đã xác nhận giữ bản trước trong lịch sử; thay đổi tiếp phải tạo lần hiệu chỉnh mới. Không lấy .bht cũ nạp đè để xóa lịch sử.

Để quay lại bản phần mềm trước, giữ bản vẽ kiểm tra riêng, đóng CAD và dùng bộ cài cũ trong `old`. Bản cũ vẫn có lỗi giới hạn đầu/cuối nên không dùng để tiếp tục xác nhận các hồ sơ đang nghi sai.
