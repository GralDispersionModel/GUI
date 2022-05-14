#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2019]  [Dietmar Oettl, Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.IO;

namespace Gral3DFunctions
{
    /// <summary>
    /// Interaction logic for X_3D_Win.xaml
    /// </summary>
    public partial class X_3D_Win : Window
	{
		public string DtaPath;
		public string ArrowPath;
		public string ElementsPath;
		public double Thickness = 0.03;
		public bool Smooth;
		
		public X_3D_Win(string a, string b, string c, bool smo)
		{
			InitializeComponent();
			DtaPath = a;
			ArrowPath = b;
			ElementsPath = c;
			Smooth = smo;		
		}
		
		// The main object model group.
        private Model3DGroup MainModel3Dgroup = new Model3DGroup();

        // The camera.
        private PerspectiveCamera TheCamera;

        // The camera's current location.
        private double CameraPhi = 0; 
        private double CameraTheta = Math.PI / 2.0;     
        private double CameraShiftx = 0;
        private double CameraShifty = 0;
        private double CameraShiftz = 0;
        private double CameraR = 13.0; 
        private double Cam_Zoom = 70;     // 15 degrees
        
        // The change in CameraPhi when you press the up and down arrows.
        private const double CameraDPhi = Math.PI / 50;

        // The change in CameraTheta when you press the left and right arrows.
        private const double CameraDTheta = Math.PI / 50;

        // The change in CameraR when you press + or -.
        private const double CameraDR = 0.2;

        // The models.
        public GeometryModel3D
            SurfaceModel, WireframeModel, NormalsModel, VertexNormalsModel, 
            ArrowModel, XArrowModel, YArrowModel, ZArrowModel, TextModel, ElementsModel;

        // Create the scene.
        // MainViewport is the Viewport3D defined
        // in the XAML code that displays everything.
        private void X_3D_Win_Loaded(object sender, RoutedEventArgs e)
        {
            // Give the camera its initial position.
            TheCamera = new PerspectiveCamera
            {
                FieldOfView = Cam_Zoom
            };
            MainViewport.Camera = TheCamera;
            PositionCamera();
            
            // Define lights.
            DefineLights();
			
            // Create the model.
            DefineModel(MainModel3Dgroup);

            // Add the group of models to a ModelVisual3D.
            ModelVisual3D model_visual = new ModelVisual3D
            {
                Content = MainModel3Dgroup
            };

            // Display the main visual in the viewportt.
            MainViewport.Children.Add(model_visual);
        }
       
        private void X_3D_Win_Closing(object sender, System.EventArgs e)
        {
        	try
        	{
        		
        		MainViewport.Children.Clear();
        		//File.SetAttributes(dta_Path, FileAttributes.Normal);
        		//File.SetAttributes(arrow_Path, FileAttributes.Normal);
        		
        		if (File.Exists(DtaPath))
                {
                    File.Delete(DtaPath);
                }

                if (File.Exists(ArrowPath))
                {
                    File.Delete(ArrowPath);
                }

                if (File.Exists(ElementsPath))
                {
                    File.Delete(ElementsPath);
                }
            }
        	catch
        	{}
        }

        // Define the lights.
        private void DefineLights()
        {
            AmbientLight ambient_light = new AmbientLight(Colors.Gray);
            DirectionalLight directional_light =
                new DirectionalLight(Colors.LightGray, new Vector3D(-1.2, -2.0, -2.0));
            MainModel3Dgroup.Children.Add(ambient_light);
            MainModel3Dgroup.Children.Add(directional_light);
        }

