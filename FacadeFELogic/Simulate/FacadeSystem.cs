using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Rhino.Geometry;

using FacadeFELogic.Helper;

namespace FacadeFELogic
{
    public class FacadeSystem
    {
        //attributes
        public Brep FacadeBrep;
        public List<Curve> OrderedEdges;
        public List<EdgeHelper> EdgeHelpers;
        public List<PanelHelper> PanelHelpers;
        public Plane RefPlane;
        public List<Node> Nodes; //implicitely including hinges and supports
        public List<Element> Elements;
        public List<ElementFE> ElementsFE;

        //(#JustForDebugging)
        public List<Brep> OrderedPanels;


        //loads

        //constant
        //private static  double ModelTolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

        //Constructor
        public FacadeSystem()
        {
            this.FacadeBrep = new Brep();
            this.EdgeHelpers = new List<EdgeHelper>();
            this.PanelHelpers = new List<PanelHelper>();
            this.Nodes = new List<Node>();
            this.Elements = new List<Element>();
            this.ElementsFE = new List<ElementFE>();
        }

        public FacadeSystem(List<Brep> facadePanels, Point3d refPoint)
        {
            //order and join panels
            this.FacadeBrep = FacadeSystem.OrderJoinPanels(facadePanels, refPoint, out List<Brep> orderedPanelBreps);

            //add refPlane
            this.RefPlane = FacadeSystem.ConstructRefPlane(this.FacadeBrep, refPoint);

            //order edges (anti-clockwise based on the user-defined RefPoint)
            this.OrderedEdges = FacadeSystem.OrderEdges(this.FacadeBrep, this.RefPlane);

            //Ordered panels (#JustForDebugging)
            this.OrderedPanels = orderedPanelBreps;

            this.EdgeHelpers = new List<EdgeHelper>();
            this.PanelHelpers = new List<PanelHelper>();
            this.Nodes = new List<Node>();
            this.Elements = new List<Element>();
            this.ElementsFE = new List<ElementFE>();

            //Add EdgeHelpers (ordered based on this.OrderedEdges)
            this.AddEdgeHelpers();

            //add PanelHelpers
            this.AddPanelHelpers();
        }

        private static double GetModelTolerence()
        {
            return RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
        }


        //FINITE ELEMENT PROCESSING METHOD (main method)
        public bool FEProcess(List<Element> elements, List<Support> supports, List<Hinge> hinges, List<PointLoad> pointLoads, out List<string> stopWarningMessages)
        {
            stopWarningMessages = new List<string>();

            //Add elements (mainly material & section data
            bool addElements = this.AddElements(elements);
            if (!addElements)
            {
                stopWarningMessages.Add("Cannot generate FacadeSystem: not all facade reference lines have corresponding elements(material, section).");
            }

            //Finite Element Subdivide
            bool subdivide = this.FESubdivide();


            //ADD USER-INPUT
            //supports and hinges
            this.AddSupports(supports);
            this.AddHinges(hinges);
            //loads
            this.AddPointLoads(pointLoads);


            //GENERATE NODE LIST
            this.GenerateNodeList();

            //GENERATE ELEMENT LIST
            this.GenerateElementsFEList();


            return subdivide;
        }

        public bool FESubdivide()
        {
            //check if there are nonManifold edges/isolated edges stop Method
            if (this.CheckNonManifoldNone()) return false;

            //LOOP OVER PANELHELPERS (TO SUBDIVIDE THEM)
            bool panelSubdivideSuccess = true;
            foreach (PanelHelper panelHelper in this.PanelHelpers)
            {
                bool subSucess = panelHelper.Subdivide(FacadeSystem.GetModelTolerence());
                if (!subSucess) panelSubdivideSuccess = false;
            }
            //return either success or not
            return panelSubdivideSuccess;

        }

        private void AddEdgeHelpers()
        {
            //loop through edges and add EdgHelpers
            foreach (Curve edge in this.OrderedEdges)
            {
                //initialize edgeHelper
                EdgeHelper edgeHelper = new EdgeHelper(edge, this.OrderedEdges, this.FacadeBrep.Edges); //getting index from OrderedEdges list rather then Brep
                this.EdgeHelpers.Add(edgeHelper);
            }
        }

