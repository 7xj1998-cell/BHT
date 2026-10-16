# BHT 1.1.5 - Layer theo tên tuyến

Ngày 16/10/2026. Dành cho AutoCAD 2024. Bản này thay thế 1.1.4 để kiểm tra cục bộ và giữ các sửa lỗi phạm vi lý trình của 1.1.4.

## 1. Điều gì thay đổi?

Tim tham chiếu của BHT được đặt trên layer `BHT_TUYEN_<mã tuyến>`. Ví dụ tuyến `DOAN_11` dùng `BHT_TUYEN_DOAN_11`, tuyến `DOAN_15` dùng `BHT_TUYEN_DOAN_15`.

Trước đây nhiều tim cùng dùng `BHT_TUYEN_TDT`. AutoCAD thêm `$0$` khi sao chép layer trùng tên; nạp qua nhiều đoạn làm tên dài dần. BHT 1.1.5 đưa tim tham chiếu vào layer mang mã tuyến cuối cùng sau khi xử lý trùng mã. Không lấy mã gói thay cho tên tuyến.

Phạm vi thay đổi là layer của tim tham chiếu BHT. Layer nền, điểm RTK, block biển, nhãn và tài nguyên TDT gốc được giữ nguyên. Các layer cũ có thể còn hiện trong Layer Manager; phần mềm không tự xóa vì có thể chứa thực thể khác. Không dùng thay thế hàng loạt chuỗi `$0$` trong toàn bản vẽ.

## 2. Sửa layer trong bản vẽ đã nạp

1. Lưu bản vẽ bằng **SAVEAS** sang tên mới để kiểm tra.
2. Cài bản 1.1.5 khi AutoCAD đã đóng. Mở lại bản sao, gõ **BHT** hoặc **BTH** và kiểm tra phiên bản trên Palette.
3. Vào tab **Tuyến & báo cáo**, tìm phần **TIM ĐANG CHỌN**.
4. Bấm **Đặt layer theo tên tuyến…**. Nút này xử lý tất cả tim BHT trong bản vẽ, không chỉ dòng đang chọn.
5. Đọc danh sách tên tuyến, layer hiện tại và layer đề nghị. Bấm **Cancel** nếu chưa đúng; bấm **OK** để áp dụng.
6. Chọn một tim, mở Properties và kiểm tra Layer. Tắt/bật layer của tuyến đó để kiểm tra đúng tim, sau đó bật lại.
7. Kiểm tra số hồ sơ, RTK, lý trình và trạng thái khóa trước/sau. Lưu bản sao rồi mở lại để kiểm tra.

Đã đối chiếu trên bản sao DOAN15.bht: 9 tim, gồm DOAN_08, DOAN_09_GOI1, DOAN_10 đến DOAN_15 và TUYEN1_GOI1. BHT giữ nguyên các mã tuyến đang có; không tự suy đoán TUYEN1_GOI1 thuộc đoạn nào để đổi mã. Các tuyến có hậu tố GOI1 dùng đúng mã đó trong tên layer.

Tạo tuyến, đổi tên tuyến hoặc nạp hồ sơ từ bản 1.1.5 sẽ đặt layer theo quy tắc mới. Chỉ mở Palette hoặc mở DWG không tự chuyển layer cũ. Nạp lại gói đã nạp có thể không phát sinh thay đổi; dùng nút trên để chuyển các tim cũ đã có trong bản vẽ.

## 3. Khi phần mềm chặn đổi layer

| Thông báo | Cách xử lý |
| --- | --- |
| Layer tim đang khóa | Kiểm tra đúng layer tim trong Layer Manager. Chỉ mở khóa layer trình bày khi được phép; không mở khóa hồ sơ khảo sát. |
| Layer đích có thuộc tính khác | Đối chiếu màu, nét, độ dày, độ trong suốt, thiết lập in và trạng thái bật/tắt/đóng băng. Không đổi thuộc tính của layer đang chứa dữ liệu khác chỉ để vượt lỗi. |
| Có thiết lập riêng trong viewport | Kiểm tra bố cục in và thiết lập của từng khung nhìn; bản này giữ nguyên để tránh làm đổi bản in. |
| Nhiều tuyến cùng tham chiếu một đường | Đối chiếu liên kết tuyến trước. Một đường không thể đồng thời ở hai layer riêng. |
| Thiếu tim hoặc tim nằm trong block/xref | Khôi phục đúng liên kết hoặc tim tham chiếu thường bằng quy trình nạp tuyến hiện có. Không phá block/xref hoặc TDT gốc để ép đổi layer. |
| Bản vẽ đã phát hành/lưu trữ | Dùng bản làm việc theo quy trình quản lý bình đồ; không sửa bản đã bàn giao. |

Nếu có lỗi, việc áp dụng không được ghi một phần các tuyến. Không tạo layer mới với dấu hoặc hậu tố tùy ý để né xung đột thuộc tính.

## 4. Bài kiểm tra nhanh

| Thử trên bản sao | Kết quả cần thấy |
| --- | --- |
| Chuyển 9 tim của DOAN15 | Mỗi tim ở layer BHT_TUYEN_ kèm đúng mã tuyến |
| Bấm áp dụng lần hai | Báo 0 tim thay đổi, không nhân bản tim |
| Tạo tuyến TEST_LAYER | Tim thuộc BHT_TUYEN_TEST_LAYER |
| Đổi mã tuyến chưa khóa | Layer theo mã mới; hồ sơ tham chiếu theo quy trình đổi tên hiện có |
| Nạp lại cùng gói | Không tăng hồ sơ, điểm hoặc nhãn |
| Tạo layer đích khác màu trước khi nạp | Báo xung đột và giữ trạng thái trước nhập |
| Undo ngay sau chuyển layer | Tim trở lại layer trước đó |
| Lưu và mở lại bản sao | Layer mới được giữ, RTK/lý trình/khóa không đổi |

Không đổi tên tuyến đã khóa chỉ để sửa tên layer. Nút chuyển layer hoạt động riêng và giữ hồ sơ đã xác nhận.

## 5. Phục hồi và giới hạn

Bấm Cancel trước áp dụng để không ghi. Sau áp dụng, dùng **U** ngay hoặc mở lại bản sao trước thao tác. Không nạp .bht cũ để xóa lịch sử. Bộ cài cũ được giữ trong `old/before-1.1.5`; đóng CAD trước khi đổi phiên bản phần mềm.

Đã chạy AutoCAD Core Console với FAS/DLL bảo vệ: 4 kiểm tra Lisp, 18 kiểm tra layer/dữ liệu, 1 Undo; hồi quy nạp kế thừa và hiệu chỉnh đạt. File DOAN15.bht nguồn giữ nguyên SHA256. Chưa nghiệm thu thao tác chuột trong phiên AutoCAD của người dùng. 11 lỗi kiểm thử ký hiệu cũ chưa được xử lý trong đợt sửa layer này. Không coi đây là nghiệm thu toàn bộ các milestone.

Hướng dẫn xử lý lý trình/hiệu chỉnh của bản trước: [1.1.4](old/1.1.4/HUONG_DAN_HIEU_CHINH_1.1.4.md). Bài test toàn bộ: [HUONG_DAN_TEST_TOAN_BO](HUONG_DAN_TEST_TOAN_BO.md).
