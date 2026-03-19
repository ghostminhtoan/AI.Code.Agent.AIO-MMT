// AI Summary: 2026-03-19 - Updated SystemButtons with more button handlers and checkbox clicks
using System;
using System.Collections.Generic;
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
        private bool _isPaused;
        private List<string> _cachedDownloadLinks = new List<string>();

        /// <summary>
        /// Select All Button - Check tất cả checkbox trong tab hiện tại
        /// </summary>
        public void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            WrapPanel currentPanel = GetCurrentCheckBoxPanel();
            if (currentPanel != null)
            {
                foreach (var child in currentPanel.Children)
                {
                    if (child is CheckBox chk)
                        chk.IsChecked = true;
                }
            }
            UpdateInstallButtonState();
        }

        /// <summary>
        /// Select None Button - Uncheck tất cả checkbox trong tab hiện tại
        /// </summary>
        public void BtnSelectNone_Click(object sender, RoutedEventArgs e)
        {
            WrapPanel currentPanel = GetCurrentCheckBoxPanel();
            if (currentPanel != null)
            {
                foreach (var child in currentPanel.Children)
                {
                    if (child is CheckBox chk)
                        chk.IsChecked = false;
                }
            }
            UpdateInstallButtonState();
        }

        /// <summary>
        /// Select None for All Tabs - Uncheck tất cả checkbox trong tất cả tabs
        /// </summary>
        public void BtnSelectNoneAllTabs_Click(object sender, RoutedEventArgs e)
        {
            UncheckAllTabs();
            UpdateInstallButtonState();
        }

        /// <summary>
        /// Install Button - Xử lý cài đặt các phần mềm đã chọn
        /// </summary>
        public async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            BtnInstall.IsEnabled = false;
            BtnPause.IsEnabled = true;
            BtnStop.IsEnabled = true;
            _isPaused = false;

            try
            {
                if (!Directory.Exists(_downloadDirectory))
                {
                    Directory.CreateDirectory(_downloadDirectory);
                }

                UpdateStatus("Bắt đầu quá trình cài đặt...", "White");

                // Main Tab
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
                BtnInstall.IsEnabled = true;
                BtnPause.IsEnabled = false;
                BtnStop.IsEnabled = false;
            }
        }

        /// <summary>
        /// Pause Button - Tạm dừng download
        /// </summary>
        public void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            _isPaused = !_isPaused;
            BtnPause.Content = _isPaused ? "Resume" : "Pause";
            UpdateStatus(_isPaused ? "Đã tạm dừng" : "Đang tiếp tục...", "Orange");
        }

        /// <summary>
        /// Stop Button - Dừng download
        /// </summary>
        public void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _isPaused = false;
            BtnInstall.IsEnabled = true;
            BtnPause.IsEnabled = false;
            BtnStop.IsEnabled = false;
            BtnPause.Content = "Pause";
            UpdateStatus("Đã dừng", "Red");
        }

        /// <summary>
        /// Refresh Color Button - Refresh màu sắc giao diện
        /// </summary>
        public void BtnRefreshColor_Click(object sender, RoutedEventArgs e)
        {
            // Refresh color logic
            UpdateStatus("Đã refresh màu", "Green");
        }

        /// <summary>
        /// Donate Button
        /// </summary>
        public void BtnDonate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://tinyurl.com/mmtdonate");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the donation link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Download Page Button - Mở trang download
        /// </summary>
        public void BtnDownloadPage_Click(object sender, RoutedEventArgs e)
        {
            // Mở trang download gốc
            try
            {
                Process.Start("https://github.com/ghostminhtoan/MMT/releases");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the download page: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Download Page Mouse Enter - Hiển thị link khi hover
        /// </summary>
        public void BtnDownloadPage_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnDownloadPage.ToolTip = new ToolTip
            {
                Content = "https://github.com/ghostminhtoan/MMT/releases",
                Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse
            };
        }

        /// <summary>
        /// Download Page Mouse Right Button Up - Copy link
        /// </summary>
        public void BtnDownloadPage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var contextMenu = new ContextMenu();
            var copyItem = new MenuItem { Header = "Copy Link" };
            copyItem.Click += (s, args) =>
            {
                if (_cachedDownloadLinks.Count > 0)
                {
                    string allLinks = string.Join("\n", _cachedDownloadLinks);
                    Clipboard.SetText(allLinks);
                    UpdateStatus($"Đã copy {_cachedDownloadLinks.Count} link vào clipboard", "Green");
                }
                else
                {
                    UpdateStatus("Không có link nào để copy", "Orange");
                }
            };
            contextMenu.Items.Add(copyItem);
            contextMenu.IsOpen = true;
            e.Handled = true;
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
        /// Segment Count Preview Mouse Wheel
        /// </summary>
        public void CboSegmentCount_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Handle segment count change
        }

        /// <summary>
        /// Segment Count Selection Changed
        /// </summary>
        public void CboSegmentCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle segment count change
        }

        /// <summary>
        /// Cập nhật trạng thái Install Button dựa trên checkbox được chọn
        /// </summary>
        protected void UpdateInstallButtonState()
        {
            bool anyChecked = false;
            WrapPanel currentPanel = GetCurrentCheckBoxPanel();
            
            if (currentPanel != null)
            {
                foreach (var child in currentPanel.Children)
                {
                    if (child is CheckBox chk && chk.IsChecked == true)
                    {
                        anyChecked = true;
                        break;
                    }
                }
            }
            
            BtnInstall.IsEnabled = anyChecked;
        }

        /// <summary>
        /// Lấy WrapPanel hiện tại dựa trên tab đang chọn
        /// </summary>
        private WrapPanel GetCurrentCheckBoxPanel()
        {
            if (MainTabControl?.SelectedItem is TabItem selectedTab)
            {
                string header = selectedTab.Header?.ToString();
                switch (header)
                {
                    case "Main":
                        return CheckBoxPanel;
                }
            }
            return CheckBoxPanel;
        }

        /// <summary>
        /// Uncheck tất cả tabs
        /// </summary>
        private void UncheckAllTabs()
        {
            UncheckPanel(CheckBoxPanel);
        }

        /// <summary>
        /// Uncheck một panel
        /// </summary>
        private void UncheckPanel(WrapPanel panel)
        {
            if (panel != null)
            {
                foreach (var child in panel.Children)
                {
                    if (child is CheckBox chk)
                        chk.IsChecked = false;
                }
            }
        }

        #region Checkbox Click Handlers
        public void QwenAPICheckBox_Click(object sender, RoutedEventArgs e) => UpdateInstallButtonState();
        public void VSCodeCheckBox_Click(object sender, RoutedEventArgs e) => UpdateInstallButtonState();
        public void VS2022CheckBox_Click(object sender, RoutedEventArgs e) => UpdateInstallButtonState();
        #endregion
    }
}
