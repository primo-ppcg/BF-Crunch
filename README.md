# BF Crunch #

Crunches BF programs of the form
```
{...s2}<{s1}<{s0}[{k0}[<{j0}>{j1}>{c0}>{c1}>{c2...}<<<]{h}>{k1}]
```
of the given length. Programs of this form fill the tape with simple recurrence relations.

The shortest useful program of this type has length 14:
```
+[[<+>->++<]>]
```
which computes the powers of 2 as _f<sub>n</sub> = 2*f<sub>n-1</sub>_, _f<sub>0</sub> = 1_

## Usage ##

```
Usage: bfcrunch [--options] text [limit]

Arguments
------------------------------------------------------------
  text              The text to produce.
  limit             The maximum BF program length to search for. If zero, the length of the
                    shortest program found so far will be used (-r). Default = 0

Options
------------------------------------------------------------
  -i, --max-init=#  The maximum length of the initialization segment. If excluded, the program
                    will run indefinitely.
  -I, --min-init=#  The minimum length of the initialization segment. Default = 14
  -t, --max-tape=#  The maximum tape size to consider. Programs that utilize more tape than
                    this will be ignored. Default = 1250
  -T, --min-tape=#  The minimum tape size to consider. Programs that utilize less tape than
                    this will be ignored. Default = 1
  -r, --rolling-limit
                    If set, the limit will be adjusted whenever a shorter program is found.
  -?, --help        Display this help text.
```

## Output ##

Output is given in three lines:
 1. Total length of the program found, and the initialization segment.
 2. Path taken, starting with the current tape pointer. Each node corresponds to one character of output, represented as (pointer, cost).
 3. Utilized tape segment.
 
For example, the final result for `bfcrunch "hello world" 70 -r -i23` is:
```
64: ++++[[<+>->+++++>+<<]>]
49, (45, 5), (44, 3), (45, 6), (45, 1), (45, 4), (42, 4), (43, 5), (45, 3), (45, 4), (46, 2), (44, 4)
32, 116, 100, 104, 108, 132, 0, 0, 132, 0
```
This corresponds to the full program:
```
++++[[<+>->+++++>+<<]>]<<<<.<+.>++++..+++.<<<.>+++.>>.+++.>.<<-.
```
