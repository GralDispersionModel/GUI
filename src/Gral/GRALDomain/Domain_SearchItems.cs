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
 * Date: 24.01.2019
 * Time: 14:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Data;
using System.Windows.Forms;

using GralDomForms;
using GralItemData;
using GralItemForms;

namespace GralDomain
{
	public partial class Domain
	{
		/// <summary>
		/// Collect all item data, start search items dialog and evaluate the user selected item
		/// </summary>
		void SearchItemToolStripMenuItemClick(object sender, EventArgs e)
		{
			HideWindows(0); // hide all edit forms
			using (Search_Item search = new Search_Item())
			{
				DataTable data = new DataTable();
				data.Columns.Add("Nr", typeof(int));
				data.Columns.Add("Name", typeof(string));
				data.Columns.Add("Type", typeof(string));
				data.Columns.Add("Height", typeof(double));
				data.Columns.Add("Vert. ext.", typeof(double));
				data.Columns.Add("Source_group", typeof(int));
				data.Columns.Add("Exit_velocity", typeof(double));
				data.Columns.Add("Temperature", typeof(double));
				data.Columns.Add("Diameter", typeof(double));
                data.Columns.Add("ER_SG", typeof(int));

                int emission_index = 10;
				
				for (int j = 1; j < 11; j++)
				{
					data.Columns.Add("ER Nr. " + Convert.ToString(j), typeof(double));
					data.Columns.Add("Poll. "+ Convert.ToString(j), typeof(string));
				}
				

				int i = 0;
				string[] text1 = new string[1000];

                search.EditPSSearch = EditPS;
                search.EditASSearch = EditAS;
                search.EditLSSearch = EditLS;
                search.EditPortalsSearch = EditPortals;
                search.EditBSearch = EditB;
                search.EditRSearch = EditR;
                search.EditWallSearch = EditWall;
                search.EditVegetationSearch = EditVegetation;
                search.Locked = Gral.Main.Project_Locked;

                foreach (PointSourceData _psdata in EditPS.ItemData)
				{
					try
					{
						i++;
						
						{
							DataRow workrow;
							workrow = data.NewRow();
							workrow[0] = i;
							workrow[1] =  _psdata.Name;
							workrow[2] = "Point source";
							workrow[3] =  Math.Round(_psdata.Height, 1);
							workrow[4] =  0;
							workrow[5] =  _psdata.Poll.SourceGroup;
							workrow[6] =  Math.Round(_psdata.Velocity, 1); // exit velocity
							workrow[7] =  Math.Round(_psdata.Temperature - 273, 1); // Temperature
							workrow[8] =  Math.Round(_psdata.Diameter, 1); // Diameter
                            
							for (int j = 0; j < 10; j++)
							{
								//if (_psdata.Poll.EmissionRate[j] > 0)
								{
									workrow[emission_index + j * 2] = _psdata.Poll.EmissionRate[j];
									workrow[emission_index + 1 + j * 2] = Gral.Main.PollutantList[_psdata.Poll.Pollutant[j]];
								}
							}

							data.Rows.Add(workrow);
						}
					}
					catch {
					}
				}
				
				i=0;
				foreach (LineSourceData _ls in EditLS.ItemData)
				{
					try
					{
						i++;
                        int k = 0;
						foreach (PollutantsData _poll in _ls.Poll)
						{
							DataRow workrow;
							workrow = data.NewRow();
							workrow[0] = i;
							workrow[1] =  _ls.Name;
							workrow[2] = "Line source";
							workrow[3] =  Math.Round(_ls.Height, 1);
							workrow[4] =  Math.Round(_ls.VerticalExt, 1); // vert extension
							workrow[5] = _poll.SourceGroup;
                            workrow[9] = k; // Emission Rate index for multiple SG per source

                            for (int j = 0; j < 10; j++)
							{
								workrow[emission_index + j * 2] = _poll.EmissionRate[j];
								workrow[emission_index + 1 + j * 2] = Gral.Main.PollutantList[_poll.Pollutant[j]];
							}
							
							data.Rows.Add(workrow);
                            k++;
						}
					}
					catch
					{}
				}
				
				i=0;
				foreach (AreaSourceData _as in EditAS.ItemData)
				{
					try
					{
						i++;
						
						decimal height = Convert.ToDecimal(_as.Height);
						height = Math.Abs(height);
						double vert_ext = Convert.ToDouble(_as.VerticalExt);
						
						DataRow workrow;
						workrow = data.NewRow();
						workrow[0] = i;
						workrow[1] =  _as.Name;
						workrow[2] = "Area source";
						workrow[3] =  Math.Round(Convert.ToDouble(height), 1);
						workrow[4] =  Math.Round(vert_ext, 1);
						workrow[5] = _as.Poll.SourceGroup;
						
						for (int j = 0; j < 10; j++)
						{
							//if (_as.Poll.EmissionRate[j] > 0)
							{
								workrow[emission_index + j * 2] = _as.Poll.EmissionRate[j];
								workrow[emission_index + 1 + j * 2] = Gral.Main.PollutantList[_as.Poll.Pollutant[j]];
							}
						}
						
						data.Rows.Add(workrow);
						
					}
					catch {
					}
				}

				i=0;
				foreach (PortalsData _po in EditPortals.ItemData)
				{
					try
					{
						i++;

                        int k = 0;
						foreach (PollutantsData _poll in _po.Poll)
						{
							DataRow workrow;
							workrow = data.NewRow();
							workrow[0] = i;
							workrow[1] =  _po.Name;
							workrow[2] = "Portal source";
							workrow[3] =  0;
							workrow[4] =   Math.Round( _po.Height, 1);
							workrow[5] =  _poll.SourceGroup;
							workrow[6] =  Math.Round(_po.ExitVel, 1); // exit velocity
							workrow[7] =  Math.Round(_po.DeltaT, 1); // Temperature
                            workrow[9] = k; // Emission Rate index for multiple SG per source

                            for (int j = 0; j < 10; j++)
							{
								//if (_poll.EmissionRate[j] > 0)
								{
									workrow[emission_index + j] = _poll.EmissionRate[j];
									workrow[emission_index + j + 1] = Gral.Main.PollutantList[_poll.Pollutant[j]];
								}
							}
							data.Rows.Add(workrow);
                            k++;
						}
						
					}
					catch {
					}
				}
				
				i=0;
				foreach (BuildingData _bd in EditB.ItemData)
				{
					try
					{
						i++;
						data.Rows.Add(i, _bd.Name, "Building" , Math.Round(_bd.Height, 1));
					}
					catch
					{}
				}
				
				i=0;
				foreach (ReceptorData _rd in EditR.ItemData)
				{
					try
					{
						i++;
						data.Rows.Add(i, _rd.Name, "Receptor point" , Math.Round(_rd.Height, 1));
					}
					catch
					{
					}
				}
				
				i=0;
				foreach (WallData _wd in EditWall.ItemData)
				{
					try
					{
						i++;
						data.Rows.Add(i, _wd.Name, "Wall");
					}
					catch {
					}
				}
				
				i=0;
				foreach (VegetationData _vdata in EditVegetation.ItemData)
				{
					try
					{
						i++;
						data.Rows.Add(i, _vdata.Name, "Vegetation" , Math.Round(_vdata.VerticalExt, 1), false);
					}
					catch
					{}
				}
				
				for (i = 0; i < 6; i++) // set basic data visible
                {
                    SearchDatagridviewVisible[i] = true;
                }

                SearchDatagridviewVisible[emission_index] = true;
				SearchDatagridviewVisible[emission_index + 1] = true;
				
				search.Items_visible = SearchDatagridviewVisible; // set visible items
				search.Data = data;
                search.StartPosition = FormStartPosition.Manual;
                search.Left = GralStaticFunctions.St_F.GetScreenAtMousePosition() + 160;
                search.Top = 80;
                search.Size = SearchFormSize;
                search.FormSize = SearchFormSize;
                
                search.ShowDialog();
				SearchDatagridviewVisible = search.Items_visible; // get visible items
                SearchFormSize = search.FormSize;

				// search selected item
				i = search.Selected_item;
				string type = search.Selected_Type;

                if (i == -1)
                {
                    WriteAllItemsToDisk(null, null);
                }
                else
                {
                    if (type == "Point source")
                    {
                        if (EditPS.SetTrackBar(i))
                        {
                            EditPS.ItemDisplayNr = i - 1;
                            EditPS.FillValues();

                            if (checkBox4.Checked == false)
                            {
                                PointSourcesToolStripMenuItemClick(null, null);
                            }

                            // move map to the item
                            if ((i - 1) >= 0 && (i - 1) < EditPS.ItemData.Count)
                            {
                                double x0 = Convert.ToDouble(EditPS.ItemData[i - 1].Pt.X) - 150;
                                double y0 = Convert.ToDouble(EditPS.ItemData[i - 1].Pt.Y) + 100;
                                double xmax = x0 + 300;

                                ZoomSection(x0, xmax, y0);
                            }
                        }
                        else
                        {
                            MessageBox.Show(this, "This element is not available - check the 'Control files' button", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    if (type == "Line source")
                    {
                        if (EditLS.SetTrackBar(i))
                        {
                            EditLS.ItemDisplayNr = i - 1;
                            EditLS.FillValues();

                            if (checkBox8.Checked == false)
                            {
                                LineSourcesToolStripMenuItemClick(null, null);
                            }

                            // move map to the item
                            if (text1.Length > 32)
                            {
                                double x0 = Convert.ToDouble(EditLS.ItemData[i - 1].Pt[0].X) - 150;
                                double y0 = Convert.ToDouble(EditLS.ItemData[i - 1].Pt[0].Y) + 100;
                                double xmax = x0 + 300;

                                ZoomSection(x0, xmax, y0);
                            }
                        }
                        else
                        {
                            MessageBox.Show(this, "This element is not available - check the 'Control files' button", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    if (type == "Area source")
                    {
                        if (EditAS.SetTrackBar(i))
                        {
                            EditAS.ItemDisplayNr = i - 1;
                            EditAS.FillValues();

                            if (checkBox5.Checked == false)
                            {
                                AreaSourcesToolStripMenuItemClick(null, null);
                            }

                            // move map to the item
                            if ((i - 1) >= 0 && (i - 1) < EditAS.ItemData.Count)
                            {
                                double x0 = Convert.ToDouble(EditAS.ItemData[i - 1].Pt[0].X) - 150;
                                double y0 = Convert.ToDouble(EditAS.ItemData[i - 1].Pt[0].Y) + 100;
                                double xmax = x0 + 300;

                                ZoomSection(x0, xmax, y0);
                            }
                        }
                        else
                        {
                            MessageBox.Show(this, "This element is not available - check the 'Control files' button", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    if (type == "Portal source")
                    {
                        if (EditPortals.SetTrackBar(i))
                        {
                            EditPortals.ItemDisplayNr = i - 1;
                            EditPortals.FillValues();

                            if (checkBox12.Checked == false)
                            {
                                TunnelPortalsToolStripMenuItemClick(null, null);
                            }

                            // move map to the item
                            if ((i - 1) >= 0 && (i - 1) < EditPortals.ItemData.Count)
                            {
                                double x0 = Convert.ToDouble(EditPortals.ItemData[i - 1].Pt1.X) - 150;
                                double y0 = Convert.ToDouble(EditPortals.ItemData[i - 1].Pt1.Y) + 100;
                                double xmax = x0 + 300;

                                ZoomSection(x0, xmax, y0);
                            }
                        }
                        else
                        {
                            MessageBox.Show(this, "This element is not available - check the 'Control files' button", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    if (type == "Building")
                    {
                        if (EditB.SetTrackBar(i))
                        {
                            EditB.ItemDisplayNr = i - 1;
                            EditB.FillValues();

                            if (checkBox15.Checked == false)
                            {
                                BuildingsToolStripMenuItemClick(null, null);
                            }

                            // move map to the item
                            if ((i - 1) >= 0 && (i - 1) < EditB.ItemData.Count)
                            {
                                double x0 = Convert.ToDouble(EditB.ItemData[i - 1].Pt[0].X) - 150;
                                double y0 = Convert.ToDouble(EditB.ItemData[i - 1].Pt[0].Y) + 100;
                                double xmax = x0 + 300;

                                ZoomSection(x0, xmax, y0);
                            }
                        }
                        else
                        {
                            MessageBox.Show(this, "This element is not available - check the 'Control files' button", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    if (type == "Receptor point")
                    {
                        if (EditR.SetTrackBar(i))
                        {
                            EditR.ItemDisplayNr = i - 1;
                            EditR.FillValues();

                            // move map to the item
                            if (checkBox20.Checked == false)
                            {
                                ReceptorPointsToolStripMenuItemClick(null, null);
                            }

                            double x0 = EditR.ItemData[i - 1].Pt.X - 150;
                            double y0 = EditR.ItemData[i - 1].Pt.Y + 100;
                            double xmax = x0 + 300;

                            ZoomSection(x0, xmax, y0);
                        }
                        else
                        {
                            MessageBox.Show(this, "This element is not available - check the 'Control files' button", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    if (type == "Wall")
                    {
                        if (EditWall.SetTrackBar(i))
                        {
                            EditWall.ItemDisplayNr = i - 1;
                            EditWall.FillValues();

                            if (checkBox25.Checked == false)
                            {
                                WallsToolStripMenuItemClick(null, null);
                            }

                            // move map to the item
                            WallData _wd = EditWall.ItemData[i - 1];
                            double x0 = _wd.Pt[0].X - 150;
                            double y0 = _wd.Pt[0].Y + 100;
                            double xmax = x0 + 300;
                            ZoomSection(x0, xmax, y0);
                        }
                        else
                        {
                            MessageBox.Show(this, "This element is not available - check the 'Control files' button", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    if (type == "Vegetation")
                    {
                        if (EditVegetation.SetTrackBar(i))
                        {
                            EditVegetation.ItemDisplayNr = i - 1;
                            EditVegetation.FillValues();

                            if (checkBox26.Checked == false)
                            {
                                VegetationToolStripMenuItemClick(null, null);
                            }

                            // move map to the item
                            VegetationData _vdata = EditVegetation.ItemData[i - 1];
                            double x0 = _vdata.Pt[0].X - 150;
                            double y0 = _vdata.Pt[0].Y + 100;
                            double xmax = x0 + 300;
                            ZoomSection(x0, xmax, y0);
                        }
                        else
                        {
                            MessageBox.Show(this, "This element is not available - check the 'Control files' button", "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
				data = null;
			}
		}
	}
}
