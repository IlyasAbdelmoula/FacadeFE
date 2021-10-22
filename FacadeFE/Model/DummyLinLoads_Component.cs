using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Model
{
    public class Dummy2_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Dummy2_Component()
          : base("DummyLinLoads", "dummylinloads",
              "dummy component.",
              "FacadeFE", "2_Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Element", "element", "Element", GH_ParamAccess.item);
            pManager.AddGenericParameter("LinearLoads", "linloads", "Linear loads", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Element", "element", "Element", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            Element element = null;
            List<LinearLoad> linearLoads = new List<LinearLoad>();

            //retrieve inputs
            if (!DA.GetData("Element", ref element)) return;
            if (!DA.GetDataList("LinearLoads", linearLoads)) return;

            //LOGIC
            //add loads
            bool addLoads = element.AddLoads(linearLoads);
            //warning (in case)
            if (!addLoads)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Loads could not be added (reference line may be different)");
            }

            //OUTPUT
            DA.SetData("Element", element);

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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("98bd80fe-9800-443a-9059-b1b9a0a6135f"); }
        }
    }
}