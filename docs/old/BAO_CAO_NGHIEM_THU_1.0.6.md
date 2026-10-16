# BHT 1.0.6 — Báo cáo kiểm thử bố trí báo hiệu

Ngày kiểm thử: 12/10/2026. Chỉ dùng AutoCAD 2024. Kết quả dưới đây áp dụng cho mã và bộ chạy được bảo vệ của 1.0.6; không thay thế nghiệm thu hồ sơ thiết kế.

## Hành vi đã thay đổi

Công cụ **Bố trí tất cả báo hiệu… / BHTKHOANGCACH** có ba cách: theo tim đường đã nạp, ngoài mép đường bằng PL chọn trực tiếp và khoảng cách từ điểm đo theo hướng đã đặt. Khoảng cách đến tim/mép tính tới vùng bao hình và nhãn, không tính tới gốc block.

Chỉ dịch ký hiệu và nhãn, cập nhật đường nối. Giữ điểm khảo sát, lý trình, mốc/tuyến, góc và tỷ lệ. Trái/phải suy từ điểm đo theo chiều tham chiếu. Tim cong/gãy khúc dùng hướng tại vị trí chiếu từng hồ sơ; dùng vùng bao an toàn để toàn bộ hình ở ngoài khoảng cách tối thiểu. Không tự đổi góc ký hiệu.

## Kết quả đã có bằng chứng

| Bộ thử | Kết quả | Phạm vi |
| --- | --- | --- |
| Bố trí ROAD106 | 34 ca đạt, 0 lỗi | Hai phía; chiều tham chiếu đảo; hình xoay/scale; hai trụ; đèn; tim đã nạp; đoạn cong/gãy khúc; Polyline 2D dạng cũ |
| An toàn dữ liệu | Đạt trong bộ ROAD106 | Không đổi điểm đo/lý trình/tim; khóa lớp; không có vùng bao; điểm sát tim; ngoài đầu/cuối; thiếu tim; hủy thao tác |
| Phục hồi | Đạt | Gây lỗi lúc vẽ đường nối: phục hồi hình, bản ghi và cấu hình; sửa lỗi cập nhật thời điểm sửa khi phục hồi |
| Undo | Đạt trên Core Console | Một Undo trả hình, hồ sơ và cấu hình về trước lần bố trí |
| Lưu/mở lại | Đạt | Tham chiếu, chiều, bề rộng và kiểu nối được lưu; mở lại DWG bằng bộ chạy bảo vệ |
| Chế độ đường nối DISPLAY105 | 20 ca đạt, 0 lỗi | Thẳng/gấp khúc/điểm trung gian và ưu tiên kiểu riêng; giữ điểm khảo sát |
| Giao diện Palette | 2.615 kiểm tra đạt | Bố cục, nhập số dấu phẩy, từ chối số sai, khôi phục lựa chọn, tim đã nạp, chọn kiểu nối; các điều khiển được tạo và kiểm tra bằng WinForms |
| BOT830 — bản sao | 120/120 ký hiệu đạt kiểm tra khoảng lùi | 801 điểm, 177 hồ sơ; không đổi điểm, góc, tỷ lệ và dữ liệu lý trình/mốc/tuyến |
| Tốc độ trên bản sao | 2.125 ms cho 120 ký hiệu | Một lượt với FAS/DLL bảo vệ, chạy cùng các bộ thử khác; phép đo gồm lập kế hoạch và cập nhật hình/đường nối, chưa phải trung vị 5 lượt |
| Core | 233 ca đạt | Đã chạy lúc build; không thay bộ máy tính lý trình trong đợt này |

Các bài thử mới đã chạy với **FAS và DLL bảo vệ**. Các ca hủy/đầu PL được kiểm tra bằng đầu vào thay thế để chạy tự động; chưa phải thao tác chuột trong phiên CAD thật.

PL trên bản sao BOT830 nằm ở lớp **BHT_QA_THAM_CHIEU**, được tạo từ phạm vi điểm để kiểm tra thuật toán. Bề rộng 20/20 và khoảng lùi 2 là số liệu kiểm thử. Đây **không phải tim/mép đường đã khảo sát** và không dùng nghiệm thu vị trí thực tế. Không ghi đè DWG nguồn.

## Giới hạn cần đọc trước khi dùng

- MCP chưa xác nhận được bản vẽ đang mở của người dùng. Chưa chạy nghiệm thu tương tác chọn PL/đầu PL và các nút trong phiên CAD đó; không kết luận ảnh người dùng đã được sửa.
- Chưa có nghiệm thu cài trên máy sạch hoặc người dùng thứ hai. Không thử phiên bản CAD khác 2024.
- Chỉ nhận tim/Polyline 2D mở trong Model, trên mặt phẳng XY; không dùng proxy TDT trực tiếp, tim kín hoặc Polyline 3D. Nạp tim thường từ công cụ TDT hiện có vẫn được giữ.
- Không tự đo bề rộng đường, không suy lý trình từ PL bố trí, không tự tránh chồng giữa biển với biển hoặc với nội dung bản đồ nền.
- Khoảng lùi vùng bao có thể lớn hơn khoảng lùi nhìn tới nét thật; đoạn cong/gãy khúc dùng vùng bao tròn bảo thủ. Trường hợp không tìm được vị trí an toàn sẽ được bỏ qua và báo ID.
- Báo cáo tốc độ này không phải phép thử ngưỡng tính lý trình 100/1.000/5.000 điểm đã chốt trước đó.
- Xuất một DWG không cần phông ngoài chưa được hoàn thiện; chức năng mới không thay cơ chế bàn giao phông.

## Tệp kiểm tra đi kèm

- `Bo-tri-ngoai-duong.dwg`: ví dụ nhỏ có hai phía, biển xoay, biển hai trụ, đèn, điểm trên tim và điểm ngoài phạm vi; có tuyến thẳng/gãy và cung cong để xem lại.
- `BOT830-bo-tri-ngoai-duong.dwg`: bản sao mẫu với PL minh họa kiểm thử.
- `road106-create.log`, `road106-reopen.log`, `sample-road106.log`, `display105-create.log`, `palette-result.txt`: bằng chứng bộ thử.
- `road-placement.png`, `center-placement.png`: hộp bố trí ngoài đường và theo tim.

Trước khi áp dụng hồ sơ thật: chọn đúng tim/PL và chiều, lấy bề rộng từ hồ sơ, đối chiếu vài báo hiệu ở hai bên và đoạn cong/gãy, rồi đọc các ID chưa bố trí tại F2. Nếu cần trả toàn bộ lần vừa đặt, dùng **U** ngay sau thao tác.
