# BHT 1.0.31 — Hướng dẫn tính và xác nhận bổ sung

Ngày 15/10/2026. Bản kiểm tra cục bộ cho AutoCAD 2024, .NET Framework 4.8. Bộ cài BHT-Setup-1.0.31.exe dùng để kiểm tra Milestone 1; chưa nghiệm thu trên AutoCAD GUI với hồ sơ thực tế.

## Cài và kiểm tra trên AutoCAD

1. Lưu công việc, đóng tất cả cửa sổ AutoCAD.
2. Chạy `D:\lisp\BHT\BHT-Setup-1.0.31.exe`, bấm **Cài đặt BHT**, chờ báo thành công rồi bấm **Hoàn tất**. Bộ cài dành cho tài khoản Windows hiện tại, giữ bản cũ trong thư mục backup.
3. Mở AutoCAD 2024, gõ **BHT** hoặc **BTH**, kiểm tra phiên bản 1.0.31. Không cần NETLOAD thủ công.
4. Mở bản sao DWG để thử quy trình 25 hồ sơ khóa + một hồ sơ bổ sung bên dưới. Kiểm tra cả một lần Undo, lưu/mở lại và nạp lặp tệp tích lũy.
5. Khi cần quay về bản ổn định, đóng AutoCAD và cài lại `D:\lisp\BHT\old\local-20261015\installers\BHT-Setup-1.0.29.exe`; sử dụng bản sao dữ liệu trước khi thử 1.0.31.

Bộ cài chứa FAS, ba DLL, thư viện biển/đèn và phông. EXE chưa có chữ ký Authenticode. Các kiểm tra tự động không thay thế kiểm tra thao tác thực tế trên máy. Xem [phạm vi kiểm chứng bộ cài](BAO_CAO_NGHIEM_THU_1.0.31.md).

## Chức năng đã bổ sung

Đã cho phép tính và xác nhận hồ sơ mới trong đoạn có hồ sơ đã khóa. Ví dụ: DOAN_08 đã xác nhận 25 biển, đang làm DOAN_11 thì phát hiện thiếu OBJ026. Có thể bổ sung OBJ026 vào DOAN_08; 25 hồ sơ cũ giữ nguyên bản ghi, điểm RTK và kết quả. Không cần dùng Mở lại kết quả đoạn để bổ sung.

## Thao tác trên bản sao DWG

1. Lưu một bản sao DWG trước nâng cấp. Tạo hồ sơ OBJ026 và liên kết đúng điểm khảo sát, gán DOAN_08 bằng bảng gán đoạn hiện có.
2. Mở BHT hoặc BTH, vào Quản lý xử lý theo đoạn. Chọn DOAN_08 và tim đã gắn với đoạn đó.
3. Chọn **Tính lý trình theo đoạn**. Đọc bảng xem trước: hồ sơ khóa được bỏ qua; hồ sơ thiếu điều kiện có lý do riêng. Áp dụng khi dữ liệu đúng.
4. Kiểm tra lý trình, offset, phía đường, phân đoạn, điểm và nhãn của hồ sơ bổ sung.
5. Chọn **Xác nhận kết quả đoạn**. BHT chỉ xác nhận những hồ sơ chưa khóa của đoạn, sau khi kiểm tra lại kết quả. Nếu còn hồ sơ chưa hợp lệ, cần xử lý trước khi xác nhận.
6. Xuất dữ liệu tích lũy hoặc hồ sơ đoạn sang tên mới, ví dụ `DOAN_08_BO_SUNG_20261015.bht`. Không ghi đè tệp đã xuất.

Khi toàn bộ hồ sơ đã khóa, nút xác nhận không được bật; lời gọi xác nhận lặp trả về không có thay đổi. Tính khi không có hồ sơ đủ điều kiện cũng không ghi dữ liệu và không cập nhật nhãn hàng loạt.

## Khi BHT chặn thao tác

| Thông báo hoặc tình huống | Cách xử lý |
|---|---|
| Dữ liệu đổi sau xem trước | Đóng bảng dự kiến, kiểm tra thay đổi, tính lại phương án trước khi áp dụng. |
| Tim, mốc, điểm hoặc phân đoạn đã đổi | Đối chiếu với bản đã xác nhận. Không xác nhận bổ sung cho đến khi giải quyết sự khác biệt. |
| Hồ sơ cũ thiếu bằng chứng xác nhận | Dữ liệu vẫn đọc được. Giữ bản gốc và đối chiếu; BHT không tự tạo snapshot để coi dữ liệu cũ là đã được kiểm chứng. |
| Gói trong OBJ không khớp SEG | Kiểm tra bảng đoạn và hồ sơ nguồn. Không bỏ bảo vệ phân đoạn để ép nạp. |
| Khác kết quả đã xác nhận hoặc nội dung khảo sát | Xem bảng đối chiếu hai bản; chưa có cơ chế tự chọn revision trong 1.0.31. |
| Snapshot có tọa độ không hữu hạn | Đối chiếu hồ sơ nguồn; thao tác xuất/nạp bị chặn, không coi NaN là tọa độ hợp lệ. |

