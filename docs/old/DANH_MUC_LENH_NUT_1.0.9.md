# BHT 1.0.9 — Danh mục lệnh và rà soát nút

Ngày 12/10/2026. Rà mã đăng ký lệnh Lisp/.NET, điểm gắn sự kiện của nút và cây điều khiển của năm tab. Danh mục này không có nghĩa mọi lệnh đã được bấm thử tương tác trên CAD.

## Những phần đã gộp

| Trước đây | Bản 1.0.9 |
| --- | --- |
| Hai nút tính toàn bộ ở trang tuyến | Một nút **Tính lý trình tất cả hồ sơ** ở đầu trang; giữ phương pháp đang chọn |
| Kiểm tra bản vẽ ở Tổng quan và trang tuyến | Chỉ còn ở nhóm Báo cáo của trang tuyến |
| Xuất hồ sơ ở Tổng quan và nhóm Báo cáo | Bỏ nút tắt ở Tổng quan; Excel/CSV ở Báo cáo |
| Mốc đã nhập ở Tổng quan và trang tuyến | Chỉ còn ở đầu trang Tuyến & báo cáo |
| Nhập KMZ ở Tổng quan, hồ sơ và ảnh | Chỉ còn ở Ảnh hiện trường |
| CSV và thống kê ảnh đặt ở Tổng quan | Chuyển CSV về Điểm vị trí; thống kê về Ảnh hiện trường |
| Tên block chung dễ nhầm với gán riêng | Đổi thành **Gán block mặc định cho nhóm…** |
| Nút tính hồ sơ có tên gần giống tính toàn bộ | Đổi thành **Tính lý trình hồ sơ này** |

Tổng quan giữ vai trò điều hướng; nút Tiếp tục dẫn sang đúng tab khi chưa có điểm. Không xóa dữ liệu hoặc thay hàm tính để dọn giao diện.

## Những phần nhìn giống nhưng khác phạm vi

| Cặp thao tác | Lý do giữ |
| --- | --- |
| Tính một hồ sơ / tính tất cả | Khác phạm vi cập nhật |
| Chọn 2 mốc / dùng danh sách mốc | Một bên chỉ định cặp; bên kia dùng danh sách đã kiểm tra |
| Tạo từ RTK / tạo hồ sơ từ điểm | Chọn trên CAD / chọn trong bảng; không đặt lặp cạnh nhau |
| Đặt ký hiệu / vẽ cập nhật | Một bên hỏi vị trí/hướng; bên kia cập nhật theo dữ liệu đã lưu |
| Tỷ lệ RTK / tỷ lệ biển | Khác loại hình và nhãn cần thay cỡ |
| Gán block riêng / mặc định nhóm | Một hồ sơ / quy ước chung của nhóm |
| Nạp Polyline / tuyến TDT | Khác nguồn hình học |
| Xuất Excel / CSV | Báo cáo biển báo / bộ dữ liệu kỹ thuật nhiều nhóm |
| Đề xuất ghép / xác nhận ghép ảnh | Tìm ứng viên / chấp nhận liên kết; không được tự gộp thành xác nhận |
| Thu phóng / nhấp đúp | Hai cách gọi cùng thao tác để tiện sử dụng; không phải hai nút trùng |
| BHT / BTH; phím số / BHTLAY… | Tên tương thích và phương án khi trùng lệnh phần mềm khác |

Lệnh BHTNET… phục vụ tích hợp/chẩn đoán, không đưa thành dãy nút cho người dùng thường. Giữ tương thích trong đợt này; không xóa chỉ vì có chức năng dữ liệu giống một nút trên bảng. Không có định nghĩa BHT cho 44.

## Toàn bộ nút trong năm tab

Có **89 nút** trong cây điều khiển, gồm cả nút ẩn theo nhóm đối tượng hoặc phần nâng cao. Các mục bật/tắt, ô lọc và thao tác nhấp đúp được rà riêng trong bộ thử giao diện. Cột nguồn là vị trí đăng ký để người bảo trì truy vết; đường dẫn tương đối từ mã nguồn.

