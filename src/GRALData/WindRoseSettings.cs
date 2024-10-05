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


namespace GralData
{
    /// <summary>
    /// Stores the wind rose settings
    /// </summary>
    public class WindRoseSettings
    {
        /// <summary>
        /// Max velocity scale
        /// </summary>
        public int MaxVelocity { get; set; }
        /// <summary>
        /// Starting hour of the time filter
        /// </summary>
        public int StartStunde { get; set; }
        /// <summary>
        /// Final hour of the time filter
        /// </summary>
        public int EndStunde { get; set; }
        /// <summary>
        /// Apply a bias correction
        /// </summary>
        public bool BiasCorrection { get; set; }
        /// <summary>
        /// Show the Bias Textbox
        /// </summary>
        public bool ShowBias { get; set; }
        /// <summary>
        /// Draw a frame around each pie segment
        /// </summary>
        public bool ShowFrames { get; set; }
        /// <summary>
        /// Draw smaller sectors with a little gap
        /// </summary>
        public bool DrawSmallSectors { get; set; }
        /// <summary>
        /// Number of wind rose sectors
        /// </summary>
        public int SectorCount { get; set; }
        /// <summary>
        /// Ignore values with wind speed and direction = 0
        /// </summary>
        public bool Ignore00Values { get; set; }
        /// <summary>
        /// Show the wind sector group box in the dialog
        /// </summary>
        public bool ShowWindSectorGroupBox { get; set; }
        /// <summary>
        /// Show the Max Scale group box
        /// </summary>
        public bool ShowMaxScaleGroupBox { get; set; }
        /// <summary>
        /// Set the max vertical scale ; 0 = Auto Scale
        /// </summary>
        public int MaxScaleVertical { get; set; }

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
            ShowFrames = true;
            DrawSmallSectors = false;
            SectorCount = 16;
            Ignore00Values = false;
            ShowWindSectorGroupBox = true;
            ShowMaxScaleGroupBox = false;
            MaxScaleVertical = 0;
        }

        /// <summary>
        /// Copy wind rose settings
        /// </summary>
        public WindRoseSettings(WindRoseSettings other)
        {
            MaxVelocity = other.MaxVelocity;
            StartStunde = other.StartStunde;
            EndStunde = other.EndStunde;
            BiasCorrection = other.BiasCorrection;
            ShowBias = other.ShowBias;
            ShowFrames = other.ShowFrames;
            DrawSmallSectors = other.DrawSmallSectors;
            SectorCount = other.SectorCount;
            Ignore00Values = other.Ignore00Values;
            ShowWindSectorGroupBox = other.ShowWindSectorGroupBox;
            ShowMaxScaleGroupBox = other.ShowMaxScaleGroupBox;
            MaxScaleVertical = other.MaxScaleVertical;
        }
    }
}
