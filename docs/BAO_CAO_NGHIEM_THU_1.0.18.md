# Nghiệm thu BHT 1.0.18

Ngày 14/10/2026. AutoCAD Core Console 2024, .NET Framework 4.8 x64. Sao lưu source trước sửa tại `D:/lisp/BHT/audit-1.0.18/source-before-1.0.18.zip`.

## Chẩn đoán và phạm vi

Ảnh người dùng có hồ sơ OBJ-000120, nhóm KHAC, tên “ĐỊA PHẬN XÃ ĐỨC HÒA”, hai trụ và lý trình Km48+525.03. Mã nguồn đồng thời loại nhãn lý trình khi không có nhãn tên bên ngoài và loại nhóm KHAC khỏi xuất Excel. Đây là hai bộ lọc độc lập, không phải mất lý trình khảo sát. Cụm W.239a/S.509a còn dùng phép bố trí nhãn phụ tính riêng và chỉ mục đường nối trước cập nhật.

Đã tách nhãn lý trình khỏi điều kiện tên ngoài, đưa cụm nhãn lên đầu mặt biển, cho các dòng phụ theo vị trí thực của nhãn chính, dùng đường nối hiện tại, đưa TEXT có XData BHT_KH/NHAN lên trên bằng DrawOrderTable theo từng không gian. Báo cáo dùng chung bộ chọn bản ghi và tạo dòng trong Core để kiểm tra cả lọc, dữ liệu và XLSX.

## Kết quả trước đóng gói

| Phạm vi | Kết quả |
| --- | --- |
| Core, gồm lọc và xuất XLSX | 264 PASS, 0 FAIL |
| CAPTION118, FAS/DLL bảo vệ | 21 PASS, API .NET PASS, Layout PASS, lưu/mở lại PASS |
| Trình bày PRESENTATION115 | 13 PASS, so sánh đặt riêng/hàng loạt |
| Cọc MARKER114 | 21 PASS, Undo/UCS và lưu/mở lại PASS |
| ROAD106 | 44 PASS, khoảng lùi, Undo, lưu/mở lại |
| DISPLAY105 | 20 PASS, kiểu đường nối, Undo, lưu/mở lại |
| STATION117 | 22 PASS, tính lại theo tim, Undo, lưu/mở lại |
| FIELD | 51 PASS, lưu/mở lại và Undo |
| Chuyển dự án DISPLAY104 | 10 PASS hiển thị, 12 PASS chuyển dữ liệu |
| Palette | 2.723 PASS trên DLL bảo vệ |

CAPTION118 kiểm tra bảng tên hai trụ của nhóm Khác/Quảng cáo/Chưa xác định chỉ có một nhãn lý trình ngoài mặt biển; cụm hai mặt có ba dòng thẳng hàng ở bốn góc; nhãn phía đầu biển; cập nhật lặp lại ổn định; nhãn tay và nhãn phụ đi cùng nhau; còn trụ mất mặt vẫn ghi lý trình. Kiểm tra cập nhật/xóa chữ sau tính riêng và toàn tuyến, không đổi INSERT và RTK. Gây lỗi khi ghi nhãn giữa chừng xác nhận hoàn tác dữ liệu/chữ. Đặt SOLID sau nhãn và xác minh DrawOrderTable trên Model/Layout, cả sau lưu/mở lại. API thử qua Application.Invoke, không chỉ gọi Lisp trực tiếp.

Core kiểm tra năm nhóm biển, tên địa danh có dấu, số trụ/mặt, lý trình/nguồn tính, mã rỗng, không nhập cọc/đèn/công trình vào báo cáo. Đọc lại XML trong XLSX, đối chiếu số dòng chi tiết và tổng hợp; bảng khác nhóm nhưng cùng tên không bị gộp.

Log bộ cài và runtime sau đóng gói, SHA256 và fingerprint source cuối cùng ghi bên ngoài source ở `D:/lisp/BHT/audit-1.0.18/BAO_CAO.md` để giữ phát hành bất biến.

## Giới hạn

Các phép thử dựng DWG riêng, không sửa DWG gốc hoặc workbook đang mở trong ảnh. Chưa có bản lưu mang trạng thái hiện tại của DWG trong ảnh để nghiệm thu trực tiếp bố trí. Không khẳng định số biển mới của bản vẽ đó từ tổng 106 trong ảnh. Cần xuất lại Excel sau cài; phần mềm không ghi trực tiếp vào báo cáo đã xuất. Chưa kiểm tra native TDT có khóa hoặc CAD hãng khác.
