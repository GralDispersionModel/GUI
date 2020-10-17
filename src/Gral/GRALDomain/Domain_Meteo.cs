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
using System.Data;

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
            List<GralBackgroundworkers.Point_3D> receptor_points = new List<GralBackgroundworkers.Point_3D>();
            int meteoModel = 0;
            bool localSCL = false;
            // Release send coors
            if (sender is SelectMultiplePoints _sl)
            {
                SendCoors -= _sl.ReceiveClickedCoordinates;
                localSCL = _sl.LocalStability;
                GRAMMmeteofile = _sl.MeteoInit; //value + ".met";
                meteoModel = _sl.MeteoModel;

                foreach (DataRow row in _sl.PointCoorData.Rows)
                {
                    if (row[0] != DBNull.Value && row[1] != DBNull.Value && row[2] != DBNull.Value && row[3] != DBNull.Value)
                    {
                        string a = _sl.MeteoInit + "_" + Convert.ToString(row[0]) + ".met";
                        a = string.Join("_", a.Split(Path.GetInvalidFileNameChars())); // remove invalid characters

                        GralBackgroundworkers.Point_3D item = new GralBackgroundworkers.Point_3D
                        {
                            filename = a,
                            X = Convert.ToDouble(row[1]),
                            Y = Convert.ToDouble(row[2]),
                            Z = Convert.ToDouble(row[3])
                        };
                        receptor_points.Add(item);
                    }
                }
                _sl.Close();
            }
			
			if (receptor_points.Count == 0)
            {
                MessageBox.Show(this, "No points defined", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (meteoModel == 1) // GRAMM
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
                    LocalStability = localSCL, // use local stability?
                    EvalPoints = receptor_points // evaluation points
                };

                GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                {
                    Text = DataCollection.Caption
                };
                BackgroundStart.Show();
			}
            else if (meteoModel == 2) // GRAL
            {
                GralBackgroundworkers.BackgroundworkerData DataCollection = new GralBackgroundworkers.BackgroundworkerData
                {
                    Path_GRAMMwindfield = Path.GetDirectoryName(MainForm.GRAMMwindfield),
                    Projectname = Gral.Main.ProjectName,
                    GrammWest = MainForm.GrammDomRect.West,
                    GrammSouth = MainForm.GrammDomRect.South,
                    GRAMMhorgridsize = MainForm.GRAMMHorGridSize,
                    GFFGridSize = (double)MainForm.numericUpDown10.Value,
                    Decsep = decsep,
                    UserText = "The process may take some minutes. The data can afterwards be analysed in the menu Meteorology",
                    Caption = "Compute GRAL Wind-Statistics ", // + DataCollection.Meteofilename;
                    Rechenart = 3, // ; 3 = analyse the GRAL Windfield
                    LocalStability = localSCL, // use local stability?
                    EvalPoints = receptor_points // evaluation points
                };

                GralBackgroundworkers.ProgressFormBackgroundworker BackgroundStart = new GralBackgroundworkers.ProgressFormBackgroundworker(DataCollection)
                {
                    Text = DataCollection.Caption
                };
                BackgroundStart.Show();
            }
            // now the backgroundworker works
            // gen_meteofile(Convert.ToDouble(trans), GRAMMmeteofile);
            // Reset mousecontrol
            MouseControl = 0;
        }
		
		// Cancel the Mettime Dialog
		private void CancelMetTimeSeries(object sender, EventArgs e)
		{
            // Release send coors
            if (sender is SelectMultiplePoints _sl)
            {
                SendCoors -= _sl.ReceiveClickedCoordinates;
                _sl.Close();
            }
            MouseControl = 0;	
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
