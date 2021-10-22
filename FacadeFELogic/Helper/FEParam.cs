using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacadeFELogic.Helper
{
    public class FEParam
    {
        //ATTRIBUTES
        //general
        public double Param;
        public bool IsCorner; //IsCorner is useful later when creating nodes from FEParams, corner FEParams should not be added twice (becauce they are shared between several edges)
        
        //probably adding a param for FE linearloads
        public double WindFELoad;

        //just maybe for later, add ispanelcorner (related more to distinguish panels corner points)
        public bool IsPanelCorner;

        //Hinge-related
        public bool IsHinge;
        public HingeFreedom HingeFreedom; //0 is locked, 1 is free
        public HingeStiffness HingeStiffness; //0 is stiff, >0 is spring

        //Support-related
        public bool IsSupport;
        public int TranIndicator;
        public int RotIndicator;

        //point load (for later)
        public PointLoad PointLoad;

        //constructor
        public FEParam(double param, bool isCorner) //no windFELoad in this case
        {
            this.Param = param;
            this.IsCorner = isCorner;
            this.WindFELoad = -1; // -1 in this cas means does not exist

            //just to make sure, by default hinge and support are off
            this.IsHinge = false;
            this.IsSupport = false;

            this.PointLoad = null;
        }

        public FEParam(double param, bool isCorner, double windFELoad) //existing windFELoad
        {
            this.Param = param;
            this.IsCorner = isCorner;
            this.WindFELoad = windFELoad;

            //just to make sure, by default hinge and support are off
            this.IsHinge = false;
            this.IsSupport = false;

            this.PointLoad = null;
        }

        //methods
        public void AddFEWindLoad(double loadValue)
        {
            if (this.WindFELoad == -1) this.WindFELoad = loadValue;
            else if (loadValue != -1) this.WindFELoad += loadValue;
        }

        public bool AddPointLoad(PointLoad pointLoad, double tolerance)
        {
            if (this.PointLoad is null)
            {
                this.PointLoad = pointLoad;
                return true;
            }

            //else add it to existing
            else
            {
                //make list
                List<PointLoad> loads = new List<PointLoad>() { this.PointLoad, pointLoad };
                //make combination
                bool combine = PointLoad.CombinePointLoads(loads, tolerance, out PointLoad combinedLoad);
                if (combine)
                {
                    this.PointLoad = combinedLoad;
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public bool IsInEdgeHelperFEParams(EdgeHelper edgeHelper, double tolerance, out int feParamIndex)
        {
            feParamIndex = edgeHelper.FEParams.FindIndex(f => HelperClass.ToleranceEqual(f.Param, this.Param, tolerance));

            if (feParamIndex != -1) //-1 means not found
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
