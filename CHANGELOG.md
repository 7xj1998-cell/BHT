## 1.1.5 - 2026-10-16, layer tim theo tên tuyến

Đặt tim tham chiếu trên BHT_TUYEN_<mã tuyến>, giữ cô lập block/style khi clone; dùng mã cuối sau xử lý trùng tuyến. Thêm nút Đặt layer theo tên tuyến trong Palette với đối chiếu trước khi ghi. Giao dịch nguyên tử, Undo; giữ hồ sơ/RTK/hình học/XData và layer gốc. Chặn layer khóa, thuộc tính xung đột và thiết lập viewport; không tự thêm hậu tố. Tương thích Line trong hồ sơ cũ.

443 Core; layer 4 Lisp + 18 .NET + Undo; Progress125 13+28+7/nhãn; M2 50 đạt với runtime bảo vệ. M1 31+29/reopen, phạm vi28/reopen và layout3142 đạt trong đợt. DOAN15: 9 tim chuyển layer, canonical dữ liệu và hash nguồn giữ nguyên. Chưa nghiệm thu GUI chuột; 11 lỗi signs legacy còn tồn tại. Hướng dẫn mới HUONG_DAN_HIEU_CHINH_1.1.5; tài liệu 1.1.4 lưu old có hash. Không push.

## 1.1.4 - 2026-10-16, chặn lý trình giả ở đầu/cuối tim

Tái hiện 1.1.3 nhận điểm vượt đầu/cuối tim thành lý trình đầu mút và cho phép xác nhận; hồ sơ gán tay còn bỏ qua khoảng lý trình/phía đường. route-project mới kiểm tra tiếp tuyến đầu mút tim mở; giữ điểm đúng mặt cắt ranh, tim cong, đảo chiều và tim kín. Tính/xác nhận bổ sung luôn kiểm tra phạm vi/phía, kể cả gán tay. Sửa tìm tim tiếp theo khi tim đầu không có offset. Không tự sửa hồ sơ khóa hoặc RTK.

Bảng hiệu chỉnh ẩn mục duyệt chưa liên quan, giải thích điều kiện còn thiếu, chỉ bật xác nhận sau preview và duyệt cần thiết. 443 Core; 28 phạm vi/Undo + reopen; M1 31+29/reopen; M2 50; UI9; grid101; hồi quy10; Progress125 13+27+7 và nhãn kế thừa đạt với FAS/DLL bảo vệ. DOAN15 nguồn giữ hash, 179 OBJ/801 RTK/108 khóa/8 khóa D11; chưa có trạng thái 10 khóa trong ảnh. Chưa nghiệm thu chuột trong CAD. Hướng dẫn mới 1.1.4, cũ ở old/1.1.3 có hash. Không push.

## 1.1.3 - 2026-10-16, gom đoạn trùng có đối chiếu

Đã thêm Kiểm tra / gom đoạn trùng trong bảng hiện có. Chỉ gom SEG không tham chiếu khi cùng phạm vi/phía/tên gói/cách viết mã, giữ tim, lưu snapshot đầy đủ vào SEG_ALIAS chỉ thêm, transaction và Undo. Giữ nguyên OBJ/RTK/ROUTE/CAD và hồ sơ khóa; không tự đổi mã hoặc tạo revision. Chặn mã segment khác cách viết trong nạp kế thừa/nạp khảo sát và thay bảng trước khi ghi.

Tái hiện 1.1.2 nạp DOAN10 vào DOAN9 làm 15 → 30 SEG. 1.1.3 gom bản sao DOAN15 về 15, giữ 179 OBJ/801 RTK/108 khóa; DOAN08/GOI03 còn 15 khóa nên giữ mã. DOAN11/12/13 có ứng viên dùng cả hai mã bị chặn toàn bộ. Chưa thống nhất toàn bộ mã gói. 443 Core; 78+2 SEG/Undo; 16 import; 33 picker; 316 bảng; M1 31+29/reopen; M2 50; merge17; project20 đạt. GUI và lỗi đứng/proxy chưa nghiệm thu từ các kiểm tra này. Hướng dẫn mới 1.1.3, tài liệu 1.1.2 lưu old có hash. Không push.

## 1.1.2 - 2026-10-16, tách bước chọn tệp kế thừa

Thêm bảng chọn hồ sơ kế thừa: chọn/dán đường dẫn rồi bấm Kiểm tra và nạp; chưa đọc gói hoặc ghi DWG trong lúc chọn tệp. Dùng cửa sổ Windows không xem trước, giữ dòng lệnh/script FILEDIA=0. Nhật ký bắt đầu trước chọn tệp, ghi đóng/mở bảng, làm mới Palette và lỗi chi tiết. Sau lệnh nạp .NET, Palette không gọi Lisp DRAIN, chỉ làm mới khi lệnh đã kết thúc. Giữ chính sách xung đột/revision, giao dịch, RTK, tim và ZIP .bht.

428 Core, 33 chọn tệp/lỗi/hủy/bảng, 16 import, 2 lệnh/FAS, 316 bảng, 3.102 Palette, M1 31+29/reopen và M2 50 đạt. GUI 1.1.1 không đứng khi dùng đường dẫn script; chưa xác định nguyên nhân gốc cửa sổ chọn tệp của người dùng, chưa tuyên bố sửa triệt để. Hướng dẫn mới 1.1.2; hướng dẫn 1.1.1 giữ nguyên trong docs/old/1.1.1. Build cục bộ, không push.

## 1.1.1 - 2026-10-16, bảng phân đoạn và chẩn đoán nạp dữ liệu

Đã thiết kế lại bảng đoạn/tim và quản lý bình đồ với chữ lớn, tương phản rõ, header xuống dòng, khung chi tiết và nút đủ chỗ. Bảng xung đột mở modeless sau khi lệnh kết thúc. Nạp tích luỹ có bước xử lý và nhật ký cục bộ; dispose bộ đệm XRecord.

Đối chiếu 7 gói New: đúng cấu trúc/SHA256, mỗi gói 179 OBJ/801 RTK, kiểm tra điểm/tim xác nhận đạt. D14/D15 có hai hồ sơ khác nhau trùng OBJ-000001; giữ chặn 6 trường, không đổi nguồn/đích. 428 unit; 316 bảng; 28 import + 2 lệnh/FAS; M1 31+29/reopen; M2 50; merge17; presentation21; project20; correctiongrid101; Palette3102; regression10 đạt. Chưa tái hiện/xác định triệt để lỗi đứng GUI; không tuyên bố đã xử lý hết. 11 lỗi signs và giới hạn 1.1.0 còn được ghi nhận. Hướng dẫn mới kèm ảnh; tài liệu cũ lưu docs/old/1.1.0. Build cục bộ, không push GitHub.

## 1.1.0 - 2026-10-15, bản kiểm tra báo cáo và bố trí tuyến

Báo cáo giữ đủ dòng, kéo/đổi kích thước, điều chỉnh chữ và không chặn CAD; xem trước tính đoạn áp dụng bằng token kiểm tra lại dữ liệu. Chặn điểm đầu xa tim/chiều đi ra ngoài và đổi cơ sở mốc. Nhãn biển chọn hướng theo chiều tăng Km, độc lập UCS/VIEW; ghi góc tham chiếu để giữ nhãn xoay tay. Giảm quét lặp, sửa hội tụ bố trí D13 và giới hạn cọc theo tim/phạm vi, giữ khoảng lùi khi chạy lặp, đối chiếu rollback và báo nguyên nhân.

428 unit; FAS/protected: 10 regression, 269 nhãn, 10 D13 + reopen canonical; M1 31 Lisp + 29 .NET; M2 50 + D10 11; M3 17; M4 21; M5 20; UI 23 + 101 + 3102, tỷ lệ API 9, thư viện đèn đạt. Bộ signs cũ còn 11 FAIL giống baseline 1.0.36, giữ nguyên bằng chứng; chưa nghiệm thu GUI/giấy phép AutoCAD. Bàn giao bộ cài để kiểm tra cục bộ, không coi toàn bộ roadmap ổn định. Hướng dẫn 1.1.0 có bài P-T và A-O, tài liệu cũ ở docs/old. Không push GitHub.

## 1.0.36 - 2026-10-15, quản lý bình đồ và phát hành

Thêm đăng ký danh tính bình đồ, liên kết DWG cùng dự án, kiểm tra UP_TO_DATE/UPDATE_AVAILABLE/CONFLICT, xem trước chính importer trên snapshot DWG riêng. Đồng bộ cần nguồn/đích không đổi sau preview. Thêm RELEASED/READ_ONLY_ARCHIVE, bản làm việc mới giữ ancestry, phát hành đoạn R01/R02 không ghi đè và lịch sử hash. Bảng xung đột tự giãn tiêu đề. 428 unit; protected: 20 project, 21 display, 17 merge, 50 correction, 11 D10, 101 grid; Palette layout 3097 checks; incremental 31 Lisp + 29 .NET + mở lại DWG đạt. Chưa nghiệm thu GUI/DPI đầy đủ, chưa đồng bộ trực tiếp tệp đóng hoặc đối chiếu legacy tự động.

## 1.0.35 - 2026-10-15, bố trí GLOBAL/LOCAL

Lưu bố trí rõ ràng sau MOVE/ROTATE, nhãn và đường dẫn; GLOBAL tăng Display Revision riêng, không sửa RTK/lý trình. LOCAL có lịch sử riêng trong DWG, nhận revision chung nhưng giữ hình riêng. Chặn bố trí tay chưa lưu và nguồn LOCAL phát tán thành GLOBAL. 21 bài trình bày protected, 17 merge và 50 correction hồi quy đạt; 428 CoreTests. Chưa tự ghi DWG khác hoặc quản lý phát hành theo đoạn.

## 1.0.34 - 2026-10-15, hợp nhất revision và ngừng sử dụng

Thêm hợp nhất theo project/GUID và chuỗi cha, giữ đích mới hơn, nhập lặp không đổi, báo xung đột nhánh. Luồng riêng ghi hồ sơ khóa trong transaction, giữ lịch sử và kiểm tra RTK/tim/đoạn. Bảo vệ bố trí thủ công bằng đối chiếu trước/sau. Ngừng sử dụng có revision, giữ RTK/lịch sử, loại ký hiệu và báo cáo, chặn gói cũ phục hồi. 428 unit, 17 merge/tombstone protected, 11 D10 protected, 50 correction và 101 grid đạt. Chưa có GLOBAL/LOCAL hay quản lý nhiều DWG.

## 1.0.32 - 2026-10-15, kiểm tra cục bộ Milestone 2

Thêm hiệu chỉnh từng hồ sơ khóa trong Palette: bốn loại thay đổi, bảng trước/sau, lý do, người sửa, duyệt baseline và chuyển đoạn. Lưu project/object identity, revision cha, snapshot SHA256 và lịch sử chỉ thêm trong DWG; không thay ZIP .bht. Tính riêng khi đổi RTK/số trụ/đoạn, giữ dữ liệu đo gốc và hồ sơ khác.

Dùng nhóm Undo riêng gồm journal, dữ liệu, lịch sử và ký hiệu; lỗi hoàn tác và đối chiếu snapshot. Chặn mở lại đoạn có lịch sử, ghi ngoài quy trình và rolling/survey-only có lịch sử; full import/export giữ phiên bản. Chưa sửa mã biển nhiều mặt/nội dung tùy chỉnh, mốc Km/H ảnh hưởng tuyến, chưa merge theo ancestry hay GLOBAL/LOCAL. Hướng dẫn mới HUONG_DAN_HIEU_CHINH_1.0.32.md. Bộ cài kiểm tra trên bản sao, chưa nghiệm thu GUI. Không push GitHub.
## 1.0.31 - 2026-10-15, build cục bộ Milestone 1

Cho phép tính/xác nhận bổ sung trong đoạn có hồ sơ khóa; chỉ cập nhật hồ sơ chưa khóa, không tính lại kết quả cũ. Chặn phương án xem trước lỗi thời, kiểm tra điểm/tim/mốc/ngữ cảnh xác nhận và trả về không thay đổi khi không có hồ sơ cần ghi. Bổ sung bằng chứng ngữ cảnh vào XRecord mới, không tự nâng cấp hồ sơ cũ.

Thống nhất chính sách so sánh trước nạp và thực thi; chuẩn hoá GOI01/GOI_01 có điều kiện, giữ bảo vệ phân đoạn và dùng lại tim đích đã đối chiếu. Sửa rollback khi Undo của side database tắt, xử lý danh sách clone rỗng và từ chối snapshot số không hữu hạn. Thêm kiểm thử 25+1, lỗi giữa giao dịch, Undo, nhập lặp và lưu/mở lại DWG. Chưa có hiệu chỉnh từng hồ sơ hay revision ancestry. Đóng gói bộ cài FAS/DLL 1.0.31 để người dùng kiểm tra trên AutoCAD; chưa push GitHub, chưa nghiệm thu GUI thực tế. Hướng dẫn và báo cáo mới tại docs/RELEASE_NOTES_1.0.31.md và docs/BAO_CAO_MILESTONE_1_1.0.31.md.

## 1.0.30 - đang phát triển

Công bố mã nguồn BHT theo MIT tại https://github.com/7xj1998-cell/BHT. Sửa phiên bản NuGet Microsoft.NETFramework.ReferenceAssemblies thành 1.0.3; giữ nguyên phiên bản phụ thuộc khi tăng phiên bản BHT và kiểm tra ràng buộc này trước build. Sửa nhãn mới được yêu cầu chưa hoàn tất; chưa phát hành bộ cài 1.0.30.
## 1.0.29 - 2026-10-14

Sửa bad function: 1.51086 do biến angle che hàm AutoLISP trong bố trí hàng loạt. Nhãn báo hiệu theo đoạn dẫn dài: mã/biển phụ trên, lý trình dưới; hai trụ dùng đường dẫn phía trên. Thêm BHTMAUNHAN để áp dụng cho nhãn kéo tay mà giữ hình biển. Đối chiếu sign_content theo mô hình XML hợp lệ/NFC và hiển thị văn bản; HÒA và HOÀ vẫn khác.

## 1.0.28 - 2026-10-14

Kiểm tra toàn bộ khác biệt trước nạp kế thừa, bảng đối chiếu có tìm kiếm và chi tiết hai bản. Giữ tính nguyên tử khi từ chối nạp. Giữ vị trí cuộn bảng tuyến, chặn ẩn/hiện tạm nhóm cọc TDT và cuộn do trả tiêu điểm; giữ cuộn chủ động khi mở nhóm và dùng Tab.

## 1.0.27 - 2026-10-14

Sửa lỗi eInvalidInput khi Áp dụng tỷ lệ/hướng nhãn: bổ sung bht:api-sign-display vào danh sách vl-acad-defun. Tái hiện lỗi trên FAS1.0.26 và kiểm tra .NET Application.Invoke trên source/FAS sửa. Bỏ lề thừa khung con CỌC TDT để nút đọc/kiểm tra cùng hai cạnh với nút thêm/xem mốc; giữ ẩn/hiện theo tuyến.
## 1.0.26 - 2026-10-14

Chọn hướng nhãn song song/vuông góc theo đoạn đường nối gần biển. Cụm mã biển, biển phụ và lý trình giữ cùng hướng dễ đọc; biển hai trụ căn giữa hai đường nối khi thay tỷ lệ. Giữ vị trí/hướng của nhãn đã chỉnh thủ công. Hộp tỷ lệ có phần nội dung cuộn và hàng nút cố định, thêm chọn hướng nhãn; áp dụng tỷ lệ/hướng trong một giao dịch.

