using System;
using UnityEngine;

namespace TyVoronoi {

    /// <summary>
    /// Class representing breakpoint along edge between two arcs of the beachline
    /// </summary>
    public class Breakpoint {
        public Edge edge;
        public Arc upperArc;
        public Arc lowerArc;
        public int intersectionIndex;

        public Breakpoint(
            Arc lowerArc,
            Arc upperArc,
            Edge edge,
            int intersectionIndex
        ) {
            this.lowerArc = lowerArc;
            this.upperArc = upperArc;
            this.edge = edge;
            this.intersectionIndex = intersectionIndex;
        }

        public override string ToString() {
            return String.Format("B{0}:{1}<{2}({3})>", lowerArc.index, upperArc.index, edge, intersectionIndex);
        }

    }
}
