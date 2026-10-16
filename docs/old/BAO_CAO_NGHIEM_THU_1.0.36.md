# Báo cáo kiểm tra BHT 1.0.36

Ngày 15/10/2026, nhánh development/incremental-confirmation; Git root D:/lisp/BHT/sharing-0654/source. Bản cài trên máy trước công việc là 1.0.32; các mốc độc lập đã build: 1.0.33 UI, 1.0.34 merge/tombstone, 1.0.35 trình bày, 1.0.36 bình đồ/phát hành. Giữ dirty worktree; không commit/push GitHub.

## Vấn đề và thay đổi

1. Chữ bị đè: tiêu đề DataGridView có chiều cao cố định. ObjectCorrectionForm.Grid và ImportConflictForm dùng AutoSize, wrap, padding. Trước sửa 30/101 bài đo chữ lỗi, sau sửa 101 đạt.
2. Nạp cũ: trạng thái khóa không cho biết tổ tiên. RevisionMerge.Decide/Chain kiểm tra project/GUID/cha/snapshot, RevisionTransfer dùng cùng quyết định ở preflight và writer riêng. Giữ đích mới hơn, chặn fork/identity sai, không dùng timestamp. Tombstone giữ RTK/lịch sử, bỏ ký hiệu và báo cáo, chặn phục hồi qua gói cũ.
3. Trình bày: ObjectRevision.Next tách display counter; ObjectCorrection lưu GLOBAL sau khi thao tác CAD hoàn tất. DrawingPresentation lưu VIEW_LOCAL/LOCAL_HISTORY. Giữ hình LOCAL khi nhận revision trình bày chung; bố trí tay chưa lưu gây xung đột.
4. Bình đồ: DrawingProject đăng ký identity/state, kiểm tra DWG liên quan chỉ đọc, fingerprint preview, tạo bản làm việc mới và Release Rxx. TabDrawingProject tích hợp trong Palette. Guard C#/Lisp chặn ghi dữ liệu và các đường cập nhật ký hiệu chính trên RELEASED/READ_ONLY_ARCHIVE. Không tự sửa tệp nguồn hoặc DWG liên quan.

## Bằng chứng

| Lớp kiểm tra | Kết quả bản 1.0.36 |
|---|---|
| Unit CoreTests | 428 PASS, 0 FAIL |
| Core Console, FAS/DLL protected - M1 | 31 Lisp + 29 .NET, mở lại DWG đạt |
| Core Console - hiệu chỉnh | 50 PASS, gồm fault/rollback/U/REDO |
| Core Console - merge/tombstone | 17 PASS |
| Core Console - GLOBAL/LOCAL | 21 PASS, cả nhãn/đường dẫn sửa tay |
| Core Console - bình đồ/phát hành | 20 PASS, R01 giữ nguyên khi tạo R02 |
| Core Console - D10 từ New | 11 PASS, 178 OBJ khác và 801 điểm giữ nguyên |
| WinForms tự động | 101 đo bảng + 21 UI, Palette layout 3097 checks đạt |
| AutoCAD GUI thực tế/DPI | Cần người dùng nghiệm thu; không suy từ Core Console |

Bằng chứng ở D:/lisp/BHT/audit-1.0.33 đến audit-1.0.36 và build/v1.0.36. Kiểm tra dùng snapshot canonical bản ghi, tọa độ và hình CAD; lỗi giữa giao dịch đã khôi phục. Test runner mới sao chép DWG vào output rồi đối chiếu SHA256 trước khi mở.

Một lỗi quan trọng được phát hiện khi thử: Wblock không sao chép NOD dự án, nên bản preview có thể thiếu dữ liệu. Đã thay bằng snapshot SaveAs(false) và ReadDwgFile; test riêng xác minh tên tệp, DBMOD, dữ liệu và toàn bộ entget của bản hiện hành không đổi. Các lỗi trung gian giữ log, không đổi test để che lỗi. Test số loại hiệu chỉnh cập nhật từ 4 sang 6 vì có hai tính năng mới, đồng thời kiểm tra chúng không cho nhập dữ liệu khảo sát ngoài phạm vi.

## Giới hạn và rủi ro còn lại

- Chưa nghiệm thu tất cả màn hình/DPI và công trình đủ 15 đoạn bằng GUI. Không tuyên bố toàn bộ Milestone 2-5 hoàn tất.
- Legacy vẫn đọc được, baseline chỉ ghi khi duyệt; chưa có UI gắn danh tính giữa hai hồ sơ cũ. Khác project/GUID hoặc fork bị chặn, chưa giải quyết nhánh tự động.
- Biển nhiều mặt/nội dung tùy chỉnh, sửa mốc Km/H ảnh hưởng tuyến chưa hỗ trợ trong hiệu chỉnh. Chuẩn hóa gói có lịch sử cần quy trình riêng.
- LOCAL thuộc DWG, chưa theo layout; chưa xuất thành nguồn GLOBAL; nhận thay đổi khảo sát/phân đoạn vào LOCAL bị chặn. Chuyển về GLOBAL cần hình đã khớp. Một số loại CAD ngoài danh sách hỗ trợ bị từ chối.
- Kiểm tra bình đồ liên quan dùng DWG đã lưu, không lấy chỉnh sửa chưa lưu trong cửa sổ CAD khác. Trạng thái BHT cần lưu DWG; lệnh CAD gốc vẫn hoạt động.
- Phát hành dùng transaction DWG và tệp mới riêng. Lỗi thông thường bỏ tệp vừa tạo và phục hồi; mất điện/ngắt tiến trình có thể để lại Rxx chưa được ghi lịch sử. Không ghi đè; đối chiếu SHA256 và lưu lại bằng chứng trước khi tiếp tục. Chưa có kiểm thử crash giữa hai kho.

Tiếp theo cần nghiệm thu theo HUONG_DAN_TEST_TOAN_BO, ưu tiên ảnh chụp bảng ở DPI máy người dùng, cập nhật hai DWG A/B, rồi mở rộng quy trình legacy, LOCAL và biển phức tạp. Tài liệu cũ lưu trong docs/old, nhật ký hash ở audit tương ứng.