REA/Ghidra phân tích AlignmentDb.dbx: GetStationAtPoint/GetPointAtStation xử lý trắc dọc, không dùng trực tiếp cho điểm bình đồ. Ghi lại nguồn hình học, lý trình gốc, khoảng cách và hiệu chỉnh cọc; chưa tích hợp gọi native chưa kiểm chứng. Thử DwgFiler .NET bị lỗi truy cập bộ nhớ trong Core Console riêng, đã loại mã thử khỏi phát hành, giữ DWG gốc.
## 1.0.25 - 2026-10-14

Thêm BHTXULYDOAN và BHTNAPTICHLUY: bảng chọn đoạn/tim, xem trước tính lý trình, xác nhận và mở lại kết quả, xuất tích lũy hoặc riêng đoạn. Nạp kế thừa giữ nhãn và lý trình đã xác nhận, tránh trùng ID, chặn khác tọa độ/nội dung/kết quả, xử lý trùng tên tim. Cọc có số giữ số Km/H quan sát.

Bảo vệ kết quả tại kho dữ liệu Lisp/.NET; tính hàng loạt bỏ qua hồ sơ đã xác nhận. Chặn sửa/xóa tim liên quan trước khi thay đổi dữ liệu. Xuất kiểm tra lại điểm và hình học tim. Khôi phục phép tính thất bại cả bản ghi và liên kết tim; xử lý thông báo lỗi nil khi phục hồi. Chuẩn hóa giao diện theo thuật ngữ kỹ thuật. Kiểm tra thật phát hiện ba cọc xa tim và một biển khác phía đường cần đối chiếu, không sửa DWG gốc.

## 1.0.24 - 2026-10-14

BHTGANDOAN mở bảng chọn mã đoạn/gói, tên công trình, khoảng Km, phía đường, tim và đối chiếu điều kiện. Hủy giữ nguyên; gán/bỏ gán có một Undo. Tính lại theo tuyến cập nhật phân đoạn tự động và giữ gán thủ công. Không dùng phía so với tim rãnh thay phía đường. Trạng thái KHAC_PHIA_DUONG giải thích đúng trường hợp nằm trong khoảng nhưng sai phía.

I.449 có HEADER (mặc định ĐƯỜNG) phía trên và NAME phía dưới, căn giữa trong mặt biển, giữ dữ liệu NAME cũ và xuất đủ hai dòng. Kiểm tra DWG đoạn 10: OBJ-000058 lưu PHAI trong khi DOAN_10 là TRAI; TIMRANHD10 còn khai báo TIM_DUONG, bảng đoạn chưa gắn tim. Giữ nguyên bản vẽ gốc, kiểm tra trên bản sao.
## 1.0.23 - 2026-10-14

Chọn loại Tim đường/Tim rãnh/Tuyến khác ngay khi nạp TDT hoặc Polyline; sửa tuyến bằng hộp nhập, nhận dấu phẩy/chấm và báo lỗi đúng trường. TDT không còn tạo cố định TIM_DUONG. Hủy không đổi dữ liệu; áp dụng thông số không đổi giữ revision. Cập nhật TDT đã có giữ loại/mốc.

BHTDSMOC mở bảng mốc xếp theo khoảng cách dọc tim, hiện lý trình sau/trước, hệ số, nguồn và cảnh báo. Chọn dòng xem vị trí bằng dấu cộng vàng; chọn nhiều dòng xóa theo bản ghi gốc, xác nhận số mốc, snapshot/rollback và một Undo. Giữ cọc gốc, mốc tự động quản lý tại hồ sơ; dòng hỏng vẫn có thể chọn xóa. Hồ sơ liên quan cần tính lại.

## 1.0.22 - 2026-10-14

BHTNAPBAOHIEU nạp điểm, hồ sơ, các mặt biển, ảnh và bảng đoạn để tính theo tim đích. Bỏ nguồn tim/mốc/phương pháp tính cũ; lý trình và gán đoạn cũ giữ đối chiếu. Cọc có số Km/H giữ quan sát, không làm mốc tự động. Đoạn mới chưa gán tim; đoạn đã có ở đích được giữ.

Tim TDT chưa có mốc cũng không dùng Km/H hồ sơ cũ. Bộ quét loại ký hiệu/nhãn BHT; từ chối nguyên lượt cọc thiếu số trước phép tính gây numberp:nil. Sửa handle trong chữ ký hình học khi xuất/nạp và trong XData tham chiếu TDT, giữ mẫu hình học để vẫn phát hiện tim bị sửa.

Sáu bước trên tab Tuyến: nạp báo hiệu, chọn tim, xác nhận mốc, tính lại/kiểm tra, trình bày, xuất hồ sơ. Phương pháp theo cọc và nạp nguyên dự án nằm riêng; nút cần chọn tim chỉ bật khi có dòng được chọn. Xuất/nạp báo số hồ sơ, tim, đoạn. .bht giữ bảng đoạn và liên kết tim khi nạp nguyên dự án.

## 1.0.21 - 2026-10-14

Biển Khác, bảng quảng cáo, bảng chỉ dẫn và nhóm chưa xác định có dấu chấm đặc tại từng điểm khảo sát. Khi có nhiều trụ, lý trình, phía và lệch tim dùng cùng trụ khảo sát gần tim nhất theo mặt bằng. Vị trí đại diện cho trình bày giữ nguyên; ưu tiên tim riêng của đoạn ở mọi đường tính.

Thêm BHTDIMTIM và nút DIM từ điểm trụ gần nhất tới tim. Tạo AlignedDimension thật trên BHT_DIM_KIEM_TRA, không thay số đo bằng chữ; gọi lại cập nhật DIM cùng hồ sơ/tim. Sửa block bảng chỉ dẫn cũ không có vòng chân trả về Lisp nil thay vì lỗi eInvalidInput. Hồ sơ cũ cần tính lại để áp dụng cách đo mới; xuất Excel vẫn đọc dữ liệu đã lưu.

## 1.0.20 - 2026-10-14

Sửa báo cáo Excel chỉ xuất mã biển chính: đọc đủ danh sách mặt của từng cụm, giữ tấm trùng mã và số mặt kiểu cũ giống cách vẽ CAD. Mỗi mặt có một dòng với tên/nhóm/nội dung riêng; giữ chung ID hồ sơ, lý trình, tim, đoạn/gói. Mã và tham số @ tách ra cột nội dung. DESC_3 của S.509a ưu tiên nội dung đã lưu như cách dựng mặt CAD.

Tổng hợp đếm từng mặt và phân biệt nội dung khác nhau; hiện riêng số hồ sơ/cụm và số mặt. Trụ/tổng số mặt cụm chỉ ghi ở dòng đầu để tránh cộng trùng. Đổi Offset thành “Lệch tim tuyến (m)” và giải thích ngay trong workbook. Xuất không tính lại lý trình và không sửa hồ sơ/DWG gốc.

## 1.0.19 - 2026-10-14

Thêm bảng đoạn/gói thầu riêng `.bhtseg` (BHTSEG/1), kiểm tra phiên bản, số bản ghi và SHA256; lưu qua tệp tạm và áp dụng toàn bảng bằng một transaction. Nạp trực tiếp XLSX không cần cài Excel, xem trước/sửa dữ liệu, chuyển sang `.bhtseg`. Hỗ trợ bảng phân chia thực tế với Km tách ô và tên gói gộp ô; công khai mã gói tự sinh. TSV chỉ còn làm đầu vào chuyển đổi, không dùng để lưu mới. Dòng lỗi bị báo rõ, không bỏ qua để nạp một phần.

Mỗi đoạn gán tim đường, tim rãnh hoặc tuyến khác đã khai báo. Phân đoạn tính độc lập trên tim từng đoạn, chọn tim hợp lệ gần nhất; tim gần bằng nhau trong 1 cm hoặc chung mốc biên cần chọn thủ công. Tính riêng/hàng loạt giữ tim của đoạn, cả khi chế độ chung dùng cọc. Bảo toàn số Km/H cọc. Đổi tên tim cập nhật liên kết; mất tim hoặc thiếu mốc chặn phân đoạn. Thay bảng bỏ gán tự động cũ; gán tay chỉ giữ khi tim/phạm vi/phía/gói không đổi. Có Undo và hoàn tác cả dữ liệu/nhãn khi ghi lỗi.

## 1.0.18 - 2026-10-14

Biển địa danh/nhóm Khác, bảng quảng cáo và bảng chưa xác định hiện lý trình độc lập với tên đã có trên mặt biển. Cụm mã biển và lý trình đặt phía đầu mặt biển, cùng trục, góc và khoảng dòng; dùng đường nối hiện tại khi tránh chồng lấn. Mã các mặt phụ đi theo nhãn chính khi kéo nhãn. Biển mất mặt còn trụ vẫn ghi lý trình đã lưu.

Nhãn biển và lý trình tự lên trên thứ tự vẽ bằng Sortents, chạy từ Palette và lưu qua mở lại DWG. Tính/xóa lý trình cập nhật chữ đã vẽ, không dựng lại hay di chuyển biển. Tính lại theo tim hoàn tác cả chữ và dữ liệu nếu cập nhật lỗi.

Báo cáo Excel nhận đủ BIEN_BAO, BANG_CHI_DAN, BANG_QC, KHAC và CHUA_XAC_DINH. Giữ tên riêng, lý trình, số trụ/mặt và nguồn tính; biển chưa có mã vẫn xuất. Tổng hợp phân biệt nhóm, mã, tên và tình trạng, mỗi hồ sơ tính một bộ biển. Cần xuất lại XLSX để cập nhật báo cáo đã tạo bằng bản cũ.
## 1.0.17 - 2026-10-14

Thêm Tính lại theo tim đang chọn / BHTTINHTHEOTUYEN: chọn đúng tim, xem số hồ sơ tính được và phạm vi mốc trước khi áp dụng, chuyển khỏi phương pháp cọc cũ và lưu tim đang tính. Hồ sơ chưa đủ điều kiện được xóa lý trình cũ và đánh trạng thái; bỏ phân đoạn/gói cũ. Có Undo và hoàn tác khi lỗi ghi.

Tim có mốc TDT đã xác nhận không ghép thêm Km/H từ hồ sơ cọc của dự án cũ. Giữ số Km/H khảo sát, RTK và hình học. Chẩn đoán nêu rõ phương pháp/ID tim đang dùng và hồ sơ chưa tính theo tim này. Đổi tên tim cập nhật ID; xóa tim không tự dùng tuyến cũ thay thế.

## 1.0.16 - 2026-10-14

Xuất dự án bằng tệp `.bht` có phiên bản định dạng, kiểm tra cấu trúc, giới hạn dung lượng và SHA256 trước khi nạp; vẫn nhận DWG BHT cũ. Gói giữ hình học/block, điểm, hồ sơ, tuyến/mốc và liên kết ảnh. Xuất qua tệp tạm, xác minh trước khi hoàn tất và không ghi đè tệp đã có.

Sửa chặn nạp do INSUNITS khác nhau: bản vẽ trống nhận đơn vị nguồn, bản vẽ có hình yêu cầu chọn rõ giữ nguyên tọa độ hoặc hủy. Không tự co giãn, không đổi đơn vị của bản vẽ có hình, không ghi đè dự án BHT đã tồn tại. Từ chối xuất bản vẽ chỉ có cấu hình. Đã kiểm tra trên filebaohieu.dwg: 801 điểm, 179 hồ sơ, 334 bản ghi ảnh, tuyến và mốc; Undo, cập nhật ký hiệu và lưu/mở lại.

## 1.0.15 - 2026-10-14

Đưa nhãn vàng vào giữa thân cọc tiêu, tự giảm cỡ khi nội dung dài; bỏ nhãn tên điểm BHT bị lặp khi điểm chỉ thuộc các cọc đã vẽ. Giữ nhãn điểm dùng chung với nhóm khác, cao độ, ID và chữ không do BHT quản lý. Nút cọc hiển thị xám “Cọc đặt tự động”, không mở thao tác đặt tự do gây lỗi thiếu tuyến.

Bố trí hàng loạt lấy hướng tuyến cục bộ tại từng biển, dùng chung bộ dựng ký hiệu/nhãn/đường nối với đặt riêng. Tính khoảng lùi theo hình đã xoay và kiểm tra lại sau khi dựng nhãn. Giữ tỷ lệ, RTK, mốc và lý trình; thiếu tuyến giữ góc cũ và báo rõ. Bỏ qua đối tượng không bố trí được, giữ trạng thái cũ; lỗi khi thực hiện hoàn tác cả hình, nhãn điểm và dữ liệu.

## 1.0.14 - 2026-10-14

Cọc tiêu được bố trí riêng: chân cọc tại điểm khảo sát, đầu cọc theo phương vuông góc với tiếp tuyến và hướng về tim, không có đường nối. Bố trí tất cả sửa cả cọc đã dịch tay, bỏ đường nối cũ; giữ tỷ lệ, tọa độ RTK, lý trình và mốc. Cọc thiếu hướng giữ góc cũ và được báo riêng; bỏ qua lớp khóa. Nút đặt riêng cọc không mở tùy chọn đường nối của biển báo.

Giữ liên kết đường tham chiếu bố trí khi xuất/nạp dự án. Sửa ưu tiên thư mục runtime đã xác định thay vì bản cùng phiên bản trên Support Path. Sửa encoding script kiểm thử bộ cài cho PowerShell 5.1 và đồng bộ thông tin phiên bản README.

## 1.0.13 - 2026-10-13

Đã sửa lỗi lý trình quá lớn làm hỏng định dạng; thống nhất nhập dấu phẩy và phần mét lẻ giữa Lisp/Core. Đã chặn chia cho 0 với mốc trùng và kết quả VALID cho NaN/Infinity. Cập nhật tim TDT yêu cầu đúng liên kết nguồn, không ghi đè chỉ vì trùng lớp.

Đã xuất Excel qua tệp tạm, giữ báo cáo cũ khi lỗi hoặc tệp bị khóa; mã hóa ký tự điều khiển và giữ chuỗi escape/Unicode. Giữ sửa cung đơn chính xác của 1.0.12. Đã chạy hồi quy rộng, sửa hai kỳ vọng thử cũ theo hành vi từ 1.0.9/1.0.10. Chưa kiểm chứng lý trình native TDT vì không có khóa.

## 1.0.12 - 2026-10-13

Đã giữ nguyên cung tròn khi lấy tim TDT trả về một Arc đơn, thay vì chia cung thành các đoạn thẳng. Đoạn thẳng đơn cũng giữ hai đỉnh. Chiều dài trả về khớp với Polyline tham chiếu thực tế, kể cả các loại đường cong vẫn cần lấy mẫu.

Đã kiểm thử cung thuận/ngược chiều, cung qua góc 0, bán kính nhỏ/lớn, nối cung với đoạn thẳng đảo chiều và lưu/mở lại DWG. Không thay đổi tim TDT gốc. Chưa tích hợp lý trình native TDT; kiểm chứng trong phiên TDT có khóa được để lại cho đợt tiếp theo.

## 1.0.11 - 2026-10-13

Đưa năm tab điều hướng lên đầu bảng, bỏ dải bên phải rộng 108 px. Trang Tổng quan giữ một nút Tiếp tục công việc thay cho các nút chuyển tab trùng lặp. Giữ bảng màu xanh hiện có và các chức năng nhập liệu.

BHTKIEMTRATDT bổ sung tiến trình/kiến trúc CAD, đường dẫn và phiên bản module TDT đang nạp, lớp gốc của proxy. Có tài liệu phân tích API native TDT 9.1; chưa kích hoạt kết nối native chưa được kiểm chứng. Giữ sửa thứ tự hatch và màu khi copy của 1.0.10.

## 1.0.10 - 2026-10-13

Ngày 13/10/2026.

Biển tạo/cập nhật bằng bản này giữ thứ tự hatch và chữ ngay trong block riêng. Đã kiểm tra chuyển sang DWG trống, lưu/mở lại và trường hợp lớp cùng tên ở DWG đích có màu trắng. Xuất/nạp dự án cũng khôi phục thứ tự vẽ.

