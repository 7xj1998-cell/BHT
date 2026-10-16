# BHT 1.0.35 - Hiệu chỉnh, hợp nhất và bố trí ký hiệu

Ngày 15/10/2026. Cài BHT-Setup-1.0.35.exe sau khi lưu công việc và đóng AutoCAD. Chỉ test bản sao; bật Undo All. Lệnh chính BHT/BTH giữ nguyên. Bảng hiệu chỉnh và lịch sử tự giãn dòng để tránh cắt chữ.

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
