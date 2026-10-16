# Báo cáo kiểm tra 1.0.35

Ngày 15/10/2026, 1.0.34 -> 1.0.35. Nguyên nhân cần sửa: metadata trình bày cũ chưa có lịch sử; merge có thể ghi đè bố trí riêng. ObjectRevision.Next tách display counter; ObjectCorrection lưu pose qua preview bất biến và native Undo. DrawingPresentation lưu VIEW_LOCAL/LOCAL_HISTORY; RevisionTransfer giữ hình LOCAL, kiểm tra thay đổi tay và chỉ cập nhật đúng OBJ. Palette tích hợp trong tab hiện có; không event listener tạo revision liên tục.

428 CoreTests đạt. FAS/DLL protected: 21 bài Presentation135Probe đạt, 17 bài Revision134Probe đạt, 50 bài correction hồi quy đạt. Chứng minh RTK/tim/lý trình giữ nguyên, đổi vị trí/rotation/nhãn/đường dẫn truyền đúng, LOCAL không bị ghi đè, nhập lặp không trùng, preview lỗi thời bị chặn, U/REDO và mở lại DWG giữ dữ liệu. Bằng chứng D:/lisp/BHT/audit-1.0.35. Một lỗi C# ref parameter trong lambda được phát hiện ở build đầu và sửa trước kiểm thử.

Chưa nghiệm thu GUI/DPI thực tế. LOCAL là phạm vi DWG, chưa tách từng layout; không xuất nguồn LOCAL thành GLOBAL; chuyển LOCAL về GLOBAL cần hình đã khớp. LOCAL gặp thay đổi khảo sát/phân đoạn bị chặn. Các kiểu hình trình bày ngoài block, DBText, Line, Polyline và AlignedDimension cần quy trình riêng. Chưa hoàn tất quản lý nhiều bình đồ/Milestone 5. Giữ dữ liệu cũ, không thay ZIP, không push GitHub.
