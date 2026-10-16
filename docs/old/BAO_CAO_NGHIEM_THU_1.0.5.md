# BHT 1.0.5 — Báo cáo kiểm thử

Ngày 12/10/2026. Môi trường: AutoCAD 2024, .NET Framework 4.8, DLL đã bảo vệ và Lisp FAS. Thử trên bản sao; không ghi đè DWG khảo sát gốc, không cài DLL mới vào phiên CAD của người dùng.

## 1. Lỗi và cách sửa

| Hiện tượng | Nguyên nhân đã xác định | Sửa trong 1.0.5 |
| --- | --- | --- |
| Tick Công cụ bổ sung nhưng không thấy nội dung | Đổi Visible chưa tính lại đầy đủ chiều cao/vùng cuộn của trang đang cuộn xuống cuối | Tính lại bố cục sau đổi trạng thái, cuộn tới nội dung và vẽ lại ngay; thử cả gói thầu |
| Đường thẳng và gấp khúc trông bị ngược | Nhãn/mã lựa chọn đúng; hàm dựng đường nối biển tự tạo gấp khúc khi không có điểm trung gian, chưa lưu kiểu riêng của hồ sơ | Lưu kiểu nối riêng; DIRECT tạo thẳng, ELBOW dùng gấp khúc tự động, WAYPOINT giữ các điểm chỉ định |
| Chọn toàn bộ báo đủ số lượng nhưng không giữ lựa chọn | Lệnh Lisp chưa có cờ giữ lựa chọn; bảng còn gọi các hàm Lisp đọc thông báo trước/sau lệnh chọn qua ngữ cảnh EXECUTEFUNCTION | Chuyển sang lệnh .NET UsePickSet/Redraw; các lệnh chọn không gọi Lisp đọc thông báo trước/sau |
| SELECTSIMILAR bỏ các mã biển khác | Chế độ 130 so cả lớp và tên block; các biển khác mã có block khác tên. CAD còn phân biệt loại đối tượng | Thêm công cụ đặt chế độ 2 và chạy SELECTSIMILAR; thêm lựa chọn theo nhóm hồ sơ để chọn đủ cả chữ/đường nối/dấu gốc |

Đây là lỗi đường dựng nối, không phải chỉ đổi thứ tự hai dòng trong hộp chọn. Cả luồng đặt một hồ sơ và áp dụng toàn bộ đã dùng chung quy tắc kiểu nối.

## 2. Hành vi đã chốt

- **Đặt tự do:** kiểu nối áp dụng riêng cho hồ sơ. Hộp mở theo kiểu đã lưu. Gấp khúc riêng vẫn dùng được khi mặc định toàn bản vẽ là thẳng, và ngược lại.
- **Áp dụng tất cả:** đổi kiểu nối/khoảng cách cho biển, bảng và đèn đã vẽ, thay lựa chọn riêng và điểm trung gian trước đó của các ký hiệu được cập nhật. Cọc tiêu và cột Km không bị dịch khỏi vị trí thực địa. Cụm thiếu điểm hoặc trên lớp khóa được bỏ qua và báo số lượng.
- **Đường thẳng:** nối trực tiếp từ điểm gốc tới chân trụ hoặc mép dưới block. **Gấp khúc:** đi theo trục của ký hiệu và tránh mặt biển khi cần. Nếu hai điểm đã thẳng hàng ở phía dưới, hình có thể chỉ cần đường thẳng. Điểm gốc nằm bên trong mặt biển vẫn cần đặt lại biển/tăng khoảng cách.
- **Chọn tất cả:** chọn toàn bộ thành phần hiển thị BHT trong Model; không chọn điểm RTK/nhãn RTK hoặc hình CAD thông thường.
- **Chọn cùng nhóm:** lấy nhóm từ hồ sơ của mẫu. Có thể lấy mẫu bằng block, nhãn hoặc đường nối. Nhóm Biển báo chọn các mã biển khác nhau, không tự chọn Đèn/Cọc tiêu. Có thể dùng mẫu đang chọn hoặc chọn trên CAD.
- **SELECTSIMILAR theo lớp:** đặt SELECTSIMILARMODE=2 cho CAD. Chọn block chỉ chọn các block cùng lớp; không tự chọn TEXT/LINE. Dùng hai lệnh chọn BHT nếu muốn COPY đủ thành phần.
- Các lệnh chọn bật PICKFIRST khi cần và giữ lựa chọn khi kết thúc. Esc hoặc bắt đầu một thao tác CAD khác có thể bỏ lựa chọn theo cơ chế của AutoCAD.

COPY thông thường chỉ mang hình và dữ liệu gắn trên hình, không mang đầy đủ kho hồ sơ BHT. Xuất/nạp dự án vẫn chỉ nạp vào DWG chưa có BHT, giữ tọa độ và liên kết ảnh; tệp ảnh và phông nằm ngoài DWG.

## 3. Kết quả kiểm thử

| Bộ thử | Kết quả | Nhật ký trong build/v1.0.5/qa |
| --- | --- | --- |
| Hàm lõi | 233 đạt, 0 lỗi | build.txt |
| Giao diện/bố cục/nội dung | 2.493 kiểm tra đạt | palette_layout-protected-summary.txt |
| Thẳng/gấp khúc/điểm trung gian | 20 kiểm tra đạt; Undo và lưu/mở lại đạt | display105-protected-summary.txt |
| Lựa chọn | 12 kiểm tra đạt | selection105-protected-summary.txt |
| Thư viện đèn | 9 mẫu và logic đặt/hướng/tỷ lệ đạt | lights-protected-summary.txt |
| Chế độ nối trên BOT830 | Giữ điểm/góc/tỷ lệ; thẳng, gấp khúc, Undo và chọn hơn 500 thành phần đạt | sample-modes-protected-summary.txt |
| Xuất/nạp BOT830 | So từng nhóm dữ liệu và lưu/mở lại đạt | sample-transfer-protected-summary.txt |

