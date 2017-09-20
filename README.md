# Rebel

## Introduction

REBEL stands for Regular Expression Based Esoteric Language. REBEL is a declarative programming language that works by iteratively replacing substrings.

A REBEL program contains three parts: An initial state, a list of regexes, and a list of replacement strings. These parts are separated by forward slashes. The initial state simply initializes the program's state to the given string. This state acts as the memory of the program. The regexes and replacements are grouped into pairs. Each regex pairs with the replacement string following it.

To execute a program, the interpreter operates similar to this pseudocode:

```
Set state = _initialState
Loop While True
	Loop Foreach pair In _pairs
		Set match = state.match(pair.regex)
		If match.success
			state = state.replace(match, pair.replacement)
			Continue Outer Loop
		End
	End
	Exit
End
```

That is, each iteration of the program it finds the first matching regex and replaces the match. When none of the regexes match, the program terminates.
The default file extension for REBEL programs is .re.

## Hello, World!

None of this will be very clear without an example, so here is the canonical Hello World written in REBEL:

```
Hello, World!/.+/$>$0
```

To understand the program, you need to split it into its constituent parts. Splitting on the forward slashes, we get:

```
Hello, World!
.+
$>$0
```

The first part is the initial state. When the program starts, the state is set to `Hello, World!`.

The next two lines represent a regex/replacement pair. When `.+` is encountered, it is replaced with `$>$0`. This is not a literal replacement. A `$` signifies a special replacement. There are two special replacements here. The first is `$>`, which outputs the rest of the replacement string to stdout, and evaluates to an empty string in the replacement result. The second is `$0`, which evaluates to the text matched by the regular expression. Together, they cause the entire match to be printed on stdout and replaced with an empty string.

Here is what goes on internally during the execution of the program:

The state is set to `Hello, World!`.
`.+` matches `Hello, World!`.
`$>$0` is evaluated. It prints `Hello, World!` and evaluates to the empty string.
`Hello, World!` is replaced with the empty string. The state is now the empty string.
None of the regexes match the empty string. The program terminates.
Here are some variations of the Hello World program:

```
a/a/$>Hello, World!
```
```
/^$/a$>Hello, World!
```

## Details

### Syntax

A REBEL program consists of a number of strings separated by unescaped forward slashes. A valid program must have an odd number of such strings. The first string in the program is the initial state. All strings after that are grouped into regex/replacement pairs. Each pair's first item is a regex, and its second is a replacement string.

Forward slashes can be included in these substrings by escaping them with a preceding backslash. Backslashes can be escaped with another backslash. In the initial state and replacement strings, escaping backslashes are removed, but in regex strings they are not.

### Regex

REBEL uses .NET-compatible regex, which is described [here](http://msdn.microsoft.com/en-us/library/az24scfc.aspx).

### Replacement

REBEL uses a superset of the .NET substitution strings, which is described [here](http://msdn.microsoft.com/en-us/library/ewy2t5e0.aspx).

There are two additional replacement patterns, used for I/O. First is `$<`, which reads a line of input from stdin and evaluates to the result. Second is `$>`, which prints the rest of the replacement string to stdout and evaluates to an empty string.

### Execution

A program begins by setting the state to the provided string. It then repeatedly finds and replaces regex matches.

The first regex that matches any part of the string at each step is then replaced by the corresponding replacement string. When none of the regexes match, the program terminates.

## Tools

The official package has a command line interpreter and the Visual REBEL IDE and debugger. Requires the .NET Framework 4.5.

[rebel.zip](rebel.zip)

## Sample Programs

[Cat](cat.re)

[99 bottles of beer](99bottles.re)

[Quine](quine.re)
