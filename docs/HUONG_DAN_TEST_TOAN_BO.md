# Bài kiểm tra bổ sung 1.1.1

Trước bài A–T bên dưới, đọc phần Bài kiểm tra sau cài đặt trong HUONG_DAN_HIEU_CHINH_1.1.1.md: bảng đoạn/tim, quản lý bình đồ, xung đột OBJ-000001 khi D14 vào D15, import lặp và nhật ký. Chỉ dùng bản sao. Bản 1.1.1 chưa giải quyết trùng mã tự động hoặc xác minh triệt để GUI đứng.

# Kiểm thử toàn bộ luồng BHT

Ngày 15/10/2026. Mốc hiện tại 1.1.0. Bạn chỉ thử trên bản sao; đánh dấu ĐẠT khi kết quả quan sát và đối chiếu dữ liệu đều khớp. Thông báo thành công không đủ để nghiệm thu.

## Chuẩn bị

1. Giữ thư mục New và DWG/.bht nguồn nguyên trạng. Dùng bản sao D10 tại D:/lisp/BHT/Test-1.1.0/D10-Test.dwg. Các bộ mẫu dưới đây nằm trong cùng thư mục Test-1.1.0; sao chép tiếp trước mỗi bài để có thể làm lại.
2. Lưu DWG trước từng bài dưới tên có hậu tố BEFORE. Xuất hồ sơ .bht với tên mới; lưu Excel trước/sau để đối chiếu lý trình, offset, phía, đoạn, gói, mã biển, điểm liên kết.
3. Mở BHT, kiểm tra Lisp và Palette cùng phiên bản. Bật Undo All. Ghi phiên bản, tên bản sao, mã OBJ và bước thực hiện vào phiếu kết quả.
4. Đừng dùng bản vẽ D10 thật làm dữ liệu giả 25+1. Bài giả lập đã có trong tests/IntegrationTests/test_INCREMENTAL131.lsp và test_CORRECTION132.lsp; runner chạy bản vẽ riêng.

## Kiểm tra các lỗi mới đã báo ở bản 1.1

Thực hiện P-T trước bài A-O. Đánh dấu ĐẠT chỉ khi cả giao diện và dữ liệu trước/sau đều đúng. Bộ mẫu có D13-Test.dwg là bản sao Đoạn 13 đã nạp hồ sơ trong New, D10-Test.dwg để kiểm tra dữ liệu nhiều đoạn. D13-After-Layout.dwg là kết quả tự động để đối chiếu, không dùng thay bản đầu vào khi tái hiện.

### P - Báo cáo cọc và thao tác CAD

1. Trong Tuyến, chọn tim và chạy kiểm tra cọc. Ghi số cọc/bản ghi ở đầu báo cáo, tìm cả dòng đầu lẫn dòng cuối. Báo cáo trên 40 dòng phải đọc và sao chép được hết.
2. Kéo cửa sổ sang cạnh màn hình, đổi kích thước, tăng/giảm chữ, tắt/bật xuống dòng. Không bị chữ trắng trên nền trắng, cắt tiêu đề hoặc mất hàng nút.
3. Để bảng mở, đưa chuột vào CAD, zoom/pan đến một cọc rồi quay lại bảng. Ghi kết quả thực tế; Core Console không chứng minh được bước này.
4. Mở xem trước Tính lý trình theo đoạn. Đóng không áp dụng phải giữ nguyên DWG. Mở lại, sửa một hồ sơ chưa khóa rồi bấm Áp dụng: phải báo dữ liệu thay đổi, không dùng kết quả cũ. Lập lại phương án rồi áp dụng mới được ghi.

### Q - Chọn sai điểm đầu

Trên tuyến thử chưa có mốc: chọn điểm cách tim 5 m, điểm cuối với chiều đi ra ngoài, điểm đầu hợp lệ với chiều đi vào. Hai trường hợp sai phải có cảnh báo và giữ nguyên khai báo. Chọn giữa tuyến phải có cảnh báo phần bị loại. Khi đã có mốc, thử đổi điểm đầu/chiều: phải từ chối, giữ bảng mốc và lý trình. Không làm bài này bằng cách xóa mốc D13 thật.

### R - Hướng nhãn và nhãn chỉnh tay

