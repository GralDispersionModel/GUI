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
 * Time: 16:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;

namespace GralDomain
{
   public partial class Domain
   {
        /// <summary>
        /// Search edge point next to the mouse position and start rubberline to move this edgepoint
        /// </summary>
        private void MoveEdgepointArea()
		{
			int i=0;
			double min = 10000000000;
			int indexmin=0;
			int vertices = 0;
			if (EditAS.ItemDisplayNr >= 0 && EditAS.ItemDisplayNr < EditAS.ItemData.Count)
			{
				vertices = EditAS.ItemData[EditAS.ItemDisplayNr].Pt.Count;
			}
			
			if (vertices < 1)
				return;
			
			while (i < vertices)
			{
				double dx = Convert.ToDouble(textBox1.Text.Replace(".",decsep)) - EditAS.CorneAareaX[i];
				double dy = Convert.ToDouble(textBox2.Text.Replace(".",decsep)) - EditAS.CornerAreaY[i];
				if (Math.Sqrt(dx*dx+dy*dy) < min) // search min
				{
					min = Math.Sqrt(dx*dx+dy*dy);
					indexmin = i;
				}
				i++;
			}
			// Reset Rubber-Line Drawing
			Cursor.Clip = Bounds;
			
			// 1st line
			int index = indexmin - 1;
			if (index < 0)
				index = vertices - 1;
			
			CornerAreaSource[1].X =  Convert.ToInt32((EditAS.CorneAareaX[index] - MapSize.West) * 1 / BmpScale / MapSize.SizeX + TransformX);
			CornerAreaSource[1].Y =  Convert.ToInt32((EditAS.CornerAreaY[index] - MapSize.North) * 1 / BmpScale / MapSize.SizeY + TransformY);
			
			//2nd line
			index  = indexmin + 1;
			if (index >= vertices)
				index = 0;
			RubberLineCoors[1].X =  Convert.ToInt32((EditAS.CorneAareaX[index] - MapSize.West) * 1 / BmpScale / MapSize.SizeX + TransformX);
			RubberLineCoors[1].Y =  Convert.ToInt32((EditAS.CornerAreaY[index] - MapSize.North) * 1 / BmpScale / MapSize.SizeY + TransformY);
			
			RubberLineCoors[0].X = -1;RubberLineCoors[0].Y = -1; // for lenght label
			EditAS.CornerAreaCount = indexmin;
			MouseControl = 108; // Marker for editing area-source point
			//MessageBox.Show(this, Convert.ToString(indexmin));
		}
        
		/// <summary>
        /// Search nearest edge point to the mouse position and start rubberline to move this edgepoint
        /// </summary>
		private void MoveEdgepointVegetation()
		{
			int i=0;
			double min = 10000000000;
			int indexmin=0;
			int vertices = 0;
			if (EditVegetation.ItemDisplayNr >= 0 && EditVegetation.ItemDisplayNr < EditVegetation.ItemData.Count)
			{
				vertices = EditVegetation.ItemData[EditVegetation.ItemDisplayNr].Pt.Count;
			}
			
			if (vertices < 1)
				return;
			
			while (i < vertices)
			{
				double dx = Convert.ToDouble(textBox1.Text.Replace(".",decsep)) - EditVegetation.CornerVegX[i];
				double dy = Convert.ToDouble(textBox2.Text.Replace(".",decsep)) - EditVegetation.CornerVegY[i];
				
				if (Math.Sqrt(dx*dx+dy*dy) < min) // search min
				{
					min = Math.Sqrt(dx*dx+dy*dy);
					indexmin = i;
				}
				i++;
			}
			// Reset Rubber-Line Drawing
			Cursor.Clip = Bounds;
			
			// 1st line
			int index = indexmin - 1;
			if (index < 0)
				index = vertices - 1;
			
			CornerAreaSource[1].X =  Convert.ToInt32((EditVegetation.CornerVegX[index] - MapSize.West) * 1 / BmpScale / MapSize.SizeX + TransformX);
			CornerAreaSource[1].Y =  Convert.ToInt32((EditVegetation.CornerVegY[index] - MapSize.North) * 1 / BmpScale / MapSize.SizeY + TransformY);
			
			//2nd line
			index  = indexmin + 1;
			if (index >= vertices)
				index = 0;
			
			RubberLineCoors[1].X =  Convert.ToInt32((EditVegetation.CornerVegX[index] - MapSize.West) * 1 / BmpScale / MapSize.SizeX + TransformX);
			RubberLineCoors[1].Y =  Convert.ToInt32((EditVegetation.CornerVegY[index] - MapSize.North) * 1 / BmpScale / MapSize.SizeY + TransformY);
			
			RubberLineCoors[0].X = -1;RubberLineCoors[0].Y = -1; // for lenght label
			EditVegetation.CornerVegetation = indexmin;
			MouseControl = 109; // Marker for editing vegetation point
			//MessageBox.Show(this, Convert.ToString(indexmin));
		}
		
