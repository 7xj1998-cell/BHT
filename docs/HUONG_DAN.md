# BHT 1.1.5 — Hướng dẫn sử dụng

Tài liệu ngày 16/10/2026, dành cho AutoCAD 2024 trên Windows. Gõ **BHT** hoặc **BTH** để mở bảng. Hai lệnh này mở cùng một giao diện.

## Layer theo tên tuyến trong 1.1.5

Đọc [hướng dẫn 1.1.5](HUONG_DAN_HIEU_CHINH_1.1.5.md). Trong Tuyến & báo cáo → TIM ĐANG CHỌN, bấm Đặt layer theo tên tuyến để đối chiếu tất cả tim. Tim DOAN_11 dùng BHT_TUYEN_DOAN_11. Giữ hình học, RTK, hồ sơ khóa và layer cũ. Không tự chuyển khi mở DWG.

## Phạm vi lý trình và hiệu chỉnh trong 1.1.4

Đọc [hướng dẫn 1.1.4](old/1.1.4/HUONG_DAN_HIEU_CHINH_1.1.4.md). Điểm vượt đầu/cuối tim không còn nhận lý trình đầu mút; gán tay không bỏ kiểm tra phạm vi/phía đường. Bảng hiệu chỉnh hiện điều kiện còn thiếu. Không tự sửa kết quả đã khóa. OUT_OF_RANGE kèm tên tim là lần thử thất bại, không đồng nghĩa đã gán đoạn.

## Bảng 30 dòng thành 15 đoạn trong 1.1.3

Đọc [hướng dẫn gom đoạn trùng 1.1.3](old/1.1.3/HUONG_DAN_HIEU_CHINH_1.1.3.md). Bảng đoạn có nút Kiểm tra / gom đoạn trùng, đối chiếu và xác nhận riêng. Giữ dòng đang được hồ sơ tham chiếu; không mở khóa hay đổi OBJ/RTK/tim. DOAN15 có thể gom từ 30 xuống 15 trên bản sao; DOAN08/GOI03 giữ mã cũ. Không tự thống nhất mọi mã gói hoặc xóa cả bộ mã cũ.

## Nạp kế thừa trong 1.1.2

Đọc [hướng dẫn 1.1.2](old/1.1.2/HUONG_DAN_HIEU_CHINH_1.1.2.md). Chọn/dán đường dẫn rồi bấm Kiểm tra và nạp. D14/D15 vẫn chặn hai hồ sơ khác nhau trùng OBJ-000001. Chưa có bằng chứng kết luận đã sửa triệt để lỗi đứng CAD.

## Bản sửa 1.1.1

Đã thiết kế lại bảng phân đoạn và quản lý bình đồ, thêm nhật ký nạp và bảng xung đột không chặn CAD. Trước khi nạp DOAN14 vào đoạn 15, đọc phần đối chiếu OBJ-000001 trong hướng dẫn 1.1.1; hai hồ sơ trùng mã đang liên kết hai điểm khác nhau.

## Các chức năng kế thừa 1.1.0

Đọc [hướng dẫn tính năng 1.1.0](old/1.1.1/HUONG_DAN_HIEU_CHINH_1.1.1.md) và làm bài P-T trong [hướng dẫn kiểm thử](HUONG_DAN_TEST_TOAN_BO.md) trước bài A-O. Báo cáo/xem trước có thể kéo sang bên để kiểm tra CAD; báo cáo không còn bị cắt còn 40 dòng. Nhãn dùng chiều tăng Km, điểm đầu sai bị chặn, bố trí D13 và tốc độ cập nhật hàng loạt đã được sửa. [Báo cáo kiểm chứng](old/1.1.1/BAO_CAO_NGHIEM_THU_1.1.1.md) nêu cả những kiểm tra chưa đạt và giới hạn GUI.

## Hiệu chỉnh riêng hồ sơ đã xác nhận (1.0.32)

Chọn hồ sơ khóa trong Hồ sơ đối tượng rồi bấm **Hiệu chỉnh hồ sơ đã xác nhận…**. Đọc [hướng dẫn 1.1.0](old/1.1.1/HUONG_DAN_HIEU_CHINH_1.1.1.md), gồm xem trước, lịch sử, duyệt chuyển đoạn, GLOBAL/LOCAL và phục hồi. Tab Tuyến có quản lý bình đồ, đồng bộ và phát hành R01/R02. Chỉ kiểm tra trên bản sao theo [bài test A-O và bình đồ](HUONG_DAN_TEST_TOAN_BO.md); chưa nghiệm thu GUI đầy đủ.

## Tính và xác nhận bổ sung (1.0.31)

Chọn đoạn đã có hồ sơ khóa và tim đang gắn với đoạn, tính rồi xác nhận các hồ sơ mới. Hồ sơ khóa giữ nguyên; không dùng Mở lại kết quả đoạn chỉ để bổ sung. BHT kiểm tra dữ liệu điểm/tim/mốc và chặn khi thiếu bằng chứng hoặc có thay đổi ảnh hưởng kết quả cũ.

Chức năng bổ sung của 1.0.31 được giữ và kiểm thử hồi quy trong 1.0.36. Tài liệu bản cũ nằm trong docs/old trên máy phát triển.

## Mẫu nhãn báo hiệu (1.0.29)

Chọn hướng Song song đường nối biển–trụ trong Tỷ lệ biển / nhãn để có mã chính, biển phụ phía trên đường dẫn và lý trình dưới. Biển hai trụ dùng đường dẫn phía trên; lấy đoạn dài của đường gấp khúc làm hướng chuẩn. Dòng chữ căn giữa, khoảng hở theo tỷ lệ nhãn.

Nút Tuyến > 5. Trình bày bản vẽ > Áp dụng mẫu nhãn báo hiệu… (BHTMAUNHAN): chọn biển/nhãn rồi Enter; Enter ngay không chọn áp dụng tất cả biển đã vẽ, Esc hủy. Thao tác đưa cả nhãn kéo tay về mẫu và giữ hình biển. Cập nhật thông thường vẫn giữ nhãn kéo tay.

## Đối chiếu dữ liệu nạp (1.0.28)

Khi Nạp dữ liệu kế thừa gặp xung đột, bảng liệt kê tất cả mã và trường khác nhau, có hai cột Bản vẽ hiện tại và Tệp BHT nạp vào. Tìm mã hoặc nội dung để lọc, chọn dòng để đọc đầy đủ. Sao chép toàn bộ đối chiếu lấy cả các dòng ngoài bộ lọc. Đóng bảng không thay đổi bản vẽ. Đối chiếu số liệu gốc trước khi sửa và nạp lại; bản này chưa có chọn ghi đè hay tự hợp nhất. Từ 1.0.29, nội dung biển được hiển thị thành mặt/địa danh/lý trình trên biển; định dạng XML và Unicode tương đương không tạo xung đột. HÒA và HOÀ vẫn được báo khác để đối chiếu.

