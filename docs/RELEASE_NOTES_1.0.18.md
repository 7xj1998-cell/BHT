# BHT 1.0.18 — Nhãn biển, lý trình và báo cáo

## Các lỗi đã sửa

Biển nhóm Khác như “ĐỊA PHẬN XÃ ĐỨC HÒA” đã có lý trình trong hồ sơ nhưng bị thiếu chữ trên bản vẽ vì bộ dựng nhãn yêu cầu có tên biển bên ngoài. Nay lý trình được dựng riêng, giữ tên trên mặt biển, không lặp lại tên phía ngoài. Bảng quảng cáo, bảng chưa xác định và biển mất mặt còn trụ cũng hiện lý trình nếu hồ sơ có giá trị.

Mã biển và lý trình được đưa lên phía đầu mặt biển trong hệ tọa độ của biển, không phụ thuộc phía trên màn hình. Cả cụm dùng chung trục, góc, cỡ chữ và khoảng dòng. BHT đo đường nối sau khi tạo/cập nhật, tránh lệch và nhảy vị trí do dùng đường nối cũ. Khi đường nối đặc biệt cắt khu vực nhãn, BHT dịch cả cụm để tránh, thay vì dịch rời từng dòng. Kéo nhãn chính bằng CAD giữ vị trí tay và các dòng phụ đi theo khi cập nhật.

Các nhãn biển và lý trình còn được tự đưa lên trên cùng thứ tự hiển thị CAD, theo đúng Model/Layout chứa chúng; không gọi lệnh DRAWORDER từ Palette. Tính lại, nhập hoặc xóa lý trình cập nhật phần chữ đã vẽ; không di chuyển ký hiệu hay tọa độ RTK. Nếu tính lại theo tim gặp lỗi cập nhật nhãn, dữ liệu và chữ được trả về trạng thái trước thao tác.

Báo cáo Excel trước đây chỉ lọc BIEN_BAO. Bản này nhận thêm BANG_CHI_DAN, BANG_QC, KHAC, CHUA_XAC_DINH. Biển không có mã vẫn có dòng với tên riêng và lý trình đã lưu. Mỗi hồ sơ là một bộ biển, số trụ/mặt giữ trong cột riêng; không nhân đôi bộ nhiều mặt. Bảng Tổng hợp đếm theo nhóm, mã, tên và tình trạng; bảng Danh sách biển giữ nguồn, trạng thái và revision lý trình để đối chiếu.

## Cập nhật bản vẽ và Excel hiện có

1. Đóng AutoCAD, cài BHT-Setup-1.0.18.exe, rồi mở lại DWG.
2. Trong tab Tuyến, chọn **Cập nhật mẫu biển đã vẽ** để cập nhật các biển đang có và giữ vị trí ký hiệu. Có thể dùng **Vẽ / cập nhật ký hiệu** cho từng hồ sơ.
3. Kiểm tra lý trình đã lưu. Nếu cần đổi theo tim mới, dùng **Tính lại theo tim đang chọn…** sau khi kiểm tra đủ mốc bao quanh khu vực cần tính.
4. Bấm **Xuất báo cáo biển báo Excel** để tạo lại XLSX. File Excel đã xuất bằng bản cũ không tự thay đổi khi cài phần mềm.

Biển chưa có lý trình hợp lệ để trống; không tự suy ra từ tên DWG hoặc chữ lý trình nền CAD. Đối tượng thuộc công trình ven tuyến, đèn và cọc không đưa vào báo cáo biển. Dữ liệu nhóm Chưa xác định xuất đúng trạng thái để người kiểm kê rà soát.

Đã kiểm tra trên AutoCAD Core Console 2024, gồm FAS/DLL bảo vệ, API từ .NET, Model/Layout, lưu/mở lại và hồi quy bố trí. Chưa chạy trực tiếp DWG lưu trạng thái đang thấy trong ảnh hoặc nghiệm thu vị trí chữ trên chính bản vẽ đó. Kết quả bàn giao ở `D:/lisp/BHT/audit-1.0.18/BAO_CAO.md`.
