// AI Summary: 2026-03-19 - Created SystemButtons for handling all button click events and checkbox hover
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AI.Code.Agent.AIO_MMT
{
    public partial class MainWindow
    {
        /// <summary>
        /// Select All Button - Check tất cả checkbox
        /// </summary>
        public void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in CheckBoxPanel.Children)
            {
                if (child is CheckBox chk)
                    chk.IsChecked = true;
            }
            UpdateInstallButtonState();
        }

        /// <summary>
        /// Select None Button - Uncheck tất cả checkbox
        /// </summary>
        public void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in CheckBoxPanel.Children)
            {
                if (child is CheckBox chk)
                    chk.IsChecked = false;
            }
            UpdateInstallButtonState();
        }

        /// <summary>
        /// Install Button - Xử lý cài đặt các phần mềm đã chọn
        /// </summary>
        public async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable Install button during installation
            InstallButton.IsEnabled = false;

            try
            {
                // Tạo download directory nếu chưa tồn tại
                if (!Directory.Exists(_downloadDirectory))
                {
                    Directory.CreateDirectory(_downloadDirectory);
                }

                UpdateStatus("Bắt đầu quá trình cài đặt...", "White");

                if (QwenAPICheckBox.IsChecked == true)
                {
                    UpdateStatus("Đang tải AI Code Agent AIO - MMT...", "White");
                    await DownloadAndInstallQwenToAPI();
                    UpdateStatus("AI Code Agent AIO - MMT đã được cài đặt.", "Green");
                }

                if (VSCodeCheckBox.IsChecked == true)
                {
                    UpdateStatus("Đang tải Visual Studio Code...", "White");
                    await DownloadAndInstallVSCode();
                    UpdateStatus("Visual Studio Code đã được cài đặt.", "Green");
                }

                if (VS2022CheckBox.IsChecked == true)
                {
                    UpdateStatus("Đang tải Visual Studio 2022...", "White");
                    await DownloadAndInstallVS2022();
                    UpdateStatus("Visual Studio 2022 đã được cài đặt.", "Green");
                }

                UpdateStatus("Tất cả cài đặt đã hoàn thành!", "Green");
                UpdateProgressAndStatus("All installations completed successfully!", 100, 0);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi: {ex.Message}", "Red");
                MessageBox.Show($"An error occurred: {ex.Message}", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                InstallButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Support Button - Mở link ủng hộ
        /// </summary>
        public void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://tinyurl.com/mmtdonate");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                MessageBox.Show("Could not open the donation link: " + noBrowser.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception other)
            {
                MessageBox.Show("An error occurred: " + other.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Checkbox MouseEnter - Hiển thị link khi hover
        /// </summary>
        public void Checkbox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                string link = null;
                switch (checkBox.Name)
                {
                    case "QwenAPICheckBox":
                        link = QWEN_TO_API_DOWNLOAD_URL;
                        break;
                    case "VSCodeCheckBox":
                        link = VSCode_DOWNLOAD_URL;
                        break;
                    case "VS2022CheckBox":
                        link = VS2022_DOWNLOAD_URL;
                        break;
                }

                if (!string.IsNullOrEmpty(link))
                {
                    checkBox.ToolTip = new ToolTip
                    {
                        Content = link,
                        Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse
                    };
                }
            }
        }

        /// <summary>
        /// Cập nhật trạng thái Install Button dựa trên checkbox được chọn
        /// </summary>
        protected void UpdateInstallButtonState()
        {
            bool anyChecked = false;
            foreach (var child in CheckBoxPanel.Children)
            {
                if (child is CheckBox chk && chk.IsChecked == true)
                {
                    anyChecked = true;
                    break;
                }
            }
            InstallButton.IsEnabled = anyChecked;
        }

        #region Download and Install Methods
        /// <summary>
        /// Tải và cài đặt Qwen To API
        /// </summary>
        private async Task DownloadAndInstallQwenToAPI()
        {
            string fileName = "Qwen.to.API.exe";
            string filePath = Path.Combine(_downloadDirectory, fileName);

            await DownloadFileWithProgress(QWEN_TO_API_DOWNLOAD_URL, filePath, "AI Code Agent AIO - MMT");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = QWEN_TO_API_INSTALL_ARGUMENTS,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process process = Process.Start(startInfo);
            await Task.Run(() => process.WaitForExit());

            // Mở các link
            OpenUrl("https://chromewebstore.google.com/detail/cookie-editor/hlkenndednhfkekhgcdicdfddnkalmdm");
            OpenUrl("https://www.morphllm.com/");
            OpenUrl("https://chat.qwen.ai/");
        }

        /// <summary>
        /// Tải và cài đặt VSCode
        /// </summary>
        private async Task DownloadAndInstallVSCode()
        {
            string fileName = "VSCodeUserSetup-x64-1.105.1.exe";
            string filePath = Path.Combine(_downloadDirectory, fileName);

            await DownloadFileWithProgress(VSCode_DOWNLOAD_URL, filePath, "Visual Studio Code");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = VSCode_INSTALL_ARGUMENTS,
                UseShellExecute = true,
            };

            Process process = Process.Start(startInfo);
            await Task.Run(() => process.WaitForExit());

            // Cài extension qua PowerShell
            try
            {
                ProcessStartInfo psStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Start-Process cmd -ArgumentList '/c', 'code --install-extension kilocode.Kilo-Code --force' -Verb RunAs\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                };

                Process.Start(psStartInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while attempting to install the VSCode extension via PowerShell: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Tải và cài đặt VS2022
        /// </summary>
        private async Task DownloadAndInstallVS2022()
        {
            string fileName = "VS2022Installer.exe";
            string filePath = Path.Combine(_downloadDirectory, fileName);

            await DownloadFileWithProgress(VS2022_DOWNLOAD_URL, filePath, "Visual Studio 2022");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process process = Process.Start(startInfo);
            await Task.Run(() => process.WaitForExit());
        }

        /// <summary>
        /// Mở URL trong trình duyệt mặc định
        /// </summary>
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while opening the link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion
    }
}