        private bool Load_Data(MeshGeometry3D mesh)
        {
        	try
        	{
        		if (File.Exists(DtaPath) == false)
                {
                    return false;
                }

                string[] dummy = new string[1000000];
        		int x_anz; int xc;
        		int y_anz; int yc;
        		double dx; double dy;

                using (BinaryReader myreader = new BinaryReader(File.Open(DtaPath, FileMode.Open))) // read Header
                {
                    dx = myreader.ReadDouble();
        			dy = myreader.ReadDouble();
                    x_anz = myreader.ReadInt32();
                    y_anz = myreader.ReadInt32();
                }

                double[,] heights = new double[x_anz + 1,y_anz + 1];
        		Thickness = 0.02;
        			if (x_anz >  20)
                {
                    Thickness = 0.01;
                }

                if (x_anz >  50)
                {
                    Thickness = 0.004;
                }

                if (x_anz >  100)
                {
                    Thickness = 0.002;
                }

                using (BinaryReader myreader = new BinaryReader(File.Open(DtaPath, FileMode.Open)))
                {
                    dx = myreader.ReadDouble();
                    dy = myreader.ReadDouble();
                    x_anz = myreader.ReadInt32();
                    y_anz = myreader.ReadInt32();
                    //MessageBox.Show(Convert.ToString(x_anz) + "/" + Convert.ToString(y_anz));        			
                    xc =0; yc=0;
        			try
        			{
        				while (myreader.BaseStream.Position != myreader.BaseStream.Length && xc <= x_anz)
        				{
        					//MessageBox.Show(dummy[0]);
        					for (yc = 0; yc <= y_anz; yc++)
        					{
                                heights[xc, yc] = myreader.ReadDouble();
        					}
        					xc++;
        				}
        			}
        			catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
        		}
        		//myreader.Close();
        		//MessageBox.Show(Convert.ToString(xc));

        		double x = -5;
        		for (xc = 0; xc <= x_anz;xc++)
        		{
        			double y = -(y_anz/2);
        			if (y_anz > 3)
                    {
                        y = -5; // for the 3D surface 
                    }

                    for (yc=0; yc < y_anz;yc++)
        			{
        				if (Smooth)
        				{
        					// Make points at the corners of the surface
        					// over (x, z) - (x + dx, y + dy).
        					if (xc < x_anz) // last point is used one step before 
        					{
        						Point3D p00 = new Point3D(x, heights[xc,yc], y);
        						Point3D p10 = new Point3D(x + dx, heights[xc+1,yc], y);
        						Point3D p01 = new Point3D(x, heights[xc,yc+1], y + dy);
        						Point3D p11 = new Point3D(x + dx,heights[xc+1,yc+1], y + dy);
        						// Add the triangles.
        						AddTriangle(mesh, p00, p01, p11);
        						AddTriangle(mesh, p00, p11, p10);
        					}
        				}
        				else
        				{
        					double hmin = 1000;     					
        					{
        						for (int i=-1; i<2; i++)
                                {
                                    for (int j = -1; j<2;j++)
                                    {
                                        if (xc+i >= 0 && yc+j >= 0 && xc+i <= x_anz && yc+j <= y_anz) // search lowest height around the actual Box
                                        {
                                            hmin = Math.Min(hmin, heights[xc+i, yc+j]);
                                        }
                                    }
                                }
                            }
        					if (hmin > 999)
                            {
                                hmin = heights[xc,yc] * 0.4;
                            }

                            double dxh = dx/2; double dyh = dy/2;
        					Point3D p0= new Point3D(x-dxh, hmin, y+dyh);
        					Point3D p1= new Point3D(x+dxh, hmin, y+dyh);
        					Point3D p2= new Point3D(x+dxh, heights[xc,yc], y+dyh);
        					Point3D p3= new Point3D(x-dxh, heights[xc,yc], y+dyh);
        					Point3D p4= new Point3D(x-dxh, hmin, y-dyh);
        					Point3D p5= new Point3D(x+dxh, hmin, y-dyh);
        					Point3D p6= new Point3D(x+dxh, heights[xc,yc], y-dyh);
        					Point3D p7= new Point3D(x-dxh, heights[xc,yc], y-dyh);
        					//front
        					AddTriangle(mesh, p0, p1, p2);
        					AddTriangle(mesh, p2, p3, p0);
        					// top
        					AddTriangle(mesh, p1, p5, p6);
        					AddTriangle(mesh, p6, p2, p1);
        					//back
        					AddTriangle(mesh, p7, p6, p5);
        					AddTriangle(mesh, p5, p4, p7);
        					// bottom
        					AddTriangle(mesh, p4, p0, p3);
        					AddTriangle(mesh, p3, p7, p4);
        					// left
//        					AddTriangle(mesh, p4, p5, p1);
//        					AddTriangle(mesh, p1, p0, p4);
        					// right
        					AddTriangle(mesh, p3, p2, p6);
        					AddTriangle(mesh, p6, p7, p3);
//        					Point3D p00 = new Point3D(x, heights[xc,yc], y);
//        					Point3D p10 = new Point3D(x + dx, heights[xc,yc], y);
//        					Point3D p01 = new Point3D(x, heights[xc,yc], y + dy);
//        					Point3D p11 = new Point3D(x + dx,heights[xc,yc], y + dy);
//        					// Add the triangles. Ebene
//        					AddTriangle(mesh, p00, p01, p11);
//        					AddTriangle(mesh, p00, p11, p10);
//        					p00 = new Point3D(x + dx, heights[xc,yc], y);
//        					p10 = new Point3D(x + dx, heights[xc,yc], y+dy);
//        					p01 = new Point3D(x + dx, heights[xc+1,yc], y);
//        					p11 = new Point3D(x + dx,heights[xc+1,yc+1], y + dy);
//        					// Add the triangles. vorne
//        					AddTriangle(mesh, p00, p01, p11);
//        					AddTriangle(mesh, p00, p11, p10);
//        					
//        					p00 = new Point3D(x, heights[xc,yc], y);
//        					p10 = new Point3D(x+dx, heights[xc,ycm], y);
//        					p01 = new Point3D(x+dx, heights[xc,yc], y);
//        					p11 = new Point3D(x,heights[xc,ycm], y);
//        					// Add the triangles. links
//        					AddTriangle(mesh, p00, p01, p11);
//        					AddTriangle(mesh, p01, p11, p10);
//        					p00 = new Point3D(x + dx, heights[xc,yc], y+dy);
//        					p10 = new Point3D(x + dx, heights[xc,yc], y);
//        					p01 = new Point3D(x + dx, heights[xc,yc+1], y + dy);
//        					p11 = new Point3D(x , heights[xc,yc+1], y +dy);
//        					// Add the triangles.rechts
//        					AddTriangle(mesh, p00, p01, p11);
//        					AddTriangle(mesh, p00, p11, p10);
        				}
        				
        				y+=dy;
        			}
        			x += dx;
        		}
        		
        		
        		return true;
        	}
        	catch
        	{
        		return false;
        	}
        }
        
