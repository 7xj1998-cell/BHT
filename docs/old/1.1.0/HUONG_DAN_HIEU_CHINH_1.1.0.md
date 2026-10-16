# BHT 1.1.0 - Hướng dẫn tính năng mới và quản lý hồ sơ

Bản kiểm tra cục bộ cho AutoCAD 2024. Bạn dùng bản sao trong D:/lisp/BHT/Test-1.1.0; giữ New làm nguồn đối chiếu. Đọc HUONG_DAN_TEST_TOAN_BO.md để ghi kết quả từng bài.

## Cài và mở bản thử

1. Lưu công việc và đóng toàn bộ AutoCAD. Mở BHT-Setup-1.1.0.exe, bấm Cài đặt BHT. Bộ cài tự kiểm tra tệp, giữ backup bản cũ và từ chối cài khi CAD còn chạy.
2. Mở Test-1.1.0/D13-Test.dwg, gõ BHT hoặc BTH. Kiểm tra Palette và Lisp cùng 1.1.0. Chỉ chạy một phiên AutoCAD khi thử.
3. SAVEAS thành D13-BEFORE.dwg, rồi D13-Working.dwg. Xuất Excel BEFORE để đối chiếu hồ sơ, RTK, lý trình, offset và phía đường.
4. Nếu gặp “License checkout timed out”, dừng bài GUI và xử lý đăng nhập/giấy phép Autodesk qua công cụ hỗ trợ chính thức. Bộ cài BHT không sửa giấy phép; lỗi này không chứng minh lỗi thuật toán BHT.

## Báo cáo đủ dòng, vừa xem vừa kiểm tra CAD

Mở báo cáo kiểm tra cọc hoặc Xem thông báo gần nhất trong Palette. Cửa sổ có tên bản vẽ nguồn. Kéo thanh tiêu đề sang bên, kéo góc để phóng to, dùng Chữ + / Chữ - và bật/tắt Xuống dòng. Khi tắt xuống dòng, cuộn ngang để đọc cột dài. Sao chép báo cáo lấy cả phần ngoài màn hình.

Báo cáo không chặn CAD: bạn có thể chuyển vào vùng vẽ để zoom/pan rồi quay lại đọc. Bảng chỉ là kết quả tại lúc chạy; nếu sửa dữ liệu, hãy chạy báo cáo lại. Tối đa 20.000 thông báo hoặc khoảng 8 triệu ký tự trong bộ đệm; vượt giới hạn có cảnh báo rõ và cần xem F2, không âm thầm bỏ 40 dòng đầu như trước.

Trong Quản lý xử lý theo đoạn, bấm Tính lý trình theo đoạn. Bảng xem trước cũng cho phép kiểm tra CAD trước khi bấm Áp dụng phương án. Nếu đổi hồ sơ/tim/mốc hoặc chuyển bản vẽ, BHT từ chối áp dụng sai ngữ cảnh. Đóng bảng không ghi dữ liệu; muốn thử lại sau khi áp dụng hoặc bị từ chối, lập phương án mới. Bảng chọn đoạn ban đầu và các hộp cần xác nhận vẫn chặn CAD trong lúc lựa chọn.

## Chọn điểm đầu tuyến

Dùng bắt điểm trên chính tim đã chọn. Điểm cách tim trên 1 m bị từ chối, dữ liệu tuyến giữ nguyên. Chọn cuối tuyến phải chọn chiều đi vào tuyến; chiều đi ra ngoài không còn phần tuyến hữu ích sẽ bị chặn. Chọn giữa tuyến có cảnh báo; chỉ tiếp tục nếu đó là phạm vi cần xử lý.

Khi tuyến đã có mốc khoảng cách, không đổi điểm đầu/chiều bằng thao tác này. Cần đối chiếu và khai báo lại mốc trong bản làm việc có kiểm soát. Không xóa mốc hoặc mở khóa cả đoạn chỉ để vượt cảnh báo. BHT không thể nhận biết ý định nếu điểm được chọn nằm hợp lệ trên một tuyến khác hoặc chiều khai báo chưa khớp hồ sơ thực địa; vẫn phải đối chiếu tên tuyến, mốc Km và chiều khảo sát.

## Hướng nhãn theo chiều tăng lý trình

Quy ước đọc của bản 1.1: hình dung xoay tờ bình đồ sao cho chiều tăng Km hướng lên trên. Chữ được chọn một trong hai góc cách nhau 180 độ để đọc trong khung đó. Trong Tỷ lệ biển / nhãn, Song song hoặc Vuông góc vẫn xác định trục chữ so với đường nối. Chọn hướng đọc không xoay hình biển hay sửa RTK, lý trình, offset và phía đường.

BHT dùng tim gắn với hồ sơ, hình học đã đối chiếu, mốc hiệu lực và tiếp tuyến tại điểm khảo sát; biển nhiều trụ dùng điểm tính lý trình tương ứng. Khi thiếu tim/mốc, hệ số bằng 0 hoặc ngay tại mốc mà hai chiều tăng Km đối nghịch, nhãn cũ giữ vị trí/góc và có cảnh báo. Nhãn mới chưa đủ dữ liệu chỉ là bố trí tạm, cần kiểm tra trước khi bàn giao.

