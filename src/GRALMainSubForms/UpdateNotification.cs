#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2022]  [Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace GralMainForms
{
    /// <summary>
    /// Check for updates using a small xml file on GitHub 
    /// </summary>
    public partial class UpdateNotification : Form
    {
        /// <summary>
        /// Show error messages if true
        /// </summary>
        public bool ShowUserInfo = false;
        /// <summary>
        /// Recent app version x.x.x.x e.g. 2.2.0.3
        /// </summary>
        public string RecentVersion;
        private string version;
        private string downloadUri;
        private string changelogUri;
        private BackgroundWorker CheckForUpdates = null;
        private string Url;
        private string Error = string.Empty;
        private string XMLFile = string.Empty;
        private object obj = new object();

        public UpdateNotification()
        {
            this.Hide();
            InitializeComponent();
        }

        /// <summary>
        /// Load the update file from GitHub
        /// </summary>
        public void LoadUpdateFile()
        {
            Url = "https://github.com/GralDispersionModel/GUI/releases/download/V22.03/AutoUpdater.xml";

            if (CheckForUpdates == null)
            {
                CheckForUpdates = new BackgroundWorker();
                CheckForUpdates.DoWork += new System.ComponentModel.DoWorkEventHandler(this.LoadUpdateFileHttp);
                CheckForUpdates.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CheckForUpdatesCompleted);
                CheckForUpdates.WorkerSupportsCancellation = true;

                if (CheckForUpdates.IsBusy != true)
                {
                    CheckForUpdates.RunWorkerAsync();
                }
            }
        }

        /// <summary>
        /// Compare the recent version with the version in the update file
        /// </summary>
        private void CheckForUpdatesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (CheckForUpdates.CancellationPending == false)
                {
                    if (!string.IsNullOrEmpty(XMLFile))
                    {
                        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                        doc.LoadXml(XMLFile);

                        System.Xml.XmlNodeList xmlNode = doc.GetElementsByTagName("version");
                        if (xmlNode.Count > 0)
                        {
                            version = xmlNode[0].InnerText;
                        }
                        xmlNode = doc.GetElementsByTagName("url");
                        if (xmlNode.Count > 0)
                        {
                            downloadUri = xmlNode[0].InnerText;
                        }
                        xmlNode = doc.GetElementsByTagName("changelog");
                        if (xmlNode.Count > 0)
                        {
                            changelogUri = xmlNode[0].InnerText;
                        }

                        int versionDifference = (Convert.ToInt32(version.Replace(".", String.Empty)) - Convert.ToInt32(RecentVersion.Replace(".", String.Empty)));

                        if (versionDifference == 0)
                        {
                            if (ShowUserInfo)
                            {
                                MessageBox.Show(this, "The application is up to date, there is no update available. Please try again later.", "No update available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else if (versionDifference < 0)
                        {
                            if (ShowUserInfo)
                            {
                                MessageBox.Show(this, "The application is more recent than the official release, there is no update available. Please try again later.", "No update available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            label1.Text = "You are using version V" + RecentVersion;
                            label2.Text = "There is new version V" + version + " available";
                            linkLabel1.Text = downloadUri;
                            linkLabel2.Text = changelogUri;
                            this.Show();
                            this.TopLevel = true;
                            this.TopMost = true;
                        }
                    }
                    else if (!String.IsNullOrEmpty(Error))
                    {
                        if (ShowUserInfo)
                        {
                            MessageBox.Show(this, Error, "GRAL GUI Update notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ShowUserInfo)
                {
                    MessageBox.Show(this, ex.Message, "GRAL GUI Update notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            CheckForUpdates.DoWork -= new System.ComponentModel.DoWorkEventHandler(this.LoadUpdateFileHttp);
            CheckForUpdates.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(CheckForUpdatesCompleted);
            CheckForUpdates = null;
            if (!this.Visible)
            {
                this.Close();
            }
        }

/// <summary>
/// Try to load the update file using the System.Net.Http class
/// </summary>
/// <param name="Url">The web adress containing the xml file</param>
/// <returns>The string with the xml file, a string with the error message</returns>
private void LoadUpdateFileHttp(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler();
                handler.UseProxy = true;
                handler.Proxy = null;
                //handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
#if __MonoCS__
#else
                handler.DefaultProxyCredentials = System.Net.CredentialCache.DefaultNetworkCredentials;
#endif
                handler.UseDefaultCredentials = true;
                handler.AllowAutoRedirect = true;
                handler.PreAuthenticate = true;

                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(handler))
                {
                    Uri baseUri = new Uri(Url);
                    string xmlFile = client.GetStringAsync(baseUri).Result;
                    lock (obj)
                    {
                        XMLFile = xmlFile;
                    }
                }
            }
            catch (Exception exception)
            {
                lock (obj)
                {
                    Error = exception.Message + Environment.NewLine + exception.GetType().ToString();
                }
            }
        }

        private void UpdateNotification_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Start the download of the new version
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = linkLabel1.Text, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "GRAL GUI Update notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Start to show the changelog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = linkLabel2.Text, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "GRAL GUI Update notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckForUpdates == null || CheckForUpdates.IsBusy != true)
            {
                obj = null;
                CheckForUpdates = null;
                this.Close();
            }
            else
            {
                this.Hide();
                if (CheckForUpdates != null && CheckForUpdates.IsBusy)
                {
                    CheckForUpdates.CancelAsync();
                }
            }
        }
    }
}