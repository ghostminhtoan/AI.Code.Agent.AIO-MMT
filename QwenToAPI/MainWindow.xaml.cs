using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QwenToAPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string downloadDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "InstallerDownloads");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Duyệt qua tất cả các checkbox trong WrapPanel và đánh dấu là checked
            foreach (var child in CheckBoxPanel.Children)
            {
                if (child is CheckBox chk) chk.IsChecked = true;
            }
        }

        private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            // Duyệt qua tất cả các checkbox trong WrapPanel và bỏ đánh dấu
            foreach (var child in CheckBoxPanel.Children)
            {
                if (child is CheckBox chk) chk.IsChecked = false;
            }
        }

        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://tinyurl.com/mmtdonate");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                // Handle exception if no browser is found
                MessageBox.Show("Could not open the donation link: " + noBrowser.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception other)
            {
                // Handle other exceptions
                MessageBox.Show("An error occurred: " + other.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable Install button during installation
            InstallButton.IsEnabled = false;

            try
            {
                // Create download directory if it doesn't exist
                if (!Directory.Exists(downloadDirectory))
                {
                    Directory.CreateDirectory(downloadDirectory);
                }

                int totalTasks = (QwenAPICheckBox.IsChecked == true ? 1 : 0) +
                                (VSCodeCheckBox.IsChecked == true ? 1 : 0) +
                                (VS2022CheckBox.IsChecked == true ? 1 : 0);
                int completedTasks = 0;

                if (QwenAPICheckBox.IsChecked == true)
                {
                    UpdateProgressAndStatus("Starting QwenToAPI download...", 0, 0);
                    await DownloadAndInstallQwenToAPI(); // Downloads, installs, then opens link
                    completedTasks++;
                    UpdateProgressAndStatus("QwenToAPI download and install completed.", 100, 0);
                }

                if (VSCodeCheckBox.IsChecked == true)
                {
                    UpdateProgressAndStatus("Starting Visual Studio Code download...", 0, 0);
                    await DownloadAndInstallVSCode(); // Downloads, installs VSCode, then installs extension via PowerShell
                    completedTasks++;
                    UpdateProgressAndStatus("Visual Studio Code and extension installation completed.", 100, 0);
                }

                if (VS2022CheckBox.IsChecked == true)
                {
                    UpdateProgressAndStatus("Starting Visual Studio 2022 download...", 0, 0);
                    await DownloadAndInstallVS2022();
                    completedTasks++;
                    UpdateProgressAndStatus("Visual Studio 2022 installation completed.", 100, 0);
                }

                UpdateProgressAndStatus("All selected installations completed successfully!", 100, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                InstallButton.IsEnabled = true;
            }
        }

        private void UpdateProgressAndStatus(string status, double progress, double speed)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DownloadProgressBar.Value = progress;
                ProgressTextBlock.Text = $"{progress:F1}%";
                SpeedTextBlock.Text = $"{speed:F1} KB/s";
            }));
        }

        private async Task DownloadAndInstallQwenToAPI()
        {
            string downloadUrl = "https://github.com/ghostminhtoan/MMT/releases/download/Qwen_to_API/Qwen.to.API.exe";
            string fileName = "Qwen.to.API.exe";
            string filePath = System.IO.Path.Combine(downloadDirectory, fileName);

            // Tải file với tiến độ
            await DownloadFileWithProgress(downloadUrl, filePath, "QwenToAPI");

            // Cài đặt với tham số /s
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = "/s",
                UseShellExecute = true,
                Verb = "runas" // Yêu cầu quyền admin nếu cần
            };

            Process process = Process.Start(startInfo);
            await Task.Run(() => process.WaitForExit());

            // Mở liên kết
            string url = "https://chromewebstore.google.com/detail/cookie-editor/hlkenndednhfkekhgcdicdfddnkalmdm";
            try
            {
                Process.Start(url);
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                MessageBox.Show("Could not open the link: " + noBrowser.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("An error occurred while opening the link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Mở thêm 2 liên kết mới
            string url2 = "https://www.morphllm.com/";
            try
            {
                Process.Start(url2);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("An error occurred while opening the second link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            string url3 = "https://chat.qwen.ai/";
            try
            {
                Process.Start(url3);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("An error occurred while opening the third link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task DownloadAndInstallVSCode()
        {
            string downloadUrl = "https://vscode.download.prss.microsoft.com/dbazure/download/stable/7d842fb85a0275a4a8e4d7e040d2625abbf7f084/VSCodeUserSetup-x64-1.105.1.exe";
            string fileName = "VSCodeUserSetup-x64-1.105.1.exe";
            string filePath = System.IO.Path.Combine(downloadDirectory, fileName);

            // Tải file với tiến độ
            await DownloadFileWithProgress(downloadUrl, filePath, "Visual Studio Code");

            // Run installer with /silent flag
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = "/silent",
                UseShellExecute = true,
            };

            Process process = Process.Start(startInfo);
            await Task.Run(() => process.WaitForExit());

            // Sau khi cài đặt xong, dùng PowerShell để cài extension với --force
            try
            {
                ProcessStartInfo psStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Start-Process cmd -ArgumentList '/c', 'code --install-extension kilocode.Kilo-Code --force' -Verb RunAs\"",
                    UseShellExecute = false, // Important: UseShellExecute=false when redirecting output or using -Command
                    CreateNoWindow = true, // Optional: hides the PowerShell window
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) // Ensure a valid working directory
                };

                Process.Start(psStartInfo); // Start the process
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("An error occurred while attempting to install the VSCode extension via PowerShell: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task DownloadAndInstallVS2022()
        {
            string downloadUrl = "https://c2rsetup.officeapps.live.com/c2r/downloadVS.aspx?sku=community&channel=Release&version=VS2022&source=VSLandingPage&cid=2030:7fd6333476b84b559fcb67bb623abf8b";
            string fileName = "VS2022Installer.exe";
            string filePath = System.IO.Path.Combine(downloadDirectory, fileName);

            // Tải file với tiến độ
            await DownloadFileWithProgress(downloadUrl, filePath, "Visual Studio 2022");

            // Run installer
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                Verb = "runas" // Request administrator privileges
            };

            Process process = Process.Start(startInfo);
            await Task.Run(() => process.WaitForExit());
        }

        private async Task DownloadFileWithProgress(string downloadUrl, string filePath, string fileName)
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
                            var progressText = canReportProgress ? 
                                $"{fileName} download progress: {totalBytesRead / 1024} KB / {(totalBytes / 1024)} KB" : 
                                $"{fileName} download progress: {totalBytesRead / 1024} KB";

                            UpdateProgressAndStatus(progressText, progress, speed);

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
    }
}
