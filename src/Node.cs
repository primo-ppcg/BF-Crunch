using System;

namespace ppcg {
    public struct Node : IEquatable<Node> {

        public int Pointer;
        public int Cost;
        public bool Rolling;

        public Node(int pointer, int cost, bool rolling = false) {
            this.Pointer = pointer;
            this.Cost = cost;
            this.Rolling = rolling;
        }

        public override string ToString() {
            return string.Format("({0} {1})", this.Pointer, this.Cost);
        }

        public bool Equals(Node other) {
            return this.Pointer == other.Pointer;
        }

    }
}