        private void AddPanelHelpers()
        {
            foreach (BrepFace panel in this.FacadeBrep.Faces)
            {
                PanelHelper panelHelper = new PanelHelper(panel, this);

                //add panelHelper to global PanelHelpers attribute
                this.PanelHelpers.Add(panelHelper);
            }
        }

        public bool AddElements(List<Element> elements)
        {
            List<EdgeHelper> matchingEdgeHelpers = new List<EdgeHelper>();

            foreach (Element element in elements)
            {
                int matchingId = this.EdgeHelpers.FindIndex(e => (e.Edge.PointAtStart.DistanceTo(element.Start) < FacadeSystem.GetModelTolerence() &&
                                                               e.Edge.PointAtEnd.DistanceTo(element.End) < FacadeSystem.GetModelTolerence()) ||
                                                               (e.Edge.PointAtStart.DistanceTo(element.End) < FacadeSystem.GetModelTolerence()) &&
                                                               (e.Edge.PointAtEnd.DistanceTo(element.Start) < FacadeSystem.GetModelTolerence()));

                if (matchingId != -1)
                {
                    EdgeHelper matchingeEdgeHelper = this.EdgeHelpers[matchingId];
                    matchingeEdgeHelper.AddElementProperties(element);

                    matchingEdgeHelpers.Add(matchingeEdgeHelper);
                }
            }

            if (matchingEdgeHelpers.Count == this.EdgeHelpers.Count) return true;
            else return false;
        }

        private void GenerateNodeList()
        {
            //the main list of nodes (implicitely including hinges and supports)
            List<Node> orderedNodes = new List<Node>();

            //list to keep track of corner points parameters (mainly to skip FEParam + merge their FEWindLoads)
            List<Node> processedCornerNodes = new List<Node>();

            //loop through ordered edgehelpers
            foreach (EdgeHelper edgeHelper in this.EdgeHelpers)
            {
                //order FEParams
                edgeHelper.OrderFEParams();

                //loop through FE Params
                foreach (FEParam feParam in edgeHelper.FEParams)
                {
                    //corresponding point //maybe double checking
                    Point3d fePoint = edgeHelper.Edge.PointAt(feParam.Param);
                    bool isEndPoint = false;
                    if (edgeHelper.Edge.PointAtStart.DistanceTo(fePoint) < FacadeSystem.GetModelTolerence() ||
                        edgeHelper.Edge.PointAtEnd.DistanceTo(fePoint) < FacadeSystem.GetModelTolerence())
                    {
                        isEndPoint = true;
                    }

                    //check if corner
                    if (feParam.IsCorner || isEndPoint)
                    {
                        //if the corner already processed
                        bool cornerIsProcessed = processedCornerNodes.Any(n => n.Position.DistanceTo(fePoint) < FacadeSystem.GetModelTolerence());
                        if (cornerIsProcessed)
                        {
                            //find matching corner node
                            Node node = processedCornerNodes.Find(n => n.Position.DistanceTo(fePoint) < FacadeSystem.GetModelTolerence());
                            
                            //check if type of FEParam is different from the existing node, update it then //#potential #bug of overwriting, when user enters the same point as support and hinge
                            if (feParam.IsHinge || feParam.IsSupport)
                            {
                                Node hingeOrSupport = Node.FEParamToNode(feParam, edgeHelper.Edge); //.FEParamToNode method implicitely return support, hinge or node depending on FEParam type
                                hingeOrSupport.AddLoads(FacadeSystem.GetModelTolerence(), node.Load);
                                hingeOrSupport.AddFEWindLoad(node.WindFELoad);
                                
                                //update node
                                node = hingeOrSupport;
                                //processedCornerNodes.Add(node);
                                //orderedNodes.Add(node);
                            }

                            else
                            {
                                //update node
                                node.AddLoads(FacadeSystem.GetModelTolerence(), feParam.PointLoad);
                                node.AddFEWindLoad(feParam.WindFELoad);

                                //processedCornerNodes.Add(node);
                                //orderedNodes.Add(node);
                            }

                            
                        }

                        else
                        {
                            Node node = Node.FEParamToNode(feParam, edgeHelper.Edge); //.FEParamToNode method implicitely return support, hinge or node depending on FEParam type

                            processedCornerNodes.Add(node);
                            orderedNodes.Add(node);
                        }
                    }

                    //if not corner, just create it and add it to ordered nodes
                    else
                    {
                        Node node = Node.FEParamToNode(feParam, edgeHelper.Edge); //.FEParamToNode method implicitely return support, hinge or node depending on FEParam type

                        orderedNodes.Add(node);
                    }
                }
            }

            //update attribute
            this.Nodes = orderedNodes;
        }

