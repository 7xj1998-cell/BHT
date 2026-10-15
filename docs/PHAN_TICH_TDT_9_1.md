# Phân tích tích hợp TDT 9.1 với BHT

Ngày khảo sát: 13/10/2026. Phạm vi: bộ TDT Solution 2022 đang cài, AutoCAD 2024 x64 và bản sao dữ liệu thử TDT cũ. Không sửa thư viện nhà cung cấp.

**Cập nhật 14/10/2026:** REA đã phân tích thân hàm và xác định `GetStationAtPoint`/`GetPointAtStation` đi qua bộ quản lý **trắc dọc**, không phải phép chiếu điểm bình đồ. Không dùng đề xuất gọi trực tiếp cặp hàm này cho báo hiệu. Xem [Nghiên cứu REA 1.0.26](NGHIEN_CUU_TDT_REA_1.0.26.md) để biết đường dữ liệu tim/cọc gốc, bằng chứng và giới hạn thử nghiệm mới.

## Kết luận từ mã và thư viện

BHT 1.0.10 lấy hình học bằng `Tdt91Alignment.Snapshot`: explode trong bộ nhớ, chọn/nối các đoạn Curve rồi tạo Polyline tham chiếu. `Tdt91Stakes.Scan` quét TEXT, MTEXT, attribute và hình explode để tìm chuỗi Km. Đây là hai nguồn dữ liệu khác với lý trình do TDT quản lý; không đủ cơ sở suy ra rằng đầu Polyline luôn là Km0 hoặc chiều Polyline luôn là chiều tăng lý trình.

Tài liệu tính năng đi kèm xác nhận tên sản phẩm TDTSolution 9.1 dù thư mục cài mang tên TDT Solution 2022. Các module kỹ thuật chính là PE x64 (`0x8664`); một số DLL giao diện phụ là x86. Không dùng việc thấy tên thư mục hay biểu tượng để kết luận module native đã nạp trong CAD.

## Cấu trúc phụ thuộc

| Thành phần | Vai trò và bằng chứng |
| --- | --- |
| AlignmentDb.dbx | 1.153 export; chứa TdtDbAlignment, TdtGePoleManager và các hàm tính lý trình; import TDTObjects.dbx |
| TDTObjects.dbx | 180 export; chứa TdtDbPointObj và dữ liệu điểm/cọc cơ sở |
| AlignmentUI.arx | Giao diện tuyến; phụ thuộc AlignmentDb, TDTObjects, ProjectDH, NhapSoLieuTuyen, FileManager |
| ProjectDH.arx | Quản lý dữ liệu dự án; liên kết AlignmentDb, BtBD, TDTObjects, FileManager |
| RoadSignsUI.arx | Biển báo; phụ thuộc cả dữ liệu tuyến, TDTObjects, AcGeomentObj và thư viện CAD |
| Acad.rx | Danh sách nạp các module UI trong phiên đầy đủ, bắt đầu bằng TDTObjectsUI.arx |

Tệp `modules.json` lưu SHA256, kiến trúc PE, import/export của từng module. `native-signatures.txt` giải mã chữ ký bằng DbgHelp. Đây là bằng chứng tĩnh của bản đang cài, không chứng minh API được nhà cung cấp bảo đảm ổn định.

## API lý trình được xác nhận tồn tại

```cpp
bool TdtDbAlignment::GetStationAtPoint(const AcGePoint3d&, double&, double&);
bool TdtDbAlignment::GetPointAtStation(double, AcGePoint3d&, double&);
double TdtGePoleManager::GetChainageAtPole(int) const;
```

Các export khác có tên `OnChangedAlignmentStation`, `CocTrongPhamviDutthuoc`, `PhamviCocTheoDutthuoc`. Chúng cho thấy mô hình có xử lý lý trình và đứt thước riêng; không được thay toàn bộ bằng công thức khoảng cách dọc Polyline cộng một mốc.

