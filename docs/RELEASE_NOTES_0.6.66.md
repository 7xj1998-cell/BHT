# BHT 0.6.66 — Mốc lý trình gồm cột Km và cọc tiêu

## Thay đổi

- Sửa thiếu cột Km trong tính lý trình theo mốc thực địa. Cột Km dùng số KmN+000, cọc tiêu dùng số KmN+H×100. Danh sách tự lấy sắp chung hai loại, chia khoảng tính tại từng mốc đủ dữ liệu.
- Đổi nút **Kiểm tra cọc đã nhập…** thành **Mốc lý trình đã nhập…** trong tab **Tuyến & báo cáo**. Thêm cột **Loại mốc**, **Nguồn**. Hiện cả mốc đo thiếu số/vị trí và mốc thủ công chưa xác nhận; chúng chưa tham gia nội suy.
- Chọn hai mốc hoặc lập nhóm đường/nhánh cũng nhận cả hai loại. Nhóm/cặp gán riêng được giữ nguyên: cần kiểm tra lại để đưa cột Km nằm giữa vào đúng nhóm, hoặc chuyển hồ sơ về **Theo cọc đã nhập…** để dùng danh sách chung.
- Xác nhận/bỏ xác nhận riêng từng mốc thủ công, gồm cả cột Km. Xác nhận lưu ngay, kể cả sau đó hủy bảng. Giữ nguồn MANUAL_CAD; kết quả sử dụng mốc này ghi tham khảo. Đổi loại, số hoặc vị trí so với lúc xác nhận phải xác nhận lại. Không đưa mốc thủ công vào cơ chế tự nhận mốc theo tim tuyến.
- Danh sách mốc cũ cần xem lại trước khi tính theo bộ mốc mới. Không tự cập nhật DWG khi mở. Không dùng nội suy để ghi đè số Km/H của mốc.
- Giữ kiểm tra trùng lý trình, đảo chiều, nhiều đoạn ứng viên, khoảng cách và hệ số bất thường. Cột Km không tự được ưu tiên để bỏ qua mâu thuẫn với cọc tiêu; cần đối chiếu các mốc trước khi chấp nhận kết quả.
- Bao gồm sửa tên I.428 thành **Cửa hàng xăng dầu** và tìm kiếm theo tên từ đợt 0.6.65 chưa bàn giao.

## Sử dụng

1. Lưu công việc, đóng AutoCAD, chạy `BHT-Setup-0.6.66.exe`, mở lại AutoCAD 2024.
2. Vào **Tuyến & báo cáo → Mốc lý trình đã nhập…**. Kiểm tra loại, số mốc, vị trí và cùng đường/nhánh.
3. Với mốc thủ công đã kiểm tra, chọn từng dòng → **Xác nhận cọc làm mốc**. Thiếu số/vị trí thì sửa hồ sơ trước.
4. Bấm **Dùng các mốc này**, rồi **Tính lý trình các hồ sơ**. Đọc các cảnh báo; không coi mọi mốc có số là đã được kiểm chứng ngoài hiện trường.

## Kiểm tra với dữ liệu BOT830

Dữ liệu đọc trực tiếp từ phiên CAD ngày 11/10/2026 được dựng lại trong phiên AutoCAD Core Console 2024 riêng: 171 hồ sơ, 795 điểm. Không sửa hoặc lưu bản vẽ gốc.

- Bảng hiện đủ 51 mốc: 35 mốc từ điểm đo đủ dữ liệu (31 cọc tiêu + 4 cột Km), 5 mốc thủ công chờ xác nhận, 11 cọc thiếu Km/H.
- Cột Km39, Km41, Km45, Km46 được dùng chia các khoảng trước/sau, không bỏ qua trong danh sách tự lấy.
- Hai cọc OBJ-000157/OBJ-000158 cùng H3/49 bị chặn khi cùng xác nhận; chưa tự đổi cọc nào thành H4/49.
- OBJ-000170 đang H7/47 vẫn giữ số đã nhập; đoạn từ H2/47 cảnh báo khoảng cách/hệ số. Mô tả điểm đo gốc ghi H4/47 cần người dùng đối chiếu.

## Kiểm thử và đóng gói

Core: 233 kiểm tra đạt. Nội suy thực địa: 51 kiểm tra cùng lưu/mở lại và Undo đạt. Bộ mốc chung/thủ công: 52 kiểm tra cùng lưu/mở lại/API đạt trên FAS và DLL đã bảo vệ. Kiểm thử dữ liệu BOT830 dựng lại: 21 kiểm tra đạt. Giao diện được kiểm tra riêng, gồm loại/nguồn mốc, số lượng, xác nhận mốc thủ công, mốc thiếu dữ liệu và bố cục cửa sổ.

Bộ cài cục bộ kèm thư viện DWG/phông như các bản trước; không công bố tài nguyên bên thứ ba lên GitHub. Chưa nghiệm thu cài đặt trên máy sạch khác. Không cam kết xử lý đầy đủ phụ thuộc phông khi gửi DWG sang máy không có BHT.
