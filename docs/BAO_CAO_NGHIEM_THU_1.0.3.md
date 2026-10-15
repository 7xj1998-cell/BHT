# BHT 1.0.3 — Báo cáo rà soát và nghiệm thu

Ngày lập: 12/10/2026. Môi trường: Windows, AutoCAD 2024, .NET Framework 4.8. Phạm vi: dọn nút trùng, rà lệnh, kiểm tra hồi quy và hướng dẫn sử dụng.

## 1. Kết luận

Đã sửa phần giao diện bị lặp và build bản 1.0.3. Các bộ thử tự động được liệt kê dưới đây đã đạt. Chưa đủ bằng chứng để tuyên bố đã nghiệm thu mọi thao tác bằng chuột trên AutoCAD đầy đủ, mọi nhánh TDT 9.1 hoặc cài đặt trên máy sạch.

Rà mã sản phẩm tìm được **78 tên lệnh khác nhau**; cây giao diện năm tab có **82 nút**, kể cả nút ẩn theo ngữ cảnh. Không phát hiện tên lệnh bị khai báo hai lần trong tập mã này. Bảng đối chiếu đầy đủ nằm trong **DANH_MUC_LENH_NUT_1.0.3.md**.

Không thay tọa độ khảo sát hoặc số mốc để làm hết cảnh báo. Bản gốc đang mở có thay đổi chưa lưu; không lưu đè hoặc đóng bản gốc. Phần thử sử dụng bản sao của dữ liệu DWG đã lưu trên đĩa, nên không bao gồm các sửa đổi chưa lưu ở phiên người dùng.

## 2. Thay đổi đã thực hiện

| Vấn đề | Xử lý |
| --- | --- |
| Hai nút cùng tính lý trình hàng loạt | Giữ một nút ở đầu trang Tuyến & báo cáo |
| Nút tính ở đầu trang có thể chuyển sang mốc thực địa dù đang dùng tuyến | Giữ phương pháp hiện tại khi nguồn dùng được; chỉ mở bảng kiểm tra mốc khi thiếu nguồn |
| Tính riêng và tính toàn bộ dễ nhầm | Đổi tên nút tính riêng thành “Tính lý trình hồ sơ này” |
| CSV, KMZ, thống kê, kiểm tra và xuất đặt ở nhiều nơi | Gom từng chức năng về tab sở hữu; Tổng quan chỉ điều hướng |
| Gán block riêng và mẫu mặc định nhóm khó phân biệt | Đổi tên nút mẫu chung; giữ hai phạm vi dữ liệu khác nhau |
| Tiếp tục khi chưa có điểm chỉ báo mà chưa chuyển tab | Chuyển tới Điểm vị trí và chỉ đúng nút Nhập CSV RTK |
| Trợ giúp thiếu các lệnh tuyến và phím lớp hiện tại | Cập nhật BHTHELP và hướng dẫn; giữ nguyên lệnh 44 của TDT |

Giữ những thao tác khác phạm vi: một/tất cả hồ sơ; cặp mốc/danh sách mốc; đặt ký hiệu/cập nhật ký hiệu; Excel/CSV; block riêng/mặc định nhóm. Không xóa CLI đang dùng để chỉ làm giao diện ngắn hơn.

## 3. Bằng chứng kiểm thử

Các đường dẫn dưới đây tính từ `sharing-0654/source`. Khi bàn giao, nhật ký tóm tắt được chép vào thư mục nghiệm thu.