		/// <summary>
        /// Search nearest edge point to the mouse position and start rubberline to move this edgepoint
        /// </summary>
		private void MoveEdgepointLine()
		{
			int i=0;
			double min = 10000000000;
			int indexmin=0;
			int vertices = 0;
			Int32.TryParse(EditLS.GetNumberOfVerticesText(), out vertices);
			if (vertices < 1)
				return;
			
			while (i < vertices)
			{
				double dx = Convert.ToDouble(textBox1.Text.Replace(".",decsep)) - EditLS.CornerLineX[i];
				double dy = Convert.ToDouble(textBox2.Text.Replace(".",decsep)) - EditLS.CornerLineY[i];
				if (Math.Sqrt(dx*dx+dy*dy) < min) // search min
				{
					min = Math.Sqrt(dx*dx+dy*dy);
					indexmin = i;
				}
				i++;
			}
			// Reset Rubber-Line Drawing
			Cursor.Clip = Bounds;
			
			// 1st line
			int index = indexmin - 1;
			if (index < 0)
				index = indexmin + 1;
			CornerAreaSource[1].X =  Convert.ToInt32((EditLS.CornerLineX[index] - MapSize.West) * 1 / BmpScale / MapSize.SizeX + TransformX);
			CornerAreaSource[1].Y =  Convert.ToInt32((EditLS.CornerLineY[index] - MapSize.North) * 1 / BmpScale / MapSize.SizeY + TransformY);
			
			//2nd line
			index  = indexmin + 1;
			if (index >= vertices)
				index = vertices - 2;
			
			RubberLineCoors[1].X =  Convert.ToInt32((EditLS.CornerLineX[index] - MapSize.West) * 1 / BmpScale / MapSize.SizeX + TransformX);
			RubberLineCoors[1].Y =  Convert.ToInt32((EditLS.CornerLineY[index] - MapSize.North) * 1 / BmpScale / MapSize.SizeY + TransformY);
			RubberLineCoors[0].X = -1;RubberLineCoors[0].Y = -1; // for lenght label
			
			EditLS.CornerLineSource = indexmin;
			MouseControl = 100; // Marker for editing line-source point
		}
		
