using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

using FacadeFELogic.Helper;

namespace FacadeFELogic
{
    public class Hinge : Node
    {
        //additional attributes
        public HingeFreedom HingeFreedom; //0 is locked, 1 is free
        public HingeStiffness HingeStiffness; //0 is stiff, >0 is spring

        //constructor
        public Hinge(Point3d position, HingeFreedom freedom, HingeStiffness stiffness)
        {
            this.Position = position;
            this.HingeFreedom = freedom; //HingeFreedom and HingeStiffness may be not necessery after all, an array would be enough
            this.HingeStiffness = stiffness; //HingeStiffness may be not necessery after all, an array would be enough
        }

        //methods
        public bool HingeToFEParam(EdgeHelper edgeHelper, double tolerance)
        {
            //get corresponding FEParam
            bool getFEParam = Node.NodeToFEParam(this, edgeHelper, tolerance, out FEParam hingeFEParam);

            if (!getFEParam)
            {
                return false;
            }

            else
            {
                hingeFEParam.IsHinge = true;
                hingeFEParam.HingeFreedom = this.HingeFreedom;
                hingeFEParam.HingeStiffness = this.HingeStiffness;
            }

            //check if FEParam exists in edgeHelper FEParams
            if (hingeFEParam.IsInEdgeHelperFEParams(edgeHelper, tolerance, out int feParamIndex))
            {
                edgeHelper.FEParams[feParamIndex] = hingeFEParam;
            }
            else
            {
                edgeHelper.FEParams.Add(hingeFEParam);
            }

            return true;
        }
    }
}