1. Trên D13-Working, chọn Song song rồi Vuông góc trong Tỷ lệ biển / nhãn. Đối chiếu mũi tên tăng Km của tim, nhãn chính/biển phụ/lý trình và bảng hai trụ.
2. Dùng UCS quay và PLAN Current, cập nhật lại: góc/điểm chữ trong WCS phải giữ nguyên. Không lấy hướng màn hình làm hướng tuyến.
3. Sau lần cập nhật bằng 1.1, ROTATE riêng nhãn chính, MOVE riêng nhãn phụ rồi cập nhật: vị trí/góc chỉnh tay giữ nguyên. Lưu BEFORE cho nhãn cũ trước 1.1 vì góc xoay tay chưa có bằng chứng lịch sử.
4. Bấm Áp dụng mẫu nhãn báo hiệu cho riêng cụm đó: cả cụm trở về mẫu; block biển, RTK, lý trình giữ nguyên. Thử U/REDO và lưu/mở lại.
5. Tạo bản thử không có mốc hoặc tim đã thay hình: phải có cảnh báo thiếu căn cứ hướng nhãn. Không được âm thầm coi hình tạm là kết quả đã xác minh.

### S - Bố trí D13 không lặp dịch chuyển

1. Mở bản sao D13-Test.dwg. Bố trí theo tim DOAN_13, tối thiểu 10 m, đường nối gấp khúc. Kiểm tra OBJ-000087, OBJ-000088, OBJ-000090 và cọc H OBJ-000089, OBJ-000098, OBJ-000175.
2. Ba biển phải bố trí thành công, giữ mã/nội dung, trụ và liên kết đo. Khoảng lùi đo tới bao hình và nhãn, không tính đường dẫn nối về điểm đo. Tim cong có thể khiến khoảng lùi thực tế lớn hơn 10 m.
3. Thực hiện cùng thao tác lần hai: không dịch thêm, không thêm block/nhãn. Các ký hiệu ngoài DOAN_13 phải giữ nguyên. So sánh Excel và điểm/tim với BEFORE.
4. U phải khôi phục cả lần bố trí; REDO khôi phục kết quả. Lưu AFTER, mở lại và đối chiếu. Test lỗi cố ý chạy bằng fixture tự động, không sửa XRecord thủ công để gây lỗi.

### T - Tốc độ và luồng phiên bản

Dùng cùng bản sao D10 đầu vào, cùng tỷ lệ và cùng thư viện. Đo thời gian từ bấm đến khi lệnh kết thúc cho Cập nhật ký hiệu và Tỷ lệ nhãn, chạy mỗi bài trên bản sao mới. Ghi tải máy, số OBJ, số thực thể, các cảnh báo. Sau đó tiếp tục A-O: giảm thời gian không được làm sai RTK, mất nhãn sửa tay hoặc vượt bảo vệ revision. Không yêu cầu thời gian bằng máy phát triển; dùng báo cáo để so sánh cùng điều kiện.

## Kiểm tra giao diện

Mở Hồ sơ đối tượng, chọn hồ sơ khóa, mở Hiệu chỉnh hồ sơ đã xác nhận. Thu nhỏ rồi phóng to cửa sổ; kiểm tra tiêu đề Nội dung/Giá trị đề nghị/Trước/Sau, chữ có dấu và ghi chú nhiều dòng. Kiểm tra lịch sử tương tự. Cuộn bảng phải xem đủ dữ liệu. Ghi rõ Windows Scale đang dùng; chưa coi mô phỏng font là kiểm chứng DPI thật.

## Bắt đầu với D10: sửa mã và kiểm tra bảng

