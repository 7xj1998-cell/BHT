;;; ----------------------------------------------------------------------
;;; Ky hieu (lop the hien) - INSERT layer BHT_KYHIEU + TEXT layer BHT_NHAN
;;;  XData "BHT_KH":
;;;   0.3.2: (object_id) tren ca INSERT va TEXT.
;;;   0.3.3 INSERT: (object_id trang_thai x y goc ty_le)
;;;         TEXT  : (object_id "NHAN" trang_thai x y)
;;;   trang_thai TU_DONG: BHT dat (x y goc ty_le = gia tri BHT dat lan cuoi);
;;;   TAY: nguoi dung da doi vi tri / goc / ty le -> GIU NGUYEN khi cap nhat.
;;;  Vi tri tu dong = trung binh toa do RTK cua doi tuong (khong dich trinh bay).
;;;  Cap nhat theo object_id: tao thieu, sua thay doi, chi xoa ky hieu cua ho so
;;;  da xoa. Khong dung vao thuc the khong mang XData BHT_KH. Ho so (XRecord)
;;;  la nguon du lieu; ky hieu chi la lop the hien.
;;; ----------------------------------------------------------------------

(setq *bht-bb-catalog*
  '(("W.207"  "BHT_KH_BB_W207_V044"       "Giao nhau với đường không ưu tiên")
    ("W.209"  "BHT_KH_BB_W209_V044"       "Giao nhau có tín hiệu đèn")
    ("W.239a" "BHT_KH_BB_W239A_V044"      "Đường cáp điện phía trên + S.509a")
    ("W.245a" "BHT_KH_BB_W245A_V044"      "Đi chậm")
    ("W.201"  "BHT_KH_BB_W201_V044"       "Chỗ ngoặt nguy hiểm")
    ("W.225"  "BHT_KH_BB_W225_V044"       "Trẻ em")
    ("R.412"  "BHT_KH_BB_R412_V044"       "Làn dành riêng cho từng loại xe")
    ("I.414"  "BHT_KH_BB_I414_V044"       "Chỉ hướng đường")
    ("I.423a" "BHT_KH_BB_I423A_V044"      "Vị trí người đi bộ sang ngang")
    ("I.428a" "BHT_KH_BB_I428A_V044"      "Cửa hàng xăng dầu")
    ("I.434a" "BHT_KH_BB_I434A_V044"      "Bến xe buýt")
    ("P.115"  "BHT_KH_BB_P115_V044"       "Hạn chế trọng tải toàn bộ xe")
    ("P.119"  "BHT_KH_BB_P119_V044"       "Hạn chế chiều dài xe")
    ("P.124a" "BHT_KH_BB_P124A_V044"      "Cấm quay đầu xe")
    ("P.125"  "BHT_KH_BB_P125_V044"       "Cấm vượt")
    ("P.127"  "BHT_KH_BB_P127_V044"       "Tốc độ tối đa cho phép")))

(defun bht:bb-code-key (s / u)
  (setq u (strcase (bht:trim s)))
  (foreach ch '(" " "." "-" "_" "/" "+" "," ";" ":")
    (setq u (bht:replace u ch "")))
  u
)

;; Chon block theo ma hieu ho so. Bien the a,b,c... dung chung hinh tong quat
;; cua cung ma; gia tri so 20/40 cua P.127 duoc giu rieng neu co trong ma.
(defun bht:tdt-block-name (code / s out i ch a under)
  (setq s (strcase (bht:trim code)) out "BHT_TDT_V0613_" i 1 under nil)
  (while (<= i (strlen s))
    (setq ch (substr s i 1) a (ascii ch))
    (if (or (and (>= a 48) (<= a 57)) (and (>= a 65) (<= a 90)))
      (progn (setq out (strcat out ch)) (setq under nil))
      (if (not under) (progn (setq out (strcat out "_")) (setq under T))))
    (setq i (1+ i)))
  (vl-string-right-trim "_" out)
)

