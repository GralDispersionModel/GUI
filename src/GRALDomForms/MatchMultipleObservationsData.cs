#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2020]  [Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.Collections.Generic;

namespace GralDomForms
{
    /// <summary>
    /// Settings for the match tuning process
    /// </summary>
    public class MatchMultipleObservationsData
    {
        /// <summary>
        /// 1 = vectorial method, 2 = component method
        /// </summary>
        public int Optimization = 1;
        /// <summary>
        /// Remove outliers?
        /// </summary>
        public bool Outliers = false;
        /// <summary>
        /// stronger weighting for stability classes 1 and 7?
        /// </summary>
        public bool StrongerWeightedSC1AndSC7 = true;
        /// <summary>
        /// buffer for the meteopgt.all data
        /// </summary>
        public List<GralData.PGTAll> PGT;
        /// <summary>
        /// Weighting for the wind vector
        /// </summary>
        public double[] WeightingFactor;
        /// <summary>
        /// Weighting for the wind direction in the component mode
        /// </summary>
        public double[] WeightingDirection;
        /// <summary>
        /// Weighting of meteo station for the automatic mode
        /// </summary>
        public double[] WeightingAutoMode;
        /// <summary>
        /// Sum of the vectorial wind vector error in several classes
        /// </summary>
        public int[,] VectorErrorSum;
        /// <summary>
        /// Sum of the stability class error within 0 or 1 classes
        /// </summary>
        public int[,] SCErrorSum;
        /// <summary>
        /// Use the automatic match mode?
        /// </summary>
        public bool AutomaticMode = false;
        /// <summary>
        /// Percentage for the bonus of an existing situation
        /// </summary>
        public int ReduceSituations = 0;
        /// <summary>
        /// Auto Mode: bit 0 = Vectorial, bit 1 = Components, bit 2 = Iterations
        /// </summary>
        public int AtomaticModePasses = 7;

        public MatchMultipleObservationsData()
        {
            PGT = new List<GralData.PGTAll>();
        }

        /// <summary>
        /// Init MMO Data
        /// </summary>
        /// <param name="Opti">Optimization mode</param>
        /// <param name="Outl">Remove Outliers</param>
        /// <param name="Stronger">SC1/7 stronger weighted</param>
        /// <param name="Auto">Use automatic mode</param>
        /// <param name="ReduceSituationNumber">Reduce number of situations in percent</param>
        /// <param name="AutoModePasses">Passes for Auto Mode</param>
        public MatchMultipleObservationsData(int Opti, bool Outl, bool Stronger, bool Auto, int ReduceSituationNumber, int AutoModePasses)
        {
            Optimization = Opti;
            Outliers = Outl;
            StrongerWeightedSC1AndSC7 = Stronger;
            AutomaticMode = Auto;
            ReduceSituations = ReduceSituationNumber;
            AtomaticModePasses = AutoModePasses;
            PGT = new List<GralData.PGTAll>();
        }

        /// <summary>
        /// Copy matching data 
        /// </summary>
        /// <param name="other">MMO Data</param>
        public MatchMultipleObservationsData(MatchMultipleObservationsData other)
        {
            Optimization = other.Optimization;
            Outliers = other.Outliers;
            AutomaticMode = other.AutomaticMode;
            StrongerWeightedSC1AndSC7 = other.StrongerWeightedSC1AndSC7;
            ReduceSituations = other.ReduceSituations;
            AtomaticModePasses = other.AtomaticModePasses;

            PGT = new List<GralData.PGTAll>();
            foreach (GralData.PGTAll _data in other.PGT)
            {
                PGT.Add(_data);
            }
            WeightingFactor = new double[other.WeightingFactor.Length];
            Array.Copy(other.WeightingFactor, WeightingFactor, other.WeightingFactor.Length);
            WeightingDirection = new double[other.WeightingDirection.Length];
            Array.Copy(other.WeightingDirection, WeightingDirection, other.WeightingDirection.Length);
            WeightingAutoMode = new double[other.WeightingAutoMode.Length];
            Array.Copy(other.WeightingAutoMode, WeightingAutoMode, other.WeightingAutoMode.Length);

            int len = other.VectorErrorSum.GetUpperBound(0) + 1;
            VectorErrorSum = new int[len, 4];
            Array.Copy(other.VectorErrorSum, VectorErrorSum, other.VectorErrorSum.Length);
            SCErrorSum = new int[len, 2];
            Array.Copy(other.SCErrorSum, SCErrorSum, other.SCErrorSum.Length);
        }
    }
}
