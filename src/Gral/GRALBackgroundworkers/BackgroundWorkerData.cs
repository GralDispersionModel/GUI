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
        /// <summary>
        /// Path to the GRAMM wind field folder
        /// </summary>
        public string Path_GRAMMwindfield { get; set; }
        /// <summary>
        /// Path to the project folder
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// Prefix for reslut file name
        /// </summary>
        public string Prefix {get; set;}
        /// <summary>
        /// Local culture decimal seperator
        /// </summary>
        public string DecSep { get; set; }
        /// <summary>
        /// Meteo file for reorder or mean wind velocity result
        /// </summary>
        public string MeteoFileName { get; set; }
        /// <summary>
        /// Backgroundworker progress form user text
        /// </summary>
        public string UserText { get; set; }
        /// <summary>
        /// Backgroundworker progress form caption
        /// </summary>
        public string Caption { get; set; }
        /// <summary>
        /// Type of backgroundworker calculation
        /// </summary>
        public BWMode BackgroundWorkerFunction { get; set; }
        /// <summary>
        /// GRAMM Subdomain X0 index 
        /// </summary>
        public int XDomain { get; set; }
        /// <summary>
        /// GRAMM Subdomain Y0 index
        /// </summary>
        public int YDomain { get; set; }
        /// <summary>
        /// GRAMM domain western border
        /// </summary>
        public double GrammWest { get; set; }
        /// <summary>
        /// GRAMM domain southern border
        /// </summary>
        public double GrammSouth { get; set; }
        /// <summary>
        /// Horizontal grid size for GRAMM flow field
        /// </summary>
        public double GRAMMhorgridsize { get; set; }
        /// <summary>
        /// Vertical index for mean wind velocity and reorder function
        /// </summary>
        public double VericalIndex { get; set; }
        /// <summary>
        /// selected source group names
        /// </summary>
        public string SelectedSourceGroup { get; set; } 
        /// <summary>
        /// computed source group names
        /// </summary>
        public string ComputedSourceGroup { get; set; } 
        /// <summary>
        /// Number of GRAL concentration cells in X direction
        /// </summary>
        public int CellsGralX { get; set; }
        /// <summary>
        /// Number of GRAL concentration cells in Y direction
        /// </summary>
        public int CellsGralY { get; set; }
        /// <summary>
        /// Max. number of calculated source groups
        /// </summary>
        public int MaxSource { get; set; }
        /// <summary>
        /// Max. index of calculated sourc egroups
        /// </summary>
        public int MaxSourceComputed { get; set; }
        /// <summary>
        /// Percentil value for high percentile calculation
        /// </summary>
        public decimal Percentile { get; set; }
        /// <summary>
        /// Emission factor for odour calculation
        /// </summary>
        public float EmissionFactor { get; set; }
        /// <summary>
        /// Horizontal grid size for concentration grid
        /// </summary>
        public double Horgridsize { get; set; }
        /// <summary>
        /// Vertical grid size for concentration grid
        /// </summary>
        public double VertgridSize { get; set; }
        /// <summary>
        /// GRAL domain western border
        /// </summary>
        public double DomainWest { get; set; }
        /// <summary>
        /// GRAL domain southern border
        /// </summary>
        public double DomainSouth { get; set; }
        /// <summary>
        /// Polutant name
        /// </summary>
        public string Pollutant { get; set; }
        /// <summary>
        /// Index of selected horizontal slice
        /// </summary>
        public int Slice { get; set; }
        /// <summary>
        /// Clice heights for horizontal slices
        /// </summary>
        public double[] SliceHeights { get; set; }
        /// <summary>
        /// Calculate DayMay value
        /// </summary>
        public bool CalculateDayMax { get; set; }
        /// <summary>
        /// Classified meteorology? True = not classified
        /// </summary>
        public bool MeteoNotClassified { get; set; }
        /// <summary>
        /// Calculate Mean value
        /// </summary>
        public bool CalculateMean { get; set; }
        /// <summary>
        /// Calculate MaxHour value
        /// </summary>
        public bool CalculateMaxHour { get; set; }
        public string ListBox5_SelectedItem { get; set; }
        /// <summary>
        /// Name for horizontal slice
        /// </summary>
        public string Slicename { get; set; }
        /// <summary>
        /// Internal used StreamWriter for result files
        /// </summary>
        public StreamWriter Writer { get; set; }
        /// <summary>
        /// Threshold for odor calculation
        /// </summary>
        public decimal OdourThreshold { get; set; }
        /// <summary>
        /// Peak to Mean value for odor calculation
        /// </summary>
        public int Peakmean { get; set; }
        /// <summary>
        /// Odor emission for compost odor calculation
        /// </summary>
        public double[] Odemi { get; set; }
        /// <summary>
        /// Odor frequency for compost odor calculation
        /// </summary>
        public double[] OdFreq { get; set; }
        /// <summary>
        /// Scale for compost odor calculation
        /// </summary>
        public double Scale { get; set; }
        /// <summary>
        /// Breeding cycle duration for all in all out odor calculation
        /// </summary>
        public float Division { get; set; }
        /// <summary>
        /// Empty stable time for all in all out odor calculation
        /// </summary>
        public float Emptytimes { get; set; }
        /// <summary>
        /// Source groups for all in all out odor calculation
        /// </summary>
        public string AllInnSelSourceGroup { get; set; }
        /// <summary>
        /// File name for raster A
        /// </summary>
        public string RasterA { get; set; }
        /// <summary>
        /// File name for raster B
        /// </summary>
        public string RasterB { get; set; }
        /// <summary>
        /// File name for raster C
        /// </summary>
        public string RasterC { get; set; }
        /// <summary>
        /// File name for raster D
        /// </summary>
        public string RasterD { get; set; }
        /// <summary>
        /// File name for raster G
        /// </summary>
        public string RasterE { get; set; }
        /// <summary>
        /// File name for raster (output) F
        /// </summary>
        public string RasterF { get; set; }
        /// <summary>
        /// Equation for mathraster operation
        /// </summary>
        public string MathRasterEquation { get; set; }
        /// <summary>
        /// Filename for result file
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// Unit for result file
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// Use local stabilit classes?
        /// </summary>
        public bool LocalStability { get; set; }
        /// <summary>
        /// List with evaluation points
        /// </summary>
        public List<Point_3D> EvalPoints { get; set; }
        /// <summary>
        /// Cell numbers for GRAMM domain
        /// </summary>
        public CellNumbers GRAMMCells { get; set; }
        /// <summary>
        /// Sub domain cell numbers for GRAMM export
        /// </summary>
        public CellNumbers GRAMMSubCells { get; set; }
        /// <summary>
        /// Write additional deposition or odor files?
        /// </summary>
        public bool WriteDepositionOrOdourData { get; set; }
        /// <summary>
        /// Grid size for flow field or concentration raster
        /// </summary>
        public double GFFGridSize { get; set; }
        /// <summary>
        /// Year for meteo time series
        /// </summary>
        public int FictiousYear { get; set; }
    }

    public class CellNumbers
    {
        public int NX { get; set; }
        public int NY { get; set; }
        public int NZ { get; set; }
    }
}