## Hướng và tỷ lệ nhãn (1.0.26)

Trong tab Hồ sơ, chọn **Tỷ lệ biển / nhãn…**. Chọn tỷ lệ biển/đèn, tỷ lệ nhãn và hướng **Song song đường nối biển–trụ** hoặc **Vuông góc đường nối biển–trụ**, rồi **Áp dụng**. Hướng lấy theo đoạn đường nối gần biển nhất; mã biển, biển phụ và lý trình xoay cùng nhau theo chiều dễ đọc. Cụm nhãn của biển hai trụ căn giữa hai đường nối. Nhãn đã dịch chuyển thủ công giữ vị trí và hướng. Tỷ lệ quá lớn so với khoảng trống vẫn cần chỉnh trình bày; BHT không tự giảm cỡ chữ đã chọn.

Hàng nút **Về chuẩn 1:1 / Áp dụng / Hủy** cố định ở đáy hộp; nội dung phía trên cuộn khi màn hình nhỏ. Về chuẩn chỉ đặt lại hai tỷ lệ. **Hủy** không lưu thay đổi. Thao tác trình bày không tính lại lý trình khảo sát.

Nghiên cứu cách lấy dữ liệu gốc TDT được ghi tại [báo cáo REA](NGHIEN_CUU_TDT_REA_1.0.26.md). Bản này chưa thay cách đọc mốc hiện có bằng API native TDT.

## Xử lý theo đoạn và kế thừa dữ liệu (1.0.25)

Các báo hiệu đã nhập trong bản tổng hợp được dùng lại. Trên DWG đang làm, gõ **BHTXULYDOAN**, chọn dòng đoạn và tim đúng, **Tính lý trình theo đoạn**, kiểm tra bảng dự kiến rồi áp dụng. Bảng nêu lý do cho hồ sơ chưa đủ điều kiện. Đối chiếu nhãn, lý trình, phía đường và DIM trước khi **Xác nhận kết quả đoạn**.

Chọn **Xuất dữ liệu tích lũy (.bht)** để lưu cả hồ sơ hiện có và bảng phân đoạn. Trong DWG đoạn kế tiếp, dùng **BHTNAPTICHLUY / Nạp dữ liệu kế thừa (.bht)**. Chọn tim và đoạn mới để tính; lý trình cùng nhãn đã xác nhận của đoạn trước vẫn được giữ. Nạp cùng tệp lần nữa không tạo thêm hồ sơ. Khác tọa độ, nội dung khảo sát hoặc kết quả đã xác nhận sẽ bị chặn để đối chiếu.

**Xuất hồ sơ đoạn (.bht)** chỉ lấy các hồ sơ đã xác nhận của dòng đang chọn và tài nguyên liên quan. Nếu bình đồ thay đổi hình học tim hoặc vị trí báo hiệu, dùng **Mở lại kết quả đoạn**, cập nhật tim/mốc rồi tính lại và kiểm tra. Hai bản vẽ phải cùng hệ tọa độ thực tế; ảnh gốc phải được chuyển riêng.

**Nạp khảo sát mới (tính lại lý trình)** đặt lại kết quả nguồn để bắt đầu phép tính theo tim đích. Quy trình tiếp tục kế thừa kết quả dùng **Nạp dữ liệu kế thừa**. Hồ sơ đã thuộc đoạn khác chưa được tự chuyển đoạn; dùng bảng **BHTGANDOAN** sau khi đối chiếu. Kết quả đã xác nhận cần mở lại trước khi chỉnh sửa.

Đối với DT.830, xem [hướng dẫn và các điểm phải đối chiếu tại đoạn 10](old/RELEASE_NOTES_1.0.25.md). Xác nhận đoạn chỉ bảo vệ hồ sơ đã kiểm tra, không chứng minh đã khảo sát đủ mọi đối tượng ngoài thực địa.

## 1. Cài bản mới và bắt đầu

1. Lưu công việc đang làm, đóng toàn bộ AutoCAD.
2. Chạy **BHT-Setup-1.1.5.exe**, bấm **Cài đặt BHT**, chờ hoàn tất rồi mở lại AutoCAD 2024. Bản này dành cho kiểm tra trên bản sao DWG; chưa nghiệm thu GUI thực tế.
3. Mở bản vẽ và gõ **BHT**. Kiểm tra phiên bản bằng **BHTTRANGTHAI** hoặc **BHTNETINFO**.
4. Khi cần thử thay đổi hàng loạt, dùng một bản sao DWG trước.

Gói cài chứa runtime FAS, ba DLL và thư viện đi kèm. Không cần tự tìm thư mục mã nguồn. Không NETLOAD chồng DLL mới vào phiên CAD đang dùng bản cũ. Nếu bảng chưa mở, gõ **BHTLOAD**, rồi **BHT**; thao tác này không thay DLL đã nạp.

BHT lưu hồ sơ trong DWG. Tệp ảnh gốc vẫn nằm ngoài DWG. Việc chuyển mọi chữ thành nét để gửi một DWG không cần phông chưa được triển khai trong bản này.

## Chọn loại tim và quản lý mốc (1.0.23)

Khi **Nạp tuyến…**, chọn hình học rồi chọn **Tim đường / Tim rãnh / Tuyến khác** trong hộp thông tin. **Sửa tuyến…** mở cùng hộp để đổi loại, ID, phạm vi lệch và ngoại suy. Các số nhận dấu phẩy hoặc dấu chấm; thông số sai được báo ngay tại ô. Cập nhật một tim TDT đã nạp giữ loại và mốc hiện có.

Chọn dòng tim rồi mở **Bảng mốc: xem vị trí / xóa mốc…**. Bảng xếp theo khoảng cách dọc tim, hiện lý trình trước/sau, nguồn và cảnh báo hệ số. Chọn một dòng hoặc nhấp đúp để xem vị trí trên CAD bằng dấu cộng vàng; Enter trở lại bảng. Ctrl/Shift chọn nhiều mốc rồi **Xóa mốc đã chọn…**. Xóa chỉ bỏ mốc BHT của tim; giữ cọc TDT gốc. Mốc tự động từ hồ sơ cọc được sửa tại hồ sơ. Một lần Undo khôi phục lần xóa. Sau khi đổi thông số hoặc xóa mốc, tính lại lý trình trước khi xuất.

