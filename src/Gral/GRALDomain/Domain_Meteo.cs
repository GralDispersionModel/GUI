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
using System.Windows.Forms;
using System.IO;
using GralDomForms;

namespace GralDomain
{
    /// /////////////////////////////////////////////////////////////////////////////
    //
    //  GRAMM windfield analysis tools
    //
    /////////////////////////////////////////////////////////////////////////////////

    public partial class Domain
	{
		/// <summary>
        /// Start the evaluation of Meteo-Stations
        /// </summary>
		private void MetTimeSeries(object sender, EventArgs e)
		{
			
			GRAMMmeteofile = MeteoDialog.Meteo_Init; //value + ".met";
			int trans = MeteoDialog.Meteo_Height;
			
			//select height above ground for the windfield analysis
//				int trans = Convert.ToInt16(10);
//				if (InputBox1("Height above ground", "Height above ground [m]:", 0, 10000, ref trans) == DialogResult.OK)
			
			//generate the meteo-file
			// Kuntner: call backgroundworker

			//Oettl: check whether multiple statistics at all receptor points were selected and set the loop
			
			List <GralBackgroundworkers.Point_3D> receptor_points = new List <GralBackgroundworkers.Point_3D>();
			
			if (MeteoDialog.Receptor_Points == true) // Receptor points
			{
				for (int i = 0; i < EditR.ItemData.Count; i++)
				{
                    GralBackgroundworkers.Point_3D item = new GralBackgroundworkers.Point_3D
                    {

                        // get data from editr
                        Z = EditR.ItemData[i].Height
                    };
                    string a = MeteoDialog.Meteo_Init + "_" + EditR.ItemData[i].Name + ".met";
					a =string.Join("_", a.Split(Path.GetInvalidFileNameChars())); // remove invalid characters
					item.filename = a;
					item.X = EditR.ItemData[i].Pt.X;
					item.Y = EditR.ItemData[i].Pt.Y;
					if ((item.X < MainForm.GrammDomRect.West) || (item.X > MainForm.GrammDomRect.East) || (item.Y < MainForm.GrammDomRect.South) || (item.Y > MainForm.GrammDomRect.North))
                    {
                        a += "";
                    }
                    else
                    {
                        receptor_points.Add(item);
                    }
                }
			}
			else // Single point
			{
			    string a = GRAMMmeteofile;
				a =string.Join("_", a.Split(Path.GetInvalidFileNameChars())); // remove invalid characters
                GralBackgroundworkers.Point_3D item = new GralBackgroundworkers.Point_3D
                {
                    X = XDomain,
                    Y = YDomain,
                    Z = Convert.ToDouble(trans),
                    filename = a    
                };
                
				if ((item.X < MainForm.GrammDomRect.West) || (item.X > MainForm.GrammDomRect.East) || (item.Y < MainForm.GrammDomRect.South) || (item.Y > MainForm.GrammDomRect.North))
                {
                    a += "";
                }
                else
                {
                    receptor_points.Add(item);
                }
            }
			
			if (receptor_points.Count == 0)
            {
                MessageBox.Show(this, "No points inside GRAMM domain", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
			{

                GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData
                {
                    Path_GRAMMwindfield = Path.GetDirectoryName(MainForm.GRAMMwindfield),
                    Projectname = Gral.Main.ProjectName,
                    GrammWest = MainForm.GrammDomRect.West,
                    GrammSouth = MainForm.GrammDomRect.South,
                    GRAMMhorgridsize = MainForm.GRAMMHorGridSize,
                    Decsep = decsep,
                    UserText = "The process may take some minutes. The data can afterwards be analysed in the menu Meteorology",
                    Caption = "Compute Wind-Statistics ", // + DataCollection.Meteofilename;
                    Rechenart = 1, // ; 1 = analyse the GRAMM_Windfield
                    LocalStability = MeteoDialog.Local_Stability, // use local stability?
                    EvalPoints = receptor_points // evaluation points
                };

                GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                {
                    Text = DataCollection.Caption
                };
                BackgroundStart.Show();
			}
			// now the backgroundworker works
			//gen_meteofile(Convert.ToDouble(trans), GRAMMmeteofile);
			// Reset mousecontrol
			MouseControl = 0;
            MeteoDialog.Start_computation -= new StartCreateMeteoStation(MetTimeSeries);
            MeteoDialog.Close();
			MeteoDialog.Dispose();
		}
		
		// Cancel the Mettime Dialog
		private void CancelMetTimeSeries(object sender, EventArgs e)
		{
			MouseControl = 0;	
			MeteoDialog.Cancel_computation -= new CancelCreateMeteoStation(CancelMetTimeSeries);			
			MeteoDialog.Close();
			MeteoDialog.Dispose();
		}

        private void ComputeMeanWindVelocity(object sender, EventArgs e)
        {
            using (DialogCreateMeteoStation met_st = new DialogCreateMeteoStation
            {
                Meteo_Title = "GRAL GUI Compute mean wind velocity",
                Meteo_Init = "Av_Windspeed",
                Meteo_Ext = ".txt",
                Meteo_Height = 10,
                Meteo_Model = 4, // just Filename & Height
                X1 = Left + 70,
                Y1 = Top + 50,
                Xs = 0,
                Ys = 0
            })
            {
                if (met_st.ShowDialog() == DialogResult.OK)
                {
                    string file = met_st.Meteo_Init; // "Av_Windspeed";
                    int trans = met_st.Meteo_Height;  //Convert.ToInt32(10);
                                                      //			if (InputBox("Filename", "Save data as (filename without extension):", ref file) == DialogResult.OK)
                    file = Path.Combine(Gral.Main.ProjectName, @"Maps", file);
                    //select height above ground for the windfield analysis
                    //				if (InputBox1("Height above ground", "Height above ground [m]:", 0, 10000, ref trans) == DialogResult.OK)
                    {
                        GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData
                        {
                            Schnitt = Convert.ToDouble(trans),
                            Meteofilename = GRAMMmeteofile,
                            Projectname = Gral.Main.ProjectName,
                            Path_GRAMMwindfield = MainForm.GRAMMwindfield,
                            XDomain = XDomain,
                            YDomain = YDomain,
                            GrammWest = MainForm.GrammDomRect.West,
                            GrammSouth = MainForm.GrammDomRect.South,
                            GRAMMhorgridsize = MainForm.GRAMMHorGridSize,
                            Decsep = decsep,
                            UserText = @"The process may take some minutes. The file " + Path.GetFileName(file) + @" is stored in the subdirectory \maps\",
                            Caption = "Compute mean wind velocity",
                            Rechenart = 31, // ; 31
                            Filename = file
                        };

                        GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                        {
                            Text = DataCollection.Caption
                        };
                        BackgroundStart.Show();
                        // now the backgroundworker works
                    }
                }
            }
        }
	}
}
