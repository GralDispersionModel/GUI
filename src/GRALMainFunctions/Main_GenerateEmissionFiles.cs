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

using GralDomain;
using GralIO;
using GralItemData;
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Gral
{
    /// <summary>
    /// Description of Generate_Emission_Files.
    /// </summary>
    public partial class Main
    {
        /// <summary>
        /// Enumeration for Time Series Mode
        /// </summary>
        private enum SetTimeSeriesModeEnum { PointSourceVelocity = 0, PointSourceTemperature = 1, PortalSourceVelocity = 2, PortalSourceTemperature = 3 };

        private CultureInfo ic = CultureInfo.InvariantCulture;

        //////////////////////////////////////////////////////////////////////////
        //
        //generate gral emission files
        //
        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Generate Emission files for the GRAL computation core
        /// </summary>
        private void CreateGralEmissionFiles()
        {
            string SelectedPollutant = Pollmod[listBox5.SelectedIndex];
            List<int> SelectedSourceGroups = new List<int>();
            //read the seleceted source groups for the simulation
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                string selpoll = listView1.Items[i].SubItems[0].Text;
                string[] dummy = selpoll.Split(new char[] { ':' });
                int sg = 0;
                if (dummy.Length > 1)
                {
                    Int32.TryParse(dummy[1], out sg);
                }
                else
                {
                    Int32.TryParse(dummy[0], out sg);
                }
                SelectedSourceGroups.Add(sg);
            }

            RectangleF DomainRect = new RectangleF((float)GralDomRect.West, (float)GralDomRect.South, (float)(GralDomRect.East - GralDomRect.West), (float)(GralDomRect.North - GralDomRect.South));

            //Point Source__________________________________________________________________
            List<PointSourceData> _psList = new List<PointSourceData>();
            PointSourceDataIO _ps = new PointSourceDataIO();
            string _file = Path.Combine(Main.ProjectName, "Emissions", "Psources.txt");
            _ps.LoadPointSources(_psList, _file, true, DomainRect);
            _ps = null;

            bool file = false;
            string newPath = Path.Combine(ProjectName, @"Computation", "point.dat");
            //remove existing file
            File.Delete(newPath);

            try
            {
                List<string> TempTimeSeries = new List<string>();
                List<string> VelTimeSeries = new List<string>();

                using (StreamWriter myWriter = File.CreateText(newPath))
                {
                    myWriter.WriteLine("Generated: V2019");
                    string Header = "x,y,z,";
                    Header += SelectedPollutant + "[kg/h],--,--,--,exit vel.[m/s],diameter[m],Temp.[K],Source group,deposition parameters F2.5, F10,DiaMax,Density,VDep2.5,VDep10,VDepMax,Dep_Conc";

                    myWriter.WriteLine(Header);

                    foreach (PointSourceData _psdata in _psList)
                    {
                        //filter source groups
                        if (SelectedSourceGroups.Contains(_psdata.Poll.SourceGroup)) // SG is selected
                        {
                            //filter pollutant
                            for (int j = 0; j < 10; j++)
                            {
                                if (SelectedPollutant.Equals(PollutantList[_psdata.Poll.Pollutant[j]]) &&
                                    _psdata.Poll.EmissionRate[j] > 0)
                                {
                                    //write file point.dat
                                    string pqline =
                                        _psdata.Pt.X.ToString(ic) + "," +
                                        _psdata.Pt.Y.ToString(ic) + "," +
                                        _psdata.Height.ToString(ic) + "," +
                                        _psdata.Poll.EmissionRate[j].ToString(ic) + "," +
                                        "0,0,0," +
                                        _psdata.Velocity.ToString(ic) + "," +
                                        _psdata.Diameter.ToString(ic) + "," +
                                        _psdata.Temperature.ToString(ic) + "," +
                                        _psdata.Poll.SourceGroup.ToString(ic);

                                    if (_psdata.GetDep() != null) // deposition entry exists
                                    {
                                        pqline += "," +
                                            _psdata.GetDep()[j].Frac_2_5.ToString(ic) + "," +
                                            _psdata.GetDep()[j].Frac_10.ToString(ic) + "," +
                                            _psdata.GetDep()[j].DM_30.ToString(ic) + "," +
                                            _psdata.GetDep()[j].Density.ToString(ic) + "," +
                                            _psdata.GetDep()[j].V_Dep1.ToString(ic) + "," +
                                            _psdata.GetDep()[j].V_Dep2.ToString(ic) + "," +
                                            _psdata.GetDep()[j].V_Dep3.ToString(ic) + "," +
                                            _psdata.GetDep()[j].Conc.ToString(ic);
                                    }

                                    if (!string.IsNullOrEmpty(_psdata.TemperatureTimeSeries))
                                    {
                                        pqline += ",Temp@_" + _psdata.TemperatureTimeSeries;
                                        if (TempTimeSeries.Contains(_psdata.TemperatureTimeSeries) == false)
                                        {
                                            TempTimeSeries.Add(_psdata.TemperatureTimeSeries);
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(_psdata.VelocityTimeSeries))
                                    {
                                        pqline += ",Vel@_" + _psdata.VelocityTimeSeries;
                                        if (VelTimeSeries.Contains(_psdata.VelocityTimeSeries) == false)
                                        {
                                            VelTimeSeries.Add(_psdata.VelocityTimeSeries);
                                        }
                                    }
                                    myWriter.WriteLine(pqline);
                                    file = true;
                                }
                            }
                        }
                    }
                }

                // create new Time Series for Temperature and Velocity
                if (VelTimeSeries.Count > 0)
                {
                    WriteTempVelTimeSeries(VelTimeSeries, SetTimeSeriesModeEnum.PointSourceVelocity);
                }
                else
                {
                    AskAndDeleteFile(Path.Combine(Main.ProjectName, @"Computation", "TimeSeriesPointSourceVel.txt"));
                }
                if (TempTimeSeries.Count > 0)
                {
                    WriteTempVelTimeSeries(TempTimeSeries, SetTimeSeriesModeEnum.PointSourceTemperature);
                }
                else
                {
                    AskAndDeleteFile(Path.Combine(Main.ProjectName, @"Computation", "TimeSeriesPointSourceTemp.txt"));
                }
                VelTimeSeries.Clear();
                VelTimeSeries.TrimExcess();
                TempTimeSeries.Clear();
                TempTimeSeries.TrimExcess();
            }
            catch (Exception ex) { MessageBox.Show(this, "Error writing point.dat \n" + ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            _psList.Clear();
            _psList.TrimExcess();
            _psList = null;

            if (file == false)
            {
                File.Delete(newPath);
            }

            //Area Source__________________________________________________________________
            List<AreaSourceData> _asList = new List<AreaSourceData>();
            AreaSourceDataIO _as = new AreaSourceDataIO();
            _file = Path.Combine(Main.ProjectName, "Emissions", "Asources.txt");
            _as.LoadAreaData(_asList, _file);
            _as = null;

            file = false;
            newPath = Path.Combine(ProjectName, @"Computation", "cadastre.dat");
            //remove existing file
            File.Delete(newPath);

            try
            {
                using (StreamWriter myWriter = File.CreateText(newPath))
                {
                    string Header = "x,y,z,dx,dy,dz,";
                    Header += SelectedPollutant + "[kg/h],--,--,--,source group, deposition data";
                    myWriter.WriteLine(Header);
                    //List for points inside area source polygons
                    List<PointD> xyCell = new List<PointD>(10000);

                    foreach (AreaSourceData _asdata in _asList)
                    {
                        double xmin = double.MaxValue;
                        double xmax = double.MinValue;
                        double ymin = double.MaxValue;
                        double ymax = double.MinValue;
                        foreach (PointD _pt in _asdata.Pt)
                        {
                            xmin = Math.Min(xmin, _pt.X);
                            xmax = Math.Max(xmax, _pt.X);
                            ymin = Math.Min(ymin, _pt.Y);
                            ymax = Math.Max(ymax, _pt.Y);
                        }

                        //filter domain
                        if ((xmin >= GralDomRect.West) && (xmax <= GralDomRect.East) &&
                            (ymin >= GralDomRect.South) && (ymax <= GralDomRect.North))
                        {
                            //filter source groups
                            if (SelectedSourceGroups.Contains(_asdata.Poll.SourceGroup)) // SG is selected
                            {
                                //filter pollutant
                                for (int j = 0; j < 10; j++)
                                {
                                    if (SelectedPollutant.Equals(PollutantList[_asdata.Poll.Pollutant[j]]) &&
                                        _asdata.Poll.EmissionRate[j] > 0)
                                    {
                                        xyCell.Clear();
                                        //raster area source -> todo: we should use a delauny triangulation in the future...
                                        double rasterFactor = 2.2; //start using a factor of 1 -> the factor is reduced some lines below
                                        //reduce raster size, if less than 2 points inside the area source have been found
                                        while (xyCell.Count < 2 && rasterFactor > 0.0005)
                                        {
                                            xyCell.Clear();
                                            rasterFactor /= 2.2; // reduce raster size if not more than 1 point has been found, avoid testing the same points multiple times
                                            double yraster = ymax - _asdata.RasterSize * 0.5 * rasterFactor;
                                            while ((yraster >= ymin) && (yraster <= ymax))
                                            {
                                                double xraster = xmin + _asdata.RasterSize * 0.5 * rasterFactor;
                                                while ((xraster >= xmin) && (xraster <= xmax))
                                                {
                                                    if (St_F.PointInPolygonD(new PointD(xraster, yraster), _asdata.Pt))
                                                    {
                                                        xyCell.Add(new PointD(xraster, yraster));
                                                    }
                                                    xraster += _asdata.RasterSize * rasterFactor;
                                                }
                                                yraster -= _asdata.RasterSize * rasterFactor;
                                            }
                                        }

                                        int decPlaces = 3;
                                        if (_asdata.RasterSize * rasterFactor > 10)
                                        {
                                            decPlaces = 1;
                                        }
                                        else if (_asdata.RasterSize * rasterFactor > 5)
                                        {
                                            decPlaces = 2;
                                        }

                                        //write file cadastre.dat
                                        for (int l = 0; l < xyCell.Count; l++)
                                        {
                                            //write file cadestre.dat
                                            string cadestre =
                                                Math.Round(xyCell[l].X, 1).ToString(ic) + "," +
                                                Math.Round(xyCell[l].Y, 1).ToString(ic) + "," +
                                                _asdata.Height.ToString(ic) + "," +
                                                Math.Round(_asdata.RasterSize * rasterFactor, decPlaces).ToString(ic) + "," +
                                                Math.Round(_asdata.RasterSize * rasterFactor, decPlaces).ToString(ic) + "," +
                                                _asdata.VerticalExt.ToString(ic) + "," +
                                                (_asdata.Poll.EmissionRate[j] / Convert.ToDouble(xyCell.Count)).ToString(ic) + "," +
                                                "0,0,0," +
                                                _asdata.Poll.SourceGroup.ToString(ic);

                                            if (_asdata.GetDep() != null) // deposition entry exists
                                            {
                                                cadestre += "," +
                                                    _asdata.GetDep()[j].Frac_2_5.ToString(ic) + "," +
                                                    _asdata.GetDep()[j].Frac_10.ToString(ic) + "," +
                                                    _asdata.GetDep()[j].DM_30.ToString(ic) + "," +
                                                    _asdata.GetDep()[j].Density.ToString(ic) + "," +
                                                    _asdata.GetDep()[j].V_Dep1.ToString(ic) + "," +
                                                    _asdata.GetDep()[j].V_Dep2.ToString(ic) + "," +
                                                    _asdata.GetDep()[j].V_Dep3.ToString(ic) + "," +
                                                    _asdata.GetDep()[j].Conc.ToString(ic);
                                            }
                                            myWriter.WriteLine(cadestre);
                                            file = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(this, "Error writing cadestre.dat \n" + ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            _asList.Clear();
            _asList.TrimExcess();
            _asList = null;

            if (file == false)
            {
                File.Delete(newPath);
            }

            //Line Source__________________________________________________________________
            List<LineSourceData> _lsList = new List<LineSourceData>();
            LineSourceDataIO _ls = new LineSourceDataIO();
            _file = Path.Combine(Main.ProjectName, "Emissions", "Lsources.txt");
            _ls.LoadLineSources(_lsList, _file);
            _ls = null;
            file = false;

            newPath = Path.Combine(ProjectName, @"Computation", "line.dat");
            //remove existing file
            File.Delete(newPath);

            try
            {
                using (StreamWriter myWriter = File.CreateText(newPath))
                {
                    myWriter.WriteLine("Generated: ");
                    myWriter.WriteLine("Generated: ");
                    myWriter.WriteLine("Generated: ");
                    myWriter.WriteLine("Generated: ");
                    string Header = "StrName,Section,Sourcegroup,x1,y1,z1,x2,y2,z2,width,noiseabatementwall,Length[km],--,";
                    Header += SelectedPollutant + "[kg/(km*h)],--,--,--,--,--,deposition data";
                    myWriter.WriteLine(Header);

                    foreach (LineSourceData _lsdata in _lsList)
                    {
                        //filter domain
                        double xmin = double.MaxValue;
                        double xmax = double.MinValue;
                        double ymin = double.MaxValue;
                        double ymax = double.MinValue;

                        foreach (GralData.PointD_3d _pt in _lsdata.Pt)
                        {
                            xmin = Math.Min(xmin, _pt.X);
                            xmax = Math.Max(xmax, _pt.X);
                            ymin = Math.Min(ymin, _pt.Y);
                            ymax = Math.Max(ymax, _pt.Y);
                        }

                        if ((xmin >= GralDomRect.West) && (xmax <= GralDomRect.East) &&
                            (ymin >= GralDomRect.South) && (ymax <= GralDomRect.North))
                        {
                            //filter source groups
                            foreach (PollutantsData _poll in _lsdata.Poll)
                            {
                                if (SelectedSourceGroups.Contains(_poll.SourceGroup)) // SG is selected
                                {
                                    double vert_extension = (-1) * Math.Abs(_lsdata.VerticalExt);

                                    //filter pollutant
                                    for (int j = 0; j < 10; j++)
                                    {
                                        if (SelectedPollutant.Equals(PollutantList[_poll.Pollutant[j]]) &&
                                            _poll.EmissionRate[j] > 0)
                                        {
                                            //write file line.dat
                                            int linesegments = _lsdata.Pt.Count - 1;

                                            for (int i = 0; i < linesegments; i++)
                                            {
                                                double x1 = _lsdata.Pt[i].X;
                                                double x2 = _lsdata.Pt[i + 1].X;
                                                double y1 = _lsdata.Pt[i].Y;
                                                double y2 = _lsdata.Pt[i + 1].Y;
                                                double h1 = Math.Round(_lsdata.Pt[i].Z, 1);
                                                double h2 = Math.Round(_lsdata.Pt[i + 1].Z, 1);

                                                string line =
                                                    _lsdata.Name + "," +
                                                    _lsdata.Section + "," +
                                                    _poll.SourceGroup.ToString(ic) + "," +
                                                    Math.Round(x1, 1).ToString(ic) + "," +
                                                    Math.Round(y1, 1).ToString(ic) + "," +
                                                    h1.ToString(ic) + "," +
                                                    Math.Round(x2, 1).ToString(ic) + "," +
                                                    Math.Round(y2, 1).ToString(ic) + "," +
                                                    h2.ToString(ic) + "," +
                                                    _lsdata.Width.ToString(ic) + "," +
                                                    vert_extension.ToString(ic) + "," +
                                                    "0," + //Alternative: Math.Round(Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)), 1).ToString(ic) + "," +
                                                    "0," +
                                                    _poll.EmissionRate[j].ToString(ic) + "," +
                                                    "0,0,0,0,0";

                                                if (_lsdata.GetDep() != null) // deposition entry exists
                                                {
                                                    line += "," +
                                                        _lsdata.GetDep()[j].Frac_2_5.ToString(ic) + "," +
                                                        _lsdata.GetDep()[j].Frac_10.ToString(ic) + "," +
                                                        _lsdata.GetDep()[j].DM_30.ToString(ic) + "," +
                                                        _lsdata.GetDep()[j].Density.ToString(ic) + "," +
                                                        _lsdata.GetDep()[j].V_Dep1.ToString(ic) + "," +
                                                        _lsdata.GetDep()[j].V_Dep2.ToString(ic) + "," +
                                                        _lsdata.GetDep()[j].V_Dep3.ToString(ic) + "," +
                                                        _lsdata.GetDep()[j].Conc.ToString(ic);
                                                }
                                                myWriter.WriteLine(line);
                                                file = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(this, "Error writing line.dat \n" + ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            _lsList.Clear();
            _lsList.TrimExcess();
            _lsList = null;

            if (file == false)
            {
                File.Delete(newPath);
            }

            Tunnelportals(SelectedSourceGroups);

        }

        /// <summary>
        /// Create Portalsources.txt file
        /// </summary>
        /// <param name="SelectedSourceGroups"></param>
        private void Tunnelportals(List<int> SelectedSourceGroups)
        {
            string SelectedPollutant = Pollmod[listBox5.SelectedIndex];

            List<PortalsData> _poList = new List<PortalsData>();
            PortalsDataIO _pd = new PortalsDataIO();
            string _file = Path.Combine(Main.ProjectName, "Emissions", "Portalsources.txt");
            _pd.LoadPortalSources(_poList, _file, true, new RectangleF((float)GralDomRect.West, (float)GralDomRect.South, (float)(GralDomRect.East - GralDomRect.West), (float)(GralDomRect.North - GralDomRect.South)));
            _pd = null;

            bool file = false;
            string newPath = Path.Combine(ProjectName, @"Computation", "meteopgt.all");
            bool meteo = false;
            List<string> meteopgt = new List<string>();
            List<string> tunnel = new List<string>();
            if (File.Exists(newPath) == true)
            {
                try
                {
                    meteo = true;
                    using (StreamReader myreader = new StreamReader(newPath))
                    {
                        string line = myreader.ReadLine();
                        while (line != null)
                        {
                            meteopgt.Add(line);
                            line = myreader.ReadLine();
                        }
                    }
                }
                catch { }
            }

            newPath = Path.Combine(ProjectName, @"Computation", "portals.dat");
            //remove existing file
            File.Delete(newPath);

            try
            {
                List<string> TempTimeSeries = new List<string>();
                List<string> VelTimeSeries = new List<string>();

                using (StreamWriter myWriter = File.CreateText(newPath))
                {
                    myWriter.WriteLine("Generated: V2019");
                    string Header = "x1,y1,x2,y2,z0,z1,";
                    Header += SelectedPollutant + "[kg/h],--,--,--,source group,deposition parameters F2.5, F10,DiaMax,Density,VDep2.5,VDep10,VDepMax,Dep_Conc,DeltaT,ExitVel";
                    myWriter.WriteLine(Header);
                    //double stiffness=0;
                    foreach (PortalsData _pdData in _poList)
                    {
                        //filter source groups
                        foreach (PollutantsData _poll in _pdData.Poll)
                        {
                            if (SelectedSourceGroups.Contains(_poll.SourceGroup)) // SG is selected
                            {
                                //filter pollutant
                                for (int j = 0; j < 10; j++)
                                {
                                    if (SelectedPollutant.Equals(PollutantList[_poll.Pollutant[j]]) &&
                                        _poll.EmissionRate[j] > 0)
                                    {
                                        double baseHeight = _pdData.BaseHeight;
                                        double height = Math.Abs(_pdData.Height) + Math.Abs(baseHeight);
                                        if (baseHeight < 0)
                                        {
                                            height *= (-1);
                                        }

                                        string portal =
                                            Math.Round(_pdData.Pt1.X, 1).ToString(ic) + "," +
                                            Math.Round(_pdData.Pt1.Y, 1).ToString(ic) + "," +
                                            Math.Round(_pdData.Pt2.X, 1).ToString(ic) + "," +
                                            Math.Round(_pdData.Pt2.Y, 1).ToString(ic) + "," +
                                            baseHeight.ToString(ic) + "," +
                                            height.ToString(ic) + "," +
                                            _poll.EmissionRate[j].ToString(ic) + "," +
                                            "0,0,0," +
                                            _poll.SourceGroup.ToString(ic);

                                        if (_pdData.GetDep() != null) // deposition entry exists
                                        {
                                            portal += "," +
                                                _pdData.GetDep()[j].Frac_2_5.ToString(ic) + "," +
                                                _pdData.GetDep()[j].Frac_10.ToString(ic) + "," +
                                                _pdData.GetDep()[j].DM_30.ToString(ic) + "," +
                                                _pdData.GetDep()[j].Density.ToString(ic) + "," +
                                                _pdData.GetDep()[j].V_Dep1.ToString(ic) + "," +
                                                _pdData.GetDep()[j].V_Dep2.ToString(ic) + "," +
                                                _pdData.GetDep()[j].V_Dep3.ToString(ic) + "," +
                                                _pdData.GetDep()[j].Conc.ToString(ic);
                                        }

                                        portal += "," + _pdData.DeltaT.ToString(ic) + "," +
                                            _pdData.ExitVel.ToString(ic);

                                        if (!string.IsNullOrEmpty(_pdData.TemperatureTimeSeries))
                                        {
                                            portal += ",Temp@_" + _pdData.TemperatureTimeSeries;
                                            if (TempTimeSeries.Contains(_pdData.TemperatureTimeSeries) == false)
                                            {
                                                TempTimeSeries.Add(_pdData.TemperatureTimeSeries);
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(_pdData.VelocityTimeSeries))
                                        {
                                            portal += ",Vel@_" + _pdData.VelocityTimeSeries;
                                            if (VelTimeSeries.Contains(_pdData.VelocityTimeSeries) == false)
                                            {
                                                VelTimeSeries.Add(_pdData.VelocityTimeSeries);
                                            }
                                        }

                                        //write file portals.dat
                                        myWriter.WriteLine(portal);
                                        file = true;

                                        //write exit vel, temp. diff, stiffness to meteopgt.all ->compatibility to old GRAL versions before 20.01
                                        if (meteo == true)
                                        {
                                            //compute stiffness parameter in dependence on portal orientation and wind direction
                                            double xmin = _pdData.Pt1.X;
                                            double xmax = _pdData.Pt2.X;
                                            double ymin = _pdData.Pt1.Y;
                                            double ymax = _pdData.Pt2.Y;
                                            double winkel = 0;
                                            Angle(xmin, xmax, ymin, ymax, ref winkel);
                                            tunnel.Add(_pdData.ExitVel.ToString(ic) + "," + _pdData.DeltaT.ToString(ic) + "," + Convert.ToString(Math.Round(winkel, 0), ic) + "," + _pdData.Nemo.TrafficSit.ToString(ic) + "," + _pdData.Nemo.AvDailyTraffic.ToString(ic));
                                        }
                                    }
                                }
                            }
                        }

                    }
                }

                // create new Time Series for Temperature and Velocity
                if (VelTimeSeries.Count > 0)
                {
                    WriteTempVelTimeSeries(VelTimeSeries, SetTimeSeriesModeEnum.PortalSourceVelocity);
                }
                else
                {
                    AskAndDeleteFile(Path.Combine(Main.ProjectName, @"Computation", "TimeSeriesPortalSourceVel.txt"));
                }
                if (TempTimeSeries.Count > 0)
                {
                    WriteTempVelTimeSeries(TempTimeSeries, SetTimeSeriesModeEnum.PortalSourceTemperature);
                }
                else
                {
                    AskAndDeleteFile(Path.Combine(Main.ProjectName, @"Computation", "TimeSeriesPortalSourceTemp.txt"));
                }

                VelTimeSeries.Clear();
                VelTimeSeries.TrimExcess();
                TempTimeSeries.Clear();
                TempTimeSeries.TrimExcess();

            }
            catch (Exception ex) { MessageBox.Show(this, "Error writing portals.dat \n" + ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            if (file == false)
            {
                File.Delete(newPath);
            }
            //else
            //{
            //	//write exit vel, temp. diff to meteopgt.all
            //	if (meteo == true)
            //	{
            //		newPath = Path.Combine(ProjectName, @"Computation","meteopgt.all");
            //		File.Delete(newPath);
            //		string[] values = new string[1000];
            //		string[] dummy = new string[5];

            //		using (StreamWriter myWriter = File.CreateText(newPath))
            //		{
            //			for (int a = 0; a < meteopgt.Count; a++)
            //			{
            //				if (a > 1)
            //				{
            //					values = meteopgt[a].Split(new char[] { ',', ' ', '\t', ';' });
            //					meteopgt[a] = values[0] + "," + values[1] + "," + values[2] + "," + values[3];
            //					double w1 = Convert.ToDouble(values[0], ic) * 10;
            //					for (int i = 0; i < tunnel.Count; i++)
            //					{
            //						dummy = tunnel[i].Split(new char[] { ',' });
            //						double w2 = Convert.ToDouble(dummy[2], ic);
            //						//double winkeldifference1 = Math.Abs(w2 - w1);
            //						//if (winkeldifference1 > 180)
            //						//    winkeldifference1 = 360 - winkeldifference1;
            //						//if (dummy[3] == "0")
            //						//{
            //						//if (winkeldifference1 > 135)
            //						//stiffness = 10;
            //						//else
            //						//stiffness = 50;
            //						//}
            //						//else
            //						//{
            //						//if (winkeldifference1 > 135)
            //						//stiffness = Math.Pow(Math.Atan(Convert.ToDouble(dummy[4]) / 1000 / 24), 1) * 15;
            //						//else
            //						//stiffness = Math.Pow(Math.Atan(Convert.ToDouble(dummy[4]) / 1000 / 24), 1) * 70;
            //						//}
            //						//meteopgt[a] = meteopgt[a] + "," + dummy[0] + "," + dummy[1] + "," + Convert.ToString(Math.Round(stiffness, 0));
            //						meteopgt[a] = meteopgt[a] + "," + dummy[0] + "," + dummy[1];
            //					}
            //				}
            //				myWriter.WriteLine(meteopgt[a]);
            //			}
            //		}

            //	}
            //}
        } // GRAL_emifiles

        private void AskAndDeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                if (MessageBox.Show(this, "Delete the file " + Path.GetFileName(filePath),
                    "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch { }
                }
            }
        }

        // Write the Precipitation.txt file for wet deposition
        /// <summary>
        /// Write the file Precipitation.txt if wet deposition is activated
        /// </summary>
        private void Write_Precipitation_txt()
        {
            string Precip = Path.Combine(ProjectName, @"Computation", "Precipitation.txt");
            string mettimeseries = Path.Combine(ProjectName, @"Computation", "mettimeseries.dat");
            CultureInfo CI = CultureInfo.InvariantCulture;

            if (File.Exists(mettimeseries) == true)
            {
                if (File.Exists(Precip))
                {
                    if (MessageBox.Show(this, "Overwrite Precipitation.txt?", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            File.Delete(Precip);
                        }
                        catch { }
                    }
                    else
                    {
                        return;
                    }
                }

                if (File.Exists(Precip) == false) // do not overwrite existing file!
                {
                    try
                    {
                        // read mettimeseries
                        List<string> data_mettimeseries = new List<string>();
                        ReadMetTimeSeries(mettimeseries, ref data_mettimeseries);

                        if (data_mettimeseries.Count == 0) // no data available
                        {
                            MessageBox.Show(this, "mettimeseries.dat not available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // now cerate the file Precipitation.txt
                        using (StreamWriter myWriter = File.CreateText(Precip))
                        {
                            string header = "Day.Month\tHour\tPrecipitation[mm/h]";
                            myWriter.WriteLine(header);

                            string[] text2 = new string[5];
                            string[] text3 = new string[2];

                            foreach (string met in data_mettimeseries)
                            {
                                text2 = met.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
                                int month = Convert.ToInt32(text3[1]);
                                string day = text3[0];
                                int hour = Convert.ToInt32(text2[1]);

                                string line = day + "." + month.ToString(CI) + "\t" + hour.ToString(CI) + "\t0.0";
                                myWriter.WriteLine(line);
                            }
                        } // using
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(this, "mettimeseries.dat not available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } //


        // Write the emission_timeseries.txt
        private bool Write_Emission_Timeseries()
        {
            string emission_ts = Path.Combine(ProjectName, @"Computation", "emissions_timeseries.txt");
            if (Directory.Exists(ProjectSetting.EmissionModulationPath))
            {
                emission_ts = Path.Combine(ProjectSetting.EmissionModulationPath, "emissions_timeseries.txt");
            }

            string mettimeseries = Path.Combine(ProjectName, @"Computation", "mettimeseries.dat");
            DialogResult dr;
            CultureInfo ic = CultureInfo.InvariantCulture;
            bool result = false;

            if (File.Exists(mettimeseries) == true)
            {

                if (File.Exists(emission_ts))
                {
                    dr = MessageBox.Show(this, "Overwrite emission modulation file" + Environment.NewLine + "emissions_timeseries.txt", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                }
                else
                {
                    dr = MessageBox.Show(this, "Create new emission modulation file" + Environment.NewLine + "emissions_timeseries.txt", "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }

                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        // read mettimeseries
                        List<string> data_mettimeseries = new List<string>();
                        ReadMetTimeSeries(mettimeseries, ref data_mettimeseries);

                        if (data_mettimeseries.Count == 0) // no data available
                        {
                            MessageBox.Show(this, "File mettimeseries.dat not available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return result;
                        }

                        // read all emission factors
                        int SG_Count = listView1.Items.Count;

                        float[,,] fac_combined = new float[SG_Count, 13, 25]; // emission factor for each SG and month and day
                        int[] SG_Number = new int[SG_Count];

                        float[] fac_diurnal = new float[25];
                        float[] fac_seasonal = new float[13];


                        for (int i = 0; i < listView1.Items.Count; i++)
                        {
                            string entry = listView1.Items[i].SubItems[0].Text;

                            // get the number of the selected Source Group = name
                            string[] sg = new string[2];
                            sg = entry.Split(new char[] { ':' });

                            string name = "";
                            try
                            {
                                sg[1] = sg[1].Trim();
                                name = sg[1];
                            }
                            catch
                            {
                                name = sg[0];
                            }

                            SG_Number[i] = Convert.ToInt32(name); // remember the original SG_Number

                            // Read emission modulation
                            if (ReadEmissionModulation(name, ref fac_diurnal, ref fac_seasonal) == true)
                            {
                                for (int month = 0; month < 12; month++)
                                {
                                    for (int hour = 0; hour < 24; hour++)
                                    {
                                        fac_combined[i, month, hour] = fac_seasonal[month] * fac_diurnal[hour];
                                    }
                                }
                            }
                        } //for each listbox items

                        fac_seasonal = null;
                        fac_diurnal = null;

                        // now cerate the file emission_timeseries.txt
                        using (StreamWriter myWriter = File.CreateText(emission_ts))
                        {
                            string header = "Day.Month\tHour";
                            for (int SG_Class = 0; SG_Class <= SG_Number.GetUpperBound(0); SG_Class++)
                            {
                                header += "\t" + SG_Number[SG_Class].ToString(ic);
                            }
                            myWriter.WriteLine(header);

                            string[] text2 = new string[5];
                            string[] text3 = new string[2];

                            foreach (string met in data_mettimeseries)
                            {
                                text2 = met.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
                                int month = Convert.ToInt32(text3[1]);
                                string day = text3[0];
                                int hour = Convert.ToInt32(text2[1]);

                                string line = day + "." + month.ToString(ic) + "\t" + hour.ToString(ic);

                                for (int SG_Class = 0; SG_Class <= SG_Number.GetUpperBound(0); SG_Class++)
                                {
                                    line += "\t" + fac_combined[SG_Class, month - 1, hour].ToString(ic);
                                }

                                myWriter.WriteLine(line);
                            }
                        } // using
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(this, "mettimeseries.dat not available", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return result;
        } // Write_emission_timeseries.txt

        /// <summary>
        /// Write a Time Series for Temperature or Velocity of Point Sources or Portal Sources
        /// </summary>
        /// <param name="EntryList">Time Series List</param>
        /// <param name="Mode">0: Point Source Velocity 1: Point Source Temperature 2: Portal Source Velocity 3: Portal Source Temperature</param>
        /// <returns></returns>
        private bool WriteTempVelTimeSeries(List<string> EntryList, SetTimeSeriesModeEnum Mode)
        {
            GralItemForms.EditTimeSeriesValues edT = new GralItemForms.EditTimeSeriesValues();
            float[,] GridValues = new float[12, 4];
            string filePath = string.Empty;
            List<string> data_result = new List<string>();

            // read mettimeseries
            List<string> data_mettimeseries = new List<string>();
            ReadMetTimeSeries(Path.Combine(ProjectName, @"Computation", "mettimeseries.dat"), ref data_mettimeseries);

            if (data_mettimeseries.Count == 0) // no data available
            {
                return false;
            }

            if (Mode == SetTimeSeriesModeEnum.PointSourceVelocity)
            {
                filePath = Path.Combine(Main.ProjectName, @"Emissions", "TimeSeriesPointSourceVel.tsd");
            }
            else if (Mode == SetTimeSeriesModeEnum.PointSourceTemperature)
            {
                filePath = Path.Combine(Main.ProjectName, @"Emissions", "TimeSeriesPointSourceTemp.tsd");
            }
            else if (Mode == SetTimeSeriesModeEnum.PortalSourceVelocity)
            {
                filePath = Path.Combine(Main.ProjectName, @"Emissions", "TimeSeriesPortalSourceVel.tsd");
            }
            else if (Mode == SetTimeSeriesModeEnum.PortalSourceTemperature)
            {
                filePath = Path.Combine(Main.ProjectName, @"Emissions", "TimeSeriesPortalSourceTemp.tsd");
            }

            if (!File.Exists(filePath))
            {
                return false;
            }

            string header = "Day.Month\tHour";
            foreach (string entry in EntryList)
            {
                header += "\t" + entry;
            }
            data_result.Add(header);

            string[] text2 = new string[5];
            string[] text3 = new string[2];

            foreach (string met in data_mettimeseries) // add date & Time
            {
                text2 = met.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
                int month = Convert.ToInt32(text3[1]);
                string day = text3[0];
                int hour = Convert.ToInt32(text2[1]);
                data_result.Add(day + "." + month.ToString(ic) + "\t" + hour.ToString(ic));
            }

            foreach (string entry in EntryList)
            {
                GridValues = edT.ReadValues(filePath, entry, false); // read Values from file

                int i = 1; // Header = 0
                foreach (string met in data_mettimeseries)
                {
                    text2 = met.Split(new char[] { ' ', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    text3 = text2[0].Split(new char[] { '.', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    int month = Math.Max(0, Math.Min(11, Convert.ToInt32(text3[1]) - 1));
                    int hour = Convert.ToInt32(text2[1]);
                    string line = string.Empty;

                    if (GridValues != null)
                    {
                        line += "\t" + GridValues[month, Math.Min(3, (int)(hour / 6))].ToString(ic);
                    }
                    else
                    {
                        line += "\t" + "0.0";
                    }

                    data_result[i] += line;
                    i++;
                }
            }

            // now create the timeseries file
            string write_path = Path.Combine(ProjectName, @"Computation", Path.GetFileName(filePath).Replace("tsd", "txt"));

            DialogResult res = DialogResult.Yes;
            // Write new time series
            if (File.Exists(write_path))
            {
                res = MessageBox.Show(this, "Overwrite the file " + Path.GetFileName(write_path),
                "GRAL GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            if (res == DialogResult.Yes)
            {

                using (StreamWriter myWriter = File.CreateText(write_path))
                {
                    foreach (string line in data_result)
                    {
                        myWriter.WriteLine(line);
                    }
                }
            }

            edT.Dispose();
            edT = null;
            data_result.Clear();
            data_result.TrimExcess();
            data_mettimeseries.Clear();
            data_mettimeseries.TrimExcess();

            return true;
        }

        /// <summary>
        /// Read the time series from file
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="data">String List getting the readed data</param>
        /// <returns></returns>
        private bool ReadMetTimeSeries(string filename, ref List<string> data)
        {
            bool ok = true;
            data.Clear();
            try
            {
                FileStream fs_mettimeseries = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (StreamReader mettimeseries = new StreamReader(fs_mettimeseries))
                {
                    string text;

                    // read data
                    while (mettimeseries.EndOfStream == false)
                    {
                        text = mettimeseries.ReadLine();
                        data.Add(text);
                    }

                }
            }
            catch
            {
                ok = false;
            }
            return ok;
        }

        //computes total emissions of selected source groups and pollutants
        private void ComputeTotalEmissions(double[] totalemissions)
        {
            List<PollutantsData> AllPollutants = new List<PollutantsData>();
            List<int> SelectedSourceGroups = new List<int>();

            AllPollutants = ReadAllPollutants();  // read all pollutants into this List

            //read the seleceted source groups for the simulation
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                string selpoll = listView1.Items[i].SubItems[0].Text;
                string[] dummy = selpoll.Split(new char[] { ':' });
                int sg = 0;
                if (dummy.Length > 1)
                {
                    Int32.TryParse(dummy[1], out sg);
                }
                else
                {
                    Int32.TryParse(dummy[0], out sg);
                }
                SelectedSourceGroups.Add(sg);
            }

            foreach (PollutantsData _poll in AllPollutants)
            {
                int sg = _poll.SourceGroup;

                if (SelectedSourceGroups.Contains(sg)) // SG is selected
                {
                    //filter pollutant
                    for (int i = 0; i < 10; i++)
                    {
                        if (PollutantList[_poll.Pollutant[i]].Equals(Pollmod[listBox5.SelectedIndex]))
                        {
                            if (sg <= totalemissions.GetUpperBound(0))
                            {
                                totalemissions[sg] += _poll.EmissionRate[i] * 0.365 * 24;
                            }
                            else
                            {
                                MessageBox.Show(this, "Invalid source group nr: " + sg.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            //Cursor = Cursors.Default;
        }

        /// <summary>
        /// Raster Buildings, Walls and Vegetation and write buildings.dat and vegetation.dat 
        /// </summary>
        private void GenerateObstacleRaster()
        {
            CultureInfo ic = CultureInfo.InvariantCulture;
            List<BuildingData> _bdList = new List<BuildingData>();
            List<WallData> _wdList = new List<WallData>();
            List<VegetationData> _vdList = new List<VegetationData>();

            IO_ReadFiles read_source = new IO_ReadFiles();
            // import walls
            try
            {
                WallDataIO _wd = new WallDataIO();
                string _file = Path.Combine(Main.ProjectName, "Emissions", "Walls.txt");
                _wd.LoadWallData(_wdList, _file);
                _wd = null;
            }
            catch { }

            // import buildings
            try
            {
                BuildingDataIO _bd = new BuildingDataIO();
                string _file = Path.Combine(Main.ProjectName, "Emissions", "Buildings.txt");
                if (File.Exists(_file) == false) // try old Computation path
                {
                    _file = Path.Combine(Main.ProjectName, "Computation", "Buildings.txt");
                }
                _bd.LoadBuildings(_bdList, _file);
                _bd = null;
            }
            catch { }

            // import vegetation
            try
            {
                VegetationDataIO _vdata = new VegetationDataIO();
                string _file = Path.Combine(Main.ProjectName, "Emissions", "Vegetation.txt");
                if (File.Exists(_file) == false) // try old Computation path
                {
                    _file = Path.Combine(Main.ProjectName, "Computation", "Vegetation.txt");
                }
                _vdata.LoadVegetation(_vdList, _file);
                _vdata = null;
            }
            catch { }

            read_source = null;

            bool file = false;
            double[] xcell = new double[1000];
            double[] ycell = new double[1000];

            GralMessage.MessageWindow message = new GralMessage.MessageWindow();
            message.Show();
            message.listBox1.Items.Add("Raster buildings on GRAL grid....");
            message.Refresh();

            double flow_field_grid = Convert.ToDouble(numericUpDown10.Value);
            double GRAL_West = GralDomRect.West;
            double GRAL_East = GralDomRect.East;
            double GRAL_South = GralDomRect.South;
            double GRAL_North = GralDomRect.North;

            int xRmax = (int)((GRAL_East - GRAL_West) / flow_field_grid);
            int yRmax = (int)((GRAL_North - GRAL_South) / flow_field_grid);

            //			try
            //			{
            string newPath = Path.Combine(ProjectName, @"Computation", "buildings.dat");

            try
            {
                //remove existing file
                File.Delete(newPath);
            }
            catch { }
            try
            {
                using (StreamWriter myWriter = File.CreateText(newPath))
                {
                    foreach (BuildingData _bd in _bdList)
                    {
                        //show the acutal building
                        message.listBox1.Items.Clear();
                        message.listBox1.Items.Add(_bd.Name);
                        message.Refresh();
                        Application.DoEvents();

                        //filter domain
                        double xmin = double.MaxValue;
                        double xmax = double.MinValue;
                        double ymin = double.MaxValue;
                        double ymax = double.MinValue;

                        foreach (PointD _pt in _bd.Pt)
                        {
                            xmin = Math.Min(xmin, _pt.X);
                            xmax = Math.Max(xmax, _pt.X);
                            ymin = Math.Min(ymin, _pt.Y);
                            ymax = Math.Max(ymax, _pt.Y);
                        }

                        if (((xmin >= GRAL_West) && (xmin <= GRAL_East)) || ((xmax >= GRAL_West) && (xmax <= GRAL_East)) ||
                            ((ymin >= GRAL_South) && (ymin <= GRAL_North)) || ((ymax >= GRAL_South) && (ymax <= GRAL_North)))
                        {
                            //raster buildings
                            int indexX = Convert.ToInt32((xmin - GRAL_West) / flow_field_grid);
                            int indexY = Convert.ToInt32((ymax - GRAL_South) / flow_field_grid);

                            double maxy = GRAL_South + (indexY) * flow_field_grid;
                            double minx = GRAL_West + (indexX) * flow_field_grid;

                            //evaluate number of raster cells
                            double yraster = maxy - flow_field_grid * 0.5;
                            int count = 0;
                            while (yraster >= ymin)
                            {
                                double xraster = minx + flow_field_grid * 0.5;
                                while (xraster <= xmax)
                                {
                                    int pointsInside = 0;
                                    if (St_F.PointInPolygonD(new PointD(xraster, yraster), _bd.Pt))
                                    {
                                        ++pointsInside;
                                    }
                                    if (St_F.PointInPolygonD(new PointD(xraster - flow_field_grid * 0.3, yraster - flow_field_grid * 0.3), _bd.Pt))
                                    {
                                        ++pointsInside;
                                    }
                                    if (St_F.PointInPolygonD(new PointD(xraster + flow_field_grid * 0.3, yraster + flow_field_grid * 0.3), _bd.Pt))
                                    {
                                        ++pointsInside;
                                    }
                                    if (St_F.PointInPolygonD(new PointD(xraster, yraster + flow_field_grid * 0.3), _bd.Pt))
                                    {
                                        ++pointsInside;
                                    }
                                    if (St_F.PointInPolygonD(new PointD(xraster, yraster - flow_field_grid * 0.3), _bd.Pt))
                                    {
                                        ++pointsInside;
                                    }
                                    if (pointsInside < BuildingCellCoverageThreshold && pointsInside > 0) // otherwise the cell is inside the polygon for the most part or the cell can't reach BuildingCellCoverageThreshold hits
                                    {
                                        if (St_F.PointInPolygonD(new PointD(xraster - flow_field_grid * 0.3, yraster + flow_field_grid * 0.3), _bd.Pt))
                                        {
                                            ++pointsInside;
                                        }
                                        if (St_F.PointInPolygonD(new PointD(xraster + flow_field_grid * 0.3, yraster - flow_field_grid * 0.3), _bd.Pt))
                                        {
                                            ++pointsInside;
                                        }
                                        if (St_F.PointInPolygonD(new PointD(xraster + flow_field_grid * 0.3, yraster), _bd.Pt))
                                        {
                                            ++pointsInside;
                                        }
                                        if (pointsInside == BuildingCellCoverageThreshold - 1) // otherwise BuildingCellCoverageThreshold is already reached or can't be reached
                                        {
                                            if (St_F.PointInPolygonD(new PointD(xraster - flow_field_grid * 0.3, yraster), _bd.Pt))
                                            {
                                                ++pointsInside;
                                            }
                                        }
                                    }

                                    if (count >= xcell.GetUpperBound(0))
                                    {
                                        Array.Resize(ref xcell, xcell.GetUpperBound(0) + 1000);
                                        Array.Resize(ref ycell, ycell.GetUpperBound(0) + 1000);
                                    }

                                    if (pointsInside > BuildingCellCoverageThreshold - 1) // the cell is inside for the most part
                                    {
                                        xcell[count] = xraster;
                                        ycell[count] = yraster;
                                        count++;
                                    }
                                    xraster += flow_field_grid;
                                }
                                yraster -= flow_field_grid;
                            }
                            //write file buildings.dat
                            for (int l = 0; l < count; l++)
                            {
                                myWriter.WriteLine(Math.Round(xcell[l], 1).ToString(ic) + "," + Math.Round(ycell[l], 1).ToString(ic) + ","
                                    + _bd.LowerBound.ToString(ic) + "," + _bd.Height.ToString(ic));
                                file = true;
                            }
                        }
                    }

                    // Write wall data to buildings.dat
                    foreach (WallData _wd in _wdList)
                    {
                        message.listBox1.Items.Clear();
                        message.listBox1.Items.Add(_wd.Name);
                        message.Refresh();
                        Application.DoEvents();
                        int vertices = _wd.Pt.Count - 1; // number of vertices
                        for (int i = 0; i < vertices; i++) // loop through all vertices
                        {
                            double x0 = _wd.Pt[i].X;
                            double y0 = _wd.Pt[i].Y;
                            double z0 = _wd.Pt[i].Z;
                            double x1 = _wd.Pt[i + 1].X;
                            double y1 = _wd.Pt[i + 1].Y;
                            double z1 = _wd.Pt[i + 1].Z;

                            int x0R = (int)((x0 - GRAL_West) / flow_field_grid);  // x0 cell
                            int y0R = (int)((y0 - GRAL_South) / flow_field_grid); // y0 cell
                            int x1R = (int)((x1 - GRAL_West) / flow_field_grid);  // x1 cell
                            int y1R = (int)((y1 - GRAL_South) / flow_field_grid); // y0 cell

                            double dz = z1 - z0;
                            double l_tot = Math.Sqrt(Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2)); // distance from x,y, to x0,y0

                            // loop over wall-line with bresenham algorithm
                            foreach (Point raster in GralStaticFunctions.Bresenham.GetNewPoint(x0R, y0R, x1R, y1R))
                            {
                                // inside GRAL domain?
                                if (raster.X > 2 && raster.Y > 2 && raster.X < (xRmax - 2) && raster.Y < (yRmax - 2))
                                {
                                    double x = raster.X * flow_field_grid + GRAL_West + flow_field_grid * 0.5;
                                    double y = raster.Y * flow_field_grid + GRAL_South + flow_field_grid * 0.5;
                                    double l = Math.Sqrt(Math.Pow(x - x0, 2) + Math.Pow(y - y0, 2)); // distance from x,y, to x0,y0
                                    double h = 0;

                                    // catch segments with a lenght of 0
                                    if (l_tot > 2 * double.Epsilon)
                                    {
                                        h = Math.Round(z0 + dz * l / l_tot, 1);
                                    }
                                    else
                                    {
                                        h = (z0 + z1) / 2;
                                    }

                                    // if relative height, avoid negative values by rounding effects
                                    if (z0 >= -0.01)
                                    {
                                        h = Math.Abs(h);
                                    }
                                    myWriter.WriteLine(Math.Round(x, 1).ToString(ic) + "," + Math.Round(y, 1).ToString(ic) + "," + "0" + "," + St_F.DblToIvarTxt(h));
                                    file = true;
                                }
                            }
                        }
                    }  // Write wall data to buildings.dat
                } // using()
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Rastering buildings " + ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (_vdList.Count > 0)
            {
                string newPathV = Path.Combine(ProjectName, @"Computation", "vegetation.dat");
                try
                {
                    //remove existing file
                    File.Delete(newPathV);
                }
                catch { }

                GRAL_East -= flow_field_grid * 3;
                GRAL_North -= flow_field_grid * 3;
                GRAL_West += flow_field_grid * 3;
                GRAL_South += flow_field_grid * 3;

                try
                {
                    using (StreamWriter myWriter = File.CreateText(newPathV))
                    {

                        foreach (VegetationData _vd in _vdList)
                        {
                            if (_vd.Pt.Count > 2)
                            {
                                //show the acutal vegetation 
                                message.listBox1.Items.Clear();
                                message.listBox1.Items.Add(_vd.Name);
                                message.Refresh();
                                Application.DoEvents();

                                //filter domain
                                double xmin = double.MaxValue;
                                double xmax = double.MinValue;
                                double ymin = double.MaxValue;
                                double ymax = double.MinValue;

                                int vertices = _vd.Pt.Count; // number of vertices

                                foreach (PointD _pt in _vd.Pt)
                                {
                                    xmin = Math.Min(xmin, _pt.X);
                                    xmax = Math.Max(xmax, _pt.X);
                                    ymin = Math.Min(ymin, _pt.Y);
                                    ymax = Math.Max(ymax, _pt.Y);
                                }

                                //raster vegetation
                                int indexX = Convert.ToInt32((xmin - GRAL_West) / flow_field_grid);
                                int indexY = Convert.ToInt32((ymax - GRAL_South) / flow_field_grid);

                                double maxy = GRAL_South + (indexY) * flow_field_grid;
                                double minx = GRAL_West + (indexX) * flow_field_grid;

                                //evaluate number of raster cells
                                double yraster = maxy - flow_field_grid * 0.5;
                                int count = 0;
                                while (yraster >= ymin)
                                {
                                    double xraster = minx + flow_field_grid * 0.5;
                                    while (xraster <= xmax)
                                    {
                                        int pointsInside = 0;
                                        if (St_F.PointInPolygonD(new PointD(xraster, yraster), _vd.Pt))
                                        {
                                            ++pointsInside;
                                        }
                                        //if (St_F.PointInPolygonD(new PointD(xraster - flow_field_grid * 0.45, yraster - flow_field_grid * 0.45), _vd.Pt))
                                        //{
                                        //    ++pointsInside;
                                        //}
                                        //if (St_F.PointInPolygonD(new PointD(xraster + flow_field_grid * 0.45, yraster + flow_field_grid * 0.45), _vd.Pt))
                                        //{
                                        //    ++pointsInside;
                                        //}
                                        //if (St_F.PointInPolygonD(new PointD(xraster, yraster + flow_field_grid * 0.45), _vd.Pt))
                                        //{
                                        //    ++pointsInside;
                                        //}
                                        //if (St_F.PointInPolygonD(new PointD(xraster, yraster - flow_field_grid * 0.45), _vd.Pt))
                                        //{
                                        //    ++pointsInside;
                                        //}
                                        //if (pointsInside < 5) // otherwise the cell is inside for the most part
                                        //{
                                        //    if (St_F.PointInPolygonD(new PointD(xraster - flow_field_grid * 0.45, yraster + flow_field_grid * 0.45), _vd.Pt))
                                        //    {
                                        //        ++pointsInside;
                                        //    }
                                        //    if (St_F.PointInPolygonD(new PointD(xraster + flow_field_grid * 0.45, yraster - flow_field_grid * 0.45), _vd.Pt))
                                        //    {
                                        //        ++pointsInside;
                                        //    }
                                        //    if (St_F.PointInPolygonD(new PointD(xraster + flow_field_grid * 0.45, yraster), _vd.Pt))
                                        //    {
                                        //        ++pointsInside;
                                        //    }
                                        //    if (St_F.PointInPolygonD(new PointD(xraster - flow_field_grid * 0.45, yraster), _vd.Pt))
                                        //    {
                                        //        ++pointsInside;
                                        //    }
                                        //}

                                        if (count >= xcell.GetUpperBound(0))
                                        {
                                            Array.Resize(ref xcell, xcell.GetUpperBound(0) + 1000);
                                            Array.Resize(ref ycell, ycell.GetUpperBound(0) + 1000);
                                        }

                                        if (pointsInside > 0) // the cell is inside for the most part
                                        {
                                            xcell[count] = xraster;
                                            ycell[count] = yraster;
                                            count++;
                                        }
                                        xraster += flow_field_grid;
                                    }
                                    yraster -= flow_field_grid;
                                }

                                if (count > 0)
                                {
                                    //write file vegetation.dat
                                    // D(ata)	Height	TrunkZone	Trunk	Crown	Coverage
                                    myWriter.WriteLine("D\t" + _vd.VerticalExt.ToString(ic) + "\t" + _vd.TrunkZone.ToString(ic) +
                                                       "\t" + _vd.LADTrunk.ToString(ic) + "\t" + _vd.LADCrown.ToString(ic) + "\t" + _vd.Coverage.ToString(ic));

                                    for (int l = 0; l < count; l++)
                                    {
                                        myWriter.WriteLine(Math.Round(xcell[l], 1).ToString(ic) + "," + Math.Round(ycell[l], 1).ToString(ic));
                                        file = true;
                                    }
                                }
                            }
                        }
                    } // Using
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Rastering vegetation " + ex.Message, "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            message.Close();
            message.Dispose();

            ChangeButtonLabel(ButtonColorEnum.ButtonBuildings, ButtonColorEnum.BlackHook); // Building label green

            if (file == false) // problem at writing buildings.dat
            {
                File.Delete(newPath);
                ChangeButtonLabel(ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.RedDot); // Building label red & delete buildings.dat

                if (_bdList.Count == 0 && _vdList.Count == 0 && _wdList.Count == 0)
                {
                    ChangeButtonLabel(ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.Invisible); // Building label invisible
                }
                MessageBox.Show(this, "There are no buildings, walls or vegetation areas \ninside the GRAL domain area,\nor no domain area has been defined", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            //			}
            //			catch(Exception ex)
            //			{
            //				Change_Label(SetButtonColorEnum.ButtonBuildings, Gral.SetButtonColorEnum.Invisible) // Building label invisible
            //				MessageBox.Show(this, ex.Message,"GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //			}

            _bdList.Clear();
            _bdList.TrimExcess();
            _vdList.Clear();
            _vdList.TrimExcess();
            _wdList.Clear();
            _wdList.TrimExcess();
        }

        private void Angle(double x1, double x2, double y1, double y2, ref double angle)
        {
            angle = 0;
            if (x2 != x1)
            {
                angle = Math.Atan((y2 - y1) / (x2 - x1)) * 180 / 3.14;
                if ((x1 < x2) && (y1 < y2))
                {
                    angle = 360 - angle;
                }

                if ((x1 > x2) && (y1 < y2))
                {
                    angle = 180 - angle;
                }

                if ((x1 < x2) && (y1 > y2))
                {
                    angle = Math.Abs(angle);
                }

                if ((x1 > x2) && (y1 > y2))
                {
                    angle = 180 - angle;
                }
            }
            else
                if (y1 < y2)
            {
                angle = 90;
            }

            if (angle == 0)
            {
                if (x1 < x2)
                {
                    angle = 0;
                }
                else if (x1 > x2)
                {
                    angle = 180;
                }
            }
        }
    }
}