1. Mở D10-Test.dwg, gõ BHT. Kiểm tra Palette 1.1.0, 179 hồ sơ, 801 điểm. Dùng SAVEAS lưu D10-BEFORE.dwg rồi tạo D10-Bai-B.dwg để làm việc.
2. Trong Hồ sơ đối tượng, tìm OBJ-000044. Mã ban đầu W.207b. Chụp thông tin mã, điểm RTK, lý trình, offset, phía đường và đoạn; xuất Excel BEFORE.
3. Bấm Hiệu chỉnh hồ sơ đã xác nhận, chọn Thuộc tính khảo sát. Đổi mã thành W.207c, nhập lý do và người sửa, bấm Xem trước. Kiểm tra chữ không đè/cắt và chỉ các trường phụ thuộc mã biển thay đổi. Hủy lần đầu: dữ liệu phải vẫn W.207b.
4. Làm lại, đọc cảnh báo baseline cũ rồi duyệt nếu dữ liệu đã đối chiếu đúng; bấm Xác nhận. OBJ vẫn khóa, lịch sử giữ W.207b và bản W.207c. 178 hồ sơ khác, 801 điểm và hình tim giữ nguyên.
5. Đóng hộp hiệu chỉnh, gõ U: trở lại W.207b. Gõ REDO: về W.207c. Lưu D10-AFTER.dwg, mở lại và đối chiếu Excel AFTER. Không dùng MOVE điểm RTK để thử đổi liên kết.

## Các bộ mẫu đã chuẩn bị

Mỗi thư mục là một dự án giả lập riêng, có 50 hồ sơ. Chỉ nhập các tệp cùng bộ vào nhau. Trộn merge/presentation/project có thể báo khác project/GUID; đó là hành vi bảo vệ, không phải lỗi nạp.

| Thư mục | Tệp | Cách dùng |
|---|---|---|
| merge | rev1.bht, rev2.bht, fork.bht, retired.bht, merged.dwg | OBJ001: P.103a ở REV1; P.104 ở REV2; fork P.105 cùng cha REV1; retired là con của fork |
| presentation | base.bht, next.bht, local.dwg | GLOBAL ban đầu, GLOBAL đã dời/xoay/sửa nhãn/đường dẫn; local.dwg đã nhận next nhưng giữ vị trí riêng |
| project | base.bht, next.bht, related.dwg, working-copy.dwg, working-copy-2.dwg | related còn ở bản cũ; working-copy có bản mới và danh tính bản vẽ riêng |
| project | DOAN_01_R01.bht, DOAN_01_R02.bht | Mẫu hai lần phát hành; giữ nguyên để đối chiếu, không dùng thư mục này làm nơi xuất bài thử mới |

### Nạp cũ, nhập lặp, xung đột và ngừng sử dụng

1. Sao merge/merged.dwg thành Merge-Test.dwg. OBJ001 phải là P.104. Nạp tích lũy merge/rev1.bht: vẫn P.104. Nạp merge/rev2.bht hai lần: số hồ sơ, ký hiệu, nhãn và lịch sử không tăng.
2. Nạp merge/fork.bht vào Merge-Test.dwg: phải có xung đột nhánh, đích vẫn P.104. Chụp bảng Trước/Sau; đóng bảng không ghi gì. Không có chức năng tự chọn nhánh thắng.
3. Dùng DWG trống khác, nạp đầy đủ merge/fork.bht. Sau đó nạp tích lũy merge/retired.bht: OBJ001 ngừng sử dụng, còn hồ sơ/lịch sử, không còn ký hiệu trong báo cáo hiện hành. Nạp lại fork.bht: không phục hồi OBJ001. Không nhập retired trực tiếp vào REV2 vì đây là hai nhánh khác nhau.

### GLOBAL và LOCAL trên hai DWG

1. Dùng DWG trống, nạp đầy đủ presentation/base.bht, lưu Global-BEFORE.dwg. Ghi vị trí/góc của ký hiệu OBJ001, nhãn và đường dẫn; ghi RTK/lý trình.
2. Nạp tích lũy presentation/next.bht: ký hiệu/nhãn/đường dẫn đổi theo nguồn; RTK/lý trình giữ nguyên. Display Revision từ 1 thành 2, Survey Revision giữ 0. Nạp lại phải không trùng.
3. Làm lại từ base trong DWG khác. MOVE ký hiệu OBJ001 sang vị trí dễ nhận biết, kết thúc lệnh. Trong Hồ sơ đối tượng -> Bố trí GLOBAL / LOCAL -> nhập lý do -> Lưu bố trí hiện tại thành LOCAL. Lưu Local-BEFORE.dwg.
4. Nạp next.bht: nhận revision chung mới nhưng giữ vị trí LOCAL. Lưu/mở lại: vị trí riêng vẫn còn. Tệp local.dwg có sẵn là mẫu kết quả của bước này.
5. Làm lại từ base, MOVE nhưng không lưu GLOBAL/LOCAL. Nạp next.bht: phải chặn bố trí tay chưa lưu, giữ đích. Không xuất DWG có LOCAL thành nguồn chung; bản này chủ động chặn thao tác đó.

