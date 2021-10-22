using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Rhino.Geometry;

namespace FacadeFELogic.Helper
{
    public static class HelperClass
    {

        //helper methods
        public static bool IsFlippedLine(Line refLine, Line compareLine)
        {
            if (refLine.Length == compareLine.Length
                && refLine.PointAt(0) == compareLine.PointAt(1)
                && refLine.PointAt(1) == compareLine.PointAt(0))
            {
                return true;
            }
            else return false;
        }

        public static bool ToleranceEqual(double value1, double value2, double tolerance)
        {
            return (Math.Abs(value1 - value2) < tolerance);
        }

        public static bool IsPointOnEdge(Point3d point, Curve edge, double tolerance)
        {
            //get param on edge
            edge.ClosestPoint(point, out double nodeParam);

            //check if node is on curve (Curve.contains may work, but including tolerance is safer)
            Point3d pointOnEdge = edge.PointAt(nodeParam);

            if (pointOnEdge.DistanceTo(point) > tolerance)
            {
                return false;
            }
            else return true;
        }

        public static EdgeHelper FindClosestEdgeHelper(Point3d point, List<EdgeHelper> edgeHelpers)
        {
            //list of dists
            List<double> distsSquared = new List<double>();
            foreach (EdgeHelper edgeHelper in edgeHelpers)
            {
                //find closest point
                edgeHelper.Edge.ClosestPoint(point, out double closestParam);
                Point3d closestPoint = edgeHelper.Edge.PointAt(closestParam);

                //get distance
                double dist = closestPoint.DistanceToSquared(point);

                //add it to list
                distsSquared.Add(dist);
            }

            //find edgeHelper with the shortest distance (find index of the shortest distance
            int shortestDistIndex = distsSquared.IndexOf(distsSquared.Min());

            //return edgeHelper with the shortest distance (closest
            return edgeHelpers[shortestDistIndex];
        }

        public static ElementFE FindClosestElementFE(Point3d point, List<ElementFE> elementsFE, out int elementId)
        {
            //list of dists
            List<double> distsSquared = new List<double>();

            foreach (ElementFE elementFE in elementsFE)
            {
                //find closest point
                elementFE.RefSegment.ClosestPoint(point, out double closestParam);
                Point3d closestPoint = elementFE.RefSegment.PointAt(closestParam);

                //get distance
                double dist = closestPoint.DistanceToSquared(point);

                //add it to list
                distsSquared.Add(dist);
            }

            //find edgeHelper with the shortest distance (find index of the shortest distance
            elementId = distsSquared.IndexOf(distsSquared.Min());

            //return edgeHelper with the shortest distance (closest
            return elementsFE[elementId];
        }

        public static bool Discontinuity(Curve curve, out List <double> kinkParams, out List<Point3d> cornerPoints, out List<Curve> segments)
        {
            //initialize variables
            kinkParams = new List<double>();
            cornerPoints = new List<Point3d>();
            segments = new List<Curve>();

            //add first discontinuity parameter (because not automatically added in the next steps)
            kinkParams.Add(0);

            //discontiuity parameters
            Continuity kinks = Continuity.C1_continuous;
            double t0 = curve.Domain[0];
            double t1 = curve.Domain[1]; 

            //get disconituity curve parameters
            while (true)
            {
                bool discontinuity = curve.GetNextDiscontinuity(kinks, t0, t1, out double t);
                if (!discontinuity) break;

                else
                {
                    kinkParams.Add(t);
                    t0 = t;
                }
            }

            //get discontinuity points
            foreach (double t in kinkParams)
            {
                Point3d pt = curve.PointAt(t);
                cornerPoints.Add(pt);
            }

            //get discontinuity segments
            for (int i = 0; i < kinkParams.Count - 1; i++)
            {
                Curve segment = curve.Trim(kinkParams[i], kinkParams[i + 1]);
                segments.Add(segment);
            }

            //add last segment
            Curve lastSegment = curve.Trim(kinkParams[kinkParams.Count - 1], t1);
            segments.Add(lastSegment);

            if (kinkParams.Count <= 2) return false;
            else return true;

        }

        public static bool GetMatchingNodesIndex(Curve edge, List<Node> nodes, double tolerance, out int startId, out int endId)
        {
            startId = nodes.FindIndex(n => n.Position.DistanceTo(edge.PointAtStart) < tolerance);
            endId = nodes.FindIndex(n => n.Position.DistanceTo(edge.PointAtEnd) < tolerance);

            if (startId == -1 || endId == -1) return false;
            return true;
        }

        public static bool SortEdges(Brep brep, out List<Curve> nakedEdges, out List<Curve> interiorEdges, out List<Curve> nonManifoldEdges, out List<Curve> noneEdges)
        {
            //initialise variables
            nakedEdges = new List<Curve>();
            interiorEdges = new List<Curve>();
            nonManifoldEdges = new List<Curve>();
            noneEdges = new List<Curve>();
          
            
            //edge valence
            foreach (BrepEdge brepEdge in brep.Edges)
            {
                //valence
                int valence = (int)brepEdge.Valence;

                //sort edge according to type
                if (valence == 0)
                {
                    noneEdges.Add(brepEdge.EdgeCurve);
                }
                else if (valence == 1)
                {
                    nakedEdges.Add(brepEdge.EdgeCurve);
                }
                else if (valence == 2)
                {
                    interiorEdges.Add(brepEdge.EdgeCurve);
                }
                else if (valence > 2)
                {
                    nonManifoldEdges.Add(brepEdge.EdgeCurve);
                }
            }

            //check if there are noneEdges or nonManifold
            if (noneEdges.Count > 0 | nonManifoldEdges.Count > 0) return false;
            else return true;
        }
    }

}
