# Nghiên cứu dữ liệu tuyến TDT bằng REA

Ngày: 14/10/2026. Công cụ: skill `reverse-engineer-anything`, REA/Ghidra 12.1.4. Phạm vi là đọc dữ liệu kỹ thuật tuyến phục vụ BHT; không sửa thư viện TDT.

## Kết luận kỹ thuật

**Không dùng `TdtDbAlignment::GetStationAtPoint` để chiếu điểm báo hiệu trên bình đồ.** Phân tích thân hàm đã cho thấy nó gọi `TdtVerticalAlignmentManager::GetDistanceAtPoint`. Hàm con lấy tọa độ X, tỷ lệ trắc dọc và các đường dóng, rồi mới cộng lý trình gốc. Hàm đối ứng `GetPointAtStation` cũng đi qua bộ quản lý trắc dọc. Đây là kết luận từ mã máy, chưa phải phép đo chạy thực tế.

Phát hiện này thay thế giả thuyết cũ rằng cặp hàm trên có thể trực tiếp đổi tọa độ bình đồ ↔ lý trình/khoảng cách lệch tim. Tên hàm và chữ ký export không đủ xác định hệ tọa độ đầu vào.

TDT có mô hình dữ liệu không phụ thuộc chữ Km trên màn hình:

- `TdtDbAlignment` chứa hình học tuyến, bộ quản lý cọc và lý trình gốc.
- `TdtGeAlignment::GetGeometry` tạo polyline từ các đoạn tuyến; `subExplode` của đối tượng tuyến sao chép polyline nội bộ rồi bổ sung hình cọc.
- `TdtGePoleManager::GetChainageAtPole` cộng trường khoảng cách từng cọc. `UpdateChainageOfAPole` đối chiếu tổng đó với khoảng cách dọc polyline.
- `TdtGePoleManager::DisplayInformation` cộng **lý trình gốc + khoảng cách từng cọc + trường hiệu chỉnh từng cọc** để tạo thông tin lý trình. Các hàm liên quan đứt thước tồn tại, nhưng chưa xác nhận đầy đủ quy ước hiệu chỉnh tại hai phía điểm đứt thước.
- `GetPoleLocations` lấy tọa độ được lưu trong từng cọc. `dwgOutFields` ghi khoảng cách/hiệu chỉnh/tên cọc; việc ghi tọa độ phụ thuộc cờ trạng thái cọc. Không được giả định mọi cọc đều có tọa độ hợp lệ.

Do đó, yêu cầu phải bật DTST/BTST hiện nay xuất phát từ luồng BHT quét nhãn hiển thị. Mã TDT cho thấy một hướng đọc dữ liệu gốc khả thi; chưa đủ bằng chứng để công bố BHT đã làm được việc này trong phiên TDT của người dùng.

## Phương án tích hợp

1. Chọn đúng đối tượng `TdtDbAlignment`, kiểm tra không phải proxy và xác định đúng module đang nạp. Tên/handle tuyến phải gắn với đoạn và loại tim đã chọn.
2. Đọc hình học tim, chiều tuyến, lý trình gốc và danh sách cọc gốc bằng bộ kết nối native tương thích bản TDT đã kiểm chứng. Ưu tiên giao diện do nhà cung cấp hỗ trợ; không đưa các offset bộ nhớ nghiên cứu vào luồng hồ sơ.
3. Đối chiếu khoảng cách tích lũy với vị trí cọc trên tim. Lập quan hệ khoảng cách dọc tim → lý trình có phân khoảng tại đứt thước. Không nội suy xuyên qua bước nhảy và không mặc định đầu polyline là Km0.
4. Chiếu điểm trụ lên tim của đoạn để lấy khoảng cách dọc tuyến và độ lệch ngang. Với hai trụ, dùng trụ gần tim theo yêu cầu đã thống nhất; giữ định danh trụ dùng tính toán.
5. So sánh kết quả với TDT tại đầu, giữa, cuối tuyến, đoạn cong, trái/phải tim, tuyến đảo chiều và hai phía đứt thước. Thử khi nhãn Km bật và tắt; kết quả dữ liệu phải không đổi. Trạng thái thiếu/không xác định không được đổi thành số 0.
6. Khi kiểm chứng đạt, bổ sung thao tác **Đọc dữ liệu tuyến TDT** trong BHT: lấy tim và bảng mốc trong một lần, cho xem trước nguồn, tên tuyến, lý trình đầu/cuối và sai số trước khi gán cho đoạn. Bộ chuyển đổi TDT là một nguồn nhập riêng, để BHT vẫn dùng được với polyline/mốc và dự án khác.

Đối với DT.830, dữ liệu mỗi đoạn cần lưu cả hình học và quan hệ lý trình đã xác nhận vào gói BHT. Khi nhập thêm đoạn khác, giữ kết quả đã xác nhận của các đoạn trước, chỉ tính theo tim của đoạn đang xử lý. Không lấy nhãn cọc tiêu hoặc nhãn báo hiệu cũ làm mốc tuyến TDT.

## Hồ sơ bằng chứng

Đối tượng phân tích: `C:\Program Files (x86)\TDT Solution 2022\AlignmentDb.dbx`, PE x64.

