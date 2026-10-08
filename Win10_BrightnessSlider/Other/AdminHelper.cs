using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{
    public static class AdminHelper
    {
        /// <summary>
        /// Checks if the current application process has Administrator privileges.
        /// </summary>
        public static bool IsRunningAsAdmin()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Restarts the application requesting Administrator privileges via UAC prompt (runas).
        /// If the user cancels the UAC prompt, the current instance remains running.
        /// </summary>
        public static void RestartAsAdmin()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Verb = "runas",
                    UseShellExecute = true
                };

                Process.Start(psi);
                Application.Exit();
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                // User cancelled UAC prompt; do nothing to keep the current instance running
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to restart as Administrator:\r\n\r\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
