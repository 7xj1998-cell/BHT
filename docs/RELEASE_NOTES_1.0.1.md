# BHT 1.0.1 — sửa hiển thị và tăng tốc đổi nền biển

Ngày phát hành: 11/10/2026. Kiểm thử trên AutoCAD 2024 x64.

## Cài đặt và cập nhật biển cũ

1. Lưu công việc rồi đóng AutoCAD. Chạy `BHT-Setup-1.0.1.exe`, sau đó mở lại CAD.
2. Với bản vẽ đã có biển, mở **Tuyến & báo cáo → Công cụ bổ sung → Cập nhật mẫu biển đã vẽ** một lần. Thao tác này áp dụng phông mới và bỏ điểm dựng hình thừa; giữ vị trí, góc và tỷ lệ ký hiệu.
3. Kiểm tra kết quả rồi lưu bản vẽ. BHT không tự vẽ lại biển khi mở DWG. Biển chèn mới dùng ngay mẫu mới.

Nâng cấp mẫu là thao tác riêng, có thể mất lâu hơn bật/tắt nền. Mẫu kiểm tra 177 hồ sơ mất khoảng 15 giây để cập nhật một lần.

## Giao diện và công cụ

- Khôi phục nền xanh, ô nhập sáng. Tên tab có đủ chỗ cho hai dòng và tăng kích thước theo DPI.
- **Tuyến & báo cáo**: chọn nguồn Polyline hoặc TDT rồi bấm **Nạp tuyến…**. Công cụ đọc cọc TDT chỉ xuất hiện khi dòng đang chọn là tuyến TDT.
- Bỏ ghi chú “TDTSolution 9.1 bản thường: sẵn sàng”.
- Công cụ ghép ảnh và thư mục ảnh nằm ở **Ảnh hiện trường**; cỡ dấu X và nhãn RTK nằm ở **Điểm vị trí**.
- Nhóm **Báo cáo** chứa xuất Excel, xuất CSV và kiểm tra bản vẽ. Nhóm **Công cụ bổ sung** chứa cập nhật mẫu biển, thứ tự hiển thị và nạp block riêng. Lệnh kỹ thuật cũ vẫn dùng được qua dòng lệnh.

## Phông và điểm thừa

Chữ trên mặt biển dùng Giaothong1, gồm chữ tiêu đề và số khoảng cách. Đã sửa trường hợp IE.472a nhập 500 m bị rơi về phông nét mảnh. Các điểm dựng hình nằm trong bản sao block biển được bỏ; điểm khảo sát, vòng tròn chân trụ và dấu gốc được giữ. Chữ nhãn ngoài biển vẫn dùng kiểu chữ CAD của BHT.

Thư viện DWG/phông được đóng gói trong bộ cài cục bộ. Chưa bảo đảm gửi một DWG cho máy thiếu phông vẫn hiển thị giống hoàn toàn; đợt này không chuyển chữ thành nét hình học.

## Bật/tắt tô nền

Chỉ đổi mẫu có nền/không nền trên các ký hiệu hiện có. Không tính lại lý trình, không sắp lại nhãn và đường dẫn, không tự dời/xoay/đổi tỷ lệ biển. Hình tô đặc bên trong và viền vẫn được giữ khi tắt nền.

Trên bản sao `261011_test.dwg`: 177 hồ sơ, 118 ký hiệu thuộc nhóm đổi nền. Năm lượt thử bằng Core Console 2024:

| Thao tác | Bản 1.0.0, trung vị | Bản 1.0.1, trung vị |
| --- | ---: | ---: |
| Tắt nền | 14,55 giây | 78 ms |
| Bật nền | 14,67 giây | 63 ms |

Lần đầu tạo mẫu không nền mất khoảng 1,20–2,13 giây trong các lượt thử. Số đo không gồm thời gian màn hình CAD vẽ lại; tốc độ thực tế phụ thuộc bản vẽ và máy. Undo khôi phục cả mẫu biển và trạng thái tô nền. Nếu có ký hiệu trên lớp khóa, thao tác báo lỗi và giữ nguyên toàn bộ đợt đổi nền.

## Đoạn / gói thầu — tùy chọn

Mở **Tuyến & báo cáo → Phân chia đoạn / gói thầu (tùy chọn)**. Chọn **Lưu bảng TSV mẫu…**, sửa dữ liệu rồi dùng **Nạp bảng TSV…**.

- `SEGMENT_ID`: mã đoạn, không trùng nhau.
- `PACKAGE_ID`: mã gói thầu.
- `KM_START_M`, `KM_END_M`: lý trình đầu/cuối tính bằng mét. Km40+500 nhập `40500`.
- `SIDE`: `TRAI`, `PHAI` hoặc `HAI_BEN`.

Lưu UTF-8, cột phân cách bằng Tab. Dòng ví dụ chỉ minh họa, cần thay bằng dữ liệu công trình. BHT không suy gói thầu từ tên DWG. Hai đoạn có chung mốc biên cần chọn đoạn cho hồ sơ tại mốc đó. Công trình không phân chia đoạn/gói có thể bỏ qua mục này; vẫn tính lý trình và xuất báo cáo.

## Phím tắt lớp

| Lệnh | Chức năng khi BHT cung cấp |
| --- | --- |
| `11` | Bật các lớp — LAYON |
| `22` | Tắt lớp của đối tượng chọn — LAYOFF |
| `33` | Cô lập lớp — LAYISO |
| `44` | Giữ nguyên lệnh của TDT; BHT không đăng ký |
| `55` | Rã đông các lớp — LAYTHW |
| `BHTLAYFRZ` | Đóng băng lớp — LAYFRZ |

BHT không ghi đè phím tắt đã có lúc nạp. Có thể dùng các tên riêng `BHTLAYON`, `BHTLAYOFF`, `BHTLAYISO`, `BHTLAYFRZ`, `BHTLAYTHW`. `LAYTHW` rã đông lớp; để khôi phục trạng thái trước LAYISO dùng `LAYUNISO` của AutoCAD.

## Phạm vi kiểm tra

Đã kiểm tra Core Console 2024, giao diện WinForms, mẫu biển toàn thư viện, tham số biển, hoàn tác, lỗi lớp khóa và lưu/mở lại DWG. Bản gốc `261011_test.dwg` không thay đổi. Nguồn TDT 9.1 đang cài có mô tả 44 là “Xóa lớp”; theo lựa chọn của người dùng, BHT giữ lệnh đó.

Chưa nghiệm thu trên máy sạch chưa từng cài BHT. Chưa xác nhận bằng phiên CAD tương tác đang làm việc cùng TDT; kết nối tự động tại thời điểm kiểm tra không truy cập được tài liệu CAD. Kiểm tra DPI gồm kích thước tab 100/125/150/200% và ảnh giao diện thử nghiệm, chưa thay thế kiểm tra màn hình của người dùng.
