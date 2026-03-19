// AI Summary: 2026-03-19 - Created BuildInfo class for build number display
using System;

namespace AI.Code.Agent.AIO_MMT
{
    /// <summary>
    /// Build information display
    /// Format: YYYY-MM-DD-hh-mm-ss
    /// </summary>
    public static class BuildInfo
    {
        public static string BUILD_NUMBER => $"2026-03-19-{DateTime.Now:HH-mm-ss}";
    }
}
