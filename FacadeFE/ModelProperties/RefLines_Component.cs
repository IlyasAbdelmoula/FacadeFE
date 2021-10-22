using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic.Helper;

namespace FacadeFE
{
    public class GetRefLines_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GetRefLines_Component()
          : base("Element RefLines", "RefLines",
              "From the list of facade panels (breps), extract the common edges (naked and interior)",
              "FacadeFE", "1_Properties")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("FacadePanels", "Panels", "Facade panels as individual Breps", GH_ParamAccess.list); ;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Naked edges", "Naked edges", "Naked edges of the panels combined", GH_ParamAccess.list);
            pManager.AddCurveParameter("Interior edges", "Interior edges", "Interior edges of the panels combined", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            List<Brep> facadePanels = new List<Brep>();
            //retrieve data
            if (!DA.GetDataList(0, facadePanels)) return;

            //LOGIC
            if (facadePanels.Count != 0) //to avoid runtime error when facadePanel is empty
            {
                Brep[] joinedBreps = Brep.JoinBreps(facadePanels, DocumentTolerance());
                //check if all panels are connected
                if (joinedBreps.Length != 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Cannot extract edge: not all panels are connected.");
                    return; //add warnings later
                }
                Brep joinedPanels = joinedBreps[0];

                //sort
                HelperClass.SortEdges(joinedPanels, out List<Curve> nakedEdges, out List<Curve> interiorEdges, out List<Curve> nonManifoldEdges, out List<Curve> noneEdges);

                //output
                DA.SetDataList(0, nakedEdges);
                DA.SetDataList(1, interiorEdges);
            }
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
                return Properties.Resources.RefLines;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b2ff56a9-3ff5-4fec-9006-0541ad345392"); }
        }
    }
}