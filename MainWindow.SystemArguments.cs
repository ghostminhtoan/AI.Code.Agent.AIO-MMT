// AI Summary: 2026-03-19 - Created SystemArguments with download URLs and install arguments constants
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace AI.Code.Agent.AIO_MMT
{
    public partial class MainWindow
    {
        // ==================== DOWNLOAD URL CONSTANTS ====================
        private const string QWEN_TO_API_DOWNLOAD_URL = "https://github.com/ghostminhtoan/MMT/releases/download/Qwen_to_API/Qwen.to.API.exe";
        private const string VSCode_DOWNLOAD_URL = "https://vscode.download.prss.microsoft.com/dbazure/download/stable/7d842fb85a0275a4a8e4d7e040d2625abbf7f084/VSCodeUserSetup-x64-1.105.1.exe";
        private const string VS2022_DOWNLOAD_URL = "https://c2rsetup.officeapps.live.com/c2r/downloadVS.aspx?sku=community&channel=Release&version=VS2022&source=VSLandingPage&cid=2030:7fd6333476b84b559fcb67bb623abf8b";

        // ==================== INSTALL ARGUMENTS CONSTANTS ====================
        private const string QWEN_TO_API_INSTALL_ARGUMENTS = "/s";
        private const string VSCode_INSTALL_ARGUMENTS = "/silent";
        private const string VS2022_INSTALL_ARGUMENTS = ""; // VS2022 installer không cần argument

        // ==================== TEMP FOLDER ====================
        private string _downloadDirectory = Path.Combine(Path.GetTempPath(), "InstallerDownloads");

        /// <summary>
        /// Tải file với tiến độ hiển thị trên ProgressBar
        /// </summary>
        protected async Task DownloadFileWithProgress(string downloadUrl, string filePath, string fileName)
        {
            using (HttpClient client = new HttpClient())
            {
                // Lấy thông tin file để biết kích thước
                var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    var totalBytesRead = 0L;
                    var startTime = DateTime.Now;
                    var lastUpdateTime = DateTime.Now;
                    var lastBytesRead = 0L;

                    while (true)
                    {
                        var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        // Cập nhật tiến độ và tốc độ sau mỗi khoảng thời gian
                        var currentTime = DateTime.Now;
                        if (currentTime - lastUpdateTime >= TimeSpan.FromMilliseconds(500)) // Cập nhật mỗi 500ms
                        {
                            var bytesSinceLastUpdate = totalBytesRead - lastBytesRead;
                            var timeSinceLastUpdate = (currentTime - lastUpdateTime).TotalSeconds;
                            var speed = timeSinceLastUpdate > 0 ? bytesSinceLastUpdate / timeSinceLastUpdate / 1024 : 0; // KB/s

                            var progress = canReportProgress ? (double)totalBytesRead / totalBytes * 100 : 0;
                            UpdateProgressAndStatus($"{fileName} download progress: {progress:F1}%", progress, speed);

                            lastBytesRead = totalBytesRead;
                            lastUpdateTime = currentTime;
                        }
                    }

                    // Cập nhật hoàn tất
                    var totalTime = (DateTime.Now - startTime).TotalSeconds;
                    var avgSpeed = totalTime > 0 ? totalBytesRead / totalTime / 1024 : 0; // KB/s
                    UpdateProgressAndStatus($"{fileName} download completed.", 100, avgSpeed);
                }
            }
        }

        /// <summary>
        /// Cập nhật ProgressBar và status text
        /// </summary>
        protected void UpdateProgressAndStatus(string status, double progress, double speed)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DownloadProgressBar != null)
                    DownloadProgressBar.Value = progress;
                if (ProgressTextBlock != null)
                    ProgressTextBlock.Text = $"{progress:F1}%";
                if (SpeedTextBlock != null)
                    SpeedTextBlock.Text = $"{speed:F1} KB/s";
            }));
        }

        #region Download and Install Methods
        /// <summary>
        /// Tải và cài đặt Qwen To API
        /// </summary>
        protected async Task DownloadAndInstallQwenToAPI()
        {
            string fileName = "Qwen.to.API.exe";
            string filePath = Path.Combine(_downloadDirectory, fileName);

            await DownloadFileWithProgress(QWEN_TO_API_DOWNLOAD_URL, filePath, "AI Code Agent AIO - MMT");

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                Arguments = QWEN_TO_API_INSTALL_ARGUMENTS,
                UseShellExecute = true,
                Verb = "runas"
            };

            var process = System.Diagnostics.Process.Start(startInfo);
            await Task.Run(() => process.WaitForExit());

            // Mở các link
            OpenUrl("https://chromewebstore.google.com/detail/cookie-editor/hlkenndednhfkekhgcdicdfddnkalmdm");
            OpenUrl("https://www.morphllm.com/");
            OpenUrl("https://chat.qwen.ai/");
        }

        /// <summary>
        /// Tải và cài đặt VSCode
        /// </summary>
        protected async Task DownloadAndInstallVSCode()
        {
            string fileName = "VSCodeUserSetup-x64-1.105.1.exe";
            string filePath = Path.Combine(_downloadDirectory, fileName);

            await DownloadFileWithProgress(VSCode_DOWNLOAD_URL, filePath, "Visual Studio Code");

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                Arguments = VSCode_INSTALL_ARGUMENTS,
                UseShellExecute = true,
            };

            var process = System.Diagnostics.Process.Start(startInfo);
            await Task.Run(() => process.WaitForExit());

            // Cài extension qua PowerShell
            try
            {
                var psStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Start-Process cmd -ArgumentList '/c', 'code --install-extension kilocode.Kilo-Code --force' -Verb RunAs\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                };

                System.Diagnostics.Process.Start(psStartInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while attempting to install the VSCode extension via PowerShell: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Tải và cài đặt VS2022
        /// </summary>
        protected async Task DownloadAndInstallVS2022()
        {
            string fileName = "VS2022Installer.exe";
            string filePath = Path.Combine(_downloadDirectory, fileName);

            await DownloadFileWithProgress(VS2022_DOWNLOAD_URL, filePath, "Visual Studio 2022");

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            var process = System.Diagnostics.Process.Start(startInfo);
            await Task.Run(() => process.WaitForExit());
        }

        /// <summary>
        /// Mở URL trong trình duyệt mặc định
        /// </summary>
        protected void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while opening the link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion
    }
}
