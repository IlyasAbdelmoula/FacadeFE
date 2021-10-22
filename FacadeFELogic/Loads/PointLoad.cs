using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

using FacadeFELogic.Helper;

namespace FacadeFELogic
{
    public class PointLoad
    {
        //attributes
        public Point3d Position;
        public double Fx; //force along x
        public double Fy;
        public double Fz;
        public double Mx; //moment around x
        public double My;
        public double Mz;

        //constructors
        public PointLoad(Point3d position, double fx, double fy, double fz, double mx, double my, double mz)
        {
            this.Position = position;
            this.Fx = fx;
            this.Fy = fy;
            this.Fz = fz;
            this.Mx = mx;
            this.My = my;
            this.Mz = mz;
        }

        public PointLoad()
        {
            this.Position = new Point3d();
            this.Fx = 0;
            this.Fy = 0;
            this.Fz = 0;
            this.Mx = 0;
            this.My = 0;
            this.Mz = 0;
        }


        public PointLoad(List<PointLoad> pointLoads, double tolerance)
        {
            bool combine = PointLoad.CombinePointLoads(pointLoads, tolerance, out PointLoad combinedLoad);

            if (!combine)
            {
                //if not same positions, return exception error
                throw new ArgumentException(" Point loads cannot be combined because they are not associated to the same reference point", "pointLoads");
            }

            else
            {
                this.Position = combinedLoad.Position;
                this.Fx = combinedLoad.Fx;
                this.Fy = combinedLoad.Fy;
                this.Fz = combinedLoad.Fz;
                this.Mx = combinedLoad.Mx;
                this.My = combinedLoad.My;
                this.Mz = combinedLoad.Mz;
            }

            //previous, redundant code
            /*
            //if there is one load
            if (pointLoads.Count == 1)
            {
                this.Position = pointLoads[0].Position;
                this.Fx = pointLoads[0].Fx;
                this.Fy = pointLoads[0].Fy;
                this.Fz = pointLoads[0].Fz;
                this.Mx = pointLoads[0].Mx;
                this.My = pointLoads[0].My;
                this.Mz = pointLoads[0].Mz;
            }

            //else: combine all
            else if (pointLoads.Count > 1)
            {
                //set first load as reference to check if all have the same point
                Point3d refPosition = pointLoads[0].Position;
                bool isSamePosition = true;

                //check all positions
                foreach (PointLoad load in pointLoads)
                {
                    if (load.Position != refPosition) isSamePosition = false;
                }
                //if not same positions, return exception error
                if (!isSamePosition) throw new ArgumentException(" Point loads cannot be combined because they are not associated to the same reference point", "pointLoads");

                //else: combine all attributes into 1
                else
                {
                    //temp variables
                    double fx = 0;
                    double fy = 0;
                    double fz = 0;
                    double mx = 0;
                    double my = 0;
                    double mz = 0;
                    //loop through loads
                    foreach (PointLoad load in pointLoads)
                    {
                        fx += load.Fx;
                        fy += load.Fy;
                        fz += load.Fz;
                        mx += load.Mx;
                        my += load.My;
                        mz += load.Mz;
                    }
                    //update object attributes
                    this.Position = pointLoads[0].Position;
                    this.Fx = fx;
                    this.Fy = fy;
                    this.Fz = fz;
                    this.Mx = mx;
                    this.My = my;
                    this.Mz = mz;

                }
            }
            */
        }

        //Method for PointLoad to FEParam
        public bool PointLoadToFEParam(EdgeHelper edgeHelper, double tolerance)
        {
            //make node out of pointLoad
            Node ptLoadNode = new Node(this.Position, this);

            //get FEParam
            bool getFEParam = Node.NodeToFEParam(ptLoadNode, edgeHelper, tolerance, out FEParam pointLoadFEParam);
            
            if (!getFEParam)
            {
                return false;
            }
            else
            {
                //check if FEParam exists in edgeHelper FEParams // #potential #bug (tolerance should not be integrated in params, but in distances only)
                if (pointLoadFEParam.IsInEdgeHelperFEParams(edgeHelper, tolerance, out int feParamIndex))
                {
                    edgeHelper.FEParams[feParamIndex] = pointLoadFEParam;
                }
                else
                {
                    edgeHelper.FEParams.Add(pointLoadFEParam);
                }
                return true;
            }
        }

        //Method for combining point loads
        public static bool CombinePointLoads(List<PointLoad> pointLoads, double tolerance, out PointLoad combinedLoad)
        {
            combinedLoad = null;

            //maybe remove null loads (as combining loads is used inside nodes which have null loads by default, but no need for that, as Node.AddLoads function deals with that)
            //pointLoads.RemoveAll(p => p == null);

            //if there is one load
            if (pointLoads.Count == 1)
            {
                combinedLoad = pointLoads[0];
                return true;
            }

            //else: combine all
            else if (pointLoads.Count > 1)
            {
                //set first load as reference to check if all have the same point
                Point3d refPosition = pointLoads[0].Position;
                bool isSamePosition = true;

                //check all positions
                foreach (PointLoad load in pointLoads)
                {
                    if (load.Position.DistanceTo(refPosition) > tolerance) isSamePosition = false;
                }
                //if not same positions, return exception error
                if (!isSamePosition) return false;

                //else: combine all attributes into 1
                else
                {
                    //temp variables
                    double fx = 0;
                    double fy = 0;
                    double fz = 0;
                    double mx = 0;
                    double my = 0;
                    double mz = 0;
                    //loop through loads
                    foreach (PointLoad load in pointLoads)
                    {
                        fx += load.Fx;
                        fy += load.Fy;
                        fz += load.Fz;
                        mx += load.Mx;
                        my += load.My;
                        mz += load.Mz;
                    }
                    //update object attributes
                    combinedLoad = new PointLoad(pointLoads[0].Position, fx, fy, fz, mx, my, mz);
                    return true;
                }
            }

            //case of empty input
            else return false;
        }

    }


}
