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
 * User: Markus_2
 * Date: 29.05.2015
 * Time: 18:16
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using GralIO;

namespace GralBackgroundworkers
{
    public partial class ProgressFormBackgroundworker
	{
		/// <summary>
        /// GRAMM re-order function
        /// </summary>
    	private void Reorder(GralBackgroundworkers.BackgroundworkerData mydata, 
                             System.ComponentModel.DoWorkEventArgs e)
		{
			//generate directory "Re-ordered"
			string newPath = System.IO.Path.Combine(mydata.Projectname, "Re-ordered");
			System.IO.Directory.CreateDirectory(newPath);

            //reading geometry file "ggeom.asc"
            GGeomFileIO ggeom = new GGeomFileIO
            {
                PathWindfield = mydata.Path_GRAMMwindfield
            };

            double[,] AH = new double[1, 1];
			double[, ,] ZSP = new double[1, 1, 1];
			string[] text = new string[1];
			int NX = 1;
			int NY = 1;
			int NZ = 1;
			int ischnitt = 1;
			
			if (ggeom.ReadGGeomAsc(0) == true)
			{
				NX = ggeom.NX;
				NY = ggeom.NY;
				NZ = ggeom.NZ;
				AH = ggeom.AH;
				ZSP = ggeom.ZSP;
				ggeom = null;
			}
						
			int ix = 0;
			int iy = 1;
			
			double xsi=mydata.XDomain-mydata.GrammWest;
			double eta = mydata.YDomain - mydata.GrammSouth;
			
			//obtain indices of selected point
			//double schnitt = trans; // AK Kuntner

			//obtain indices of selected point
			ix = Convert.ToInt32(Math.Floor(xsi / mydata.GRAMMhorgridsize)) + 1;
			iy = Convert.ToInt32(Math.Floor(eta / mydata.GRAMMhorgridsize)) + 1;
			//obtain index in the vertical direction
			for (int k = 1; k <= NZ; k++)
				if (ZSP[ix, iy, k] - AH[ix, iy] >= mydata.Schnitt)
			{
				ischnitt = k;
				break;
			}

			//windfield file Readers
			//PSFReader windfield = new PSFReader(form1.GRAMMwindfield, true);
			StreamReader meteopgt = new StreamReader(Path.Combine(mydata.Path_GRAMMwindfield, @"meteopgt.all"));
			//loop over all weather situations
			double Uoben = 0;
			double Voben = 0;
			double Uunten = 0;
			double Vunten = 0;
			double Umittel = 0;
			double Vmittel = 0;
			double[] nsektor = new double[100000];
			double[] wg = new double[100000];
			int[] iakla = new int[100000];
			int[] iwr = new int[100000];
			double[] u_GRAMM = new double[100000];
			double[] v_GRAMM = new double[100000];
			double[] u_METEO = new double[100000];
			double[] v_METEO = new double[100000];
			int iiwet = 0;
			double[] wgi = new double[100000];
			double richtung;
			float[, ,] UWI = new float[NX + 1, NY + 1, NZ + 1];
			float[, ,] VWI = new float[NX + 1, NY + 1, NZ + 1];
			float[, ,] WWI = new float[NX + 1, NY + 1, NZ + 1];
			text = meteopgt.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
			text = meteopgt.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
			for (int n = 1; n < 100000; n++)
			{				
				iiwet = iiwet + 1;
				SetText("Reading windfields: "+ Convert.ToString(iiwet));
				
				if (Rechenknecht.CancellationPending)
				{
                    e.Cancel = true;
				    return;
				}
				if (n % 4 == 0)
        		{
				    Rechenknecht.ReportProgress((int) (n / (double) 100000 * 100D));
				}
				
				try
				{
					text = meteopgt.ReadLine().Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
					nsektor[n] = Convert.ToDouble(text[0].Replace(".", mydata.Decsep));
					wg[n] = Convert.ToDouble(text[1].Replace(".", mydata.Decsep));
					iakla[n] = Convert.ToInt32(text[2]);
					richtung = nsektor[n] * 10;
					richtung = (270 - richtung) * 3.1415 / 180;
					u_METEO[n] = wg[n] * Math.Cos(richtung);
					v_METEO[n] = wg[n] * Math.Sin(richtung);
				}
				catch
				{
					break;
				}
				try
				{
					//read wind fields
					string wndfilename = Path.Combine(mydata.Path_GRAMMwindfield, Convert.ToString(iiwet).PadLeft(5, '0') + ".wnd");
					Windfield_Reader Reader = new Windfield_Reader();
					if (Reader.Windfield_read(wndfilename, NX, NY, NZ, ref UWI, ref VWI, ref WWI) == false)
					{
						throw new IOException();
					}
					
				}
				catch
				{
					break;
				}

				Uoben = UWI[ix, iy+1, ischnitt];
				Voben = VWI[ix, iy+1, ischnitt];
				if (ischnitt > 1)
				{
					Uunten = UWI[ix, iy+1, ischnitt - 1];
					Vunten = VWI[ix, iy+1, ischnitt - 1];
					Umittel = Uunten + (Uoben - Uunten) / (ZSP[ix, iy+1, ischnitt] - ZSP[ix, iy+1, ischnitt - 1]) *
						(mydata.Schnitt + AH[ix, iy] - ZSP[ix, iy, ischnitt - 1]);
					Vmittel = Vunten + (Voben - Vunten) / (ZSP[ix, iy+1, ischnitt] - ZSP[ix, iy+1, ischnitt - 1]) *
						(mydata.Schnitt + AH[ix, iy] - ZSP[ix, iy+1, ischnitt - 1]);
				}
				else
				{
					Umittel = Uoben / (ZSP[ix, iy+1, ischnitt] - AH[ix, iy]) * mydata.Schnitt;
					Vmittel = Voben / (ZSP[ix, iy+1, ischnitt] - AH[ix, iy]) * mydata.Schnitt;
				}
				if (Vmittel == 0)
					iwr[n] = 90;
				else
					iwr[n] = Convert.ToInt32(Math.Abs(Math.Atan(Umittel / Vmittel)) * 180 / 3.14);
				if ((Vmittel > 0) && (Umittel <= 0))
					iwr[n] = 180 - iwr[n];
				if ((Vmittel >= 0) && (Umittel > 0))
					iwr[n] = 180 + iwr[n];
				if ((Vmittel < 0) && (Umittel >= 0))
					iwr[n] = 360 - iwr[n];
				wgi[n] = Math.Sqrt(Umittel * Umittel + Vmittel * Vmittel);
				u_GRAMM[n] = Umittel;
				v_GRAMM[n] = Vmittel;
			}
			meteopgt.Close();
			iiwet = iiwet - 1;

			//loop over all computed dispersion situations
			double diffwind1 = 0;
			double diffwind2 = 0;
			double diffwr1 = 0;
			double diffwr11 = 0;
			//double diffwr2 = 0;
			string windfieldfile;
			for (int i = 1; i <= iiwet; i++)
			{
				SetText("Copy windfields: "+ Convert.ToString(i));
				//initial differences in wind vector and -direction
				diffwind1 = Math.Sqrt(Math.Pow(u_GRAMM[i] - u_METEO[i], 2) + Math.Pow(v_GRAMM[i] - v_METEO[i], 2));
				diffwr1 = Math.Abs(iwr[i] - nsektor[i] * 10);
				diffwr11 = 360 - Math.Max(iwr[i], nsektor[i] * 10) + Math.Min(iwr[i], nsektor[i] * 10);
				diffwr1 = Math.Min(diffwr1, diffwr11);

				windfieldfile = Convert.ToString(i).PadLeft(5, '0') + ".wnd";

				//search for the GRAMM wind field, which fits most the observed wind data at the observation point
				for (int n = 1; n <= iiwet; n++)
				{
					//search just in those dispersion situations with a similiar stability class
					if (((iakla[i] > 4) && (iakla[n] > 4)) || ((iakla[i] < 4) && (iakla[n] < 4)) || ((iakla[i] == 4) && (iakla[n] == 4)))
					{
						//optimization criterion: wind vector
						diffwind2 = Math.Sqrt(Math.Pow(u_GRAMM[n] - u_METEO[i], 2) + Math.Pow(v_GRAMM[n] - v_METEO[i], 2));
						if (diffwind2 < diffwind1)
						{
							diffwind1 = diffwind2;
							windfieldfile = Convert.ToString(n).PadLeft(5, '0') + ".wnd";
						}

						/*
                            //optimization criterion: wind direction --> brachte bei Tests für Voitsberg schlechtere Ergebnisse.
                            diffwr2 = Math.Abs(iwr[n] - nsektor[i] * 10);
                            diffwr11 = 360 - Math.Max(iwr[n], nsektor[i] * 10) + Math.Min(iwr[n], nsektor[i] * 10);
                            diffwr2 = Math.Min(diffwr2, diffwr11);
                            if (diffwr2 < diffwr1)
                            {
                                diffwr1 = diffwr2;
                                windfieldfile = Convert.ToString(n).PadLeft(5, '0') + ".wnd";
                            }
						 */
					}
				}
				
				//copy selected wind field file to subdirectory re-ordered
				string orifile=Path.Combine(mydata.Path_GRAMMwindfield, windfieldfile);
				string targetfile=Path.Combine(Path.Combine(mydata.Projectname, "Re-ordered"), Convert.ToString(i).PadLeft(5, '0') + ".wnd");
				File.Copy(orifile, targetfile,true);
				
				// copy .scl file
				orifile=Path.Combine(mydata.Path_GRAMMwindfield, Path.GetFileNameWithoutExtension(windfieldfile) + ".scl");
				targetfile=Path.Combine(Path.Combine(mydata.Projectname, "Re-ordered"), Convert.ToString(i).PadLeft(5, '0') + ".scl");
				if (File.Exists(orifile))
					File.Copy(orifile, targetfile,true);
				
				// copy .obl file
				orifile=Path.Combine(mydata.Path_GRAMMwindfield, Path.GetFileNameWithoutExtension(windfieldfile) + ".obl");
				targetfile=Path.Combine(Path.Combine(mydata.Projectname, "Re-ordered"), Convert.ToString(i).PadLeft(5, '0') + ".obl");
				if (File.Exists(orifile))
					File.Copy(orifile, targetfile,true);
				
				// copy .ust file
				orifile=Path.Combine(mydata.Path_GRAMMwindfield, Path.GetFileNameWithoutExtension(windfieldfile) + ".ust");
				targetfile=Path.Combine(Path.Combine(mydata.Projectname, "Re-ordered"), Convert.ToString(i).PadLeft(5, '0') + ".ust");
				if (File.Exists(orifile))
					File.Copy(orifile, targetfile,true);
				
			}
			
		}
	}
}
