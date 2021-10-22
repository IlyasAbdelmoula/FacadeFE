using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;
using FacadeFELogic.Helper;

using FacadeFE.Helper;

namespace FacadeFE.Model
{
    public class FacadeSystem_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public FacadeSystem_Component()
          : base("FacadeSystem", "FacadeSystem",
              "Builds a facade system by combining model and load components.",
              "FacadeFE", "4_Simulate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("FacadePanels", "Panels", "Facade panels as individual Breps", GH_ParamAccess.list);
            pManager.AddPointParameter("Reference Point", "RefPoint", "Point acting as a reference point for Finite Element subdivision (to get anti-clockwise subdivision directions)", GH_ParamAccess.item);
            
            pManager.AddGenericParameter("Elements", "Elements", "Elements obtained from 'Element' component", GH_ParamAccess.list);

            pManager.AddGenericParameter("Supports", "Supports", "Supports obtained from 'Support' component", GH_ParamAccess.list);
            pManager.AddGenericParameter("Hinges", "Hinges", "Hinges obtained from 'Hinge' component (optional)", GH_ParamAccess.list);
            pManager.AddGenericParameter("PointLoads", "PtLoads", "Point loads ontained from 'PointLoad' component", GH_ParamAccess.list);
            //pManager.AddGenericParameter("LinearLoads", "LinLoads", "Linear loads obtained from 'LinearLoad' component", GH_ParamAccess.list);

            //set optional parameters
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true; //index will change
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FacadeSystem", "FacadeSystem", "facade system by combining model and load components", GH_ParamAccess.item);
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
            Point3d refPoint = new Point3d();
            List<Element> elements = new List<Element>();

            List<Support> supports = new List<Support>();
            List<Hinge> hinges = new List<Hinge>();
            List<PointLoad> pointLoads = new List<PointLoad>();

            //retrieve input data
            if (!DA.GetDataList(0, facadePanels)) return;
            if (!DA.GetData(1, ref refPoint)) return;
            if (!DA.GetDataList(2, elements)) return;
            DA.GetDataList(3, supports);
            DA.GetDataList(4, hinges);
            DA.GetDataList(5, pointLoads);



            //LOGIC
            //check that all panels are connected
            if (Brep.JoinBreps(facadePanels, DocumentTolerance()).Length != 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Not all panels are connected");
                return;
            }

            //initialise FacadeSystem
            FacadeSystem facadeSystem = new FacadeSystem(facadePanels, refPoint);


            //Finite Element processing
            facadeSystem.FEProcess(elements, supports, hinges, pointLoads, out List<string> stopWarningMessages);
            
            //stop if errors
            if(stopWarningMessages.Count != 0)
            {
                foreach (string message in stopWarningMessages)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                }
                return;
            }


            //OUTPUT
            DA.SetData(0, facadeSystem);
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
                return Properties.Resources.FacadeSystem;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get  { return new Guid("fde9098b-3f9c-48ee-841b-62428b1fb788"); }
        }
    }
}