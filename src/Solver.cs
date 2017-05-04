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
        private int[] zeros;

        public Solver(byte[] goal, byte[] tape, int pointer, int max_pointer) {
            this.goal = goal;
            this.tape = tape;
            this.min_cost = (goal.Length - 1) * 2;
            this.min_cost -= Enumerable.Range(0, goal.Length - 1).Count(i => goal[i] == goal[i + 1]);
            this.pointer = pointer;
            this.max_pointer = max_pointer;
            this.zeros = Enumerable.Range(0, max_pointer + 1).Where(i => tape[i] == 0).ToArray();
        }

        public Path Solve(int max_cost) {
            return Exhaustive(0, 0, pointer, max_cost - min_cost);
        }

        private Path Exhaustive(int cost, int start, int pointer, int max_cost) {
            // lots of code repetition for the sake of speed :/

            if(start == goal.Length) {
                return new Path();
            }

            int next_cost = (start + 1 == goal.Length ? 0 : (goal[start] == goal[start + 1] ? 1 : 2));

            Path min_path = null;
            int i_max = max_cost - cost;

            {
                int ncost = MyMath.Abs(goal[start] - tape[pointer]) + 1;
                if(ncost <= i_max) {
                    Node node = new Node(pointer, ncost);
                    byte tval = tape[node.Pointer];
                    tape[node.Pointer] = goal[start];
                    Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                    tape[node.Pointer] = tval;

                    if(subpath is Path) {
                        if(min_path == null || subpath.Cost + ncost < min_path.Cost) {
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
                    tape[node.Pointer] = goal[start];
                    Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                    tape[node.Pointer] = tval;

                    if(subpath is Path) {
                        if(min_path == null || subpath.Cost + ncost < min_path.Cost) {
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
                    tape[node.Pointer] = goal[start];
                    Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                    tape[node.Pointer] = tval;

                    if(subpath is Path) {
                        if(min_path == null || subpath.Cost + ncost < min_path.Cost) {
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
                            tape[node.Pointer] = goal[start];
                            Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                            tape[node.Pointer] = tval;

                            if(subpath is Path) {
                                if(min_path == null || subpath.Cost + ncost < min_path.Cost) {
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
                            tape[node.Pointer] = goal[start];
                            Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                            tape[node.Pointer] = tval;

                            if(subpath is Path) {
                                if(min_path == null || subpath.Cost + ncost < min_path.Cost) {
                                    subpath.AddFirst(node);
                                    min_path = subpath;
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
                            tape[node.Pointer] = goal[start];
                            Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                            tape[node.Pointer] = tval;

                            if(subpath is Path) {
                                if(min_path == null || subpath.Cost + ncost < min_path.Cost) {
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
                            tape[node.Pointer] = goal[start];
                            Path subpath = Exhaustive(cost + ncost, start + 1, node.Pointer, max_cost + next_cost);
                            tape[node.Pointer] = tval;

                            if(subpath is Path) {
                                if(min_path == null || subpath.Cost + ncost < min_path.Cost) {
                                    subpath.AddFirst(node);
                                    min_path = subpath;
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