Tên export không chứa tên tham số. Chưa được mặc định hai giá trị double trả qua tham chiếu là lý trình và khoảng cách lệch tim theo quy ước cụ thể. Cần kiểm tra với điểm nằm trên tim, điểm lệch trái/phải, đầu/cuối tuyến, đoạn cong, tuyến đảo chiều và đoạn đứt thước. Giá trị trả về false phải được giữ là không xác định, không đổi thành 0.

## Vì sao chỉ đọc hình học chưa đủ

- Polyline phụ thuộc hình hiển thị và thứ tự đoạn sau explode; hướng nối có thể khác chiều tăng lý trình của tuyến.
- Cọc/nhãn trong DWG có thể ẩn, đổi kiểu hiển thị hoặc là proxy khi thiếu module.
- Quét chữ Km gần tim có thể lấy nhãn của tuyến song song hoặc nhánh khác; cần giữ nguồn và yêu cầu duyệt mốc.
- Lý trình đứt thước có thể khiến một lý trình có nhiều vị trí; phải gắn đoạn/nhánh thay vì chỉ lưu một số station.
- Tuyến thay hình học hoặc thay mốc cần làm mất hiệu lực kết quả đã tính, kể cả khi handle tim không đổi.

## Hướng tích hợp đề xuất

1. **Nguồn native ưu tiên:** kết nối trực tiếp TdtDbAlignment trong phiên TDT hoạt động, kiểm tra tên lớp thật, trạng thái proxy, module đang nạp và phiên bản/hash đã kiểm chứng. Không tự nạp DLL lạ hay gọi con trỏ trên proxy.
2. **Đối chiếu hai chiều:** tọa độ → lý trình → tọa độ; ghi sai số, kết quả native và quy ước lệch trái/phải. Điểm ngoài phạm vi hoặc phép đổi không nhất quán phải trả trạng thái cần kiểm tra.
3. **Dữ liệu cọc:** lấy dữ liệu xuất tuyến chính thức của TDT khi API cọc chưa có hợp đồng ổn định. Menu VNROAD có `XSLT` (Xuất số liệu tuyến), `CTHH` (chọn tuyến hiện hành), `TIM` (tìm cọc). Cần xem định dạng xuất thật trước khi viết bộ nhập.
4. **Phương án hình học:** vẫn hỗ trợ Polyline và mốc được người dùng duyệt, nhưng hiển thị rõ nguồn lý trình là mốc tham chiếu, không ghi là native TDT.
5. **Lưu nguồn truy vết:** handle tuyến, fingerprint hình học, phiên bản module, chiều tuyến, nguồn lý trình và thời điểm xác nhận; tính lại khi một trong các yếu tố thay đổi.

`NativeStationProbe.cs` là chương trình nghiên cứu riêng, không đưa vào bộ cài. Chương trình chỉ đọc bản sao DWG trong bộ nhớ và ghi phép đối chiếu hai chiều. Chỉ dùng kết quả chạy thành công để quyết định có đưa bộ kết nối native vào sản phẩm.

## Giới hạn kiểm chứng hiện tại

Core Console không nạp được đầy đủ TDTObjects/AlignmentDb; từng báo lớp cha TdtDbPointObj chưa đăng ký. Nhật ký thử cũ cũng ghi nhận giới hạn này. Đây không phải bằng chứng bản cài TDT của người dùng bị hỏng.

Chưa tìm được đúng đường dẫn `doan1.dwg`. Bản thử dùng `C:/Users/Le Bao/BHT_TDT_TEST_068/doan1-bd.dwg`, sao chép thành `doan1-bd-copy.dwg`; SHA256 hai file đều là `3A03CE0150F7A0B63C47F34600C29DC9708F64CFA2B941DFFAD744B8E280F4FF`.

Chưa có bằng chứng chạy native thành công trong đợt kiểm tra này. Vì vậy chưa công bố BHT đã đọc đúng lý trình gốc TDT 9.1 và chưa gắn lời gọi native thử nghiệm vào luồng tính hồ sơ.
