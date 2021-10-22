using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace FacadeFELogic
{
    public class Element
    {
        //Attributes
        public Line RefLine;
        public Point3d Start;
        public Point3d End;
        public Material Material;
        public Section Section;
        public LinearLoad Load;

        //constructor
        public Element(Line refLine, Material material, Section section)
        {
            this.RefLine = refLine;
            this.Start = refLine.PointAt(0);
            this.End = refLine.PointAt(1);
            this.Material = material;
            this.Section = section;
        }

        //Methods
        public bool AddLoads(List<LinearLoad> linearLoads)
        {
            //check if element does not have already a load
            if (this.Load is null)
            {
                //check same line in case of 1 load
                if (linearLoads.Count == 1 && linearLoads[0].RefLine == this.RefLine)
                {
                    this.Load = linearLoads[0];
                    return true;
                }

                //case of list of loads
                else if (linearLoads.Count > 1 && linearLoads[0].RefLine == this.RefLine)
                {
                    bool addLoads = LinearLoad.CombineLinearLoads(linearLoads, out LinearLoad combinedLoad);
                    //update attribute
                    if (addLoads)
                    {
                        this.Load = combinedLoad;
                        return true;
                    }

                    //else return false
                    else return false;
                }

                else return false;
            }

            //else: update existing load
            else
            {
                //add attribute to list
                linearLoads.Add(this.Load);

                //combine loads
                bool addLoads = LinearLoad.CombineLinearLoads(linearLoads, out LinearLoad combinedLoad);
                
                //update attribute
                if (addLoads)
                {
                    this.Load = combinedLoad;
                    return true;
                }

                //else return false
                else return false;
            }
        }
    }
}
