﻿#region Copyright
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
using System.Linq;
using System.Windows.Forms;

namespace GralDomForms
{
    /// <summary>
    /// Match GRAMM wind fields to multiple oberservation stations dialog
    /// </summary>
    public partial class MatchMultipleObservations : Form
    {
        private void AutoTuning()
        {
            this.Hide();
            double[] testValuesVector = new double[9] { 0.01, 0.1, 0.5, 1, 5, 10, 20, 40, 60 };
            double[] testValuesDirection = new double[5] { 0.1, 0.5, 1, 3, 6 };
            double[] Error = new double[MetFileNames.Count];
            double[] BestMatchFactor = new double[MetFileNames.Count];
            double[] BestMatchDirection = new double[MetFileNames.Count];
            int[] BestMatchMode = new int[MetFileNames.Count];
           
            for (int i = 0; i < MetFileNames.Count; i++)
            {
                Error[i] = 20000000;
            }

            GralMessage.WaitProgressbarCancel wait = new GralMessage.WaitProgressbarCancel("");
#if __MonoCS__
            wait.Width = 350;
#endif
            // Check, if all weighting factors are 1 -> otherwise only iterative solution
            wait.Text = "Automatic Tuning - Pass 1/3";
            wait.Show();

            if (!OnlyIterativeTuning)
            {
                wait.BeginInvoke(wait.UpdateProgressDelegate, this, testValuesVector.Count());
                // 1st guess: brute force test for all meteo stations
                // Mode 1
                foreach (double fact in testValuesVector)
                {
                    if (!GralDomain.Domain.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        wait.BeginInvoke(wait.UpdateProgressDelegate, this, 0);
                        Application.DoEvents();
                        //local copy of Matching data
                        MatchMultipleObservationsData _m = new MatchMultipleObservationsData(MatchingData);
                        _m.AutomaticMode = true;
                        _m.Optimization = 1;
                        // Set all factors
                        for (int j = 0; j < MetFileNames.Count; j++)
                        {
                            _m.WeightingFactor[j] = fact;
                        }
                        Array.Clear(_m.SCErrorSum, 0, _m.SCErrorSum.Length);
                        Array.Clear(_m.VectorErrorSum, 0, _m.VectorErrorSum.Length);

                        MatchTuning(_m, GralDomain.Domain.CancellationTokenSource.Token);
                        double err = AutoTuningError(_m.VectorErrorSum, _m.SCErrorSum);

                        for (int i = 0; i < MetFileNames.Count; i++)
                        {
                            if (err < Error[i])
                            {
                                Error[i] = err;
                                BestMatchDirection[i] = _m.WeightingDirection[i];
                                BestMatchFactor[i] = _m.WeightingFactor[i];
                                BestMatchMode[i] = _m.Optimization;
                            }
                        }

                    }
                }

                wait.Text = "Automatic Tuning - Pass 2/3";
                wait.BeginInvoke(wait.UpdateProgressDelegate, this, testValuesDirection.Count() * testValuesVector.Count());
                // 2nd guess: check Mode 2
                foreach (double fact in testValuesVector)
                {
                    if (!GralDomain.Domain.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        //local copy of Matching data
                        MatchMultipleObservationsData _m = new MatchMultipleObservationsData(MatchingData);
                        _m.AutomaticMode = true;
                        _m.Optimization = 2;

                        foreach (double dirfact in testValuesDirection)
                        {
                            wait.BeginInvoke(wait.UpdateProgressDelegate, this, 0);
                            Application.DoEvents();
                            Array.Clear(_m.SCErrorSum, 0, _m.SCErrorSum.Length);
                            Array.Clear(_m.VectorErrorSum, 0, _m.VectorErrorSum.Length);
                            // Set all factors
                            for (int j = 0; j < MetFileNames.Count; j++)
                            {
                                _m.WeightingFactor[j] = fact;
                                _m.WeightingDirection[j] = dirfact;
                            }

                            MatchTuning(_m, GralDomain.Domain.CancellationTokenSource.Token);
                            double err = AutoTuningError(_m.VectorErrorSum, _m.SCErrorSum);

                            for (int i = 0; i < MetFileNames.Count; i++)
                            {
                                if (err < Error[i])
                                {
                                    Error[i] = err;
                                    BestMatchDirection[i] = _m.WeightingDirection[i];
                                    BestMatchFactor[i] = _m.WeightingFactor[i];
                                    BestMatchMode[i] = _m.Optimization;
                                }
                            }

                        }
                    }
                }
            } // only iterative tuning
            else
            {
                for (int i = 0; i < MetFileNames.Count; i++)
                {
                    BestMatchDirection[i] = MatchingData.WeightingDirection[i];
                    BestMatchFactor[i] = MatchingData.WeightingFactor[i];
                    BestMatchMode[i] = MatchingData.Optimization;
                }
            }

            //Iterative fine tuning for all stations
            {
                List<int> MetfileList = new List<int>();
                for (int j = 0; j < MetFileNames.Count; j++)
                {
                    MetfileList.Add(j);
                }
                // Add station 0 at the end of the iteration list -> improve 2 times per iteration
                if (MetFileNames.Count > 1)
                {
                    MetfileList.Add(0);
                }
                Random rnd = new Random();
                wait.BeginInvoke(wait.UpdateProgressDelegate, this, Math.Max(5, (int)(numericUpDown1.Value * MetfileList.Count)));

                for (int iterations = 0; iterations < (int)numericUpDown1.Value; iterations++)
                {
                    wait.Text = "Automatic Tuning - Pass 3/3-iter: " + (iterations + 1).ToString();
                    MetfileList.Reverse();

                    foreach (int i in MetfileList)
                    {
                        wait.BeginInvoke(wait.UpdateProgressDelegate, this, 0);
                        Application.DoEvents();
                        if (!GralDomain.Domain.CancellationTokenSource.Token.IsCancellationRequested)
                        {
                            //local copy of Matching data
                            MatchMultipleObservationsData _m = new MatchMultipleObservationsData(MatchingData);
                            _m.AutomaticMode = true;
                            _m.Optimization = BestMatchMode[0];
                            Array.Copy(BestMatchFactor, _m.WeightingFactor, BestMatchFactor.Length);
                            Array.Copy(BestMatchDirection, _m.WeightingDirection, BestMatchDirection.Length);

                            int direction = 1;
                            double _fac = 1.2;
                            if (BestMatchFactor[i] < 1)
                            {
                                _fac = 1.3;
                            }

                            for (int j = 0; j < 3; j++)
                            {
                                Array.Clear(_m.SCErrorSum, 0, _m.SCErrorSum.Length);
                                Array.Clear(_m.VectorErrorSum, 0, _m.VectorErrorSum.Length);
                                _m.WeightingFactor[i] = _m.WeightingFactor[i] * Math.Pow(_fac, direction);

                                MatchTuning(_m, GralDomain.Domain.CancellationTokenSource.Token);
                                double err = AutoTuningError(_m.VectorErrorSum, _m.SCErrorSum);
                                if (err < Error[i])
                                {
                                    Error[i] = err;

                                    BestMatchDirection[i] = Round3Digits(_m.WeightingDirection[i]);
                                    BestMatchFactor[i] = Round3Digits(_m.WeightingFactor[i]);
                                    BestMatchMode[i] = _m.Optimization;
                                }
                                else if (j < 1) // not improved -> try to reduce the factor
                                {
                                    direction *= -1;
                                    // reset weighting factor
                                    _m.WeightingFactor[i] = _m.WeightingFactor[i] * Math.Pow(_fac, direction);
                                }
                                else // neither up nor down improved the model -> try random values
                                {
                                    direction = 0;
                                    _m.WeightingFactor[i] = Math.Max(0, _m.WeightingFactor[i] + _m.WeightingFactor[i] * (0.75 - rnd.NextDouble()));
                                }

                                if (_m.WeightingFactor[i] > 100)
                                {
                                    j = 1000; // cancel
                                }
                            }
                        }
                    }
                }
            }

            Array.Clear(MatchingData.SCErrorSum, 0, MatchingData.SCErrorSum.Length);
            Array.Clear(MatchingData.VectorErrorSum, 0, MatchingData.VectorErrorSum.Length);
            Array.Copy(BestMatchDirection, MatchingData.WeightingDirection, BestMatchDirection.Length);
            Array.Copy(BestMatchFactor, MatchingData.WeightingFactor, BestMatchFactor.Length);

            MatchingData.Optimization = BestMatchMode[0];
            WriteMetTimeSeries(MatchTuning(MatchingData, GralDomain.Domain.CancellationTokenSource.Token));
            SetTuningResults(MatchingData);

            wait.Close();
            wait.Dispose();

            GralDomain.Domain.CancellationTokenSource.Dispose();
            GralDomain.Domain.CancellationTokenSource = null;
            if (GralDomain.Domain.CancellationTokenSource == null)
            {
                GralDomain.Domain.CancellationTokenSource = new System.Threading.CancellationTokenSource();
            }

            Show();
        }

