﻿using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace FacadeFE
{
    public class FacadeFEInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "FacadeFE";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "FacadeFE works as a grasshopper interface for FiniteElement Analysis of facades, mainly to communicate with an external propriatary FE software for TH-OWL";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("b86e2adc-9ebc-4806-8c1c-cec502d59e5a");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Ilyas Abdelmoula, TH-OWL";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "ilyas.abdelmoula@stud.th-owl.de";
            }
        }
    }
}
