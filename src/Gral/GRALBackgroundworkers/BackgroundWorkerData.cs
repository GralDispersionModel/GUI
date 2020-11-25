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
 * Date: 15.01.2019
 * Time: 17:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.IO;
using System.Collections.Generic;

namespace GralBackgroundworkers
{
    public enum BWMode
    {
        None = 0,
        GrammMetFile = 1,
        ReOrder = 2,
        GralMetFile = 3,
        OdorConcentrationPercentile = 23,
        OdorHoursTransient = 24,
        MeanMaxTimeSeries = 25,
        OdorCompost = 26,
        OdorHours = 27,
        MeanMeteoPGT = 28,
        OdorAllinAllout = 29,
        GrammMeanWindVel = 31,
        ReceptorTimeSeries = 37,
        EvalPointsTimeSeries = 38,
        HighPercentiles = 40,
        MathRasterOperations = 50,
        GrammExportSubDomain = 60
    }

    /// <summary>
    /// Class to collect data used by the Backgroundworker
    /// </summary>
    public class BackgroundworkerData
    {
        public string Path_GRAMMwindfield { get; set; }
        public string Projectname { get; set; }
        public string Prefix {get; set;}
        public string Decsep { get; set; }
        public string Meteofilename { get; set; }
        public string UserText { get; set; }
        public string Caption { get; set; }
        public BWMode Rechenart { get; set; }
        public int XDomain { get; set; }
        public int YDomain { get; set; }
        public double GrammWest { get; set; }
        public double GrammSouth { get; set; }
        public double GRAMMhorgridsize { get; set; }
        public double Schnitt { get; set; }
        public string Sel_Source_Grp { get; set; } // Contains the selected sourcegroup-Names
        public string Comp_Source_Grp { get; set; } // Contains the computed sourcegroup-Names
        public int CellsGralX { get; set; }
        public int CellsGralY { get; set; }
        public int MaxSource { get; set; }
        public int MaxSourceComputed { get; set; }
        public decimal Percentile { get; set; }
        public float EmissionFactor { get; set; }
        public double Horgridsize { get; set; }
        public double VertgridSize { get; set; }
        public double DomainWest { get; set; }
        public double DomainSouth { get; set; }
        public string Pollutant { get; set; }
        public int Slice { get; set; }
        public double[] SliceHeights { get; set; }
        public bool Checkbox3 { get; set; }
        public bool Checkbox19 { get; set; }
        public bool Checkbox2 { get; set; }
        public bool Checkbox1 { get; set; }
        public string ListBox5_SelectedItem { get; set; }
        public string Slicename { get; set; }
        public StreamWriter Writer { get; set; }
        public decimal OdourThreshold { get; set; }
        public int Peakmean { get; set; }
        public double[] Odemi { get; set; }
        public double[] OdFreq { get; set; }
        public double Scale { get; set; }
        public float Division { get; set; }
        public float Emptytimes { get; set; }
        public string AllInn_Sel_Source_Grp { get; set; }
        public string RasterA { get; set; }
        public string RasterB { get; set; }
        public string RasterC { get; set; }
        public string RasterD { get; set; }
        public string RasterE { get; set; }
        public string RasterF { get; set; }
        public string TextBox1 { get; set; }
        public string Filename { get; set; }
        public string Unit { get; set; }
        public bool LocalStability { get; set; }
        public List<Point_3D> EvalPoints { get; set; }
        public CellNumbers GRAMMCells { get; set; }
        public CellNumbers GRAMMSubCells { get; set; }
        public bool WriteDepositionOrOdourData { get; set; }
        public double GFFGridSize { get; set; }
    }

    public class CellNumbers
    {
        public int NX { get; set; }
        public int NY { get; set; }
        public int NZ { get; set; }
    }
}
