using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;
using FacadeFELogic.Helper;

using FacadeFE.Helper;

namespace FacadeFE.Simulate
{
    public class ExtractFE_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ExtractFE_Component()
          : base("ExtractFE", "ExtractFE",
              "Extract FacadeSystem components (mainly related the Finite Element subidivision)",
              "FacadeFE", "4_Simulate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FacadeSystem", "FacadeSystem", "Facade system to extract components from", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "Panels", "ordered breps correspodnig to the order of the Facasystem panels", GH_ParamAccess.list);
            //pManager.AddGenericParameter("PanelEdges", "PanelsFromList", "Just a test", GH_ParamAccess.list);
            //pManager.AddGenericParameter("PanelSegments", "EdgeHelpers", "Just a test", GH_ParamAccess.list);
            //pManager.AddIntegerParameter("EdgeToSegIds", "EdgeHelpers", "Just a test", GH_ParamAccess.tree);

            pManager.AddGenericParameter("OrderedEdges", "OrderedEdges", "Ordered edges corresponding to ordered^panels  of the facade system", GH_ParamAccess.list);

            pManager.AddCurveParameter("ElementFeEdges", "ElementFeEdges", "Ordered edges corresponding to finite elements of the facade system", GH_ParamAccess.list);
            pManager.AddPointParameter("NodePoints", "NodePoints", "Ordered points corresponding to nodes of the facade system", GH_ParamAccess.list);
            pManager.AddPointParameter("FEProjectionPoints", "FEProjPts", "Projection points inside panels", GH_ParamAccess.list);

            pManager.AddLineParameter("FEIntersectionLines", "FEIntersectionLine", "FE Intersection lines from panel corners.", GH_ParamAccess.list);
            pManager.AddLineParameter("FEWindLines", "FEWindLines", "FE Wind load lines", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            FacadeSystem facadeSystem = new FacadeSystem();
            //retrieve data
            if(!DA.GetData(0, ref facadeSystem)) return;

            //LOGIC
            //elementFelist
            List<Curve> elementFeList = facadeSystem.ElementsFE.Select(e => e.RefSegment).ToList();
            //node list
            List<Point3d> nodeList = facadeSystem.Nodes.Select(n => n.Position).ToList();
            //FE Projection points
            List<Point3d> feProjectionPoints = new List<Point3d>();
            foreach (PanelHelper panelHelper in facadeSystem.PanelHelpers)
            {
                feProjectionPoints.AddRange(panelHelper.FEProjectionPoints);
            }

            //FEIntersection lines
            List<Line> feIntersectionLines = new List<Line>();
            foreach (PanelHelper panelHelper in facadeSystem.PanelHelpers)
            {
                feIntersectionLines.AddRange(panelHelper.FEIntersectionLines);
            }

            //FEWindLines lines
            List<Line> feWindLines = new List<Line>();
            foreach (PanelHelper panelHelper in facadeSystem.PanelHelpers)
            {
                feWindLines.AddRange(panelHelper.FEWindLines);
            }

            //OUTPUT
            DA.SetDataList("Panels", facadeSystem.PanelHelpers.Select(p => p.Panel).ToList());

            //DA.SetDataList("PanelEdges", facadeSystem.PanelHelpers[0].PanelEdgeHelpers.Select(e => e.Edge).ToList());
            //DA.SetDataList("PanelSegments", facadeSystem.PanelHelpers[0].ContourSegments);
            //DA.SetDataTree(4, HelperClassGH.ListOfListsToTree(facadeSystem.PanelHelpers.Select(p => p.EdgeToSegIds).ToList()));

            DA.SetDataList("OrderedEdges", facadeSystem.OrderedEdges);

            DA.SetDataList("ElementFeEdges", elementFeList);
            DA.SetDataList("NodePoints", nodeList);
            DA.SetDataList("FEProjectionPoints", feProjectionPoints);

            DA.SetDataList("FEIntersectionLines", feIntersectionLines);
            DA.SetDataList("FEWindLines", feWindLines);

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
                return Properties.Resources.ExportFE;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("758549ef-3e95-49af-a313-3dfbebf70566"); }
        }
    }
}