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
        public int Optimization = 1;
        public bool Outliers = false;
        public bool StrongerWeightedSC1AndSC7 = true;
        public List <GralData.PGTAll> PGT;
        public double[] WeightingFactor;
        public double[] WeightingDirection;
        public int[,] VectorErrorSum;
        public int[,] SCErrorSum;
        public bool AutomaticMode = false;

        public MatchMultipleObservationsData()
        {
            PGT = new List<GralData.PGTAll>();
        }

        public MatchMultipleObservationsData(int Opti, bool Outl, bool Stronger, bool Auto)
        {
            Optimization = Opti;
            Outliers = Outl;
            StrongerWeightedSC1AndSC7 = Stronger;
            AutomaticMode = Auto;
            PGT = new List<GralData.PGTAll>();
        }

        /// <summary>
        /// Copy matching data 
        /// </summary>
        /// <param name="other"></param>
        public MatchMultipleObservationsData(MatchMultipleObservationsData other)
        {
            Optimization = other.Optimization;
            Outliers = other.Outliers;
            AutomaticMode = other.AutomaticMode;
            StrongerWeightedSC1AndSC7 = other.StrongerWeightedSC1AndSC7;

            PGT = new List<GralData.PGTAll>();
            foreach (GralData.PGTAll _data in other.PGT)
            {
                PGT.Add(_data);
            }
            WeightingFactor = new double[other.WeightingFactor.Length];
            Array.Copy(other.WeightingFactor, WeightingFactor, other.WeightingFactor.Length);
            WeightingDirection = new double[other.WeightingDirection.Length];
            Array.Copy(other.WeightingDirection, WeightingDirection, other.WeightingDirection.Length);
            int len = other.VectorErrorSum.GetUpperBound(0) + 1;
            VectorErrorSum = new int[len, 4];
            Array.Copy(other.VectorErrorSum, VectorErrorSum, other.VectorErrorSum.Length);
            SCErrorSum = new int[len, 2];
            Array.Copy(other.SCErrorSum, SCErrorSum, other.SCErrorSum.Length);
        }
    }
}
