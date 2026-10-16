# BHT 1.0.0 — giao diện thống nhất, tính hàng loạt nhanh hơn

Ngày phát hành: 11/10/2026. Kiểm thử trên AutoCAD 2024 x64.

## Giao diện

- Trang Tổng quan chia công việc thành bốn bước: Dữ liệu → Báo hiệu → Lý trình → Bàn giao. Gợi ý bước tiếp theo theo dữ liệu trong bản vẽ.
- Nền sáng, nút xanh; tên nút và thông báo bằng tiếng Việt. Các chức năng đang dùng vẫn có trong từng trang.
- Hồ sơ có bộ lọc nhanh: chưa có lý trình, chưa có mã biển, thiếu điểm vị trí/chân trụ, lý trình cần kiểm tra.
- Chỉ cập nhật trang đang xem; đọc trang còn lại khi chuyển sang. Giữ nội dung hồ sơ chưa lưu khi làm mới và tìm kiếm.

## Hiệu năng

- Tra cứu điểm theo ID/handle bằng chỉ mục riêng cho từng bản vẽ. Cache đọc tự hết hiệu lực khi dữ liệu CAD thay đổi; bản sao trả về không làm hỏng cache.
- Thống kê ký hiệu, nhãn và điểm cũ trong một lượt duyệt bản vẽ.
- Tìm kiếm chờ 180 ms sau lần gõ cuối để giảm việc dựng lại danh sách liên tục.
- Thư viện nạp ảnh trong vùng đang xem và hàng kế tiếp. Cache ảnh giới hạn 120 mẫu; mỗi cửa sổ giữ bản sao riêng.
- Khi tính lý trình hàng loạt, dùng chỉ mục điểm trong phạm vi một lượt tính, chỉ đưa cọc hợp lệ vào danh sách tra cứu và đọc mỗi dictionary một lần. Giữ nguyên công thức, ngưỡng và quy tắc xác nhận mốc.

Trên bộ thử tổng hợp 5.000 hồ sơ, trung vị năm lượt giảm từ 45,28 giây xuống 4,63 giây. Thời gian gồm tính lý trình và ghi hồ sơ, không gồm vẽ lại ký hiệu. Sai số lớn nhất so với kết quả hình học chuẩn là 0,5 mm. Đây là số đo trên máy phát triển; bản vẽ nhiều nhánh hoặc nhiều mốc có thể mất thêm thời gian.

## Bản vẽ mẫu

Mẫu chính: `261011_test.dwg`, gồm 801 điểm vị trí, 177 hồ sơ và 334 ảnh. Đã thử tính trên bản sao, lưu và mở lại; kết quả hồ sơ trùng bản 0.6.68, ngoài thời điểm cập nhật. Tọa độ và nguồn điểm được giữ nguyên.

Trong mẫu còn một điểm đã lệch khỏi số liệu nhập, hai hồ sơ hai trụ thiếu chân trụ, một cảnh báo hệ số mốc tim tuyến và một ảnh chưa có ký hiệu. Ngoài ra, 104 hồ sơ cần đối chiếu điều kiện mốc trước khi tự nhận kết quả tính. Chi tiết có trong báo cáo và bảng hồ sơ đi kèm.

## Cài và sử dụng

1. Lưu công việc và đóng mọi phiên AutoCAD.
2. Chạy `BHT-Setup-1.0.0.exe`, mở lại AutoCAD 2024 rồi gõ `BHT` hoặc `BTH`.
3. Mở bản sao thử nghiệm trước. Vào Tổng quan → Xem mốc đã nhập để kiểm tra cột Km và cọc tiêu.
4. Trong trang Hồ sơ đối tượng, chọn bộ lọc Lý trình cần kiểm tra để xử lý từng trường hợp. Chỉ xác nhận cặp mốc sau khi đã đối chiếu số cọc và vị trí.

Bộ cài cục bộ chứa thư viện DWG/phông. Không tự đổi hình học khi mở bản vẽ. Mẫu bàn giao là bản sao kiểm thử, có kết quả và cảnh báo; chưa phải hồ sơ đã nghiệm thu. Chưa thử cài trên máy sạch khác và chưa nghiệm thu trên phiên bản CAD khác. Việc gửi một DWG không cần thêm phông vẫn chưa được bảo đảm.

