# BHT 1.0.33 - Hiệu chỉnh riêng hồ sơ đã xác nhận

Ngày 15/10/2026. Bộ cài kiểm tra cục bộ cho AutoCAD 2024, .NET Framework 4.8. Phiên bản trước: 1.0.32. Lệnh chính vẫn là **BHT** hoặc **BTH**.

## Cài đặt

Lưu công việc và đóng AutoCAD, chạy `BHT-Setup-1.0.33.exe`. Mở **bản sao** DWG đang làm việc. Giữ DWG/.bht nguồn và bộ cài cũ trong `old`. Kiểm tra Palette hiển thị 1.0.33. Bật Undo ở chế độ All.

Không cần mở khóa cả đoạn. Mở Palette, mở lịch sử và xem trước không tạo phiên bản mới.

Tiêu đề và dòng bảng ở 1.0.33 tự giãn để tránh cắt chữ khi tăng cỡ chữ. Bạn có thể kéo rộng cửa sổ hoặc cuộn bảng để xem hết nội dung. [Hướng dẫn kiểm thử toàn bộ A-O](HUONG_DAN_TEST_TOAN_BO.md) nêu rõ các tính năng đã có và đang chờ milestone tiếp theo.

## Sửa mã biển hoặc thuộc tính

1. Trong **Hồ sơ đối tượng**, chọn hồ sơ đã khóa, bấm **Hiệu chỉnh hồ sơ đã xác nhận…**.
2. Chọn **Thuộc tính khảo sát**. Sửa mã biển, mô tả, số trụ/số mặt, tình trạng hoặc ghi chú. Nhập lý do và người hiệu chỉnh.
3. Bấm **Xem trước / tính riêng** và đối chiếu các dòng Trước/Sau. Đổi số trụ được tính lại nếu ảnh hưởng điểm tính lý trình.
4. Lần đầu sửa hồ sơ cũ, đánh dấu đồng ý lưu bản gốc. Bản gốc mang trạng thái `LEGACY_UNVERIFIED`: lưu nguyên dữ liệu đã có, không chứng nhận lại nguồn gốc.
5. Bấm **Xác nhận hiệu chỉnh**. BHT lưu phiên bản mới, giữ khóa, cập nhật ký hiệu hiện có và giữ vị trí/góc/tỷ lệ ký hiệu đã chỉnh tay.
6. Bấm **Xem lịch sử…** để xem phiên bản, lý do, người sửa và toàn bộ dữ liệu cũ.

Ví dụ `P.102 -> P.103a`: chỉ hồ sơ đang chọn có Survey Revision mới; các hồ sơ khác không phải xác nhận lại. Chưa hỗ trợ đổi mã biển nhiều mặt, nội dung tùy chỉnh hoặc block tùy chỉnh trong cửa sổ này; BHT báo rõ và giữ dữ liệu.

## Đổi liên kết RTK và phân đoạn

Chọn **Liên kết điểm RTK**, nhập mã điểm đã tồn tại, phân cách nhiều mã bằng dấu `;`. BHT không sửa tọa độ đo gốc. Điểm thiếu, trùng mã, hoặc điểm mới đang thuộc hồ sơ khác sẽ bị chặn.

BHT tính lại lý trình, offset, phía tuyến và kiểm tra tim/mốc/phạm vi đoạn. Nếu nằm ngoài đoạn chọn, chọn đúng mã đoạn rồi xem trước lại. Việc chuyển đoạn có ô duyệt riêng; gói lấy từ bảng đoạn. Không đổi riêng tên gói trong cửa sổ này.

**Tính lại lý trình riêng** dùng tim của đoạn hiện hành, không nhập đè lý trình thủ công. **Phân đoạn / gói thầu** kiểm tra lại kết quả theo tim đoạn đích.

## Khi BHT từ chối

- Dữ liệu đổi sau xem trước: xem trước lại; nếu hồ sơ đổi sau lúc mở cửa sổ, đóng rồi mở lại cửa sổ hiệu chỉnh.
- Tim, mốc hoặc điểm khác kết quả xác nhận: đối chiếu nguồn trước, không mở khóa hàng loạt để bỏ cảnh báo.
- Hồ sơ cũ thiếu bằng chứng: giữ nguyên; cần quy trình đối chiếu riêng. Một số cọc Km/H trong D08 thuộc trường hợp này.
- Ký hiệu nằm trên lớp khóa: mở khóa lớp trình bày rồi xem trước lại; không mở khóa dữ liệu khảo sát.
- Đổi liên kết/lý trình cọc Km/H có thể tác động mốc tuyến: chưa hỗ trợ trong đợt này.

## Phục hồi

Đóng cửa sổ trước khi xác nhận không ghi dữ liệu. Sau xác nhận, một lần **U** hoàn tác cả hồ sơ, lịch sử và hình CAD; **REDO** khôi phục lại. BHT tạo nhóm Undo riêng, kết thúc nhóm trước đó trước khi hiệu chỉnh.

Khi lỗi giữa chừng, BHT hoàn tác nhóm riêng và đối chiếu trạng thái. Nếu không xác minh được phục hồi, **không lưu DWG đó**, mở lại bản sao trước thao tác. Journal chưa hoàn tất sẽ chặn hiệu chỉnh, xuất và nạp tiếp; không tự xóa journal/XRecord.

Lưu DWG và xuất `.bht` với **tên mới**. Xuất toàn dự án hoặc xuất đoạn giữ lịch sử hồ sơ tương ứng. `.bht` vẫn dùng cấu trúc ZIP chứa DWG snapshot hiện có.

## Giới hạn

- Có lịch sử theo đối tượng và nhật ký đoạn bị ảnh hưởng; chưa đồng bộ nhiều DWG.
- Nạp tích lũy và nạp chỉ dữ liệu khảo sát bị chặn nếu nguồn hoặc đích có lịch sử mới. Có thể **nạp đầy đủ vào DWG trống**. Hợp nhất theo phiên bản thuộc Milestone 3.
- Chưa có GLOBAL/LOCAL hoặc Display Revision cho MOVE/ROTATE; thuộc Milestone 4. Di chuyển trình bày hiện tại không tự tạo Survey Revision.
- Core Console và kiểm tra giao diện tự động không thay thế nghiệm thu AutoCAD GUI thực tế.
- Muốn trở lại 1.0.31: dùng **bản sao dữ liệu trước hiệu chỉnh**, không chỉnh DWG có lịch sử bằng BHT cũ.

## Bài kiểm tra trên AutoCAD

Trên bản sao, chọn một hồ sơ khóa, ghi/chụp dữ liệu trước khi sửa. Đổi mã biển, xem trước, hủy một lần; thực hiện lại và xác nhận. Kiểm tra mã biển/nhãn, vị trí ký hiệu, lý trình, RTK và một hồ sơ bên cạnh. Xem lịch sử, thử U/REDO, lưu DWG, mở lại. Xuất `.bht` tên mới rồi nạp đầy đủ vào DWG trống. Khi báo lỗi, ghi mã hồ sơ, bản vẽ, thao tác và nguyên văn thông báo.
