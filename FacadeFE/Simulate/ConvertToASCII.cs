using System;
using System.Collections.Generic;


using Grasshopper.Kernel;
using Rhino.Geometry;

using FacadeFELogic;
using FacadeFELogic.Helper;

namespace FacadeFE.Simulate
{
    public class ConvertToASCII : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ConvertToASCII()
          : base("ConvertToASCII", "ConvertToASCII",
              "Get ASCII string list from FacadeSystem",
              "FacadeFE", "4_Simulate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FacadeSystem", "FacadeSystem", "Facade system to convert", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Export", "Export", "Export ASCII file", GH_ParamAccess.item);
            pManager.AddTextParameter("FolderPath", "FolderPath", "Folder path to export ASCII file to", GH_ParamAccess.item);
            pManager.AddTextParameter("FileName", "FileName", "Filename of the exported ASCII file (without extension)", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("all ASCII", "Nodes", "a combined list of all the components (with \"...\" as separator beween them)", GH_ParamAccess.list);
            pManager.AddTextParameter("Nodes ASCII", "NodeASCII", "Node list", GH_ParamAccess.list); //#for later: add better desciption by including the exact ASCII syntax
            pManager.AddTextParameter("Elements ASCII", "ElementFeASCII", "Element list", GH_ParamAccess.list);
            pManager.AddTextParameter("Boundary Translation ASCII", "BoundaryTranASCII", "Boundary conditions (translation)", GH_ParamAccess.list);
            pManager.AddTextParameter("Boundary Rotation ASCII", "BoundaryRotASCII", "Boundary conditions (rotation)", GH_ParamAccess.list);
            pManager.AddTextParameter("Hinges ASCII", "HingeASCII", "Hinges list", GH_ParamAccess.list);
            pManager.AddTextParameter("Loads ASCII", "LoadASCII", "Loads list (0 for point loads and 1 for linear loads)", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            //declare variables
            bool export = false;
            FacadeSystem facadeSystem = new FacadeSystem();
            string folderPath = null;
            string fileName = null;

            //retrieve data
            if (!DA.GetData(0, ref facadeSystem)) return;
            DA.GetData(1, ref export);
            DA.GetData(2, ref folderPath);
            DA.GetData(3, ref fileName);

            //LOGIC
            //get ASCII syntax
            List<string> combinedASCII = AsciiHelper.ConvertToASCII(facadeSystem,
                                                                    out List<string> nodeASCII,
                                                                    out List<string> elementFeASCII,
                                                                    out List<string> boundaryTranASCII,
                                                                    out List<string> boundaryRotASCII,
                                                                    out List<string> hingeASCII,
                                                                    out List<string> loadASCII);

            //exportASCII
            if(export && folderPath != null && fileName != null)
            {
                string filePath = AsciiHelper.MakeFilePath(folderPath, fileName, ".txt");
                AsciiHelper.exportASCII(combinedASCII, filePath);
                //Rhino.UI.Dialogs.ShowMessage("ASCII file sucessfully exported", "export ASCII");
            }

            else if (export)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Cannot export because of missing input (check folder path and filename).");
            }

            
            //OUTPUT
            DA.SetDataList(0, combinedASCII);
            DA.SetDataList(1, nodeASCII);
            DA.SetDataList(2, elementFeASCII);
            DA.SetDataList(3, boundaryTranASCII);
            DA.SetDataList(4, boundaryRotASCII);
            DA.SetDataList(5, hingeASCII);
            DA.SetDataList(6, loadASCII);
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
                return Properties.Resources.ConvertToASCII;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e86ed992-5896-42dd-b04e-c387373a88d8"); }
        }
    }
}