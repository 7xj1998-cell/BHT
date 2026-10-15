# BHT 0.6.63 — Bổ sung cọc thủ công và sắp xếp nút

## Cách dùng
1. Đóng AutoCAD, cài BHT 0.6.63 rồi mở lại bản vẽ.
2. Trong Hồ sơ đối tượng, bấm **Thêm cọc…** cạnh **Tạo từ RTK…**.
3. Chọn Cọc tiêu hoặc Cột Km. Nhập Km/H nếu biết; chưa biết thì để trống cả hai. Ô Căn cứ vị trí lưu ghi chú do người dùng nhập, ví dụ ảnh hiện trường.
4. Bấm **Chọn vị trí trên CAD…**, chọn điểm hoặc nhập tọa độ theo UCS hiện tại. BHT lưu WCS XY trong Model. Esc hủy, không tạo dữ liệu dở dang.
5. BHT tạo điểm, hồ sơ và vẽ ký hiệu. Hồ sơ mới được chọn để sửa số Km/H, tình trạng, ảnh và cách đặt ký hiệu. Nếu Lisp chưa sẵn sàng hoặc lỗi vẽ, hồ sơ vẫn giữ; nạp Lisp rồi bấm **Chèn / cập nhật**.

Vị trí bổ sung không phải điểm đo RTK. Cao độ chưa biết: Z=0 chỉ là mặt phẳng vẽ. Chương trình không tự suy ra vị trí từ ảnh hoặc chia đều khoảng hai cọc. Người dùng chọn đúng vị trí theo căn cứ mình có.

Cọc thủ công không dùng làm mốc tính lý trình trong chuỗi cọc hoặc mốc tự nhận theo tim tuyến. Số Km/H nhập vẫn hiển thị/lưu vào hồ sơ. Cọc cùng loại tại cùng vị trí trong phạm vi 1 mm được chặn để tránh tạo trùng do bấm lại; đây không phải dung sai đo đạc.

## Các nút
- Tạo mới: **Tạo từ RTK…** / **Thêm cọc…**.
- Sửa hồ sơ: **Chọn biển…** / **Lưu hồ sơ**.
- Ký hiệu: **Đặt ký hiệu…** / **Chèn / cập nhật**.
- **Tỷ lệ biển / nhãn…** rộng cả hàng; **Tô nền tất cả biển** ngay dưới.
- **Gắn điểm có sẵn…** thay tên **Thêm điểm CAD…**, cạnh **Gỡ điểm chọn** trong phần vị trí.
- Tab **Điểm RTK** đổi thành **Điểm vị trí**; Tổng quan tách số RTK/thủ công; thông tin hồ sơ/điểm ghi rõ nguồn vị trí.
- Xóa hồ sơ ở cuối bảng, tách khỏi nút tạo mới.

## Lưu trữ và kiểm thử
- Điểm: layer BHT_MANUAL, XData BHT_PT, dataset/file nguồn MANUAL_CAD, ID MANUAL-M-<GUID>, Z gốc trống. Không ghi XData BHT_RTK giả.
- Hồ sơ: position_source=MANUAL_CAD, position_z_status=UNKNOWN, station_control_eligible=0. Điểm và hồ sơ tạo cùng transaction; sửa/lưu/mở lại giữ nguồn. Hồ sơ khác dùng chung điểm thủ công cũng không được coi là mốc thực địa.
- AutoCAD 2024: Core 233, giao diện 727, chuỗi cọc 51 cùng save/reopen và Undo API. Kiểm tra tạo/sửa cọc thủ công, Km/H sai/để trống, trùng vị trí, nguồn/cao độ, vẽ ký hiệu, save/reopen, Undo tạo cọc và chặn dùng làm mốc. Đạt trên mã nguồn và FAS/DLL bảo vệ.
- Chưa nghiệm thu trên máy sạch khác; chưa cài thay DLL trong phiên CAD đang mở. Chưa thêm cọc tại vị trí người dùng nêu: vị trí cụ thể do người dùng chọn.
- Bộ cài cục bộ tiếp tục kèm thư viện biển/đèn/phông. Không đưa tài nguyên bên thứ ba lên GitHub. Phương án một DWG không phụ thuộc phông chưa thay đổi.
