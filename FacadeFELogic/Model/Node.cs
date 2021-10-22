using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using FacadeFELogic.Helper;

namespace FacadeFELogic
{
    public class Node
    {
        //Atributes
        public Point3d Position;
        public PointLoad Load;
        public double WindFELoad; //for FESubdivision windloads

        //Constructors
        public Node()
        {
            this.Position = new Point3d();
            this.Load = null; //set load as null
            this.WindFELoad = -1; // -1 in this cas means does not exist

    }

        public Node(Point3d position)
        {
            this.Position = position;
            this.Load = null; //set load as null
            this.WindFELoad = -1;  // -1 in this cas means does not exist
        }

        public Node(Point3d position, PointLoad load)
        {
            this.Position = position;
            this.Load = load;
            this.WindFELoad = -1;  // -1 in this cas means does not exist
        }

        //Methods
        public void AddFEWindLoad(double loadValue)
        {
            if (this.WindFELoad == -1) this.WindFELoad = loadValue;
            else if (loadValue != -1) this.WindFELoad += loadValue;
        }

        //method to convert FEParam to node
        public static Node FEParamToNode(FEParam feParam, Curve edge)
        {
            //position point
            Point3d position = edge.PointAt(feParam.Param);

            //initialise node depending on feParam type
            if (feParam.IsHinge)
            {
                Hinge hinge = new Hinge(position, feParam.HingeFreedom, feParam.HingeStiffness);
                hinge.WindFELoad = feParam.WindFELoad;
                hinge.Load = feParam.PointLoad;

                return hinge;
            }

            else if (feParam.IsSupport)
            {
                Support support = new Support(position, feParam.TranIndicator, feParam.RotIndicator);
                support.WindFELoad = feParam.WindFELoad;
                support.Load = feParam.PointLoad;

                return support;
            }

            else
            { 
                Node node = new Node(position);
                node.WindFELoad = feParam.WindFELoad;
                node.Load = feParam.PointLoad;
                return node;
            }

        }

        public static bool NodeToFEParam(Node node, EdgeHelper edgeHelper, double tolerance, out FEParam nodeFEParam)
        {
            //get param on edge
            edgeHelper.Edge.ClosestPoint(node.Position, out double nodeParam);

            //check if node is on curve (Curve.contains may work, but including tolerance is safer)
            bool onEdgeCondition = HelperClass.IsPointOnEdge(node.Position, edgeHelper.Edge, tolerance);

            if (!onEdgeCondition)
            {
                nodeFEParam = null;
                return false;
            }

            else
            {
                //get corresponding FEParam if existing
                if (edgeHelper.FEParams.Any(f => HelperClass.ToleranceEqual(f.Param, nodeParam, tolerance)))
                {
                    //if existing, retrieve it (double calculation using Linq Find?)
                    nodeFEParam = edgeHelper.FEParams.Find(f => HelperClass.ToleranceEqual(f.Param, nodeParam, tolerance));
                }

                else
                {
                    nodeFEParam = new FEParam(nodeParam, isCorner: false);
                }


                //add point load of the node if existing
                if ( !(node.Load is null))
                {
                    nodeFEParam.AddPointLoad(node.Load, tolerance);
                }

                return true;
            }
        }

        public bool AddLoads(double tolerance, params PointLoad[] pointLoads)
        {
            //remove null loads (as they can figure in FEParams and nodes (as a default value)
            List<PointLoad> pointLoadList = pointLoads.ToList();
            pointLoadList.RemoveAll(p => p == null);

            //check if node does not have already a load
            if (this.Load is null)
            {
                //check same position in case of 1 load
                if (pointLoadList.Count == 1 && pointLoadList[0].Position == this.Position)
                {
                    this.Load = pointLoads[0];
                    return true;
                }

                //case of list of loads
                else if (pointLoadList.Count > 1 && pointLoadList[0].Position == this.Position)
                {
                    //combine loads
                    bool addLoads = PointLoad.CombinePointLoads(pointLoadList, tolerance, out PointLoad combinedLoad);

                    //update attribute
                    if (addLoads)
                    {
                        this.Load = combinedLoad;
                        return true;
                    }
                    else return false;
                }

                //else return false
                else return false;
            }
            //else: update existing load.
            else
            {
                //add attribute to list
                pointLoadList.Add(this.Load);

                //combine loads
                bool addLoads = PointLoad.CombinePointLoads(pointLoadList, tolerance, out PointLoad combinedLoad);

                //update attribute
                if (addLoads)
                {
                    this.Load = combinedLoad;
                    return true;
                }

                else return false;
            }



        }
    }
}
