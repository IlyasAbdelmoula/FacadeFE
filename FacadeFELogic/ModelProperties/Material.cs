using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacadeFELogic
{
    public class Material
    {
        //attributes
        public double E; //Young's modulus
        public double G; //Shear modulus
        public double ExpansionCoef; //expansion coefficient

        //Constructors
        public Material(double e, double g, double expansionCoef)
        {
            this.E = e;
            this.G = g;
            this.ExpansionCoef = expansionCoef;
        }
    }
}