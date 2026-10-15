# BHT 1.0.7 — Báo cáo sửa lỗi nhận tim tuyến

Ngày kiểm thử: 12/10/2026. Chỉ dùng AutoCAD 2024. Mã trước khi sửa được sao lưu tại `D:\lisp\BHT\old\pre-1.0.7`.

## Nguyên nhân đã xác định

Đã đọc bản sao của tệp đã lưu `C:\Users\Le Bao\Desktop\BOT\261010.Bienbao BOT830.dwg`, không ghi đè bản gốc. Hồ sơ `TUYEN1` có loại `KHAC`, handle `22864`, chiều dài 13942.397560 đơn vị. Hình học là LWPOLYLINE 2D mở, 89 đỉnh, thuộc Model, trên mặt phẳng XY; đạt điều kiện bố trí.

Bản 1.0.6 dùng bộ lọc `loai == TIM_DUONG` trong giao diện và kiểm tra tương tự trong Lisp. Vì vậy tuyến vẫn hiện trong bảng quản lý nhưng bị loại khỏi lựa chọn Cách tim.

## Cách sửa

Danh sách Cách tim nhận tất cả tuyến đã nạp, hiển thị ID và loại để người dùng chọn rõ đường làm tim bố trí. Gửi ID thật cho Lisp, giữ nguyên phân loại và dữ liệu tuyến. Thêm cột Loại trong bảng quản lý; sửa chỗ đọc cột nguồn để các công cụ TDT vẫn hiện đúng theo tuyến được chọn.

Kiểm tra hình học vẫn thực hiện trước khi dịch báo hiệu. Không nhận đường đã xóa, tim kín, Polyline 3D hoặc proxy TDT trực tiếp. Nạp tuyến TDT thành tham chiếu thường bằng chức năng hiện có vẫn được giữ.

## Kết quả có bằng chứng

| Bộ kiểm tra | Kết quả | Phạm vi |
| --- | --- | --- |
| Core | 233 ca đạt, 0 lỗi | Chạy khi build, không sửa bộ máy lý trình |
| ROAD106 mở rộng | 44 ca đạt, 0 lỗi | Bố trí hai bên, tuyến cong/gãy, tuyến loại Khác/Tim rãnh/chưa phân loại/chữ thường, giữ loại và mốc, thiếu tuyến, tuyến kín hoặc đã xóa |
| Undo và phục hồi | Đạt | Một Undo hoàn tác lần bố trí; lỗi cập nhật đường nối phục hồi dữ liệu và hình |
| Giao diện | 2.676 kiểm tra đạt | Danh sách hiển thị loại, trả đúng ID, chọn tuyến, nhập số, bố cục và công cụ TDT theo cột nguồn |
| DWG BOT830 đã lưu — bản sao | 120 báo hiệu được bố trí, 0 bỏ qua | Dùng chính TUYEN1 loại Khác, 801 điểm, 177 hồ sơ |
| Dữ liệu khảo sát và lý trình | Đạt trên bản sao | Giữ điểm, góc, tỷ lệ, loại tuyến, mốc, chuỗi cọc, lý trình và phía trong hồ sơ |
| Khoảng cách hình/nhãn | 120/120 đạt | Kiểm tra vùng bao hình và nhãn cách tim ít nhất 20 đơn vị; 20 là giá trị thử |
| Lưu/mở lại | Đạt | Giữ tim TUYEN1, loại Khác, chế độ Cách tim và khoảng cách 20 |
| Thời gian thử | 2.031 ms cho 120 báo hiệu | Một lượt trên bản sao, FAS/DLL bảo vệ; chưa phải trung vị 5 lượt |

Tệp nguồn được đối chiếu SHA256 trước/sau và không thay đổi. Bản sao mẫu dùng **khoảng cách thử 20 đơn vị**, không phải thông số bố trí đã được nghiệm thu cho hồ sơ thực tế.

## Bằng chứng đi kèm

- `BOT830-CachTim-TUYEN1.dwg`: bản sao kết quả thử, không ghi đè bản gốc.
- `sample-center107.log`, `sample-center107-reopen.log`: kiểm tra trên tim TUYEN1 và mở lại bản sao.
- `road106-create.log`, `road106-reopen.log`: 44 ca bố trí, Undo và lưu/mở lại.
- `palette-result.txt`, `center-loaded-route.png`: kết quả và ảnh hộp chọn tuyến loại Khác.

## Phần chưa nghiệm thu

- MCP trả về `Drawing1.dwg`, không khớp bản vẽ trong ảnh. Chưa kiểm thử thao tác trực tiếp trong phiên CAD của người dùng; nội dung chưa lưu có thể khác bản DWG đã đọc.
- Chưa cài thử trên máy sạch hoặc máy của người dùng thứ hai. Không tự cài, đóng CAD hay lưu bản vẽ đang mở.
- Kiểm tra giao diện dùng các điều khiển WinForms trong bộ thử; không thay thế toàn bộ thao tác chuột thực tế.
- Chức năng chỉ bố trí hình theo tim đã chọn; không xác nhận tim đó đúng với khảo sát, không đổi phương pháp tính lý trình và không tự tránh chồng giữa các biển.
- Không thay cơ chế phông chữ khi bàn giao DWG. Xuất một DWG không cần phông ngoài chưa được hoàn thiện.

Để dùng: cài 1.0.7 sau khi lưu công việc và đóng CAD, chọn **TUYEN1 — Khác** trong hộp Cách tim, nhập khoảng cách thực tế và kiểm tra vài biển ở hai bên/đoạn cong. Nhấn F2 để xem các ID chưa bố trí; dùng U ngay sau thao tác nếu cần hoàn tác.