        private void GenerateElementsFEList()
        {
            foreach (EdgeHelper edgehelper in this.EdgeHelpers)
            {
                //list of params and list of subdivision segments
                List<double> paramList = edgehelper.FEParams.Select(p => p.Param).ToList();
                List<Curve> feSegments = new List<Curve>();

                //subdivide edge using params
                for (int i = 0; i < (paramList.Count - 1); i++)
                {
                    feSegments.Add(edgehelper.Edge.Trim(paramList[i], paramList[i + 1]));
                }

                //loop through segments
                foreach (Curve feSegment in feSegments)
                {
                    //get node ids (maybe no need for tolerance)
                    HelperClass.GetMatchingNodesIndex(feSegment, this.Nodes, FacadeSystem.GetModelTolerence(), out int startId, out int endId);

                    //make ElementFE
                    Curve refSegment = new Line(feSegment.PointAtStart, feSegment.PointAtEnd).ToNurbsCurve();
                    ElementFE elementFE = new ElementFE(startId, endId, refSegment, edgehelper.Material, edgehelper.Section);
                    this.ElementsFE.Add(elementFE);
                }

            }
        }

        public void AddHinges(List<Hinge> hinges)
        {
            foreach (Hinge hinge in hinges)
            {
                //find closest edgeHelper (or the one including it)
                EdgeHelper closestEdgeHelper = HelperClass.FindClosestEdgeHelper(hinge.Position, this.EdgeHelpers);

                //add corresponding FEParam (if closest edge is also containing the hinge)
                bool hingeToFEParam = hinge.HingeToFEParam(closestEdgeHelper, FacadeSystem.GetModelTolerence());
            }
        }

        public void AddSupports(List<Support> supports)
        {
            foreach (Support support in supports)
            {
                //find closest edgeHelper (or the one including it)
                EdgeHelper closestEdgeHelper = HelperClass.FindClosestEdgeHelper(support.Position, this.EdgeHelpers);

                //add corresponding FEParam (if closest edge is also containing the hinge)
                bool supportToFEParam = support.SupportToFEParam(closestEdgeHelper, FacadeSystem.GetModelTolerence());
            }
        }



        public void AddPointLoads(List<PointLoad> pointLoads)
        {
            //find closest edgeHelper (or the one including it)
            foreach (PointLoad pointLoad in pointLoads)
            {
                EdgeHelper closestEdgeHelper = HelperClass.FindClosestEdgeHelper(pointLoad.Position, this.EdgeHelpers);

                bool pointLoadToFEParam = pointLoad.PointLoadToFEParam(closestEdgeHelper, FacadeSystem.GetModelTolerence());
            }
        }

        public bool CheckNonManifoldNone()
        {
            foreach (EdgeHelper edgeHelper in this.EdgeHelpers)
            {
                if (edgeHelper.IsNone || edgeHelper.IsNonManifold) return true;
            }
            return false;
        }



        private static Brep OrderJoinPanels(List<Brep> facadePanels, Point3d refPoint, out List<Brep> orderedPanelBreps)
        {
            List<PanelOrderHelper> panelOrderHelpers = new List<PanelOrderHelper>();

            //create order helpers for panels
            foreach (Brep panel in facadePanels)
            {
                PanelOrderHelper panelOrderHelper = new PanelOrderHelper(panel, refPoint);
                panelOrderHelpers.Add(panelOrderHelper);
            }

            //order list by local coordinates (by Z, then by Y, then by X)
            panelOrderHelpers = panelOrderHelpers.OrderBy(o => o.LocalZ).ThenBy(o => o.LocalY).ThenBy(o => o.LocalX).ToList();

            //ordered panels
            List<Brep> orderedPanels = panelOrderHelpers.Select(o => o.Panel).ToList();
            orderedPanelBreps = orderedPanels;
            //join
            Brep facadeBrep = Brep.JoinBreps(orderedPanels, FacadeSystem.GetModelTolerence())[0]; //first because we assume all panels are connected (currently checked inside GH component before FacadeSystem  initialisation)

            return facadeBrep;
        }

