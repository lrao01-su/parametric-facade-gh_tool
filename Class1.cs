using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using System.Drawing;
using Rhino.Geometry;

namespace Facade_tool
{
    public static class Method1
    {
        public static Curve[] List_curves(Brep Facade, bool Flip)
        {
            Curve[] curves = Facade.DuplicateEdgeCurves();
            if (Flip)
            {
                Array.Reverse(curves);
            }
            return curves;

        }
        public static List<Line> Bay_division(Curve[] edge_crv, double bay)
        {
            Curve h_bay = edge_crv[0];
            Curve h_bay1 = edge_crv[2];

            int count = h_bay.DivideEquidistant(bay).Length;

            //Sorted Points from side 1
            Point3d[] bay_points = null;
            h_bay.DivideByCount(count, true, out bay_points);
            Point3d temp;
            for (int j = 0; j <= bay_points.Length - 2; j++)
            {
                for (int i = 0; i <= bay_points.Length - 2; i++)
                {
                    if (bay_points[i].X > bay_points[i + 1].X)
                    {
                        temp = bay_points[i + 1];
                        bay_points[i + 1] = bay_points[i];
                        bay_points[i] = temp;
                    }
                }
            }

            //Sorted Points from side 2
            Point3d[] bay_points1 = null;
            h_bay1.DivideByCount(count, true, out bay_points1);

            Point3d temp1;
            for (int j = 0; j <= bay_points1.Length - 2; j++)
            {
                for (int i = 0; i <= bay_points1.Length - 2; i++)
                {
                    if (bay_points1[i].X > bay_points1[i + 1].X)
                    {
                        temp1 = bay_points1[i + 1];
                        bay_points1[i + 1] = bay_points1[i];
                        bay_points1[i] = temp1;
                    }
                }
            }

            //get list of lines from connecting points from 2 sides
            List<Line> Output = new List<Line>();
            for (int i = 0; i < bay_points.Length; i++)
            {
                Point3d pt1 = bay_points[i];
                Point3d pt2 = bay_points1[i];
                Line temp_L = new Line(pt1, pt2);
                Output.Add(temp_L);
            }
            return Output;

        }
        public static Brep Splited_W(Brep Facade, List<Line> bay_row)
        {
            List<LineCurve> Line2Crv = new List<LineCurve>();
            foreach (Line L in bay_row)
            {
                LineCurve temp = new LineCurve(L);
                Line2Crv.Add(temp);
            }
            BrepFace bf = Facade.Faces[0];
            Brep split = bf.Split(Line2Crv, 1);
            return split;
        }

        public static List<Line> Height_division(Curve[] edge_crv, List<double> Floor_H)
        {
            Curve v_bay = edge_crv[1];
            Curve v_bay1 = edge_crv[3];
            v_bay1.Reverse();

            List<Point3d> height_points = new List<Point3d>();

            Point3d temp_p;
            for (int i = 0; i < Floor_H.Count; i++)
            {
                temp_p = v_bay.PointAtLength(Floor_H[i]);
                height_points.Add(temp_p);
            }
            Point3d temp_pt = new Point3d();
            for (int j = 0; j <= height_points.Count - 2; j++)
            {
                for (int i = 0; i <= height_points.Count - 2; i++)
                {
                    if (height_points[i].Z > height_points[i + 1].Z)
                    {
                        temp_pt = height_points[i + 1];
                        height_points[i + 1] = height_points[i];
                        height_points[i] = temp_pt;
                    }
                }
            }

            List<Point3d> height_points1 = new List<Point3d>();
            Point3d temp_p1;
            for (int i = 0; i < Floor_H.Count; i++)
            {
                temp_p1 = v_bay1.PointAtLength(Floor_H[i]);
                height_points1.Add(temp_p1);
            }

            Point3d temp_pt1 = new Point3d();
            for (int j = 0; j <= height_points1.Count - 2; j++)
            {
                for (int i = 0; i <= height_points1.Count - 2; i++)
                {
                    if (height_points1[i].Z > height_points1[i + 1].Z)
                    {
                        temp_pt1 = height_points1[i + 1];
                        height_points1[i + 1] = height_points1[i];
                        height_points1[i] = temp_pt1;
                    }
                }
            }

            List<Line> Output = new List<Line>();
            for (int i = 0; i < height_points.Count; i++)
            {
                Point3d pt1 = height_points[i];
                Point3d pt2 = height_points1[i];
                Line temp_L = new Line(pt2, pt1);
                Output.Add(temp_L);
            }
            return Output;

        }
        public static Brep[] Splited_H(Brep split_window, List<Line> height_row, List<Line> bay_row)
        {
            List<LineCurve> Line2Crv = new List<LineCurve>();
            foreach (Line L in bay_row)
            {
                LineCurve temp = new LineCurve(L);
                Line2Crv.Add(temp);
            }
            List<LineCurve> Line2Crv1 = new List<LineCurve>();
            foreach (Line L in height_row)
            {
                LineCurve temp = new LineCurve(L);
                Line2Crv1.Add(temp);
            }
            Line2Crv.AddRange(Line2Crv1);
            Brep[] Sp_window = split_window.Split(Line2Crv1, 1);
            return Sp_window;

        }

