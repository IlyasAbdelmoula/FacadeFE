using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Props
{
    public class MyComponent1 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MyComponent1()
          : base("Material", "material",
              "Creates a material.",
              "FacadeFE", "1_Properties")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Young's modulus (E)", "E", "Young's modulus", GH_ParamAccess.item);
            pManager.AddNumberParameter("Shear modulus (G)", "G", "Shear modulus", GH_ParamAccess.item);
            pManager.AddNumberParameter("Expansion coefficient (α)", "α", "Expansion coefficient", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "material", "Material with its properties", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            double e = 0;
            double g = 0;
            double expCoef = 0;

            //retrieve input
            if (!DA.GetData(0,ref e)) return;
            if (!DA.GetData(1, ref g)) return;
            if (!DA.GetData(2, ref expCoef)) return;

            //LOGIC
            //initialize material
            Material material = new Material(e, g, expCoef);

            //OUTPUT
            DA.SetData(0, material);
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
                return Properties.Resources.Material;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ba385abf-c719-4b91-af94-61575759799d"); }
        }
    }
}