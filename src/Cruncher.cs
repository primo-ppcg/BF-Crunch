using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ppcg.math;

namespace ppcg {
    public class Cruncher {

        private int min_tape;
        private int max_tape;

        private byte[] goal;
        private int limit;
        private bool rolling_limit;

        private static int[] modinv256 = new int[] {
            //  1       3       5       7      9      11      13      15      17      19
            0,  1, 0, 171, 0, 205, 0, 183, 0, 57, 0, 163, 0, 197, 0, 239, 0, 241, 0,  27,
            // 21      23      25      27     29      31      33      35      37      39
            0, 61, 0, 167, 0,  41, 0,  19, 0, 53, 0, 223, 0, 225, 0, 139, 0, 173, 0, 151
        };

        public Cruncher(MyOptions options) {
            this.min_tape = options.MinTape;
            this.max_tape = options.MaxTape;

            this.goal = Encoding.GetEncoding("ISO-8859-1").GetBytes(options.Text);

            if(options.Limit == 0) {
                int diff = 0;
                byte last = 0;
                foreach(byte b in goal) {
                    diff += MyMath.Abs(b - last);
                    last = b;
                }
                this.limit = (diff / 3) + goal.Length + 20;
                this.rolling_limit = true;
            } else {
                this.limit = options.Limit;
                this.rolling_limit = options.RollingLimit;
            }
        }

