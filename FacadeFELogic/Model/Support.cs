using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

using FacadeFELogic.Helper;

namespace FacadeFELogic
{
    public class Support : Node
    {
        //Additional attributes
        public bool TranXCondition;
        public bool TranYCondition;
        public bool TranZCondition;
        public bool RotXCondition;
        public bool RotYCondition;
        public bool RotZCondition;

        //constructors
        public Support(Point3d position, bool TranX, bool TranY, bool TranZ, bool RotX, bool RotY, bool RotZ)
        {
            this.Position = position;
            this.TranXCondition = TranX;
            this.TranYCondition = TranY;
            this.TranZCondition = TranZ;
            this.RotXCondition = RotX;
            this.RotYCondition = RotY;
            this.RotZCondition = RotZ;
        }

        public Support(Point3d position, int translationIndicator, int rotationIndicator)
        {
            this.Position = position;
            this.UpdateAttributesFromTranIndicator(translationIndicator);
            this.UpdateAttributesFromRotIndicator(rotationIndicator);
        }


        //methods
        public int GetTranIndicator()
        {
            if (this.TranXCondition && !this.TranYCondition && !this.TranZCondition) return 1;
            else if (!this.TranXCondition && this.TranYCondition && !this.TranZCondition) return 2;
            else if (this.TranXCondition && this.TranYCondition && !this.TranZCondition) return 3;
            else if (!this.TranXCondition && !this.TranYCondition && this.TranZCondition) return 4;
            else if (this.TranXCondition && !this.TranYCondition && this.TranZCondition) return 5;
            else if (!this.TranXCondition && this.TranYCondition && this.TranZCondition) return 6;
            else if (this.TranXCondition && this.TranYCondition && this.TranZCondition) return 7;
            else return 0;
        }

        public int GetRotIndicator()
        {
            if (this.RotXCondition && !this.RotYCondition && !this.RotZCondition) return 1;
            else if (!this.RotXCondition && this.RotYCondition && !this.RotZCondition) return 2;
            else if (this.RotXCondition && this.RotYCondition && !this.RotZCondition) return 3;
            else if (!this.RotXCondition && !this.RotYCondition && this.RotZCondition) return 4;
            else if (this.RotXCondition && !this.RotYCondition && this.RotZCondition) return 5;
            else if (!this.RotXCondition && this.RotYCondition && this.RotZCondition) return 6;
            else if (this.RotXCondition && this.RotYCondition && this.RotZCondition) return 7;
            else return 0;
        }

        public bool UpdateAttributesFromTranIndicator(int tranCondition)
        {
            if (tranCondition == 1) { this.TranXCondition = true; this.TranYCondition = false; this.TranZCondition = false; return true; }
            else if (tranCondition == 2) { this.TranXCondition = false; this.TranYCondition = true; this.TranZCondition = false; return true; }
            else if (tranCondition == 3) { this.TranXCondition = true; this.TranYCondition = true; this.TranZCondition = false; return true; }
            else if (tranCondition == 4) { this.TranXCondition = false; this.TranYCondition = false; this.TranZCondition = true; return true; }
            else if (tranCondition == 5) { this.TranXCondition = true; this.TranYCondition = false; this.TranZCondition = true; return true; }
            else if (tranCondition == 6) { this.TranXCondition = false; this.TranYCondition = true; this.TranZCondition = true; return true; }
            else if (tranCondition == 7) { this.TranXCondition = false; this.TranYCondition = true; this.TranZCondition = true; return true; }
            else return false;
        }

        public bool UpdateAttributesFromRotIndicator(int rotCondition)
        {
            if (rotCondition == 1) { this.RotXCondition = true; this.RotYCondition = false; this.RotZCondition = false; return true; }
            else if (rotCondition == 2) { this.RotXCondition = false; this.RotYCondition = true; this.RotZCondition = false; return true; }
            else if (rotCondition == 3) { this.RotXCondition = true; this.RotYCondition = true; this.RotZCondition = false; return true; }
            else if (rotCondition == 4) { this.RotXCondition = false; this.RotYCondition = false; this.RotZCondition = true; return true; }
            else if (rotCondition == 5) { this.RotXCondition = true; this.RotYCondition = false; this.RotZCondition = true; return true; }
            else if (rotCondition == 6) { this.RotXCondition = false; this.RotYCondition = true; this.RotZCondition = true; return true; }
            else if (rotCondition == 7) { this.RotXCondition = false; this.RotYCondition = true; this.RotZCondition = true; return true; }
            else return false;
        }

        public bool SupportToFEParam(EdgeHelper edgeHelper, double tolerance)
        {
            //get corresponding FEParam
            bool getFEParam = Node.NodeToFEParam(this, edgeHelper, tolerance, out FEParam supportFEParam);

            if (!getFEParam)
            {
                return false;
            }

            else
            {
                supportFEParam.IsSupport = true;
                supportFEParam.TranIndicator = this.GetTranIndicator();
                supportFEParam.RotIndicator = this.GetRotIndicator();
            }

            //check if FEParam exists in edgeHelper FEParams
            if (supportFEParam.IsInEdgeHelperFEParams(edgeHelper, tolerance, out int feParamIndex))
            {
                edgeHelper.FEParams[feParamIndex] = supportFEParam;
            }
            else
            {
                edgeHelper.FEParams.Add(supportFEParam);
            }

            return true;
        }
    }
}
