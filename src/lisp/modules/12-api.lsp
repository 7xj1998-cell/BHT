;;; ----------------------------------------------------------------------
;;; 0.4.3: API cho plugin .NET (BHT.Bridge / BHT.Palette)
;;;  - Moi ham tra ve DANH SACH CHUOI: ("OK" ...) hoac ("LOI" "ly do").
;;;  - Khong hoi nguoi dung, khong phu thuoc palette.
;;;  - Duoc dang ky bang vl-acad-defun de .NET goi qua Application.Invoke
;;;    (acedInvoke) trong ngu canh lenh; cung goi duoc tu dong lenh:
;;;    (bht:api-version)
;;; ----------------------------------------------------------------------

(setq *bht-api-level* "1")

;; Layer shortcuts: defer to commands already supplied by TDT or another tool.
;; Built-in international command names avoid localization and alias recursion.
(defun c:BHTLAYON () (command "_.LAYON") (princ))
(defun c:BHTLAYOFF () (command "_.LAYOFF") (princ))
(defun c:BHTLAYISO () (command "_.LAYISO") (princ))
(defun c:BHTLAYFRZ () (command "_.LAYFRZ") (princ))
(defun c:BHTLAYTHW () (command "_.LAYTHW") (princ))
(if (and (not (bht:fn-defined-p 'c:11)) (not (getcname "11"))) (defun c:11 () (c:BHTLAYON)))
(if (and (not (bht:fn-defined-p 'c:22)) (not (getcname "22"))) (defun c:22 () (c:BHTLAYOFF)))
(if (and (not (bht:fn-defined-p 'c:33)) (not (getcname "33"))) (defun c:33 () (c:BHTLAYISO)))
;; 44 is reserved for TDT's existing layer command; BHT never registers it.
(if (and (not (bht:fn-defined-p 'c:55)) (not (getcname "55"))) (defun c:55 () (c:BHTLAYFRZ)))

(if (and (not (bht:fn-defined-p 'c:66)) (not (getcname "66"))) (defun c:66 () (c:BHTLAYTHW)))

(defun bht:api-alist (r)
  (mapcar '(lambda (p) (strcat (strcase (bht:str (car p)) T) "=" (bht:str (cdr p)))) r)
)

(defun bht:api-ids (s)
  (vl-remove "" (bht:split (bht:replace (bht:replace (bht:str s) "," " ") ";" " ") " "))
)

;; Chay fn an toan; ket qua (danh sach) -> ("OK" chuoi...).
(defun bht:api-run (fn args / r)
  (setq r (vl-catch-all-apply fn args))
  (bht:log-flush)
  (cond
    ((vl-catch-all-error-p r) (list "LOI" (vl-catch-all-error-message r)))
    ((and (listp r) (= (car r) 'LOI)) (list "LOI" (bht:str (cadr r))))
    (T (cons "OK" (mapcar 'bht:str (if (listp r) r (list r))))))
)

(defun bht:api-version ()
  (list "OK" *bht-version* *bht-api-level* *bht-build*)
)

;; Lay cac thong bao cua lenh tuong tac de hien ngay trong Palette.
;; DRAIN/CLEAR: tra ve theo dung thu tu roi xoa bo dem; PEEK: chi doc.
(defun bht:api-messages (mode / out)
  (setq out (reverse *bht-screen-messages*))
  (if (member (strcase (bht:str mode)) '("DRAIN" "CLEAR"))
    (setq *bht-screen-messages* nil *bht-screen-problems* nil))
  (cons "OK" out)
)

;; 0.4.6-fix3: loi / canh bao (bht:err / bht:warn) cua lenh vua chay, dang "ERROR|noi dung" / "WARN|noi dung".
;; Palette goi TRUOC bht:api-messages de to trang thai that bai. DRAIN/CLEAR: doc roi xoa; PEEK: chi doc.
(defun bht:api-problems (mode / out)
  (setq out (reverse *bht-screen-problems*))
  (if (member (strcase (bht:str mode)) '("DRAIN" "CLEAR"))
    (setq *bht-screen-problems* nil))
  (cons "OK" out)
)

;; Thong tin giong BHTINFO (theo handle thuc the / ID diem / ID ho so / ma anh).
(defun bht:api-info-handle (h)
  (bht:api-run '(lambda (h / e)
                  (if (and (setq e (handent (bht:str h))) (entget e))
                    (bht:info-lines e)
                    (list 'LOI (strcat "không có thực thể handle " (bht:str h)))))
               (list h))
)

(defun bht:api-info-point (pid)
  (bht:api-run '(lambda (pid / p)
                  (if (setq p (bht:pt-find (bht:str pid) (bht:pt-all)))
                    (bht:point-info-lines p)
                    (list 'LOI (strcat "không có điểm " (bht:str pid)))))
               (list pid))
)

(defun bht:api-info-object (oid)
  (bht:api-run '(lambda (oid) (bht:obj-info-lines (strcase (bht:str oid)))) (list oid))
)

(defun bht:api-info-photo (pid)
  (bht:api-run '(lambda (pid) (bht:photo-info-lines (bht:str pid))) (list pid))
)

;; Duong dan JPG ma Lisp tim thay ("" = khong thay) - dung doi chieu voi C#.
(defun bht:api-photo-path (pid)
  (bht:api-run '(lambda (pid / rec p)
                  (setq rec (bht:photo-read (strcase (bht:str pid))))
                  (if (null rec) (list 'LOI (strcat "không có ảnh " (bht:str pid)))
                    (list (if (setq p (bht:photo-path rec)) p ""))))
               (list pid))
)

;; Ky hieu theo object_id: ids "" = moi ho so; "OBJ-1,OBJ-2" = chi cac ho so do.
(defun bht:api-sign-free (oid)
  (bht:api-run '(lambda (oid / r)
    (bht:ensure-model)
    (setq r (bht:kh-place-free oid))
    (cond ((eq (car r) 'LOI) r) ((eq (car r) 'HUY) (list "Đã hủy, hồ sơ không đổi."))
          (T (bht:api-alist r)))) (list oid)))

(defun bht:api-sign-fill (value)
  (bht:api-run '(lambda (value / n)
    (if (not (member value '("0" "1"))) (list 'LOI "Giá trị tô màu không hợp lệ.")
      (if (not (bht:fn-defined-p 'BHTSIGNFILLALL)) (list 'LOI "Nạp BHT.Bridge cùng phiên bản để đổi tô nền.")
        (progn (setq n (BHTSIGNFILLALL value))
          (list (strcat "Đã " (if (= value "1") "bật" "tắt") " tô nền: " (itoa n) " ký hiệu. Giữ nguyên vị trí, góc và tỷ lệ.")))))) (list value)))

(defun bht:api-object-delete (oid)
  (bht:api-run '(lambda (oid)
    (if (or (= (bht:trim oid) "") (not (bht:obj-read oid))) (list 'LOI "Hồ sơ không tồn tại.")
      (if (bht:obj-delete oid) (list (strcase oid) "Đã xóa hồ sơ và ký hiệu; điểm RTK và ảnh gốc giữ nguyên.")
        (list 'LOI "Không xóa được hồ sơ.")))) (list oid)))

(defun bht:api-symbol-upgrade ()
  (bht:api-run '(lambda (/ *bht-preserve-symbol-placement* ids r pair)
    (cond
      ((= (bht:meta "symbol_build" "") *bht-version*) (list "Ký hiệu đã ở phiên bản hiện tại."))
      ((= (getvar "WRITESTAT") 0) (list 'LOI "Bản vẽ chỉ đọc; chưa cập nhật ký hiệu."))
      ((not (bht:fn-defined-p 'BHTSIGNBOUNDS)) (list 'LOI "Lõi Bridge chưa sẵn sàng."))
      (T
        (foreach pair (bht:tagged-pairs "INSERT" "BHT_KH" 1) (setq ids (bht:unique-add ids (car pair))))
        ;; This action is "Cap nhat mau bien da ve"; lights are updated in their own profiles.
        (setq ids (vl-remove-if-not '(lambda (id) (or (bht:kh-fillable-p (bht:obj-read id))
                      (= (bht:get (bht:obj-read id) "nhom") "COC_TIEU"))) ids))
        (if ids
          (progn
            (setq *bht-preserve-symbol-placement* T r (bht:symbol-sync-ex ids nil))
            (bht:meta-set "symbol_build" *bht-version*)
            (cons (strcat "Đã cập nhật ký hiệu BHT " *bht-version* "; lưu bản vẽ để giữ thay đổi.") (bht:api-alist r)))
          (list "Không có ký hiệu biển BHT cần cập nhật."))))) nil))

(defun bht:api-sign-place (oid mode direction degrees)
  (bht:api-run '(lambda (oid mode direction degrees / r)
    (bht:ensure-model)
    (setq r (bht:kh-place-options oid mode direction degrees))
    (cond ((eq (car r) 'LOI) r) ((eq (car r) 'HUY) (list "Đã hủy, hồ sơ không đổi."))
          (T (bht:api-alist r)))) (list oid mode direction degrees)))

(defun bht:api-sign-scale (symbol label)
  (bht:api-sign-display symbol label (bht:meta "sign_label_orientation" "PARALLEL")))

(defun bht:api-sign-display (symbol label orientation)
  (bht:api-run '(lambda (symbol label orientation)
    (bht:kh-display-transaction 'bht:sign-display-write (list symbol label orientation))) (list symbol label orientation)))

(defun bht:sign-display-write (symbol label orientation / s h ids rec e d st nx sc pr index *bht-preserve-symbol-placement*)
    (setq s (bht:num (bht:replace (bht:str symbol) "," "."))
          h (bht:num (bht:replace (bht:str label) "," ".")))
    (if (not (member orientation '("PARALLEL" "PERPENDICULAR")))
      (list 'LOI "Hướng nhãn phải song song hoặc vuông góc đường nối biển–trụ.")
    (if (not (and s h (>= s 0.01) (<= s 100.0) (>= h 0.01) (<= h 100.0)
                  (equal s (bht:num (bht:fnum s 3)) 1e-10) (equal h (bht:num (bht:fnum h 3)) 1e-10)))
      (list 'LOI "Tỷ lệ từ 0,01 đến 100, tối đa 3 chữ số thập phân.")
      (progn
        (bht:ensure-model)
        (bht:meta-set "sign_scale" (bht:fnum s 3))
        (bht:meta-set "sign_label_scale" (bht:fnum h 3))
        (bht:meta-set "sign_label_orientation" orientation)
        (setq ids (vl-remove-if-not '(lambda (oid) (bht:kh-sign-scale-p (bht:obj-read oid))) (bht:obj-ids))
              index (bht:pt-all))
        (foreach pr (bht:tagged-pairs "INSERT" "BHT_KH" 1)
          (if (member (car pr) ids)
            (progn
              (setq e (cdr pr) d (entget e) rec (bht:obj-read (car pr))
                    st (bht:kh-ins-state e (car (bht:kh-auto-transform rec (bht:obj-position rec index) (bht:kh-scale-for rec))))
                    sc (bht:kh-scale-for rec)
                    nx (bht:kh-xdata-ins (car pr) (if (eq st 'AUTO) "TU_DONG" "TAY") (cdr (assoc 10 d)) (cdr (assoc 50 d)) sc))
              (entmod (append (bht:dxf-put (bht:dxf-put (bht:dxf-put d 41 sc) 42 sc) 43 sc) (list nx))) (entupd e))))
        (setq *bht-preserve-symbol-placement* T)
        (if ids (bht:api-alist (bht:symbol-sync-ex ids nil)) (list "Đã lưu tỷ lệ; áp dụng khi chèn biển và bảng."))))))

(defun bht:api-symbol-sync (ids)
  (bht:api-run '(lambda (ids / sc result manual index rec pid oid)
                  (setq sc (bht:api-ids ids))
                  (bht:ensure-model)
                  (setq result (bht:symbol-sync (if sc sc nil)) index (bht:pt-all))
                  (foreach oid (if sc sc (bht:obj-ids))
                    (setq rec (bht:obj-read oid))
                    (if (and (member (bht:get rec "nhom") '("COC_TIEU" "COT_KM")) (bht:obj-manual-position-p rec index))
                      (foreach pid (bht:get-all rec "pt") (setq manual (bht:unique-add manual (strcase pid))))))
                  (if manual (bht:lbl-sync-scope manual))
                  (bht:api-alist result))
               (list ids))
)

;; Nhan diem: "" = chi diem moi / doi noi dung; "ALL" = bo tri lai tat ca;
;; danh sach ID = bo tri lai cac diem do (nhan doi tay van giu).
(defun bht:api-label-sync (ids)
  (bht:api-run '(lambda (ids / sc r ov)
                  (setq sc (cond ((= (strcase (bht:str ids)) "ALL") 'ALL)
                                 ((bht:api-ids ids) (mapcar 'strcase (bht:api-ids ids)))
                                 (T nil)))
                  (setq r (bht:lbl-sync-scope sc)
                        ov (bht:lbl-overlaps (if (listp sc) sc nil)))
                  (append (bht:api-alist r) (list (strcat "overlap=" (bht:lbl-overlap-text ov)))))
               (list ids))
)

;; Dinh dang POINT thanh dau X dung tam va sap lai nhan toan ban ve.
(defun bht:api-point-style (size)
  (bht:api-run '(lambda (size / s)
                  (setq s (bht:num (bht:str size)))
                  (if (not (and s (> s 0.0)))
                    (list 'LOI "kích thước dấu X phải lớn hơn 0")
                    (bht:api-alist (bht:point-style-apply s T))))
               (list size))
)

(defun bht:api-photo-sync ()
  (bht:api-run '(lambda () (bht:api-alist (bht:photo-sync))) nil)
)

;; Scale only existing RTK labels; creating/arranging labels is a separate action.
(defun bht:api-rtk-scale (symbol label)
  (bht:api-run '(lambda (symbol label / s factor h r count pair e d)
    (setq s (bht:num (bht:replace (bht:str symbol) "," "."))
          factor (bht:num (bht:replace (bht:str label) "," ".")))
    (if (not (and s factor (>= s 0.01) (<= s 100.0) (>= factor 0.01) (<= factor 100.0)
                  (equal s (bht:num (bht:fnum s 3)) 1e-10) (equal factor (bht:num (bht:fnum factor 3)) 1e-10)))
      (list 'LOI "Tỷ lệ từ 0,01 đến 100, tối đa 3 chữ số thập phân.")
      (progn
        (setq h (* 0.5 factor) count 0)
        (bht:meta-set "nhan_h" (bht:fnum h 6))
        (setq r (bht:point-style-apply s nil))
        (foreach pair (bht:tagged-pairs "TEXT" "BHT_NHAN" 2)
          (setq e (cdr pair) d (entget e))
          (if (not (equal (cdr (assoc 40 d)) h 1e-9))
            (progn (entmod (bht:dxf-put d 40 h)) (entupd e) (setq count (1+ count)))))
        (append (bht:api-alist r) (list (strcat "label_height=" (bht:fnum h 6)) (strcat "updated=" (itoa count))))))) (list symbol label)))

(defun bht:api-photo-stats ()
  (bht:api-run '(lambda () (bht:api-alist (bht:photo-stats))) nil)
)

;; Kiem tra toan ven (giong BHTKT): ("OK" "loi=n" "canh_bao=n" dong...)
(defun bht:api-check ()
  (bht:api-run '(lambda (/ r)
                  (setq r (bht:check))
                  (append (list (strcat "loi=" (itoa (car r))) (strcat "canh_bao=" (itoa (cadr r)))) (caddr r)))
               nil)
)

(defun bht:api-object-station (id) (if (bht:field-mode-p) (bht:field-api-run 'bht:station-one (list id)) (bht:api-run 'bht:station-one (list id))))

(defun bht:api-station-label-refresh (id)
  (bht:api-run '(lambda (id) (bht:kh-station-label-refresh (list id)) (list "Đã cập nhật nhãn lý trình.")) (list id)))

(defun bht:api-route-select (id)
  (bht:api-run '(lambda (id)
    (if (bht:route-read id) (progn (setq *bht-route-selected* id) (list id))
      (list 'LOI "Tuyến đã bị xóa hoặc không tồn tại."))) (list id)))

(defun bht:api-route-diag (id)
  (bht:api-run
    '(lambda (id / ids out)
       (setq id (strcase (bht:str id)) ids (if (= id "") (bht:route-ids) (list id)) out nil)
       (if (null ids) (list 'LOI "chưa có tuyến tham chiếu")
         (progn
           (foreach rid ids
             (if out (setq out (append out (list "" "----------------------------------------"))))
             (setq out (append out (bht:route-diag-lines rid))))
           out)))
    (list id))
)

;; Thu tu hien thi (dung lenh DRAWORDER). Chi chay duoc khi goi tu dong lenh / Lisp o cap cao nhat.
;; Goi qua Application.Invoke (acedInvoke), AutoCAD TU CHOI lenh (kiem chung Core Console 0.4.0):
;; tra ve LOI ro rang - plugin dung duong lenh BHTTHUTUVE (SendStringToExecute) thay the.
(defun bht:api-draworder (/ r)
  (setq r (bht:api-run '(lambda () (bht:api-alist (bht:draworder-sync))) nil))
  (if (= (car r) "LOI") (bht:sv-restore))
  (if (and (= (car r) "LOI") (vl-string-search "COMMAND" (strcase (bht:str (cadr r)))))
    (list "LOI" (strcat "DRAWORDER không chạy được trong ngữ cảnh gọi API (" (bht:str (cadr r))
                        ") - dùng lệnh BHTTHUTUVE"))
    r)
)

(defun bht:field-api-run (fn args / doc own result)
  (setq doc (vlax-get-acad-object))
  (if doc (setq doc (vla-get-ActiveDocument doc)))
  (setq own (and doc (= 0 (logand 8 (getvar "UNDOCTL")))))
  (if own (vla-StartUndoMark doc))
  (setq result (bht:api-run fn args))
  (if own (vla-EndUndoMark doc))
  result)
(defun bht:api-field-save (id name ids maxoff maxspan tol) (bht:field-api-run 'bht:field-save (list id name ids maxoff maxspan tol)))
(defun bht:api-field-input-preview () (bht:api-run 'bht:field-input-preview nil))
(defun bht:api-field-manual-control (oid value) (bht:field-api-run 'bht:field-manual-control (list oid value)))
(defun bht:api-field-input-use (snapshot) (bht:field-api-run 'bht:field-input-use (list snapshot)))
(defun bht:api-field-pair (oid aid bid) (bht:field-api-run 'bht:field-pair (list oid aid bid)))
(defun bht:api-field-mode (mode cid)
  (bht:field-api-run '(lambda (mode cid)
    (cond ((not (member mode '("FIELD_MARKERS" "ROUTE"))) (list 'LOI "Chế độ không hợp lệ."))
      ((and (= mode "FIELD_MARKERS") (/= cid "") (null (bht:rec-read "FIELD_CHAIN" cid))) (list 'LOI "Chuỗi đã bị xóa."))
      (T (bht:meta-set "station_method" mode) (bht:meta-set "field_active_chain" cid) (list "Đã chọn phương pháp tính; hồ sơ chỉ đổi khi tính lại.")))) (list mode cid)))
(defun bht:api-field-auto (oid / rec)
  (bht:field-api-run '(lambda (oid / rec cid)
    (setq rec (bht:obj-read oid) cid (bht:meta "field_active_chain" ""))
    (if (or (null rec) (null (bht:rec-read "FIELD_CHAIN" cid))) (list 'LOI "Chọn chuỗi cọc đang dùng trong tab Tuyến trước.")
      (progn (foreach key '("field_pair_a" "field_pair_b") (setq rec (bht:set rec key "")))
        (setq rec (bht:set (bht:set rec "field_chain_id" cid) "station_status" "NEEDS_REVIEW"))
        (bht:obj-write oid rec) (bht:meta-set "station_method" "FIELD_MARKERS") (bht:field-one oid)))) (list oid)))
(defun bht:api-field-delete (cid)
  (bht:field-api-run '(lambda (cid)
    (if (null (bht:rec-read "FIELD_CHAIN" cid)) (list 'LOI "Chuỗi không tồn tại.")
      (progn (bht:rec-delete "FIELD_CHAIN" cid) (bht:field-invalidate cid) (list "Đã xóa khai báo chuỗi. Giữ cọc, RTK và lý trình cũ; tính lại sẽ báo hồ sơ thiếu chuỗi.")))) (list cid)))
(defun bht:api-field-heading (ids)
  (bht:field-api-run '(lambda (ids) (bht:api-alist (bht:kh-heading-apply (if (= ids "ALL") (bht:obj-ids) (bht:api-ids ids)) "FIELD_PERP" "" nil))) (list ids)))

(defun bht:api-label-rotate (mode / own result)
  ;; command-s works in both full AutoCAD and Core Console (no ActiveX document).
  ;; Respect an undo group already owned by the caller.
  (setq own (= 0 (logand 8 (getvar "UNDOCTL"))))
  (if own (command-s "_.UNDO" "_Begin"))
  (setq result (bht:api-run 'bht:lbl-rotate-request (list mode)))
  (if own (command-s "_.UNDO" "_End"))
  result)

(defun bht:api-symbol-gap (value / own result)
  (setq own (= 0 (logand 8 (getvar "UNDOCTL"))))
  (if own (command-s "_.UNDO" "_Begin"))
  (setq result (bht:api-run 'bht:kh-gap-apply (list value)))
  (if own (command-s "_.UNDO" "_End")) result)

(defun bht:api-symbol-display (value mode / own result)
  (setq own (= 0 (logand 8 (getvar "UNDOCTL"))))
  (if own (command-s "_.UNDO" "_Begin"))
  (setq result (bht:api-run 'bht:kh-gap-apply-mode (list value mode)))
  (if own (command-s "_.UNDO" "_End")) result)

(defun bht:kh-road-pick (/ pick ent start dir)
  (setq pick (entsel "\nChọn PL giữa đường để bố trí hình báo hiệu (không dùng tính lý trình): "))
  (cond
    ((null pick) (list 'HUY))
    ((not (bht:kh-road-valid-p (car pick))) (list 'LOI "Chọn PL 2D mở trong Model, nằm trên mặt phẳng XY."))
    (T (setq ent (car pick)) (redraw ent 3)
       (setq start (vl-catch-all-apply 'getpoint (list "\nChọn gần đầu PL mong muốn; trái/phải theo chiều đầu → cuối (Enter = hủy): ")))
       (redraw ent 4)
       (if (or (null start) (vl-catch-all-error-p start)) (list 'HUY)
         (progn (setq start (trans start 1 0)
                      dir (if (<= (distance start (vlax-curve-getStartPoint ent)) (distance start (vlax-curve-getEndPoint ent))) 1 -1))
                (list ent dir))))))

(defun bht:kh-road-request (value left right mode / pick)
  (cond
    ((not (vl-every '(lambda (s / v) (and (setq v (bht:num s)) (<= 0.0 v 100000.0))) (list value left right)))
     (list 'LOI "Nhập bề rộng hai bên và khoảng lùi từ 0 đến 100000 đơn vị bản vẽ."))
    ((not (member mode '("THANG" "GAP_KHUC"))) (list 'LOI "Chọn đường nối thẳng hoặc gấp khúc."))
    (T (setq pick (bht:kh-road-pick))
       (cond ((= (car pick) 'HUY) (list "Đã hủy bố trí; giữ nguyên bản vẽ."))
             ((= (car pick) 'LOI) pick)
             (T (bht:kh-road-apply (car pick) left right value (cadr pick) mode))))))

(defun bht:api-symbol-road (value left right mode / own result)
  (setq own (= 0 (logand 8 (getvar "UNDOCTL"))))
  (if own (command-s "_.UNDO" "_Begin"))
  (setq result (bht:api-run 'bht:kh-road-request (list value left right mode)))
  (if own (command-s "_.UNDO" "_End")) result)

(defun bht:kh-center-apply (id value mode / rec ent *bht-center-route*)
  (setq rec (bht:route-read id) ent (if rec (bht:route-ent rec) nil))
  (cond
    ((null rec) (list 'LOI "Không tìm thấy tuyến. Nạp tim tuyến trong bảng Tuyến trước khi bố trí."))
    ;; The user explicitly selects this route as the display center reference.
    ;; Its stored classification and chainage controls must remain unchanged.
    ((null ent) (list 'LOI "Hình tim tuyến đã bị xóa. Chọn lại tuyến trước khi bố trí."))
    ((not (bht:kh-road-valid-p ent)) (list 'LOI "Tim phải là Polyline 2D mở, nằm trên mặt phẳng XY trong Model. Chọn lại tim phù hợp; không dùng proxy TDT trực tiếp."))
    (T (setq *bht-center-route* id)
       (bht:kh-road-apply ent "0" "0" value (if (= (bht:int (bht:get rec "direction")) -1) -1 1) mode))))

(defun bht:api-symbol-center (id value mode / own result)
  (setq own (= 0 (logand 8 (getvar "UNDOCTL"))))
  (if own (command-s "_.UNDO" "_Begin"))
  (setq result (bht:api-run 'bht:kh-center-apply (list id value mode)))
  (if own (command-s "_.UNDO" "_End")) result)

(defun bht:kh-road-ask-mode (/ mode)
  (initget "Thang GapKhuc")
  (setq mode (getkword "\nĐường nối [Thang/GapKhuc] <GapKhuc>: "))
  (if (= mode "Thang") "THANG" "GAP_KHUC"))

(defun c:BHTKHOANGCACH (/ *error* method left right v mode result line id)
  (setq *error* bht:on-error)
  (initget "CachTim NgoaiDuong DiemDo")
  (setq method (getkword "\nBố trí [CachTim/NgoaiDuong/DiemDo] <NgoaiDuong>: "))
  (cond
    ((= method "CachTim")
     (setq id (bht:ask-route))
     (if id (progn
       (initget 4) (setq v (getreal "\nKhoảng cách tối thiểu từ tim đến hình biển và nhãn (Enter = hủy): "))
       (if v (setq result (bht:api-symbol-center id (bht:fnum v 8) (bht:kh-road-ask-mode)))))))
    ((/= method "DiemDo")
     (initget 4) (setq left (getreal "\nBề rộng từ PL đến mép trái (đơn vị bản vẽ; Enter = hủy): "))
     (if left (progn (initget 4) (setq right (getreal "\nBề rộng từ PL đến mép phải (Enter = hủy): "))))
     (if right (progn (initget 4) (setq v (getreal "\nKhoảng lùi tối thiểu ngoài mép đường (Enter = hủy): "))))
     (if v (setq result (bht:api-symbol-road (bht:fnum v 8) (bht:fnum left 8) (bht:fnum right 8) (bht:kh-road-ask-mode)))))
    (T
     (initget 4) (setq v (getreal "\nKhoảng cách điểm gốc đến gốc ký hiệu (đơn vị bản vẽ; Enter = hủy): "))
     (if v (progn
       (initget "Thang GapKhuc")
       (setq mode (getkword "\nĐường nối [Thang/GapKhuc] <giữ kiểu hiện tại>: "))
       (setq result (bht:api-symbol-display (bht:fnum v 8) (cond ((= mode "Thang") "THANG") ((= mode "GapKhuc") "GAP_KHUC") (T ""))))))))
  (if result (if (= (car result) "OK") (foreach line (cdr result) (bht:msg line)) (bht:warn (cadr result))))
  (princ))

;; Display selection is a .NET UsePickSet/Redraw command. No follow-up Lisp
;; invocation may clear its PICKFIRST selection when returning to the Palette.

(setq *bht-api-functions*
  '(bht:api-field-manual-control bht:api-field-input-preview bht:api-field-input-use bht:api-field-save bht:api-field-pair bht:api-field-mode bht:api-field-auto bht:api-field-delete bht:api-field-heading bht:api-version bht:api-messages bht:api-problems bht:api-info-handle bht:api-info-point bht:api-info-object bht:api-info-photo
    bht:api-photo-path bht:api-object-delete bht:api-symbol-upgrade bht:api-sign-fill bht:api-sign-scale bht:api-sign-display bht:api-sign-place bht:api-sign-free bht:api-symbol-sync bht:api-label-sync bht:api-point-style bht:api-rtk-scale bht:api-photo-sync bht:api-photo-stats
    bht:api-check bht:api-object-station bht:api-station-label-refresh bht:api-route-select bht:api-route-diag bht:api-draworder bht:api-label-rotate bht:api-symbol-gap bht:api-symbol-display bht:api-symbol-road bht:api-symbol-center))

(defun bht:api-register (/ n)
  (setq n 0)
  (if vl-acad-defun
    (foreach f *bht-api-functions*
      (if (not (vl-catch-all-error-p (vl-catch-all-apply 'vl-acad-defun (list f)))) (setq n (1+ n)))))
  n
)
(setq *bht-api-registered* (bht:api-register))

;; Tu nap Palette theo cung co che APPLOAD cua GKIN. BTH va BHT la command
;; .NET, nen Lisp khong dinh nghia c:BTH / c:BHT de tranh hai lenh trung ten.
(defun bht:command-available-p (name / value)
  (setq value (vl-catch-all-apply 'getcname (list name)))
  (and (not (vl-catch-all-error-p value)) value)
)

(defun bht:palette-load (/ result)
  (cond
    ((bht:command-available-p "BTH") T)
    ((or (null *bht-palette-dll*) (not (findfile *bht-palette-dll*))) nil)
    (T
      (setq result (vl-catch-all-apply 'vl-cmdf (list "_.NETLOAD" *bht-palette-dll*)))
      (and (not (vl-catch-all-error-p result)) (bht:command-available-p "BTH"))))
)

(defun c:BHTLOAD ()
  (if (bht:palette-load)
    (bht:msg "BHT: Palette đã sẵn sàng. Gõ BTH hoặc BHT để mở bảng.")
    (progn
      (bht:warn "BHT: không nạp được Palette.")
      (bht:msg (strcat "  Kiểm tra BHT.Palette.dll, BHT.Bridge.dll và BHT.Core.dll nằm cạnh BHT-" *bht-version* ".lsp."))))
  (princ)
)

;;; ----------------------------------------------------------------------
(if (and (getvar "LISPSYS") (= (getvar "LISPSYS") 0))
  (princ "\nBHT CANH BAO: LISPSYS=0 - tieng Viet co dau co the hien sai. Dat LISPSYS=1, khoi dong lai AutoCAD roi nap lai."))
(bht:point-style-apply nil nil)
;; Symbol updates are explicit; do not modify a drawing while it is opening.
(setq *bht-palette-loaded* (bht:command-available-p "BTH"))
(defun bht:load-message () (strcat "BHT " *bht-version* " đã nạp thành công."))
(princ (strcat "\n" (bht:load-message)))
(if *bht-palette-loaded*
  (princ "\nGõ BTH hoặc BHT để mở Palette bên trái; BHTHELP để xem danh sách lệnh.")
  (princ "\nPalette chỉ nạp khi gọi lệnh. Gõ BHTLOAD rồi BTH để mở bảng."))
(princ)

