// AI Summary: 2026-03-19 - Created SystemDPI for handling DPI scaling
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AI.Code.Agent.AIO_MMT
{
    public partial class MainWindow
    {
        private double _currentDpiScale = 1.0;

        /// <summary>
        /// Xử lý khi click nút DPI -
        /// </summary>
        private void BtnDPIMinus_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = CboDPIValue.SelectedIndex;
            if (currentIndex > 0)
            {
                CboDPIValue.SelectedIndex = currentIndex - 1;
            }
        }

        /// <summary>
        /// Xử lý khi click nút DPI +
        /// </summary>
        private void BtnDPIPlus_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = CboDPIValue.SelectedIndex;
            if (currentIndex < CboDPIValue.Items.Count - 1)
            {
                CboDPIValue.SelectedIndex = currentIndex + 1;
            }
        }

        /// <summary>
        /// Xử lý khi thay đổi DPI value
        /// </summary>
        private void CboDPIValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboDPIValue.SelectedValue is string selectedValue)
            {
                string value = selectedValue.Replace("%", "");
                if (int.TryParse(value, out int dpiPercent))
                {
                    _currentDpiScale = dpiPercent / 100.0;
                    ApplyDpiScale(_currentDpiScale);
                }
            }
        }

        /// <summary>
        /// Áp dụng DPI scale
        /// </summary>
        private void ApplyDpiScale(double scale)
        {
            var transform = new ScaleTransform(scale, scale);
            MainGrid.LayoutTransform = transform;
        }

        /// <summary>
        /// Xử lý khi window loaded
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set DPI mặc định là 100%
            CboDPIValue.SelectedIndex = 5; // 100%
            
            // Populate temp folder
            PopulateTempFolderComboBox();
            
            // Set default scale
            ApplyDpiScale(1.0);
        }

        /// <summary>
        /// Xử lý khi window size changed
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Có thể thêm logic responsive ở đây
        }

        /// <summary>
        /// Xử lý khi preview key down
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Có thể thêm shortcut keys ở đây
        }

        /// <summary>
        /// Xử lý khi preview mouse wheel
        /// </summary>
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Có thể thêm zoom bằng scroll wheel ở đây
        }
    }
}
