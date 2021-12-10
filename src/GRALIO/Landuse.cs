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
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using GralStaticFunctions;
using Gral;

namespace GralIO
{
    /////////////////////////////////////
    /// routine to generate landuse files for GRAMM and GRAL
    /////////////////////////////////////
    
    /// <summary>
    /// This class creates the landuse file
    /// </summary>
    public class Landuse
    {
        public bool ok = true;

        private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        private string _projectname;
        public string ProjectName { set { _projectname = value; } }
        private int _NX;
        public int NX { set { _NX = value; } }
        private int _NY;
        public int NY { set { _NY = value; } }
        private double[,] _RHOB;
        public double[,] RHOB { set { _RHOB = value; } get { return _RHOB; } }
        private double[,] _ALAMBDA;
        public double[,] ALAMBDA { set { _ALAMBDA = value; } get { return _ALAMBDA; } }
        private double[,] _Z0;
        public double[,] Z0 { set { _Z0 = value; } get { return _Z0; } }
        private double[,] _FW;
        public double[,] FW { set { _FW = value; } get { return _FW; } }
        private double[,] _EPSG;
        public double[,] EPSG { set { _EPSG = value; } get { return _EPSG; } }
        private double[,] _ALBEDO;
        public double[,] ALBEDO { set { _ALBEDO = value; } get { return _ALBEDO; } }
        private int _IKOOA;
        public int IKOOA { set { _IKOOA = value; } }
        private int _JKOOA;
        public int JKOOA { set { _JKOOA = value; } }
        private double _NODDATA;
        public double NODATA { set { _NODDATA = value; } }
        private double _DX;
        public double DX { set { _DX = value; } }
        
        private CultureInfo ic = CultureInfo.InvariantCulture;

        //module to initialze a jagged array
        /// <summary>
        /// Create a jagged array
        /// </summary>
        public static T[] CreateArray<T>(int cnt, Func<T> itemCreator)
        {
            T[] result = new T[cnt];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = itemCreator();
            }
            return result;
        }
        