        /**
         * Crunches BF programs of the form
         * {...s2}<{s1}<{s0}[{k0}[<{j0}>{j1}>{c0}>{c1}>{c2...}<<<]{h}>{k1}]
         * of the given length.
         * 
         * The shortest useful program of this type has length 14
         * +[[<+>->++<]>]
         * which computes the powers of 2 as f(n) = 2*f(n-1), f(0) = 1
         */
        public void Crunch(int len) {
            Console.WriteLine(string.Format("init-len: {0}; limit: {1}", len, limit));
            for(int slen = 1; slen <= len - 12; slen++)
            foreach(List<int> s in SListGen(slen)) {

                for(int clen = 3; clen <= len - slen - 9; clen++)
                foreach(List<int> c in CListGen(clen)) {

                    for(int klen = 0; klen <= len - slen - clen - 9; klen++)
                    foreach(int[] k in KListGen(klen)) {

                        for(int jlen = 2; jlen <= len - slen - clen - klen - 7; jlen++)
                        foreach(int[] j in JListGen(jlen)) {

                            int hlen = len - slen - clen - klen - jlen - 7;
                            int[] hlist = hlen > 0 ? new int[] { -hlen, hlen } : new int[] { hlen };
                            foreach(int h in hlist) {
                                byte[] tape;
                                int pntr = FillTape(s, c, k[0], k[1], j[0], j[1], h, out tape);
                                int max_pntr = pntr + c.Count + 1;
                                if(pntr > 0 && max_pntr >= min_tape && max_pntr <= max_tape) {
                                    Solver solver1 = new Solver(goal, tape, pntr, max_pntr);
                                    Path solution1 = solver1.Solve(this.limit - len);
                                    if(solution1 is Path) {
                                        int min_pntr = solution1.Min(node => node.Pointer);
                                        Console.WriteLine(string.Format("{0}: {1}", solution1.Cost + len, ToBFString(s, c, k, j, h)));
                                        Console.WriteLine(string.Format("{0}, {1}", pntr, solution1));
                                        Console.WriteLine(string.Join(", ", tape.Skip(min_pntr).Take(max_pntr - min_pntr)));
                                        if(rolling_limit) {
                                            this.limit = solution1.Cost + len;
                                        }
                                    }

                                    if(c.Count > 1) {
                                        // same tape, different tail
                                        for(int i = 1; i <= c.Count; i++) {
                                            tape[pntr + i] = (byte)(-tape[pntr + i]);
                                        }
                                        Solver solver2 = new Solver(goal, tape, pntr, max_pntr);
                                        Path solution2 = solver2.Solve(this.limit - len);
                                        if(solution2 is Path && (solution1 == null || solution2.Cost < solution1.Cost)) {
                                            int min_pntr = solution2.Min(node => node.Pointer);
                                            List<int> sneg = s.Select(i => -i).ToList();
                                            int[] kneg = new int[] { -k[0], -k[1] };
                                            int[] jneg = new int[] { -j[0], j[1] };
                                            Console.WriteLine(string.Format("{0}: {1}", solution2.Cost + len, ToBFString(sneg, c, kneg, jneg, -h)));
                                            Console.WriteLine(string.Format("{0}, {1}", pntr, solution2));
                                            Console.WriteLine(string.Join(", ", tape.Skip(min_pntr).Take(max_pntr - min_pntr)));
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

        private int FillTape(List<int> s, List<int> c, int k0, int k1, int j0, int j1, int h, out byte[] tape) {
            tape = new byte[max_tape + 1];
            for(int i = 0; i < s.Count; i++) {
                tape[i + 2] = (byte)s[i];
            }

            int lsb = j1 & -j1;
            int mask = lsb - 1;
            int shift = 0;
            while((lsb & 1) == 0) {
                lsb >>= 1;
                shift += 1;
            }
            int m = modinv256[j1 >> shift];

            int[] carr = c.ToArray();

            // leave a zero at the beginning for a zip point
            int pntr = 2;
            int stop = max_tape - carr.Length;
            while(pntr < stop && tape[pntr] != 0) {
                tape[pntr] += (byte)k0;
                if(tape[pntr] != 0) {
                    if((tape[pntr] & mask) != 0) {
                        return 0;
                    }
                    int tmp = (tape[pntr] >> shift) * m;
                    for(int i = 0; i < carr.Length; i++) {
                        tape[pntr + i + 1] += (byte)(tmp * carr[i]);
                    }
                    tape[pntr - 1] += (byte)(tmp * j0);
                }
                tape[pntr] = (byte)h;
                pntr++;
                tape[pntr] += (byte)k1;
            }

            return pntr < stop ? pntr : 0;
        }

        /**
         * Generates all possible s-lists whose BF translation has the given length.
         */
        private static IEnumerable<List<int>> SListGen(int len, bool first = true) {
            if(len < 1) {
                yield return new List<int>(0);
            }
            for(int i = -len; i <= len; i++) {
                if(first && i == 0 || MyMath.Abs(i) == len - 1) {
                    continue;
                }
                foreach(List<int> tail in SListGen(len - MyMath.Abs(i) - 1, false)) {
                    List<int> slist = new List<int>() { i };
                    slist.AddRange(tail);
                    yield return slist;
                }
            }
        }

        /**
         * Generates all possible c-lists whose BF translation has the given length.
         */
        private static IEnumerable<List<int>> CListGen(int len) {
            if(len < 1) {
                yield return new List<int>(0);
            }
            for(int i = 2-len; i <= len-2; i++) {
                if(len < 3 && i == 0) {
                    continue;
                }
                foreach(List<int> tail in CListGen(len - MyMath.Abs(i) - 2)) {
                    List<int> clist = new List<int>() { i };
                    clist.AddRange(tail);
                    yield return clist;
                }
            }
        }

        /**
         * Generates all possible k-lists whose BF translation has the given length.
         */
        private static IEnumerable<int[]> KListGen(int len) {
            if(len == 0) {
                yield return new int[] { 0, 0 };
            } else {
                yield return new int[] { -len, 0 };
                for(int i = 1-len; i < len; i++) {
                    int k1 = len - MyMath.Abs(i);
                    yield return new int[] { i, k1 };
                    yield return new int[] { i, -k1 };
                }
                yield return new int[] { len, 0 };
            }
        }

        private static IEnumerable<int[]> JListGen(int len) {
            for(int i = 1; i < len; i++) {
                yield return new int[] { len - i, i };
            }
        }

        private static string ToBFString(List<int> s, List<int> c, int[] k, int[] j, int h) {
            StringBuilder sb = new StringBuilder();
            char sdelim = '[';
            foreach(int sterm in s) {
                sb.Insert(0, sdelim);
                sb.Insert(0, sterm < 0 ? "-" : "+", MyMath.Abs(sterm));
                sdelim = '<';
            }
            sb.Append(k[0] < 0 ? '-' : '+', MyMath.Abs(k[0]));
            sb.Append("[<");
            sb.Append(j[0] < 0 ? '-' : '+', MyMath.Abs(j[0]));
            sb.Append('>');
            sb.Append('-', j[1]);
            foreach(int cterm in c) {
                sb.Append('>');
                sb.Append(cterm < 0 ? '-' : '+', MyMath.Abs(cterm));
            }
            sb.Append('<', c.Count);
            sb.Append(']');
            sb.Append(h < 0 ? '-' : '+', MyMath.Abs(h));
            sb.Append('>');
            sb.Append(k[1] < 0 ? '-' : '+', MyMath.Abs(k[1]));
            sb.Append(']');

            return sb.ToString();
        }

    }
}
