using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Props
{
    public class HingeStiffness_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public HingeStiffness_Component()
          : base("HingeStiffness", "HStifness",
              "Defines spring stifness values for a hinge",
              "FacadeFE", "1_Properties")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("TranX", "Tx", "Translation spring stiffness along X axis (value>0 => spring)", GH_ParamAccess.item);
            pManager.AddNumberParameter("TranY", "Ty", "Translation spring stiffness along Y axis (value>0 => spring)", GH_ParamAccess.item);
            pManager.AddNumberParameter("TranZ", "Tz", "Translation spring stiffness along Z axis (value>0 => spring)", GH_ParamAccess.item);
            pManager.AddNumberParameter("RotX", "Rx", "Rotation spring stiffness around X axis (value>0 => spring)", GH_ParamAccess.item);
            pManager.AddNumberParameter("RotY", "Ry", "Rotation spring stiffness around Y axis (value>0 => spring)", GH_ParamAccess.item);
            pManager.AddNumberParameter("RotZ", "Rz", "Rotation spring stiffness around Z axis (value>0 => spring)", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("HingeStiffness", "HStiffness", "Hinge stiffness", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            double tranX = 0;
            double tranY = 0;
            double tranZ = 0;
            double rotX = 0;
            double rotY = 0;
            double rotZ = 0;

            //retirieve input
            if (!DA.GetData(0, ref tranX)) return;
            if (!DA.GetData(1, ref tranY)) return;
            if (!DA.GetData(2, ref tranZ)) return;
            if (!DA.GetData(3, ref rotX)) return;
            if (!DA.GetData(4, ref rotY)) return;
            if (!DA.GetData(5, ref rotZ)) return;

            //LOGIC
            //initialize HingeStiffness
            HingeStiffness hingeStiffness = new HingeStiffness(tranX, tranY, tranZ, rotX, rotY, rotZ);

            //OUTPUT
            DA.SetData(0, hingeStiffness);
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
                return Properties.Resources.HingeStiffness;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ddcc2cb3-fbef-47ec-b2c1-91e8d187c3b1"); }
        }
    }
}