		/// <summary>
        /// Search nearest edge point to the mouse position and start rubberline to move this edgepoint
        /// </summary>
		private void MoveEdgepointWall()
		{
			int i=0;
			double min = 10000000000;
			int indexmin=0;
			int vertices = 0;
			Int32.TryParse(EditWall.GetNumberOfVerticesText(), out vertices);
			if (vertices < 1)
				return;
			
			while (i < vertices)
			{
				double dx = Convert.ToDouble(textBox1.Text.Replace(".",decsep)) - EditWall.CornerWallX[i];
				double dy = Convert.ToDouble(textBox2.Text.Replace(".",decsep)) - EditWall.CornerWallY[i];
				if (Math.Sqrt(dx*dx+dy*dy) < min) // search min
				{
					min = Math.Sqrt(dx*dx+dy*dy);
					indexmin = i;
				}
				i++;
			}
			// Reset Rubber-Line Drawing
			Cursor.Clip = Bounds;
			
			// 1st line
			int index = indexmin - 1;
			if (index < 0)
				index = indexmin + 1;
			CornerAreaSource[1].X =  Convert.ToInt32((EditWall.CornerWallX[index] - MapSize.West) * 1 / BmpScale / MapSize.SizeX + TransformX);
			CornerAreaSource[1].Y =  Convert.ToInt32((EditWall.CornerWallY[index] - MapSize.North) * 1 / BmpScale / MapSize.SizeY + TransformY);
			
			//2nd line
			index  = indexmin + 1;
			if (index >= vertices)
				index = vertices - 2;
			
			RubberLineCoors[1].X =  Convert.ToInt32((EditWall.CornerWallX[index] - MapSize.West) * 1 / BmpScale / MapSize.SizeX + TransformX);
			RubberLineCoors[1].Y =  Convert.ToInt32((EditWall.CornerWallY[index] - MapSize.North) * 1 / BmpScale / MapSize.SizeY + TransformY);
			RubberLineCoors[0].X = -1;RubberLineCoors[0].Y = -1; // for lenght label
			EditWall.CornerWallCount = indexmin;
			MouseControl = 101; // Marker for editing wall point
		}
		
		/// <summary>
        /// Search nearest edge point to the mouse position and start rubberline to move this edgepoint
        /// </summary>
		private void MoveEdgepointBuilding()
		{
			int i=0;
			double min = 10000000000;
			int indexmin=0;
			int vertices = 0;
			if (EditB.ItemDisplayNr >= 0 && EditB.ItemDisplayNr < EditB.ItemData.Count)
			{
				vertices = EditB.ItemData[EditB.ItemDisplayNr].Pt.Count;
			}
			if (vertices < 1)
				return;
			
			while (i < vertices)
			{
				double dx = Convert.ToDouble(textBox1.Text.Replace(".",decsep)) - EditB.CornerBuildingX[i];
				double dy = Convert.ToDouble(textBox2.Text.Replace(".",decsep)) - EditB.CornerBuildingY[i];
				if (Math.Sqrt(dx*dx+dy*dy) < min) // search min
				{
					min = Math.Sqrt(dx*dx+dy*dy);
					indexmin = i;
				}
				i++;
			}
			// Reset Rubber-Line Drawing
			Cursor.Clip = Bounds;
			
			// 1st line
			int index = indexmin - 1;
			if (index < 0)
				index = vertices - 1;
			CornerAreaSource[1].X =  Convert.ToInt32((EditB.CornerBuildingX[index] - MapSize.West) * 1 / BmpScale / MapSize.SizeX + TransformX);
			CornerAreaSource[1].Y =  Convert.ToInt32((EditB.CornerBuildingY[index] - MapSize.North) * 1 / BmpScale / MapSize.SizeY + TransformY);
			
			//2nd line
			index  = indexmin + 1;
			
			if (index >= vertices)
				index = 0;
			
			RubberLineCoors[1].X =  Convert.ToInt32((EditB.CornerBuildingX[index] - MapSize.West) * 1 / BmpScale / MapSize.SizeX + TransformX);
			RubberLineCoors[1].Y =  Convert.ToInt32((EditB.CornerBuildingY[index] - MapSize.North) * 1 / BmpScale / MapSize.SizeY + TransformY);
			RubberLineCoors[0].X = -1;RubberLineCoors[0].Y = -1; // for lenght label
			EditB.CornerBuilding = indexmin;
			MouseControl = 117; // Marker for editing buliding edge
			//MessageBox.Show(this, Convert.ToString(indexmin));
		}        
    }
}