;;; ----------------------------------------------------------------------
;;; Tuyen tham chieu + moc Km - dictionary "ROUTE"
;;; Truong V5: route_id handle loai (TIM_DUONG/TIM_RANH/KHAC) max_offset
;;;  ngoai_suy_m chieu (legacy station direction), start_dist, direction (1/-1),
;;;  route_revision, geometry_length, geometry_signature, nguon, tao_luc/cap_nhat_luc.
;;;  moc* = "dist|ly_trinh_sau|ly_trinh_truoc|nguon|ghi_chu"
;;;  (moc thuong: sau = truoc; diem gay Km: sau (back) khac truoc (ahead))
;;; ----------------------------------------------------------------------

(setq *bht-ratio-tol* 0.03)

;; Kiem tra thuc the lam tuyen. Tra ve (T "") hoac (nil "ly do").
(defun bht:route-check-ent (ent / d typ flags obj oname)
  (if (null ent)
    (list nil "không có đối tượng")
    (progn
      (setq d (entget ent) typ (cdr (assoc 0 d)) flags (cdr (assoc 70 d)))
      (setq obj (vl-catch-all-apply 'vlax-ename->vla-object (list ent))
            oname (if (or (null obj) (vl-catch-all-error-p obj)) ""
                    (vl-catch-all-apply 'vla-get-ObjectName (list obj))))
      (if (vl-catch-all-error-p oname) (setq oname ""))
      (cond
        ((or (= typ "ACAD_PROXY_ENTITY") (wcmatch (strcase oname) "*ZOMBIE*,*TDT*,*PROXY*"))
         (list nil (strcat "đối tượng proxy/TDT (" typ (if (/= oname "") (strcat ", " oname) "")
                           ") - KHÔNG dùng để tính lý trình. Hãy xuất/vẽ Polyline tham chiếu thường.")))
        ((= typ "LWPOLYLINE") (list T ""))
        ((and (= typ "POLYLINE") (= 0 (logand (if flags flags 0) (+ 16 64)))) (list T ""))
        (T (list nil (strcat "loại " typ " không được hỗ trợ; chỉ nhận LWPOLYLINE/POLYLINE thường."))))))
)

(defun bht:route-read (id) (bht:rec-read "ROUTE" id))
(defun bht:route-write (id rec) (bht:rec-write "ROUTE" id rec))
(defun bht:route-ids () (bht:rec-keys "ROUTE"))

(defun bht:route-ent (rec / ent)
  (setq ent (handent (bht:get rec "handle")))
  (if (and ent (entget ent)) ent nil)
)

(defun bht:curve-length (ent / ep len)
  (setq ep (vl-catch-all-apply 'vlax-curve-getEndParam (list ent)))
  (if (vl-catch-all-error-p ep) nil
    (progn
      (setq len (vl-catch-all-apply 'vlax-curve-getDistAtParam (list ent ep)))
      (if (or (vl-catch-all-error-p len) (null len)) nil (abs len)))))

(defun bht:curve-closed-p (ent / v)
  (setq v (vl-catch-all-apply 'vlax-curve-isClosed (list ent)))
  (if (vl-catch-all-error-p v) nil v))

;; Chu ky gon de phat hien Polyline nguon thay doi ma khong sua geometry.
;; Lay 9 mau doc chieu dai nen van nhan ra thay doi hinh dang khi bbox/length
;; khong doi. Chi goi vlax-curve tren Polyline BHT thuong, KHONG tren proxy TDT.
(defun bht:route-geometry-signature (ent start direction / len h closed samples i d p den)
  (setq len (bht:curve-length ent) h (cdr (assoc 5 (entget ent)))
        closed (bht:curve-closed-p ent) samples nil i 0 den (if closed 9.0 8.0))
  (repeat 9
    (setq d (if (and len (> len 0.0)) (* len (/ i den)) 0.0)
          p (vl-catch-all-apply 'vlax-curve-getPointAtDist (list ent d)))
    (if (vl-catch-all-error-p p) (setq p nil))
    (setq samples (cons (if p (strcat (bht:fnum (car p) 6) "," (bht:fnum (cadr p) 6)) "") samples)
          i (1+ i)))
  (bht:join
    (append (list (bht:str h) (bht:fnum (if len len 0.0) 6) (if closed "C" "O")
                  (bht:fnum (if start start 0.0) 6) (itoa (if (= direction -1) -1 1)))
            (reverse samples))
    "|"))

(defun bht:route-revision (rec / n)
  (setq n (bht:int (bht:get rec "route_revision")))
  (if (and n (> n 0)) n 1))

(defun bht:route-bump (rec reason / n)
  (setq n (1+ (bht:route-revision rec))
        rec (bht:set rec "route_revision" (itoa n))
        rec (bht:set rec "cap_nhat_luc" (bht:now))
        rec (bht:set rec "ly_do_cap_nhat" reason))
  rec)

(defun bht:route-create (id ent loai maxoff extrap chieu nguon / chk rec len sig)
  (setq id (strcase id) chk (bht:route-check-ent ent))
  (cond
    ((not (bht:valid-id id)) (list nil "ID tuyến không hợp lệ"))
    ((bht:route-read id) (list nil "ID tuyến đã tồn tại; dùng Sửa tuyến hoặc Chọn lại tuyến."))
    ((or (not (numberp maxoff)) (<= maxoff 0) (not (numberp extrap)) (< extrap 0)) (list nil "Khoảng cách phải dương, ngoại suy không âm."))
    ((not (car chk)) chk)
    ((or (null (bht:curve-length ent)) (<= (bht:curve-length ent) 1e-8)) (list nil "Tuyến phải có chiều dài lớn hơn 0."))
    (T
     (setq len (bht:curve-length ent) sig (bht:route-geometry-signature ent 0.0 1))
     (setq rec (list (cons "route_id" id) (cons "handle" (cdr (assoc 5 (entget ent))))
                     (cons "loai" loai) (cons "max_offset" (bht:fnum maxoff 3))
                     (cons "ngoai_suy_m" (bht:fnum extrap 3))
                     (cons "chieu" (if chieu (itoa chieu) ""))
                     (cons "start_dist" "0.000000") (cons "direction" "1")
                     (cons "route_revision" "1")
                     (cons "geometry_length" (bht:fnum (if len len 0.0) 6))
                     (cons "geometry_signature" sig)
                     (cons "station_source" "CHUA_CO_MOC")
                     (cons "nguon" nguon) (cons "tao_luc" (bht:now))))
     (bht:route-write id rec)
     (bht:log (strcat "Tạo tuyến tham chiếu " id " (" loai ") handle " (bht:get rec "handle")))
     (list T id)))
)

;; Danh sach moc da sap xep: ((dist back ahead nguon ghichu) ...)
(defun bht:route-marks (rec / out f m)
  (setq out nil)
  (foreach m (bht:get-all rec "moc")
    (setq f (bht:split m "|"))
    (if (and (>= (length f) 3) (bht:num (nth 0 f)) (bht:num (nth 1 f)) (bht:num (nth 2 f)))
      (setq out (cons (list (bht:num (nth 0 f)) (bht:num (nth 1 f)) (bht:num (nth 2 f))
                            (if (nth 3 f) (nth 3 f) "") (if (nth 4 f) (nth 4 f) "")) out))))
  (vl-sort out '(lambda (a b) (< (car a) (car b))))
)

(defun bht:route-add-mark (id dist back ahead nguon note / rec marks next)
  (setq rec (bht:route-read id))
  (if rec
    (progn
      (setq marks (bht:get-all rec "moc"))
      ;; khong cho 2 moc cung vi tri
      (if (vl-some '(lambda (m) (< (abs (- (car m) dist)) 0.001)) (bht:route-marks rec))
        (list nil "đã có mốc tại vị trí này (xóa mốc cũ trước)")
        (progn
          (setq marks (append marks (list (bht:join (list (bht:fnum dist 4) (bht:fnum back 4) (bht:fnum ahead 4)
                                                          (bht:replace (bht:clean nguon) "|" "/")
                                                          (bht:replace (bht:clean note) "|" "/")) "|"))))
          (setq next (bht:set-all rec "moc" marks)
                next (bht:set next "station_source" (if (bht:starts (strcase nguon) "TDT") "TDT_STAKES" "CONTROL_POINTS"))
                next (bht:route-bump next "Thay đổi Station Control"))
          (bht:route-write id next)
          (bht:log (strcat "Mốc Km tuyến " id ": d=" (bht:fnum dist 3) " sau=" (bht:fmt-km back)
                           " trước=" (bht:fmt-km ahead) " nguồn=" nguon))
          (list T (length marks)))))
    (list nil "không có tuyến"))
)

(defun bht:route-del-mark (id idx / rec marks next)
  (setq rec (bht:route-read id) marks (bht:route-marks rec))
  (if (and rec (nth idx marks))
    (progn
      (setq marks (vl-remove (nth idx marks) marks))
      (setq next (bht:set-all rec "moc"
                    (mapcar '(lambda (m) (bht:join (list (bht:fnum (nth 0 m) 4) (bht:fnum (nth 1 m) 4)
                                                         (bht:fnum (nth 2 m) 4) (nth 3 m) (nth 4 m)) "|")) marks))
            next (bht:route-bump next "Xóa mốc Station Control"))
      (bht:route-write id next)
      T)
    nil)
)

;; Reject damaged/imported controls before any interpolation or extrapolation.
(defun bht:controls-valid-p (marks / ok prev m)
  (setq ok T prev nil)
  (foreach m marks
    (if (and (listp m) (numberp (car m)) (numberp (cadr m)) (numberp (caddr m))
             (or (null prev) (> (- (car m) prev) 1e-9)))
      (setq prev (car m))
      (setq ok nil)))
  ok)

;; LOI GIAI LY TRINH THUAN TUY (kiem thu duoc):
;; marks: ((dist back ahead ...) ...) da sap xep ; d: khoang cach doc tuyen
;; extrap: so met toi da cho phep ngoai suy ; chieu: 1/-1/nil (khi chi co 1 moc)
;; Tra ve (trang_thai ly_trinh dir he_so ghi_chu)
(defun bht:km-from-dist (marks d extrap chieu / n i a b ratio s dir found mfirst mlast)
  (setq n (length marks))
  (cond
    ((or (not (numberp d)) (not (numberp extrap)) (< extrap 0.0) (not (bht:controls-valid-p marks)))
     (list "CHUA_XAC_DINH" nil nil nil "mốc trùng vị trí hoặc số liệu không hợp lệ; kiểm tra lại mốc và giới hạn ngoại suy"))
    ((= n 0) (list "CHUA_CO_MOC" nil nil nil "tuyến chưa có mốc Km"))
    ((= n 1)
     (setq a (car marks))
     (cond
       ((null chieu) (list "CHUA_XAC_DINH" nil nil nil "chỉ 1 mốc và chưa khai báo chiều tăng Km"))
       ((> (abs (- d (car a))) extrap)
        (list "CHUA_XAC_DINH" nil chieu nil "ngoài phạm vi cho phép tính từ 1 mốc"))
       (T (list "MOT_MOC" (+ (if (>= d (car a)) (caddr a) (cadr a)) (* chieu (- d (car a))))
                chieu 1.0 (strcat "tính từ 1 mốc " (bht:fmt-km (caddr a)) ", hệ số 1, chưa hiệu chỉnh")))))
    (T
     (setq i 0 found nil)
     (while (and (not found) (< i (1- n)))
       (setq a (nth i marks) b (nth (1+ i) marks))
       (if (and (>= d (car a)) (<= d (car b)))
         (setq found T)
         (setq i (1+ i))))
     (if found
       (progn
         (setq ratio (/ (- (cadr b) (caddr a)) (- (car b) (car a)))
               s (+ (caddr a) (* (- d (car a)) ratio))
               dir (if (< ratio 0.0) -1 1))
         (if (= d (car b)) (setq s (cadr b)))
         (list (if (> (abs (- (abs ratio) 1.0)) *bht-ratio-tol*) "CAN_KIEM_TRA" "NOI_SUY")
               s dir ratio
               (strcat "nội suy giữa mốc " (bht:fmt-km (caddr a)) " và " (bht:fmt-km (cadr b))
                       ", hệ số " (bht:fnum ratio 5))))
       (progn
         (setq mfirst (car marks) mlast (last marks))
         (if (< d (car mfirst))
           (progn
             (setq b (cadr marks)
                   ratio (/ (- (cadr b) (caddr mfirst)) (- (car b) (car mfirst)))
                   dir (if (< ratio 0.0) -1 1))
             (if (<= (- (car mfirst) d) extrap)
               (list "NGOAI_SUY" (+ (cadr mfirst) (* dir (- d (car mfirst)))) dir 1.0
                     (strcat "ngoại suy trước mốc " (bht:fmt-km (cadr mfirst)) " " (bht:fnum (- (car mfirst) d) 2) " m"))
               (list "CHUA_XAC_DINH" nil dir nil "ngoài phạm vi các mốc Km")))
           (progn
             (setq a (nth (- n 2) marks)
                   ratio (/ (- (cadr mlast) (caddr a)) (- (car mlast) (car a)))
                   dir (if (< ratio 0.0) -1 1))
             (if (<= (- d (car mlast)) extrap)
               (list "NGOAI_SUY" (+ (caddr mlast) (* dir (- d (car mlast)))) dir 1.0
                     (strcat "ngoại suy sau mốc " (bht:fmt-km (caddr mlast)) " " (bht:fnum (- d (car mlast)) 2) " m"))
               (list "CHUA_XAC_DINH" nil dir nil "ngoài phạm vi các mốc Km"))))))))
)

;; Diem tren tuyen: (khoang_cach_doc_tuyen offset2d phia_theo_huong_ve diem_chieu)
;; phia: 1 = trai, -1 = phai (theo chieu ve polyline), 0 = tren tuyen
(defun bht:curve-project (ent pt / cp d off par der p1 p2 dx dy cross)
  (setq pt (list (car pt) (cadr pt) (if (caddr pt) (caddr pt) 0.0)))
  (setq cp (vl-catch-all-apply 'vlax-curve-getClosestPointToProjection (list ent pt '(0.0 0.0 1.0))))
  (if (vl-catch-all-error-p cp)
    (setq cp (vl-catch-all-apply 'vlax-curve-getClosestPointTo (list ent pt))))
  (if (or (null cp) (vl-catch-all-error-p cp))
    nil
    (progn
      (setq d (vl-catch-all-apply 'vlax-curve-getDistAtPoint (list ent cp)))
      (if (or (null d) (vl-catch-all-error-p d))
        nil
        (progn
          (setq off (distance (list (car pt) (cadr pt)) (list (car cp) (cadr cp))))
          (setq par (vl-catch-all-apply 'vlax-curve-getParamAtPoint (list ent cp)))
          (setq der (if (and par (not (vl-catch-all-error-p par)))
                      (vl-catch-all-apply 'vlax-curve-getFirstDeriv (list ent par)) nil))
          (if (or (null der) (vl-catch-all-error-p der) (< (+ (abs (car der)) (abs (cadr der))) 1e-12))
            (progn
              (setq p1 (vl-catch-all-apply 'vlax-curve-getPointAtDist (list ent (max 0.0 (- d 0.05))))
                    p2 (vl-catch-all-apply 'vlax-curve-getPointAtDist (list ent (+ d 0.05))))
              (if (or (null p2) (vl-catch-all-error-p p2)) (setq p2 cp))
              (if (or (null p1) (vl-catch-all-error-p p1)) (setq p1 cp))
              (setq der (list (- (car p2) (car p1)) (- (cadr p2) (cadr p1)) 0.0))))
          (setq dx (car der) dy (cadr der)
                cross (- (* dx (- (cadr pt) (cadr cp))) (* dy (- (car pt) (car cp)))))
          (list d off (cond ((< off 0.005) 0) ((> cross 0.0) 1) (T -1)) cp)))))
)

;; Doi raw distance theo thu tu vertex thanh khoang cach tu diem dau V5.
;; Polyline mo: diem nam sau StartPoint theo chieu da chon -> nil.
;; Polyline kin: wrap qua dau/cuoi Polyline.
(defun bht:route-distance-from-raw (rec ent raw / len start dir closed d)
  (setq len (bht:curve-length ent)
        start (bht:num (bht:get rec "start_dist"))
        dir (if (= (bht:int (bht:get rec "direction")) -1) -1 1)
        closed (bht:curve-closed-p ent))
  (if (or (not (numberp raw)) (not (numberp len)) (< len 1e-9)
          (< raw -0.001) (> raw (+ len 0.001))) nil
    (progn
      (setq raw (max 0.0 (min len raw)))
      (if (null start) (setq start 0.0))
      (setq start (max 0.0 (min len start))
            d (if (= dir -1) (- start raw) (- raw start)))
      (if closed
        (progn
          (while (< d -1e-8) (setq d (+ d len)))
          (while (>= d (- len 1e-8)) (setq d (- d len)))
          (if (< (abs d) 1e-8) 0.0 d))
        (if (< d -1e-8) nil (max 0.0 d))))))

;; (model-distance offset side-model closest raw-distance). side-model: 1 trai, -1 phai.
(defun bht:route-project (rec ent pt / p d dir)
  (setq p (bht:curve-project ent pt))
  (if (null p) nil
    (progn
      (setq d (bht:route-distance-from-raw rec ent (car p))
            dir (if (= (bht:int (bht:get rec "direction")) -1) -1 1))
      (if (null d) nil
        (list d (cadr p) (* (caddr p) dir) (cadddr p) (car p))))))

;; Gan diem dau + chieu tang ly trinh. Giu route_id; tang revision khi thay doi.
;; Station Control duoc giu lai nhung danh dau can xem lai, khong tu sua moc TDT/RTK.
(defun bht:route-set-start-dir (id pt direction / rec ent raw oldstart olddir next sig)
  (setq rec (bht:route-read id) ent (if rec (bht:route-ent rec) nil)
        direction (if (= direction -1) -1 1))
  (cond
    ((null rec) (list nil "không có tuyến"))
    ((null ent) (list nil "không tìm thấy Polyline tuyến"))
    ((null (setq raw (bht:curve-project ent pt))) (list nil "không chiếu được điểm đầu lên tuyến"))
    (T
     (setq oldstart (bht:num (bht:get rec "start_dist")) olddir (bht:int (bht:get rec "direction")))
     (if (null oldstart) (setq oldstart 0.0))
     (if (null olddir) (setq olddir 1))
     (setq next (bht:set rec "start_dist" (bht:fnum (car raw) 6))
           next (bht:set next "start_x" (bht:fnum (car (cadddr raw)) 4))
           next (bht:set next "start_y" (bht:fnum (cadr (cadddr raw)) 4))
           next (bht:set next "direction" (itoa direction))
           next (bht:set next "station_control_status" (if (bht:route-marks rec) "NEEDS_REVIEW" "CHUA_CO_MOC")))
     (setq sig (bht:route-geometry-signature ent (car raw) direction)
           next (bht:set next "geometry_signature" sig)
           next (bht:set next "geometry_length" (bht:fnum (bht:curve-length ent) 6)))
     (if (or (> (abs (- oldstart (car raw))) 1e-6) (/= olddir direction))
       (setq next (bht:route-bump next "Đổi điểm đầu/chiều tuyến")))
     (bht:route-write id next)
     (list T id (bht:route-revision next) (car raw) direction))))

;; Live controls from saved numbered markers. Never persist copies: editing/deleting
;; a marker or changing route geometry takes effect on the next explicit calculation.
;; Only a single eligible route is accepted; overlapping routes require an explicit control.
(defun bht:obj-manual-position-p (rec index)
  (or (= (bht:get rec "position_source") "MANUAL_CAD")
      (= (bht:get rec "station_control_eligible") "0")
      (vl-some '(lambda (id / pt) (setq pt (bht:pt-find id index))
        (or (= (bht:pv pt 'src) "MANUAL_CAD") (= (bht:pv pt 'ds) "MANUAL_CAD"))) (bht:get-all rec "pt"))))

(defun bht:route-tdt-controls-p (rec)
  (or (/= (bht:get rec "tdt_source_handle") "")
      (vl-string-search "TDTSOLUTION" (strcase (bht:get rec "nguon")))
      (= (bht:get rec "station_control_mode") "ROUTE_ONLY")
      (= (bht:get rec "station_source") "TDT_STAKES")
      (vl-some '(lambda (m) (bht:starts (strcase (nth 3 m)) "TDT_STAKES")) (bht:route-marks rec))))

(defun bht:route-object-controls (rec / marks conflicts skipped routes index item obj station pos candidates rr ent pr maxoff hit near source route)
  (setq marks (bht:route-marks rec) conflicts nil skipped nil
        routes (bht:rec-all "ROUTE") index (bht:pt-all))
  ;; Confirmed TDT controls define this alignment. Imported Km/H captions remain
  ;; survey attributes and must not silently recalibrate the new TDT alignment.
  (if (not (bht:route-tdt-controls-p rec)) (foreach item (bht:rec-all "OBJ")
    (setq obj (cdr item) station (bht:obj-marker-metres obj))
    (if (and station (not (bht:obj-manual-position-p obj index)) (setq pos (bht:obj-position obj index)))
      (progn
        (setq candidates nil)
        (foreach route routes
          (setq rr (cdr route) ent (bht:route-ent rr) maxoff (bht:num (bht:get rr "max_offset")))
          (if (and ent (car (bht:route-check-ent ent)) maxoff
                   (setq pr (bht:route-project rr ent pos)) (<= (cadr pr) maxoff))
            (setq candidates (cons (list (car route) (car pr)) candidates))))
        (if (setq hit (assoc (bht:get rec "route_id") candidates))
          (if (/= (length candidates) 1)
            (setq skipped (cons (car item) skipped))
            (progn
              (setq source (strcat "Hồ sơ " (car item))
                    near (vl-some '(lambda (m) (if (< (abs (- (car m) (cadr hit))) 0.001) m)) marks))
              (cond
                ;; A station equation can legitimately have two values at the same point.
                (near (if (and (> (abs (- station (cadr near))) 0.001)
                               (> (abs (- station (caddr near))) 0.001))
                        (setq conflicts (cons (strcat source " khác giá trị mốc cùng vị trí") conflicts))))
                ((vl-some '(lambda (m) (or (< (abs (- station (cadr m))) 0.001)
                                          (< (abs (- station (caddr m))) 0.001))) marks)
                  (setq conflicts (cons (strcat source " trùng số Km/H ở vị trí khác") conflicts)))
                (T (setq marks (cons (list (cadr hit) station station source "Km/H đã nhập") marks)))))))))))
  (list (vl-sort marks '(lambda (a b) (< (car a) (car b)))) conflicts skipped))

(defun bht:route-controls (rec / cached)
  (if (setq cached (assoc (bht:get rec "route_id") *bht-control-cache*))
    (cdr cached)
    (bht:route-object-controls rec)))

;; Ly trinh cua mot diem theo 1 tuyen.
;; Tra ve assoc: status station offset side route dist note ratio
(defun bht:station-route (rec pt / ent pr maxoff km side dir chieu has-v5 controls)
  (setq ent (bht:route-ent rec) maxoff (bht:num (bht:get rec "max_offset"))
        chieu (bht:int (bht:get rec "chieu")))
  (cond
    ((null ent) (list (cons 'status "TUYEN_MAT") (cons 'route (bht:get rec "route_id"))
                      (cons 'note "không tìm thấy Polyline tuyến (đã xóa?)")))
    ((not (car (bht:route-check-ent ent)))
     (list (cons 'status "TUYEN_KHONG_HOP_LE") (cons 'route (bht:get rec "route_id"))
           (cons 'note (cadr (bht:route-check-ent ent)))))
    ((null (setq pr (bht:route-project rec ent pt)))
     (list (cons 'status "NGOAI_PHAM_VI_HINH_HOC") (cons 'route (bht:get rec "route_id"))
           (cons 'revision (bht:route-revision rec))
           (cons 'note "điểm nằm trước StartPoint hoặc không chiếu được lên tuyến")))
    ((and maxoff (> (cadr pr) maxoff))
     (list (cons 'status "XA_TUYEN") (cons 'route (bht:get rec "route_id")) (cons 'offset (cadr pr))
           (cons 'note (strcat "cách tuyến " (bht:fnum (cadr pr) 2) " m > " (bht:fnum maxoff 1) " m"))))
    (T
     (setq controls (bht:route-controls rec)
           km (if (cadr controls)
                (list "MAU_THUAN_MOC" nil nil nil (bht:join (cadr controls) "; "))
                (bht:km-from-dist (car controls) (car pr)
                  (bht:num (bht:get rec "ngoai_suy_m")) chieu))
           dir (caddr km))
     (if (caddr controls)
       (setq km (append (list (car km) (cadr km) (caddr km) (cadddr km))
          (list (strcat (nth 4 km) ". Cọc nằm trong phạm vi nhiều tuyến, chưa dùng làm mốc: "
             (bht:join (caddr controls) ", ") ". Kiểm tra phạm vi tuyến hoặc khai báo mốc riêng.")))))
     ;; Route V5: caddr(pr) da la phia theo direction. Route cu: giu logic station-dir.
     (setq has-v5 (/= (bht:get rec "direction") "")
           side (cond ((= (caddr pr) 0) "TREN_TUYEN")
                      (has-v5 (if (= (caddr pr) 1) "TRAI" "PHAI"))
                      ((null dir) "CHUA_XAC_DINH")
                      ((= (* (caddr pr) dir) 1) "TRAI")
                      (T "PHAI")))
     (list (cons 'status (car km)) (cons 'station (cadr km)) (cons 'offset (cadr pr))
           (cons 'side side) (cons 'route (bht:get rec "route_id")) (cons 'dist (car pr))
           (cons 'ratio (cadddr km)) (cons 'loai (bht:get rec "loai"))
           (cons 'revision (bht:route-revision rec))
           (cons 'note (nth 4 km)))))
)

;; Chon tuyen tot nhat (offset nho nhat trong pham vi) trong tat ca tuyen.
(defun bht:station (pt / best r rank active ids)
  (setq best nil)
  (setq active (bht:meta "station_active_route" "")
        ids (if (= active "") (bht:route-ids) (if (bht:route-read active) (list active) nil)))
  (foreach id ids
    (setq r (bht:station-route (bht:route-read id) pt))
    (if (cdr (assoc 'offset r))
      (if (or (null best)
              (and (= (cdr (assoc 'status best)) "XA_TUYEN") (/= (cdr (assoc 'status r)) "XA_TUYEN"))
              (and (= (= (cdr (assoc 'status best)) "XA_TUYEN") (= (cdr (assoc 'status r)) "XA_TUYEN"))
                   (< (cdr (assoc 'offset r)) (cdr (assoc 'offset best)))))
        (setq best r))
      (if (null best) (setq best r))))
  (if best best (list (cons 'status "CHUA_CO_TUYEN") (cons 'note (if (= active "") "chưa khai báo tuyến tham chiếu" "Tim tuyến đang dùng đã bị xóa; chọn lại tim để tính."))))
)

;; Keep the presentation anchor at the centroid. Measurements of multi-post
;; signs use the surveyed support nearest the particular alignment in XY.
(defun bht:station-support (rec index route fallback / count ent best pid p xyz pr)
  (if (and (member (bht:get rec "nhom") '("BIEN_BAO" "BANG_CHI_DAN" "BANG_QC" "KHAC" "CHUA_XAC_DINH"))
           (setq count (bht:int (bht:get rec "so_tru"))) (> count 1) (setq ent (bht:route-ent route)))
    (foreach pid (acad_strlsort (bht:get-all rec "pt"))
      (if (and (setq p (bht:pt-find pid index)) (setq xyz (bht:pv p 'xyz)) (setq pr (bht:curve-project ent xyz))
               (or (null best) (< (cadr pr) (- (nth 2 best) 1e-9))))
        (setq best (list pid xyz (cadr pr) (cadddr pr))))))
  (if best best (list "" fallback nil nil)))

(defun bht:station-object-route (route rec index pos / support point result ent pr note)
  (setq support (bht:station-support rec index route pos) point (cadr support)
        result (if point (bht:station-route route point) '((status . "KHONG_CO_VI_TRI")))
        ent (bht:route-ent route) pr (if (and ent point) (bht:curve-project ent point)))
  (if (/= (car support) "")
    (progn
      (setq note (strcat (bht:str (cdr (assoc 'note result))) "; trụ gần tim nhất: " (car support)))
      (setq result (if (assoc 'note result) (subst (cons 'note note) (assoc 'note result) result) (append result (list (cons 'note note)))))))
  (append result (list (cons 'point_id (car support)) (cons 'point point)
    (cons 'projection (if pr (cadddr pr)))
    (cons 'point_method (if (= (car support) "") "REPRESENTATIVE" "NEAREST_SUPPORT")))))

(defun bht:station-object (rec index pos / best r active ids id)
  (setq active (bht:meta "station_active_route" "")
        ids (if (= active "") (bht:route-ids) (if (bht:route-read active) (list active) nil)))
  (foreach id ids
    (setq r (bht:station-object-route (bht:route-read id) rec index pos))
    (if (cdr (assoc 'offset r))
      (if (or (null best) (null (cdr (assoc 'offset best)))
              (and (= (cdr (assoc 'status best)) "XA_TUYEN") (/= (cdr (assoc 'status r)) "XA_TUYEN"))
              (and (= (= (cdr (assoc 'status best)) "XA_TUYEN") (= (cdr (assoc 'status r)) "XA_TUYEN"))
                   (< (cdr (assoc 'offset r)) (cdr (assoc 'offset best))))) (setq best r))
      (if (null best) (setq best r))))
  (if best best (bht:station pos)))

(defun bht:station-point-record (rec result / point projection pair)
  (setq point (cdr (assoc 'point result)) projection (cdr (assoc 'projection result)))
  (foreach pair (list (cons "station_point_id" (bht:str (cdr (assoc 'point_id result))))
    (cons "station_point_method" (bht:str (cdr (assoc 'point_method result))))
    (cons "station_point_e" (if point (bht:fnum (car point) 6) ""))
    (cons "station_point_n" (if point (bht:fnum (cadr point) 6) ""))
    (cons "station_projection_e" (if projection (bht:fnum (car projection) 6) ""))
    (cons "station_projection_n" (if projection (bht:fnum (cadr projection) 6) "")))
    (setq rec (bht:set rec (car pair) (cdr pair)))) rec)

;; Explicit inspection only: no automatic DIMs while calculating or opening DWG.
(defun bht:offset-dimension (oid / rec index pos id route result point projection offset size native saved)
  (setq rec (bht:obj-read oid) index (bht:pt-all) pos (if rec (bht:obj-position rec index))
        id (if rec (bht:seg-route rec) ""))
  (if (and rec (= id "")) (setq id (bht:get rec "route_id")))
  (if (= id "") (setq id (bht:meta "station_active_route" "")))
  (setq route (if (/= id "") (bht:route-read id)))
  (cond ((null rec) (list 'LOI "Không tìm thấy hồ sơ."))
    ((null pos) (list 'LOI "Hồ sơ chưa có điểm khảo sát hợp lệ."))
    ((null route) (list 'LOI "Hồ sơ chưa có tim. Chọn tim và tính lý trình trước khi DIM."))
    ((null (bht:route-ent route)) (list 'LOI "Tim đã mất hình học; chọn lại tim trước khi DIM."))
    (T
      (setq result (bht:station-object-route route rec index pos)
            point (cdr (assoc 'point result)) projection (cdr (assoc 'projection result))
            saved (bht:num (bht:get rec "offset_m")) size (max 0.1 (* 0.10 (bht:kh-scale-for rec))))
      (cond ((or (null point) (null projection)) (list 'LOI "Không chiếu được vị trí hồ sơ lên tim."))
        ((not (bht:bridge-load 'BHTOFFSETDIM)) (list 'LOI "Cần BHT.Bridge.dll cùng phiên bản để tạo DIM."))
        (T
          (setq offset (distance (list (car point) (cadr point)) (list (car projection) (cadr projection))))
          (setq native (BHTOFFSETDIM oid id (bht:str (cdr (assoc 'point_id result)))
            (car point) (cadr point) (car projection) (cadr projection) size))
          (if (/= (substr native 1 3) "OK|") (list 'LOI native)
            (list (strcat "DIM thật trên lớp BHT_DIM_KIEM_TRA, tim " id ": " (bht:fnum offset 3) " m.")
              (if (= (cdr (assoc 'point_id result)) "") "Dùng vị trí đại diện của hồ sơ."
                (strcat "Lấy trụ gần tim nhất: " (cdr (assoc 'point_id result)) "."))
              (if (and saved (> (abs (- saved offset)) 0.001))
                (strcat "Offset đã lưu " (bht:fnum saved 3) " m khác DIM hiện tại; tính lại lý trình để cập nhật.")
                "DIM lấy khoảng cách hình học thực, không ghi đè số đo bằng chữ."))))))))

(defun c:BHTDIMTIM (/ *error* oid result started)
  (defun *error* (msg) (if started (command-s "_.UNDO" "_End")) (bht:on-error msg))
  (if (setq oid (bht:pick-object "Chọn biển/điểm hồ sơ để DIM tới tim"))
    (progn
      (if (= 0 (logand 8 (getvar "UNDOCTL")))
        (progn (command-s "_.UNDO" "_Begin") (setq started T)))
      (setq result (bht:offset-dimension oid))
      (if started (command-s "_.UNDO" "_End")) (setq started nil)
      (if (eq (car result) 'LOI) (bht:warn (cadr result)) (foreach line result (bht:msg line)))))
  (princ))

(defun bht:station-determined (st)
  (member st '("NOI_SUY" "CAN_KIEM_TRA" "NGOAI_SUY" "MOT_MOC"))
)

(defun bht:station-status-v5 (st)
  (cond
    ((bht:station-determined st) "VALID")
    ((member st '("XA_TUYEN" "NGOAI_PHAM_VI_HINH_HOC")) "OUT_OF_RANGE")
    ((member st '("CHUA_CO_TUYEN" "TUYEN_MAT" "TUYEN_KHONG_HOP_LE" "LOI_HINH_HOC")) "NO_ROUTE")
    ((member st '("CHUA_CO_MOC" "CHUA_XAC_DINH" "MAU_THUAN_MOC")) "NO_STATION_CONTROL")
    (T st)))

;; Tinh va luu ly trinh cho moi doi tuong. Tra ve (so_da_tinh so_chua_xd)
(defun bht:station-objects-base (/ index rec pos r ok und st failed *bht-control-cache*)
  (if (bht:field-mode-p) (bht:field-objects) (progn
  (setq index (bht:pt-all) ok 0 und 0
        *bht-control-cache* (mapcar '(lambda (p) (cons (car p) (bht:route-object-controls (cdr p)))) (bht:rec-all "ROUTE")))
  (foreach oid (bht:progress-open-ids)
    (setq rec (bht:obj-read oid) pos (bht:obj-position rec index))
    (setq r (if pos (bht:station-for-object rec pos index) (list (cons 'status "KHONG_CO_VI_TRI") (cons 'note "đối tượng không có điểm RTK"))))
    (setq st (cdr (assoc 'status r)))
    (setq rec (bht:set (bht:set rec "station_source" "ROUTE") "field_last_error" "")
          rec (bht:set rec "vi_tri_e" (if pos (bht:fnum (car pos) 3) ""))
          rec (bht:set rec "vi_tri_n" (if pos (bht:fnum (cadr pos) 3) ""))
          rec (bht:set rec "route_id" (bht:str (cdr (assoc 'route r))))
          rec (bht:set rec "trang_thai_km" st)
          rec (bht:set rec "ly_trinh_m" (if (bht:station-determined st) (bht:fnum (cdr (assoc 'station r)) 3) ""))
          rec (bht:set rec "ly_trinh_km" (if (bht:station-determined st) (bht:fmt-km (cdr (assoc 'station r))) ""))
          rec (bht:set rec "offset_m" (if (cdr (assoc 'offset r)) (bht:fnum (cdr (assoc 'offset r)) 3) ""))
          rec (bht:set rec "phia_tuyen" (bht:str (cdr (assoc 'side r))))
          rec (bht:set rec "loai_tuyen" (bht:str (cdr (assoc 'loai r))))
          rec (bht:set rec "station_route_revision" (if (cdr (assoc 'revision r)) (itoa (cdr (assoc 'revision r))) ""))
          rec (bht:set rec "station_status" (bht:station-status-v5 st))
          rec (bht:set rec "nguon_km" (bht:str (cdr (assoc 'note r)))))
    (setq rec (bht:station-point-record rec r))
    (setq rec (bht:seg-refresh-record rec))
    (if (and (not failed) (not (bht:obj-write oid rec))) (setq failed (strcat "Không ghi được lý trình " oid)))
    (if (or (bht:obj-marker-metres rec) (bht:station-determined st)) (setq ok (1+ ok)) (setq und (1+ und))))
  (if failed (list 'LOI failed) (progn (bht:kh-station-label-refresh nil) (list ok und)))
)))

(defun bht:station-objects (/ result)
  (bht:kh-display-transaction 'bht:station-objects-base nil))

(defun bht:route-type (value / s)
  (setq s (strcase (bht:trim value)))
  (cond ((member s '("D" "TIM_DUONG" "TIM DUONG" "TIM ĐƯỜNG")) "TIM_DUONG")
        ((member s '("R" "TIM_RANH" "TIM RANH" "TIM RÃNH")) "TIM_RANH")
        ((member s '("K" "KHAC" "KHÁC" "TUYẾN KHÁC")) "KHAC") (T nil)))

(defun bht:route-options (title id loai maxoff extrap / reply v)
  (if (not (bht:fn-defined-p 'BHTROUTEOPTIONS)) (bht:palette-load))
  (if (bht:fn-defined-p 'BHTROUTEOPTIONS)
    (setq reply (vl-catch-all-apply 'BHTROUTEOPTIONS (list title id loai maxoff extrap))))
  (cond
    ((and reply (vl-catch-all-error-p reply)) (bht:warn (vl-catch-all-error-message reply)) nil)
    ((= (car reply) "LOI") (bht:warn (cadr reply)) nil)
    ((= (car reply) "CANCEL") nil)
    ((= (car reply) "OK") (list (nth 1 reply) (bht:route-type (nth 2 reply)) (bht:num (nth 3 reply)) (bht:num (nth 4 reply))))
    (T
     (setq id (strcase (bht:ask-string "ID tuyến" id))
           v (bht:ask-string "Loại tim: D = Tim đường, R = Tim rãnh, K = Khác" loai)
           maxoff (bht:num (bht:replace (bht:ask-string "Phạm vi lệch tim tối đa (m)" maxoff) "," "."))
           extrap (bht:num (bht:replace (bht:ask-string "Ngoại suy tối đa (m)" extrap) "," ".")))
     (list id (bht:route-type v) maxoff extrap))))

(defun c:BHTTUYEN (/ *error* sel ent chk id loai options maxoff extrap res)
  (setq *error* bht:on-error)
  (if (setq sel (entsel "\nChọn Polyline tuyến tham chiếu (KHÔNG chọn tuyến proxy TDT): "))
    (progn
      (setq ent (car sel) chk (bht:route-check-ent ent))
      (if (not (car chk))
        (bht:warn (strcat "BHT: " (cadr chk)))
        (progn
          (setq options (bht:route-options "Nạp tim Polyline" (strcat "TUYEN" (itoa (1+ (length (bht:route-ids))))) "TIM_DUONG" "100" "0"))
          (if options (setq id (nth 0 options) loai (nth 1 options) maxoff (nth 2 options) extrap (nth 3 options)))
          (if (and options loai maxoff extrap (> maxoff 0) (>= extrap 0))
            (progn
              (setq res (bht:route-create id ent loai maxoff extrap nil "Polyline người dùng chọn (BHTTUYEN)"))
              (if (car res)
                (progn
                  (bht:msg (strcat "BHT: đã tạo tuyến " id ". Chọn đầu tuyến và xác nhận chiều tăng lý trình."))
                  (bht:route-pick-start id)
                  (bht:msg "Cọc đã nhập số Km/H tự làm mốc khi xác định được một tuyến. BHTMOCKM dùng cho mốc bổ sung hoặc điểm gãy Km."))
                (bht:err (strcat "BHT: " (cadr res))))))))))
  (bht:log-flush)
  (princ)
)

;; Tim ban ghi tuyen da tao tu cung mot TdtDbAlignment.
(defun bht:route-by-tdt-source (source-handle / hit rec id)
  (setq hit nil)
  (foreach id (bht:route-ids)
    (setq rec (bht:route-read id))
    (if (= (strcase (bht:get rec "tdt_source_handle")) (strcase source-handle))
      (setq hit id)))
  hit
)

;; Tich hop TDTSolution 9.1 ban thuong. Bridge chi mo tim TDT ForRead, Explode vao bo nho,
;; roi tao/cap nhat mot LWPOLYLINE tham chieu rieng tren layer BHT_TUYEN_TDT.
;; BHT tinh ly trinh tren ban sao nay de khong sua hoac khoa doi tuong goc cua TDT.

;; AutoLISP khong co ham kiem tra "fbound" kieu Common Lisp (0.4.6 goi ham do -> loi
;; "no function definition" khi chay BHTTUYENTDT).
;; Ham da dinh nghia khi gia tri cua ky hieu la SUBR (ham san), USUBR (defun)
;; hoac EXRXSUBR (ObjectARX / .NET [LispFunction] nhu BHTTDT91ROUTE).
(defun bht:fn-defined-p (sym / v)
  (setq v (if (and sym (= (type sym) 'SYM)) (vl-catch-all-apply 'eval (list sym)) nil))
  (if (and v (not (vl-catch-all-error-p v)) (member (type v) '(SUBR USUBR EXRXSUBR))) T nil)
)

;; Ham Lisp cua BHT.Bridge (BHTTDT91ROUTE ...) chi co khi BHT.Bridge.dll da duoc
;; AutoCAD NETLOAD. Palette chi NETLOAD BHT.Palette.dll; Bridge co the chua dang ky
;; ham Lisp -> nap BHT.Bridge.dll nam canh file Lisp (cung co che bht:palette-load).
(defun bht:bridge-load (fn / result)
  (cond
    ((bht:fn-defined-p fn) T)
    ((or (null *bht-bridge-dll*) (not (findfile *bht-bridge-dll*))) nil)
    (T
      (setq result (vl-catch-all-apply 'vl-cmdf (list "_.NETLOAD" *bht-bridge-dll*)))
      (and (not (vl-catch-all-error-p result)) (bht:fn-defined-p fn))))
)

(defun c:BHTTUYENTDT (/ *error* sel src src-h existing-id existing-rec existing-h id loai options maxoff extrap r ref res rec oldsig newsig start dir)
  (setq *error* bht:on-error)
  (if (not (bht:bridge-load 'BHTTDT91ROUTE))
    (bht:err (strcat "BHT: chưa nạp được BHT.Bridge.dll (hàm BHTTDT91ROUTE). Kiểm tra BHT.Bridge.dll nằm cạnh BHT-"
                     *bht-version* ".lsp; nếu vẫn lỗi, đóng AutoCAD, cài lại BHT " *bht-version*
                     " rồi mở bằng profile TDT 9.1."))
    (if (setq sel (entsel "\nChọn tim tuyến TDTSolution 9.1: "))
      (progn
        (setq src (car sel) src-h (cdr (assoc 5 (entget src)))
              existing-id (bht:route-by-tdt-source src-h)
              existing-rec (if existing-id (bht:route-read existing-id) nil)
              existing-h (if existing-rec (bht:get existing-rec "handle") ""))
        (if existing-id
          (setq id existing-id loai (bht:get existing-rec "loai") maxoff (bht:num (bht:get existing-rec "max_offset"))
                extrap (bht:num (bht:get existing-rec "ngoai_suy_m")))
          (progn
            (setq options (bht:route-options "Nạp tim TDT" (strcat "TUYEN" (itoa (1+ (length (bht:route-ids))))) "TIM_DUONG" "100" "0"))
            (if options (setq id (nth 0 options) loai (nth 1 options) maxoff (nth 2 options) extrap (nth 3 options)))))
        (if (and id loai (bht:valid-id id) (or existing-id (null (bht:route-read id))) maxoff (> maxoff 0.0) extrap (>= extrap 0.0))
          (progn
            (setq r (vl-catch-all-apply 'BHTTDT91ROUTE (list src-h existing-h)))
            (cond
              ((vl-catch-all-error-p r)
               (bht:err (strcat "BHT: không gọi được tích hợp TDT 9.1: " (vl-catch-all-error-message r))))
              ((or (not (listp r)) (/= (strcase (bht:str (car r))) "OK"))
               (bht:err (strcat "BHT: " (if (and (listp r) (cadr r)) (bht:str (cadr r)) "không lấy được hình học tim TDT 9.1."))))
              (T
               (setq ref (handent (cadr r)))
               (if (null ref)
                 (bht:err "BHT: Bridge đã trả về nhưng không tìm thấy Polyline tham chiếu.")
                 (progn
                   (if existing-id
                     (setq res (list T existing-id))
                     (setq res (bht:route-create id ref loai maxoff extrap nil
                                 "Bản sao tham chiếu từ TDTSolution 9.1")))
                   (if (car res)
                     (progn
                       (setq rec (bht:route-read id)
                             oldsig (bht:get rec "geometry_signature")
                             start (bht:num (bht:get rec "start_dist"))
                             dir (if (= (bht:int (bht:get rec "direction")) -1) -1 1)
                             newsig (bht:route-geometry-signature ref (if start start 0.0) dir)
                             rec (bht:set rec "handle" (cadr r))
                             rec (bht:set rec "nguon" "TDTSolution 9.1 bản thường (đọc tim gốc, tính trên bản sao)")
                             rec (bht:set rec "tdt_source_handle" src-h)
                             rec (bht:set rec "station_control_mode" "ROUTE_ONLY")
                             rec (bht:set rec "tdt_source_class" (bht:str (caddr r)))
                             rec (bht:set rec "tdt_refreshed_at" (bht:now))
                             rec (bht:set rec "geometry_length" (bht:fnum (bht:curve-length ref) 6))
                             rec (bht:set rec "geometry_signature" newsig))
                       (if (and existing-id (/= oldsig "") (/= oldsig newsig))
                         (setq rec (bht:route-bump rec "Hình học tim TDT thay đổi")))
                       (bht:route-write id rec)
                       (bht:msg (strcat "BHT: " (if existing-id "đã cập nhật" "đã tạo") " tuyến " id
                                        " từ tim TDT 9.1; Polyline tham chiếu handle " (cadr r)
                                        "."))
                       (if (not existing-id) (bht:route-pick-start id))
                       (bht:msg "Đã lấy hình học tim; chưa tự lấy lý trình gốc TDT. Chạy BHTDOCCOCTDT để đọc / kiểm tra cọc TDT rồi xác nhận mốc. Nếu không có nhãn Km, dùng BHTMOCKM để khai báo mốc đã biết."))
                     (progn
                       (if (not existing-id) (entdel ref))
                       (bht:err (strcat "BHT: " (cadr res))))))))))
          (if (or existing-id options) (bht:warn (strcat "BHT: không tạo tuyến - ID và loại tim phải hợp lệ, ID chưa trùng; khoảng cách tối đa > 0, ngoại suy >= 0"
                           (if existing-id (strcat " (kiểm tra max_offset/ngoai_suy_m của tuyến " existing-id ")") "")
                           ".")))))))
  (bht:log-flush)
  (princ)
)

;; Doc nhan Km, block long nhau va nhan trong TDT. Bridge chi doc ForRead.
;; Candidate: (model-dist station offset x y type handle text layer confidence raw-dist).
(defun bht:tdt-stake-candidates (rec / ent r out f d line raw station offset x y confidence invalid)
  (setq ent (bht:route-ent rec) out nil)
  (cond
    ((null ent) (list 'LOI "không tìm thấy Polyline tuyến"))
    ((not (bht:bridge-load 'BHTTDT91STAKES)) (list 'LOI "chưa nạp được BHT.Bridge.dll / BHTTDT91STAKES"))
    (T
     (setq r (vl-catch-all-apply 'BHTTDT91STAKES
               (list (bht:get rec "handle") (if (bht:num (bht:get rec "max_offset")) (bht:num (bht:get rec "max_offset")) 100.0))))
     (cond
       ((vl-catch-all-error-p r) (list 'LOI (vl-catch-all-error-message r)))
       ((or (not (listp r)) (/= (strcase (bht:str (car r))) "OK"))
        (list 'LOI (if (cadr r) (bht:str (cadr r)) "không đọc được cọc")))
       (T
        (foreach line (cdr r)
          (if (= (type line) 'STR)
            (progn
              (setq f (bht:split line "|") raw (bht:num (nth 1 f)) station (bht:num (nth 0 f))
                    offset (bht:num (nth 2 f)) x (bht:num (nth 3 f)) y (bht:num (nth 4 f))
                    confidence (bht:int (nth 9 f)))
              (if (and (= (length f) 10) raw station offset (>= offset 0.0) x y confidence
                       (>= confidence 0) (<= confidence 100) (/= (nth 6 f) ""))
                (progn
                  (setq d (bht:route-distance-from-raw rec ent raw))
                  (if d (setq out (cons (list d station offset x y (nth 5 f) (nth 6 f)
                                              (nth 7 f) (nth 8 f) confidence raw) out))))
                (setq invalid T)))
            (setq invalid T)))
        (if invalid (list 'LOI "Dữ liệu cọc trả về thiếu/sai số khoảng cách hoặc lý trình; chưa nạp mốc nào. Kiểm tra tim TDT và nạp BHT.Bridge cùng phiên bản.")
          (vl-sort out '(lambda (a b) (< (car a) (car b))))))))))

;; Tra alist (handle . canh-bao). Cọc có cảnh báo KHÔNG tự đưa vào Station Control.
(defun bht:tdt-stake-issues (cands / out prev dd ds ratio reason c)
  (setq out nil prev nil)
  (foreach c cands
    (setq reason nil)
    (if prev
      (progn
        (setq dd (- (car c) (car prev)) ds (- (cadr c) (cadr prev)))
        (cond
          ((< (abs dd) 0.01) (setq reason "trùng vị trí hình học"))
          ((< (abs ds) 0.01) (setq reason "trùng lý trình"))
          ((< ds -0.01) (setq reason "lý trình giảm/ngược chiều"))
          ((and (> (abs dd) 0.01) (> (abs (- (/ ds dd) 1.0)) 0.10))
           (setq reason (strcat "tỷ lệ ΔKm/Δd=" (bht:fnum (/ ds dd) 4) " bất thường"))))))
    (if (or (null (nth 9 c)) (< (nth 9 c) 85))
      (setq reason (if reason (strcat reason "; độ tin cậy thấp") "độ tin cậy thấp")))
    (if reason (setq out (cons (cons (nth 6 c) reason) out)))
    (setq prev c))
  (reverse out))

(defun bht:route-add-tdt-stakes (id cands issues / rec marks n skipped next c)
  (setq rec (bht:route-read id) marks (bht:get-all rec "moc") n 0 skipped 0)
  (foreach c cands
    (if (assoc (nth 6 c) issues)
      (setq skipped (1+ skipped))
      (if (vl-some '(lambda (m) (< (abs (- (car m) (car c))) 0.001)) (bht:route-marks (bht:set-all rec "moc" marks)))
        (setq skipped (1+ skipped))
        (progn
          (setq marks (append marks (list
            (bht:join (list (bht:fnum (car c) 4) (bht:fnum (cadr c) 4) (bht:fnum (cadr c) 4)
                            (strcat "TDT_STAKES " (nth 6 c))
                            (strcat (nth 7 c) "; offset " (bht:fnum (nth 2 c) 2) " m")) "|"))))
          (setq n (1+ n))))))
  (if (> n 0)
    (progn
      (setq next (bht:set-all rec "moc" marks)
            next (bht:set next "station_source" "TDT_STAKES")
            next (bht:set next "station_control_status" "VALIDATED_BY_USER")
            next (bht:set next "tdt_stake_count" (itoa (length cands)))
            next (bht:set next "tdt_stake_warning_count" (itoa (length issues)))
            next (bht:route-bump next "Nạp cọc TDT đã được người dùng xác nhận"))
      (bht:route-write id next)))
  (list n skipped))

(defun c:BHTDOCCOCTDT (/ *error* id rec cands issues ans r i c)
  (setq *error* bht:on-error)
  (if (and (setq id (bht:ask-route)) (setq rec (bht:route-read id)))
    (progn
      (setq cands (bht:tdt-stake-candidates rec))
      (if (and (listp cands) (= (car cands) 'LOI))
        (bht:err (strcat "BHT: " (cadr cands)))
        (progn
          (setq issues (bht:tdt-stake-issues cands) i 0)
          (bht:msg (strcat "BHT: tìm thấy " (itoa (length cands)) " cọc Km gần tuyến; "
                           (itoa (length issues)) " cọc cảnh báo (không tự nạp)."))
          (foreach c cands
            (bht:msg (strcat "  [" (itoa i) "] " (bht:fmt-km (cadr c)) "  d=" (bht:fnum (car c) 2)
                             "  offset=" (bht:fnum (nth 2 c) 2) " m  " (nth 5 c) "#" (nth 6 c)
                             (if (assoc (nth 6 c) issues) (strcat "  <- CẢNH BÁO: " (cdr (assoc (nth 6 c) issues))) "")))
            (setq i (1+ i)))
          (if (null cands) (bht:warn "BHT: chưa đọc được nhãn KmN+M quanh tuyến. Kiểm tra đã nạp module TDT, chọn đúng tim và khoảng cách quét tới nhãn cọc.")
            (progn
              (setq ans (strcase (bht:ask-string "Nạp các cọc KHÔNG cảnh báo vào Station Control? [C/K]" "K")))
              (if (= ans "C")
                (progn (setq r (bht:route-add-tdt-stakes id cands issues))
                       (bht:msg (strcat "BHT: đã nạp " (itoa (car r)) " cọc; bỏ qua " (itoa (cadr r))
                                        " cọc cảnh báo/trùng. Route revision " (itoa (bht:route-revision (bht:route-read id))) ".")))
                (bht:msg "BHT: chỉ đọc/kiểm tra cọc, chưa thay đổi Station Control."))))))))
  (bht:log-flush) (princ))

(defun bht:ask-route (/ ids v sel)
  (setq ids (bht:route-ids) sel *bht-route-selected* *bht-route-selected* nil)
  (cond
    ((null ids) (bht:warn "BHT: chưa có tuyến. Dùng BHTTUYEN trước.") nil)
    ((and sel (member sel ids))
      sel)
    ((= (length ids) 1) (car ids))
    (T (setq v (strcase (bht:ask-string (strcat "ID tuyến (" (bht:join ids ", ") ")") (car ids))))
       (if (member v ids) v (progn (bht:warn "Không có tuyến này.") nil))))
)

;; Thuc the xem truoc ton tai qua getstring/REGEN; grdraw bi xoa khi CAD doi prompt.
;; Cac diem la WCS; khong phu thuoc UCS hien tai. Ham tra ve danh sach de don khi huy/loi.
(defun bht:route-preview-clear (entities)
  (foreach e entities (if (and e (entget e)) (entdel e)))
  (redraw) nil)

(defun bht:route-preview-direction (rec raw direction / ent len param tangent p0 p1 dx dy mag nx ny s a b shaft head out)
  (setq ent (bht:route-ent rec) len (if ent (bht:curve-length ent) nil))
  (if (and ent len (> len 0.01))
    (progn
      (setq raw (max 0.0 (min len raw))
            param (vlax-curve-getParamAtDist ent raw)
            tangent (vl-catch-all-apply 'vlax-curve-getFirstDeriv (list ent param)))
      (setq p0 (vl-catch-all-apply 'vlax-curve-getPointAtDist (list ent raw))
            s (max 0.1 (min (* (getvar "VIEWSIZE") 0.10) (* len 0.20))))
      (if (and p0 tangent (not (vl-catch-all-error-p p0)) (not (vl-catch-all-error-p tangent)))
        (progn
          (setq dx (* (if (= direction -1) -1.0 1.0) (car tangent))
                dy (* (if (= direction -1) -1.0 1.0) (cadr tangent))
                mag (sqrt (+ (* dx dx) (* dy dy))))
          (if (> mag 1e-9)
            (progn
              (setq dx (/ dx mag) dy (/ dy mag) nx (- dy) ny dx
                    p1 (list (+ (car p0) (* s dx)) (+ (cadr p0) (* s dy)) 0.0)
                    a (list (+ (car p1) (* -0.30 s dx) (* 0.13 s nx))
                            (+ (cadr p1) (* -0.30 s dy) (* 0.13 s ny)) 0.0)
                    b (list (+ (car p1) (* -0.30 s dx) (* -0.13 s nx))
                            (+ (cadr p1) (* -0.30 s dy) (* -0.13 s ny)) 0.0)
                    shaft (entmakex (list '(0 . "LWPOLYLINE") '(100 . "AcDbEntity") '(8 . "0") '(62 . 2)
                                          '(100 . "AcDbPolyline") '(90 . 2) '(70 . 0) (cons 43 (* s 0.025))
                                          (cons 10 (list (car p0) (cadr p0))) (cons 10 (list (car p1) (cadr p1)))))
                    out (list shaft)
                    head (entmakex (list '(0 . "SOLID") '(100 . "AcDbEntity") '(8 . "0") '(62 . 2)
                                         '(100 . "AcDbTrace") (cons 10 p1) (cons 11 a) (cons 12 b) (cons 13 b)))
                    out (cons head out))))))))
  (vl-remove nil out))

(defun bht:route-pick-start (id / *error* rec ent pt raw dir ans res preview)
  (defun *error* (msg) (bht:route-preview-clear preview) (bht:on-error msg))
  (if (and id (setq rec (bht:route-read id)) (setq ent (bht:route-ent rec)))
    (if (setq pt (getpoint "\nChọn điểm đầu tuyến trên/gần Polyline: "))
      (progn
        (setq pt (trans pt 1 0) raw (bht:curve-project ent pt) dir 1)
        (if (null raw) (bht:warn "BHT: không chiếu được điểm đầu lên tuyến.")
          (progn
            (setq preview (bht:route-preview-direction rec (car raw) dir))
            (setq ans (strcase (bht:ask-string "Chiều mũi tên [C=Chấp nhận/D=Đảo/K=Hủy]" "C")))
            (if (= ans "D")
              (progn (bht:route-preview-clear preview)
                     (setq dir -1 preview (bht:route-preview-direction rec (car raw) dir))
                     (setq ans (strcase (bht:ask-string "Chấp nhận chiều đã đảo? [C/K]" "C")))))
            (bht:route-preview-clear preview) (setq preview nil)
            (if (= ans "C")
              (progn
                (setq res (bht:route-set-start-dir id pt dir))
                (if (car res)
                  (bht:msg (strcat "BHT: đã lưu StartPoint/chiều tuyến " id ", revision " (itoa (caddr res))
                                   ". Nếu đã có mốc, hãy kiểm tra lại Station Control."))
                  (bht:err (strcat "BHT: " (cadr res)))))
              (bht:msg "BHT: chưa đổi điểm đầu/chiều. Tuyến giữ thiết lập trước bước chọn; dùng BHTROUTESTART để xác nhận lại.")))))
      (bht:msg "BHT: đã bỏ qua chọn đầu tuyến; tuyến giữ thiết lập hiện tại. Dùng BHTROUTESTART để chọn lại.")))
  res)

(defun c:BHTROUTESTART (/ *error*)
  (setq *error* bht:on-error)
  (bht:route-pick-start (bht:ask-route))
  (bht:log-flush) (princ))

(defun c:BHTROUTEREVERSE (/ *error* id rec ent start dir p res ans preview)
  (defun *error* (msg) (bht:route-preview-clear preview) (bht:on-error msg))
  (if (and (setq id (bht:ask-route)) (setq rec (bht:route-read id)) (setq ent (bht:route-ent rec)))
    (progn
      (setq start (bht:num (bht:get rec "start_dist")) dir (if (= (bht:int (bht:get rec "direction")) -1) -1 1))
      (if (null start) (setq start 0.0))
      (setq preview (bht:route-preview-direction rec start (- dir)))
      (setq ans (strcase (bht:ask-string "Đảo chiều tuyến như mũi tên? [C=Chấp nhận/K=Hủy]" "K")))
      (bht:route-preview-clear preview) (setq preview nil)
      (if (= ans "C")
        (progn
          (setq p (vlax-curve-getPointAtDist ent start) res (bht:route-set-start-dir id p (- dir)))
          (if (car res) (bht:msg (strcat "BHT: đã đảo chiều " id ", revision " (itoa (caddr res)) "."))
            (bht:err (strcat "BHT: " (cadr res)))))
        (bht:msg "BHT: giữ nguyên chiều tuyến."))))
  (bht:log-flush) (princ))

(defun bht:route-diag-lines (id / rec ent len start dir marks tdt nstale rev sig current changed closed o controls nold index pos pr)
  (setq rec (bht:route-read id))
  (if (null rec) (list (strcat "Không có tuyến " id))
    (progn
      (setq ent (bht:route-ent rec) len (if ent (bht:curve-length ent) nil)
            start (bht:num (bht:get rec "start_dist")) dir (if (= (bht:int (bht:get rec "direction")) -1) -1 1)
            controls (bht:route-controls rec) marks (car controls) rev (bht:route-revision rec) tdt 0 nstale 0 nold 0 index (bht:pt-all)
            sig (bht:get rec "geometry_signature") closed (if (and ent (bht:curve-closed-p ent)) "Có" "Không"))
      (foreach m marks (if (bht:starts (strcase (nth 3 m)) "TDT") (setq tdt (1+ tdt))))
      (foreach oid (bht:obj-ids)
        (setq o (bht:obj-read oid))
        (if (and (= (bht:get o "route_id") id)
                 (/= (bht:int (bht:get o "station_route_revision")) rev)) (setq nstale (1+ nstale)))
        (if (and (not (bht:obj-marker-metres o)) ent
                 (setq pos (bht:obj-position o index)) (setq pr (bht:route-project rec ent pos))
                 (<= (cadr pr) (bht:num-or-zero (bht:get rec "max_offset")))
                 (or (/= (bht:get o "station_source") "ROUTE") (/= (bht:get o "route_id") id)))
          (setq nold (1+ nold))))
      (setq current (if ent (bht:route-geometry-signature ent (if start start 0.0) dir) "")
            changed (and ent (/= sig "") (/= sig current)))
      (list
        (strcat "ROUTE ID: " id "  | revision " (itoa rev))
        (strcat "Nguồn: " (bht:get rec "nguon"))
        (strcat "Polyline handle: " (bht:get rec "handle") "  | Alignment TDT: " (bht:get rec "tdt_source_handle"))
        (strcat "Closed: " closed "  | Length: " (if len (strcat (bht:fnum len 3) " m") "KHÔNG ĐỌC ĐƯỢC"))
        (strcat "Start distance: " (if start (bht:fnum start 3) "0.000") "  | Direction: " (if (= dir -1) "REVERSE" "FORWARD"))
        (strcat "Station source: " (bht:get rec "station_source") "  | Control points: " (itoa (length marks)))
        (strcat "Phương pháp đang dùng: " (if (bht:field-mode-p) "Cọc đã nhập (không tính theo tim)" "Tim tuyến")
          " | Tim tính lý trình: " (bht:meta "station_active_route" "Tự chọn theo vị trí"))
        (if (bht:route-tdt-controls-p rec) "Nguồn mốc: TDT và mốc bổ sung của tim; không dùng số Km/H hồ sơ cũ."
          "Nguồn mốc: mốc tim và cọc hồ sơ đủ điều kiện.")
        (strcat "Mốc hồ sơ tự động: " (itoa (length (vl-remove-if-not '(lambda (m) (bht:starts (nth 3 m) "Hồ sơ ")) marks)))
          " | Mâu thuẫn: " (bht:join (cadr controls) "; ") " | Cọc chưa rõ tuyến: " (bht:join (caddr controls) ", "))
        (strcat "TDT stakes: " (itoa tdt) "  | Station Control: " (bht:get rec "station_control_status"))
        (strcat "Geometry verified: " (if changed "KHÔNG - nguồn đã thay đổi" (if ent "CÓ" "KHÔNG")))
        (strcat "Hồ sơ cần cập nhật: " (itoa nstale))
        (strcat "Hồ sơ trong phạm vi chưa tính theo tim này: " (itoa nold))
        (strcat "Cập nhật cuối: " (if (/= (bht:get rec "cap_nhat_luc") "") (bht:get rec "cap_nhat_luc") (bht:get rec "tao_luc")))))))

(defun c:BHTROUTEDIAG (/ *error* id lines)
  (setq *error* bht:on-error)
  (if (setq id (bht:ask-route))
    (progn (setq lines (bht:route-diag-lines id)) (foreach l lines (bht:msg l))))
  (bht:log-flush) (princ))

(defun c:BHTMOCKM (/ *error* id rec ent pt pr v s back ahead src res)
  (setq *error* bht:on-error)
  (if (and (setq id (bht:ask-route)) (setq rec (bht:route-read id)) (setq ent (bht:route-ent rec)))
    (if (setq pt (getpoint "\nChọn vị trí mốc (có thể bắt NODE điểm RTK cột Km, ví dụ cockm45): "))
      (progn
        (setq pt (trans pt 1 0) pr (bht:route-project rec ent pt))
        (if (null pr)
          (bht:warn "BHT: không chiếu được điểm lên tuyến.")
          (progn
            (bht:msg (strcat "Vị trí trên tuyến: cách StartPoint theo chiều tuyến " (bht:fnum (car pr) 3) " m, lệch tuyến "
                             (bht:fnum (cadr pr) 3) " m."))
            (setq v (bht:ask-string "Lý trình tại mốc (vd Km45+000) hoặc G = điểm gãy Km" ""))
            (if (= (strcase v) "G")
              (setq back (bht:parse-km (bht:ask-string "Lý trình phía SAU (theo chiều tăng, trước điểm gãy)" ""))
                    ahead (bht:parse-km (bht:ask-string "Lý trình phía TRƯỚC (sau điểm gãy)" "")))
              (setq back (bht:parse-km v) ahead back))
            (if (and back ahead)
              (progn
                (setq src (bht:ask-string "Nguồn mốc (vd RTK cockm45 / bảng cọc TDT / hồ sơ)" ""))
                (setq res (bht:route-add-mark id (car pr) back ahead src
                                              (strcat "lệch " (bht:fnum (cadr pr) 2) " m")))
                (if (car res)
                  (bht:msg (strcat "BHT: đã thêm mốc " (bht:fmt-km back)
                                   (if (/= back ahead) (strcat " / " (bht:fmt-km ahead) " (gãy)") "")
                                   ". Tuyến " id " có " (itoa (cadr res)) " mốc."))
                  (bht:err (strcat "BHT: " (cadr res)))))
              (bht:warn "BHT: lý trình không hợp lệ.")))))))
  (bht:log-flush)
  (princ)
)

(defun bht:print-marks (id / rec marks i prev ratio controls m note)
  (setq rec (bht:route-read id) marks (bht:route-marks rec) i 0 prev nil)
  (bht:msg (strcat "Tuyến " id " | loại " (bht:get rec "loai") " | handle " (bht:get rec "handle")
                   " | lệch tối đa " (bht:get rec "max_offset") " m | ngoại suy " (bht:get rec "ngoai_suy_m")
                   " m | nguồn: " (bht:get rec "nguon")))
  (foreach m marks
    (setq ratio (if (and prev (> (abs (- (car m) (car prev))) 1e-9)) (/ (- (nth 1 m) (nth 2 prev)) (- (nth 0 m) (nth 0 prev))) nil))
    (princ (strcat "\n  [" (itoa i) "] d=" (bht:fnum (nth 0 m) 3) "  " (bht:fmt-km (nth 1 m))
                   (if (/= (nth 1 m) (nth 2 m)) (strcat " / " (bht:fmt-km (nth 2 m)) " (GÃY)") "")
                   (if ratio (strcat "  hệ số đoạn trước " (bht:fnum ratio 5)
                                     (if (> (abs (- (abs ratio) 1.0)) *bht-ratio-tol*) " <- CẦN KIỂM TRA" "")) "")
                   "  nguồn: " (nth 3 m)))
    (setq prev m i (1+ i)))
  (setq controls (bht:route-controls rec))
  (foreach m (car controls)
    (if (bht:starts (nth 3 m) "Hồ sơ ")
      (princ (strcat "\n  [Tự động] d=" (bht:fnum (car m) 3) "  " (bht:fmt-km (cadr m))
        " | " (nth 3 m) " (sửa/xóa tại hồ sơ cọc)"))))
  (foreach note (cadr controls) (princ (strcat "\n  MÂU THUẪN: " note)))
  (if (caddr controls) (princ (strcat "\n  Chưa gán được tuyến duy nhất cho: " (bht:join (caddr controls) ", "))))
  (if (null (car controls)) (princ "\n  Chưa có mốc: nhập số Km/H vào hồ sơ cọc hoặc khai báo mốc bổ sung."))
)

(defun bht:route-mark-rows (rec / rows i raw m controls saved)
  (setq rows nil i 0)
  (foreach raw (bht:get-all rec "moc")
    (setq rows (cons (strcat (itoa i) "|" raw) rows) i (1+ i)))
  (setq rows (reverse rows) i -1 controls (bht:route-controls rec) saved (bht:route-marks rec))
  (foreach m (car controls)
    (if (and (not (member m saved)) (bht:starts (nth 3 m) "Hồ sơ "))
      (setq rows (append rows (list (bht:join (list (itoa i) (bht:fnum (car m) 6) (bht:fnum (cadr m) 6)
        (bht:fnum (caddr m) 6) (nth 3 m) "Sửa/xóa tại hồ sơ cọc") "|"))) i (1- i))))
  rows)

;; Delete by the original stored row identity, not the sorted display position.
(defun bht:route-delete-marks (id snapshot indices / rec raw seen invalid i keep result objects pair)
  (setq rec (bht:route-read id) raw (bht:get-all rec "moc") seen nil)
  (foreach i indices
    (if (or (/= (type i) 'INT) (< i 0) (>= i (length raw)) (member i seen)) (setq invalid T))
    (setq seen (cons i seen)))
  (cond ((or (null rec) (not (equal rec snapshot))) (list 'LOI "Dữ liệu tim đã thay đổi; mở lại bảng mốc trước khi xóa."))
    ((or invalid (null indices)) (list 'LOI "Chọn mốc đã khai báo của tim; mốc tự động quản lý tại hồ sơ cọc."))
    (T
     (setq i 0 keep nil objects (bht:rec-all "OBJ"))
     (foreach pair raw (if (not (member i indices)) (setq keep (cons pair keep))) (setq i (1+ i)))
     (setq result (vl-catch-all-apply '(lambda ()
       (bht:route-write id (bht:route-bump (bht:set-all rec "moc" (reverse keep)) "Xóa mốc đã chọn trong bảng"))
       (bht:route-invalidate-objects id nil)
       (list (itoa (length indices)))) nil))
     (if (vl-catch-all-error-p result)
       (progn (bht:route-write id rec) (foreach pair objects (bht:obj-write (car pair) (cdr pair)))
         (list 'LOI (strcat "Không xóa được mốc; đã khôi phục dữ liệu. " (vl-catch-all-error-message result)))) result))))

(defun bht:route-mark-raw (rec dist / ent len start dir raw)
  (setq ent (bht:route-ent rec) len (if ent (bht:curve-length ent)) start (bht:num (bht:get rec "start_dist"))
        dir (if (= (bht:int (bht:get rec "direction")) -1) -1 1))
  (if (and len (> len 0.0) (numberp dist) (>= dist 0.0) (<= dist (+ len 0.001)))
    (progn (setq raw (+ (if start start 0.0) (* dir dist)))
      (if (bht:curve-closed-p ent) (progn (while (< raw 0.0) (setq raw (+ raw len))) (while (> raw len) (setq raw (- raw len)))))
      (if (and (>= raw -0.001) (<= raw (+ len 0.001))) (max 0.0 (min len raw))))))

(defun bht:route-mark-preview (pt / size center out delta)
  (setq size (max 0.1 (* (getvar "VIEWSIZE") 0.025)) center (list (car pt) (cadr pt) 0.0))
  (foreach delta (list (list size 0.0 0.0) (list 0.0 size 0.0))
    (setq out (cons (entmakex (list '(0 . "LINE") '(8 . "0") '(62 . 2)
      (cons 10 (mapcar '- center delta)) (cons 11 (mapcar '+ center delta)))) out)))
  (setq out (cons (entmakex (list '(0 . "CIRCLE") '(8 . "0") '(62 . 2) (cons 10 center) (cons 40 (* size 0.5)))) out))
  (vl-remove nil out))

(defun c:BHTDSMOC (/ *error* id snapshot reply rows done indices started preview raw pt)
  (defun *error* (msg) (bht:route-preview-clear preview) (if started (command-s "_.UNDO" "_End")) (bht:on-error msg))
  (if (setq id (bht:ask-route))
    (progn
      (if (not (bht:fn-defined-p 'BHTROUTEMARKS)) (bht:palette-load))
      (while (not done)
        (setq snapshot (bht:route-read id) rows (bht:route-mark-rows snapshot))
        (setq reply (if (bht:fn-defined-p 'BHTROUTEMARKS) (vl-catch-all-apply 'BHTROUTEMARKS (cons id rows)) '("SKIP")))
        (cond
          ((vl-catch-all-error-p reply) (bht:warn (vl-catch-all-error-message reply)) (setq done T))
          ((= (car reply) "LOI") (bht:warn (cadr reply)) (setq done T))
          ((= (car reply) "DELETE")
           (setq indices (mapcar 'bht:int (cdr reply)))
           (if (= 0 (logand 8 (getvar "UNDOCTL"))) (progn (command-s "_.UNDO" "_Begin") (setq started T)))
           (setq reply (bht:route-delete-marks id snapshot indices))
           (if started (command-s "_.UNDO" "_End")) (setq started nil)
           (if (eq (car reply) 'LOI) (bht:warn (cadr reply)) (bht:msg (strcat "Đã xóa " (car reply) " mốc. Tính lại lý trình; Undo khôi phục lần xóa này."))))
          ((= (car reply) "VIEW")
           (setq raw (bht:route-mark-raw snapshot (bht:num (cadr reply))) pt nil)
           (if raw (setq pt (vl-catch-all-apply 'vlax-curve-getPointAtDist (list (bht:route-ent snapshot) raw))))
           (if (and pt (not (vl-catch-all-error-p pt)))
             (progn
               (vl-cmdf "_.ZOOM" "_C" (trans pt 0 1) 40.0)
               (setq preview (bht:route-mark-preview pt))
               (bht:ask-string "Dấu cộng vàng tại vị trí mốc trên tim. Enter để trở lại bảng" "")
               (bht:route-preview-clear preview) (setq preview nil))
             (bht:warn "Không xác định được vị trí mốc trên tim hiện tại.")))
          ((= (car reply) "SKIP") (bht:print-marks id) (setq done T))
          (T (setq done T))))))
  (bht:log-flush) (princ))

(defun c:BHTLYTRINH (/ *error* r)
  (setq *error* bht:on-error)
  (setq r (bht:station-objects))
  (if (eq (car r) 'LOI) (bht:err (cadr r))
    (bht:msg (strcat "BHT lý trình: " (itoa (car r)) " đối tượng đã có lý trình, "
                   (itoa (cadr r)) " chưa xác định (xem cột trang_thai_km khi xuất).")))
  (princ)
)

;; Recalculate explicitly against one alignment. Build the full plan before
;; writing; errors restore the records, method, and active alignment together.
(defun bht:route-recalculate-plan (id / route ent start dir sig controls marks index plan item rec pos result st ok missing markers key *bht-control-cache*)
  (setq route (bht:route-read id) ent (if route (bht:route-ent route) nil))
  (cond
    ((null route) (list 'LOI "Chọn một tim tuyến đã nạp trước khi tính lại."))
    ((or (null ent) (not (car (bht:route-check-ent ent)))) (list 'LOI "Tim tuyến không còn hình học hợp lệ. Nạp lại tim trước khi tính."))
    ((progn
       (setq start (bht:num (bht:get route "start_dist")) dir (if (= (bht:int (bht:get route "direction")) -1) -1 1)
             sig (bht:get route "geometry_signature"))
       (and (/= sig "") (/= sig (bht:route-geometry-signature ent (if start start 0.0) dir))))
      (list 'LOI "Hình học tim đã đổi; cập nhật tim và xác nhận lại các mốc trước khi tính."))
    (T
      (setq controls (bht:route-controls route) marks (car controls))
      (cond
        ((cadr controls) (list 'LOI (strcat "Mốc tuyến mâu thuẫn: " (bht:join (cadr controls) "; "))))
        ((< (length marks) 2) (list 'LOI "Cần ít nhất hai mốc lý trình trên tim mới. Đọc cọc TDT hoặc thêm mốc đã biết; không lấy lại mốc hồ sơ cũ."))
        ((not (bht:controls-valid-p marks)) (list 'LOI "Mốc của tim chưa hợp lệ; kiểm tra số liệu và các mốc trùng vị trí trước khi tính."))
        (T
          (setq index (bht:pt-all) plan nil ok 0 missing 0 markers 0 *bht-control-cache* (list (cons id controls)))
          (foreach item (vl-remove-if '(lambda (pair) (or (bht:progress-locked (cdr pair)) (and (/= (bht:seg-route (cdr pair)) "") (/= (bht:seg-route (cdr pair)) id)))) (bht:rec-all "OBJ"))
            (setq rec (cdr item))
            (if (bht:obj-marker-metres rec)
              (setq markers (1+ markers))
              (progn
                (setq pos (bht:obj-position rec index)
                      result (if pos (bht:station-object-route route rec index pos) (list (cons 'status "KHONG_CO_VI_TRI") (cons 'note "Hồ sơ chưa có điểm RTK.")))
                      st (cdr (assoc 'status result)))
                (foreach key '("ly_trinh_m" "ly_trinh_km" "offset_m" "phia_tuyen" "loai_tuyen" "station_route_revision" "field_chain_id" "field_pair_a" "field_pair_b" "field_last_error" "doan" "goi" "doan_ung_vien")
                  (setq rec (bht:set rec key "")))
                (setq rec (bht:set rec "route_id" id)
                      rec (bht:set rec "station_source" "ROUTE")
                      rec (bht:set rec "trang_thai_km" st)
                      rec (bht:set rec "station_status" (bht:station-status-v5 st))
                      rec (bht:set rec "gan_doan_pp" "CHUA_PHAN_DOAN")
                      rec (bht:set rec "nguon_km" (bht:str (cdr (assoc 'note result))))
                      rec (bht:set rec "offset_m" (if (cdr (assoc 'offset result)) (bht:fnum (cdr (assoc 'offset result)) 3) ""))
                      rec (bht:set rec "phia_tuyen" (bht:str (cdr (assoc 'side result))))
                      rec (bht:set rec "loai_tuyen" (bht:get route "loai"))
                      rec (bht:set rec "station_route_revision" (itoa (bht:route-revision route))))
                (if (bht:station-determined st)
                  (setq rec (bht:set rec "ly_trinh_m" (bht:fnum (cdr (assoc 'station result)) 3))
                        rec (bht:set rec "ly_trinh_km" (bht:fmt-km (cdr (assoc 'station result)))) ok (1+ ok))
                  (setq missing (1+ missing)))
                (if (or (= (bht:seg-route (cdr item)) id) (= (bht:get (cdr item) "gan_doan_pp") "THU_CONG"))
                  (foreach key '("doan" "goi" "gan_doan_pp" "doan_ung_vien") (setq rec (bht:set rec key (bht:get (cdr item) key)))))
                (setq rec (bht:station-point-record rec result))
                (setq rec (bht:seg-refresh-record rec))
                (setq plan (cons (cons (car item) rec) plan)))))
          (list id (reverse plan) ok missing markers
            (strcat "Mốc trên tim: " (bht:fmt-km (cadar marks)) " đến " (bht:fmt-km (cadr (last marks)))
              "; ngoại suy tối đa " (bht:get route "ngoai_suy_m") " m."
              (if (bht:route-tdt-controls-p route) " Dùng mốc TDT và mốc bổ sung đã khai báo; bỏ mốc tự động từ hồ sơ." ""))))))))

(defun bht:route-recalculate-write (plan / pair failed)
  (foreach pair (cadr plan)
    (if (and (not failed) (not (bht:obj-write (car pair) (cdr pair)))) (setq failed "Không ghi được lý trình hồ sơ.")))
  (if (and (not failed) (not (bht:meta-set "station_method" "ROUTE"))) (setq failed "Không lưu được phương pháp tính."))
  (if (and (not failed) (not (bht:meta-set "station_active_route" (car plan)))) (setq failed "Không lưu được tim đang tính."))
  (if (and (not failed) (not (bht:meta-set "field_active_chain" ""))) (setq failed "Không cập nhật được nguồn cọc đang dùng."))
  (if (not failed) (bht:kh-station-label-refresh (mapcar 'car (cadr plan))))
  (if failed (list 'LOI failed) (list (strcat "Tim đang tính: " (car plan))
        (strcat "Đã tính: " (itoa (nth 2 plan)) "; chưa xác định: " (itoa (nth 3 plan)) "; cọc giữ số Km/H: " (itoa (nth 4 plan)) ".")
        (nth 5 plan)
        "Hồ sơ chưa xác định được xóa lý trình cũ và ghi trạng thái để kiểm tra. Bổ sung mốc bao quanh vị trí trước khi tính tiếp.")))

(defun bht:route-recalculate (id / plan before meta result pair)
  (setq plan (bht:route-recalculate-plan id))
  (if (eq (car plan) 'LOI) plan
    (progn
      (setq before (bht:rec-all "OBJ") meta (bht:rec-read "META" "CONFIG")
            result (bht:kh-display-transaction 'bht:route-recalculate-write (list plan)))
      (if (or (vl-catch-all-error-p result) (eq (car result) 'LOI))
        (progn
          (foreach pair before (bht:rec-write "OBJ" (car pair) (cdr pair)))
          (if meta (bht:rec-write "META" "CONFIG" meta) (bht:rec-delete "META" "CONFIG"))
          (list 'LOI (strcat "Đã hoàn tác dữ liệu vì tính lại lỗi: " (if (vl-catch-all-error-p result) (vl-catch-all-error-message result) (cadr result)))))
        result))))

(defun c:BHTTINHTHEOTUYEN (/ *error* id plan answer own result line)
  (defun *error* (msg) (if own (command-s "_.UNDO" "_End")) (bht:on-error msg))
  (if (setq id (bht:ask-route))
    (progn
      (setq plan (bht:route-recalculate-plan id))
      (if (eq (car plan) 'LOI) (bht:warn (cadr plan))
        (progn
          (bht:msg (strcat "Tính lại hồ sơ theo tim " id ": " (itoa (nth 2 plan)) " tính được, " (itoa (nth 3 plan)) " chưa xác định. " (nth 5 plan)))
          (setq answer (strcase (bht:ask-string "Áp dụng? Hồ sơ chưa xác định sẽ xóa lý trình cũ [C/K]" "K")))
          (if (= answer "C")
            (progn
              (setq own (= 0 (logand 8 (getvar "UNDOCTL"))))
              (if own (command-s "_.UNDO" "_Begin"))
              (setq result (bht:route-recalculate id))
              (if own (command-s "_.UNDO" "_End")) (setq own nil)
              (if (eq (car result) 'LOI) (bht:warn (cadr result)) (foreach line result (bht:msg line)))))))))
  (princ))


;; Route maintenance preserves original survey entities and invalidates derived station data.
(defun bht:route-invalidate-objects (id detached / rec index angle zero oid key)
  (setq index (if detached (bht:pt-all) nil))
  (foreach oid (bht:obj-ids)
    (setq rec (bht:obj-read oid))
    (if (or (= (bht:get rec "route_id") id) (= (bht:get rec "sign_heading_route") id))
      (progn
        (if (and detached (= (bht:get rec "sign_heading_route") id))
          (progn
            (setq angle (bht:kh-orientation-angle rec (bht:obj-position rec index))
                  zero (bht:kh-picked-rotation rec '(0.0 0.0 0.0) '(1.0 0.0 0.0)))
            (setq rec (bht:set rec "sign_heading_mode" (if angle "FIXED" ""))
                  rec (bht:set rec "sign_heading_angle" (if angle (bht:fnum (- angle zero) 8) ""))
                  rec (bht:set rec "sign_heading_route" ""))))
        (if (= (bht:get rec "route_id") id)
          (progn
            (foreach key '("ly_trinh_m" "ly_trinh_km" "offset_m" "phia_tuyen" "loai_tuyen" "station_route_revision")
              (setq rec (bht:set rec key "")))
            (setq rec (bht:station-point-record rec nil))
            (setq rec (bht:set rec "route_id" (if detached "" id))
                  rec (bht:set rec "station_status" (if detached "NO_ROUTE" "STALE"))
                  rec (bht:set rec "trang_thai_km" (if detached "CHUA_CO_TUYEN" "CAN_CAP_NHAT"))
                  rec (bht:set rec "nguon_km" "Tuyến đã đổi; chạy Cập nhật lý trình."))))
        (bht:obj-write oid rec)))))

(defun bht:route-edit (id newid loai maxoff extrap / rec o segment oid)
  (setq newid (strcase newid) loai (bht:route-type loai) rec (bht:route-read id))
  (cond
    ((null rec) (list nil "Không có tuyến."))
    ((not (bht:valid-id newid)) (list nil "ID chỉ gồm A-Z, 0-9, dấu gạch, chấm và gạch dưới."))
    ((and (/= id newid) (bht:route-read newid)) (list nil "ID tuyến đã tồn tại."))
    ((null loai) (list nil "Chọn loại tim: Tim đường, Tim rãnh hoặc Tuyến khác."))
    ((or (not (numberp maxoff)) (<= maxoff 0)) (list nil "Phạm vi lệch tim tối đa phải lớn hơn 0 m."))
    ((or (not (numberp extrap)) (< extrap 0)) (list nil "Ngoại suy tối đa phải từ 0 m trở lên."))
    ((and (= id newid) (= loai (bht:get rec "loai")) (= maxoff (bht:num (bht:get rec "max_offset"))) (= extrap (bht:num (bht:get rec "ngoai_suy_m")))) (list T id))
    (T
      (setq rec (bht:route-bump (bht:set (bht:set (bht:set (bht:set rec "route_id" newid) "loai" loai) "max_offset" (bht:fnum maxoff 3)) "ngoai_suy_m" (bht:fnum extrap 3)) "Sửa thuộc tính tuyến"))
      (bht:progress-guard "ROUTE" id rec)
      (bht:route-write newid rec)
      (bht:route-invalidate-objects id nil)
      (if (/= id newid)
        (progn
          (foreach oid (bht:obj-ids)
            (setq o (bht:obj-read oid))
            (if (or (= (bht:get o "route_id") id) (= (bht:get o "sign_heading_route") id)) (progn (if (= (bht:get o "route_id") id) (setq o (bht:set o "route_id" newid)))
            (if (= (bht:get o "sign_heading_route") id) (setq o (bht:set o "sign_heading_route" newid)))
            (bht:obj-write oid o))))
          (foreach segment (bht:rec-all "SEG")
            (if (= (bht:get (cdr segment) "route_id") id)
              (bht:rec-write "SEG" (car segment) (bht:set (cdr segment) "route_id" newid))))
          (bht:rec-delete "ROUTE" id)))
      (if (= (bht:meta "station_active_route" "") id) (bht:meta-set "station_active_route" newid))
      (list T newid))))

(defun bht:route-replace (id ent / rec chk len)
  (bht:progress-guard "ROUTE" id nil)
  (setq rec (bht:route-read id) chk (bht:route-check-ent ent) len (if (car chk) (bht:curve-length ent) nil))
  (cond ((null rec) (list nil "Không có tuyến.")) ((not (car chk)) chk)
    ((or (null len) (<= len 1e-8)) (list nil "Tuyến phải có chiều dài lớn hơn 0."))
    ((= (bht:get rec "handle") (cdr (assoc 5 (entget ent)))) (list nil "Đây là tuyến hiện tại; dùng Chọn điểm đầu hoặc cập nhật hình học."))
    (T
      (bht:route-invalidate-objects id nil)
      (setq rec (bht:set-all rec "moc" nil))
      (foreach key '("tdt_source_handle" "tdt_source_class" "tdt_refreshed_at" "start_x" "start_y" "chieu") (setq rec (bht:set rec key "")))
      (setq rec (bht:set rec "handle" (cdr (assoc 5 (entget ent))))
            rec (bht:set rec "start_dist" "0.000000") rec (bht:set rec "direction" "1")
            rec (bht:set rec "geometry_length" (bht:fnum len 6))
            rec (bht:set rec "geometry_signature" (bht:route-geometry-signature ent 0.0 1))
            rec (bht:set rec "station_source" "CHUA_CO_MOC") rec (bht:set rec "station_control_status" "CHUA_CO_MOC")
            rec (bht:set rec "nguon" "Polyline chọn lại") rec (bht:route-bump rec "Thay hình học tuyến"))
      (bht:route-write id rec) (list T id))))

(defun bht:route-delete (id / rec)
  (bht:progress-guard "ROUTE" id nil)
  (if (setq rec (bht:route-read id))
    (progn (bht:route-invalidate-objects id T) (bht:rec-delete "ROUTE" id) (list T id))
    (list nil "Không có tuyến.")))

(defun c:BHTSUATUYEN (/ *error* id rec options newid loai maxoff extrap result started)
  (defun *error* (msg) (if started (command-s "_.UNDO" "_End")) (bht:on-error msg))
  (if (setq id (bht:ask-route))
    (progn
      (setq rec (bht:route-read id) options (bht:route-options "Sửa thông tin tim tuyến" id (bht:get rec "loai") (bht:get rec "max_offset") (bht:get rec "ngoai_suy_m")))
      (if options (progn
      (setq newid (nth 0 options) loai (nth 1 options) maxoff (nth 2 options) extrap (nth 3 options))
      (if (= 0 (logand 8 (getvar "UNDOCTL"))) (progn (command-s "_.UNDO" "_Begin") (setq started T)))
      (setq result (bht:route-edit id newid loai maxoff extrap))
      (if started (command-s "_.UNDO" "_End")) (setq started nil)
      (if (car result) (bht:msg "Đã lưu thông tin tuyến. Nếu đổi thông số, tính lại lý trình hồ sơ liên quan.") (bht:warn (cadr result)))))))
  (bht:log-flush) (princ))

(defun c:BHTCHONLAITUYEN (/ *error* id sel result started)
  (defun *error* (msg) (if started (command-s "_.UNDO" "_End")) (bht:on-error msg))
  (if (and (setq id (bht:ask-route)) (setq sel (entsel "\nChọn Polyline thay thế (giữ hình học cũ): ")))
    (progn
      (bht:msg "Thay hình học sẽ bỏ các mốc Km cũ của tuyến; cần chọn lại điểm đầu và nạp lại mốc.")
      (if (= (strcase (bht:ask-string "Xác nhận thay tuyến [C=Co/K=Khong]" "K")) "C")
        (progn
          (if (= 0 (logand 8 (getvar "UNDOCTL"))) (progn (command-s "_.UNDO" "_Begin") (setq started T)))
          (setq result (bht:route-replace id (car sel)))
          (if (car result) (progn (bht:route-pick-start id) (bht:msg "Đã thay hình học. Nạp lại mốc Km và cập nhật lý trình.")) (bht:warn (cadr result)))
          (if started (command-s "_.UNDO" "_End")) (setq started nil)))))
  (bht:log-flush) (princ))

(defun c:BHTXOATUYEN (/ *error* id result started)
  (defun *error* (msg) (if started (command-s "_.UNDO" "_End")) (bht:on-error msg))
  (if (setq id (bht:ask-route))
    (progn
      (bht:msg (strcat "Xóa khai báo tuyến " id ": giữ Polyline gốc và RTK; bỏ mốc Km/liên kết lý trình, giữ hướng biển hiện tại."))
      (if (= (strcase (bht:ask-string "Xác nhận xóa tuyến [C=Co/K=Khong]" "K")) "C")
        (progn
          (if (= 0 (logand 8 (getvar "UNDOCTL"))) (progn (command-s "_.UNDO" "_Begin") (setq started T)))
          (setq result (bht:route-delete id))
          (if started (command-s "_.UNDO" "_End")) (setq started nil)
          (if (car result) (bht:msg "Đã xóa khai báo tuyến; dùng Undo để khôi phục.") (bht:warn (cadr result)))))))
  (bht:log-flush) (princ))

(defun bht:station-one (oid / rec pos r st index)
  (setq rec (bht:obj-read oid))
  (cond ((null rec) (list 'LOI "Không tìm thấy hồ sơ đã lưu."))
    ((bht:progress-locked rec) (list 'LOI "Hồ sơ đã xác nhận. Mở lại đoạn trong Quản lý xử lý theo đoạn trước khi tính lại."))
    ((bht:obj-marker-metres rec)
      (bht:obj-write oid rec)
      (list (strcat "Hồ sơ: " oid) (strcat "Lý trình: " (bht:fmt-km (bht:obj-marker-metres rec)))
        "Lấy từ số Km/H đã nhập; không cần xác nhận lại bằng BHTMOCKM."))
    ((and (bht:field-mode-p) (= (bht:seg-route rec) "")) (bht:field-one oid))
    ((null (bht:route-ids)) (list 'LOI "Chưa có tuyến tham chiếu. Hai cọc Km chưa đủ mô tả đường cong; dùng Kiểm tra 2 cọc Km để ước tính. Giữ nguyên lý trình cũ."))
    (T
      (setq index (bht:pt-all) pos (bht:obj-position rec index)
            r (if pos (bht:station-for-object rec pos index) (list (cons 'status "KHONG_CO_VI_TRI") (cons 'note "Hồ sơ chưa có vị trí RTK.")))
            st (cdr (assoc 'status r)))
      (if (not (bht:station-determined st))
        (list 'LOI (strcat "Chưa tính được: " st ". " (bht:str (cdr (assoc 'note r))) ". Giữ nguyên lý trình cũ. Kiểm tra tuyến và mốc Km."))
        (progn
    (setq rec (bht:set (bht:set rec "station_source" "ROUTE") "field_last_error" "")
          rec (bht:set rec "vi_tri_e" (if pos (bht:fnum (car pos) 3) ""))
          rec (bht:set rec "vi_tri_n" (if pos (bht:fnum (cadr pos) 3) ""))
          rec (bht:set rec "route_id" (bht:str (cdr (assoc 'route r))))
          rec (bht:set rec "trang_thai_km" st)
          rec (bht:set rec "ly_trinh_m" (if (bht:station-determined st) (bht:fnum (cdr (assoc 'station r)) 3) ""))
          rec (bht:set rec "ly_trinh_km" (if (bht:station-determined st) (bht:fmt-km (cdr (assoc 'station r))) ""))
          rec (bht:set rec "offset_m" (if (cdr (assoc 'offset r)) (bht:fnum (cdr (assoc 'offset r)) 3) ""))
          rec (bht:set rec "phia_tuyen" (bht:str (cdr (assoc 'side r))))
          rec (bht:set rec "loai_tuyen" (bht:str (cdr (assoc 'loai r))))
          rec (bht:set rec "station_route_revision" (if (cdr (assoc 'revision r)) (itoa (cdr (assoc 'revision r))) ""))
          rec (bht:set rec "station_status" (bht:station-status-v5 st))
          rec (bht:set rec "nguon_km" (bht:str (cdr (assoc 'note r)))))

          (setq rec (bht:station-point-record rec r))
          (setq rec (bht:seg-refresh-record rec))
          (bht:obj-write oid rec)
          (bht:kh-station-label-refresh (list oid))
          (list (strcat "Hồ sơ: " oid) (strcat "Lý trình: " (bht:get rec "ly_trinh_km"))
            (strcat "Tuyến: " (bht:get rec "route_id") " | " st)
            (strcat "Cách tính: " (bht:get rec "nguon_km"))))))))

;; Estimate only: projection onto the XY chord between two surveyed Km marks.
(defun bht:km-two-marks (a ka b kb p / dx dy len2 u foot)
  (setq dx (- (car b) (car a)) dy (- (cadr b) (cadr a)) len2 (+ (* dx dx) (* dy dy)))
  (cond ((<= len2 1e-12) (list nil "Hai cọc trùng vị trí trên bình đồ."))
    ((equal ka kb 1e-9) (list nil "Hai cọc có cùng lý trình; cần kiểm tra lại."))
    (T (setq u (/ (+ (* (- (car p) (car a)) dx) (* (- (cadr p) (cadr a)) dy)) len2))
      (if (or (< u 0.0) (> u 1.0)) (list nil "Vị trí nằm ngoài khoảng hai cọc; không ngoại suy.")
        (progn (setq foot (list (+ (car a) (* u dx)) (+ (cadr a) (* u dy))))
          (list T (+ ka (* u (- kb ka))) (distance foot (list (car p) (cadr p))) (sqrt len2)))))))

(defun c:BHTKM2COC (/ *error* a b ka kb p r)
  (setq *error* bht:on-error)
  (bht:msg "Ước tính theo đoạn thẳng nối hai cọc trên bình đồ; không tự ghi hồ sơ, không dùng thay chiều dài đường cong.")
  (if (and (setq a (getpoint "\nChọn cọc Km thứ nhất (bắt NODE): "))
           (setq ka (bht:parse-km (bht:ask-string "Lý trình cọc 1 (vd Km45+000)" "")))
           (setq b (getpoint "\nChọn cọc Km thứ hai (bắt NODE): "))
           (setq kb (bht:parse-km (bht:ask-string "Lý trình cọc 2 (vd Km46+000)" "")))
           (setq p (getpoint "\nChọn vị trí cần kiểm tra: ")))
    (progn (setq r (bht:km-two-marks (trans a 1 0) ka (trans b 1 0) kb (trans p 1 0)))
      (if (car r)
        (progn (bht:msg (strcat "Lý trình ƯỚC TÍNH: " (bht:fmt-km (cadr r))))
          (bht:msg (strcat "Lệch đoạn nối cọc: " (bht:fnum (caddr r) 3) " m; khoảng cách hai cọc: " (bht:fnum (cadddr r) 3) " m."))
          (bht:msg "Chưa ghi vào hồ sơ. Chỉ dùng khi đã kiểm tra đoạn đường đủ thẳng và giá trị cọc đúng."))
        (bht:warn (cadr r))))
    (bht:msg "Chưa tính: đã hủy chọn điểm hoặc lý trình cọc không hợp lệ."))
  (princ))

;;; Field-marker stationing: COC_TIEU and COT_KM use their surveyed positions.
;;; No alignment, copied Station Control or symbol placement participates.
(defun bht:field-mode-p () (= (bht:meta "station_method" "ROUTE") "FIELD_MARKERS"))
(defun bht:field-fail (status note) (list (cons 'status status) (cons 'note note)))
(defun bht:field-marker (id index / rec pos station ids)
  (if *bht-field-cache* (cdr (assoc id *bht-field-cache*)) (progn
  (setq rec (bht:obj-read id))
  (if (and rec (member (bht:get rec "nhom") '("COC_TIEU" "COT_KM"))
           (or (not (bht:obj-manual-position-p rec index)) (bht:field-manual-approved rec index))
           (setq station (bht:obj-marker-metres rec)) (setq ids (bht:get-all rec "pt"))
           (not (vl-some '(lambda (pid) (null (bht:pt-find pid index))) ids))
           (setq pos (bht:obj-position rec index)))
    (list id station pos (if (bht:obj-manual-position-p rec index) "MANUAL_CAD" "RTK") (bht:get rec "nhom"))))))

(defun bht:field-manual-approved (rec index / pos ids x y)
  (and (= (bht:get rec "field_manual_approved") "1")
       (= (bht:get rec "nhom") (bht:get rec "field_manual_group"))
       (= (bht:get rec "marker_km") (bht:get rec "field_manual_km"))
       (= (bht:get rec "marker_h") (bht:get rec "field_manual_h"))
       (setq ids (bht:get-all rec "pt"))
       (= (bht:join ids ";") (bht:get rec "field_manual_points"))
       (not (vl-some '(lambda (id) (null (bht:pt-find id index))) ids))
       (setq pos (bht:obj-position rec index) x (bht:num (bht:get rec "field_manual_x")) y (bht:num (bht:get rec "field_manual_y")))
       x y (equal (car pos) x 1e-7) (equal (cadr pos) y 1e-7)))

(defun bht:field-manual-control (oid value / rec index pos ids item other)
  (setq rec (bht:obj-read oid) index (bht:pt-all) ids (bht:get-all rec "pt") pos (if rec (bht:obj-position rec index)))
  (cond
    ((not (member value '("0" "1"))) (list 'LOI "Lựa chọn mốc không hợp lệ."))
    ((or (null rec) (not (member (bht:get rec "nhom") '("COC_TIEU" "COT_KM"))) (not (bht:obj-manual-position-p rec index))) (list 'LOI "Chỉ xác nhận riêng cọc tiêu hoặc cột Km bổ sung thủ công."))
    ((and (= value "1") (or (null pos) (null (bht:obj-marker-metres rec)) (null ids)
       (vl-some '(lambda (id) (null (bht:pt-find id index))) ids))) (list 'LOI "Cọc cần đủ Km/H và điểm vị trí trước khi xác nhận."))
    (T
      (setq rec (bht:set (bht:set rec "field_manual_approved" value) "field_manual_time" (bht:now)))
      (if (= value "1")
        (foreach pair (list (cons "field_manual_group" (bht:get rec "nhom")) (cons "field_manual_km" (bht:get rec "marker_km")) (cons "field_manual_h" (bht:get rec "marker_h"))
          (cons "field_manual_points" (bht:join ids ";")) (cons "field_manual_x" (bht:fnum (car pos) 8)) (cons "field_manual_y" (bht:fnum (cadr pos) 8)))
          (setq rec (bht:set rec (car pair) (cdr pair)))))
      (bht:obj-write oid rec)
      (foreach item (bht:rec-all "OBJ")
        (setq other (cdr item))
        (if (or (= (bht:get other "field_used_a") oid) (= (bht:get other "field_used_b") oid))
          (bht:obj-write (car item) (bht:set (bht:set other "station_status" "NEEDS_REVIEW") "field_last_error" "Xác nhận mốc thủ công đã đổi; cần tính lại."))))
      (list (strcat oid (if (= value "1") ": đã xác nhận làm mốc nội suy theo cọc; nguồn vẫn là thủ công." ": đã bỏ xác nhận làm mốc."))))))

;; Return projection even for bad station values: a bad nearby interval must not
;; disappear and allow a distant interval to silently take its place.
(defun bht:field-interval (aid bid chain index pt / a b pa pb dx dy len2 u foot off span ratio maxoff maxspan tol note)
  (setq a (bht:field-marker aid index) b (bht:field-marker bid index))
  (cond
    ((or (null a) (null b)) (bht:field-fail "COC_KHONG_HOP_LE" (strcat "Cọc " aid " / " bid " đã mất hoặc thiếu Km/H, RTK.")))
    (T
      (setq pa (caddr a) pb (caddr b) dx (- (car pb) (car pa)) dy (- (cadr pb) (cadr pa)) len2 (+ (* dx dx) (* dy dy)))
      (if (<= len2 1e-12) (bht:field-fail "COC_TRUNG_VI_TRI" (strcat "Hai cọc trùng vị trí: " aid " / " bid))
        (progn
          (setq u (/ (+ (* (- (car pt) (car pa)) dx) (* (- (cadr pt) (cadr pa)) dy)) len2)
                foot (list (+ (car pa) (* u dx)) (+ (cadr pa) (* u dy))))
          (if (or (< u -1e-9) (> u (+ 1.0 1e-9)))
            (bht:field-fail "NGOAI_DOAN_COC" "Điểm nằm ngoài khoảng hai cọc; không ngoại suy.")
            (progn
              (setq u (max 0.0 (min 1.0 u)) off (distance foot (list (car pt) (cadr pt))) span (sqrt len2)
                    maxoff (bht:num (bht:get chain "max_offset")) maxspan (bht:num (bht:get chain "max_span")) tol (bht:num (bht:get chain "ratio_tol")))
              (if (null maxoff) (setq maxoff 100.0)) (if (null maxspan) (setq maxspan 500.0)) (if (null tol) (setq tol 0.03))
              (cond
                ((> off maxoff) (bht:field-fail "XA_CHUOI_COC" "Điểm ở ngoài phạm vi đường nối cọc."))
                ((member (strcat aid "|" bid) (bht:get-all chain "blocked_pair"))
                  (append (bht:field-fail "COC_CAN_DOI_CHIEU" "Đoạn có cọc trùng lý trình hoặc thứ tự vị trí đổi hướng bất thường. Chọn 2 cọc đã đối chiếu hoặc dùng nhóm đường/nhánh trong Nâng cao.") (list (cons 'offset off))))
                ((<= (- (cadr b) (cadr a)) 0.001)
                  (append (bht:field-fail "SAI_THU_TU_COC" (strcat aid " → " bid ": số Km/H không tăng theo thứ tự chuỗi.")) (list (cons 'offset off))))
                (T
                  (setq ratio (/ (- (cadr b) (cadr a)) span)
                        note (strcat "Mốc " aid " (" (bht:fmt-km (cadr a)) ") → " bid " (" (bht:fmt-km (cadr b)) "); tỷ phần " (bht:fnum u 6)
                          "; dài đoạn nối " (bht:fnum span 2) " m; hệ số " (bht:fnum ratio 5) "; lệch đường nối cọc " (bht:fnum off 3) " m. Nội suy thực địa, không phải lệch tim."))
                  (list (cons 'status (if (or (> span maxspan) (> (abs (- ratio 1.0)) tol)) "CAN_KIEM_TRA" "NOI_SUY"))
                    (cons 'manual_control (if (or (= (nth 3 a) "MANUAL_CAD") (= (nth 3 b) "MANUAL_CAD")) "1" "0"))
                    (cons 'station (+ (cadr a) (* u (- (cadr b) (cadr a))))) (cons 'offset off) (cons 'fraction u)
                    (cons 'marker_a aid) (cons 'marker_b bid) (cons 'bearing (atan dy dx)) (cons 'span span) (cons 'ratio ratio)
                    (cons 'note (strcat note (if (or (= (nth 3 a) "MANUAL_CAD") (= (nth 3 b) "MANUAL_CAD")) " Có mốc bổ sung thủ công đã xác nhận; kết quả tham khảo." "") (if (> span maxspan) " Đoạn dài/thiếu cọc; kiểm tra đường cong." "")
                      (if (> (abs (- ratio 1.0)) tol) " Khoảng cách không phù hợp chênh lệch Km/H; kiểm tra số cọc và RTK." "")))))))))))))

(defun bht:field-chain-default () '(("max_offset" . "100") ("max_span" . "500") ("ratio_tol" . "0.03")))
(defun bht:field-result (rec pt index / cid chain ids a b r candidates missing count)
  (setq cid (bht:get rec "field_chain_id"))
  (if (= cid "") (setq cid (bht:meta "field_active_chain" "")))
  (setq chain (if (/= cid "") (if *bht-field-chains* (cdr (assoc cid *bht-field-chains*)) (bht:rec-read "FIELD_CHAIN" cid))) a (bht:get rec "field_pair_a") b (bht:get rec "field_pair_b"))
  (cond
    ((null pt) (bht:field-fail "KHONG_CO_VI_TRI" "Hồ sơ chưa có RTK."))
    ((and (/= a "") (/= b "")) (bht:field-interval a b (bht:field-chain-default) index pt))
    ((null chain) (bht:field-fail "CHUA_CO_CHUOI_COC" "Bấm Mốc lý trình đã nhập trong tab Tuyến hoặc Chọn 2 cọc tại hồ sơ."))
    ((and (= (bht:get chain "kind") "IMPORTED_MARKERS")
          (/= (bht:get chain "input_snapshot") (if *bht-field-input-snapshot* *bht-field-input-snapshot* (bht:field-input-snapshot (bht:field-input-rows index)))))
      (bht:field-fail "COC_DA_THAY_DOI" "Danh sách cọc, số Km/H hoặc vị trí đã thay đổi. Bấm Mốc lý trình đã nhập để xem lại trước khi tính. Giữ nguyên lý trình cũ."))
    (T
      (setq ids (bht:get-all chain "marker") candidates nil missing 0)
      (while (cadr ids)
        (setq r (bht:field-interval (car ids) (cadr ids) chain index pt))
        (if (member (cdr (assoc 'status r)) '("COC_KHONG_HOP_LE" "COC_TRUNG_VI_TRI")) (setq missing (1+ missing)))
        (if (assoc 'offset r) (setq candidates (cons r candidates)))
        (setq ids (cdr ids)))
      (setq count (length candidates))
      ;; Multiple in-range segments are ambiguous, regardless of their distances.
      ;; A missing vertex never causes the chain to join its surviving neighbours.
      (cond
        ((> count 1) (bht:field-fail "NHIEU_DOAN_COC" "Điểm thuộc phạm vi nhiều đoạn cọc. Bấm Chọn 2 cọc để xác nhận đoạn đúng."))
        ((= count 1) (append (car candidates) (list (cons 'chain cid))))
        (T (bht:field-fail "CHUA_XAC_DINH" (strcat "Không có đoạn cọc bao quanh vị trí này. Không ngoại suy."
           (if (> missing 0) " Chuỗi có cọc thiếu dữ liệu/trùng vị trí; chọn cặp cọc đã kiểm tra." ""))))))))

(defun bht:field-store-result (rec pos r / st)
  (setq st (cdr (assoc 'status r)) rec (bht:station-point-record rec nil))
  (foreach pair (list
    (cons "vi_tri_e" (bht:fnum (car pos) 3)) (cons "vi_tri_n" (bht:fnum (cadr pos) 3))
    (cons "ly_trinh_m" (bht:fnum (cdr (assoc 'station r)) 3)) (cons "ly_trinh_km" (bht:fmt-km (cdr (assoc 'station r))))
    (cons "trang_thai_km" st) (cons "station_status" (if (or (= st "CAN_KIEM_TRA") (= (cdr (assoc 'manual_control r)) "1")) "NEEDS_REVIEW" "VALID"))
    (cons "field_manual_control" (if (= (cdr (assoc 'manual_control r)) "1") "1" "0"))
    '("field_last_error" . "") '("station_source" . "FIELD_MARKERS") '("route_id" . "") '("station_route_revision" . "") '("offset_m" . "") '("phia_tuyen" . "") '("loai_tuyen" . "")
    (cons "field_offset_m" (bht:fnum (cdr (assoc 'offset r)) 3)) (cons "field_used_a" (cdr (assoc 'marker_a r))) (cons "field_used_b" (cdr (assoc 'marker_b r)))
    (cons "field_fraction" (bht:fnum (cdr (assoc 'fraction r)) 8)) (cons "nguon_km" (cdr (assoc 'note r)))
    '("doan" . "") '("goi" . "") '("gan_doan_pp" . "CHUA_PHAN_DOAN") '("doan_ung_vien" . ""))
    (setq rec (bht:set rec (car pair) (cdr pair))))
  (if (assoc 'chain r) (setq rec (bht:set rec "field_chain_id" (cdr (assoc 'chain r)))))
  rec)

(defun bht:field-one (oid / rec index pos r st)
  (setq rec (bht:obj-read oid) index (bht:pt-all) pos (if rec (bht:obj-position rec index)))
  (cond ((null rec) (list 'LOI "Hồ sơ không tồn tại."))
    ((bht:obj-marker-metres rec) (list (strcat "Lý trình cọc giữ theo số đã nhập: " (bht:fmt-km (bht:obj-marker-metres rec)))))
    (T (setq r (bht:field-result rec pos index) st (cdr (assoc 'status r)))
      (if (not (bht:field-accepted rec r))
        (list 'LOI (strcat (cdr (assoc 'note r)) " Giữ nguyên lý trình cũ. Chọn 2 cọc để xác nhận đoạn cần kiểm tra."))
        (progn
          (setq rec (bht:field-store-result rec pos r)) (bht:obj-write oid rec)
          (bht:kh-station-label-refresh (list oid))
          (list (strcat "Hồ sơ " oid ": " (bht:get rec "ly_trinh_km"))
            (if (or (= st "CAN_KIEM_TRA") (= (cdr (assoc 'manual_control r)) "1")) "CẦN KIỂM TRA — kết quả tham khảo theo cọc." "Nội suy theo mốc thực địa (cột Km / cọc tiêu).") (cdr (assoc 'note r))))))))

(defun bht:field-objects (/ index oid rec pos r ok und records item marker failed *bht-field-cache* *bht-field-chains* *bht-field-input-snapshot* *bht-fast-point-source* *bht-fast-point-tree* *bht-subdict-cache*)
  (setq index (bht:pt-all) ok 0 und 0)
  (setq *bht-subdict-cache* (mapcar '(lambda (name) (cons name (bht:subdict name))) '("OBJ" "META" "FIELD_CHAIN")))
  (setq *bht-fast-point-source* index *bht-fast-point-tree* (bht:pt-lookup-tree index))
  (setq records (bht:rec-all "OBJ") *bht-field-chains* (bht:rec-all "FIELD_CHAIN"))
  ;; Only markers belong in the interval lookup. Previously every sign/photo
  ;; record occupied a nil entry, making each endpoint lookup scan all objects.
  (foreach item records
    (if (and (member (bht:get (cdr item) "nhom") '("COC_TIEU" "COT_KM"))
             (setq marker (bht:field-marker (car item) index)))
      (setq r (cons (cons (car item) marker) r))))
  (setq *bht-field-cache* (cons '(nil) r))
  (setq *bht-field-input-snapshot* (bht:field-input-snapshot (bht:field-input-rows index)))
  (foreach item records
    (setq oid (car item) rec (cdr item) pos (bht:obj-position rec index))
    (cond ((bht:progress-locked rec) (setq ok (1+ ok)))
      ((bht:obj-marker-metres rec) (setq ok (1+ ok)))
      ((/= (bht:seg-route rec) "")
        (setq r (if pos (bht:station-for-object rec pos index) '((status . "KHONG_CO_VI_TRI"))))
        (if (and (not failed) (not (bht:obj-write oid (bht:seg-station-record rec r)))) (setq failed "Không ghi được lý trình theo tim của đoạn."))
        (if (bht:station-determined (cdr (assoc 'status r))) (setq ok (1+ ok)) (setq und (1+ und))))
      (T
        (setq r (bht:field-result rec pos index))
        (if (bht:field-accepted rec r)
          (progn (if (and (not failed) (not (bht:obj-write oid (bht:field-store-result rec pos r)))) (setq failed "Không ghi được lý trình theo cọc.")) (setq ok (1+ ok)))
          (progn
            ;; Preserve old value and its provenance, but flag it stale explicitly.
            (setq rec (bht:set (bht:set rec "station_status" "NEEDS_REVIEW") "field_last_error" (cdr (assoc 'note r))))
            (if (and (not failed) (not (bht:obj-write oid rec))) (setq failed "Không ghi được trạng thái lý trình.")) (setq und (1+ und)))))))
  (if failed (list 'LOI failed) (progn (bht:kh-station-label-refresh nil) (list ok und))))

(defun bht:field-save (id name markertext maxoff maxspan tol / ids index rec revision)
  (setq id (strcase (bht:trim id)) ids (mapcar 'strcase (bht:api-ids markertext)) index (bht:pt-all)
        maxoff (bht:num maxoff) maxspan (bht:num maxspan) tol (bht:num tol))
  (cond
    ((= id "BHT_IMPORTED_MARKERS") (list 'LOI "Danh sách cọc đã nhập được quản lý bằng nút Mốc lý trình đã nhập."))
    ((or (= id "") (wcmatch id "*[,;|= ]*")) (list 'LOI "ID chuỗi không được rỗng hoặc chứa dấu cách, dấu phân cách."))
    ((or (< (length ids) 2) (/= (length ids) (length (bht:field-unique ids)))) (list 'LOI "Chọn ít nhất hai cọc khác nhau."))
    ((vl-some '(lambda (oid) (null (bht:field-marker oid index))) ids) (list 'LOI "Chỉ chọn cột Km hoặc cọc tiêu đủ số và vị trí; cọc thủ công cần xác nhận riêng trước."))
    ((not (and maxoff (> maxoff 0) maxspan (> maxspan 0) tol (> tol 0) (< tol 1))) (list 'LOI "Phạm vi và độ dài phải dương; ngưỡng hệ số từ 0 đến 1."))
    (T
      (setq rec (bht:rec-read "FIELD_CHAIN" id) revision (1+ (if rec (atoi (bht:get rec "revision")) 0)))
      (setq rec (list (cons "chain_id" id) (cons "name" name) (cons "max_offset" (bht:fnum maxoff 3))
        (cons "max_span" (bht:fnum maxspan 3)) (cons "ratio_tol" (bht:fnum tol 6)) (cons "revision" (itoa revision))))
      (foreach oid ids (setq rec (append rec (list (cons "marker" oid)))))
      (bht:rec-write "FIELD_CHAIN" id rec)
      (bht:field-invalidate id)
      (bht:meta-set "field_active_chain" id) (bht:meta-set "station_method" "FIELD_MARKERS")
      (list (strcat "Đã lưu chuỗi " id ". Tính theo cột Km / cọc tiêu; không dùng mốc tuyến. Chạy cập nhật lý trình để tính lại.")))))

(defun bht:field-pair (oid aid bid / rec index pos r)
  (setq rec (bht:obj-read oid) index (bht:pt-all) pos (if rec (bht:obj-position rec index)))
  (cond ((or (null rec) (null pos)) (list 'LOI "Hồ sơ chưa có vị trí RTK."))
    ((bht:obj-marker-metres rec) (list 'LOI "Cọc có số Km/H giữ nguyên lý trình đã nhập."))
    ((= aid bid) (list 'LOI "Chọn hai cọc khác nhau theo chiều Km tăng."))
    (T (setq r (bht:field-interval aid bid (bht:field-chain-default) index pos))
      (if (not (bht:station-determined (cdr (assoc 'status r)))) (list 'LOI (cdr (assoc 'note r)))
        (progn
          (setq rec (bht:set (bht:set (bht:set rec "field_pair_a" aid) "field_pair_b" bid) "field_chain_id" ""))
          (bht:obj-write oid (bht:field-store-result rec pos r))
          (bht:meta-set "station_method" "FIELD_MARKERS")
          (bht:kh-station-label-refresh (list oid))
          (list (strcat (bht:fmt-km (cdr (assoc 'station r))) " | " (cdr (assoc 'status r))) (cdr (assoc 'note r))))))))

(defun bht:field-unique (ids / out) (foreach id ids (setq out (bht:unique-add out id))) out)

;; Imported markers are proposed from current records. Reviewing a snapshot is
;; separate from calculating: never silently bridge a deleted or edited marker.
(defun bht:field-input-rows (index / rows m)
  (foreach id (bht:obj-ids) (if (setq m (bht:field-marker id index)) (setq rows (cons m rows))))
  (vl-sort rows '(lambda (a b) (if (= (cadr a) (cadr b)) (< (car a) (car b)) (< (cadr a) (cadr b))))))

(defun bht:field-input-snapshot (rows)
  (bht:join (mapcar '(lambda (m) (strcat (car m) "|" (bht:fnum (cadr m) 8) "|"
    (bht:fnum (car (caddr m)) 8) "|" (bht:fnum (cadr (caddr m)) 8) "|" (bht:str (nth 3 m)) "|" (bht:str (nth 4 m)))) rows) ";"))

(defun bht:field-input-blocked (rows / todo a b c prev bad pa pb pc dot)
  (setq todo rows)
  (while (cadr todo)
    (setq a (car todo) b (cadr todo) c (caddr todo))
    (if (= (cadr a) (cadr b))
      (progn
        (if prev (setq bad (bht:unique-add bad (strcat (car prev) "|" (car a)))))
        (setq bad (bht:unique-add bad (strcat (car a) "|" (car b))))
        (if c (setq bad (bht:unique-add bad (strcat (car b) "|" (car c)))))))
    (if c
      (progn
        (setq pa (caddr a) pb (caddr b) pc (caddr c)
          dot (+ (* (- (car pb) (car pa)) (- (car pc) (car pb)))
                 (* (- (cadr pb) (cadr pa)) (- (cadr pc) (cadr pb)))))
        ;; A reversal or >=90 degree turn requires checking the local order.
        (if (<= dot 0.0)
          (setq bad (bht:unique-add (bht:unique-add bad (strcat (car a) "|" (car b))) (strcat (car b) "|" (car c)))))))
    (setq prev a todo (cdr todo)))
  bad)

(defun bht:field-input-chain (rows / rec)
  (setq rec (append (bht:field-chain-default)
    '(("chain_id" . "BHT_IMPORTED_MARKERS") ("name" . "Cọc đã nhập") ("kind" . "IMPORTED_MARKERS"))
    (list (cons "input_snapshot" (bht:field-input-snapshot rows)))))
  (foreach m rows (setq rec (append rec (list (cons "marker" (car m))))))
  (foreach key (bht:field-input-blocked rows) (setq rec (append rec (list (cons "blocked_pair" key)))))
  rec)

(defun bht:field-input-preview (/ index rows chain lines prev pos r status item rec station ids manual)
  (setq index (bht:pt-all) rows (bht:field-input-rows index) chain (bht:field-input-chain rows)
    lines (list (bht:field-input-snapshot rows)))
  (foreach m rows
    (setq status "Cọc đầu")
    (if prev
      (progn
        (setq pos (mapcar '(lambda (a b) (/ (+ a b) 2.0)) (caddr prev) (caddr m))
          r (bht:field-interval (car prev) (car m) chain index pos)
          status (if (= (cdr (assoc 'status r)) "NOI_SUY") "Nội suy" (strcat "CẦN ĐỐI CHIẾU: " (cdr (assoc 'note r)))))))
    (setq lines (append lines (list (strcat (car m) "|" (bht:fnum (cadr m) 8) "|"
      (bht:fnum (car (caddr m)) 8) "|" (bht:fnum (cadr (caddr m)) 8) "|"
      (if (= (nth 3 m) "MANUAL_CAD") "[MANUAL_ON] " "") status "|" (nth 4 m) "|" (nth 3 m)))) prev m))
  ;; All excluded markers remain visible, including measured ones missing Km/H.
  (setq ids (mapcar 'car rows))
  (foreach item (bht:rec-all "OBJ")
    (setq rec (cdr item))
    (if (and (member (bht:get rec "nhom") '("COC_TIEU" "COT_KM")) (not (member (car item) ids)))
      (progn
        (setq pos (bht:obj-position rec index) station (bht:obj-marker-metres rec) manual (bht:obj-manual-position-p rec index))
        (if (vl-some '(lambda (pid) (null (bht:pt-find pid index))) (bht:get-all rec "pt")) (setq pos nil))
        (setq lines (append lines (list (strcat (car item) "|" (if station (bht:fnum station 8) "NaN") "|"
          (if pos (bht:fnum (car pos) 8) "NaN") "|" (if pos (bht:fnum (cadr pos) 8) "NaN") "|" (if manual "[MANUAL_OFF] " "[INCOMPLETE] ")
          (if (and station pos) "Chưa dùng làm mốc; chọn dòng rồi xác nhận nếu đã kiểm tra vị trí và số cọc." "Thiếu Km/H hoặc vị trí; sửa hồ sơ trước khi xác nhận.")
          (if (= (bht:get rec "field_manual_approved") "1") " Dữ liệu đã đổi; xác nhận cũ hết hiệu lực." "")
          "|" (bht:get rec "nhom") "|" (if manual "MANUAL_CAD" "RTK"))))))))
  lines)

(defun bht:field-input-use (snapshot / rows rec old)
  (setq rows (bht:field-input-rows (bht:pt-all)) old (bht:rec-read "FIELD_CHAIN" "BHT_IMPORTED_MARKERS"))
  (cond
    ((and old (/= (bht:get old "kind") "IMPORTED_MARKERS")) (list 'LOI "Đã có nhóm cọc cũ mang tên BHT_IMPORTED_MARKERS; chưa thay thế dữ liệu cũ. Cần xử lý tên nhóm bị trùng trước khi dùng danh sách tự lấy."))
    ((< (length rows) 2) (list 'LOI "Cần ít nhất hai mốc (cột Km hoặc cọc tiêu) đủ dữ liệu. Cọc thủ công cần xác nhận riêng trước khi dùng làm mốc."))
    ((/= snapshot (bht:field-input-snapshot rows)) (list 'LOI "Dữ liệu cọc đã đổi trong lúc xem; mở lại Mốc lý trình đã nhập."))
    (T
      (setq rec (bht:field-input-chain rows) old (bht:rec-read "FIELD_CHAIN" "BHT_IMPORTED_MARKERS"))
      (if (/= (bht:get old "input_snapshot") snapshot)
        (progn
          (setq rec (bht:set rec "revision" (itoa (1+ (atoi (bht:get old "revision"))))))
          (bht:rec-write "FIELD_CHAIN" "BHT_IMPORTED_MARKERS" rec)
          (bht:field-invalidate "BHT_IMPORTED_MARKERS")))
      (bht:meta-set "station_method" "FIELD_MARKERS")
      (bht:meta-set "field_active_chain" "BHT_IMPORTED_MARKERS")
      (list "Đã dùng cọc đã nhập. Đoạn bất thường cần Chọn 2 cọc; nhóm/cặp riêng đã gán cho từng hồ sơ được giữ nguyên."))))

(defun bht:field-accepted (rec r)
  (and (bht:station-determined (cdr (assoc 'status r)))
    (or (/= (cdr (assoc 'status r)) "CAN_KIEM_TRA")
        (and (/= (bht:get rec "field_pair_a") "") (/= (bht:get rec "field_pair_b") "")))))

(defun bht:field-invalidate (cid / item rec)
  (foreach item (bht:rec-all "OBJ")
    (setq rec (cdr item))
    (if (and (= (bht:get rec "station_source") "FIELD_MARKERS") (= (bht:get rec "field_chain_id") cid))
      (bht:obj-write (car item) (bht:set (bht:set rec "station_status" "NEEDS_REVIEW") "field_last_error" "Chuỗi cọc đã thay đổi; cần tính lại.")))))
