# Kiểm chứng BHT 1.0.24

Ngày 14/10/2026. AutoCAD 2024 x64, .NET Framework 4.8. Backup nguồn và log: D:/lisp/BHT/audit-1.0.24. Kết quả xác nhận bộ cài cuối ghi tại BAO_CAO.md trong thư mục audit.

## DWG thực tế

Kiểm tra bản sao D:/lisp/BHT/New/BD_TD_TN(Km50+140-Km51+634.24).dwg. Hash bản gốc trước/sau: 936A8CE32E2E339A63A7DE99736A56296A7427FFA1698C492B90E4D832750875.

OBJ-000058: Km50+871.602, offset 1.450 m, NOI_SUY, phia_duong=PHAI, phia_tuyen=TRAI, đoạn trống. DOAN_10: 50140–51634 m, TRAI, GOI_03. Đây là xung đột dữ liệu phía đường, không phải lý trình cọc tiêu đè lên biển. OBJ-000057 và 59 có phia_duong=TRAI và đã vào DOAN_10; OBJ-000060 có phia_duong=PHAI nhưng gán thủ công DOAN_10. Không tự đảo dữ liệu khảo sát.

TIMRANHD10 lưu loai=TIM_DUONG; 15 đoạn chưa gắn route_id. Cần người dùng đối chiếu công trình để chọn Tim rãnh và gán tim riêng nếu đúng thiết kế. Không đổi loại chỉ dựa trên tên ID. Việc tính được thử trên hình học tham chiếu và mốc BHT đã lưu; chưa kiểm chứng đọc lại native TDT trong AutoCAD đầy đủ. File còn tham chiếu KT.dwg không có bên cạnh bản sao; không xử lý Xref trong đợt này.

## Kiểm tra đã đạt

- Core: 305, gồm hai ca xuất HEADER/NAME của I.449 cũ/mới và Excel thật.
- SELECTION124: 16 ca dữ liệu/lệnh trên bản sao DWG thật, gồm lý do khác phía, sửa phía đúng thì gán tự động, không dùng phía rãnh thay phía đường, khác tim không nhận chỉ vì Km, chồng đoạn yêu cầu chọn, tính một biển/tính theo tim cập nhật đoạn, giữ gán tay, Hủy, chọn đúng ID, bỏ gán và một Undo.
- Selection124Probe: 34 ca CAD/giao diện gồm hai trường theo đúng thứ tự, NAME cũ bổ sung HEADER mặc định, hai dòng không chồng và nằm trong mặt biển với tên dài/ngắn, bản tô nền/đường nét, cache đổi khi đổi HEADER, bảng đủ 15 đoạn, nguyên nhân xung đột thật, nút gán theo dòng, Hủy/bỏ gán. Đã xem ảnh bảng chọn.
- SELECTION124 đạt cả DLL thường + Lisp nguồn và DLL bảo vệ + FAS.
- SEG119: 19 ca, giao dịch/rollback, tim riêng, biên, nhiều tim, cọc số, Undo, lưu/mở lại, .bhtseg và .bht; đạt thường và bảo vệ.
- STATION117: 22 và lưu/mở lại.
- Palette: 2.897 kiểm tra bố cục và luồng biên tập.

Bộ kiểm tra UI/CAD chạy trên bản vẽ sạch và đọc bảng từ DWG thực tế; bộ kiểm tra Lisp chạy trực tiếp trên bản sao DWG đoạn 10. Không cài tự động, không sửa hoặc lưu đè bản gốc. Tên cũ chứa sẵn chữ “Đường” được giữ nguyên; người dùng có thể chỉnh ô tên để tránh lặp với dòng HEADER.
