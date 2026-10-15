# BHT 1.0.16 — Dự án .bht

## Lỗi đã xác định

Filebaohieu.dwg có INSUNITS = Millimeters. Phiên bản 1.0.15 từ chối nạp vào bản vẽ có INSUNITS = Undefined, kể cả khi bản vẽ trống. Khi đặt đơn vị đích bằng nguồn, bản cũ nạp được 4.235 đối tượng CAD. Dữ liệu nguồn có 801 điểm, 179 hồ sơ, 334 bản ghi ảnh, 2 bộ dữ liệu, 1 chuỗi cọc và 1 tuyến kèm mốc. Đây là nguyên nhân tái hiện được; chưa có bản vẽ đích cụ thể của người dùng để xác nhận mọi trường hợp lỗi.

## Cách dùng

1. Đóng AutoCAD và cài BHT-Setup-1.0.16.exe; mở lại bản vẽ nguồn.
2. Tab Tuyến > Xuất dự án BHT (.bht), hoặc lệnh BHTXUATDUAN. Chọn tên tệp mới.
3. Mở bản vẽ đích > Nạp dự án BHT, hoặc BHTNAPDUAN; chọn tệp .bht. Vẫn có thể chọn DWG BHT cũ trong bộ lọc tệp.
4. Bản vẽ trống nhận đơn vị nguồn. Nếu bản vẽ đã có hình và khác đơn vị, chọn Giunguyen chỉ khi hai bản vẽ dùng cùng hệ tọa độ thực tế; mặc định Huy. BHT giữ đúng trị số tọa độ, không quy đổi đơn vị hay hệ tọa độ.

Nạp là một thao tác Undo. Bản vẽ có sẵn dự án BHT vẫn bị chặn để tránh trùng mã và ghi đè hồ sơ. Gói .bht không dùng INSERT hay APPLOAD; dùng BHTNAPDUAN.

## Nội dung gói

Gói .bht là tệp dự án có manifest phiên bản 1 và dữ liệu CAD nội bộ, giữ block tùy chỉnh/cách trình bày và các tham chiếu tuyến. Không chỉ đổi phần mở rộng DWG. BHT xác minh cấu trúc, dung lượng tối đa 512 MiB trước nén và SHA256 trước khi chạm vào bản vẽ đích. Tệp hỏng, thiếu dữ liệu hay định dạng chưa hỗ trợ bị từ chối. SHA256 dùng phát hiện sai hỏng, không phải chữ ký xác thực tác giả.

Giữ điểm, hồ sơ, tuyến/mốc, chuỗi cọc, cấu hình và liên kết ảnh. Ảnh gốc vẫn ở thư mục ngoài như các bản trước; khi chuyển máy cần mang theo thư mục ảnh. Gói không gom toàn bộ bản đồ nền, xref hay phông cài riêng. Không ghi đè bản xuất đã tồn tại.

## Kiểm chứng

Core 258 PASS, gồm 9 kiểm tra gói tệp. PROJECT116 đạt 44 kiểm tra trên dữ liệu người dùng bằng DLL thường và DLL bảo vệ. Luồng lệnh đạt 7 kiểm tra với FAS/DLL bảo vệ: nạp/đơn vị, Undo, hủy, chọn giữ tọa độ, cập nhật ký hiệu, xuất và lưu/mở lại. Bộ hiển thị/chuyển dự án cũ đạt 10 + 12 kiểm tra trên bản bảo vệ.

Kết quả bộ cài và runtime độc lập sau đóng gói nằm trong `D:/lisp/BHT/audit-1.0.16/BAO_CAO.md`. Chưa kiểm chứng native TDT có khóa và các CAD hãng khác. Bản vẽ gốc trên Desktop không bị sửa.
