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
 * Date: 15.01.2019
 * Time: 17:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using GralStaticFunctions;

namespace GralIO
{
    /// <summary>
    /// Class to Read Header of World Files
    /// </summary>
    public class ReadWorldFileHeader
    {
        public string Mapfile { get; set; }
        public string Imagefile { get; set; }
        public double PixelMx { get; set; }
        public double PixelMy { get; set; }
        public double West { get; set; }
        public double North { get; set; }
        
        /// <summary>
        /// Read the Header of world files
        /// </summary>
        public bool ReadHeader()
        {
            try
            {
                //open data of world file
                using (StreamReader myReader = new StreamReader(Mapfile))
                {
                    PixelMx = St_F.TxtToDbl(myReader.ReadLine(), false);
                    
                    double dummy = St_F.TxtToDbl(myReader.ReadLine(), false);
                    dummy = St_F.TxtToDbl(myReader.ReadLine(), false);
                    
                    PixelMy = St_F.TxtToDbl(myReader.ReadLine(), false);
                    West = St_F.TxtToDbl(myReader.ReadLine(), false);
                    North = St_F.TxtToDbl(myReader.ReadLine(), false);
                    
                    if (myReader.EndOfStream)
                    {
                        Imagefile = String.Empty;
                    }
                    else
                    {
                        Imagefile = myReader.ReadLine();
                    }
                }
                
                #if __MonoCS__
                if (Imagefile.Contains(@"\")) // compatibility to windows projects
                {
                    string[] text = Imagefile.Split( '\\' );
                    if(text.Length > 0)
                    {
                        string map_path = Path.GetDirectoryName(Mapfile); 		   // Path of mapfile +
                        Imagefile = Path.Combine(map_path, text[text.Length -1]);  // Filename
                    }
                }
                #endif
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }    
}
