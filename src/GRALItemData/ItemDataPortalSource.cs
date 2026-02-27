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
using GralStaticFunctions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GralItemData
{
    /// <summary>
    /// This class represents the portal source data
    /// </summary>
    [Serializable]
    public class PortalsData
    {
        public string Name { get; set; }
        public string Direction { get; set; }
        public int Crosssection { get; set; }
        public float Height { get; set; }
        public float BaseHeight { get; set; }
        public float ExitVel { get; set; }
        public float DeltaT { get; set; }
        public GralDomain.PointD Pt1 { get; set; }
        public GralDomain.PointD Pt2 { get; set; }
        public List<PollutantsData> Poll { get; set; }

        public NemoData Nemo { get; set; }

        private Deposition[] dep;
        public Deposition[] GetDep()
        {
            return dep;
        }
        public void SetDep(Deposition[] value)
        {
            dep = value;
        }

        public string VelocityTimeSeries { get; set; }
        public string TemperatureTimeSeries { get; set; }

        private static CultureInfo ic = CultureInfo.InvariantCulture;

        /// <summary>
        /// Create a new empty portal source data object
        /// </summary>
        public PortalsData()
        {
            Name = "Portal";
            Direction = "1";

            Poll = new List<PollutantsData>();
            Nemo = new NemoData();
            SetDep(new Deposition[10]);
            Pt1 = new PointD(0, 0);
            Pt2 = new PointD(0, 0);
            BaseHeight = 0;

            for (int i = 0; i < 10; i++)
            {
                GetDep()[i] = new Deposition(); // initialize Deposition array
                GetDep()[i].init();
            }
        }

        /// <summary>
        /// Create a new portal source data object from string
        /// </summary>
        public PortalsData(string sourcedata) // new item from string
        {
            int index = 0;
            int version = 0;
            string[] text = new string[1];
            text = sourcedata.Split(new char[] { ',' });
            SetDep(new Deposition[10]);
            Nemo = new NemoData();

            try
            {
                if (text.Length > 15) // otherwise the file is corrupt
                {
                    version = Convert.ToInt32(text[index++]);
                    Name = text[index++];
                    Height = (float)(St_F.TxtToDbl(text[index++], false));

                    ExitVel = (float)(St_F.TxtToDbl(text[index++], false));
                    DeltaT = (float)(St_F.TxtToDbl(text[index++], false));
                    Direction = text[index++];
                    Crosssection = Convert.ToInt32(text[index++]);

                    Nemo.AvDailyTraffic = Convert.ToInt32(text[index++]);
                    Nemo.ShareHDV = (float)(St_F.TxtToDbl(text[index++], false));
                    Nemo.Slope = (float)(St_F.TxtToDbl(text[index++], false));
                    Nemo.TrafficSit = Convert.ToInt32(text[index++]);
                    Nemo.BaseYear = Convert.ToInt32(text[index++]);

                    int sg_count = Convert.ToInt32(text[index++]);

                    if (sg_count > 0)
                    {
                        Poll = new List<PollutantsData>();
                        for (int sg = 0; sg < sg_count; sg++)
                        {
                            PollutantsData _poll = new PollutantsData
                            {
                                SourceGroup = Convert.ToInt32(text[index++])
                            };
                            for (int i = 0; i < 10; i++)
                            {
                                _poll.Pollutant[i] = Convert.ToInt32(text[index++]);
                                _poll.EmissionRate[i] = St_F.TxtToDbl(text[index++], false);
                            }
                            Poll.Add(_poll);
                        }
                    }

                    double x1 = St_F.TxtToDbl(text[index++], false);
                    double y1 = St_F.TxtToDbl(text[index++], false);
                    double x2 = St_F.TxtToDbl(text[index++], false);
                    double y2 = St_F.TxtToDbl(text[index++], false);
                    Pt1 = new PointD(x1, y1);
                    Pt2 = new PointD(x2, y2);

                    int depostart = text.Length;
                    for (int i = 4; i < text.Length; i++)
                    {
                        if (text[i] == "Dep@_")
                        {
                            depostart = i + 1;
                            break;
                        }
                    }

                    if (text.Length > depostart + 2) // read deposition
                    {
                        try
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                GetDep()[i] = new Deposition(); // initialize Deposition array
                                GetDep()[i].String_to_Val(depostart + i * 10, text);
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            GetDep()[i] = new Deposition(); // initialize Deposition array
                            GetDep()[i].init();
                        }
                    }

                    try
                    {
                        // get Base Height
                        if (text.Length > depostart + 10 * 10) // read vertical extension
                        {
                            BaseHeight = (float)(St_F.TxtToDbl(text[depostart + 10 * 10], false));
                        }
                        else
                        {
                            BaseHeight = 0; // old standard value = 0 m
                        }
                    }
                    catch
                    {
                        BaseHeight = 0; // old standard value = 0 m
                    }

                    for (int i = text.Length - 1; i > 3; i--)
                    {
                        if (text[i] == "Vel@_" && text.Length > (i + 1))
                        {
                            VelocityTimeSeries = text[i + 1];
                            break;
                        }
                    }

                    for (int i = text.Length - 1; i > 3; i--)
                    {
                        if (text[i] == "Temp@_" && text.Length > (i + 1))
                        {
                            TemperatureTimeSeries = text[i + 1];
                            break;
                        }
                    }
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            catch
            {
                Name = String.Empty;
                Direction = "1";
                Poll.Clear();

                Poll = new List<PollutantsData>();
                Nemo = new NemoData();
                SetDep(new Deposition[10]);
                Pt1 = new PointD();
                Pt2 = new PointD();
                for (int i = 0; i < 10; i++)
                {
                    GetDep()[i] = new Deposition(); // initialize Deposition array
                    GetDep()[i].init();
                }
            }
        }

        /// <summary>
        /// Create a new portal source data object from other object
        /// </summary>
        public PortalsData(PortalsData other) // Deep copy for new item
        {
            Name = other.Name;
            Direction = other.Direction;
            Crosssection = other.Crosssection;
            Height = other.Height;
            ExitVel = other.ExitVel;
            DeltaT = other.DeltaT;
            BaseHeight = other.BaseHeight;

            Pt1 = new PointD();
            Pt2 = new PointD();
            Pt1 = other.Pt1;
            Pt2 = other.Pt2;
            Crosssection = other.Crosssection;

            Poll = new List<PollutantsData>();
            foreach (PollutantsData _poll in other.Poll)
            {
                PollutantsData _pollnew = new PollutantsData();
                for (int i = 0; i < 10; i++)
                {
                    _pollnew.EmissionRate[i] = _poll.EmissionRate[i];
                    _pollnew.Pollutant[i] = _poll.Pollutant[i];
                }
                _pollnew.SourceGroup = _poll.SourceGroup;
                Poll.Add(_poll);
            }

            Nemo = new NemoData(other.Nemo);
            SetDep(new Deposition[10]);
            for (int i = 0; i < 10; i++)
            {
                GetDep()[i] = new Deposition(other.GetDep()[i]); // initialize Deposition array
            }

            if (other.TemperatureTimeSeries != null)
            {
                TemperatureTimeSeries = other.TemperatureTimeSeries;
            }
            if (other.VelocityTimeSeries != null)
            {
                VelocityTimeSeries = other.VelocityTimeSeries;
            }
        }

        /// <summary>
        /// Convert object data to a string as used in the item file
        /// </summary>
        public override string ToString() // compatibility to old source format
        {
            if (Name == null || Name == String.Empty)
            {
                Name = "PS";
            }

            string dummy = St_F.RemoveinvalidChars(Name) + "," +
                St_F.DblToIvarTxt(Math.Round(Height, 1)) + "," +
                St_F.DblToIvarTxt(Math.Round(ExitVel, 1)) + "," +
                St_F.DblToIvarTxt(Math.Round(DeltaT, 1)) + "," +
                Direction + "," +
                Convert.ToString(Crosssection) + "," +
                Nemo.ToString() + "," +
                Convert.ToString(Poll.Count) + ",";

            foreach (PollutantsData _poll in Poll)
            {
                dummy += _poll.SourceGroup.ToString() + ",";
                for (int i = 0; i < 10; i++)
                {
                    dummy += _poll.Pollutant[i].ToString() + "," +
                        St_F.DblToIvarTxt(_poll.EmissionRate[i]) + ",";
                }
            }

            dummy += St_F.DblToIvarTxt(Math.Round(Pt1.X, 1)) + "," +
                St_F.DblToIvarTxt(Math.Round(Pt1.Y, 1)) + "," +
                St_F.DblToIvarTxt(Math.Round(Pt2.X, 1)) + "," +
                St_F.DblToIvarTxt(Math.Round(Pt2.Y, 1)) + ",";

            dummy += "Dep@_,";
            for (int i = 0; i < 10; i++)
            {
                dummy += GetDep()[i].ToString() + ",";
            }

            dummy += St_F.DblToIvarTxt(Math.Round(BaseHeight, 1)) + ",";

            if (string.IsNullOrEmpty(VelocityTimeSeries) == false)
            {
                dummy += "Vel@_," + VelocityTimeSeries + ",";
            }

            if (string.IsNullOrEmpty(TemperatureTimeSeries) == false)
            {
                dummy += "Temp@_," + TemperatureTimeSeries + ",";
            }

            return dummy;
        }

        public double Lenght()
        {
            if (Pt1 != null && Pt2 != null)
            {
                return Math.Sqrt(Math.Pow(Pt1.X - Pt2.X, 2) + Math.Pow(Pt1.Y - Pt2.Y, 2));
            }
            else
            {
                return 0;
            }
        }

        public double Area()
        {
            return Height * Lenght();
        }
    }
}