Nhãn đã kéo tay giữ vị trí; nhãn xoay tay được nhận biết nếu đã có dấu góc do 1.1 ghi. Hồ sơ cũ trước 1.1 không lưu góc tham chiếu, nên không thể luôn phân biệt một lần ROTATE tay trước nâng cấp. Với các nhãn cũ quan trọng, lưu bản BEFORE và so sánh lần cập nhật đầu tiên.

Muốn chủ động đưa cả cụm mã, biển phụ, lý trình về mẫu: Tuyến -> 5. Trình bày bản vẽ -> Áp dụng mẫu nhãn báo hiệu (BHTMAUNHAN), chọn đúng cụm cần sửa. Enter ngay khi không chọn áp dụng mọi biển đã vẽ; cần đọc dòng nhắc trước khi tiếp tục. Thao tác này được thiết kế để đặt lại cả nhãn chỉnh tay, giữ hình biển và dữ liệu đo.

## Bố trí D13 và thao tác hàng loạt

Trong bố trí báo hiệu, chọn theo tim DOAN_13, khoảng cách tối thiểu 10 m, đường nối gấp khúc. Bản mẫu có ba biển OBJ-000087, OBJ-000088, OBJ-000090 và ba cọc H OBJ-000089, OBJ-000098, OBJ-000175 thuộc tuyến này. Kiểm tra biển phụ S.509a@5 có nội dung thực tế 4,75 m và bảng quảng cáo hai trụ; không thay nội dung để dễ bố trí.

Khoảng cách là mức tối thiểu: biển đã nằm đủ xa được giữ tại chỗ, chạy lần hai không kéo vào gần tim. Với tim cong/gấp khúc, BHT dùng bao hình bảo thủ nên có thể đặt xa hơn con số nhập. Không phải thuật toán tránh mọi va chạm giữa tất cả ký hiệu. Các cọc cùng tuyến được giữ ở điểm đo và xoay theo tim; cọc ngoài phạm vi tuyến không được cập nhật theo tim D13.

Cập nhật ký hiệu và đổi tỷ lệ vẫn chạy trong luồng lệnh CAD để giữ giao dịch an toàn. Bản này giảm số lần quét thực thể; chưa đưa ghi Database sang luồng nền hoặc tách DLL chỉ để giảm lag. Chờ lệnh kết thúc trước khi thao tác tiếp. Sau MOVE/ROTATE hoặc bố trí hàng loạt trên hồ sơ có revision, dùng Lưu thay đổi trình bày GLOBAL hoặc LOCAL trước khi đồng bộ.

Nếu không hội tụ, thông báo phải có mã hồ sơ và nguyên nhân. BHT đối chiếu phục hồi OBJ, cấu hình và thực thể trình bày. Nếu báo phục hồi chưa đầy đủ, không SAVE đè; mở bản BEFORE. Kiểm tra U/REDO trên bản thử. Chưa chứng minh phục hồi sau mất điện hoặc mọi thay đổi định nghĩa block trong một lỗi native.

## Hiệu chỉnh, phiên bản, GLOBAL/LOCAL và phát hành

Các thao tác kế thừa từ 1.0.36 được hướng dẫn tiếp dưới đây. Giới hạn của milestone vẫn áp dụng; 1.1 không mở đường ghi vượt bảo vệ hồ sơ đã xác nhận.

### Các thao tác quản lý hồ sơ

Ngày 15/10/2026. Cài BHT-Setup-1.1.0.exe sau khi lưu công việc và đóng AutoCAD. Chỉ test bản sao; bật Undo All. Lệnh chính BHT/BTH giữ nguyên. Bảng hiệu chỉnh và lịch sử tự giãn dòng để tránh cắt chữ.

## Hiệu chỉnh khảo sát

Hồ sơ đối tượng -> chọn OBJ khóa -> Hiệu chỉnh hồ sơ đã xác nhận. Chọn thuộc tính, liên kết RTK, tính lý trình riêng hoặc phân đoạn. Nhập lý do/người sửa, xem trước Trước/Sau. Duyệt baseline nếu hồ sơ cũ; duyệt riêng nếu chuyển đoạn; xác nhận. Chỉ OBJ đang chọn đổi revision, giữ khóa. Mã biển đơn đồng bộ mặt; đổi điểm tính lại lý trình/offset/phía theo tim đoạn. Dùng Xem lịch sử để tra dữ liệu cũ. Ngừng sử dụng giữ RTK/lịch sử, bỏ ký hiệu và loại khỏi báo cáo hiện hành.

Chưa đổi mã biển nhiều mặt/nội dung tùy chỉnh, chưa sửa mốc Km/H có ảnh hưởng tuyến; hồ sơ thiếu bằng chứng bị chặn. Không dùng mở khóa cả đoạn để vượt chốt này.

## Bố trí GLOBAL