| Tab | Nút | Nguồn đăng ký |
| --- | --- | --- |
| Tổng quan | Điểm vị trí | TabOverviewPoints.cs:30 |
| Tổng quan | Ảnh hiện trường | TabOverviewPoints.cs:30 |
| Tổng quan | Hồ sơ và ký hiệu | TabOverviewPoints.cs:31 |
| Tổng quan | Tiếp tục | TabOverviewPoints.cs:31 |
| Tổng quan | 3 · Tuyến, lý trình và báo cáo | TabOverviewPoints.cs:32 |
| Tổng quan | Làm mới | TabOverviewPoints.cs:35 |
| Điểm vị trí | Nhập CSV RTK | TabOverviewPoints.cs:156 |
| Điểm vị trí | Thu phóng | TabOverviewPoints.cs:157 |
| Điểm vị trí | Thông tin (BHTINFO) | TabOverviewPoints.cs:158 |
| Điểm vị trí | Ảnh liên quan | TabOverviewPoints.cs:159 |
| Điểm vị trí | Tạo hồ sơ từ điểm | TabOverviewPoints.cs:160 |
| Điểm vị trí | Cập nhật nhãn RTK | TabOverviewPoints.cs:161 |
| Điểm vị trí | Tỷ lệ ký hiệu và nhãn RTK… | TabOverviewPoints.cs:162 |
| Điểm vị trí | Gom / xoay nhãn… | TabOverviewPoints.cs:163 |
| Hồ sơ đối tượng | Chọn loại đèn… | TabObjectsRoutes.cs:154 |
| Hồ sơ đối tượng | Chọn block CAD | TabObjectsRoutes.cs:163 |
| Hồ sơ đối tượng | Nạp DWG… | TabObjectsRoutes.cs:164 |
| Hồ sơ đối tượng | Bỏ gán | TabObjectsRoutes.cs:165 |
| Hồ sơ đối tượng | Gắn điểm có sẵn… | TabObjectsRoutes.cs:184 |
| Hồ sơ đối tượng | Gỡ điểm chọn | TabObjectsRoutes.cs:185 |
| Hồ sơ đối tượng | Xem ở tab Ảnh | TabObjectsRoutes.cs:119 |
| Hồ sơ đối tượng | Lưu lý trình đã nhập | TabObjectsRoutes.cs:126 |
| Hồ sơ đối tượng | Xóa lý trình đã nhập | TabObjectsRoutes.cs:127 |
| Hồ sơ đối tượng | Tính lý trình hồ sơ này | TabObjectsRoutes.cs:128 |
| Hồ sơ đối tượng | Chọn 2 mốc… | TabObjectsRoutes.cs:132 |
| Hồ sơ đối tượng | Dùng mốc trong danh sách… | TabObjectsRoutes.cs:133 |
| Hồ sơ đối tượng | Tạo từ RTK… | TabObjectsRoutes.cs:200 |
| Hồ sơ đối tượng | Thêm cọc… | TabObjectsRoutes.cs:201 |
| Hồ sơ đối tượng | Chọn biển… | TabObjectsRoutes.cs:202 |
| Hồ sơ đối tượng | Lưu hồ sơ | TabObjectsRoutes.cs:203 |
| Hồ sơ đối tượng | Đặt ký hiệu… | TabObjectsRoutes.cs:204 |
| Hồ sơ đối tượng | Vẽ / cập nhật ký hiệu | TabObjectsRoutes.cs:205 |
| Hồ sơ đối tượng | Tỷ lệ biển / nhãn… | TabObjectsRoutes.cs:206 |
| Hồ sơ đối tượng | Xóa hồ sơ… | TabObjectsRoutes.cs:218 |
| Hồ sơ đối tượng | Thông tin hồ sơ | TabObjectsRoutes.cs:221 |
| Ảnh hiện trường | ◀ Trước | TabPhotos.cs:44 |
| Ảnh hiện trường | Sau ▶ | TabPhotos.cs:45 |
| Ảnh hiện trường | Bỏ lọc | TabPhotos.cs:46 |
| Ảnh hiện trường | Nhập KMZ | TabPhotos.cs:47 |
| Ảnh hiện trường | Đề xuất ghép ảnh | TabPhotos.cs:48 |
| Ảnh hiện trường | Chọn thư mục ảnh | TabPhotos.cs:49 |
| Ảnh hiện trường | Gắn ảnh vào hồ sơ | TabPhotos.cs:59 |
| Ảnh hiện trường | Thu phóng vị trí chụp | TabPhotos.cs:61 |
| Ảnh hiện trường | Gỡ ảnh khỏi hồ sơ | TabPhotos.cs:62 |
| Ảnh hiện trường | Mở ảnh gốc | TabPhotos.cs:63 |
| Ảnh hiện trường | Thông tin ảnh | TabPhotos.cs:64 |
| Ảnh hiện trường | Thống kê ảnh | TabPhotos.cs:69 |
| Ảnh hiện trường | Cập nhật ký hiệu ảnh | TabPhotos.cs:70 |
| Tuyến & báo cáo | Mốc lý trình đã nhập… | FieldMarkerControl.cs:154 |
| Tuyến & báo cáo | Tính lý trình tất cả hồ sơ | FieldMarkerControl.cs:155 |
| Tuyến & báo cáo | Xoay các biển vuông góc theo cọc | FieldMarkerControl.cs:156 |
| Tuyến & báo cáo | Nâng cao: nhóm đường / nhánh ▸ | FieldMarkerControl.cs:158 |
| Tuyến & báo cáo | Tạo nhóm… | FieldMarkerControl.cs:162 |
| Tuyến & báo cáo | Sửa nhóm… | FieldMarkerControl.cs:162 |
| Tuyến & báo cáo | Xóa nhóm | FieldMarkerControl.cs:163 |
| Tuyến & báo cáo | Dùng nhóm đang chọn | FieldMarkerControl.cs:164 |
| Tuyến & báo cáo | Gán hồ sơ đang chọn vào nhóm | FieldMarkerControl.cs:165 |
| Tuyến & báo cáo | Chuyển về tính theo tim tuyến | FieldMarkerControl.cs:166 |
| Tuyến & báo cáo | Làm mới tuyến | RouteDataControl.cs:50 |
| Tuyến & báo cáo | Hiển thị / zoom tuyến | RouteDataControl.cs:51 |
| Tuyến & báo cáo | Sửa tuyến… | RouteDataControl.cs:54 |
| Tuyến & báo cáo | Chọn lại tuyến… | RouteDataControl.cs:55 |
| Tuyến & báo cáo | Xóa tuyến… | RouteDataControl.cs:56 |
| Tuyến & báo cáo | Nạp tuyến… | TabObjectsRoutes.cs:970 |
| Tuyến & báo cáo | Chọn điểm đầu và chiều tuyến | TabObjectsRoutes.cs:973 |
| Tuyến & báo cáo | Đảo chiều tuyến | TabObjectsRoutes.cs:974 |
| Tuyến & báo cáo | Đọc / kiểm tra cọc TDT | TabObjectsRoutes.cs:979 |
| Tuyến & báo cáo | Thêm mốc bổ sung / điểm gãy Km | TabObjectsRoutes.cs:982 |
| Tuyến & báo cáo | Xem / xóa mốc trên tim tuyến | TabObjectsRoutes.cs:983 |
| Tuyến & báo cáo | Kiểm tra 2 cọc Km (không có tuyến) | TabObjectsRoutes.cs:984 |
| Tuyến & báo cáo | Kiểm tra tim tuyến | TabObjectsRoutes.cs:988 |
| Tuyến & báo cáo | Xoay biển vuông góc với hướng… | TabObjectsRoutes.cs:993 |
| Tuyến & báo cáo | Sắp xếp nhãn trên bản vẽ | TabObjectsRoutes.cs:994 |
| Tuyến & báo cáo | Xuất báo cáo biển báo Excel | TabObjectsRoutes.cs:998 |
| Tuyến & báo cáo | Kiểm tra bản vẽ | TabObjectsRoutes.cs:1000 |
| Tuyến & báo cáo | Xuất dữ liệu kỹ thuật CSV | TabObjectsRoutes.cs:1002 |
| Tuyến & báo cáo | Xuất dự án BHT sang DWG… | TabObjectsRoutes.cs:1003 |
| Tuyến & báo cáo | Nạp dự án BHT từ DWG… | TabObjectsRoutes.cs:1004 |
| Tuyến & báo cáo | Nạp bảng TSV… | TabObjectsRoutes.cs:1008 |
| Tuyến & báo cáo | Lưu bảng TSV mẫu… | TabObjectsRoutes.cs:1009 |
| Tuyến & báo cáo | Gán đoạn / gói theo lý trình | TabObjectsRoutes.cs:1011 |
| Tuyến & báo cáo | Chọn đoạn / gói cho hồ sơ… | TabObjectsRoutes.cs:1012 |
| Tuyến & báo cáo | Cập nhật mẫu biển đã vẽ | TabObjectsRoutes.cs:1016 |
| Tuyến & báo cáo | Bố trí tất cả báo hiệu… | TabObjectsRoutes.cs:1021 |
| Tuyến & báo cáo | Chọn tất cả báo hiệu trên CAD | TabObjectsRoutes.cs:1023 |
| Tuyến & báo cáo | Chọn báo hiệu cùng nhóm… | TabObjectsRoutes.cs:1024 |
| Tuyến & báo cáo | SELECTSIMILAR theo lớp… | TabObjectsRoutes.cs:1025 |
| Tuyến & báo cáo | Sửa thứ tự hiển thị | TabObjectsRoutes.cs:1027 |
| Tuyến & báo cáo | Gán block mặc định cho nhóm… | TabObjectsRoutes.cs:1028 |

