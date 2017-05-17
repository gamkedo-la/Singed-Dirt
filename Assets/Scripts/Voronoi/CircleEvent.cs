using System;
using UnityEngine;

namespace TyVoronoi {
    public class CircleEvent : IComparable {
        public bool valid;
        public Arc arc;
        public Vector2 center;
        public Vector2 vertex;

        public CircleEvent(Arc arc, Vector2 center, Vector2 vertex) {
            this.valid = true;
            this.arc = arc;
            this.center = center;
            this.vertex = vertex;
        }

        public Int32 CompareTo(object otherObj) {
            CircleEvent other = (CircleEvent) otherObj;

            if (this.vertex.x < other.vertex.x) {
                return -1;
            }
            if (this.vertex.x > other.vertex.x) {
                return 1;
            }
            return 0;
        }

        public Int32 CompareTo(CircleEvent other) {
            if (this.vertex.x < other.vertex.x) {
                return -1;
            }
            if (this.vertex.x > other.vertex.x) {
                return 1;
            }
            return 0;
        }

        public override string ToString() {
            return String.Format("arc: {0} center: {1} int: {2}", arc, center, vertex);
        }

    }
}
