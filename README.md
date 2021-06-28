# HaloScriptExtended

[![.NET 5](https://github.com/num0005/HaloScriptExtended/actions/workflows/dotnet.yml/badge.svg)](https://github.com/num0005/HaloScriptExtended/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/num0005/HaloScriptExtended/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/num0005/HaloScriptExtended/actions/workflows/codeql-analysis.yml)

An extension to the HaloScript language used in the [Halo game series](https://en.wikipedia.org/wiki/Halo_(franchise)) and [Stubs the Zombie](https://en.wikipedia.org/wiki/Stubbs_the_Zombie_in_Rebel_Without_a_Pulse).

# Features

## Loops
Loops are supported if the start and end values can be evaulated at compile time
```lisp
; (loop <iterator> <start> <end> [expressions])

(loop i 1 (+ 2 2) 
	(physics_set_gravity i)
	(sleep (* i 15))
)
```

## Macros

```lisp
; (script macro <return type> <name> (<argument type> <argument name) ...) [expressions])

(script macro void record (string message)
	(print message)
	(log_print message)
)

(script startup example_script_main
	(record project)
)
```


Macros are not currently type checked.

## Constant globals

Similar to standard HaloScript globals but readonly.

## Optimisation

The transpiler will attempt to optimise the code.
