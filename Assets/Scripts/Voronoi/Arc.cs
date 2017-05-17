using System;
using UnityEngine;

namespace TyVoronoi {

    public class Arc : IComparable<Arc>  {
        static int nextIndex;
        public int index;
        public Vector2 site;
        public CircleEvent circleEvent;
        public Breakpoint upperBreakpoint;
        public Breakpoint lowerBreakpoint;

        public Int32 CompareTo(Arc other) {
            return this.index.CompareTo(other.index);
        }

        static Arc() {
            nextIndex = 0;
        }

        public Arc(Vector2 site) {
            this.index = nextIndex++;
            this.site = site;
        }

        public override string ToString() {
            return String.Format("A{0}({1},{2})[{3}:{4}]",
                index,
                (int) site.x,
                (int) site.y,
                lowerBreakpoint,
                upperBreakpoint);
        }
    }
}