## Nạp từ bản tổng hợp vào DWG đoạn (1.0.22)

1. Mở DWG đoạn chưa có hồ sơ BHT; có thể đã nạp tim và bảng đoạn.
2. Trong Tuyến, dùng **Nạp khảo sát mới (tính lại lý trình)…**. Giữ điểm/ảnh/mặt biển và bảng đoạn; kết quả lý trình nguồn chỉ giữ đối chiếu. Nếu muốn giữ cả tim và cách tính nguồn, dùng **Nạp nguyên dự án** trong mục bổ sung.
3. Nạp tim TDT, chọn loại **Tim rãnh** trong hộp nạp nếu tính theo rãnh; chọn đúng dòng tim của đoạn, điểm đầu và chiều tăng Km.
4. Đọc và xác nhận cọc TDT, hoặc thêm ít nhất hai mốc đã biết bao quanh phạm vi tính. Số Km/H của cọc tiêu nhập từ file tổng hợp không làm mốc tim TDT.
5. **Tính lại theo tim đang chọn…**; kiểm tra hồ sơ chưa xác định, dùng DIM trực tiếp, rồi chỉnh nhãn và xuất Excel.
6. Nếu phân chia đoạn, mở bảng đoạn/gói và gán tim mới cho đoạn cần dùng rồi phân đoạn. Đoạn mới nhập chưa gán tim; đoạn có sẵn ở đích giữ tim cũ của đích.

**Xuất .bht có cả bảng đoạn/gói đã nạp vào DWG** và liên kết tim của đoạn. Sau xuất/nạp, xem thông báo số hồ sơ/tim/đoạn. Có XLSX bên ngoài nhưng chưa nạp vào DWG thì bảng đó không nằm trong .bht. Filebaohieu.bht gốc được kiểm tra có 0 đoạn.

## Đo lệch tim tại trụ gần nhất (1.0.21)

Với biển có từ hai trụ và nhiều điểm khảo sát, BHT chọn trụ có khoảng cách mặt bằng nhỏ nhất tới tim đang dùng. Lý trình và phía tuyến tính tại cùng trụ đó. Ký hiệu trình bày vẫn giữ vị trí đã đặt. Nếu đoạn có tim riêng, phép đo dùng tim của đoạn. Hồ sơ cũ cần **Tính lý trình** lại, rồi xuất lại Excel.

Gõ **BHTDIMTIM**, chọn biển/điểm hoặc nhập ID hồ sơ. Cũng có thể bấm **DIM từ điểm trụ gần nhất tới tim** trong Tuyến & báo cáo. DIM nằm trên lớp vàng **BHT_DIM_KIEM_TRA**, từ điểm khảo sát tới hình chiếu trên tim, đo XY, không tính chênh cao. Số hiện ba chữ số thập phân và đơn vị m. Gọi lại cập nhật DIM cùng hồ sơ/tim; có Undo. Nếu hồ sơ chưa có tim hoặc lớp DIM bị khóa/tắt/đóng băng, BHT báo nguyên nhân.

Sau khi sửa tim hoặc điểm khảo sát, gọi lại DIM và tính lại lý trình để cập nhật hồ sơ. Dấu chấm đặc tại tâm X xuất hiện khi cập nhật ký hiệu, bao gồm nhóm Khác. Cọc tiêu tiếp tục giữ cách vẽ riêng.

## Đoạn / gói thầu và tim riêng (1.0.19)

Trong **Tuyến & báo cáo**, mở **Phân chia đoạn / gói thầu**. Chọn **Nạp Excel / bảng BHT (.bhtseg)**, chọn `.xlsx`, xem trước/sửa dữ liệu rồi chọn **Chuyển sang .bhtseg và nạp**. BHT giữ file Excel gốc; lưu mới dùng `.bhtseg` có kiểm tra dữ liệu, không dùng TSV. Bảng phân chia hiện có đọc được 15 đoạn và 3 nhóm ô gộp. Mã gói GOI_01…03 được tạo từ tên nhóm vì Excel chưa có mã gói; kiểm tra trước khi lưu.

Chọn **Xem đoạn / gán tim đường, tim rãnh**. Ở cột **Tim của đoạn**, chọn tuyến đã khai báo bằng Polyline hoặc TDT. Có thể chọn nhiều dòng rồi **Gán tim cho các dòng chọn**. Dùng **Lưu .bhtseg và áp dụng** để cập nhật cả tệp riêng; **Áp dụng vào bản vẽ** chỉ cập nhật trong DWG. Lưu DWG để giữ dữ liệu.

Chọn **Gán đoạn / gói theo tim của đoạn**. Tim cần ít nhất hai mốc hợp lệ; BHT tính lý trình trên từng tim, chọn tim phù hợp gần nhất. Hai tim chênh offset không quá 1 cm hoặc hai đoạn cùng mốc biên cần **Chọn đoạn / gói cho hồ sơ** để quyết định. Trái/phải theo chiều tăng lý trình tim của đoạn. Đoạn để trống tim dùng lý trình hiện có. Số Km/H đã nhập của cọc được giữ nguyên.

Đổi tim/phạm vi sẽ bỏ gán cũ để phân lại; gán tay giữ nếu định nghĩa đoạn không đổi. Đổi tên tuyến cập nhật liên kết. Nếu thiếu tim, mở bảng gán lại; không lấy tim khác thay thế. Tệp `.bhtseg` chỉ chứa bảng; chuyển cả hình học và tim dùng **Xuất dự án BHT (.bht)**.

Với Excel khác cấu trúc, dùng **Lưu bảng Excel mẫu**. Các cột bắt buộc: SEGMENT_ID, KM_START_M, KM_END_M, SIDE. PACKAGE_ID, ROUTE_ID, PACKAGE_NAME, NOTE là thông tin bổ sung. Lý trình nhận mét hoặc Km48+469,10. Công thức Excel phải được tính/lưu trước khi nạp.
## 2. Năm tab dùng để làm gì?

| Tab | Công việc chính |
| --- | --- |
| Tổng quan | Xem số liệu bản vẽ, chuyển nhanh đến bước làm tiếp |
| Điểm vị trí | Nhập CSV RTK, tìm điểm, thu phóng, tạo hồ sơ và chỉnh nhãn điểm |
| Hồ sơ đối tượng | Chọn biển/đèn, sửa nội dung, gắn điểm chân trụ, đặt ký hiệu, tính riêng một hồ sơ |
| Ảnh hiện trường | Nhập KMZ, xem ảnh, ghép ảnh với hồ sơ và cập nhật ký hiệu ảnh |
| Tuyến & báo cáo | Quản lý mốc/tuyến, tính toàn bộ hồ sơ, chỉnh hướng biển, kiểm tra và xuất báo cáo |

