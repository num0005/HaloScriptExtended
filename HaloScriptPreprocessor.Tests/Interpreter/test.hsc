"
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
"

(constglobal string conststring "A constglobal string!")
(constglobal long constlong (* 3 3 13))
(constglobal long constreal (* 3 3 13))

(global string otherstring "A constglobal string!")
(global long otherlong (* 3 3 13))
(global long otherreal (* 3 3 13))

(script static string test_script
	(* 3 3 13) ; 0 - halo number
	(= 3 5) ; 1 - false
	(= true true) ; 2 - true
	(= unk true) ; 3 - undefined
	(* (someOtherFunct) 3) ; 4 - undefined
	(* constreal 2) ; 5 - 234
	(and true true false) ; 6 - false
	(and true true true) ; 7 - true
	(and true true someOtherFunct) ; 8 - undefined
	(or true true false) ; 9 - true
	(or true true true) ; 10 - true
	(or true true someOtherFunct) ; 11 - true
	(or false true someOtherFunct) ; 12 - true
	(or someOtherFunct true false) ; 13 - undefined
	(+ 2 3) ; 14 - 5
	(+ 2 4 6) ; 15 - 12
	(+ someOtherFunct 3 4) ; 16 - undefined
	(+ 3 3 someOtherFunct) ; 17 - undefined
	(- 2 3) ; 18 - (-1)
	(- 2 4 6) ; 19 - undefined
	(- 3 3 4) ; 20 - undefined
	(- 3 3 someOtherFunct) ; 21 - undefined
	(- 3 someOtherFunct) ; 22 - undefined
	(/ 3 5 6) ; 23 - undefined
	(/ 10 2) ; 24 - 5
	(/ 10 0) ; 25 - infinity
	(min 1 3 0) ; 26 - 0
	(min 1 3 someOtherFunct) ; 27 - undefined
	(max 1 3 0) ; 28 - 3
	(max 1 3 someOtherFunct) ; 29 - undefined
	(!= 3 5) ; 30 - true
	(!= true true) ; 31 - false
	(!= unk true) ; 32 - undefined
	(or false false) ;  33 - false
	; gtr
	(> 3 4) ; 34 - false
	(> 3 3) ; 35 - false
	(> 4 3) ; 36 - true
	(> 4 min) ; 37 - undefined
	; lss
	(< 3 4) ; 38 - true
	(< 3 3) ; 39 - false
	(< 4 3) ; 40 - false
	(< 4 min) ; 41 - undefined
	; geq
	(>= 3 4) ; 42 - false
	(>= 3 3) ; 43 - true
	(>= 4 3) ; 44 - true
	(>= 4 min) ; 45 - undefined
	; leq
	(<= 3 4) ; 46 - true
	(<= 3 3) ; 47 - true
	(<= 4 3) ; 48 - false
	(<= 4 min) ; 49 - undefined
)