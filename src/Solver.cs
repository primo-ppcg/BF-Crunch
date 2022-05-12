using System;
using System.Collections.Generic;
using System.Linq;

using ppcg.math;

namespace ppcg {
    public class Solver {

        private byte[] goal;
        private byte[] tape;
        private int min_cost;
        private int pointer;
        private int max_pointer;
        private int max_node_cost;
        private bool unique_cells;
        private int[] zeros;

        public Solver(byte[] goal, byte[] tape, int pointer, int max_pointer, int max_node_cost, bool unique_cells = false) {
            this.goal = goal;
            this.tape = tape;
            this.min_cost = (goal.Length - 1) * 2;
            this.min_cost -= Enumerable.Range(0, goal.Length - 1).Count(i => goal[i] == goal[i + 1]);
            this.pointer = pointer;
            this.max_pointer = max_pointer;
            this.max_node_cost = max_node_cost;
            this.unique_cells = unique_cells;
            this.zeros = Enumerable.Range(0, max_pointer + 1).Where(i => tape[i] == 0).ToArray();
        }

        public Path Solve(int max_cost) {
            return Exhaustive(0, 0, pointer, max_cost - min_cost);
        }

        public Path SolveExactTape(int max_cost) {
            int cost = 0;
            int start = 0;
            max_cost -= min_cost;

            int next_cost = (start + 1 == goal.Length ? 0 : (goal[start] == goal[start + 1] ? 1 : 2));

            Path min_path = null;
            int i_max = max_cost;

            for(int i = 1; i < i_max && i <= pointer; i++) {
                int ncost = MyMath.Abs(goal[start] - tape[pointer - i]) + i + 1;
                if(ncost <= i_max) {
                    Node node = new Node(pointer - i, ncost);
                    byte tval = tape[node.Pointer];
                    tape[node.Pointer] = goal[start];
                    Path subpath = ExactTape(cost + ncost, start + 1, node.Pointer - 1, max_cost + next_cost);
                    tape[node.Pointer] = tval;

                    if(subpath is Path) {
                        if(min_path == null || subpath.Cost + ncost < min_path.Cost) {
                            subpath.AddFirst(node);
                            min_path = subpath;
                        }
                    }
                }
            }

            return min_path;
        }

        private Path ExactTape(int cost, int start, int pointer, int max_cost) {
            if(start == goal.Length) {
                return new Path();
            }

            if(pointer < 0) {
                return null;
            }

            int next_cost = (start + 1 == goal.Length ? 0 : (goal[start] == goal[start + 1] ? 1 : 2));

            Path min_path = null;
            int i_max = max_cost - cost;

            int ncost = MyMath.Abs(goal[start] - tape[pointer]) + 1;
            if(ncost <= i_max) {
                Node node = new Node(pointer, ncost);
                byte tval = tape[node.Pointer];
                tape[node.Pointer] = goal[start];
                Path subpath = ExactTape(cost + ncost, start + 1, node.Pointer - 1, max_cost + next_cost);
                tape[node.Pointer] = tval;

                if(subpath is Path) {
                    if(min_path == null || subpath.Cost + ncost < min_path.Cost) {
                        subpath.AddFirst(node);
                        min_path = subpath;
                    }
                }
            }

            return min_path;
        }

