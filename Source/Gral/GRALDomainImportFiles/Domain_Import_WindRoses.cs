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
 * User: Markus Kuntner
 * Date: 31.12.2018
 * Time: 16:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using GralIO;
using GralDomForms;
using GralData;

namespace GralDomain
{
	public partial class Domain
	{
		private CultureInfo ic = CultureInfo.InvariantCulture;
		
		// import a wind rose at a given point
		/// <summary>
		/// Import wind roses from *.met file
		/// </summary>
		void Button53Click(object sender, EventArgs e)
		{
			using (DomainWindRoseDialog _wrose = new DomainWindRoseDialog())
			{
				int index = -1;
				for (int i = 0; i < ItemOptions.Count; i++)
				{
					if (ItemOptions[i].Name == "WINDROSE")
					{
						index = i;
					}
				}
				
				DrawingObjects _dr;
				// new Object
				if (index < 0)
				{
					_dr = new DrawingObjects("WINDROSE");
				}
				else
				{
					_dr = ItemOptions[index];
				}
				
				_wrose.DrawingObject = _dr;
                _wrose.StartPosition = FormStartPosition.Manual;
                _wrose.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
                _wrose.Top = 80;

				DialogResult wroseResult = _wrose.ShowDialog();
				
				// Add to Drawing list
				if (wroseResult == DialogResult.OK && index < 0 && !string.IsNullOrEmpty(_dr.ContourFilename))
				{
					ItemOptions.Insert(0, _dr);
				}
				
				// Remove from Drawing list
				if (index >= 0 && string.IsNullOrEmpty(_dr.ContourFilename))
				{
					RemoveItemFromItemOptions("WINDROSE");
				}
				
				// Reload WindRoseData
				if (wroseResult == DialogResult.OK)
				{
					LoadWindroseData(_dr);
				}
			}
			
			SaveDomainSettings(1);
			Picturebox1_Paint();
		}
		
		private void LoadWindroseData(DrawingObjects _dr)
		{
			List<WindData> MeteoTimeSeries = new List<WindData>();      // time series of meteorological  data
		    List <WindFileData> Windfiles = new List<WindFileData>();
			_dr.ContourPoints.Clear();
			_dr.ContourPoints.TrimExcess();
			
			string[] w = _dr.ContourFilename.Split(new char[] { '\t' });
			Windfiles.Clear();
			Windfiles.TrimExcess();
			
			foreach(string _wnd in w)
			{
				string[] f = _wnd.Split(new char[] { '?' }); // get 1st entry = filename
				if (f.Length > 2)
				{
					try
					{
                        WindFileData _wdata = new WindFileData
                        {
                            Filename = f[0],
                            DecSep = f[1],
                            RowSep = Convert.ToChar(f[2]),
                            X0 = Convert.ToDouble(f[3], ic),
                            Y0 = Convert.ToDouble(f[4], ic),
                            Z0 = Convert.ToDouble(f[5], ic)
                        };
                        Windfiles.Add(_wdata);		
					}
					catch{}
				}
			}

            foreach (WindFileData _wdata in Windfiles)
			{
				MeteoTimeSeries.Clear();
				MeteoTimeSeries.TrimExcess();

                
                string _filename = _wdata.Filename;
                if (! System.IO.File.Exists(_filename))
                {
                    // check if file exists in the meteo folder
                    if (System.IO.File.Exists(System.IO.Path.Combine(
                        Gral.Main.ProjectName, "Meteo", System.IO.Path.GetFileName(_filename))))
                    {
                        _filename = System.IO.Path.Combine(Gral.Main.ProjectName, "Meteo", System.IO.Path.GetFileName(_filename));
                    }
                }

                IO_ReadFiles readwindfile = new IO_ReadFiles
                {
                    WindDataFile = _filename,
                    WindData = MeteoTimeSeries
                };

                readwindfile.ReadMeteoFiles(1000000, _wdata.RowSep, decsep, _wdata.DecSep);
				AddWindDataToObjectList(MeteoTimeSeries, _dr, _wdata.MaxValue, _wdata.X0, _wdata.Y0);
			}
		}
		
