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
 * User: Markus
 * Date: 17.01.2019
 * Time: 16:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Windows.Forms;
using GralIO;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
    {
        private void GRAMMin(bool write)
        {
            if (write == true)
            {
                try
                {
                    // get final situation
                    int final_sit = 0;
                    IO_ReadFiles OpenProject = new IO_ReadFiles
                    {
                        ProjectName = ProjectName
                    };
                    if (OpenProject.ReadGrammInFile() == true) // get final situation from GRAMMin.dat if sunrise is activated
                        final_sit = OpenProject.GRAMMsunrise;
                    OpenProject = null;

                    using (StreamWriter mywriter = new StreamWriter(Path.Combine(ProjectName, @"Computation", "GRAMMin.dat")))
                    {
                        mywriter.WriteLine("Version 17.01");
                        mywriter.WriteLine("y");
                        mywriter.WriteLine(numericUpDown38.Value.ToString(ic)); // Roughness lenght
                        mywriter.WriteLine(Convert.ToString(numericUpDown24.Value) + "," + Convert.ToString(CellNrTopographySmooth));
                        if (checkBox27.Checked == true)
                            mywriter.WriteLine("yes      ! write steady-state-file");
                        else
                            mywriter.WriteLine("no       ! write steady-state-file");

                        if (checkBox30.Checked) // sunrise option selected
                        {
                            if (final_sit > 0) // final situation from GRAMMin.dat
                                GRAMM_Sunrise = final_sit;
                            else
                            {
                                string filename = Path.Combine(ProjectName, "Computation", "meteopgt.all");
                                GRAMM_Sunrise = (int)GralStaticFunctions.St_F.CountLinesInFile(filename) - 2;
                            }

                            if (GRAMM_Sunrise >= 1)
                                mywriter.WriteLine(GRAMM_Sunrise.ToString() + "      ! Number of original meteo length for GRAMM Sunrise option");
                            else
                            {
                                //MessageBox.Show("Error writing the sunrise option. Sunrise is not activated. Create meteo files first, please", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //checkBox30.Checked = false;
                                mywriter.WriteLine("0        ! GRAMM Sunrise option not activated"); // at the moment. If a meteo file is written, the GRAMMin.dat will be actualized
                            }
                        }
                        else
                        {
                            mywriter.WriteLine("0        ! GRAMM Sunrise option not activated");
                        }
                    }

                }
                catch
                {
                    MessageBox.Show("Could not write file \"Computation\\GRAMMin.dat\"", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// set flat terrain adjustments in the file IIN.dat for GRAMM simulations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRAMMSetFlatTerrainOption(object sender, EventArgs e)
        {
            SaveIINDatFile();
            
            if (checkBox31.Checked == false) // no flat terrain -> sunrise allowed
                checkBox30.Enabled = true;
            else
                checkBox30.Enabled = false;
            
            if (checkBox31.Checked == true && checkBox30.Checked == true) // deactivate sunrise option if necessary
            {
                if (SunriseResetMeteoFiles() == true)
                {
                    checkBox30.Checked = false;
                    checkBox30.Enabled = false;
                }
                else
                {
                    checkBox31.Checked = false;
                    checkBox30.Enabled = true;
                    SaveIINDatFile(); // reset value for flat terrain in this case
                }
            }
        }		        
        
        //write file "GRAMM.geb"
        public void WriteGrammGebFile()
        {
            if (EmifileReset == true)
            {
                string filename1 = Path.Combine("Computation", "GRAMM.geb");
                string newPath1 = Path.Combine(ProjectName, filename1);

                using (StreamWriter myWriter1 = File.CreateText(newPath1))
                {
                    myWriter1.WriteLine(Convert.ToString(Convert.ToInt32((GrammDomRect.East - GrammDomRect.West) / GRAMMHorGridSize, ic)) + "                !number of cells in x-direction");
                    myWriter1.WriteLine(Convert.ToString(Convert.ToInt32((GrammDomRect.North - GrammDomRect.South) / GRAMMHorGridSize, ic)) + "                !number of cells in y-direction");
                    myWriter1.WriteLine(Convert.ToString(numericUpDown16.Value, ic) + "               !number of cells in z-direction");
                    myWriter1.WriteLine(textBox13.Text + "                !West border of GRAMM model domain [m]");
                    myWriter1.WriteLine(textBox12.Text + "                !East border of GRAMM model domain [m]");
                    myWriter1.WriteLine(textBox14.Text + "                !South border of GRAMM model domain [m]");
                    myWriter1.WriteLine(textBox15.Text + "                !North border of GRAMM model domain [m]");
                }
                listBox2.Visible = true;
                button22.Visible = true;
                label95.Visible = true;
                button19.Visible = true;

                //save GRAMM control file "IIN.dat" and "reinit.dat"
                SaveIINDatFile();
            }
        }

        //save all control data for GRAMM to the file "IIN.dat" and "reinit.dat"
        private void SaveIINDatFile()
        {
            if (EmifileReset == true)
            {
                try
                {
                    using (StreamWriter mywriter = new StreamWriter(Path.Combine(ProjectName, @"Computation", "IIN.dat")))
                    {
                        DateTime selTime = dateTimePicker1.Value;
                        string yy = selTime.Year.ToString();
                        if (!checkBox35.Checked)
                        {
                            yy = selTime.Year.ToString().Substring(2,2);
                        }
                        string mm = selTime.Month.ToString("00");
                        string dd = selTime.Day.ToString("00");
                        string hh = selTime.Hour.ToString("00");
                        string mi = selTime.Minute.ToString("00");

                        if (checkBox35.Checked == true)
                        {
                            mywriter.WriteLine("COMPUTATION DATE         (YYYYMMDD)           :  " + yy + mm + dd + "             !No influence for steady-state simulations");
                        }
                        else
                        {
                            mywriter.WriteLine("COMPUTATION DATE         (YYMMDD)             :  " + yy + mm + dd + "               !No influence for steady-state simulations");
                        }

                        mywriter.WriteLine("BEGINNING OF COMPUTATION   (hhmm)             :  " + hh + mi + "                 !No influence for steady-state simulations");
                        mywriter.WriteLine("MAXIMUM ALLOWED TIME STEP DT  [ s ]           :  " + Convert.ToString(numericUpDown20.Value, ic) + "                   !Range 1-100");
                        mywriter.WriteLine("MODELLING TIME (FOR VALUES >1(s) AND <1[%])   :  " + Convert.ToString(numericUpDown21.Value, ic) + "                 !Range 0.01-1% or 2-infinite sec.");
                        mywriter.WriteLine("BUFFERING AFTER TIMESTEPS                     :  3600                 !No influence for steady-state simulations");
                        mywriter.WriteLine("MAX. PERMISSIBLE W-DEVIATION ABOVE < 1 [mm/s] :  0.01                 !Not used    ");
                        mywriter.WriteLine("RELATIVE INITIAL HUMIDITY  [ % ] GT.0         :  20                   !No influence for steady-state simulations");
                        mywriter.WriteLine("HEIGHT OF THE LOWEST COMPUTATION HEIGHT [ m ] :  330.                 !Not used    ");
                        mywriter.WriteLine("AIR TEMPERATURE AT GROUND  [ K ]              :  280.0                !No influence when using meteopgt.all");
                        mywriter.WriteLine("MOIST-ADIABATIC TEMPERATURE GRADIENT  [K/100m]:  -0.0065              !No influence for steady-state simulations");
                        mywriter.WriteLine("NEUTRAL LAYERING UP TO THE HEIGHT ABOVE GROUND:  5000                 !used when using meteopgt.all");
                        mywriter.WriteLine("SURFACE TEMPERATURE [ K ]                     :  280.0                !No influence when using meteopgt.all");
                        mywriter.WriteLine("TEMPERATURE OF THE SOIL IN 1 M DEPTH  [ K ]   :  280.0                !No influence when using meteopgt.all");
                        mywriter.WriteLine("LATITUDE                                      :  " + numericUpDown39.Value.ToString(ic) + "                   !Range -90 - +90 deg.");
                        mywriter.WriteLine("UPDATE OF RADIATION (TIMESTEPS)               :  300                  !No influence when using meteopgt.all");
                        mywriter.WriteLine("DEBUG LEVEL 0 none, 3 highest                 :  " + Convert.ToString(numericUpDown25.Value).Replace(" ", "") + "                    !not used    ");
                        if (checkBox31.Checked == false)
                        {
                            mywriter.WriteLine("COMPUTE  U V W PN T GW FO            YES=1    :  1111100              !GW and FO not used   ");
                            mywriter.WriteLine("COMPUTE  BR PR QU PSI TE TB STR      YES=1    :  1110011              !BR, PSI not used, TE not recommended");
                        }
                        else
                        {
                            mywriter.WriteLine("COMPUTE  U V W PN T GW FO            YES=1    :  1111000              !GW and FO not used   ");
                            mywriter.WriteLine("COMPUTE  BR PR QU PSI TE TB STR      YES=1    :  1110000              !BR, PSI not used, TE not recommended");
                        }

                        mywriter.WriteLine("DIAGNOSTIC INITIAL CONDITIONS        YES=1    :  1                    !Not used    ");
                        mywriter.WriteLine("EXPLICIT/IMPLICIT TIME INTEGRATION  I=1/E=0   :  1                    !Not used    ");
                        mywriter.WriteLine("RELAXATION VELOCITY                           :  " + Convert.ToString(Convert.ToDouble(numericUpDown22.Value), ic) + "                  !Range 0.01-1.0");
                        mywriter.WriteLine("RELAXATION SCALARS                            :  " + Convert.ToString(Convert.ToDouble(numericUpDown23.Value), ic) + "                  !Range 0.01-1.0");
                        if (checkBox21.Checked == false)
                        {
                            mywriter.WriteLine(@"Force catab flows -40/-25/-15 W/m" + SquareString + " AKLA 7/6/5 :  0                    !1=ON suitable with steady state meteopgt.all");
                        }
                        else
                            mywriter.WriteLine(@"Force catab flows -40/-25/-15 W/m" + SquareString + " AKLA 7/6/5 :  1                    !1=ON suitable with steady state meteopgt.all");

                        mywriter.WriteLine("UPDATE ERA5 BOUNDARY CONDITIONS (HOURS)       :  " + Convert.ToString(Convert.ToDouble(numericUpDown37.Value), ic) + "                    !Defines the interval after which the boundary conditions are updated with certain ERA5 data");

                        if (checkBox31.Checked == false)
                        {
                            mywriter.WriteLine("BOUNDARY CONDITION (1,2,3,4,5,6)              :  6                    !1=homogenous Von Neumann, 6=Nudging (standard)");
                        }
                        else
                        {
                            mywriter.WriteLine("BOUNDARY CONDITION (1,2,3,4,5,6)              :  1                    !1=homogenous Von Neumann, 6=Nudging (standard)");
                        }

                        if (checkBox35.Checked == false)
                        {
                            mywriter.WriteLine("TRANSIENT FORCING METEOPGT.ALL(1), ERA5(2)    :  0                    !Large-scale forcing using either meteopgt.all (geostrophic wind at 10000m) or ERA5 data");
                        }
                        else
                        {
                            mywriter.WriteLine("TRANSIENT FORCING METEOPGT.ALL(1), ERA5(2)    :  2                    !Large-scale forcing using either meteopgt.all (geostrophic wind at 10000m) or ERA5 data");
                        }
                        mywriter.WriteLine("RE-INITIALIZATION INTERVAL IN HOURS           :  " + Convert.ToString(Convert.ToDouble(numericUpDown44.Value), ic) + "                   !Defines the interval after which GRAMM is completely re-initialized with ERA5 data");
                        mywriter.WriteLine("GRAMM IN GRAMM NESTING YES=1                  :  0                    !Not used    ");
                        mywriter.WriteLine("Flowfield Output Format                       :  0                    !0=binary stream +header, 3=binary stream");
                    }

                    // int check = 0;
                    if (checkBox20.Checked == true)
                    {
                        //check = 1;
                        using (StreamWriter mywriter = new StreamWriter(Path.Combine(ProjectName, @"Computation", "reinit.dat")))
                        {
                            mywriter.WriteLine("9");     //max number re-inits   4, 7, 9 or 10 are feasible
                            mywriter.WriteLine("1");     //max number of monitoring stations used for forcing, currently 1
                            mywriter.WriteLine("0.7");   //Relax Factor for correction with wind difference vector [0.3 - 0.7]
                            mywriter.WriteLine("0.25");  //relative (accuracy) criteria in % Vdif/Vobs Vdif = Vobs- Vsim [0.15-0.3]
                            mywriter.WriteLine("0.0");   //critical low wind speed omitting criteria sim vs obs [0. - 1.2]
                            mywriter.WriteLine("0.3");   //Factor to reduce integration time [%] in case criterias are earlier fulfilled (1-factor)[0.0 - 0.4]
                        }
                    }
                    else
                    {
                        File.Delete(Path.Combine(ProjectName, @"Computation", "reinit.dat"));
                    }

                    //save GRAMM input file
                    GRAMMin(EmifileReset);
                    //delete windfields only if GRAMM is not used for mikroscale applications
                    if (GRALSettings.BuildingMode != 3)
                    {
                        //File.Delete(Path.Combine(projectname, @"Computation", "windfeld.txt"));
                    }

                }
                catch
                {
                    MessageBox.Show(this, "Error writing IIN.Dat", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            //enable/disable GRAMM simulations
            Enable_GRAMM();
        }
        
    }
}