Với biển đã vẽ, chạy `BHTSUASAOCHEP` trong DWG nguồn khi hình đang đúng, chọn các block BHT cần chuyển rồi copy lại. Có thể dùng **Cập nhật mẫu biển đã vẽ** để cập nhật từ hồ sơ. Block cùng tên đã tồn tại tại đích có thể được CAD tái sử dụng. Lệnh không đoán lại thứ tự từ bản copy đã hỏng.

`BHTKIEMTRATDT` đọc loại đối tượng và handle tim, không sửa hình học. `BHTTUYENTDT` lấy hình học; sau đó cần `BHTDOCCOCTDT` để đọc nhãn Km và xác nhận mốc. Nếu không có nhãn Km, nhập mốc đã biết bằng `BHTMOCKM`.

Chưa xác nhận đã giải quyết trường hợp TDT của người dùng: phiên thử riêng không nạp được đầy đủ module TDT; cần DWG có lỗi và phiên CAD chạy TDT 9.1 bình thường. Chưa kiểm tra CAD hãng khác hoặc phiên bản khác AutoCAD 2024.

Đóng AutoCAD rồi cài BHT 1.0.10. Không NETLOAD DLL mới chồng lên DLL cũ trong cùng phiên.

## 1.0.9 — 2026-10-12

- Đã tăng khoảng hở dưới hình và dịch cả cụm tên/lý trình để tránh đường nối của báo hiệu.
- Đã đổi Xoay nhãn RTK thành Gom / xoay nhãn: sắp các dòng gần điểm theo thứ tự tên, mô tả, cao độ, mã điểm rồi xoay theo UCS hoặc hướng A → B; đồng bộ cả nhãn và ký hiệu ảnh Timemark.
- Đã đổi góc hiển thị dấu điểm mà giữ tọa độ khảo sát, dữ liệu GPS và liên kết ảnh. Cụm khóa lớp hoặc khác mặt phẳng XY được bỏ qua; lỗi ghi giữa chừng phục hồi cả thao tác.
- Đã bổ sung nhãn cọc tiêu thủ công từ số Km/H trong hồ sơ; chưa có số vẫn hiện Cọc tiêu, không tạo lý trình hoặc tự xác nhận làm mốc.
- Đã tạo nhãn điểm cọc bổ sung khi vẽ/cập nhật ký hiệu; giữ hướng nhãn Timemark sau đồng bộ ảnh.

## 1.0.8 — 2026-10-12

- Đã tách lý trình thành dòng dưới tên biển, giữ cùng phông, cỡ chữ và góc xoay. Với nhiều mặt biển, lý trình nằm dưới tất cả tên mặt biển.
- Đã cập nhật nhãn cũ khi dùng Cập nhật mẫu biển đã vẽ; giữ dòng lý trình riêng để sửa nội dung, đổi tỷ lệ và bố trí theo tim. Xóa lý trình trong hồ sơ sẽ bỏ dòng dưới, không tạo nhãn trùng.
- Đã giới hạn nút Cập nhật mẫu biển đã vẽ ở các nhóm biển/bảng, tránh thay block đèn trong thao tác này. Đèn vẫn được chỉnh và cập nhật trong hồ sơ riêng.
- Đã kiểm thử 27 ca nhãn, 44 ca bố trí theo tim, lưu/mở lại và 25 biển có lý trình trên bản sao BOT830. Giữ điểm, hồ sơ, mốc/tuyến và biến đổi ký hiệu; kiểm tra khoảng cách theo tim trên 120 ký hiệu có cả nhãn hai dòng.

## 1.0.7 — 2026-10-12

- Đã sửa lựa chọn Cách tim để nhận tuyến đã nạp kể cả khi loại đang là Khác, Tim rãnh hoặc chưa phân loại. Người dùng chọn rõ tuyến dùng làm tim bố trí; BHT kiểm tra hình học trước khi thực hiện.
- Đã hiển thị loại tuyến trong bảng dữ liệu tuyến và danh sách chọn tim. Giữ ID riêng khi gửi lệnh, không dùng nhãn hiển thị làm ID.
- Đã giữ nguyên loại tuyến, mốc, điểm đo và lý trình khi bố trí hình. Giữ công cụ TDT theo nguồn tuyến sau khi thêm cột Loại.
- Đã kiểm thử FAS/DLL bảo vệ: 44 ca bố trí, 2.676 kiểm tra giao diện và 120 báo hiệu theo TUYEN1 trên bản sao DWG đã lưu của người dùng; giữ dữ liệu gốc, Undo và lưu/mở lại.

## 1.0.6 — 2026-10-12

- Đã nâng cấp BHTKHOANGCACH thành một công cụ bố trí hàng loạt: theo tim đã nạp, ra ngoài mép đường bằng PL tham chiếu, hoặc giữ cách đổi khoảng cách từ điểm đo.
- Đã tính khoảng lùi theo kích thước hình và nhãn sau khi xoay/scale, xác định phía bằng điểm đo và hướng vuông góc tại từng vị trí; giữ điểm khảo sát, lý trình, góc và tỷ lệ.
- Đã hỗ trợ tim/Polyline 2D thường gồm đoạn cong và gãy khúc; bỏ qua các trường hợp không xác định được phía hoặc khoảng lùi an toàn và báo ID tại dòng lệnh.
- Đã sửa phục hồi hồ sơ sau lỗi để giữ đúng thời điểm sửa của bản ghi trước thao tác.
- Đã kiểm thử bản FAS/DLL được bảo vệ, 34 ca bố trí, Undo, lưu/mở lại, 20 ca đường nối, 2.615 kiểm tra giao diện và 120 ký hiệu trên bản sao BOT830. Cập nhật hướng dẫn và báo cáo giới hạn nghiệm thu.

## 1.0.5 — 2026-10-12

- Đã sửa mở Công cụ bổ sung/gói thầu: tính lại bố cục, cuộn tới nội dung ngay khi tick, không cần chuyển tab.
- Đã thống nhất đường thẳng/gấp khúc/điểm trung gian giữa Đặt tự do và áp dụng toàn bộ. Giữ kiểu riêng trong hồ sơ khi cập nhật, lưu/mở lại; cho chọn kiểu nối cùng khoảng cách.
- Đã chuyển chọn báo hiệu sang lệnh .NET giữ PICKFIRST, bỏ bước gọi Lisp đọc thông báo trước/sau các lệnh chọn để giữ lựa chọn.
- Đã thêm chọn trọn thành phần cùng nhóm và SELECTSIMILAR theo lớp, bỏ điều kiện tên block khi dùng công cụ này.
- Đã kiểm thử 20 ca chế độ nối, 10 ca lựa chọn, giao diện kích thước nhỏ và bản sao BOT830; cập nhật hướng dẫn.

## 1.0.4 — 2026-10-12

- Đã thêm nút Xoay nhãn RTK và lệnh BHTXOAYNHAN: xoay toàn bộ tên, mã điểm, mô tả, cao độ theo UCS hiện tại hoặc hướng A → B chọn trên CAD.
- Đã giữ điểm đo, nội dung, phông, cỡ chữ và vị trí neo từng cụm; cập nhật nhãn giữ hướng đã xoay. Một lần Undo hoàn tác cả lần xoay.
- Đã rút gọn hộp tỷ lệ RTK, chuyển giải thích chi tiết vào gợi ý khi đưa chuột lên ô nhập.
- Đã bổ sung kiểm thử nhãn xoay, hoàn tác, lưu/mở lại và dữ liệu BOT830 trên AutoCAD 2024; cập nhật hướng dẫn và danh mục lệnh/nút.
- Đã nạp lại bốn DWG đèn tín hiệu người dùng cung cấp; chuẩn hóa lớp và block lồng trong bộ nhớ, giữ màu đèn và nguồn DWG.
- Đã thêm khoảng cách đặt biển/bảng/đèn dùng chung; dịch hình đã có và tính lại đường nối, giữ điểm đo, góc và tỷ lệ. Gom lớp hiển thị về BHT_KYHIEU, thêm chọn toàn bộ báo hiệu.
- Đã thêm BHTXUATDUAN/BHTNAPDUAN để chuyển cả hình CAD và hồ sơ BHT, remap handle tuyến và tên block. Hiện hỗ trợ nạp vào DWG chưa có BHT, chưa gộp dự án.
- Đã sửa nhận nhầm cấu hình BHT khởi tạo là dự án trong DWG trống; kiểm chứng nạp bằng lệnh, Undo một lần và cập nhật sau nạp. Đường nối tự động tránh mặt biển cả khi điểm gốc nằm thẳng phía trên biển.

## 1.0.3 — 2026-10-12

- Đã gộp nút tính toàn bộ hồ sơ, kiểm tra bản vẽ và các lối tắt trùng ở Tổng quan; đưa CSV về Điểm vị trí, KMZ và thống kê ảnh về Ảnh hiện trường.
- Đã giữ phương pháp tính lý trình hiện tại khi bấm tính toàn bộ; chỉ mở bảng kiểm tra mốc khi nguồn chưa đủ. Nút tính một hồ sơ có tên riêng.
- Đã làm rõ gán block mặc định cho nhóm và gán riêng từng hồ sơ; nút Tiếp tục chuyển tới tab nhập điểm khi chưa có dữ liệu.
- Đã cập nhật trợ giúp trong CAD, viết hướng dẫn và danh mục 78 tên lệnh/82 nút; thêm bằng chứng kiểm thử trên bản sao BOT830 bằng AutoCAD 2024.
- Đã kiểm tra bản FAS/DLL bảo vệ, giữ điểm đo và ký hiệu sau tính/lưu/mở lại; ghi rõ các bước tương tác TDT và máy sạch chưa nghiệm thu.

## 1.0.2 — 2026-10-11

- Thiết kế lại nhóm chức năng, nút và bố cục hồ sơ khi bảng hẹp; sửa hàng chồng lên danh sách tuyến.
- Khóa con lăn của ô chọn nguồn tuyến; bỏ dải lỗi phía dưới, giữ hộp thoại và lịch sử lệnh CAD.
- 55 gọi LAYFRZ, 66 gọi LAYTHW; giữ 44 và các phím tắt đã được phần mềm khác đăng ký.


## 1.0.1 — 2026-10-11

- Đã khôi phục nền xanh, tăng diện tích tab theo DPI để tên trang không chồng lên nhau.
- Đã thống nhất chữ trên mặt biển bằng Giaothong1, giữ nội dung số đã nhập và bỏ điểm dựng hình trong block biển. Điểm khảo sát và dấu gốc dưới chân được giữ.
- Đã tách bật/tắt nền khỏi quy trình vẽ lại ký hiệu; giữ vị trí, góc, tỷ lệ, nhãn và đường dẫn. Hoàn tác bằng Undo.
- Đã gom công cụ ảnh về Ảnh hiện trường, xuất Excel/CSV về Báo cáo; cọc TDT chỉ hiện khi chọn tuyến TDT. Bỏ dòng trạng thái TDT không cần thiết.
- Đã đưa đoạn/gói thầu vào mục tùy chọn, thêm nút lưu TSV mẫu và hướng dẫn nhập.
- Đã thêm phím tắt lớp 11, 22, 33, 55 khi chưa có lệnh cùng tên; giữ nguyên 44 của TDT. Đóng băng lớp bằng BHTLAYFRZ.

## 1.0.0 — 2026-10-11

- Đã gom các bước nhập dữ liệu, quản lý báo hiệu, lý trình và bàn giao vào Tổng quan; thêm gợi ý bước tiếp theo theo dữ liệu thực tế.
- Đã thêm bộ lọc hồ sơ thiếu lý trình, mã biển, điểm vị trí hoặc cần đối chiếu mốc; đổi nền giao diện sang sáng, giữ nút xanh.
- Đã giảm việc tải lại các trang ẩn, hoãn lọc tới khi ngừng gõ, thêm chỉ mục và cache riêng từng bản vẽ; giữ nội dung chưa lưu.
- Đã giới hạn ảnh thư viện theo vùng xem và cache 120 ảnh mẫu, kiểm tra hiển thị khi cuộn trước khi nhấp.
- Đã tối ưu tra cứu điểm, danh sách mốc và đọc dictionary khi tính lý trình hàng loạt. Bộ thử 5.000 hồ sơ giảm từ 45,28 xuống 4,63 giây; sai số tối đa 0,5 mm so với hình học chuẩn.
- Đã kiểm tra trên bản sao 261011_test.dwg, giữ tọa độ/nguồn điểm và kết quả như bản 0.6.68; báo riêng lỗi dữ liệu, không tự đoán lại số cọc.
## 0.6.68 — 2026-10-11

- Hoàn thiện báo cáo và tên nút dễ hiểu từ bản 0.6.67; đồng bộ cả hướng dẫn trong hộp chọn đèn, nhập block và thông báo tạo cọc với tên nút mới.
- Bản bàn giao của đợt chỉnh nội dung tiếng Việt; giữ nguyên cách tính lý trình và dữ liệu bản vẽ.

## 0.6.67 — 2026-10-11

- Viết lại báo cáo kiểm tra: việc cần làm ở trên, số liệu ở dưới; giải thích lỗi và hướng xử lý, thay thuật ngữ “mồ côi” bằng liên kết cụ thể bị thiếu.
- Báo cáo tự xuống dòng, mở từ đầu; sao chép đầy đủ nội dung; kiểm tra hiển thị ở cửa sổ nhỏ.
- Đổi tên các nút ảnh, mốc, ký hiệu và báo cáo để nói rõ thao tác; giữ mã lệnh và cách xử lý dữ liệu.
- Rà tiếng Việt theo vietnamese-tech-writing: không có lỗi/cảnh báo. CAD 2024: Core 233, UI 771, kiểm tra dữ liệu 11.

## 0.6.66 — 2026-10-11

- Tính theo cả cột Km và cọc tiêu; chia khoảng tại các mốc đủ dữ liệu thay vì bỏ qua cột Km.
- Bảng Mốc lý trình đã nhập có loại/nguồn, hiện cả mốc đo thiếu dữ liệu và mốc bổ sung chưa xác nhận.
- Xác nhận riêng mốc thủ công áp dụng cho cả cột Km; giữ nguồn, kết quả tham khảo và yêu cầu xác nhận lại khi đổi loại/số/vị trí.
- Danh sách cũ phải xem lại; giữ mốc gốc, cặp/nhóm gán riêng và cảnh báo mâu thuẫn; không tự sửa DWG khi mở.
- CAD 2024: Core 233, nội suy 51, bộ mốc 52 cùng lưu/mở lại/API; 21 kiểm tra trên dữ liệu BOT830 dựng lại trong phiên riêng.

## 0.6.65 — 2026-10-11

- Sửa tên I.428 thành “Cửa hàng xăng dầu”, đối chiếu mục I.428a trong danh mục; tìm được theo tên không dấu.
- Hiện cọc tiêu bổ sung thủ công trong bảng kiểm tra, kể cả khi thiếu Km/H; xác nhận/bỏ xác nhận riêng từng cọc để dùng làm mốc nội suy theo cọc.
- Giữ nguồn thủ công, kết quả dùng mốc này được đánh dấu tham khảo; xác nhận hết hiệu lực khi dữ liệu cọc không còn khớp số/vị trí đã xác nhận.
- Giữ kiểm tra hình học, cập nhật trạng thái kết quả liên quan khi thay đổi xác nhận; chưa xác nhận thì không tự dùng làm mốc.
- AutoCAD 2024: Core 233, UI 745, danh sách/mốc thủ công 39 cùng lưu/mở lại/API, cọc thực địa 51 và hồi quy tạo cọc thủ công.

## 0.6.64 — 2026-10-11

