using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Props
{
    public class HingeFreedom_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public HingeFreedom_Component()
          : base("HingeFreedom", "HFreedom",
              "Defines the Degree of freedom for a hinge.",
              "FacadeFE", "1_Properties")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("TranX", "Tx", "Translation freedom along X axis", GH_ParamAccess.item);
            pManager.AddBooleanParameter("TranY", "Ty", "Translation freedom along Y axis", GH_ParamAccess.item);
            pManager.AddBooleanParameter("TranZ", "Tz", "Translation freedom along Z axis", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RotX", "Rx", "Rotation freedom around X axis", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RotY", "Ry", "Rotation freedom around Y axis", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RotZ", "Rz", "Rotation freedom around Z axis", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("HingeFreedom", "HFreedom", "Hinge freedom", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            bool tranX = false;
            bool tranY = false;
            bool tranZ = false;
            bool rotX = false;
            bool rotY = false;
            bool rotZ = false;

            //retrieve input
            if (!DA.GetData(0, ref tranX)) return;
            if (!DA.GetData(1, ref tranY)) return;
            if (!DA.GetData(2, ref tranZ)) return;
            if (!DA.GetData(3, ref rotX)) return;
            if (!DA.GetData(4, ref rotY)) return;
            if (!DA.GetData(5, ref rotZ)) return;

            //LOGIC
            HingeFreedom hingeFreedom = new HingeFreedom(tranX, tranY, tranZ, rotX, rotY, rotZ);

            //OUTPUT
            DA.SetData(0, hingeFreedom);
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
                return Properties.Resources.HingeFreedom;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e19e1eae-ab02-46e7-93ff-3065a9567fbb"); }
        }
    }
}