# BHT 1.1.3 — Xử lý bảng 30 dòng thành 15 đoạn

Ngày 16/10/2026, AutoCAD 2024. Bản này thêm đối chiếu và gom đoạn trùng, giữ nguyên hồ sơ đã xác nhận. Chỉ thao tác trên bản sao.

## Vì sao xuất hiện 30 dòng?

Đây là **30 dòng phân đoạn**, không phải 30 gói thầu. Công trình có 15 đoạn và 3 gói theo tên. Hai cách viết `DOAN08/GOI03` và `DOAN_08/GOI_03` đã được nạp như hai bộ mã khác nhau. Đã tái hiện bằng bản 1.1.2: nạp DOAN10 vào DOAN9 làm bảng tăng từ 15 lên 30 dòng.

DOAN15.bht có 179 hồ sơ, 801 điểm RTK, 108 hồ sơ khóa. Riêng **DOAN08/GOI03 đang được 15 hồ sơ khóa sử dụng**, gắn tim DOAN_08. Không xóa toàn bộ các dòng mã cũ hoặc mở khóa hàng loạt để làm bảng ngắn lại.

## Dùng bản sao đã xử lý

1. Lưu công việc và đóng AutoCAD trước khi cài BHT-Setup-1.1.3.exe. Mở lại, gõ BHT hoặc BTH và kiểm tra phiên bản 1.1.3.
2. Mở `D:\lisp\BHT\Test-1.1.3\DOAN15-15doan.dwg`. Đây là bản sao snapshot trong gói DOAN15, không phải thay thế bình đồ đoạn 15 đang làm.
3. Mở bảng đoạn/gói và kiểm tra có **15 dòng**; DOAN08 vẫn gắn tim DOAN_08, các đoạn 9–15 giữ mã có dấu gạch dưới.
4. Đối chiếu 179 hồ sơ, 801 điểm và 108 hồ sơ khóa với bản gốc. Kiểm tra các biển thuộc DOAN08, lý trình và ký hiệu trước khi sử dụng tiếp.
5. File `DOAN15-15doan.bht` trong cùng thư mục là gói riêng đã xử lý. Nạp vào DWG thử trống để kiểm tra, rồi nạp lại cùng gói: không được nhân bản hồ sơ, block, nhãn hoặc nhật ký.

Nếu nạp vào bản vẽ đang có dữ liệu khác cùng mã OBJ, BHT vẫn có thể chặn xung đột. File đã gom đoạn không giải quyết các hồ sơ khác nhau trùng mã. Giữ bản vẽ nguồn và file DOAN15.bht gốc trong New.

## Gom đoạn trùng ngay trên DWG làm việc

1. Save As bản vẽ thành tên mới, ví dụ D15-Truoc-Gom.dwg.
2. Trong Palette BHT, mở **Tuyến & báo cáo → Đoạn / gói thầu và tim tham chiếu**; hoặc dùng lệnh có sẵn **BHTTIMDOAN**.
3. Chưa sửa ô nào trong bảng. Bấm **Kiểm tra / gom đoạn trùng…**. Nếu đã sửa ô, hủy bảng rồi mở lại; thao tác gom chỉ dùng dữ liệu đã áp dụng trong DWG.
4. Đọc số dòng có thể gom và số cặp cần đối chiếu riêng. Cuộn để xem dòng giữ, dòng gom, mã gói, phạm vi, phía và tim.
5. Với DOAN15 đã gửi: phải có **15 dòng có thể gom, 0 cặp bị chặn**. Bấm **Xác nhận gom 15 dòng**. Bảng mở lại với 15 dòng.
6. Đóng bảng bằng Hủy nếu không cần sửa thêm. Gom đã thực hiện sau nút xác nhận riêng; Hủy bảng không hoàn tác việc gom. Không cần bấm Áp dụng vào bản vẽ lần nữa.
7. Kiểm tra rồi lưu thành DWG mới. Chỉ xuất gói .bht mới sau khi đối chiếu xong; không ghi đè file đã phát hành.

![Bảng đối chiếu DOAN15 trong kiểm thử WinForms](images/1.1.3/DOAN15-review.png)

