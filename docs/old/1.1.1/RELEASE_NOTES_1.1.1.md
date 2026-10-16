# BHT 1.1.1 — Bảng dễ đọc và kiểm tra nạp đoạn 14/15

Đã thiết kế lại bảng đoạn/tim và quản lý bình đồ: chữ Segoe UI 10,5, nền sáng, tương phản cao, tiêu đề xuống dòng, vùng chi tiết và các nút đủ chỗ. Cột phía hiển thị tiếng Việt, giữ nguyên mã dữ liệu.

Bảng xung đột nạp mở sau khi lệnh kết thúc, không chặn CAD. BHTNAPTICHLUY hiện bước xử lý và lưu nhật ký lần gần nhất tại `%LOCALAPPDATA%\BHT\Logs\rolling-import-last.log`. Đã giải phóng bộ đệm đọc/ghi XRecord đúng vòng đời.

7 gói BHT trong New đọc được, đúng cấu trúc/SHA256 và kiểm tra điểm/tim xác nhận. DOAN14 → D15 có 6 khác biệt của OBJ-000001: đích là đèn tín hiệu BOT19-R-000144, nguồn là biển P.127-80 BOT1-R-000041. BHT chặn và giữ nguyên đích. Không tự đổi mã, xoá hồ sơ hoặc đồng nhất DOAN01/DOAN_01.

428 unit; 316 kiểm tra bảng; 28 dữ liệu thật trên bản sao; 2 lệnh nạp/FAS; M1 31 Lisp + 29 .NET + mở lại; M2 50; merge 17; presentation 21; project 20; bảng hiệu chỉnh 101; Palette 3.102; regression 10 đều đạt. Đã kiểm tra DLL/FAS bảo vệ. GUI đang đứng chưa tái hiện được trong Core Console, công cụ kiểm tra cửa sổ mất kết nối. Chưa xác định nguyên nhân triệt để hoặc nghiệm thu DPI/zoom/pan thực tế. Các giới hạn và 11 lỗi signs từ 1.1.0 vẫn được ghi nhận, chưa chạy lại bộ signs rộng trong đợt này.

Đọc HUONG_DAN_HIEU_CHINH_1.1.1.md và BAO_CAO_NGHIEM_THU_1.1.1.md. Bản sửa cục bộ; không tự push GitHub. ZIP .bht giữ nguyên.