        /// <summary>
        /// Create the landuse file
        /// </summary>
        public bool GenerateLanduseFile(string FileName, bool Mode)
        {	
            GralMessage.Waitprogressbar wait = new GralMessage.Waitprogressbar("Reading landuse file");
            wait.Show();
            
            //User defined column seperator and decimal seperator
            string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
 
            //Albedo [%]
            double [] AGL=new double[1000];
            //Emissivity of the surface [%]
            double[] EPSGL = new double[1000];
            //Soil moisture [%]
            double[] FWL = new double[1000];
            //Roughness length [m]
            double[] Z0L = new double[1000];
            //Heat conductivity [W/m" + SquareString]
            double[] ALAMBDAL = new double[1000];
            //Temperature conductivity [m�/s]
            double[] ALAMBDAT = new double[1000];


            //Version 19.01
            if (Mode)
            {
                //0 = Any default values (here forest 311)
                AGL[0] = 0.16; EPSGL[0] = 0.95; FWL[0] = 0.40; Z0L[0] = 1.0; ALAMBDAL[0] = 0.5; ALAMBDAT[0] = 0.0000008;
                //111 = Continuous urban fabric
                AGL[111] = 0.25; EPSGL[111] = 0.95; FWL[111] = 0.03; Z0L[111] = 1.0; ALAMBDAL[111] = 4.0; ALAMBDAT[111] = 0.000002;
                //112 = Discontinuous urban fabric
                AGL[112] = 0.25; EPSGL[112] = 0.95; FWL[112] = 0.03; Z0L[112] = 0.5; ALAMBDAL[112] = 4.0; ALAMBDAT[112] = 0.0000013;
                //121 = Industrial or commercial units
                AGL[121] = 0.25; EPSGL[121] = 0.95; FWL[121] = 0.03; Z0L[121] = 0.5; ALAMBDAL[121] = 4.0; ALAMBDAT[121] = 0.0000013;
                //122 = Road and rail networks and associated land
                AGL[122] = 0.25; EPSGL[122] = 0.95; FWL[122] = 0.03; Z0L[122] = 0.3; ALAMBDAL[122] = 4.0; ALAMBDAT[122] = 0.0000013;
                //123 = Port areas
                AGL[123] = 0.25; EPSGL[123] = 0.95; FWL[123] = 0.03; Z0L[123] = 1.0; ALAMBDAL[123] = 4.0; ALAMBDAT[123] = 0.0000013;
                //124 = Airports
                AGL[124] = 0.25; EPSGL[124] = 0.95; FWL[124] = 0.03; Z0L[124] = 0.2; ALAMBDAL[124] = 4.0; ALAMBDAT[124] = 0.0000013;
                //131 = Mineral extraction sites
                AGL[131] = 0.25; EPSGL[131] = 0.95; FWL[131] = 0.03; Z0L[131] = 0.2; ALAMBDAL[131] = 2.0; ALAMBDAT[131] = 0.0000013;
                //132 = Dump sites
                AGL[132] = 0.25; EPSGL[132] = 0.95; FWL[132] = 0.03; Z0L[132] = 0.2; ALAMBDAL[132] = 2.0; ALAMBDAT[132] = 0.0000013;
                //133 = Construction sites
                AGL[133] = 0.25; EPSGL[133] = 0.95; FWL[133] = 0.03; Z0L[133] = 0.2; ALAMBDAL[133] = 2.0; ALAMBDAT[133] = 0.0000013;
                //141 = Green urban areas
                AGL[141] = 0.19; EPSGL[141] = 0.92; FWL[141] = 0.10; Z0L[141] = 0.3; ALAMBDAL[141] = 1.0; ALAMBDAT[141] = 0.0000007;
                //142 = Sport and leisure facilities
                AGL[142] = 0.19; EPSGL[142] = 0.92; FWL[142] = 0.10; Z0L[142] = 0.3; ALAMBDAL[142] = 1.0; ALAMBDAT[142] = 0.0000007;
                //211 = Non-irrigated arable land
                AGL[211] = 0.19; EPSGL[211] = 0.92; FWL[211] = 0.10; Z0L[211] = 0.1; ALAMBDAL[211] = 0.5; ALAMBDAT[211] = 0.0000007;
                //212 = Permanently-irrigated arable land
                AGL[212] = 0.19; EPSGL[212] = 0.92; FWL[212] = 0.50; Z0L[212] = 0.1; ALAMBDAL[212] = 1.5; ALAMBDAT[212] = 0.0000007;
                //213 = Rice fields
                AGL[213] = 0.19; EPSGL[213] = 0.92; FWL[213] = 0.50; Z0L[213] = 0.1; ALAMBDAL[213] = 2.0; ALAMBDAT[213] = 0.0000007;
                //221 = Vineyards
                AGL[221] = 0.19; EPSGL[221] = 0.92; FWL[221] = 0.10; Z0L[221] = 0.15; ALAMBDAL[221] = 0.5; ALAMBDAT[221] = 0.0000007;
                //222 = Fruit trees and berry plantations
                AGL[222] = 0.19; EPSGL[222] = 0.92; FWL[222] = 0.10; Z0L[222] = 0.25; ALAMBDAL[222] = 0.5; ALAMBDAT[222] = 0.0000007;
                //223 = Olive groves
                AGL[223] = 0.19; EPSGL[223] = 0.92; FWL[223] = 0.05; Z0L[223] = 0.30; ALAMBDAL[223] = 0.5; ALAMBDAT[223] = 0.0000007;
                //231 = Pastures
                AGL[231] = 0.19; EPSGL[231] = 0.92; FWL[231] = 0.10; Z0L[231] = 0.10; ALAMBDAL[231] = 0.5; ALAMBDAT[231] = 0.0000007;
                //241 = Annual crops associated with permanent crops
                AGL[241] = 0.19; EPSGL[241] = 0.92; FWL[241] = 0.10; Z0L[241] = 0.10; ALAMBDAL[241] = 0.5; ALAMBDAT[241] = 0.0000007;
                //242 = Complex cultivation patterns
                AGL[242] = 0.19; EPSGL[242] = 0.92; FWL[242] = 0.10; Z0L[242] = 0.20; ALAMBDAL[242] = 0.5; ALAMBDAT[242] = 0.0000007;
                //243 = Land principally occupied by agriculture, with significant areas of natural vegetation
                AGL[243] = 0.19; EPSGL[243] = 0.92; FWL[243] = 0.10; Z0L[243] = 0.20; ALAMBDAL[243] = 0.5; ALAMBDAT[243] = 0.0000007;
                //244 = Agro-forestry areas
                AGL[244] = 0.17; EPSGL[244] = 0.95; FWL[244] = 0.40; Z0L[244] = 1.0; ALAMBDAL[244] = 0.5; ALAMBDAT[244] = 0.0000008;
                //311 = Broad-leaved forest
                AGL[311] = 0.16; EPSGL[311] = 0.95; FWL[311] = 0.40; Z0L[311] = 1.0; ALAMBDAL[311] = 0.5; ALAMBDAT[311] = 0.0000008;
                //312 = Coniferous forest
                AGL[312] = 0.12; EPSGL[312] = 0.95; FWL[312] = 0.40; Z0L[312] = 1.0; ALAMBDAL[312] = 0.5; ALAMBDAT[312] = 0.0000008;
                //313 = Mixed forest
                AGL[313] = 0.14; EPSGL[313] = 0.95; FWL[313] = 0.40; Z0L[313] = 1.0; ALAMBDAL[313] = 0.5; ALAMBDAT[313] = 0.0000008;
                //321 = Natural grasslands
                AGL[321] = 0.15; EPSGL[321] = 0.92; FWL[321] = 0.10; Z0L[321] = 0.02; ALAMBDAL[321] = 0.5; ALAMBDAT[321] = 0.000001;
                //322 = Moors and heathland
                AGL[322] = 0.15; EPSGL[322] = 0.92; FWL[322] = 0.10; Z0L[322] = 0.02; ALAMBDAL[322] = 2.7; ALAMBDAT[322] = 0.000001;
                //323 = Sclerophyllous vegeatation
                AGL[323] = 0.15; EPSGL[323] = 0.92; FWL[323] = 0.02; Z0L[323] = 0.05; ALAMBDAL[323] = 0.5; ALAMBDAT[323] = 0.000001;
                //324 = Transitional woodland-shrub
                AGL[324] = 0.15; EPSGL[324] = 0.92; FWL[324] = 0.10; Z0L[324] = 0.02; ALAMBDAL[324] = 0.5; ALAMBDAT[324] = 0.000001;
                //331 = Beaches, dunes, sands
                AGL[331] = 0.25; EPSGL[331] = 0.95; FWL[331] = 0.60; Z0L[331] = 0.05; ALAMBDAL[331] = 0.3; ALAMBDAT[331] = 0.000001;
                //332 = Bare rocks
                AGL[332] = 0.15; EPSGL[332] = 0.92; FWL[332] = 0.01; Z0L[332] = 0.10; ALAMBDAL[332] = 1.5; ALAMBDAT[332] = 0.000001;
                //333 = Sparsely vegetated areas
                AGL[333] = 0.15; EPSGL[333] = 0.92; FWL[333] = 0.01; Z0L[333] = 0.01; ALAMBDAL[333] = 0.5; ALAMBDAT[333] = 0.000001;
                //334 = Burnt areas
                AGL[334] = 0.15; EPSGL[334] = 0.92; FWL[334] = 0.05; Z0L[334] = 0.10; ALAMBDAL[334] = 0.3; ALAMBDAT[334] = 0.000001;
                //335 = Glaciers and perpetual snow
                AGL[335] = 0.60; EPSGL[335] = 0.95; FWL[335] = 0.10; Z0L[335] = 0.01; ALAMBDAL[335] = 1.0; ALAMBDAT[335] = 0.0000005;
                //411 = Inland marshes
                AGL[411] = 0.14; EPSGL[411] = 0.95; FWL[411] = 0.70; Z0L[411] = 0.01; ALAMBDAL[411] = 20.0; ALAMBDAT[411] = 0.000001;
                //412 = Peat bogs
                AGL[412] = 0.14; EPSGL[412] = 0.95; FWL[412] = 0.70; Z0L[412] = 0.01; ALAMBDAL[412] = 20.0; ALAMBDAT[412] = 0.000001;
                //421 = Salt marshes
                AGL[421] = 0.50; EPSGL[421] = 0.95; FWL[421] = 0.70; Z0L[421] = 0.01; ALAMBDAL[421] = 20.0; ALAMBDAT[421] = 0.000001;
                //422 = Salines
                AGL[422] = 0.50; EPSGL[422] = 0.95; FWL[422] = 0.70; Z0L[422] = 0.01; ALAMBDAL[422] = 20.0; ALAMBDAT[422] = 0.000001;
                //423 = Intertidal flats
                AGL[423] = 0.14; EPSGL[423] = 0.95; FWL[423] = 0.70; Z0L[423] = 0.01; ALAMBDAL[423] = 20.0; ALAMBDAT[423] = 0.000001;
                //511 = Water courses
                AGL[511] = 0.08; EPSGL[511] = 0.98; FWL[511] = 1.00; Z0L[511] = 0.0001; ALAMBDAL[511] = 100.0; ALAMBDAT[511] = 0.000001;
                //512 = Water bodies
                AGL[512] = 0.08; EPSGL[512] = 0.98; FWL[512] = 1.00; Z0L[512] = 0.0001; ALAMBDAL[512] = 100.0; ALAMBDAT[512] = 0.000001;
                //521 = Coastal lagoons
                AGL[521] = 0.08; EPSGL[521] = 0.98; FWL[521] = 1.00; Z0L[521] = 0.0001; ALAMBDAL[521] = 100.0; ALAMBDAT[521] = 0.000001;
                //522 = Estuaries
                AGL[522] = 0.08; EPSGL[522] = 0.98; FWL[522] = 1.00; Z0L[522] = 0.0001; ALAMBDAL[522] = 100.0; ALAMBDAT[522] = 0.000001;
                //523 = Sea and Ocean
                AGL[523] = 0.08; EPSGL[523] = 0.98; FWL[523] = 1.00; Z0L[523] = 0.0001; ALAMBDAL[523] = 100.0; ALAMBDAT[523] = 0.000001;
            }
            else
            {
                //Version 20.01
                //0 = Any default values (here forest 311)
                AGL[0] = 0.16; EPSGL[0] = 0.95; FWL[0] = 0.40; Z0L[0] = 1.0; ALAMBDAL[0] = 0.2; ALAMBDAT[0] = 0.0000008;
                //111 = Continuous urban fabric
                AGL[111] = 0.25; EPSGL[111] = 0.95; FWL[111] = 0.03; Z0L[111] = 1.0; ALAMBDAL[111] = 1.0; ALAMBDAT[111] = 0.000002;
                //112 = Discontinuous urban fabric
                AGL[112] = 0.25; EPSGL[112] = 0.95; FWL[112] = 0.03; Z0L[112] = 0.5; ALAMBDAL[112] = 1.0; ALAMBDAT[112] = 0.0000013;
                //121 = Industrial or commercial units
                AGL[121] = 0.25; EPSGL[121] = 0.95; FWL[121] = 0.03; Z0L[121] = 0.5; ALAMBDAL[121] = 1.0; ALAMBDAT[121] = 0.0000013;
                //122 = Road and rail networks and associated land
                AGL[122] = 0.25; EPSGL[122] = 0.95; FWL[122] = 0.03; Z0L[122] = 0.3; ALAMBDAL[122] = 1.0; ALAMBDAT[122] = 0.0000013;
                //123 = Port areas
                AGL[123] = 0.25; EPSGL[123] = 0.95; FWL[123] = 0.03; Z0L[123] = 1.0; ALAMBDAL[123] = 1.0; ALAMBDAT[123] = 0.0000013;
                //124 = Airports
                AGL[124] = 0.25; EPSGL[124] = 0.95; FWL[124] = 0.03; Z0L[124] = 0.2; ALAMBDAL[124] = 1.0; ALAMBDAT[124] = 0.0000013;
                //131 = Mineral extraction sites
                AGL[131] = 0.25; EPSGL[131] = 0.95; FWL[131] = 0.03; Z0L[131] = 0.2; ALAMBDAL[131] = 1.0; ALAMBDAT[131] = 0.0000013;
                //132 = Dump sites
                AGL[132] = 0.25; EPSGL[132] = 0.95; FWL[132] = 0.03; Z0L[132] = 0.2; ALAMBDAL[132] = 1.0; ALAMBDAT[132] = 0.0000013;
                //133 = Construction sites
                AGL[133] = 0.25; EPSGL[133] = 0.95; FWL[133] = 0.03; Z0L[133] = 0.2; ALAMBDAL[133] = 1.0; ALAMBDAT[133] = 0.0000013;
                //141 = Green urban areas
                AGL[141] = 0.19; EPSGL[141] = 0.92; FWL[141] = 0.10; Z0L[141] = 0.3; ALAMBDAL[141] = 0.2; ALAMBDAT[141] = 0.0000007;
                //142 = Sport and leisure facilities
                AGL[142] = 0.19; EPSGL[142] = 0.92; FWL[142] = 0.10; Z0L[142] = 0.3; ALAMBDAL[142] = 0.2; ALAMBDAT[142] = 0.0000007;
                //211 = Non-irrigated arable land
                AGL[211] = 0.19; EPSGL[211] = 0.92; FWL[211] = 0.10; Z0L[211] = 0.1; ALAMBDAL[211] = 0.2; ALAMBDAT[211] = 0.0000007;
                //212 = Permanently-irrigated arable land
                AGL[212] = 0.19; EPSGL[212] = 0.92; FWL[212] = 0.50; Z0L[212] = 0.1; ALAMBDAL[212] = 1.0; ALAMBDAT[212] = 0.0000007;
                //213 = Rice fields
                AGL[213] = 0.19; EPSGL[213] = 0.92; FWL[213] = 0.50; Z0L[213] = 0.1; ALAMBDAL[213] = 2.0; ALAMBDAT[213] = 0.0000007;
                //221 = Vineyards
                AGL[221] = 0.19; EPSGL[221] = 0.92; FWL[221] = 0.10; Z0L[221] = 0.15; ALAMBDAL[221] = 0.2; ALAMBDAT[221] = 0.0000007;
                //222 = Fruit trees and berry plantations
                AGL[222] = 0.19; EPSGL[222] = 0.92; FWL[222] = 0.10; Z0L[222] = 0.25; ALAMBDAL[222] = 0.2; ALAMBDAT[222] = 0.0000007;
                //223 = Olive groves
                AGL[223] = 0.19; EPSGL[223] = 0.92; FWL[223] = 0.05; Z0L[223] = 0.30; ALAMBDAL[223] = 0.2; ALAMBDAT[223] = 0.0000007;
                //231 = Pastures
                AGL[231] = 0.19; EPSGL[231] = 0.92; FWL[231] = 0.10; Z0L[231] = 0.10; ALAMBDAL[231] = 0.2; ALAMBDAT[231] = 0.0000007;
                //241 = Annual crops associated with permanent crops
                AGL[241] = 0.19; EPSGL[241] = 0.92; FWL[241] = 0.10; Z0L[241] = 0.10; ALAMBDAL[241] = 0.2; ALAMBDAT[241] = 0.0000007;
                //242 = Complex cultivation patterns
                AGL[242] = 0.19; EPSGL[242] = 0.92; FWL[242] = 0.10; Z0L[242] = 0.20; ALAMBDAL[242] = 0.2; ALAMBDAT[242] = 0.0000007;
                //243 = Land principally occupied by agriculture, with significant areas of natural vegetation
                AGL[243] = 0.19; EPSGL[243] = 0.92; FWL[243] = 0.10; Z0L[243] = 0.20; ALAMBDAL[243] = 0.2; ALAMBDAT[243] = 0.0000007;
                //244 = Agro-forestry areas
                AGL[244] = 0.17; EPSGL[244] = 0.95; FWL[244] = 0.40; Z0L[244] = 1.0; ALAMBDAL[244] = 0.2; ALAMBDAT[244] = 0.0000008;
                //311 = Broad-leaved forest
                AGL[311] = 0.16; EPSGL[311] = 0.95; FWL[311] = 0.40; Z0L[311] = 1.0; ALAMBDAL[311] = 0.2; ALAMBDAT[311] = 0.0000008;
                //312 = Coniferous forest
                AGL[312] = 0.12; EPSGL[312] = 0.95; FWL[312] = 0.40; Z0L[312] = 1.0; ALAMBDAL[312] = 0.2; ALAMBDAT[312] = 0.0000008;
                //313 = Mixed forest
                AGL[313] = 0.14; EPSGL[313] = 0.95; FWL[313] = 0.40; Z0L[313] = 1.0; ALAMBDAL[313] = 0.2; ALAMBDAT[313] = 0.0000008;
                //321 = Natural grasslands
                AGL[321] = 0.15; EPSGL[321] = 0.92; FWL[321] = 0.10; Z0L[321] = 0.02; ALAMBDAL[321] = 0.2; ALAMBDAT[321] = 0.000001;
                //322 = Moors and heathland
                AGL[322] = 0.15; EPSGL[322] = 0.92; FWL[322] = 0.10; Z0L[322] = 0.02; ALAMBDAL[322] = 2.0; ALAMBDAT[322] = 0.000001;
                //323 = Sclerophyllous vegeatation
                AGL[323] = 0.15; EPSGL[323] = 0.92; FWL[323] = 0.02; Z0L[323] = 0.05; ALAMBDAL[323] = 0.2; ALAMBDAT[323] = 0.000001;
                //324 = Transitional woodland-shrub
                AGL[324] = 0.15; EPSGL[324] = 0.92; FWL[324] = 0.10; Z0L[324] = 0.02; ALAMBDAL[324] = 0.2; ALAMBDAT[324] = 0.000001;
                //331 = Beaches, dunes, sands
                AGL[331] = 0.25; EPSGL[331] = 0.95; FWL[331] = 0.60; Z0L[331] = 0.05; ALAMBDAL[331] = 0.3; ALAMBDAT[331] = 0.000001;
                //332 = Bare rocks
                AGL[332] = 0.15; EPSGL[332] = 0.92; FWL[332] = 0.01; Z0L[332] = 0.10; ALAMBDAL[332] = 1.0; ALAMBDAT[332] = 0.000001;
                //333 = Sparsely vegetated areas
                AGL[333] = 0.15; EPSGL[333] = 0.92; FWL[333] = 0.01; Z0L[333] = 0.01; ALAMBDAL[333] = 0.2; ALAMBDAT[333] = 0.000001;
                //334 = Burnt areas
                AGL[334] = 0.15; EPSGL[334] = 0.92; FWL[334] = 0.05; Z0L[334] = 0.10; ALAMBDAL[334] = 0.1; ALAMBDAT[334] = 0.000001;
                //335 = Glaciers and perpetual snow
                AGL[335] = 0.60; EPSGL[335] = 0.95; FWL[335] = 0.10; Z0L[335] = 0.01; ALAMBDAL[335] = 1.0; ALAMBDAT[335] = 0.0000005;
                //411 = Inland marshes
                AGL[411] = 0.14; EPSGL[411] = 0.95; FWL[411] = 0.70; Z0L[411] = 0.01; ALAMBDAL[411] = 20.0; ALAMBDAT[411] = 0.000001;
                //412 = Peat bogs
                AGL[412] = 0.14; EPSGL[412] = 0.95; FWL[412] = 0.70; Z0L[412] = 0.01; ALAMBDAL[412] = 20.0; ALAMBDAT[412] = 0.000001;
                //421 = Salt marshes
                AGL[421] = 0.50; EPSGL[421] = 0.95; FWL[421] = 0.70; Z0L[421] = 0.01; ALAMBDAL[421] = 20.0; ALAMBDAT[421] = 0.000001;
                //422 = Salines
                AGL[422] = 0.50; EPSGL[422] = 0.95; FWL[422] = 0.70; Z0L[422] = 0.01; ALAMBDAL[422] = 20.0; ALAMBDAT[422] = 0.000001;
                //423 = Intertidal flats
                AGL[423] = 0.14; EPSGL[423] = 0.95; FWL[423] = 0.70; Z0L[423] = 0.01; ALAMBDAL[423] = 20.0; ALAMBDAT[423] = 0.000001;
                //511 = Water courses
                AGL[511] = 0.08; EPSGL[511] = 0.98; FWL[511] = 1.00; Z0L[511] = 0.0001; ALAMBDAL[511] = 100.0; ALAMBDAT[511] = 0.000001;
                //512 = Water bodies
                AGL[512] = 0.08; EPSGL[512] = 0.98; FWL[512] = 1.00; Z0L[512] = 0.0001; ALAMBDAL[512] = 100.0; ALAMBDAT[512] = 0.000001;
                //521 = Coastal lagoons
                AGL[521] = 0.081; EPSGL[521] = 0.98; FWL[521] = 1.00; Z0L[521] = 0.0001; ALAMBDAL[521] = 100.0; ALAMBDAT[521] = 0.000001;
                //522 = Estuaries
                AGL[522] = 0.081; EPSGL[522] = 0.98; FWL[522] = 1.00; Z0L[522] = 0.0001; ALAMBDAL[522] = 100.0; ALAMBDAT[522] = 0.000001;
                //523 = Sea and Ocean
                AGL[523] = 0.081; EPSGL[523] = 0.98; FWL[523] = 1.00; Z0L[523] = 0.0001; ALAMBDAL[523] = 100.0; ALAMBDAT[523] = 0.000001;
            }

            //searching and reading for file containing default values or additional landuse values or updated landuse values
            bool defaultvalues = File.Exists(Path.Combine(Main.ProjectName, @"Computation", "Landuse_Default.txt"));
            if (defaultvalues==true)
            {
                try
                {
                    using (StreamReader reader1 = new StreamReader(Path.Combine(Main.ProjectName, @"Computation", "Landuse_Default.txt")))
                    {
                        string[] text1 = new string[10];
                        try
                        {
                            //header line
                            text1 = reader1.ReadLine().Split(new char[] {'\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                            //consecutive lines containing landuse values
                            while ((text1 = reader1.ReadLine().Split(new char[] {'\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) != null)
                            {
                                int NR = Convert.ToInt32(text1[0]);
                                AGL[NR] = Convert.ToDouble(text1[1], ic);
                                EPSGL[NR] = Convert.ToDouble(text1[2], ic);
                                FWL[NR] = Convert.ToDouble(text1[3], ic);
                                Z0L[NR] = Convert.ToDouble(text1[4], ic);
                                ALAMBDAL[NR] = Convert.ToDouble(text1[5], ic);
                                ALAMBDAT[NR] = Convert.ToDouble(text1[6], ic);
                            }
                        }
                        catch{ }
                    }
                    
                }
                catch
                {
                    MessageBox.Show("Unable to read file Landuse_Default.txt", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            GralMessage.MessageWindow mw = new GralMessage.MessageWindow();

            //reading field sizes from file GRAMM.geb
            StreamReader reader=new StreamReader(Path.Combine(Main.ProjectName, @"Computation", "GRAMM.geb"));
            string [] text=new string[5];
            text=reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            int NX=Convert.ToInt32(text[0]);  //number of horizontal cells in x direction
            text=reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            int NY=Convert.ToInt32(text[0]);  //number of horizontal cells in y direction
            text=reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            int NZ=Convert.ToInt32(text[0]);  //number of vertical cells in z direction
            text = reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            double xsimin=Convert.ToDouble(text[0].Replace(".",decsep)); //western boarder
            text = reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            double xsimax=Convert.ToDouble(text[0].Replace(".",decsep)); //eastern boarder
            text = reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            double etamin=Convert.ToDouble(text[0].Replace(".",decsep)); //southern boarder
            text = reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            double etamax=Convert.ToDouble(text[0].Replace(".",decsep)); //northern boarder
            reader.Close();

            int DX = Convert.ToInt32((xsimax - xsimin) / NX);
            int DY = Convert.ToInt32((etamax - etamin) / NY);
            
            int IKOOA = Convert.ToInt32(xsimin);
            int JKOOA = Convert.ToInt32(etamin);

            int NX1=NX+2;
            int NY1=NY+2;
            int NZ1 = NZ + 2;
            int NX2 = NX + 2;
            int NY2 = NY + 2;
            int NZ2 = NZ + 2;

            //double[,,] LUSNR = new double[NX2, NY2, 1000];  //Corine-Landuse-numbers
            //Int16[, ,] LUSPROZ = new Int16[NX2, NY2, 1000];  //Corine-Landuse-frequencies within the chosen cell
            Int16[][][] LUSPROZ = CreateArray<Int16[][]>(1, () => CreateArray<Int16[]>(1, () => new Int16[1]));        //Absolute temperature in K
            LUSPROZ = CreateArray<Int16[][]>(NX2, () => CreateArray<Int16[]>(NY2, () => new Int16[1000]));
            
            double[,] PROZGES = new double[NX2, NY2];  //Corine-Landuse-frequencies within the chosen cell
            double[,] RHOB = new double[NX1, NY1];  //soil density
            double[,] ALAMBDA = new double[NX1, NY1];  //soil temperature leitungsfaehigkeit
            double[,] ALBEDO = new double[NX1, NY1];  //surface albedo
            double[,] AWQ = new double[NX1, NY1];  //surface albedo
            double[,] Z0 = new double[NX1, NY1];  //surface roughness length
            double[,] FW = new double[NX1, NY1];  //soil moisture content
            double[,] EPSG = new double[NX1, NY1];  //surface emissivity
            double NODDATA=0;

            if (FileName != "Default-Values")
            {
                reader = new StreamReader(FileName);
                text = reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                int NCOL = Convert.ToInt32(text[1]);  //number of cells in x-direction of topography file
                text = reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                int NROW = Convert.ToInt32(text[1]);  //number of cells in y-direction of topography file
                text = reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                double ILIUN = Convert.ToDouble(text[1].Replace(".", decsep));  //western boarder of topography file
                text = reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                double JLIUN = Convert.ToDouble(text[1].Replace(".", decsep));  //southern boarder of topography file
                text = reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                double ICSIZE = Convert.ToDouble(text[1].Replace(".", decsep));  //grid size
                text = reader.ReadLine().Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                NODDATA = Convert.ToDouble(text[1].Replace(".", decsep));  //no-data value
                
                int IKOOE = IKOOA + DX * NX;
                int JKOOE = JKOOA + DY * NY;

                mw.listBox1.Items.Add(Path.GetFileName(FileName) + "- East border: " + Convert.ToString(ILIUN));
                mw.listBox1.Items.Add(Path.GetFileName(FileName) + "- West border: " + Convert.ToString(ILIUN + ICSIZE * NROW));
                mw.listBox1.Items.Add(Path.GetFileName(FileName) + "- South border: " + Convert.ToString(JLIUN));
                mw.listBox1.Items.Add(Path.GetFileName(FileName) + "- North border: " + Convert.ToString(JLIUN + ICSIZE * NCOL));
                mw.Refresh();
                mw.listBox1.Items.Add("Model Domain East border: " + Convert.ToString(IKOOA));
                mw.listBox1.Items.Add("Model Domain West border: " + Convert.ToString(IKOOE));
                mw.listBox1.Items.Add("Model Domain South border: " + Convert.ToString(JKOOA));
                mw.listBox1.Items.Add("Model Domain North border: " + Convert.ToString(JKOOE));
                mw.listBox1.Items.Add(" ");

                //reading landuse file
                int J = NY;
                text = new string[NCOL+2];
                int[] ADH = new int[NCOL + 1];
                
                for (int NNJ = 1; NNJ < NROW + 1; NNJ++)
                {
                    Application.DoEvents(); // Kuntner
                    
                    if (NNJ % 40 == 0)
                    {
                        wait.Text =  "Reading landuse file " + ((int)((float)NNJ / (NROW + 2) * 100F)).ToString() +"%";
                    }

                    int I = 1;
                    int JKOO = Convert.ToInt32(JLIUN + (NROW - NNJ + 1) * ICSIZE);
                    string line_text = reader.ReadLine();
                    
                    //check if landuse data point is within the model domain
                    if ((JKOO >= JKOOA) && (JKOO <= JKOOE))
                    {
                        text = line_text.Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                        for (int NNI = 1; NNI < NCOL + 1; NNI++)
                        {
                            text[NNI - 1] = St_F.StrgToICult(text[NNI - 1]); // Kuntner
                            try
                            {
                                ADH[NNI] = int.Parse(text[NNI - 1], System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch
                            {}
                        }
                        
                        // ADH[NNI] = Convert.ToInt32(text[NNI - 1].Replace(".", form1.decsep));
                        for (int NNI = 1; NNI < NCOL + 1; NNI++)
                        {
                            Application.DoEvents(); // Kuntner
                            //check if corine landuse classes are correct
                            if ((ADH[NNI] < 0) || (ADH[NNI] > 999))
                            {
                                if (ADH[NNI] == -9999)
                                {
                                    ADH[NNI] = 0;
                                }
                                else
                                {
                                    MessageBox.Show("CLC-Class: " + Convert.ToString(ADH[NNI] + "\r\ncheck your clc-data (must be 3-digits)\r\nand missing values need to be set to -9999"), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    throw new IOException();
                                }
                            }
                            int IKOO = Convert.ToInt32(ILIUN + (NNI - 1) * ICSIZE);
                            //check if landuse data point is within the model domain
                            if ((IKOO >= IKOOA) && (IKOO <= IKOOE) && (JKOO >= JKOOA) && (JKOO <= JKOOE))
                            {
                                //compute cell indices
                                if (IKOO >= IKOOA + DX * I)
                                {
                                    I += 1;
                                }

                                if (JKOO <= JKOOA + DY * (J - 1))
                                {
                                    J -= 1;
                                }

                                int LUSDUM = ADH[NNI];

                                /*
                            //which landuse data exist
                            for (int NR = 0; NR < 1000; NR++)
                            {
                                if ((LUSDUM == LUSNR[I, J, NR]))
                                    break;
                                if ((LUSDUM != LUSNR[I, J, NR]) && (LUSNR[I, J, NR] == 0))
                                {
                                    LUSNR[I, J, NR] = LUSDUM;
                                    break;
                                }
                            }

                            //compute share of landuse classes within cell
                            PROZGES[I, J] = PROZGES[I, J] + 1;
                            for (int NR = 0; NR < 1000; NR++)
                            {
                                if (LUSDUM == LUSNR[I, J, NR])
                                    LUSPROZ[I, J, NR] = LUSPROZ[I, J, NR] + 1;
                            }
                                 */

                                PROZGES[I, J] = PROZGES[I, J] + 1;
                                LUSPROZ[I][J][LUSDUM]++;
                            }
                        }
                    }
                }
                reader.Close();
                reader.Dispose();

                //soil heat capacity
                double CPBOD = 900;

                //computation of mean values for each cell
                for (int I = 1; I < NX1; I++)
                {
                    Application.DoEvents(); // Kuntner
                    for (J = 1; J < NY1; J++)
                    {
                        ALAMBDA[I, J] = 0;
                        AWQ[I, J] = 0;
                        Z0[I, J] = 0;
                        FW[I, J] = 0;
                        EPSG[I, J] = 0;
                        ALBEDO[I, J] = 0;

                        double TERMPROZ = 0;

                        if (PROZGES[I, J] == 0)
                        {
                            RHOB[I, J] = ALAMBDAT[0];
                            ALAMBDA[I, J] = ALAMBDAL[0];
                            Z0[I, J] = Z0L[0];
                            FW[I, J] = FWL[0];
                            EPSG[I, J] = EPSGL[0];
                            ALBEDO[I, J] = AGL[0];
                        }
                        else
                        {
                            TERMPROZ = 1 / PROZGES[I, J];
                            for (int NR = 0; NR <= 999; NR++)
                            {
                                RHOB[I, J] = RHOB[I, J] + ALAMBDAT[NR] * (float)LUSPROZ[I][J][NR] * TERMPROZ;
                                ALAMBDA[I, J] = ALAMBDA[I, J] + ALAMBDAL[NR] * (float)LUSPROZ[I][J][NR] * TERMPROZ;
                                Z0[I, J] = Z0[I, J] + Z0L[NR] * (float)LUSPROZ[I][J][NR] * TERMPROZ;
                                FW[I, J] = FW[I, J] + FWL[NR] * (float)LUSPROZ[I][J][NR] * TERMPROZ;
                                EPSG[I, J] = EPSG[I, J] + EPSGL[NR] * (float)LUSPROZ[I][J][NR] * TERMPROZ;
                                ALBEDO[I, J] = ALBEDO[I, J] + AGL[NR] * (float)LUSPROZ[I][J][NR] * TERMPROZ;
                            }
                        }

                        //check if a value has been assigned, otherwise use default values
                        if (ALBEDO[I, J] == 0)
                        {
                            RHOB[I, J] = ALAMBDAT[0];
                            ALAMBDA[I, J] = ALAMBDAL[0];
                            Z0[I, J] = Z0L[0];
                            FW[I, J] = FWL[0];
                            EPSG[I, J] = EPSGL[0];
                            ALBEDO[I, J] = AGL[0];
                        }

                        RHOB[I, J] = ALAMBDA[I, J] / RHOB[I, J] / CPBOD;
                    }
                }
            }
            //solely use default-values to generate landuse.asc
            else
            {
                //soil heat capacity
                double CPBOD = 900;

                //computation of mean values for each cell
                for (int I = 1; I < NX1; I++)
                {
                    Application.DoEvents(); // Kuntner
                    for (int J = 1; J < NY1; J++)
                    {
                        ALAMBDA[I, J] = 0;
                        AWQ[I, J] = 0;
                        Z0[I, J] = 0;
                        FW[I, J] = 0;
                        EPSG[I, J] = 0;
                        ALBEDO[I, J] = 0;

                        RHOB[I, J] = ALAMBDAT[0];
                        ALAMBDA[I, J] = ALAMBDAL[0];
                        Z0[I, J] = Z0L[0];
                        FW[I, J] = FWL[0];
                        EPSG[I, J] = EPSGL[0];
                        ALBEDO[I, J] = AGL[0];

                        RHOB[I, J] = ALAMBDA[I, J] / RHOB[I, J] / CPBOD;
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter(Path.Combine(Main.ProjectName, @"Computation", "landuse.asc")))
            {
                for (int j = 1; j < NY + 1; j++)
                {
                    for (int i = 1; i < NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(RHOB[i, j], 0), ic) + " ");
                    }
                }
                writer.WriteLine();
                for (int j = 1; j < NY + 1; j++)
                {
                    for (int i = 1; i < NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(ALAMBDA[i, j], 3), ic) + " ");
                    }
                }
                writer.WriteLine();
                for (int j = 1; j < NY + 1; j++)
                {
                    for (int i = 1; i < NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(Z0[i, j], 4), ic) + " ");
                    }
                }
                writer.WriteLine();
                for (int j = 1; j < NY + 1; j++)
                {
                    for (int i = 1; i < NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(FW[i, j], 4), ic) + " ");
                    }
                }
                writer.WriteLine();
                for (int j = 1; j < NY + 1; j++)
                {
                    for (int i = 1; i < NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(EPSG[i, j], 4), ic) + " ");
                    }
                }
                writer.WriteLine();
                for (int j = 1; j < NY + 1; j++)
                {
                    for (int i = 1; i < NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(ALBEDO[i, j], 3), ic) + " ");
                    }
                }
            }

            WriteESRIFile Result = new WriteESRIFile
            {
                NCols = NX,
                NRows = NY,
                YllCorner = JKOOA,
                XllCorner = IKOOA,
                CellSize = DX,
                Z = -1,

                //output of Roughness length
                Unit = "m",
                Round = 4,
                DblArr = Z0,
                FileName = Path.Combine(Main.ProjectName, @"Maps", "roughness.txt")
            };
            Result.WriteDblArrResult();

            //output of surface density
            Result.Unit = string.Empty;
            Result.Round = 0;
            Result.DblArr = RHOB;
            Result.FileName = Path.Combine(Main.ProjectName, @"Maps", "density.txt");
            Result.WriteDblArrResult();

            //output of heat conductivity
            Result.Unit = string.Empty;
            Result.Round = 2;
            Result.DblArr = ALAMBDA;
            Result.FileName = Path.Combine(Main.ProjectName, @"Maps", "conductivity.txt");
            Result.WriteDblArrResult();
            
            //output of surface moisture
            Result.Unit = string.Empty;
            Result.Round = 3;
            Result.DblArr = FW;
            Result.FileName = Path.Combine(Main.ProjectName, @"Maps", "moisture.txt");
            Result.WriteDblArrResult();
            
            //output of emissivity
            Result.Unit = string.Empty;
            Result.Round = 3;
            Result.DblArr = EPSG;
            Result.FileName = Path.Combine(Main.ProjectName, @"Maps", "emissivity.txt");
            Result.WriteDblArrResult();
            
            //output of surface albedo
            Result.Unit = string.Empty;
            Result.Round = 3;
            Result.DblArr = ALBEDO;
            Result.FileName = Path.Combine(Main.ProjectName, @"Maps", "albedo.txt");
            Result.WriteDblArrResult();
            
            
            mw.listBox1.Items.Add("Landuse File successfully generated. This window can be closed.");
            mw.Refresh();
            mw.Show();

            wait.Close();
            wait.Dispose();
            
            return ok;
        }

        //reading landuse.asc
        /// <summary>
        /// Read the landuse file landuse.asc
        /// </summary>
        public bool ReadLanduseFile()
        {
            try
            {
                if (File.Exists(Path.Combine(_projectname, @"landuse.asc")))
                {
                    string[] text = new string[(_NX + 2) * (_NY + 2)];

                    using (FileStream fs = new FileStream(Path.Combine(_projectname, @"landuse.asc"), FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader r = new StreamReader(fs))
                        {
                            _RHOB = new double[_NX + 1, _NY + 1];
                            _ALAMBDA = new double[_NX + 1, _NY + 1];
                            _Z0 = new double[_NX + 1, _NY + 1];
                            _FW = new double[_NX + 1, _NY + 1];
                            _EPSG = new double[_NX + 1, _NY + 1];
                            _ALBEDO = new double[_NX + 1, _NY + 1];
                            text = Convert.ToString(r.ReadLine()).Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            int n = 0;
                            for (int j = 1; j < _NY + 1; j++)
                            {
                                for (int i = 1; i < _NX + 1; i++)
                                {
                                    _RHOB[i, j] = Convert.ToDouble(text[n].Replace(".", decsep));
                                    n++;
                                }
                            }
                            text = Convert.ToString(r.ReadLine()).Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            n = 0;
                            for (int j = 1; j < _NY + 1; j++)
                            {
                                for (int i = 1; i < _NX + 1; i++)
                                {
                                    _ALAMBDA[i, j] = Convert.ToDouble(text[n].Replace(".", decsep));
                                    n++;
                                }
                            }
                            text = Convert.ToString(r.ReadLine()).Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            n = 0;
                            for (int j = 1; j < _NY + 1; j++)
                            {
                                for (int i = 1; i < _NX + 1; i++)
                                {
                                    _Z0[i, j] = Convert.ToDouble(text[n].Replace(".", decsep));
                                    n++;
                                }
                            }
                            text = Convert.ToString(r.ReadLine()).Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            n = 0;
                            for (int j = 1; j < _NY + 1; j++)
                            {
                                for (int i = 1; i < _NX + 1; i++)
                                {
                                    _FW[i, j] = Convert.ToDouble(text[n].Replace(".", decsep));
                                    n++;
                                }
                            }
                            text = Convert.ToString(r.ReadLine()).Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            n = 0;
                            for (int j = 1; j < _NY + 1; j++)
                            {
                                for (int i = 1; i < _NX + 1; i++)
                                {
                                    _EPSG[i, j] = Convert.ToDouble(text[n].Replace(".", decsep));
                                    n++;
                                }
                            }
                            text = Convert.ToString(r.ReadLine()).Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            n = 0;
                            for (int j = 1; j < _NY + 1; j++)
                            {
                                for (int i = 1; i < _NX + 1; i++)
                                {
                                    _ALBEDO[i, j] = Convert.ToDouble(text[n].Replace(".", decsep));
                                    n++;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        //exporting landuse.asc for GRAMM sub-domain
        /// <summary>
        /// Exporting landuse.asc for GRAMM sub-domain
        /// </summary>
        public bool ExportLanduse()
        {
            try
            {
                StreamWriter writer = new StreamWriter(Path.Combine(_projectname, @"Computation", "landuse.asc"));
                for (int j = 1; j < _NY + 1; j++)
                {
                    for (int i = 1; i < _NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_RHOB[i, j], 0), ic) + " ");
                    }
                }
                writer.WriteLine();
                for (int j = 1; j < _NY + 1; j++)
                {
                    for (int i = 1; i < _NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_ALAMBDA[i, j], 3), ic) + " ");
                    }
                }
                writer.WriteLine();
                for (int j = 1; j < _NY + 1; j++)
                {
                    for (int i = 1; i < _NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_Z0[i, j], 4), ic) + " ");
                    }
                }
                writer.WriteLine();
                for (int j = 1; j < _NY + 1; j++)
                {
                    for (int i = 1; i < _NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_FW[i, j], 4), ic) + " ");
                    }
                }
                writer.WriteLine();
                for (int j = 1; j < _NY + 1; j++)
                {
                    for (int i = 1; i < _NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_EPSG[i, j], 4), ic) + " ");
                    }
                }
                writer.WriteLine();
                for (int j = 1; j < _NY + 1; j++)
                {
                    for (int i = 1; i < _NX + 1; i++)
                    {
                        writer.Write(Convert.ToString(Math.Round(_ALBEDO[i, j], 3), ic) + " ");
                    }
                }
                writer.WriteLine();
                writer.Close();
                writer.Dispose();

                GralIO.WriteESRIFile Result = new GralIO.WriteESRIFile
                {
                    NCols = _NX,
                    NRows = _NY,
                    YllCorner = _JKOOA,
                    XllCorner = _IKOOA,
                    CellSize = _DX,
                    Z = -1,

                    //output of Roughness length
                    Unit = "m",
                    Round = 4,
                    DblArr = _Z0,
                    FileName = Path.Combine(_projectname, @"Maps", "roughness.txt")
                };
                Result.WriteDblArrResult();

                //output of surface density
                Result.Unit = string.Empty;
                Result.Round = 0;
                Result.DblArr = _RHOB;
                Result.FileName = Path.Combine(_projectname, @"Maps", "density.txt");
                Result.WriteDblArrResult();

                //output of heat conductivity
                Result.Unit = string.Empty;
                Result.Round = 2;
                Result.DblArr = _ALAMBDA;
                Result.FileName = Path.Combine(_projectname, @"Maps", "conductivity.txt");
                Result.WriteDblArrResult();
                
                //output of surface moisture
                Result.Unit = string.Empty;
                Result.Round = 3;
                Result.DblArr = _FW;
                Result.FileName = Path.Combine(_projectname, @"Maps", "moisture.txt");
                Result.WriteDblArrResult();
                
                //output of emissivity
                Result.Unit = string.Empty;
                Result.Round = 3;
                Result.DblArr = _EPSG;
                Result.FileName = Path.Combine(_projectname, @"Maps", "emissivity.txt");
                Result.WriteDblArrResult();
                
                //output of surface albedo
                Result.Unit = string.Empty;
                Result.Round = 3;
                Result.DblArr = _ALBEDO;
                Result.FileName = Path.Combine(_projectname, @"Maps", "albedo.txt");
                Result.WriteDblArrResult();
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
