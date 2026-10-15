# Kiểm chứng BHT 1.0.26

Ngày 14/10/2026. AutoCAD 2024/Core Console, profile BHTResearch111; mọi thử nghiệm DWG dùng bản sao. Các số kiểm tra dưới đây là kết quả tự động, không thay thế nghiệm thu toàn bộ bình đồ.

| Nhóm kiểm tra | Kết quả |
| --- | --- |
| Build .NET và CoreTests | 305 đạt, 0 lỗi |
| PaletteLayoutProbe | 2.987 đạt; gồm kiểm tra mới về hướng nhãn, Hủy, xác nhận và hàng nút tại các mức tỷ lệ giao diện |
| SCALE652 | 21 đạt: tỷ lệ, vị trí/hướng ký hiệu, hồ sơ, điểm khảo sát, lặp thao tác |
| LABEL126 | 58 đạt: 8 hướng đường nối × 2 chế độ, cụm hai trụ, mặt chính/phụ/lý trình, nhãn tay, tham số không hợp lệ |
| Bản bảo vệ DLL + FAS | SCALE652/LABEL126 đều đạt như bản source |
| PROGRESS125 với DLL bảo vệ | 13 Lisp + 20 .NET + 7 chuyển đoạn + 1 kiểm tra nhãn kế thừa + 10 UI + 8 bản sao đoạn 10 đều đạt |
| Bản sao đoạn 10: đổi hướng/tỷ lệ, giữ dữ liệu | 7 đạt |
| Lưu và mở lại bản sao đoạn 10 | 4 đạt: hướng, toàn bộ hồ sơ, hình học nhãn, điểm khảo sát |
| Đồng bộ phiên bản | 1.0.26 / 1.0.26.0 đạt |

Đã xem ảnh hộp tỷ lệ: nút Về chuẩn 1:1, Áp dụng và Hủy hiện đầy đủ. Phần nội dung phía trên có cuộn; kiểm tra giao diện mô phỏng tỷ lệ không phải nghiệm thu mọi màn hình/driver.

Thử trên dữ liệu thực phát hiện đồng bộ tỷ lệ từng cập nhật trường block của hồ sơ đèn cũ. Đã chặn việc ghi hồ sơ đó khi chỉ đổi trình bày; kiểm tra lại toàn bộ OBJ và điểm khảo sát đạt. Lưu/mở lại so sánh định danh bền vững và số thực 8 chữ số thập phân, không so sánh ename tạm của hai phiên CAD.

Thư mục kết quả: `build/v1.0.26/bin/coretests_result.txt`, `build/v1.0.26/scale652.log`, các thư mục `progress125-*`, `label126-real-*`; ảnh `build/v1.0.26/bin/ui-preview/sign-scale.png`. Kết quả đóng gói/runtime/bộ cài sau phát hành được lưu riêng tại `D:\lisp\BHT\audit-1.0.26\BAO_CAO.md` để giữ nội dung bản phát hành bất biến.

## Phạm vi và giới hạn

- Hướng nhãn áp dụng toàn bản vẽ theo đoạn đường nối gần biển nhất. Nhãn dịch chuyển thủ công giữ vị trí/hướng. Cỡ chữ quá lớn vẫn có thể vượt khoảng trống hoặc chạm các đối tượng nền; không tự thay tỷ lệ người dùng chọn.
- Giữ cơ chế kế thừa/xác nhận lý trình theo đoạn. Ba cọc OBJ-000182/183/184 xa tim vẫn chặn xác nhận theo kiểm tra từ 1.0.25; không chỉnh dữ liệu gốc để vượt kiểm tra.
- Nghiên cứu REA là bằng chứng tĩnh. Chưa xác nhận đọc lý trình native trong phiên TDT đầy đủ. Phép thử DwgFiler .NET thất bại ngay với polyline chuẩn trong tiến trình riêng; đã loại mã đó khỏi phát hành. Xem [báo cáo TDT](NGHIEN_CUU_TDT_REA_1.0.26.md).
- DWG gốc đoạn 10 giữ SHA-256 `936A8CE32E2E339A63A7DE99736A56296A7427FFA1698C492B90E4D832750875`. Không tự cài bản mới vào AutoCAD đang làm việc.
