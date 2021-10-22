using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace FacadeFELogic.Helper
{
    public class ElementFE
    {
        //attributes
        public int StartNodeId;
        public int EndNodeId;
        public Curve RefSegment;
        public Material Material;
        public Section Section;
        public LinearLoad Load;

        //constructor
        public ElementFE(int startNodeId, int endNodeId, Curve refSegment, Material material, Section section)
        {
            this.RefSegment = refSegment;
            this.StartNodeId = startNodeId;
            this.EndNodeId = endNodeId;
            this.Material = material;
            this.Section = section;
        }


        /*
        public ElementFE(EdgeHelper edgeHelper, List<Node> nodeList, double tolerance)
        {
            new EdgeHelper()
            //retrieve node Ids
            edgeHelper.GetNodesIndex(nodeList, tolerance, out int startId, out int endId);

            //update attributes
            this.StartNodeId = startId;
            this.EndNodeId = endId;
            this.Material = edgeHelper.Material;

        }
        */
    }
}