## Hộp thoại và công cụ phụ

| Hộp thoại | Các thao tác đã rà | Mức kiểm tra |
| --- | --- | --- |
| Thư viện biển | Chọn, đóng, trang trước/sau, thêm/bỏ mặt, lên/xuống, sửa nội dung, thêm mẫu tùy chỉnh | Kiểm thử tự động bố cục, chọn và lưu nội dung; dữ liệu mẫu trong Core Console |
| Nội dung biển | Áp dụng, dùng dòng mẫu, đóng; ô số/giờ/kích thước/địa danh; bố trí khung/trụ | Kiểm tra giá trị sai, nhiều mặt, bảo toàn nội dung |
| Danh sách mốc đã nhập | Dùng các mốc, hủy, xác nhận/ngừng dùng mốc thủ công | Giao diện tự động và API trên CAD; xác nhận có điều kiện |
| Hai mốc / nhóm cọc | Thêm, bỏ, lên, xuống, lưu/tính, hủy | Giao diện tự động, kiểm tra thứ tự và dữ liệu đầu vào |
| Thêm cọc | Kiểm tra Km/H, chọn vị trí, hủy | Giao diện tự động; API tạo/hủy/Undo/lưu-mở lại |
| Chọn loại đèn | Chọn mẫu, dùng mẫu, đóng | Giao diện, thư viện DWG và logic đặt đèn tự động |
| Gom / xoay nhãn RTK và Timemark | UCS, A → B, hủy | Đã chạy RTK104 27 ca và LABEL109 30 ca trong 1.0.9, có Undo và lưu/mở lại |
| Khoảng cách và kiểu nối | Thẳng/gấp khúc, áp dụng tất cả, hủy | Giao diện; hình CAD, Undo và lưu/mở lại |
| Tỷ lệ biển và tỷ lệ RTK | Áp dụng, về 1:1, hủy | Dữ liệu hợp lệ/không hợp lệ; giữ nội dung đang sửa |
| Đặt ký hiệu | Chọn chế độ/hướng, đặt, hủy | Giao diện và phép biến đổi hình học trên CAD |
| Báo cáo | Sao chép, đóng; xuống dòng và cuộn | Kiểm thử bố cục/nội dung; chưa nghiệm thu clipboard qua chuột thật |
| Chọn/mở/lưu tệp của Windows | CSV, KMZ, DWG, ảnh, TSV, Excel | Rà điểm gọi; chưa thao tác tay hết các nhánh hệ thống |