1. Ở bản vẽ A, chọn OBJ khóa và lưu bố trí ban đầu: Hiệu chỉnh -> Lưu thay đổi trình bày GLOBAL -> nhập lý do -> Xem trước -> duyệt baseline nếu có -> Xác nhận.
2. Xuất gói A_R01.bht và nạp đầy đủ vào DWG trống B. Hai bản có cùng danh tính và revision.
3. Tại A, dùng MOVE/ROTATE, sửa nhãn/đường dẫn. Kết thúc lệnh CAD rồi lưu GLOBAL như bước 1. Bảng trước/sau hiển thị bố trí; RTK và lý trình không đổi, chỉ Display Revision tăng.
4. Xuất A_R02.bht tên mới. Tại B, nạp tích lũy A_R02.bht. Kiểm tra đúng vị trí/góc/tỷ lệ, nhãn và đường dẫn. Nạp lại phải không tạo trùng.
5. Nếu B có chỉnh tay chưa lưu, BHT báo xung đột và giữ B. Xem lại nguồn và bố trí trước khi thử tiếp; không chọn bản theo thời gian.

## Bố trí LOCAL

Ở B đã nhận GLOBAL, chỉnh ký hiệu rồi chọn Bố trí GLOBAL / LOCAL -> nhập lý do -> Lưu bố trí hiện tại thành LOCAL. Không tăng Survey Revision hoặc revision chung; lịch sử riêng lưu trong DWG. Lưu DWG để giữ bố trí.

Khi nhận gói GLOBAL mới, B nhận revision chung nhưng giữ ký hiệu/nhãn/đường dẫn LOCAL. Nếu nguồn sửa khảo sát/phân đoạn ảnh hưởng hình, BHT chặn để đối chiếu. Chuyển về GLOBAL chỉ được khi hình đã khớp GLOBAL lưu trong hồ sơ; chưa tự kéo hình về GLOBAL. Bản LOCAL chưa được xuất thành gói chung, tránh phát tán vị trí riêng. Dùng bản GLOBAL làm nguồn .bht.

## Hợp nhất và phục hồi

Cùng revision bỏ qua; nguồn cũ giữ đích; nguồn kế thừa được nâng cấp; hai nhánh hoặc project/GUID khác bị chặn. Legacy không tự xem là cùng đối tượng. Không đổi tên gói trên lịch sử bằng sửa tay XRecord.

Sau mỗi bài, kiểm tra lịch sử, Excel, số block/nhãn, U/REDO, lưu/mở lại DWG và .bht. Lỗi phục hồi chưa xác minh: không lưu, mở bản BEFORE. Hướng dẫn A-O và phiếu kết quả ở HUONG_DAN_TEST_TOAN_BO.md. Core Console không thay thế nghiệm thu trên giao diện AutoCAD và DPI màn hình của bạn.


## Quản lý bình đồ và phát hành

1. Mở tab Tuyến -> QUẢN LÝ XỬ LÝ THEO ĐOẠN -> Quản lý bình đồ / đồng bộ / phát hành. Đăng ký tên bình đồ sau khi đã có baseline dự án, rồi lưu DWG.
2. Muốn có bình đồ B cùng dự án: nạp .bht đầy đủ vào DWG trống, đăng ký tên B, lưu DWG. Ở A, chọn Liên kết bình đồ cùng dự án và trỏ tới B. Tệp B chỉ được đọc.
3. Kiểm tra bình đồ liên quan hiển thị UP_TO_DATE, UPDATE_AVAILABLE, CONFLICT, RELEASED hoặc READ_ONLY_ARCHIVE. Đây là trạng thái của tệp đã lưu; đóng/mở Palette không tự sửa tệp.
4. Xuất gói từ A. Mở B -> Xem trước gói cập nhật -> chọn gói -> đọc danh sách OBJ và quyết định -> Đồng bộ gói đã xem trước -> xác nhận. Nếu nguồn/đích đã đổi sau preview, phải xem trước lại. Lưu DWG sau khi đối chiếu.
5. Danh sách đoạn -> chọn đoạn đã xác nhận -> Phát hành đoạn R01/R02 -> chọn thư mục -> xác nhận. Tệp mới có tên DOAN_xx_R01.bht; nếu đã có R01 dùng R02. Bình đồ chuyển RELEASED; lưu DWG để giữ trạng thái.
6. Muốn sửa tiếp, chọn Tạo bản DWG làm việc mới, đặt tên chưa tồn tại, mở tệp mới rồi làm việc. Bản lịch sử giữ nguyên; không đổi trạng thái bằng sửa XRecord. Đánh dấu lưu trữ tạo READ_ONLY_ARCHIVE cho bản đang làm việc đã đăng ký.

Không dùng lệnh CAD gốc để vượt bảo vệ BHT. Nếu phần mềm bị ngắt lúc phát hành và thấy tệp Rxx nhưng chưa có lịch sử trong DWG, giữ tệp đó để đối chiếu; không ghi đè hoặc tự xem là phát hành đã hoàn tất.
