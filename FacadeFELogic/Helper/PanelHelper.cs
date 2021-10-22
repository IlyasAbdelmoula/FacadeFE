using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Rhino.Geometry.Intersect;

//for tolerance equality
//using static FacadeFELogic.Helper.HelperClass;

namespace FacadeFELogic.Helper
{
    public class PanelHelper
    {
        //attributes
        public BrepFace Panel;
        public List<EdgeHelper> PanelEdgeHelpers;
        public Curve Contour;
        public List<Curve> ContourSegments;
        public List<Point3d> ContourCorners;
        public List<int> EdgeToSegIds; //keep it just in case
        public List<Point3d> FEProjectionPoints;

        //#for debug + #for viz
        public List<Line> FEIntersectionLines;
        public List<Line> FEWindLines;

        //Constructor
        public PanelHelper(BrepFace panel)
        {
            this.Panel = panel;
            this.PanelEdgeHelpers = new List<EdgeHelper>();
            this.Contour = null;
            this.ContourSegments = new List<Curve>();
            this.ContourCorners = new List<Point3d>();
            
        }

        public PanelHelper(BrepFace panel, FacadeSystem facadeSystem)
        {
            this.Panel = panel;
            this.PanelEdgeHelpers = new List<EdgeHelper>();
            this.Contour = null;
            this.ContourSegments = new List<Curve>();
            this.ContourCorners = new List<Point3d>();
            //this.EdgeToSegIds = new List<int>();
            this.FEProjectionPoints = new List<Point3d>();

            //#for debug purposes + #for viz purposes
            this.FEIntersectionLines = new List<Line>();
            this.FEWindLines = new List<Line>();

            //edges list (edge helpers, needed for later)
            //List<EdgeHelper> panelEdgeHelpers = new List<EdgeHelper>();

            //curves list (needed to check the type of the face (square or rectangle...)
            List<Curve> panelCurves = new List<Curve>();
            List<EdgeHelper> panelEdgeHelpers = new List<EdgeHelper>();

            //get edgehelpers and retrieve their corresponding curves
            foreach (int i in panel.AdjacentEdges())
            {
                //get corresponding curve from Brep
                Curve panelBrepCurve = facadeSystem.FacadeBrep.Edges[i].EdgeCurve;
                //get corresponding curve from edgehelpers
                //EdgeHelper panelEdgeHelper = facadeSystem.EdgeHelpers[i]; #obsolete
                EdgeHelper panelEdgeHelper = facadeSystem.EdgeHelpers.Find(e => e.Edge == panelBrepCurve);

                Curve panelCurve = panelEdgeHelper.Edge.DuplicateCurve();

                //update temporary list of curves and edgeHelpers
                panelCurves.Add(panelCurve);
                panelEdgeHelpers.Add(panelEdgeHelper);
            }

            //get the total contour and simplify it
            Curve contour = Curve.JoinCurves(panelCurves)[0]; //[0] as one curve

            //discontinuity (to get segments/cornerpoints)
            HelperClass.Discontinuity(contour, out List<double> kinkParams, out List<Point3d> cornerPoints, out List<Curve> segments);

            //update panelHelper attributes (contour-related)
            this.Contour = contour;
            this.ContourSegments = segments;
            this.ContourCorners = cornerPoints; //maybe not needed after all?

            //add edge helpers to panel (while updating their FEParams if are belonging to panel corners)
            foreach (EdgeHelper panelEdgeHelper in panelEdgeHelpers)
            {
                for (int paramIndex = 0; paramIndex < 2; paramIndex++)
                {
                    //find corresponding FEParam (
                    FEParam feParam = panelEdgeHelper.FEParams[paramIndex];

                    //get corresponding endpoint
                    double param = panelEdgeHelper.Edge.Domain[paramIndex];
                    Point3d point = panelEdgeHelper.Edge.PointAt(param);

                    //check if it is a corner point for the panel
                    if (this.ContourCorners.Contains(point))
                    {
                        //add it WindFELoad as 0 (needed for FE subdivision in panel corner points)
                        feParam.AddFEWindLoad(0);
                    }
                }
                /*
                //add FEParams for the end points (as corner ones)
                for (int paramIndex = 0; paramIndex < 2; paramIndex++)
                {
                    //end point param
                    double param = panelEdgeHelper.Edge.Domain[paramIndex];

                    //get corresponding point
                    Point3d point = panelEdgeHelper.Edge.PointAt(param);

                    //check if it is a corner point for the panel
                    if (this.ContourCorners.Contains(point))
                    {
                        //add it with a already a wind load of 0 (exisiting one)
                        FEParam feParam = new FEParam(param, isCorner: true, 0);
                        feParam.IsPanelCorner = true;
                        panelEdgeHelper.FEParams.Add(feParam);
                    }

                    //otherwise just add without wind load of 0 (-1 meaning unexistant)
                    else
                    {
                        //add it without a wind load
                        FEParam feParam = new FEParam(param, isCorner: true);
                        panelEdgeHelper.FEParams.Add(feParam);
                    }
                

                }
                */

                //update panel edgehelpers internal attribute
                this.PanelEdgeHelpers.Add(panelEdgeHelper);
            }


            /*
            //adapt contour seam to edgehelpers order
            Point3d startPoint = this.PanelEdgeHelpers[0].Edge.PointAtStart;
            contour.ClosestPoint(startPoint, out double startParam);
            contour.ChangeClosedCurveSeam(startParam);
            */

            /* //no need to simplify, because after joined, discontinuity will be applied, which considers only kinks
            if (panelHelper.PanelEdgeHelpers.Count > 4) //#PotentialBug: targeted face with more than 4 faces, but face with 3 faces skipped
            {
                //simplify contour
                CurveSimplifyOptions simplifyOptions = CurveSimplifyOptions.Merge;
                contour = contour.Simplify(simplifyOptions,
                    Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                    Rhino.RhinoDoc.ActiveDoc.ModelAngleToleranceRadians);
            }
            */


            //Get EdgeToSegIds (corresponding Ids)
            //List<Curve> orderedSegments = new List<Curve>();
            List<int> correspondingSegmentIds = new List<int>();
            List<Curve> edges = this.PanelEdgeHelpers.Select(e => e.Edge).ToList();

            for (int i = 0; i < edges.Count; i++)
            {
                Curve edge = edges[i];
                EdgeHelper currentEdgeHelper = PanelEdgeHelpers[i];
                bool correspondingSegment = false;

                foreach (Curve segment in segments)
                {
                    //checking the case of undivided edges
                    if (!correspondingSegment)
                    {
                        //if they are the same, even with other 
                        if ( (segment.PointAtStart == edge.PointAtStart && segment.PointAtEnd == edge.PointAtEnd) ||
                             (segment.PointAtStart == edge.PointAtEnd && segment.PointAtEnd == edge.PointAtStart) )
                        {
                            //add it to list (maybe needed later)
                            correspondingSegmentIds.Add(segments.IndexOf(segment));
                            //add it to corresponding edgeHelper
                            currentEdgeHelper.AssociatedSegmentId = segments.IndexOf(segment);  //#bug: not working as intended (for now use this.EdgeToSegIds instead)
                            correspondingSegment = true;
                        }

                        //if there are devided edges found, check parts
                        if (!correspondingSegment)
                        {
                            //get closest points
                            segment.ClosestPoint(edge.PointAtStart, out double paramStart);
                            segment.ClosestPoint(edge.PointAtEnd, out double paramEnd);

                            Point3d pointStart = segment.PointAt(paramStart);
                            Point3d pointEnd = segment.PointAt(paramEnd);

                            //if edge points are inside the the segment, then it is the corresponding one
                            if (edge.PointAtStart == pointStart && edge.PointAtEnd == pointEnd)
                            {
                                //add it to list (maybe needed later)
                                correspondingSegmentIds.Add(segments.IndexOf(segment));
                                //add it to corresponding edgeHelper
                                currentEdgeHelper.AssociatedSegmentId = segments.IndexOf(segment); //#bug: not working as intended (for now use this.EdgeToSegIds instead)
                                correspondingSegment = true;
                            }
                        }
                    }
                }
            }

            this.EdgeToSegIds = correspondingSegmentIds;

            /*
            //order segments
            List<Curve> edges = this.PanelEdgeHelpers.Select(e => e.Edge).ToList();
            foreach (Curve edge in edges)
            {
                
            }
            */
        }

