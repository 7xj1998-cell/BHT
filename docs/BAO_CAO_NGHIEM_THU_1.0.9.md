# BHT 1.0.9 — Báo cáo kiểm thử nhãn

Ngày 12/10/2026. Môi trường: AutoCAD 2024 trên máy phát triển hiện tại. Phạm vi: khoảng hở nhãn báo hiệu, gom/xoay cụm nhãn RTK và Timemark, nhãn cọc tiêu bổ sung. Không nghiệm thu toàn bộ ứng dụng ở mọi môi trường.

## Kết quả sửa

Nhãn tên biển và lý trình vẫn nằm dưới hình. BHT tăng khoảng hở tối thiểu dưới hình, kiểm tra đoạn đường nối trong hệ trục của biển và dịch cả cụm chữ sang bên khi đường nối đi qua vùng nhãn có đệm. Các dòng tên mặt biển và lý trình dùng cùng vị trí cột; lý trình ở cuối cụm. Chỉ số đường nối được tạo theo từng lượt đồng bộ, nhận cả đường mới tạo và không lưu lâu dài qua các thao tác CAD.

Nút **Gom / xoay nhãn…** thay thế **Xoay nhãn RTK…**. Lệnh sắp tên, mô tả, cao độ và mã điểm thành một cụm gần điểm; khoảng dòng bằng 1,5 lần cỡ chữ lớn hơn giữa hai dòng kề nhau. Hướng lấy theo UCS hoặc A → B. Nhãn và ký hiệu ảnh Timemark cùng xoay; đồng bộ ảnh giữ hướng và vị trí tương đối của nhãn. POINT đổi góc hiển thị, không đổi tọa độ hay XData khảo sát. Thao tác này chủ động bố trí lại cả nhãn đã kéo tay; cập nhật nhãn thông thường vẫn giữ vị trí tay.

Cọc bổ sung có số Km/H hiện H3/49 theo hồ sơ thay vì mã OBJ và mô tả dài. Cọc chưa biết số vẫn hiện Cọc tiêu, không tạo lý trình. Vẽ/cập nhật ký hiệu cọc bổ sung tạo nhãn điểm nếu thiếu; không tự xác nhận cọc làm mốc tính lý trình. Trong mẫu có cọc đã lưu hồ sơ nhưng chưa có hình ký hiệu; các cọc này được tạo hình qua bước vẽ/cập nhật riêng.

## Bằng chứng

| Bộ kiểm tra | Kết quả | Phạm vi |
| --- | --- | --- |
| Core | 233 đạt, 0 lỗi | Build DLL 1.0.9 |
| LABEL109 | 30 đạt, 0 lỗi; lưu/mở lại đạt | Nhãn bị tách và khác góc; 5 góc xoay; RTK, Timemark, đồng bộ ảnh, cọc có/chưa có số, không tạo cao độ cho điểm thủ công, nhãn tránh đường nối |
| LABEL108 | 27 đạt, 0 lỗi; lưu/mở lại đạt | Tên/lý trình hai dòng, nhiều mặt biển, sửa/xóa lý trình, cập nhật lặp, tỷ lệ, giữ block đèn |
| RTK104 | 27 đạt, 0 lỗi; lưu/mở lại đạt | UCS, A → B, hủy, hướng 0, khóa lớp, TEXT ngoài BHT, phục hồi lỗi ghi giữa chừng, Undo, tọa độ giữ nguyên |
| ROAD106 | 44 đạt, 0 lỗi; Undo và lưu/mở lại đạt | Vùng bao mới trong bố trí theo tim/PL, hai phía, cong/gãy và xử lý lỗi |
| Cọc bổ sung | Đạt; lưu/mở lại và Undo đạt | Tạo cọc, số Km/H, nguồn thủ công, cọc chưa biết số, không tự dùng làm mốc |
| Giao diện | 2.676 kiểm tra đạt | Bố cục và liên kết thao tác của Palette |
| BOT830 | 6 kiểm tra đạt; mở lại đạt | 801 điểm, 177 hồ sơ; 2.113 nhãn điểm, 331 nhãn Timemark; 10 cọc tiêu bổ sung có nhãn; 25/25 biển có lý trình giữ đúng bố cục hai dòng |

Các bộ LABEL109, LABEL108, RTK104, ROAD106 và BOT830 đã chạy với FAS và DLL bảo vệ của bản 1.0.9. Kiểm tra cọc bổ sung và giao diện dùng DLL/source 1.0.9.

Dữ liệu mẫu: bản sao **C:\Users\Le Bao\Desktop\BOT\261010.Bienbao BOT830.dwg**. Đã đối chiếu SHA256 file nguồn trước/sau, không ghi đè bản gốc. Đã so sánh điểm, hồ sơ, PHOTO, ROUTE, FIELD_CHAIN và vị trí GPS trước/sau thao tác. Nhãn cọc lấy từ số thực tế đã nhập trong hồ sơ.

Thời gian gom/xoay 2.113 nhãn điểm và 331 nhãn Timemark là **1.203 ms** trong một lượt. Cập nhật mẫu hình/nhãn trên bản sao là **20.016 ms** trong một lượt. Các số này không phải trung vị 5 lượt hoặc phép nghiệm thu tốc độ tính lý trình. Không thể kết luận mọi thao tác đều dưới một giây.

## Giới hạn

- MCP kết nối tới Drawing1.dwg trong C:\Windows\system32, khác bản vẽ người dùng. Vì vậy đợt này dùng AutoCAD Core Console 2024 trên bản sao DWG đã lưu; chưa xác nhận giao diện và ảnh hiển thị trong phiên CAD người dùng đang mở.
- Việc gom chữ về gần điểm không bảo đảm hết chồng nhãn giữa các điểm rất gần nhau. Cần kiểm tra bản vẽ ở tỷ lệ in thực tế; bố trí tự động hoặc dịch nhãn riêng khi cần.
- Chưa nghiệm thu trên máy sạch hoặc CAD khác. Chưa có người thử độc lập.
- Gói có thư viện/phông đi kèm chỉ dùng nội bộ; không công bố tài nguyên bên thứ ba.

## Cách cập nhật bản vẽ cũ

1. Cài BHT-Setup-1.0.9.exe và khởi động lại AutoCAD 2024.
2. Trong Tuyến & báo cáo → Công cụ bổ sung, bấm **Cập nhật mẫu biển đã vẽ** để sửa nhãn biển và cọc đã có hình.
3. Trong Điểm vị trí, bấm **Cập nhật nhãn RTK**, rồi **Gom / xoay nhãn…**; chọn UCS hoặc hướng A → B. Nếu cọc đã có hồ sơ nhưng chưa có hình, mở hồ sơ cọc và bấm **Vẽ / cập nhật ký hiệu**.
4. Lưu DWG. Dùng U ngay sau thao tác gom/xoay nếu muốn phục hồi cả lần thay đổi.

## Tệp đi kèm

- BOT830-Nhan-gon.dwg: bản sao sau kiểm thử, giữ nguyên tọa độ khảo sát và hồ sơ.
- Nhan-gon-va-duong-noi.dwg: mẫu nhỏ tái hiện nhãn rời, Timemark, cọc có/chưa có số và đường nối biển.
- Các log LABEL109, LABEL108, RTK104, ROAD106, cọc bổ sung, giao diện, runtime và bộ cài; hướng dẫn và danh mục lệnh/nút dạng Markdown và HTML.

Bản mã nguồn trước đợt sửa được lưu tại D:\lisp\BHT\old\pre-1.0.9.
