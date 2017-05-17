using System;
using UnityEngine;

namespace TyVoronoi {

    public static class Geometry {
        public static bool FindCircle(
            Vector2 a,
            Vector2 b,
            Vector2 c,
            out Vector2 center,
            out float radius
        ) {
        	// Find the rightmost point on the circle through a,b,c.
        	// Algorithm from O'Rourke 2ed p. 189.
        	var A = b.x - a.x;
        	var B = b.y - a.y;
        	var C = c.x - a.x;
        	var D = c.y - a.y;
        	var E = A*(a.x+b.x) + B*(a.y+b.y);
        	var F = C*(a.x+c.x) + D*(a.y+c.y);
        	var G = 2.0f*(A*(c.y-b.y) - B*(c.x-b.x));

        	// Points are co-linear and no finite radius exists
        	if (Mathf.Abs(G) < 0.00001) {
                center = Vector2.zero;
                radius = 0;
                return false;
            }

        	center = new Vector2((D*E-B*F)/G, (A*F-C*E)/G);
        	radius = (center-a).magnitude;
            return true;
        }

        public static bool FindParabolaIntersection(
            Vector2 focus0,
            Vector2 focus1,
            float directrix,
            out Vector2[] intersections
        ) {
            // get the intersection of two parabolas
            var parabola = focus0;
            float[] ys;

            // first find Y
            // if both parabola foci are on the same X, split the difference in Y
            if (focus0.x == focus1.x) {
                ys = new float[] {(focus0.y + focus1.y) / 2f};

            // if either parabola lies on the directrix, choose that parabola's Y
            } else if (focus1.x == directrix) {
                ys = new float[] {focus1.y};
            } else if (focus0.x == directrix) {
                ys = new float[] {focus0.y};
                parabola = focus1;

            // otherwise... use quadratic formula
            } else {
                // f(y) = (y-b)^2/2(a-k) + (a+k)/2 where (a,b) = (focus.x,focus.y) and k = directrix
                // f(y) = (y-b0)^2/2(a0-k) + (a0+k)/2
                // g(y) = (y-b1)^2/2(a1-k) + (a1+k)/2
                // Solve for y: (y-b0)^2/2(a0-k) + (a0+k)/2 = (y-b1)^2/2(a1-k) + (a1+k)/2
                // -- expand Y exponents --
                // (y^2 -2*y*b0 + b0^2)/2(a0-k) + (a0+k)/2 = (y^2 -2*y*b1 + b1^2)^2/2(a1-k) + (a1+k)/2
                // -- move to LHS --
                // (y^2 -2*y*b0 + b0^2)/2(a0-k) - (y^2 -2*y*b1 + b1^2)^2/2(a1-k) + (a0+k)/2 - (a1+k)/2 = 0
                // -- expand denominators --
                // y^2/2*(a0-k) - 2*y*b0/2*(a0-k) + b0^2/2*(a0-k) - y^2/2*(a1-k) + 2*y*b1/2*(a1-k) - b1^2/2*(a1-k) + (a0+k)/2 - (a1+k)/2 = 0
                // -- combine y^2 terms --
                // (1/(2*a0-k) - 1/(2*a1-k)) * y^2 + 2*y*b1/2*(a1-k) - 2*y*b0/2*(a0-k) + b0^2/2*(a0-k) - b1^2/2*(a1-k) + (a0+k)/2 - (a1+k)/2 = 0
                // -- combine y terms --
                // (1/(2*a0-k) - 1/(2*a1-k)) * y^2 + (2*b1/2*(a1-k) - 2*b0/2*(a0-k)) * y + b0^2/2*(a0-k) - b1^2/2*(a1-k) + (a0+k)/2 - (a1+k)/2 = 0
                // -- quadratic equation --
                // a = 1/(2*(a0-k)) - 1/(2*(a1-k))
                // b = b1/(a1-k) - b0/(a0-k)
                // c = b0^2/(2*(a0-k)) - b1^2/(2*(a1-k)) + (a0+k)/2 - (a1+k)/2
                // y = (-b +/- sqrt(b^2 - 4*a*c))/2*a
                var a = 1f/(2f*(focus0.x-directrix)) - 1f/(2f*(focus1.x-directrix));
                var b = focus1.y/(focus1.x-directrix) - focus0.y/(focus0.x-directrix);
                var c = focus0.y*focus0.y/(2f*(focus0.x-directrix)) - focus1.y*focus1.y/(2f*(focus1.x-directrix)) + (focus0.x+directrix)/2f - (focus1.x+directrix)/2f;
                if (a>0) {
                    ys = new float[] {
                        (-b-Mathf.Sqrt(b*b - 4f*a*c)) / (2f*a),
                        (-b+Mathf.Sqrt(b*b - 4f*a*c)) / (2f*a)
                    };
                } else {
                    ys = new float[] {
                        (-b+Mathf.Sqrt(b*b - 4f*a*c)) / (2f*a),
                        (-b-Mathf.Sqrt(b*b - 4f*a*c)) / (2f*a)
                    };
                }
            }

            // plug ys back into f(y) to get x for each
            intersections = new Vector2[ys.Length];
            for (var i=0; i<ys.Length; i++) {
                var px = (ys[i]-parabola.y)*(ys[i]-parabola.y)/(2f*(parabola.x-directrix)) + (parabola.x+directrix)/2f;
                intersections[i] = new Vector2 (px, ys[i]);
            }
            return true;
        }

