using System;
using UnityEngine;

namespace TyVoronoi {
    /// <summary>
    /// Class representing edge of Voronoi diagram
    /// An edge sits between two regions (sites)
    /// Start and End vertices are found as circle events are processed
    /// </summary>
    public class Edge {
        public float slope;
        public float intercept;
        public Vector3[] vertices;
        public bool done;
        public Vector3 upperSite;
        public Vector3 lowerSite;
        int vertexCount;

        public Vector3 bisector {
            get {
                return lowerSite + (upperSite-lowerSite)/2f;
            }
        }

        public Edge(Vector3 lowerSite, Vector3 upperSite) {
            this.vertices = new Vector3[2];
            this.upperSite = upperSite;
            this.lowerSite = lowerSite;
            this.done = false;

            // compute slope/intercept based on lower/upper sites
            // y = m*x + b
            // -- find bisector of lower/upper sites
            var bisector = this.bisector;
            // slope of edge is inverse of slope of line between upper/lower sites, so run over rise
            if (lowerSite.y != upperSite.y) {
                this.slope = -(upperSite.x-lowerSite.x)/(upperSite.y-lowerSite.y);
                // solve for b
                // b = y - m*x
                this.intercept = bisector.y - (this.slope * bisector.x);
            } else {
                this.slope = float.NaN;
                this.intercept = float.NaN;
            }
        }

        public void AssignVertex(Vector3 vertex, int vertexIndex) {
            vertices[vertexIndex] = vertex;
            vertexCount++;
            done = vertices[0] != default(Vector3) && vertices[1] != default(Vector3);
            Debug.Log(String.Format("edge: {0} AssignVertex[{1}]: {2}: done: {3}",
                this, vertexIndex, vertex, done));
        }

        public override string ToString() {
            //return String.Format("us: {0} ls: {1}", upperSite, lowerSite);
            return String.Format("E({0},{1})({2},{3})",
                (int)lowerSite.x, (int)lowerSite.y,
                (int)upperSite.x, (int)upperSite.y);
        }

    }
}