### Bình đồ liên quan và phát hành R01/R02

1. Sao project/working-copy.dwg thành Project-A.dwg, sao project/related.dwg thành Project-B.dwg. Mở A -> tab Tuyến -> QUẢN LÝ XỬ LÝ THEO ĐOẠN -> Quản lý bình đồ / đồng bộ / phát hành.
2. Liên kết Project-B.dwg, bấm Kiểm tra bình đồ liên quan: B phải UPDATE_AVAILABLE, 1 hồ sơ. Kiểm tra chỉ đọc B; không tự ghi tệp B. Lưu A để giữ liên kết.
3. Mở B -> cùng hộp quản lý -> Xem trước gói cập nhật -> project/next.bht. OBJ001 phải Upgrade. Bấm Đồng bộ gói đã xem trước, xác nhận rồi lưu B. Kiểm tra lại từ A: B phải UP_TO_DATE.
4. Thử xem trước rồi thay đổi đích trước khi bấm Đồng bộ: phải yêu cầu xem trước lại. Gói fork phải báo CONFLICT và không ghi một phần.
5. Trên A, Danh sách đoạn -> DOAN_01 -> Phát hành đoạn R01/R02 -> chọn thư mục mới Test-1.1.0/MyReleases. Sau xác nhận có DOAN_01_R01.bht, A chuyển RELEASED. Lưu A; thao tác hiệu chỉnh/đồng bộ của BHT phải bị chặn.
6. Chọn Tạo bản DWG làm việc mới, tên Project-A-R02-Working.dwg chưa tồn tại. Mở bản mới, hiệu chỉnh một OBJ, đối chiếu lịch sử rồi phát hành DOAN_01 vào MyReleases. Có R02 mới; SHA256/nội dung R01 phải giữ nguyên.
7. Tạo bản làm việc khác rồi Đánh dấu lưu trữ, lưu/mở lại: READ_ONLY_ARCHIVE, BHT chặn ghi. Lệnh CAD gốc không bị khóa bởi cơ chế này; không dùng chúng để sửa bản lịch sử.

Bình đồ liên quan được kiểm tra theo tệp đã lưu. Thay đổi chưa lưu trong cửa sổ khác chưa được phản ánh. Nếu mất điện/ngắt phần mềm giữa lúc phát hành, có thể còn tệp Rxx chưa ghi lịch sử DWG: giữ tệp để đối chiếu, không ghi đè; bài này chưa có nghiệm thu crash thực tế.

## Các bài A-O

