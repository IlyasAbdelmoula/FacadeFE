using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace FacadeFELogic.Helper
{
    public class PanelOrderHelper
    {
        //attributes
        public Brep Panel;
        public Point3d RefPoint;
        public Point3d PanelCentroid;
        public double LocalX;
        public double LocalY;
        public double LocalZ;

        //constructor
        public PanelOrderHelper(Brep panel, Point3d refPoint)
        {
            this.Panel = panel;
            this.RefPoint = refPoint;

            //Get centroid
            AreaMassProperties panelProps = AreaMassProperties.Compute(this.Panel);
            Point3d centroid = panelProps.Centroid;
            this.PanelCentroid = centroid;

            //get local coordinates
            this.LocalX = Math.Abs(centroid.X - refPoint.X);
            this.LocalY = Math.Abs(centroid.Y - refPoint.Y);
            this.LocalZ = Math.Abs(centroid.Z - refPoint.Z);
        }
    }
}