Các nút Hủy/Đóng/Áp dụng nằm ở những hộp thoại khác nhau và khác trạng thái dữ liệu. Không xóa các nút này vì cùng nhãn.

## Toàn bộ lệnh đăng ký

Có **85 tên lệnh khác nhau** trong 12 module Lisp và mã .NET sản phẩm; không có tên bị khai báo hai lần trong tập này. Không tính lệnh của bộ thử hoặc API hàm Lisp. Đây là kiểm tra mã nguồn, không phải chứng nhận không xung đột với mọi Lisp được cài trên máy.

| Lệnh | Chức năng | Nguồn |
| --- | --- | --- |
| 11 | Gọi BHTLAYON nếu tên 11 chưa được đăng ký | src/lisp/modules/12-api.lsp:19 |
| 22 | Gọi BHTLAYOFF nếu tên 22 chưa được đăng ký | src/lisp/modules/12-api.lsp:20 |
| 33 | Gọi BHTLAYISO nếu tên 33 chưa được đăng ký | src/lisp/modules/12-api.lsp:21 |
| 55 | Gọi BHTLAYFRZ nếu tên 55 chưa được đăng ký | src/lisp/modules/12-api.lsp:23 |
| 66 | Gọi BHTLAYTHW nếu tên 66 chưa được đăng ký | src/lisp/modules/12-api.lsp:25 |
| BHT | Mở bảng quản lý BHT | src/dotnet/BHT.Palette/PaletteHost.cs:68 |
| BHTANH | Mở ảnh theo mã, có tùy chọn đổi thư mục | src/lisp/modules/05-photos.lsp:791 |
| BHTANHNAP | Nạp bảng BHT_PHOTO.tsv có sẵn | src/lisp/modules/05-photos.lsp:191 |
| BHTANNHAN | Ẩn/hiện nhãn điểm RTK | src/lisp/modules/03-labels.lsp:667 |
| BHTBBDANHMUC | In danh mục mã tham khảo cũ; giữ khả năng tra cứu | src/lisp/modules/08-signs.lsp:276 |
| BHTBLOCK | Tra cứu/nạp block, đổi mẫu mặc định cho nhóm | src/lisp/modules/08-signs.lsp:1655 |
| BHTBOANH | Gỡ liên kết ảnh khỏi hồ sơ | src/lisp/modules/05-photos.lsp:1167 |
| BHTBOTDIEM | Gỡ liên kết điểm khỏi hồ sơ | src/lisp/modules/04-objects.lsp:462 |
| BHTCHENANH | Chèn ảnh được chọn thành raster CAD | src/lisp/modules/05-photos.lsp:899 |
| BHTCHONBAOHIEU | Chọn đủ hình, nhãn, đường nối và dấu gốc BHT; giữ lựa chọn sau khi lệnh kết thúc | src/dotnet/BHT.Bridge/DisplaySelection.cs:50 |
| BHTCHONLAITUYEN | Chọn lại hình học tham chiếu cho tuyến | src/lisp/modules/06-routes.lsp:943 |
| BHTCHONTUONGTU | Chọn trọn thành phần các báo hiệu cùng nhóm với mẫu chọn | src/dotnet/BHT.Bridge/DisplaySelection.cs:52 |
| BHTDATTUDO | Đặt ký hiệu bằng hướng/vị trí chọn trên CAD | src/lisp/modules/08-signs.lsp:975 |
| BHTDIAG | Chẩn đoán dữ liệu và đối tượng/proxy | src/lisp/modules/11-diagnostics.lsp:265 |
| BHTDOCCOCTDT | Đọc/kiểm tra cọc của tuyến TDT | src/lisp/modules/06-routes.lsp:629 |
| BHTDOITUONG | Tạo hồ sơ từ các điểm, kiểm tra dùng chung điểm | src/lisp/modules/04-objects.lsp:392 |
| BHTDONGBOANH | Cập nhật ký hiệu ảnh từ dữ liệu đã lưu | src/lisp/modules/05-photos.lsp:599 |
| BHTDSMOC | Xem/xóa mốc tim tuyến | src/lisp/modules/06-routes.lsp:840 |
| BHTGANANH | Gắn ảnh vào hồ sơ theo chỉ định | src/lisp/modules/05-photos.lsp:1150 |
| BHTGANDOAN | Chọn đoạn/gói cho hồ sơ theo chỉ định | src/lisp/modules/07-segments.lsp:130 |
| BHTGHEPANH | Tạo đề xuất ghép ảnh, chưa xác nhận liên kết | src/lisp/modules/05-photos.lsp:1079 |
| BHTGOITHAU | Nạp TSV đoạn/gói thầu | src/lisp/modules/07-segments.lsp:100 |
| BHTHELP | In danh sách lệnh vào lịch sử CAD | src/lisp/modules/11-diagnostics.lsp:460 |
| BHTHETOADO | Khai báo hệ tọa độ dùng chuyển GPS ảnh | src/lisp/modules/05-photos.lsp:84 |
| BHTHUONGBIEN | Xoay biển vuông góc theo hướng hoặc tim tuyến | src/lisp/modules/08-signs.lsp:1063 |
| BHTINFO | Xem thông tin đối tượng CAD/BHT | src/lisp/modules/11-diagnostics.lsp:241 |
| BHTKHOANGCACH | Bố trí tất cả theo tim, ngoài đường theo PL hoặc từ điểm đo; chọn kiểu nối | src/lisp/modules/12-api.lsp:319 |
| BHTKIEUDIEM | Đặt kiểu/kích thước dấu điểm, tùy chọn sắp nhãn | src/lisp/modules/03-labels.lsp:676 |
| BHTKM2COC | Nội suy kiểm tra một vị trí giữa hai mốc, không tự ghi mọi hồ sơ | src/lisp/modules/06-routes.lsp:1014 |
| BHTKMZ | Nhập KMZ, giải nén ảnh và dữ liệu ảnh | src/lisp/modules/05-photos.lsp:357 |
| BHTKT | Kiểm tra toàn vẹn dữ liệu bản vẽ | src/lisp/modules/11-diagnostics.lsp:175 |
| BHTKYHIEU | Vẽ/cập nhật ký hiệu theo hồ sơ, có tùy chọn trả vị trí tự động | src/lisp/modules/08-signs.lsp:1626 |
| BHTLAYFRZ | Đóng băng lớp bằng LAYFRZ | src/lisp/modules/12-api.lsp:17 |
| BHTLAYISO | Cô lập lớp bằng LAYISO | src/lisp/modules/12-api.lsp:16 |
| BHTLAYOFF | Tắt lớp bằng LAYOFF | src/lisp/modules/12-api.lsp:15 |
| BHTLAYON | Bật lớp bằng LAYON | src/lisp/modules/12-api.lsp:14 |
| BHTLAYTHW | Mở băng lớp bằng LAYTHW | src/lisp/modules/12-api.lsp:18 |
| BHTLOAD | Kiểm tra/nạp bảng, không thay DLL đã nạp | src/lisp/modules/12-api.lsp:361 |
| BHTLYTRINH | Tính toàn bộ hồ sơ theo phương pháp hiện tại | src/lisp/modules/06-routes.lsp:853 |
| BHTMOCKM | Thêm mốc tim tuyến hoặc điểm gãy Km | src/lisp/modules/06-routes.lsp:786 |
| BHTNAPDUAN | Nạp dự án vào DWG chưa có BHT, giữ tọa độ và liên kết | src/dotnet/BHT.Bridge/ProjectTransfer.cs:80 |
| BHTNETBOANH | Gỡ ảnh theo ID qua .NET | src/dotnet/BHT.Bridge/BridgeCommands.cs:164 |
| BHTNETCACHE | Kiểm thử cache bằng điểm tạm; chỉ dùng bản sao | src/dotnet/BHT.Bridge/BridgeCommands.cs:220 |
| BHTNETDIEUPHOI | Kiểm thử trạng thái bận/ngữ cảnh lệnh; dùng trên bản sao | src/dotnet/BHT.Bridge/BridgeCommands.cs:194 |
| BHTNETDUMP | Xuất dữ liệu chẩn đoán từ .NET | src/dotnet/BHT.Bridge/BridgeCommands.cs:76 |
| BHTNETGANANH | Gắn ảnh theo ID qua .NET | src/dotnet/BHT.Bridge/BridgeCommands.cs:156 |
| BHTNETINFO | Thông tin dữ liệu từ bộ máy .NET | src/dotnet/BHT.Bridge/BridgeCommands.cs:61 |
| BHTNETLISP | Gọi API Lisp và ghi kết quả để chẩn đoán | src/dotnet/BHT.Bridge/BridgeCommands.cs:173 |
| BHTNETPING | Kiểm tra đăng ký lệnh .NET, ghi biến USERS5 | src/dotnet/BHT.Bridge/BridgeCommands.cs:261 |
| BHTNETSUA | Sửa trường hồ sơ theo tên trường; dùng hỗ trợ kỹ thuật | src/dotnet/BHT.Bridge/BridgeCommands.cs:139 |
| BHTNETTAO | Tạo hồ sơ bằng lời nhắc .NET; dùng hỗ trợ/tích hợp | src/dotnet/BHT.Bridge/BridgeCommands.cs:118 |
| BHTNETTHEMDIEM | Gắn điểm theo ID qua .NET | src/dotnet/BHT.Bridge/BridgeCommands.cs:148 |
| BHTNHANANH | Ẩn/hiện nhãn mã ảnh | src/lisp/modules/05-photos.lsp:611 |
| BHTNHANDIEM | Chọn nội dung và cách hiển thị nhãn điểm | src/lisp/modules/03-labels.lsp:604 |
| BHTNHANTUDONG | Trả nhãn về cách đặt tự động, nằm ngang WCS | src/lisp/modules/03-labels.lsp:652 |
| BHTNHAP | Nhập CSV điểm RTK | src/lisp/modules/02-points.lsp:310 |
| BHTNHAPTSV | Nhập điểm từ TSV chuẩn hóa; không nhập lặp CSV đã có | src/lisp/modules/02-points.lsp:322 |
| BHTPHANDOAN | Tính lại lý trình và phân đoạn/gói theo dữ liệu đã khai báo | src/lisp/modules/07-segments.lsp:114 |
| BHTROUTEDIAG | Kiểm tra hình học và mốc tim tuyến | src/lisp/modules/06-routes.lsp:780 |
| BHTROUTEREVERSE | Đảo chiều tuyến | src/lisp/modules/06-routes.lsp:734 |
| BHTROUTESTART | Chọn điểm đầu và chiều tăng lý trình | src/lisp/modules/06-routes.lsp:729 |
| BHTSAPNHAN | Sắp nhãn tránh chồng theo phạm vi chọn | src/lisp/modules/03-labels.lsp:643 |
| BHTSELECTSIMILAR | SELECTSIMILAR theo lớp: đặt SELECTSIMILARMODE=2; CAD vẫn phân biệt loại đối tượng | src/dotnet/BHT.Bridge/DisplaySelection.cs:75 |
| BHTSUADT | Mở sửa hồ sơ | src/lisp/modules/04-objects.lsp:428 |
| BHTSUATUYEN | Sửa khai báo tuyến | src/lisp/modules/06-routes.lsp:929 |
| BHTTEST | Tự kiểm tra hàm; dành cho hỗ trợ kỹ thuật | src/lisp/modules/11-diagnostics.lsp:398 |
| BHTTHEMDIEM | Gắn thêm điểm vào hồ sơ | src/lisp/modules/04-objects.lsp:445 |
| BHTTHUMUCANH | Chỉ lại thư mục chứa ảnh gốc | src/lisp/modules/05-photos.lsp:621 |
| BHTTHUTUVE | Sắp thứ tự hiển thị nhãn/ký hiệu/ảnh | src/lisp/modules/09-draworder.lsp:157 |
| BHTTRANGTHAI | Xem tình trạng dữ liệu hiện tại | src/lisp/modules/11-diagnostics.lsp:507 |
| BHTTUYEN | Khai báo tuyến từ Polyline thường | src/lisp/modules/06-routes.lsp:433 |
| BHTTUYENTDT | Khai báo tuyến từ TDT | src/lisp/modules/06-routes.lsp:494 |
| BHTXACNHANANH | Duyệt/xác nhận đề xuất ghép ảnh | src/lisp/modules/05-photos.lsp:1111 |
| BHTXEMANH | Chọn ký hiệu/điểm/mã ảnh để xem và điều hướng | src/lisp/modules/05-photos.lsp:783 |
| BHTXOADT | Xóa hồ sơ sau xác nhận, giữ điểm đo | src/lisp/modules/04-objects.lsp:435 |
| BHTXOATUYEN | Xóa khai báo tuyến | src/lisp/modules/06-routes.lsp:956 |
| BHTXOAYNHAN | Gom / xoay nhãn RTK và Timemark theo UCS hoặc hướng A → B | src/lisp/modules/03-labels.lsp:531 |
| BHTXUAT | Tính lại lý trình và xuất các bảng CSV kỹ thuật | src/lisp/modules/10-reports.lsp:128 |
| BHTXUATDUAN | Xuất dữ liệu BHT và hình liên quan thành DWG mới | src/dotnet/BHT.Bridge/ProjectTransfer.cs:79 |
| BTH | Tên gọi tương đương BHT, giữ để tương thích | src/dotnet/BHT.Palette/PaletteHost.cs:65 |

