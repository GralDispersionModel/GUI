using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
