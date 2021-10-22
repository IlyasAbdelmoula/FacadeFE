using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Model
{
    public class Hinge_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Hinge_Component()
          : base("Hinge", "Hinge",
              "Creates a hinge with degree of freedom and spring stiffness indicators.",
              "FacadeFE", "2_Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Positions", "Pos", "Position point or points", GH_ParamAccess.item);
            pManager.AddGenericParameter("HingeFreedom", "Freedom", "HingeFreedom object specifying the associated degrees of freedom", GH_ParamAccess.item);
            pManager.AddGenericParameter("HingeStiffness", "Stiffness", "HingeStiffness object specifying the associated spring stiffness", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Hinges", "Hinge", "Hinge object", GH_ParamAccess.item);
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
            HingeFreedom hingeFreedom = null;
            HingeStiffness hingeStiffness = null;

            //retrieve input
            if (!DA.GetData(0, ref position)) return;
            if (!DA.GetData(1, ref hingeFreedom)) return;
            if (!DA.GetData(2, ref hingeStiffness)) return;

            //LOGIC
            //initialize hinge
            Hinge hinge = new Hinge(position, hingeFreedom, hingeStiffness);

            //OUTPUT
            DA.SetData(0, hinge);
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
                return Properties.Resources.Hinge;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ba87fa39-2837-4f36-b0e7-e6142bd07f69"); }
        }
    }
}