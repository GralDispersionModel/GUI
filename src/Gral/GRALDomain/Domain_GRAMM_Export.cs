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

/*
 * Created by SharpDevelop.
 * User: oettl1
 * Date: 28.04.2017
 * Time: 10:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using GralIO;
using GralMessage;

namespace GralDomain
{
    public partial class Domain
	{
		//////////////////////////////////////////////////////////////////////////
		//
		//Export GRAMM Sub-Domains
		//
		//////////////////////////////////////////////////////////////////////////

		/// <summary>
        /// Export a GRAMM Sub domain to a new GRAL project
        /// </summary>
		public void GrammExportFile()
		{
			try
			{
				//compute sub-model domain extenstions in natural coordinates and clip them to the chosen raster size of the flow field grid

				double subGRAMMnorth = Math.Round((GRAMMDomain.Top - TransformY) * BmpScale * MapSize.SizeY + MapSize.North, 1, MidpointRounding.AwayFromZero);
				subGRAMMnorth = Math.Round(subGRAMMnorth / MainForm.GRAMMHorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown18.Value);

				double subGRAMMsouth = Math.Round((GRAMMDomain.Bottom - TransformY) * BmpScale * MapSize.SizeY + MapSize.North, 1, MidpointRounding.AwayFromZero);
				subGRAMMsouth = Math.Round(subGRAMMsouth / MainForm.GRAMMHorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown18.Value);

				double subGRAMMwest = Math.Round((GRAMMDomain.Left - TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
				subGRAMMwest = Math.Round(subGRAMMwest / MainForm.GRAMMHorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown18.Value);

				double subGRAMMeast = Math.Round((GRAMMDomain.Right - TransformX) * BmpScale * MapSize.SizeX + MapSize.West, 1, MidpointRounding.AwayFromZero);
				subGRAMMeast = Math.Round(subGRAMMeast / MainForm.GRAMMHorGridSize, 0, MidpointRounding.AwayFromZero) * Convert.ToDouble(MainForm.numericUpDown18.Value);


				//clip rectangle to raster
				int x1 = Convert.ToInt32((subGRAMMwest - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
				int y1 = Convert.ToInt32((subGRAMMnorth - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
				int x2 = Convert.ToInt32((subGRAMMeast - MapSize.West) / BmpScale / MapSize.SizeX) + TransformX;
				int y2 = Convert.ToInt32((subGRAMMsouth - MapSize.North) / BmpScale / MapSize.SizeY) + TransformY;
				int width = x2 - x1;
				int height = y2 - y1;
				GRAMMDomain = new Rectangle(x1, y1, width, height);
				Cursor = Cursors.Default;

				//check if GRAMM sub-domain is within the GRAMM main-domain
				if ((subGRAMMeast > MainForm.GrammDomRect.East) || (subGRAMMwest < MainForm.GrammDomRect.West) || (subGRAMMsouth < MainForm.GrammDomRect.South) || (subGRAMMnorth > MainForm.GrammDomRect.North))
				{
					MessageBox.Show(this, "GRAMM sub-domain is outside GRAMM main-domain", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				//check if GRAMM sub-domain dimension is at least 2x2 cells
				if (Math.Abs(width) < 2 || Math.Abs(height) < 2)
				{
					MessageBox.Show(this, "GRAMM sub-domain is too small - create a larger frame", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
				//MessageBox.Show(this, width.ToString() + "/" + height.ToString());
				
				MessageWindow message = new MessageWindow();

                //open file dialog for defining new GRAMM project folder for exporting the sub-domain
                FolderBrowserDialog dialog = new FolderBrowserDialog
                {
                    Description = "Set GRAMM project directory"
                };
                string currentDir = Environment.CurrentDirectory;
				dialog.SelectedPath = currentDir;
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					if (!Directory.Exists(dialog.SelectedPath))
					{
						MessageBox.Show(this, "Can't find the project folder", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					string projectname = dialog.SelectedPath;

					//create subdirectories
					try
					{
						string newPath;
						newPath = System.IO.Path.Combine(projectname, "Computation");
						System.IO.Directory.CreateDirectory(newPath);
						newPath = System.IO.Path.Combine(projectname, "Emissions");
						System.IO.Directory.CreateDirectory(newPath);
						newPath = System.IO.Path.Combine(projectname, "Maps");
						System.IO.Directory.CreateDirectory(newPath);
						newPath = System.IO.Path.Combine(projectname, "Settings");
						System.IO.Directory.CreateDirectory(newPath);
						newPath = System.IO.Path.Combine(projectname, "Metfiles");
						System.IO.Directory.CreateDirectory(newPath);
						Thread.Sleep(250);
					}
					catch
					{
						MessageBox.Show(this, "Can't open this project. Are the folders write protected?", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					//copy and create all necessary files for a GRAMM project
					message.Show();
					message.listBox1.Items.Add("Starting GRAMM sub-domain export");
					message.Refresh();

					//generate pointer for location of new wind field files
					string GRAMMwindfield = Path.Combine(projectname, "Computation") + Path.DirectorySeparatorChar;
					
					try
					{
						using (StreamWriter GRAMMwrite = new StreamWriter(Path.Combine(projectname, @"Computation","windfeld.txt")))
						{
							GRAMMwrite.WriteLine(GRAMMwindfield);
							#if __MonoCS__
							GRAMMwrite.WriteLine(GRAMMwindfield);
							#endif
						}
					}
					catch(Exception ex)
					{
						MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
						return;
					}
					
					string GRAMMoriginal = string.Empty;
					//generate pointer for location of original GRAMM wind-field files
					try
					{
						using (StreamReader GRAMMori = new StreamReader(Path.Combine(Gral.Main.ProjectName, @"Computation","windfeld.txt")))
						{
							GRAMMoriginal = GRAMMori.ReadLine();
							if(GRAMMoriginal.Length > 1 && GRAMMoriginal[GRAMMoriginal.Length -1] != Path.DirectorySeparatorChar)
							{
								GRAMMoriginal = GRAMMoriginal + Path.DirectorySeparatorChar;
							}
						}
					}
					catch(Exception ex)
					{
						MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
						return;
					}
					

					//create GRAMM.geb
					message.listBox1.Items.Add("Creating new GRAMM.geb");
					message.Refresh();
					string filename1 = Path.Combine("Computation", "GRAMM.geb");
					string newPath1 = Path.Combine(projectname, filename1);
					
					try
					{
						using (StreamWriter myWriter1 = File.CreateText(newPath1))
						{
							myWriter1.WriteLine(Convert.ToString(Convert.ToInt32((subGRAMMeast - subGRAMMwest) / MainForm.GRAMMHorGridSize)) + "                !number of cells in x-direction");
							myWriter1.WriteLine(Convert.ToString(Convert.ToInt32((subGRAMMnorth - subGRAMMsouth) / MainForm.GRAMMHorGridSize)) + "                !number of cells in y-direction");
							myWriter1.WriteLine(Convert.ToString(MainForm.numericUpDown16.Value) + "               !number of cells in z-direction");
							myWriter1.WriteLine(subGRAMMwest.ToString("0") + "                !West border of GRAMM model domain [m]");
							myWriter1.WriteLine(subGRAMMeast.ToString("0") + "                !East border of GRAMM model domain [m]");
							myWriter1.WriteLine(subGRAMMsouth.ToString("0") + "                !South border of GRAMM model domain [m]");
							myWriter1.WriteLine(subGRAMMnorth.ToString("0") + "                !North border of GRAMM model domain [m]");
						}
					}
					catch(Exception ex)
					{
						MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
						return;
					}

					//domain size
					int subNX = Convert.ToInt32((subGRAMMeast - subGRAMMwest) / MainForm.GRAMMHorGridSize);
					int subNY = Convert.ToInt32((subGRAMMnorth - subGRAMMsouth) / MainForm.GRAMMHorGridSize);
					int subNZ = Convert.ToInt32(MainForm.numericUpDown16.Value);

					//copy meteopgt.all, mettimeseries.dat, and IIN.dat
					message.listBox1.Items.Add("Copying files meteopgt.all, mettimeseries.dat, and IIN.dat");
					message.Refresh();
					try
					{
						File.Copy(Path.Combine(Gral.Main.ProjectName, @"Computation","meteopgt.all"), Path.Combine(projectname, @"Computation","meteopgt.all"), true);
						File.Copy(Path.Combine(Gral.Main.ProjectName, @"Computation","mettimeseries.dat"), Path.Combine(projectname, @"Computation","mettimeseries.dat"), true);
						File.Copy(Path.Combine(Gral.Main.ProjectName, @"Computation","IIN.dat"), Path.Combine(projectname, @"Computation","IIN.dat"), true);
					}
					catch
					{
						MessageBox.Show(this, "Unable to copy one of the files 'meteopgt.all', 'mettimeseries.dat', or 'IIN.dat'");
					}

					// copy file GRAMMin.dat
					message.listBox1.Items.Add("Copying GRAMMin.dat");
					message.Refresh();
					
					try
					{
						File.Copy(Path.Combine(Gral.Main.ProjectName, @"Computation","GRAMMin.dat"), Path.Combine(projectname, @"Computation","GRAMMin.dat"), true);
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}

                    //create new Topograhpy.txt file
                    try
                    {
                        string dummy1 = String.Empty;
                        string dummy2 = String.Empty;
                        string dummy3 = String.Empty;
                        
                        using (StreamReader GRAMMtopoori = new StreamReader(Path.Combine(Gral.Main.ProjectName, @"Settings", "Topography.txt")))
                        {
                            dummy1 = GRAMMtopoori.ReadLine();
                            dummy2 = GRAMMtopoori.ReadLine();
                            dummy3 = GRAMMtopoori.ReadLine();
                        }
                        
                        using (StreamWriter mywriter = new StreamWriter(Path.Combine(projectname, @"Settings", "Topography.txt")))
                        {
                            mywriter.WriteLine(GRAMMwindfield + @"ggeom.asc");
                            mywriter.WriteLine(dummy2);
                            mywriter.WriteLine(dummy3);
                        }
					}
					catch(Exception ex)
					{
						MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
						return;
					}

					//create new landuse.txt file
					try
					{
						using (StreamWriter mywriter = new StreamWriter(Path.Combine(projectname, @"Settings","Landuse.txt")))
						{
							mywriter.WriteLine(GRAMMwindfield + @"landuse.asc");
						}
						
						File.Copy(Path.Combine(Gral.Main.ProjectName,  "Settings","Meteorology.txt"), Path.Combine(projectname, "Settings","Meteorology.txt"), true);
					}
					catch{}
					
					//read original ggeom.asc
					int subimin = 0;
					int subjmin = 0;
					int NX = 0;
					int NY = 0;
					int NZ = 0;
					try
					{
						message.listBox1.Items.Add("Reading ggeom.asc");
						message.Refresh();
                        GGeomFileIO ggeom = new GGeomFileIO
                        {
                            PathWindfield = GRAMMoriginal
                        };
                        ggeom.ReadGGeomAsc(-1);
						NX = ggeom.NX;
						NY = ggeom.NY;
						NZ = ggeom.NZ;
						

						//write new ggeom.asc for sub-domain
						ggeom.ProjectName = projectname;
						ggeom.NX = subNX;
						ggeom.NY = subNY;
						ggeom.NZ = subNZ;

						//defining local variables for sub-domain extensions
						double[,] subAH = new double[subNX + 1, subNY + 1];
						double[] subX = new double[subNX + 2];
						double[] subY = new double[subNY + 2];
						double[, ,] subVOL = new double[subNX + 1, subNY + 1, subNZ + 1];
						double[, ,] subAREAX = new double[subNX + 2, subNY + 1, subNZ + 1];
						double[, ,] subAREAY = new double[subNX + 1, subNY + 2, subNZ + 1];
						double[, ,] subAREAZ = new double[subNX + 1, subNY + 1, subNZ + 2];
						double[, ,] subAREAZX = new double[subNX + 1, subNY + 1, subNZ + 2];
						double[, ,] subAREAZY = new double[subNX + 1, subNY + 1, subNZ + 2];
						double[, ,] subAHE = new double[subNX + 2, subNY + 2, subNZ + 2];
						double[, ,] subZSP = new double[subNX + 1, subNY + 1, subNZ + 1];
						double[] subZAX = new double[subNX + 1];
						double[] subZAY = new double[subNY + 1];
						int subIKOOA = Convert.ToInt32(subGRAMMwest);
						int subJKOOA = Convert.ToInt32(subGRAMMsouth);

						//get indices and values for sub-domain
						subimin = Convert.ToInt32((subGRAMMwest - ggeom.IKOOA) / MainForm.GRAMMHorGridSize) + 1;
						subjmin = Convert.ToInt32((subGRAMMsouth - ggeom.JKOOA) / MainForm.GRAMMHorGridSize) + 1;

						int n = subimin;
						for (int i = 2; i < subNX + 2; i++)
						{
							subX[i] = subX[i - 1] + ggeom.DDX[n];
							n++;
						}
						
						n = subimin;
						for (int i = 1; i < subNX + 1; i++)
						{
							subZAX[i] = ggeom.ZAX[n];
							n++;
						}

						n = subjmin;
						for (int i = 2; i < subNY + 2; i++)
						{
							subY[i] = subY[i - 1] + ggeom.DDY[n];
							n++;
						}

						n = subjmin;
						for (int i = 1; i < subNY + 1; i++)
						{
							subZAY[i] = ggeom.ZAY[n];
							n++;
						}

						int ni = subimin;
						int nj = subjmin;
						for (int i = 1; i < subNX + 1; i++)
						{
							for (int j = 1; j < subNY + 1; j++)
							{
								subAH[i, j] = ggeom.AH[ni, nj];
								nj++;
							}
							ni++;
							nj = subjmin;
						}

						ni = subimin;
						nj = subjmin;
						for (int i = 1; i < subNX + 1; i++)
						{
							for (int j = 1; j < subNY + 1; j++)
							{
								for (int k = 1; k < ggeom.NZ + 1; k++)
								{
									subVOL[i, j, k] = ggeom.VOL[ni, nj, k];
									subZSP[i, j, k] = ggeom.ZSP[ni, nj, k];
								}
								nj++;
							}
							ni++;
							nj = subjmin;
						}

						ni = subimin;
						nj = subjmin;
						for (int i = 1; i < subNX + 2; i++)
						{
							for (int j = 1; j < subNY + 1; j++)
							{
								for (int k = 1; k < ggeom.NZ + 1; k++)
								{
									subAREAX[i, j, k] = ggeom.AREAX[ni, nj, k];
								}
								nj++;
							}
							ni++;
							nj = subjmin;
						}

						ni = subimin;
						nj = subjmin;
						for (int i = 1; i < subNX + 1; i++)
						{
							for (int j = 1; j < subNY + 2; j++)
							{
								for (int k = 1; k < ggeom.NZ + 1; k++)
								{
									subAREAY[i, j, k] = ggeom.AREAY[ni, nj, k];
								}
								nj++;
							}
							ni++;
							nj = subjmin;
						}

						ni = subimin;
						nj = subjmin;
						for (int i = 1; i < subNX + 1; i++)
						{
							for (int j = 1; j < subNY + 1; j++)
							{
								for (int k = 1; k < ggeom.NZ + 2; k++)
								{
									subAREAZX[i, j, k] = ggeom.AREAZX[ni, nj, k];
									subAREAZY[i, j, k] = ggeom.AREAZY[ni, nj, k];
									subAREAZ[i, j, k] = ggeom.AREAZ[ni, nj, k];
								}
								nj++;
							}
							ni++;
							nj = subjmin;
						}

						ni = subimin;
						nj = subjmin;
						for (int i = 1; i < subNX + 2; i++)
						{
							for (int j = 1; j < subNY + 2; j++)
							{
								for (int k = 1; k < ggeom.NZ + 2; k++)
								{
									subAHE[i, j, k] = ggeom.AHE[ni, nj, k];
								}
								nj++;
							}
							ni++;
							nj = subjmin;
						}

						message.listBox1.Items.Add("Creating new ggeom.asc");
						message.Refresh();
						ggeom.AH = subAH;
						ggeom.X = subX;
						ggeom.Y = subY;
						ggeom.VOL = subVOL;
						ggeom.AREAX = subAREAX;
						ggeom.AREAY = subAREAY;
						ggeom.AREAZ = subAREAZ;
						ggeom.AREAZX = subAREAZX;
						ggeom.AREAZY = subAREAZY;
						ggeom.ZSP = subZSP;
						ggeom.ZAX = subZAX;
						ggeom.ZAY = subZAY;
						ggeom.IKOOA = subIKOOA;
						ggeom.JKOOA = subJKOOA;
						ggeom.AHE = subAHE;
						ggeom.PathWindfield = Path.Combine(projectname, @"Computation","ggeom.asc");
						ggeom.ProjectName = projectname;
						ggeom.NODATA = -9999;

						ggeom.WriteGGeomFile();
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}

					//read original landuse.asc
					try
					{
						message.listBox1.Items.Add("Reading landuse.asc");
						message.Refresh();
                        Landuse landuse = new Landuse
                        {
                            ProjectName = GRAMMoriginal,
                            NX = NX,
                            NY = NY
                        };
                        landuse.ReadLanduseFile();

						//write new landuse.asc for sub-domain
						landuse.NX = subNX;
						landuse.NY = subNY;
						landuse.DX = MainForm.GRAMMHorGridSize;

						//defining local variables for sub-domain extensions
						double[,] subRHOB = new double[subNX + 1, subNY + 1];
						double[,] subALAMBDA = new double[subNX + 1, subNY + 1];
						double[,] subZ0 = new double[subNX + 1, subNY + 1];
						double[,] subFW = new double[subNX + 1, subNY + 1];
						double[,] subEPSG = new double[subNX + 1, subNY + 1];
						double[,] subALBEDO = new double[subNX + 1, subNY + 1];
						int subIKOOA = Convert.ToInt32(subGRAMMwest);
						int subJKOOA = Convert.ToInt32(subGRAMMsouth);

						int ni = subimin;
						int nj = subjmin;
						for (int i = 1; i < subNX + 1; i++)
						{
							for (int j = 1; j < subNY + 1; j++)
							{
								subRHOB[i, j] = landuse.RHOB[ni, nj];
								subALAMBDA[i, j] = landuse.ALAMBDA[ni, nj];
								subZ0[i, j] = landuse.Z0[ni, nj];
								subFW[i, j] = landuse.FW[ni, nj];
								subEPSG[i, j] = landuse.EPSG[ni, nj];
								subALBEDO[i, j] = landuse.ALBEDO[ni, nj];
								nj++;
							}
							ni++;
							nj = subjmin;
						}

						message.listBox1.Items.Add("Creating new landuse.asc");
						message.Refresh();
						landuse.RHOB = subRHOB;
						landuse.ALAMBDA = subALAMBDA;
						landuse.Z0 = subZ0;
						landuse.FW = subFW;
						landuse.EPSG = subEPSG;
						landuse.ALBEDO = subALBEDO;
						landuse.IKOOA = subIKOOA;
						landuse.JKOOA = subJKOOA;
						landuse.NODATA = -9999;
						landuse.ProjectName = projectname;

						landuse.ExportLanduse();
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}

					//read original .wnd-files
					try
					{
                        message.listBox1.Items.Add("Reading wind data");
                        //delete existing wnd and scl files in the target directory, if existent
                        string newPath = Path.Combine(projectname, "Computation" + Path.DirectorySeparatorChar);
						DirectoryInfo di = new DirectoryInfo(newPath);
						FileInfo[] files_wnd = di.GetFiles("*.wnd");
						if (files_wnd.Length > 0)
						{
                            using (FileDeleteMessage fdm = new FileDeleteMessage
                            {
                                deletegramm = true
                            })
                            {
                                if (files_wnd.Length > 0)
                                {
                                    fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "*.wnd");
                                }

                                if (fdm.ShowDialog() == DialogResult.OK)
                                {
                                    for (int i = 0; i < files_wnd.Length; i++)
                                    {
                                        files_wnd[i].Delete();
                                    }
                                    //delete *.scl files
                                    files_wnd = di.GetFiles("*.scl");
                                    if (files_wnd.Length > 0)
                                    {
                                        for (int i = 0; i < files_wnd.Length; i++)
                                        {
                                            files_wnd[i].Delete();
                                        }
                                    }
                                    //delete *.obl files
                                    files_wnd = di.GetFiles("*.obl");
                                    if (files_wnd.Length > 0)
                                    {
                                        for (int i = 0; i < files_wnd.Length; i++)
                                        {
                                            files_wnd[i].Delete();
                                        }
                                    }
                                    //delete *.ust files
                                    files_wnd = di.GetFiles("*.ust");
                                    if (files_wnd.Length > 0)
                                    {
                                        for (int i = 0; i < files_wnd.Length; i++)
                                        {
                                            files_wnd[i].Delete();
                                        }
                                    }
                                    //delete steady_state.txt files
                                    files_wnd = di.GetFiles("?????_steady_state.txt");
                                    if (files_wnd.Length > 0)
                                    {
                                        for (int i = 0; i < files_wnd.Length; i++)
                                        {
                                            files_wnd[i].Delete();
                                        }
                                    }
                                }
                            }
					    }
                        message.listBox1.Items.Add("..");
                        GralBackgroundworkers.CellNumbers cellOrig = new GralBackgroundworkers.CellNumbers
                        {
                            NX = NX,
                            NY = NY,
                            NZ = NZ
                        };
                        message.listBox1.Items.Add("..");
                        GralBackgroundworkers.CellNumbers cellSub = new GralBackgroundworkers.CellNumbers
                        {
                            NX = subNX,
                            NY = subNY,
                            NZ = subNZ
                        };
                        message.listBox1.Items.Add("..");
                        GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData
                        {
                            Projectname = projectname,
                            Path_GRAMMwindfield = GRAMMoriginal,
                            XDomain = subimin,
                            YDomain = subjmin,
                            Horgridsize = MainForm.GRAMMHorGridSize,
                            UserText = @"Copying GRAMM wind fields from  " +
                                GRAMMoriginal + "  to  " + Path.Combine(projectname, "Computation")  +"  " + 
                                "The process may take some minutes",
                            Caption = "Export GRAMM Wind Fields ",
                            GRAMMCells = cellOrig,
                            GRAMMSubCells = cellSub,
                            Rechenart = 60 
                        };
                        message.listBox1.Items.Add("..");
                        GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart =
                            new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                            {
                                Text = DataCollection.Caption
                            };
                        message.listBox1.Items.Add("..");
                        BackgroundStart.Show();
                    }
					catch (Exception ex)
					{
						MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}

					try
					{
						if (File.Exists(Path.Combine(Gral.Main.ProjectName, @"Computation","Logfile_GRAMM.txt")))
						{
							File.Copy(Path.Combine(Gral.Main.ProjectName, @"Computation","Logfile_GRAMM.txt"), Path.Combine(projectname, @"Computation","Logfile_GRAMM.txt"), true);
						}
						
						using (StreamWriter mywriter = File.AppendText(Path.Combine(projectname, @"Computation","Logfile_GRAMM.txt"))) // Using closes mywriter and does error-handling
						{
							mywriter.WriteLine(" ");
							mywriter.WriteLine("".PadRight(120,'-'));
							mywriter.WriteLine(" ");
							mywriter.WriteLine("This is an exported wind field from: " +  Gral.Main.ProjectName);
							mywriter.WriteLine(" ");
							mywriter.WriteLine("".PadRight(120,'-'));
						}
					}
					catch
					{
						
					}
					
					dialog.Dispose();
				}

                message.Close();
                message.Dispose();

                Picturebox1_Paint();
			}
			catch
			{
				GRAMMDomain = new Rectangle();
			}

		}
	}
}

