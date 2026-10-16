# BHT 1.0.2 — Bố cục bảng và phím tắt lớp

Bản sửa ngày 11/10/2026, tiếp nối 1.0.1. Giữ nền xanh và các chức năng hồ sơ hiện có.

- Đã chia trang Tuyến & báo cáo thành các nhóm rõ ràng, dùng viền nút nhẹ và làm nổi bật thao tác chính.
- Bảng tuyến co theo chiều rộng, các nút xuống hàng khi cần; danh sách tuyến giữ chiều cao để không đè các hàng phía dưới.
- Ô chọn nguồn tuyến không đổi lựa chọn khi lăn chuột. Vẫn chọn bằng chuột hoặc bàn phím.
- Nhãn ô nhập chuyển lên trên khi bảng hồ sơ hẹp. Cụm lý trình có ô nhập và năm nút riêng, không ép chữ vào ba cột nhỏ.
- Đã căn trái các nút chức năng ở trang tuyến, gồm Lưu bảng TSV mẫu và Cập nhật mẫu biển đã vẽ.
- Đã bỏ dải trạng thái dưới cùng. Thông báo vẫn ghi vào lịch sử lệnh CAD; lỗi và cảnh báo vẫn có hộp thoại. Nhấn F2 để xem lại.

## Lệnh lớp

| Lệnh | Chức năng BHT |
| --- | --- |
| 11 | LAYON — bật lớp |
| 22 | LAYOFF — tắt lớp được chọn |
| 33 | LAYISO — cô lập lớp |
| 44 | Giữ nguyên lệnh TDT; BHT không đăng ký |
| 55 | LAYFRZ — đóng băng lớp được chọn |
| 66 | LAYTHW — mở băng các lớp |

BHT chỉ đăng ký phím tắt chưa có lúc nạp. Nếu 55 hoặc 66 thuộc TDT/Lisp khác, dùng BHTLAYFRZ hoặc BHTLAYTHW. LAYTHW không khôi phục trạng thái trước LAYISO; thao tác đó dùng LAYUNISO.

## Kiểm tra và cài đặt

Đã kiểm tra giao diện WinForms qua AutoCAD Core Console 2024: đổi kích thước, mở/đóng nhóm nâng cao, không chồng hàng, chiều cao danh sách, căn nút và chặn con lăn. Đã kiểm tra luồng sửa hồ sơ và lõi dữ liệu.

Đã xác nhận 55 gọi LAYFRZ, 66 mở băng lớp thử nghiệm và không ghi đè lệnh 44/55/66 có sẵn. Chọn đối tượng tương tác bằng LAYFRZ trong CAD có TDT và cài đặt trên máy sạch chưa được nghiệm thu.

Đóng toàn bộ AutoCAD, cài BHT-Setup-1.0.2.exe rồi mở lại CAD 2024. Không nạp chồng DLL mới vào phiên CAD đang dùng bản cũ. Bản này không tự dựng lại ký hiệu hay thay lý trình khi mở bản vẽ.
