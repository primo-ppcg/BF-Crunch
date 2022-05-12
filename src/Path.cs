using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ppcg {
    [Serializable]
    public class Path : LinkedList<Node> {

        public int Cost {
            get {
                return this.Sum(node => node.Cost);
            }
        }

        public Path() { }

        public Path(IEnumerable<Node> nodes) : base(nodes) { }

        protected Path(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override string ToString() {
            return string.Join(", ", this);
        }

    }
}