        /// <summary>
        /// Calculate the weighted error sum for the meteo station Index 
        /// </summary>
        /// <param name="VectorErrorSum">Vector error values</param>
        /// <param name="SCErrorSum">SC error values</param>
        /// <param name="Index">Number of the meteo station</param>
        /// <returns></returns>
        private double AutoTuningError(int[,] VectorErrorSum, int[,] SCErrorSum, int Index)
        {
            double meteopgtlen = Math.Max(1, MetFileLenght[0]); //avoid integer division
            double err = ((1.0 - VectorErrorSum[Index, 2] / meteopgtlen) * (100 - hScrollBar1.Value) + (1.0 - SCErrorSum[Index, 1] / meteopgtlen) * hScrollBar1.Value) / 100;
            return err;
        }
        /// <summary>
        /// Calculate the weighted error sum for all meteo stations 
        /// </summary>
        /// <param name="VectorErrorSum">Vector error values</param>
        /// <param name="SCErrorSum">SC error values</param>
        /// <returns></returns>
        private double AutoTuningError(int[,] VectorErrorSum, int[,] SCErrorSum)
        {
            double err = 0;
            double meteopgtlen = Math.Max(1, MetFileLenght[0]);  //avoid integer division
            for (int i = 0; i < MetFileNames.Count; i++)
            {
               err += ((1.0 - VectorErrorSum[i, 2] / meteopgtlen) * 1.2 * (100 - hScrollBar1.Value) + (1.0 - SCErrorSum[i, 1] / meteopgtlen) * hScrollBar1.Value) / 100;
            }
            err /= MetFileNames.Count;
            return err;
        }

        private double Round3Digits(double val)
        {
            double _roundval = val;
            if (_roundval < 0.1)
            {
                _roundval = Math.Round(_roundval, 3);
            }
            else if (_roundval < 10)
            {
                _roundval = Math.Round(_roundval, 2);
            }
            else
            {
                _roundval = Math.Round(_roundval, 1);
            }
            return _roundval;
        }

    }
}