- Lấy trực tiếp cọc tiêu đã nhập Km/H, mở bảng kiểm tra theo lý trình tăng; không cần tạo chuỗi thủ công cho một đường.
- Chuyển quản lý nhóm cọc theo đường/nhánh vào Nâng cao; giữ chuỗi cũ và các cặp/nhóm gán riêng.
- Kiểm tra trùng lý trình, đổi hướng vị trí, khoảng cách bất thường; không tự tính qua đoạn mơ hồ hoặc dữ liệu cọc đã đổi.
- Thêm “Theo cọc đã nhập…” tại hồ sơ; lưu rõ hai cọc dùng để tính và kiểm tra lại danh sách sau khi thêm/xóa/sửa cọc.
- AutoCAD 2024: Core 233, UI 738, danh sách tự lấy 25 cùng save/reopen/API .NET, cọc thực địa 51 và hồi quy cọc thủ công.

## 0.6.63 — 2026-10-11

- Thêm cọc thủ công bằng chọn/nhập vị trí CAD; hỗ trợ cọc tiêu/cột Km, số Km/H có thể để trống, lưu nguồn và căn cứ vị trí.
- Không dùng cọc thủ công làm mốc ở cả chuỗi cọc và chế độ tim tuyến, kể cả hồ sơ khác dùng chung điểm.
- Sắp xếp nút tạo mới, sửa hồ sơ, đặt/cập nhật ký hiệu; nút tỷ lệ rộng cả hàng; đổi “Thêm điểm CAD…” thành “Gắn điểm có sẵn…”.
- Thông tin điểm/hồ sơ và Tổng quan phân biệt nguồn thủ công, cao độ chưa biết. Chặn tạo cọc cùng loại trùng vị trí.
- AutoCAD 2024: Core 233, UI 727, chuỗi cọc 51; kiểm tra cọc thủ công, lưu/mở lại, Undo và FAS/DLL bảo vệ.

## 0.6.62 — 2026-10-11

- Thêm tính lý trình theo cọc tiêu thực địa, không cần tim tuyến hoặc Station Control.
- Lập/sửa/xóa chuỗi cọc và chọn hai cọc cho từng báo hiệu ngay trong Palette; lưu nguồn tính trong DWG.
- Cảnh báo đoạn dài, số Km/H bất thường, đoạn mơ hồ; giữ giá trị cũ khi không tính được và khoanh lỗi theo đoạn.
- Xoay biển vuông góc phương cọc, giữ RTK/vị trí chèn/tỷ lệ; phân biệt lệch đường nối cọc với lệch tim.
- Kiểm thử AutoCAD 2024: Core 233, UI 718, cọc thực địa 51 cùng lưu/mở lại/API Undo, hướng biển 35; đối chiếu OBJ-000153 trên bản sao phiên CAD đang mở.

## 0.6.61 — 11/10/2026

- Bỏ ghi chú hướng dẫn ở bên phải thư viện.
- Cho phép ghép biển tùy chỉnh với biển QCVN trên cùng trụ, thêm/bỏ mặt và đổi thứ tự; mặt đầu nằm trên cùng.
- Nội dung tùy chỉnh lưu theo chỉ số mặt trong sign_content của DWG. Hai mặt cùng mã vẫn có nội dung riêng; không phụ thuộc tệp mẫu trên máy khác.
- Biển tùy chỉnh dùng Giaothong1 như block STOP và các biển phụ trong thư viện; giữ WidthFactor=1, giảm đều chiều cao chữ để vừa khung thay vì kéo méo.
- Chuẩn hóa cạnh lớn nhất của bảng về 1,8 đơn vị như biển phụ; bảng hai dòng rộng bằng mặt STOP chuẩn, cao 0,9 đơn vị. Dùng cùng Tỷ lệ biển.
- Xem trước dùng phông và tỷ lệ tương ứng; giữ thư viện mẫu, hỗ trợ chuyển bảng riêng bản cũ sang mặt biển tùy chỉnh khi chọn/cập nhật.
- Không tự thay đổi các ký hiệu cũ khi khởi động.

## 0.6.60 — 11/10/2026

- Trụ mất mặt biển: nền xanh RGB 0/118/190, viền trắng lùi vào trong, chữ TRỤ MẤT BIỂN; giữ kích thước, số trụ và điểm chân.
- Sửa lệch hướng 90° của block trụ mất biển khi chọn hướng theo tuyến/hai điểm: nhận đúng trục đầu biển +Y như biển thường.
- Giữ công tắc Tô nền: tắt nền vẫn còn khung và chữ; không sinh nhãn dưới chân.
- Ẩn sáu tên trùng nội dung của nhóm P.124 trong thư viện khi có mẫu thay thế a1/a2/b1/b2. Giữ đủ biến thể trái/phải, xe/ô tô và c/d/e/f.
- Mã P.124A, A-1, A-2, B, B-1, B-2 vẫn đọc/nạp được cho bản vẽ cũ; tìm mã cũ trong thư viện dẫn đến biến thể tương ứng. Không xóa DWG nguồn.
- Không tự sửa bản vẽ khi khởi động; cập nhật ký hiệu và áp dụng hướng là thao tác chủ động.

## 0.6.59 — 11/10/2026

- Chuyển bảng chữ vào nhóm Biển tùy chỉnh trong Thư viện biển báo BHT; bỏ nút Nhập bảng chữ tùy chỉnh ở bảng hồ sơ ngoài.
- Thêm mẫu, sửa tên/nội dung nhiều dòng bằng Nội dung biển; xem trước trực tiếp trong thư viện.
- Lưu mẫu riêng theo người dùng, giữ nguyên mẫu khác khi cập nhật; tệp thư viện lỗi không bị ghi đè.
- Khi chọn mẫu, sao chép nội dung vào hồ sơ DWG hiện có; không tự cập nhật các biển đã chèn khi sửa mẫu.
- Bảng chữ cũ vẫn mở được từ thư viện để sửa. Giữ chức năng đèn cần vươn và đồng bộ tỷ lệ của 0.6.58.

## 0.6.58 — 11/10/2026

- Thêm đèn tín hiệu ba màu cần vươn trái/phải bằng block CAD 2D của BHT; đóng gói đủ 9 mẫu đèn.
- Bỏ hệ số phóng đèn cố định ×4. Chuẩn hóa cạnh lớn nhất của ký hiệu đèn về 2,4 đơn vị ở tỷ lệ 1; giữ tỷ lệ hình học gốc.
- Tỷ lệ biển áp dụng cả đèn tín hiệu, đèn chiếu sáng và nhóm Đèn cũ, bao gồm các ký hiệu đã chèn. Giữ vị trí, góc xoay, hồ sơ và điểm RTK.
- Tắt nền biển vẫn giữ các đĩa màu đèn tín hiệu. Không thêm nhãn dưới chân đèn.
- Mẫu cần vươn là ký hiệu minh họa, không biểu diễn chiều dài cần thực tế; không tự sửa bản vẽ khi khởi động.

## 0.6.57 — 11/10/2026

- Phát triển từ nền 0.6.54 đầy đủ thư viện, không dùng gói công khai rút gọn 0.6.55.
- Thêm nút Nhập bảng chữ tùy chỉnh trong Hồ sơ đối tượng. Nhập nhiều dòng, dùng nhóm Khác hoặc Bảng quảng cáo hiện có.
- Giữ xuống dòng chủ động khi tạo bảng nền xanh, viền trắng. Bảng một dòng cũ giữ cách tự ngắt dòng hiện có.
- Nội dung lưu trong trường mô tả hiện có, không đổi định dạng hồ sơ DWG. Bản này không bổ sung xuất DWG chuyển chữ thành nét.

## 0.6.54 — 11/10/2026

- Sửa lỗi khởi động “bad argument type: stringp T”: OR trong AutoLISP trả boolean, không trả đường dẫn FAS/LSP. Giữ trực tiếp kết quả findfile, ưu tiên FAS rồi LSP.
- Bổ sung hồi quy khi có Support Path như bộ cài thật, khi không có Support Path, nạp FAS trước Bridge và nạp loader mã nguồn LSP.
- Đã tái hiện lỗi trên FAS 0.6.53 với Support Path trước khi sửa.

## 0.6.53 — 11/10/2026

- Khi mở BHT mà API Lisp chưa sẵn sàng, thử nạp đúng FAS cùng phiên bản nằm cạnh DLL; kiểm tra lại API sau khi nạp hoàn tất.
- Phân biệt lỗi thiếu/nạp Lisp với lỗi khác phiên bản thật; giữ chi tiết lỗi và hiển thị tại vùng trạng thái, tránh hộp thoại lỗi giả khi mở bảng.
- Chờ CAD kết thúc lệnh, chặn kiểm tra trùng và bỏ kết quả cũ khi đổi/đóng bản vẽ. Mỗi lần mở bảng chỉ thử nạp một lần, có thời hạn chờ.
- Không tự cập nhật ký hiệu trên bản vẽ khi khởi động.

## 0.6.52 — 11/10/2026

- Đồng bộ tỷ lệ của biển báo, bảng chỉ dẫn, bảng quảng cáo, bảng nhóm Khác và Chưa xác định theo cấu hình Tỷ lệ biển.
- Sửa trường hợp biển địa phận nhóm Khác vẫn ở ×1 trong khi biển báo ở ×3; áp dụng cho biển đã chèn và biển mới.
- Đổi tỷ lệ hàng loạt giữ nguyên vị trí/góc của các INSERT, không dịch chân theo tỷ lệ; không thay đổi cọc, đèn hoặc tọa độ RTK.
- Dùng chung chỉ mục điểm khảo sát trong một lần đổi tỷ lệ, tránh đọc lại toàn bộ điểm cho từng biển.

## 0.6.51 — 11/10/2026

- Trụ mất mặt biển hiển thị bằng biển khung chữ nhật có nội dung “Trụ mất biển” bên trong; giữ số trụ và điểm chân.
- Bỏ nhãn dưới chân và nhãn mặt phụ của trường hợp mất biển. Chèn / cập nhật xóa nhãn cũ, giữ vị trí/hướng đã đặt.
- Giữ mã biển trong hồ sơ để khôi phục ký hiệu khi đổi tình trạng về Tốt.

## 0.6.50 — 11/10/2026

- Phát hành thay bản đóng gói kiểm tra nội bộ 0.6.49: nhãn cọc dưới ký hiệu, xoay cùng block và tự dùng hồ sơ Km/H làm mốc tuyến.
- Đồng bộ kiểm tra hồi quy nhãn cọc với yêu cầu bố trí phía dưới.

## 0.6.49 — 11/10/2026

- Nhãn cọc tiêu đặt dưới biên block, xoay đúng góc của ký hiệu; giữ nội dung H/Km và cỡ chữ gọn.
- Hồ sơ cọc tiêu/cọc Km đã nhập đủ số Km/H tự làm mốc tính lý trình khi điểm khảo sát thuộc phạm vi duy nhất một tuyến. Không cần nhập lại bằng BHTMOCKM.
- Mốc hồ sơ được đọc trực tiếp khi tính: sửa/xóa số cọc hoặc thay đổi hình học tuyến không để lại bản sao mốc cũ. Tính hàng loạt dùng chung dữ liệu mốc trong lượt chạy.
- Báo mâu thuẫn mốc, cọc nằm trong phạm vi nhiều tuyến; giữ mốc khai báo thủ công. BHTDSMOC và chẩn đoán tuyến hiển thị cả mốc lấy từ hồ sơ.
- Lý trình của chính cọc lấy từ số Km/H dù chưa có tuyến; tuyến vẫn cần để tính lý trình dọc đường cho đối tượng khác.

## 0.6.48 — 11/10/2026

- Tăng kích thước hiển thị các nhóm đèn lên 4 lần; giữ khoảng cách tới điểm khảo sát, tọa độ RTK và hướng đặt.
- Bỏ nhãn dưới ký hiệu đèn chiếu sáng, đèn tín hiệu và nhóm đèn cũ. Chèn / cập nhật xóa nhãn BHT đã có, không xóa chữ khảo sát.
- Đèn do BHT quản lý được cập nhật kích thước theo giá trị tuyệt đối, không phóng to lặp. Ký hiệu đã sửa tay bằng lệnh CAD vẫn giữ tỷ lệ/biến đổi tay theo quy tắc hiện có.
- Tiếp tục phát hành runtime FAS và DLL protected; mở CAD không tự cập nhật bản vẽ.

## 0.6.47 — 10/10/2026

- Bản runtime dùng Lisp biên dịch FAS; không giao module LSP hoặc mã nguồn C#.
- Làm rối tên nội bộ Core/Bridge bằng Obfuscar có giữ public API. Palette giữ nguyên để bảo toàn các ràng buộc tên UI/reflection.
- Bộ cài kiểm tra runtime khai báo trong manifest trước khi thay bundle; gói protected chặn IncludeSource, file nguồn/PDB/mapping và DLL khác dấu kiểm tra sau xử lý.
- Giữ tính năng 0.6.46; không tự sửa bản vẽ khi khởi động. Chưa có chứng thư ký số, chống sao chép tuyệt đối hoặc bản ARX native.
## 0.6.46 — 10/10/2026

- Tách nhóm Đèn chiếu sáng và Đèn tín hiệu; giữ nhóm DEN cũ để không tự phân loại lại hồ sơ.
- Thêm Chọn loại đèn ngay trong hồ sơ: 5 mẫu chiếu sáng, 2 mẫu tín hiệu. Hiện tên mẫu đã gán, kiểm tra đúng nhóm và nhắc chọn mẫu trước khi chèn.
- Thêm tình trạng Mất mặt biển, còn trụ: vẽ trụ trống và nhãn rõ ràng, không có nền dấu hỏi; giữ mã biển cũ để khôi phục sau sửa chữa. Mô tả tru.matbb gợi ý trạng thái khi tạo hồ sơ từ RTK.
## 0.6.45 — 10/10/2026

- Bảng/biển hai trụ dùng một thanh nối hai chân và hai nhánh từ điểm khảo sát về giữa thanh, thay hai đường gấp độc lập.
- Bỏ hai chấm đặc ở chân trong block hai trụ; giữ mốc chân ẩn để xoay/nối chính xác và giữ dấu gốc tại điểm RTK.
- Giữ đường qua điểm trung gian đã chọn tay, bố trí CAP1 và biển một trụ. Mở bản vẽ không tự sửa ký hiệu; chọn hồ sơ rồi Chèn / cập nhật để áp dụng.
## 0.6.44 — 10/10/2026

- Bỏ tự cập nhật ký hiệu khi mở hoặc kích hoạt bản vẽ; không gọi Lisp dựng lại ký hiệu từ sự kiện Idle. Lệnh tương thích BHTSYMBOLUPGRADESCHEDULE không tạo lịch cập nhật.
- Người dùng chủ động chọn hồ sơ rồi Chèn / cập nhật để áp dụng ký hiệu mới. Giữ bản sửa chữ S.509a của 0.6.43.
- Đây là thay đổi giảm rủi ro khi khởi động sau báo lỗi truy cập bộ nhớ trong acdb24.dll; chưa xác nhận là nguyên nhân duy nhất của crash.
## 0.6.43 — 10/10/2026

- Sửa biển S.509a mất hai dòng CHIỀU CAO / AN TOÀN sau khi áp dụng nội dung. Bảng chữ mẫu tham khảo dùng cùng giá trị với bộ dựng biển, thay vì lấy ô trống từ DWG gốc.
- Khôi phục hai dòng chú thích đã lưu trống khi mở bảng nội dung hoặc Chèn / cập nhật. Giữ khoảng cách thực tế và các dòng chữ tùy chỉnh không rỗng. Tạo block nội dung mới để không dùng lại block lỗi đã lưu trong bản vẽ.
## 0.6.42 — 10/10/2026

