using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ppcg.argparse;
using ppcg.math;

namespace ppcg {
    public class KSabCruncher {

        private int min_tape;
        private int max_tape;
        private int max_node_cost;
        private int max_loops;

        private int min_slen;
        private int max_slen;
        private int min_clen;
        private int max_clen;

        private byte[] goal;
        private int limit;
        private bool rolling_limit;
        private bool unique_cells;

        public KSabCruncher(MyOptions options) {
            this.min_tape = options.MinTape;
            this.max_tape = options.MaxTape;
            this.max_node_cost = options.MaxNodeCost;
            this.max_loops = options.MaxLoops;

            this.min_slen = options.MinSLen;
            this.max_slen = options.MaxSLen ?? int.MaxValue;
            this.min_clen = options.MinCLen;
            this.max_clen = options.MaxCLen ?? int.MaxValue;

            this.goal = Encoding.GetEncoding("ISO-8859-1").GetBytes(Regex.Unescape(options.Text));

            if(options.Limit.HasValue) {
                this.limit = options.Limit.Value;
                this.rolling_limit = options.RollingLimit;
            } else {
                int diff = 0;
                byte last = 0;
                foreach(byte b in goal) {
                    diff += MyMath.Abs(b - last);
                    last = b;
                }
                this.limit = (diff / 3) + goal.Length + 20;
                this.rolling_limit = true;
            }

            this.unique_cells = options.UniqueCells;
        }

