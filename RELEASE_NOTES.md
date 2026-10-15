# BHT 1.1.0 - Bản thử nghiệm cho AutoCAD 2024

Đây là bộ cài runtime đã build và kiểm tra cục bộ ngày 15/10/2026, nâng từ 1.0.36. Chưa nghiệm thu đầy đủ trên giao diện AutoCAD và chưa phải bản ổn định cho hồ sơ bàn giao.

## Tải và cài

1. Tải `BHT-Setup-1.1.0.exe` và `BHT-SHA256.txt` tại Assets bên dưới.
2. Đối chiếu SHA-256 trước khi chạy. Lưu DWG, đóng toàn bộ AutoCAD, chạy bộ cài và chọn **Cài đặt BHT**.
3. Mở AutoCAD 2024, mở **bản sao** DWG, gõ **BHT** hoặc **BTH**. Kiểm tra phiên bản bằng **BHTTRANGTHAI** / **BHTNETINFO**.
4. Không NETLOAD DLL mới vào phiên CAD đã nạp bản cũ. Bộ cài chưa có chữ ký Authenticode.

SHA-256 của EXE:

`136B35EC0D6AA7668D35BC4D86F5B2F312B437F50FD284449177FF07F0BDA433`

## Các thay đổi chính

- Báo cáo nhanh giữ đầy đủ thông báo trong giới hạn công khai, có cuộn, xuống dòng, đổi cỡ chữ, sao chép và đổi kích thước; bỏ việc âm thầm chỉ giữ 40 dòng cuối.
- Báo cáo và xem trước tính theo đoạn cho phép tiếp tục thao tác CAD. Phương án được kiểm tra lại trước khi ghi; dữ liệu đổi phải xem trước lại.
- Chặn điểm đầu lệch tim trên 1 m, chiều đi ra ngoài đầu/cuối tuyến và thay cơ sở khoảng cách khi đã có mốc; cảnh báo khi chọn giữa tuyến.
- Nhãn chọn chiều đọc theo chiều tăng Km và tiếp tuyến cục bộ, độc lập UCS/góc nhìn; giữ dữ liệu RTK và kết quả khảo sát.
- Giảm lượt quét khi cập nhật ký hiệu/tỷ lệ hàng loạt; sửa bố trí lặp dịch chuyển rồi lỗi trên mẫu D13, kiểm tra khoảng lùi thực tế và phục hồi khi lỗi.
- Giữ nền tảng tính/xác nhận bổ sung, hiệu chỉnh từng hồ sơ khóa, lịch sử revision, merge theo tổ tiên, GLOBAL/LOCAL và quản lý phát hành R01/R02. Không đổi cấu trúc ZIP `.bht`.

## Bằng chứng và giới hạn

- 428 unit test đạt, 0 lỗi; đã chạy lại với DLL protected.
- AutoCAD Core Console: 269 assertion hướng nhãn; 10 assertion bố trí D13; 50 assertion hiệu chỉnh; 17 assertion merge; 21 assertion trình bày; 20 assertion quản lý bình đồ. Đã kiểm tra Undo/REDO và lưu/mở DWG ở các fixture tương ứng.
- Gói đã được xác minh 1.651 file bundle và chạy 10 kiểm tra smoke bằng chính FAS/DLL đóng gói.
- Bộ hồi quy biển báo ADS còn **11 FAIL SIGN**, giống danh sách ở baseline 1.0.36. Chưa kết luận tất cả là kỳ vọng test cũ; không coi bộ này đã đạt.
- AutoCAD GUI chưa nghiệm thu đầy đủ: cần kiểm tra zoom/pan khi bảng mở, DPI, hướng đọc nhãn và thao tác thực tế. Lỗi giấy phép **License checkout timed out** của máy kiểm tra chưa được giải quyết bằng bản BHT này.
- Chưa hoàn thiện mọi tình huống hiệu chỉnh biển nhiều mặt, mốc ảnh hưởng toàn tuyến, đối chiếu danh tính legacy, chọn nhánh xung đột và LOCAL theo từng layout. Không đồng bộ tự ghi các DWG đang đóng.
- Bảo vệ RELEASED/READ_ONLY_ARCHIVE áp dụng cho thao tác BHT; không khóa lệnh CAD gốc. Phục hồi lỗi chủ động không chứng minh an toàn khi mất điện hoặc mọi lỗi native.

## Phạm vi bản công bố

Tag này xác định **bản phân phối runtime**, không phải snapshot mã nguồn phát triển 1.1.0. Nhánh `releases/build-1.1.0` chỉ chứa manifest và ghi chú phân phối; nhánh mã nguồn `main` không được ghi đè. Runtime dùng FAS và DLL protected; không công bố source riêng của build này.

Bộ cài có tài nguyên bên thứ ba. Chủ dự án đã xác nhận quyền phân phối công khai bộ cài đầy đủ trong yêu cầu phát hành này; quyền của các nhà cung cấp vẫn được giữ nguyên, không chuyển sang giấy phép mã nguồn BHT.

Giáo trình PDF có hình ảnh được bổ sung thành asset riêng sau khi hoàn tất kiểm tra bố cục. Bản vẽ khảo sát, ảnh hiện trường và dữ liệu người dùng không được đưa lên GitHub.
