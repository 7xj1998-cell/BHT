# Báo cáo kiểm tra BHT 1.0.33

Ngày 15/10/2026, phiên bản 1.0.32 -> 1.0.33, nhánh development/incremental-confirmation. Không push GitHub.

Người dùng báo chữ bị đè/cắt trong cửa sổ hiệu chỉnh. Nguyên nhân: DataGridView giữ chiều cao tiêu đề mặc định sau khi font tăng; bài UI cũ chỉ kiểm tra chiều cao cả bảng. ObjectCorrectionForm.Grid nay tự tính chiều cao tiêu đề, cho phép xuống dòng và thêm khoảng đệm. Các hàng nhập liệu chuyển sang AutoSize, nhãn thông báo giới hạn theo độ rộng cửa sổ. Áp dụng cùng cấu hình cho lịch sử.

Trước: test CorrectionGrid133Probe trên DLL 1.0.32 có 30 FAIL/101 ở font 125/150/200%. Sau: 101 PASS, 0 FAIL; 19 UI hồi quy và 410 CoreTests PASS. Đã xem ảnh render font 150%, các dấu tiếng Việt và tiêu đề rõ, nội dung dài xuống dòng. Font tăng trong test mô phỏng mật độ chữ, không thay cho chạy thực tế trên màn hình Windows ở từng DPI.

Mã sửa: src/dotnet/BHT.Palette/ObjectCorrectionForm.cs, hàm Grid và constructor. Test mới: tests/IntegrationTests/CorrectionGrid133Probe.cs. Bằng chứng: D:/lisp/BHT/audit-1.0.33, gồm ảnh người dùng, log trước/sau, ảnh render và baseline dirty worktree.

Runtime FAS/DLL protected cuối đạt lại 101 kiểm tra bảng và 50 kiểm tra hiệu chỉnh, bao gồm lỗi cố ý/rollback, U/REDO, lịch sử, đổi RTK, xuất/nạp và mở lại DWG. Đây là bằng chứng trên runtime sẽ đóng gói.

Không đổi nghiệp vụ, dữ liệu hoặc lịch sử revision. Milestone 2 còn loại biển phức tạp; Milestone 3-5 chưa nghiệm thu. Cần kiểm tra UI trên AutoCAD thực tế và tiếp tục theo lộ trình độc lập. [Hướng dẫn test](HUONG_DAN_TEST_TOAN_BO.md).