Chức năng nhập CSV chỉ nằm ở **Điểm vị trí**; nhập KMZ và thống kê ảnh nằm ở **Ảnh hiện trường**. Trong **Tuyến & báo cáo**, bước **4. Tính lại và kiểm tra** dùng tim đang chọn. Tính theo cọc nằm trong mục **Phương pháp theo cọc / nạp nguyên dự án**.

## 3. Nhập điểm và tạo hồ sơ báo hiệu

1. Mở **Điểm vị trí → Nhập CSV RTK**. Chọn tệp và mã đợt số liệu theo hướng dẫn trên dòng lệnh. CSV RTK có các cột tên điểm, Bắc, Đông, cao độ, mô tả; xem trước dữ liệu trước khi nhập.
2. Tìm theo tên/mô tả. Chọn điểm để làm sáng trên bản vẽ; nhấp đúp hoặc bấm **Thu phóng** để xem vị trí.
3. Chọn một hoặc nhiều điểm rồi bấm **Tạo hồ sơ từ điểm**. Cũng có thể dùng **Hồ sơ đối tượng → Tạo từ RTK…** để chọn trực tiếp trên CAD.
4. Chọn nhóm đối tượng, số trụ, tình trạng và thông tin thực tế. Biển hai trụ cần đủ hai điểm chân trụ; không dùng một điểm rồi tự đoán điểm còn lại.
5. Bấm **Lưu hồ sơ**, sau đó **Đặt ký hiệu…** để chọn cách đặt trên CAD, hoặc **Vẽ / cập nhật ký hiệu** để cập nhật theo vị trí đã lưu.

**Gỡ điểm chọn** chỉ gỡ liên kết khỏi hồ sơ. **Xóa hồ sơ…** xóa hồ sơ theo xác nhận; điểm đo được giữ. Không kéo điểm RTK để làm đẹp bản vẽ: hãy dời ký hiệu/nhãn, giữ tọa độ khảo sát.

### Gom và xoay nhãn RTK / Timemark

Trong **Điểm vị trí**, bấm **Gom / xoay nhãn…**. Thao tác áp dụng cho các cụm nhãn RTK và nhãn ảnh Timemark của BHT trong bản vẽ.

- **Theo UCS hiện tại**: chữ đọc theo chiều dương trục X của UCS hiện hành.
- **Chọn hướng A → B trên CAD…**: chọn hai điểm để xác định chiều đọc chữ. Đổi chiều A/B sẽ đảo chữ 180°.

Các dòng tên, mô tả, cao độ và mã điểm được gom về gần điểm rồi xoay cùng hướng, xếp thành một cụm với khoảng dòng theo cỡ chữ. Nhãn Timemark và ký hiệu ảnh cùng theo hướng chọn. Dấu điểm được đổi góc hiển thị; tọa độ khảo sát, vị trí GPS, hồ sơ và liên kết ảnh được giữ nguyên. Cụm có lớp khóa hoặc khác mặt phẳng XY được bỏ qua và báo trên dòng lệnh. Lệnh này chủ động bố trí lại cả nhãn đã kéo tay; nếu cần giữ vị trí tay, dùng Cập nhật nhãn RTK.

Lệnh tắt: **BHTXOAYNHAN** → **UCS** hoặc **Huong**. Dùng **U** ngay sau thao tác để hoàn tác cả lần xoay. Nhấn Enter khi chọn A/B để hủy.

**Cập nhật nhãn RTK** giữ hướng và vị trí nhãn đã xoay, đồng thời có thể tạo nhãn còn thiếu. Nếu vừa nhập thêm điểm hoặc bật thêm loại nhãn, hãy cập nhật trước rồi xoay tất cả. **BHTNHANTUDONG** đưa các nhãn trong phạm vi chọn về bố trí tự động, chữ nằm ngang WCS.

Hộp **Tỷ lệ ký hiệu và nhãn RTK…** đã thu gọn; đưa chuột lên ô nhập để xem giải thích.

Cọc bổ sung có số Km/H hiện nhãn như **H3/49**; cọc chưa nhập số hiện **Cọc tiêu**. Nhãn lấy từ hồ sơ, không tự xác nhận cọc làm mốc tính lý trình. Bấm **Vẽ / cập nhật ký hiệu** trong hồ sơ cọc để tạo hoặc sửa nhãn.

## 4. Chọn biển, sửa chữ và biển ghép

Trong **Hồ sơ đối tượng**, chọn nhóm biển báo rồi bấm **Chọn biển…**.

- Tìm bằng mã hoặc tên, có thể gõ không dấu.
- Chọn mẫu rồi mở **Nội dung biển…** để nhập số, giờ, khoảng cách, chiều cao hoặc địa danh theo hồ sơ. Không dùng địa danh mẫu làm số liệu thực tế.
- Biển tùy chỉnh được thêm bằng **Thêm biển tùy chỉnh…** ngay trong thư viện.
- Muốn ghép STOP và bảng “DỪNG LẠI QUAN SÁT”, dùng **Thêm mặt**, rồi **Lên/Xuống** để xếp bảng phụ dưới STOP. Đây là nhiều mặt trên một hồ sơ.
- Bấm **Chọn biển**, quay về hồ sơ, lưu và cập nhật ký hiệu.

**Tỷ lệ biển / nhãn…** điều chỉnh cỡ ký hiệu và cỡ nhãn. **Tô nền** bật/tắt nền biển; hình nhận dạng và viền vẫn được giữ. Lần đổi đầu tiên có thể chậm hơn vì cần chuẩn bị mẫu hiển thị.

**Chọn block CAD / Nạp DWG…** áp dụng block riêng cho hồ sơ đang sửa. **Công cụ bổ sung → Gán block mặc định cho nhóm…** thay mẫu chung của cả nhóm; chỉ dùng khi muốn thay quy ước chung.

### Bố trí hàng loạt theo tim hoặc ra ngoài đường

**Nhãn biển có lý trình:** tên/mã biển ở dòng trên, lý trình ở dòng dưới, cùng phông, cỡ chữ và góc xoay. Ví dụ dòng trên `W.202a`, dòng dưới `Km38+966.96`. Khi một trụ có nhiều mặt biển, lý trình nằm dưới danh sách tên các mặt biển. Biển chưa có lý trình chỉ hiện tên; BHT không tự thêm số lý trình.