        /**
         * Crunches BF programs of the form
         * {...s2}<{s1}<{s0}[{j0}>{j1}>{j2...}[{c0}>{c1}>{c2...}<<<]{k0}>{k1}>{k2...}]
         * 
         * The shortest "useful" program of this type has length 7?
         * +[[<]+]
         */
        public void Crunch(int len) {
            Console.WriteLine(string.Format("init-len: {0}; limit: {1}", len, limit));
            int cmin = Math.Max(this.min_clen, 1);
            int smin = Math.Max(this.min_slen, 1);
            int klimit = len - (4 + cmin + smin);
            for(int klen = 1; klen <= klimit; klen++) {
                Console.Write(string.Format("{0:0.000}%\r", (klen - 1) * 100f / klimit));
                foreach(List<int> k in KListGen(klen)) {
                    int[] karr = k.ToArray();

                    int climit = Math.Min(this.max_clen, len - klen - (4 + smin));
                    for(int clen = cmin; clen <= climit; clen++)
                    foreach(List<int> c in CListGen(clen)) {
                        int[] carr = c.ToArray();

                        int slimit = Math.Min(this.max_slen, len - klen - clen - 4);
                        for(int slen = smin; slen <= slimit; slen++)
                        foreach(List<int> s in SListGen(slen)) {
                            int[] sarr = s.ToArray();

                            int jlen = len - klen - clen - slen - 4;
                            foreach(List<int> j in KListGen(jlen)) {
                                int[] jarr = j.ToArray();
                                int lastk = karr[karr.Length - 1];
                                int firstj = jarr[0];
                                if(lastk > 0 && firstj < 0 || lastk < 0 && firstj > 0) {
                                    continue;
                                }

                                byte[] tape;
                                int max_pntr;
                                int pntr = FillTape(sarr, carr, karr, jarr, out tape, out max_pntr);
                                if(pntr > 0) {

                                    Solver solver1 = new Solver(goal, tape, pntr, max_pntr, max_node_cost, unique_cells);
                                    Path solution1 = solver1.Solve(this.limit - len);

                                    if(solution1 is Path) {
                                        int min_pntr = Math.Min(pntr, solution1.Min(node => node.Pointer));
                                        Console.WriteLine(string.Format("{0}: {1}", solution1.Cost + len, ToBFString(sarr, carr, karr, jarr)));
                                        Console.WriteLine(string.Format("{0}, {1}", pntr, solution1));
                                        Console.WriteLine(string.Join(", ", tape.Skip(min_pntr).Take(max_pntr - min_pntr + 1)));
                                        if(rolling_limit) {
                                            this.limit = solution1.Cost + len;
                                        }
                                    }
                                }

                                if(sarr.Length > 1) {
                                    pntr = FillTape2(sarr, carr, karr, jarr, out tape, out max_pntr);
                                    if(pntr > 0) {

                                        Solver solver2 = new Solver(goal, tape, pntr, max_pntr, max_node_cost, unique_cells);
                                        Path solution2 = solver2.Solve(this.limit - len);

                                        if(solution2 is Path) {
                                            int min_pntr = Math.Min(pntr, solution2.Min(node => node.Pointer));
                                            Console.WriteLine(string.Format("{0}: {1}", solution2.Cost + len, ToBFString(sarr, carr, karr, jarr)));
                                            Console.WriteLine(string.Format("{0}, {1}", pntr, solution2));
                                            Console.WriteLine(string.Join(", ", tape.Skip(min_pntr).Take(max_pntr - min_pntr + 1)));
                                            if(rolling_limit) {
                                                this.limit = solution2.Cost + len;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

        private int FillTape(int[] s, int[] c, int[] k, int[] j, out byte[] tape, out int max_pntr) {
            tape = new byte[max_tape * 2];
            int pntr = max_tape;
            int min_pntr = pntr;
            max_pntr = pntr;
            int loops = 0;

            for(int i = 0; i < s.Length; i++) {
                tape[pntr + i] = (byte)s[i];
            }

            try {
                while(tape[pntr] != 0) {
                    for(int i = 0; i < j.Length; i++) {
                        tape[pntr + i] += (byte)j[i];
                    }
                    pntr += j.Length - 1;
                    max_pntr = Math.Max(max_pntr, pntr);
                    while(tape[pntr] != 0) {
                        for(int i = 0; i < c.Length; i++) {
                            tape[pntr + i] += (byte)c[i];
                        }
                        pntr--;
                    }
                    min_pntr = Math.Min(min_pntr, pntr);
                    for(int i = 0; i < k.Length; i++) {
                        tape[pntr + i] += (byte)k[i];
                    }
                    pntr += k.Length - 1;
                    max_pntr = Math.Max(max_pntr, pntr);

                    if(loops++ > max_loops) {
                        return 0;
                    }
                }

                tape = tape.Skip(min_pntr - 1).Take(max_pntr - min_pntr + 2).ToArray();
                max_pntr -= min_pntr - 1;

                return pntr - (min_pntr - 1);
            } catch {
                return 0;
            }
        }

        private int FillTape2(int[] s, int[] c, int[] k, int[] j, out byte[] tape, out int max_pntr) {
            tape = new byte[max_tape * 2];
            int pntr = max_tape;
            int min_pntr = pntr;
            max_pntr = pntr;
            int loops = 0;

            for(int i = 0; i < s.Length; i++) {
                tape[pntr - i] = (byte)s[i];
            }

            try {
                while(tape[pntr] != 0) {
                    for(int i = 0; i < j.Length; i++) {
                        tape[pntr + i] += (byte)j[i];
                    }
                    pntr += j.Length - 1;
                    max_pntr = Math.Max(max_pntr, pntr);
                    while(tape[pntr] != 0) {
                        for(int i = 0; i < c.Length; i++) {
                            tape[pntr + i] += (byte)c[i];
                        }
                        pntr--;
                    }
                    min_pntr = Math.Min(min_pntr, pntr);
                    for(int i = 0; i < k.Length; i++) {
                        tape[pntr + i] += (byte)k[i];
                    }
                    pntr += k.Length - 1;
                    max_pntr = Math.Max(max_pntr, pntr);

                    if(loops++ > max_loops) {
                        return 0;
                    }
                }

                tape = tape.Skip(min_pntr - 1).Take(max_pntr - min_pntr + 2).ToArray();
                max_pntr -= min_pntr - 1;

                return pntr - (min_pntr - 1);
            } catch {
                return 0;
            }
        }

        /**
         * Generates all possible s-lists whose BF translation has the given length.
         */
        public static IEnumerable<List<int>> SListGen(int len, bool first = true) {
            if(len < 1) {
                yield return new List<int>(0);
            }
            for(int i = -len; i <= len; i++) {
                if(first && i == 0 || MyMath.Abs(i) == len - 1) {
                    continue;
                }
                foreach(List<int> tail in SListGen(len - MyMath.Abs(i) - 1, false)) {
                    tail.Insert(0, i);
                    yield return tail;
                }
            }
        }

        /**
         * Generates all possible c-lists whose BF translation has the given length.
         */
        public static IEnumerable<List<int>> CListGen(int len, bool first = true) {
            if(len < 1) {
                yield return new List<int>(0);
            }
            int j = first ? 1 : 2;
            for(int i = j - len; i <= len - j; i++) {
                if(i == 0 && len < 3 && !first) {
                    continue;
                }
                foreach(List<int> tail in CListGen(len - MyMath.Abs(i) - j, false)) {
                    tail.Insert(0, i);
                    yield return tail;
                }
            }
        }

        /**
         * Generates all possible k-lists whose BF translation has the given length.
         */
        public static IEnumerable<List<int>> KListGen(int len) {
            if(len < 0) {
                yield return new List<int>(0);
            }
            for(int i = -len; i <= len; i++) {
                foreach(List<int> tail in KListGen(len - MyMath.Abs(i) - 1)) {
                    tail.Insert(0, i);
                    yield return tail;
                }
            }
        }

        private static string ToBFString(int[] s, int[] c, int[] k, int[] j) {
            StringBuilder sb = new StringBuilder();
            char sdelim = '[';
            foreach(int sterm in s) {
                sb.Insert(0, sdelim);
                sb.Insert(0, sterm < 0 ? "-" : "+", MyMath.Abs(sterm));
                sdelim = '<';
            }
            for(int i = 0; i < j.Length; i++) {
                sb.Append(j[i] < 0 ? '-' : '+', MyMath.Abs(j[i]));
                sb.Append(i < j.Length - 1 ? '>' : '[');
            }
            for(int i = 0; i < c.Length; i++) {
                sb.Append(c[i] < 0 ? '-' : '+', MyMath.Abs(c[i]));
                sb.Append(i < c.Length - 1 ? '>' : '<');
            }
            sb.Append('<', c.Length - 1);
            sb.Append(']');
            for(int i = 0; i < k.Length; i++) {
                sb.Append(k[i] < 0 ? '-' : '+', MyMath.Abs(k[i]));
                sb.Append(i < k.Length - 1 ? '>' : ']');
            }

            return sb.ToString();
        }

    }
}
