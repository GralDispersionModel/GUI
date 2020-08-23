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
 * User: Markus
 * Date: 31.01.2019
 * Time: 18:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.IO;

namespace GralItemData
{
    /// <summary>
    /// Contains all possible FileSystemWatchers
    /// </summary>
    public class FileWatcherCollection
	{
		public FileSystemWatcher UVGramm	{ get; set; }  //control changes in the file uv_Gramm.txt, containing the actual GRAMM windfield online
		public FileSystemWatcher UGramm		{ get; set; }  //control changes in the file u_Gramm.txt, containing the actual GRAMM windcomponent u online
		public FileSystemWatcher SpeedGramm	{ get; set; }  //control changes in the file speed_Gramm.txt, containing the actual GRAMM horizontal windspeed online
		public FileSystemWatcher VGramm		{ get; set; }  //control changes in the file v_Gramm.txt, containing the actual GRAMM windcomponent v online
		public FileSystemWatcher WGramm		{ get; set; }  //control changes in the file w_Gramm.txt, containing the actual GRAMM windcomponent w online
		public FileSystemWatcher TAbsGramm	{ get; set; }  //control changes in the file tabs_Gramm.txt, containing the actual GRAMM absolut temperature online
		public FileSystemWatcher TPotGramm	{ get; set; }  //control changes in the file tpot_Gramm.txt, containing the actual GRAMM potential temperature online
		public FileSystemWatcher HumGramm	{ get; set; }  //control changes in the file hum_Gramm.txt, containing the actual GRAMM humidity online
		public FileSystemWatcher NhpGramm	{ get; set; }  //control changes in the file nhp_Gramm.txt, containing the actual GRAMM non-hydrostatic pressure online
		public FileSystemWatcher GlobGramm	{ get; set; }  //control changes in the file glob_Gramm.txt, containing the actual GRAMM global radiation online
		public FileSystemWatcher TerrGramm	{ get; set; }  //control changes in the file terr_Gramm.txt, containing the actual GRAMM terrestrial radiation online
		public FileSystemWatcher SensHeatGramm	{ get; set; }   //control changes in the file sensheat_Gramm.txt, containing the actual GRAMM sensible heat flux online
		public FileSystemWatcher LatHeatGramm	{ get; set; }    //control changes in the file latheat_Gramm.txt, containing the actual GRAMM sensible latent flux online
		public FileSystemWatcher VricVelGramm	{ get; set; }    //control changes in the file fricvel_Gramm.txt, containing the actual GRAMM friction velocity online
		public FileSystemWatcher InverseMOGramm	{ get; set; }  //control changes in the file inverseMO_Gramm.txt, containing the actual GRAMM inverse MO-length online
		public FileSystemWatcher SurfTempGramm	{ get; set; }   //control changes in the file surfTemp_Gramm.txt, containing the actual GRAMM surface temperature online
		public FileSystemWatcher StabClassGramm { get; set; }  //control changes in the file stabilityclass_Gramm.txt, containing the actual GRAMM stabilty classes (1-7) online
		public FileSystemWatcher TkeGramm		{ get; set; }  //control changes in the file tke_Gramm.txt, containing the actual GRAMM turbulent kinetic energy online
		public FileSystemWatcher DisGramm		{ get; set; }  //control changes in the file dis_Gramm.txt, containing the actual GRAMM dissipation online
		
	}
}