Hồ sơ cũ có mốc khai báo và bằng chứng điểm/tim/revision phù hợp có thể bổ sung. Hồ sơ dùng mốc tự động nhưng thiếu snapshot ngữ cảnh sẽ bị chặn khi không đủ bằng chứng. Không tự điền metadata vào 25 hồ sơ cũ khi bổ sung hồ sơ mới.

## Nạp tích lũy và tên gói

Dùng **Nạp dữ liệu kế thừa (.bht)** như trước. BHT chuẩn hóa riêng cách viết `GOI01` và `GOI_01` sang cách viết của bản vẽ đích khi chứng minh được cùng tên gói không rỗng, cùng toàn bộ tập mã đoạn, cùng khoảng Km và phía đường. Không suy đoán `GOI1` là `GOI01`; không tự chuyển đối tượng sang đoạn khác. OBJ và SEG được kiểm tra cùng chính sách ở bước đối chiếu và bước nạp.

Tim đã gắn ở đích chỉ được dùng lại nếu thông số và kiểm tra hình học khớp. Nạp lặp không tạo thêm hồ sơ hoặc tim cho ca đã kiểm chứng. Hai kết quả xác nhận khác nhau vẫn bị chặn. Bản này chưa có quan hệ tổ tiên revision để xác định bản mới hơn.

## Undo và khôi phục

Lệnh tính/xác nhận qua giao diện có nhóm Undo; kiểm thử đã xác nhận một lần `U` khôi phục từng thao tác khi không nằm trong nhóm Undo bao ngoài. Nếu script bên ngoài đang gom nhiều lệnh vào một nhóm Undo, phạm vi Undo phụ thuộc nhóm đó. Khi ghi lỗi, cơ chế rollback khôi phục bản ghi và thực thể trong phạm vi thao tác đã kiểm thử.

Giữ bản sao DWG/.bht trước nâng cấp để quay lại phần mềm cũ. Không mở bản làm việc chưa sao lưu bằng phần mềm cũ rồi lưu đè để thử khả năng tương thích. Bản 1.0.31 không đổi cấu trúc ZIP .bht; metadata bổ sung nằm trong OBJ/XRecord. Chưa kiểm chứng việc phần mềm cũ chỉnh rồi lưu mọi metadata mới.

## Nạp build phát triển trên máy

DLL nằm ở `build/v1.0.31/bin`, loader ở `src/lisp/BHT-1.0.31.lsp`. Dùng phiên AutoCAD mới, bảo đảm không có DLL BHT phiên bản khác đã nạp. NETLOAD BHT.Bridge.dll và BHT.Palette.dll từ cùng thư mục, sau đó đặt đường dẫn module và nạp loader tại dòng lệnh CAD:

```lisp
(setq *bht-module-root* "D:/lisp/BHT/sharing-0654/source/src/lisp")
(load "D:/lisp/BHT/sharing-0654/source/src/lisp/BHT-1.0.31.lsp")
```

Cách nạp này dành cho kiểm tra phát triển; không thay bộ cài đang dùng trên máy. Nếu AutoCAD đã tự nạp BHT cũ từ bundle, cần dùng profile/phiên kiểm thử cô lập để tránh trộn DLL; không NETLOAD chồng phiên bản.

## Phần chưa có

Chưa có hiệu chỉnh riêng hồ sơ khóa, lịch sử revision, hợp nhất theo tổ tiên, tombstone, bố trí GLOBAL/LOCAL hay đồng bộ nhiều DWG. Không dùng Mở lại cả đoạn như quy trình mặc định để sửa một hồ sơ. Các chức năng này thuộc Milestone 2–5.

Xem [báo cáo kiểm thử](BAO_CAO_MILESTONE_1_1.0.31.md), [bản đồ kiến trúc](MILESTONE_1_ARCHITECTURE.md) và [mục lục tài liệu cũ](LICH_SU_TAI_LIEU.md).
