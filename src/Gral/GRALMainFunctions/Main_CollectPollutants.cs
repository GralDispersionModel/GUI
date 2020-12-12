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
 * Date: 28.10.2016
 * Time: 18:26
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using GralStaticFunctions;
using GralItemData;

namespace Gral
{
    public partial class Main
    {
        /// <summary>
        /// Collect all pollutants within the defined model domain
        /// </summary>
        private void CollectAllUsedPollutants()
        {
            listBox5.Items.Clear();
            ChangeButtonLabel(Gral.ButtonColorEnum.ButtonEmission, ButtonColorEnum.RedDot); // Emission label red
            
            List <int> SelectedSourceGroups = new List<int>();
            List <PollutantsData> AllPollutants    = new List<PollutantsData>();
            AllPollutants = ReadAllPollutants();  // read all pollutants into this List
            
            //read the seleceted source groups for the simulation
            for(int i = 0; i < listView1.Items.Count; i++)
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
            
            //when the current source group is selected by the user, the corresponding pollutants are added
            foreach(PollutantsData _poll in AllPollutants)
            {
                if (SelectedSourceGroups.Contains(_poll.SourceGroup)) // SG is selected
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (_poll.EmissionRate[i] > 0 && Pollmod.Contains(PollutantList[_poll.Pollutant[i]]) == false)
                        {
                            Pollmod.Add(PollutantList[_poll.Pollutant[i]]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Read pollutants from all sources within the GRAL domain area into this list
        /// </summary>
        private List <PollutantsData> ReadAllPollutants()
        {
            List<PollutantsData> AllPollutants = new List<PollutantsData>();
            List <PointSourceData> _psList = new List<PointSourceData>();
            List <AreaSourceData> _asList  = new List<AreaSourceData>();
            List <LineSourceData> _lsList  = new List<LineSourceData>();
            List <PortalsData>    _poList  = new List<PortalsData>();
            
            // load point sources
            PointSourceDataIO _ps = new PointSourceDataIO();
            string _file = Path.Combine(Main.ProjectName,"Emissions","Psources.txt");
            _ps.LoadPointSources(_psList, _file);
            _ps = null;
            foreach(PointSourceData _psdata in _psList)
            {
                if (( _psdata.Pt.X >= GralDomRect.West) && (_psdata.Pt.X <= GralDomRect.East) &&
                    ( _psdata.Pt.Y >= GralDomRect.South) && (_psdata.Pt.Y <= GralDomRect.North) && _psdata.Poll != null)
                {
                    AllPollutants.Add(_psdata.Poll);
                }
            }
            _psList.Clear();
            _psList.TrimExcess();
            _psList = null;
            
            // load area sources
            AreaSourceDataIO _as = new AreaSourceDataIO();
            _file = Path.Combine(Main.ProjectName,"Emissions","Asources.txt");
            _as.LoadAreaData(_asList, _file);
            _as = null;
            foreach(AreaSourceData _asdata in _asList)
            {
                double xmin = double.MaxValue;
                double xmax = double.MinValue;
                double ymin = double.MaxValue;
                double ymax = double.MinValue;
                foreach (GralDomain.PointD _pt in _asdata.Pt)
                {
                    xmin = Math.Min(xmin, _pt.X);
                    xmax = Math.Max(xmax, _pt.X);
                    ymin = Math.Min(ymin, _pt.Y);
                    ymax = Math.Max(ymax, _pt.Y);
                }
                if ((xmin >= GralDomRect.West) && (xmax <= GralDomRect.East) && (ymin >= GralDomRect.South) && (ymax <= GralDomRect.North) && _asdata.Poll != null)
                {
                    AllPollutants.Add(_asdata.Poll);
                }
            }
            _asList.Clear();
            _asList.TrimExcess();
            _asList = null;
            
            // load line sources
            LineSourceDataIO _ls = new LineSourceDataIO();
            _file = Path.Combine(Main.ProjectName,"Emissions","Lsources.txt");
            _ls.LoadLineSources(_lsList, _file);
            _ls = null;
            foreach(LineSourceData _lsdata in _lsList)
            {
                double xmin = double.MaxValue;
                double xmax = double.MinValue;
                double ymin = double.MaxValue;
                double ymax = double.MinValue;
                double lenght = St_F.CalcLenght(_lsdata.Pt) / 1000;
                
                foreach (GralData.PointD_3d _pt in _lsdata.Pt)
                {
                    xmin = Math.Min(xmin, _pt.X);
                    xmax = Math.Max(xmax, _pt.X);
                    ymin = Math.Min(ymin, _pt.Y);
                    ymax = Math.Max(ymax, _pt.Y);
                }
                
                if ((xmin >= GralDomRect.West) && (xmax <= GralDomRect.East) && (ymin >= GralDomRect.South) && (ymax <= GralDomRect.North) && _lsdata.Poll != null)
                {
                    foreach(PollutantsData _lspoll in _lsdata.Poll)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            _lspoll.EmissionRate[i] *= lenght;
                        }
                        AllPollutants.Add(_lspoll);
                    }
                }
            }
            _lsList.Clear();
            _lsList.TrimExcess();
            _lsList = null;
            
            // load portal data
            PortalsDataIO _pd = new PortalsDataIO();
            _file = Path.Combine(Main.ProjectName,"Emissions","Portalsources.txt");
            _pd.LoadPortalSources(_poList, _file);
            _pd = null;
            foreach(PortalsData _podata in _poList)
            {
                double xmin = double.MaxValue;
                double xmax = double.MinValue;
                double ymin = double.MaxValue;
                double ymax = double.MinValue;
                xmin = Math.Min(xmin, _podata.Pt1.X);
                xmax = Math.Max(xmax, _podata.Pt1.X);
                ymin = Math.Min(ymin, _podata.Pt1.Y);
                ymax = Math.Max(ymax, _podata.Pt1.Y);
                xmin = Math.Min(xmin, _podata.Pt2.X);
                xmax = Math.Max(xmax, _podata.Pt2.X);
                ymin = Math.Min(ymin, _podata.Pt2.Y);
                ymax = Math.Max(ymax, _podata.Pt2.Y);
                
                if ((xmin >= GralDomRect.West) && (xmax <= GralDomRect.East) && (ymin >= GralDomRect.South) && (ymax <= GralDomRect.North) && _podata.Poll != null)
                {
                    foreach(PollutantsData _popoll in _podata.Poll)
                    {
                        AllPollutants.Add(_popoll);
                    }
                }
            }
            _poList.Clear();
            _poList.TrimExcess();
            _poList = null;
            
            return AllPollutants;
        }

    }
}