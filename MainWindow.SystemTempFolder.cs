// AI Summary: 2026-03-19 - Updated SystemTempFolder with proper Defender exclusion management
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AI.Code.Agent.AIO_MMT
{
    public partial class MainWindow
    {
        // Temp folder selection
        private string _selectedTempDrivePath = null;
        private string _previousTempFolderPath = null; // Track previous temp folder for cleanup
        private string _systemTempFolderPath = null; // System folder path (LocalAppData) - never delete exclusion

        /// <summary>
        /// Populate the Temp folder ComboBox with all available drives (excluding CD-ROM)
        /// </summary>
        private void PopulateTempFolderComboBox()
        {
            try
            {
                if (CboTempFolder == null) return;

                CboTempFolder.Items.Clear();

                // Add default option (C:\Temp Folder)
                string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GMTPC", "GMTPC Tools");
                CboTempFolder.Items.Add(new ComboBoxItem
                {
                    Content = $"Mặc định (C:) - {defaultPath}",
                    Tag = defaultPath
                });

                // Add all drives except CD-ROM
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType != DriveType.CDRom && drive.IsReady)
                    {
                        string drivePath = Path.Combine(drive.Name.TrimEnd('\\'), "Temp Folder");
                        string displayText = $"{drive.Name} ({FormatBytes(drive.TotalFreeSpace)} free)";

                        CboTempFolder.Items.Add(new ComboBoxItem
                        {
                            Content = displayText,
                            Tag = drivePath
                        });
                    }
                }

                // Select default (first) item
                if (CboTempFolder.Items.Count > 0)
                {
                    CboTempFolder.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi tải danh sách ổ cứng: {ex.Message}", "Red");
            }
        }

        /// <summary>
        /// Handle Temp folder ComboBox selection changed
        /// Auto-create folder, delete old temp folder (except system folder)
        /// and manage Windows Defender exclusions
        /// </summary>
        private async void CboTempFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CboTempFolder.SelectedItem is ComboBoxItem selectedItem)
                {
                    string newTempPath = selectedItem.Tag as string;

                    if (!string.IsNullOrEmpty(newTempPath))
                    {
                        // Initialize system temp folder path on first run
                        if (_systemTempFolderPath == null)
                        {
                            _systemTempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GMTPC", "GMTPC Tools");

                            // Always ensure system folder has defender exclusion (never remove it)
                            await AddDefenderExclusionAsync(_systemTempFolderPath);
                        }

                        // Remove defender exclusion from previous temp folder if it's not the system folder
                        if (!string.IsNullOrEmpty(_previousTempFolderPath) &&
                            _previousTempFolderPath != newTempPath &&
                            !_previousTempFolderPath.Equals(_systemTempFolderPath, StringComparison.OrdinalIgnoreCase))
                        {
                            await RemoveDefenderExclusionAsync(_previousTempFolderPath);

                            // Delete previous temp folder if it's not the system folder
                            try
                            {
                                if (Directory.Exists(_previousTempFolderPath))
                                {
                                    Directory.Delete(_previousTempFolderPath, true);
                                    UpdateStatus($"Đã xóa folder tạm: {_previousTempFolderPath}", "Gray");
                                }
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus($"Không thể xóa folder cũ ({_previousTempFolderPath}): {ex.Message}", "Yellow");
                            }
                        }

                        // Create new temp folder
                        if (!Directory.Exists(newTempPath))
                        {
                            try
                            {
                                Directory.CreateDirectory(newTempPath);
                                UpdateStatus($"Đã tạo folder: {newTempPath}", "Green");
                            }
                            catch (Exception ex)
                            {
                                UpdateStatus($"Không thể tạo folder ({newTempPath}): {ex.Message}", "Red");
                                return;
                            }
                        }

                        // Add defender exclusion for new temp folder (skip if it's the system folder - already added)
                        if (!newTempPath.Equals(_systemTempFolderPath, StringComparison.OrdinalIgnoreCase))
                        {
                            await AddDefenderExclusionAsync(newTempPath);
                        }

                        // Update status and track previous path
                        _previousTempFolderPath = newTempPath;
                        _selectedTempDrivePath = newTempPath;
                        _downloadDirectory = newTempPath;
                        UpdateStatus($"Temp folder: {newTempPath}", "Green");
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi chọn folder: {ex.Message}", "Red");
            }
        }

        /// <summary>
        /// Get the selected temp folder path
        /// </summary>
        private string GetSelectedTempFolderPath()
        {
            if (!string.IsNullOrEmpty(_selectedTempDrivePath))
            {
                if (!Directory.Exists(_selectedTempDrivePath))
                {
                    Directory.CreateDirectory(_selectedTempDrivePath);
                }
                return _selectedTempDrivePath;
            }

            // Default to LocalAppData if nothing selected
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GMTPC", "GMTPC Tools");
        }

        // ===================== Defender Exclusion Methods =====================
        /// <summary>
        /// Add Windows Defender exclusion for a folder path
        /// Requires administrator privileges
        /// </summary>
        private async Task AddDefenderExclusionAsync(string folderPath)
        {
            try
            {
                // Check if running as administrator
                if (!IsRunningAsAdministrator())
                {
                    UpdateStatus($"Không thể thêm Defender exclusion (cần admin): {folderPath}", "Yellow");
                    return;
                }

                // Check if exclusion already exists
                if (await IsDefenderExclusionExistsAsync(folderPath))
                {
                    UpdateStatus($"Defender exclusion đã tồn tại: {folderPath}", "Gray");
                    return;
                }

                // Use PowerShell to add exclusion
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Add-MpPreference -ExclusionPath '{folderPath}' -Force\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = Process.Start(startInfo))
                {
                    await Task.Run(() =>
                    {
                        process.WaitForExit(10000); // Wait up to 10 seconds
                    });

                    if (process.ExitCode == 0)
                    {
                        UpdateStatus($"Đã thêm Defender exclusion: {folderPath}", "Green");
                    }
                    else
                    {
                        UpdateStatus($"Không thể thêm Defender exclusion (exit code: {process.ExitCode}): {folderPath}", "Yellow");
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi thêm Defender exclusion: {ex.Message}", "Red");
            }
        }

        /// <summary>
        /// Remove Windows Defender exclusion for a folder path
        /// Requires administrator privileges
        /// </summary>
        private async Task RemoveDefenderExclusionAsync(string folderPath)
        {
            try
            {
                // Check if running as administrator
                if (!IsRunningAsAdministrator())
                {
                    UpdateStatus($"Không thể xóa Defender exclusion (cần admin): {folderPath}", "Yellow");
                    return;
                }

                // Check if exclusion exists
                if (!await IsDefenderExclusionExistsAsync(folderPath))
                {
                    return; // Exclusion doesn't exist, nothing to remove
                }

                // Use PowerShell to remove exclusion
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Remove-MpPreference -ExclusionPath '{folderPath}' -Force\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = Process.Start(startInfo))
                {
                    await Task.Run(() =>
                    {
                        process.WaitForExit(10000); // Wait up to 10 seconds
                    });

                    if (process.ExitCode == 0)
                    {
                        UpdateStatus($"Đã xóa Defender exclusion: {folderPath}", "Green");
                    }
                    else
                    {
                        UpdateStatus($"Không thể xóa Defender exclusion (exit code: {process.ExitCode}): {folderPath}", "Yellow");
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi xóa Defender exclusion: {ex.Message}", "Red");
            }
        }

        /// <summary>
        /// Check if a folder path is already in Windows Defender exclusion list
        /// </summary>
        private async Task<bool> IsDefenderExclusionExistsAsync(string folderPath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-Command \"Get-MpPreference | Select-Object -ExpandProperty ExclusionPath\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    process.WaitForExit(5000);

                    if (!string.IsNullOrEmpty(output))
                    {
                        var exclusions = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var exclusion in exclusions)
                        {
                            if (exclusion.Trim().Equals(folderPath.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
            catch
            {
                return false; // Assume not exists if check fails
            }
        }

        /// <summary>
        /// Check if running as administrator
        /// </summary>
        private bool IsRunningAsAdministrator()
        {
            try
            {
                using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
                {
                    var principal = new System.Security.Principal.WindowsPrincipal(identity);
                    return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Format bytes to human readable string
        /// </summary>
        private string FormatBytes(long bytes)
        {
            if (bytes > 1024 * 1024 * 1024)
            {
                return $"{(double)bytes / (1024 * 1024 * 1024):F2} GB";
            }
            else if (bytes > 1024 * 1024)
            {
                return $"{(double)bytes / (1024 * 1024):F2} MB";
            }
            else if (bytes > 1024)
            {
                return $"{(double)bytes / 1024:F2} KB";
            }
            else
            {
                return $"{bytes} B";
            }
        }
    }
}
