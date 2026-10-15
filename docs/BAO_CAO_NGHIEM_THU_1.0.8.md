# BHT 1.0.8 — Báo cáo kiểm thử nhãn biển hai dòng

Ngày kiểm thử: 12/10/2026. Chỉ dùng AutoCAD 2024. Mã trước khi sửa được sao lưu tại `D:\lisp\BHT\old\pre-1.0.8`.

## Thay đổi

Trước đây nhãn ghép tên biển và lý trình vào cùng một TEXT, ví dụ `W.202a  Km38+966.96`. Bản 1.0.8 dùng tên biển ở dòng trên, lý trình ở dòng dưới; căn giữa, cùng phông, cỡ chữ và góc xoay. Dòng dưới nằm theo hướng dưới của chữ khi biển xoay, không cố định theo trục Y của bản vẽ.

Với nhiều mặt biển, giữ từng tên mặt và đặt lý trình dưới tất cả các tên. Khoảng cách dòng là 1,5 lần chiều cao chữ. Hồ sơ không có lý trình không tạo dòng trống. Giữ nguyên nhãn cọc tiêu, cột Km, đèn và trụ mất biển theo quy tắc hiện có.

Nhãn dưới là TEXT thông thường có liên kết tới cùng hồ sơ BHT. Bộ đo vùng bao, chọn thành phần, dịch hình và đổi tỷ lệ vẫn nhận dòng này. Khi cập nhật, giữ một dòng lý trình, sửa nội dung tại chỗ hoặc xóa khi hồ sơ đã bỏ lý trình.

Nút **Cập nhật mẫu biển đã vẽ** chỉ cập nhật biển/bảng. Đợt kiểm tra phát hiện thao tác cũ còn chuẩn hóa block đèn; đã loại nhóm đèn khỏi phạm vi nút này để giữ nguyên các đèn hiện có. Chèn/cập nhật đèn từ hồ sơ riêng vẫn được giữ.

## Kết quả có bằng chứng

| Bộ kiểm tra | Kết quả | Phạm vi |
| --- | --- | --- |
| Core | 233 ca đạt, 0 lỗi | Chạy lúc build |
| LABEL108 | 27 ca đạt, 0 lỗi | Chữ hai dòng ở 5 góc xoay, cùng phông/cỡ chữ, nhãn cũ, nhiều mặt biển, sửa/xóa lý trình, đổi tỷ lệ, cập nhật lặp, giữ block đèn |
| Lưu/mở lại nhãn | Đạt | Giữ các tên mặt và đúng một dòng lý trình |
| ROAD106 | 44 ca đạt, 0 lỗi | Bố trí theo tim, hai phía, cong/gãy, thiếu tuyến, loại Khác, Undo và phục hồi lỗi |
| BOT830 — cập nhật nhãn | 25/25 biển có lý trình đạt | Dùng bản sao tệp đã lưu của người dùng, giữ nguyên 801 điểm, 177 hồ sơ, mốc/tuyến, chuỗi cọc, vị trí/góc/tỷ lệ các ký hiệu |
| BOT830 — bố trí theo tim sau cập nhật | 120 ký hiệu đạt khoảng cách thử | Tính vùng bao có cả dòng lý trình, tim TUYEN1 loại Khác, khoảng cách thử 20 đơn vị; giữ điểm, mốc, lý trình, góc và tỷ lệ |
| Mở lại BOT830 | Đạt | Nhãn hai dòng và cấu hình bản sửa được lưu vào bản sao |

Thời gian cập nhật mẫu trên bản sao là 17.640 ms, một lượt. Thời gian bố trí 120 ký hiệu theo tim sau cập nhật là 1.984 ms, một lượt. Đây không phải phép đo trung vị 5 lượt hoặc phép nghiệm thu tốc độ tính lý trình.

Đã đối chiếu SHA256 trước/sau khi dùng mẫu; tệp nguồn không thay đổi. Không ghi đè bản vẽ đang mở.

## Tệp đi kèm

- `BOT830-Nhan-bien-hai-dong.dwg`: bản sao sau cập nhật nhãn, giữ vị trí ký hiệu từ tệp nguồn.
- `Nhan-bien-hai-dong.dwg`: mẫu nhỏ có một biển, nhiều mặt biển và đèn dùng kiểm tra phạm vi cập nhật.
- `label108-create.log`, `label108-reopen.log`: kết quả 27 ca nhãn và mở lại.
- `sample-label108.log`, `sample-label108-reopen.log`: kiểm tra dữ liệu BOT830 và nhãn sau lưu/mở lại.
- `road106-create.log`, `road106-reopen.log`, `sample-center107.log`: kiểm tra bố trí theo tim có nhãn hai dòng.

## Giới hạn

- Đã chạy bằng AutoCAD 2024 Core Console; chưa nghiệm thu thao tác trực tiếp trên phiên CAD của người dùng. Tệp đã lưu có thể khác nội dung chưa lưu trong phiên đang mở.
- Chưa cài thử trên máy sạch hoặc máy của người dùng thứ hai.
- Kiểm tra bố trí theo tim dùng khoảng cách 20 đơn vị để thử thuật toán; không xác nhận thông số đó phù hợp hồ sơ thực tế.
- Không tính lại hoặc bổ sung lý trình đang thiếu. BHT chỉ trình bày giá trị đã có trong hồ sơ.
- Không thay cơ chế phông chữ khi chia sẻ DWG; chuyển mọi chữ thành nét để gửi một DWG không cần phông ngoài chưa được hoàn thiện.

Để cập nhật biển đã vẽ: lưu công việc, đóng CAD, cài 1.0.8 và mở lại. Chọn **Tuyến & báo cáo → Công cụ bổ sung → Cập nhật mẫu biển đã vẽ**, kiểm tra một biển có lý trình, một cụm nhiều mặt và một biển xoay theo tuyến, rồi lưu DWG.
