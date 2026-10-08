using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{
    public static class TaskSchedulerHelper
    {
        public const string TaskName = "Win10_BrightnessSlider_Admin";

        /// <summary>
        /// Checks if the elevated startup task is currently registered in Windows Task Scheduler.
        /// </summary>
        public static bool IsAdminStartupEnabled()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/query /tn \"{TaskName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var proc = Process.Start(psi))
                {
                    proc?.WaitForExit();
                    return proc?.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Registers or removes the elevated Task Scheduler task for auto-starting at user logon.
        /// </summary>
        public static bool SetAdminStartup(bool enable)
        {
            try
            {
                string arguments;
                if (enable)
                {
                    string exePath = Application.ExecutablePath;
                    arguments = $"/create /tn \"{TaskName}\" /tr \"\\\"{exePath}\\\"\" /sc onlogon /rl highest /f";
                }
                else
                {
                    arguments = $"/delete /tn \"{TaskName}\" /f";
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = arguments,
                    Verb = AdminHelper.IsRunningAsAdmin() ? "" : "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    proc?.WaitForExit();
                    return proc?.ExitCode == 0;
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                // User cancelled UAC prompt
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to configure scheduled task:\r\n\r\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
