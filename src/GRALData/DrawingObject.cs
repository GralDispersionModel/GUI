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
using System.Drawing;

namespace GralDomain
{
    /// <summary>
    /// Drawing options for each layer (drawing object)
    /// </summary>
    public class DrawingObjects : IDisposable
    {
        /// <summary>
        /// Object layer name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// bitmap for drawing one pixel (for filling contour maps) -> unused
        /// </summary>
        public Bitmap BM { get; set; }
        /// <summary>
        /// position and size of the color scale
        /// </summary>
        public string ColorScale { get; set; }
        /// <summary>
        /// minimal aerea of countor polygon
        /// </summary>
        public int ContourAreaMin { get; set; }
        /// <summary>
        /// fill color 
        /// </summary>
        public int[,] ContourColor { get; set; }
        /// <summary>
        /// Geometry data for contour maps and post maps
        /// </summary>
        public ContourGeometries ContourGeometry { get; set; }
        /// <summary>
        /// distance of label drawing (each, 2nd, 3rd ... contour line)
        /// </summary>
        public int ContourLabelDist { get; set; }
        /// <summary>
        /// filename of contourplots
        /// </summary>
        public string ContourFilename { get; set; }
        /// <summary>
        /// decimal places for itemvalues
        /// </summary>
        public int DecimalPlaces { get; set; }
        /// <summary>
        /// source rectangle to draw base maps
        /// </summary>
        public Rectangle DestRec { get; set; }
        /// <summary>
        /// fill property of objects false/true
        /// </summary>
        public bool Fill { get; set; }
        /// <summary>
        /// list of fill colors for objects
        /// </summary>
        public List<Color> FillColors { get; set; }
        /// <summary>
        /// fill objects?
        /// </summary>
        public bool FillYesNo { get; set; }
        /// <summary>
        /// low pass filter of contour maps yes/no
        /// </summary>
        public bool Filter { get; set; }
        /// <summary>
        /// selected item of object for fill or drawing color
        /// </summary>
        public int Item { get; set; }
        /// <summary>
        /// List of item values 
        /// </summary>
        public List<double> ItemValues { get; set; }
        /// <summary>
        /// value determining visibility of the names or labels of objects (0=no labeling; 1=not visible; 2=visible)
        /// </summary>
        public int Label { get; set; }
        /// <summary>
        /// distance of labels
        /// </summary>
        public int LabelInterval { get; set; }
        /// <summary>
        /// label color
        /// </summary>
        public Color LabelColor { get; set; }
        /// <summary>
        /// label font
        /// </summary>
        public Font LabelFont { get; set; }
        /// <summary>
        /// title string of the legend
        /// </summary>
        public string LegendTitle { get; set; }
        /// <summary>
        /// unit of the legend and the data
        /// </summary>
        public string LegendUnit { get; set; }
        /// <summary>
        /// line color 
        /// </summary>
        public Color LineColor { get; set; }
        /// <summary>
        /// list of line colors of objects
        /// </summary>
        public List<Color> LineColors { get; set; }
        /// <summary>
        /// line width 
        /// </summary>
        public int LineWidth { get; set; }
        /// <summary>
        /// north border of base maps
        /// </summary>
        public double North { get; set; }
        /// <summary>
        /// bitmap data of a base map
        /// </summary>
        public Bitmap Picture { get; set; }
        /// <summary>
        /// natural size of pixels of base maps
        /// </summary>
        public double PixelMx { get; set; }
        /// <summary>
        /// Show this object?
        /// </summary>
        public bool Show { get; set; }
        /// <summary>
        /// selected sourcegroup of object
        /// </summary>
        public int SourceGroup { get; set; }
        /// <summary>
        /// destination rectangle to draw base maps
        /// </summary>
        public Rectangle SourceRec { get; set; }
        /// <summary>
        /// transparancy of objects
        /// </summary>
        public int Transparancy { get; set; }
        /// <summary>
        /// scale of vectors for vector maps
        /// </summary>
        public float VectorScale { get; set; }
        /// <summary>
        /// west border of base maps
        /// </summary>
        public double West { get; set; }
        /// <summary>
        /// simple contour algorithm?
        /// </summary>
        public bool DrawSimpleContour { get; set; }
        /// <summary>
        /// tension value for contour splines
        /// </summary>
        public float ContourTension { get; set; }
        /// <summary>
        /// filter value for contour douglas peucker
        /// </summary>
        public float ContourFilter { get; set; }
        /// <summary>
        /// clipping rectangle 
        /// </summary>
        public RectangleF ContourClipping { get; set; }
        /// <summary>
        /// draw border around clipping rectangle?
        /// </summary>
        public bool ContourDrawBorder { get; set; }
        /// <summary>
        /// draw label with fixed font size or scalable font size?
        /// </summary>
        public bool ScaleLabelFont { get; set; }
        /// <summary>
        /// Angle value for the spike removement algorithm
        /// </summary>
        public int RemoveSpikes { get; set; }
        /// <summary>
        /// list of Points for contour maps
        /// </summary>
        public List<List<PointF>> ContourPoints { get; set; }
        /// <summary>
        /// list of Polygons for contour maps
        /// </summary>
        public List<GralData.ContourPolygon> ContourPolygons { get; set; }
        /// <summary>
        /// list of points when importing shape files
        /// </summary>
        public List<PointF> ShpPoints { get; set; }
        /// <summary>
        /// list of lines when importing shape files
        /// </summary>
        public List<GralShape.SHPLine> ShpLines { get; set; }
        /// <summary>
        /// list of polygons when importing shape files
        /// </summary>
        public List<GralShape.SHPPolygon> ShpPolygons { get; set; }
        /// <summary>
        /// list of strings for item infos
        /// </summary>
        public List<string> ItemInfo { get; set; }

