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

using System.Drawing;
using System.Drawing.Imaging;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Draw Base Maps
        /// </summary>
        private void DrawBaseMap(Graphics g, DrawingObjects _drobj)
        {
            // Kuntner: check bitmap zoom - avoid error
            if (_drobj.DestRec.Height > 1 && _drobj.DestRec.Width > 1 &&
                _drobj.SourceRec.Height > 1 && _drobj.SourceRec.Width > 1 &&
                _drobj.DestRec.Width < 67108864 && _drobj.DestRec.Height < 67108864 &&
                _drobj.SourceRec.Width < 67108864 && _drobj.SourceRec.Height < 67108864)
            {
                //create a color matrix object
                ColorMatrix matrix = new ColorMatrix();

                //set the opacity
                if (_drobj.Transparancy <= 0 || _drobj.Transparancy >= 100) // set standard-values - compatibility to old projects
                {
                    _drobj.Transparancy = 100;
                }

                matrix.Matrix33 = _drobj.Transparancy * 0.01F;

                //create image attributes
                ImageAttributes attributes = new ImageAttributes();

                //set the color(opacity) of the image
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                //now draw the image
                g.DrawImage(_drobj.Picture, _drobj.DestRec, _drobj.SourceRec.Left, _drobj.SourceRec.Top, _drobj.SourceRec.Width, _drobj.SourceRec.Height, GraphicsUnit.Pixel, attributes);

                matrix = null;
                attributes = null;
                //						g.DrawImage(_drobj.Picture, _drobj.DestRec, _drobj.SourceRec, GraphicsUnit.Pixel);
            }
        }
    }
}