        // assumes line crosses bounds
        public static void FindBoundsLineIntersection(
            Bounds bounds,
            Edge edge,
            out Vector2[] intersections
        ) {
            intersections = new Vector2[2];
            if (edge.slope == 0) {
                intersections[0] = new Vector2(bounds.min.x, edge.intercept);
                intersections[1] = new Vector2(bounds.max.x, edge.intercept);
            } else if (float.IsNaN(edge.slope)) {
                intersections[0] = new Vector2(edge.bisector.x, bounds.min.y);
                intersections[1] = new Vector2(edge.bisector.x, bounds.max.y);
            } else {
                // y = m*x + b
                // check left bounds
                var minYintersect = edge.slope*bounds.min.x + edge.intercept;
                var maxYintersect = edge.slope*bounds.max.x + edge.intercept;
                var minXintersect = (bounds.min.y - edge.intercept)/edge.slope;
                var maxXintersect = (bounds.max.y - edge.intercept)/edge.slope;

                // traverses bottom edge of bounds
                if (minXintersect >= bounds.min.x && minXintersect < bounds.max.y) {
                    intersections[0] = new Vector2(minXintersect, 0);
                }
                // traverses top edge of bounds
                if (maxXintersect >= bounds.min.x && maxXintersect < bounds.max.y) {
                    intersections[1] = new Vector2(maxXintersect, bounds.max.y);
                }
                // traverses left edge of bounds?
                if (minYintersect >= bounds.min.y && minYintersect < bounds.max.y) {
                    intersections[(edge.slope>0) ? 0 : 1] = new Vector2(0, minYintersect);
                }
                // traverses right edge of bounds?
                if (maxYintersect >= bounds.min.y && maxYintersect < bounds.max.y) {
                    intersections[(edge.slope>0) ? 1 : 0] = new Vector2(bounds.max.x, maxYintersect);
                }
            }
        }

