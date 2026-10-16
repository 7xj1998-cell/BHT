# Mục lục bộ bàn giao BHT 1.1.5

Ngày tổng hợp: 16/10/2026. Mục đích: để AI hoặc người phát triển khác hiểu dự án và tiếp tục làm việc mà không phải suy từ các đoạn hội thoại rời.

1. `BHT_AI_HANDOFF_1.1.5.md`: toàn bộ nội dung chính trong một tệp, phù hợp gửi trực tiếp cho AI.
2. `01_TRANG_THAI.md`: build đang làm được gì, phạm vi công bố và điều chưa nghiệm thu.
3. `02_LICH_SU.md`: các giai đoạn từ nền Lisp đến kiến trúc có lịch sử, cùng các sửa lỗi 1.1.x.
4. `03_KIEN_TRUC.md`: quá trình khởi động, kho dữ liệu, bản đồ xử lý và các điều kiện bảo vệ.
5. `04_KIEM_THU_VA_TON_TAI.md`: bằng chứng theo phiên bản, rủi ro và ưu tiên tiếp tục.
6. `05_BUILD_VA_TIEP_TUC.md`: cách build, test, đóng gói và bàn giao tiếp.
7. `06_PROMPT_AI.md`: yêu cầu khởi đầu cho AI tiếp nhận.
8. `07_CHI_MUC_PHIEN_BAN.md`: chỉ mục trích từ CHANGELOG; ghi rõ mục thiếu/trùng, không tự bịa phiên bản.
9. `SOURCE_MANIFEST.json`, `BUILD_MANIFEST.json`: SHA256 nguồn và bộ cài, phạm vi công bố.
10. `evidence/`: kết quả chọn lọc từ kiểm thử build, không chứa DWG/.bht khảo sát.

Trong gói ZIP nhỏ có thêm CHANGELOG đầy đủ, hợp đồng dữ liệu, hướng dẫn kiểm thử A–O và báo cáo 1.1.5. Gói mã nguồn riêng chứa C#/Lisp, script, test và tài liệu lịch sử. Không gửi token, hồ sơ công trình, ảnh hiện trường hay DLL Autodesk cho AI.

Phạm vi nguồn chứng cứ: mã nguồn thực tế, CHANGELOG, tài liệu lịch sử còn lưu và log kiểm thử cục bộ. Lịch sử Git công khai không chứa mỗi build từ ban đầu; không coi chỉ mục phiên bản là chuỗi commit tương ứng. Phần trước 0.4.0 chỉ có mô tả nền, chưa đủ dữ liệu khôi phục mọi quyết định hoặc ngày khởi tạo.
