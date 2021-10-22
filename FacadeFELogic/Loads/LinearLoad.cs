using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace FacadeFELogic
{
    public class LinearLoad
    {
        //attributes
        public Line RefLine; //refernce line #Q: no need for a refplane?
        public double Fy1; //1st vector y
        public double Fy2; //2nd vector y
        public double Fz1; //1st vector z
        public double Fz2; //1st vector z

        public LinearLoad(Line refLine, double fy1, double fy2, double fz1, double fz2)
        {
            this.RefLine = refLine;
            this.Fy1 = fy1;
            this.Fy2 = fy2;
            this.Fz1 = fz1;
            this.Fz2 = fz2;
        }

        public LinearLoad()
        {
            this.RefLine = new Line();
            this.Fy1 = 0;
            this.Fy2 = 0;
            this.Fz1 = 0;
            this.Fz2 = 0;
        }

        //Method for combining linear loads
        public static bool CombineLinearLoads(List<LinearLoad> linearLoads, out LinearLoad combinedLoad)
        {
            combinedLoad = null;

            //if there is 1 load
            if (linearLoads.Count == 1)
            {
                combinedLoad = linearLoads[0];
                return true;
            }

            //if there are more loads
            else if (linearLoads.Count > 1)
            {
                //check if reflines are the same
                Line refLine = linearLoads[0].RefLine;
                bool isSameRef = true;

                //check all reflines
                foreach (LinearLoad linLoad in linearLoads)
                {
                    if (linLoad.RefLine != refLine) isSameRef = false;
                }

                //return false if not same
                if (!isSameRef) return false;

                //else: combine all
                else
                {
                    //temp variables
                    double fy1 = 0;
                    double fy2 = 0;
                    double fz1 = 0;
                    double fz2 = 0;
                    //loop through loads
                    foreach (LinearLoad linLoad in linearLoads)
                    {
                        fy1 += linLoad.Fy1;
                        fy2 += linLoad.Fy2;
                        fz1 += linLoad.Fz1;
                        fz2 += linLoad.Fz2;
                    }

                    //update object attributes
                    combinedLoad = new LinearLoad(linearLoads[0].RefLine, fy1, fy2, fz1, fz2);
                    return true;
                }
            }

            //case of empty input
            else return false;
        }

        internal static bool CombineLinearLoads(List<LinearLoad> linearLoads, out object combinedLoad)
        {
            throw new NotImplementedException();
        }

        public void FlipRefLine()
        {

            Line refLine = new Line(this.RefLine.PointAt(1), this.RefLine.PointAt(0));
            this.RefLine = refLine;
        }

        public void FlipVectorsOrder()
        {
            //temp variables
            double fy1 = this.Fy2;
            double fy2 = this.Fy1;
            double fz1 = this.Fz2;
            double fz2 = this.Fz1;

            //replace variables
            this.Fy1 = fy1;
            this.Fy2 = fy2;
            this.Fz1 = fz1;
            this.Fz2 = fz2;
        }

    }
}
