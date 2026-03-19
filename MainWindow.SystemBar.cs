// AI Summary: 2026-03-19 - Created SystemBar for status and progress bar management
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AI.Code.Agent.AIO_MMT
{
    public partial class MainWindow
    {
        /// <summary>
        /// Cập nhật trạng thái trên StatusBar
        /// </summary>
        protected void UpdateStatus(string message, string colorName = "White")
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (StatusBarTextBlock != null)
                {
                    StatusBarTextBlock.Text = message;
                    StatusBarTextBlock.Foreground = new SolidColorBrush(GetColorFromName(colorName));
                }
            }));
        }

        /// <summary>
        /// Cập nhật ProgressBar
        /// </summary>
        protected void UpdateProgress(double value, string statusMessage = "")
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DownloadProgressBar != null)
                    DownloadProgressBar.Value = value;
                if (ProgressTextBlock != null && !string.IsNullOrEmpty(statusMessage))
                    ProgressTextBlock.Text = statusMessage;
            }));
        }

        /// <summary>
        /// Chuyển đổi tên màu thành Color object
        /// </summary>
        private Color GetColorFromName(string colorName)
        {
            switch (colorName.ToLower())
            {
                case "green":
                    return Colors.LimeGreen;
                case "red":
                    return Colors.Red;
                case "orange":
                    return Colors.Orange;
                case "yellow":
                    return Colors.Yellow;
                case "white":
                default:
                    return Colors.White;
            }
        }
    }
}
