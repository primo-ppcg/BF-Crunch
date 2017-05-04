using System;

namespace ppcg {
    public struct Node {

        public int Pointer;
        public int Cost;

        public Node(int pointer, int cost) {
            this.Pointer = pointer;
            this.Cost = cost;
        }

        public override string ToString() {
            return string.Format("({0}, {1})", this.Pointer, this.Cost);
        }

    }
}