- Biển tên đường I.449 dùng chiều cao mặt biển gốc 0,65 thay vì bị phóng lên 1,8. Khung mở rộng theo tên trong khoảng 1,2–3,2; tên quá dài được giảm cỡ đều, không bóp ngang chữ.
- Nhãn cọc H/Km giữ dạng H4/40, giảm chiều cao từ 0,35 xuống 0,22 theo tỷ lệ ký hiệu, đưa sát cọc và xoay theo ký hiệu với chiều chữ dễ đọc. Tọa độ RTK và nội dung hồ sơ không đổi.
- Sau khi cài, khởi động lại AutoCAD, chọn hồ sơ rồi Chèn / cập nhật để áp dụng cho ký hiệu đã đặt.
## 0.6.41 — 10/10/2026

- Đường dẫn tự động cho biển/bảng nhiều trụ gấp vuông theo trục của ký hiệu, đi phía ngoài chân trụ. Mỗi điểm RTK nối tới chân tương ứng; giữ đường qua điểm trung gian đã chọn tay. Nét nối liền màu 7, bề dày in 0,09 mm.
- Bổ sung chấm tròn đặc tại chân trụ trong block nhiều trụ và khung CAP1; vẫn giữ thông tin chân để nối và xoay đúng.
- Tạo dấu gốc tại các điểm RTK cho bảng nhiều trụ thuộc nhóm Bảng quảng cáo/Khác/Chưa xác định, tương tự nhóm Biển báo. Dấu ở layer ký hiệu nên vẫn thấy khi ẩn layer điểm.

Sau khi cài, chọn hồ sơ rồi Chèn / cập nhật để áp dụng cho biển đã đặt. Muốn đổi đường dẫn đã chọn tay sang kiểu tự động, đặt lại ký hiệu với Đường dẫn thẳng; chọn G nếu cần giữ vị trí tại tâm gốc. Tọa độ RTK không đổi.
## 0.6.40 — 10/10/2026

- Sửa lỗi eInvalidLayer ở Hiển thị / zoom tuyến khi tuyến nằm trên layer hiện hành. Chỉ bật/tan băng layer khi cần; giữ layer hiện hành và trạng thái khóa. Làm sáng sau khi kết thúc transaction và Regen.
- Sửa Đặt tự do: thay lựa chọn Theo tuyến bằng Vuông góc với tuyến, dùng tiếp tuyến tại điểm RTK và chiều tuyến A→B; thống nhất với xoay hàng loạt. Nếu chưa liên kết tuyến, hỏi chọn tuyến. Thiếu hình học hoặc quá xa tuyến sẽ báo lỗi và giữ biển cũ, không tự trả góc 0°.
- Lưu liên kết hướng tuyến để Chèn / cập nhật tiếp tục giữ hướng vuông góc. Giữ quy ước block thư viện và block tự chọn; vị trí RTK không thay đổi.
## 0.6.39 — 10/10/2026

- Sửa nút Tính tuyến cạnh ô lý trình: chỉ tính hồ sơ đang chọn; luôn hiện kết quả hoặc thông báo nguyên nhân thiếu dữ liệu. Không ghi đè lý trình cũ khi thiếu tuyến, thiếu mốc Km hoặc không chiếu được vị trí.
- Thêm Kiểm tra 2 cọc Km (`BHTKM2COC`) trong Tuyến & báo cáo. Chọn hai cọc, nhập lý trình, chọn điểm cần tính: nội suy theo hình chiếu lên đoạn thẳng nối cọc trong mặt phẳng XY. Kết quả ghi rõ ƯỚC TÍNH, kèm độ lệch và khoảng cách; không tự ghi hồ sơ và không ngoại suy ngoài hai cọc.
- Các lệnh lý trình/nhập mốc mở bảng kết quả kể cả khi thông báo ngắn. Chức năng Cập nhật lý trình ở tab Tuyến vẫn áp dụng toàn bộ hồ sơ theo tuyến tham chiếu.
- Bao gồm quản lý tuyến, chọn/làm sáng và sửa/xóa/chọn lại tuyến của 0.6.38.

Không có tim tuyến thực thì hai cọc không xác định được chiều dài đường cong. PL vẽ thử không được xem là cơ sở lý trình thiết kế. Chỉ dùng kết quả hai cọc cho đoạn đủ thẳng sau khi kiểm tra; muốn ghi vào hồ sơ dùng Ghi tay và chịu trách nhiệm xác nhận giá trị.
## 0.6.38 — 10/10/2026

- Chọn một dòng trong Dữ liệu tuyến để chọn và làm sáng hình học tương ứng trên CAD. Đổi dòng, đổi tab hoặc đóng bản vẽ sẽ bỏ làm sáng tuyến cũ. Hiển thị / zoom tuyến vẫn mở lớp ẩn và thu toàn bộ tuyến vào khung nhìn.
- Thêm Sửa tuyến, Chọn lại tuyến và Xóa tuyến ngay dưới bảng. Sửa được ID, loại tuyến, khoảng cách tối đa và phạm vi ngoại suy. Đổi ID cập nhật cả liên kết lý trình và hướng biển.
- Các nút điểm đầu, đảo chiều, đọc cọc, thêm/xóa mốc và chẩn đoán dùng tuyến đang chọn trong bảng.
- Xóa chỉ bỏ khai báo và liên kết tuyến; giữ Polyline, RTK và hướng biển hiện tại. Thay Polyline giữ đường cũ nhưng bỏ mốc Km và thông tin nguồn TDT cũ để tránh tính lý trình theo sai hình học. Sửa/xóa có thể Undo.
- Chặn tạo tuyến trùng ID và ngoại suy âm, gồm cả luồng nhập TDT. Đổi tên chỉ ghi lại hồ sơ liên quan; sửa thuộc tính không quét lại điểm RTK.

## 0.6.37 — 10/10/2026

- Ảnh của cả trang thư viện được nạp sẵn dưới dạng thu nhỏ: cuộn đến biển là thấy ảnh, không phải bấm vào. Giới hạn 60 mẫu/trang, giải phóng ảnh trang cũ.
- Gom tốc độ, tải trọng, kích thước, khoảng cách, giờ và thông tin cầu vào Nội dung biển. Bỏ các ô nhập bên ngoài; hỗ trợ thông số riêng cho từng mặt. Biển không có thông số có thông báo rõ.
- Thêm bảng Dữ liệu tuyến: danh sách tuyến đã nạp, nguồn, chiều dài, trạng thái hình học. Nhấp đúp hoặc Hiển thị / zoom tuyến để mở lớp và zoom trọn tuyến.
- BHTHUONGBIEN xoay đầu biển vuông góc về bên trái hướng A→B hoặc chiều tuyến. Đổi A/B để đảo phía. Giữ vị trí, tỷ lệ, RTK và khả năng Undo cả nhóm; giữ cách hiểu dữ liệu hướng cũ.
- Sửa BHTTRANGTHAI chỉ hiện tiêu đề trong báo cáo; đưa đủ các dòng thống kê vào cửa sổ kết quả.
- Bỏ lượt làm mới Palette bị gọi hai lần. Rà soát 51 lệnh Lisp công khai: không trùng tên; giữ lệnh tương thích và công cụ nâng cao.

## 0.6.36 — 2026-10-09

- Giảm thời gian mở/chọn biển: chia 60 biển mỗi trang, chỉ nạp ảnh thu nhỏ đang nhìn thấy, giải phóng ảnh ngoài vùng xem; gộp tìm kiếm khi gõ nhanh.
- Sửa lỗi Sequence contains no elements khi đóng hoặc đổi kích thước thư viện; ngừng nạp ảnh trong lúc hủy cửa sổ và đổi trang.
- Đổi phần CAP1 thành tùy chọn Bố trí khung/trụ nâng cao trong Nội dung biển. Mặc định ẩn, vẫn giữ bố trí đã lưu trong bản vẽ cũ.
- Thêm Nạp DWG… để chọn block riêng cho hồ sơ và thư viện 7 mẫu đèn: chiếu sáng đơn, đôi, trang trí, mặt đứng trái/phải, tín hiệu ba màu và cảnh báo vàng.
- Năm mẫu chiếu sáng được chuẩn hóa từ thư viện CAD có sẵn; hai mẫu tín hiệu do BHT tạo. Thư viện được đóng gói để dùng độc lập, không cần cài phần mềm nguồn.
- Chưa thay đổi cách xoay vuông góc theo đường tham chiếu; xử lý ở đợt tiếp theo.

## 0.6.35 — 2026-10-09

- Đã thêm lệnh BHTHUONGBIEN và nút ở tab Tuyến & báo cáo để xoay đầu biển hàng loạt theo chiều A→B của tuyến hoặc hướng chung chọn bằng hai điểm.
- Theo tuyến dùng tiếp tuyến tại vị trí RTK và chiều đã xác nhận; hai bên đường cùng hướng A→B. Hướng chung áp dụng được trong UCS xoay.
- Giữ vị trí, tỷ lệ XYZ, tọa độ RTK và điểm gấp khúc; cập nhật nhãn và nối đường dẫn vào chân thật của khung CAP1 sau khi xoay.
- Lưu chế độ hướng trong hồ sơ để giữ khi cập nhật; đặt riêng một biển sẽ thay chế độ hàng loạt của biển đó. Một Undo hoàn tác cả lần xoay.
- Giữ các thay đổi 0.6.34: 47 biển phụ, chọn đầu tuyến PL/TDT, dấu tròn đặc tại tâm X; đã gom các bản bàn giao cũ vào Old.

## 0.6.34 — 2026-10-09 (gói kiểm tra nội bộ)

- Đã khôi phục bộ lọc Biển phụ với đủ 47 mẫu mã S.; nhóm cao tốc giữ riêng các mẫu tương ứng.
- Tạo tuyến từ PL thường hoặc TDT 9.1 mở ngay bước chọn điểm đầu và xác nhận/đảo mũi tên. Cập nhật tim TDT giữ điểm đầu và chiều đã chọn.
- Đã sửa lỗi đổi đường dẫn của biển nhiều chân từ thẳng sang gấp khúc: bỏ qua entity cũ đã xóa khi ghép đường dẫn cho chân tiếp theo.

- Đã thêm block tròn đặc tại tâm X của từng điểm RTK liên kết với biển báo, để nhận ra vị trí gốc sau khi ẩn điểm. Block trên layer BHT_KYHIEU, màu 7, bán kính 0,07 đơn vị trước khi nhân tỷ lệ biển.
- Dấu giữ đúng tọa độ điểm khi di chuyển/xoay ký hiệu, đặt tại G hoặc dùng đường dẫn gấp khúc. Bỏ nền biển vẫn giữ dấu tròn đặc.
- Đồng bộ dấu khi đổi điểm, đổi nhóm, nâng cấp bản vẽ hoặc xóa hồ sơ; không tạo dấu trùng và không sửa điểm RTK gốc. Có thể chọn dấu để mở hồ sơ biển trong Palette.

## 0.6.33 — 2026-10-09

- Đã đổi chế độ tắt tô màu thành bỏ riêng nền biển, giữ hatch biểu tượng, mũi tên, đường nhánh và các khoảng trắng bên trong hình để in trắng đen.
- Đã giữ đầy đủ viền tròn, tam giác và khung biển khi bỏ nền; viền tạo từ đường bao gốc, nét khung 0,25 mm khi in.
- Đã rà 469 DWG nguồn / 467 mẫu trong danh mục; bổ sung sửa số trong TEXT/MTEXT và attribute, gồm khoảng cách, kích thước, tải trọng, tốc độ theo làn, giờ, tần số và số điện thoại.
- Đã thêm nhãn thông số theo đơn vị, nhận dấu phẩy thập phân, giữ đơn vị mẫu, cho nhập khoảng cách 0 và giờ qua đêm; báo lỗi khi nhập sai số, đơn vị hoặc giờ.
- Nội dung vẫn lưu riêng từng mặt trong DWG, giữ mẫu thư viện và bố trí CAP1 2D. Các bản sao để in giữ thứ tự vẽ và vùng rỗng của biểu tượng.

## 0.6.32 — 2026-10-09

- Đã sửa bộ lọc biển chỉ dẫn cao tốc đang trỏ sang biển phụ; bỏ mục lọc Biển phụ trống, giữ các mẫu trong Tất cả/tìm kiếm.
- Đã đổi tên cửa sổ thành Thư viện biển báo BHT, bỏ chữ ADSCivil trong mã ứng dụng; bộ cài dùng SignLibrary/BHT.
- Đã thêm bảng nội dung attribute theo từng mặt, giữ riêng các mặt cùng mã; tách địa danh mẫu khỏi nội dung hồ sơ, lưu Unicode NFC và thu hẹp chữ dài theo ô mẫu.
- Đã triển khai 10 bố trí CAP1 cho trụ/khung 2D, sơ đồ xem trước, khoảng cách mặt và chiều cao đáy; nối RTK theo chân thật của bố trí.
- Đã giữ nội dung và bố trí trong hồ sơ/bản nháp, bảo toàn hatch và thứ tự vẽ khi clone. CAP1 chưa có tính toán kết cấu hoặc tự đối chiếu địa danh với hồ sơ ngoài.

## 0.6.31 — 2026-10-08

- Đã chuyển nguồn biển mặc định sang ADSCivil, đọc sáu danh mục và các DWG ngoài danh mục; giữ các biến thể có số, dấu phẩy và dấu nháy.
- Đã sửa R.415a/b bằng CAD gốc ADSCivil. Khôi phục thứ tự vẽ để nền không che xe, vạch phân làn và dải hủy.
- Đã kèm 1.630 tài nguyên trong bộ cài cục bộ, đối chiếu SHA-256; máy nhận không cần ARX ADSCivil hoặc TDT.
- Đã giữ thông số biển, nhiều mặt, hai chân, chỉ nét và dữ liệu RTK khi nâng cấp. Sửa quy ước hướng đặt cho tên block ADSCivil, giữ API cũ và thêm BHTADSBLOCK.
- Đã nghiên cứu XML/JSON và ghi hướng phát triển khung/trụ. Không tìm thấy mã nguồn C++/Lisp của phần đã biên dịch.

## 0.6.30 — 2026-10-08

- Tắt Tô màu tất cả biển áp dụng cả bảng quảng cáo, Khác, Chưa xác định và bảng chỉ dẫn; bỏ hatch, nền MText và bề rộng nét tô trong block riêng, giữ block màu gốc để bật lại.
- Vẽ lại R.415a/b bằng đường cong CAD và cung tròn, đủ sáu hình xe; lưu nguồn SVG để chỉnh sửa, đổi cache hình và đặt dải hủy phía trên biểu tượng.
- Tự cập nhật ký hiệu biển/đèn đã có khi mở bản vẽ sau khi nạp đúng lõi Lisp; giữ điểm chèn, góc xoay, tỷ lệ, chế độ đặt và RTK. Lưu bản vẽ để giữ cập nhật.
- Thêm Xóa hồ sơ có xác nhận; dọn ký hiệu, nhãn, đường nối và liên kết ảnh của hồ sơ đã xóa; giữ RTK, ảnh gốc và hồ sơ khác.
- Đạt 178 kiểm thử Core, 149 kiểm tra giao diện, hồi quy CAD và các ca cập nhật/xóa/tô màu. Bộ cài một file giữ thông báo hoàn tất và chặn cài lặp.
## 0.6.29 — 2026-10-08

- Đã chỉnh chiều dài trụ IE.472b để dùng hai chân và ghép nhiều mặt không giữ trụ giữa thừa; đổi cache IE.472a/b và bổ sung ca kiểm tra native cho cả hai chế độ.
- Giữ nhãn đèn phía dưới, mẫu trạm thu phí và chọn biển modeless của 0.6.28. Bản 0.6.28 là gói kiểm tra nội bộ; bộ cài bàn giao là 0.6.29.
- Đạt 178 kiểm thử Core, 144 kiểm tra giao diện và hồi quy CAD/Unicode/G/T/hai chân, gồm hai biển trạm thu phí khi không có TDT.

