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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GralData
{
    /// <summary>
    /// Stores the wind rose settings
    /// </summary>
    public class WindRoseSettings
    {
        public int MaxVelocity { get; set; }
        public int StartStunde { get; set; }
        public int EndStunde { get; set; }
        public bool BiasCorrection { get; set; }
        public bool ShowBias { get; set; }
        public bool ShowFrames { get; set; }
        public bool DrawSmallSectors { get; set; }

        /// <summary>
        /// Initialize the windrose settings with default values
        /// </summary>
        public WindRoseSettings()
        {
            MaxVelocity = 6;
            StartStunde = 0;
            EndStunde = 23;
            BiasCorrection = true;
            ShowBias = true;
            ShowFrames = false;
            DrawSmallSectors = false;
        }

        public WindRoseSettings(WindRoseSettings other)
        {
            MaxVelocity = other.MaxVelocity;
            StartStunde = other.StartStunde;
            EndStunde = other.EndStunde;
            BiasCorrection = other.BiasCorrection;
            ShowBias = other.ShowBias;
            ShowFrames = other.ShowFrames;
            DrawSmallSectors = other.DrawSmallSectors;
        }

    }
}