        private static Plane ConstructRefPlane(Brep facadeBrep, Point3d refPoint)
        {
            //first frame
            facadeBrep.Faces[0].FrameAt(0, 0, out Plane firstFrame);

            //test angles with world X
            double angleX = Vector3d.VectorAngle(Plane.WorldXY.XAxis, firstFrame.XAxis);
            double angleY = Vector3d.VectorAngle(Plane.WorldXY.XAxis, firstFrame.YAxis);

            Vector3d xAxis;
            Vector3d yAxis;
            if (angleX < angleY)
            {
                xAxis = firstFrame.XAxis;
                yAxis = firstFrame.YAxis;
            }
            else
            {
                xAxis = firstFrame.YAxis;
                yAxis = firstFrame.XAxis;
            }

            Plane refPlane = new Plane(refPoint, xAxis, yAxis);
            return refPlane;
        }


        private static List<Curve> OrderEdges(Brep facadeBrep, Plane refPlane)
        {
            //declare containers

            List<Curve> orderedEdges = new List<Curve>();
            List<int> processedEdgeIds = new List<int>(); //for ordering edges within one panel
            List<Curve> checkedEdges = new List<Curve>(); // to check edges direction, end reverse it if necessary

            //loop through panels
            foreach (BrepFace panel in facadeBrep.Faces)
            {
                //declare local containers
                List<Curve> panelEdges = new List<Curve>();
                List<Curve> panelOrderedEdges = new List<Curve>();
                List<double> refPlaneEdgeAngles = new List<double>();

                //loop through edges (to sort them radially)
                foreach (int edgeId in panel.AdjacentEdges())
                {
                    //check if edge already processed. If so, skip it
                    if (processedEdgeIds.Contains(edgeId)) continue;

                    //add it to processed
                    processedEdgeIds.Add(edgeId);

                    //Edge
                    Curve edge = facadeBrep.Edges[edgeId].EdgeCurve;
                    panelEdges.Add(edge);

                    //get edge center point
                    Point3d edgeCenter = edge.PointAtNormalizedLength(0.5);

                    //edge to RefPlane vector
                    Vector3d EdgerefPlaneVec = edgeCenter - refPlane.Origin;

                    //get angles to XAxis
                    double edgeAngle = Vector3d.VectorAngle(refPlane.XAxis, EdgerefPlaneVec);
                    //temporary #bug fix
                    //if (edgeAngle > Math.PI) edgeAngle -= Math.PI;

                    refPlaneEdgeAngles.Add(edgeAngle);
                }

                //orderlist by the angles list (zipping them into one dictionary, then order it by angle values, then extract edge list again
                panelOrderedEdges = panelEdges.Zip(refPlaneEdgeAngles, (edge, angle) => new { edge, angle })
                                              .OrderBy(a => a.angle)
                                              .Select(a => a.edge)
                                              .ToList();

                //check edge direction and reverse if necessary
                for (int i = 0; i < panelOrderedEdges.Count; i++)
                {
                    //conditions
                    Curve currentEdge = panelOrderedEdges[i];

                    //skip if the first edge //#potentialBug if the first has the wrong direction already
                    if (i == 0 && checkedEdges.Count != 0)
                    {
                        bool conditionAllPreviousFirst = checkedEdges.Any(a => a.PointAtEnd == currentEdge.PointAtStart);
                        if (!conditionAllPreviousFirst) currentEdge.Reverse();
                    }

                    else if (i != 0)
                    {
                        //conditions
                        Curve previousEdge = panelOrderedEdges[i - 1];
                        bool conditionPrevious = false;

                        //condition with previous edge in the list
                        if (currentEdge.PointAtStart == previousEdge.PointAtEnd) conditionPrevious = true;
                        //check conditions and reverse edge if not applicable
                        if (!conditionPrevious)
                        {
                            //condition with all previous edges
                            bool conditionAllPrevious = checkedEdges.Any(a => a.PointAtEnd == currentEdge.PointAtStart);
                            if(!conditionAllPrevious) currentEdge.Reverse();
                        }
                    }

                    //add it to already checked
                    checkedEdges.Add(currentEdge);
                }

                //add panel ordered edges to the global orderEdges list
                orderedEdges.AddRange(panelOrderedEdges);
            }

                   

            return orderedEdges;
        }

