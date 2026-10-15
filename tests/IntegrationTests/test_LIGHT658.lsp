;; All light models, legacy scale, explicit position and rotation.
(setq light-snapshots nil light-row (tblnext "BLOCK" T))
(while light-row
 (setq bn (cdr (assoc 2 light-row)))
 (if (wcmatch bn "BHT_LIGHT_*")
  (progn
   (setq oid (strcat "S658-" bn))
   (bht:obj-create oid (list (cons "nhom" (if (wcmatch bn "*DEN_CS*") "DEN_CS" "DEN_TH")) (cons "custom_block" bn)) '("LIGHT-PT") T)
   (bht:kh-free-apply oid '(125.0 134.0 0.0) 1.23 nil)
   (setq e (cdr (assoc oid (bht:tagged-pairs "INSERT" "BHT_KH" 1))))
   (entmod (bht:dxf-put (bht:dxf-put (bht:dxf-put (entget e) 41 4.0) 42 4.0) 43 4.0))
   (setq light-snapshots (cons (list oid (entget e) (bht:obj-read oid)) light-snapshots))))
 (setq light-row (tblnext "BLOCK")))
(foreach scale '("1" "3" "0.5" "0.5")
 (light-check (strcat "apply-scale-" scale) (= (car (bht:api-sign-scale scale "1")) "OK")))
 ;; Validate each intermediate scale below as well.
(foreach scale '("1" "3" "0.5" "0.5")
 (bht:api-sign-scale scale "1")
 (foreach saved light-snapshots
  (setq oid (car saved) prev (cadr saved) rec (bht:obj-read oid)
        e (cdr (assoc oid (bht:tagged-pairs "INSERT" "BHT_KH" 1))) d (entget e)
        expected (* (atof scale) (BHTLIGHTFACTOR (cdr (assoc 2 d)))))
  (light-check (strcat oid "-uniform-absolute-" scale) (and (equal expected (cdr (assoc 41 d)) 1e-8) (equal expected (cdr (assoc 42 d)) 1e-8) (equal expected (cdr (assoc 43 d)) 1e-8)))
  (light-check (strcat oid "-placement-" scale) (and (equal (cdr (assoc 10 prev)) (cdr (assoc 10 d)) 1e-8) (equal (cdr (assoc 50 prev)) (cdr (assoc 50 d)) 1e-8)))
  (light-check (strcat oid "-record-" scale) (equal rec (caddr saved)))
  (light-check (strcat oid "-no-label-" scale) (null (assoc oid (bht:tagged-pairs "TEXT" "BHT_KH" 1))))
  (light-check (strcat oid "-offset-" scale) (equal (bht:kh-auto-transform (bht:kh-free-clear rec) '(100.0 100.0 0.0) (bht:kh-scale-for rec)) (bht:kh-route-transform (bht:kh-free-clear rec) '(100.0 100.0 0.0) (bht:kh-scale)) 1e-8))))
(bht:meta-set "sign_fill_all" "0")
(foreach saved light-snapshots
 (setq rec (bht:obj-read (car saved)))
 (light-check (strcat (car saved) "-background-toggle-retains-light-block") (= (bht:kh-block rec) (bht:kh-base-block rec))))
(light-check "scale-all-retains-RTK" (equal light-before (entget light-p '("BHT_PT"))))
(bht:meta-set "sign_fill_all" "1")