SHA-256: `ead3d32abdad019d05100e5ddf70cdb7fdaf6785b4d253a6078392e1bed60f28`.

| Kết luận/quan sát | Địa chỉ | REA Evidence ID |
| --- | --- | --- |
| GetStationAtPoint gọi bộ quản lý trắc dọc, cộng trường lý trình gốc | 0x1800e5b50 | ev_7b9b4d8c242fd05f536fa7cd84be7c0e174bb4e29a778ada0e934ca54a729d62 |
| Hàm con dùng X, tỷ lệ và đường dóng | 0x18019cd30 | ev_7c76c70571a9d9eaf80607f873cd0766d3947afdac45e781db9f2897392a0767 |
| GetPointAtStation đi qua trắc dọc | 0x1800e5bf0 | ev_7063fe0721825dfd732110b970b8528fffd29988d9c331703db83d912f43ee5e |
| Chuỗi tại địa chỉ được subList sử dụng là “Lý trình gốc” | 0x1801e6130 | ev_31cc4af620898fd27f00f97613f9ee3473c02d2e8dd713c9413049f5509693af |
| subList đọc lý trình gốc của tuyến | 0x1800e9490 | ev_cd7e73f1167ee6f59a9bfa5634a1ac75d5d7deee75e4989978357eec5cfb05ac |
| GetChainageAtPole cộng khoảng cách cọc | 0x180175d70 | ev_270967da0ecd20b9aafa5235d83edd93fb5df74e02cc7d66d0f9a31a40da6231 |
| Đối chiếu khoảng cách cọc với polyline | 0x180173d00 | ev_7dd736dcdd19fba83e0c25a6d17c435db40f7aeafc684b6b63a7c895ac09f65a |
| Tạo thông tin lý trình từ gốc, khoảng cách và hiệu chỉnh | 0x180179840 | ev_f9d6c39ea6ccfe3016731bff8bd4de92799123005415e474e15c5886fcc1b5e6 |
| Đọc tọa độ cọc | 0x180179550 | ev_738e7398ec2c882783169bd2bfa66540c0b49d09e4d21a5751970eff2914ee35 |
| Tạo hình học tim | 0x180116270 | ev_d592be669f82359599a647420f3a1ccce556aec6a1063c507f1daf44029469a0 |
| Explode trả bản sao polyline nội bộ trước hình cọc | 0x1800e9370 | ev_d7b3f3d0e80d2ec31560c77da689821a14fbb1191da3a0d42d5cf1b9b9bbd9f8 |
| Ghi dữ liệu cọc và tọa độ có điều kiện | 0x1801569d0 | ev_16e78df19fcc9c77a58a7dc1b15100f00607795f92c8ee42992a3fa3e8063f07 |

Bản bằng chứng đầy đủ (34 bản ghi) và snapshot lưu tại `D:\lisp\BHT\audit-1.0.26\tdt-rea-evidence.json` và `tdt-alignment.snapshot.json`. Pseudocode là kết quả Ghidra, không phải mã nguồn TDT. Offset/địa chỉ chỉ thuộc hash đã nêu, không phải hợp đồng API ổn định.

## Kiểm tra chạy và giới hạn

- Tại lần kiểm tra tiến trình này không thấy `acad.exe`; chỉ có Core Console phục vụ kiểm thử. Chưa đối chiếu trong phiên đầy đủ đang dùng khóa TDT.
- Thử bộ ghi `DwgFiler` .NET trong một Core Console riêng, profile `BHTResearch111`, trên bản sao DWG. Phép tự kiểm với polyline chuẩn gặp **Unhandled Access Violation**, trước khi chạy phép đọc đối tượng TDT. Vì vậy phép thử không xác nhận dữ liệu native; cũng không chứng minh thư viện TDT bị lỗi.
- Mã thử đã chuyển vào thư mục audit và loại khỏi source/release. DLL thử không dành cho nạp vào CAD làm việc. Log: `read-probe-console.log`.
- File gốc `D:\lisp\BHT\New\BD_TD_TN(Km50+140-Km51+634.24).dwg` không bị ghi. SHA-256 trước/sau đều `936A8CE32E2E339A63A7DE99736A56296A7427FFA1698C492B90E4D832750875`.
- Chưa thực hiện: đọc native thành công, xác nhận dấu/đơn vị hiệu chỉnh, sai số hình học đường cong chuyển tiếp, các trường hợp đứt thước, định lượng tốc độ. Không bật tự động đọc lý trình native trong BHT 1.0.26.

## Trạng thái các câu hỏi

| Câu hỏi | Trạng thái |
| --- | --- |
| Vì sao BHT hiện cần nhãn Km hiển thị? | Đã xác định từ source BHT: quét chữ/attribute/hình explode. |
| Hai hàm Station/Point có dùng ngay cho bình đồ không? | Đã xác định không phù hợp: đường gọi trắc dọc. |
| Có dữ liệu tim và cọc độc lập với nhãn không? | Đã có bằng chứng tĩnh trong thư viện. |
| Đọc tự động chính xác trên đoạn 10 chưa? | Chưa; cần bộ kết nối native và kiểm chứng trong AutoCAD/TDT đầy đủ. |