## 0.6.28 — 2026-10-08

- Đặt nhãn đèn chiếu sáng/tín hiệu bên dưới ký hiệu; giữ vị trí, góc quay và điểm RTK khi cập nhật.
- Bổ sung block native IE.472a/b nền xanh, khung trắng bo góc và chữ trắng; IE.472a nhập khoảng cách mét và xem trước số thực tế, lưu riêng từng mặt.
- Chuyển thư viện chọn biển sang modeless để tiếp tục zoom/pan CAD; giữ hồ sơ đang sửa, đóng khi đổi hồ sơ/bản vẽ và hủy lựa chọn khi Đóng/Escape.
- Đạt 178 kiểm thử Core, 144 kiểm tra giao diện và hồi quy native AutoCAD, thư viện, Unicode, G/T, tỷ lệ, hai chân; kiểm tra IE.472a/b khi không có TDT.

## 0.6.27 — 2026-10-08

- Đã bỏ sáu đoạn trắng thừa sát mép phải R.415a/b, phát hiện khi xem ảnh xuất từ AutoCAD; đổi cache để cập nhật hình cũ.
- Giữ sửa W.207a, tốc độ DP.134/R.306 và tự đếm mặt biển của 0.6.26. Bản 0.6.26 là gói kiểm tra nội bộ; bộ cài bàn giao là 0.6.27.
- Đã đạt 176 kiểm thử Core, 139 kiểm tra giao diện và hồi quy CAD/ký hiệu/Unicode/G/T/hai chân.

## 0.6.26 — 2026-10-08

- Đã sửa W.207a có đủ hai nhánh đường không ưu tiên đối diện; ảnh thư viện dùng cùng đường CAD đã sửa.
- Đã vẽ lại R.415a/b theo mẫu người dùng, làm mượt hình xe, giữ cửa kính/bánh xe và bỏ nét thừa ở mép biển.
- Đã cho nhập tốc độ trên DP.134 và R.306, lưu giá trị riêng từng mặt, xem trước số đã nhập và kiểm tra số nguyên 5–130 km/h.
- Đã tự đếm Số mặt biển từ danh sách, kể cả một mã chính; chọn lại một mặt xoá danh sách nhiều mặt trước đó. Số trụ/chân vẫn nhập riêng.
- Đã đạt 176 kiểm thử Core, 139 kiểm tra giao diện và hồi quy AutoCAD 2024, gồm tô màu/chỉ nét, hai chân và đặt ký hiệu G/T.
- Bộ cài một file EXE giữ thông báo hoàn tất và chặn cài lặp; bản cũ được lưu trong Old.

## 0.6.25 — 2026-10-08

- Đã vẽ số trụ và dấu chân theo Số trụ/chân cho biển báo, bảng quảng cáo, Khác và Chưa xác định; bảng tên một trụ có dấu chân.
- Đã nối riêng từng điểm RTK đo chân vào chân tương ứng của ký hiệu nhiều trụ, thay đường nối từ điểm trung bình; giữ tọa độ đo.
- Đã cập nhật đường nối khi đặt/xoay/đổi tỷ lệ, thêm/gỡ điểm hoặc đổi số trụ; cập nhật lặp không tạo đường trùng.
- Đã hiện nhắc thiếu điểm chân trong hồ sơ và cảnh báo trong BHTKT; block tùy chỉnh giữ hình người dùng.
- Đã đạt 171 kiểm thử Core, 134 kiểm tra giao diện, hồi quy CAD/ký hiệu/Unicode/G/T và kiểm tra hai trụ/hai đường nối.
- Đã kiểm tra 7 trường hợp sao lưu/khôi phục và 39 kiểm tra bộ cài EXE độc lập.

## 0.6.24 — 2026-10-08

- Đã đổi nút đặt X/sắp nhãn thành Tỷ lệ ký hiệu và nhãn RTK, chọn riêng cỡ X và cỡ chữ.
- Đã tách Cập nhật nhãn RTK để tạo nhãn thiếu và sắp lại nhãn tự động; giữ nhãn dời tay và tỷ lệ đã lưu.
- Đã giữ dòng đầu và các điểm đang chọn khi làm mới danh sách RTK.
- Đã đạt 171 kiểm thử Core, 131 kiểm tra giao diện và hồi quy CAD/ký hiệu/Unicode/G/T, gồm kiểm tra tỷ lệ RTK độc lập.
- Đã kiểm tra bộ cài với 39 kiểm tra EXE độc lập và 7 kiểm tra sao lưu/khôi phục.

## 0.6.23 — 2026-10-08

- Đã hiện màn hình cài đặt thành công, bỏ nút Cài đặt BHT và chỉ giữ Hoàn tất để đóng.
- Đã chặn cài lặp sau hoàn tất và bấm liên tiếp khi đang xử lý; cài lỗi vẫn cho thử lại.
- Đã đồng bộ tên file bộ cài EXE trong hướng dẫn khi tăng phiên bản.

- Đã đạt 171 kiểm thử Core, 39 kiểm tra bộ cài EXE và 7 kiểm tra cài đặt/sao lưu/khôi phục.

## 0.6.22 — 2026-10-08

- Đã thêm bộ cài BHT-Setup-0.6.22.exe: một file để gửi, kèm bundle và phông chữ, tự giải nén và kiểm tra dữ liệu.
- Đã thêm giao diện cài đặt tiếng Việt, cài cho người dùng hiện tại, giữ bản cũ và báo lỗi AutoCAD đang mở.
- Đã giữ gói ZIP có mã nguồn; giải thích CMD cần các file đi kèm và yêu cầu AutoCAD tương thích.

- Đã đạt 171 kiểm thử Core, hồi quy CAD/ký hiệu/Unicode/G/T, 7 kiểm tra cài đặt và kiểm tra EXE độc lập/giao diện/toàn vẹn dữ liệu.

## 0.6.21 — 2026-10-08

- Đã thêm nút Tỷ lệ biển / nhãn, chọn riêng kích thước hình biển và nhãn mã; có mức gợi ý, nhập tùy chỉnh và về chuẩn 1:1.
- Đã lưu tỷ lệ theo bản vẽ, cập nhật các biển đã chèn và áp dụng cho biển chèn sau; giữ vị trí, hướng, đường dẫn và dữ liệu RTK.
- Đã nhóm nút theo tạo/lưu hồ sơ, chọn biển/tỷ lệ, đặt/chèn ký hiệu; đưa thêm/gỡ điểm về cạnh danh sách RTK.

- Đã đạt 171 kiểm thử Core, 111 kiểm tra form, hồi quy CAD/ký hiệu/Unicode/G/T và 7 kiểm tra bộ cài.

## 0.6.20 — 2026-10-07

- Đã vẽ lại nét R.415a/b và ký hiệu điện W.239a bằng đường CAD sạch, bỏ răng cưa từ ảnh nguồn.
- Đã tránh tạo đường bao trùng khi bỏ Hatch, nối đường bao cong và giữ cung tròn; số/thuộc tính trong block lồng nhau kế thừa màu trắng khi tắt tô màu.
- Đã thêm nhãn riêng cho từng mặt biển, giữ thứ tự và các giá trị mét/tấn; dọn nhãn khi bỏ mặt, không nhân đôi khi cập nhật.
- Đã giữ dòng đầu của danh sách sau làm mới và vị trí vùng nhập khi quay lại từ thư viện biển.
- Đã đạt 171 kiểm thử Core, 75 kiểm tra giao diện, hồi quy CAD/ký hiệu/Unicode/G/T, hình PDF xuất trực tiếp từ CAD và 7 kiểm tra bộ cài.

## 0.6.19 — 2026-10-07

- Đã thêm ô Trọng lượng (tấn) cho S.505a, hiển thị 8T dưới hình xe như biển khảo sát.
- Đã lưu giá trị theo từng mặt biển; nhận dấu phẩy thập phân, chặn giá trị sai và giữ mẫu chỉ có xe khi để trống.
- Đã tạo mẫu CAD riêng theo trọng lượng, giữ hình xe gốc, nền trắng/viền đen và hỗ trợ bật/tắt Hatch độc lập.
- Đã kiểm tra Core, bộ chọn biển, hình học CAD, lưu/đồng bộ hồ sơ nhiều mặt và bộ cài.

## 0.6.18 — 2026-10-07

- Đã sửa Tô màu tất cả biển tự bật lại do làm mới đọc giá trị cũ khi thao tác còn chờ xử lý.
- Đã giữ lựa chọn đang chờ, khóa checkbox khi xử lý và đọc lại trạng thái bản vẽ khi hoàn tất hoặc bị từ chối.
- Đã bỏ qua kết quả Lisp cũ sau khi đổi/gắn lại bản vẽ, tránh ghi đè thao tác mới.
- Đã đạt 166 kiểm thử Core, 63 kiểm tra form/đồng bộ trạng thái, hồi quy ký hiệu/Unicode/G/T và 7 kiểm tra bộ cài.

## 0.6.17 — 2026-10-07

- Đã thêm nhấp đúp trên dòng hồ sơ để thu phóng tới vị trí RTK và bỏ nút Thu phóng dư ở vùng nhập.
- Đã thêm G (Gốc) và T (Tuỳ chọn) khi đặt ký hiệu, ở cả bước điểm trung gian và vị trí cuối.
- G đặt tại tâm X và bỏ đường dẫn cũ; T cho chọn vị trí trên CAD; giữ điểm RTK, hướng đã chọn và thao tác hủy.
- Đã đạt 166 kiểm thử Core, 49 kiểm tra form, 9 tình huống G/T tương tác AutoCAD, bộ hồi quy ký hiệu/Unicode và 7 kiểm tra bộ cài.

## 0.6.16 — 2026-10-07

- Đã sửa form tự cuộn xuống khi tạo hồ sơ từ RTK; gom cập nhật bố trí để giảm giật, giữ vị trí cuộn khi làm mới hoặc đổi nhóm.
- Đã bỏ tên cấu hình nội bộ khỏi thông báo trùng Cọc tiêu/Cột Km.
- Đã thêm ô Tên trên bảng cho Bảng quảng cáo và Khác, dùng dữ liệu mô tả hiện có; ký hiệu mặc định là hình chữ nhật Hatch xanh, viền và chữ trắng.
- Đã đổi ký hiệu Chưa xác định sang cùng mẫu bảng với chữ Chưa xác định ở chính giữa.
- Đã kiểm tra 44 tình huống form, 21 kiểm tra hình học bảng, hồi quy ký hiệu/Unicode/chế độ không có TDT, 166 kiểm thử Core và 7 kiểm tra bộ cài.

## 0.6.15 — 2026-10-07

- Đã tách Lưu hồ sơ khỏi thao tác chèn/cập nhật ký hiệu; ghi lý trình tay cùng hồ sơ và chỉ lưu sau khi xác nhận hộp tuỳ chọn đặt.
- Đã giữ nội dung chưa lưu khi làm mới/lọc, hỏi trước khi bỏ thay đổi và khôi phục bản nháp theo bản vẽ trong cùng phiên.
- Đã thêm tìm hồ sơ không dấu, vùng danh sách có thể đổi chiều cao, tên nhóm/phía đường dễ đọc và bố trí nút cho bảng hẹp.
- Đã sửa Palette tự ẩn khi đổi bản vẽ và cập nhật số phiên bản/đường dẫn hiện tại trong tài liệu.
- Đã bổ sung 26 kiểm tra giao diện/luồng hồ sơ; giữ 166 kiểm thử Core và bộ hồi quy AutoCAD/bộ cài.

## 0.6.14 — 2026-10-07

- Giữ phần thập phân của bán kính tìm ảnh/điểm trên Palette, kể cả bán kính nhỏ hơn 1 m; giá trị không hợp lệ dùng mặc định 10 m.
- Chặn kích thước chia chuỗi không dương trong Core và Lisp, tránh lặp vô hạn.
- Chiếu tuyến giữ đoạn ngắn khác 0 và tính đủ chiều dài tích lũy.
- Bỏ 5 hàm Lisp nội bộ và 1 hàm Palette không còn được gọi; rút gọn nhật ký trong mã và ghi chú phân cách trùng.
- Đồng bộ tên cache block TDT giữa Lisp và Bridge; sửa phiên bản/đường dẫn build trong README và cập nhật kiểm thử hồi quy.

## 0.6.13 — 2026-10-05

- Giữ tỷ lệ biển phụ theo chiều lớn nhất, tránh biển ngang phình rộng khi ghép cùng biển chính.
- Đưa hatch gạch chéo đỏ lên trên biểu tượng; giữ nền trắng và màu đen khi in các biển nhập từ TDT.
- Sửa nền trắng/viền đen I.401–I.402 và bổ sung mẫu CAD IE.473.
- Cho nhập giờ riêng ở R.E.9b/R.E.10b trong thư viện; lưu theo từng mặt và tạo block có giờ thực tế.
- Bổ sung test hình học/hatch/thời gian trong AutoCAD. Test trên bản sao đã đạt; test MCP trực tiếp chưa hoàn tất.

## 0.6.12 — 2026-10-05

- Gom nội dung I.439 (tên cầu, lý trình trên biển, tên đường) vào Thư viện biển; bỏ ba ô nhập trùng trong hồ sơ của mọi mã biển.
- Giữ nội dung biển đã lưu khi mở, sửa và lưu hồ sơ; mã biển/nội dung được chọn qua thư viện như trước.

## v0.6.10 — 2026-10-04

## 0.6.11 — 2026-10-05

- Sửa chọn hướng CAD bị quay ngược đầu cọc; kiểm tra UCS xoay và các thiết lập góc.
- I.439 dùng lại khung đôi, phông Giaothong1, nền xanh và chân trụ mẫu cũ; ghép dòng KM38+723-ĐT.830 và giữ viền trắng khi in.
- Chuyển điểm chèn cọc Km về giữa thanh đen; ẩn nhãn ngoài cọc Km và cọc tiêu chưa có lý trình.
- Tự ghi lý trình từ Km/H khi lưu, bỏ nhập tay trùng lặp, ghi rõ nguồn và giữ các liên kết.

- Sửa ảnh và mã biển R.415a/b, W.239a/b; sửa hình W.205c/W.207a và thứ tự hatch che biểu tượng.
- Nhập tên cầu, lý trình và tên đường trong bảng chọn I.439; lưu nội dung và tự co chữ vừa khung.
- Thêm số Km/H trên cọc tiêu dạng H9/39 và số Km trong block cọc Km, có tùy chọn bật/tắt.
- Kiểm tra màu khi in, lưu/mở lại dữ liệu, bảng chọn và các chức năng biển hiện có trong AutoCAD 2024.

## v0.6.9 — 2026-10-02

- Đọc cọc trong hình hiển thị của tuyến TDT và block lồng nhau, lấy vị trí vạch cọc trên tim.
- Giữ mũi tên trong lúc chọn/đảo chiều tuyến và xác nhận; sửa cập nhật tim có cung, giữ handle tham chiếu.

## v0.6.8 — 2026-10-02

- Đã bỏ 14 bí danh Lisp và hai bí danh mở bảng; có bảng lệnh thay thế cho script CAD.
- Đã chặn tốc độ/mét sai và mã không khớp, tránh vẽ biển mặc định khi nhập sai. Giữ các cách ghi tốc độ cũ và mã ghép W.239a + S.509a.
- Đã giới hạn bộ chọn ở 20 mặt/trụ và kiểm tra từng mặt trước khi xác nhận; kiểm tra block thiếu và cụm rỗng.
- Đã từ chối giá trị mét quá ba số thập phân thay vì âm thầm làm tròn, kể cả làm tròn về 0.
- Đã bỏ control không dùng, hai bản sao thư mục ảnh và tệp biên dịch trong mã nguồn đóng gói. Ảnh được nhúng trong DLL Palette.
- Đã đồng bộ README/hướng dẫn với phiên bản và thêm kiểm tra chống sai tên ZIP cài đặt.

