# BHT 0.6.57 — bảng chữ tùy chỉnh

Nền mã nguồn 0.6.54; bộ cài đầy đủ thư viện.

Trong Hồ sơ đối tượng:
1. Chọn/tạo hồ sơ và gắn điểm RTK.
2. Bấm Nhập bảng chữ tùy chỉnh… ở phần Ký hiệu trên CAD.
3. Nhập nội dung, Enter để xuống dòng. Ví dụ DỪNG LẠI ở dòng 1, QUAN SÁT ở dòng 2.
4. Dùng nội dung; kiểm tra số trụ. Bấm Lưu, rồi Chèn / cập nhật ký hiệu.

Hồ sơ được chuyển sang nhóm Khác nếu trước đó không phải Khác/Bảng quảng cáo. Block tùy chỉnh đã gán chỉ được bỏ sau xác nhận. Hủy cửa sổ không đổi nội dung.

Bảng mặc định nền xanh, chữ/viền trắng; dùng tỷ lệ biển chung. Không tự gán mã QCVN. Bảng một dòng có sẵn không tự thay bố cục. DWG vẫn chứa chữ CAD; việc xuất một DWG không phụ thuộc phông đang thảo luận, chưa triển khai ở bản này.

Kiểm tra: 233 CoreTests; tạo bảng hai dòng, kiểm tra nội dung/thứ tự, cache tách với bảng một dòng, lưu/mở DWG giữ nội dung và hai đối tượng chữ. Kiểm tra giao diện theo bộ PaletteLayoutProbe. Chưa thay thế nghiệm thu thao tác trên AutoCAD giao diện thật.

683 kiểm tra Palette đạt; 0.6.56 chỉ là gói nội bộ trước khi đồng bộ kiểm thử nhãn giao diện, không bàn giao.