| Nhóm | Kết quả | Bằng chứng |
| --- | --- | --- |
| Build .NET và quy tắc dữ liệu | 233 đạt, 0 lỗi | build/v1.0.3/qa/build-final.txt |
| Bố cục, nội dung, hộp chọn, trạng thái sửa | 2.132 kiểm tra đạt trên DLL thường | qa/ui-summary.txt; bin/ui-preview/result.txt |
| Giao diện dùng DLL bảo vệ + FAS | 2.132 kiểm tra đạt | qa/ui-protected-summary.txt |
| Khởi động, API, lệnh cũ, nhãn, tìm biển | REVIEW 10; S0 20; V5 22 đạt | qa/review-summary.txt |
| Mốc đã nhập | 52 kiểm tra và lưu/mở lại đạt | qa/markers-summary.txt |
| Cọc bổ sung thủ công | Tạo, hủy, sửa, nguồn vị trí, xác nhận, ký hiệu, Undo, mở lại đạt | qa/markers-summary.txt |
| Sửa/chọn lại/xóa tuyến; đầu/chiều tuyến | Đạt | qa/routes-summary.txt |
| Bảng tuyến, làm sáng/zoom và trạng thái lớp | 16 kiểm tra đạt | qa/route-data-summary.txt |
| Xoay biển và đặt một ký hiệu | Đạt các trường hợp góc tiếp tuyến, hướng chọn, hủy, sai phạm vi, giữ điểm đo | qa/routes-summary.txt |
| Lý trình một hồ sơ / hai cọc | 9 kiểm tra đạt; lệnh BHTKM2COC trả Km45+250 tại vị trí thử | qa/chainage-summary.txt |
| Thư viện đèn | 9 DWG mẫu; nhập, điểm neo, cỡ chuẩn, native 2D, màu và hướng đạt | qa/lights-summary.txt |
| Biển tùy chỉnh nhiều mặt | Tạo và mở lại giữ nội dung/mặt biển | qa/custom-summary.txt |
| Đồng bộ tỷ lệ | Đạt, giữ vị trí/góc/hồ sơ/điểm đo | qa/scale-summary.txt |
| Toàn thư viện: hình, nền, phông | 3.052 kiểm tra đạt | qa/signs-summary.txt; bin/print-probe.txt |
| Toàn thư viện: tham số nội dung | 1.027 kiểm tra đạt | qa/parameters-summary.txt |
| Bật/tắt nền trên bản vẽ mẫu | 10 lượt thao tác đạt, giữ điểm/hồ sơ/handle/góc/tỷ lệ | qa/fill-summary.txt |
| Phím lớp | 55 gọi LAYFRZ; 66 mở băng; không định nghĩa 44; giữ lệnh 44/55/66 có sẵn | qa/layers-summary.txt |
| BOT830 — DLL thường | Tính 177 hồ sơ; giữ điểm, ký hiệu, phương pháp, nhóm mốc; lưu/mở lại đạt | qa/sample-summary.txt |
| BOT830 — DLL bảo vệ/FAS | Cùng kết quả 73/104; lưu/mở lại đạt | qa/sample-protected-summary.txt |
| Phiên CAD đang mở, bản sao | Runtime 1.0.2: chạy lệnh BHTLYTRINH, giữ nguồn/điểm và Undo hồ sơ đạt | qa/live-cad.txt |

Các kiểm tra giao diện chạy WinForms trong AutoCAD Core Console: có thực thi thao tác lập trình trên điều khiển và kiểm tra dữ liệu/kích thước. Con số 2.132 là số điều kiện kiểm tra, không phải 2.132 nút và không chứng minh đã bấm mọi nút bằng chuột thật trên bản vẽ.

Bộ thử V5 có hai kỳ vọng lịch sử không còn đúng: đòi nhãn chung “Cọc tiêu” khi chưa có số và tên mã TDT chưa chuẩn hóa. Đã sửa bộ thử để kiểm tra đúng quy ước hiện tại: cọc không số không sinh nhãn giả; nhập H4/40 thì có nhãn đúng phông; R.E9a trả mã chuẩn, không trả bí danh trùng. Không thay mã sản phẩm để chiều theo kỳ vọng cũ.

## 4. Bản vẽ BOT830 và dữ liệu cần xử lý

Nguồn: `C:\Users\Le Bao\Desktop\BOT\261010.Bienbao BOT830.dwg` (bản đã lưu trên đĩa). Bản thử nguồn giữ hash không đổi sau thử. Bản xuất thử mới nằm ở `build/v1.0.3/qa/bot830-protected/BHT-1.0.3-test.dwg`.

Số liệu: **177 hồ sơ**, **801 điểm** gồm **790 điểm RTK** và **11 điểm bổ sung thủ công**; **334 ảnh**, trong đó **332 ảnh có GPS hợp lệ**. Có 177 ký hiệu đối tượng và 331 ký hiệu ảnh.

### Lỗi và cảnh báo đã có trước khi sửa giao diện

| Mục | Phát hiện | Hướng xử lý |
| --- | --- | --- |
| BOT1-R-000323 | Vị trí hiện tại khác tọa độ lúc nhập | Đối chiếu CSV/sổ đo trước khi quyết định phục hồi hoặc xác nhận điều chỉnh |
| OBJ-000004 | Khai báo hai trụ nhưng thiếu điểm chân trụ | Mở hồ sơ, gắn đúng điểm chân trụ còn thiếu |
| OBJ-000006 | Cùng vấn đề thiếu điểm cho hai trụ | Đối chiếu vị trí thực tế và gắn điểm |
| TUYEN1 | Hệ số mốc 1–2 là 0,9152; chênh lý trình không khớp chiều dài tim | Kiểm tra nếu dùng phương pháp theo tim; không dùng PL thử làm tim thật |
| Ảnh | Một ảnh có GPS chưa có ký hiệu | Mở Ảnh hiện trường → Cập nhật ký hiệu ảnh sau khi kiểm tra dữ liệu |

Không có điểm trùng mã, liên kết điểm bị mất hoặc ký hiệu đối tượng mất hồ sơ trong bản sao này. Hai ảnh không có GPS hợp lệ được báo riêng, không đặt ký hiệu tại 0,0.

### Kết quả tính theo mốc thực địa

Phương pháp đang dùng: **FIELD_MARKERS — cột Km và cọc tiêu**. Giữ nguyên danh sách mốc và các xác nhận hiện có.

