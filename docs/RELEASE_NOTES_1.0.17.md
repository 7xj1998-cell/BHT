# BHT 1.0.17 — Tính lại theo tim TDT mới

## Trường hợp đã sửa

Sau khi nạp dự án, phương pháp tính FIELD_MARKERS của bản cũ vẫn còn. Nạp tim TDT không tự chuyển phương pháp. Vì vậy nút tính tại hồ sơ vẫn có thể báo danh sách cọc đã thay đổi thay vì dùng tim mới. Trước đây các mốc TDT còn được ghép với số Km/H trong hồ sơ cọc đã nạp, làm thay đổi nội suy của tim TDT.

Tim có mốc TDT đã xác nhận nay chỉ dùng các mốc trên tim và mốc bổ sung được khai báo. Không tự dùng Km/H hồ sơ cũ để hiệu chỉnh. Cách tính tuyến thông thường không có mốc TDT vẫn hỗ trợ cọc hồ sơ như trước.

## Tính lại

1. Đóng AutoCAD, cài BHT 1.0.17 rồi mở lại DWG đang kiểm tra.
2. Tab Tuyến > chọn đúng tim TDT mới trong bảng Dữ liệu tuyến.
3. Đọc / kiểm tra cọc TDT (BHTDOCCOCTDT), xem và xác nhận các mốc đúng. Dùng BHTMOCKM cho mốc bổ sung hoặc điểm gãy Km đã biết chính xác trên tuyến mới.
4. Xem / xóa mốc trên tim tuyến (BHTDSMOC). Cần mốc phủ khu vực các hồ sơ cần tính. Trong ảnh người dùng, ba mốc TDT từ Km49+103.28 đến Km49+294.25 không đủ tính biển gần Km48+540 với ngoại suy 50 m. Cần mốc trước/sau vùng này; không tăng ngoại suy chỉ để có kết quả.
5. Bấm Tính lại theo tim đang chọn… (BHTTINHTHEOTUYEN). Xem số hồ sơ tính được/chưa xác định và phạm vi mốc, rồi nhập C để áp dụng hoặc K để hủy.

Thao tác chuyển sang phương pháp tim tuyến, lưu đúng ID tim đang tính để các lần tính riêng/toàn bộ sau dùng cùng tuyến. Không tiếp tục chọn tuyến cũ chỉ vì gần điểm hơn. Tim đang tính bị xóa sẽ báo thiếu tuyến thay vì tự chọn tuyến khác. Đổi tên tuyến cập nhật ID này.

## Kết quả dữ liệu

Các hồ sơ tính được nhận lý trình mới. Hồ sơ ngoài phạm vi, thiếu mốc hoặc thiếu vị trí được xóa lý trình cũ và ghi trạng thái chưa xác định. Bỏ gán đoạn/gói cũ để tránh giữ phân đoạn sai theo lý trình cũ. Có một bước xác nhận trong lệnh trước khi ghi.

Số Km/H trên cọc là dữ liệu khảo sát nên được giữ, không bị đổi theo tim; chúng không tham gia hiệu chỉnh tim TDT. Điểm RTK, hình học tim, ký hiệu, ảnh và hồ sơ gốc được giữ. Cần chạy lại gán đoạn/gói sau khi lý trình đã đúng. Một lần Undo khôi phục cả lý trình, chế độ tính và tim đang dùng.

Đã kiểm tra 22 ca mới và lưu/mở lại trên FAS/DLL bảo vệ, gồm loại mốc cũ, tuyến cũ gần hơn, ngoài phạm vi, mốc bổ sung, cung đảo chiều, hủy, Undo và lỗi ghi hoàn tác. Các bộ tuyến/cọc hiện có được chạy hồi quy. Kết quả đầy đủ ở `D:/lisp/BHT/audit-1.0.17/BAO_CAO.md`.

Ảnh chụp cho thấy dữ liệu hiện tại nhưng DWG đang chỉnh sửa trong ảnh chưa được cung cấp dưới dạng bản lưu có trạng thái BHT đó. Bộ thử tái hiện logic bằng dữ liệu kiểm thử; chưa nghiệm thu lý trình native TDT trên chính bản vẽ đang mở của người dùng.
