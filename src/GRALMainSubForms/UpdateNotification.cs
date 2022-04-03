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
                
        public UpdateNotification()
        {
            this.Hide();
            InitializeComponent();
        }

        /// <summary>
        /// Load the update file from GitHub and compare the recent version with the version in the update file
        /// </summary>
        public void LoadUpdateFile()
        {
            string uri = "https://github.com/GralDispersionModel/GUI/releases/download/V22.03/AutoUpdater.xml";

            (string XMLFile, string Error) = LoadUpdateFileHttp(uri);

            string Error2 = String.Empty;
            if (!String.IsNullOrEmpty(Error))
            {
                (XMLFile, Error2) = LoadUpdateFileWeb(uri);
            }

            try
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
                else
                {
                    if (String.IsNullOrEmpty(Error))
                    {
                        if (String.IsNullOrEmpty(Error2))
                        {
                            Error2 = "Update notification not available";
                        }
                        if (ShowUserInfo)
                        {
                            MessageBox.Show(this, Error2, "Update notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        if (ShowUserInfo)
                        {
                            MessageBox.Show(this, Error, "Update notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                if (ShowUserInfo)
                {
                    MessageBox.Show(this, ex.Message, "Update notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Try to load the update file using the System.Net.Http class
        /// </summary>
        /// <param name="Url">The web adress containing the xml file</param>
        /// <returns>The string with the xml file, a string with the error message</returns>
        private (string, string) LoadUpdateFileHttp(string Url)
        {
            string error = string.Empty;
            string XMLFile = string.Empty;

            try
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    Uri baseUri = new Uri(Url);

                    using (var response = client.GetAsync(baseUri).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string temp = response.Content.ReadAsStringAsync().Result;
                            XMLFile = temp;
                        }
                        else
                        {
                            error = "Error" + response.ReasonPhrase;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                error = exception.Message + Environment.NewLine + exception.GetType().ToString();
            }
            return (XMLFile, error);
        }

        /// <summary>
        /// Use the old System.Net.WebClient class -> on some machines it is possible using the default proxy settings using this class
        /// </summary>
        /// <param name="Url">The web adress containing the xml file</param>
        /// <returns>The string with the xml file, a string with the error message</returns>
        private (string, string) LoadUpdateFileWeb(string Url)
        {
            string error = string.Empty;
            string XMLFile = string.Empty;
            try
            {
                System.Net.WebRequest.DefaultWebProxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    Uri baseUri = new Uri(Url);
                    string xml_File = client.DownloadString(baseUri);
                    XMLFile = xml_File;
                }
            }
            catch (Exception exception)
            {
                error = exception.Message + Environment.NewLine + exception.GetType().ToString();
            }
            return (XMLFile, error);
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
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Update notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Update notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