## v0.6.7 — 2026-10-02

- Sửa đăng ký hàm ghép cụm nhiều mặt biển để Lisp gọi được; kiểm tra chèn/cập nhật 1 trụ với W.239 và S.509a@7 qua API Palette.
- Thanh thao tác hồ sơ có hai hàng cố định; Đặt tự do và Thư viện biển nằm trên cùng.
- Hiển thị hướng dẫn khi Lisp trả lỗi rỗng.

## v0.6.6 — 2026-10-02

- Nhúng ảnh biển báo vào DLL Palette, bảo đảm R.122 và toàn bộ thư viện có ảnh ngay cả khi thiếu thư mục Images. Giữ hỗ trợ ảnh TDT cho mã ngoài bộ ảnh đi kèm.
- Kiểm tra riêng ảnh R.122, P.131c, S.508a/b và toàn bộ mục thư viện bằng WinForms.

## 0.6.5 (2026-10-02)

- Tiếp tục từ v0.6.3; không lấy v0.6.4 làm nền.
- Gốc cọc tiêu tại chân đuôi; nhãn trên đầu, hướng theo ký hiệu. Đặt tự do và xoay theo tuyến áp dụng mọi nhóm.
- Hộp tùy chọn đặt tự do riêng; Palette chia thông tin hồ sơ, ký hiệu CAD, vị trí/ảnh. Bỏ BHTNANGCAP, BHTVEMODEL, BHTROUTE đời cũ.
- Dựng đủ nhiều mặt trên một trụ, giữ thứ tự và các biến thể số; lặp mã vẫn tạo đủ tấm. Nút chèn/cập nhật lưu thay đổi trước khi dựng ký hiệu.
- Nhập giá trị mét cho S.501, S.502, S.509a và P.117–P.120; xử lý cả số TDT bị tách thành nhiều TEXT. Không đổi chữ hay mã hiệu khác.
- 376 mã biển hợp lệ có ảnh thật; bỏ 36 tiêu đề/mục trùng của XML cũ khỏi bảng chọn. Ảnh bổ sung có nguồn QCVN hoặc hình từ block CAD và được đóng gói.

## 0.6.3 (2026-10-01)

- Palette chỉ tạo/hiện khi gọi BTH/BHT; bỏ tự nạp giao diện từ Lisp và lệnh khôi phục Palette cũ. Đóng bảng hoặc chuyển bản vẽ ngắt theo dõi dữ liệu và timer; gọi lệnh để mở lại.
- “Tô màu biển” nằm cùng hàng “Chèn biển tự do”, lưu theo bản vẽ và cập nhật mọi biển BHT đã chèn. Biển chèn sau theo cùng lựa chọn; không thay Hatch ngoài BHT.
- Nhãn biển đặt dưới, căn giữa khung block và quay cùng biển kể cả có điểm trung gian. Cỡ chữ tự tính theo chiều cao block, bỏ ô cỡ nhãn CAD; trường cũ giữ trong hồ sơ nhưng không dùng cho biển.
- Rút gọn tiêu đề hồ sơ mới; hướng dẫn phân nhóm chuyển vào tooltip. Đổi “Cho phép tạo hồ sơ mới” thành “Cho phép dùng chung điểm RTK”.
- Core 127 ca đạt; biển/native/offline/WinForms đạt; REVIEW 10, S0 20, V5 21 ca đạt. TX thiếu fixture. GUI chỉ đọc được Start của bản cũ, chưa thao tác chuột v0.6.3; tỷ lệ số mặc định TDT chưa xác minh.
## 0.5.1 — 2026-10-01

- Đã sửa chữ CAD mặc định sang `vnromanc.shx` và tự chuyển Unicode sang TCVN3 tại lớp hiển thị, gồm nhãn điểm RTK, nhãn đối tượng và chữ BHT tạo trên CAD. Hồ sơ, Palette, XData và báo cáo giữ Unicode.
- Đã đồng bộ phiên bản `0.5.1` cho Lisp, DLL, bundle, bộ cài và tên gói phát hành.
- Đã bổ sung công cụ tăng/kiểm tra phiên bản và chặn phát hành cùng số phiên bản khi nội dung thay đổi.
- Giữ các sửa lỗi biển báo: Hatch, chiều cao chuẩn, tốc độ P.127, block riêng từng hồ sơ, thư viện ảnh và xoay theo tuyến.

# CHANGELOG

## 0.6.2 (2026-10-01)

- Layer BHT_KYHIEU mặc định ACI 7 (trắng trên nền tối); layer đỏ mặc định cũ đổi sang trắng, màu tùy chỉnh khác giữ nguyên. Biển không tô màu dùng nét ByBlock/layer 0 để theo màu INSERT thay vì màu xanh cố định của nguồn TDT.
- Chặn con lăn đổi lựa chọn trong mọi ComboBox Palette và thư viện ảnh.
- Nhãn CAD mặc định 0.35 thay cho 1.5; thêm ô cỡ nhãn theo hồ sơ và đặt nhãn tự động cao hơn mặt biển.
- Thao tác nối tiếp từ Palette chờ kết thúc EXECUTEFUNCTION trước khi xử lý kết quả; ngăn bấm lặp khi BHT đang chạy. Không bỏ kiểm tra lệnh CAD đang chạy.
- Đặt tự do có đường dẫn thẳng, gấp khúc tự động hoặc điểm trung gian; hướng ngang/theo tuyến/chọn trên CAD/nhập góc. Luồng nhanh chỉ cần chọn vị trí, hủy không ghi hồ sơ.
- Core, CAD Core Console, offline và WinForms đạt; AutoCAD GUI chưa xác minh do công cụ capture timeout sau một lần phục hồi.


## 0.6.1 (2026-10-01)

- Thêm “Tô màu báo hiệu (Hatch)” trong thư viện ảnh và hồ sơ đối tượng; mặc định bật, lưu riêng từng hồ sơ.
- Tắt tô màu tạo biến thể block riêng, ẩn Hatch/Solid và tạo đường bao, xử lý cả block lồng nhau; hỗ trợ biển TDT, biển nội bộ và block tùy chỉnh.
- Bật lại khôi phục block có màu; đồng bộ giữ vị trí, góc quay, tỉ lệ và không đổi FILLMODE toàn bản vẽ.
- Xác nhận chuỗi “Tô mầu biển báo” / “Không tô mầu biển báo” / HatchBB trong RoadSignsUI.arx. Giao diện TDT trực tiếp bị chặn bởi lỗi ProjectDH.arx.
- 124 kiểm tra Core đạt; kiểm tra AutoCAD Core Console, chế độ không TDT và WinForms đạt. Chưa nghiệm thu thao tác giao diện AutoCAD thật.


## 0.6.0 (2026-10-01)

- Bắt đầu chuyển Lisp sang .NET: Unicode NFC, encode/decode TCVN và suy ra biến thể tốc độ dùng BHT.Bridge/BHT.Core; giữ fallback Lisp và kiểm thử tương đương cả hai engine.
- Tách lõi Lisp thành 12 module và loader chung; bundle tự nạp Bridge, bộ kiểm phiên bản kiểm đủ module.
- Thêm chế độ BUILTIN không dùng TDT; đọc XML theo tên thuộc tính, chặn DTD/external entity, từ chối container TDT chưa hỗ trợ và fallback an toàn. Biển tốc độ dự phòng có số thực 5–130, không nhầm 120 với 20.
- Bộ cài kiểm SHA256/phiên bản/thiếu file/file lạ/path traversal trước khi cài, giữ bản cũ và rollback khi thay thế lỗi. Gói mặc định không kèm mã nguồn C#; -IncludeSource chỉ dùng khi cần bàn giao nguồn.
- Core/AutoCAD/WinForms, 6 ca lỗi nguồn TDT và 7 ca bộ cài đạt. Kiểm thử giao diện AutoCAD thật chưa xác nhận: công cụ capture timeout hai lần; không tính kiểm thử tự động là nghiệm thu UI.

## 0.5.5 (2026-10-01)

- Bỏ giao diện DCL dự phòng và hai lệnh BHTDCL/BHTUITEST theo review mới; giữ Palette .NET và 67 lệnh Lisp còn lại.
- Giữ hàm bỏ dấu tìm kiếm, tình trạng và các hàm trạng thái; sửa BHTTEST để không gọi helper DCL đã xóa.
- Cập nhật BHTHELP/BHTLOAD, thông báo nạp và hướng dẫn cài/sử dụng; khi Palette lỗi hướng dẫn kiểm tra DLL và thử lại.
- Thêm runner kiểm tra review dùng profile riêng; cập nhật kỳ vọng kiểm thử theo font Unicode VNRomancUpdate.shx và 17 hàm API hiện có.
- Build/122 kiểm tra Core, REVIEW/S0/V5 và bộ kiểm thử biển/phông/WinForms đạt. TX chưa chạy do thiếu bản vẽ proxy mẫu. Đồng bộ phiên bản 0.5.5/0.5.5.0.

## 0.5.4 (2026-10-01)

- Xử lý review v0.5.3: rút gọn bảng TCVN/NFC, giữ đủ chữ hoa và đầu vào cũ; sửa ghép dấu trên chữ đã ghép một phần và thứ tự dấu đảo.
- Dùng helper chung cho ẩn/hiện nhãn, giữ 69 lệnh và alias; làm rõ các section và kết quả đếm hàm trong report.
- Chuẩn hóa ID khi chèn biển tự do; bổ sung hồi quy ID thiếu, hồ sơ không có RTK và đảo thứ tự điểm trung gian của đường dẫn.
- 122 kiểm tra Core, kiểm thử AutoCAD/WinForms và chính sách tăng phiên bản đạt; đồng bộ Lisp/DLL/bundle/bộ cài 0.5.4/0.5.4.0.

## 0.5.3 (2026-10-01)

- Thêm “Chèn biển tự do” trên Palette và lệnh BHTBIENTUDO: chọn hướng, thêm/xóa điểm trung gian, rồi chọn vị trí biển. Hủy trước bước cuối không ghi thay đổi.
- Lưu bố trí tự do theo hồ sơ; đường dẫn gấp khúc giữ qua lần cập nhật, theo điểm cuối khi di chuyển biển. BHTKYHIEU > R bỏ bố trí tự do để trở về tự động theo tuyến.
- Giữ RTK, liên kết tuyến, ảnh và nội dung biển. Quy đổi hướng từ UCS sang WCS; tiếp tục dùng phông Unicode VNRomancUpdate.shx.
- Build và kiểm tra Core, AutoCAD, nhập tương tác và thư viện ảnh đạt; đồng bộ phiên bản 0.5.3/0.5.3.0.

## 0.5.2 (2026-10-01)

- Dùng VNRomancUpdate.shx Unicode, sửa Ê/ê và chữ hoa có dấu; giữ tên kiểu cũ, chuyển TEXT TCVN3 của BHT khi cập nhật phông, bảo toàn dữ liệu Unicode/vị trí/góc quay.
- Thư viện biển thêm ảnh xem trước lớn, tốc độ P.127 gợi ý từ hồ sơ, kiểm tra giá trị và chọn nhiều mặt cùng trụ có thêm/bỏ/sắp thứ tự; hủy không ghi thay đổi.
- Ghi nghiên cứu menu/tài nguyên TDT 9.1 và giới hạn xác nhận giao diện do lỗi ProjectDH.arx; không sửa tài nguyên TDT gốc.
- Đồng bộ phiên bản 0.5.2/0.5.2.0, kèm phông mới, chặn phát hành thiếu phông mặc định; kiểm thử Core, AutoCAD và WinForms đạt.

## 0.4.6-fix3 (2026-09-29)

- Thanh thẻ dọc bên phải Palette có tên thẻ (trước đây là các ô xanh trống). Nguyên nhân: 0.4.6/fix2 xoay chữ −90° bằng `TextRenderer` (GDI) — GDI bỏ qua phép xoay của `Graphics` và ô chữ sau khi xoay chỉ dài 34 px, nên chữ bị vẽ lệch/cắt mất. Nay chữ ngang trong ô 86 × 38 px (độ rộng dải thẻ giữ nguyên 86 px), tự xuống 2 dòng khi dài. Tên thẻ: **Tổng quan**, **Điểm RTK**, **Ảnh hiện trường**, **Hồ sơ đối tượng**, **Tuyến & báo cáo**; rê chuột lên thẻ để xem mô tả. Thẻ thường nền `#065F46` chữ `#D1FAE5`; thẻ đang chọn nền `#D1FAE5` chữ `#065F46` và vạch đậm bên trái.
- Lỗi và cảnh báo cần người dùng xử lý hiện cửa sổ thông báo (tiêu đề `BHT`, biểu tượng lỗi/cảnh báo) và vẫn in ra dòng lệnh: lỗi `BHT lỗi: …` của các lệnh BHT, “chưa có tuyến. Dùng BHTTUYEN trước.”, tim TDT dạng proxy, không nạp được `BHT.Bridge.dll`, không mở được file, không ghi được file (đang mở trong Excel), dữ liệu nhập không hợp lệ… Hủy lệnh (Esc, `*Cancel*`) và thông tin thường chỉ in dòng lệnh. Tắt cửa sổ bằng `(setq *bht-popup* nil)`. Không hiện cửa sổ khi chạy script `.scr` hoặc trong AutoCAD Core Console (kiểm thử tự động không bị treo).
- Palette: lỗi phía Palette (lỗi đọc/ghi bản vẽ, gọi Lisp thất bại, Lisp không cùng phiên bản, xuất Excel lỗi, dữ liệu nhập sai…) hiện MessageBox `BHT`. Khi lệnh gửi từ Palette báo lỗi, vùng trạng thái hiện “Lệnh … THẤT BẠI.” kèm nội dung lỗi trên nền đỏ nhạt (trước đây hiện “đã kết thúc. 1 dòng kết quả.” như thành công); lệnh có cảnh báo hiện nền vàng nhạt.
- Thông báo khi tim TDT còn là proxy giải thích rõ: phiên AutoCAD này chưa nạp TDTSolution 9.1 nên không nhận ra tim tuyến; lưu và đóng AutoCAD, mở lại bằng biểu tượng/profile TDTSolution 9.1 (cắm khóa USB TDT nếu phần mềm yêu cầu), mở bản vẽ rồi chạy lại bước 1. BHT không sửa tim TDT gốc.
- Đồng bộ phiên bản: `BHT-0.4.6-fix3.lsp`, `*bht-version*` = `0.4.6-fix3`, DLL `0.4.6.3`, `PackageContents.xml` `AppVersion` `0.4.6.3` (giữ ProductCode/UpgradeCode), bộ cài fix1 (CMD ASCII + PS1 UTF-8 BOM) với phiên bản `0.4.6-fix3`.
- Dữ liệu bản vẽ, POINT RTK, thuật toán nhãn và lý trình, bố cục Palette và bảng màu fix2 giữ nguyên.

## 0.4.6-fix2 (2026-09-29)

