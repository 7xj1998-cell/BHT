# BHT 1.0.36 - Bình đồ, đồng bộ và phát hành

Ngày 15/10/2026, từ 1.0.35. Dùng BHT-Setup-1.0.36.exe trên AutoCAD 2024; chỉ test bản sao. Bao gồm sửa chữ bị cắt của 1.0.33, hợp nhất/tombstone 1.0.34 và GLOBAL/LOCAL 1.0.35.

Trong tab Tuyến, QUẢN LÝ XỬ LÝ THEO ĐOẠN có Quản lý bình đồ / đồng bộ / phát hành. Đăng ký tên/danh tính, liên kết DWG cùng dự án và kiểm tra bản cần cập nhật. Xem trước gói dùng chính importer trên snapshot riêng, sau đó xác nhận đồng bộ bản đang mở. Bản đã phát hành/lưu trữ được chặn bởi các đường ghi BHT; tạo bản làm việc mới để tiếp tục. Phát hành đoạn thành R01/R02, không ghi đè; ghi lịch sử hash và revision.

Lưu DWG sau đăng ký/phát hành để giữ trạng thái. Không tự ghi các DWG đang đóng; các tệp liên quan được đọc từ bản đã lưu. Không chặn lệnh CAD gốc, không tự phát hành lại bản lịch sử. Tệp phát hành có thể còn tồn tại nếu tiến trình bị ngắt giữa hai bước ghi; không ghi đè nó và cần đối chiếu trước khi phát hành tiếp.

Chưa hoàn tất mọi trường hợp của roadmap: biển nhiều mặt/nội dung tùy chỉnh, đối chiếu identity legacy, merge nhánh có người duyệt, LOCAL theo từng layout và nhận sửa khảo sát vào LOCAL còn hạn chế. Xem báo cáo và hướng dẫn test A-O.
