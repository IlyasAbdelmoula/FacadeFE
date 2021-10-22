using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Model
{
    public class Element_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Element_Component()
          : base("Element", "Element",
              "Creates a profile element with line, material and section properties.",
              "FacadeFE", "2_Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("RefLines", "RefLine", "Reference line or lines", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "material", "Material properties", GH_ParamAccess.item);
            pManager.AddGenericParameter("Section", "Section", "Section properties", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "Elements", "Element objects from reflines", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            Line refLine = new Line();
            Material material = null;
            Section section = null;

            //retrieve inputs
            if(!DA.GetData(0, ref refLine)) return;
            if (!DA.GetData(1, ref material)) return;
            if (!DA.GetData(2, ref section)) return;

            //LOGIC
            Element element = new Element(refLine, material, section);

            //out
            DA.SetData(0, element);
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
                return Properties.Resources.Element;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("afd0ef71-8b1d-48f1-a9a6-f05996721afc"); }
        }
    }
}