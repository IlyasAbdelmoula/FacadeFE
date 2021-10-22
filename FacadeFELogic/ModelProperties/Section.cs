using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacadeFELogic
{
    public class Section
    {
        //Attributes
        public double A; //area
        public double Ay; //reduced area, shear  rigidity factor (calculated)
        public double Az; //reduced area, shear  rigidity factor (calculated)
        public double Iy; //inertia moment across Y (bh3/12, usually in beams the bigger value)
        public double Iz; //inertia moment across Z (hb3/12, usually in beams the smaller value)
        public double J; //polar inertia moment (about Z axis, J = Iy + Iz)
        public double Hy; //width
        public double Hz; //height
        public double Alpha; //usually 1, #Q: calculated or user-defined?

        //constructors
        public Section(double hy, double hz, double alpha)
        {
            this.A = hy * hz;
            this.Ay = this.A * 0.8; //Just a dummy calculation
            this.Az = this.A * 0.2; //Just a dummy calculation
            this.Iy = hy * Math.Pow(hz, 3) / 12; // bh3/12
            this.Iz = hz * Math.Pow(hy, 3) / 12; // hb3/12
            this.J = this.Iy + this.Iz;
            this.Hy = hy; //width
            this.Hz = hz; //height
            this.Alpha = alpha; //#Q: calculated or user-defined?
        }
    }
}
