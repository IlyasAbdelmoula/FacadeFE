using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace FacadeFE
{
    public class PointLoadItem_Component : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PointLoadItem_Component()
          : base("PointLoad", "ptload",
              "Creates point load with a position/positions,  force and moment values associated to axes.",
              "FacadeFE", "3_Loads")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "pos", "Position point/points of the load", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fx", "fx", "Force along X axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fy", "fy", "Force along Y axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fz", "fz", "Force along Z axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mx", "Mx", "Moment along X axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("My", "My", "Moment along Y axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mz", "Mz", "Moment along Z axis", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PointLoad", "ptload", "Point load", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //initialize variables
            //input
            Point3d position = new Point3d();
            double fx = 0;
            double fy = 0;
            double fz = 0;
            double mx = 0;
            double my = 0;
            double mz = 0;

            //retrieve input
            if(!DA.GetData(0, ref position)) return;
            DA.GetData(1, ref fx);
            DA.GetData(2, ref fy);
            DA.GetData(3, ref fz);
            DA.GetData(4, ref mx);
            DA.GetData(5, ref my);
            DA.GetData(6, ref mz);

            if (fx == 0 && fy == 0 && fz == 0 && mx == 0 && my == 0 && mz == 0) return;

            //initialize pointload
            PointLoad ptLoad = new PointLoad(position, fx, fy, fz, mx, my, mz);

            //output
            DA.SetData(0, ptLoad);

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resources.IconPointLoad;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b36b4a4d-0ce9-43c3-868b-ad65630e7c16"); }
        }
    }
}