Để chuyển các biển đã vẽ sang nhãn hai dòng, vào **Tuyến & báo cáo → Công cụ bổ sung → Cập nhật mẫu biển đã vẽ** sau khi cài bản 1.0.8. Thao tác giữ vị trí, góc, tỷ lệ và dữ liệu khảo sát, không tính lại lý trình. Nút này chỉ cập nhật biển/bảng; đèn được chỉnh bằng hồ sơ đèn riêng. Khi sửa một biển, **Chèn / cập nhật biển** cũng cập nhật nhãn của biển đó.

Mở **Tuyến & báo cáo → Công cụ bổ sung → Bố trí tất cả báo hiệu…**, hoặc gõ **BHTKHOANGCACH**. Áp dụng cho các biển, bảng và đèn đã vẽ; cọc tiêu, cột Km và điểm khảo sát giữ vị trí thực địa.

| Cách bố trí | Khi dùng | Dữ liệu cần nhập |
| --- | --- | --- |
| **Cách tim: chọn tuyến đã nạp** | Bản vẽ có đường tham chiếu trong bảng Tuyến BHT | Chọn tuyến dùng làm tim; nhập khoảng cách tối thiểu từ tim đến hình biển/nhãn |
| **Đưa ra ngoài đường theo PL tham chiếu** | Chưa có tim đường gốc | Bề rộng từ PL đến mép trái/phải; khoảng lùi ngoài mép |
| **Đổi khoảng cách từ điểm đo** | Muốn giữ hướng dịch chuyển đang đặt | Khoảng cách từ điểm đo đến gốc ký hiệu |

**Có tim đường:** chọn tim trong hộp thoại, nhập khoảng cách, chọn kiểu đường nối rồi bấm **Áp dụng tất cả**. BHT dùng chiều tuyến đã lưu. Ví dụ nhập 12 nghĩa là toàn bộ hình biển và nhãn cách tim ít nhất 12 đơn vị bản vẽ; không phải gốc block cách tim 12.

Danh sách hiển thị **ID tuyến — Loại**, ví dụ **TUYEN1 — Khác**. Từ bản 1.0.7, tuyến đang phân loại Khác vẫn có thể được chọn làm tim bố trí hình; không cần nạp lại. Chỉ chọn đường thực sự muốn dùng làm tim. Thao tác này giữ nguyên loại tuyến, mốc và cách tính lý trình. Nếu muốn đổi phân loại hồ sơ tuyến, dùng **Dữ liệu tuyến → Sửa tuyến…** riêng.

Nếu báo thiếu hình học, vào **Chọn lại tuyến…** để gắn một Polyline hợp lệ. Nếu báo hình học không phù hợp, kiểm tra đường có phải Polyline 2D mở trong Model, nằm trên mặt phẳng XY hay không. Tuyến proxy TDT cần nạp bằng công cụ tuyến TDT hiện có trước.

**Chưa có tim đường:** nhập bề rộng hai bên và khoảng lùi, bấm **Áp dụng tất cả**, sau đó chọn một PL giữa đường trên CAD và bấm gần đầu PL mong muốn. Trái/phải tính theo chiều từ đầu vừa chọn đến cuối PL. Ví dụ PL → mép trái = 8, PL → mép phải = 5, khoảng lùi = 1: hình và nhãn bên trái cách PL ít nhất 9, bên phải ít nhất 6 đơn vị. Đây chỉ là đường tham chiếu để bố trí hình; BHT không nạp PL này làm tim tính lý trình.

Khoảng cách dùng **đơn vị bản vẽ**. Nếu bản vẽ dùng mét thì nhập mét; nếu dùng milimét thì nhập milimét. Bề rộng cần lấy từ hồ sơ/bản vẽ, BHT không suy bề rộng đường từ ảnh.

Hai cách trên xác định phía bằng **tọa độ điểm đo**, rồi dịch hình theo phương vuông góc với tim/PL tại từng vị trí. Hỗ trợ Polyline 2D mở, gồm đoạn thẳng, cung tròn và đoạn gãy khúc. Giữ góc xoay, tỷ lệ, điểm đo, lý trình và quan hệ mốc/tuyến. Muốn đổi góc biển phải dùng công cụ hướng biển riêng.

Khoảng cách được tính cả kích thước block và nhãn sau khi scale/xoay. Với đường cong hoặc gãy khúc, BHT dùng vùng bao an toàn nên một số biển sẽ nằm xa hơn giá trị tối thiểu đã nhập. Phía được xác định lại từ điểm đo khi đổi chiều tham chiếu; không đổi từ bên này sang bên kia theo vị trí hình đã kéo tay.

Nhấn **Enter/Esc** khi chọn PL/đầu PL để hủy. Cụm có thành phần khóa lớp, thiếu điểm, không đo được kích thước, điểm cách PL ≤ 0.001 đơn vị, nằm ngoài đầu/cuối PL hoặc các chân trụ nằm khác phía sẽ được bỏ qua. Kết quả và ID cần kiểm tra được ghi ở dòng lệnh CAD; nhấn **F2** để đọc lại. **U** hoàn tác cả lần bố trí. Các kiểu nối riêng cũ được thay bằng kiểu đã chọn cho những hồ sơ thực sự cập nhật.

Chức năng không tự tránh chồng giữa các biển hoặc giữa biển với nhà, thửa đất, chữ nền. Nếu công trình có nhiều nhánh, chọn tham chiếu bao quát đúng phần cần bố trí; không dùng một PL xuyên nhiều nhánh để suy phía. Proxy TDT trực tiếp và tim kín không được hỗ trợ; dùng tim/Polyline 2D thường đã nạp vào BHT.

### Chọn báo hiệu để copy

Với **Đường thẳng**, đường nối đi trực tiếp từ điểm gốc đến chân trụ hoặc mép dưới block. Với **Gấp khúc**, đường nối đi ra cạnh và xuống dưới biển để tránh mặt biển; khi hai điểm đã thẳng hàng ở phía dưới, đường có thể không cần gấp. Ký hiệu, nhãn biển, đường nối và dấu gốc dùng chung lớp **BHT_KYHIEU**, giữ màu hiển thị riêng. Nút **Chọn tất cả báo hiệu trên CAD** hoặc lệnh **BHTCHONBAOHIEU** chọn đủ các thành phần để COPY.

