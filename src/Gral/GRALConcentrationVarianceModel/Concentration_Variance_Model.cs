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

using System;

namespace GralConcentrationVarianceModel
{
    class Concentration_Variance_Model
    {   
    	/// <summary>
    	/// Calculate R90 for the odour concentration variation model
    	/// </summary>
    	public static void R90_calculate(int i, int j, float conc_ip, float conc_im, float conc_jp, float conc_jm, float concp, float concm, float conc, float Q_cv0, float td, float Horgridsize, float vertgridsize, ref float R90, ref float Q_cv)
    	{
            //concentration gradients (attention: gradients near buildings!!!)
            float dCdr = (float)((concp - concm) * 0.5 / vertgridsize +
                (conc_ip - conc_im) * 0.5 / Horgridsize +
                (conc_jp - conc_jm) * 0.5 / Horgridsize);
            dCdr *= dCdr;

            //loop over time (time step of 1 second)
            if (dCdr > 0)
            {
                float prop_fact = 2;

                //source-term for the concentration variance
                Q_cv = 0F;

                //dissipation time-scale set equal to the vertical lagrangian time-scale; note the proportionality constant is different in various publications
                float td_mod = td * prop_fact;

                //time integration of concentration variance
                Q_cv += (Q_cv0 * dCdr - Q_cv / td_mod) * td_mod;

                //fluctuation intensity
                if (conc > 0)
                {
                    Q_cv = (float)Math.Sqrt(Q_cv) / conc;
                }
                else
                {
                    Q_cv = 3.77F;
                }

                Q_cv = (float)Math.Min(Q_cv, 3.77); //3.77 leads to R90=1 when using a 2-parameter Weibull PDF, lower values than 1 are not allowed


                //ratio c90/cmean according to 2-parameter Weibull PDF
                float k_shapeparameter = (float)Math.Pow(1 / Q_cv, 1.086);
                float lamda_shapeparameter = (float)GAMMA_Function.Gamma(1 + 1 / k_shapeparameter); //note that the division by the mean concentration cancels out when computing R90
                R90 = (float)(Math.Pow(2.3, 1 / k_shapeparameter) / lamda_shapeparameter * 1.5);
            }
        }

        static Func<double, double> Pow2 = (double x) =>
            (x * x);

        static Func<double, double> Pow3 = (double x) =>
            (x * x * x);

        static Func<double, double> Pow4 = (double x) =>
            (x * x * x * x);
        }
    }