        private bool Load_Arrows (Model3DGroup model_group)
        {
        	
        	try
        	{
        		string a;
        		string[] dummy = new string[1000000];
        		bool valid = false;
        		
        		if (File.Exists(ArrowPath) == false)
                {
                    return false;
                }

                using (BinaryReader myreader = new BinaryReader(File.Open(ArrowPath, FileMode.Open)))
                {
                    double dx = myreader.ReadDouble();
                    double dy = myreader.ReadDouble();
                    int x_anz = myreader.ReadInt32();
                    int z_anz = myreader.ReadInt32();
                    //MessageBox.Show(Convert.ToString(x_anz) + "/" + Convert.ToString(z_anz));
        			
        			int zc; double x = -5;
        			
        			try
        			{
        				while (myreader.BaseStream.Position != myreader.BaseStream.Length)
        				{
                            for (zc = 1; zc <= z_anz; zc++)
                            {

                                double z = myreader.ReadDouble(); ;
                                double u = myreader.ReadDouble(); ;
                                double v = myreader.ReadDouble(); ;
                                double vert = myreader.ReadDouble(); ;
                                double abs = myreader.ReadDouble(); ;

                                if (z >= 0 & abs > 0) // otherwise arrow is not valid
                                {
                                    valid = true;
                                    byte red = 20; byte green = 255; byte blue = 0;
                                    if (abs > 4.0)
                                    {
                                        green = Convert.ToByte(Math.Max(0, (10.0 - abs) * 40.0));
                                        red = 255;
                                    }
                                    else
                                    {
                                        green = Convert.ToByte(Math.Min(255, (190 + abs * 20.0)));
                                        red = Convert.ToByte(Math.Min(255, (75 + abs * 44.0)));
                                        blue = Convert.ToByte(Math.Max(0, 200 - abs * 30.0));
                                    }
                                    MeshGeometry3D mesh2 = new MeshGeometry3D();
                                    mesh2.AddArrow(new Point3D(x, z, 0), new Point3D(x - v, z + vert, u),
                                                   new Vector3D(0, 1, 0), Math.Max(0.01, Math.Min(0.1, abs / 20.0)));

                                    DiffuseMaterial arrow_material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(red, green, blue)));

                                    ArrowModel = new GeometryModel3D(mesh2, arrow_material);
                                    model_group.Children.Add(ArrowModel);
                                }
                            }
                            x += dx;
        				}
        			}
        			catch
        			{}
        		}
       
