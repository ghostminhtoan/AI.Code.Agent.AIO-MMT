// AI Summary: 2026-03-19 - Created SystemInstallDefault mechanism for basic installation with download and silent arguments
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AI.Code.Agent.AIO_MMT
{
    public partial class MainWindow
    {
        /// <summary>
        /// Cơ chế cài đặt cơ bản - tải file và chạy với argument
        /// Sử dụng cho các checkbox cài đặt phần mềm thông thường
        /// </summary>
        protected async Task InstallWithDefaultAsync(string downloadUrl, string filePath, string installArguments, string displayName)
        {
            // Tải file với tiến độ
            await DownloadFileWithProgress(downloadUrl, filePath, displayName);

            // Cài đặt với tham số
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = installArguments,
                UseShellExecute = true,
                Verb = "runas" // Yêu cầu quyền admin nếu cần
            };

            Process process = Process.Start(startInfo);
            await Task.Run(() => process.WaitForExit());
        }

        /// <summary>
        /// Cơ chế cài đặt cơ bản với retry logic (max 3 lần)
        /// </summary>
        protected async Task InstallWithDefaultAndRetryAsync(string downloadUrl, string filePath, string installArguments, string displayName, int maxRetries = 3)
        {
            int retryCount = 0;
            Exception lastException = null;

            while (retryCount < maxRetries)
            {
                try
                {
                    await InstallWithDefaultAsync(downloadUrl, filePath, installArguments, displayName);
                    return; // Thành công thì thoát
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    retryCount++;
                    if (retryCount < maxRetries)
                    {
                        UpdateStatus($"Lỗi: {ex.Message}. Thử lại lần {retryCount}/{maxRetries}...", "Orange");
                        await Task.Delay(1000); // Đợi 1 giây trước khi retry
                    }
                }
            }

            throw lastException;
        }
    }
}
