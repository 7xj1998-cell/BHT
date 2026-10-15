# BHT 0.6.60 — Trụ mất biển và các biến thể P.124

Đóng AutoCAD trước khi cài, mở lại sau khi cài. Giữ đầy đủ thư viện của các bản trước, gồm biển tùy chỉnh và đèn cần vươn.

## Trụ mất biển

Hồ sơ nhóm Biển báo, tình trạng Mất mặt biển, còn trụ dùng bảng nền xanh RGB 0/118/190, viền trắng và chữ TRỤ MẤT BIỂN. Bảng dùng cùng kích thước, vị trí chân, số trụ như mẫu cũ. Không thêm nhãn dưới chân.

Để cập nhật biển cũ: bật Tô nền nếu muốn nền xanh, chọn hồ sơ rồi Chèn / cập nhật. Áp dụng lại Xoay biển vuông góc với tuyến hoặc hướng hai điểm để cập nhật góc theo chế độ bạn chọn. Các hồ sơ đã lưu hướng sẽ dùng góc đúng khi cập nhật; góc xoay tay chưa lưu chế độ hướng không bị tự đoán lại.

Nguyên nhân góc lệch: block BHT_MISSING_FACE trước đây rơi vào quy ước trục +X của block tùy chỉnh. Mẫu này thực tế có đầu biển theo +Y. Đã thống nhất với biển BHT thông thường.

Tắt Tô nền vẫn ẩn nền xanh và giữ khung/chữ để in trắng đen.

## P.124

Chỉ gộp tên hiển thị tương đương đã đối chiếu:
- P.124A, P.124A-1 → P.124a1: cấm quay đầu trái.
- P.124A-2 → P.124a2: cấm quay đầu phải.
- P.124B, P.124B-1 → P.124b1: cấm ô tô quay đầu trái.
- P.124B-2 → P.124b2: cấm ô tô quay đầu phải.

Không gộp trái với phải, xe với ô tô, hoặc c/d/e/f. Các tệp nguồn khác nhau về cách dựng/preview; đây là gộp lựa chọn cùng nội dung, không phải xóa tệp giống hệt. Giữ nạp mã cũ và block đã chèn.

## Kiểm tra

AutoCAD 2024 Core Console: hướng theo tuyến và hai điểm, giữ vị trí, nền xanh/ẩn nền/khôi phục nền, đổi mẫu một/hai trụ, bỏ nhãn cũ, thư viện đèn; kiểm tra UI tìm mã cũ và bộ lọc P.124; CoreTests; FAS/DLL bảo vệ, lưu/mở lại DWG và xác minh bộ cài. Chưa cài chồng vào phiên CAD người dùng đang mở, chưa nghiệm thu máy sạch.