## Trạng thái nghiệm thu

Bố cục và luồng giao diện: 2.493 kiểm tra tự động. Các phần mới dùng bộ thử chuyên biệt; kết quả rộng hơn của các bản trước chỉ là bằng chứng lịch sử; xem **BAO_CAO_NGHIEM_THU_1.0.9.md** để biết bằng chứng và giới hạn.

Chưa nghiệm thu đầy đủ từng nút bằng chuột trên phiên AutoCAD có TDT, các hộp chọn tệp của Windows và các lệnh chẩn đoán kỹ thuật theo mọi nhánh. Không có yêu cầu cài đè hoặc đóng bản vẽ gốc đang có thay đổi để hoàn thành kiểm thử này.


## Bố trí hàng loạt trong 1.0.9

Nút **Bố trí tất cả báo hiệu…** và **BHTKHOANGCACH** dùng cùng bộ xử lý. Một hộp thoại có ba cách bố trí; không thêm nút trùng chức năng. Tim đã nạp dùng chiều tuyến lưu trong hồ sơ; PL chọn trực tiếp yêu cầu chọn đầu để xác định phía. Chỉ dịch hình và nhãn, giữ điểm đo/lý trình. Khoảng cách tới tim/mép tính đến vùng bao hình và nhãn. Xem hướng dẫn để hiểu trường hợp bị bỏ qua.

Bằng chứng mới: 34 ca bố trí, 20 ca kiểu nối, 2.615 kiểm tra giao diện và bản sao 120 ký hiệu. Các kết quả kiểm thử của chức năng khác trong bảng ở trên là bằng chứng lịch sử; không có nghĩa toàn bộ thao tác CAD/TDT đã được chạy lại trong 1.0.9.

## Sửa lựa chọn tim trong 1.0.9

Bố trí tất cả báo hiệu đã nhận tuyến nạp loại Khác, Tim rãnh hoặc chưa phân loại khi người dùng chọn rõ làm tim bố trí; giữ phân loại và dữ liệu lý trình. Bảng tuyến có cột Loại, danh sách chọn hiển thị ID và loại. Không thêm lệnh hoặc nút trùng.

## Nhãn biển hai dòng trong 1.0.9

Tên biển ở dòng trên, lý trình ở dòng dưới. Cập nhật mẫu biển đã vẽ áp dụng cho biển/bảng, giữ block đèn; đèn được cập nhật trong hồ sơ riêng. Không thêm nút hoặc lệnh trùng.
