# BHT 0.6.59 — Biển tùy chỉnh trong thư viện

Đóng AutoCAD trước khi cài; mở lại sau khi cài. Bộ cài đầy đủ thư viện, tiếp tục nền 0.6.54 và các sửa đổi 0.6.57–0.6.58.

## Cách dùng

1. Mở Chọn biển… → chọn nhóm Biển tùy chỉnh.
2. Bấm Thêm biển tùy chỉnh… để nhập tên mẫu và chữ trên biển; mỗi dòng nhập là một dòng trên bảng.
3. Chọn mẫu và bấm Nội dung biển… hoặc nhấp đúp để sửa tên/chữ, rồi Lưu mẫu.
4. Bấm Chọn biển → Lưu hồ sơ → Chèn / cập nhật.

Đã bỏ nút Nhập bảng chữ tùy chỉnh… nằm ngoài thư viện. Bảng đang dùng từ bản cũ sẽ xuất hiện trong nhóm Biển tùy chỉnh khi mở thư viện từ hồ sơ đó.

Mẫu hiện dùng bảng chữ nhật nền xanh, viền/chữ trắng và một bảng trên cụm trụ. Bản này chưa bổ sung chọn hình dạng hoặc ghép bảng tùy chỉnh với nhiều mặt QCVN trên cùng trụ.

## Lưu dữ liệu

Mẫu dùng lại lưu tại %APPDATA%/BHT/custom-boards.xml. Tên mẫu giúp tìm trong thư viện; chỉ nội dung được vẽ trên biển. Mẫu thuộc máy/tài khoản hiện tại, không tự đồng bộ sang máy khác.

Khi chọn biển, nội dung được sao chép vào hồ sơ DWG và tạo block CAD như trước. Sửa mẫu không tự đổi các đối tượng đã chèn. Muốn áp dụng nội dung mới, chọn lại mẫu cho hồ sơ rồi lưu/cập nhật. Mở hồ sơ cũ luôn ưu tiên nội dung của hồ sơ đó.

Bản này giữ cách dùng phông hiện tại; không bổ sung xuất chữ thành nét hay cam kết DWG độc lập phông.

## Kiểm tra

Build và CoreTests; kiểm tra giao diện thư viện/bộ lọc, bảng cũ, áp dụng hồ sơ, preview; lưu/mở mẫu Unicode nhiều dòng, giới hạn nội dung, không ghi đè tệp lỗi; runtime FAS và bộ cài trên AutoCAD 2024 Core Console. Chưa nghiệm thu trên máy sạch hoặc thao tác trực tiếp bản vẽ người dùng.
