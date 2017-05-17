using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TyVoronoi {

    public class SiteComparer : IComparer {
        int IComparer.Compare(object x, object y) {
            var v1 = (Vector2) x;
            var v2 = (Vector2) y;
            if (v1.x < v2.x) {
                return 1;
            }
            if (v1.x > v2.x) {
                return -1;
            }
            if (v1.y < v2.y) {
                return 1;
            }
            if (v1.y > v2.y) {
                return -1;
            }
            return 0;
        }
    }

    public class EventComparer : IComparer {
        int IComparer.Compare(object x, object y) {
            var v1 = (CircleEvent) x;
            var v2 = (CircleEvent) y;
            if (v1.vertex.x < v2.vertex.x) {
                return 1;
            }
            if (v1.vertex.x > v2.vertex.x) {
                return -1;
            }
            if (v1.vertex.y < v2.vertex.y) {
                return 1;
            }
            if (v1.vertex.y > v2.vertex.y) {
                return -1;
            }
            return 0;
        }
    }

    public class Voronoi {
        public Bounds bounds;
        public Heap<Vector2> siteQ;
        public Heap<CircleEvent> eventQ;
        public Beachline beachline;
        public List<Edge> edgeList;
        public List<Vector2> vertexList;

        public Voronoi(Bounds bounds, Vector2[] sites) {
            this.bounds = bounds;
            var siteComparer = new SiteComparer();
            var eventComparer = new EventComparer();
            siteQ = new Heap<Vector2>(siteComparer, sites);
            eventQ = new Heap<CircleEvent>(eventComparer);
            beachline = new Beachline();
            edgeList = new List<Edge>();
            vertexList = new List<Vector2>();
        }

        public void Compute() {
            Compute(float.MaxValue);
        }

        public void Compute(float maxX) {
            bool earlyOut = false;
            while (siteQ.Count > 0) {
                // compute up to maxX
                if ((eventQ.Count <= 0 || eventQ.Top().vertex.x > maxX) &&
                    (siteQ.Count > 0 && siteQ.Top().x > maxX)) {
                    earlyOut = true;
                    break;
                }

                /*
                Debug.Log(String.Format("eventQ.Count: {0} eventQ.Top().vertex {1} siteQ.Top(): {2}",
                    eventQ.Count,
                    (eventQ.Count) > 0 ? eventQ.Top().vertex : Vector2.zero,
                    siteQ.Top() ));
                    */

                // if next circle event is higher priority than next site event ... process next circle event
                if ((eventQ.Count > 0) && (eventQ.Top().vertex.x <= siteQ.Top().x)) {
                    var circleEvent = eventQ.Extract();
                    ProcessEvent(circleEvent);

                // otherwise, process next site event
                } else {
                    // extract next site
                    var site = siteQ.Extract();
                    ProcessSite(site);
                }
            }

            // drain the remaining circle events
            while (!earlyOut && eventQ.Count > 0) {
                var circleEvent = eventQ.Extract();
                if (circleEvent.vertex.x > maxX) {
                    earlyOut = true;
                    break;
                }
                ProcessEvent(circleEvent);
            }

            // finish graph edges
            FinishEdges();
        }

        public void CreateCircleEvent(object arcNode, float directrix) {
            if (arcNode == null) return;
            // invalidate any circle event already associated with arc
            // this is cheaper than removing the entry from the Q
            var middleArc = beachline.GetNodeData(arcNode);
            if (middleArc.circleEvent != null) {
                middleArc.circleEvent.valid = false;
                middleArc.circleEvent = null;
            }
            //beachline.Dump();
            var lowerNode = beachline.GetPredecessor(arcNode);
            var upperNode = beachline.GetSuccessor(arcNode);
            if (lowerNode == null || upperNode == null) return;
            var lowerArc = beachline.GetNodeData(lowerNode);
            var upperArc = beachline.GetNodeData(upperNode);

            // find determinant for three sites
            // order of comparison is important here, this assumes bottom to top
            // determinant < 0 => converging edges, consider intersection
            //             = 0 => colinear, ignore
            //             > 0 => diverging edges, ignore
            var ba = middleArc.site - lowerArc.site;
            var cb = upperArc.site - middleArc.site;
            var determinant = ba.x*cb.y - ba.y*cb.x;
            if (determinant >= 0) {
                /*
                Debug.Log(String.Format(
                    "ignoring invalid circle event: larc: {0}\nmarc: {1}\nuarc: {2}\ndx: {3} determinant:{4}",
                    lowerArc, middleArc, upperArc, directrix, determinant));
                */
                return;
            }

            // find center of circle
            Vector2 center;
            float radius;
            var circleExists = Geometry.FindCircle(
                lowerArc.site,
                middleArc.site,
                upperArc.site,
                out center,
                out radius
            );

            if (circleExists && (center.x + radius) > directrix) {
                var point = new Vector2(center.x+radius, center.y);
                var circleEvent = new CircleEvent(middleArc, center, point);
                /*
                Debug.Log(String.Format(
                    "new circle event: larc: {0}\nmarc: {1}\nuarc: {2}\ndx: {3} determinant:{4}\ncenter: {5} point: {6}\nevent: {7}",
                    lowerArc, middleArc, upperArc, directrix, determinant, center, point, circleEvent));
                */
                middleArc.circleEvent = circleEvent;
                // add to event q
                eventQ.Insert(circleEvent);
            }

        }

        void ProcessSite(Vector2 site) {
            // create new arc for site and insert into beachline
            var arc = new Arc(site);
            //Debug.Log(String.Format("ProcessSite: {0} -> arc {1}", site, arc));
            var arcNode = beachline.Insert(arc);

            // check for circle events for three new arcs
            //CreateCircleEvent(arcNode, site.x);
            CreateCircleEvent(beachline.GetPredecessor(arcNode), site.x);
            CreateCircleEvent(beachline.GetSuccessor(arcNode), site.x);
            //beachline.Dump();

        }

        void ProcessEvent(CircleEvent circleEvent) {
            //Debug.Log("ProcessEvent: " + circleEvent);
            // if event has been marked as invalid, skip
            if (!circleEvent.valid) return;

            // FIXME: remove
            vertexList.Add(circleEvent.center);

            // circle event is associated with arc which is disappearing from beachline
            // that arc has two breakpoints which are converging, so update those edges associated with
            // those breakpoints with event vertex.  this is a vertex in the voronoi diagram
            circleEvent.arc.upperBreakpoint.edge.AssignVertex(
                circleEvent.center,
                circleEvent.arc.upperBreakpoint.intersectionIndex);
            if (circleEvent.arc.upperBreakpoint.edge.done) {
                AddFinishedEdge(circleEvent.arc.upperBreakpoint.edge);
            }
            circleEvent.arc.lowerBreakpoint.edge.AssignVertex(
                circleEvent.center,
                circleEvent.arc.lowerBreakpoint.intersectionIndex);
            if (circleEvent.arc.lowerBreakpoint.edge.done) {
                AddFinishedEdge(circleEvent.arc.lowerBreakpoint.edge);
            }

            // convergence of edge means that arc associated w/ circle event also converged to a point and
            // needs to be removed from beachline
            // a new edge and associated breakpoints are added instead
            // edge starts w/ circle event vertex
            var arcNode = beachline.GetNode(circleEvent.arc);
            var lowerNode = beachline.GetPredecessor(arcNode);
            var upperNode = beachline.GetSuccessor(arcNode);
            beachline.Delete(circleEvent.arc);

            // update vertex of new edge between lower/upper nodes
            var lowerArc = beachline.GetNodeData(lowerNode);
            var upperArc = beachline.GetNodeData(upperNode);
            lowerArc.upperBreakpoint.edge.AssignVertex(circleEvent.center,
                (upperArc.site.x > lowerArc.site.x) ? 1 : 0);

            // check for new circle events for each side of arc that has been removed
            CreateCircleEvent(lowerNode, circleEvent.vertex.x);
            CreateCircleEvent(upperNode, circleEvent.vertex.x);
            //beachline.Dump();

        }

        void FinishEdgeFunction(object node, object data) {
            var arc = beachline.GetNodeData(node);
            // handle degenerative cases for zero slope or NaN slope
            if (arc.upperBreakpoint == null) return;
            var edge = arc.upperBreakpoint.edge;

            // find intersections of edge w/ bounding box
            Vector2[] intersections;
            Geometry.FindBoundsLineIntersection(bounds, edge, out intersections);

            // if edge has a single point specified...
            // check if that point is within bounds, if not, exclude this edge
            if ((edge.vertices[0] != default(Vector2)) &&
                !bounds.Contains(edge.vertices[0])) {
                return;
            }
            if ((edge.vertices[1] != default(Vector2)) &&
                !bounds.Contains(edge.vertices[1])) {
                return;
            }

            // fill in missing vertices
            if (edge.vertices[0] == default(Vector2)) {
                edge.AssignVertex(intersections[0], 0);
            }
            if (edge.vertices[1] == default(Vector2)) {
                edge.AssignVertex(intersections[1], 1);
            }

            // add edge to final edge list
            AddFinishedEdge(edge);
        }

        void AddFinishedEdge(Edge edge) {
            // check if edge endpoints are in bounds
            var inboundsV0 = bounds.Contains(edge.vertices[0]);
            var inboundsV1 = bounds.Contains(edge.vertices[1]);
            // skip edge completely out of bounds
            if (!inboundsV0 && !inboundsV1) {
                return;
            }
            // for one vertex out of bounds, move vertex to boundary
            if (!inboundsV0 || !inboundsV1) {
                Vector2[] intersections;
                Geometry.FindBoundsLineIntersection(bounds, edge, out intersections);
                if (!inboundsV0) {
                    edge.vertices[0] = intersections[0];
                }
                if (!inboundsV1) {
                    edge.vertices[1] = intersections[1];
                }
            }
            edgeList.Add(edge);
        }

        void FinishEdges() {
            // walk through remaining nodes in beachline, for each edge find intersection w/ bounding box
            beachline.WalkInOrder(FinishEdgeFunction, null);
        }
    }

}