        public List<Support> GetSupports(out List<int> supportIds)
        {
            List < Support > supports = this.Nodes.OfType<Support>().ToList();
            supportIds = new List<int>();

            foreach (Support support in supports)
            {
                supportIds.Add(this.Nodes.IndexOf(support));
            }

            return supports;
        }

        public List<Hinge> GetHinges(out List<int> hingeIds)
        {
            List<Hinge> hinges = this.Nodes.OfType<Hinge>().ToList();
            hingeIds = new List<int>();

            foreach (Hinge hinge in hinges)
            {
                hingeIds.Add(this.Nodes.IndexOf(hinge));
            }

            return hinges;
        }

        //METHODS FOR generating ASCII
        public List<string> GetNodeASCII()
        {
            List<string> nodeASCII = new List<string>();

            //add node count
            nodeASCII.Add(this.Nodes.Count.ToString());

            //loop through nodes
            foreach (Node node in this.Nodes)
            {
                string coordsString = String.Format("{0} {1} {2}", node.Position.X, node.Position.Y, node.Position.X);
                nodeASCII.Add(coordsString);
            }

            return nodeASCII;
        }

        public List<string> GetElementFeASCII()
        {
            List<string> elementFeASCII = new List<string>();

            //add elementFE count
            elementFeASCII.Add(this.ElementsFE.Count.ToString());

            //loop through elementsFE
            foreach (ElementFE elementFE in this.ElementsFE)
            {
                string nodeIdString = String.Format("{0} {1}", elementFE.StartNodeId, elementFE.EndNodeId);
                string materialString = String.Format("{0} {1} {2}", elementFE.Material.E, elementFE.Material.G, elementFE.Material.ExpansionCoef);
                string inertiaString = String.Format("{0} {1} {2}", elementFE.Section.Iy, elementFE.Section.Iz, elementFE.Section.J);
                string sectionString = String.Format("{0} {1} {2} {3} {4} {5}", elementFE.Section.A, elementFE.Section.Ay, elementFE.Section.Az, elementFE.Section.Hy, elementFE.Section.Hz, elementFE.Section.Alpha);
                
                elementFeASCII.Add(nodeIdString);
                elementFeASCII.Add(materialString);
                elementFeASCII.Add(inertiaString);
                elementFeASCII.Add(sectionString);
            }

            return elementFeASCII;
        }

        public List<String> GetBoundaryTranASCII()
        {
            List<string> boundaryTran = new List<string>();

            //get supports from node list
            List<Support> supports = this.GetSupports(out List<int> supportIds);

            for (int i = 0; i < supports.Count; i++)
            {
                int id = supportIds[i];
                int tranIndicator = supports[i].GetTranIndicator();

                if (tranIndicator != 0)
                {
                    string supportTranString = String.Format("{0} {1}", id, tranIndicator);
                    boundaryTran.Add(supportTranString);
                }
            }

            //add boundary count to the beginning of the list
            int tranCount = boundaryTran.Count;
            boundaryTran.Insert(0, tranCount.ToString());
            return boundaryTran;
        }


