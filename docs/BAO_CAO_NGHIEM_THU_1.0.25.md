# Kiểm chứng BHT 1.0.25

Ngày 14/10/2026. AutoCAD 2024 x64, .NET Framework 4.8. Nguồn dự phòng và log ở D:/lisp/BHT/audit-1.0.25. Kết quả kiểm tra bộ cài cuối ghi trong BAO_CAO.md tại thư mục audit.

## Dữ liệu và giới hạn

Kiểm tra trên bản sao BD_TD_TN(Km50+140-Km51+634.24).dwg. SHA256 bản gốc: 936A8CE32E2E339A63A7DE99736A56296A7427FFA1698C492B90E4D832750875. Không lưu đè DWG, .bht nguồn và không cài tự động.

Tim TIMRANHD10 và mốc đã lưu được dùng để tính trong CoreConsole. Chưa xác minh lại toàn bộ adapter native TDT trong AutoCAD đầy đủ. Môi trường kiểm thử dùng profile BHTResearch111 để tránh các lệnh nạp menu TDT/VNROAD làm gián đoạn script; không sửa profile TDT đang dùng.

## Kiểm tra

- Core: 305 ca, gồm bảng Excel thực tế 15 đoạn/3 gói và xuất đủ các mặt biển.
- PROGRESS125: tính riêng đoạn, bảo vệ kết quả, mở lại, chặn đổi tên/xóa tim trước khi thay đổi dữ liệu liên quan.
- Truyền gói: giữ nhãn/lý trình, nạp lặp không nhân bản, xung đột tọa độ/kết quả bị chặn, tên tim trùng được đổi tên liên kết, xuất đoạn chỉ có dữ liệu tương ứng, lưu/mở DWG và giữ ghi chú đích.
- Thử qua hai tiến trình/bản vẽ: đoạn 10 xác nhận, đoạn 11 chưa tính; xuất .bht rồi nạp vào DWG có tim đoạn 11; tính/xác nhận đoạn 11 vẫn giữ nguyên bản ghi và nhãn đoạn 10.
- Lệnh: Hủy không thay đổi dữ liệu; lỗi ghi chủ động trả lại hồ sơ và liên kết tim của đoạn; một Undo khôi phục phép tính.
- UI: 10 ca cho bảng đủ 15 đoạn, chọn đúng ID/tim, trạng thái nút theo xác nhận, xuất riêng/toàn bộ, đóng. Đã kiểm tra ảnh giao diện. Palette: 2.967 ca bố cục và luồng biên tập.
- DWG thực tế: 8 ca. Chặn xác nhận khi còn ba cọc XA_TUYEN; chỉ trên bản sao kiểm thử, bỏ gán ba hồ sơ này rồi xác nhận các hồ sơ hợp lệ để kiểm tra bảo vệ. Đây không phải quyết định nghiệm thu hoặc sửa dữ liệu khảo sát gốc.
- Hồi quy: SEG119 19 ca và lưu/mở lại; STATION117 22 ca; SELECTION124 16 ca và 34 kiểm tra UI/CAD; WORKFLOW122 11+23 ca, gồm gói gốc 179 hồ sơ và bảng đủ 15 đoạn.

Đọc RELEASE_NOTES_1.0.25.md để biết các điểm cần đối chiếu tại đoạn 10. Xác nhận đoạn không thay thế kiểm tra hiện trường. Ảnh liên kết vẫn nằm ngoài gói .bht.
