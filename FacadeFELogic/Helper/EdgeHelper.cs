using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace FacadeFELogic.Helper
{
    public class EdgeHelper
    {
        //ATTRIBUTES
        //base
        public Curve Edge;
        public int Id;

        //FE related
        //FEParams
        public List<FEParam> FEParams;
        //associated segment (needed because of subdivisions) //#bug: not working as intended (for now use PanelHelper.EdgeToSegIds & PanelHelper.GetSegmentAssociatedEdgeHelpers() instead)
        public int AssociatedSegmentId;

        //topology properties
        public EdgeAdjacency Valence;
        public bool IsNone;
        public bool IsNaked;
        public bool IsInterior;
        public bool IsNonManifold;

        //element related
        public Material Material;
        public Section Section;


        //linearload related (for later)


        //CONSTRUCTOR
        public EdgeHelper() //in case of getting edge Ids directly from Brep
        {
            this.FEParams = new List<FEParam>();
            this.IsNone = false;
            this.IsNaked = false;
            this.IsInterior = false;
            this.IsNonManifold = false;
        }

        public EdgeHelper(BrepEdge edge) //not working
        {
            this.Edge = edge.EdgeCurve;
            this.Id = edge.EdgeIndex;

            this.FEParams = new List<FEParam>();

            //set topology attributes according to edge valence
            this.Valence = edge.Valence;
            this.UpdateValenceStatus(edge);
        }

        public EdgeHelper(Curve edge, List<Curve> facadeOrderedEdges, BrepEdgeList facadeBrepEdges)
        {
            this.Edge = edge;

            this.Id = facadeOrderedEdges.IndexOf(this.Edge);

            this.FEParams = new List<FEParam>();

            //set topology attributes according to edge valence
            //find corresponding brepEdge
            BrepEdge brepEdge = facadeBrepEdges.ToList().Find(e => e.EdgeCurve == edge);
            //update its valence
            this.Valence = brepEdge.Valence;
            this.UpdateValenceStatus(brepEdge);

            //add FEParams for the end points (add them without WindFELoad of 0 (WindFELoad will be -1 meaning unexistant))
            for (int paramIndex = 0; paramIndex < 2; paramIndex++)
            {
                //end point param
                double param = edge.Domain[paramIndex];

                //make a FEParam out of endpoint Id
                FEParam feParam = new FEParam(param, isCorner: true);
                feParam.IsPanelCorner = true;

                //add it to FEParams
                this.FEParams.Add(feParam);
            }
        }

        //methods
        private void UpdateValenceStatus(BrepEdge edge)
        {
            //set topology attributes according to edge valence
            //valence
            int valence = (int)this.Valence;
            //set topology attributes
            if (valence == 0)
            {
                this.IsNone = true;
            }
            else if (valence == 1)
            {
                this.IsNaked = true;
            }
            else if (valence == 2)
            {
                this.IsInterior = true;
            }
            else if (valence > 2)
            {
                this.IsNonManifold = true;
            }
        }

        public bool Subdivide() //needed to divide it using FEParams
        {
            return true;
        }

        public void AddFEParam(double param, bool isCorner)
        {
            FEParam feParam = new FEParam(param, isCorner);
            this.FEParams.Add(feParam);
        }

        public void AddFEParamWithLoad(double param, bool isCorner, double windFELoad)
        {
            FEParam feParam = new FEParam(param, isCorner, windFELoad);
            this.FEParams.Add(feParam);
        }

        public void AddFEParams(List<FEParam> feParams)
        {
            this.FEParams.AddRange(feParams);
        }

        public void OrderFEParams()
        {
            //List<FEParam> SortedList = objListOrder.OrderByDescending(o => o.OrderDate).ToList()
            List<FEParam> sortedParams = this.FEParams.OrderBy(o => o.Param).ToList();
            this.FEParams = sortedParams;
        }

        public void AddElementProperties(Element element)
        {
            this.Material = element.Material;
            this.Section = element.Section;
        }
    }
}