		private void AddWindDataToObjectList(List<GralData.WindData> MeteoTimeSeries, DrawingObjects _dr, int maxwind, double x0, double y0)
		{
			double[,] sectfrequency = new double[16, 8];
			double[] wndclasses = new double[7] { 0.5, 1, 2, 3, 4, 5, 6 };
			int count = 0;
			int MaxWindSpeedClasses = maxwind + 1;
            float sectorWidth = 1;
            if (_dr.ContourDrawBorder)
            {
                sectorWidth = GralStaticFunctions.GetMetFileSectionWidth.GetMetSectionWidth(MeteoTimeSeries);
            }

            if (maxwind <= 6)
				wndclasses = new double[] { 0.5, 1, 2, 3, 4, 5, 6 };
			else if (maxwind <= 7)
				wndclasses = new double[] { 0.5, 1, 2, 3, 4, 5, 7 };
			else if (maxwind <= 8)
				wndclasses = new double[] { 0.5, 1, 2, 3, 4, 6, 8 };
			else if (maxwind <= 10)
				wndclasses = new double[] { 0.5, 1, 2, 3, 4, 7, 10 };
			else if (maxwind <= 14)
				wndclasses = new double[] { 0.5, 1, 2, 4, 7, 10, 14 };
			else if (maxwind <= 20)
				wndclasses = new double[] { 0.5, 1, 2, 4, 8, 12, 20 };

			foreach (WindData data in MeteoTimeSeries)
			{
				{
					int sektor = Convert.ToInt32(Math.Round(data.Dir / 22.5, 0));
					if (sektor > 15)
					  sektor = 0;
					  
					if (_dr.Filter) // show wind velocity
					{
					    int wklass = 0; //Convert.ToInt32(Math.Truncate(windge[i])) + 1;

					    for (int c = 0; c < 6; c++)
					    {
					        if (data.Vel > wndclasses[c] && data.Vel <= wndclasses[c + 1])
					            wklass = c + 1;
					    }

					    if (data.Vel <= wndclasses[0])
					        wklass = 0;
					    if (data.Vel > wndclasses[6])
					        wklass = 7;
                        if (_dr.ContourDrawBorder && sectorWidth > 1)
                        {
                            double start = data.Dir - sectorWidth * 0.5F;
                            double ende  = data.Dir + sectorWidth * 0.5F;

                            for (double subsect = start; subsect < ende; subsect += 0.5)
                            {
                                double _sect = subsect;
                                if (_sect < 0)
                                {
                                    _sect += 360;
                                }
                                if (_sect > 360)
                                {
                                    _sect -= 360;
                                }
                                sektor = (int) (Math.Round(_sect / 22.5, 0));
                                if (sektor > 15)
                                {
                                    sektor = 0;
                                }
                                count++;
                                sectfrequency[sektor, wklass]++;
                            }
                        }
                        else
                        {
                            count++;
                            sectfrequency[sektor, wklass]++;
                        }
                        
					}
					else // show stability class
					{
                        if (_dr.ContourDrawBorder && sectorWidth > 1) //BiasCorrection
                        {
                            double start = data.Dir - sectorWidth * 0.5F;
                            double ende  = data.Dir + sectorWidth * 0.5F;

                            for (double subsect = start; subsect < ende; subsect += 0.5)
                            {
                                double _sect = subsect;
                                if (_sect < 0)
                                {
                                    _sect += 360;
                                }
                                if (_sect > 360)
                                {
                                    _sect -= 360;
                                }
                                sektor = (int) (Math.Round(_sect / 22.5, 0));
                                if (sektor > 15)
                                {
                                    sektor = 0;
                                }
                                count++;
                                data.StabClass = Math.Max(1, Math.Min(7, data.StabClass));
                                sectfrequency[sektor, data.StabClass]++;
                            }
                        }
                        else
                        {
                            count++;
                            data.StabClass = Math.Max(1, Math.Min(7, data.StabClass));
                            sectfrequency[sektor, data.StabClass]++;
                        }                       
					}
				}
			}
			if (count == 0)
			{
				count = 1;
			}
			
			for (int sektor = 0; sektor < 16; sektor++)
			{
                for (int wklass = 0; wklass < 8; wklass++)
                {
                    sectfrequency[sektor, wklass] /= Convert.ToDouble(count);
                }
			}
			
			_dr.ContourPoints.Add(new List<PointF>());
			int index = _dr.ContourPoints.Count - 1;
			_dr.ContourPoints[index].Add(new PointF((float) x0, (float) y0));
			
			for (int sektor = 0; sektor < 16; sektor++)
			{
				for (int wklass = 0; wklass < 7; wklass+=2)
				{
					_dr.ContourPoints[index].Add(new PointF((float) sectfrequency[sektor, wklass], (float) sectfrequency[sektor, wklass + 1]));
				}
			}
			//MessageBox.Show(_dr.ContourPoints[index].Count.ToString());
		}
	}
}