        //methods
        public bool Subdivide(double tolerance)
        {
            //check that the panel has 4 segments
            if (this.ContourSegments.Count != 4)
            { 
                return false; 
            }

            //square condition
            bool isSquare = true;
            double sideLength = this.ContourSegments[0].GetLength();
            //compare segment lengths
            foreach (Curve segment in this.ContourSegments)
            {
                double segmentLength = segment.GetLength();
                if (!HelperClass.ToleranceEqual(segmentLength, sideLength, tolerance))
                { 
                    isSquare = false;
                    break;
                }
            }
            
            //check if it is square
            if (isSquare)
            {
                //get centroid
                AreaMassProperties contourProps = AreaMassProperties.Compute(this.Contour);
                Point3d centroid = contourProps.Centroid;
                //add it to FE projection points
                this.FEProjectionPoints.Add(centroid);

                //project point and generate FEParams + windloads
                this.ProjectPoint(centroid, PanelEdgeHelpers, tolerance);

            }

            //else if it is rectangle/parallelogram/trapeze
            else
            {
                //check condition of faced edges total (to see which one should go)
                //declare containers
                List<int> shortestLengthIds = new List<int>(); //needed to project the new point generated later
                List<List<Curve>> shortestLengthAssociatedSegments = new List<List<Curve>>(); //needed to calculate vectors to calculate FEProjectionPoints
                List<List<Vector3d>> shortestLengthAssociatedVectors = new List<List<Vector3d>>() { new List<Vector3d>(), new List<Vector3d>() }; //needed to calculated vectors

                List<int> longestLengthIds = new List<int>(); //needed to project the centroid

                //check lengths
                double length1 = this.ContourSegments[0].GetLength() + this.ContourSegments[2].GetLength();
                double length2 = this.ContourSegments[1].GetLength() + this.ContourSegments[3].GetLength();
                
                if (length1 < length2)
                {
                    //shortest length Ids
                    shortestLengthIds.Add(0);
                    shortestLengthIds.Add(2);
                    //shortest length associated segments
                    shortestLengthAssociatedSegments.Add(new List<Curve>() { this.ContourSegments[3], this.ContourSegments[0], this.ContourSegments[1] });
                    shortestLengthAssociatedSegments.Add(new List<Curve>() { this.ContourSegments[1], this.ContourSegments[2], this.ContourSegments[3] });

                    //longest length Ids
                    longestLengthIds.Add(1);
                    longestLengthIds.Add(3);
                }

                else
                {
                    //shortest length Ids
                    shortestLengthIds.Add(1);
                    shortestLengthIds.Add(3);
                    //shortest length associated segments
                    shortestLengthAssociatedSegments.Add(new List<Curve>() { this.ContourSegments[0], this.ContourSegments[1], this.ContourSegments[2] });
                    shortestLengthAssociatedSegments.Add(new List<Curve>() { this.ContourSegments[2], this.ContourSegments[3], this.ContourSegments[0] });

                    //longest length Ids
                    longestLengthIds.Add(0);
                    longestLengthIds.Add(2);

                }

                //calculate associated vectors
                //loop throough associated segment lists
                for (int i = 0; i < shortestLengthAssociatedSegments.Count; i++)
                {
                    List<Curve> associatedSegments = shortestLengthAssociatedSegments[i];
                    //loop through each 2 pairs (to calculate the angle)
                    for (int j = 0; j < 2; j++)
                    {
                        //get vectors
                        Vector3d firstVector = associatedSegments[j].PointAtStart - associatedSegments[j].PointAtEnd; //because it should be the inverse of the segment direction
                        Vector3d secondVector = associatedSegments[j + 1].PointAtEnd - associatedSegments[j + 1].PointAtStart; //same as segment direction

                        //unitize vectors, otherwise we get the panel diagonals later when we intersect their lines
                        firstVector.Unitize();
                        secondVector.Unitize();

                        //calculate vector
                        Vector3d combinedVector = firstVector + secondVector;
                        shortestLengthAssociatedVectors[i].Add(combinedVector);

                    }
                }

                //loop through shortestLength associated segments
                for (int i = 0; i < shortestLengthIds.Count; i++)
                {
                    //GET EDGEHELPERS
                    //retrieve segment
                    int segmentId = shortestLengthIds[i];
                    Curve segment = this.ContourSegments[segmentId];

                    //retrieve associated EdgeHelper(s) (several if subdivided, otherwise just one)
                    List<EdgeHelper> associatedEdgeHelpers = this.GetSegmentAssociatedEdgeHelpers(segmentId);

                    //List<EdgeHelper> associatedEdgeHelpers = this.PanelEdgeHelpers.FindAll(e => e.AssociatedSegmentId == segmentId); //#bug: not working as intended (for now use this.EdgeToSegIds instead)


                    //GET PROJECTION POINT
                    //Get the point by intersecting the 2 associated vectors
                    List<Line> linesToIntersect = new List<Line>();

                    for (int j = 0; j < shortestLengthAssociatedVectors[i].Count; j++)
                    {
                        Vector3d vector = shortestLengthAssociatedVectors[i][j];
                        Point3d originPoint = segment.PointAtNormalizedLength(j);

                        Line lineToIntersect = new Line(originPoint, vector);
                        linesToIntersect.Add(lineToIntersect);
                    }

                    //get intersection params
                    Intersection.LineLine(linesToIntersect[0], linesToIntersect[1], out double paramA, out double paramB);

                    //get point
                    Point3d projectionPoint = linesToIntersect[0].PointAt(paramA);

                    //add it to projection points
                    this.FEProjectionPoints.Add(projectionPoint);

                    //PROJECT POINT AT ASSOCIATED EDGEHELPERS
                    this.ProjectPoint(projectionPoint, associatedEdgeHelpers, tolerance);

                    //#for debug and #for viz
                    //draw FEIntersectionLines between corners and projection point
                    foreach (Line lineToIntersect in linesToIntersect)
                    {
                        Line feIntersectionLine = new Line(lineToIntersect.PointAt(0), projectionPoint);
                        this.FEIntersectionLines.Add(feIntersectionLine);
                    }
                }

                //get point between the two projection points
                Point3d pointInBetween = new Line(this.FEProjectionPoints[0], this.FEProjectionPoints[1]).PointAt(0.5);

                //add it to FE projection points
                this.FEProjectionPoints.Add(pointInBetween);

                //Loop through 
                for (int i = 0; i < longestLengthIds.Count; i++)
                {
                    //GET EDGEHELPERS
                    //retrieve segment
                    int segmentId = longestLengthIds[i];
                    Curve segment = this.ContourSegments[segmentId];

                    //retrieve associated EdgeHelper(s) (several if subdivided, otherwise just one)
                    List<EdgeHelper> associatedEdgeHelpers = this.GetSegmentAssociatedEdgeHelpers(segmentId);

                    //List<EdgeHelper> associatedEdgeHelpers = this.PanelEdgeHelpers.FindAll(e => e.AssociatedSegmentId == segmentId); //#bug: not working as intended (for now use this.EdgeToSegIds instead)

                    //project centroid on associated edge helpers
                    foreach (Point3d feProjectionPoint in this.FEProjectionPoints)
                    {
                        this.ProjectPoint(feProjectionPoint, associatedEdgeHelpers, tolerance);
                    }
                }

            }

            return true;

        }