        		if (valid && (bool)chkScale.IsChecked) // valid Arrows - show color values
        		{
        			
        			double height = 0.2;
        			double z = 3;
        			for (int i = 5; i < 100; i+= 5)
        				
        			{
        				byte red=20; byte green=255; byte blue=0;
        				if (i/10 > 4.0)
        				{
        					green = Convert.ToByte(Math.Max(0,(10.0-i/10) * 40.0));
        					red = 255;
        				}
        				else
        				{
        					green = Convert.ToByte(Math.Min(255,(190 + i/10 * 20.0)));
        					red = Convert.ToByte(Math.Min(255,(75 + i/10 * 44.0)));
        					blue = Convert.ToByte(Math.Max(0, 200- i/10*30.0));
        				}
        				
        				string text = (i*0.1).ToString("F1") + " m/s";
                        TextBlock tb = new TextBlock(new Run(text))
                        {
                            FontFamily = new FontFamily("Arial"),
                            Foreground = new SolidColorBrush(Color.FromRgb(red, green, blue))
                        };
                        DiffuseMaterial mat = new DiffuseMaterial
                        {
                            Brush = new VisualBrush(tb)
                        };
                        double width = text.Length * height;
        				Vector3D over = new Vector3D(1,0,0);
        				Vector3D up = new Vector3D(0,1,0);
        				Point3D center = new Point3D(4,z,4);
        				z -= 0.22;
        				
        				Point3D p0 = center - width / 2 * over - height / 2 * up;
        				Point3D p1 = p0 + up * 1 * height;
        				Point3D p2 = p0 + over * width;
        				Point3D p3 = p0 + up * 1 * height + over * width;

                        MeshGeometry3D mg = new MeshGeometry3D
                        {
                            Positions = new Point3DCollection()
                        };
                        mg.Positions.Add(p0);    // 0
        				mg.Positions.Add(p1);    // 1
        				mg.Positions.Add(p2);    // 2
        				mg.Positions.Add(p3);    // 3

        				
        				mg.Positions.Add(p0);    // 4
        				mg.Positions.Add(p1);    // 5
        				mg.Positions.Add(p2);    // 6
        				mg.Positions.Add(p3);    // 7
        				

        				mg.TriangleIndices.Add(0);
        				mg.TriangleIndices.Add(3);
        				mg.TriangleIndices.Add(1);
        				mg.TriangleIndices.Add(0);
        				mg.TriangleIndices.Add(2);
        				mg.TriangleIndices.Add(3);

        				
        				mg.TriangleIndices.Add(4);
        				mg.TriangleIndices.Add(5);
        				mg.TriangleIndices.Add(7);
        				mg.TriangleIndices.Add(4);
        				mg.TriangleIndices.Add(7);
        				mg.TriangleIndices.Add(6);
        				

        				// These texture coordinates basically stretch the
        				// TextBox brush to cover the full side of the label.

        				mg.TextureCoordinates.Add(new Point(0, 1));
        				mg.TextureCoordinates.Add(new Point(0, 0));
        				mg.TextureCoordinates.Add(new Point(1, 1));
        				mg.TextureCoordinates.Add(new Point(1, 0));

        				
        				mg.TextureCoordinates.Add(new Point(1, 1));
        				mg.TextureCoordinates.Add(new Point(1, 0));
        				mg.TextureCoordinates.Add(new Point(0, 1));
        				mg.TextureCoordinates.Add(new Point(0, 0));
        				
        				TextModel = new GeometryModel3D(mg, mat);
        				model_group.Children.Add(TextModel);
        				
        				if (i>=40)
                        {
                            i +=5;
                        }
                    }
        		}
        		
