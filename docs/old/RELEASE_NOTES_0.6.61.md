# BHT 0.6.61 — Ghép biển tùy chỉnh trên cùng trụ

Đóng AutoCAD trước khi cài, mở lại sau khi cài.

## STOP và bảng DỪNG LẠI / QUAN SÁT

1. Mở Chọn biển… và bật Chọn nhiều mặt trên cùng trụ.
2. Thêm R.122 (STOP) vào danh sách.
3. Chọn Biển tùy chỉnh, chọn mẫu DỪNG LẠI / QUAN SÁT rồi Thêm mặt.
4. Dùng Lên/Xuống để STOP ở dòng đầu, bảng chữ ở dòng dưới. Danh sách có sẵn STOP thì không cần thêm lại.
5. Bấm Chọn biển → Lưu hồ sơ → Chèn / cập nhật.

Bỏ ghi chú hướng dẫn ở bên phải thư viện. Danh sách mặt và nút sắp xếp vẫn giữ để ghép biển.

Muốn sửa chữ của một mặt tùy chỉnh: chọn mặt đó trong danh sách rồi Nội dung biển…; lưu mẫu và chọn biển để ghi nội dung mới vào hồ sơ. Mẫu vẫn được lưu lại cho lần dùng sau; mặt khác cùng mã giữ nội dung riêng.

## Phông và tỷ lệ

Đã đối chiếu DWG R.122, S.501, S.502 và S.509a: phần chữ dùng giaothong1.ttf. Biển tùy chỉnh nay dùng cùng phông Giaothong1 có hỗ trợ Unicode tiếng Việt. Bộ cài tiếp tục kèm phông này.

Bảng hai dòng có mặt rộng 1,8, cao 0,9 đơn vị trước tỷ lệ ký hiệu, cùng chiều rộng mặt STOP chuẩn. Chiều cao bảng điều chỉnh theo số dòng và không vượt 1,8. Chữ có WidthFactor=1; giảm chiều cao đồng đều nếu dài, không bóp ngang.

Đây là tỷ lệ ký hiệu CAD; không phải bản vẽ chế tạo biển theo kích thước thực. Xem trước dùng cùng phông và tỷ lệ khung. Công tắc Tô nền và Tỷ lệ biển áp dụng cho toàn cụm.

## Tương thích

Nội dung từng mặt lưu trong sign_content của DWG, không cần tệp custom-boards.xml để mở lại nội dung hồ sơ trên BHT 0.6.61. Chưa bổ sung chuyển chữ thành nét; xem đúng chữ trên máy không có phông vẫn thuộc phần bàn giao DWG đang thảo luận.

Bảng riêng bản cũ được giữ nguyên đến khi chọn lại trong thư viện và cập nhật. Khi chuyển, bảng trở thành mặt của nhóm Biển báo, có thể ghép với STOP hoặc dùng một mình.

Đã kiểm tra trên AutoCAD 2024 Core Console: tạo/lưu/mở lại cụm hai mặt, STOP phía trên, kích thước bảng, phông/WidthFactor và chữ nằm trong khung; hai mặt cùng mã khác chữ; UI thêm/sắp xếp/khôi phục nội dung. Chưa nghiệm thu máy sạch hoặc tự cập nhật bản vẽ người dùng.