**Chọn báo hiệu cùng nhóm… / BHTCHONTUONGTU:** chọn một hình, nhãn hoặc đường nối BHT làm mẫu. Lệnh chọn đủ hình, chữ, đường nối và dấu gốc thuộc cùng nhóm hồ sơ. Ví dụ chọn mẫu Biển báo sẽ chọn các mã biển khác nhau thuộc nhóm Biển báo; không tự chọn đèn hoặc cọc tiêu.

**SELECTSIMILAR theo lớp… / BHTSELECTSIMILAR:** đặt SELECTSIMILARMODE=2 rồi chạy SELECTSIMILAR, bỏ điều kiện tên block. Thiết lập này cũng áp dụng cho lệnh SELECTSIMILAR gõ trực tiếp về sau. Giá trị 130 thường dùng cả layer và tên block, nên các mã biển khác nhau bị chọn thiếu. CAD vẫn phân biệt loại đối tượng: chọn block không tự chọn TEXT/LINE. Muốn COPY đủ cả hình, chữ và đường nối, dùng hai lệnh chọn của BHT ở trên.

Các lệnh chọn giữ tập lựa chọn sau khi kết thúc. Tiếp tục COPY/MOVE ngay trên CAD; Esc sẽ bỏ lựa chọn. Nếu PICKFIRST bị tắt, lệnh BHT sẽ bật lại. COPY chỉ mang hình và dữ liệu gắn trên hình; chuyển đầy đủ hồ sơ để chỉnh bằng BHT cần dùng xuất/nạp dự án.

Trong **Đặt ký hiệu…**, lựa chọn **Đường dẫn thẳng / Gấp khúc tự động / Chọn điểm trung gian** áp dụng riêng cho hồ sơ hiện tại. Hộp mở lại theo kiểu đã lưu trong hồ sơ. Kiểu nối giữ khi cập nhật, đổi tỷ lệ và lưu/mở lại; không tự biến Đường dẫn thẳng thành Gấp khúc. Chọn điểm trung gian dùng các điểm đã chỉ định; thao tác áp dụng toàn bộ sẽ thay lựa chọn riêng này.

## 5. Đèn tín hiệu và đèn chiếu sáng

Chọn đúng nhóm trong **Hồ sơ đối tượng**, rồi bấm **Chọn loại đèn…**.

| Nhóm | Mẫu được chọn |
| --- | --- |
| Đèn tín hiệu | Ba màu; ba màu cần vươn trái; ba màu cần vươn phải; cảnh báo vàng |
| Đèn chiếu sáng | Đơn mặt bằng; đôi mặt bằng; trang trí mặt bằng; cần trái mặt đứng; cần phải mặt đứng |

Bấm **Dùng mẫu này**, lưu hồ sơ và đặt/cập nhật ký hiệu. Các mẫu đèn không có nhãn ghi chú dưới hình. Đổi cỡ qua công cụ tỷ lệ trong hồ sơ, rồi kiểm tra bản vẽ ở tỷ lệ trình bày đang dùng.

## 6. Tính lý trình khi chỉ có cọc thực địa

Dùng quy trình này khi không có tim đường gốc. Đường PL vẽ thử không phải cơ sở để suy khoảng lệch tim đường.

1. Nhập cọc tiêu và cột Km vào hồ sơ, gắn đúng điểm vị trí. Cọc tiêu có Km và H; cột Km có Km. Ví dụ **H4/40** là **Km40+400**.
2. Mở **Tuyến & báo cáo → Mốc lý trình đã nhập…**. Bảng gồm cả cọc tiêu và cột Km, nguồn vị trí và cảnh báo.
3. Kiểm tra số cọc, thứ tự, vị trí và khoảng cách. Nếu có cọc bổ sung thủ công, chỉ chọn **Xác nhận dùng mốc** cho từng cọc đã đối chiếu. Cọc thiếu số hoặc vị trí chưa đủ điều kiện làm mốc.
4. Bấm **Dùng các mốc này**. Dòng trạng thái sẽ cho biết đang tính theo cột Km và cọc tiêu.
5. Bấm **Tính lý trình tất cả hồ sơ**. Mở báo cáo kết quả; nhấn **F2** để đọc lại thông báo.

Không cần tạo nhóm cọc cho một tuyến đơn giản đã nhập đủ mốc. **Nâng cao: nhóm đường / nhánh** dùng khi có nhiều đường hoặc nhánh cần tách riêng. Cặp/nhóm riêng đã gán cho một hồ sơ vẫn được giữ khi đổi danh sách mặc định.

**Tính riêng một hồ sơ:** chọn hồ sơ → **Tính lý trình hồ sơ này**. Muốn chỉ định hai mốc đã đối chiếu, chọn **Chọn 2 mốc…** và xếp theo chiều Km tăng. **Dùng mốc trong danh sách…** mở bảng kiểm tra mốc rồi tính hồ sơ đang chọn.

**Bổ sung cọc biết vị trí:** chọn **Thêm cọc…**, nhập nhóm và số Km/H, chọn vị trí trên CAD. Nguồn vẫn được ghi là thủ công. Việc tạo cọc không tự xác nhận cọc làm mốc; xác nhận riêng trong bảng mốc. Kết quả sử dụng mốc thủ công được đánh dấu tham khảo/cần kiểm tra.

### Khi kết quả chưa xác định

| Thông báo | Cần làm |
| --- | --- |
| Không có đoạn cọc bao quanh | Kiểm tra vị trí và các mốc đầu/cuối; BHT không tự ngoại suy |
| Nằm trong nhiều đoạn cọc | Chọn hai cọc đúng hoặc tách nhóm đường/nhánh |
| Trùng lý trình / đổi hướng bất thường | Đối chiếu số Km/H, thứ tự và nhánh đường; không xóa mốc chỉ để hết cảnh báo |
| Đoạn dài / thiếu cọc | Kiểm tra các mốc ở giữa và đường cong trước khi chấp nhận |
| Khoảng cách không phù hợp chênh lệch Km/H | Kiểm tra số cọc và tọa độ; không tự đổi tọa độ khảo sát |
| Cần kiểm tra lại | Mốc hoặc dữ liệu đã thay đổi, hoặc kết quả dùng mốc thủ công; xem ghi chú chi tiết |

Khi tính theo cọc thất bại, giá trị cũ có thể được giữ nhưng được đánh dấu **cần kiểm tra lại**. Có số lý trình trên màn hình chưa có nghĩa là kết quả đã được xác nhận. Khoảng cách tới đường nối hai cọc không phải khoảng lệch tim đường.

## Chuyển toàn bộ dự án sang bản vẽ khác

