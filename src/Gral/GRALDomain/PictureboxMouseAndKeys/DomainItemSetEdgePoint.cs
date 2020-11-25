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
 * User: U0178969
 * Date: 21.01.2019
 * Time: 16:51
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.Drawing;

namespace GralDomain
{
    public partial class Domain
	{
        /// <summary>
        /// Set the new edge point coordinates
        /// </summary>
     	private void SetNewEdgepointArea()
		{
			EditAS.CornerAreaX[EditAS.CornerAreaCount] = Convert.ToDouble(textBox1.Text.Replace(".",decsep));
			EditAS.CornerAreaY[EditAS.CornerAreaCount] = Convert.ToDouble(textBox2.Text.Replace(".",decsep));
			
			EditAS.SaveArray();
			
			EditAS.CornerAreaCount = 0;
			// Reset Rubber-Line Drawing
			Cursor.Clip = Rectangle.Empty;

            if (MouseControl == MouseMode.AreaSourceEditFinal)
            {
                MouseControl = MouseMode.AreaSourcePos; // reset to areasource-Input
            }
            else if (MouseControl == MouseMode.AreaInlineEdit)
            {
                EditAndSaveAreaSourceData(this, null); // save changes
                MouseControl = MouseMode.AreaSourceSel; //continue area selection
            }
			RubberLineCoors[0].X = -1;RubberLineCoors[0].Y = -1; // for lenght label
			Picturebox1_Paint();
		}
		
     	/// <summary>
        /// Set the new edge point coordinates
        /// </summary>
		private void SetNewEdgepointLine()
		{
			EditLS.CornerLineX[EditLS.CornerLineSource] = Convert.ToSingle(textBox1.Text.Replace(".",decsep));
			EditLS.CornerLineY[EditLS.CornerLineSource] = Convert.ToSingle(textBox2.Text.Replace(".",decsep));
			
			EditLS.SaveArray();

			EditLS.CornerLineSource = 0;
			// Reset Rubber-Line Drawing
			Cursor.Clip = Rectangle.Empty;
			
			if (MouseControl == MouseMode.LineSourceEditFinal)
            {
                MouseControl = MouseMode.LineSourcePos; // reset to linesource-Input
            }
            else if (MouseControl == MouseMode.LineSourceInlineEdit)
			{
				EditAndSaveLineSourceData(null, null); // save changes
				MouseControl = MouseMode.LineSourceSel; //continue line selection
			}
			RubberLineCoors[0].X = -1;RubberLineCoors[0].Y = -1; // for lenght label
			Picturebox1_Paint();
		}
		
		/// <summary>
        /// Set the new edge point coordinates
        /// </summary>
		private void SetNewEdgepointWall()
		{
			EditWall.CornerWallX[EditWall.CornerWallCount] = Convert.ToDouble(textBox1.Text.Replace(".",decsep));
			EditWall.CornerWallY[EditWall.CornerWallCount] = Convert.ToDouble(textBox2.Text.Replace(".",decsep));
			EditWall.SaveArray();
			
			EditWall.CornerWallCount = 0;
			// Reset Rubber-Line Drawing
			Cursor.Clip = Rectangle.Empty;
			
			if (MouseControl == MouseMode.WallEditFinal)
            {
                MouseControl = MouseMode.WallSet; // reset to wall-Input
            }
            else if (MouseControl == MouseMode.WallInlineEdit)
			{
				EditAndSaveWallData(this, null); // save changes
				MouseControl = MouseMode.WallSel; //continue line selection
			}
			RubberLineCoors[0].X = -1;RubberLineCoors[0].Y = -1; // for lenght label
			Picturebox1_Paint();
		}
		
		/// <summary>
        /// Set the new edge point coordinates
        /// </summary>
		private void SetNewEdgepointBuilding()
		{
			EditB.CornerBuildingX[EditB.CornerBuilding] = Convert.ToDouble(textBox1.Text.Replace(".",decsep));
			EditB.CornerBuildingY[EditB.CornerBuilding] = Convert.ToDouble(textBox2.Text.Replace(".",decsep));
			EditB.SaveArray();
			
			EditB.CornerBuilding = 0;
			// Reset Rubber-Line Drawing
			Cursor.Clip = Rectangle.Empty;
			
			if (MouseControl == MouseMode.BuildingEditFinal)
            {
                MouseControl = MouseMode.BuildingPos; // reset to building-Input
            }
            else if (MouseControl == MouseMode.BuildingInlineEdit)
			{
				EditAndSaveBuildingsData(null, null); // save changes
				MouseControl = MouseMode.BuildingSel; //continue building selection
			}
			RubberLineCoors[0].X = -1;RubberLineCoors[0].Y = -1; // for lenght label
			Picturebox1_Paint();
		}
		
		/// <summary>
        /// Set the new edge point coordinates
        /// </summary>
		private void SetNewEdgepointVegetation()
		{
			EditVegetation.CornerVegX[EditVegetation.CornerVegetation] = Convert.ToDouble(textBox1.Text.Replace(".",decsep));
			EditVegetation.CornerVegY[EditVegetation.CornerVegetation]  = Convert.ToDouble(textBox2.Text.Replace(".",decsep));
			EditVegetation.SaveArray();
			
			EditVegetation.CornerVegetation = 0;
			// Reset Rubber-Line Drawing
			Cursor.Clip = Rectangle.Empty;
			
			if (MouseControl == MouseMode.VegetationEditFinal)
            {
                MouseControl = MouseMode.AreaPosCorner; // reset to vegetation-Input
            }
            else if (MouseControl == MouseMode.VegetationInlineEdit)
			{
				EditAndSaveVegetationData(this, null); // save changes
				MouseControl = MouseMode.VegetationSel; //continue vegetation selection
			}
			RubberLineCoors[0].X = -1;RubberLineCoors[0].Y = -1; // for lenght label
			Picturebox1_Paint();
		}
    }
}
