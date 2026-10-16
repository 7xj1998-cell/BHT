# Phạm vi kiểm chứng bộ cài BHT 1.0.31

Ngày 15/10/2026. Bộ cài kiểm tra cục bộ theo yêu cầu người dùng, không đưa lên GitHub. Giữ phiên bản 1.0.31 của đợt Milestone 1; không thay đổi mã nghiệp vụ trong bước đóng gói. Chưa nghiệm thu AutoCAD GUI với hồ sơ thực tế.

## Bản runtime dùng để đóng gói

FAS được biên dịch từ 12 module AutoLISP hiện hành. Core/Bridge được làm rối tên nội bộ bằng Obfuscar 2.2.50, Palette giữ nguyên. Không đóng gói source nghiệp vụ, PDB hoặc bản đồ tên nội bộ. Bộ cài có thư viện biển/đèn và phông, dùng được như một file EXE.

Các kiểm tra đã chạy lại trên FAS/DLL bảo vệ trước khi đóng gói:

- CoreTests: 378 PASS, 0 FAIL.
- Incremental131: 31 Lisp/Undo, 29 .NET/UI và mở lại DWG đạt; so sánh dữ liệu trước/sau của 25 hồ sơ khóa khi bổ sung.
- Progress125: 13 Lisp, 27 .NET, 7 stage cùng kiểm tra nhãn đạt.
- PaletteLayout: 3.062 kiểm tra đạt trong Core Console.
- Thư viện đèn và đặt/hướng đèn bằng Lisp đạt.
- Bộ kiểm tra an toàn installer: 7 ca đạt, gồm file thay đổi/thiếu/thừa, đường dẫn ngoài gói, backup và rollback khi thay thế lỗi; chạy trong fixture riêng.

## Giới hạn còn tồn tại

`run_signs.ps1` mặc định dừng tại BHTSIGNPROBE. Khi dùng thư viện đi kèm, bộ rộng vẫn có ca thất bại về thứ tự vẽ/kỳ vọng ký hiệu. Tài liệu lịch sử 1.0.14 đã ghi các nhóm lỗi tương tự; không tính bộ rộng là PASS và chưa kết luận mọi thất bại hiện tại là cùng nguyên nhân. Kiểm tra runtime độc lập với biển mẫu và kiểm thử bổ sung không thay thế kiểm tra toàn bộ thư viện.

Chưa có hiệu chỉnh riêng hồ sơ khóa, revision ancestry, GLOBAL/LOCAL hay đồng bộ nhiều bình đồ. Chưa cài đè BHT đang dùng trên máy trong quá trình đóng gói. Người dùng cài EXE rồi kiểm tra trên bản sao DWG; xem [hướng dẫn 1.0.31](RELEASE_NOTES_1.0.31.md).

## Nhật ký xác minh

Log biên dịch, kiểm thử runtime, EXE và SHA256 được giữ ngoài gói phát hành tại `D:/lisp/BHT/audit-1.0.31/installer`. Sau khi tạo EXE, bước xác minh dùng `--validate-only`, kiểm tra payload từ đường dẫn có dấu/khoảng trắng, phát hiện payload sửa đổi và xác nhận không thay bundle đang cài. Bước runtime độc lập nạp FAS ở thư mục riêng, tạo biển mẫu rồi lưu/mở lại DWG.

Kết quả cuối của các bước sau đóng gói nằm ở `D:/lisp/BHT/audit-1.0.31/installer/DELIVERY.md` và chỉ mục `D:/lisp/BHT/BHT-HIEN-TAI.md`; không sửa nội dung gói đã phát hành dưới cùng số phiên bản. Báo cáo kỹ thuật đầy đủ của Milestone 1 nằm trong source/docs/BAO_CAO_MILESTONE_1_1.0.31.md.