        public List<EdgeHelper> GetSegmentAssociatedEdgeHelpers(int segmentId)
        {
            //retrieve associated EdgeHelper(s) (several if subdivided, otherwise just one)
            List<EdgeHelper> associatedEdgeHelpers = new List<EdgeHelper>();

            //loop through transition Ids to retrieve matching
            for (int i = 0; i < this.EdgeToSegIds.Count; i++)
            {
                int transitionId = this.EdgeToSegIds[i];
                if (transitionId == segmentId) associatedEdgeHelpers.Add(this.PanelEdgeHelpers[i]);
            }
            return associatedEdgeHelpers;
        }    

        private void ProjectPoint(Point3d point, List<EdgeHelper> edgeHelpers, double tolerance)
        {
            //make a temp list to fill, when we want to skip shared points
            List<Point3d> temporarySharedPoints = new List<Point3d>();

            foreach (EdgeHelper edgeHelper in edgeHelpers)
            {
                //make projection
                edgeHelper.Edge.ClosestPoint(point, out double projectedParam);
                Point3d projectedPoint = edgeHelper.Edge.PointAt(projectedParam);

                //check angle of projection, if not 90 degrees, skip:
                Vector3d projectionVec = projectedPoint - point;
                Vector3d edgeHelperVec = edgeHelper.Edge.PointAtEnd - edgeHelper.Edge.PointAtStart;
                double vecAngle = Vector3d.VectorAngle(projectionVec, edgeHelperVec);
                if ( !HelperClass.ToleranceEqual(vecAngle, 0.5 * Math.PI, tolerance) )
                {
                    continue;
                }

                //check if projected point already exists (shared with another edgeHelper)
                else if (temporarySharedPoints.Any(p => p.DistanceTo(projectedPoint) < tolerance))
                { 
                    continue; 
                }

                //otherrwise add it
                temporarySharedPoints.Add(projectedPoint);

                //windFELoad
                double windFELoad = projectedPoint.DistanceTo(point);

                //check if edgehelper contains it
                if (edgeHelper.FEParams.Any( f => HelperClass.ToleranceEqual(f.Param, projectedParam, tolerance) ))
                {
                    FEParam foundFEParam = edgeHelper.FEParams.Find(f => HelperClass.ToleranceEqual(f.Param, projectedParam, tolerance));

                    //add wind load directly to this existing parameter
                    foundFEParam.AddFEWindLoad(windFELoad);
                }

                else
                {
                    //else, add a new FEParam
                    edgeHelper.AddFEParamWithLoad(projectedParam, isCorner: false, windFELoad);
                }

                //add WindFELines (for viz+debug purposes)
                Line windFELine = new Line(point, projectedPoint);
                this.FEWindLines.Add(windFELine);
            }
        }