| Bài | Thao tác trên bản sao | Kết quả phải thấy | Trạng thái ở 1.1.0 |
|---|---|---|---|
| A: bổ sung | Đoạn có 25 hồ sơ khóa, thêm hồ sơ thứ 26; tính và xác nhận bổ sung | Chỉ OBJ026 thay đổi; 25 hồ sơ cũ, RTK và tim không đổi; không trùng ký hiệu | Đã có; fixture tự động M1 |
| B: sửa mã | D10, OBJ-000044 W.207b -> W.207c; xem trước, hủy một lần rồi xác nhận | Chỉ OBJ chọn có revision mới; giữ khóa/lý trình/RTK; lịch sử có mã cũ và lý do | Đạt runtime protected 1.1.0 trên D10 |
| C: đổi RTK | Chọn điểm đúng đã tồn tại, xem trước | Lý trình/offset/phía tính lại; RTK gốc không đổi; bản cũ trong lịch sử; điểm thiếu/trùng bị chặn | Đã có; chưa đổi mốc Km/H ảnh hưởng tuyến |
| D: dời ký hiệu | MOVE/ROTATE ký hiệu rồi kiểm tra hồ sơ | Tọa độ RTK và lý trình không đổi; vị trí thủ công giữ qua hiệu chỉnh khảo sát | Đã có Display Revision khi Lưu thay đổi trình bày GLOBAL |
| E: bố trí chung | Lưu bố trí GLOBAL ở DWG A, đồng bộ sang DWG B | B nhận đúng vị trí/góc/nhãn/đường dẫn; không thêm block trùng | Đạt Core Console 1.1.0; thử GUI theo hướng dẫn mới |
| F: bố trí riêng | Đặt LOCAL ở B rồi nhận thay đổi GLOBAL | B giữ bố trí riêng | Đạt Core Console 1.1.0; thử GUI theo hướng dẫn mới |
| G: nạp cũ | Đích REV2, nguồn REV1, nạp tích lũy | Giữ REV2, không nhân bản | Đạt tự động: nguồn tổ tiên bị bỏ qua |
| H: hai nhánh | Sao REV1 thành A/B; sửa khác nhau; nhập A vào B | Bảng xung đột, giữ nguyên đích; không chọn theo thời gian | Đạt tự động: xung đột giữ nguyên đích |
| I: trùng mã | Hai project/định danh khác nhau cùng OBJ001 | Báo trùng mã; không coi là một đối tượng, không đổi ID âm thầm | Đã chặn khác project/GUID; chưa có UI đối chiếu legacy |
| J: đổi tên gói | Dùng cùng bảng đoạn, GOI01/GOI_01; nạp | OBJ và SEG nhất quán, không báo xung đột khảo sát giả | Đã có với điều kiện M1; đối tượng có lịch sử vẫn chặn đổi gói khác |
| K: đổi đoạn | Xem trước chuyển DOAN_08 -> DOAN_09 | Kiểm tra phạm vi/phía/tim; ô duyệt chuyển đoạn; lưu lịch sử | Đã có trong hiệu chỉnh; ngoài phạm vi bị chặn |
| L: ngừng dùng | Ngừng sử dụng OBJ rồi nhập gói cũ | Không phục hồi OBJ đã ngừng dùng | Đạt tự động: hiệu chỉnh Ngừng sử dụng |
| M: rollback | Chạy fixture lỗi cố ý; thử U/REDO sau hiệu chỉnh | Khôi phục cả dữ liệu, lịch sử và hình CAD theo snapshot | Đã có test tự động; không gây crash trên hồ sơ thật |
| N: nhập lặp | Nhập cùng gói 2-3 lần | Số OBJ/block/nhãn/lịch sử không tăng | Đạt tự động: legacy và revision |
| O: hồ sơ cũ | Nạp .bht trước revision vào DWG trống | Đọc được; chưa tự gán revision; hiệu chỉnh đầu tiên yêu cầu duyệt baseline cũ | Đã có |

## Sau mỗi bài

So sánh Excel và dữ liệu hồ sơ với BEFORE; mở lịch sử; thử U/REDO. Lưu thành AFTER, đóng/mở lại, xuất .bht tên mới rồi nạp đầy đủ vào DWG trống. Kiểm tra lại số lượng và liên kết. Các bài tự động còn so sánh canonical snapshot của bản ghi và thực thể CAD; không thể thay bằng nhìn vài dòng Excel.

Nếu báo lỗi phục hồi chưa được xác minh: không lưu bản vẽ đang thử, mở BEFORE. Không xóa journal/XRecord thủ công. Nếu ảnh chụp, bản vẽ hoặc dữ liệu khác dự kiến, ghi nguyên văn lỗi và đánh dấu KHÔNG ĐẠT.

## Phiếu kết quả

| Phiên bản | Bài | DWG/OBJ | Kết quả mong đợi | Kết quả thực tế | Đạt/Lỗi/Chưa thử | Bằng chứng |
|---|---|---|---|---|---|---|
| | | | | | | |

Kết quả tự động 1.1.0, các lỗi còn tồn tại và bằng chứng cụ thể nằm trong old/1.1.0/BAO_CAO_NGHIEM_THU_1.1.0.md. Không dùng số liệu của phiên bản trước thay cho lần kiểm tra này; bảng A-O mô tả luồng và kết quả mong đợi, không phải phiếu nghiệm thu GUI.

Các phần còn giới hạn: hiệu chỉnh biển nhiều mặt/nội dung tùy chỉnh; sửa mốc Km/H ảnh hưởng tuyến; đối chiếu danh tính legacy; xử lý chọn nhánh xung đột; LOCAL theo layout và cập nhật khảo sát vào LOCAL; đồng bộ tự động DWG đóng. Không đánh dấu các phần này ĐẠT chỉ vì luồng cơ bản đã qua test.
