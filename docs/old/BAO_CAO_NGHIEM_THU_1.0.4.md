# BHT 1.0.4 — Báo cáo kiểm thử và phạm vi nghiệm thu

Ngày 12/10/2026. Môi trường: AutoCAD 2024, .NET Framework 4.8, Lisp đã biên dịch FAS và DLL đã bảo vệ. Các kiểm tra dưới đây chạy trên bản sao dữ liệu; không cài đè vào CAD đang mở và không ghi đè DWG khảo sát gốc.

## 1. Kết quả và phạm vi

Đã kiểm thử các chức năng mới: xoay toàn bộ nhãn RTK, thay bốn mẫu đèn, đổi khoảng cách đặt báo hiệu, đường nối tránh mặt biển, lớp hiển thị chung và chuyển dự án sang DWG chưa có BHT. Có hướng dẫn sử dụng và danh mục đầy đủ **83 tên lệnh, 87 nút** trong năm tab.

Đây là nghiệm thu tự động trên máy phát triển. Chưa nghiệm thu toàn bộ thao tác bằng chuột trên CAD có TDT và chưa thử cài trên máy CAD 2024 sạch. Kết quả này không có nghĩa mọi nút và mọi lệnh của BHT đã được chạy hết các nhánh tương tác.

## 2. Các thay đổi đã kiểm tra

| Phần | Hành vi trong bản 1.0.4 | Cách kiểm tra |
| --- | --- | --- |
| Xoay nhãn RTK | Tên, mã điểm, mô tả và cao độ cùng xoay theo UCS hoặc hướng A → B | API/lệnh trong CAD; UCS 30°; hướng chọn; hủy; hướng không hợp lệ; Undo; lưu/mở lại |
| Bảo toàn nhãn | Giữ điểm đo, nội dung, phông, cỡ chữ và vị trí neo của cụm; cập nhật giữ góc đã xoay | So sánh dữ liệu và hình học trước/sau; cụm khóa lớp; lỗi ghi giữa chừng |
| Giao diện RTK | Một nút Xoay nhãn RTK; hộp tỷ lệ gọn, giải thích qua gợi ý | Cây điều khiển, chụp hộp thoại và các chiều rộng bảng |
| Mẫu đèn | Thay bốn DWG tín hiệu do người dùng cung cấp, gồm cần vươn trái/phải, cảnh báo vàng và ba màu | Kiểm tra nạp block, màu tô, hình CAD 2D, kích thước chuẩn hóa, hướng đặt và tỷ lệ |
| Khoảng cách đặt | Áp dụng cho tất cả biển, bảng và đèn đã vẽ; cọc tiêu/Km giữ vị trí thực địa | Kiểm tra khoảng cách, điểm đo, góc, tỷ lệ, dữ liệu sai, áp dụng lặp và hoàn tác lỗi |
| Đường nối | Nối vào chân trụ hoặc mép dưới block; đường tự động đi ngoài cạnh và dưới biển khi điểm gốc ở phía trên | Hình học theo trục block, kể cả góc 90° và tỷ lệ 2; thử W.209 và biển tốc độ |
| Lớp chung | Hình, nhãn, đường nối và dấu gốc BHT dùng BHT_KYHIEU sau cập nhật; giữ màu riêng | Rà toàn bộ thành phần BHT_KH và dấu gốc trên bộ thử và bản mẫu |
| Chọn để COPY | BHTCHONBAOHIEU chọn đủ thành phần; COPY mang hình, không mang toàn bộ hồ sơ | Rà phạm vi lựa chọn và giải thích trong hướng dẫn; chưa bấm COPY/SELECTSIMILAR bằng chuột thật |
| Xuất/nạp dự án | Chuyển điểm, hồ sơ, mốc/tuyến, đoạn/gói, liên kết ảnh và hình liên quan tại tọa độ gốc | So sánh từng nhóm hồ sơ; remap handle tuyến/tên block; lưu/mở lại; nạp bằng lệnh, Undo và cập nhật sau nạp |

Khoảng cách là từ **điểm gốc đến gốc ký hiệu**, theo đơn vị bản vẽ; không phải chiều dài toàn bộ đường gấp khúc và không phải bề rộng nét. Những ký hiệu thiếu điểm hoặc có thành phần trên lớp khóa được bỏ qua và báo số lượng.

Đường nối được chọn thủ công vẫn được giữ. Nếu điểm gốc nằm bên trong mặt biển, cần tăng khoảng cách hoặc đặt lại biển; đường nối không thể vừa xuất phát từ điểm đó vừa tránh hoàn toàn nội thất mặt biển.

