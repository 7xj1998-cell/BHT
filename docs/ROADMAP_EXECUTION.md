# Tiến độ thực hiện lộ trình

Ngày 15/10/2026. Bản kiểm tra hiện hành 1.1.0; xem BAO_CAO_NGHIEM_THU_1.1.0.md cho kết quả mới và 11 lỗi baseline chưa giải quyết. M1-M5 đã chạy hồi quy bằng runtime 1.1.0; GUI/công trình đầy đủ chưa nghiệm thu. Yêu cầu tiếp tục M2-M5, build cục bộ, giữ dữ liệu gốc, hướng dẫn test toàn bộ.

| Mốc | Phiên bản | Kết quả đã kiểm chứng | Việc còn lại |
|---|---|---|---|
| M1 | 1.0.31, hồi quy 1.0.36 | Bổ sung 25+1, context tim/mốc, no-op, alias gói, rollback, mở lại DWG | Nghiệm thu công trình thật trên bản sao |
| M2 và sửa bảng | 1.0.32 -> 1.0.33 | Hiệu chỉnh từng OBJ, lịch sử, 50 regression, 11 D10; bảng từ 30 lỗi thành 101 đạt | Biển phức tạp, mốc Km/H phụ thuộc, GUI/DPI |
| M3 | 1.0.34 | 17 merge/tombstone; tổ tiên, fork, rollback, nhập lặp; 428 unit | UI đối chiếu legacy, duyệt nhánh và đổi gói có lịch sử |
| M4 | 1.0.35 | 21 GLOBAL/LOCAL, tách Display Revision, nhãn/đường dẫn sửa tay, lưu/mở lại | LOCAL theo layout, nguồn LOCAL và khảo sát vào LOCAL |
| M5 | 1.0.36 | 20 bình đồ/phát hành; preview snapshot, liên kết chỉ đọc, R01/R02 và guard | Nghiệm thu 15 đoạn; phục hồi crash tệp/DWG; không tự đồng bộ tệp đóng |

Các bản trên là build kiểm tra cục bộ, không tuyên bố toàn bộ lộ trình đã nghiệm thu ổn định. M3 mở rolling có lịch sử qua đường riêng xác minh ancestry; vẫn chặn các trường hợp chưa chứng minh an toàn. Bản gốc và GitHub giữ nguyên. Trạng thái bài A-O, mẫu test và các bước nghiệm thu: HUONG_DAN_TEST_TOAN_BO.md. Báo cáo hiện hành: BAO_CAO_NGHIEM_THU_1.1.0.md; báo cáo mốc trước ở old/BAO_CAO_NGHIEM_THU_1.0.36.md.