Dữ liệu BHT phải ở **Model**. Nếu có đối tượng BHT trong Layout, lệnh dừng và yêu cầu chuyển về Model; không tự bỏ các đối tượng đó khi xuất.

Trong **Tuyến & báo cáo → 6. Xuất hồ sơ**, dùng **Xuất dự án .bht…** hoặc lệnh **BHTXUATDUAN**. Chọn tên `.bht` mới. BHT xuất các điểm, hình/nhãn báo hiệu, ảnh đã chèn và hình học tuyến được tham chiếu, kèm toàn bộ dữ liệu hồ sơ, mốc/chuỗi cọc, tuyến, đoạn/gói và liên kết ảnh. Bảng phân chia phải được nạp và lưu trong DWG nguồn trước khi xuất; thông báo kết quả cho biết số đoạn có trong gói.

Mở DWG đích rồi dùng **Nạp nguyên dự án…** hoặc **BHTNAPDUAN** trong mục phương pháp bổ sung. Lệnh nhận `.bht` và DWG BHT cũ, nạp vào DWG **chưa có dữ liệu BHT**; có thể có bình đồ và hình CAD khác. Bản vẽ trống nhận đơn vị nguồn; nếu bản vẽ đã có hình khác đơn vị, BHT hỏi có giữ nguyên tọa độ hay hủy. BHT tự đổi tham chiếu handle tuyến và tên block khi AutoCAD tạo tên khác; không tự đổi tỷ lệ tọa độ.

Nếu DWG đích đã có BHT, lệnh dừng và giữ dữ liệu hiện tại. Theo phạm vi đã chốt, bản này chỉ nạp vào DWG chưa có dữ liệu BHT và không gộp hai dự án. Có thể mở bản DWG xuất trực tiếp để chỉnh bằng BHT, hoặc nạp vào một bản sao bình đồ chưa có BHT.

Liên kết ảnh được giữ, **tệp ảnh gốc nằm ngoài gói .bht**. Khi chuyển máy, gửi thư mục ảnh và dùng công cụ chỉ lại thư mục ảnh nếu đường dẫn thay đổi. Phông chữ cần có trên máy nhận để hiển thị đúng.

## 7. Làm việc khi có tim tuyến đáng tin cậy

1. Trong **Tuyến & báo cáo**, chọn **Polyline thường** hoặc **Tuyến TDT 9.1**, bấm **Nạp tuyến…** và chọn hình học trên CAD.
2. Chọn dòng tuyến để làm sáng; bấm **Hiển thị / zoom tuyến** để xem toàn tuyến. Chức năng này có thể bật/mở băng lớp chứa tuyến, nhưng giữ trạng thái khóa lớp.
3. Chọn **Chọn điểm đầu và chiều tuyến**; dùng **Đảo chiều tuyến** nếu cần. Không mặc nhiên coi đầu PL là đầu tuyến thực tế.
4. Kiểm tra mốc. Công cụ **Đọc / kiểm tra cọc TDT** chỉ hiện khi chọn tuyến TDT. Nạp tim chỉ lấy hình học; chạy `BHTDOCCOCTDT` và xác nhận mốc trước khi tính lý trình. Nếu tim là proxy, chạy `BHTKIEMTRATDT` để xem chẩn đoán và mở lại DWG bằng profile TDT 9.1.
5. Dùng **Thêm mốc bổ sung / điểm gãy Km** khi cần khai báo mốc ngoài hồ sơ. Cọc đã có số và đủ điều kiện không cần nhập lặp bằng một lệnh xác nhận Km khác.
6. Chọn đúng dòng tim rồi bấm **Tính lại theo tim đang chọn…** ở bước 4. Lệnh chuyển sang tính theo tim đó, cho xem kết quả trước khi xác nhận; các hồ sơ không xác định được sẽ bỏ lý trình cũ và có trạng thái để kiểm tra.

**Sửa tuyến…** sửa khai báo; **Chọn lại tuyến…** thay hình học tham chiếu; **Xóa tuyến…** xóa khai báo theo xác nhận. Kiểm tra và tính lại hồ sơ sau khi đổi tuyến.

**Kiểm tra 2 cọc Km (không có tuyến)** chỉ nội suy thử tại một vị trí. Lệnh này không cập nhật toàn bộ hồ sơ; muốn cập nhật hồ sơ hãy dùng quy trình mốc thực địa ở mục 6.

Nhãn tên biển và dòng lý trình được đặt phía đầu mặt biển, cùng trục và có khoảng hở với đường nối. Khi cần tránh đường nối, BHT dịch cả cụm nhãn, giữ lý trình đi cùng tên biển. Với các biển đã vẽ ở bản cũ, dùng **Cập nhật mẫu biển đã vẽ** trong nhóm trình bày.

## 8. Xoay biển và chỉnh nhãn

- Có hướng tham chiếu rõ: dùng **Xoay biển vuông góc với hướng…**, chọn phạm vi và hướng trên CAD theo dòng nhắc.
- Chỉ có cọc thực địa: kiểm tra nguồn mốc trước rồi dùng **Xoay các biển vuông góc theo cọc**. Hướng tính từ đoạn cọc hợp lệ; các trường hợp không đủ mốc cần xử lý riêng.
- Muốn đặt một biển theo ý: **Đặt ký hiệu…** cho phép chọn hướng/vị trí mà vẫn giữ điểm khảo sát.
- **Cập nhật nhãn RTK** sắp lại nhãn; **Tỷ lệ ký hiệu và nhãn RTK…** chỉnh cỡ dấu X và chữ riêng nhau.
- **Sắp xếp nhãn trên bản vẽ** dùng khi có nhiều nhãn chồng. Nhãn đã dời tay được quản lý riêng; chỉ trả về tự động khi chủ động chọn.

## 9. Ảnh, báo cáo và gói thầu

**Ảnh:** nhập bằng **Ảnh hiện trường → Nhập KMZ**. **Đề xuất ghép ảnh** chỉ đưa gợi ý; dùng thao tác gắn/xác nhận để lưu liên kết. Ảnh thiếu GPS không được đặt bừa tại tọa độ 0,0. Khi chuyển thư mục, dùng **Chọn thư mục ảnh**. Ảnh không tự được nhúng vào DWG.

**Kiểm tra:** mở **Tuyến & báo cáo → Kiểm tra bản vẽ**. Báo cáo ưu tiên việc cần xử lý, sau đó là số liệu. “Ký hiệu không tìm thấy hồ sơ” nghĩa là còn hình vẽ nhưng liên kết tới hồ sơ BHT bị thiếu; không có nghĩa nên xóa ngay hình đó. Sao chép báo cáo hoặc xem **F2** khi cần đọc lại. Dải lỗi màu hồng dưới bảng đã được bỏ.

