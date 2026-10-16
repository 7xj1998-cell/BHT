# BHT 1.0.25 — Xử lý theo đoạn và kế thừa dữ liệu

Đã bổ sung quy trình tính lý trình riêng cho từng đoạn, xác nhận kết quả và xuất dữ liệu tích lũy. Các báo hiệu đã nhập trong bản tổng hợp được sử dụng lại; không phải nhập lại từng biển khi chuyển sang DWG đoạn khác.

## Chức năng

- **Nạp dữ liệu kế thừa (.bht)** giữ hồ sơ, điểm, ký hiệu, nhãn lý trình và kết quả đã xác nhận. Hồ sơ trùng được đối chiếu theo ID và tọa độ; xung đột bị chặn. Nạp lại cùng gói không tạo thêm hồ sơ.
- **Quản lý xử lý theo đoạn** hiển thị bảng đoạn/gói, tên công trình, khoảng Km, phía đường, tim đã gán, số hồ sơ và số đã xác nhận.
- **Tính lý trình theo đoạn** dùng tim được chọn, có bảng xem trước hồ sơ được tính và hồ sơ bị loại. Giữ các hồ sơ đã xác nhận và hồ sơ đang thuộc đoạn khác.
- **Xác nhận kết quả đoạn** kiểm tra lại số liệu trước khi bảo vệ kết quả. **Mở lại kết quả đoạn** cho phép chỉnh sửa đúng đoạn cần xử lý.
- **Xuất dữ liệu tích lũy** chứa toàn bộ dữ liệu hiện có, kể cả bảng phân đoạn. **Xuất hồ sơ đoạn** chỉ chứa hồ sơ đã xác nhận của đoạn được chọn cùng điểm, ảnh liên kết, ký hiệu, nhãn và tim liên quan.
- Chặn xuất nếu điểm hoặc hình học tim của hồ sơ đã xác nhận bị thay đổi. Chặn đổi tên/xóa tim đang dùng bởi kết quả đã xác nhận trước khi sửa các hồ sơ liên quan.

## Quy trình DT.830

1. Tiếp tục dùng DWG đoạn 10 hiện có. Nạp đủ bảng 15 đoạn nếu chưa có. Không nhập lại báo hiệu đã lưu.
2. Kiểm tra tim TDT, loại tim, chiều tuyến và mốc lý trình của đoạn. Dùng DTST/BTST trong môi trường TDT khi cần hiện thông số; BHT cần ít nhất hai mốc hợp lệ.
3. Mở **BHTXULYDOAN**, chọn DOAN_10 và tim đúng. **Tính lý trình theo đoạn**, xem bảng dự kiến rồi áp dụng.
4. Đối chiếu lý trình, phía đường, các trụ và DIM; xử lý các hồ sơ bị loại hoặc gán nhầm đoạn. **Xác nhận kết quả đoạn**.
5. **Xuất dữ liệu tích lũy** thành tệp mới, ví dụ DT830-sau-doan10.bht.
6. Mở DWG đoạn 11, dùng **BHTNAPTICHLUY** để nạp tệp này. Khai báo tim/mốc đoạn 11, chọn DOAN_11 và tính. Lý trình cùng nhãn đã xác nhận của đoạn 10 tiếp tục được giữ.
7. Xuất tệp tích lũy mới sau mỗi đoạn. Khi bàn giao từng đoạn, dùng **Xuất hồ sơ đoạn** và lưu DWG riêng.

**Nạp khảo sát mới (tính lại lý trình)** là chức năng khởi tạo lại kết quả tính theo tim đích. Để tiếp tục kế thừa kết quả đã xử lý, dùng **Nạp dữ liệu kế thừa**.

## Các điểm phải đối chiếu tại đoạn 10

Bản DWG BD_TD_TN(Km50+140-Km51+634.24).dwg có ba cọc thủ công OBJ-000182, OBJ-000183, OBJ-000184 đang gán DOAN_10 nhưng bị XA_TUYEN, khoảng cách tới tim lần lượt 24,52 m; 21,08 m; 20,90 m. Cần kiểm tra vị trí khảo sát, tim và phạm vi lệch cho phép. Không tăng ngưỡng chỉ để vượt kiểm tra. Nếu các cọc không thuộc đoạn, bỏ gán sau khi đối chiếu; số Km/H quan sát được giữ riêng.

OBJ-000058 lưu phía đường PHAI trong khi DOAN_10 là TRAI. TIMRANHD10 còn khai báo TIM_DUONG; chỉ sửa thành Tim rãnh khi đã xác nhận đúng loại. Bảng đoạn lưu cuối đoạn 10 là Km51+634,00, còn tên DWG là Km51+634,24; cần đối chiếu bảng phân chia gói thầu và hồ sơ thiết kế để xác định mốc cuối chính thức.

## Phạm vi sử dụng

Hai DWG phải dùng cùng hệ tọa độ thực tế; BHT không tự chuyển hệ tọa độ. Tệp ảnh gốc vẫn là liên kết ngoài .bht. Xác nhận đoạn bảo vệ những hồ sơ đang thuộc đoạn, không chứng minh đã khảo sát đủ mọi báo hiệu ngoài thực địa. Bản 1.0.25 đọc gói BHT cũ; nên dùng cùng bản này để tiếp tục chỉnh sửa gói đã xác nhận. Bản BHT cũ không có cơ chế bảo vệ kết quả mới.

Đã kiểm thử tính theo hình học tham chiếu và mốc lưu trong bản sao DWG thực tế; chưa nghiệm thu lại toàn bộ thao tác đọc native TDT trong AutoCAD đầy đủ.