Kết quả hàm tính: **73 hồ sơ có số/kết quả**, **104 hồ sơ chưa được chấp nhận để cập nhật**. Số 73 bao gồm cọc đã có số Km/H; không đồng nghĩa 73 biển vừa tính mới hoặc mọi kết quả đã được xác nhận. Một số hồ sơ có số vẫn mang trạng thái cần kiểm tra do dùng mốc thủ công.

| Nguyên nhân của 104 hồ sơ chưa cập nhật | Số lượng | Việc cần làm |
| --- | ---: | --- |
| Đoạn dài/thiếu cọc | 39 | Kiểm tra các mốc ở giữa và đường cong |
| Khoảng cách không phù hợp chênh lệch Km/H | 24 | Đối chiếu số cọc, nguồn điểm và vị trí |
| Không có đoạn cọc bao quanh | 21 | Kiểm tra mốc trước/sau; không ngoại suy |
| Trùng số hoặc thứ tự vị trí đổi hướng | 13 | Phân biệt đường/nhánh, kiểm tra cọc trùng |
| Vị trí phù hợp nhiều đoạn | 5 | Chọn hai mốc đúng hoặc nhóm riêng |
| Chưa chọn nguồn mốc phù hợp | 2 | Mở Mốc lý trình đã nhập hoặc chọn cặp cho hồ sơ |

Danh sách từng ID, trạng thái và lý do được xuất ở `qa/bot830-protected/objects-after.tsv`. Bản mẫu bàn giao giữ các cảnh báo này, không phải hồ sơ kỹ thuật đã được xác nhận ngoài hiện trường.

## 5. Thời gian đo trên máy hiện tại

| Tác vụ | Năm lượt (ms) | Trung vị |
| --- | --- | ---: |
| Tính 177 hồ sơ, DLL thường | 266; 265; 266; 266; 250 | 266 ms |
| Tính 177 hồ sơ, DLL bảo vệ/FAS | 250; 234; 266; 234; 250 | 250 ms |
| Tắt nền 118 ký hiệu trên bản sao 261011_test | 1.141; 62; 78; 63; 78 | 78 ms |
| Bật nền 118 ký hiệu trên bản sao 261011_test | 63; 63; 62; 47; 63 | 63 ms |

Lượt tắt nền đầu tiên có chi phí chuẩn bị mẫu. Thời gian tính lý trình chỉ gồm tính và cập nhật hồ sơ, không gồm vẽ lại mọi ký hiệu. Đây là kết quả trên bộ dữ liệu/máy này; không suy thành bảo đảm cho mọi DWG hoặc benchmark 100/1.000/5.000 điểm. Chênh nhỏ giữa DLL thường và bảo vệ không đủ để kết luận bộ bảo vệ làm nhanh hơn.

## 6. MCP/COM và giới hạn nghiệm thu

- MCP kết nối được AutoCAD COM, đọc trạng thái và lấy ảnh bản vẽ. Lệnh thử trên bản sao được gửi qua AutoCAD COM, có nhật ký runtime và Undo.
- Phiên người dùng đang nạp BHT 1.0.2. Bản 1.0.3 được thử trong tiến trình Core Console riêng để không thay DLL đang chạy.
- Công cụ điều khiển giao diện đọc được cây cửa sổ nhưng chụp/định vị chuột native gặp lỗi. Không ghi nhận các thao tác chuột thất bại là đã nghiệm thu.
- Bộ TX cần bản vẽ proxy TDT thực nhưng fixture hiện thiếu; báo **chưa kiểm thử**, không coi là đạt. Các bộ thử hình học Polyline không thay thế thử đầy đủ TDT 9.1.
- Chưa thử toàn bộ hộp chọn tệp, mở ảnh bằng ứng dụng ngoài, clipboard và mọi lệnh BHTNET theo từng nhánh tương tác.
- Chưa có người/máy CAD 2024 sạch để cài thử độc lập. Kiểm tra runtime trong thư mục tách biệt vẫn ở máy phát triển.
- Chưa xác nhận một DWG có mọi chữ đúng khi máy nhận không có phông bên thứ ba. Đóng gói thư viện để cài BHT không tự giải quyết phụ thuộc phông của DWG.

## 7. Tệp bàn giao và kiểm tra tiếp

Gồm bộ cài 1.0.3, ZIP, hướng dẫn, danh mục lệnh/nút, báo cáo này, nhật ký và DWG thử riêng. Không đưa DWG khảo sát hoặc ảnh công trình vào bộ cài. Kiểm tra gói cài cuối cùng được ghi trong phụ lục bàn giao, sau khi tạo gói.

Để nghiệm thu tương tác tiếp: lưu công việc, đóng CAD, cài 1.0.3 rồi mở bản sao thử. Kiểm tra theo thứ tự nhập điểm → chọn biển/sửa nội dung → đặt/cập nhật → xem mốc → tính một/tất cả → làm sáng/zoom tuyến → xoay biển → ảnh → xuất báo cáo. Với TDT, kiểm tra 44 giữ chức năng gốc và dùng BHTLAYFRZ/BHTLAYTHW khi phím số đã có.
