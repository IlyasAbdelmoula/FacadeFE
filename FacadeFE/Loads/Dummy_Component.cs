using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;

namespace FacadeFE.Model
{
    public class Dummy_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Dummy_Component()
          : base("Dummy", "dummy",
              "dummy component",
              "FacadeFE", "3_Loads")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddGenericParameter("PointLoadsCons", "ptloadscst", "point loads", GH_ParamAccess.list);
            pManager.AddGenericParameter("PointLoads", "ptloads", "point loads", GH_ParamAccess.list);
            pManager.AddGenericParameter("LinearLoads", "linloads", "linear loads", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddGenericParameter("CombinedLoad", "combinedload", "combined load", GH_ParamAccess.item);
            pManager.AddGenericParameter("CombinedPointLoad", "combinedpointload", "combined point load", GH_ParamAccess.item);
            pManager.AddGenericParameter("CombinedLinearLoad", "combinedlinearload", "combined linear load", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //declare variables
            //input
            List<PointLoad> pointLoads = new List<PointLoad>();
            List<PointLoad> pointLoadsCombine = new List<PointLoad>();
            List<LinearLoad> linearLoadCombine = new List<LinearLoad>();
            //output
            PointLoad combinedLoad = null;
            PointLoad combinedPointLoad = null;
            LinearLoad combinedLinearLoad = null;

            //get inputs
            //DA.GetDataList(0, pointLoads);
            DA.GetDataList("PointLoads", pointLoadsCombine);
            DA.GetDataList("LinearLoads", linearLoadCombine);

            //combine with constructor
            if (pointLoads.Count != 0) combinedLoad = new PointLoad(pointLoads, DocumentTolerance());

            //combine with method
            if (pointLoadsCombine.Count != 0)
            {
                bool combine = PointLoad.CombinePointLoads(pointLoadsCombine, DocumentTolerance(), out combinedPointLoad) ;
            }

            if (linearLoadCombine.Count != 0)
            {
                bool combine = LinearLoad.CombineLinearLoads(linearLoadCombine, out combinedLinearLoad);
            }

            //output
            //DA.SetData(0, combinedLoad);
            DA.SetData("CombinedPointLoad", combinedPointLoad);
            DA.SetData("CombinedLinearLoad", combinedLinearLoad);
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
            get { return new Guid("87b04207-471d-43c6-a871-638dd75f3101"); }
        }
    }
}