        public static bool FindParabolaLineIntersection(
            Vector2 focus,
            float directrix,
            Edge edge,
            out Vector2[] intersections
        ) {

            // special case: zero slope -> y = intercept
            if (edge.slope == 0f) {
                intersections = new Vector2[1];
                var px = (edge.intercept-focus.y)*(edge.intercept-focus.y)/(2f*(focus.x-directrix)) + (focus.x+directrix)/2f;
                intersections[0] = new Vector2 (px, edge.intercept);
                return true;

            // special case #2: infinite slope -> x = bisector.x
            // function for parabola
            // x = (y-b)^2/2(a-k) + (a+k)/2 where (a,b) = (focus.x,focus.y) and k = directrix
            // function for line
            // x = midX
            // -- Solve for y --
            // (y-b)^2/2(a-k) + (a+k)/2 = midX
            // -- expand Y exponents --
            // (y^2 -2*y*b + b^2) + (a+k)*(a-k) = midX*(2*(a-k))
            // -- move to LHS --
            // (y^2 - 2*y*b + b^2) + (a+k)*(a-k) - midX*(2*(a-k)) = 0
            // -- quadratic equation --
            // a = 1
            // b = -2*b
            // c = b^2 + (a+k)*(a-k) - 2*midX*(a-k)
            // y = (-b +/- sqrt(b^2 - 4*a*c))/2*a
            } else if (float.IsNaN(edge.slope)) {
                var vertexX = focus.x + (directrix - focus.x)/2f;
                var midX = edge.lowerSite.x + (edge.upperSite.x - edge.lowerSite.x)/2f;

                // two intersections if midX < vertexX
                if (midX < vertexX) {
                    var a = 1f;
                    var b = -2f*focus.y;
                    var c = focus.y*focus.y + (focus.x + directrix)*(focus.x - directrix) - 2f*midX*(focus.x - directrix);
                    var ys = new float[] {
                        (-b-Mathf.Sqrt(b*b - 4f*a*c)) / (2f*a),
                        (-b+Mathf.Sqrt(b*b - 4f*a*c)) / (2f*a)
                    };
                    intersections = new Vector2[ys.Length];
                    for (var i=0; i<ys.Length; i++) {
                        intersections[i] = new Vector2 (midX, ys[i]);
                    }
                    return true;

                // one intersection if midX == vertexX
                } else if (midX == vertexX) {
                    intersections = new Vector2[1] {new Vector2(focus.x, focus.y)};
                    return true;

                // zero intersections otherwise
                } else {
                    intersections = new Vector2[0];
                    return false;
                }

            // otherwise... use quadratic equation to solve...
            // function for parabola
            // f(y) = (y-b)^2/2(a-k) + (a+k)/2 where (a,b) = (focus.x,focus.y) and k = directrix
            // function for line
            // g(y) = (y-intercept)/slope
            // -- Solve for y --
            // (y-b)^2/2(a-k) + (a+k)/2 = (y-intercept)/slope
            // -- expand Y exponents --
            // (y^2 -2*y*b + b^2)/2(a-k) + (a+k)/2 = y/slope - intercept/slope
            // (y^2 -2*y*b + b^2) + (a+k)*(a-k) = (y/slope)*2(a-k) - (intercept/slope)*2(a-k)
            // -- move to LHS --
            // y^2 - 2*y*b - (y/slope)*2(a-k) + b^2 + (a+k)*(a-k) + (intercept/slope)*2(a-k) = 0
            // -- combine Y terms --
            // y^2 + (-2*b - 2(a-k)/slope)*y + b^2 + (a+k)*(a-k) + (intercept/slope)*2(a-k) = 0
            // -- quadratic equation --
            // a = 1
            // b = -2*b - 2*(a-k)/slope
            // c = b^2 + (a+k)*(a-k) + (intercept/slope)*2(a-k)
            // y = (-b +/- sqrt(b^2 - 4*a*c))/2*a
            } else {
                var a = 1f;
                var b = -2f*focus.y - 2f*(focus.x-directrix)/edge.slope;
                var c = focus.y*focus.y + (focus.x+directrix)*(focus.x-directrix) + (edge.intercept/edge.slope)*2f*(focus.x-directrix);
                var sqrValue = b*b - 4f*a*c;

                // two intersections
                if (sqrValue > 0) {
                    var ys = new float[] {
                        (-b-Mathf.Sqrt(b*b - 4f*a*c)) / (2f*a),
                        (-b+Mathf.Sqrt(b*b - 4f*a*c)) / (2f*a)
                    };

                    // plug ys back into f(y) to get x for each
                    intersections = new Vector2[ys.Length];
                    for (var i=0; i<ys.Length; i++) {
                        var px = (ys[i]-focus.y)*(ys[i]-focus.y)/(2f*(focus.x-directrix)) + (focus.x+directrix)/2f;
                        intersections[i] = new Vector2 (px, ys[i]);
                    }
                    return true;

                // one intersections
                } else if (sqrValue == 0) {
                    var py = -b / (2f*a);
                    var px = (py-focus.y)*(py-focus.y)/(2f*(focus.x-directrix)) + (focus.x+directrix)/2f;
                    intersections = new Vector2[1] {new Vector2(px, py)};
                    return true;

                // no intersections
                } else {
                    intersections = new Vector2[0];
                    return false;
                }
            }

        }

    }
}
