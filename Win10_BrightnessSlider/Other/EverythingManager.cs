using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{
    public static class EverythingManager
    {
        public const string DefaultPath = @"C:\Program Files\Everything\Everything.exe";
        public static string DefaultDirectory => Path.GetDirectoryName(DefaultPath);

        /// <summary>
        /// Displays an OpenFileDialog on an STA thread to locate Everything.exe.
        /// </summary>
        public static string PromptForLocation()
        {
            string selectedPath = null;
            void ShowDialog()
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Select Everything.exe";
                    ofd.Filter = "Everything.exe|Everything.exe;*.exe|All Executables (*.exe)|*.exe|All Files (*.*)|*.*";
                    ofd.CheckFileExists = true;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        selectedPath = ofd.FileName;
                    }
                }
            }

            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                ShowDialog();
            }
            else
            {
                var staThread = new Thread(ShowDialog);
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();
            }

            return selectedPath;
        }

        /// <summary>
        /// Creates a file symbolic link at C:\Program Files\Everything\Everything.exe pointing to targetExePath.
        /// Requires elevation (runas) to write to Program Files.
        /// </summary>
        public static bool CreateSymbolicLink(string targetExePath, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                string targetDir = DefaultDirectory;
                // Create folder if missing, remove existing file/link if any, and create the file symbolic link
                string cmdArgs = $"/c if not exist \"{targetDir}\" mkdir \"{targetDir}\" & if exist \"{DefaultPath}\" del /f /q \"{DefaultPath}\" & mklink \"{DefaultPath}\" \"{targetExePath}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = cmdArgs,
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };

                using (var p = Process.Start(psi))
                {
                    p?.WaitForExit();
                }

                if (File.Exists(DefaultPath))
                {
                    return true;
                }

                errorMessage = "Symbolic link was not created at default path.";
                return false;
            }
            catch (Win32Exception wEx) when (wEx.NativeErrorCode == 1223)
            {
                errorMessage = "Administrator permission was cancelled by user.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Prompts the user to locate Everything.exe, updates settings, and creates an mklink at the default path.
        /// </summary>
        public static bool RelocateAndLinkEverything(bool isManualConfig = false)
        {
            if (!isManualConfig)
            {
                var promptResult = MessageBox.Show(
                    $"Everything.exe was not found at default location:\n{DefaultPath}\n\nWould you like to locate Everything.exe on your computer?\nA link (mklink) will be created at the default location so it works seamlessly.",
                    "Everything.exe Not Found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (promptResult != DialogResult.Yes)
                {
                    return false;
                }
            }

            string chosenPath = PromptForLocation();
            if (string.IsNullOrWhiteSpace(chosenPath) || !File.Exists(chosenPath))
            {
                return false;
            }

            // Persist chosen path to settings
            Settings_json.Update(s => s.Everything_ExePath = chosenPath);

            // Attempt to create symlink at default location
            bool linkCreated = CreateSymbolicLink(chosenPath, out string linkError);

            if (linkCreated)
            {
                if (isManualConfig)
                {
                    MessageBox.Show(
                        $"Successfully created link:\n{DefaultPath} -> {chosenPath}",
                        "Everything Location Configured",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                return true;
            }
            else
            {
                MessageBox.Show(
                    $"Could not create symbolic link ({linkError}).\n\nThe application will run Everything.exe directly from:\n{chosenPath}",
                    "Notice",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return true;
            }
        }

        /// <summary>
        /// Resolves the executable path to launch, prompting the user if not currently found.
        /// </summary>
        public static string ResolveExecutablePath()
        {
            if (File.Exists(DefaultPath))
            {
                return DefaultPath;
            }

            var st = Settings_json.Get();
            if (!string.IsNullOrWhiteSpace(st.Everything_ExePath) &&
                File.Exists(st.Everything_ExePath) &&
                !string.Equals(st.Everything_ExePath, DefaultPath, StringComparison.OrdinalIgnoreCase))
            {
                return st.Everything_ExePath;
            }

            bool configured = RelocateAndLinkEverything(isManualConfig: false);
            if (configured)
            {
                if (File.Exists(DefaultPath))
                {
                    return DefaultPath;
                }

                var updated = Settings_json.Get();
                if (File.Exists(updated.Everything_ExePath))
                {
                    return updated.Everything_ExePath;
                }
            }

            return null;
        }
    }
}
