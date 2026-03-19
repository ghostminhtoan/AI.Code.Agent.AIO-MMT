// AI Summary: 2026-03-19 - Updated SystemBar with proper status methods and FormatSpeed
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AI.Code.Agent.AIO_MMT
{
    public partial class MainWindow
    {
        private bool _isInstalling = false;
        private string _installationStatus = "";

        /// <summary>
        /// Cập nhật trạng thái trên StatusBar
        /// </summary>
        protected void UpdateStatus(string message, string color)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (_isInstalling)
                {
                    _installationStatus = message;
                    if (ProgressTextBlock != null)
                    {
                        ProgressTextBlock.Text = message;
                        ProgressTextBlock.Foreground = GetBrush(color);
                    }
                }
                else
                {
                    if (ProgressTextBlock != null)
                    {
                        ProgressTextBlock.Text = message;
                        ProgressTextBlock.Foreground = GetBrush(color);
                    }
                }
            });
        }

        /// <summary>
        /// Cập nhật Secondary Status
        /// </summary>
        private void UpdateSecondaryStatus(string message, string color = "Gray")
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (SecondaryProgressTextBlock != null)
                {
                    SecondaryProgressTextBlock.Text = message;
                    SecondaryProgressTextBlock.Foreground = GetBrush(color);
                }

                if (!_isInstalling && ProgressTextBlock != null)
                {
                    ProgressTextBlock.Text = message;
                    ProgressTextBlock.Foreground = GetBrush(color);
                }
            });
        }

        /// <summary>
        /// Set installing state
        /// </summary>
        private void SetInstallingState(bool isInstalling)
        {
            _isInstalling = isInstalling;
            if (!isInstalling)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (SecondaryProgressTextBlock != null)
                        SecondaryProgressTextBlock.Text = "";
                });
            }
        }

        /// <summary>
        /// Cập nhật ProgressBar
        /// </summary>
        protected void UpdateProgress(double value, string statusMessage = "")
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (DownloadProgressBar != null)
                    DownloadProgressBar.Value = value;
                if (ProgressTextBlock != null && !string.IsNullOrEmpty(statusMessage))
                    ProgressTextBlock.Text = statusMessage;
            });
        }

        /// <summary>
        /// Chuyển đổi tên màu thành Brush object
        /// </summary>
        private Brush GetBrush(string colorName)
        {
            switch (colorName.ToLower())
            {
                case "green":
                    return Brushes.LimeGreen;
                case "red":
                    return Brushes.Red;
                case "orange":
                    return Brushes.Orange;
                case "yellow":
                    return Brushes.Yellow;
                case "cyan":
                    return Brushes.Cyan;
                case "gray":
                    return Brushes.Gray;
                case "white":
                default:
                    return Brushes.White;
            }
        }

        /// <summary>
        /// Format speed to human readable string
        /// </summary>
        private string FormatSpeed(double bytesPerSecond)
        {
            if (bytesPerSecond > 1024 * 1024)
            {
                return $"{bytesPerSecond / (1024 * 1024):F2} MB/s";
            }
            else if (bytesPerSecond > 1024)
            {
                return $"{bytesPerSecond / 1024:F2} KB/s";
            }
            else
            {
                return $"{bytesPerSecond:F2} B/s";
            }
        }
    }
}
