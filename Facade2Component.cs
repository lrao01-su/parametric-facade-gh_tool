using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Facade_tool
{
    public class Facade2Component : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Facade2Component()
          : base("Facade tool", "Facade2",
              "When brilliantly applied, makes a brilliant elevation for rendering scene",
              "Elevation", "QuickElevation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Facade", "Srf", "Brep Srf facade window", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Flip", "False start", "Flip surface if dir of surface is inversed", GH_ParamAccess.item);
            pManager.AddNumberParameter("Row", "double", "Equal distance for dividing the facade into rows", GH_ParamAccess.item);
            pManager.AddCurveParameter("Crv_V", "Vertical", "mullion section curve", GH_ParamAccess.item);
            pManager.AddCurveParameter("Crv_H", "Horizontal", "Transom Section Curve", GH_ParamAccess.item);
            pManager.AddNumberParameter("Floor_H", "double list", "Give floor height at level for division", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Bake", "False starat bake button", "Bake with objects in different layers", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("VMull", "V_Mu", "Vertical mullions baked into list of mesh", GH_ParamAccess.list);
            pManager.AddMeshParameter("HTran", "H_Tr", "Horizontal Transom baked into list of mesh", GH_ParamAccess.list);
            pManager.AddMeshParameter("Window", "G_Wi", "Window glass divided and baked into list of mesh", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep Facade = null;
            Boolean Flip = false;
            double Row = 0;
            Curve Crv_V = null;
            Curve Crv_H = null;
            List<Double> Floor_H = new List<double>();
            Boolean Bake = false;



            if (!DA.GetData(0, ref Facade)) return;
            if (!DA.GetData(1, ref Flip)) return;
            if (!DA.GetData(2, ref Row)) return;
            if (!DA.GetData(3, ref Crv_V)) return;
            if (!DA.GetData(4, ref Crv_H)) return;
            if (!DA.GetDataList(5, Floor_H)) return;
            if (!DA.GetData(6, ref Bake)) return;

            List<Mesh> output_V = new List<Mesh>();
            List<Mesh> output_H = new List<Mesh>();
            List<Mesh> output_W = new List<Mesh>();

            Curve[] edge_crv = Method1.List_curves(Facade, Flip);
            List<Line> bay_row = Method1.Bay_division(edge_crv, Row);
            Brep Split_window = Method1.Splited_W(Facade, bay_row);
            List<Line> height_row = Method1.Height_division(edge_crv, Floor_H);
            Brep[] Split_height = Method1.Splited_H(Split_window, height_row, bay_row);
            

            
            for (int i = 0; i < Split_height.Length; i++)
            {
                
                Mesh Vertical = Method1.Mesh_v(Crv_V, Split_height[i]);
                Mesh Orient_V = Method1.Orientation(Split_height[i], Crv_V, Vertical);
                output_V.Add(Orient_V);
                //G = output_V;

            }
            for (int i = 0; i < Split_height.Length; i++)
            {
                Mesh Horizontal = Method1.Mesh_h(Crv_H, Split_height[i]);
                Mesh Orient_H = Method1.Orientation2(Split_height[i], Crv_H, Horizontal);
                output_H.Add(Orient_H);
                //E = output_H;
            }

            for (int i = 0; i < Split_height.Length; i++)
            {
                Mesh Window = Method1.Brep2mesh(Split_height[i]);
                output_W.Add(Window);
            }

            for (int i = 0; i < Split_height.Length; i++)
            {
                Mesh Orient_Ver = output_V[i];
                Mesh Orient_Hori = output_H[i];
                Mesh Window_output = output_W[i];
                bool Bake2layer = Method1.Bake_now(Orient_Ver, Orient_Hori, Window_output, Bake);
                //G = Bake2layer;
            }

            DA.SetDataList(0, output_V);
            DA.SetDataList(1, output_H);
            DA.SetDataList(2, output_W);





        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Facade_tool.Properties.Resources.appleb;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2930CDE1-FD8B-4F8C-A4BE-8D24610569BF"); }
        }
        
    }
    public class FacadetoolCategoryIcon : Grasshopper.Kernel.GH_AssemblyPriority
    {
        public override Grasshopper.Kernel.GH_LoadingInstruction PriorityLoad()
        {
            Grasshopper.Instances.ComponentServer.AddCategoryIcon("Elev", Facade_tool.Properties.Resources.appleb);
            Grasshopper.Instances.ComponentServer.AddCategorySymbolName("Elevation", 'E');
            return Grasshopper.Kernel.GH_LoadingInstruction.Proceed;
        }
    }
}
