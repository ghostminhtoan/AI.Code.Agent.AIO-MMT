// AI Summary: 2026-03-19 - Created SystemTempFolder for managing temp folder selection and Windows Defender exclusions
using System;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AI.Code.Agent.AIO_MMT
{
    public partial class MainWindow
    {
        private string _selectedTempDrivePath;
        private string _previousTempFolderPath;
        private readonly string _systemTempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GMTPC", "GMTPC Tools");

        /// <summary>
        /// Điền danh sách ổ cứng vào ComboBox
        /// </summary>
        private void PopulateTempFolderComboBox()
        {
            CboTempFolder.Items.Clear();
            
            // Thêm system folder mặc định
            CboTempFolder.Items.Add(new ComboBoxItem { Content = _systemTempFolderPath, Tag = "system" });
            
            // Thêm các ổ cứng
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed && drive.IsReady)
                {
                    string tempFolderPath = Path.Combine(drive.Name, "Temp Folder");
                    CboTempFolder.Items.Add(new ComboBoxItem { Content = tempFolderPath, Tag = "drive" });
                }
            }
            
            // Chọn mặc định là system folder
            CboTempFolder.SelectedIndex = 0;
        }

        /// <summary>
        /// Xử lý khi thay đổi ổ cứng
        /// </summary>
        private void CboTempFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboTempFolder.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedPath = selectedItem.Content.ToString();
                string tag = selectedItem.Tag as string;
                
                // Xóa folder cũ nếu không phải system folder
                if (!string.IsNullOrEmpty(_previousTempFolderPath) && 
                    _previousTempFolderPath != _systemTempFolderPath && 
                    _previousTempFolderPath != selectedPath)
                {
                    try
                    {
                        if (Directory.Exists(_previousTempFolderPath))
                        {
                            Directory.Delete(_previousTempFolderPath, true);
                        }
                        RemoveDefenderExclusionAsync(_previousTempFolderPath).Wait();
                    }
                    catch { /* Bỏ qua lỗi khi xóa */ }
                }
                
                // Tạo folder mới
                if (!Directory.Exists(selectedPath))
                {
                    try
                    {
                        Directory.CreateDirectory(selectedPath);
                        AddDefenderExclusionAsync(selectedPath).Wait();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Không thể tạo folder: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                
                _selectedTempDrivePath = selectedPath;
                _previousTempFolderPath = selectedPath;
                _downloadDirectory = selectedPath;
            }
        }

        /// <summary>
        /// Lấy path của temp folder đang chọn
        /// </summary>
        private string GetSelectedTempFolderPath()
        {
            return _selectedTempDrivePath ?? _systemTempFolderPath;
        }

        /// <summary>
        /// Thêm Windows Defender exclusion
        /// </summary>
        private async Task AddDefenderExclusionAsync(string path)
        {
            if (!IsRunningAsAdministrator())
                return;
                
            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Defender", "SELECT * FROM MSFT_MpPreference"))
                {
                    var inSettings = searcher.Get();
                    foreach (var setting in inSettings)
                    {
                        var exclusionPaths = setting["ExclusionPath"] as string[];
                        if (exclusionPaths == null || !Array.Exists(exclusionPaths, item => item == path))
                        {
                            using (var inClass = new ManagementClass("root\\Microsoft\\Windows\\Defender", "MSFT_MpPreference", null))
                            {
                                using (var inParams = inClass.GetMethodParameters("Add"))
                                {
                                    inParams["Path"] = new string[] { path };
                                    var outParams = await Task.Run(() => inClass.InvokeMethod("Add", inParams, null));
                                }
                            }
                        }
                        break;
                    }
                }
            }
            catch { /* Bỏ qua lỗi nếu không có quyền */ }
        }

        /// <summary>
        /// Xóa Windows Defender exclusion
        /// </summary>
        private async Task RemoveDefenderExclusionAsync(string path)
        {
            if (!IsRunningAsAdministrator())
                return;
                
            try
            {
                using (var inClass = new ManagementClass("root\\Microsoft\\Windows\\Defender", "MSFT_MpPreference", null))
                {
                    using (var inParams = inClass.GetMethodParameters("Remove"))
                    {
                        inParams["Path"] = new string[] { path };
                        await Task.Run(() => inClass.InvokeMethod("Remove", inParams, null));
                    }
                }
            }
            catch { /* Bỏ qua lỗi nếu không có quyền */ }
        }

        /// <summary>
        /// Kiểm tra xem ứng dụng đang chạy với quyền admin không
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
    }
}
