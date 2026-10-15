(setq b129-points (bht:pt-all) b129-records (bht:rec-all "OBJ") b129-routes (bht:rec-all "ROUTE"))
(setq b129-route (bht:route-read "TIMRANHD8") b129-ent (bht:route-ent b129-route))
(if b129-ent
  (progn
    (setq b129-result (bht:api-run 'bht:kh-road-apply (list b129-ent "5" "5" "2" 1 "GAP_KHUC")))
    (princ (strcat "\nBULK129-RESULT=" (vl-princ-to-string b129-result)))
    (princ (if (= (car b129-result) "OK") "\nBULK129-APPLY-PASS" "\nBULK129-APPLY-FAIL"))
    (princ (if (and (equal b129-points (bht:pt-all)) (equal b129-routes (bht:rec-all "ROUTE"))
      (vl-every '(lambda (pair / after) (setq after (bht:obj-read (car pair)))
        (vl-every '(lambda (key) (equal (bht:get (cdr pair) key) (bht:get after key)))
          '("ly_trinh_m" "ly_trinh_km" "route_id" "doan" "goi" "ma_hieu" "phia_duong"))) b129-records))
      "\nBULK129-SURVEY-PASS" "\nBULK129-SURVEY-FAIL")))
  (princ "\nBULK129-NO-ROUTE-FAIL"))
(princ)
