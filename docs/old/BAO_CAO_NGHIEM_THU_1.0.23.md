# Kiểm chứng BHT 1.0.23

Ngày 14/10/2026. AutoCAD 2024 x64, .NET Framework 4.8/C#5. Backup nguồn: D:/lisp/BHT/audit-1.0.23/source-before-1.0.23.zip. Log và kết quả bộ cài cuối: D:/lisp/BHT/audit-1.0.23/BAO_CAO.md.

## Phát hiện

Lệnh nạp TDT trước đây gọi route-create với TIM_DUONG cố định. Sửa tuyến nhận chuỗi tự do trong prompt trông giống lựa chọn; chỉ chấp nhận đúng TIM_DUONG/TIM_RANH/KHAC và bộ đọc số chỉ nhận dấu chấm. Giá trị khác dẫn đến thông báo chung “Thông số tuyến không hợp lệ”. Chưa biết chính xác chuỗi người dùng nhập trong ảnh. Bản 1.0.23 dùng ComboBox chọn loại và kiểm tra riêng từng trường số, nhận cả dấu phẩy; CLI dự phòng nhận D/R/K, mã đầy đủ và tên tiếng Việt.

Bảng mốc trả mã dòng lưu gốc, không dùng số hàng đã sắp xếp. Xóa nhiều mốc kiểm tra snapshot và toàn bộ lựa chọn trước khi ghi, khôi phục dữ liệu nếu lỗi cập nhật hồ sơ. Thao tác thuộc một nhóm Undo. Dòng hỏng vẫn hiện để quản lý; các dòng không chọn giữ nguyên. Mốc từ hồ sơ cọc không cho xóa tại đây. Xem vị trí đổi khoảng cách dọc tim về vị trí hình học theo điểm đầu/chiều tim rồi vẽ dấu tạm đúng tâm.

## Kiểm thử

- ROUTE123: 31 kiểm tra Lisp/lệnh đạt, gồm chọn loại TDT qua adapter giả lập, hủy trước khi gọi adapter, cập nhật giữ loại/mốc, sửa loại, thông số sai, áp dụng không đổi, bảng lỗi thời, lựa chọn sai/trùng, xóa nhiều dòng khác thứ tự lưu, rollback khi lỗi, bảo toàn số cọc/hình học, vị trí theo chiều ngược, dấu tạm/dọn dấu, dòng hỏng, lệnh công khai và Undo. Lưu/mở lại giữ loại và mốc.
- Route123UiProbe: 24 đạt, gồm ba loại tim, dấu phẩy, ngoại suy 0, lỗi số/ID tại trường, thứ tự bảng, điểm gãy, hệ số bất thường, chọn nhiều dòng, mốc tự động, dòng hỏng, bảng trống và bố cục tối thiểu. Đã xem ảnh render hộp sửa tuyến và bảng mốc.
- PaletteLayout: 2.897 đạt, giữ bố cục sáu bước và các phần khác.
- RouteManage: sửa/đổi tên/xóa/chọn lại hình học và Undo đạt.
- STATION117: 22 đạt và lưu/mở lại. WORKFLOW122: nạp gói thật 179 hồ sơ, cô lập mốc nguồn/đích, bảo toàn bảng đoạn và 5 ca tính trên tim đoạn 8 đã có mốc xác nhận đạt.

ROUTE123 và UI chạy lại với DLL bảo vệ/FAS trước phát hành. Kết quả runtime và bộ cài cuối ghi tại báo cáo audit sau đóng gói.

## Giới hạn

Các kiểm thử tạo DWG riêng, không ghi đè DWG/.bht của người dùng hoặc tài nguyên TDT. Nhánh nạp TDT dùng adapter giả lập để kiểm tra chọn/giữ loại; không coi là nghiệm thu trích hình học/cọc native TDT trên DWG đoạn 10. Đọc cọc native vẫn cần đúng profile TDT hoạt động.

Sau cập nhật cần đóng/mở lại AutoCAD để thay DLL; tính lại lý trình sau đổi thông số hoặc xóa mốc để cập nhật hồ sơ và nhãn đã vẽ. Không tự cài bộ phát hành.
