using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Model
{
    public class DummyPtLoads_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DummyPtLoads_Component()
          : base("DummyPtLoads", "dummyotloads",
              "dummy component.",
              "FacadeFE", "2_Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
            pManager.AddGenericParameter("PointLoads", "ptloads", "pt loads", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            Node node = null;
            List<PointLoad> pointLoads = new List<PointLoad>();

            //retrieve inputs
            if (!DA.GetData("Node", ref node)) return;
            if (!DA.GetDataList("PointLoads", pointLoads)) return;

            //LOGIC
            //add loads
            bool addLoads = node.AddLoads(pointLoads, DocumentTolerance());
            //warning (in case)
            if (!addLoads)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Loads could not be added (reference positions may be different)");
            }

            //OUTPUT
            DA.SetData("Node", node);

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
            get { return new Guid("B2051B99-EE36-44AE-9758-D92836DADCD9"); }
        }
    }
}