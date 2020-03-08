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
 * Date: 17.01.2019
 * Time: 17:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Windows.Forms;
using GralMessage;

namespace Gral
{
    /// <summary>
    /// The functions within the Main MeteoTab.
    /// </summary>
    partial class Main
	{
        /// <summary>
        /// deletes all intermediate GRAL *.con and *.odr files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRALDeleteConcentrationFiles(object sender, EventArgs e)
        {
            if (ProjectName == "") return; // exit if no project loaded
            try
            {
                string newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                DirectoryInfo di = new DirectoryInfo(newPath);
                FileInfo[] files_conc = di.GetFiles("*.con");
                if (files_conc.Length > 0)
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage())
                    {
                        if (files_conc.Length > 0)
                            fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "*.con");

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            for (int i = 0; i < files_conc.Length; i++)
                                files_conc[i].Delete();
                            Project_Locked = false;                 // unlock project
                            ProjectLockedButtonClick(null, null); // change locked-Button
                            DeleteTempGralFiles();
                        }
                    }
                }
                
                files_conc = di.GetFiles("*.grz"); //compressed files ?
                if (files_conc.Length > 0)
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage())
                    {
                        if (files_conc.Length > 0)
                            fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "*.grz");

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            for (int i = 0; i < files_conc.Length; i++)
                                files_conc[i].Delete();
                            Project_Locked = false;                 // unlock project
                            ProjectLockedButtonClick(null, null); // change locked-Button
                            DeleteTempGralFiles();
                        }
                    }
                }

                files_conc = di.GetFiles("*.odr");
                if (files_conc.Length > 0)
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage())
                    {
                        if (files_conc.Length > 0)
                            fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "*.odr");

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            for (int i = 0; i < files_conc.Length; i++)
                                files_conc[i].Delete();
                            Project_Locked = false;                 // unlock project
                            ProjectLockedButtonClick(null, null); // change locked-Button
                            DeleteTempGralFiles();
                        }
                    }
                }

                files_conc = di.GetFiles("*.dep");
                if (files_conc.Length > 0)
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage())
                    {
                        if (files_conc.Length > 0)
                            fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "*.dep");

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            for (int i = 0; i < files_conc.Length; i++)
                                files_conc[i].Delete();
                            Project_Locked = false;                 // unlock project
                            ProjectLockedButtonClick(null, null); // change locked-Button
                            DeleteTempGralFiles();
                        }
                    }
                }

                newPath = Path.Combine(ProjectName, "Computation", "DispNr.txt");
                if (File.Exists(newPath))
                    File.Delete(newPath);

                newPath = Path.Combine(ProjectName, "Computation", "Percent.txt");
                if (File.Exists(newPath))
                    File.Delete(newPath);

                newPath = Path.Combine(ProjectName, "Computation", "zeitreihe.dat");
                if (File.Exists(newPath))
                    File.Delete(newPath);

                newPath = Path.Combine(ProjectName, "Computation", "ReceptorConcentrations.dat");
                if (File.Exists(newPath))
                    File.Delete(newPath);

                newPath = Path.Combine(ProjectName, "Computation", "GRAL_Meteozeitreihe.dat");
                if (File.Exists(newPath))
                    File.Delete(newPath);

                DispNrChanged(null, null); // change displaynumber
                GralFileSizes();
            }
            catch
            { }
        }

        private void DeleteTempGralFiles()
        {
            try
            {
                string file = Path.Combine(ProjectName, "Computation", "Transient_Concentrations1.tmp");
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                file = Path.Combine(ProjectName, "Computation", "Transient_Concentrations2.tmp");
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                file = Path.Combine(ProjectName, "Computation", "Vertical_Concentrations.tmp");
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch { }
        }

        /// <summary>
        /// deletes all GRAMM Windfield files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GRAMMDeleteWindFields(object sender, EventArgs e)
        {
            if (ProjectName == "") return; // exit if no project loaded
            try
            {
                string newPath = Path.Combine(ProjectName, "Computation" + Path.DirectorySeparatorChar);
                DirectoryInfo di = new DirectoryInfo(newPath);

                FileInfo[] files_wnd = di.GetFiles("*.wnd");
                if (files_wnd.Length > 0)
                {
                    using (FileDeleteMessage fdm = new FileDeleteMessage
                    {
                        deletegramm = true
                    })
                    {
                        if (files_wnd.Length > 0)
                            fdm.listView1.Items.Add("..Computation" + Path.DirectorySeparatorChar + "*.wnd");

                        if (fdm.ShowDialog() == DialogResult.OK)
                        {
                            for (int i = 0; i < files_wnd.Length; i++)
                                files_wnd[i].Delete();

                            //delete *.scl files
                            files_wnd = di.GetFiles("*.scl");
                            if (files_wnd.Length > 0)
                            {
                                for (int i = 0; i < files_wnd.Length; i++)
                                    files_wnd[i].Delete();
                            }

                            //delete *.obl files
                            files_wnd = di.GetFiles("*.obl");
                            if (files_wnd.Length > 0)
                            {
                                for (int i = 0; i < files_wnd.Length; i++)
                                    files_wnd[i].Delete();
                            }

                            //delete *.ust files
                            files_wnd = di.GetFiles("*.ust");
                            if (files_wnd.Length > 0)
                            {
                                for (int i = 0; i < files_wnd.Length; i++)
                                    files_wnd[i].Delete();
                            }

                            //delete steady_state.txt files
                            files_wnd = di.GetFiles("?????_steady_state.txt");
                            if (files_wnd.Length > 0)
                            {
                                for (int i = 0; i < files_wnd.Length; i++)
                                    files_wnd[i].Delete();
                            }

                            GRAMM_Locked = false;                 // unlock GRAMM project
                            Gramm_locked_buttonClick(null, null); // change locked-Button
                        }
                    }
                }

                newPath = Path.Combine(ProjectName, "Computation", "DispNrGramm.txt");
                if (File.Exists(newPath))
                    File.Delete(newPath);
                DispnrGrammChanged(null, null); // change displaynumber
                GralFileSizes(); // actualize file size
            }
            catch
            { }
        }

        /// <summary>
        /// deletes all intermediate GRAL *.gff files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GRALDeleteFlowFieldFiles(object sender, EventArgs e)
        {
            if (ProjectName == "") return; // exit if no project loaded
            DeleteGralGffFile();
        }
		
		//delete file GRAL_topofile.txt whenever items are changed by the user affecting the topography
        /// <summary>
        /// Deletes the high resolution GRAL_topofile.txt
        /// </summary>
        public void DeleteGralTopofile()
        {
            try
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "GRAL_topofile.txt"));
            }
            catch
            { }
        }

        private void DeleteGralGeometries()
        {
            try
            {
                File.Delete(Path.Combine(ProjectName, @"Computation", "GRAL_geometries.txt"));
            }
            catch
            { }
        }

        //delete intermediate *gff-files whenever items are changed by the user affecting the flow field calculations
        /// <summary>
        /// Ask if files should be deleted and deletes all *.gff files and the file GRAL_geometries.txt
        /// </summary>
        /// <returns></returns>
        public DialogResult DeleteGralGffFile()
        {
            DialogResult dia = new DialogResult();
            dia = DialogResult.OK;

            string GffFilePath = GralStaticFunctions.St_F.GetGffFilePath(Path.Combine(ProjectName, "Computation")) + Path.DirectorySeparatorChar;
            
            DirectoryInfo di = new DirectoryInfo(GffFilePath);
            FileInfo[] files_conc = di.GetFiles("*.gff");

            if (files_conc.Length > 0)
            {
                using (FileDeleteMessage fdm = new FileDeleteMessage())
                {
                    if (files_conc.Length > 0)
                        fdm.listView1.Items.Add("..." + GffFilePath.Substring(Math.Max(0, GffFilePath.Length - 45)) + "*.gff");

                    if (fdm.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < files_conc.Length; i++)
                        {
                            files_conc[i].Delete();
                        }
                        DeleteGralGeometries();
                    }
                }
            }

            GralFileSizes();
            return dia;
        }

        /// <summary>
        /// Delete Emission Timeseries.txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeleteEmissionTimeseriesFile(object sender, EventArgs e)
        {
            string newpath = Path.Combine(ProjectName, "Computation", "emissions_timeseries.txt");
            if (File.Exists(newpath) == true && MessageBox.Show(this, "Delete emissions_timeseries.txt ?",
                                                                "GRAL Message", MessageBoxButtons.OKCancel,
                                                                MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    File.Delete(newpath);
                    button49.Visible = false;
                    button51.Visible = false;
                    TabControl1Click(null, null);
                }
                catch{}
            }
        }
	}
}

