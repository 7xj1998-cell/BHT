# Kiểm chứng BHT 1.0.27

Ngày 14/10/2026. AutoCAD 2024 Core Console, profile BHTResearch111, môi trường thử riêng.

| Kiểm tra | Kết quả |
| --- | --- |
| Build .NET/CoreTests | 305 đạt, 0 lỗi |
| Tái hiện lỗi gói phát hành 1.0.26 | Gọi `LispApi.Call("bht:api-sign-display", …)` nhận eInvalidInput ở cả hai hướng |
| DISPLAY127 source | 9 đạt, 0 lỗi: .NET gọi được hai hướng, thông số được lưu, tham số sai bị từ chối, giữ API cũ và hồ sơ |
| DISPLAY127 DLL/FAS bảo vệ | 9 đạt, 0 lỗi qua cùng Application.Invoke của giao diện |
| PaletteLayoutProbe | 3.012 đạt; thêm 25 kiểm tra căn hai cạnh nút TDT/không cuộn ngang tại chiều rộng 340, 400, 480, 560 rồi thu lại 400 px |
| Ảnh kiểm tra | Ba nút cùng lề trái/phải, khoảng cách đều; TDT vẫn ẩn/hiện theo loại tuyến |

Nguyên nhân bỏ sót trước đây: kiểm tra tỷ lệ gọi hàm Lisp trực tiếp nên không phát hiện thiếu đăng ký để .NET gọi. Bổ sung `SignDisplay127Probe.cs` và `run_sign_display127.ps1` để kiểm tra đúng ranh giới này; có chế độ tái hiện lỗi với gói cũ.

Không sửa dữ liệu khảo sát hoặc bản vẽ gốc. Không thay thuật toán nhãn/tim/lý trình. Kết quả xác minh bộ cài và runtime cuối cùng lưu tại `D:\lisp\BHT\audit-1.0.27\BAO_CAO.md`, ngoài nội dung bản phát hành bất biến.