## 3. Kết quả các bộ thử

| Bộ thử | Kết quả | Bằng chứng trong mã nguồn |
| --- | --- | --- |
| Hàm lõi | 233 đạt, 0 lỗi | build/v1.0.4/qa/build.txt |
| Giao diện và nội dung | 2.347 kiểm tra đạt | qa/ui-protected-summary.txt; bin-protected/ui-preview/result.txt |
| Nhãn RTK | 27 kiểm tra đạt và lưu/mở lại đạt | qa/rtk-protected-summary.txt; tests/IntegrationTests/test_RTK104.lsp |
| Nhãn trên BOT830 | 8 kiểm tra đạt và lưu/mở lại đạt | qa/sample-final-summary.txt; test_RTK104_SAMPLE.lsp |
| Thư viện đèn | Cả 9 mẫu đạt; logic đặt/hướng/tỷ lệ đạt | qa/lights-protected-summary.txt; LightLibraryProbe.cs |
| Hiển thị báo hiệu | 10 kiểm tra đạt | qa/display-project-protected-summary.txt; test_DISPLAY104.lsp |
| Chuyển dự án nhỏ | 10 kiểm tra đạt | qa/display-project-protected-summary.txt; Project104Probe.cs |
| Chuyển BOT830 | 10 kiểm tra đạt; khoảng cách/giữ điểm/Undo đạt | qa/sample-display-final-summary.txt; SampleProject104Probe.cs |
| Dùng lại sau nạp | 5 kiểm tra đạt | qa/project-reuse-protected-summary.txt; run_project_reuse104.ps1 |

Các đường dẫn bắt đầu bằng qa hoặc bin-protected ở bảng trên nằm trong build/v1.0.4. Nhật ký rút gọn được sao chép vào thư mục bàn giao Kiem-thu. Kiểm tra bộ cài cuối cùng được ghi riêng trong phụ lục bàn giao sau khi đóng gói.

Rà mã đăng ký cho thấy 83 tên lệnh khác nhau, không có tên khai báo hai lần trong mã sản phẩm. BHT không đăng ký 44. Các phím 11/22/33/55/66 chỉ được đăng ký khi chưa có lệnh cùng tên lúc nạp; tên BHTLAY… vẫn là cách gọi khi phần mềm khác chiếm phím số.

## 4. Thử trên BOT830

Bản nguồn kiểm thử bảo toàn từ đợt trước có **801 điểm, 177 hồ sơ và 2.102 nhãn RTK**. Bản sao gốc này không bị ghi đè. Thử đổi khoảng cách áp dụng cho 120 ký hiệu biển/bảng/đèn; 57 hồ sơ còn lại không thuộc phạm vi đó.

- Điểm RTK và cao độ không đổi khi xoay nhãn hoặc đổi khoảng cách đặt biển.
- Nội dung, phông và cỡ của 2.102 nhãn hiện hữu được giữ. Đồng bộ nhãn tạo thêm 22 nhãn còn thiếu cho 11 điểm thủ công; các nhãn mới được xoay lại trước khi lưu bản mẫu.
- Toàn bộ điểm, 177 hồ sơ và các nhóm DATASET, FIELD_CHAIN, META, OBJ, PHOTO, ROUTE, SEG được giữ khi xuất/nạp. Handle hình học tuyến và tên block được đổi theo tham chiếu của DWG đích.
- Một lần Undo khôi phục cả hồ sơ lẫn hình khi đổi khoảng cách; một lần Undo gỡ lần nạp dự án.
- Sau nạp, cập nhật ký hiệu vẫn giữ đủ 177 hình và 801 điểm; đổi khoảng cách tiếp vẫn thực hiện được.

Đổi khoảng cách 120 ký hiệu ở lần thử cuối khoảng **1,53 giây**. Các lượt trước khoảng 1,42–1,49 giây. Xoay 2.102 nhãn ở lần thử cuối khoảng **0,64 giây**; các lần trước khoảng 0,67–0,74 giây. Đây là số đo riêng từng lượt trên máy hiện tại, không phải trung vị năm lượt, không thay thế benchmark 100/1.000/5.000 điểm và không bảo đảm cho mọi bản vẽ.

## 5. Điều kiện chuyển dự án