        private Path Exhaustive(int cost, int start, int pointer, int max_cost) {
            // lots of code repetition for the sake of speed :/

            //int[] zeros = Enumerable.Range(0, max_pointer + 1).Where(x => tape[x] == 0).ToArray();

            if(start == goal.Length) {
                return new Path();
            }

            int next_cost = (start + 1 == goal.Length ? 0 : (goal[start] == goal[start + 1] ? 1 : 2));

            Path min_path = null;
            int i_max = Math.Min(max_cost - cost, max_node_cost);

            {
                int ncost = MyMath.Abs(goal[start] - tape[pointer]) + 1;
                if(ncost <= i_max) {
                    Node node = new Node(pointer, ncost);
                    byte tval = tape[node.Pointer];
                    int[] oldzeros = zeros;
                    if(tape[node.Pointer] == 0) {
                        zeros = Enumerable.Range(0, max_pointer + 1).Where(i => tape[i] == 0).ToArray();
                    }
                    tape[node.Pointer] = goal[start];
                    Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                    tape[node.Pointer] = tval;
                    zeros = oldzeros;

                    if(subpath is Path) {
                        if(
                            (min_path == null || subpath.Cost + ncost < min_path.Cost) &&
                            (!unique_cells || !subpath.Contains(node))
                        ) {
                            subpath.AddFirst(node);
                            min_path = subpath;
                        }
                    }
                }
            }

            for(int i = 1; i < i_max && i <= pointer; i++) {
                int ncost = MyMath.Abs(goal[start] - tape[pointer - i]) + i + 1;
                if(ncost <= i_max) {
                    Node node = new Node(pointer - i, ncost);
                    byte tval = tape[node.Pointer];
                    int[] oldzeros = zeros;
                    if(tape[node.Pointer] == 0) {
                        zeros = Enumerable.Range(0, max_pointer + 1).Where(x => tape[x] == 0).ToArray();
                    }
                    tape[node.Pointer] = goal[start];
                    Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                    tape[node.Pointer] = tval;
                    zeros = oldzeros;

                    if(subpath is Path) {
                        if(
                            (min_path == null || subpath.Cost + ncost < min_path.Cost) &&
                            (!unique_cells || !subpath.Contains(node))
                        ) {
                            subpath.AddFirst(node);
                            min_path = subpath;
                        }
                    }
                }
            }

            for(int i = 1; i < i_max && i <= max_pointer - pointer; i++) {
                int ncost = MyMath.Abs(goal[start] - tape[pointer + i]) + i + 1;
                if(ncost <= i_max) {
                    Node node = new Node(pointer + i, ncost);
                    byte tval = tape[node.Pointer];
                    int[] oldzeros = zeros;
                    if(tape[node.Pointer] == 0) {
                        zeros = Enumerable.Range(0, max_pointer + 1).Where(x => tape[x] == 0).ToArray();
                    }
                    tape[node.Pointer] = goal[start];
                    Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                    tape[node.Pointer] = tval;
                    zeros = oldzeros;

                    if(subpath is Path) {
                        if(
                            (min_path == null || subpath.Cost + ncost < min_path.Cost) &&
                            (!unique_cells || !subpath.Contains(node))
                        ) {
                            subpath.AddFirst(node);
                            min_path = subpath;
                        }
                    }
                }
            }

            // zip to previous zero [<]
            int pj = 0;
            while(pj <= pointer && tape[pointer - pj] == 0) {
                pj++;
            }
            if(pj <= pointer) {
                int zcost = 3 + pj;
                int pz_idx = MyMath.BinarySearch(zeros, pointer - pj) - 1;
                if(pz_idx >= 0) {
                    int prev_zero = zeros[pz_idx];
                    for(int i = 1; i < i_max - 3 && i <= prev_zero; i++) {
                        int ncost = MyMath.Abs(goal[start] - tape[prev_zero - i]) + i + 1 + zcost;
                        if(ncost <= i_max) {
                            Node node = new Node(prev_zero - i, ncost);
                            byte tval = tape[node.Pointer];
                            int[] oldzeros = zeros;
                            if(tape[node.Pointer] == 0) {
                                zeros = Enumerable.Range(0, max_pointer + 1).Where(x => tape[x] == 0).ToArray();
                            }
                            tape[node.Pointer] = goal[start];
                            Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                            tape[node.Pointer] = tval;
                            zeros = oldzeros;

                            if(subpath is Path) {
                                if(
                                    (min_path == null || subpath.Cost + ncost < min_path.Cost) &&
                                    (!unique_cells || !subpath.Contains(node))
                                ) {
                                    subpath.AddFirst(node);
                                    min_path = subpath;
                                }
                            }
                        }
                    }

                    for(int i = 1; i < i_max - 3 && i <= (pointer - prev_zero - 3) >> 1; i++) {
                        int ncost = MyMath.Abs(goal[start] - tape[prev_zero + i]) + i + 1 + zcost;
                        if(ncost <= i_max) {
                            Node node = new Node(prev_zero + i, ncost);
                            byte tval = tape[node.Pointer];
                            int[] oldzeros = zeros;
                            if(tape[node.Pointer] == 0) {
                                zeros = Enumerable.Range(0, max_pointer + 1).Where(x => tape[x] == 0).ToArray();
                            }
                            tape[node.Pointer] = goal[start];
                            Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                            tape[node.Pointer] = tval;
                            zeros = oldzeros;

                            if(subpath is Path) {
                                if(
                                    (min_path == null || subpath.Cost + ncost < min_path.Cost) &&
                                    (!unique_cells || !subpath.Contains(node))
                                ) {
                                    subpath.AddFirst(node);
                                    min_path = subpath;
                                }
                            }
                        }
                    }

                    // roll to previous zero [.<]
                    int run_len = pointer - pj - prev_zero;
                    while(pj < 1 && pointer - pj < max_pointer && tape[pointer - pj + 1] != 0 && run_len < goal.Length - start) {
                        pj--;
                        run_len++;
                    }
                    for(; run_len >= 4 && run_len <= goal.Length - start; pj++, run_len--) {
                        Path subpath = new Path();
                        int ncost = MyMath.Abs(goal[start] - tape[pointer - pj]) + MyMath.Abs(pj) + 1;
                        if(ncost <= i_max) {
                            subpath.AddLast(new Node(pointer - pj, ncost + 3));
                            int dcost = next_cost;
                            for(int i = 1; i < run_len; i++) {
                                ncost = MyMath.Abs(goal[start + i] - tape[pointer - pj - i]);
                                if(ncost > i_max) {
                                    break;
                                }
                                subpath.AddLast(new Node(pointer - pj - i, ncost, true));
                                dcost += (start + i + 1 == goal.Length ? 0 : (goal[start + i] == goal[start + i + 1] ? 1 : 2));
                            }
                            if(subpath.Count == run_len && cost + subpath.Cost <= max_cost + dcost) {
                                byte[] temptape = new byte[run_len];
                                int pt = 0;
                                foreach(Node node in subpath) {
                                    temptape[pt] = tape[node.Pointer];
                                    tape[node.Pointer] = goal[start + pt];
                                    pt++;
                                }
                                Path subpath2 = Exhaustive(cost + subpath.Cost, start + run_len, prev_zero, max_cost + dcost);
                                pt = 0;
                                foreach(Node node in subpath) {
                                    tape[node.Pointer] = temptape[pt];
                                    pt++;
                                }
                                if(subpath2 is Path) {
                                    if(
                                        (min_path == null || subpath.Cost + subpath2.Cost < min_path.Cost)
                                    ) {
                                        min_path = new Path(subpath.Concat(subpath2));
                                    }
                                }
                            }
                        }
                    }

                }
            }


            // zip to next zero [>]
            int nj = 0;
            while(nj + pointer <= max_pointer && tape[pointer + nj] == 0) {
                nj++;
            }
            if(nj + pointer <= max_pointer) {
                int zcost = 3 + nj;
                int nz_idx = MyMath.BinarySearch(zeros, pointer + nj);
                if(nz_idx < zeros.Length) {
                    int next_zero = zeros[nz_idx];
                    for(int i = 1; i < i_max - 3 && i <= max_pointer - next_zero; i++) {
                        int ncost = MyMath.Abs(goal[start] - tape[next_zero + i]) + i + 1 + zcost;
                        if(ncost <= i_max) {
                            Node node = new Node(next_zero + i, ncost);
                            byte tval = tape[node.Pointer];
                            int[] oldzeros = zeros;
                            if(tape[node.Pointer] == 0) {
                                zeros = Enumerable.Range(0, max_pointer + 1).Where(x => tape[x] == 0).ToArray();
                            }
                            tape[node.Pointer] = goal[start];
                            Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                            tape[node.Pointer] = tval;
                            zeros = oldzeros;

                            if(subpath is Path) {
                                if(
                                    (min_path == null || subpath.Cost + ncost < min_path.Cost) &&
                                    (!unique_cells || !subpath.Contains(node))
                                ) {
                                    subpath.AddFirst(node);
                                    min_path = subpath;
                                }
                            }
                        }
                    }

                    for(int i = 1; i < i_max - 3 && i <= (next_zero - pointer - 3) >> 1; i++) {
                        int ncost = MyMath.Abs(goal[start] - tape[next_zero - i]) + i + 1 + zcost;
                        if(ncost <= i_max) {
                            Node node = new Node(next_zero - i, ncost);
                            byte tval = tape[node.Pointer];
                            int[] oldzeros = zeros;
                            if(tape[node.Pointer] == 0) {
                                zeros = Enumerable.Range(0, max_pointer + 1).Where(x => tape[x] == 0).ToArray();
                            }
                            tape[node.Pointer] = goal[start];
                            Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                            tape[node.Pointer] = tval;
                            zeros = oldzeros;

                            if(subpath is Path) {
                                if(
                                    (min_path == null || subpath.Cost + ncost < min_path.Cost) &&
                                    (!unique_cells || !subpath.Contains(node))
                                ) {
                                    subpath.AddFirst(node);
                                    min_path = subpath;
                                }
                            }
                        }
                    }

                    // roll to next zero [.>]
                    int run_len = next_zero - pointer - nj;
                    while(nj < 1 && pointer + nj > 0 && tape[pointer + nj - 1] != 0 && run_len < goal.Length - start) {
                        nj--;
                        run_len++;
                    }
                    for(; run_len >= 4 && run_len <= goal.Length - start; nj++, run_len--) {
                        Path subpath = new Path();
                        int ncost = MyMath.Abs(goal[start] - tape[pointer + nj]) + MyMath.Abs(nj) + 1;
                        if(ncost <= i_max) {
                            subpath.AddLast(new Node(pointer + nj, ncost + 3));
                            int dcost = next_cost;
                            for(int i = 1; i < run_len; i++) {
                                ncost = MyMath.Abs(goal[start + i] - tape[pointer + nj + i]);
                                if(ncost > i_max) {
                                    break;
                                }
                                subpath.AddLast(new Node(pointer + nj + i, ncost, true));
                                dcost += (start + i + 1 == goal.Length ? 0 : (goal[start + i] == goal[start + i + 1] ? 1 : 2));
                            }
                            if(subpath.Count == run_len && cost + subpath.Cost <= max_cost + dcost) {
                                byte[] temptape = new byte[run_len];
                                int pt = 0;
                                foreach(Node node in subpath) {
                                    temptape[pt] = tape[node.Pointer];
                                    tape[node.Pointer] = goal[start + pt];
                                    pt++;
                                }
                                Path subpath2 = Exhaustive(cost + subpath.Cost, start + run_len, next_zero, max_cost + dcost);
                                pt = 0;
                                foreach(Node node in subpath) {
                                    tape[node.Pointer] = temptape[pt];
                                    pt++;
                                }
                                if(subpath2 is Path) {
                                    if(
                                        (min_path == null || subpath.Cost + subpath2.Cost < min_path.Cost)
                                    ) {
                                        min_path = new Path(subpath.Concat(subpath2));
                                    }
                                }
                            }
                        }
                    }

                }
            }

            return min_path;
        }

    }
}
