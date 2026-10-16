# 6. Prompt giao cho AI tiếp nhận

Bạn tiếp nhận BHT1.1.5, phần mềm khảo sát tuyến trên AutoCAD2024, C#/.NET Framework4.8 và AutoLISP. Hãy đọc START_HERE_AI.md, handoff/1.1.5/BHT_AI_HANDOFF_1.1.5.md, AGENTS.md và mã thực tế trước khi sửa.

Mục tiêu: tiếp tục theo “một dự án logic — đối tượng có phiên bản — nhiều bình đồ”, ưu tiên tính toàn vẹn dữ liệu. Không viết lại BHT, không bỏ guard, không dùng thời gian sửa làm thẩm quyền. Giữ RTK, hình học tim, mốc, hồ sơ khóa ngoài phạm vi và bản phát hành. Mọi test thực địa dùng bản sao.

Bắt đầu bằng xác minh Git root/branch/status, VERSION và build đang cài; lập bản đồ chức năng liên quan yêu cầu mới. Đọc báo cáo hiện hành và lỗi chưa xử lý. Tạo test tái hiện, sửa mã, build và kiểm thử phù hợp. So sánh canonical trước/sau thay vì chỉ thông báo thành công. Báo bằng tiếng Việt: phiên bản, nguyên nhân, file/hàm, trước/sau, test PASS/FAIL/BLOCKED, giới hạn và hướng dẫn phục hồi.

Các phần M1–M5 có nền tảng triển khai, chưa nghiệm thu trọn lộ trình. Có11 lỗi ký hiệu legacy chưa sửa và GUI15đoạn chưa nghiệm thu. Không tuyên bố tất cả đã hoàn tất hoặc dùng CoreTests thay kiểm thử AutoCAD GUI. Không tự mở khóa cả đoạn, tự hợp nhất identity legacy hoặc tự ghi đè bình đồ bàn giao.

Bản công khai nguồn là releases/source-1.1.5, main hiện là nền lịch sử. Chỉ dẫn “không push” trong báo cáo cũ mô tả phiên trước; công bố1.1.5 đã được người dùng yêu cầu. Sau này không tự push/phát hành thay đổi mới nếu không có yêu cầu tương ứng. Khi đổi tính năng, dùng set-version và hướng dẫn mới. Không tái phát hành mã khác dưới1.1.5.

Trước khi thực hiện công việc mới, nêu ngắn gọn phần đã xác minh và phần thiếu bằng chứng. Nếu lỗi liên quan dữ liệu riêng không có trong repo, dùng fixture giả lập cho phần độc lập; chỉ yêu cầu bản sao cần thiết, không suy đoán dữ liệu nguồn. Tiếp tục thực thi phần đã được yêu cầu, không chỉ viết đề xuất.
