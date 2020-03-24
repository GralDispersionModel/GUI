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
 * Date: 19.05.2019
 * Time: 19:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace GralIO
{
    public class InDatVariables
    {
        public string InDatPath { get; set; }
        /// <summary>
        ///Number of particles per second
        /// </summary>
        private int _particlenumber;
        public int ParticleNumber
        {
            get
            {
                return _particlenumber;
            }
            set
            {
                _particlenumber = Math.Max(1, value);
            }
        }
        /// <summary>
        ///Dispersion time
        /// </summary>
        private int _dispersionTime;
        public int DispersionTime
        {
            get
            {
                return _dispersionTime;
            }
            set
            {
                _dispersionTime = Math.Max(300, Math.Min(3600, value));
            }
        }
        /// <summary>
        ///flag determining whether receptor points are set or not
        /// </summary>
		public string Receptorflag { get; set; }
        /// <summary>
        ////Roughness length
        /// </summary>
        private double _roughness;
        public double Roughness
        {
            get
            {
                return _roughness;
            }
            set
            {
                _roughness = Math.Max(0.001, Math.Min(100, value));
            }
        }
        /// <summary>
        ///Latitude
        /// </summary>
        private double _latitude;
        public double Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                _latitude = Math.Max(-90, Math.Min(90, value));
            }
        }
        /// <summary>
        ///Unused: Pollutant for which dispersion shall be simulated
        /// </summary>
        public string Pollutant { get; set; }
        /// <summary>
        ///Number of horizontal slices for concentration grids in GRAL
        /// </summary>
        private int _numHorSlices;
        public int NumHorSlices
        {
            get
            {
                return _numHorSlices;
            }
            set
            {
                _numHorSlices = Math.Max(0, Math.Min(9, value));
            }
        }
        /// <summary>
        ///Height of the horizontal slices above ground level for the concentration fields
        /// </summary>
		public double[] HorSlices { get; set; }
        /// <summary>
        ///Vertical extension of the horizontal concentration layers
        /// </summary>
        private double _deltaz;
        public double Deltaz
        {
            get
            {
                return _deltaz;
            }
            set
            {
                _deltaz = Math.Max(0.1, Math.Min(5000, value));
            }
        }
        /// <summary>
        ///Number of dispersion situation, for starting the simulation
        /// </summary>
		public int DispersionSituation { get; set; }
        /// <summary>
        ///0: no buildings, 1: diagnostic approach, 2: prognostic approach 
        /// </summary>
        private int _buildingmode;
        public int BuildingMode
        {
            get
            {
                return _buildingmode;
            }
            set
            {
                _buildingmode = Math.Max(0, Math.Min(2, value));
            }
        }
        /// <summary>
        ///Write the file buildings_heights.txt ?
        /// </summary>
		public bool BuildingHeightsWrite { get; set; }
        /// <summary>
        ///Use compressed *.con files ? (*.grz)
        /// </summary>
        public int Compressed { get; set; }
        /// <summary>
        ///Use the transient GRAL mode 1=steady state, 0=single case - transient
        /// </summary>
        private int _transientFlag;
        public int Transientflag
        {
            get
            {
                return _transientFlag;
            }
            set
            {
                _transientFlag = Math.Max(0, Math.Min(1, value));
            }
        }
        /// <summary>
        ///Factor for the Prognostic sub domains
        /// </summary>
        private int _prognosticSubDomains;
        public int PrognosticSubDomains
        {
            get
            {
                return _prognosticSubDomains;
            }
            set
            {
                _prognosticSubDomains = Math.Max(15, Math.Min(1000, value));
            }
        }
        /// <summary>
        ///Use Keystroke when exiting the GRAL console
        /// </summary>
        public bool WaitForKeyStroke { get; set; }
        /// <summary>
        ///Use Keystroke when exiting the GRAL console
        /// </summary>
        public bool WriteESRIResult { get; set; }

        public InDatVariables()
        {
            InDatPath = String.Empty;
            ParticleNumber = 100;
            DispersionTime = 3600;
            Receptorflag = "0";
            Roughness = 0.2;
            Latitude = 47;
            Pollutant = "NOx";
            NumHorSlices = 1;
            HorSlices = new double[10];
            HorSlices[0] = 3;
            Deltaz = 2;
            DispersionSituation = 1;
            BuildingMode = 0;
            BuildingHeightsWrite = false;
            Compressed = 1;
            Transientflag = 0;
            PrognosticSubDomains = 15;
            WaitForKeyStroke = true;
            WriteESRIResult = false;
        }

        public InDatVariables(InDatVariables other)
        {
            InDatPath = other.InDatPath;
            ParticleNumber = other.ParticleNumber;
            DispersionTime = other.DispersionTime;
            Receptorflag = other.Receptorflag;
            Roughness = other.Roughness;
            Latitude = other.Latitude;
            Pollutant = other.Pollutant;
            NumHorSlices = other.NumHorSlices;
            HorSlices = new double[10];
            for (int i = 0; i < 9; i++)
            {
                HorSlices[i] = other.HorSlices[i];
            }
            Deltaz = other.Deltaz;
            DispersionSituation = other.DispersionSituation;
            BuildingMode = other.BuildingMode;
            BuildingHeightsWrite = other.BuildingHeightsWrite;
            Compressed = other.Compressed;
            Transientflag = other.Transientflag;
            PrognosticSubDomains = other.PrognosticSubDomains;
            WaitForKeyStroke = other.WaitForKeyStroke;
            WriteESRIResult = other.WriteESRIResult;
        }

    }
}

