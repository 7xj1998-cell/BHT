# BHT 0.6.62 — Lý trình theo cọc tiêu thực địa

## Thay đổi
- Chế độ Cọc tiêu thực địa độc lập với tim đường và các Station Control đã khai báo. Chỉ hồ sơ COC_TIEU có Km/H và đầy đủ điểm RTK được dùng làm mốc; COT_KM không tự tham gia.
- Tab Tuyến & báo cáo có Tạo/Sửa/Xóa chuỗi cọc. Chuỗi lưu thứ tự ID cọc do người dùng xác nhận trong DWG; không lưu bản sao số Km/H hoặc vị trí. Có bảng so sánh chiều dài đoạn nối và chênh lý trình để kiểm tra.
- Ở ô lý trình: Tính lý trình theo chế độ đang chọn; Chọn 2 cọc… tính cho hồ sơ hiện tại; Theo chuỗi đang chọn bỏ cặp riêng và tính lại theo chuỗi.
- Nội suy trong mặt phẳng XY giữa hai cọc, không ngoại suy. Khi nhiều đoạn cùng bao quanh một vị trí, yêu cầu chọn cặp; không đoán đoạn từ khoảng cách gần nhất.
- Lỗi cọc/thứ tự chỉ ảnh hưởng đoạn liên quan. Đoạn quá dài hoặc có hệ số bất thường không được tự ghi kết quả: cần chọn và kiểm tra hai cọc. Kết quả từ cặp được chọn rõ ràng vẫn ghi trạng thái CẦN KIỂM TRA nếu vượt ngưỡng.
- Nguồn tính, hai cọc, tỷ phần và khoảng lệch đường nối cọc lưu riêng. Không ghi khoảng lệch này vào trường lệch tim; giữ nguyên phía đường thực địa.
- Xoay các biển vuông góc theo cọc dùng cặp riêng/chuỗi đã chọn; giữ vị trí chèn và tỷ lệ. Đoạn chưa xác định hoặc chưa xác nhận cảnh báo được bỏ qua.
- Có thể chuyển về phương pháp theo tim tuyến. Không tự thay đổi phương pháp khi mở bản vẽ cũ hoặc tự vẽ lại ký hiệu.

## Cách dùng với bản vẽ không có tim đường gốc
1. Lưu số Km/H vào hồ sơ các cọc tiêu, kiểm tra điểm RTK gắn đúng cọc.
2. Với một báo hiệu: mở hồ sơ → Chọn 2 cọc… → thêm cọc đầu và cuối theo chiều Km tăng → kiểm tra bảng khoảng cách → Tính từ 2 cọc.
3. Với nhiều báo hiệu: tab Tuyến & báo cáo → Cọc tiêu thực địa → Tạo chuỗi… → thêm cọc cùng đường/nhánh theo thứ tự thực địa, chỉnh Lên/Xuống → Lưu và dùng chuỗi → Cập nhật lý trình.
4. Khi cần đổi nguồn cho một hồ sơ đã có cặp/chuỗi riêng: chọn chuỗi mặc định rồi bấm Theo chuỗi đang chọn tại hồ sơ đó.
5. Chỉ bấm Xoay các biển vuông góc theo cọc khi muốn cập nhật hướng các biển đã chèn.

Mặc định: phạm vi lệch 100 m, cảnh báo đoạn dài 500 m, cảnh báo hệ số lệch 3%. Các ngưỡng này được hiển thị và chỉnh trong chuỗi. Cặp riêng hiển thị các mặc định trên. Đây là kiểm tra hợp lý dữ liệu, không phải cam kết độ chính xác lý trình thực địa. Khoảng dài hoặc đường cong cần bổ sung cọc/đối chiếu hình học hiện trạng.

## Lưu trữ và tương thích
- FIELD_CHAIN: chain_id, name, marker lặp theo thứ tự, max_offset, max_span, ratio_tol, revision.
- META: station_method=FIELD_MARKERS/ROUTE; field_active_chain.
- OBJ: field_chain_id; field_pair_a/b (cặp chọn riêng); field_used_a/b; field_fraction; field_offset_m; field_last_error; station_source=FIELD_MARKERS.
- Sửa/xóa chuỗi đánh dấu kết quả gắn chuỗi cần tính lại. Sửa Km/H, RTK hoặc xóa cọc có hiệu lực khi người dùng tính lại. Không ghép tắt qua cọc đã mất.
- Lỗi tính giữ lý trình cũ và báo rõ. Cập nhật hàng loạt đánh dấu hồ sơ thất bại cần kiểm tra; cọc có số giữ lý trình đã nhập.
- Giữ các chức năng biển tùy chỉnh ghép nhiều mặt, đèn, thư viện biển và giao diện từ 0.6.61.

## Kiểm thử AutoCAD 2024
- 233 kiểm thử Core; 718 kiểm tra giao diện/luồng sửa hồ sơ, gồm thêm/xếp cọc, cảnh báo đảo thứ tự, giới hạn hai cọc và Hủy.
- 51 kiểm thử cọc thực địa; kiểm tra lưu/mở lại DWG và Undo qua lời gọi API .NET. Đạt với Lisp nguồn và FAS + DLL được bảo vệ.
- 35 kiểm tra hướng biển trước đó đạt.
- Bản sao dữ liệu phiên CAD đang mở: OBJ-000153 dùng OBJ-000152 và OBJ-000074 đạt Km39+901.95, lệch đường nối cọc 0.094 m; xoay được một biển. RTK, hồ sơ cọc nghi sai và dữ liệu mốc tuyến cũ giữ nguyên. Chuỗi thử dùng ngưỡng cảnh báo độ dài 600 m cho đoạn dài 500.57 m.
- Chưa nghiệm thu trên máy sạch khác; chỉ thử AutoCAD 2024 trên máy phát triển.

Bộ cài cục bộ vẫn kèm thư viện biển/đèn/phông hiện có. Không công bố bộ tài nguyên bên thứ ba lên GitHub. Chưa thay đổi phương án bàn giao chữ không phụ thuộc phông.
