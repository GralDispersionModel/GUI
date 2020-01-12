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
using System.Drawing;
using System.Windows.Forms;
using System.IO;


namespace GralDomain
{
    public partial class Domain
	{
		/////////////////////////////////////////////////////////////
		//
		//   Georeferencing
		//
		/////////////////////////////////////////////////////////////

		/// <summary>
        /// Georeferencing using one reference point and a map scale
        /// </summary>
		private void GeoReferencingOne()
		{
			//when there is no Base map defined yet, a blank map is established automatically
			bool basemapexists = false;
			foreach (DrawingObjects _drobj in ItemOptions)
			{
				if ((_drobj.Name.Substring(0, 3) == "BM:") || (_drobj.Name.Substring(0, 3) == "SM:"))
				{
					basemapexists = true;
					break;
				}
			}
			if (basemapexists == false)
			{
                //add base map to object list
                DrawingObjects _drobj = new DrawingObjects("BM: " + Path.GetFileNameWithoutExtension(MapFileName))
                {
                    Picture = new Bitmap(picturebox1.Width, picturebox1.Height)
                };
                string newPath = Path.Combine(Gral.Main.ProjectName, @"Maps", "Blank.gif");
				_drobj.Picture.Save(newPath);
				MapFileName = "Blank.gif";
				
				_drobj.Label = 0;
				_drobj.ContourFilename = MapFileName;
				_drobj.DestRec   = new Rectangle(0, 0, Convert.ToInt32(_drobj.Picture.Width * XFac), Convert.ToInt32(_drobj.Picture.Height * XFac));
				_drobj.SourceRec = new Rectangle(0, 0, _drobj.Picture.Width, _drobj.Picture.Height);
				_drobj.PixelMx = MapSize.SizeX;
				_drobj.West = MapSize.West;
				_drobj.North = MapSize.North;
				ItemOptions.Insert(0, _drobj);
				SaveDomainSettings(1);	
			}
			// end of establishing a blank map

			HideWindows(0);
			if (toolStripButton3.Checked == true)
			{
				GeoReferenceOne.Show();
				GeoReferenceOne.TopMost = true;
				MouseControl = 3;
				Cursor = Cursors.Cross;
				//this.WindowState = System.Windows.Forms.FormWindowState.Normal;
				//this.Location = new System.Drawing.Point(0, 0);
				//this.Width = ScreenWidth - this.geo1.Width;
				//this.Height = ScreenHeight - 50;
				GeoReferenceOne.Location = new System.Drawing.Point(Right - 180 - 220, Top + 60); // Kuntner

				//avoid zoom buttons
				button1.Visible = false;
				button2.Visible = false;
				button3.Visible = false;
				button4.Visible = false;
				button5.Visible = false;
				button7.Visible = false;
			}
			else if (toolStripButton3.Checked == false)
			{
				//enable zoom buttons
				button1.Visible = true;
				button2.Visible = true;
				button3.Visible = true;
				button4.Visible = true;
				button5.Visible = true;
				button7.Visible = true;

				try
				{
					//scaling of the map
					double scale = 0, x1= 0, y1 = 0, x2 = 0, y2 = 0, xpicture = 0, ypicture = 0;
					
					if (double.TryParse(GeoReferenceOne.textBox5.Text, out scale) &&
					    double.TryParse(GeoReferenceOne.textBox1.Text, out x1) &&
					    double.TryParse(GeoReferenceOne.textBox2.Text, out y1) &&
					    double.TryParse(GeoReferenceOne.textBox3.Text, out x2) &&
					    double.TryParse(GeoReferenceOne.textBox4.Text, out y2) &&
					    double.TryParse(GeoReferenceOne.textBox6.Text, out xpicture) &&
					    double.TryParse(GeoReferenceOne.textBox7.Text, out ypicture))
					{;}
					else
					{
					    MessageBox.Show(this, "Error parsing input values", "GUI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					    return;
					}
					
					ItemOptions[0].PixelMx = scale / Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
					ItemOptions[0].West = Convert.ToDouble(GeoReferenceOne.textBox9.Text.Replace(".", decsep)) - xpicture * ItemOptions[0].PixelMx;
					ItemOptions[0].North = Convert.ToDouble(GeoReferenceOne.textBox8.Text.Replace(".", decsep)) + ypicture * ItemOptions[0].PixelMx;
					ItemOptions[0].DestRec = new Rectangle(TransformX + Convert.ToInt32((ItemOptions[0].West - MapSize.West) / BmpScale / MapSize.SizeX), TransformY - Convert.ToInt32((ItemOptions[0].North - MapSize.North) / BmpScale / MapSize.SizeX), Convert.ToInt32(ItemOptions[0].Picture.Width * ItemOptions[0].PixelMx / MapSize.SizeX * XFac), Convert.ToInt32(ItemOptions[0].Picture.Height * ItemOptions[0].PixelMx / MapSize.SizeX * XFac));

					//change geometry values for the base maps
					int k = -1;
					foreach (DrawingObjects _drobj in ItemOptions)
					{
						k = k + 1;
						if ((_drobj.Name.Substring(0, 3) == "BM:") || (_drobj.Name.Substring(0, 3) == "SM:"))
						{
							_drobj.DestRec = new Rectangle(TransformX + Convert.ToInt32((_drobj.West - MapSize.West) / BmpScale / MapSize.SizeX),
							                               TransformY - Convert.ToInt32((_drobj.North - MapSize.North) / BmpScale / MapSize.SizeX), 
							                               Convert.ToInt32(_drobj.Picture.Width * _drobj.PixelMx / MapSize.SizeX * XFac), 
							                               Convert.ToInt32(_drobj.Picture.Height * _drobj.PixelMx / MapSize.SizeX * XFac));
						}
					}
					SaveDomainSettings(1);

					try
					{
						//save data to world file
						if (SaveWorldFile() == false)
							throw new IOException();
						
						SwitchMenuGeoreference();
					}
					catch
					{
						MessageBox.Show(this, "Please define Project Folder first. Click on the 'New' or 'Open' button in the Projectmenue","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}

					//Zoom to map extents
					double xmin = 10000000;
					double ymin = 10000000;
					double xmax = -10000000;
					double ymax = -10000000;
					double fac1 = 1;
					double fac2 = 1;
					
					foreach(DrawingObjects _drobj in ItemOptions)
					{
						if ((_drobj.Name.Substring(0, 3) == "BM:") || (_drobj.Name.Substring(0, 3) == "SM:"))
						{
							xmin = Math.Min(xmin, _drobj.West);
							xmax = Math.Max(xmax, _drobj.West + _drobj.PixelMx * _drobj.Picture.Width);
							ymax = Math.Max(ymax, _drobj.North);
							ymin = Math.Min(ymin, _drobj.North - _drobj.PixelMx * _drobj.Picture.Height);
						}
					}
					
					fac1 = Convert.ToDouble(picturebox1.Width) / (xmax - xmin) * MapSize.SizeX;
					fac2 = Convert.ToDouble(picturebox1.Height) / (ymax - ymin) * MapSize.SizeX;
					XFac = Math.Min(fac1, fac2);
					BmpScale = 1 / XFac;

					TransformX = Convert.ToInt32(-(xmin - MapSize.West) / BmpScale / MapSize.SizeX);
					TransformY = Convert.ToInt32(-(ymax - MapSize.North) / BmpScale / MapSize.SizeY);

					//set source - and destination rectangle
					foreach(DrawingObjects _drobj in ItemOptions)
					{
						_drobj.DestRec = new Rectangle(TransformX + Convert.ToInt32((_drobj.West - MapSize.West) / BmpScale / MapSize.SizeX),
						                               TransformY - Convert.ToInt32((_drobj.North - MapSize.North) / BmpScale / MapSize.SizeX),
						                               Convert.ToInt32(_drobj.Picture.Width * _drobj.PixelMx / MapSize.SizeX * XFac),
						                               Convert.ToInt32(_drobj.Picture.Height * _drobj.PixelMx / MapSize.SizeX * XFac));
						_drobj.SourceRec = new Rectangle(0, 0, _drobj.Picture.Width, _drobj.Picture.Height);
					}

					//compute GRAL model domain
					try
					{
						int x11 = Convert.ToInt32((MainForm.GralDomRect.West - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
						int y11 = Convert.ToInt32((MainForm.GralDomRect.North - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
						int x21 = Convert.ToInt32((MainForm.GralDomRect.East - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
						int y21 = Convert.ToInt32((MainForm.GralDomRect.South - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
						int width = x21 - x11;
						int height = y21 - y11;
						GRALDomain = new Rectangle(x11, y11, width, height);
					}
					catch
					{
					}

					//compute GRAMM model domain
					try
					{
						int x11 = Convert.ToInt32((MainForm.GrammDomRect.West - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
						int y11 = Convert.ToInt32((MainForm.GrammDomRect.North - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
						int x21 = Convert.ToInt32((MainForm.GrammDomRect.East - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
						int y21 = Convert.ToInt32((MainForm.GrammDomRect.South - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
						int width = x21 - x11;
						int height = y21 - y11;
						GRAMMDomain = new Rectangle(x11, y11, width, height);
					}
					catch
					{
					}

					Picturebox1_Paint();
				}
				catch
				{
					GeoReferenceOne.textBox1.Text = "0";
					GeoReferenceOne.textBox2.Text = "0";
					GeoReferenceOne.textBox3.Text = "0";
					GeoReferenceOne.textBox4.Text = "0";
					GeoReferenceOne.textBox5.Text = "0";
					GeoReferenceOne.textBox6.Text = "0";
					GeoReferenceOne.textBox7.Text = "0";
					GeoReferenceOne.textBox8.Text = "0";
					GeoReferenceOne.textBox9.Text = "0";
					MessageBox.Show(this, "Invalid Input values. Georeferencing failed.","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				GeoReferenceOne.Hide();
				Cursor = Cursors.Default;
				MouseControl = 0;
				//this.Width = ScreenWidth; // RM Kuntner
				//this.Height = ScreenHeight - 50; // RM Kuntner
				Picturebox1_Paint();
			}
		}
		
		/// <summary>
        /// Georeferencing using two reference points
        /// </summary>
		private void GeoReferencingTwo()
		{
			//when there is no Base map defined yet, a blank map is established automatically
			bool basemapexists = false;
			
			foreach (DrawingObjects _drobj in ItemOptions)
			{
				if ((_drobj.Name.Substring(0, 3) == "BM:") || (_drobj.Name.Substring(0, 3) == "SM:"))
				{
					basemapexists = true;
					break;
				}
			}
			
			if (basemapexists == false)
			{
                //add base map to object list
                DrawingObjects _drobj = new DrawingObjects("BM: " + Path.GetFileNameWithoutExtension(MapFileName))
                {
                    Picture = new Bitmap(picturebox1.Width, picturebox1.Height)
                };
                string newPath = Path.Combine(Gral.Main.ProjectName, @"Maps", "Blank.gif");
				_drobj.Picture.Save(newPath);
				MapFileName = "Blank.gif";
				
				_drobj.Label = 0;
				_drobj.ContourFilename = MapFileName;
				_drobj.DestRec   = new Rectangle(0, 0, Convert.ToInt32(_drobj.Picture.Width * XFac), Convert.ToInt32(_drobj.Picture.Height * XFac));
				_drobj.SourceRec = new Rectangle(0, 0, _drobj.Picture.Width, _drobj.Picture.Height);
				_drobj.PixelMx = MapSize.SizeX;
				_drobj.West = MapSize.West;
				_drobj.North = MapSize.North;
				ItemOptions.Insert(0, _drobj);
				SaveDomainSettings(1);
			}
			// end of establishing a blank map

			HideWindows(0);
			if (toolStripButton4.Checked == true)
			{
				GeoReferenceTwo.Show();
				GeoReferenceTwo.TopMost = true;
				MouseControl = 12;
				Cursor = Cursors.Cross; // Kuntner
				//this.WindowState = System.Windows.Forms.FormWindowState.Normal; // RM Kuntner
				//this.Location = new System.Drawing.Point(0, 0); // RM Kuntner
				//this.Width = ScreenWidth - this.geo2.Width; // RM Kuntner
				//this.Height = ScreenHeight - 50; // RM Kuntner

				GeoReferenceTwo.Location = new System.Drawing.Point(Right - 180 - 220, Top + 60); // Kuntner

				//avoid zoom buttons
				button1.Visible = false;
				button2.Visible = false;
				button3.Visible = false;
				button4.Visible = false;
				button5.Visible = false;
				button7.Visible = false;
			}
			else if (toolStripButton4.Checked == false)
			{
				//enable zoom buttons
				button1.Visible = true;
				button2.Visible = true;
				button3.Visible = true;
				button4.Visible = true;
				button5.Visible = true;
				button7.Visible = true;

				try
				{
					//scaling of the map
					double x1= 0, y1 = 0, x2 = 0, y2 = 0, x3 = 0, y3 = 0, x4 = 0, y4 = 0, xpicture = 0, ypicture = 0;
					if (double.TryParse(GeoReferenceTwo.textBox9.Text, out x1) &&
    					double.TryParse(GeoReferenceTwo.textBox8.Text, out y1) &&
    					double.TryParse(GeoReferenceTwo.textBox2.Text, out x2) &&
    					double.TryParse(GeoReferenceTwo.textBox1.Text, out y2) &&
    					double.TryParse(GeoReferenceTwo.textBox6.Text, out x3) &&
    					double.TryParse(GeoReferenceTwo.textBox7.Text, out y3) &&
    					double.TryParse(GeoReferenceTwo.textBox4.Text, out x4) &&
    					double.TryParse(GeoReferenceTwo.textBox3.Text, out y4) &&
    					double.TryParse(GeoReferenceTwo.textBox6.Text, out xpicture) &&
    					double.TryParse(GeoReferenceTwo.textBox7.Text, out ypicture))
					{}
					else
					{
					    MessageBox.Show(this, "Error while parsing the input values", "GUI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					    return;
					}

					double scale = Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));

					
					ItemOptions[0].PixelMx = scale / Math.Sqrt(Math.Pow((x3 - x4), 2) + Math.Pow((y3 - y4), 2));
					ItemOptions[0].West = Convert.ToDouble(GeoReferenceTwo.textBox9.Text.Replace(".", decsep)) - xpicture * ItemOptions[0].PixelMx;
					ItemOptions[0].North = Convert.ToDouble(GeoReferenceTwo.textBox8.Text.Replace(".", decsep)) + ypicture * ItemOptions[0].PixelMx;
					ItemOptions[0].DestRec = new Rectangle(TransformX + Convert.ToInt32((ItemOptions[0].West - MapSize.West) / BmpScale / MapSize.SizeX), 
					                                       TransformY - Convert.ToInt32((ItemOptions[0].North - MapSize.North) / BmpScale / MapSize.SizeX), 
					                                       Convert.ToInt32(ItemOptions[0].Picture.Width * ItemOptions[0].PixelMx / MapSize.SizeX * XFac),
					                                       Convert.ToInt32(ItemOptions[0].Picture.Height * ItemOptions[0].PixelMx / MapSize.SizeX * XFac));

					//change geometry values for the base maps
					foreach (DrawingObjects _drobj in ItemOptions)
					{
						if ((_drobj.Name.Substring(0, 3) == "BM:") || (_drobj.Name.Substring(0, 3) == "SM:"))
						{
							_drobj.DestRec = new Rectangle(TransformX + Convert.ToInt32((_drobj.West - MapSize.West) / BmpScale / MapSize.SizeX), 
							                           TransformY - Convert.ToInt32((_drobj.North - MapSize.North) / BmpScale / MapSize.SizeX), 
							                           Convert.ToInt32(_drobj.Picture.Width * _drobj.PixelMx / MapSize.SizeX * XFac), 
							                           Convert.ToInt32(_drobj.Picture.Height * _drobj.PixelMx / MapSize.SizeX * XFac));
						}
					}
					
					SaveDomainSettings(1);

					try
					{
						//save data to world file
						if (SaveWorldFile() == false)
							throw new IOException();
						
						SwitchMenuGeoreference();
					}
					catch
					{
						MessageBox.Show(this, "Please define Project Folder first. Click on the 'New' or 'Open' button in the Projectmenue","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}

					//Zoom to map extents
					double xmin = 10000000;
					double ymin = 10000000;
					double xmax = -10000000;
					double ymax = -10000000;
					double fac1 = 1;
					double fac2 = 1;
					
					foreach(DrawingObjects _drobj in ItemOptions)
					{
						if ((_drobj.Name.Substring(0, 3) == "BM:") || (_drobj.Name.Substring(0, 3) == "SM:"))
						{
							xmin = Math.Min(xmin, _drobj.West);
							xmax = Math.Max(xmax, _drobj.West + _drobj.PixelMx * _drobj.Picture.Width);
							ymax = Math.Max(ymax, _drobj.North);
							ymin = Math.Min(ymin, _drobj.North - _drobj.PixelMx * _drobj.Picture.Height);
						}
					}
					
					fac1 = Convert.ToDouble(picturebox1.Width) / (xmax - xmin) * MapSize.SizeX;
					fac2 = Convert.ToDouble(picturebox1.Height) / (ymax - ymin) * MapSize.SizeX;
					XFac = Math.Min(fac1, fac2);
					BmpScale = 1 / XFac;

					TransformX = Convert.ToInt32(-(xmin - MapSize.West) / BmpScale / MapSize.SizeX);
					TransformY = Convert.ToInt32(-(ymax - MapSize.North) / BmpScale / MapSize.SizeY);

					//set source - and destination rectangle
					foreach(DrawingObjects _drobj in ItemOptions)
					{
						_drobj.DestRec = new Rectangle(TransformX + Convert.ToInt32((_drobj.West - MapSize.West) / BmpScale / MapSize.SizeX), 
						                               TransformY - Convert.ToInt32((_drobj.North - MapSize.North) / BmpScale / MapSize.SizeX), 
						                               Convert.ToInt32(_drobj.Picture.Width * _drobj.PixelMx / MapSize.SizeX * XFac),
						                               Convert.ToInt32(_drobj.Picture.Height * _drobj.PixelMx / MapSize.SizeX * XFac));
						_drobj.SourceRec = new Rectangle(0, 0, _drobj.Picture.Width, _drobj.Picture.Height);
					}
					
					//compute GRAL model domain
					try
					{
						int x11 = Convert.ToInt32((MainForm.GralDomRect.West - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
						int y11 = Convert.ToInt32((MainForm.GralDomRect.North - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
						int x21 = Convert.ToInt32((MainForm.GralDomRect.East - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
						int y21 = Convert.ToInt32((MainForm.GralDomRect.South - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
						int width = x21 - x11;
						int height = y21 - y11;
						GRALDomain = new Rectangle(x11, y11, width, height);
					}
					catch
					{
					}

					//compute GRAMM model domain
					try
					{
						int x11 = Convert.ToInt32((MainForm.GrammDomRect.West - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
						int y11 = Convert.ToInt32((MainForm.GrammDomRect.North - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
						int x21 = Convert.ToInt32((MainForm.GrammDomRect.East - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
						int y21 = Convert.ToInt32((MainForm.GrammDomRect.South - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
						int width = x21 - x11;
						int height = y21 - y11;
						GRAMMDomain = new Rectangle(x11, y11, width, height);
					}
					catch
					{
					}
					Picturebox1_Paint();
				}
				catch
				{
					GeoReferenceTwo.textBox1.Text = "";
					GeoReferenceTwo.textBox2.Text = "";
					GeoReferenceTwo.textBox3.Text = "";
					GeoReferenceTwo.textBox4.Text = "";
					GeoReferenceTwo.textBox6.Text = "0";
					GeoReferenceTwo.textBox7.Text = "0";
					GeoReferenceTwo.textBox8.Text = "";
					GeoReferenceTwo.textBox9.Text = "";
					MessageBox.Show(this, "Invalid Input values. Georeferencing failed.","GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				GeoReferenceTwo.Hide();
				Cursor = Cursors.Default;
				MouseControl = 0;
				//this.Width = ScreenWidth; // RM Kuntner
				// this.Height = ScreenHeight - 50; // RM Kuntner
			}
		}
		
		// switch menu elements if map is loaded
		private void SwitchMenuGeoreference()
		{
			groupBox2.Visible = true;
			button6.Visible = true;
			button18.Visible = true;
			button19.Visible = true;
			button20.Visible = true;
			button21.Visible = true;
			button26.Visible = true;
			button27.Visible = true;
			button28.Visible = true;
			button29.Visible = false;
			button35.Visible = true;
			//dont show button to define GRAMM domain, when GRAMM is used to take buildings into account
			if (MainForm.GRALSettings.BuildingMode != 3)
				button29.Visible = true;
		}
		
		private bool SaveWorldFile()
		{
			try
			{
				string Imagefile = Path.GetFileName(MapFileName);
				string newPath = Path.Combine(Gral.Main.ProjectName, @"Maps", Imagefile + "w");
				
				using(StreamWriter myWriter = File.CreateText(newPath))
				{
				    myWriter.WriteLine(Convert.ToString(ItemOptions[0].PixelMx, ic));
					myWriter.WriteLine(Convert.ToString(0, ic));
					myWriter.WriteLine(Convert.ToString(0, ic));
					myWriter.WriteLine(Convert.ToString(-ItemOptions[0].PixelMx, ic));
					myWriter.WriteLine(Convert.ToString(ItemOptions[0].West, ic));
					myWriter.WriteLine(Convert.ToString(ItemOptions[0].North, ic));
					myWriter.WriteLine(MapFileName);
				}
				
				return true;
			}
			catch
			{
				return false;
			}
		}
		
	}
}