Ảnh là kết quả dựng form trong kiểm thử, không phải ảnh chụp thao tác chuột trong AutoCAD.

## Điều kiện an toàn và cách phục hồi

Chỉ gom cặp khác dấu gạch dưới khi cùng phạm vi, phía, cách viết mã gói và tên gói. Dòng bị bỏ phải không được bản ghi BHT tham chiếu và không làm mất liên kết tim. Giữ nguyên mọi OBJ, RTK, ROUTE, lịch sử khảo sát và thực thể CAD. Bản ghi SEG bị bỏ và bản ghi giữ được lưu đầy đủ trong nhật ký SEG_ALIAS bên trong DWG.

Nếu cả hai mã đang được dùng, khác tim, khác nghĩa hoặc có lịch sử phiên bản/phát hành cần đối chiếu, BHT **giữ nguyên toàn bộ bảng**. Không gom một phần khi còn cặp bị chặn. Trong bộ file đã kiểm tra, DOAN11/12/13 còn danh sách ứng viên tham chiếu cả hai mã nên bị chặn; không dùng kết quả DOAN15 để kết luận mọi file cũ đều gom được.

Muốn hoàn tác ngay: đóng bảng, dùng **U / Undo** trước khi thực hiện thao tác khác. Đã kiểm tra một Undo phục hồi SEG và nhật ký. Nếu đã lưu/đóng hoặc có các thay đổi khác, mở bản DWG sao lưu trước gom. Không dùng bản BHT cũ để sửa rồi ghi tiếp lên dữ liệu mới; bộ cài cũ nằm trong old.

## Mã gói sau khi gom

DOAN08 tiếp tục dùng GOI03 vì 15 hồ sơ khóa đang tham chiếu. Các đoạn khác có thể dùng GOI_03. Bảng còn đúng 15 đoạn nhưng **chưa thống nhất toàn bộ cách viết mã gói**; đây vẫn là ba gói theo tên, có mã cũ và mới cùng tồn tại. Không tự đổi GOI03 thành GOI_03 chỉ trong một ô SEG vì OBJ khóa phải khớp gói. Chuẩn hóa toàn bộ mã cần đợt chuyển đổi có duyệt và kiểm tra mọi tham chiếu riêng.

## Kiểm tra sau xử lý

| Bài | Thao tác | Kết quả cần thấy |
|---|---|---|
| 1 | Mở bản sao DOAN15 gốc, kiểm tra trùng rồi Hủy | Vẫn 30 dòng; chưa sửa dữ liệu |
| 2 | Xác nhận gom | Còn 15 dòng; DOAN08 và 15 hồ sơ khóa giữ nguyên |
| 3 | Đóng bảng rồi Undo | Trở lại 30 dòng và trạng thái trước gom |
| 4 | Gom lại, lưu tên mới, đóng/mở DWG | Còn 15 dòng, 179 hồ sơ/801 điểm, kết quả khóa giữ nguyên |
| 5 | Kiểm tra trùng lần nữa | Không có thay đổi, không ghi thêm nhật ký |
| 6 | Nạp gói 30 dòng cũ vào bản đã gom | Chặn mã đoạn khác cách viết, không trở lại 30 dòng |
| 7 | Nạp gói đã gom vào DWG trống rồi nạp lại | 15 dòng, không nhân bản dữ liệu |
| 8 | File có cả hai mã được tham chiếu | Báo cần đối chiếu, không xóa dòng nào |

Giữ ảnh bảng, thông báo và đường dẫn DWG nếu kết quả khác bảng trên. Các bài tự động đã kiểm tra dữ liệu và giao dịch; thao tác thực tế trong phiên AutoCAD của người dùng vẫn cần đối chiếu. Lỗi đứng khi chọn tệp và 11 lỗi signs cũ chưa được kết luận đã sửa trong bản này.

Các chức năng trước xem [hướng dẫn 1.1.2 lưu trữ](old/1.1.2/HUONG_DAN_HIEU_CHINH_1.1.2.md) và [bài kiểm tra toàn bộ](HUONG_DAN_TEST_TOAN_BO.md).
