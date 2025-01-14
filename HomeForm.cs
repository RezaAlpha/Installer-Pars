using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer_RezaAlpha
{
    public partial class HomeForm : Form
    {
        private StringBuilder logBuilder = new StringBuilder();

        public HomeForm()
        {
            InitializeComponent();
        }

        public string[] GetUrls()
        {
            return new string[]
            {
                "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user",
                "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.7.5/npp.8.7.5.Installer.x64.exe",
                "https://download-installer.cdn.mozilla.net/pub/firefox/releases/94.0/win64/en-US/Firefox%20Installer.exe",
                "https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7BF84A6E00-0154-3C5E-6FA7-A0A8704E317F%7D%26lang%3Den%26browser%3D4%26usagestats%3D1%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26ap%3Dx64-statsdef_1%26installdataindex%3Dempty/update2/installers/ChromeSetup.exe",
                "https://referrals.brave.com/latest/BraveBrowserSetup-BRV010.exe"
            };
        }

        public string[] GetFilePaths(string[] urls)
        {
            return urls.Select(url => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.GetFileName(new Uri(url).LocalPath))).ToArray();
        }

        public async Task DownloadAppFiles(string url, string filePath)
        {
            // Download files from the internet
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 1L;
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        long totalRead = 0;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;

                            // Update ProgressBar
                            int progress = (int)((double)totalRead / totalBytes * 100);
                            progressBar1.Value = progress;

                            // Log download progress
                            logBuilder.AppendLine($"Downloading {Path.GetFileName(filePath)}: {progress}%");
                            UpdateLog();
                        }
                    }
                }
            }
            logBuilder.AppendLine($"Downloaded {Path.GetFileName(filePath)} from {url}");
            UpdateLog();
        }

        private void QuitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public async void InstallBtn_Click(object sender, EventArgs e)
        {
            string[] urls = GetUrls();
            string[] paths = GetFilePaths(urls);

            if (VScode_App.Checked)
            {
                await DownloadAndInstall(urls[0], paths[0], "/VERYSILENT /NORESTART /MERGETASKS=\"addtopath\"");
            }
            if (Notepad_App.Checked)
            {
                await DownloadAndInstall(urls[1], paths[1], "/S");
            }
            if (Firefox_App.Checked)
            {
                await DownloadAndInstall(urls[2], paths[2], "/S");
            }
            if (Chrome_App.Checked)
            {
                await DownloadAndInstall(urls[3], paths[3], "/silent /install");
            }
            if (Brave_App.Checked)
            {
                await DownloadAndInstall(urls[4], paths[4], "/S");
            }
        }

        private async Task DownloadAndInstall(string url, string filePath, string arguments)
        {
            progressBar1.Value = 0; // Reset Progress Bar

            try
            {
                await DownloadAppFiles(url, filePath);
                logBuilder.AppendLine($"{Path.GetFileName(filePath)} Downloaded Successfully! Starting silent installation...");
                UpdateLog();

                // Silent installation
                Process process = new Process();
                process.StartInfo.FileName = filePath;
                process.StartInfo.Arguments = arguments; // Install switches
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true; // Don't show command prompt window
                process.Start();
                process.WaitForExit();

                logBuilder.AppendLine($"{Path.GetFileName(filePath)} Installation complete!");
                UpdateLog();
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"An error occurred: {ex.Message}");
                UpdateLog();
            }
        }

        private void UpdateLog()
        {
            if (logRichTextBox.InvokeRequired)
            {
                logRichTextBox.Invoke(new Action(UpdateLog));
            }
            else
            {
                logRichTextBox.Text = logBuilder.ToString();
                logRichTextBox.SelectionStart = logRichTextBox.Text.Length;
                logRichTextBox.ScrollToCaret();
            }
        }

        private void VScode_App_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
