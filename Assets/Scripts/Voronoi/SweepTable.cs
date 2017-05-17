using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TyVoronoi {

    public class ArcInsertComparer : IComparer {
        int IComparer.Compare(object o1, object o2) {
            var a1 = (Arc) o1;
            var a2 = (Arc) o2;

            // Arc1 is the arc being inserted...
            // intersection should then be a single intersection at Arc1's site.y and current directrix is
            // Arc1's site.x
            var currentSweep = a1.site.x;

            // compute intersection of two arcs
            Vector2[] intersections;
            Geometry.FindParabolaIntersection(a1.site, a2.site, currentSweep, out intersections);
            Vector2 arcIntersection = intersections[0];

            // compute current intersection of Arc2 and bounding edges
            Vector2 lowerIntersection;
            Vector2 upperIntersection;
            if (a2.lowerBreakpoint != null) {
                Geometry.FindParabolaIntersection(
                    a2.lowerBreakpoint.edge.lowerSite,
                    a2.lowerBreakpoint.edge.upperSite,
                    currentSweep,
                    out intersections);
                lowerIntersection = intersections[a2.lowerBreakpoint.intersectionIndex];
                if (arcIntersection.y <= lowerIntersection.y) {
                    return -1;
                }
            }
            if (a2.upperBreakpoint != null) {
                Geometry.FindParabolaIntersection(
                    a2.upperBreakpoint.edge.lowerSite,
                    a2.upperBreakpoint.edge.upperSite,
                    currentSweep,
                    out intersections);
                upperIntersection = intersections[a2.upperBreakpoint.intersectionIndex];
                if (arcIntersection.y > upperIntersection.y) {
                    return 1;
                }
            }

            return 0;
        }
    }

    public class Beachline: BinarySearchTree<Arc> {
        IComparer insertComparer;
        public Beachline(): base() {
            insertComparer = new ArcInsertComparer();
        }

        public override object Insert(
            Arc arc,
            IComparer comparer = null
        ) {
            if (comparer == null) comparer = insertComparer;
            if (root == null) {
                return base.Insert(arc);
            }

            // find arc within beachline that intersects w/ new arc
            BinarySearchTreeNode<Arc> intersectingArcNode = (BinarySearchTreeNode<Arc>) base.GetNode(arc, comparer);
            Arc intersectingArc = GetNodeData(intersectingArcNode);
            //Debug.Log("intersectingArcNode: " + intersectingArcNode + " intersectingArc: " + intersectingArc);

            // edge is created between site of new arc and site of intersecting arc
            var edge = new Edge(intersectingArc.site, arc.site);

            // intersecting arc is replaced by three arcs and two breakpoints
            var lowerArc = intersectingArc;
            var middleArc = arc;
            var upperArc = new Arc(intersectingArc.site);
            var lowerBreakpoint = new Breakpoint(lowerArc, middleArc, edge, 0);
            var upperBreakpoint = new Breakpoint(middleArc, upperArc, edge, 1);

            // update breakpoints for each arc
            if (lowerArc.upperBreakpoint != null) {
                var oldBreakpoint = lowerArc.upperBreakpoint;
                var newBreakpoint = new Breakpoint(
                    upperArc,
                    oldBreakpoint.upperArc,
                    oldBreakpoint.edge,
                    oldBreakpoint.intersectionIndex);
                upperArc.upperBreakpoint = newBreakpoint;
                if (oldBreakpoint.upperArc != null) {
                    oldBreakpoint.upperArc.lowerBreakpoint = newBreakpoint;
                }
            }
            upperArc.lowerBreakpoint = upperBreakpoint;
            middleArc.upperBreakpoint = upperBreakpoint;
            middleArc.lowerBreakpoint = lowerBreakpoint;
            lowerArc.upperBreakpoint = lowerBreakpoint;
            //lowerArc.lowerBreakpoint remains intact

            // insert new nodes
            // this means that arc and upperArc are added as new nodes
            // lowerArc is original arc
            // new arc becomes parent of lower/upper arcs
            // create new bst nodes
            var arcNode = new BinarySearchTreeNode<Arc>(arc);
            var upperArcNode = new BinarySearchTreeNode<Arc>(upperArc);

            // link BST nodes
            // arc node is the new parabola being added and becomes root of 3 node cluster
            arcNode.parent = intersectingArcNode.parent;
            arcNode.left = intersectingArcNode;
            arcNode.right = upperArcNode;
            if (arcNode.parent == null) {
                root = arcNode;
            } else {
                if (arcNode.parent.right == intersectingArcNode) {
                    arcNode.parent.right = arcNode;
                } else {
                    arcNode.parent.left = arcNode;
                }
            }

            // upper arc is the new arc split from intersecting arc by new parabola
            // left child is null, right child becomes original arc's right
            upperArcNode.parent = arcNode;
            upperArcNode.right = intersectingArcNode.right;
            if (upperArcNode.right != null) {
                upperArcNode.right.parent = upperArcNode;
            }

            // lower arc is the original intersecting arc node
            // left child remains intact, right is set to null
            intersectingArcNode.parent = arcNode;
            intersectingArcNode.right = null;

            return arcNode;

        }

        public override object GetNode(
            Arc arc,
            IComparer comparer=null
        ) {
            // in order traversal of nodes to find matching arc node
            var node = _FindMinNode(root);
            while (node != null) {
                if (node.data.index == arc.index) {
                    return (object) node;
                }
                node = (BinarySearchTreeNode<Arc>) GetSuccessor(node);
            }
            return null;
        }

        public override void Delete(
            Arc arc,
            IComparer comparer=null
        ) {
            if (comparer == null) comparer = this.comparer;
            // removal of arc requires that adjacent arcs are updated
            // needs to be removed from beachline
            // a new edge and associated breakpoints are added instead

            // find adjacent nodes
            var arcNode = (BinarySearchTreeNode<Arc>) GetNode(arc);
            var lowerNode = GetPredecessor(arcNode);
            var upperNode = GetSuccessor(arcNode);
            /*
            Debug.Log(String.Format("ST Delete arc: {0} arcNode: {1} lowerNode: {2} upperNode: {3}",
                arc, arcNode, lowerNode, upperNode));
                */

            // remove breakpoints from adjacent arcs
            Vector2 vertex = Vector2.zero;
            var lowerArc = GetNodeData(lowerNode);
            var upperArc = GetNodeData(upperNode);
            if (lowerNode != null) {
                vertex = lowerArc.upperBreakpoint.edge.vertices[lowerArc.upperBreakpoint.intersectionIndex];
                lowerArc.upperBreakpoint = null;
            }
            if (upperNode != null) {
                upperArc.lowerBreakpoint = null;
            }

            // create new edge (if both lower/upper arcs exist)
            if (lowerNode != null && upperNode != null) {
                var intersectionIndex = (upperArc.site.x <= lowerArc.site.x) ? 1 : 0;
                var edge = new Edge(lowerArc.site, upperArc.site);
                //edge.AssignVertex(vertex, (upperArc.site.x > lowerArc.site.x) ? 1 : 0);
                var breakpoint = new Breakpoint(lowerArc, upperArc, edge, intersectionIndex);
                //Debug.Log(String.Format("new edge: {0} breakpoint: {1}", edge, breakpoint));
                lowerArc.upperBreakpoint = breakpoint;
                upperArc.lowerBreakpoint = breakpoint;
            }

            // remove node from tree
            //Debug.Log(String.Format("ST Delete arcNode: {0} arc: {1}", arcNode, arc));
            _DeleteNode(arcNode);

        }

    }
}
