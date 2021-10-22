using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacadeFELogic
{
    public class HingeFreedom
    {
        //attributes
        public List<int> FreedomVector;

        //Constructor
        public HingeFreedom(bool transX, bool transY, bool transZ, bool rotX, bool rotY, bool rotZ)
        {
            //temp list of integers
            List<int> freedom = new List<int>();

            //temp list of booleans
            List<bool> freedomBool = new List<bool>()
            {
                transX, transY, transZ, rotX, rotY, rotZ
            };

            //convert to int
            foreach (bool i in freedomBool)
            {
                int itemValue;
                if (i) itemValue = 1;
                else itemValue = 0;
                freedom.Add(itemValue);
            }

            //assign to attribute
            this.FreedomVector = freedom;
        }

        //methods
    }
}
