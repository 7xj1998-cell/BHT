# BHT 1.0.26 — Hướng nhãn và tỷ lệ trình bày

- Thêm chọn nhãn song song/vuông góc theo đường nối biển–trụ, đúng hướng tham chiếu đã thống nhất.
- Mã biển, các mặt biển phụ và lý trình cùng hướng dễ đọc; sửa lỗi chữ lộn ngược sau đổi tỷ lệ.
- Cụm nhãn biển hai trụ đặt giữa hai đường nối; thay tỷ lệ giữ tâm cụm. Nhãn chỉnh tay giữ vị trí/hướng.
- Hộp tỷ lệ có nội dung cuộn và hàng nút Áp dụng/Hủy cố định. Thay tỷ lệ/hướng áp dụng cùng giao dịch, không thay điểm khảo sát và lý trình.

Trong tab Hồ sơ, mở **Tỷ lệ biển / nhãn…**, chọn hướng và hai tỷ lệ rồi **Áp dụng**. Hướng lấy theo đoạn nối gần biển nhất. Tỷ lệ rất lớn có thể vượt khoảng trống giữa các đường nối; cần chỉnh trình bày, không tự thu nhỏ chữ đã chọn. **Về chuẩn 1:1** đặt lại hai tỷ lệ, giữ hướng đang chọn.

Giữ các chức năng xử lý theo đoạn, xác nhận kết quả và nạp dữ liệu kế thừa từ 1.0.25. Bản vẽ/gói cũ không phải nhập lại từng báo hiệu.

## Nghiên cứu TDT

Đã dùng REA/Ghidra phân tích thư viện đang cài. Phát hiện `GetStationAtPoint`/`GetPointAtStation` thuộc luồng trắc dọc, không phải phép chiếu điểm bình đồ. Đã xác định nguồn hình học tim, lý trình gốc và dữ liệu cọc/hiệu chỉnh độc lập với nhãn hiển thị. Xem [báo cáo và bằng chứng](NGHIEN_CUU_TDT_REA_1.0.26.md).

Chưa đưa bộ đọc native vào sản phẩm: cần kiểm chứng trong phiên AutoCAD/TDT đầy đủ. Thử giao diện DwgFiler .NET đã thất bại trong Core Console riêng và được loại khỏi bản phát hành. Bản này chưa loại bỏ yêu cầu có mốc hợp lệ khi tính lý trình theo quy trình hiện tại.
