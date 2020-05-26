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
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace GralIO
{
    /////////////////////////////////////
    /// routine to generate GRAMM grid
    /////////////////////////////////////
    
    /// <summary>
    /// This class creates the GRAMM grid
    /// </summary>
    public class CreateGrammGrid
	{
        private bool _writeCompressedFile = false;
		public bool WriteCompressedFile {set {_writeCompressedFile = value;} }
		public int SmoothBorderCellNr;
		public string ProjectName;
		
		/// <summary>
    	/// Generates the ggeom.asc file
    	/// </summary> 
		public bool GenerateGgeomFile()
		{
            float[][] ADH;

            GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Reading GRAMM topography" + "0 %");
			wait.Show();
			
			//User defined column seperator and decimal seperator
			string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
			char[] splitchar = null;
			if (decsep == ",")
            {
                splitchar = new char[] { ' ', '\t', ';' };
            }
            else
            {
                splitchar = new char[] { ' ', '\t', ',', ';' };
            }

            GralMessage.MessageWindow mw = new GralMessage.MessageWindow();

			//reading field sizes from file GRAMM.geb
			StreamReader reader=new StreamReader(Path.Combine(ProjectName, @"Computation", "GRAMM.geb"));
			string [] text=new string[5];
			text=reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			int NX=Convert.ToInt32(text[0]);  //number of horizontal cells in x direction
			text=reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			int NY=Convert.ToInt32(text[0]);  //number of horizontal cells in y direction
			text=reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			int NZ=Convert.ToInt32(text[0]);  //number of vertical cells in z direction
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			double xsimin=Convert.ToDouble(text[0].Replace(".",decsep)); //western boarder
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			double xsimax=Convert.ToDouble(text[0].Replace(".",decsep)); //eastern boarder
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			double etamin=Convert.ToDouble(text[0].Replace(".",decsep)); //southern boarder
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			double etamax=Convert.ToDouble(text[0].Replace(".",decsep)); //northern boarder
			reader.Close();
            reader.Dispose();

			//reading the filename, the height of the lowest cell height, and the vertical streching factor
			reader=new StreamReader(Path.Combine(ProjectName, @"Computation", "geom.in"));
			string filename=reader.ReadLine();  //filename of the chosen topography
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			double DDZ=Convert.ToDouble(text[0].Replace(".",decsep)); //cell height of the first level
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			double ADZ=Convert.ToDouble(text[0].Replace(".",decsep)); //vertical streching factor
			reader.Close();
            reader.Dispose();

			double winkel=0;  //angle between domain orientation and north
			int NX1=NX+1;
			int NY1=NY+1;
			int NZ1=NZ+1;
			int NX2=NX+2;
			int NY2=NY+2;
			int NZ2=NZ+2;
			int NXY=NX*NY;  //number of cells in a horizontal layer
			int NXYZ=NX*NY*NZ; //total number of cells
			int NNNS=4*NXY;

			double [] XKO=new double[NX2];  //x-coordinates of cells
			double [] YKO=new double[NY2];  //y-coordinates of cells
			double [] ZKO=new double[NZ2];  //z-coordinates of cells
			double [,] AH= new double[NX1,NY1]; //height of the topography at each cell point
			double [,,] VOL= new double[NX1,NY1,NZ1]; //volume of each cell
			double [,,] AREAX= new double[NX2,NY1,NZ1]; //western cell face
			double [,,] AREAY= new double[NX1,NY2,NZ1]; //eastern cell face
			double [,,] AREAZ= new double[NX1,NY1,NZ2]; //bottom cell face
			double [,,] AREAZX= new double[NX1,NY1,NZ2]; //x-projection of bottom cell face
			double [,,] AREAZY= new double[NX1,NY1,NZ2]; //y-projection of bottom cell face
			double [,,] AHE= new double[NX2,NY2,NZ2]; //heights of the corner points of a cell
			double [,,] ZSP= new double[NX1,NY1,NZ1]; //height of the center of cells
			double [] DDX= new double[NX1]; //cell size in x-direction
			double [] DDY= new double[NY1]; //cell size in y-direction
			double [] ZAX= new double[NX1]; //x-coordinate of cell center
			double [] ZAY= new double[NY1]; //y-coordinate of cell center
			double [] X= new double[NX2]; //x-coordinate of cell faces
			double [] Y= new double[NY2]; //y-coordinate of cell faces
			double [] DX= new double[NX1]; //cell size for each cell in x-direction
			double [] DY= new double[NY1]; //cell size for each cell in y-direction
			double [] Z= new double[NZ2]; //absolute cell height for each cell in z-direction
			double [] DW= new double[NZ2]; //cell height for each cell in z-direction
			double [,,] AREAXYZ= new double[NX1,NY1,NZ1]; //area of intersecting face between two half-cells
			double [,,] AREA= new double[NX1,NY1,NZ1]; //area of bottom face
			double [,,] AXZP= new double[NX1,NY1,NZ1];
			double [,,] AXXYZP= new double[NX1,NY1,NZ1];
			double [,,] AXZM= new double[NX1,NY1,NZ1];
			double [,,] AXXYZM= new double[NX1,NY1,NZ1];
			double [,,] AXX= new double[NX1,NY1,NZ1];
			double [,,] AYZP= new double[NX1,NY1,NZ1];
			double [,,] AYXYZP= new double[NX1,NY1,NZ1];
			double [,,] AYZM= new double[NX1,NY1,NZ1];
			double [,,] AYXYZM= new double[NX1,NY1,NZ1];
			double [,,] AYY= new double[NX1,NY1,NZ1];
			double [,,] AZXP= new double[NX1,NY1,NZ1];
			double [,,] AZYP= new double[NX1,NY1,NZ1];
			double [,,] AZXYZP= new double[NX1,NY1,NZ1];
			double [,,] AZXM= new double[NX1,NY1,NZ1];
			double [,,] AZYM= new double[NX1,NY1,NZ1];
			double [,,] AZXYZM= new double[NX1,NY1,NZ1];
			double [,,] AZZ= new double[NX1,NY1,NZ1];
			double [,,] AXYZXP= new double[NX1,NY1,NZ1];
			double [,,] AXYZYP= new double[NX1,NY1,NZ1];
			double [,,] AXYZZP= new double[NX1,NY1,NZ1];
			double [,,] AXYZXM= new double[NX1,NY1,NZ1];
			double [,,] AXYZYM= new double[NX1,NY1,NZ1];
			double [,,] AXYZZM= new double[NX1,NY1,NZ1];
			double [,,] AXYZXYZ= new double[NX1,NY1,NZ1];
			double [,] LAND= new double[NX2,NY2];
			double NODDATA=0;

			//reading topography file
			reader=new StreamReader(filename);
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			int NCOL = Convert.ToInt32(text[1]);  //number of cells in x-direction of topography file
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			int NROW = Convert.ToInt32(text[1]);  //number of cells in y-direction of topography file
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			double ILIUN = Convert.ToDouble(text[1].Replace(".", decsep));  //western boarder of topography file
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			double JLIUN = Convert.ToDouble(text[1].Replace(".", decsep));  //southern boarder of topography file
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			double ICSIZE = Convert.ToDouble(text[1].Replace(".", decsep));  //grid size
			text = reader.ReadLine().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
			NODDATA = Convert.ToDouble(text[1].Replace(".", decsep));  //no-data value

			int IMODI=Convert.ToInt32((xsimax-xsimin)/NX);
			int I1=Convert.ToInt32(xsimin);
			int J1=Convert.ToInt32(etamin);
			int IKOOA=I1;
			int JKOOA=J1;
			int ILANG=IMODI*NX;
			int JLANG=IMODI*NY;
			if(Convert.ToInt32(ILANG/IMODI)>NX)
			{
				MessageBox.Show("Number of cells in x-direction to small", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				wait.Close();
				wait.Dispose();
				return false;
			}
			if(Convert.ToInt32(JLANG/IMODI)>NY)
			{
				MessageBox.Show("Number of cells in y-direction to small", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				wait.Close();
				wait.Dispose();
				return false;
			}

			//computation of the corner points of the domain
			double sinus=Math.Sin(-winkel*Math.PI/180);
			double cosinus=Math.Cos(-winkel*Math.PI/180);
			int I2=Convert.ToInt32(I1-JLANG*sinus);
			int J2=Convert.ToInt32(J1+JLANG*cosinus);
			int I3=Convert.ToInt32(I2+ILANG*cosinus);
			int J3=Convert.ToInt32(J2+ILANG*sinus);
			int I4=Convert.ToInt32(I1+ILANG*cosinus);
			int J4=Convert.ToInt32(J1+ILANG*sinus);

			//check, if domain fits within selected topography file
			if(Math.Min(Math.Min(I1,I2),Math.Min(I3,I4))<ILIUN)
			{
				MessageBox.Show("Selected area is to far in the west", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				wait.Close();
				wait.Dispose();
				return false;
			}
			if(Math.Max(Math.Max(I1,I2),Math.Max(I3,I4))>ILIUN+ICSIZE*NCOL)
			{
				MessageBox.Show("Selected area is to far in the east", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				wait.Close();
				wait.Dispose();
				return false;
			}
			if(Math.Min(Math.Min(J1,J2),Math.Min(J3,J4))<JLIUN)
			{
				MessageBox.Show("Selected area is to far in the south", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				wait.Close();
				wait.Dispose();
				return false;
			}
			if(Math.Max(Math.Max(J1,J2),Math.Max(J3,J4))>JLIUN+ICSIZE*NROW)
			{
				MessageBox.Show("Selected area is to far in the north", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				wait.Close();
				wait.Dispose();
				return false;
			}

			//reading topography
			double AHMIN=10000;
			double AHMAX = -10000;
			double AHMIN_BORDER = 10000;
			text = new string[NCOL];
			bool sizeOK = true;
			try
			{
                 ADH = Landuse.CreateArray<float[]>(NCOL + 2, () => new float[NROW + 2]);
			}
			catch
			{
				reader.Close();
                reader.Dispose();
				sizeOK = false;
				MessageBox.Show("Topography file is too large. Exeeding available memory space of this computer", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				wait.Close();
				wait.Dispose();
				return false;
			}
			if (sizeOK == true)
			{
                //read topography data only for the area of interest
                char[] splitChar = new char[] { ' ', '\t', ';' };
                for (int i = 1; i < NROW + 1; i++)
				{
					Application.DoEvents(); // Kuntner
					text = reader.ReadLine().Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
					//for (int j = 1; j < NCOL + 1; j++)
					Parallel.For(1, NCOL + 1, j =>
					{
						ADH[j][i] = Convert.ToSingle(text[j - 1], CultureInfo.InvariantCulture);
					});
					
					if (i % 40 == 0)
                    {
                        wait.Text =  "Reading GRAMM topography " + ((int)((float)i / (NROW + 2) * 100F)).ToString() +"%";
                    }
                }
				reader.Close();
                reader.Dispose();

				//computation of cell heights
				for (int NJ = 1; NJ < NY + 2; NJ++)
				{
					Application.DoEvents(); // Kuntner
					for (int NI = 1; NI < NX + 2; NI++)
					{
						//non-transformed coordinates
						double X1 = (NI - 1) * IMODI;
						double Y1 = (NJ - 1) * IMODI;

						//transformed coordinates
						double X2 = X1 * cosinus - Y1 * sinus + I1;
						double Y2 = X1 * sinus + Y1 * cosinus + J1;
						
                        //computation of indices for the Topography file date
						int IP = Convert.ToInt32(((X2 - ILIUN) / ICSIZE) + 1);
						int JP = -Convert.ToInt32(((Y2 - JLIUN) / ICSIZE) - NROW);
						
                        //computation of coordinates
						int JKOO = Convert.ToInt32(JLIUN + (NROW - JP + 1) * ICSIZE);
						int IKOO = Convert.ToInt32(ILIUN + (IP - 1) * ICSIZE);
						double gewges = 0;
						double gew = 0;
						double wert = 0;

						//computation of missing mean value from the 4 corners
						for (int IPP = IP; IPP < IP + 2; IPP++)
						{
							for (int JPP = JP; JPP < JP + 2; JPP++)
							{
								if (ADH[IPP][JPP] == NODDATA)
								{
									gewges = 0;
									gew = 0;
									wert = 0;
									//seeking north/south
									for (int NS = JPP + 1; NS < NROW + 1; NS++)
									{
										if (ADH[IPP][NS] != NODDATA)
										{
											gew = Math.Abs(1 / ((NS - JPP) * Convert.ToDouble(ICSIZE)));
											gewges = gewges + gew;
											wert = ADH[IPP][NS] * gew + wert;
											break;
										}
									}
									//seeking north/south
									for (int NS = JPP - 1; NS > 0; NS--)
									{
										if (ADH[IPP][NS] != NODDATA)
										{
											gew = Math.Abs(1 / ((NS - JPP) * Convert.ToDouble(ICSIZE)));
											gewges = gewges + gew;
											wert = ADH[IPP][NS] * gew + wert;
											break;
										}
									}
									//seeking west/east
									for (int NS = IPP + 1; NS < NCOL + 1; NS++)
									{
										if (ADH[NS][JPP] != NODDATA)
										{
											gew = Math.Abs(1 / ((NS - IPP) * Convert.ToDouble(ICSIZE)));
											gewges = gewges + gew;
											wert = ADH[NS][JPP] * gew + wert;
											break;
										}
									}
									//seeking west/east
									for (int NS = IPP - 1; NS > 0; NS--)
									{
										if (ADH[NS][JPP] != NODDATA)
										{
											gew = Math.Abs(1 / ((NS - IPP) * Convert.ToDouble(ICSIZE)));
											gewges = gewges + gew;
											wert = ADH[NS][JPP] * gew + wert;
											break;
										}
									}
									//calculating total weight
									ADH[IPP][JPP] = (float)Math.Max(wert / gewges, 0);
								}
							}
						}
						double H12 = ADH[IP][JP] + (ADH[IP][JP + 1] - ADH[IP][JP]) / ICSIZE * (Y2 - JKOO);
						double H34 = ADH[IP + 1][JP] + (ADH[IP + 1][JP + 1] - ADH[IP + 1][JP]) / ICSIZE * (Y2 - JKOO);
						AHE[NI, NJ, 1] = Math.Max(H12 + (H34 - H12) / ICSIZE * (X2 - IKOO), 0);

						//minimum of terrain data
						if (ADH[IP][JP] < AHMIN)
                        {
                            AHMIN = ADH[IP][JP];
                        }

                        //minimum elevation at the border
                        if ((ADH[IP][JP] < AHMIN_BORDER) && ((NI == 1) || (NJ == 1) || (NI == NX + 1) || (NJ == NY + 1)))
                        {
                            AHMIN_BORDER = ADH[IP][JP];
                        }
                    }
				}

				//flatten topography towards domain borders
				/*
                for(int I=2;I<NX+1;I++)
                {
                    AHE[I,3,1]    = AHE[I,4,1];
                    AHE[I,2,1]    = AHE[I,3,1];
                    AHE[I,1,1]    = AHE[I,2,1];
                    AHE[I,NY-1,1] = AHE[I,NY-2,1];
                    AHE[I,NY,1]   = AHE[I,NY-1,1];
                    AHE[I,NY+1,1] = AHE[I,NY,1];
                }
                for(int J=1;J<NY+2;J++)
                {
                    AHE[3,J,1]    = AHE[4,J,1];
                    AHE[2,J,1]    = AHE[3,J,1];
                    AHE[1,J,1]    = AHE[2,J,1];
                    AHE[NX-1,J,1] = AHE[NX-2,J,1];
                    AHE[NX,J,1]   = AHE[NX-1,J,1];
                    AHE[NX+1,J,1] = AHE[NX,J,1];
                }
				 */


				//coordinates of cells in x-direction
				int NK = NZ;
				for (int I = 1; I < NX + 1; I++)
                {
                    DDX[I] = Convert.ToDouble(IMODI);
                }

                for (int J = 1; J < NY + 1; J++)
                {
                    DDY[J] = Convert.ToDouble(IMODI);
                }

                X[1] = 0;
				for (int I = 2; I < NX + 2; I++)
                {
                    X[I] = X[I - 1] + DDX[I - 1];
                }

                for (int I = 1; I < NX; I++)
                {
                    ZAX[I] = (X[I + 1] + DDX[I + 1] / 2) - (X[I] + DDX[I] / 2);
                }

                //flatten topography towards domain borders
                double abstand = 0;
				double totaldistance = 0; //horizontal distance between model boundary and the last cell, which is smoothed, yet
				for (int smooth = Math.Min(SmoothBorderCellNr, NY); smooth > 0; smooth--)
				{
					totaldistance = totaldistance + DDY[smooth];
				}

				for (int I = 1; I < NX + 2; I++)
				{
					Application.DoEvents(); // Kuntner
					abstand = 0;
					for (int smooth = Math.Min(SmoothBorderCellNr, NY); smooth > 0; smooth--)
					{
						//AHE[I, smooth, 1] = AHE[I, smooth + 1, 1] - (AHE[I, smooth + 1, 1] - AHMIN) / 4;

						//lineare Interpolation zum Rand hin
						abstand = abstand + DDY[smooth];
						AHE[I, smooth, 1] = AHE[I, SmoothBorderCellNr + 1, 1] - (AHE[I, SmoothBorderCellNr + 1, 1] - AHMIN_BORDER) * abstand / totaldistance;
					}
					/*
                    AHE[I, 6, 1] = AHE[I, 7, 1] - (AHE[I, 7, 1] - AHMIN) / 4;
                    AHE[I, 5, 1] = AHE[I, 6, 1] - (AHE[I, 7, 1] - AHMIN) / 4;
                    AHE[I, 4, 1] = AHE[I, 5, 1] - (AHE[I, 7, 1] - AHMIN) / 4;
                    AHE[I, 3, 1] = AHMIN;
                    AHE[I, 2, 1] = AHMIN;
                    AHE[I, 1, 1] = AHMIN;
					 */

					abstand = 0;
					for (int smooth = Math.Min(SmoothBorderCellNr, NY); smooth > 0; smooth--)
					{
						//AHE[I, NY - smooth + 2, 1] = AHE[I, NY - smooth + 1, 1] - (AHE[I, NY - smooth + 1, 1] - AHMIN) / 4;

						//lineare Interpolation zum Rand hin
						abstand = abstand + DDY[NY - smooth + 1];
						AHE[I, NY - smooth + 2, 1] = AHE[I, NY - SmoothBorderCellNr + 1, 1] - (AHE[I, NY - SmoothBorderCellNr + 1, 1] - AHMIN_BORDER) * abstand / totaldistance;
					}

					/*
                    AHE[I, NY - 4, 1] = AHE[I, NY - 5, 1] - (AHE[I, NY - 5, 1] - AHMIN) / 4;
                    AHE[I, NY - 3, 1] = AHE[I, NY - 4, 1] - (AHE[I, NY - 5, 1] - AHMIN) / 4;
                    AHE[I, NY - 2, 1] = AHE[I, NY - 3, 1] - (AHE[I, NY - 5, 1] - AHMIN) / 4;
                    AHE[I, NY - 1, 1] = AHMIN;
                    AHE[I, NY, 1] = AHMIN;
                    AHE[I, NY + 1, 1] = AHMIN;
					 */
				}

				totaldistance = 0;
				for (int smooth = Math.Min(SmoothBorderCellNr, NX); smooth > 0; smooth--)
				{
					totaldistance = totaldistance + DDX[smooth];
				}

				for (int J = 1; J < NY + 2; J++)
				{
					Application.DoEvents(); // Kuntner
					abstand = 0;
					for (int smooth = Math.Min(SmoothBorderCellNr, NX); smooth > 0; smooth--)
					{
						//AHE[smooth, J, 1] = AHE[smooth + 1, J, 1] - (AHE[smooth + 1, J, 1] - AHMIN) / 4;

						//lineare Interpolation zum Rand hin
						abstand = abstand + DDX[smooth];
						AHE[smooth, J, 1] = AHE[SmoothBorderCellNr + 1, J, 1] - (AHE[SmoothBorderCellNr + 1, J, 1] - AHMIN_BORDER) * abstand / totaldistance;
					}

					/*
                    AHE[6, J, 1] = AHE[7, J, 1] - (AHE[7, J, 1] - AHMIN) / 4;
                    AHE[5, J, 1] = AHE[6, J, 1] - (AHE[7, J, 1] - AHMIN) / 4;
                    AHE[4, J, 1] = AHE[5, J, 1] - (AHE[7, J, 1] - AHMIN) / 4;
                    AHE[3, J, 1] = AHMIN;
                    AHE[2, J, 1] = AHMIN;
                    AHE[1, J, 1] = AHMIN;
					 */

					abstand = 0;
					for (int smooth = Math.Min(SmoothBorderCellNr, NX); smooth > 0; smooth--)
					{
						//AHE[NX - smooth + 2, J, 1] = AHE[NX - smooth + 1, J, 1] - (AHE[NX - smooth + 1, J, 1] - AHMIN) / 4;

						//lineare Interpolation zum Rand hin
						abstand = abstand + DDY[NY - smooth + 1];
						AHE[NX - smooth + 2, J, 1] = AHE[NX - SmoothBorderCellNr + 1, J, 1] - (AHE[NX - SmoothBorderCellNr + 1, J, 1] - AHMIN_BORDER) * abstand / totaldistance;
					}

					/*
                    AHE[NX - 4, J, 1] = AHE[NX - 5, J, 1] - (AHE[NX - 5, J, 1] - AHMIN) / 4;
                    AHE[NX - 3, J, 1] = AHE[NX - 4, J, 1] - (AHE[NX - 5, J, 1] - AHMIN) / 4;
                    AHE[NX - 2, J, 1] = AHE[NX - 3, J, 1] - (AHE[NX - 5, J, 1] - AHMIN) / 4;
                    AHE[NX - 1, J, 1] = AHMIN;
                    AHE[NX, J, 1] = AHMIN;
                    AHE[NX + 1, J, 1] = AHMIN;
					 */
				}


				//minimum and maximum elevations
				for (int J = 1; J < NY + 2; J++)
				{
					for (int I = 1; I < NX + 2; I++)
					{
						if (AHE[I, J, 1] < AHMIN)
                        {
                            AHMIN = AHE[I, J, 1];
                        }

                        if (AHE[I, J, 1] > AHMAX)
                        {
                            AHMAX = AHE[I, J, 1];
                        }
                    }
				}
				mw.listBox1.Items.Add("Minimum elevation: " + Convert.ToString(Math.Round(AHMIN, 0)));
				mw.listBox1.Items.Add("Maximum elevation: " + Convert.ToString(Math.Round(AHMAX, 0)));
				mw.Show();

				mw.listBox1.Items.Add("Number of cells in E-W direction: " + Convert.ToString(NX));
				mw.listBox1.Items.Add("Cell length in E-W direction: " + Convert.ToString(DDX[1]));
				mw.listBox1.Items.Add("E-W extents: " + Convert.ToString(Math.Round(X[NX + 1], 0)));
				mw.Refresh();

				//coordinates of cells in y-direction
				Y[1] = 0;
				for (int J = 2; J < NY + 2; J++)
                {
                    Y[J] = Y[J - 1] + DDY[J - 1];
                }

                for (int J = 1; J < NY; J++)
                {
                    ZAY[J] = (Y[J + 1] + DDY[J + 1] / 2) - (Y[J] + DDY[J] / 2);
                }

                mw.listBox1.Items.Add("Number of cells in S-N direction: " + Convert.ToString(NY));
				mw.listBox1.Items.Add("Cell length in S-N direction: " + Convert.ToString(DDY[1]));
				mw.listBox1.Items.Add("S-N extents: " + Convert.ToString(Math.Round(Y[NY + 1], 0)));
				mw.Refresh();

				//coordinates of cells in z-direction
				Z[1] = AHMIN;
				int K = 1;
				mw.listBox1.Items.Add("Z" + Convert.ToString(K) + "= " + Convert.ToString(Math.Round(Z[K], 1)) + " m");
				mw.Refresh();
				for (K = 2; K < NZ + 2; K++)
                {
                    Z[K] = Z[K - 1] + DDZ * Math.Pow(ADZ, K - 2);
                }

                for (K = 1; K < NZ + 1; K++)
				{
					DW[K] = Z[K + 1] - Z[K];
					mw.listBox1.Items.Add("Z" + Convert.ToString(K + 1) + "= " + Convert.ToString(Math.Round(Z[K + 1], 1)) + " m");
				}
				mw.Refresh();

				//top of model domain needs to be larger than 3 times the maximum elevation
				if ((Z[NZ + 1] - AHMIN) < (AHMAX - AHMIN) * 3)
				{
					MessageBox.Show("Height of the model domain is too low.\rIncrease vertical streching factor or\rIncrease the number of vertical grid points or\rIncrease the height of the first layer", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
					wait.Close();
					wait.Dispose();
					return false;
				}

				double DWMAX = Z[NZ + 1] - AHMIN;

				//computation of the heights of the cell corners
				for (int I = 1; I < NX + 2; I++)
                {
                    for (int J = 1; J < NY + 2; J++)
                    {
                        for (K = 2; K < NZ + 2; K++)
                        {
                            AHE[I, J, K] = AHE[I, J, K - 1] + DW[K - 1] / DWMAX * (Z[NZ + 1] - AHE[I, J, 1]);
                        }
                    }
                }

                //computation of areas and volumes
                for (int I = 1; I < NX + 2; I++)
                {
                    for (int J = 1; J < NY + 1; J++)
                    {
                        for (K = 1; K < NZ + 1; K++)
                        {
                            AREAX[I, J, K] = (AHE[I, J, K + 1] - AHE[I, J, K] + AHE[I, J + 1, K + 1] - AHE[I, J + 1, K]) * 0.5 * DDY[J];
                        }
                    }
                }

                for (int I = 1; I < NX + 1; I++)
                {
                    for (int J = 1; J < NY + 2; J++)
                    {
                        for (K = 1; K < NZ + 1; K++)
                        {
                            AREAY[I, J, K] = (AHE[I, J, K + 1] - AHE[I, J, K] + AHE[I + 1, J, K + 1] - AHE[I + 1, J, K]) * 0.5 * DDX[I];
                        }
                    }
                }

                for (int I = 1; I < NX + 1; I++)
                {
                    for (int J = 1; J < NY + 1; J++)
                    {
                        for (K = 1; K < NZ + 2; K++)
				{
					AREAZX[I, J, K] = ((AHE[I, J + 1, K] - AHE[I + 1, J + 1, K]) + (AHE[I, J, K] - AHE[I + 1, J, K])) * 0.5 * DDY[J];
					AREAZY[I, J, K] = ((AHE[I + 1, J, K] - AHE[I + 1, J + 1, K]) + (AHE[I, J, K] - AHE[I, J + 1, K])) * 0.5 * DDX[I];
					AREAZ[I, J, K] = Math.Sqrt(DDX[I] * DDX[I] * DDY[J] * DDY[J] + AREAZX[I, J, K] * AREAZX[I, J, K] + AREAZY[I, J, K] * AREAZY[I, J, K]);
				}
                    }
                }

                for (int I = 1; I < NX + 1; I++)
                {
                    for (int J = 1; J < NY + 1; J++)
				{
					for (K = 1; K < NZ + 1; K++)
					{
						VOL[I, J, K] = ((2.0 * AHE[I, J, K + 1] + AHE[I + 1, J, K + 1] + 2.0 * AHE[I + 1, J + 1, K + 1] + AHE[I, J + 1, K + 1]) - (2.0 * AHE[I, J, K] + AHE[I + 1, J, K] + 2.0 * AHE[I + 1, J + 1, K] + AHE[I, J + 1, K])) / 6.0 * DDX[I] * DDY[J];
						ZSP[I, J, K] = (AHE[I, J, K + 1] + AHE[I + 1, J, K + 1] + AHE[I + 1, J + 1, K + 1] + AHE[I, J + 1, K + 1] + AHE[I, J, K] + AHE[I + 1, J, K] + AHE[I + 1, J + 1, K] + AHE[I, J + 1, K]) / 8.0;
					}
					AH[I, J] = (AHE[I, J, 1] + AHE[I + 1, J, 1] + AHE[I + 1, J + 1, 1] + AHE[I, J + 1, 1]) / 4.0;
				}
                }

                //write ggeom.asc
                GGeomFileIO ggeom = new GGeomFileIO
                {
                    PathWindfield = Path.Combine(ProjectName, @"Computation", "ggeom.asc"),
                    ProjectName = ProjectName,
                    WriteCompressedFile = _writeCompressedFile,
                    NX = NX,
                    NY = NY,
                    NZ = NZ,
                    AH = AH,
                    X = X,
                    Y = Y,
                    Z = Z,
                    VOL = VOL,
                    AREAX = AREAX,
                    AREAY = AREAY,
                    AREAZ = AREAZ,
                    AREAZX = AREAZX,
                    AREAZY = AREAZY,
                    ZSP = ZSP,
                    DDX = DDX,
                    DDY = DDY,
                    ZAX = ZAX,
                    ZAY = ZAY,
                    IKOOA = IKOOA,
                    JKOOA = JKOOA,
                    Winkel = winkel,
                    AHE = AHE,
                    NODATA = NODDATA
                };

                ggeom.WriteGGeomFile();
                
			}
			
			wait.Close();
			wait.Dispose();
			
			return true;
		}
	}
}
