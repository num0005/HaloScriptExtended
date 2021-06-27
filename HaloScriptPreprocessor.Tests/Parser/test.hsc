; Copyright (c) num0005. Some rights reserved
; Released under the MIT License, see LICENSE.md for more information.


; example file

; single line comment
"this is a multi-line
comment
it's similar to python
"

(constglobal string project "Project Mapping")

;*
This is a multi line comment
*;


(script macro void record (string message)
	;* multi
line
*; (print "bungie had some bad ideas")
	(print message)
	(log_print message)
)

(script startup example_script_main
	(record project)
	(loop i 1 5 
		(physics_set_gravity i)
		(sleep (* i 15))
	)
)