;; BHTTDTBLOCK do BHT.Bridge dang ky. Ham clone dung block cua thu vien TDT
;; tren may, boc lai voi cot cao 0.6 va mat bien cao 1.8 theo extents.
;; Neu may khong co TDT/Bridge hoac ma khong co hinh, BHT dung block noi bo ben duoi.
;; 0.4.6-fix2: khi Bridge chua dang ky BHTTDTBLOCK, (vl-catch-all-apply 'BHTTDTBLOCK ...)
;; nem "bad function: BHTTDTBLOCK" ra ngoai (vl-catch-all-apply khong bat duoc) -> kiem tra truoc.
(defun bht:tdt-import-block (code description / want r)
  (if (bht:fn-defined-p 'BHTSIGNVALIDATE)
    (progn
      (setq r (BHTSIGNVALIDATE code))
      (if (/= r "") (vl-exit-with-error r))))
  (setq want (bht:tdt-block-name code))
  (cond
    ((and (= description "") (not (bht:fn-defined-p 'BHTTDTBLOCK)) (tblsearch "BLOCK" want)) want)
    ((= (bht:trim code) "") nil)
    ((not (bht:fn-defined-p 'BHTTDTBLOCK)) nil)
    (T
     (setq r (vl-catch-all-apply 'BHTTDTBLOCK (list code description)))
     (if (and (not (vl-catch-all-error-p r)) (listp r) (= (strcase (bht:str (car r))) "OK")
              (cadr r) (tblsearch "BLOCK" (cadr r)))
       (cadr r)
       nil)))
)

;; Native fallback accepts a family code or one letter variant, never an arbitrary substring.
(defun bht:bb-family-p (key base / suffix)
  (and (bht:starts key base)
       (or (= key base)
           (and (= (strlen key) (1+ (strlen base)))
                (setq suffix (ascii (substr key (1+ (strlen base)))))
                (<= 65 suffix) (<= suffix 90)))))

(defun bht:bb-block-for (code / k tdt speed name)
  (setq k (bht:bb-code-key code) tdt (bht:tdt-import-block code ""))
  ;; Exact legacy combined plate, not an arbitrary substring match.
  (if (= k "W239AS509A") (setq k "W239A"))
  (cond
    (tdt tdt)
    ((and (vl-string-search "@" code) (= (strcase (substr code 1 (vl-string-search "@" code))) "S.505A"))
      (vl-exit-with-error "Không có mẫu CAD S.505a để tạo biển trọng lượng; hãy nạp thư viện TDT."))
    ((or (vl-string-search "@" code) (member (strcase code) '("S.501" "S.502" "S.509A"))) (bht:bb-metre-native code))
    ((bht:bb-family-p k "W207") "BHT_KH_BB_W207_V044")
    ((bht:bb-family-p k "W209") "BHT_KH_BB_W209_V044")
    ((bht:bb-family-p k "W239") "BHT_KH_BB_W239A_V044")
    ((bht:bb-family-p k "W245") "BHT_KH_BB_W245A_V044")
    ((bht:bb-family-p k "W201") "BHT_KH_BB_W201_V044")
    ((bht:bb-family-p k "W225") "BHT_KH_BB_W225_V044")
    ((bht:bb-family-p k "R412") "BHT_KH_BB_R412_V044")
    ((bht:bb-family-p k "I414") "BHT_KH_BB_I414_V044")
    ((bht:bb-family-p k "I423") "BHT_KH_BB_I423A_V044")
    ((bht:bb-family-p k "I428") "BHT_KH_BB_I428A_V044")
    ((bht:bb-family-p k "I434") "BHT_KH_BB_I434A_V044")
    ((bht:bb-family-p k "P115") "BHT_KH_BB_P115_V044")
    ((bht:bb-family-p k "P119") "BHT_KH_BB_P119_V044")
    ((bht:bb-family-p k "P124") "BHT_KH_BB_P124A_V044")
    ((bht:bb-family-p k "P125") "BHT_KH_BB_P125_V044")
    ((and (bht:starts k "P127") (setq speed (bht:num (substr k 5))) (= speed (fix speed)) (<= 5 speed) (<= speed 130))
      (cond ((= speed 20) "BHT_KH_BB_P127_20_V044") ((= speed 40) "BHT_KH_BB_P127_40_V044")
        (T (setq name (strcat "BHT_KH_BB_P127_" (itoa (fix speed)) "_NATIVE"))
           (bht:block name (bht:bb-speed-g (itoa (fix speed)))) name)))
    ((= k "P127") "BHT_KH_BB_P127_V044")
    (T "BHT_KH_BIEN_BAO_V044"))
)

(defun bht:bb-metre-native (code / dot base value name parts e d text x)
  (setq dot (vl-string-search "@" code) base (strcase (if dot (substr code 1 dot) code)))
  (if (null dot) (setq code (strcat code "@" (cond ((= base "S.501") "800") ((= base "S.502") "200") (T "5")))))
  (if (not (bht:fn-defined-p 'BHTMETRETEXT)) (vl-exit-with-error "Nạp BHT.Bridge.dll để nhập giá trị mét."))
  (setq value (BHTMETRETEXT code "0"))
  (if (= value "0") (vl-exit-with-error "Giá trị mét không hợp lệ."))
  (setq name (strcat "BHT_NATIVE_METRE_" (bht:tdt-block-name code)))
  (cond
    ((tblsearch "BLOCK" name) name)
    ((= base "P.119")
      (setq e (tblobjname "BLOCK" "BHT_KH_BB_P119_V044") parts nil)
      (while (and (setq e (entnext e)) (/= (cdr (assoc 0 (setq d (entget e)))) "ENDBLK"))
        (setq d (vl-remove-if '(lambda (g) (member (car g) '(-1 5 330 360))) d))
        (if (= (cdr (assoc 0 d)) "TEXT") (setq d (bht:dxf-put d 1 (BHTMETRETEXT code (cdr (assoc 1 d))))))
        (setq parts (append parts (list d))))
      (bht:block name parts) name)
    ((member base '("S.501" "S.502" "S.509A"))
      (setq parts (append (bht:bb-post-g)
        (list (bht:rect-fill-g -1.3 0.62 1.3 2.42 (if (= base "S.509A") 5 7)))
        (bht:poly-color-g '((-1.3 0.62 0.0) (1.3 0.62 0.0) (1.3 2.42 0.0) (-1.3 2.42 0.0)) (if (= base "S.509A") 7 250))))
      (if (= base "S.509A")
        (setq parts (append parts (list (bht:text-center-color-g "CHIỀU CAO" '(0.0 2.08 0.0) 0.28 7)
          (bht:text-center-color-g "AN TOÀN" '(0.0 1.61 0.0) 0.28 7)
          (bht:text-center-color-g (strcat value " m") '(0.0 1.08 0.0) 0.38 7))))
        (progn
          (setq parts (append parts (list (bht:text-center-color-g (strcat value " m") '(0.0 1.52 0.0) 0.4 250))))
          (if (= base "S.501")
            (foreach x '(-1.03 1.03)
              (setq parts (append parts (list (bht:rect-fill-g (- x 0.055) 0.86 (+ x 0.055) 1.97 250)
                (bht:solid-g (list x 2.20 0.0) (list (- x 0.17) 1.95 0.0) (list (+ x 0.17) 1.95 0.0) (list (+ x 0.17) 1.95 0.0) 250))))))))
      (bht:block name parts) name)
    (T (vl-exit-with-error "Biển mét này cần block TDT hoặc block tùy chỉnh; không dùng số mặc định thay thế."))))

(defun bht:bb-post-g ()
  (list (bht:line-color-g '(0.0 0.0 0.0) '(0.0 0.62 0.0) 7)
        (bht:circle-g '(0.0 0.0 0.0) 0.07))
)

(defun bht:bb-triangle-base (/ ot ol orr it il ir)
  (setq ot '(0.0 1.86 0.0) ol '(-0.75 0.56 0.0) orr '(0.75 0.56 0.0)
        it '(0.0 1.68 0.0) il '(-0.58 0.67 0.0) ir '(0.58 0.67 0.0))
  (append (bht:bb-post-g)
          (list (bht:solid-g ot ol orr orr 1)
                (bht:solid-g it il ir ir 2))
          (bht:poly-color-g (list ot ol orr) 1)
          (bht:poly-color-g (list it il ir) 1))
)

(defun bht:bb-circle-base (/ c)
  (setq c '(0.0 1.20 0.0))
  (append (bht:bb-post-g)
          (bht:disc-g c 0.63 24 1)
          (bht:disc-g c 0.49 24 7)
          (list (bht:circle-g c 0.63) (bht:circle-g c 0.49)))
)

(defun bht:bb-blue-base (/ out)
  (append (bht:bb-post-g)
          (list (bht:rect-fill-g -0.68 0.56 0.68 1.84 5)
                (bht:rect-fill-g -0.51 0.72 0.51 1.68 7))
          (bht:poly-color-g '((-0.68 0.56 0.0) (0.68 0.56 0.0)
                              (0.68 1.84 0.0) (-0.68 1.84 0.0)) 5))
)

(defun bht:bb-blue-wide-base ()
  (append (bht:bb-post-g)
          (list (bht:rect-fill-g -1.08 0.62 1.08 1.72 5))
          (bht:poly-color-g '((-1.08 0.62 0.0) (1.08 0.62 0.0)
                              (1.08 1.72 0.0) (-1.08 1.72 0.0)) 7))
)

(defun bht:bb-speed-g (value)
  (append (bht:bb-circle-base)
          (list (bht:text-center-color-g value '(0.0 1.20 0.0) 0.30 250)))
)

(defun bht:bb-catalog-report (/ item)
  (bht:msg "BHT - Danh mục block biển báo chuẩn theo ảnh KMZ DT830:")
  (foreach item *bht-bb-catalog*
    (bht:msg (strcat "  " (car item) " - " (caddr item))))
  (bht:msg "Nhập mã này vào trường Mã của hồ sơ BIEN_BAO, rồi chạy BHTKYHIEU.")
  (bht:msg "Mã chưa nhận diện dùng block biển báo tổng quát; BHTBLOCK vẫn cho nạp DWG tùy chọn.")
  (princ)
)

;;; 5.0: tim bien khi go - khong dau (ca đ -> d), khong phan biet hoa, moi tu
;;; cua truy van phai co trong MA hoac TEN. Ma so khop ca dang bo dau cham
;;; ("w245a" = "W.245a"). Uu tien danh muc bien TDT day du (BHTSIGNSEARCH cua
;;; BHT.Bridge); khong co Bridge -> danh muc noi bo *bht-bb-catalog*.
;; Vietnamese accent folding shared by sign search and condition input.
(defun bht:fold-vi (s / p)
  (foreach p
    '(("À" "A") ("Á" "A") ("Ả" "A") ("Ã" "A") ("Ạ" "A")
      ("Ă" "A") ("Ằ" "A") ("Ắ" "A") ("Ẳ" "A") ("Ẵ" "A") ("Ặ" "A")
      ("Â" "A") ("Ầ" "A") ("Ấ" "A") ("Ẩ" "A") ("Ẫ" "A") ("Ậ" "A")
      ("È" "E") ("É" "E") ("Ẻ" "E") ("Ẽ" "E") ("Ẹ" "E")
      ("Ê" "E") ("Ề" "E") ("Ế" "E") ("Ể" "E") ("Ễ" "E") ("Ệ" "E")
      ("Ì" "I") ("Í" "I") ("Ỉ" "I") ("Ĩ" "I") ("Ị" "I")
      ("Ò" "O") ("Ó" "O") ("Ỏ" "O") ("Õ" "O") ("Ọ" "O")
      ("Ô" "O") ("Ồ" "O") ("Ố" "O") ("Ổ" "O") ("Ỗ" "O") ("Ộ" "O")
      ("Ơ" "O") ("Ờ" "O") ("Ớ" "O") ("Ở" "O") ("Ỡ" "O") ("Ợ" "O")
      ("Ù" "U") ("Ú" "U") ("Ủ" "U") ("Ũ" "U") ("Ụ" "U")
      ("Ư" "U") ("Ừ" "U") ("Ứ" "U") ("Ử" "U") ("Ữ" "U") ("Ự" "U")
      ("Ỳ" "Y") ("Ý" "Y") ("Ỷ" "Y") ("Ỹ" "Y") ("Ỵ" "Y")
      ("Đ" "D")
      ("à" "a") ("á" "a") ("ả" "a") ("ã" "a") ("ạ" "a")
      ("ă" "a") ("ằ" "a") ("ắ" "a") ("ẳ" "a") ("ẵ" "a") ("ặ" "a")
      ("â" "a") ("ầ" "a") ("ấ" "a") ("ẩ" "a") ("ẫ" "a") ("ậ" "a")
      ("è" "e") ("é" "e") ("ẻ" "e") ("ẽ" "e") ("ẹ" "e")
      ("ê" "e") ("ề" "e") ("ế" "e") ("ể" "e") ("ễ" "e") ("ệ" "e")
      ("ì" "i") ("í" "i") ("ỉ" "i") ("ĩ" "i") ("ị" "i")
      ("ò" "o") ("ó" "o") ("ỏ" "o") ("õ" "o") ("ọ" "o")
      ("ô" "o") ("ồ" "o") ("ố" "o") ("ổ" "o") ("ỗ" "o") ("ộ" "o")
      ("ơ" "o") ("ờ" "o") ("ớ" "o") ("ở" "o") ("ỡ" "o") ("ợ" "o")
      ("ù" "u") ("ú" "u") ("ủ" "u") ("ũ" "u") ("ụ" "u")
      ("ư" "u") ("ừ" "u") ("ứ" "u") ("ử" "u") ("ữ" "u") ("ự" "u")
      ("ỳ" "y") ("ý" "y") ("ỷ" "y") ("ỹ" "y") ("ỵ" "y")
      ("đ" "d")
      ("—" "-") ("–" "-") ("•" "*"))
    (setq s (bht:replace s (car p) (cadr p))))
  s
)

(defun bht:search-fold (s) (strcase (bht:fold-vi (bht:str s)) T))

(defun bht:sign-match-p (q code name / hay key ok k)
  (setq hay (strcat (bht:search-fold code) " " (bht:search-fold name))
        key (strcase (bht:bb-code-key (bht:fold-vi (bht:str code))) T) ok T)
  (foreach w (vl-remove "" (bht:split (bht:search-fold q) " "))
    (setq k (strcase (bht:bb-code-key w) T))
    (if (not (or (vl-string-search w hay) (and (/= k "") (vl-string-search k key))))
      (setq ok nil)))
  ok
)

;; Tra ve danh sach (ma ten), toi da maxn muc.
(defun bht:sign-search (q maxn / r out)
  (if (and (bht:bridge-load 'BHTSIGNSEARCH)
           (setq r (vl-catch-all-apply 'BHTSIGNSEARCH (list (bht:str q) maxn)))
           (not (vl-catch-all-error-p r)) (listp r) (= (car r) "OK"))
    (mapcar '(lambda (x / p) (setq p (vl-string-search "|" x))
               (if p (list (substr x 1 p) (substr x (+ p 2))) (list x "")))
            (cdr r))
    (progn
      (foreach item *bht-bb-catalog*
        (if (and (< (length out) maxn) (bht:sign-match-p q (car item) (caddr item)))
          (setq out (append out (list (list (car item) (caddr item)))))))
      out))
)

(defun bht:sign-search-report (q / hits)
  (setq hits (bht:sign-search q 30))
  (if hits
    (progn
      (bht:msg (strcat "BHT - biển khớp \"" q "\" (" (itoa (length hits)) " kết quả, tối đa 30):"))
      (foreach h hits (bht:msg (strcat "  " (car h) (if (/= (cadr h) "") (strcat " - " (cadr h)) " - (chưa có tên trong thư viện TDT)")))))
    (bht:msg (strcat "BHT: không có biển nào khớp \"" q "\".")))
  hits
)

(defun c:BHTBBDANHMUC (/ *error*)
  (setq *error* bht:on-error)
  (bht:symbol-blocks)
  (bht:bb-catalog-report)
)

(defun bht:kh-km-shift-g (g)
  (mapcar '(lambda (pair)
    (if (member (car pair) '(10 11 12 13))
      (cons (car pair) (mapcar '- (cdr pair) '(1.75 0.0 0.0))) pair)) g))

(defun bht:symbol-blocks ()
  ;; Block tong quat: diem chen o chan cot; dau hoi cho biet chua co ma QCVN.
  (bht:block "BHT_KH_BIEN_BAO_V044"
    (append (bht:bb-post-g)
            (list (bht:rect-fill-g -0.58 0.66 0.58 1.70 8)
                  (bht:text-center-color-g "?" '(0.0 1.18 0.0) 0.45 7))))

  ;; W.207 - nhom pho bien nhat trong KMZ: truc duong chinh va nhanh ben.
  (bht:block "BHT_KH_BB_W207_V044"
    (append (bht:bb-triangle-base)
            (list (bht:rect-fill-g -0.07 0.82 0.07 1.48 250)
                  (bht:rect-fill-g 0.04 1.13 0.42 1.27 250))))
  ;; W.209 - den tin hieu theo thu tu do, vang, xanh.
  (bht:block "BHT_KH_BB_W209_V044"
    (append (bht:bb-triangle-base)
            (list (bht:rect-fill-g -0.18 0.83 0.18 1.52 250))
            (bht:disc-g '(0.0 1.39 0.0) 0.09 12 1)
            (bht:disc-g '(0.0 1.18 0.0) 0.09 12 2)
            (bht:disc-g '(0.0 0.97 0.0) 0.09 12 3)))
  ;; W.239a + S.509a: bo canh bao cap dien / chieu cao 4,75 m gap nhieu tren DT830.
  (bht:block "BHT_KH_BB_W239A_V044"
    (append (bht:bb-triangle-base)
            (list (bht:solid-g '(-0.08 1.53 0.0) '(0.18 1.53 0.0)
                                '(-0.02 1.17 0.0) '(0.08 1.20 0.0) 250)
                  (bht:solid-g '(-0.02 1.23 0.0) '(0.16 1.23 0.0)
                                '(-0.20 0.84 0.0) '(-0.07 1.16 0.0) 250)
                  (bht:rect-fill-g -0.58 0.15 0.58 0.50 5)
                  (bht:text-center-color-g "4,75 m" '(0.0 0.33 0.0) 0.16 7))))
  (bht:block "BHT_KH_BB_W245A_V044"
    (append (bht:bb-triangle-base)
            (list (bht:text-center-color-g "ĐI" '(0.0 1.34 0.0) 0.17 250)
                  (bht:text-center-color-g "CHẬM" '(0.0 1.08 0.0) 0.13 250))))
  (bht:block "BHT_KH_BB_W201_V044"
    (append (bht:bb-triangle-base)
            (list (bht:line-color-g '(-0.28 0.88 0.0) '(-0.05 1.08 0.0) 250)
                  (bht:line-color-g '(-0.05 1.08 0.0) '(0.08 1.36 0.0) 250)
                  (bht:line-color-g '(0.08 1.36 0.0) '(0.31 1.49 0.0) 250))))
  (bht:block "BHT_KH_BB_W225_V044"
    (append (bht:bb-triangle-base)
            (bht:disc-g '(-0.12 1.37 0.0) 0.07 10 250)
            (bht:disc-g '(0.19 1.28 0.0) 0.06 10 250)
            (list (bht:line-color-g '(-0.12 1.30 0.0) '(-0.02 1.02 0.0) 250)
                  (bht:line-color-g '(0.19 1.22 0.0) '(0.10 0.98 0.0) 250)
                  (bht:line-color-g '(-0.08 1.18 0.0) '(0.18 1.12 0.0) 250))))

  ;; R.412 - so do phan lan tong quat, dung cho cac bang nhom xe trong KMZ.
  (bht:block "BHT_KH_BB_R412_V044"
    (append (bht:bb-blue-wide-base)
            (list (bht:line-color-g '(0.0 0.72 0.0) '(0.0 1.62 0.0) 7)
                  (bht:line-color-g '(-0.48 0.82 0.0) '(-0.48 1.52 0.0) 7)
                  (bht:line-color-g '(0.48 0.82 0.0) '(0.48 1.52 0.0) 7)
                  (bht:text-center-color-g "Ô TÔ" '(-0.52 1.16 0.0) 0.17 7)
                  (bht:text-center-color-g "XE MÁY" '(0.52 1.16 0.0) 0.15 7))))
  (bht:block "BHT_KH_BB_I414_V044"
    (append (bht:bb-blue-wide-base)
            (list (bht:line-color-g '(-0.75 1.17 0.0) '(0.62 1.17 0.0) 7)
                  (bht:line-color-g '(0.62 1.17 0.0) '(0.34 1.43 0.0) 7)
                  (bht:line-color-g '(0.62 1.17 0.0) '(0.34 0.91 0.0) 7)
                  (bht:text-center-color-g "HƯỚNG" '(-0.30 1.48 0.0) 0.14 7))))
  (bht:block "BHT_KH_BB_I423A_V044"
    (append (bht:bb-blue-base)
            (list (bht:solid-g '(0.0 1.58 0.0) '(-0.40 0.80 0.0)
                                '(0.40 0.80 0.0) '(0.40 0.80 0.0) 7))
            (bht:disc-g '(0.0 1.32 0.0) 0.06 10 250)
            (list (bht:line-color-g '(0.0 1.25 0.0) '(-0.08 1.06 0.0) 250)
                  (bht:line-color-g '(-0.08 1.06 0.0) '(-0.24 0.89 0.0) 250)
                  (bht:line-color-g '(-0.08 1.06 0.0) '(0.14 0.91 0.0) 250))))
  (bht:block "BHT_KH_BB_I428A_V044"
    (append (bht:bb-blue-base)
            (list (bht:rect-fill-g -0.20 0.88 0.12 1.48 250)
                  (bht:rect-fill-g -0.14 1.31 0.06 1.42 7)
                  (bht:line-color-g '(0.12 1.39 0.0) '(0.28 1.28 0.0) 250)
                  (bht:line-color-g '(0.28 1.28 0.0) '(0.28 0.98 0.0) 250))))
  (bht:block "BHT_KH_BB_I434A_V044"
    (append (bht:bb-blue-base)
            (list (bht:rect-fill-g -0.35 0.97 0.35 1.43 250)
                  (bht:rect-fill-g -0.27 1.23 0.27 1.36 7))
            (bht:disc-g '(-0.22 0.94 0.0) 0.07 10 250)
            (bht:disc-g '(0.22 0.94 0.0) 0.07 10 250)))

  ;; Nhom bien cam ghi tri so / hinh tu anh KMZ.
  (bht:block "BHT_KH_BB_P115_V044"
    (append (bht:bb-circle-base)
            (list (bht:text-center-color-g "2,5 t" '(0.0 1.20 0.0) 0.25 250))))
  (bht:block "BHT_KH_BB_P119_V044"
    (append (bht:bb-circle-base)
            (list (bht:text-center-color-g "8 m" '(0.0 1.20 0.0) 0.25 250)
                  (bht:line-color-g '(-0.34 0.94 0.0) '(0.34 0.94 0.0) 250))))
  (bht:block "BHT_KH_BB_P124A_V044"
    (append (bht:bb-circle-base)
            (list (bht:text-center-color-g "U" '(0.0 1.20 0.0) 0.42 250)
                  (bht:line-color-g '(-0.40 1.60 0.0) '(0.40 0.80 0.0) 1))))
  (bht:block "BHT_KH_BB_P125_V044"
    (append (bht:bb-circle-base)
            (list (bht:rect-fill-g -0.36 1.08 -0.04 1.30 250)
                  (bht:rect-fill-g 0.04 1.08 0.36 1.30 1))
            (bht:disc-g '(-0.28 1.05 0.0) 0.06 10 250)
            (bht:disc-g '(-0.10 1.05 0.0) 0.06 10 250)
            (bht:disc-g '(0.10 1.05 0.0) 0.06 10 1)
            (bht:disc-g '(0.28 1.05 0.0) 0.06 10 1)))
  (bht:block "BHT_KH_BB_P127_V044" (bht:bb-speed-g "MAX"))
  (bht:block "BHT_KH_BB_P127_20_V044" (bht:bb-speed-g "20"))
  (bht:block "BHT_KH_BB_P127_40_V044" (bht:bb-speed-g "40"))

  ;; Coc tieu: diem chen tai chan vuong trang (0,0); dau do ve phia -X.

  (bht:block "BHT_KH_COC_TIEU"
    (list
      (bht:solid-g '(-1.82 -0.22 0.0) '(-1.52 -0.22 0.0) '(-1.82 0.22 0.0) '(-1.52 0.22 0.0) 1)
      (bht:line-color-g '(-1.82 -0.22 0.0) '(-0.10 -0.22 0.0) 7)
      (bht:line-color-g '(-0.10 -0.22 0.0) '(-0.10 0.22 0.0) 7)
      (bht:line-color-g '(-0.10 0.22 0.0) '(-1.82 0.22 0.0) 7)
      (bht:line-color-g '(-1.82 0.22 0.0) '(-1.82 -0.22 0.0) 7)
      (bht:line-color-g '(-0.27 -0.34 0.0) '(0.26 -0.34 0.0) 3)
      (bht:line-color-g '(0.26 -0.34 0.0) '(0.26 0.34 0.0) 3)
      (bht:line-color-g '(0.26 0.34 0.0) '(-0.27 0.34 0.0) 3)
      (bht:line-color-g '(-0.27 0.34 0.0) '(-0.27 -0.34 0.0) 3)
      (bht:solid-g '(-0.12 -0.18 0.0) '(0.14 -0.18 0.0) '(-0.12 0.18 0.0) '(0.14 0.18 0.0) 7)))
  ;; 0.4.3: cot Km theo mau: nua tron do, than bang trang, chu KM va nep trang.
  ;; Diem chen = tam mat phang phia nua tron (0,0).
  (bht:block "BHT_KH_COT_KM_V043"
    (list
      (bht:solid-g '(0.0 0.0 0.0) '(0.0 0.75 0.0) '(-0.287 0.693 0.0) '(-0.287 0.693 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.287 0.693 0.0) '(-0.530 0.530 0.0) '(-0.530 0.530 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.530 0.530 0.0) '(-0.693 0.287 0.0) '(-0.693 0.287 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.693 0.287 0.0) '(-0.750 0.0 0.0) '(-0.750 0.0 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.750 0.0 0.0) '(-0.693 -0.287 0.0) '(-0.693 -0.287 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.693 -0.287 0.0) '(-0.530 -0.530 0.0) '(-0.530 -0.530 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.530 -0.530 0.0) '(-0.287 -0.693 0.0) '(-0.287 -0.693 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.287 -0.693 0.0) '(0.0 -0.75 0.0) '(0.0 -0.75 0.0) 1)
      (bht:line-color-g '(0.0 -0.75 0.0) '(1.75 -0.75 0.0) 7)
      (bht:line-color-g '(1.75 -0.75 0.0) '(1.75 0.75 0.0) 7)
      (bht:line-color-g '(1.75 0.75 0.0) '(0.0 0.75 0.0) 7)
      (bht:line-color-g '(0.0 0.75 0.0) '(0.0 -0.75 0.0) 7)
      (bht:solid-g '(1.62 -0.78 0.0) '(1.88 -0.78 0.0) '(1.62 0.78 0.0) '(1.88 0.78 0.0) 7)
      (bht:text-color-g "KM" '(0.35 -0.28 0.0) 0.58 7)))
  (bht:block "BHT_KH_COT_KM_V0611"
    (mapcar 'bht:kh-km-shift-g (list
      (bht:solid-g '(0.0 0.0 0.0) '(0.0 0.75 0.0) '(-0.287 0.693 0.0) '(-0.287 0.693 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.287 0.693 0.0) '(-0.530 0.530 0.0) '(-0.530 0.530 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.530 0.530 0.0) '(-0.693 0.287 0.0) '(-0.693 0.287 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.693 0.287 0.0) '(-0.750 0.0 0.0) '(-0.750 0.0 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.750 0.0 0.0) '(-0.693 -0.287 0.0) '(-0.693 -0.287 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.693 -0.287 0.0) '(-0.530 -0.530 0.0) '(-0.530 -0.530 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.530 -0.530 0.0) '(-0.287 -0.693 0.0) '(-0.287 -0.693 0.0) 1)
      (bht:solid-g '(0.0 0.0 0.0) '(-0.287 -0.693 0.0) '(0.0 -0.75 0.0) '(0.0 -0.75 0.0) 1)
      (bht:line-color-g '(0.0 -0.75 0.0) '(1.75 -0.75 0.0) 7)
      (bht:line-color-g '(1.75 -0.75 0.0) '(1.75 0.75 0.0) 7)
      (bht:line-color-g '(1.75 0.75 0.0) '(0.0 0.75 0.0) 7)
      (bht:line-color-g '(0.0 0.75 0.0) '(0.0 -0.75 0.0) 7)
      (bht:solid-g '(1.62 -0.78 0.0) '(1.88 -0.78 0.0) '(1.62 0.78 0.0) '(1.88 0.78 0.0) 7)
      (bht:text-color-g "KM" '(0.35 -0.28 0.0) 0.58 7))))
  (bht:block "BHT_KH_BANG_CHI_DAN" (bht:poly-g '((-1.5 -0.7 0.0) (1.5 -0.7 0.0) (1.5 0.7 0.0) (-1.5 0.7 0.0))))
  (bht:block "BHT_KH_BANG_QC" (append (bht:poly-g '((-1.5 -0.7 0.0) (1.5 -0.7 0.0) (1.5 0.7 0.0) (-1.5 0.7 0.0)))
                                      (list (bht:line-g '(-1.5 -0.7 0.0) '(1.5 0.7 0.0)))))
  (bht:block "BHT_KH_DEN" (list (bht:circle-g '(0.0 0.0 0.0) 0.8) (bht:line-g '(-0.8 0.0 0.0) '(0.8 0.0 0.0))
                                (bht:line-g '(0.0 -0.8 0.0) '(0.0 0.8 0.0))))
  (bht:block "BHT_KH_CONG_TRINH" (bht:poly-g '((0.0 1.0 0.0) (1.0 0.0 0.0) (0.0 -1.0 0.0) (-1.0 0.0 0.0))))
  (bht:block "BHT_KH_KHAC" (bht:poly-g '((0.0 1.0 0.0) (1.0 0.0 0.0) (0.0 -1.0 0.0) (-1.0 0.0 0.0))))
  (bht:block "BHT_KH_CHUA_XAC_DINH" (list (bht:circle-g '(0.0 0.0 0.0) 0.6)
                                          (bht:line-g '(-0.6 -0.6 0.0) '(0.6 0.6 0.0))))
)

;; Xoa ky hieu (INSERT + TEXT mang BHT_KH) cua 1 ho so; oid nil = xoa MOI ky hieu BHT.
(defun bht:symbol-delete (oid / ss i ent x n app)
  (setq n 0)
  (foreach app '("BHT_KH" "BHT_SIGN_ORIGIN")
    (setq ss (ssget "_X" (list (list -3 (list app)))) i 0)
    (if ss
      (while (< i (sslength ss))
        (setq ent (ssname ss i) x (bht:xget ent app))
        (if (or (null oid) (and x (= (strcase (car x)) (strcase oid))))
          (progn (entdel ent) (setq n (1+ n))))
        (setq i (1+ i)))))
  n
)

(defun bht:symbol-exists (oid)
  (assoc (strcase oid) (bht:tagged-pairs "INSERT" "BHT_KH" 1))
)

(defun bht:kh-scale (/ s) (setq s (bht:num (bht:meta "kh_scale" "1"))) (if (and s (> s 0)) s 1.0))
(defun bht:sign-scale (/ s)
  (setq s (bht:num (bht:meta "sign_scale" "")))
  (if (and s (> s 0)) s (bht:kh-scale)))
(defun bht:sign-label-scale (sc / s)
  (setq s (bht:num (bht:meta "sign_label_scale" "")))
  (if (and s (> s 0)) s sc))
(defun bht:kh-h (/ s)
  (setq s (bht:num (bht:meta "kh_h" "0.35")))
  (if (and s (> s 0) (not (equal s 1.5 1e-8))) s 0.35))
(defun bht:kh-label-height (rec) (* 0.36 (bht:kh-scale)))
;; Clip a leader segment against a padded label rectangle in the sign frame.
(defun bht:segment-box-hit (a b box / lo hi axis delta t1 t2 ok tmp)
  (setq lo 0.0 hi 1.0 axis 0 ok T)
  (repeat 2
    (setq delta (- (nth axis b) (nth axis a)))
    (if (< (abs delta) 1e-10)
      (if (or (< (nth axis a) (nth axis box)) (> (nth axis a) (nth (+ axis 2) box))) (setq ok nil))
      (progn
        (setq t1 (/ (- (nth axis box) (nth axis a)) delta) t2 (/ (- (nth (+ axis 2) box) (nth axis a)) delta))
        (if (> t1 t2) (setq tmp t1 t1 t2 t2 tmp))
        (setq lo (max lo t1) hi (min hi t2))
        (if (> lo hi) (setq ok nil))))
    (setq axis (1+ axis))) ok)

(defun bht:kh-caption-local (p anchor / d a)
  (setq d (mapcar '- p (car anchor)) a (caddr anchor))
  (list (+ (* (car d) (cos a)) (* (cadr d) (sin a)))
        (+ (* (- (car d)) (sin a)) (* (cadr d) (cos a)))))

;; Use the longest run, not the short return beside the sign face.
;; Ignore the crossbar, which is not a sign-to-support leader.
(defun bht:kh-caption-segments (oid / e d verts last-segment a b segments)
  (foreach e (cdr (assoc oid *bht-caption-leaders*))
    (if (and (setq d (entget e)) (/= (nth 2 (bht:xget e "BHT_KH")) "@BHT_RAIL"))
      (progn
        (setq verts (if (= (cdr (assoc 0 d)) "LINE")
          (list (cdr (assoc 10 d)) (cdr (assoc 11 d)))
          (mapcar '(lambda (v) (list (cadr v) (caddr v) (if (assoc 38 d) (cdr (assoc 38 d)) 0.0)))
                  (vl-remove-if-not '(lambda (v) (= (car v) 10)) d))) last-segment nil)
        (while (cdr verts)
          (setq a (car verts) b (cadr verts))
          (if (and (> (distance a b) 1e-8)
                   (or (null last-segment) (> (distance a b) (distance (car last-segment) (cadr last-segment)))))
            (setq last-segment (list a b)))
          (setq verts (cdr verts)))
        (if last-segment (setq segments (cons last-segment segments))))))
  segments)

(defun bht:kh-caption-leader-layout (layout anchor / rec segments segment center count dx dy a rot rows h height width tb value normal extent selected candidate best)
  (setq rec *bht-caption-rec* h (cadr layout)
        segments (bht:kh-caption-segments (bht:get rec "object_id"))
        rows (if (= (bht:get rec "nhom") "BIEN_BAO") (max 1 (length (bht:kh-sign-codes rec)))
                 (if (= (bht:kh-label (bht:get rec "object_id") rec) "") 0 1)))
  (if (/= (bht:get rec "ly_trinh_km") "") (setq rows (1+ rows)))
  (setq height (* h (+ 1.0 (* 1.5 (max 0 (1- rows))))) width 0.0)
  (foreach value (append (if (= (bht:get rec "nhom") "BIEN_BAO")
                            (mapcar '(lambda (v) (bht:kh-code-label "BIEN_BAO" v)) (bht:kh-sign-codes rec))
                            (list (bht:kh-label (bht:get rec "object_id") rec))) (list (bht:get rec "ly_trinh_km")))
    (if (/= (bht:trim value) "")
      (progn
        (setq tb (textbox (list '(0 . "TEXT") (cons 1 (bht:cad-text value (bht:kh-label-style value)))
                                (cons 7 (bht:kh-label-style value)) (cons 40 h))))
        (if tb (setq width (max width (- (caadr tb) (caar tb))))))))
  (if segments
    (progn
      (setq center '(0.0 0.0 0.0) count (length segments) dx 0.0 dy 0.0)
      (foreach segment segments
        (setq center (mapcar '+ center (mapcar '(lambda (x y) (/ (+ x y) 2.0)) (car segment) (cadr segment)))
              a (angle (car segment) (cadr segment)) dx (+ dx (cos a)) dy (+ dy (sin a))))
      (setq center (mapcar '(lambda (v) (/ v count)) center)
            a (if (< (+ (abs dx) (abs dy)) 1e-8) (angle (caar segments) (cadar segments)) (atan dy dx))))
    (setq center (car layout) a (+ (caddr anchor) (/ pi 2.0))))
  (setq rot (bht:kh-readable-angle (+ a (if (= (bht:meta "sign_label_orientation" "PARALLEL") "PERPENDICULAR") (/ pi 2.0) 0.0))))
  (if (and segments (= (bht:get rec "nhom") "BIEN_BAO"))
    (progn
      ;; The upper leader is the common datum: codes above, chainage below.
      (setq normal (+ (bht:kh-readable-angle a) (/ pi 2.0)) best nil)
      (foreach segment segments
        (setq candidate (mapcar '(lambda (x y) (/ (+ x y) 2.0)) (car segment) (cadr segment))
              extent (+ (* (car candidate) (cos normal)) (* (cadr candidate) (sin normal))))
        (if (or (null best) (> extent best)) (setq best extent center candidate selected segment)))
      (setq a (angle (car selected) (cadr selected))
            rot (bht:kh-readable-angle (+ a (if (= (bht:meta "sign_label_orientation" "PARALLEL") "PERPENDICULAR") (/ pi 2.0) 0.0)))
            rows (max 1 (length (bht:kh-sign-codes rec))))
      (list (polar center (+ rot (/ pi 2.0)) (* h (+ 1.25 (* 1.5 (1- rows)))) ) h rot))
    (progn
  (if (and segments (= count 1))
    (progn
      (setq normal (+ (bht:kh-readable-angle a) (/ pi 2.0))
            extent (+ (* 0.5 width (abs (cos (- rot normal)))) (* 0.5 height (abs (sin (- rot normal))))))
      (setq center (polar center normal (+ extent (* 0.6 h))))))
  ;; Center the complete stack between two leaders; scaling keeps its center fixed.
  (list (polar center (+ rot (/ pi 2.0)) (* 0.5 height)) h rot))))

(defun bht:kh-caption-clear (layout anchor / rec oid h width rows value box tb center candidates candidate found
                                              leaders pair e d verts segments a b gap)
  (setq rec *bht-caption-rec* oid (bht:get rec "object_id") h (cadr layout) width 0.0 rows 1)
  (if (= oid "") layout
    (progn
      (setq rows (if (= (bht:get rec "nhom") "BIEN_BAO") (max 1 (length (bht:kh-sign-codes rec)))
                    (if (= (bht:kh-label oid rec) "") 0 1)))
      (foreach value (append (if (= (bht:get rec "nhom") "BIEN_BAO")
                              (mapcar '(lambda (v) (bht:kh-code-label "BIEN_BAO" v)) (bht:kh-sign-codes rec))
                              (list (bht:kh-label oid rec))) (list (bht:get rec "ly_trinh_km")))
        (if (/= (bht:trim value) "")
          (progn
            (setq tb (textbox (list '(0 . "TEXT") (cons 1 (bht:cad-text value (bht:kh-label-style value)))
                                    (cons 7 (bht:kh-label-style value)) (cons 40 h))))
            (if tb (setq width (max width (- (caadr tb) (caar tb))))))))
      (if (/= (bht:get rec "ly_trinh_km") "") (setq rows (1+ rows)))
      (setq gap (max (* 0.6 h) (* 0.25 (cadr anchor))) center (bht:kh-caption-local (car layout) anchor))
      (foreach e (cdr (assoc oid *bht-caption-leaders*))
        (if (entget e)
          (progn
            (setq d (entget e) verts (if (= (cdr (assoc 0 d)) "LINE")
              (list (cdr (assoc 10 d)) (cdr (assoc 11 d)))
              (mapcar '(lambda (v) (list (car v) (cadr v) (cdr (assoc 38 d))))
                      (mapcar 'cdr (vl-remove-if-not '(lambda (v) (= (car v) 10)) d)))))
            (while (cdr verts)
              (setq segments (cons (list (bht:kh-caption-local (car verts) anchor) (bht:kh-caption-local (cadr verts) anchor)) segments)
                    verts (cdr verts))))))
      ;; Test center first; translate the complete stack together if its own leader crosses it.
      (setq candidates (list 0.0 (+ (* 0.5 width) gap) (- (+ (* 0.5 width) gap))
                              (+ width (* 2 gap)) (- (+ width (* 2 gap))) (+ (* 2 width) (* 3 gap)) (- (+ (* 2 width) (* 3 gap)))))
      (foreach candidate candidates
        (if (null found)
          (progn
            (setq box (list (- (+ (car center) candidate) (* 0.5 width) gap)
                            (- (cadr center) (* rows 1.5 h) gap)
                            (+ (car center) candidate (* 0.5 width) gap) (+ (cadr center) gap)))
            (if (not (vl-some '(lambda (seg) (bht:segment-box-hit (car seg) (cadr seg) box)) segments))
              (setq found (list candidate))))))
      (if found (list (bht:label-local (car layout) (car found) 0.0 (caddr anchor)) h) layout))))

(defun bht:kh-sign-label-layout (blk anchor / bounds sc rot pos center height y layout h rows rec)
  (setq sc (cadr anchor) rot (caddr anchor) pos (car anchor)
        bounds (if (bht:fn-defined-p 'BHTSIGNBOUNDS)
                 (vl-catch-all-apply 'BHTSIGNBOUNDS (list blk)) nil))
  (if (or (vl-catch-all-error-p bounds) (not (listp bounds)) (/= (length bounds) 4))
    (setq bounds '(-0.9 -0.06 0.9 2.4)))
  (setq center (* 0.5 (+ (nth 0 bounds) (nth 2 bounds)))
        height (- (nth 3 bounds) (nth 1 bounds))
        h (* 0.15 (min height 2.46) (bht:sign-label-scale sc))
        y (- (nth 1 bounds) (max (* 0.08 height) 0.45)) rec *bht-caption-rec*)
  ;; Put the whole caption above the face, centered on its actual bounds.
  ;; The leader leaves the foot on the opposite side, so routine placement
  ;; no longer needs the large sideways shift previously used to avoid it.
  (if (bht:kh-fillable-p rec)
    (progn
      (setq rows (if (= (bht:kh-label (bht:get rec "object_id") rec) "") 0
                   (if (= (bht:get rec "nhom") "BIEN_BAO") (max 1 (length (bht:kh-sign-codes rec))) 1)))
      (if (/= (bht:get rec "ly_trinh_km") "") (setq rows (1+ rows)))
      (setq y (+ (nth 3 bounds) (max (* 0.08 height) 0.45) (/ (* h (+ 1.0 (* 1.5 (max 0 (1- rows))))) sc)))))
  (setq layout (list
    (list (+ (car pos) (* sc (- (* center (cos rot)) (* y (sin rot)))))
          (+ (cadr pos) (* sc (+ (* center (sin rot)) (* y (cos rot))))) (caddr pos))
    h))
  (if (or (bht:kh-fillable-p rec) (bht:kh-light-p rec))
    (bht:kh-caption-leader-layout layout anchor)
    (append (bht:kh-caption-clear layout anchor) (list (bht:kh-readable-angle rot)))))
(defun bht:sign-layers (/ e d)
  (bht:layer "BHT_KYHIEU" 7)
  (if (and (setq e (tblobjname "LAYER" "BHT_KYHIEU")) (= (cdr (assoc 62 (setq d (entget e)))) 1))
    (entmod (bht:dxf-put d 62 7))))

(defun bht:kh-custom-key (group) (strcat "kh_block_" (strcase group T)))
(defun bht:kh-custom-src-key (group) (strcat "kh_block_src_" (strcase group T)))
(defun bht:kh-custom-factor-key (group) (strcat "kh_block_factor_" (strcase group T)))

(defun bht:kh-default-block (group code)
  (cond ((= group "BIEN_BAO") (bht:bb-block-for code))
        ((= group "COC_TIEU") "BHT_KH_COC_TIEU")
        ((= group "COT_KM") "BHT_KH_COT_KM_V0611")
        ((member group '("DEN_CS" "DEN_TH")) "BHT_KH_DEN")
        (T (strcat "BHT_KH_" group)))
)

;; Nap 1 DWG ngoai thanh 1 dinh nghia block rieng BHT_USER_<NHOM>.
;; Chen tham chieu tam bang ActiveX roi xoa tham chieu; dinh nghia block van
;; duoc luu trong DWG hien tai. Ban ve nguon khong bi sua.
(defun bht:block-load-vla (tmp / app doc ms br name err)
  (setq app (vl-catch-all-apply 'vlax-get-acad-object nil)
        doc (if (vl-catch-all-error-p app) nil
              (vl-catch-all-apply 'vla-get-ActiveDocument (list app))))
  (if (or (null doc) (vl-catch-all-error-p doc))
    'NO_ACTIVE_DOCUMENT
    (progn
      (setq ms (vla-get-ModelSpace doc)
            br (vl-catch-all-apply 'vla-InsertBlock
                 (list ms (vlax-3d-point '(0.0 0.0 0.0)) tmp 1.0 1.0 1.0 0.0)))
      (if (vl-catch-all-error-p br)
        (progn (setq *bht-block-load-error* (vl-catch-all-error-message br)) nil)
        (progn
          (setq name (vla-get-Name br)
                *bht-block-load-factor* (abs (vla-get-XScaleFactor br))
                err (vl-catch-all-apply 'vla-Delete (list br)))
          (vlax-release-object br)
          (if (vl-catch-all-error-p err)
            (progn (setq *bht-block-load-error* (vl-catch-all-error-message err)) nil)
            name)))))
)

(defun bht:block-load-cmd (tmp group / name err e d)
  (setq name (strcat "BHT_USER_" group)
        err (vl-catch-all-apply
              '(lambda ()
                 (vl-cmdf "_.-INSERT" (strcat name "=" tmp) '(0.0 0.0 0.0) 1.0 1.0 0.0)
                 (entlast)) nil))
  (cond
    ((vl-catch-all-error-p err)
     (setq *bht-block-load-error* (vl-catch-all-error-message err)) nil)
    ((not (and (setq e err) (setq d (entget e)) (= (cdr (assoc 0 d)) "INSERT")))
     (setq *bht-block-load-error* "-INSERT không tạo được tham chiếu block tạm") nil)
    (T
     (setq name (cdr (assoc 2 d))
           *bht-block-load-factor* (abs (cdr (assoc 41 d))))
     (entdel e)
     name))
)

(defun bht:block-load-dwg (path group / tmp name)
  (setq *bht-block-load-error* "" *bht-block-load-factor* 1.0
        tmp (strcat (getvar "TEMPPREFIX") "BHT_USER_" group ".dwg"))
  (if (findfile tmp) (vl-file-delete tmp))
  (if (not (vl-file-copy path tmp))
    (progn (setq *bht-block-load-error* "không sao chép được DWG vào thư mục tạm") nil)
    (progn
      (setq name (bht:block-load-vla tmp))
      (if (= name 'NO_ACTIVE_DOCUMENT) (setq name (bht:block-load-cmd tmp group)))
      (vl-file-delete tmp)
      name))
)

(defun bht:kh-group-oids (group / out rec)
  (setq out nil)
  (foreach oid (bht:obj-ids)
    (setq rec (bht:obj-read oid))
    (if (= (bht:get rec "nhom") group) (setq out (cons oid out))))
  (reverse out)
)

(defun bht:effective-sign-code (rec / result)
  (if (bht:fn-defined-p 'BHTNATIVESIGNCODE)
    (setq result (vl-catch-all-apply 'BHTNATIVESIGNCODE (list (bht:get rec "ma_hieu") (bht:get rec "mo_ta")))))
  (if (= (type result) 'STR) result (bht:get rec "ma_hieu")))
(defun bht:kh-missing-face-p (rec)
  (and (= (bht:get rec "nhom") "BIEN_BAO") (= (bht:get rec "tinh_trang") "Mất mặt biển, còn trụ")))
(defun bht:kh-missing-block (rec / n i x shapes name half caption box width)
  (setq n (max 1 (min 100 (bht:int (bht:get rec "so_tru")))) i 0 shapes nil
        name (strcat "BHT_MISSING_FACE_V0660_" (itoa n))
        half (max 1.2 (+ 0.3 (* 0.35 (- n 1)))))
  ;; A framed notice uses the same foot origin and post spacing as the old symbol.
  (repeat n
    (setq x (* 0.7 (- i (/ (- n 1) 2.0)))
          shapes (append shapes (list (bht:line-g (list x 0.0 0.0) (list x 0.6 0.0))
             (append (bht:circle-g (list x 0.0 0.0) 0.06) '((60 . 1))))) i (1+ i)))
  (setq caption (bht:text-center-color-g "TRỤ MẤT BIỂN" '(0.0 1.2 0.0) 0.22 7)
        box (textbox caption)
        width (if box (abs (- (caadr box) (caar box))) 0.0))
  (if (> width (- (* 2.0 half) 0.24))
    (setq caption (bht:dxf-put caption 40 (* 0.22 (/ (- (* 2.0 half) 0.24) width)))))
  ;; Blue background first, then an inset white frame and caption.
  (setq shapes (append shapes
    (list (append (bht:solid-g (list (- half) 0.6 0.0) (list half 0.6 0.0)
                              (list (- half) 1.8 0.0) (list half 1.8 0.0) 5)
                  (list (cons 420 30398))))
    (bht:poly-color-g (list (list (+ (- half) 0.06) 0.66 0.0) (list (- half 0.06) 0.66 0.0)
                           (list (- half 0.06) 1.74 0.0) (list (+ (- half) 0.06) 1.74 0.0)) 7)
    (list caption)))
  (bht:block name shapes)
  name)
(defun bht:kh-base-block (rec / blk custom own codes count blocks value result poles layout idx gap clearance)
  (bht:symbol-blocks)
  (setq own (bht:get rec "custom_block")
        custom (bht:meta (bht:kh-custom-key (bht:get rec "nhom")) ""))
  (if (and (member (bht:get rec "nhom") '("DEN" "DEN_TH" "DEN_CS"))
           (wcmatch own "*BHT_LIGHT_*") (bht:fn-defined-p 'BHTCURRENTLIGHT))
    (setq own (BHTCURRENTLIGHT own)))
  (cond
    ((bht:kh-missing-face-p rec) (bht:kh-missing-block rec))
    ((and (/= own "") (tblsearch "BLOCK" own)) own)
    ((and (/= custom "") (tblsearch "BLOCK" custom)) custom)
    (T
      (setq blk (if (= (bht:get rec "nhom") "BIEN_BAO")
                    (if (wcmatch (strcase (bht:get rec "ma_hieu")) "BHT.TEXT.*")
                      (BHTCUSTOMPLATE (bht:get rec "ma_hieu") 0 (bht:get rec "sign_content"))
                      (bht:tdt-import-block (bht:get rec "ma_hieu") (bht:get rec "mo_ta"))) nil))
      (if (and (= (strcase (bht:get rec "ma_hieu")) "I.439") (setq result (bht:bridge-sign-block rec))) (setq blk result))
      (if (and (= (bht:get rec "nhom") "COT_KM") (/= (bht:get rec "marker_km") "") (bht:fn-defined-p 'BHTMILESTONE))
        (setq blk (BHTMILESTONE (bht:get rec "marker_km"))))
      (if (member (bht:get rec "nhom") '("BANG_QC" "KHAC" "CHUA_XAC_DINH"))
        (setq blk (bht:named-board-block rec)))
      (if (null blk) (setq blk (bht:kh-default-block (bht:get rec "nhom") (bht:effective-sign-code rec))))
      (if (= (bht:get rec "nhom") "BIEN_BAO")
        (progn
          (setq codes (bht:get-all rec "mat") count (bht:int (bht:get rec "so_mat"))
                layout (bht:get rec "sign_layout"))
          (if (= layout "") (setq layout "LEGACY"))
          (if (null codes) (setq codes (list (bht:get rec "ma_hieu"))))
          (if (or (> count 20) (> (length codes) 20)) (vl-exit-with-error "Một trụ hỗ trợ tối đa 20 mặt biển."))
          (while (and (< (length codes) count) (< (length codes) 20)) (setq codes (append codes (list (car codes)))))
          (if (or (> (length codes) 1) (/= (bht:get rec "sign_content") "") (/= layout "LEGACY"))
            (progn
              (setq blocks nil idx 0)
              (foreach value codes
                (setq result (if (= (length codes) 1) blk
                  (if (= (strcase value) "I.439") (bht:bridge-sign-block rec) nil)))
                (if (wcmatch (strcase value) "BHT.TEXT.*")
                  (setq result (BHTCUSTOMPLATE value idx (bht:get rec "sign_content"))))
                (if (null result) (setq result (bht:tdt-import-block value (bht:get rec "mo_ta"))))
                (if (null result) (setq result (bht:kh-default-block "BIEN_BAO" value)))
                (if (and (not (wcmatch (strcase value) "BHT.TEXT.*")) (/= (bht:get rec "sign_content") ""))
                  (progn
                    (if (not (bht:fn-defined-p 'BHTSIGNCONTENT)) (vl-exit-with-error "Nạp BHT.Bridge.dll để áp dụng nội dung biển."))
                    (setq result (BHTSIGNCONTENT result value idx (bht:get rec "sign_content")))))
                (setq blocks (append blocks (list result)) idx (1+ idx)))
              (if (/= layout "LEGACY")
                (progn
                  (if (not (bht:fn-defined-p 'BHTSIGNLAYOUT)) (vl-exit-with-error "Nạp BHT.Bridge.dll để bố trí CAP1."))
                  (setq gap (if (= (bht:get rec "sign_gap") "") 0.2 (bht:num (bht:get rec "sign_gap")))
                        clearance (if (= (bht:get rec "sign_clearance") "") 0.6 (bht:num (bht:get rec "sign_clearance"))))
                  (setq result (BHTSIGNLAYOUT (bht:join blocks ";") layout gap clearance)))
                (progn
                  (if (not (bht:fn-defined-p 'BHTSIGNASSEMBLY)) (vl-exit-with-error "Nạp BHT.Bridge.dll để chèn nhiều mặt."))
                  (setq result (BHTSIGNASSEMBLY (bht:join blocks ";")))))
              (if (and (= (type result) 'STR) (tblsearch "BLOCK" result)) (setq blk result)
                (vl-exit-with-error "Không tạo được cụm biển. Kiểm tra nội dung và bố trí CAP1."))))))
      (if (and (member (bht:get rec "nhom") '("BIEN_BAO" "BANG_QC" "KHAC" "CHUA_XAC_DINH"))
               (or (null layout) (= layout "LEGACY"))
               (setq poles (bht:int (bht:get rec "so_tru"))) (/= poles 1))
        (progn
          (if (not (bht:fn-defined-p 'BHTSIGNPOSTS))
            (vl-exit-with-error "Nạp BHT.Bridge.dll bản mới để vẽ đúng số trụ/chân."))
          (setq result (vl-catch-all-apply 'BHTSIGNPOSTS (list blk poles)))
          (if (and (= (type result) 'STR) (tblsearch "BLOCK" result)) (setq blk result)
            (vl-exit-with-error "Không tạo được số trụ/chân đã nhập (0–100). Kiểm tra BHT.Bridge.dll."))))
      (if (tblsearch "BLOCK" blk) blk "BHT_KH_CHUA_XAC_DINH"))))

(defun bht:named-board-block (rec / title)
  (if (not (bht:fn-defined-p 'BHTNAMEDBOARD))
    (vl-exit-with-error "Nạp BHT.Bridge.dll bản mới trước khi chèn bảng tên."))
  (setq title (if (= (bht:get rec "nhom") "CHUA_XAC_DINH") "Chưa xác định" (vl-string-trim " " (bht:get rec "mo_ta"))))
  (if (= title "") (setq title (bht:group-label (bht:get rec "nhom"))))
  (BHTNAMEDBOARD title (bht:kh-label-style title)))

(defun bht:bridge-sign-block (rec / name station road style)
  (if (and (/= (bht:get rec "bridge_name") "") (bht:fn-defined-p 'BHTBRIDGESIGN))
    (progn
      (setq name (bht:get rec "bridge_name") station (bht:get rec "sign_chainage") road (bht:get rec "road_name")
            style (bht:kh-label-style (strcat name station road)))
      (BHTBRIDGESIGN name station road style))))

;; All displayed sign/board groups share the drawing sign scale.
(defun bht:kh-sign-scale-p (rec)
  (member (bht:get rec "nhom") '("BIEN_BAO" "BANG_CHI_DAN" "BANG_QC" "KHAC" "CHUA_XAC_DINH" "DEN" "DEN_CS" "DEN_TH")))
(defun bht:kh-fillable-p (rec)
  (member (bht:get rec "nhom") '("BIEN_BAO" "BANG_CHI_DAN" "BANG_QC" "KHAC" "CHUA_XAC_DINH")))

(defun bht:kh-block (rec / blk result)
  (setq blk (bht:kh-base-block rec))
  (if (and (bht:kh-fillable-p rec) (= (bht:meta "sign_fill_all" "1") "0"))
    (progn
      (setq result (if (bht:fn-defined-p 'BHTSIGNOUTLINE)
                     (vl-catch-all-apply 'BHTSIGNOUTLINE (list blk)) nil))
      (if (or (vl-catch-all-error-p result) (/= (type result) 'STR))
        (bht:msg "Không tạo được biển không tô màu. Kiểm tra BHT.Bridge.dll. Đang giữ block tô màu.")
        (setq blk result))))
  (if (and (= (bht:get rec "nhom") "BIEN_BAO") (= (bht:get rec "custom_block") "")
           (bht:fn-defined-p 'BHTSIGNPORTABLE))
    (setq blk (BHTSIGNPORTABLE blk)))
  blk)

(defun bht:kh-light-p (rec)
  (member (bht:get rec "nhom") '("DEN" "DEN_CS" "DEN_TH")))

;; Presentation multiplier only; never changes surveyed coordinates or source DWGs.
(defun bht:kh-light-factor (rec / block factor)
  (if (bht:kh-light-p rec)
    (progn
      (setq block (bht:kh-base-block rec))
      (if (not (bht:fn-defined-p 'BHTLIGHTFACTOR))
        (vl-exit-with-error "Nạp BHT.Bridge cùng phiên bản để chuẩn hóa kích thước đèn."))
      (setq factor (BHTLIGHTFACTOR block))
      (if (and factor (numberp factor) (> factor 0.0)) factor
        (vl-exit-with-error "Không xác định được kích thước block đèn.")))
    1.0))

(defun bht:kh-scale-for (rec / custom f)
  (setq custom (bht:meta (bht:kh-custom-key (bht:get rec "nhom")) "")
        f (bht:num (bht:meta (bht:kh-custom-factor-key (bht:get rec "nhom")) "1")))
  (* (if (bht:kh-sign-scale-p rec) (bht:sign-scale) (bht:kh-scale))
     (bht:kh-light-factor rec)
     (if (and (= (bht:get rec "custom_block") "") (/= custom "") f (> f 0.0)) f 1.0))
)

(defun bht:all-zero-p (s / ok i)
  (setq ok T i 1)
  (while (and ok (<= i (strlen s)))
    (if (/= (substr s i 1) "0") (setq ok nil))
    (setq i (1+ i)))
  ok
)

(defun bht:kh-code-label (group code / prefix value upper dot decimals)
  (setq value (vl-string-trim " " (bht:str code))
        prefix (strcat (strcase group) "_")
        upper (strcase value))
  (if (wcmatch upper "BHT.TEXT.*") (setq value "Biển tùy chỉnh" upper (strcase value)))
  ;; Ma luu co the la COC_TIEU_KM48+500; bo tien to noi bo khoi nhan ban ve.
  (if (and (>= (strlen upper) (strlen prefix)) (= (substr upper 1 (strlen prefix)) prefix))
    (setq value (substr value (1+ (strlen prefix))) upper (strcase value)))
  (if (and (= group "BIEN_BAO") (setq dot (vl-string-search "@" value)))
    (setq value (strcat (substr value 1 dot) " (" (substr value (+ dot 2)) (cond ((vl-string-search ":" value) ")") ((= (strcase (substr value 1 dot)) "S.505A") "T)") (T " m)")))))
  (if (= (substr upper 1 (min 2 (strlen upper))) "KM")
    (progn
      (setq value (vl-string-trim " " (substr value 3))
            dot (vl-string-search "." value))
      (if dot
        (progn
          (setq decimals (substr value (+ dot 2)))
          (if (bht:all-zero-p decimals) (setq value (substr value 1 dot)))))
      (strcat "Km " value))
    value)
)

(defun bht:kh-sign-codes (rec / codes count)
  (setq codes (vl-remove-if '(lambda (code) (= (vl-string-trim " " code) "")) (bht:get-all rec "mat"))
        count (bht:int (bht:get rec "so_mat")))
  (setq count (if (numberp count) (min 20 count) 0))
  (if (null codes) (setq codes (list (bht:effective-sign-code rec))))
  (while (< (length codes) count) (setq codes (append codes (list (car codes)))))
  codes)

(defun bht:kh-label (oid rec / group code chainage detail)
  ;; Nhan hien thi la ten nghiep vu + ma/ly trinh; object_id chi giu trong XData.
  (setq group (bht:get rec "nhom")
        code (bht:get rec "ma_hieu")
        chainage (bht:get rec "ly_trinh_km")
        code (if (= group "BIEN_BAO") (car (bht:kh-sign-codes rec)) code)
        detail (cond ((/= code "") (bht:kh-code-label group code))
                     ((and (/= chainage "") (not (bht:kh-fillable-p rec))) (bht:kh-code-label group chainage))
                     (T "")))
  (cond
    ((bht:kh-missing-face-p rec) "")
    ((or (= group "COT_KM") (bht:kh-light-p rec)) "")
    ((and (member group '("BANG_QC" "KHAC" "CHUA_XAC_DINH"))
          (= (bht:get rec "custom_block") "")
          (= (bht:meta (bht:kh-custom-key group) "") "")) "")
    ((and (= group "COC_TIEU") (/= (bht:get rec "marker_km") "") (/= (bht:get rec "marker_h") ""))
      (strcat "H" (bht:get rec "marker_h") "/" (bht:get rec "marker_km")))
    ((and (= group "COC_TIEU") (= chainage "")) "Cọc tiêu")
    ((= group "BIEN_BAO")
      (if (/= code "") (bht:kh-code-label group code) "Biển báo"))
    (T (strcat (bht:group-label group) (if (/= detail "") (strcat " " detail) "")))))

;; Direction describes the visible head, not an arbitrary block X axis.
;; Custom blocks keep their own +X convention.
(defun bht:kh-picked-rotation (rec target direction-point / blk heading)
  (setq blk (bht:kh-base-block rec)
        heading (cond ((member blk '("BHT_KH_COC_TIEU" "BHT_KH_COT_KM_V043" "BHT_KH_COT_KM_V0611")) pi)
                      ((wcmatch blk "BHT_KH_COT_KM_V0611_*") pi)
                      ((or (wcmatch blk "BHT_BOARD_V*,BHT_LIGHT_*,BHT_MISSING_FACE_*")
                           (and (member (bht:get rec "nhom") '("BANG_QC" "KHAC" "CHUA_XAC_DINH"))
                                (wcmatch blk "BHT_SUPPORT_*"))) (/ pi 2.0))
                      ((and (= (bht:get rec "nhom") "BIEN_BAO")
                            (wcmatch blk "BHT_ADS_*,BHT_TDT_*,BHT_I439_*,BHT_KH_BB_*,BHT_KH_BIEN_BAO_*,BHT_SIGN_*,BHT_ASSEMBLY_*,BHT_SUPPORT_*")) (/ pi 2.0))
                      (T 0.0)))
  (- (atan (- (cadr direction-point) (cadr target)) (- (car direction-point) (car target))) heading))

;; Vi tri/goc ky hieu tu dong theo tim: bien bao duoc day ra ngoai phia duong,
;; giu diem RTK lam moc that va noi bang duong dan. Cac nhom khac dat dung tam diem.
(defun bht:kh-route-transform (rec pos sc / rid rrec ent pr cp par der dl off ux uy side gap rot uniform)
  (if (or (null pos) (null rec))
    (list pos 0.0)
    (progn
      (setq rid (bht:get rec "route_id") rrec (if (/= rid "") (bht:route-read rid) nil)
            ent (if rrec (bht:route-ent rrec) nil) pr (if ent (bht:curve-project ent pos) nil)
            side (strcase (if (/= (bht:get rec "phia_tuyen") "") (bht:get rec "phia_tuyen") (bht:get rec "phia_duong")))
            gap (if (= (bht:get rec "nhom") "BIEN_BAO")
                  (cond ((bht:num (bht:meta "symbol_gap" ""))) (T (* 2.8 sc))) 0.0))
      (setq uniform (if (bht:num (bht:meta "symbol_gap" "")) (/ gap (sqrt 2.0)) gap))
      (if pr
        (progn
          (setq cp (cadddr pr) par (vl-catch-all-apply 'vlax-curve-getParamAtPoint (list ent cp))
                der (if (and par (not (vl-catch-all-error-p par)))
                      (vl-catch-all-apply 'vlax-curve-getFirstDeriv (list ent par)) nil))
          (if (or (null der) (vl-catch-all-error-p der))
            (list pos 0.0)
            (progn
              (setq dl (distance '(0.0 0.0 0.0) (list (car der) (cadr der) 0.0))
                    off (distance (list (car cp) (cadr cp)) (list (car pos) (cadr pos))))
              (if (< dl 1e-9)
                (list pos 0.0)
                (progn
                  (if (= (bht:int (bht:get rrec "direction")) -1)
                    (setq der (mapcar '- der)))
                  (if (not (member side '("TRAI" "PHAI")))
                    (setq side (if (> (* (if (= (bht:int (bht:get rrec "direction")) -1) -1 1) (caddr pr)) 0) "TRAI" "PHAI")))
                  (setq rot (+ (atan (cadr der) (car der))
                               (if (= side "TRAI") (- (/ pi 2.0)) (/ pi 2.0))))
                  (if (> off 0.05)
                    (setq ux (/ (- (car pos) (car cp)) off) uy (/ (- (cadr pos) (cadr cp)) off))
                    (if (= side "TRAI")
                      (setq ux (/ (- (cadr der)) dl) uy (/ (car der) dl))
                      (setq ux (/ (cadr der) dl) uy (/ (- (car der)) dl))))
                  (list (list (+ (car pos) (* gap ux)) (+ (cadr pos) (* gap uy)) (caddr pos)) rot))))))
        ;; Chua co tim: van tach bien khoi dau diem theo phia da khai bao.
        (if (= side "TRAI")
          (list (list (- (car pos) uniform) (+ (cadr pos) uniform) (caddr pos)) 0.0)
          (list (list (+ (car pos) uniform) (+ (cadr pos) uniform) (caddr pos)) 0.0))))))

;; Free placement overrides only display; RTK coordinates and route links remain unchanged.
(defun bht:kh-free-transform (rec / x y z r)
  (if (and (= (bht:get rec "kh_mode") "TU_DO")
           (setq x (bht:num (bht:get rec "kh_free_x")))
           (setq y (bht:num (bht:get rec "kh_free_y")))
           (setq z (bht:num (bht:get rec "kh_free_z")))
           (setq r (bht:num (bht:get rec "kh_free_rot"))))
    (list (list x y z) r)))
(defun bht:kh-auto-transform (rec pos sc / free result heading)
  ;; Sign size is independent of its offset from the surveyed point.
  (if (bht:kh-sign-scale-p rec) (setq sc (* (/ sc (bht:sign-scale)) (bht:kh-scale))))
  (setq sc (/ sc (bht:kh-light-factor rec)))
  (setq result (if (setq free (bht:kh-free-transform rec)) free (bht:kh-route-transform rec pos sc)))
  (if (= (bht:get rec "nhom") "COC_TIEU")
    (setq result (list pos (cond ((bht:kh-marker-angle rec pos))
                                ((bht:num (bht:get rec "marker_display_rotation"))) ((cadr result)) (T 0.0)))))
  (if (and result (setq heading (bht:kh-orientation-angle rec pos))) (list (car result) heading) result))
(defun bht:kh-via-points (rec / out value nums)
  (setq out nil)
  (if (= (bht:get rec "kh_mode") "TU_DO")
    (foreach value (bht:get-all rec "kh_via")
      (setq nums (mapcar 'bht:num (bht:split value ",")))
      (if (and (= (length nums) 3) (vl-every 'numberp nums)) (setq out (append out (list nums))))))
  out)
(defun bht:kh-free-clear (rec)
  (vl-remove-if '(lambda (pair) (member (car pair) '("kh_mode" "kh_free_x" "kh_free_y" "kh_free_z" "kh_free_rot" "kh_via" "sign_heading_mode" "sign_heading_route" "sign_heading_angle"))) rec))
;; Called after every input succeeds. No drawing changes during interactive picking.
(defun bht:kh-free-apply (oid target rot via / rec d pair pt)
  (setq rec (bht:obj-read oid))
  (if (and rec target (numberp rot))
    (progn
      (setq rec (bht:kh-free-clear rec))
      (foreach pair (list (cons "kh_mode" "TU_DO") (cons "kh_free_x" (bht:fnum (car target) 8))
        (cons "kh_free_y" (bht:fnum (cadr target) 8)) (cons "kh_free_z" (bht:fnum (caddr target) 8))
        (cons "kh_free_rot" (bht:fnum rot 8))) (setq rec (append rec (list pair))))
      (foreach pt via (setq rec (append rec (list (cons "kh_via" (strcat (bht:fnum (car pt) 8) "," (bht:fnum (cadr pt) 8) "," (bht:fnum (caddr pt) 8)))))))
      (if (and (boundp '*bht-placement-leader-mode*) *bht-placement-leader-mode*)
        (setq rec (bht:set rec "kh_leader_mode" *bht-placement-leader-mode*)))
      (bht:obj-write oid rec)
      ;; Explicit placement replaces an earlier manual INSERT position/heading-angle.
      (foreach pr (bht:tagged-pairs "INSERT" "BHT_KH" 1)
        (if (= (strcase (car pr)) (strcase oid))
          (progn (setq d (entget (cdr pr)))
            (entmod (append d (list (bht:kh-xdata-ins oid "TU_DONG" (cdr (assoc 10 d)) (cdr (assoc 50 d)) (cdr (assoc 41 d)))))))))
      (bht:symbol-sync (list oid)))))
(defun bht:kh-free-apply-mode (oid target rot via mode / *bht-placement-leader-mode*)
  (setq *bht-placement-leader-mode* mode)
  (bht:kh-free-apply oid target rot via))
(defun bht:kh-pick-target (source cursor / picked done prompt)
  (setq done nil prompt "\nVị trí ký hiệu [Goc/Tuychon] (G = tâm X gốc, T = chọn vị trí; Enter = hủy): ")
  (while (not done)
    (initget "Goc Tuychon")
    (setq picked (getpoint cursor prompt))
    (cond
      ((= picked "Goc") (setq picked (trans source 0 1) done T))
      ((= picked "Tuychon") (setq prompt "\nChọn vị trí tùy ý [Goc/Tuychon] (Enter = hủy): "))
      (T (setq done T))))
  picked)

(defun bht:kh-place-free (oid / rec source base angle0 rotation picked via cursor done target dir at-origin)
  (setq oid (strcase (bht:trim (bht:str oid))) rec (bht:obj-read oid))
  (cond
    ((null rec) (list 'LOI "Không có hồ sơ này."))
    ((= (bht:get rec "nhom") "COC_TIEU") (bht:kh-marker-place-request oid))
    ((null (setq source (bht:obj-position rec (bht:pt-all)))) (list 'LOI "Hồ sơ chưa có điểm RTK hợp lệ."))
    (T
      (setq base (trans source 0 1) via nil cursor base done nil)
      (setq angle0 (getangle base "\nHướng ký hiệu (chỉ hướng hoặc nhập góc) <0>: "))
      (if (null angle0) (setq angle0 0.0))
      ;; UCS heading-angle -> WCS block rotation, without ANGBASE/ANGDIR assumptions.
      (setq dir (trans (list (cos angle0) (sin angle0) 0.0) 1 0 T) rotation (bht:kh-picked-rotation rec source (mapcar '+ source dir)))
      (while (not done)
        (initget "Dat Xoa Goc Tuychon")
        (setq picked (getpoint cursor "\nĐiểm trung gian hoặc [Dat/Xoa/Goc/Tuychon] (G = tâm X, T = vị trí tùy ý) <Dat>: "))
        (cond
          ((or (null picked) (= picked "Dat")) (setq done T))
          ((= picked "Goc") (setq done T at-origin T via nil))
          ((= picked "Tuychon") (setq done T))
          ((= picked "Xoa") (if via (setq via (reverse (cdr (reverse via))))) (setq cursor (if via (trans (last via) 0 1) base)))
          ((listp picked) (setq via (append via (list (trans picked 1 0))) cursor picked))))
      (if (setq target (if at-origin base (bht:kh-pick-target source cursor)))
        (progn (setq target (trans target 1 0))
          (if (equal target source 1e-8) (setq via nil))
          (bht:kh-free-apply-mode oid target rotation via "WAYPOINT"))
        (list 'HUY)))))
(defun bht:kh-place-options (oid mode direction degrees / rec source target rot via route picked dir ang cursor done at-origin rid heading result)
  (setq oid (strcase (bht:trim (bht:str oid))) rec (bht:obj-read oid))
  (cond
    ((null rec) (list 'LOI "Không có hồ sơ này."))
    ((= (bht:get rec "nhom") "COC_TIEU") (bht:kh-marker-place-request oid))
    ((not (member mode '("DIRECT" "ELBOW" "WAYPOINT"))) (list 'LOI "Chế độ đặt ký hiệu không hợp lệ."))
    ((not (member direction '("HORIZONTAL" "ROUTE" "ROUTE_PERP" "PICK" "ANGLE"))) (list 'LOI "Hướng ký hiệu không hợp lệ."))
    ((and (= direction "ANGLE") (null (bht:num degrees))) (list 'LOI "Góc nhập không hợp lệ."))
    ((null (setq source (bht:obj-position rec (bht:pt-all)))) (list 'LOI "Hồ sơ chưa có điểm RTK hợp lệ."))
    ((and (= direction "ROUTE_PERP")
       (progn
         (setq rid (bht:get rec "sign_heading_route"))
         (if (not (bht:route-read rid)) (setq rid (bht:get rec "route_id")))
         (if (not (bht:route-read rid)) (setq rid (bht:ask-route)))
         (setq heading (if rid (bht:kh-route-heading rec rid source) nil))
         (null heading)))
      (list 'LOI "Không xác định được hướng tuyến tại điểm RTK. Kiểm tra tuyến đã chọn, hình học và khoảng cách tối đa; chưa thay đổi biển."))
    (T
      (setq via nil cursor (trans source 0 1) done nil)
      (if (= mode "WAYPOINT")
        (while (not done)
          (initget "Dat Xoa Goc Tuychon")
          (setq picked (getpoint cursor "\nĐiểm trung gian hoặc [Dat/Xoa/Goc/Tuychon] (G = tâm X, T = vị trí tùy ý) <Dat>: "))
          (cond
            ((or (null picked) (= picked "Dat")) (setq done T))
            ((= picked "Goc") (setq done T at-origin T via nil))
            ((= picked "Tuychon") (setq done T))
            ((= picked "Xoa") (if via (setq via (reverse (cdr (reverse via))))) (setq cursor (if via (trans (last via) 0 1) (trans source 0 1))))
            (T (setq via (append via (list (trans picked 1 0))) cursor picked)))))
      (if (null (setq picked (if at-origin (trans source 0 1) (bht:kh-pick-target source cursor)))) (list 'HUY)
        (progn
          (setq target (trans picked 1 0) rot 0.0 ang nil)
          (if (equal target source 1e-8) (setq via nil))
          (cond
            ((= direction "ROUTE") (setq route (bht:kh-route-transform rec source (bht:kh-scale)) rot (if route (cadr route) 0.0)))
            ((= direction "ROUTE_PERP") (setq rot (+ heading (/ pi 2.0))))
            ((= direction "ANGLE") (setq rot (* pi (/ (bht:num degrees) 180.0))))
            ((= direction "PICK")
              (setq ang (getpoint picked "\nChọn hướng đầu ký hiệu (Enter = hủy): "))
              (if (and ang (not (equal picked ang 1e-8)))
                (setq rot (bht:kh-picked-rotation rec target (trans ang 1 0))) (setq ang nil))))
          (if (and (= direction "PICK") (null ang)) (list 'HUY)
            (progn
              (if (/= mode "WAYPOINT") (setq via nil))
              (setq result (bht:kh-free-apply-mode oid target rot via
                (cond ((= mode "DIRECT") "THANG") ((= mode "ELBOW") "GAP_KHUC") (T "WAYPOINT"))))
              (if (and (= direction "ROUTE_PERP") (= (bht:get rec "nhom") "BIEN_BAO"))
                (progn
                  (setq rec (bht:obj-read oid)
                        rec (bht:set rec "sign_heading_mode" "ROUTE_PERP")
                        rec (bht:set rec "sign_heading_route" rid)
                        rec (bht:set rec "sign_heading_angle" ""))
                  (bht:obj-write oid rec)))
              result)))))))
(defun c:BHTDATTUDO (/ *error* oid r)
  (setq *error* bht:on-error)
  (bht:ensure-model)
  (setq oid (bht:ask-string "ID hồ sơ cần đặt ký hiệu tự do" ""))
  (if (/= oid "")
    (progn (setq r (bht:kh-place-free oid))
      (cond ((eq (car r) 'LOI) (bht:warn (cadr r))) ((eq (car r) 'HUY) (bht:msg "Đã hủy chèn tự do."))
            (T (bht:symbol-report r)))))
  (princ))

(defun bht:kh-readable-angle (a / two)
  (setq two (* 2.0 pi))
  (while (< a 0.0) (setq a (+ a two)))
  (while (>= a two) (setq a (- a two)))
  (if (and (> a (/ pi 2.0)) (<= a (* 1.5 pi))) (- a pi) a)
)

(defun bht:kh-leader-xdata (oid)
  (list -3 (list "BHT_KH" (cons 1000 oid) (cons 1000 "DAN")))
)

;; The visible head follows the A->B tangent, independent of the road side.
(defun bht:kh-route-heading (rec route-id pos / route ent projected tangent param dir)
  (if (and pos (setq route (bht:route-read route-id)) (setq ent (bht:route-ent route))
           (setq projected (bht:route-project route ent pos))
           (<= (cadr projected) (if (bht:num (bht:get route "max_offset")) (bht:num (bht:get route "max_offset")) 100.0)))
    (progn
      (setq param (vl-catch-all-apply 'vlax-curve-getParamAtDist (list ent (nth 4 projected))))
      (if (not (vl-catch-all-error-p param))
        (progn
          (setq tangent (vl-catch-all-apply 'vlax-curve-getFirstDeriv (list ent param))
                dir (if (= (bht:int (bht:get route "direction")) -1) -1.0 1.0))
          (if (and tangent (not (vl-catch-all-error-p tangent))
                   (> (+ (abs (car tangent)) (abs (cadr tangent))) 1e-9))
            (bht:kh-picked-rotation rec '(0.0 0.0 0.0)
              (list (* dir (car tangent)) (* dir (cadr tangent)) 0.0))))))))

(defun bht:kh-orientation-angle (rec pos / mode bearing heading-angle field)
  (if (= (bht:get rec "nhom") "BIEN_BAO")
    (progn
      (setq mode (bht:get rec "sign_heading_mode"))
      (cond
        ((= mode "FIELD_PERP")
          (setq field (bht:field-result rec pos (bht:pt-all)))
          (if (bht:field-accepted rec field)
            (progn (setq bearing (cdr (assoc 'bearing field)))
              (+ (bht:kh-picked-rotation rec '(0.0 0.0 0.0) (list (cos bearing) (sin bearing) 0.0)) (/ pi 2.0)))))
        ((member mode '("ROUTE" "ROUTE_PERP"))
          (setq heading-angle (bht:kh-route-heading rec (bht:get rec "sign_heading_route") pos))
          (if heading-angle (+ heading-angle (if (= mode "ROUTE_PERP") (/ pi 2.0) 0.0))))
        ((and (member mode '("FIXED" "FIXED_PERP")) (setq bearing (bht:num (bht:get rec "sign_heading_angle"))))
          (+ (bht:kh-picked-rotation rec '(0.0 0.0 0.0) (list (cos bearing) (sin bearing) 0.0)) (if (= mode "FIXED_PERP") (/ pi 2.0) 0.0)))))))

;; Only placed signs are rotated. All transforms, survey data and waypoints are retained.
(defun bht:kh-heading-apply (ids mode route-id bearing / rec pos heading-angle valid skipped index existing oid unique *bht-field-input-snapshot*)
  (if (= mode "FIELD_PERP") (setq *bht-field-input-snapshot* (bht:field-input-snapshot (bht:field-input-rows (bht:pt-all)))))
  (setq index (bht:pt-all) existing (mapcar 'car (bht:tagged-pairs "INSERT" "BHT_KH" 1)) valid nil skipped nil)
  (if (or (not (member mode '("ROUTE" "FIXED" "ROUTE_PERP" "FIXED_PERP" "FIELD_PERP"))) (and (member mode '("FIXED" "FIXED_PERP")) (not (numberp bearing)))
          (and (member mode '("ROUTE" "ROUTE_PERP")) (not (bht:route-read route-id))))
    (list 'LOI "Chọn tuyến hợp lệ hoặc hai điểm hướng khác nhau.")
    (progn
      (foreach oid ids (setq unique (bht:unique-add unique (strcase oid))))
      (foreach oid unique
        (setq rec (bht:obj-read oid) pos (if rec (bht:obj-position rec index) nil) heading-angle nil)
        (if (and rec (= (bht:get rec "nhom") "BIEN_BAO") (member oid existing)
                 (or (= mode "FIELD_PERP") (member mode '("FIXED" "FIXED_PERP")) (= (bht:get rec "route_id") "") (= (strcase (bht:get rec "route_id")) (strcase route-id))))
          (progn
            (setq rec (bht:set (bht:set (bht:set rec "sign_heading_mode" mode) "sign_heading_route" (if (member mode '("ROUTE" "ROUTE_PERP")) route-id ""))
                          "sign_heading_angle" (if (member mode '("FIXED" "FIXED_PERP")) (bht:fnum bearing 8) ""))
                  heading-angle (bht:kh-orientation-angle rec pos))
            (if heading-angle (progn (bht:obj-write oid rec) (setq valid (cons oid valid))))))
        (if (null heading-angle) (setq skipped (cons oid skipped))))
      (if valid
        ((lambda (/ *bht-preserve-symbol-placement* *bht-apply-sign-heading*)
           (setq *bht-preserve-symbol-placement* T *bht-apply-sign-heading* T)
           (bht:symbol-sync-ex valid nil))))
      (list (cons 'rotated (length valid)) (cons 'skipped (length skipped)) (cons 'skipped_ids (bht:join (reverse skipped) ", "))))))

(defun bht:kh-heading-selection (ss / i e x ids owners)
  (setq ids nil i 0 owners (bht:pt-owner-map))
  (if ss (repeat (sslength ss)
    (setq e (ssname ss i) x (bht:xget e "BHT_KH"))
    (if (null x) (setq x (bht:xget e "BHT_SIGN_ORIGIN")))
    (if x (setq ids (bht:unique-add ids (strcase (car x))))
      (if (setq x (bht:xget e "BHT_PT")) (foreach oid (cdr (assoc (strcase (car x)) owners)) (setq ids (bht:unique-add ids oid)))))
    (setq i (1+ i))))
  ids)

(defun c:BHTHUONGBIEN (/ *error* mode scope route-id a b ids result started)
  (defun *error* (msg) (if started (command-s "_.UNDO" "_End")) (bht:on-error msg))
  (setq mode (strcase (bht:ask-string "Xoay biển vuông góc [T=Theo tuyến/H=Chọn hướng A-B]" "T")))
  (setq mode (substr mode 1 1))
  (cond
    ((= mode "T") (setq route-id (bht:ask-route) mode "ROUTE_PERP"))
    ((= mode "H")
      (if (and (setq a (getpoint "\nChọn A của hướng chung: ")) (setq b (getpoint a "\nChọn B (đầu biển vuông góc về bên trái A -> B; đổi A/B để đảo phía): "))
               (> (distance a b) 1e-8))
        (setq a (trans a 1 0) b (trans b 1 0) mode "FIXED_PERP") (setq mode nil)))
    (T (setq mode nil)))
  (if (and mode (or (= mode "FIXED_PERP") route-id))
    (progn
      (setq scope (strcase (bht:ask-string "Phạm vi [A=Tất cả biển phù hợp/C=Chọn trên CAD]" "A")))
      (setq scope (substr scope 1 1))
      (cond ((= scope "A") (setq ids (vl-remove-if-not '(lambda (oid) (= (bht:get (bht:obj-read oid) "nhom") "BIEN_BAO")) (bht:obj-ids))))
            ((= scope "C") (bht:msg "Chọn các ký hiệu/nhãn/dấu gốc hoặc điểm RTK của biển cần xoay.")
              (setq ids (bht:kh-heading-selection (ssget)))))
      (if ids
        (progn
          (if (= 0 (logand 8 (getvar "UNDOCTL")))
            (progn (command-s "_.UNDO" "_Begin") (setq started T)))
          (setq result (bht:kh-heading-apply ids mode route-id (if (= mode "FIXED_PERP") (atan (- (cadr b) (cadr a)) (- (car b) (car a))) nil)))
          (if started (command-s "_.UNDO" "_End")) (setq started nil)
          (if (eq (car result) 'LOI) (bht:warn (cadr result))
            (bht:msg (strcat "Đã xoay " (itoa (cdr (assoc 'rotated result))) " biển; bỏ qua " (itoa (cdr (assoc 'skipped result)))
              ". Vị trí/tỷ lệ biển và điểm RTK giữ nguyên."
              (if (> (cdr (assoc 'skipped result)) 0) (strcat " ID bỏ qua: " (cdr (assoc 'skipped_ids result))) "")))))
        (bht:msg "Không có biển được chọn; chưa đổi hướng."))))
  (bht:log-flush) (princ))

;; Independent filled disk at each survey X, also when the sign is placed at G.
;; A two-arc wide polyline makes an exact disk (outer radius 0.07, inner radius 0).
(defun bht:kh-origin-block ()
  (bht:block "BHT_SIGN_ORIGIN_V0634"
    '(((0 . "LWPOLYLINE") (100 . "AcDbEntity") (8 . "0") (62 . 0)
       (100 . "AcDbPolyline") (90 . 2) (70 . 1) (43 . 0.07)
       (10 -0.035 0.0) (42 . 1.0) (10 0.035 0.0) (42 . 1.0)))))

(defun bht:kh-origin-sync (oid rec index anchor existing / used pid key p pt e d nd nx blk sc removed)
  (setq used nil removed 0)
  (if (and anchor (bht:kh-fillable-p rec))
    (progn
      (setq blk (bht:kh-origin-block) sc (cadr anchor))
      (foreach pid (bht:get-all rec "pt")
        (setq key (strcase pid))
        (if (and (not (member key used)) (setq p (bht:pt-find key index)))
          (progn
            (setq pt (bht:pv p 'xyz)
                  e (car (vl-remove-if-not '(lambda (item) (= (cadr (bht:xget item "BHT_SIGN_ORIGIN")) key)) existing)))
            (if (null e) (setq e (bht:insert blk pt "BHT_KYHIEU" sc)))
            (if e
              (progn
                (setq d (entget e)
                      nd (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put
                           (bht:dxf-put (bht:dxf-put (bht:dxf-put d 2 blk) 8 "BHT_KYHIEU") 10 pt)
                           41 sc) 42 sc) 43 sc) 50 0.0) 62 7)
                      nx (list -3 (list "BHT_SIGN_ORIGIN" (cons 1000 oid) (cons 1000 key))))
                (if (not (bht:ent-same-p e d nd nx)) (progn (entmod (append nd (list nx))) (entupd e)))
                (setq used (cons key used) existing (vl-remove e existing)))))))))
  (foreach e existing (if (entget e) (progn (entdel e) (setq removed (1+ removed)))))
  removed)

;; Dong bo mot duong dan. Tra ve so ban trung/thua da xoa.
(defun bht:kh-leader-xy (points)
  (mapcar '(lambda (pt) (list (car pt) (cadr pt))) points))
(defun bht:kh-leader-matches-p (d points elevation)
  (and (equal (mapcar 'cdr (vl-remove-if-not '(lambda (pair) (= (car pair) 10)) d)) (bht:kh-leader-xy points) 1e-8)
       (equal (cdr (assoc 38 d)) elevation 1e-8)))
(defun bht:kh-leader-sync-one (oid a b ents via / rec pair d anchor bounds)
  (setq rec (bht:obj-read oid))
  ;; A delineator is drawn at its surveyed foot; a sign leader has no meaning here.
  (if (= (bht:get rec "nhom") "COC_TIEU") (setq a nil b nil via nil))
  (if (and a b (not (equal a b 1e-8)) (null via) (= (bht:kh-leader-mode rec) "GAP_KHUC"))
    (progn
      (setq pair (assoc oid (bht:tagged-pairs "INSERT" "BHT_KH" 1)) d (if pair (entget (cdr pair)) nil))
      (if d
        (progn
          (setq anchor (list (cdr (assoc 10 d)) (abs (cdr (assoc 41 d))) (cdr (assoc 50 d)))
                bounds (if (bht:fn-defined-p 'BHTSIGNBOUNDS) (vl-catch-all-apply 'BHTSIGNBOUNDS (list (cdr (assoc 2 d)))) nil))
          (if (vl-catch-all-error-p bounds) (setq bounds nil))
          (setq via (bht:kh-support-via a b anchor bounds))))))
  (bht:kh-leader-sync-source oid a b ents via nil))

(defun bht:kh-leader-mode (rec / mode)
  (setq mode (bht:get rec "kh_leader_mode"))
  (if (member mode '("THANG" "GAP_KHUC" "WAYPOINT")) mode (bht:meta "symbol_leader_mode" "GAP_KHUC")))

(defun bht:kh-leader-sync-source (oid a b ents via source / e e2 d nd n points kind nx entry)
  (if (= (bht:kh-leader-mode (bht:obj-read oid)) "THANG") (setq via nil))
  (setq e (car ents) n 0)
  (foreach e2 (cdr ents) (if (entget e2) (progn (entdel e2) (setq n (1+ n)))))
  (setq points (if (and a b) (append (list a) via (list b)) nil)
        kind (if via "LWPOLYLINE" "LINE"))
  (if (and e (setq d (entget e)) (/= (cdr (assoc 0 d)) kind))
    (progn (entdel e) (setq e nil d nil)))
  (if (and a b (or via (> (distance a b) 0.05)))
    (progn
      (if e
        (setq nd (if via
          (if (bht:kh-leader-matches-p d points (caddr a)) d
            (append (bht:dxf-put (vl-remove-if '(lambda (pair) (member (car pair) '(10 90))) d) 38 (caddr a))
                    (list (cons 90 (length points))) (mapcar '(lambda (pt) (cons 10 (list (car pt) (cadr pt)))) points)))
          (bht:dxf-put (bht:dxf-put d 10 a) 11 b)))
        (setq nd (if via
          (append '((0 . "LWPOLYLINE") (100 . "AcDbEntity") (410 . "Model") (8 . "BHT_DUONG_DAN") (100 . "AcDbPolyline"))
            (list (cons 90 (length points)) '(70 . 0) (cons 38 (caddr a)))
            (mapcar '(lambda (pt) (cons 10 (list (car pt) (cadr pt)))) points))
          (list '(0 . "LINE") '(410 . "Model") '(8 . "BHT_DUONG_DAN") (cons 10 a) (cons 11 b)))))
      (setq nd (bht:dxf-put nd 8 "BHT_KYHIEU"))
      (if source (setq nd (bht:dxf-put (bht:dxf-put (bht:dxf-put nd 62 7) 6 "Continuous") 370 9)))
      (setq nx (bht:kh-leader-xdata oid))
      (if source (setq nx (list -3 (append (cadr nx) (list (cons 1000 source))))))
      (if e
        (if (not (bht:ent-same-p e d nd nx)) (progn (entmod (append nd (list nx))) (entupd e)))
        (setq e (entmakex (append nd (list nx)))))
      (if (and e *bht-caption-index-active*)
        (progn
          (setq entry (assoc oid *bht-caption-leaders*))
          (if entry
            (if (not (member e (cdr entry))) (setq *bht-caption-leaders* (subst (append entry (list e)) entry *bht-caption-leaders*)))
            (setq *bht-caption-leaders* (cons (list oid e) *bht-caption-leaders*))))))
    (if (and e (entget e)) (progn (entdel e) (setq n (1+ n)))))
  n)

(defun bht:kh-multiple-supports-p (rec / count)
  (and (bht:kh-fillable-p rec)
       (setq count (bht:int (bht:get rec "so_tru"))) (> count 1)))

(defun bht:kh-foot-world (pt anchor / scale heading-angle origin)
  (setq origin (car anchor) scale (cadr anchor) heading-angle (caddr anchor))
  (list (+ (car origin) (* scale (- (* (car pt) (cos heading-angle)) (* (cadr pt) (sin heading-angle)))))
        (+ (cadr origin) (* scale (+ (* (car pt) (sin heading-angle)) (* (cadr pt) (cos heading-angle)))))
        (+ (caddr origin) (* scale (caddr pt)))))

 ;; Route leaders behind the feet in the sign's local axes, never through the face.
(defun bht:kh-support-via (source target anchor bounds / a dx dy x y gap rail z origin sc sx sy tx ty side local)
  (setq a (caddr anchor) dx (- (car source) (car target)) dy (- (cadr source) (cadr target))
        x (+ (* dx (cos a)) (* dy (sin a)))
        y (+ (* (- dx) (sin a)) (* dy (cos a))) z (caddr source)
        gap (* 0.25 (cadr anchor)))
  (cond
    ((<= y (- gap))
      (list (list (- (car source) (* x (cos a))) (- (cadr source) (* x (sin a))) z)))
    ((and (listp bounds) (= (length bounds) 4))
      ;; An origin above the face needs a side corridor, even when directly
      ;; aligned with the foot. Use the full block bounds in local axes.
      (setq origin (car anchor) sc (cadr anchor)
            dx (- (car source) (car origin)) dy (- (cadr source) (cadr origin))
            sx (/ (+ (* dx (cos a)) (* dy (sin a))) sc)
            sy (/ (+ (* (- dx) (sin a)) (* dy (cos a))) sc)
            dx (- (car target) (car origin)) dy (- (cadr target) (cadr origin))
            tx (/ (+ (* dx (cos a)) (* dy (sin a))) sc)
            ty (/ (+ (* (- dx) (sin a)) (* dy (cos a))) sc)
            side (if (>= sx (- (/ (+ (car bounds) (caddr bounds)) 2.0) 1e-8))
                   (max sx (+ (caddr bounds) 0.25)) (min sx (- (car bounds) 0.25)))
            rail (- (min ty (cadr bounds)) 0.25))
      (mapcar '(lambda (local / p) (setq p (bht:kh-foot-world local anchor))
                 (list (car p) (cadr p) z))
              (list (list side sy 0.0) (list side rail 0.0) (list tx rail 0.0))))
    ((< (abs x) 1e-8) nil)
    (T
      (setq rail (min (- gap) (- y gap)))
      (list
        (list (+ (car target) (* x (cos a)) (* (- rail) (sin a)))
              (+ (cadr target) (* x (sin a)) (* rail (cos a))) z)
        (list (+ (car target) (* (- rail) (sin a))) (+ (cadr target) (* rail (cos a))) z)))))

;; Each measured support point has its own leader, keyed by survey ID.
(defun bht:kh-support-leaders (oid rec index blk anchor ents via / points pid p raw feet heading-angle sorted count i target mine used removed e x key joint bounds)
  (setq removed 0 points nil used nil)
  (if anchor
    (progn
      (foreach pid (bht:get-all rec "pt")
        (if (and (not (assoc (strcase pid) points)) (setq p (bht:pt-find pid index)))
          (setq points (cons (cons (strcase pid) (bht:pv p 'xyz)) points))))
      (setq raw (if (bht:fn-defined-p 'BHTSIGNFEET) (vl-catch-all-apply 'BHTSIGNFEET (list blk)) nil) feet nil)
      (if (vl-catch-all-error-p raw) (setq raw nil))
      (while (>= (length raw) 3)
        (setq feet (append feet (list (bht:kh-foot-world (list (car raw) (cadr raw) (caddr raw)) anchor))) raw (cdddr raw)))
      (if (null feet)
        (progn
          (setq bounds (if (bht:fn-defined-p 'BHTSIGNBOUNDS) (vl-catch-all-apply 'BHTSIGNBOUNDS (list blk)) nil))
          (if (and (listp bounds) (= (length bounds) 4))
            (setq feet (list (bht:kh-foot-world (list (/ (+ (car bounds) (caddr bounds)) 2.0) (cadr bounds) 0.0) anchor)))
            (setq feet (list (car anchor))))))
      (setq heading-angle (caddr anchor))
      (setq sorted (bht:sort-by points '(lambda (a b)
        (< (+ (* (car (cdr a)) (cos heading-angle)) (* (cadr (cdr a)) (sin heading-angle)))
           (+ (* (car (cdr b)) (cos heading-angle)) (* (cadr (cdr b)) (sin heading-angle)))))))
      (setq count (length sorted) i 0 joint nil)
      ;; Two-post boards: one crossbar and a shared midpoint for survey leaders.
      ;; Retain explicitly chosen waypoints and layouts with more than two feet.
      (if (and (= (length feet) 2) (wcmatch blk "BHT_SUPPORT_V0645_2_*") (null via) (> count 0))
        (progn
          (setq joint (mapcar '(lambda (a b) (/ (+ a b) 2.0)) (car feet) (cadr feet))
                key "@BHT_RAIL"
                mine (vl-remove-if-not '(lambda (item) (= (nth 2 (bht:xget item "BHT_KH")) key)) ents))
          (setq removed (+ removed (bht:kh-leader-sync-source oid (car feet) (cadr feet) mine nil key))
                used (cons key used))))
      (foreach p sorted
        (setq key (car p) target (if (= count 1)
          (car (bht:sort-by feet '(lambda (a b) (< (distance (cdr p) a) (distance (cdr p) b)))))
          (nth (min i (1- (length feet))) feet))
              mine (vl-remove-if-not '(lambda (item / tag)
                (and (entget item) (setq tag (bht:xget item "BHT_KH"))
                     (>= (length tag) 3) (= (nth 2 tag) key))) ents))
        (if joint (setq target joint))
        (setq bounds (if (bht:fn-defined-p 'BHTSIGNBOUNDS) (vl-catch-all-apply 'BHTSIGNBOUNDS (list blk)) nil))
        (if (vl-catch-all-error-p bounds) (setq bounds nil))
        (setq removed (+ removed (bht:kh-leader-sync-source oid (cdr p) target mine
          (cond ((= (bht:kh-leader-mode rec) "THANG") nil)
                ((= (bht:kh-leader-mode rec) "WAYPOINT") via)
                (via via) (T (bht:kh-support-via (cdr p) target anchor bounds))) key))
              used (cons key used) i (1+ i)))))
  (foreach e ents
    (setq x (bht:xget e "BHT_KH"))
    (if (and (entget e) (not (member (nth 2 x) used))) (progn (entdel e) (setq removed (1+ removed)))))
  removed)

(defun bht:kh-xdata-ins (oid state pt rot sc)
  (list -3 (list "BHT_KH" (cons 1000 oid) (cons 1000 state) (cons 1000 (bht:fnum (car pt) 6))
                 (cons 1000 (bht:fnum (cadr pt) 6)) (cons 1000 (bht:fnum rot 8)) (cons 1000 (bht:fnum sc 8))))
)

(defun bht:kh-xdata-txt (oid state pt)
  (list -3 (list "BHT_KH" (cons 1000 oid) (cons 1000 "NHAN") (cons 1000 state)
                 (cons 1000 (bht:fnum (car pt) 6)) (cons 1000 (bht:fnum (cadr pt) 6))))
)

(defun bht:kh-extra-face-label-p (e / x)
  (setq x (bht:xget e "BHT_KH"))
  (and (>= (length x) 6) (= (nth 1 x) "NHAN") (wcmatch (nth 5 x) "FACE:*")))

(defun bht:kh-station-label-p (e / x)
  (setq x (bht:xget e "BHT_KH"))
  (and (>= (length x) 6) (= (nth 1 x) "NHAN") (= (nth 5 x) "STATION")))

(defun bht:kh-extra-label-p (e)
  (or (bht:kh-extra-face-label-p e) (bht:kh-station-label-p e)))

;; Place chainage below all sign names in the title's local coordinate frame.
;; Ordinary TEXT stays editable and follows the existing move/scale/bounds code.
(defun bht:kh-sync-station-label (oid rec blk anchor existing / old title value d h rot pt rows e nd nx removed item layout *bht-caption-rec*)
  (setq *bht-caption-rec* rec)
  (setq existing (vl-remove-if-not 'entget existing))
  (setq old (vl-remove-if-not 'bht:kh-station-label-p existing)
        title (car (vl-remove-if 'bht:kh-extra-label-p existing))
        value (bht:get rec "ly_trinh_km") removed 0)
  (if (and anchor (bht:kh-fillable-p rec) (/= (bht:trim value) ""))
    (progn
      (if (and title (setq d (entget title)))
        (setq h (cdr (assoc 40 d)) rot (cdr (assoc 50 d))
              rows (if (= (bht:get rec "nhom") "BIEN_BAO") (max 1 (length (bht:kh-sign-codes rec))) 1)
              pt (polar (cdr (assoc 11 d)) (- rot (/ pi 2.0)) (* rows h 1.5)))
        ;; Named boards already have their title on the face. Chainage is independent.
        (setq layout (bht:kh-sign-label-layout blk anchor) h (cadr layout)
              rot (caddr layout) pt (car layout)))
      (setq e (car old))
      (if (null e) (setq e (bht:text value pt h "BHT_NHAN")))
      (if e
        (progn
          (setq d (entget e)
                nd (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put d 1 (bht:cad-text value (bht:kh-label-style value))) 10 pt) 11 pt) 40 h) 50 rot) 72 1) 73 3)
                nd (bht:dxf-put nd 7 (bht:kh-label-style value))
                nx (bht:kh-xdata-txt oid "TU_DONG" pt)
                nx (list -3 (append (cadr nx) (list '(1000 . "STATION")))))
          (if (not (bht:ent-same-p e d nd nx)) (progn (entmod (append nd (list nx))) (entupd e)))))))
  (foreach item old (if (and (not (eq item e)) (entget item)) (progn (entdel item) (setq removed (1+ removed)))))
  removed)

(defun bht:kh-sync-face-labels (oid rec blk anchor existing / code codes title h rot i lbl pt base e d nd nx used x tag old removed *bht-caption-rec*)
  (setq *bht-caption-rec* rec)
  (setq old (vl-remove-if-not 'bht:kh-extra-face-label-p existing) removed 0)
  (setq title (car (vl-remove-if 'bht:kh-extra-label-p existing)))
  (if (and anchor title (entget title))
    (progn
      ;; Every row follows the primary caption, including a deliberately moved caption.
      (setq d (entget title) base (cdr (assoc 11 d)) h (cdr (assoc 40 d))
            codes (if (bht:kh-missing-face-p rec) nil (cdr (bht:kh-sign-codes rec)))
            rot (cdr (assoc 50 d)) i 1)
      (foreach code codes
        (setq tag (strcat "FACE:" (itoa i))
              lbl (if (= code "") "Biển báo" (bht:kh-code-label "BIEN_BAO" code))
              pt (polar base (- rot (/ pi 2.0)) (* i h 1.5))
              e (car (vl-remove-if-not '(lambda (item) (= (nth 5 (bht:xget item "BHT_KH")) tag)) old)))
        (if (null e) (setq e (bht:text lbl pt h "BHT_NHAN")))
        (if e
          (progn
            (setq d (entget e)
                  nd (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put d 1 (bht:cad-text lbl (bht:kh-label-style lbl))) 10 pt) 11 pt) 40 h) 50 rot) 72 1) 73 3)
                  nd (bht:dxf-put nd 7 (bht:kh-label-style lbl))
                  nx (bht:kh-xdata-txt oid "TU_DONG" pt)
                  nx (list -3 (append (cadr nx) (list (cons 1000 tag)))))
            (if (not (bht:ent-same-p e d nd nx)) (progn (entmod (append nd (list nx))) (entupd e)))
            (setq used (cons e used))))
        (setq i (1+ i)))))
  (foreach e old (if (not (member e used)) (progn (entdel e) (setq removed (1+ removed)))))
  removed)

;; Trang thai ky hieu INSERT: 'AUTO / 'TAY. autopos: vi tri tu dong hien tai (de nhan ra ky hieu 0.3.2).
(defun bht:kh-ins-state (e autopos / d x pos)
  (setq d (entget e) x (bht:xget e "BHT_KH") pos (cdr (assoc 10 d)))
  (cond
    ((>= (length x) 6)
     (if (and (= (nth 1 x) "TU_DONG")
              (bht:pt-near pos (list (bht:num (nth 2 x)) (bht:num (nth 3 x))) *bht-lbl-tol*)
              (equal (cdr (assoc 50 d)) (bht:num (nth 4 x)) 1e-6)
              (equal (cdr (assoc 41 d)) (bht:num (nth 5 x)) 1e-6))
       'AUTO 'TAY))
    ;; 0.3.2: dat o trung binh diem, goc 0, ty le kh_scale -> tu dong
    ((and autopos (bht:pt-near pos autopos *bht-lbl-tol*) (equal (cdr (assoc 50 d)) 0.0 1e-9)
          (equal (cdr (assoc 41 d)) (bht:kh-scale) 1e-6))
     'AUTO)
    (T 'TAY))
)

;; Trang thai nhan ky hieu: legacy-pos = vi tri 0.3.2 (theo ky hieu + ty le).
(defun bht:kh-txt-state (e legacy-pos / d x pos)
  (setq d (entget e) x (bht:xget e "BHT_KH") pos (cdr (assoc (if (= (cdr (assoc 72 d)) 1) 11 10) d)))
  (cond
    ((and (>= (length x) 5) (= (nth 1 x) "NHAN"))
     (if (and (= (nth 2 x) "TU_DONG") (bht:pt-near pos (list (bht:num (nth 3 x)) (bht:num (nth 4 x))) *bht-lbl-tol*))
       'AUTO 'TAY))
    ((and legacy-pos (bht:pt-near pos legacy-pos *bht-lbl-tol*)) 'AUTO)
    (T 'TAY))
)

;; Thuc the khong doi: du lieu DXF va XData giong ban moi.
(defun bht:ent-same-p (e d nd nx)
  (and (equal d nd) (equal (bht:xget e (car (cadr nx))) (mapcar 'cdr (cdr (cadr nx)))))
)

;; Cap nhat ky hieu cua ho so theo object_id.
;;  scope nil = moi ho so (tao ky hieu con thieu); danh sach oid = chi cac ho so do.
;;  create nil = chi cap nhat ky hieu da co (khong tao moi).
;; Ky hieu mo coi (ho so da xoa) luon bi xoa. Tra ve assoc:
;;  created updated unchanged removed duplicates manual nopos total
(defun bht:symbol-sync-ex (scope create / *bht-caption-rec* *bht-caption-leaders* *bht-caption-index-active* s h ins txt dan origins index oids rec pos blk g e d nd nx st anchor sc rot lpos heading
                                       auto tr trot layout created updated same removed dup manual nopos all lbl ok)
  (setq s (bht:kh-scale) h (bht:kh-h) created 0 updated 0 same 0 removed 0 dup 0 manual 0 nopos 0)
  (bht:sign-layers) (bht:layer "BHT_NHAN" 2) (bht:layer "BHT_DUONG_DAN" 6)
  (bht:regapp "BHT_KH")
  (bht:regapp "BHT_SIGN_ORIGIN")
  (bht:symbol-blocks)
  (setq ins (bht:group-pairs (bht:tagged-pairs "INSERT" "BHT_KH" 1))
        txt (bht:group-pairs (bht:tagged-pairs "TEXT" "BHT_KH" 1))
        dan (bht:group-pairs (append (bht:tagged-pairs "LINE" "BHT_KH" 1) (bht:tagged-pairs "LWPOLYLINE" "BHT_KH" 1)))
        origins (bht:group-pairs (bht:tagged-pairs "INSERT" "BHT_SIGN_ORIGIN" 1))
        all (bht:obj-ids) index (bht:pt-all)
        oids (if scope (mapcar 'strcase scope) all))
  (setq *bht-caption-leaders* dan *bht-caption-index-active* T)
  ;; ky hieu cua ho so da xoa
  (foreach gr (append ins txt dan origins)
    (if (not (member (car gr) all))
      (foreach e2 (cdr gr) (if (entget e2) (progn (entdel e2) (setq removed (1+ removed)))))))
  (foreach oid oids
    (setq rec (bht:obj-read oid) *bht-caption-rec* rec)
    (if rec
      (progn
        (setq pos (bht:obj-position rec index) blk (bht:kh-block rec) sc (bht:kh-scale-for rec)
              tr (bht:kh-auto-transform rec pos sc) auto (car tr) rot (cadr tr)
              g (cdr (assoc oid ins)) anchor nil)
        (foreach e2 (cdr g) (entdel e2) (setq dup (1+ dup)))
        (cond
          ((setq e (car g))
           (setq d (entget e) st (bht:kh-ins-state e auto))
           (cond
             ((and (not *bht-preserve-symbol-placement*) (eq st 'AUTO) auto)
               (setq nd (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put d 10 auto) 41 sc) 42 sc) 43 sc) 50 rot)
                    nd (bht:dxf-put (bht:dxf-put nd 2 blk) 8 "BHT_KYHIEU")
                    nx (bht:kh-xdata-ins oid "TU_DONG" auto rot sc)))
             ((and (not *bht-preserve-symbol-placement*) (eq st 'AUTO))
              ;; ho so khong con diem hop le: giu ky hieu tai cho
              (setq nopos (1+ nopos) nd (bht:dxf-put d 2 blk)
                    nx (bht:kh-xdata-ins oid "TU_DONG" (cdr (assoc 10 d)) (cdr (assoc 50 d)) (cdr (assoc 41 d)))))
             (T
              (setq manual (1+ manual) nd (bht:dxf-put d 2 blk)
                    nx (if *bht-preserve-symbol-placement* (assoc -3 (entget e '("BHT_KH"))) (bht:kh-xdata-ins oid "TAY" (cdr (assoc 10 d)) (cdr (assoc 50 d)) (cdr (assoc 41 d)))))))
           (if (and (or (not *bht-preserve-symbol-placement*) *bht-apply-sign-heading*)
                    (setq heading (bht:kh-orientation-angle rec pos)))
             (setq nd (bht:dxf-put nd 50 heading)
                   nx (bht:kh-xdata-ins oid (if (eq st 'AUTO) "TU_DONG" "TAY") (cdr (assoc 10 nd)) heading (cdr (assoc 41 nd)))))
           (if (and (bht:kh-light-p rec) (/= (cdr (assoc 2 d)) blk))
             (progn
               (setq nd (bht:dxf-put (bht:dxf-put (bht:dxf-put nd 41 sc) 42 sc) 43 sc)
                     nx (bht:kh-xdata-ins oid (if (eq st 'AUTO) "TU_DONG" "TAY") (cdr (assoc 10 nd)) (cdr (assoc 50 nd)) sc))
               (if (not *bht-preserve-symbol-placement*)
                 (bht:obj-write oid (bht:set rec "custom_block" blk)))))
           (if (bht:ent-same-p e d nd nx)
             (setq same (1+ same))
             (progn (entmod (append nd (list nx))) (entupd e) (setq updated (1+ updated))))
           (setq d (entget e) anchor (list (cdr (assoc 10 d)) (abs (cdr (assoc 41 d))) (cdr (assoc 50 d)))))
          ((and auto create)
            (setq e (bht:insert blk auto "BHT_KYHIEU" sc))
           (if e
             (progn
                (setq d (bht:dxf-put (entget e) 50 rot))
                (entmod (append d (list (bht:kh-xdata-ins oid "TU_DONG" auto rot sc))))
               (entupd e)
                (setq created (1+ created) anchor (list auto sc rot)))))
          ((null pos) (setq nopos (1+ nopos))))
        (setq dup (+ dup (if (or (= (bht:get rec "nhom") "BIEN_BAO") (bht:kh-multiple-supports-p rec))
          (bht:kh-support-leaders oid rec index blk anchor (cdr (assoc oid dan)) (bht:kh-via-points rec))
          (bht:kh-leader-sync-one oid pos (if anchor (car anchor) nil) (cdr (assoc oid dan)) (bht:kh-via-points rec)))))
        (setq dup (+ dup (bht:kh-origin-sync oid rec index anchor (cdr (assoc oid origins)))))
        ;; Leaders may have moved or just been created. Measure the current geometry,
        ;; not the index captured before placement (which made captions jump on refresh).
        (setq *bht-caption-leaders* (cons (cons oid (mapcar 'cdr (vl-remove-if-not
          '(lambda (pr) (= (car pr) oid)) (append (bht:tagged-pairs "LINE" "BHT_KH" 1)
                                                (bht:tagged-pairs "LWPOLYLINE" "BHT_KH" 1)))))
          (vl-remove-if '(lambda (pr) (= (car pr) oid)) *bht-caption-leaders*)))
        ;; nhan ky hieu (theo vi tri ky hieu thuc te)
        (setq lbl (bht:kh-label oid rec))
        (if (= lbl "")
          (foreach e2 (cdr (assoc oid txt)) (if (and (entget e2) (not (bht:kh-station-label-p e2)))
            (progn (entdel e2) (setq removed (1+ removed))))))
        (if (and anchor (/= lbl ""))
          (progn
            (setq h (bht:kh-label-height rec) lbl (bht:kh-label oid rec) sc (cadr anchor) rot (caddr anchor)
                  trot (bht:kh-readable-angle rot)
                  lpos (polar (car anchor) (+ rot (/ pi 2.0)) (* 2.65 sc)))
            (if (or (bht:kh-fillable-p rec) (bht:kh-light-p rec))
              (progn
                (setq layout (bht:kh-sign-label-layout blk anchor) lpos (car layout) trot (caddr layout))
                (setq h (cadr layout))))
            ;; Marker labels follow the INSERT heading-angle and sit below its local bounds.
            (if (= (bht:get rec "nhom") "COC_TIEU")
              (setq layout (bht:kh-sign-label-layout blk anchor) lpos (car layout) h (* 0.22 sc)))
            (setq g (cdr (assoc oid txt)))
            (setq g (vl-remove-if 'bht:kh-extra-label-p g))
            (foreach e2 (cdr g) (entdel e2) (setq dup (1+ dup)))
            (if (setq e (car g))
              (progn
                (setq d (entget e)
                      st (bht:kh-txt-state e (if pos (list (+ (car pos) (* 1.5 (bht:kh-scale))) (+ (cadr pos) (* 1.0 (bht:kh-scale)))) nil)))
                (if (and anchor (or (eq st 'AUTO) (= (bht:get rec "nhom") "COC_TIEU")))
                  (setq nd (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put d 10 lpos) 1 (bht:cad-text lbl (bht:kh-label-style lbl))) 40 h) 8 "BHT_NHAN") 50 trot)
                        nx (bht:kh-xdata-txt oid "TU_DONG" lpos))
                  (setq manual (1+ manual)
                        nd (bht:dxf-put (bht:dxf-put d 1 (bht:cad-text lbl (bht:kh-label-style lbl))) 40 h)
                        nx (bht:kh-xdata-txt oid "TAY" (cdr (assoc (if (= (cdr (assoc 72 d)) 1) 11 10) d)))))
                (if (and anchor (or (eq st 'AUTO) (= (bht:get rec "nhom") "COC_TIEU")))
                  (setq nd (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put nd 10 lpos) 11 lpos) 72 1) 73 3)))
                ;; 5.0: kieu chu nhan (chi nhan BHT_KH do BHT tao; vi tri tay van giu)
                (setq nd (bht:dxf-put nd 7 (bht:kh-label-style lbl)))
                (if (not (bht:ent-same-p e d nd nx))
                  (progn (entmod (append nd (list nx))) (entupd e))))
              (progn
                (setq e (bht:text lbl lpos h "BHT_NHAN"))
                (if e (progn
                  (setq nd (bht:dxf-put (bht:dxf-put (entget e) 50 trot) 7 (bht:kh-label-style lbl)))
                  (if anchor
                    (setq nd (bht:dxf-put (bht:dxf-put (bht:dxf-put (bht:dxf-put nd 10 lpos) 11 lpos) 72 1) 73 3)))
                  (entmod (append nd
                                             (list (bht:kh-xdata-txt oid "TU_DONG" lpos)))) (entupd e))))))))))
  (foreach oid oids
    (setq rec (bht:obj-read oid))
    (if (and rec (bht:kh-fillable-p rec))
      (progn
        (setq e (cdr (assoc oid (bht:tagged-pairs "INSERT" "BHT_KH" 1)))
              d (if e (entget e) nil)
              anchor (if d (list (cdr (assoc 10 d)) (abs (cdr (assoc 41 d))) (cdr (assoc 50 d))) nil)
              blk (if d (cdr (assoc 2 d)) "")
              g (cdr (assoc oid (bht:group-pairs (bht:tagged-pairs "TEXT" "BHT_KH" 1)))))
        (if (= (bht:get rec "nhom") "BIEN_BAO") (setq dup (+ dup (bht:kh-sync-face-labels oid rec blk anchor g))))
        (setq dup (+ dup (bht:kh-sync-station-label oid (if anchor rec nil) blk anchor g))))))
  (foreach oid oids
    (if (= (bht:get (bht:obj-read oid) "nhom") "COC_TIEU") (bht:kh-marker-caption-sync oid)))
  (bht:kh-unify-layer oids)
  (bht:kh-labels-front)
  (bht:log (strcat "Ký hiệu theo ID: tạo " (itoa created) ", cập nhật " (itoa updated) ", giữ " (itoa same)
                   ", xóa (hồ sơ đã xóa) " (itoa removed) ", xóa trùng " (itoa dup) ", vị trí tay giữ " (itoa manual)))
  (list (cons 'created created) (cons 'updated updated) (cons 'unchanged same) (cons 'removed removed)
        (cons 'duplicates dup) (cons 'manual manual) (cons 'nopos nopos)
        (cons 'total (length (bht:tagged-pairs "INSERT" "BHT_KH" 1))))
)

(defun bht:kh-labels-front ()
  (if (bht:fn-defined-p 'BHTLABELFRONT) (BHTLABELFRONT)))

;; A station update changes captions only: no placement, block rebuild or survey edits.
(defun bht:kh-station-label-refresh (oids / ins texts pr oid rec e d anchor *bht-caption-leaders*)
  (setq ins (bht:tagged-pairs "INSERT" "BHT_KH" 1)
        texts (bht:group-pairs (bht:tagged-pairs "TEXT" "BHT_KH" 1))
        *bht-caption-leaders* (bht:group-pairs (append (bht:tagged-pairs "LINE" "BHT_KH" 1)
                                                     (bht:tagged-pairs "LWPOLYLINE" "BHT_KH" 1))))
  (foreach pr ins
    (setq oid (car pr) e (cdr pr) rec (bht:obj-read oid))
    (if (and (or (null oids) (member oid oids)) (bht:kh-fillable-p rec))
      (progn
        (setq d (entget e) anchor (list (cdr (assoc 10 d)) (abs (cdr (assoc 41 d))) (cdr (assoc 50 d))))
        (bht:kh-sync-station-label oid rec (cdr (assoc 2 d)) anchor (cdr (assoc oid texts))))))
  (bht:kh-labels-front))

(defun bht:kh-unify-layer (oids / app pr e d nd old layer color changed skipped)
  (bht:sign-layers) (setq changed 0 skipped 0)
  (foreach app '("BHT_KH" "BHT_SIGN_ORIGIN")
    (foreach pr (bht:tagged-pairs "*" app 1)
      (if (or (null oids) (member (car pr) oids))
        (progn
          (setq e (cdr pr) d (entget e) old (cdr (assoc 8 d))
                nd (bht:dxf-put d 8 "BHT_KYHIEU"))
          ;; Retain visible colors when moving an entity off its old layer.
          (if (or (null (assoc 62 d)) (= (cdr (assoc 62 d)) 256))
            (setq layer (tblsearch "LAYER" old) color (abs (cdr (assoc 62 layer))) nd (bht:dxf-put nd 62 color)))
          (if (not (equal d nd))
            (if (= 4 (logand 4 (cdr (assoc 70 (tblsearch "LAYER" old)))))
              (setq skipped (1+ skipped))
              (if (entmod nd) (progn (entupd e) (setq changed (1+ changed))) (setq skipped (1+ skipped)))))))))
  (list changed skipped))

;; Display-only road reference: never creates a ROUTE or changes chainage.
(defun bht:kh-road-valid-p (ent / d typ flags v vd z flat len)
  (setq d (if ent (entget ent) nil))
  (setq typ (cdr (assoc 0 d)) flags (cdr (assoc 70 d)) flat T)
  (if (= typ "POLYLINE")
    (progn (setq v (entnext ent))
      (while (and v (= (cdr (assoc 0 (setq vd (entget v)))) "VERTEX"))
        (if z (if (> (abs (- (caddr (cdr (assoc 10 vd))) z)) 1e-6) (setq flat nil)) (setq z (caddr (cdr (assoc 10 vd)))))
        (setq v (entnext v)))))
  (and d (member typ '("LWPOLYLINE" "POLYLINE"))
       (= (cdr (assoc 410 d)) "Model")
       flags (= 0 (logand (if (= typ "POLYLINE") 89 1) flags)) flat
       (or (null (assoc 210 d)) (equal (cdr (assoc 210 d)) '(0.0 0.0 1.0) 1e-8))
       (setq len (bht:curve-length ent)) (> len 1e-6)))

(defun bht:kh-road-straight-p (ent / d pts a b)
  (setq d (entget ent) pts (mapcar 'cdr (vl-remove-if-not '(lambda (v) (= (car v) 10)) d)))
  (and (= (length pts) 2) (not (vl-some '(lambda (v) (and (= (car v) 42) (> (abs (cdr v)) 1e-12))) d))))

;; (closest normal signed-offset raw-distance beyond-end). All XY in WCS.
(defun bht:kh-road-frame (ent pt direction / pr cp par der len normal raw along end)
  (setq pr (bht:curve-project ent pt))
  (if pr (progn
    (setq cp (nth 3 pr) raw (car pr)
          par (vl-catch-all-apply 'vlax-curve-getParamAtPoint (list ent cp))
          der (if (vl-catch-all-error-p par) nil (vl-catch-all-apply 'vlax-curve-getFirstDeriv (list ent par))))
    (if (and der (not (vl-catch-all-error-p der))
             (> (setq len (sqrt (+ (* (car der) (car der)) (* (cadr der) (cadr der))))) 1e-9))
      (progn
        (setq normal (list (* direction (/ (- (cadr der)) len)) (* direction (/ (car der) len)) 0.0)
              along (/ (+ (* (- (car pt) (car cp)) (car der)) (* (- (cadr pt) (cadr cp)) (cadr der))) len)
              end (or (and (< raw 1e-6) (< along -0.001))
                      (and (< (abs (- raw (bht:curve-length ent))) 1e-6) (> along 0.001))))
        (list cp normal (+ (* (- (car pt) (car cp)) (car normal)) (* (- (cadr pt) (cadr cp)) (cadr normal))) raw end))))))

(defun bht:kh-road-target (oid rec index e labels source base / f side n cp width bounds corners minoff point off amount target center radius deficit tries frame reason pid sp sf)
  (setq f (bht:kh-road-frame *bht-road-ent* source *bht-road-dir*))
  (cond
    ((null f) (setq reason "không chiếu được điểm lên PL"))
    ((nth 4 f) (setq reason "điểm nằm ngoài đầu/cuối PL"))
    ((<= (abs (nth 2 f)) 0.001) (setq reason "điểm nằm sát PL, chưa xác định được bên"))
    (T
     (setq side (if (> (nth 2 f) 0.0) 1 -1) cp (car f) n (mapcar '(lambda (x) (* side x)) (cadr f))
           width (if (= side 1) *bht-road-left* *bht-road-right*))
     ;; A two-post sign spanning both sides cannot be classified by its midpoint.
     (foreach pid (bht:get-all rec "pt")
       (if (and (setq sp (bht:pt-find pid index)) (setq sf (bht:kh-road-frame *bht-road-ent* (bht:pv sp 'xyz) *bht-road-dir*)))
         (if (or (nth 4 sf) (<= (* side (nth 2 sf)) 0.001)) (setq reason "các điểm gốc không cùng một phía PL"))
         (setq reason "thiếu điểm gốc hoặc không chiếu được điểm")))
     (if (null reason)
       (progn
         (setq bounds (vl-catch-all-apply 'BHTDISPLAYBOUNDS (list (cons e labels))))
         (if (or (vl-catch-all-error-p bounds) (not (listp bounds)) (/= (length bounds) 4))
           (setq reason "không đo được kích thước hình và nhãn")
           (progn
             (setq corners (list (list (car bounds) (cadr bounds)) (list (car bounds) (nth 3 bounds))
                                 (list (nth 2 bounds) (cadr bounds)) (list (nth 2 bounds) (nth 3 bounds)))
                   minoff nil)
             (foreach point corners
               (setq off (+ (* (- (car point) (car base)) (car n)) (* (- (cadr point) (cadr base)) (cadr n))))
               (if (or (null minoff) (< off minoff)) (setq minoff off)))
             (setq amount (- (+ width *bht-road-clearance*) minoff)
                   target (list (+ (car cp) (* amount (car n))) (+ (cadr cp) (* amount (cadr n))) (caddr base)))
             ;; On a bent PL a bounding circle guarantees the whole display clears
             ;; all segments, including a nearby return bend. This is conservative.
             (if (not *bht-road-straight*)
               (progn
                 (setq center (list (- (/ (+ (car bounds) (nth 2 bounds)) 2.0) (car base))
                                    (- (/ (+ (cadr bounds) (nth 3 bounds)) 2.0) (cadr base)) 0.0)
                       radius (/ (distance (car corners) (last corners)) 2.0) tries 0 deficit 1.0)
                 (while (and (< tries 16) (> deficit 1e-7))
                   (setq frame (bht:kh-road-frame *bht-road-ent* (mapcar '+ target center) *bht-road-dir*))
                   (if (and frame (> (* side (nth 2 frame)) 0.0))
                     (setq deficit (- (+ (max *bht-road-left* *bht-road-right*) *bht-road-clearance* radius)
                                      (cadr (bht:curve-project *bht-road-ent* (mapcar '+ target center)))))
                     (setq deficit 1.0 reason "PL gấp khúc, chưa tìm được phía đặt an toàn"))
                   (if (and (null reason) (> deficit 1e-7))
                     (setq amount (+ amount deficit 0.0001) target (list (+ (car cp) (* amount (car n))) (+ (cadr cp) (* amount (cadr n))) (caddr base))))
                   (setq tries (1+ tries)))
                 (if (> deficit 1e-7) (setq reason "PL gấp khúc, chưa tìm được khoảng lùi an toàn"))))))))))
  (if reason (progn (setq *bht-road-skips* (cons (strcat oid ": " reason) *bht-road-skips*)) nil) target))

(defun bht:kh-road-apply (ent left right clearance direction mode / *bht-road-ent* *bht-road-left* *bht-road-right* *bht-road-clearance* *bht-road-dir* *bht-road-straight* *bht-road-skips* r reason)
  (setq *bht-road-left* (bht:num left) *bht-road-right* (bht:num right) *bht-road-clearance* (bht:num clearance))
  (cond
    ((not (bht:kh-road-valid-p ent)) (list 'LOI "Chọn một PL 2D mở trong Model, nằm trên mặt phẳng XY."))
    ((not (member direction '(1 -1))) (list 'LOI "Chọn đầu PL để xác định phía trái/phải."))
    ((not (vl-every '(lambda (v) (and v (<= 0.0 v 100000.0))) (list *bht-road-left* *bht-road-right* *bht-road-clearance*)))
     (list 'LOI "Nhập bề rộng hai bên và khoảng lùi từ 0 đến 100000 đơn vị bản vẽ."))
    ((not (bht:fn-defined-p 'BHTDISPLAYBOUNDS)) (list 'LOI "Nạp Bridge BHT 1.0.30 trước khi bố trí ra ngoài đường."))
    (T (setq *bht-road-ent* ent *bht-road-dir* direction *bht-road-straight* (bht:kh-road-straight-p ent))
       (setq r (bht:kh-gap-apply-mode clearance mode))
       (if (and *bht-road-skips* (/= (car r) 'LOI))
         (setq r (append r (cons "Các báo hiệu chưa bố trí:" (reverse *bht-road-skips*)))))
       (if (and (boundp '*bht-center-route*) *bht-center-route*)
         (setq r (mapcar '(lambda (line) (if (= (type line) 'STR) (bht:replace line "PL" "tim tuyến") line)) r))) r)))

;; Marker reference is display-only. It never replaces route_id or station data.
(defun bht:kh-marker-caption-sync (oid / rec ins d anchor labels e nd lbl sc h tb width pt rot owners pair tag layer extra)
  (setq rec (bht:obj-read oid) ins (cdr (assoc oid (bht:tagged-pairs "INSERT" "BHT_KH" 1))))
  (if (and ins (not (bht:kh-marker-locked-p oid)))
    (progn
      (setq d (entget ins) sc (abs (cdr (assoc 41 d)))
            anchor (list (cdr (assoc 10 d)) sc (cdr (assoc 50 d)))
            lbl (bht:kh-label oid rec) h (* 0.28 sc)
            pt (bht:kh-foot-world '(-0.81 0.0 0.0) anchor)
            rot (bht:kh-readable-angle (caddr anchor)))
      (setq tb (textbox (list '(0 . "TEXT") (cons 1 (bht:cad-text lbl (bht:kh-label-style lbl)))
                             (cons 7 (bht:kh-label-style lbl)) (cons 40 h))))
      (if tb
        (progn (setq width (- (caadr tb) (caar tb)))
          (if (> width (* 1.28 sc)) (setq h (* h (/ (* 1.28 sc) width))))))
      (setq labels (mapcar 'cdr (vl-remove-if-not '(lambda (p) (= (car p) oid)) (bht:tagged-pairs "TEXT" "BHT_KH" 1)))
            e (car labels))
      (foreach extra (cdr labels) (entdel extra))
      (if (null e) (setq e (bht:text lbl pt h "BHT_KYHIEU")))
      (if e
        (progn
          (setq nd (entget e))
          (foreach pair (list (cons 1 (bht:cad-text lbl (bht:kh-label-style lbl))) (cons 7 (bht:kh-label-style lbl))
                             (cons 10 pt) (cons 11 pt) (cons 40 h) (cons 41 1.0) (cons 50 rot)
                             '(62 . 2) '(72 . 1) '(73 . 2) '(8 . "BHT_KYHIEU"))
            (setq nd (bht:dxf-put nd (car pair) (cdr pair))))
          (setq nd (vl-remove-if '(lambda (p) (member (car p) '(420 430))) nd))
          (if (not (entmod (append nd (list (bht:kh-xdata-txt oid "TU_DONG" pt)))))
            (vl-exit-with-error "Không cập nhật được chữ trong cọc tiêu."))
          (entupd e)))
      ;; Remove only the redundant BHT point-name label; retain elevations/IDs and shared point labels.
      (setq owners (bht:pt-owner-map))
      (foreach pair (bht:tagged-pairs "TEXT" "BHT_NHAN" 1)
        (setq e (cdr pair) tag (bht:xget e "BHT_NHAN") layer (tblsearch "LAYER" (cdr (assoc 8 (entget e)))))
        (if (and (= (cadr tag) "TEN") (member (car tag) (bht:get-all rec "pt"))
                 (bht:lbl-marker-caption-p (car tag) owners) (= 0 (logand 4 (cdr (assoc 70 layer)))))
          (if (not (entdel e)) (vl-exit-with-error "Không xóa được nhãn tên cọc bị lặp.")))))))

(defun bht:kh-marker-angle (rec pos / ent dir route frame normal side limit heading-angle)
  (cond
    ((and (boundp '*bht-road-ent*) *bht-road-ent*)
      (setq ent *bht-road-ent* dir *bht-road-dir*))
    ((and (/= (bht:get rec "marker_display_handle") "")
          (setq ent (handent (bht:get rec "marker_display_handle"))))
      (setq dir (if (= (bht:get rec "marker_display_direction") "-1") -1 1)))
    ((setq route (bht:route-read (bht:get rec "route_id")))
      (setq ent (bht:route-ent route) dir (if (= (bht:get route "direction") "-1") -1 1)
            limit (cond ((bht:num (bht:get route "max_offset"))) (T 100.0)))))
  (if (and pos ent (bht:kh-road-valid-p ent)
           (setq frame (bht:kh-road-frame ent pos dir)) (not (nth 4 frame))
           (or (null limit) (<= (abs (nth 2 frame)) limit)))
    (progn
      (setq side (if (< (nth 2 frame) 0.0) -1.0 1.0)
            normal (mapcar '(lambda (v) (* side v)) (cadr frame)))
      ;; Native marker head is -X. Point it towards the road at the local normal.
      (setq heading-angle (rem (bht:kh-picked-rotation rec pos (mapcar '- pos normal)) (* 2.0 pi)))
      (if (< heading-angle 0.0) (+ heading-angle (* 2.0 pi)) heading-angle))))

(defun bht:kh-marker-locked-p (oid / pr layer locked)
  (if *bht-marker-index-active* (member oid *bht-marker-locked*)
  (progn (foreach pr (bht:tagged-pairs "*" "BHT_KH" 1)
    (if (= oid (car pr))
      (progn (setq layer (tblsearch "LAYER" (cdr (assoc 8 (entget (cdr pr))))))
        (if (= 4 (logand 4 (cdr (assoc 70 layer)))) (setq locked T)))))
  locked)))

;; Move the marker and its labels as a rigid display; retain the measured POINT.
(defun bht:kh-marker-place-one (oid index / rec source heading-angle pair e d old dr pt np nd x leaders code anchor label-auto)
  (setq rec (bht:obj-read oid) source (bht:obj-position rec index)
        pair (assoc oid (if *bht-marker-index-active* *bht-marker-inserts* (bht:tagged-pairs "INSERT" "BHT_KH" 1))))
  (cond
    ((or (null source) (null pair) (bht:kh-marker-locked-p oid)) 'SKIP)
    (T
      (setq e (cdr pair) d (entget e) old (cdr (assoc 10 d))
            heading-angle (bht:kh-marker-angle rec source))
      (setq rec (bht:kh-free-clear rec)
            rec (vl-remove-if '(lambda (p) (= (car p) "kh_leader_mode")) rec))
      (if (and heading-angle (boundp '*bht-road-ent*) *bht-road-ent*)
        (setq rec (bht:set (bht:set rec "marker_display_handle" (cdr (assoc 5 (entget *bht-road-ent*))))
                          "marker_display_direction" (itoa *bht-road-dir*))))
      (setq dr (if heading-angle (- heading-angle (cdr (assoc 50 d))) 0.0)
            nd (bht:dxf-put (bht:dxf-put d 10 source) 50 (+ (cdr (assoc 50 d)) dr)))
      (setq rec (bht:set rec "marker_display_rotation" (bht:fnum (cdr (assoc 50 nd)) 8)))
      (setq anchor (list source (abs (cdr (assoc 41 d))) (cdr (assoc 50 nd))))
      (if (not (entmod (append nd (list (bht:kh-xdata-ins oid "TU_DONG" source (cdr (assoc 50 nd)) (cdr (assoc 41 d)))))))
        (vl-exit-with-error "Không đặt được cọc tiêu tại điểm đo."))
      (entupd e)
      (foreach pair (if *bht-marker-index-active*
                     (mapcar '(lambda (e) (cons oid e)) (cdr (assoc oid *bht-marker-texts*)))
                     (bht:tagged-pairs "TEXT" "BHT_KH" 1))
        (if (= (car pair) oid)
          (progn
            (setq e (cdr pair) nd (entget e) x (bht:xget e "BHT_KH")
                  label-auto (and (not (bht:kh-extra-label-p e)) (eq (bht:kh-txt-state e nil) 'AUTO)))
            (foreach code '(10 11)
              (if (assoc code nd)
                (progn (setq pt (mapcar '- (cdr (assoc code nd)) old)
                             np (bht:kh-foot-world pt (list source 1.0 dr))
                             nd (bht:dxf-put nd code np)))))
            (setq nd (bht:dxf-put nd 50 (+ (cdr (assoc 50 nd)) dr))
                  pt (cdr (assoc (if (= (cdr (assoc 72 nd)) 1) 11 10) nd)))
            (if label-auto
              (setq pt (car (bht:kh-sign-label-layout (cdr (assoc 2 d)) anchor))
                    nd (bht:dxf-put (bht:dxf-put (bht:dxf-put nd 10 pt) 11 pt) 50 (caddr anchor))))
            (if (and (>= (length x) 5) (= (nth 1 x) "NHAN"))
              (setq x (append (list (nth 0 x) (nth 1 x) (nth 2 x) (bht:fnum (car pt) 6) (bht:fnum (cadr pt) 6)) (bht:nthcdr 5 x))))
            (if (not (entmod (append nd (list (list -3 (cons "BHT_KH" (mapcar '(lambda (v) (cons 1000 v)) x)))))))
              (vl-exit-with-error "Không cập nhật được nhãn cọc tiêu."))
            (entupd e))))
      (setq leaders (if *bht-marker-index-active* (cdr (assoc oid *bht-marker-leaders*))
        (mapcar 'cdr (vl-remove-if-not '(lambda (p) (= (car p) oid))
          (append (bht:tagged-pairs "LINE" "BHT_KH" 1) (bht:tagged-pairs "LWPOLYLINE" "BHT_KH" 1))))))
      (foreach e leaders (if (not (entdel e)) (vl-exit-with-error "Không xóa được đường nối cọc tiêu.")))
      (if (not (equal rec (bht:obj-read oid)))
        (bht:rec-write "OBJ" oid (bht:set rec "sua_luc" (bht:now))))
      (bht:kh-marker-caption-sync oid)
      (if heading-angle 'OK 'NO_ROUTE))))

(defun bht:kh-marker-place-all (/ index oid result placed missing skipped ids pair
                                *bht-marker-index-active* *bht-marker-inserts* *bht-marker-texts* *bht-marker-leaders* *bht-marker-locked*)
  (setq index (bht:pt-all) placed 0 missing nil skipped nil)
  ;; Index drawing entities once, instead of scanning Model space for every marker.
  (setq *bht-marker-inserts* (bht:tagged-pairs "INSERT" "BHT_KH" 1)
        *bht-marker-texts* (bht:group-pairs (bht:tagged-pairs "TEXT" "BHT_KH" 1))
        *bht-marker-leaders* (bht:group-pairs (append (bht:tagged-pairs "LINE" "BHT_KH" 1) (bht:tagged-pairs "LWPOLYLINE" "BHT_KH" 1))))
  (foreach pair (bht:tagged-pairs "*" "BHT_KH" 1)
    (if (= 4 (logand 4 (cdr (assoc 70 (tblsearch "LAYER" (cdr (assoc 8 (entget (cdr pair)))))))))
      (setq *bht-marker-locked* (cons (car pair) *bht-marker-locked*))))
  (setq *bht-marker-index-active* T)
  (foreach pair *bht-marker-inserts* (setq ids (bht:unique-add ids (car pair))))
  (foreach oid ids
    (if (= (bht:get (bht:obj-read oid) "nhom") "COC_TIEU")
      (progn (setq result (bht:kh-marker-place-one oid index))
        (cond ((eq result 'SKIP) (setq skipped (cons oid skipped)))
              (T (setq placed (1+ placed)) (if (eq result 'NO_ROUTE) (setq missing (cons oid missing))))))))
  (strcat "Cọc tiêu: " (itoa placed) " cọc tại điểm đo, không có đường nối."
    (if missing (strcat " Chưa xác định hướng tuyến, giữ góc cũ: " (bht:join (reverse missing) ", ") ".") "")
    (if skipped (strcat " Bỏ qua do thiếu điểm hoặc lớp khóa: " (bht:join (reverse skipped) ", ") ".") "")))

(defun bht:kh-marker-place-request (oid)
  (bht:kh-display-transaction 'bht:kh-marker-place-request-write (list oid)))

(defun bht:kh-marker-place-request-write (oid / rec pos result)
  (setq rec (bht:obj-read oid) pos (bht:obj-position rec (bht:pt-all)))
  (cond
    ((null pos) (list 'LOI "Cọc tiêu chưa có điểm gốc hợp lệ."))
    ((bht:kh-marker-locked-p oid) (list 'LOI "Mở khóa lớp ký hiệu, nhãn và đường nối cọc trước khi bố trí."))
    ((null (bht:kh-marker-angle rec pos)) (list 'LOI "Chọn tuyến trong hồ sơ hoặc dùng Bố trí tất cả với tim tham chiếu để xác định hướng cọc."))
    (T
      (if (null (assoc oid (bht:tagged-pairs "INSERT" "BHT_KH" 1))) (bht:symbol-sync (list oid)))
      (setq result (bht:kh-marker-place-one oid (bht:pt-all)))
      (if (eq result 'OK) (list '(updated . 1) (cons 'message "Đã đặt cọc tại điểm đo, vuông góc với tuyến; không có đường nối."))
        (list 'LOI "Chưa đặt được cọc; kiểm tra điểm gốc và lớp ký hiệu.")))))

;; Match the ROUTE_PERP option in individual placement, using the local tangent.
(defun bht:kh-bulk-angle (rec source / frame heading-angle)
  (if (and (boundp '*bht-road-ent*) *bht-road-ent*)
    (if (and (setq frame (bht:kh-road-frame *bht-road-ent* source *bht-road-dir*)) (not (nth 4 frame)))
      (bht:kh-picked-rotation rec source (mapcar '+ source (cadr frame))))
    (if (setq heading-angle (bht:kh-route-heading rec (bht:get rec "route_id") source)) (+ heading-angle (/ pi 2.0)))))

(defun bht:kh-layout-refresh (ids / *bht-preserve-symbol-placement*)
  (setq *bht-preserve-symbol-placement* T)
  (if ids (bht:symbol-sync-ex ids nil)))

;; Captions may choose a new position after leaders change; remeasure the final display.
(defun bht:kh-layout-clearance (ids index / pass changed oid e d rec source target labels frame normal delta)
  (if (and ids (boundp '*bht-road-ent*) *bht-road-ent*)
    (progn
      (setq pass 0 changed ids)
      (while (and changed (< pass 8))
        (setq changed nil pass (1+ pass)
              labels (bht:group-pairs (bht:tagged-pairs "TEXT" "BHT_KH" 1)))
        (foreach oid ids
          (setq e (cdr (assoc oid (bht:tagged-pairs "INSERT" "BHT_KH" 1))) d (entget e)
                rec (bht:obj-read oid) source (bht:obj-position rec index)
                target (bht:kh-road-target oid rec index e (cdr (assoc oid labels)) source (cdr (assoc 10 d))))
          (if (null target) (vl-exit-with-error "Không kiểm chứng được khoảng lùi sau khi dựng lại biển."))
          (setq frame (bht:kh-road-frame *bht-road-ent* source *bht-road-dir*)
                normal (mapcar '(lambda (x) (* (if (> (nth 2 frame) 0.0) 1.0 -1.0) x)) (cadr frame))
                delta (mapcar '- target (cdr (assoc 10 d))))
          (if (> (+ (* (car delta) (car normal)) (* (cadr delta) (cadr normal))) 1e-6)
            (progn
              (setq rec (bht:set (bht:set (bht:set rec "kh_free_x" (bht:fnum (car target) 8))
                                      "kh_free_y" (bht:fnum (cadr target) 8)) "kh_free_z" (bht:fnum (caddr target) 8)))
              (bht:obj-write oid rec)
              (if (not (entmod (append (bht:dxf-put d 10 target)
                    (list (bht:kh-xdata-ins oid "TU_DONG" target (cdr (assoc 50 d)) (cdr (assoc 41 d)))))))
                (vl-exit-with-error "Không hiệu chỉnh được khoảng lùi biển."))
              (entupd e) (setq changed (cons oid changed)))))
        (bht:kh-layout-refresh changed))
      (if changed (vl-exit-with-error "Nhãn biển chưa ổn định ở khoảng lùi đã chọn; đã hủy bố trí.")))))

;; Absolute display offset in drawing units; never modifies surveyed points.
(defun bht:kh-gap-apply-write (value / gap index plans pr e d rec source base dx dy len ratio target via p oid result n skipped locked txt dan x delta nd anchor markers heading-angle orient missing original restored saved handles)
  (setq gap (bht:num value) index (bht:pt-all) plans nil n 0 skipped 0)
  (if (or (null gap) (< gap 0.0) (> gap 100000.0))
    (list 'LOI "Nhập khoảng cách từ 0 đến 100000 đơn vị bản vẽ.")
    (progn
      ;; Prepare every record before committing display changes.
      (foreach pr (append (bht:tagged-pairs "*" "BHT_KH" 1) (bht:tagged-pairs "INSERT" "BHT_SIGN_ORIGIN" 1))
        (if (= 4 (logand 4 (cdr (assoc 70 (tblsearch "LAYER" (cdr (assoc 8 (entget (cdr pr)))))))))
          (setq locked (cons (car pr) locked))))
      (setq txt (bht:group-pairs (bht:tagged-pairs "TEXT" "BHT_KH" 1)))
      (setq original (bht:group-pairs
        (mapcar '(lambda (pr) (cons (car pr) (entget (cdr pr) '("*"))))
          (append (bht:tagged-pairs "*" "BHT_KH" 1) (bht:tagged-pairs "INSERT" "BHT_SIGN_ORIGIN" 1)))))
      ;; Refresh through the same renderer as individual placement before measuring clearance.
      (foreach pr (bht:tagged-pairs "INSERT" "BHT_KH" 1)
        (setq oid (car pr) e (cdr pr) d (entget e) rec (bht:obj-read oid))
        (if (and rec (not (member oid locked))
                 (member (bht:get rec "nhom") '("BIEN_BAO" "BANG_CHI_DAN" "BANG_QC" "KHAC" "CHUA_XAC_DINH" "DEN" "DEN_TH" "DEN_CS"))
                 (setq source (bht:obj-position rec index))
                 (or (not (and (boundp '*bht-road-ent*) *bht-road-ent*))
                     (bht:kh-road-target oid rec index e (cdr (assoc oid txt)) source (cdr (assoc 10 d)))))
          (if (setq heading-angle (bht:kh-bulk-angle rec source))
            (progn
              (setq nd (bht:dxf-put d 50 heading-angle))
              (if (not (entmod (append nd (list (bht:kh-xdata-ins oid "TU_DONG" (cdr (assoc 10 d)) heading-angle (cdr (assoc 41 d)))))))
                (vl-exit-with-error "Không cập nhật được hướng biển theo tuyến."))
              (entupd e)
              (setq orient (cons oid orient)))
            (setq missing (cons oid missing)))))
      (bht:kh-layout-refresh orient)
      (setq txt (bht:group-pairs (bht:tagged-pairs "TEXT" "BHT_KH" 1))
            dan (bht:group-pairs (append (bht:tagged-pairs "LINE" "BHT_KH" 1) (bht:tagged-pairs "LWPOLYLINE" "BHT_KH" 1))))
      (foreach pr (bht:tagged-pairs "INSERT" "BHT_KH" 1)
        (setq oid (car pr) e (cdr pr) d (entget e) rec (bht:obj-read oid))
        (if (and rec (member (bht:get rec "nhom") '("BIEN_BAO" "BANG_CHI_DAN" "BANG_QC" "KHAC" "CHUA_XAC_DINH" "DEN" "DEN_TH" "DEN_CS")))
          (if (and (not (member oid locked)) (setq source (bht:obj-position rec index))
                   (= 0 (logand 4 (cdr (assoc 70 (tblsearch "LAYER" (cdr (assoc 8 d))))))))
            (progn
              (setq base (cdr (assoc 10 d)) dx (- (car base) (car source)) dy (- (cadr base) (cadr source))
                    len (sqrt (+ (* dx dx) (* dy dy))))
              (if (and (boundp '*bht-road-ent*) *bht-road-ent*)
                (setq ratio 1.0 target (bht:kh-road-target oid rec index e (cdr (assoc oid txt)) source base))
                (if (> len 1e-8)
                (setq ratio (/ gap len) target (list (+ (car source) (* dx ratio)) (+ (cadr source) (* dy ratio)) (caddr base)))
                (setq ratio 1.0 target (polar source (- (cdr (assoc 50 d)) (/ pi 2.0)) gap))))
              (setq via nil)
              (foreach p (bht:kh-via-points rec)
                (setq via (append via (list (list (+ (car source) (* (- (car p) (car source)) ratio))
                                                (+ (cadr source) (* (- (cadr p) (cadr source)) ratio)) (caddr p))))))
              (if (and (boundp '*bht-display-mode*) (member *bht-display-mode* '("THANG" "GAP_KHUC")))
                (setq rec (bht:set rec "kh_leader_mode" *bht-display-mode*) via nil))
              (if target (setq plans (cons (list oid e rec d target via) plans)) (setq skipped (1+ skipped))))
            (progn (setq skipped (1+ skipped))
              (if (and (boundp '*bht-road-ent*) *bht-road-ent*)
                (setq *bht-road-skips* (cons (strcat oid ": lớp bị khóa hoặc thiếu điểm gốc") *bht-road-skips*)))))))
      ;; If rotated extents cannot be placed, restore that object's entire display.
      (foreach oid orient
        (if (not (assoc oid plans))
          (progn
            (setq saved (cdr (assoc oid original)) handles (mapcar '(lambda (d) (cdr (assoc -1 d))) saved))
            (foreach pr (append (bht:tagged-pairs "*" "BHT_KH" 1) (bht:tagged-pairs "INSERT" "BHT_SIGN_ORIGIN" 1))
              (if (and (= (car pr) oid) (not (member (cdr pr) handles))) (entdel (cdr pr))))
            (foreach d saved (setq e (cdr (assoc -1 d))) (if (null (entget e)) (entdel e)) (entmod d) (entupd e)))))
      (if (or plans (not (and (boundp '*bht-road-ent*) *bht-road-ent*)))
        (if (and (boundp '*bht-display-mode*) (/= *bht-display-mode* "")) (bht:meta-set "symbol_leader_mode" *bht-display-mode*)))
      (if (and (boundp '*bht-road-ent*) *bht-road-ent*)
        (if plans (progn
          (bht:meta-set "symbol_layout_method" (if (and (boundp '*bht-center-route*) *bht-center-route*) "CENTER" "ROAD"))
          (if (and (boundp '*bht-center-route*) *bht-center-route*)
            (progn (bht:meta-set "symbol_center_route" *bht-center-route*)
                   (bht:meta-set "symbol_center_distance" (bht:fnum gap 8))))
          (bht:meta-set "symbol_road_handle" (cdr (assoc 5 (entget *bht-road-ent*))))
          (bht:meta-set "symbol_road_direction" (itoa *bht-road-dir*))
          (if (not (and (boundp '*bht-center-route*) *bht-center-route*))
            (progn (bht:meta-set "symbol_road_left" (bht:fnum *bht-road-left* 8))
                   (bht:meta-set "symbol_road_right" (bht:fnum *bht-road-right* 8))
                   (bht:meta-set "symbol_road_clearance" (bht:fnum gap 8))))))
        (progn (bht:meta-set "symbol_layout_method" "RADIAL") (bht:meta-set "symbol_gap" (bht:fnum gap 8))))
      (foreach p plans
        (setq oid (car p) e (cadr p) rec (nth 2 p) d (nth 3 p) target (nth 4 p) via (nth 5 p))
        (setq rec (vl-remove-if '(lambda (pair) (member (car pair) '("sign_heading_mode" "sign_heading_route" "sign_heading_angle"))) rec))
        (setq rec (bht:set (bht:set (bht:set (bht:set (bht:set rec "kh_mode" "TU_DO")
                        "kh_free_x" (bht:fnum (car target) 8)) "kh_free_y" (bht:fnum (cadr target) 8))
                        "kh_free_z" (bht:fnum (caddr target) 8)) "kh_free_rot" (bht:fnum (cdr (assoc 50 d)) 8)))
        (setq rec (bht:set-all rec "kh_via" (mapcar '(lambda (q) (strcat (bht:fnum (car q) 8) "," (bht:fnum (cadr q) 8) "," (bht:fnum (caddr q) 8))) via)))
        (bht:obj-write oid rec)
        (if (not (entmod (append (bht:dxf-put d 10 target) (list (bht:kh-xdata-ins oid "TU_DONG" target (cdr (assoc 50 d)) (cdr (assoc 41 d)))))))
          (vl-exit-with-error "Không ghi được vị trí ký hiệu."))
        (entupd e)
        (setq n (1+ n)))
      (bht:kh-layout-refresh (mapcar 'car plans))
      (bht:kh-layout-clearance (mapcar 'car plans) index)
      (setq result (if plans (bht:kh-unify-layer (mapcar 'car plans)) '(0 0)))
      (setq markers (bht:kh-marker-place-all))
      (list (strcat (if (and (boundp '*bht-road-ent*) *bht-road-ent*)
                      (strcat (if (and (boundp '*bht-center-route*) *bht-center-route*)
                                (strcat "Đã bố trí theo tim " *bht-center-route* ", khoảng cách tối thiểu ")
                                "Đã đưa báo hiệu ra ngoài đường, khoảng lùi tối thiểu ") (bht:fnum gap 3) "; ")
                      (strcat "Đã đổi khoảng cách " (bht:fnum gap 3) "; ")) "đã bố trí " (itoa n) " ký hiệu; bỏ qua " (itoa skipped)
                    " ký hiệu thiếu điểm, khóa lớp hoặc chưa xác định được phía đặt (xem F2)."
                    (if missing (strcat " Giữ góc cũ do thiếu hướng tuyến: " (bht:join (reverse missing) ", ") ".") "")
                    " Đã gom lớp " (itoa (car result)) " đối tượng; bỏ qua " (itoa (cadr result)) " đối tượng khóa lớp.") markers))))

(defun bht:kh-display-write (value mode / gap *bht-display-mode*)
  (setq gap (bht:num value))
  (cond
    ((or (null gap) (< gap 0.0) (> gap 100000.0)) (list 'LOI "Nhập khoảng cách từ 0 đến 100000 đơn vị bản vẽ."))
    ((not (member mode '("" "THANG" "GAP_KHUC"))) (list 'LOI "Chọn đường nối thẳng hoặc gấp khúc."))
    (T (setq *bht-display-mode* mode) (bht:kh-gap-apply-write value))))

(defun bht:kh-gap-apply-mode (value mode)
  (bht:kh-display-transaction 'bht:kh-display-write (list value mode)))

(defun bht:kh-display-transaction (fn args / records meta entities pr d e original result failure-message)
  (setq records (bht:rec-all "OBJ") meta (bht:rec-read "META" "CONFIG"))
  (foreach pr (append (bht:tagged-pairs "*" "BHT_KH" 1) (bht:tagged-pairs "INSERT" "BHT_SIGN_ORIGIN" 1) (bht:tagged-pairs "TEXT" "BHT_NHAN" 1))
    (setq entities (cons (entget (cdr pr) '("*")) entities)))
  (setq original (mapcar '(lambda (d) (cdr (assoc -1 d))) entities)
        result (vl-catch-all-apply fn args))
  (if (or (vl-catch-all-error-p result) (and (listp result) (eq (car result) 'LOI)))
    (progn
      (setq failure-message (bht:str (if (vl-catch-all-error-p result) (vl-catch-all-error-message result) (cadr result))))
      (foreach pr (append (bht:tagged-pairs "*" "BHT_KH" 1) (bht:tagged-pairs "INSERT" "BHT_SIGN_ORIGIN" 1) (bht:tagged-pairs "TEXT" "BHT_NHAN" 1))
        (if (not (member (cdr pr) original)) (entdel (cdr pr))))
      (foreach d entities (setq e (cdr (assoc -1 d))) (if (null (entget e)) (entdel e)) (entmod d) (entupd e))
      (foreach pr records (bht:rec-write "OBJ" (car pr) (cdr pr)))
      (if meta (bht:rec-write "META" "CONFIG" meta))
      (list 'LOI (strcat "Không hoàn tất cập nhật; đã trả về trạng thái trước thao tác. " failure-message)))
    result))

(defun bht:kh-gap-apply (value) (bht:kh-gap-apply-mode value ""))

(defun bht:symbol-sync (scope) (bht:symbol-sync-ex scope T))

;; Chi cap nhat ky hieu DA CO cua cac ho so (khong tao moi).
(defun bht:symbol-refresh (oids / have)
  (setq have (mapcar 'car (bht:tagged-pairs "INSERT" "BHT_KH" 1)))
  (setq oids (vl-remove-if-not '(lambda (o) (member (strcase o) have)) oids))
  (if oids (bht:symbol-sync-ex oids nil) nil)
)

(defun bht:kh-caption-template (oids / pr e d)
  (foreach pr (bht:tagged-pairs "TEXT" "BHT_KH" 1)
    (if (member (car pr) oids)
      (progn
        (setq e (cdr pr) d (entget e))
        (if (not (bht:kh-extra-label-p e))
          (if (not (entmod (append d (list (bht:kh-xdata-txt (car pr) "TU_DONG" (cdr (assoc 11 d)))))))
            (vl-exit-with-error "Không cập nhật được nhãn; kiểm tra lớp bị khóa."))))))
  (bht:kh-layout-refresh oids))

(defun c:BHTMAUNHAN (/ *error* ss ids i x result)
  (setq *error* bht:on-error)
  (bht:msg "Chọn biển hoặc nhãn cần áp dụng mẫu; Enter áp dụng cho tất cả biển đã vẽ. Giữ vị trí hình biển và điểm khảo sát.")
  (setq ss (ssget '((-3 ("BHT_KH")))))
  (if ss
    (progn (setq i 0)
      (repeat (sslength ss)
        (setq x (bht:xget (ssname ss i) "BHT_KH") i (1+ i))
        (if x (setq ids (bht:unique-add ids (car x))))))
    (setq ids (mapcar 'car (bht:tagged-pairs "INSERT" "BHT_KH" 1))))
  (setq ids (vl-remove-if-not '(lambda (id) (= (bht:get (bht:obj-read id) "nhom") "BIEN_BAO")) ids))
  (if ids
    (progn
      (setq result (bht:kh-display-transaction 'bht:kh-caption-template (list ids)))
      (if (eq (car result) 'LOI) (bht:msg (cadr result))
        (bht:msg (strcat "Đã áp dụng mẫu nhãn cho " (itoa (length ids)) " hồ sơ biển. Hướng nhãn theo cài đặt song song/vuông góc hiện tại."))))
    (bht:msg "Không có biển báo phù hợp trong lựa chọn."))
  (princ))

;; Bo trang thai TAY cua ky hieu (+ nhan) cac ho so -> ve lai vi tri tu dong.
(defun bht:symbol-reset (oids / up n d x oid)
  (setq up (mapcar 'strcase oids) n 0)
  (foreach oid up (if (bht:obj-read oid) (bht:obj-write oid (bht:kh-free-clear (bht:obj-read oid)))))
  (foreach pr (append (bht:tagged-pairs "INSERT" "BHT_KH" 1) (bht:tagged-pairs "TEXT" "BHT_KH" 1))
    (if (member (car pr) up)
      (progn
        (setq d (entget (cdr pr)) x (bht:xget (cdr pr) "BHT_KH"))
        (entmod (append d (list (if (= (cdr (assoc 0 d)) "INSERT")
                                  (bht:kh-xdata-ins (car pr) "TU_DONG" (cdr (assoc 10 d)) (cdr (assoc 50 d)) (cdr (assoc 41 d)))
                                  (bht:kh-xdata-txt (car pr) "TU_DONG" (cdr (assoc 10 d)))))))
        (entupd (cdr pr))
        (setq n (1+ n)))))
  (list n (bht:symbol-sync-ex up nil))
)

;; Tuong thich 0.3.2: "ve lai" = cap nhat theo ID. Tra ve so doi tuong co ky hieu.
(defun bht:symbols-draw (scale h / r)
  (bht:meta-set "kh_scale" (bht:fnum scale 3)) (bht:meta-set "kh_h" (bht:fnum h 3))
  (setq r (bht:symbol-sync nil))
  (cdr (assoc 'total r))
)

(defun bht:symbol-report (r)
  (bht:msg (strcat "BHT ký hiệu (cập nhật theo object_id): tạo " (itoa (cdr (assoc 'created r)))
                   ", cập nhật " (itoa (cdr (assoc 'updated r))) ", giữ nguyên " (itoa (cdr (assoc 'unchanged r)))
                   ", xóa của hồ sơ đã xóa " (itoa (cdr (assoc 'removed r)))
                   ", xóa trùng " (itoa (cdr (assoc 'duplicates r)))
                   " | vị trí/góc/tỷ lệ người dùng đặt được giữ: " (itoa (cdr (assoc 'manual r)))
                   " | hồ sơ chưa có vị trí: " (itoa (cdr (assoc 'nopos r)))
                   " | tổng ký hiệu: " (itoa (cdr (assoc 'total r))) "."))
)

(defun c:BHTKYHIEU (/ *error* s h v r ss i x oids)
  (setq *error* bht:on-error)
  (setq v (strcase (bht:ask-string "[Enter=Cập nhật theo ID/R=Trả ký hiệu chọn về vị trí tự động/C=Cài đặt tỷ lệ, cao chữ]" "")))
  (cond
    ((= v "C")
     (setq s (bht:num (bht:ask-string "Tỷ lệ ký hiệu" (bht:meta "kh_scale" "1")))
           h (bht:num (bht:ask-string "Chiều cao chữ nhãn" (bht:meta "kh_h" "1.5"))))
     (if (and s h (> s 0) (> h 0))
       (progn (bht:meta-set "kh_scale" (bht:fnum s 3)) (bht:meta-set "kh_h" (bht:fnum h 3))
              (bht:symbol-report (bht:symbol-sync nil)))
       (bht:msg "BHT: giá trị không hợp lệ.")))
    ((= v "R")
     (bht:msg "Chọn ký hiệu / nhãn ký hiệu BHT cần trả về vị trí tự động: ")
     (setq ss (ssget '((-3 ("BHT_KH")))) i 0 oids nil)
     (if ss (while (< i (sslength ss))
              (if (setq x (bht:xget (ssname ss i) "BHT_KH")) (setq oids (bht:unique-add oids (strcase (car x)))))
              (setq i (1+ i))))
     (if oids
       (progn (setq r (bht:symbol-reset oids))
              (bht:msg (strcat "BHT: trả " (itoa (car r)) " ký hiệu/nhãn về vị trí tự động."))
              (bht:symbol-report (cadr r)))
       (bht:msg "BHT: không chọn được ký hiệu BHT nào.")))
    (T (bht:symbol-report (bht:symbol-sync nil))))
  (bht:log-flush)
  (princ)
)

;; Thu vien block tuy chon: moi file DWG la 1 block, INSBASE cua file nguon la
;; tam chen. BHT chi luu ten dinh nghia block theo nhom; hinh hoc nguon giu nguyen.
(defun c:BHTBLOCK (/ *error* gv group action path name oids r q)
  (setq *error* bht:on-error)
  (bht:msg "Nhóm block: 1=Biển báo, 2=Cọc tiêu, 3=Cột Km, 4=Bảng chỉ dẫn, 5=Bảng QC, 6=Đèn, 7=Công trình, 8=Khác, 0=Chưa xác định.")
  (setq gv (bht:ask-string "Chọn nhóm (số hoặc mã nhóm)" "1")
        group (bht:group-code gv))
  (if group
    (setq action (strcase (bht:ask-string "[D=Danh mục chuẩn/N=Nạp DWG tùy chọn/M=Dùng block mặc định/X=Xem cấu hình]"
                                          (if (= group "BIEN_BAO") "D" "N")))))
  (cond
    ((null group)
     (bht:warn "BHT: nhóm không hợp lệ."))
    ((= action "D")
     (if (= group "BIEN_BAO")
       (progn
         (setq q (bht:ask-string "Tìm biển theo mã hoặc tên, có dấu hay không dấu (vd di cham, 245a; Enter = xem danh mục chuẩn)" ""))
         (if (= q "") (bht:bb-catalog-report) (bht:sign-search-report q)))
       (bht:msg (strcat "BHT: nhóm " group " dùng block mặc định " (bht:kh-default-block group "") "."))))
    ((= action "M")
     (bht:meta-set (bht:kh-custom-key group) "")
     (bht:meta-set (bht:kh-custom-src-key group) "")
     (bht:meta-set (bht:kh-custom-factor-key group) "")
     (setq oids (bht:kh-group-oids group)
           r (if oids (bht:symbol-sync-ex oids nil) nil))
     (bht:msg (strcat "BHT: nhóm " group " dùng lại block mặc định."
                      (if r (strcat " Đã cập nhật " (itoa (length oids)) " hồ sơ.") ""))))
    ((= action "X")
     (setq name (bht:meta (bht:kh-custom-key group) ""))
     (bht:msg (strcat "BHT: nhóm " group " -> "
                      (if (= name "")
                         (strcat "mặc định " (bht:kh-default-block group "")
                                 (if (= group "BIEN_BAO") " (tự chọn theo trường Mã)" ""))
                        (strcat name " | hệ số đơn vị " (bht:meta (bht:kh-custom-factor-key group) "1")
                                " | nguồn " (bht:meta (bht:kh-custom-src-key group) ""))))))
    (T
     (setq path (getfiled (strcat "Chọn DWG block cho nhóm " group) (bht:dwg-folder) "dwg" 0))
     (cond
       ((null path) (bht:msg "BHT: đã hủy chọn DWG block."))
       ((null (setq name (bht:block-load-dwg path group)))
        (bht:err (strcat "BHT: không nạp được block - " *bht-block-load-error*)))
       (T
        (bht:meta-set (bht:kh-custom-key group) name)
        (bht:meta-set (bht:kh-custom-src-key group) path)
        (bht:meta-set (bht:kh-custom-factor-key group) (bht:fnum *bht-block-load-factor* 8))
        (setq oids (bht:kh-group-oids group)
              r (if oids (bht:symbol-sync-ex oids nil) nil))
        (bht:msg (strcat "BHT: đã nạp " name " cho nhóm " group ". INSBASE của DWG là tâm chèn."
                         (if r (strcat " Đã cập nhật " (itoa (length oids)) " hồ sơ.") "")))))))
  (bht:log-flush)
  (princ)
)



