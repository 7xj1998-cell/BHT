# BHT 1.0.34 - Hợp nhất theo phiên bản và ngừng sử dụng

Từ 1.0.33, ngày 15/10/2026. Bản kiểm tra cục bộ cho AutoCAD 2024.

Nạp tích lũy dùng project_id, object_guid và chuỗi revision cha: cùng phiên bản bỏ qua, nguồn cũ giữ đích, nguồn kế thừa được nâng cấp; hai nhánh hoặc danh tính chưa rõ bị chặn. Không dùng timestamp để chọn dữ liệu. Kiểm tra cả điểm đo, tim, mốc, đoạn, lịch sử và bố trí trước khi nhập; thất bại khôi phục giao dịch.

Hiệu chỉnh có thêm Ngừng sử dụng: giữ bản ghi khóa, điểm RTK và lịch sử; xóa ký hiệu của đúng đối tượng, loại khỏi báo cáo hiện hành. Nạp cũ và cập nhật ký hiệu không phục hồi đối tượng. Giữ bản sửa cắt chữ 1.0.33.

Xem HUONG_DAN_HIEU_CHINH_1.0.34.md và HUONG_DAN_TEST_TOAN_BO.md. Chưa có GLOBAL/LOCAL, danh sách trạng thái nhiều DWG, giải quyết nhánh tự động hoặc đối chiếu legacy bằng UI. Trường hợp chưa đủ bằng chứng bị chặn, không ghi một phần.