        private bool disposed = false;

        /// <summary>
        /// Default settings for the drawing object
        /// </summary>
        public DrawingObjects(string _name)
        {
            Name = _name;
            Show = true;
            Label = 1;
            ScaleLabelFont = false;
            Transparancy = 255;
            LabelInterval = 1;
            Fill = false;
            FillYesNo = false;
            ContourAreaMin = 0;
            ContourLabelDist = 150;
            Item = -1;
            SourceGroup = -1;

            ColorScale = "-999,-999,-999";
            LegendTitle = "x";
            LegendUnit = "x";

            ContourFilename = "x";
            LineWidth = 1;
            Filter = false;

            DrawSimpleContour = false;
            ContourDrawBorder = true;
            ContourTension = 0.2f;
            ContourFilter = 0.2F;

            LineColors = new List<Color>();
            FillColors = new List<Color>();
            ItemValues = new List<double>();

            DecimalPlaces = 2;

            if (_name == "POINT SOURCES")
            {
                if (string.IsNullOrEmpty(_name))
                {
                    _name = "PS";
                }
                FillColors.Add(Color.Transparent);
                ItemValues.Add(0);
                LineColors.Add(Color.Transparent);
                LineColor = Color.Transparent;
                LabelColor = Color.Black;
                LabelFont = new Font("Arial", 8);
            }
            else if (_name == "AREA SOURCES")
            {
                LineColor = Color.Black;
                LabelFont = new Font("Arial", 12);
                Fill = true;
                FillColors.Add(Color.Red);
                ItemValues.Add(0);
                LineColors.Add(Color.Black);
                LegendTitle = "Area source";
                LegendUnit = "kg/h";
            }
            else if (_name == "BUILDINGS")
            {
                LineColor = Color.Blue;
                LabelFont = new Font("Arial", 12);
                LabelColor = Color.Blue;
                Fill = true;
                FillColors.Add(Color.Red);
                ItemValues.Add(0);
                LineColors.Add(Color.Black);
                LegendTitle = "Buildings";
                LegendUnit = "m";
            }
            else if (_name == "LINE SOURCES")
            {
                LineColor = Color.Black;
                LabelColor = Color.Red;
                LabelFont = new Font("Arial", 12);

                ContourLabelDist = 200;
                LabelInterval = 1;
                Fill = true;
                FillColors.Add(Color.Red);
                ItemValues.Add(0);
                LineColors.Add(Color.Black);
                LegendTitle = "Line source";
                LegendUnit = "kg/(h*km)";
            }
            else if (_name == "TUNNEL PORTALS")
            {
                LabelColor = Color.Red;
                LabelColor = Color.Black;
                LabelFont = new Font("Arial", 12);
                Fill = true;
                FillColors.Add(Color.Transparent);
                ItemValues.Add(0);
                LineColors.Add(Color.Red);
                LegendTitle = "Portal source";
                LegendUnit = "kg/h";
            }
            else if (_name == "WALLS")
            {
                LineColor = Color.Black;
                LabelColor = Color.Red;
                LabelFont = new Font("Arial", 12);
                ContourLabelDist = 100;
                LabelInterval = 1;
                Fill = true;
                FillColors.Add(Color.Gray);
                ItemValues.Add(0);
                LineColors.Add(Color.Black);
                LegendTitle = "Walls";
                LegendUnit = "m";
            }
            else if (_name == "GRAL DOMAIN")
            {
                Label = 0;
                LineColor = Color.Blue;
                LabelColor = Color.Transparent;
                LabelFont = new Font("Arial", 12);
                FillColors.Add(Color.Gray);
                ItemValues.Add(0);
                LineColors.Add(Color.Black);
            }
            else if (_name == "GRAMM DOMAIN")
            {
                Label = 0;
                LineColor = Color.Brown;
                LabelColor = Color.Transparent;
                LabelFont = new Font("Arial", 12);
                FillColors.Add(Color.Gray);
                ItemValues.Add(0);
                LineColors.Add(Color.Black);
            }
            else if (_name.StartsWith("CM: ") || _name.StartsWith("PM: ") || _name.StartsWith("VM: "))
            {
                LineColor = Color.Black;
                LabelColor = Color.Black;
                LabelFont = new Font("Arial", 12);
                LabelInterval = 1;
                ContourLabelDist = 500;
                if (_name.StartsWith("VM: "))
                {
                    ContourLabelDist = 1;
                    VectorScale = 1;
                }
                else
                {
                    ContourLabelDist = 100;
                }
                Fill = true;
                FillYesNo = true;
            }
            else if (_name.StartsWith("SM: "))
            {
                LineColor = Color.Black;
                LabelColor = Color.Black;
                LabelFont = new Font("Arial", 12);
                LabelInterval = 1;
                FillColors.Add(Color.Black);
                ItemValues.Add(0);
                LineColors.Add(Color.Black);
            }
            else if (_name.StartsWith("BM: "))
            {
                ContourLabelDist = 500;
                ItemValues.Add(0);
                LineColors.Add(Color.Black);
                LabelColor = Color.Black;
                LabelFont = new Font("Arial", 8);
            }
            else if (_name == "VEGETATION")
            {
                LineColor = Color.LightGreen;
                LabelColor = Color.DarkGreen;
                LabelFont = new Font("Arial", 12);
                Transparancy = 128;
                Fill = true;
                FillYesNo = true;
                ItemValues.Add(0);
                LineColors.Add(Color.Green);
                FillColors.Add(Color.Green);

                LegendTitle = "Vegetation";
                LegendUnit = "m";
            }
            else if (_name == "RECEPTORS")
            {
                LabelColor = Color.Red;
                LabelFont = new Font("Arial", 12);
                ScaleLabelFont = false;
                Fill = true;
                FillColors.Add(Color.Red);
                ItemValues.Add(0);
                LineColors.Add(Color.Black);
                LegendTitle = "Receptors";
                LegendUnit = "m";
            }
            else if (_name == String.Empty)
            {
                LineColor = Color.Transparent;
                LabelColor = Color.Black;
                LabelFont = new Font("Arial", 12);
            }
            else if (_name == "WINDROSE")
            {
                LabelInterval = 8;
                LineWidth = 50;
                ScaleLabelFont = true;
                ContourAreaMin = 0; // auto scale
                Filter = true;
            }
            else if (_name == "SCALE BAR")
            {
                LineColor = Color.Black;
                LabelColor = Color.Black;
                LabelFont = new Font("Arial", 12);
                FillColors.Add(Color.Transparent);
                ItemValues.Add(0);
                LineColors.Add(Color.Black);
            }
            else if (_name.StartsWith("CONCENTRATION VALUES") || _name.StartsWith("ITEM INFO"))
            {
                FillColors.Add(Color.Transparent);
                ItemValues.Add(0);
                LineColors.Add(Color.Blue);
                LineColor = Color.Transparent;
                LabelColor = Color.Black;
                LabelFont = new Font("Arial", 9);
                Label = 1;
                ScaleLabelFont = false;
                DecimalPlaces = 1;
                LineWidth = 4;
                Fill = true;
                FillYesNo = true;
                ShpPoints = new List<PointF>();
                ItemInfo = new List<string>();
                ContourLabelDist = 0;
            }
            else
            {
                FillColors.Add(Color.Transparent);
                ItemValues.Add(0);
                LineColors.Add(Color.Transparent);
                LineColor = Color.Transparent;
                LabelColor = Color.Black;
                LabelFont = new Font("Arial", 12);
            }

            ContourPoints = new List<List<PointF>>();
            ContourPolygons = new List<GralData.ContourPolygon>();
            ContourColor = new int[,] { { -1 }, { -1 } };
            ContourGeometry = new ContourGeometries(0, 0, 1, 1, 1);
            VectorScale = 1;

            DestRec = new Rectangle(0, 0, 1, 1);
            SourceRec = new Rectangle(0, 0, 1, 1);
            PixelMx = 1;
            West = 0;
            North = 0;
            Picture = new Bitmap(1, 1);
            ShpPoints = new List<PointF>();
            ShpLines = new List<GralShape.SHPLine>();
            ShpPolygons = new List<GralShape.SHPPolygon>();
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (ShpLines != null)
                {
                    for (int i = 0; i < ShpLines.Count; i++)
                    {
                        ShpLines[i] = null;
                    }
                    ShpLines.Clear();
                    ShpLines.TrimExcess();
                }
                if (ShpPolygons != null)
                {
                    for (int i = 0; i < ShpPolygons.Count; i++)
                    {
                       ShpPolygons[i] = null;
                    }
                    ShpPolygons.Clear();
                    ShpPolygons.TrimExcess();
                }
                if (ShpPoints != null)
                {
                    ShpPoints.Clear();
                    ShpPoints.TrimExcess();
                }

                if (BM != null)
                {
                    BM.Dispose();
                    BM = null;
                }

                if (Picture != null)
                {
                    Picture.Dispose();
                    Picture = null;
                }

                ContourGeometry = null;
                ContourColor = null;
                if (ContourPolygons != null)
                {
                    ContourPolygons.Clear();
                    ContourPolygons.TrimExcess();
                }

                if (ContourPoints != null)
                {
                    ContourPoints.Clear();
                    ContourPoints.TrimExcess();
                }
                if (ItemValues != null)
                {
                    ItemValues.Clear();
                    ItemValues.TrimExcess();
                }
                if (FillColors != null)
                {
                    FillColors.Clear();
                    FillColors.TrimExcess();
                }
                if (LineColors != null)
                {
                    LineColors.Clear();
                    LineColors.TrimExcess();
                }
                if (LabelFont != null)
                {
                    LabelFont.Dispose();
                    LabelFont = null;
                }

            }
            disposed = true;
        }

        /// <summary>
        /// Class for the DrawObject countour geometry
        /// </summary>
        public class ContourGeometries
        {
            /// <summary>
            /// West bound
            /// </summary>
            public double X0;
            /// <summary>
            /// South bound
            /// </summary>
            public double Y0;
            /// <summary>
            /// DeltaX
            /// </summary>
            public double DX;
            /// <summary>
            /// Number of cells in x dir
            /// </summary>
            public int NX;
            /// <summary>
            /// Number of cells in y dir
            /// </summary>
            public int NY;

            /// <summary>
            /// Set values x0: West bound y0: South bound dx: DeltaX nx/ny: Number of cells in x/y dir
            /// </summary>
            public ContourGeometries(double x0, double y0, double dx, int nx, int ny)
            {
                X0 = x0;
                Y0 = y0;
                DX = dx;
                NX = nx;
                NY = ny;
            }
        }
    }
}