        /*
        private void MigratePointsToEdgeHelpers(List<Point3d> pointProjections)
        {
            if (this.PanelEdgeHelpers.Count == 4)
            {
                for (int i = 0; i < this.PanelEdgeHelpers.Count; i++)
                {
                    //get matching edgehelper/point
                    EdgeHelper currentEdgeHelper = this.PanelEdgeHelpers[i];
                    int matchingIndex = this.EdgeToSegIds[i];
                    Point3d matchingPoint = pointProjections[matchingIndex];

                    //get point param and add it
                    currentEdgeHelper.Edge.ClosestPoint(matchingPoint, out double param);
                    currentEdgeHelper.AddFEParam(param, isCorner: false);
                }
            }

            else //case of more than 4
            {
                //
                fore



                //state indicator in case of same-segment edges, in order to skip them
                int sameSegmentSkip = 0;

                for (int i = 0; i < this.PanelEdgeHelpers.Count; i++)
                {
                    if (sameSegmentSkip > 0)
                    {
                        sameSegmentSkip -= 1;
                        continue;
                    }

                    else
                    {
                        EdgeHelper currentEdgeHelper = this.PanelEdgeHelpers[i];
                        int matchingIndex = this.EdgeToSegIds[i];

                        //check the occurence of the matching index
                        int occurence = EdgeToSegIds.Count(e => e == matchingIndex);
                        if (occurence == 1)
                        {
                            Point3d matchingPoint = pointProjections[matchingIndex];
                            //get point param and add it
                            currentEdgeHelper.Edge.ClosestPoint(matchingPoint, out double param);
                            currentEdgeHelper.AddFEParam(param, isCorner: false);
                        }

                        else
                        {
                            //activate tje Skip indicator
                            sameSegmentSkip = occurence - 1;

                            //declare empty lists (containers)
                            List<Point3d> distance
                            List<double> distances = new List<double>();

                            for (int j = 0; j < occurence; j++)
                            {
                                EdgeHelper currentRepeatingEdge = this.PanelEdgeHelpers[i + j];
                                int currentMatchingIndex = this.EdgeToSegIds[i + j];

                                currentRepeatingEdge = currentMatchingIndex;
                            }
                        }
                    }

                }
            }
        }
        */
    }
}
