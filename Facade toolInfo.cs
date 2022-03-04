using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Facade_tool
{
   
    public class Tool1 : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Facadetool";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Facade_tool.Properties.Resources.appleb;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("2930CDE1-FD8B-4F8C-A4BE-8D24610569BF");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }

        


    }

}


