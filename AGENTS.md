# Quy tắc làm việc với BHT

- Mỗi lần thay đổi chức năng, sửa lỗi hoặc tạo bản phát hành mới phải tăng phiên bản `major.minor.patch`; sửa lỗi tăng `patch` tối thiểu. Một đợt sửa và kiểm tra cùng yêu cầu dùng một phiên bản mới.
- Phiên bản hiện tại lấy từ `VERSION`. Dùng `scripts/set-version.ps1 -Version <phiên-bản-mới>` để đồng bộ Lisp, DLL, bundle và bộ cài; không phát hành mã đã thay đổi dưới số phiên bản cũ.
- Ghi thay đổi vào CHANGELOG và kiểm tra `scripts/check-version.ps1` trước khi build/phát hành. `scripts/package.ps1` kiểm tra phiên bản DLL và chặn tái phát hành cùng số phiên bản nếu nội dung đã đổi, dựa trên RELEASE_STATE.json.
- Chữ CAD mặc định: `VNRomancUpdate.shx`, bảng mã Unicode NFC. Kiểu tùy chỉnh cũ dùng `vnromanc.shx` vẫn chuyển TCVN3 tại lớp hiển thị TEXT; hồ sơ/XRecord/XData, Palette, tìm kiếm và báo cáo giữ Unicode.
- Giữ các sửa đổi và tài liệu trước đó của người dùng. Không sửa tài nguyên TDT gốc.
- Theo yêu cầu hiện hành: phát triển và build cục bộ; không push hoặc đưa bản mới lên GitHub khi chưa được yêu cầu.
- Mỗi tính năng hoàn tất phải có hướng dẫn mới theo phiên bản, nêu thao tác, điều kiện, phục hồi và giới hạn kiểm chứng. Cập nhật HUONG_DAN và mục lục tài liệu; gom hướng dẫn cũ vào old. Chỉ bỏ bản rời sau khi kiểm tra bản lưu và ghi nhật ký.
- Khi dọn dung lượng, giữ mã kiểm thử hồi quy và bằng chứng cần thiết; chỉ xóa sản phẩm kiểm thử có thể tạo lại. Không xóa bản vẽ, hồ sơ khảo sát hoặc tài nguyên nguồn.
