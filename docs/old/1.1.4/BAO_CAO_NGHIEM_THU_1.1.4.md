# Kiểm chứng BHT 1.1.4

Ngày 16/10/2026. Phiên bản trước 1.1.3, sau 1.1.4, nhánh development/incremental-confirmation. Build cục bộ, không push. Baseline và các sửa có sẵn được giữ ở D:/lisp/BHT/audit-1.1.4/baseline; không ghi lên New.

## Nguyên nhân và sửa đổi

- `06-routes.lsp`: curve-project lấy điểm gần nhất hữu hạn; route-project trước đây không phát hiện đầu mút bị kẹp. Thêm route-projection-in-domain kiểm tra tiếp tuyến XY tại đầu/cuối tim mở; 1e-6 m chỉ là ngưỡng số học. Giữ curve-project phục vụ chọn điểm/hiển thị. Sửa bht:station tiếp tục tìm khi best chưa có offset, cập nhật thông báo station-route.
- `07-segments.lsp`: progress-plan trước đây bỏ kiểm tra khoảng/phía nếu gan_doan_pp=THU_CONG. Luôn kiểm tra phạm vi/phía trước khi đưa vào updates; progress-finish tái lập plan nên chặn xác nhận sai. Giữ per-object guard, khóa và rollback.
- `ObjectCorrectionForm.cs`: giải thích thiếu preview; chỉ hiện chấp thuận theo plan, confirm chỉ bật sau các duyệt bắt buộc. Sửa giá trị/lý do làm mất hiệu lực plan và xóa chấp thuận. Backend lịch sử/quyền ghi không đổi.

## Kết quả thực chạy

| Lớp | Kết quả |
| --- | --- |
| Baseline AutoCAD 1.1.3 | 9/16 ca lỗi được tái hiện, gồm ba biển ngoài cuối cùng được tính/xác nhận |
| CoreTests | 443 PASS, 0 FAIL |
| FAS/DLL bảo vệ - scope114 | 28 PASS, 0 FAIL; mở lại DWG canonical khớp |
| Incremental131 | 31 Lisp/Undo + 29 .NET đạt; reopen no-op đạt |
| Correction132 | 50 đạt, gồm rollback dữ liệu/lịch sử/CAD, Undo/Redo và .bht |
| Correction114Ui | 9 đạt trên WinForms trong Core Console |
| CorrectionGrid133 | 101 đạt ở mức chữ 100/125/150/200% |
| Regression110 | 10 đạt |
| Progress125 | 13 Lisp + 27 .NET + 7 stage đạt và nhãn kế thừa được giữ |
| DOAN15 thực tế | 179 OBJ, 801 RTK, 108 khóa, 8 khóa D11; đọc đối chiếu không thay canonical |

Evidence: audit-1.1.4/{baseline-scope,scope-protected,correction-protected,grid-protected,ui-conditions-protected,regression-protected,real-audit}; M1/Progress125 tại source/build/v1.1.4. Lần scope-undo đầu có 2 FAIL do nhóm Undo tạo fixture chưa đóng: runner đã đóng nhóm trước lệnh thử, không sửa assertion, chạy lại source và protected đạt; giữ log thất bại. Không che lỗi baseline.

SHA256 DOAN15 nguồn: 7D510BF3B9C37350CC770E446DE61B42124676CFE0F10819A3D19A60BC7FFFDC. Thời điểm sửa nguồn 09:28:01 không đổi. Tám hồ sơ D11 tính đối chiếu khớp lý trình lưu. Các cọc Km/H dùng số nhập, không đánh đồng số chiếu hình học với số trên cọc.

## Giới hạn, rủi ro và tiếp tục

Chưa có bản DWG chứa 10 hồ sơ khóa D11 như ảnh nên chưa xác định được hai hồ sơ tăng thêm. Chưa nghiệm thu chuột trong AutoCAD GUI hoặc sửa dữ liệu đã khóa sai trên bản vẽ đang làm. Không tự sửa hồ sơ khóa: cần đúng bản sao, xem trước và hiệu chỉnh riêng có lịch sử. Kiểm tra phạm vi không giải quyết sai tim/điểm nhưng vẫn nằm trong phạm vi; cần đối chiếu hiện trường. Lỗi ký hiệu legacy 11 FAIL vẫn là giới hạn từ 1.1.0; không tuyên bố đã sửa. Bản sửa chỉ xử lý lỗi này, không nghiệm thu toàn bộ M2-5.

Hướng tiếp: đối chiếu bản đang làm có 10 khóa, xác định ID/điểm/tim trước khi đề xuất chuyển đoạn; thử thao tác bằng chuột với các tình huống trong hướng dẫn 1.1.4. Không tự đổi RTK, tim, mốc hay gói, không mở khóa cả đoạn. Có thể Undo ngay trên bản thử hoặc phục hồi bản sao trước thao tác.
