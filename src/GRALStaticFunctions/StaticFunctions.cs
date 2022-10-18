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
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace GralStaticFunctions
{
    public static class St_F
    {

        public static Font Small_Font;
        public static double Pin_Wind_Scale;
        public static Rectangle WindRoseLegend = new Rectangle();
        public static Rectangle WindRoseInfo = new Rectangle();
        public static Size WindRoseFormSize;
        public static string NumberFormat = String.Empty;
        public static readonly Guid FileDialogMeteo = new Guid("A2234CDE-CC12-1AE2-BAD3-223CA12AB23A");
        public static readonly Guid FileDialogProject = new Guid("A2234CDE-CC12-1AE2-BAD3-223CA12AB23B");
        public static readonly Guid FileDialogOtherProject = new Guid("A2234CDE-CC12-1AE2-BAD3-223CA12AB23C");
        public static readonly Guid FileDialogBuildings = new Guid("A2234CDE-CC12-1AE2-BAD3-223CA12AB23D");
        public static readonly Guid FileDialogSources = new Guid("A2234CDE-CC12-1AE2-BAD3-223CA12AB23E");
        public static readonly Guid FileDialogMaps = new Guid("A2234CDE-CC12-1AE2-BAD3-223CA12AB23F");
        public static readonly Guid FileDialogSettings = new Guid("A2234CDE-CC12-1AE2-BAD3-223CA12AB230");
        public static readonly Guid FileDialogTopo = new Guid("A2234CDE-CC12-1AE2-BAD3-223CA12AB231");
        public static readonly Guid FileDialogExe = new Guid("A2234CDE-CC12-1AE2-BAD3-223CA12AB232");
        private static readonly CultureInfo ic = CultureInfo.InvariantCulture;
        
        /// <summary>
        /// Convert an invariant culture string with a double number to a local culture string
        /// </summary>
        public static string StrgToICult(string text)
        {
            double dblOut;
        
            if (double.TryParse(text, out dblOut) == true) // try to parse with local culture settings
            {
                text = dblOut.ToString(ic);
            }
            else // cnversion failed
            {
                text = "0";
            }
            return text;
        }
        
        /// <summary>
        /// Convert a local culture string with a double number to an invariant culture string
        /// </summary>
        public static string ICultToLCult(string text)
        {
            double dblOut;
            try
            {
                dblOut = double.Parse(text, ic);
                if (NumberFormat == String.Empty)
                {
                    text = dblOut.ToString(); // use local culture for the conversion
                }
                else
                {
                    text = dblOut.ToString(NumberFormat); // use local culture for the conversion
                }
            }
            catch{}
            return text;
        }
        
        
        /// <summary>
        /// Convert a local culture double number to a local culture string
        /// </summary>
        public static string DblToLocTxt(double dblout)
        {
            String text = String.Empty;
            if (NumberFormat == String.Empty)
            {
                text = dblout.ToString();
            }
            else
            {
                text = dblout.ToString(NumberFormat);
            }
            return text;
        }
        
        /// <summary>
        /// Convert a local culture double number to an invariant culture string
        /// </summary>
        public static string DblToIvarTxt(double dblout)
        {
            String text = dblout.ToString(CultureInfo.InvariantCulture);
            return text;
        }
        
        /// <summary>
        /// Dialog to set the static small font object
        /// </summary>
        public static void SetSmallFont()
        {
            using (FontDialog ft = new FontDialog())
            {
                if (Small_Font != null)
                {
                    ft.Font = Small_Font;
                }

                if (ft.ShowDialog() == DialogResult.OK)
                {
                    Small_Font = ft.Font;
                }
            }
        }
        
        /// <summary>
        /// Convert an invariant culture text (change , to .) to a double number
        /// </summary>
        /// <param name="Text">Text to parse</param> 
        /// <param name="ShowMessage">Show a Message box if function fails</param> 
        public static double TxtToDbl(string Text, bool ShowMessage)
        {
            double dblOut;
            Text = Text.Replace(",", ".");

            try
            {
                dblOut = double.Parse(Text, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                if (ShowMessage)
                {
                    MessageBox.Show("Your input " + Text + " is not a valid number and must be deleted", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                dblOut = 0;
            }
            return dblOut;
        }
        
        /// <summary>
        /// Convert an invariant culture text to a double number
        /// </summary>
        /// <param name="Text">Text to parse</param> 
        /// <param name="ShowMessage">Show a Message box if function fails</param> 
        public static double TxtToDblICult(string Text, bool ShowMessage)
        {
            double dblOut;
            
            try
            {
                dblOut = double.Parse(Text, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                if (ShowMessage)
                {
                    MessageBox.Show("Your input " + Text + " is not a valid number and must be deleted", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                dblOut = 0;
            }
            return dblOut;
        }
        
        /// <summary>
        /// Convert a current cutlture text to a double number
        /// </summary>
        /// <param name="Text">Text to parse</param> 
        /// <param name="ShowMessage">Show a Message box if function fails</param> 
        public static double TxtToDblLCult(string Text, bool ShowMessage)
        {
            double dblOut;
            
            try
            {
                dblOut = double.Parse(Text, System.Globalization.CultureInfo.CurrentCulture);
            }
            catch
            {
                if (ShowMessage)
                {
                    MessageBox.Show("Your input " + Text + " is not a valid number and must be deleted", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                dblOut = 0;
            }
            return dblOut;
        }

        //routine to check whether a point is within a polygon or not
        /// <summary>
        /// Check if a point is within a polygon
        /// </summary>
        /// <param name="p">Point to check</param> 
        /// <param name="poly">Polygon</param> 
        public static bool PointInPolygon(Point p, List<Point> poly)
        {
            Point p1, p2;
            bool inside = false;
            if (poly.Count < 3)
            {
                return inside;
            }
            Point oldPoint = new Point(poly[poly.Count - 1].X, poly[poly.Count - 1].Y);
            for (int i = 0; i < poly.Count; i++)
            {
                Point newPoint = new Point(poly[i].X, poly[i].Y);
                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }
                if ((newPoint.X < p.X) == (p.X <= oldPoint.X) && ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X) < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
                {
                    inside = !inside;
                }
                oldPoint = newPoint;
            }
            return inside;
        }
        
         
        //routine to check whether a point is within a polygon or not
        /// <summary>
        /// Check if a point is within a polygon
        /// </summary>
        /// <param name="point">PointD to check</param> 
        /// <param name="poly">List with polygon points</param> 
        public static bool PointInPolygonD(GralDomain.PointD point, List<GralDomain.PointD> poly)
        {
            bool isInside = false;
            if (poly.Count < 3)
            {
                return isInside;
            }
            
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if (((poly[i].Y > point.Y) != (poly[j].Y > point.Y)) &&
                    (point.X < (poly[j].X - poly[i].X) * (point.Y - poly[i].Y) / (poly[j].Y - poly[i].Y) + poly[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }
 
        /// <summary>
        /// Check if a point is within a polygon
        /// </summary>
        /// <param name="point">PointD to check</param> 
        /// <param name="polygon">Array with polygon points</param> 
        public static bool PointInPolygonArray(GralDomain.PointD point, GralDomain.PointD[] polygon)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        //compute area of a polygon
        /// <summary>
        /// Compute the area of a polygon
        /// </summary>
        /// <param name="numpoints">Number of points in the array</param> 
        /// <param name="polypoints">Array for polygon points</param> 
        public static double CalcArea(int numpoints, GralDomain.PointD[] polypoints)
        {
            double area = 0;
            if (numpoints > 2 && numpoints < (polypoints.Length - 1))
            {
                polypoints[numpoints] = polypoints[0];
                for (int i = 0; i < numpoints; i++)
                {
                    area += (Convert.ToDouble(polypoints[i + 1].X) - Convert.ToDouble(polypoints[i].X)) * Convert.ToDouble(polypoints[i].Y) +
                        (Convert.ToDouble(polypoints[i + 1].X) - Convert.ToDouble(polypoints[i].X)) * (Convert.ToDouble(polypoints[i + 1].Y) - Convert.ToDouble(polypoints[i].Y)) / 2;
                }
                area = Math.Round(Math.Abs(area), 1);
            }
            //else
            //MessageBox.Show(this, "Number of vertices is below 3\t\nNo area could be computed");
            return area;
        }

        //compute area of a polygon
        /// <summary>
        /// Compute the area of a polygon; if Direction == true, the direction is calculated: (clockwise Area < 0, counter clockwise Area > 0)
        /// </summary>
        /// <param name="polypoints">List PointD with polygon points</param> 
        /// /// <param name="Direction">Analyze direction of the polygon?</param>
        public static double CalcArea(List<GralDomain.PointD> polypoints, bool Direction)
        {
            double area = 0;
            if (polypoints.Count > 2)
            {
                polypoints.Add(polypoints[0]);
                for (int i = 0; i < polypoints.Count - 1; i++)
                {
                    area += (Convert.ToDouble(polypoints[i + 1].X) - Convert.ToDouble(polypoints[i].X)) * Convert.ToDouble(polypoints[i].Y) +
                        (Convert.ToDouble(polypoints[i + 1].X) - Convert.ToDouble(polypoints[i].X)) * (Convert.ToDouble(polypoints[i + 1].Y) - Convert.ToDouble(polypoints[i].Y)) / 2;
                }
                polypoints.RemoveAt(polypoints.Count - 1);
                if (!Direction)
                {
                    area = Math.Round(Math.Abs(area), 1);
                }
            }
            //else
            //MessageBox.Show(this, "Number of vertices is below 3\t\nNo area could be computed");
            return area;
        }
        

        //compute lenght of a polygon
        /// <summary>
        /// Compute the lenght of a polyline
        /// </summary>
        /// <param name="numpoints">Number of points in the array</param> 
        /// <param name="polypoints">Array for polygon points</param> 
        public static double CalcLenght(int numpoints, GralDomain.PointD[] polypoints)
        {
            double lenght = 0;
            if (numpoints > 1 && numpoints < (polypoints.Length - 1))
            {
                polypoints[numpoints] = polypoints[0];
                for (int i = 0; i < numpoints - 1; i++)
                {
                    lenght += Math.Sqrt(Math.Pow(polypoints[i].X - polypoints[i + 1].X, 2) + Math.Pow(polypoints[i].Y - polypoints[i + 1].Y, 2));
                }
            }
            //else
            //MessageBox.Show(this, "Number of vertices is below 3\t\nNo area could be computed");
            return Math.Round(lenght, 1);
        }
        //compute lenght of a polygon
        /// <summary>
        /// Compute the lenght of a polyline
        /// </summary>
        /// <param name="numpoints">Number of points in the array</param> 
        /// <param name="polypoints">Array for polygon points</param> 
        public static double CalcLenght(int numpoints, GralData.PointD_3d[] polypoints)
        {
            double lenght = 0;
            if (numpoints > 1 && numpoints < (polypoints.Length - 1))
            {
                polypoints[numpoints] = polypoints[0];
                for (int i = 0; i < numpoints - 1; i++)
                {
                    lenght += Math.Sqrt(Math.Pow(polypoints[i].X - polypoints[i + 1].X, 2) + 
                                        Math.Pow(polypoints[i].Y - polypoints[i + 1].Y, 2) +
                                        Math.Pow(polypoints[i].Z - polypoints[i + 1].Z, 2));
                }
            }
            //else
            //MessageBox.Show(this, "Number of vertices is below 3\t\nNo area could be computed");
            return Math.Round(lenght, 1);
        }

        //compute lenght of a polygon
        /// <summary>
        /// Compute the lenght of a polyline
        /// </summary>
        /// <param name="polypoints">List of PointD with polygon points</param>
        public static double CalcLenght(List <GralDomain.PointD> polypoints)
        {
            double lenght = 0;
            if (polypoints.Count > 1)
            {
                for (int i = 0; i < polypoints.Count - 1; i++)
                {
                    lenght += Math.Sqrt(Math.Pow(polypoints[i].X - polypoints[i + 1].X, 2) + Math.Pow(polypoints[i].Y - polypoints[i + 1].Y, 2));
                }
            }
            //else
            //MessageBox.Show(this, "Number of vertices is below 3\t\nNo area could be computed");
            return Math.Round(lenght, 1);
        }
        //compute lenght of a polygon
        /// <summary>
        /// Compute the lenght of a polyline
        /// </summary>
        /// <param name="polypoints">List of PointD_3D with polygon points</param>
        public static double CalcLenght(List<GralData.PointD_3d> polypoints)
        {
            double lenght = 0;
            if (polypoints.Count > 1)
            {
                for (int i = 0; i < polypoints.Count - 1; i++)
                {
                    lenght += Math.Sqrt(Math.Pow(polypoints[i].X - polypoints[i + 1].X, 2) + 
                                        Math.Pow(polypoints[i].Y - polypoints[i + 1].Y, 2) +
                                        Math.Pow(polypoints[i].Z - polypoints[i + 1].Z, 2));
                }
            }
            //else
            //MessageBox.Show(this, "Number of vertices is below 3\t\nNo area could be computed");
            return Math.Round(lenght, 1);
        }

        /// <summary>
        /// Check if a string is a valid number
        /// </summary>
        public static bool CheckDoubleValid(string a)
        {
            double res;
            try
            {
                res = double.Parse(a);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check, if a textbox.text is a valid number
        /// </summary>
        /// <param name="sender">Textbox object</param> 
        /// <param name="e">EventArgs</param>
        /// <returns>Sets the textbox background color to yellow</returns>
        public static void CheckInput(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                if (CheckDoubleValid(tb.Text))
                {
                    tb.BackColor = Color.White;
                }
                else
                {
                    tb.BackColor = Color.Yellow;
                }
            }
        }

        /// <summary>
        ///Remove not valid characters "," and ";"
        /// </summary>
        public static string RemoveinvalidChars(string a) // check for invalid character "," Input "abc,de" and return "abc_de"
        {
            return a.Replace(',', '_').Replace(';', '_');
        }

        /// <summary>
        /// Convert a string to a rounded number and return a string
        /// </summary>
        /// <param name="a">String that contains a number</param> 
        /// <param name="dig">Digits for Math.Round()</param>
        public static string StringRound(string a, int dig)
        {
            double h;

            if (double.TryParse(a.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out h))
            {
                a = Convert.ToString(Math.Round(h, dig), ic);
            }

            return a;
        }
        
        // counts the numbers of lines of a text file
        /// <summary>
        /// Counts the number of lines in a text file
        /// </summary>
        /// <param name="filename">Full path and name of a text file</param> 
        public static long CountLinesInFile(string filename) 
        {
            long count = 0;
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        count++;
                    }
                }
            }
            catch
            {}
            return count;
        }
        
        // Get a Source-Group Number from the string
        /// <summary>
        /// Returns the source group number in a comma seperated string
        /// </summary>
        /// <param name="sg_name">String with source group information</param> 
        public static int GetSgNumber(string sg_name)
        {
            string[] text = sg_name.Split(new char[] { ',' });
            int nr = 0;
            if (text.GetUpperBound(0) > 0)
            {
                Int32.TryParse(text[1], out nr);
            }
            else
            {
                Int32.TryParse(text[0], out nr);
            }
            return nr;
        }
        
        
       // Retrieve a decimal span between min and max value
       /// <summary>
       /// Retrieve a decimal span between min and max value
       /// </summary>
       /// <param name="min">Allowed min value</param>
       /// <param name="max">Allowed max value</param>
       /// <param name="val">Value to check</param>
       public static decimal ValueSpan(double min, double max, double val)
       {
            decimal v = (decimal) Math.Max(min, Math.Min(max, val));
            return v;
       }
       
       /// <summary>
       /// Retrieve a integer span between min and max value
       /// </summary>
       /// <param name="min">Allowed min value</param>
       /// <param name="max">Allowed max value</param>
       /// <param name="val">Value to check</param>
       public static int ValueSpan(double min, double max, Int32 val)
       {
            int v = (int) Math.Max(min, Math.Min(max, val));
            return v;
       }
       
        // Restricts the entry of characters to digits (including hex), the negative sign,
        // the decimal point, and editing keystrokes (backspace).
        
        /// <summary>
        /// Restricts the entry of characters to digits (including hex), the negative sign, the decimal point, and editing keystrokes (backspace).
        /// </summary>
        /// <param name="sender">object</param> 
        /// <param name="e">KeyPressEventArgs</param>
        /// <returns>Sets e.Handled to true for invalid inputs</returns>
        public static void NumericInput(object sender, KeyPressEventArgs e)
        {
            NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
            string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
            string groupSeparator = numberFormatInfo.NumberGroupSeparator;
            string negativeSign = numberFormatInfo.NegativeSign;

            string keyInput = e.KeyChar.ToString();

            if (Char.IsDigit(e.KeyChar))
            {
                // Digits are OK
            }
            else if (keyInput.Equals(decimalSeparator) || keyInput.Equals(groupSeparator) ||
                     keyInput.Equals(negativeSign))
            {
                // Decimal separators are OK
            }
            else if (e.KeyChar == '\b')
            {
                // Backspace key is OK
            }
            else if (e.KeyChar == 'E' || e.KeyChar == 'e')
            {
                // exponent key is OK
            }
            else if ((Control.ModifierKeys & (Keys.Control | Keys.Alt)) != 0)
            {
                // Let the edit control handle control and alt key combinations
            }
            else
            {
                e.Handled = true;
            }
        }
        
        /// <summary>
        /// Shows a simple message box, similar to a MessageBox
        /// </summary>
        /// <param name="title">Title of the message box</param> 
        /// <param name="promtText">Text to be shown</param>
        /// <param name="_parent">Parent form</param>
        /// <returns>DialogResul OK or Cancel</returns>
        public static DialogResult InputBox(string title, string promptText, Form _parent)
        {
            DialogResult dialogResult = DialogResult.Cancel;
            using (Form form = new Form())
            {
                Label label = new Label();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = promptText;
                form.TopMost = true;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;
                dialogResult = form.ShowDialog(_parent);
            }
            return dialogResult;
        }
        
        /// <summary>
        /// Shows a simple message box, similar to a MessageBox and returns a string
        /// </summary>
        /// <param name="title">Title of the message box</param> 
        /// <param name="promtText">Text to be shown</param>
        /// <param name="_parent">Parent form</param>
        /// <returns>DialogResul OK or Cancel</returns>
        /// <returns>value as string</returns>
        public static DialogResult InputBoxValue(string title, string promptText, ref string value, Form _parent)
        {
            DialogResult dialogResult = DialogResult.Cancel;
            using (Form form = new Form())
            {
                Label label = new Label();
                TextBox textBox = new TextBox();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();
                
                form.Text = title;
                label.Text = promptText;
                textBox.Text = value;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 10, 372, 16);
                textBox.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                textBox.Anchor |= AnchorStyles.Right;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;
                form.TopMost = true;

                dialogResult = form.ShowDialog(_parent);
                value = textBox.Text;
            }
            return dialogResult;
        }

        /// <summary>
        /// Shows a simple message box, similar to a MessageBox
        /// </summary>
        /// <param name="title">Title of the message box</param> 
        /// <param name="promtText">Text to be shown</param>
        /// <param name="_parent">Parent form</param>
        /// <returns>DialogResul Yes or No</returns>
        public static DialogResult InputBoxYesNo(string title, string promptText, int x, int y)
        {
            DialogResult dialogResult = DialogResult.Cancel;
            using (Form form = new Form())
            {
                Label label = new Label();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = promptText;
                form.TopMost = true;

                buttonOk.Text = "Yes";
                buttonCancel.Text = "No";
                buttonOk.DialogResult = DialogResult.Yes;
                buttonCancel.DialogResult = DialogResult.No;

                label.SetBounds(9, 20, 372, 13);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.Manual;
                form.Left = x;
                form.Top = y;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;
                form.TopMost = true;

                dialogResult = form.ShowDialog();
            }
            return dialogResult;
        }

        /// <summary>
        /// Checks if the file GFF_Filepath.txt exist and reads the path to the flow field files
        /// </summary>
        /// <param name="ComputationPath">Path of the computation folder</param> 
        /// <returns>Path to the gff files as string</returns>
        public static string GetGffFilePath(string ComputationPath)
        {
            string gff_filepath = ComputationPath;
            
            try
            {
                if (File.Exists(Path.Combine(ComputationPath, "GFF_FilePath.txt")))
                {
                    using (StreamReader reader = new StreamReader(Path.Combine(ComputationPath, "GFF_FilePath.txt")))
                    {
                        string filepath = reader.ReadLine();
#if __MonoCS__
                        filepath = reader.ReadLine();
#endif
                        if (Directory.Exists(filepath))
                        {
                            gff_filepath = filepath;
                        }
                    }
                }
            }
            catch{}
            return gff_filepath;
        }

        /// <summary>
        /// Retrieve the left position in pixels of the current screen
        /// </summary>
        public static int GetScreenAtMousePosition()
        {
            Screen screen = Screen.FromPoint(Cursor.Position);
            return screen.Bounds.Left;            
        }

        /// <summary>
        /// Reduce the lenght of a file name inclusive path
        /// </summary>
        /// <param name="FileName">File name with path</param>
        /// <param name="Lenght">Lenght of resulting string</param>
        /// <returns></returns>
        public static string ReduceFileNameLenght(string FileName, int Lenght)
        {
            string _a = FileName;
            if (_a.Length > 0 && Lenght > 0 && _a.Length > Lenght)
            {
                int _pathLenght = Path.GetDirectoryName(FileName).Length;
                int _nameLenght = Path.GetFileName(FileName).Length;
                if (_nameLenght > Lenght)
                {
                    _a = Path.GetFileName(FileName);
                }
                else
                {
                    int _diff = Lenght - _nameLenght - 4;
                    if (_diff > 0)
                    {
                        _a = Path.GetDirectoryName(FileName).Substring(0, _diff) + "..." + Path.DirectorySeparatorChar + Path.GetFileName(FileName);
                    }
                    else
                    {
                        _a = Path.GetFileName(FileName);
                    }
                }
            }
            return _a;
        }

        /// <summary>
        /// Trim the text for fitting into the textbox
        /// </summary>
        /// <param name="box"></param>
        /// <param name="text"></param>
        public static void SetTrimmedTextToTextBox(TextBox box, string text)
        {
            float l1 = Math.Max(10, TextRenderer.MeasureText(text, box.Font).Width);
            box.Text = ReduceFileNameLenght(text, (int)(text.Length * (box.Width / l1)));
        }
        /// <summary>
        /// trim the text for fitting into the label
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text"></param>
        public static void SetTrimmedTextToTextBox(Label label, string text)
        {
            float l1 = Math.Max(10, TextRenderer.MeasureText(text, label.Font).Width);
            label.Text = ReduceFileNameLenght(text, (int)(text.Length * (label.Width / l1)));
        }

        /// <summary>
        /// Delete a file to recycling bin if possible
        /// </summary>
        /// <param name="FileName">File name with path</param>
        public static void FileDeleteRecyclingBin(string FileName)
        {
#if __MonoCS__
            File.Delete(FileName);
#else
            FileDelete.DeleteFileToRecyclingBin(FileName);
            Application.DoEvents();
#endif
        }

        /// <summary>
        /// Search a file at an absolute path; if not found, search within the recent project at the same sub-folder as in the original project or in a default sub folder
        /// </summary>
        /// <param name="ProjectPath">The recent project path</param>
        /// <param name="FilePath">The file with path to be searched</param>
        /// <param name="defaultSubFolder">The default folder for such files</param>
        /// <returns>(The path for the file, true if this is a new valid path - otherwise false)</returns>
        public static (string, bool) SearchAbsoluteAndRelativeFilePath(string ProjectPath, string FilePath, string defaultSubFolder)
        {
            if (File.Exists(FilePath))
            {
                return (FilePath, false);
            }
            else
            {
                // search File in the sub-directory of the recent project
                string result = string.Empty;
                string resultPath = string.Empty;
                string filename = Path.GetFileName(FilePath);
                bool saveNewFilePath = false;
                try
                {
                    DirectoryInfo di = new DirectoryInfo(ProjectPath);
                    string project = di.Name.ToString();
                    // if the file is in the project root folder
                    if (string.Equals(project, Directory.GetParent(FilePath).Name))
                    {
                        result = Path.Combine(ProjectPath, filename);
                        saveNewFilePath = true;
                    }
                    // find the file in a sub folder?
                    else
                    {
                        while (!string.IsNullOrWhiteSpace(FilePath))
                        {
                            DirectoryInfo parentFolder = Directory.GetParent(FilePath);
                            if (parentFolder == null)
                            {
                                resultPath = string.Empty;
                                break;
                            }
                            if (!string.Equals(project, parentFolder.Name.ToString()))
                            {
                                resultPath = Path.Combine(parentFolder.Name.ToString(), resultPath);
                            }
                            else
                            {
                                break;
                            }

                            FilePath = Directory.GetParent(FilePath).FullName;
                        }
                        if (!string.IsNullOrEmpty(resultPath))
                        {
                            result = Path.Combine(ProjectPath, resultPath, filename);
                            saveNewFilePath = true;
                        }
                    }
                }
                catch
                {
                    result = string.Empty;
                }
                // and finally check at the default folder
                if (string.IsNullOrEmpty(result) && File.Exists(Path.Combine(ProjectPath, defaultSubFolder, Path.GetFileName(FilePath))))
                {
                    return (Path.Combine(ProjectPath, defaultSubFolder, Path.GetFileName(FilePath)), true);
                }
                return (result, saveNewFilePath);
            }
        }

    }
}
