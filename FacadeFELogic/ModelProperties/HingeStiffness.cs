using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacadeFELogic
{
    public class HingeStiffness
    {
        //attributes
        public List<double> StiffnessVector;

        //constructor
        public HingeStiffness(double transX, double transY, double transZ, double rotX, double rotY, double rotZ)
        {
            StiffnessVector = new List<double>()
            {
                transX, transY, transZ, rotX, rotY, rotZ
            };

        }
    }
}
