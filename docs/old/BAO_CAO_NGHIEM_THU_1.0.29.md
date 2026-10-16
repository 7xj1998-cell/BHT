# Kiểm chứng BHT 1.0.29

Ngày 14/10/2026, AutoCAD 2024 Core Console, profile BHTResearch111.

- CoreTests: 315 đạt, gồm 10 kiểm tra so sánh nội dung biển: XML/NFC/NFD, khác mã/dấu, dữ liệu lạ, không sửa bản ghi.
- PaletteLayoutProbe: 3062 đạt; thêm nút mẫu nhãn vẫn nằm đúng nhóm và không tràn tại các kích thước kiểm tra.
- S652/L126/L129: kiểm tra tỷ lệ, hai hướng, chữ dễ đọc, nhãn kéo tay; 16 kiểm tra mới về đoạn dẫn dài, nhãn trên/dưới đường dẫn và áp dụng mẫu chỉ đổi nhãn. Source và DLL/FAS bảo vệ đạt.
- Progress125: 13 Lisp, 27 .NET, 7 chuyển đoạn và nhãn kế thừa đạt trên source/DLL-FAS bảo vệ. Nạp lặp nội dung XML tương đương thành công; HÒA/HOÀ vẫn bị chặn và được hiển thị bằng văn bản.
- Bản sao tệp đoạn 8: tái hiện đúng bad function: 1.51086 bằng runtime phát hành 1.0.28. Bản sửa bố trí 25 ký hiệu với bề rộng kiểm thử 5/5 và khoảng lùi 2; điểm, tim và thông tin lý trình/đoạn/gói được giữ. Đây là thông số kiểm thử, không phải thông số thiết kế bàn giao.
- Plot AutoCAD của OBJ-000119 và OBJ-000120 xác nhận mã/biển phụ phía trên, lý trình dưới, căn cùng trục, nhãn biển hai trụ theo đường dẫn phía trên.

Không sửa tệp gốc, chưa thử thao tác trực tiếp trong phiên CAD người dùng đang mở. Bản bố trí toàn dự án vẫn bỏ qua báo hiệu ngoài đầu/cuối tim đoạn. Không tự sửa nội dung bảng phân đoạn, ghép kết quả hoặc đổi HÒA thành HOÀ.

Ảnh, log tái hiện, báo cáo runtime và bộ cài lưu tại D:\lisp\BHT\audit-1.0.29.
