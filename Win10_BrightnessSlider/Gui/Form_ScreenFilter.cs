using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Win10_BrightnessSlider.Gui
{
    public class Form_ScreenFilter : Form
    {
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int MA_NOACTIVATE = 3;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST;
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEACTIVATE)
            {
                m.Result = (IntPtr)MA_NOACTIVATE;
                return;
            }
            base.WndProc(ref m);
        }

        public Form_ScreenFilter(Screen screen, Color color, double opacity)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = screen.Bounds.Location;
            this.Size = screen.Bounds.Size;
            this.TopMost = true;
            this.BackColor = color;
            this.Opacity = opacity;
        }

        protected override bool ShowWithoutActivation => true;
    }

    public static class ScreenFilterManager
    {
        private static readonly List<Form_ScreenFilter> _overlays = new List<Form_ScreenFilter>();
        private static bool _wiredDisplayChange = false;

        public static void Initialize()
        {
            if (!_wiredDisplayChange)
            {
                SystemEvents.DisplaySettingsChanged += (s, e) =>
                {
                    try
                    {
                        var st = Settings_json.Get();
                        if (st.ScreenFilter_Enabled)
                        {
                            ApplyFilter();
                        }
                    }
                    catch (Exception ex)
                    {
                        RamLogger.Log("DisplaySettingsChanged ScreenFilter error: " + ex);
                    }
                };
                Application.ApplicationExit += (s, e) => CloseAll();
                _wiredDisplayChange = true;
            }

            try
            {
                var settings = Settings_json.Get();
                if (settings.ScreenFilter_Enabled)
                {
                    ApplyFilter();
                }
            }
            catch (Exception ex)
            {
                RamLogger.Log("Initialize ScreenFilter error: " + ex);
            }
        }

        public static void ApplyFilter()
        {
            var st = Settings_json.Get();
            bool shouldApply = st.ScreenFilter_Enabled && st.ScreenFilter_Active;
            ApplyFilter(shouldApply, st.ScreenFilter_Opacity, st.ScreenFilter_Color);
        }

        public static void ApplyFilter(bool enabled, int opacityPercent, Color color)
        {
            CloseAll();
            if (!enabled) return;

            const double maxOpacity = 0.85;
            double safeOpacity = Math.Max(0.0, Math.Min(maxOpacity, (opacityPercent / 100.0) * maxOpacity));

            foreach (var screen in Screen.AllScreens)
            {
                try
                {
                    var overlay = new Form_ScreenFilter(screen, color, safeOpacity);
                    overlay.Show();
                    _overlays.Add(overlay);
                }
                catch (Exception ex)
                {
                    RamLogger.Log("ScreenFilterManager error on screen " + screen.DeviceName + ": " + ex);
                }
            }
        }

        public static void UpdateOpacity(int opacityPercent)
        {
            const double maxOpacity = 0.85;
            double safeOpacity = Math.Max(0.0, Math.Min(maxOpacity, (opacityPercent / 100.0) * maxOpacity));

            foreach (var overlay in _overlays)
            {
                try
                {
                    if (overlay != null && !overlay.IsDisposed)
                    {
                        overlay.Opacity = safeOpacity;
                    }
                }
                catch { }
            }
        }

        public static void UpdateColor(Color color)
        {
            foreach (var overlay in _overlays)
            {
                try
                {
                    if (overlay != null && !overlay.IsDisposed)
                    {
                        overlay.BackColor = color;
                    }
                }
                catch { }
            }
        }

        public static void CloseAll()
        {
            foreach (var overlay in _overlays)
            {
                try
                {
                    overlay?.Close();
                    overlay?.Dispose();
                }
                catch { }
            }
            _overlays.Clear();
        }
    }
}
