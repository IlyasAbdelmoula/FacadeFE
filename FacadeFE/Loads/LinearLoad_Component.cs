using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Model
{
    public class LinearLoad_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public LinearLoad_Component()
          : base("LinearLoad", "linload",
              "Creates point load with a reference line/lines and force values associated to axes.",
              "FacadeFE", "3_Loads")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Reference Line", "refline", "Reference line/lines for the load", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fy1", "fy1", "Force on Y axis for the 1st node", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fy2", "fy2", "Force on Y axis for the 2nd node", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fz1", "fz1", "Force on Z axis for the 1st node", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fz2", "fz2", "Force on Z axis for the 1st node", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("LinearLoad", "linload", "Linear load", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //initialize variables
            //input
            Line refLine = new Line();
            double fy1 = 0;
            double fy2 = 0;
            double fz1 = 0;
            double fz2 = 0;

            //retrieve inputs
            if(!DA.GetData(0, ref refLine)) return;
            if (!DA.GetData(1, ref fy1)) return;
            if (!DA.GetData(2, ref fy2)) return;
            if (!DA.GetData(3, ref fz1)) return;
            if (!DA.GetData(4, ref fz2)) return;

            //initialize load
            LinearLoad linearLoad = new LinearLoad(refLine, fy1, fy2, fz1, fz2);

            //output
            DA.SetData(0, linearLoad);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.LinearLoad;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2a86aa51-1e15-43cc-a917-14423eb02bc4"); }
        }
    }
}