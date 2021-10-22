using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Model
{
    public class Support_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Support_Component()
          : base("Support", "Support",
              "Creates a support with its boundary conditions",
              "FacadeFE", "2_Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Positions", "Pos", "Position reference point or points", GH_ParamAccess.item);
            pManager.AddBooleanParameter("TranX", "Tx", "Translation boundary condition along X axis", GH_ParamAccess.item);
            pManager.AddBooleanParameter("TranY", "Ty", "Translation boundary condition along Y axis", GH_ParamAccess.item);
            pManager.AddBooleanParameter("TranZ", "Tz", "Translation boundary condition along Z axis", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RotX", "Rx", "Rotation boundary condition around X axis", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RotY", "Ry", "Rotation boundary condition around Y axis", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RotZ", "Rz", "Rotation boundary condition around Z axis", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Supports", "Support", "Support object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            Point3d position = new Point3d();
            bool tranX = false;
            bool tranY = false;
            bool tranZ = false;
            bool rotX = false;
            bool rotY = false;
            bool rotZ = false;

            //retrieve input
            if (!DA.GetData(0, ref position)) return;
            if (!DA.GetData(1, ref tranX)) return;
            if (!DA.GetData(2, ref tranY)) return;
            if (!DA.GetData(3, ref tranZ)) return;
            if (!DA.GetData(4, ref rotX)) return;
            if (!DA.GetData(5, ref rotY)) return;
            if (!DA.GetData(6, ref rotZ)) return;

            //LOGIC
            //initialize support
            Support support = new Support(position, tranX, tranY, tranZ, rotX, rotY, rotZ);

            //check if all DOF are open
            if (support.GetTranIndicator() == 0 && support.GetRotIndicator() == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Support created but with no translation/rotation restrictions");
            }

            int tranValue = support.GetTranIndicator();
            int rotValue = support.GetRotIndicator();


            //OUTPUT
            DA.SetData(0, support);

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
                return Properties.Resources.Support;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5e3df535-35c3-47c2-943a-742d0fff2dc2"); }
        }
    }
}