1. DWG đích chưa có dữ liệu BHT; bình đồ CAD thông thường có thể đã có sẵn. Cấu hình khởi tạo BHT đơn thuần không bị coi là dự án đã nhập. Nếu đã có điểm/hồ sơ BHT, lệnh từ chối và giữ dữ liệu hiện tại. Theo lựa chọn đã chốt, không gộp hai dự án.
2. Hai DWG cùng INSUNITS. Tọa độ WCS được giữ; không hỏi điểm chèn, không tự scale theo đơn vị.
3. Đối tượng BHT nằm ở Model. Nếu có dữ liệu BHT trong Layout, lệnh dừng; không âm thầm bỏ dữ liệu đó.
4. Xuất tạo DWG mới và từ chối ghi đè tệp có sẵn. Không mang các hình bình đồ không liên quan; hình học tuyến được tham chiếu có đi kèm.
5. Liên kết ảnh được giữ, tệp ảnh gốc vẫn nằm ngoài DWG. Chuyển máy cần gửi ảnh và chỉ lại thư mục khi đường dẫn đổi.

Thư viện biển và đèn được đóng trong bộ cài tại máy này. Block đã chèn được lưu trong DWG nên máy không có BHT có thể xem/sửa hình CAD; chỉnh hồ sơ bằng BHT cần cài BHT. Một DWG không tự mang mọi phông SHX/TTF hoặc tệp raster. Tính năng chuyển chữ thành nét để xem đúng mà không cài phông **chưa triển khai**; chưa nghiệm thu khả năng chỉ gửi một DWG và luôn đúng mọi chữ trên máy không có BHT/phông.

## 6. Những phần chưa nghiệm thu hoặc cần theo dõi

- MCP báo có AutoCAD COM nhưng không mở được bản sao qua kết nối, phiên COM đọc được không có tài liệu đang hoạt động. Vì vậy các kiểm tra bản 1.0.4 chạy trong AutoCAD Core Console riêng, không ghi nhận đã bấm thử trên bản vẽ người dùng đang mở.
- Chưa thử tất cả hộp chọn tệp Windows, mở ảnh bằng ứng dụng ngoài, clipboard và mọi nhánh lệnh chẩn đoán.
- Chưa có bộ thử đầy đủ trên tuyến proxy TDT 9.1 thực; các kết quả Polyline không thay thế thử TDT. Chức năng 44 của TDT được giữ theo mã đăng ký, chưa thử trực tiếp trong phiên TDT ở đợt này.
- Bộ thử biển rộng hơn khi chạy thăm dò còn ba ca không đạt: description-import-80, sync-creates-speed80 và support-missing-measured-foot-warning. Ba ca này cũng tái hiện trên nguồn trước 1.0.4; không ghi bộ thử đó là đạt. Các ca RTK liên quan trong lần chạy thăm dò đạt.
- Core Console đôi lúc ghi lỗi dọn tệp tạm errno=13 lúc thoát; kết quả kiểm tra đã ghi xong. Đây không phải bằng chứng bộ cài chạy được trên máy sạch.
- Các DWG/phông bên thứ ba chỉ nằm trong gói dùng tại máy này; chưa có quyền công bố lại. Không phát hành các tài nguyên đó lên GitHub công khai.

## 7. Cách tự kiểm tra bản bàn giao

Lưu công việc và đóng CAD trước khi cài BHT-Setup-1.0.4.exe. Mở bản DWG thử trong thư mục Kiem-thu, không dùng bản khảo sát gốc để thử.

1. Điểm vị trí → Xoay nhãn RTK… → Theo UCS hoặc chọn A → B; kiểm tra cả mã điểm và tên, rồi Undo.
2. Hồ sơ đối tượng → chọn nhóm Đèn tín hiệu → Chọn loại đèn…; thử bốn mẫu mới, tỷ lệ và hướng đặt.
3. Tuyến & báo cáo → Bảo trì ký hiệu → Khoảng cách đặt báo hiệu…; nhập khoảng cách phù hợp tỷ lệ trình bày, kiểm tra đường nối/điểm gốc và Undo.
4. Dùng Chọn tất cả báo hiệu trên CAD để COPY hình. Muốn giữ hồ sơ, dùng Xuất dự án BHT sang DWG… rồi mở một bản sao bình đồ chưa có BHT để Nạp dự án BHT từ DWG….
5. Kiểm tra điểm, hồ sơ, mốc/tuyến và ảnh; cập nhật một ký hiệu rồi lưu/mở lại. Nếu chuyển máy, kiểm tra thêm phông và thư mục ảnh.

Chi tiết từng nút/lệnh: DANH_MUC_LENH_NUT_1.0.4.md. Quy trình sử dụng: HUONG_DAN.md.