Bộ lựa chọn đã tái hiện việc chế độ 130 bỏ block biển khác tên; chế độ 2 chọn được cả hai mã biển khác nhau. Kiểm tra tách riêng cho thấy lựa chọn theo nhóm gồm block, chữ và đường nối, không chọn đèn khác nhóm. Tập lựa chọn được đọc trong lệnh kế tiếp, không chỉ kiểm tra số lượng ngay bên trong lệnh chọn.

Bộ giao diện đã tick/bỏ tick tại cuối trang ở chiều rộng 340, chiều cao 340 và 560, không đổi tab. Kiểm tra nội dung hiện ngay, nút đầu nằm trong vùng nhìn thấy, các nút không chồng nhau. Ảnh chụp nằm trong bin-protected/ui-preview/routes-expanded-105-340.png và routes-expanded-105-560.png.

Bộ kiểu nối đã thử cả biển một trụ, hai trụ và đèn; xác nhận lựa chọn trên hộp Đặt tự do khớp mã DIRECT/ELBOW/WAYPOINT; kiểm tra kiểu riêng vượt mặc định chung và giữ khi cập nhật. Thử dữ liệu sai/lỗi giữa chừng xác nhận hồ sơ, cấu hình và hình được trả về trạng thái trước thao tác.

Danh mục mã sản phẩm có **85 tên lệnh khác nhau, 89 nút** trong năm tab, không có lệnh khai báo hai lần. BHTCHONBAOHIEU chuyển từ Lisp sang .NET, không giữ hai định nghĩa trùng nhau. Hai nút mới có phạm vi khác nhau: nhóm hồ sơ BHT và SELECTSIMILAR của CAD.

## 4. Bản sao BOT830 và thời gian

Bản nguồn bảo toàn từ đợt trước có 801 điểm và 177 hồ sơ. Đợt này áp dụng kiểu nối/khoảng cách cho 120 ký hiệu biển/bảng/đèn; các cọc giữ tọa độ gốc.

| Tác vụ | Một lượt trên máy hiện tại |
| --- | ---: |
| Áp dụng khoảng cách 3, đường thẳng | 1.594 ms |
| Giữ khoảng cách 3, chuyển sang gấp khúc | 1.859 ms |

Điểm đo, góc và tỷ lệ hình không đổi. Một lần Undo trả lại hồ sơ. Bản xuất/nạp giữ đủ 801 điểm, 177 hồ sơ và các nhóm DATASET, FIELD_CHAIN, META, OBJ, PHOTO, ROUTE, SEG. Đây là số đo riêng từng lượt, không phải trung vị năm lượt hoặc bảo đảm cho mọi DWG.

## 5. Giới hạn nghiệm thu

- MCP đã kết nối COM nhưng chỉ đọc được Drawing1.dwg tại C:/Windows/system32, không phải bản vẽ công trình trong ảnh. Không sửa bản Drawing1 này. Kiểm thử CAD chạy trong AutoCAD Core Console riêng; chưa bấm chuột trên đúng phiên CAD/TDT của người dùng.
- Kiểm tra luồng nút chọn gồm rà nhánh gửi/kết thúc của Palette và đọc tập lựa chọn trong CAD; chưa chứng nhận toàn bộ luồng chuột native với mọi tiện ích CAD khác.
- Chưa có máy CAD 2024 sạch để cài thử độc lập. Kiểm tra runtime/bộ cài cuối cùng được ghi thêm trong phụ lục bàn giao, sau khi đóng gói.
- Các bộ RTK, lý trình/TDT và bộ thử biển rộng hơn của 1.0.4 không được coi là đã chạy lại toàn bộ trong đợt này. Ba ca thăm dò chưa đạt của đợt trước vẫn cần theo dõi: description-import-80, sync-creates-speed80, support-missing-measured-foot-warning.
- Chưa triển khai DWG hoàn toàn không phụ thuộc phông hoặc ảnh ngoài; thư viện DWG/phông bên thứ ba chỉ ở bộ cài dùng tại máy này, không công bố lên GitHub.
- Core Console đôi lúc báo không dọn được tệp tạm khi thoát; nhật ký kiểm tra đã ghi xong. Không dùng lỗi dọn tạm này làm bằng chứng cho máy sạch.

## 6. Cách thử sau khi cài

Lưu công việc, đóng CAD và cài BHT-Setup-1.0.5.exe. Mở DWG thử riêng, tick Công cụ bổ sung; nội dung phải xuất hiện ngay. Chọn Khoảng cách đặt báo hiệu → Đường thẳng → Áp dụng tất cả; thử Undo và đổi sang Gấp khúc. Mở một hồ sơ → Đặt ký hiệu để đối chiếu ba lựa chọn riêng.

Chọn tất cả rồi gõ COPY ngay. Dùng Chọn báo hiệu cùng nhóm với một biển để kiểm tra hình/nhãn/đường nối cùng được chọn. Với SELECTSIMILAR của CAD, dùng SELECTSIMILAR theo lớp trước và kiểm tra SELECTSIMILARMODE=2; việc chỉ chọn block mà không chọn chữ/đường nối là phạm vi của CAD, không phải mất hồ sơ.

Hướng dẫn: HUONG_DAN.md. Danh mục đầy đủ: DANH_MUC_LENH_NUT_1.0.5.md.
