# Nghiệm thu BHT 1.0.15

Ngày 14/10/2026, AutoCAD Core Console 2024, .NET Framework 4.8 x64. Mục tiêu: nhãn trong thân cọc, nút cọc tự động, thống nhất trình bày biển đơn lẻ/hàng loạt theo tuyến.

| Phạm vi | Kết quả |
| --- | --- |
| Core | 249 PASS |
| Cọc tiêu MARKER114 | 21 PASS, Undo/UCS/lưu mở lại PASS |
| PRESENTATION115 | 13 PASS trên FAS và DLL bảo vệ |
| Bố trí đường ROAD106 | 44 PASS trên FAS và DLL bảo vệ, Undo/lưu mở lại PASS |
| Kiểu đường nối DISPLAY105 | 20 PASS, Undo/lưu mở lại PASS |
| Giao diện | 2.683 PASS trên DLL bảo vệ |
| Thư viện đèn và bố trí | PASS trên FAS và DLL bảo vệ |
| DWG mẫu | 52/52 cọc tại điểm, hướng vuông góc đúng; RTK/mốc/lý trình và bản gốc không đổi |

PRESENTATION115 kiểm tra chữ dài vừa thân, nhãn vàng ở giữa cọc hai bên đường và trên cung, bỏ nhãn trắng bị lặp, không tạo lại khi cập nhật nhãn điểm, giữ tên điểm dùng chung. So sánh trực tiếp hình do lệnh đặt riêng tạo với hình bố trí hàng loạt cho biển một/hai trụ; kiểm tra hướng tiếp tuyến trên cung đảo chiều.

ROAD106 giữ kiểm tra khoảng lùi toàn hình/nhãn, đường cong, lớp khóa, điểm vượt đầu/cuối tuyến, hình học không đo được, thao tác hủy và rollback khi ghi lỗi. Kỳ vọng giữ góc cũ trong hai bộ thử được đổi theo yêu cầu 1.0.15; kiểm tra bảo toàn tỷ lệ/điểm vẫn giữ, bổ sung kiểm tra hướng tuyến độc lập.

## Giới hạn

Bộ `run_signs.ps1` tổng hợp dừng ở `BHTSIGNPROBE` trước khi chạy các bước Lisp. Đã đối chiếu cùng môi trường với DLL/FAS 1.0.14 và gặp cùng hiện tượng. Không tính bộ này là PASS. Các ca bố trí liên quan yêu cầu mới được kiểm tra riêng như bảng trên.

Chưa nghiệm thu trên chính DWG 261010.Bienbao BOT830 đang có trong ảnh của người dùng, máy nhận khác hoặc CAD hãng khác. Bản sao DWG 1.0.14 có sẵn được dùng cho phép thử 52 cọc. Chưa kiểm chứng lý trình native TDT có khóa. Hình xem trước xuất từ hình học DXF; phông chữ trình xem có thể khác CAD.

Nhật ký và kết quả sau đóng gói: `D:/lisp/BHT/audit-1.0.15/BAO_CAO.md`. Bộ cài không tự chạy cài đặt; người dùng đóng AutoCAD trước khi cài.
