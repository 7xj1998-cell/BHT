# Rà soát BHT theo quy trình kỹ sư hiện trạng tuyến

Ngày 14/10/2026. Phạm vi: mã nguồn 1.0.14, bố trí ký hiệu, dữ liệu khảo sát, mốc/lý trình, giao diện hồ sơ và xuất/nhập dự án. Phân biệt tọa độ đo ngoài thực địa với vị trí hình trình bày là nguyên tắc chính. Đây là đánh giá phần mềm, không thay hồ sơ thiết kế hoặc quy chuẩn áp dụng cho công trình.

## Quy tắc theo loại đối tượng

| Loại | Cần | Không nên áp dụng mặc định | Trạng thái |
| --- | --- | --- | --- |
| Cọc tiêu | Chân tại điểm đo, trục ký hiệu vuông góc tiếp tuyến tại cọc; nhãn H/Km nếu có dữ liệu | Đẩy hình ra xa điểm; đường gấp khúc tránh mặt biển; hộp chọn nhiều kiểu đường nối | Đã sửa 1.0.14; sửa được cọc đã bố trí lệch bằng lệnh hàng loạt |
| Cột Km | Giữ mốc gốc, số Km và dữ liệu lý trình; phân biệt mốc nhập tay với số tính | Di chuyển cùng đợt dàn biển; tự đổi số Km để khớp tuyến | Giữ nguyên trong bố trí hàng loạt; chưa thay quy tắc hình cột Km |
| Biển báo/bảng chỉ dẫn | Hồ sơ nhiều mặt, đủ chân trụ, đường nối về điểm đo khi tách hình ra để đọc | Coi trung bình các chân là một điểm đo mới; nối đường xuyên mặt biển | Giữ các phép bố trí/tránh mặt biển và hồi quy nhiều chân |
| Đèn | Phân biệt vị trí chân cột ngoài thực địa và biểu diễn thu phóng trên bình đồ | Coi mọi loại đèn là một mẫu biển; dịch POINT đo khi dàn hình | Giữ cách bố trí hiện có; nên bổ sung lựa chọn chỉ dàn biển/bảng ở đợt sau |
| Công trình/nhóm khác | Mẫu theo loại, vị trí có nguồn gốc rõ; ghi chú chưa phân loại | Gán quy tắc trụ biển cho nhóm chưa xác định khi chưa biết ý nghĩa | Giữ công cụ tùy chỉnh; nên yêu cầu phân loại trước khi dàn tự động |

## Những chức năng cần giữ

- Nhập RTK và bảo toàn tọa độ, cao độ, chuỗi số gốc. Không dùng thao tác trình bày để sửa điểm đo.
- Mô hình hồ sơ nhiều chân/nhiều ảnh; cảnh báo dùng chung điểm và trùng hồ sơ. Một điểm không mặc nhiên tương đương một biển.
- Phân biệt hình học tim, chiều tuyến, mốc xác nhận và lý trình đã tính. Tuyến dùng dàn hình có thể khác tuyến dùng tính lý trình; thao tác dàn hình không đổi liên kết nghiệp vụ.
- Phân biệt mốc nhập thực địa và kết quả nội suy; cảnh báo mốc trùng, thiếu mốc, ngoài phạm vi. Không suy lý trình native chỉ từ hình học TDT hoặc tên bản vẽ.
- Undo, bảo vệ lớp khóa, báo ID bỏ qua, lưu/mở lại và xuất/nhập dự án có kiểm tra. Các trường hợp chưa xử lý không nên trả thông báo như đã hoàn thành mọi đối tượng.
- Ảnh hiện trường, bảng thống kê và đoạn/gói thầu là chức năng có ích; đoạn/gói là tùy chọn, không nên bắt buộc cho công trình nhỏ.

## Các chỗ đã xử lý trong đợt này

1. `08-signs.lsp`: hàm đường nối chung trước đây có thể tạo vòng gấp khúc khi hai đầu trùng nhau. Đã ngăn vòng vô nghĩa này và tách cọc tiêu khỏi đường nối. Đặt cọc dựa trên pháp tuyến tại điểm, không dùng một góc chung cho cả tuyến cong.
2. Cọc đã dịch tay hoặc lưu waypoint cũ được chuẩn hóa khi người dùng chủ động bố trí. Nhãn dịch/xoay cùng cọc, giữ tỷ lệ. Thiếu tuyến thì giữ góc cũ và báo; ngoài đầu/cuối tuyến không kéo dài tuyến để tự đoán hướng.
3. Giao diện đặt riêng cọc bỏ lựa chọn nối thẳng/gấp khúc/điểm trung gian. Hộp bố trí tất cả ghi rõ phạm vi, tránh hiểu khoảng lùi áp dụng cho cả cọc tiêu.
4. `ProjectTransfer.cs`: sao chép và ánh xạ lại handle đường tham chiếu hiển thị, kể cả PL chưa là một ROUTE. Nếu chỉ sao chép chuỗi handle thì sau chuyển DWG có thể trỏ sai hoặc mất đường tham chiếu.
5. Runtime ưu tiên thư mục đã chỉ định, tránh lấy DLL/tài nguyên của bản khác cùng số phiên bản trên Support Path.

## Đề xuất tiếp theo, chưa triển khai

| Ưu tiên | Đề xuất | Lý do |
| --- | --- | --- |
| P2 | Thêm phạm vi bố trí: đối tượng chọn / tuyến chọn / toàn bản vẽ và bộ lọc loại | Hiện lệnh hàng loạt duyệt các ký hiệu đủ điều kiện trong Model. Bản vẽ nhiều tuyến cần kiểm soát phạm vi rõ hơn. |
| P2 | Cho xem trước số lượng sẽ dịch, cọc giữ điểm, đối tượng thiếu tuyến/lớp khóa trước khi áp dụng | Tránh phát hiện sai lựa chọn sau khi dàn hàng loạt; vẫn cần Undo. |
| P2 | Tách thao tác dàn hình đèn/công trình khỏi mặc định dàn biển | Mỗi loại có ý nghĩa bình đồ khác nhau; chưa có đủ yêu cầu để tự thay tất cả trong đợt này. |
| P2 | Kiểm thử phiên TDT có khóa trên bản sao hồ sơ thực tế | Chưa có bằng chứng đã đọc lý trình native; kiểm tra hình học và nhãn không thay thế được. |
| P3 | Gom cập nhật mẫu, sửa copy và chẩn đoán sâu vào nhóm Bảo trì | Cần giữ khả năng sửa hồ sơ cũ nhưng không đặt ngang hàng thao tác khảo sát hằng ngày. |

Không đề xuất xóa chức năng chỉ vì ít dùng. Những chức năng hỗ trợ bản vẽ cũ nên giữ trong nhóm bảo trì; luồng hằng ngày tập trung vào nhập điểm, hồ sơ, mốc/lý trình, trình bày và xuất.

## Tham khảo atlisp.cn

Đã truy cập https://atlisp.cn/skillhub và endpoint công khai `/api/skillhub/registry`. Tại thời điểm kiểm tra, endpoint trả `code: 0` nhưng `data` chỉ chứa các tên `updated_at`, `categories`, `skills`, không có nội dung skill; giao diện lại đọc `d.skills`. Chưa tải hoặc cài skill, chưa chạy script cài đặt từ website. Đánh giá trên dựa trên mã nguồn BHT và kiểm chứng AutoCAD; không viện dẫn atlisp.cn như chứng nhận kỹ thuật.