        		return true;
        		//MessageBox.Show(Convert.ToString(xc));
        	}
        	catch
        	{
        		return false;
        	}
        }
        
        private bool Load_Elements (MeshGeometry3D mesh)
        {
        	try
        	{
        		string[] dummy = new string[1000000];
        		
        		if (File.Exists(ElementsPath) == false)
                {
                    return false;
                }

                using (BinaryReader myreader = new BinaryReader(File.Open(ElementsPath, FileMode.Open)))
                {

                    double dx = myreader.ReadDouble();
        			
        			while (myreader.BaseStream.Position != myreader.BaseStream.Length)
        			{
        				if (dummy[0] != "*")
        				{
        					double xr = myreader.ReadDouble(); ;
        					double yr = myreader.ReadDouble(); ;
        					double zr0 = myreader.ReadDouble(); ;
        					double zr1 = myreader.ReadDouble(); ;
        					
        					//mesh3.AddArrow(new Point3D(xr, zr0, yr ), new Point3D(xr, zr1 + 0.1, yr),new Vector3D(0,1,0),0.2);
        					//MessageBox.Show(dummy[0] +"/"+ dummy[1] +"/"+ dummy[2] +"/"+ dummy[3] +"/"+ dummy[4]);
        					
        					AddCylinder(mesh, new Point3D(xr,zr0, yr), new Vector3D(0, zr1-zr0, 0), dx/2, 10);
//        					DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255,0, 0)));
//        					
//        					ElementsModel = new GeometryModel3D(mesh3, material);
//        					model_group.Children.Add(ElementsModel);
        				}
        			}
        		} // Streamreader
        		return true;
        	} // Try
        	catch
        	{
        		return false;
        	}
        }
        
        
        // Add the model to the Model3DGroup.
        private void DefineModel(Model3DGroup model_group)
        {
        	try
        	{
        		// Make a mesh to hold the surface.
        		MeshGeometry3D mesh = new MeshGeometry3D();

        		if (Load_Data(mesh) == false)
                {
                    return;
                }

                // Make the surface's material using a solid green brush.
                DiffuseMaterial surface_material = new DiffuseMaterial(Brushes.LightGreen);

                // Make the surface's model.
                SurfaceModel = new GeometryModel3D(mesh, surface_material)
                {

                    // Make the surface visible from both sides.
                    BackMaterial = surface_material
                };

                // Add the model to the model groups.
                model_group.Children.Add(SurfaceModel);
        		try
        		{
        			// Make a wireframe.
        			MeshGeometry3D wireframe = mesh.ToWireframe(Thickness);
        			DiffuseMaterial wireframe_material = new DiffuseMaterial(Brushes.Gray);
        			WireframeModel = new GeometryModel3D(wireframe, wireframe_material);
        			if ((WireframeModel != null) && ((bool)chkWireframe.IsChecked))
                    {
                        model_group.Children.Add(WireframeModel);
                    }
                }
        		catch{}
        		
        		try
        		{
        			Load_Arrows(model_group);
        		}
        		catch{}
        		
        		try
        		{
        			// Make the Elements
        			if (File.Exists(ElementsPath))
        			{
        				MeshGeometry3D mesh3 = new MeshGeometry3D();
        				if (Load_Elements(mesh3))
        				{
        					DiffuseMaterial mat = new DiffuseMaterial(Brushes.Red);
        					ElementsModel = new GeometryModel3D(mesh3, mat);
        					
        					// Add the model to the model groups.
        					model_group.Children.Add(ElementsModel);
        				}
        				
        			}

        			// Make the arrows.
        			const double arrow_length = 6;
        			const double arrowhead_length = 1;
        			Point3D origin = new Point3D(0, 0, 0);
        			// X = Red.
        			MeshGeometry3D x_arrow_mesh = new MeshGeometry3D();
        			x_arrow_mesh.AddArrow(origin, new Point3D(arrow_length, 0, 0),
        			                      new Vector3D(0, 1, 0), arrowhead_length);
        			DiffuseMaterial x_arrow_material = new DiffuseMaterial(Brushes.Red);
        			XArrowModel = new GeometryModel3D(x_arrow_mesh, x_arrow_material);
        			if ((bool)chkCoor.IsChecked)
                    {
                        model_group.Children.Add(XArrowModel);
                    }

                    // Y = Green.
                    MeshGeometry3D y_arrow_mesh = new MeshGeometry3D();
        			y_arrow_mesh.AddArrow(origin, new Point3D(0, arrow_length - 1, 0),
        			                      new Vector3D(1, 0, 0), arrowhead_length);
        			DiffuseMaterial y_arrow_material = new DiffuseMaterial(Brushes.Green);
        			YArrowModel = new GeometryModel3D(y_arrow_mesh, y_arrow_material);
        			if ((bool)chkCoor.IsChecked)
                    {
                        model_group.Children.Add(YArrowModel);
                    }

                    // Z = Blue.
                    MeshGeometry3D z_arrow_mesh = new MeshGeometry3D();
        			z_arrow_mesh.AddArrow(origin, new Point3D(0, 0, arrow_length),
        			                      new Vector3D(0, 1, 0), arrowhead_length);
        			DiffuseMaterial z_arrow_material = new DiffuseMaterial(Brushes.Blue);
        			ZArrowModel = new GeometryModel3D(z_arrow_mesh, z_arrow_material);
        			if ((bool)chkCoor.IsChecked)
                    {
                        model_group.Children.Add(ZArrowModel);
                    }
                }
        		catch
        		{}
        	}
        	catch{}
        	
        }


        // Add a triangle to the indicated mesh.
        // If the triangle's points already exist, reuse them.
        private void AddTriangle(MeshGeometry3D mesh, Point3D point1, Point3D point2, Point3D point3)
        {
            // Get the points' indices.
            int index1 = AddPoint(mesh.Positions, point1);
            int index2 = AddPoint(mesh.Positions, point2);
            int index3 = AddPoint(mesh.Positions, point3);

            // Create the triangle.
            mesh.TriangleIndices.Add(index1);
            mesh.TriangleIndices.Add(index2);
            mesh.TriangleIndices.Add(index3);
        }

        // A dictionary to hold points for fast lookup.
        private Dictionary<Point3D, int> PointDictionary =
            new Dictionary<Point3D, int>();

        // If the point already exists, return its index.
        // Otherwise create the point and return its new index.
        private int AddPoint(Point3DCollection points, Point3D point)
        {
            // If the point is in the point dictionary,
            // return its saved index.
            if (PointDictionary.ContainsKey(point))
            {
                return PointDictionary[point];
            }

            // We didn't find the point. Create it.
            points.Add(point);
            PointDictionary.Add(point, points.Count - 1);
            return points.Count - 1;
        }

        // Add a cylinder.
		private void AddCylinder(MeshGeometry3D mesh,
		                               Point3D end_point, Vector3D axis, double radius, int num_sides)
		{
			// Get two vectors perpendicular to the axis.
			Vector3D v1;
			if ((axis.Z < -0.01) || (axis.Z > 0.01))
            {
                v1 = new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y);
            }
            else
            {
                v1 = new Vector3D(-axis.Y - axis.Z, axis.X, axis.X);
            }

            Vector3D v2 = Vector3D.CrossProduct(v1, axis);

			// Make the vectors have length radius.
			v1 *= (radius / v1.Length);
			v2 *= (radius / v2.Length);

			// Make the top end cap.
			double theta = 0;
			double dtheta = 2 * Math.PI / num_sides;
			for (int i = 0; i < num_sides; i++)
			{
				Point3D p1 = end_point +
					Math.Cos(theta) * v1 +
					Math.Sin(theta) * v2;
				theta += dtheta;
				Point3D p2 = end_point +
					Math.Cos(theta) * v1 +
					Math.Sin(theta) * v2;
				AddTriangle(mesh, end_point, p1, p2);
			}

			// Make the bottom end cap.
			Point3D end_point2 = end_point + axis;
			theta = 0;
			for (int i = 0; i < num_sides; i++)
			{
				Point3D p1 = end_point2 +
					Math.Cos(theta) * v1 +
					Math.Sin(theta) * v2;
				theta += dtheta;
				Point3D p2 = end_point2 +
					Math.Cos(theta) * v1 +
					Math.Sin(theta) * v2;
				AddTriangle(mesh, end_point2, p2, p1);
			}

			// Make the sides.
			theta = 0;
			for (int i = 0; i < num_sides; i++)
			{
				Point3D p1 = end_point +
					Math.Cos(theta) * v1 +
					Math.Sin(theta) * v2;
				theta += dtheta;
				Point3D p2 = end_point +
					Math.Cos(theta) * v1 +
					Math.Sin(theta) * v2;

				Point3D p3 = p1 + axis;
				Point3D p4 = p2 + axis;

				AddTriangle(mesh, p1, p3, p2);
				AddTriangle(mesh, p2, p3, p4);
			}
		}
        
        // Adjust the camera's position.
        private void X_3D_Win_KeyDown(object sender, KeyEventArgs e)
        {
        	
            switch (e.Key)
            {
            	case Key.Up:
            		if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control || (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            		{
            			CameraShiftz -= 0.2;
            			//if (CameraShiftz > Math.PI/4) CameraShiftz = Math.PI/4;
            		}
            		else
            		{
            			CameraPhi += CameraDPhi;
            			if (CameraPhi > Math.PI / 2.0)
                        {
                            CameraPhi = Math.PI / 2.0;
                        }
                    }
            		break;
            	case Key.Down:
            		if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control || (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            		{
            			CameraShiftz += 0.2;
            			//if (CameraShiftz < -Math.PI/4) CameraShiftz = -Math.PI/4;
            		}
            		else
            		{
            			CameraPhi -= CameraDPhi;
            			if (CameraPhi < -Math.PI / 2.0)
                        {
                            CameraPhi = -Math.PI / 2.0;
                        }
                    }
            		break;
            	case Key.Left:
            		if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            		{
            			CameraShiftx +=0.2;
            			
                    }
                    else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                    	CameraShifty +=0.2;
                    }
                    else
                    {
                    	CameraTheta += CameraDTheta;
                    }
                    break;
                   case Key.Right:
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                    	CameraShiftx -=0.2;
                    	
                    }
                    else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                    	CameraShifty -=0.2;
                    }
                    else
                    {
                    	CameraTheta -= CameraDTheta;
                    }
                    break;
                case Key.Add:
                case Key.OemPlus:
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            		{
                    	Cam_Zoom -= 10.0;
                    	Cam_Zoom = Math.Max(10, Cam_Zoom);
                    }
                    else
                    {
                    	CameraR -= CameraDR;
                    	if (CameraR < CameraDR)
                        {
                            CameraR = CameraDR;
                        }
                    }
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    { 
                    	
                    	Cam_Zoom += 10;
                    	Cam_Zoom = Math.Min(Cam_Zoom, 200);
                    	break;
                    }
                    else
                    {
                    	CameraR += CameraDR;
                   	break;
                    }
                    
                    
            }

            // Update the camera's position.
            PositionCamera();
        }

        // Position the camera.
        private void PositionCamera()
        {
            // Calculate the camera's position in Cartesian coordinates.
            
           
            double y = CameraR * Math.Sin(CameraPhi);
            double hyp = CameraR * Math.Cos(CameraPhi);
            double x = hyp * Math.Cos(CameraTheta)  ;
            double z = hyp * Math.Sin(CameraTheta);
           
            TheCamera.FieldOfView = Cam_Zoom;
            
            // Position of the camera
            TheCamera.Position = new Point3D(x, y, z);

//            // Look toward the origin.
//            TheCamera.LookDirection = new Vector3D(-x, -y, -z);
			
			Vector3D v = new Vector3D(-x,-y,-z);
			
			//Point3D l = new Point3D(0 , y + CameraShiftz, 0);
			Point3D l = new Point3D(0 , 0, 0);
			v = (l - TheCamera.Position);
			v.Normalize();
			
			
			//TheCamera.LookDirection = new Vector3D(v.X * Math.Cos(CameraPhi2) - v.Y*Math.Sin(CameraPhi2) , v.X * Math.Sin(CameraPhi2) + v.Y * Math.Cos(CameraPhi2), -z);
            
			TheCamera.LookDirection = v;
            // Set the Up direction.
            TheCamera.UpDirection = new Vector3D(0, 1, 0);
            //MessageBox.Show("Camera.Position: (" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ")");
            if (CameraShiftx !=0 || CameraShiftz != 0)
            {
	            TranslateTransform3D t = new TranslateTransform3D((CameraShiftx),CameraShiftz,(CameraShifty));
	           // CameraShiftx =0;
	            MainModel3Dgroup.Transform = t;     
            }
        }

        // Show and hide the appropriate GeometryModel3Ds.
        private void chkContents_Click(object sender, RoutedEventArgs e)
        {
        	// Remove the GeometryModel3Ds.
        	for (int i = MainModel3Dgroup.Children.Count - 1; i >= 0; i--)
        	{
        		if (MainModel3Dgroup.Children[i] is GeometryModel3D)
                {
                    MainModel3Dgroup.Children.RemoveAt(i);
                }
            }

        	// Add the selected GeometryModel3Ds.
        	if ((SurfaceModel != null) && ((bool)chkSurface.IsChecked))
            {
                MainModel3Dgroup.Children.Add(SurfaceModel);
            }

            if ((WireframeModel != null) && ((bool)chkWireframe.IsChecked))
            {
                MainModel3Dgroup.Children.Add(WireframeModel);
            }

            //        	if ((ArrowModel != null) && ((bool)chkArrows.IsChecked))
            //        	{
            Load_Arrows(MainModel3Dgroup);
        	//        		//MainModel3Dgroup.Children.Add(ArrowModel);
        	//        	}
        	
        	if (ElementsModel != null)
            {
                MainModel3Dgroup.Children.Add(ElementsModel);
            }

            if ((XArrowModel != null) && ((bool)chkCoor.IsChecked))
        	{
        		MainModel3Dgroup.Children.Add(XArrowModel);
        		MainModel3Dgroup.Children.Add(YArrowModel);
        		MainModel3Dgroup.Children.Add(ZArrowModel);
        	}
        	
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
        	 CameraPhi = 0; //Math.PI / 3.0;       // 15 degrees
        	 CameraTheta = Math.PI / 2.0;     // 15 degrees
        	 CameraShiftx = 0;
        	 CameraShifty = 0;
        	 CameraShiftz = 0;
        	 CameraR = 13.0;
        	
        	 //Camz_Theta = 0;       // 15 degrees
        	 Cam_Zoom = 70;     // 15 degrees
        	 PositionCamera();
        }
	}
}