**Xuất:** **Xuất báo cáo biển báo Excel** tạo hai trang Tổng hợp và Danh sách biển cho các nhóm biển báo, bảng chỉ dẫn, quảng cáo, Khác và Chưa xác định. Từ 1.0.20, mỗi mặt biển (gồm biển phụ) có một dòng riêng, cùng ID hồ sơ và lý trình của cụm. “Mặt trong cụm” ghi 1/2, 2/2…; số trụ và tổng số mặt của cụm chỉ ghi ở dòng đầu để cộng không trùng. Cột “Nội dung mặt biển” giữ số mét, tốc độ, giờ và chữ từng mặt. Tổng hợp đếm từng mặt, tách nội dung khác nhau. Báo cáo chỉ đọc dữ liệu đã lưu, không tự tính lại lý trình.

**Lệch tim tuyến (m), tên cũ Offset:** khoảng cách trên mặt bằng từ vị trí hồ sơ đến tim dùng để tính lý trình (tim đường, tim rãnh hoặc tuyến đã gán). Không phải khoảng dịch ký hiệu khi trình bày; kéo ký hiệu không làm thay đổi giá trị. Ví dụ lệch 2,662 m tới TIMRANHD8 nghĩa là vị trí hồ sơ cách tim rãnh đó 2,662 m.

**Xuất dữ liệu kỹ thuật CSV** xuất dữ liệu điểm, hồ sơ, ảnh và tổng hợp; quá trình này có tính lại lý trình theo phương pháp đang chọn. Kiểm tra phương pháp trước khi xuất.

**Gói thầu:** phần **Phân chia đoạn / gói thầu (tùy chọn)** có **Lưu bảng Excel mẫu** và **Nạp Excel / bảng BHT**. Nạp XLSX rồi chuyển thành `.bhtseg`; có thể gán tim riêng cho từng đoạn. TSV chỉ dùng chuyển đổi dữ liệu cũ. Xem phần Đoạn / gói thầu ở đầu hướng dẫn. Công trình không phân gói có thể bỏ qua phần này.

## 10. Lệnh tắt hay dùng

| Lệnh | Công dụng |
| --- | --- |
| BHT / BTH | Mở bảng quản lý |
| BHTHELP | Xem danh sách lệnh trong CAD |
| BHTNHAP | Nhập CSV RTK |
| BHTXOAYNHAN | Gom / xoay nhãn RTK và Timemark theo UCS hoặc hướng chọn |
| BHTKHOANGCACH | Bố trí tất cả biển/bảng/đèn: Cách tim, Ngoài đường theo PL, hoặc Theo điểm đo |
| BHTCHONBAOHIEU | Chọn toàn bộ thành phần báo hiệu để COPY |
| BHTCHONTUONGTU | Chọn đủ thành phần báo hiệu cùng nhóm |
| BHTSELECTSIMILAR | SELECTSIMILAR theo lớp, bỏ điều kiện tên block |
| BHTXUATDUAN | Xuất dự án BHT thành DWG mới |
| BHTNAPDUAN | Nạp dự án vào DWG chưa có dữ liệu BHT |
| BHTDOITUONG | Tạo hồ sơ từ điểm |
| BHTKYHIEU | Vẽ/cập nhật ký hiệu |
| BHTDATTUDO | Đặt ký hiệu tự do |
| BHTHUONGBIEN | Xoay biển vuông góc theo hướng/tuyến |
| BHTLYTRINH | Tính tất cả hồ sơ theo phương pháp đang chọn |
| BHTKT | Kiểm tra dữ liệu bản vẽ |
| BHTINFO | Xem dữ liệu đối tượng được chọn |
| BHTTRANGTHAI | Xem tình trạng dữ liệu BHT |
| BHTKMZ | Nhập ảnh KMZ |
| BHTXUAT | Xuất dữ liệu kỹ thuật CSV |
| 11 / BHTLAYON | Bật các lớp |
| 22 / BHTLAYOFF | Tắt lớp của đối tượng được chọn |
| 33 / BHTLAYISO | Cô lập lớp |
| 44 | Giữ nguyên chức năng TDT; BHT không đăng ký lệnh này |
| 55 / BHTLAYFRZ | Đóng băng lớp của đối tượng được chọn |
| 66 / BHTLAYTHW | Mở băng các lớp |
| LAYUNISO | Lệnh AutoCAD: khôi phục sau khi cô lập lớp |
| F2 | Xem lại lịch sử lệnh và thông báo |

BHT chỉ gán các phím số khi chưa có lệnh cùng tên lúc nạp. Nếu phần mềm khác đã dùng 55/66, gọi **BHTLAYFRZ/BHTLAYTHW**. Mở băng bằng 66 không thay thế LAYUNISO.

Danh mục đầy đủ và quyết định giữ/bỏ từng nhóm nút nằm trong **DANH_MUC_LENH_NUT_1.0.8.md**. Kết quả kiểm thử và những phần chưa nghiệm thu nằm trong **BAO_CAO_NGHIEM_THU_1.0.8.md**.

## Bổ sung 1.0.24: bảng chọn đoạn và I.449

Chọn đoạn / gói cho hồ sơ mở bảng để chọn đúng mã và tên, khoảng lý trình, phía đường và tim. Đối chiếu cột cuối trước khi gán. Gán tay được giữ khi tính lại; Hủy không đổi dữ liệu, Undo khôi phục gán/bỏ gán.

Nếu có Km nhưng chưa có đoạn, kiểm tra Phía đường đã nhập. Phía so với tim rãnh không phải phía đường trong bảng phân chia. OBJ-000058 của DWG đoạn 10 đang lưu PHAI trong khi DOAN_10 yêu cầu TRAI. Có thể chọn DOAN_10 thủ công; chỉ sửa phía đường nếu đối chiếu khảo sát xác nhận nhập sai. TIMRANHD10 hiện lưu loại TIM_DUONG; nếu là tim rãnh, sửa loại và gán tim cho DOAN_10 rồi tính lại.

I.449 → Nội dung biển có hai dòng nhập: dòng trên mặc định ĐƯỜNG, dòng dưới tên đường. Áp dụng rồi Vẽ/cập nhật ký hiệu. Dữ liệu cũ giữ nguyên tên đã nhập; không tự cắt chữ “Đường” trong tên cũ.