        // PART 2 generate window mullion and transom, appled for looping
        public static Mesh Mesh_v(Curve crv, Brep Srf)
        {
            PolyCurve polyCrv = crv as PolyCurve;
            Curve[] segs = crv.DuplicateSegments();
            List<Line> L = new List<Line>();
            foreach (Curve seg in segs)
            {
                if (seg.IsLinear())
                {
                    Line temp = new Line(seg.PointAtStart, seg.PointAtEnd);
                    L.Add(temp);
                }
                else
                {
                    int count = 10;

                    Rhino.Geometry.Point3d[] points = null;

                    seg.DivideByCount(count, true, out points);

                    for (int i = 0; i < points.Length - 1; i++)
                    {
                        Line temp = new Line(points[i], points[i + 1]);
                        L.Add(temp);
                    }
                }

            }
            //segments to lines, if not linear convert to linear divide by 10.

            Point3d[] corners = Srf.DuplicateVertices();
            Point3d temp_p;
            for (int j = 0; j <= corners.Length - 2; j++)
            {
                for (int i = 0; i <= corners.Length - 2; i++)
                {
                    if (corners[i].X > corners[i + 1].X)
                    {
                        temp_p = corners[i + 1];
                        corners[i + 1] = corners[i];
                        corners[i] = temp_p;
                    }
                }
            }

            double H = corners[1].DistanceTo(corners[0]);
            //double H = height;
            List<Line> Moved = new List<Line>();
            foreach (Line line in L)
            {
                Line temp = new Line(line.From, line.To);
                Vector3d V = new Vector3d(0, 0, H);
                temp.Transform(Transform.Translation(V));
                Moved.Add(temp);
            }
            // duplicate and move line base on height
            Mesh output = new Mesh();
            for (int i = 0; i < Moved.Count; i++)
            {
                Line line1 = L[i];
                Line line2 = Moved[i];
                Mesh temp = new Mesh();
                temp.Vertices.Add(line1.From);
                temp.Vertices.Add(line1.To);
                temp.Vertices.Add(line2.To);
                temp.Vertices.Add(line2.From);
                temp.Faces.AddFace(0, 1, 2, 3);
                output.Append(temp);
            }
            return output;
        }
        public static Mesh Mesh_h(Curve crv, Brep Srf)
        {
            PolyCurve polyCrv = crv as PolyCurve;
            Curve[] segs = crv.DuplicateSegments();
            //1.curve to segments
            List<Line> L = new List<Line>();
            foreach (Curve seg in segs)
            {
                if (seg.IsLinear())
                {
                    Line temp = new Line(seg.PointAtStart, seg.PointAtEnd);
                    L.Add(temp);
                }
                else
                {
                    int count = 10;

                    Rhino.Geometry.Point3d[] points = null;

                    seg.DivideByCount(count, true, out points);

                    for (int i = 0; i < points.Length - 1; i++)
                    {
                        Line temp = new Line(points[i], points[i + 1]);
                        L.Add(temp);
                    }
                }
            }
            //segments to lines, if not linear convert to linear divide by 10.
            Point3d[] corners = Srf.DuplicateVertices();
            Point3d temp_p;


            for (int j = 0; j <= corners.Length - 2; j++)
            {
                for (int i = 0; i <= corners.Length - 2; i++)
                {
                    if (corners[i].Z > corners[i + 1].Z)//Sort window corner points base on X value
                    {
                        temp_p = corners[i + 1];
                        corners[i + 1] = corners[i];
                        corners[i] = temp_p;
                    }
                }
            }

            double H = corners[0].DistanceTo(corners[1]);
            //double H = corners[3].X - corners[0].X;// get extrusion distnace by x value differences between 2 corners

            List<Line> Moved = new List<Line>();
            foreach (Line line in L)
            {
                Line temp = new Line(line.From, line.To);
                Vector3d V = new Vector3d(0, 0, H);
                temp.Transform(Transform.Translation(V));
                Moved.Add(temp);
            }
            // duplicate and move line base on vector

            Mesh output = new Mesh();

            for (int i = 0; i < Moved.Count; i++)
            {
                Line line1 = L[i];
                Line line2 = Moved[i];

                Mesh temp = new Mesh();
                temp.Vertices.Add(line1.From);
                temp.Vertices.Add(line1.To);
                temp.Vertices.Add(line2.To);
                temp.Vertices.Add(line2.From);
                temp.Faces.AddFace(0, 1, 2, 3);
                output.Append(temp);
            }
            return output;
        }
        public static Mesh Orientation(Brep Srf, Curve Crv, Mesh Vertical)
        {
            Plane source = new Plane(Plane.WorldYZ);
            Point3d ref_pt = new Point3d(0, 0, 0);
            Point3d[] pts = Srf.DuplicateVertices();
            Point3d temp;
            for (int j = 0; j <= pts.Length - 2; j++)
            {
                for (int i = 0; i <= pts.Length - 2; i++)
                {
                    if (pts[i].Z > pts[i + 1].Z)
                    {
                        temp = pts[i + 1];
                        pts[i + 1] = pts[i];
                        pts[i] = temp;
                    }
                }
            }

            Srf.Flip();
            Surface srf1 = Srf.Faces[0];
            double u, v;
            srf1.ClosestPoint(pts[0], out u, out v);
            Vector3d n = srf1.NormalAt(u, v);
            Plane target_1 = new Plane(pts[0], n, Vector3d.ZAxis);
            Plane target_2 = new Plane(pts[1], n, Vector3d.ZAxis);
            Transform orient = Transform.PlaneToPlane(source, target_1);
            Transform orient_2 = Transform.PlaneToPlane(source, target_2);
            Mesh Vertical_2 = new Mesh();
            Vertical_2.CopyFrom(Vertical);
            Vertical_2.Transform(orient_2);
            Vertical_2.FillHoles();
            Vertical.Transform(orient);
            Vertical.FillHoles();//cap vertical mullion
            Vertical.Append(Vertical_2);
            return Vertical;
        }
        public static Mesh Orientation2(Brep Srf, Curve Crv, Mesh Horizontal)
        {
            //Plane source = new Plane(Point3d.Origin, Vector3d.YAxis, Vector3d.ZAxis);
            Plane source = new Plane(Plane.WorldYZ);
            Point3d ref_pt = new Point3d(0, 0, 0);
            Point3d[] pts = Srf.DuplicateVertices();
            // Points pts = Points[];
            Point3d temp;
            for (int j = 0; j <= pts.Length - 2; j++)
            {
                for (int i = 0; i <= pts.Length - 2; i++)
                {
                    if (pts[i].X > pts[i + 1].X)
                    {
                        temp = pts[i + 1];
                        pts[i + 1] = pts[i];
                        pts[i] = temp;
                    }
                }
            }

            Surface srf1 = Srf.Faces[0];
            double u, v;
            srf1.ClosestPoint(pts[0], out u, out v);
            Vector3d n = srf1.NormalAt(u, v);
            Plane target_1 = new Plane(pts[0], n, Vector3d.XAxis);
            Plane target_2 = new Plane(pts[1], n, Vector3d.XAxis);
            Transform orient = Transform.PlaneToPlane(source, target_1);
            Transform orient_2 = Transform.PlaneToPlane(source, target_2);
            Mesh Horizontal_2 = new Mesh();
            Horizontal_2.CopyFrom(Horizontal);
            Horizontal_2.Transform(orient_2);
            Horizontal_2.FillHoles();
            Horizontal.Transform(orient);
            Horizontal.FillHoles();//cap Horizontal mullion
            Horizontal.Append(Horizontal_2);//Join two mesh
            return Horizontal;
        }
        public static Mesh Brep2mesh(Brep Srf)
        {
            Mesh output = Mesh.CreateFromBrep(Srf)[0];
            return output;
        }
        public static bool Bake_now(Mesh Orient_V, Mesh Orient_H, Mesh Window, bool Bake)
        {
            if (Bake)
            {
                //layers for mullion, transom, window
                string v_name = "Vertical Mullion";
                string h_name = "Horizontal Transom";
                string w_name = "Glass Window";
                Rhino.DocObjects.Layer v_layer = new Rhino.DocObjects.Layer();
                Rhino.DocObjects.Layer h_layer = new Rhino.DocObjects.Layer();
                Rhino.DocObjects.Layer w_layer = new Rhino.DocObjects.Layer();
                v_layer.Name = v_name;
                v_layer.Color = System.Drawing.Color.Beige;
                h_layer.Name = h_name;
                h_layer.Color = System.Drawing.Color.Lavender;
                w_layer.Name = w_name;
                w_layer.Color = System.Drawing.Color.DimGray;

                int v_index = Rhino.RhinoDoc.ActiveDoc.Layers.FindByFullPath(v_name, -1);
                if (v_index == -1)
                {
                    v_index = Rhino.RhinoDoc.ActiveDoc.Layers.Add(v_layer);
                }
                Rhino.DocObjects.ObjectAttributes attributes_v = new Rhino.DocObjects.ObjectAttributes();
                attributes_v.ObjectColor = System.Drawing.Color.Beige;
                attributes_v.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
                attributes_v.LayerIndex = v_index;
                int h_index = Rhino.RhinoDoc.ActiveDoc.Layers.FindByFullPath(h_name, -1);
                if (h_index == -1)
                {
                    h_index = Rhino.RhinoDoc.ActiveDoc.Layers.Add(h_layer);
                }
                Rhino.DocObjects.ObjectAttributes attributes_h = new Rhino.DocObjects.ObjectAttributes();
                attributes_h.ObjectColor = System.Drawing.Color.Lavender;
                attributes_h.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
                attributes_h.LayerIndex = h_index;

                int w_index = Rhino.RhinoDoc.ActiveDoc.Layers.FindByFullPath(w_name, -1);
                if (w_index == -1)
                {
                    w_index = Rhino.RhinoDoc.ActiveDoc.Layers.Add(w_layer);
                }
                //Render material
                int glass_index = Rhino.RhinoDoc.ActiveDoc.Materials.Add();
                Rhino.DocObjects.Material clear = Rhino.RhinoDoc.ActiveDoc.Materials[glass_index];
                clear.DiffuseColor = System.Drawing.Color.Blue;
                clear.Transparency = 0.8;
                clear.CommitChanges();

                //object attributes to set layer color and render
                Rhino.DocObjects.ObjectAttributes attributes_w = new Rhino.DocObjects.ObjectAttributes();
                attributes_w.ObjectColor = System.Drawing.Color.DimGray;
                attributes_w.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
                attributes_w.LayerIndex = w_index;
                attributes_w.MaterialIndex = glass_index;
                attributes_w.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;
                Rhino.RhinoDoc.ActiveDoc.Objects.AddMesh(Orient_V, attributes_v);
                Rhino.RhinoDoc.ActiveDoc.Objects.AddMesh(Orient_H, attributes_h);
                Rhino.RhinoDoc.ActiveDoc.Objects.AddMesh(Window, attributes_w);

            }
            return Bake;

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
}

    

