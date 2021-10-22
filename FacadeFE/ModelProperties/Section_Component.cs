using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Props
{
    public class Section_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Section_Component()
          : base("Section", "section",
              "Element rectangular section with its geometric properties",
              "FacadeFE", "1_Properties")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("width (Hy)", "hy", "Section width", GH_ParamAccess.item);
            pManager.AddNumberParameter("height (Hz)", "hz", "Section height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Alpha", "alpha", "Alpha", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Section", "section", "Section", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            double hy = 0;
            double hz = 0;
            double alpha = 0;

            //retrieve input
            if (!DA.GetData(0, ref hy)) return;
            if (!DA.GetData(1, ref hz)) return;
            if (!DA.GetData(2, ref alpha)) return;

            //LOGIC
            Section section = new Section(hy, hz, alpha);

            //OUTPUT
            DA.SetData(0, section);
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
                return Properties.Resources.Section;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5eeb1e99-2ba1-487e-a8d1-cd81e40bb87b"); }
        }
    }
}