        public List<String> GetBoundaryRotASCII()
        {
            List<string> boundaryRot = new List<string>();

            //get supports from node list
            List<Support> supports = this.GetSupports(out List<int> supportIds);

            for (int i = 0; i < supports.Count; i++)
            {
                int id = supportIds[i];
                int rotIndicator = supports[i].GetRotIndicator();

                if (rotIndicator != 0)
                {
                    string supportTranString = String.Format("{0} {1}", id, rotIndicator);
                    boundaryRot.Add(supportTranString);
                }
            }

            //add boundary count to the beginning of the list
            int rotCount = boundaryRot.Count;
            boundaryRot.Insert(0, rotCount.ToString());

            return boundaryRot;
        }

        public List<String> GetHingesASCII()
        {
            List<String> hingeASCII = new List<string>();

            List<Hinge> hinges = this.GetHinges(out List<int> hingeIds);

            hingeASCII.Add(hinges.Count.ToString());

            for (int i = 0; i < hinges.Count; i++)
            {
                int id = hingeIds[i];

                string freedomString = String.Join(" ", hinges[i].HingeFreedom.FreedomVector);
                string stiffnessString = String.Join(" ", hinges[i].HingeStiffness.StiffnessVector);

                hingeASCII.Add(id.ToString());
                hingeASCII.Add(freedomString);
                hingeASCII.Add(stiffnessString);
            }

            return hingeASCII;
        }

        public List<String> GetLoadsASCII()
        {
            List<String> loadASCII = new List<string>();

            //get ptLoad and LinearLoad string lists
            List<String> ptLoadASCII = this.GetPointLoadsASCII();
            List<String> linearLoadASCII = this.GetLinearLoadsASCII();

            //add count
            loadASCII.Add((ptLoadASCII.Count + linearLoadASCII.Count).ToString());

            //add load strings
            loadASCII.AddRange(ptLoadASCII);
            loadASCII.AddRange(linearLoadASCII);

            return loadASCII;
        }

        private List<String> GetPointLoadsASCII()
        {
            //string list
            List<String> ptLoadASCII = new List<string>();

            List<PointLoad> ptLoads = this.Nodes.Select(n => n.Load).Where(l => l != null).ToList();

            foreach (PointLoad ptLoad in ptLoads)
            {
                //get corresponding node Id
                int id = this.Nodes.IndexOf( this.Nodes.Find(n => n.Load == ptLoad) );

                String ptLoadString = String.Format("{0} {1} {2} {3} {4} {5} {6} {7}", 0, id, ptLoad.Fx, ptLoad.Fy, ptLoad.Fz, ptLoad.Mx, ptLoad.My, ptLoad.Mz); // 0 as the pointLoad code

                ptLoadASCII.Add(ptLoadString);
            }

            return ptLoadASCII;
        }

        //linear Loadsfor later 
        private List<String> GetLinearLoadsASCII()
        {
            return new List<string>();
        }

        //SortEdges method is no more relevant here
        private bool SortEdges(out List<EdgeHelper> nakedEdges, out List<EdgeHelper> interiorEdges, out List<EdgeHelper> nonManifoldEdges, out List<EdgeHelper> noneEdges)
        {
            //initialise variables
            nakedEdges = new List<EdgeHelper>();
            interiorEdges = new List<EdgeHelper>();
            nonManifoldEdges = new List<EdgeHelper>();
            noneEdges = new List<EdgeHelper>();
            /*
            //List out of edges list
            List<BrepEdge> edgeList = this.FacadePolysurface.Edges.ToList();
            */
            //edge valence
            foreach (EdgeHelper edgeHelper in this.EdgeHelpers)
            {
                //valence
                int valence = (int)edgeHelper.Valence;

                //sort edge according to type
                if (valence == 0)
                {
                    edgeHelper.IsNone = true;
                    noneEdges.Add(edgeHelper);
                }
                else if (valence == 1)
                {
                    edgeHelper.IsNaked = true;
                    nakedEdges.Add(edgeHelper);
                }
                else if (valence == 2)
                {
                    edgeHelper.IsInterior = true;
                    interiorEdges.Add(edgeHelper);
                }
                else if (valence > 2)
                {
                    edgeHelper.IsNonManifold = true;
                    nonManifoldEdges.Add(edgeHelper); 
                }
            }

            //check if there are noneEdges or nonManifold
            if (noneEdges.Count > 0 | nonManifoldEdges.Count > 0) return false;
            else return true;
        }

    }
}
