# 4. Kiểm thử, lỗi và giới hạn

## Bằng chứng của đợt 1.1.5

| Kiểm tra | Kết quả ghi nhận | Cách đọc |
| --- | --- | --- |
| CoreTests | 443 PASS, 0 FAIL | Đã chạy lại trên bộ nguồn công bố; log bổ sung public-build sau bước chuẩn bị |
| Layer native/legacy/real-data | 4 Lisp + 18 .NET + 1 Undo PASS | Runtime FAS/DLL bảo vệ cuối; có compare canonical, source hash, save/reopen, import/no-op/conflict |
| Progress125 | 13 Lisp + 28 .NET + 7 stage, nhãn kế thừa PASS | Runtime cuối; gồm route collision, giữ khóa, Line cũ, layer theo mã remap |
| Correction132 | 50 PASS | Runtime cuối, hiệu chỉnh/history/rollback |
| Incremental131 | 31 Lisp + 29 .NET + reopen PASS | Cùng đợt 1.1.5, trước thay đổi cuối nhận diện Line/thứ tự chọn lại tim |
| Scope114 | 28 + reopen PASS | Kiểm tra giới hạn/phạm vi, trước các thay đổi cuối nói trên |
| Palette layout | 3.142 PASS | Probe bố cục/editor workflow; không phải thao tác chuột thực tế |
| AutoCAD GUI thực tế | Chưa nghiệm thu đầy đủ | Công cụ ảnh từng timeout; giấy phép/proxy/font từng chặn phiên chạy |

`evidence/` chứa các log chọn lọc từ bản gốc và build kiểm tra nguồn công bố. Đường dẫn máy phát triển trong log là dấu vết lịch sử; bản DWG/.bht/ảnh thực địa không được đưa lên Git. Không thể tái chạy ca real-data chỉ bằng clone công khai nếu thiếu fixture riêng.

Các lỗi trung gian 1.1.5 đã sửa: gán thuộc tính layer trước khi có database (eNoDatabase), gán plot style trong CTB (ePlotStyleInColorDependentMode), từ chối Line native trong hồ sơ cũ, vô hiệu hóa kết quả trước khi thay tim thành công. Log lỗi cũ vẫn giữ ở máy phát triển; không coi chúng là lỗi đang tái hiện sau bản sửa cuối.

## Mức bao phủ lộ trình A–O

| Nhóm | Bằng chứng có | Phần chưa nên khẳng định |
| --- | --- | --- |
| A bổ sung 25+1 | Incremental131, M1, Progress125 | Mọi cấu hình tim/mốc ngoài thực tế |
| B sửa mã, C đổi điểm, K đổi đoạn | Correction132, snapshot/history; có tính riêng và duyệt chuyển đoạn | Biển nhiều mặt/XML tùy biến, mốc Km/H ảnh hưởng nhiều OBJ |
| D/E/F trình bày GLOBAL/LOCAL | Presentation135: 21 ca ở build trước, 1.1.0 có hồi quy ghi nhận | Không chạy lại trọn bộ M4 trên runtime 1.1.5; LOCAL theo layout chưa có |
| G/H/L/N revision cũ/fork/tombstone/lặp | Revision134: 17 ca ở build trước; Core revision vẫn được chạy trong443 | Không có UI giải quyết mọi fork hoặc gắn identity legacy |
| I trùng mã | Import conflict và kiểm tra project/GUID | Không tự hợp nhất hai đối tượng khác nhau trùng mã |
| J đổi mã gói | Alias policy, Segment113 và gom SEG có bằng chứng | Không bao phủ chuẩn hóa mọi mã gói có lịch sử revision |
| M rollback | Transaction, native U/REDO, fault injection trên các luồng test | Không chứng minh chống mất điện/crash native giữa DWG và tệp |
| O legacy | Đọc legacy, baseline có duyệt, gói .bht không đổi | Không tự xem tất cả hồ sơ legacy là verified hoặc cùng dự án |

## Các vấn đề đang mở

1. **11 lỗi kiểm thử ký hiệu legacy**: ghi nhận trong run_signs ở các đợt 1.0.36/1.1.0. Chưa được sửa trong 1.1.5. Khi xử lý, tái chạy baseline và bản sửa trên cùng fixture, giữ tên từng ca thất bại và snapshot; không giảm kỳ vọng chỉ để xanh.
2. **Nạp kế thừa bị đứng theo phản ánh người dùng**: 1.1.2 tách chọn tệp và thêm nhật ký. Chưa có bằng chứng mọi lỗi đứng GUI đã được xử lý. Phân biệt hộp proxy/font/license checkout đang chặn với deadlock/clone chậm của BHT. Không tắt hoặc giết CAD chứa công việc chưa lưu.
3. **D14/D15 trùng OBJ-000001**: đã có bằng chứng hai loại hồ sơ/điểm khác nhau; cần quy trình xử lý danh tính, không bỏ xung đột để ép nhập. Do dữ liệu riêng không có trong repo, phải xin đúng bản sao được phép dùng khi cần tái hiện.
4. **Ảnh Đoạn 11 có10 khóa, file đã lưu có8**: chưa xác minh được hai hồ sơ tăng từ bản DWG mới. Không suy từ ảnh rằng8 hồ sơ trong gói đã sai; không tự mở khóa/đổi đoạn.
5. **Hiệu chỉnh nâng cao**: thiếu luồng đủ an toàn cho biển nhiều mặt/nội dung tùy biến và mốc làm đổi hàng loạt kết quả. Cần thiết kế plan dependency và duyệt rõ ràng.
6. **GLOBAL/LOCAL**: LOCAL theo DWG; nhận survey/assignment vào LOCAL bị chặn trong trường hợp chưa an toàn; chưa có bố trí riêng theo layout hoặc nguồn LOCAL xuất thành GLOBAL.
7. **Bình đồ/phát hành**: cần nghiệm thu đủ15 đoạn, crash recovery giữa tệp và DWG, đối chiếu tệp đã tồn tại; không tự đồng bộ các tệp đóng hoặc bản bàn giao.
8. **Layer**: chỉ tim tham chiếu BHT. Layer cũ không tự xóa; viewport override/freeze bị chặn; không áp quy tắc này cho mọi lớp RTK/biển/nhãn.
9. **Tốc độ**: sửa1.1.0 giảm thao tác lặp, nhưng chưa có benchmark bảo đảm mọi bản vẽ lớn. Đo thời gian clone, scan, rebuild, regen và refresh; chia module không tự làm code nhanh hơn.

## Thứ tự tiếp tục đề nghị

Trước hết xác minh bản cài/fixture và kiểm tra luồng người dùng báo lỗi trong bản sao; tiếp theo xử lý11 lỗi signs với baseline rõ; sau đó nghiệm thu A–O và15 đoạn trên GUI. Chỉ mở rộng sửa biển phức tạp/identity legacy/LOCAL khi các guard và rollback đã được test. Nếu đổi mã, tăng phiên bản sau1.1.5 và viết hướng dẫn mới.
