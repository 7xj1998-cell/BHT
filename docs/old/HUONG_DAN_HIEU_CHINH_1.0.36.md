# BHT 1.0.36 - Hiệu chỉnh, hợp nhất và bố trí ký hiệu

Ngày 15/10/2026. Cài BHT-Setup-1.0.36.exe sau khi lưu công việc và đóng AutoCAD. Chỉ test bản sao; bật Undo All. Lệnh chính BHT/BTH giữ nguyên. Bảng hiệu chỉnh và lịch sử tự giãn dòng để tránh cắt chữ.

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
