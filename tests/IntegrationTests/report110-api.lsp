(defun qa110-eval (text) (eval (read text)) (list "OK"))
(vl-acad-defun 'qa110-eval)