- Sửa lỗi bước “1. Lấy hoặc cập nhật tim từ TDT 9.1” (`BHTTUYENTDT`) báo `BHT lỗi: no function definition: FBOUNDP`: `fboundp` không phải hàm AutoLISP. Thay bằng `bht:fn-defined-p` (kiểm tra `type` là `SUBR`/`USUBR`/`EXRXSUBR`). Đã rà toàn bộ Lisp và C#: không còn hàm ngoài AutoLISP nào khác.
- `BHTTUYENTDT` tự `NETLOAD` `BHT.Bridge.dll` nằm cạnh file Lisp khi hàm `BHTTDT91ROUTE` chưa được đăng ký, rồi mới báo lỗi; báo rõ khi ID tuyến hoặc khoảng cách tối đa không hợp lệ (trước đây lệnh kết thúc im lặng).
- Sửa lỗi `bad function: BHTTDTBLOCK` khi tạo ký hiệu biển báo lúc `BHT.Bridge` chưa đăng ký hàm Lisp (lỗi này thoát khỏi `vl-catch-all-apply` và làm dừng lệnh/phiên kiểm thử S0 từ 0.4.6): nay kiểm tra hàm trước, dùng block nội bộ như thiết kế.
- `BHT.Bridge`: khi tim TDT 9.1 được Explode thành nhiều đoạn Line/Arc nối tiếp, BHT nối các đoạn chung đầu mút thành một Polyline tham chiếu trên layer `BHT_TUYEN_TDT` (trước đây chỉ lấy đoạn dài nhất). Tim TDT gốc vẫn chỉ mở `ForRead`, không sửa.
- Palette đổi sang bảng màu xanh lá: nền `#047857`; nút, tiêu đề, thẻ và dòng đang chọn nền `#065F46` chữ `#D1FAE5`; ô nhập, danh sách, vùng thông tin và trạng thái nền `#D1FAE5` chữ `#065F46`. Thanh thẻ dọc bên phải không còn ô đen và mục chọn xanh dương. Bố cục giữ nguyên.
- Đổi tên nút thẻ RTK “Dấu X 1u + sắp nhãn” thành “Đặt dấu X (cỡ 1) + sắp lại nhãn” cho đúng chức năng (đặt POINT dạng dấu X kích thước 1 đơn vị bản vẽ và sắp lại toàn bộ nhãn). Chức năng không đổi.
- Tiêu đề Palette luôn hiện phiên bản đang chạy (`BHT 0.4.6-fix2 — QUẢN LÝ HIỆN TRẠNG TUYẾN`); không còn hiện “BHT 0.4.4” do AutoCAD khôi phục tên cũ lưu trong profile.
- Đồng bộ phiên bản: `BHT-0.4.6-fix2.lsp`, `*bht-version*` = `0.4.6-fix2`, DLL `0.4.6.2`, `PackageContents.xml` `AppVersion` `0.4.6.2` (giữ ProductCode/UpgradeCode), bộ cài fix1 (CMD ASCII + PS1 UTF-8 BOM) với phiên bản `0.4.6-fix2`.
- Dữ liệu bản vẽ, POINT RTK, thuật toán nhãn và lý trình giữ nguyên như 0.4.6.

## 0.4.6 (2026-09-29)

- Đổi Palette sang bảng màu tối phù hợp AutoCAD: nền than–xanh, chữ thao tác vàng chanh, thông tin cyan và trạng thái lỗi/cảnh báo có màu tương phản riêng.
- Chuyển năm thẻ `Tổng quan / RTK / Ảnh / Hồ sơ / Tuyến` thành thanh dọc sát mép phải, giải phóng chiều ngang và giữ phần nhập liệu theo bố cục nhãn trái–điều khiển phải.
- Nghiên cứu chỉ đọc TDT 9.1 bản thường và DPSurvey 3.3: giữ nguyên nguyên tắc block tỷ lệ 1:1 của TDT, đồng thời áp dụng mô hình kiểu điểm tách biệt dữ liệu của DPSurvey cho BHT.
- Thêm kiểu chữ `BHT_RTK` dùng Arial Unicode với hệ số rộng `0,85` cho lần nhập RTK đầu tiên trên bản vẽ mới; tên, mô tả và cao độ tiếp tục được bố trí như một cụm nhãn.
- Bảo toàn bản vẽ cũ: bản vẽ không có khóa kiểu chữ tiếp tục dùng `BHT_ARIAL`; lần cập nhật đầu không đổi font hoặc dời nhãn legacy. Dấu X vẫn đúng tâm, kích thước 1 unit; POINT RTK không bị di chuyển hoặc làm tròn.
- Đồng bộ `BHT-0.4.6.lsp`, `BHT.Core`, `BHT.Bridge`, `BHT.Palette`, Application Bundle, bộ cài và kiểm thử về phiên bản 0.4.6.
- Kiểm thử phát hành: 62 PASS Core; 172 PASS, 0 FAIL, 1 BLOCKED trên AutoCAD Core Console. Mục BLOCKED là mở giao diện DCL do Core Console không có UI; Palette tối và thanh tab phải được đưa vào checklist nghiệm thu thủ công.

## 0.4.5 (2026-09-29)

- Đã tích hợp TDTSolution 9.1 bản thường tại `C:\Program Files (x86)\TDT Solution 2022\`; không dò hoặc dùng TDT 9.1 Pro.
- Đã thêm `BHTTUYENTDT`: khi module TDT đã nạp và đối tượng tim không còn là proxy, `Tdt91Interop` mở tim `ForRead`, gọi `Entity.Explode` rồi tạo hoặc cập nhật một Polyline riêng trên layer `BHT_TUYEN_TDT`. BHT tính lý trình trên bản sao này và không gọi `vlax-curve-*` trực tiếp trên `TDTDBALIGNMENT`.
- Đã thêm `TdtSignLibrary`: đọc danh mục 412 mã từ bản TDT đang cài, hiện gợi ý `Mã — Tên biển` trong Palette và clone block vector được chọn vào DWG. Tài sản TDT không được sửa và không nằm trong gói BHT.
- Đã chuẩn hóa tỷ lệ hiển thị biển TDT: mặt biển scale `0.2`, cột cao `0.6` đơn vị; biển tự đặt ra ngoài tim theo phía đường, có leader về đúng điểm RTK và nhãn chỉ hiện mã biển cùng lý trình.
- Đã bổ sung báo cáo Excel `.xlsx` Unicode gồm sheet tổng hợp và danh sách biển: STT, công trình, đoạn/gói, loại biển, mã/tên biển, phía, lý trình, tình trạng, số trụ/mặt, trạng thái kiểm tra, ghi chú và ID hồ sơ.
- Đã rút gọn thẻ Tuyến & báo cáo còn sáu thao tác chính; các lệnh ít dùng nằm trong **Công cụ nâng cao**. Báo cáo dài mở trong hộp thoại có thể thay đổi kích thước và sao chép; vùng trạng thái dưới Palette không còn nhận con trỏ nhập.
- Đã cập nhật bố trí nhãn, kích thước chữ mặc định và thứ tự hiển thị; POINT RTK vẫn là dấu X kích thước 1 đơn vị, không di chuyển và không làm tròn tọa độ.
- Đã đồng bộ `BHT-0.4.5.lsp`, `BHT.Core`, `BHT.Bridge`, `BHT.Palette`, Application Bundle và bộ cài về phiên bản 0.4.5.
- Kiểm thử phát hành: 62 PASS Core; 172 PASS, 0 FAIL, 1 BLOCKED trên AutoCAD Core Console. Mục BLOCKED là mở giao diện DCL do Core Console không có UI và được chuyển sang checklist nghiệm thu thủ công.

## 0.4.4 (2026-09-28)

- Hợp nhất thư viện biển báo vào `BHT-0.4.4.lsp`; người dùng chỉ APPLOAD một file Lisp.
- Rà soát 205 ảnh trong KMZ tuyến DT830, ưu tiên các nhóm xuất hiện thực tế: W.207, R.412, W.239a + S.509a, W.245a, W.209, I.414, I.423a, I.428a, I.434a và các biển hạn chế P.
- Sửa các mã sai trong bản nháp: tốc độ tối đa là P.127, P.102 là cấm đi ngược chiều; giao nhau với đường ưu tiên là W.208, W.201 là chỗ ngoặt; W.245 là đi chậm; I.401/I.407 không phải cột Km/chỉ hướng đường.
- Block biển báo có điểm chèn tại chân cột `(0,0)`, tên định nghĩa mang hậu tố `V044`, màu và hình học độc lập với layer; hồ sơ `BIEN_BAO` tự chọn block theo trường `ma_hieu`.
- Thêm `BHTBBDANHMUC`; `BHTBLOCK` có lựa chọn `D` để xem danh mục chuẩn trước khi nạp DWG tùy chọn.
- Kiểm tra thư viện cục bộ của TDT Solution 2022: tách được 5 DWG chứa 329 block vector và xuất danh mục 412 biển ra CSV UTF-8 BOM; bổ sung script kiểm tra chỉ đọc, không đóng gói tài sản TDT.
- Thêm DWG/PNG gallery của 19 block và hồi quy hình học, mã hiệu, tải Lisp, dữ liệu/ảnh/tuyến/plugin: 228 PASS, 0 FAIL.

## 0.4.3 (2026-09-28)

- Đồng bộ chặt phiên bản Lisp/.NET; tiêu đề Palette lấy trực tiếp từ assembly và bộ cài chặn khi AutoCAD còn chạy để tránh DLL cũ bị giữ trong bộ nhớ.
- Palette mở mặc định bên trái rộng 430 px, dùng bảng màu xanh mới, bỏ hàng gợi ý rời bị cắt chữ và có vùng thông báo nhiều dòng.
- Thêm bộ đệm `bht:api-messages`: kết quả lệnh tương tác được hiện trong Palette sau khi lệnh kết thúc.
- CSV dùng UTF-8 BOM với tiêu đề tiếng Việt có dấu.
- Nhãn ký hiệu hiển thị tên nghiệp vụ và mã/lý trình, ví dụ `Cọc tiêu Km 48+500`, không hiện ID hồ sơ nội bộ.
- Vẽ lại block mặc định Cọc tiêu và Cột Km theo mẫu; dùng tên định nghĩa phiên bản mới để cập nhật được cả bản vẽ đã chứa block cũ.

## 0.4.2 (2026-09-27)

- Điểm RTK mặc định dùng `PDMODE=3` (dấu X) và `PDSIZE=1`; thêm `BHTKIEUDIEM` và nút Palette để đổi kích thước, áp dụng lại kiểu điểm và sắp nhãn.
- Mở rộng bố trí nhãn lên 64 vị trí ứng viên, tính cả vùng dấu X; bộ dữ liệu hồi quy 526 điểm còn 0/1574 nhãn chồng lấn sau khi sắp.
- Sửa bố cục thẻ **Ảnh** để danh sách, ảnh xem trước và nút thao tác không che nhau; thêm hướng dẫn và nút nhập KMZ/chỉ lại thư mục khi chưa có ảnh.
- Thẻ **Hồ sơ** cho nhập, xóa hoặc tính lý trình; chấp nhận `Km39+050.5`, `39+050,5` hoặc số mét và lưu trạng thái `NHAP_TAY`.
- Danh sách ảnh của hồ sơ hiện rõ `Chưa có ảnh` và điều hướng sang thẻ Ảnh.
- Nhãn ký hiệu đặt phía trên block và luôn bắt đầu bằng `object_id`, sau đó là mã hiệu.
- Thêm `BHTBLOCK`: nạp một DWG làm block tùy chọn cho từng nhóm, lấy `INSBASE` làm tâm chèn, tự giữ hệ số đơn vị và có thể trở lại block mặc định.
- Giữ nguyên hợp đồng dữ liệu `BHT_V02` và khả năng đọc bản vẽ 0.3.2–0.4.1.

## 0.4.1 (2026-09-27)

- `BTH` và `BHT` cùng mở một .NET Palette duy nhất; giữ `BHTPALETTE` làm bí danh tương thích.
- Thêm Autodesk Application Bundle và bộ cài tự động. Cách APPLOAD tự nạp DLL cạnh file Lisp, không cần người dùng chạy `NETLOAD`.
- Chuyển DCL sang lệnh dự phòng `BHTDCL`; sửa lỗi font bằng tệp DCL UTF-8 BOM.
- Rút gọn tên thẻ để không bị cắt chữ; bổ sung nhập CSV, nhập KMZ và tiếp tục quy trình ở thẻ Tổng quan.
- Cho phép chọn điểm trực tiếp trên CAD để tạo hoặc bổ sung hồ sơ trong khi Palette vẫn mở.
- Đồng bộ chọn điểm hai chiều giữa danh sách và bản vẽ; tiếp tục tự làm mới khi đổi tài liệu hoặc dữ liệu DWG.
- Giữ nguyên hợp đồng dữ liệu `BHT_V02` và các lệnh Lisp hiện có.

## 0.4.0 (2026-09-27)
* Plugin .NET (net48, x64): BHT.Core, BHT.Bridge, BHT.Palette; lệnh `BHTPALETTE` mở palette
  "BHT — QUẢN LÝ HIỆN TRẠNG TUYẾN" (5 thẻ: Tổng quan, Điểm khảo sát, Ảnh TimeMark, Hồ sơ đối tượng, Tuyến & báo cáo).
* BHT-0.4.0.lsp (từ 0.3.3): hàm API `bht:api-*` cho plugin (vl-acad-defun); `BHTPALETTE` trong Lisp chỉ hướng dẫn
  NETLOAD khi plugin chưa nạp; BHTTEST 46 mục.
* BHTTHUTUVE: nhận ảnh nền IRT dạng lưới nhiều tile (mỗi tile một IMAGE, kể cả trên layer 0) theo đường dẫn thư mục
  `IRT\` / `IRT.cache` (mẫu `irt_mau_thumuc`, cấu hình BHTTHUTUVE > C); đọc cấu hình một lần cho cả lưới; báo số tile
  trên layer khóa; không bao giờ coi ảnh BHT là IRT.
* Định dạng dữ liệu DWG không đổi so với 0.3.3 (docs/DATA_CONTRACT.md).

## 0.3.3 và cũ hơn
Xem CHANGELOG trong các gói phát hành `release/BHT-0.3.x`.
## v0.6.9 — 2026-10-02

- Sửa mũi tên chọn/đảo chiều tuyến: hiển thị đầu mũi tên đặc, giữ hình trong lúc xác nhận và REGEN; dọn hình khi chấp nhận, hủy hoặc lỗi. Hỗ trợ UCS xoay và điểm đầu/cuối tuyến.
- Đọc nhãn Km trong đối tượng TDT gốc và block lồng nhau; đọc riêng từng thuộc tính và loại nhãn trùng. Tuyến TDT nguồn chỉ được mở để đọc.
- Lấy vị trí cọc TDT tại vạch trên tim, tránh dùng vị trí chữ gây lệch khoảng 0,75 m. Nếu không nhận diện được vạch, giữ cách chiếu nhãn để người dùng kiểm tra.
- Sửa lỗi `eDegenerateGeometry` khi cập nhật lại Polyline tham chiếu có cung; giữ nguyên handle, không tạo tuyến trùng.
- Kiểm tra trên bản sao bản vẽ TDT thực tế: bản cũ đọc 0 cọc, bản mới đọc đủ 371 cọc; giữ hai cảnh báo để người dùng xét duyệt. Lưu/mở lại giữ 371 cọc đọc được và 369 mốc đã nạp trong ca thử.

## 1.0.33 - 2026-10-15, sửa bảng hiệu chỉnh

Sửa tiêu đề bị cắt khi font tăng theo DPI: tiêu đề và dòng tự giãn, có khoảng đệm, xuống dòng theo độ rộng. Áp dụng cho bảng nhập, đối chiếu và lịch sử. Hàng nhập liệu và thông báo tự giãn theo cửa sổ. Test tái hiện trên 1.0.32 có 30 lỗi; bản sửa đạt 101 kiểm tra bảng tại cỡ chữ 100/125/150/200%, 19 kiểm tra UI cũ và 410 CoreTests. Không đổi dữ liệu hay cơ chế nhập. Thêm hướng dẫn kiểm thử A-O và trạng thái từng milestone; chưa tuyên bố M3-M5 hoàn tất.

