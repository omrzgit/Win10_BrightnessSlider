using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{
    public static class BrightnessHotkeyManager
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID_UP = 9101;
        private const int HOTKEY_ID_DOWN = 9102;

        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private class HotkeyWindow : NativeWindow, IDisposable
        {
            public event Action<int> HotkeyPressed;

            public HotkeyWindow()
            {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    HotkeyPressed?.Invoke(m.WParam.ToInt32());
                }
                base.WndProc(ref m);
            }

            public void Dispose()
            {
                DestroyHandle();
            }
        }

        private static HotkeyWindow _window;
        private static bool _isRegisteredUp = false;
        private static bool _isRegisteredDown = false;
        private static string _lastError = null;

        public static bool IsRegisteredUp => _isRegisteredUp;
        public static bool IsRegisteredDown => _isRegisteredDown;
        public static string LastError => _lastError;

        public static void Initialize()
        {
            if (_window == null)
            {
                _window = new HotkeyWindow();
                _window.HotkeyPressed += OnHotkeyPressed;
            }
            RegisterHotkeys();
        }

        public static void RegisterHotkeys()
        {
            UnregisterHotkeys();

            var st = Settings_json.Get();
            if (!st.Hotkey_Brightness_Enabled)
                return;

            if (_window == null)
            {
                _window = new HotkeyWindow();
                _window.HotkeyPressed += OnHotkeyPressed;
            }

            _lastError = null;

            // Register Brightness Up
            if (TryParseHotkey(st.Hotkey_BrightnessUp, out uint modUp, out Keys keyUp))
            {
                _isRegisteredUp = RegisterHotKey(_window.Handle, HOTKEY_ID_UP, modUp, (uint)keyUp);
                if (!_isRegisteredUp)
                {
                    _lastError = "Failed to register shortcut for Brightness Up (" + st.Hotkey_BrightnessUp + "). It may be used by another application.";
                }
            }

            // Register Brightness Down
            if (TryParseHotkey(st.Hotkey_BrightnessDown, out uint modDown, out Keys keyDown))
            {
                _isRegisteredDown = RegisterHotKey(_window.Handle, HOTKEY_ID_DOWN, modDown, (uint)keyDown);
                if (!_isRegisteredDown)
                {
                    string downErr = "Failed to register shortcut for Brightness Down (" + st.Hotkey_BrightnessDown + "). It may be used by another application.";
                    _lastError = _lastError == null ? downErr : _lastError + "\n" + downErr;
                }
            }
        }

        public static void UnregisterHotkeys()
        {
            if (_window != null && _window.Handle != IntPtr.Zero)
            {
                if (_isRegisteredUp)
                {
                    UnregisterHotKey(_window.Handle, HOTKEY_ID_UP);
                    _isRegisteredUp = false;
                }
                if (_isRegisteredDown)
                {
                    UnregisterHotKey(_window.Handle, HOTKEY_ID_DOWN);
                    _isRegisteredDown = false;
                }
            }
        }

        public static void Shutdown()
        {
            UnregisterHotkeys();
            if (_window != null)
            {
                _window.Dispose();
                _window = null;
            }
        }

        private static void OnHotkeyPressed(int id)
        {
            var form = Form1.Instance;
            if (form == null || form.IsDisposed)
                return;

            var st = Settings_json.Get();
            int step = Math.Max(1, Math.Min(50, st.Hotkey_BrightnessStep));

            if (id == HOTKEY_ID_UP)
            {
                if (form.InvokeRequired)
                {
                    form.BeginInvoke((Action)(() => form.AdjustBrightnessByStep(step)));
                }
                else
                {
                    form.AdjustBrightnessByStep(step);
                }
            }
            else if (id == HOTKEY_ID_DOWN)
            {
                if (form.InvokeRequired)
                {
                    form.BeginInvoke((Action)(() => form.AdjustBrightnessByStep(-step)));
                }
                else
                {
                    form.AdjustBrightnessByStep(-step);
                }
            }
        }

        public static bool TryParseHotkey(string input, out uint modifiers, out Keys key)
        {
            modifiers = 0;
            key = Keys.None;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            var parts = input.Split(new[] { '+', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var clean = part.Trim();
                if (clean.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                    clean.Equals("Control", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= MOD_CONTROL;
                }
                else if (clean.Equals("Alt", StringComparison.OrdinalIgnoreCase) ||
                         clean.Equals("Menu", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= MOD_ALT;
                }
                else if (clean.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= MOD_SHIFT;
                }
                else if (clean.Equals("Win", StringComparison.OrdinalIgnoreCase) ||
                         clean.Equals("Windows", StringComparison.OrdinalIgnoreCase) ||
                         clean.Equals("LWin", StringComparison.OrdinalIgnoreCase) ||
                         clean.Equals("RWin", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= MOD_WIN;
                }
                else
                {
                    if (clean.Equals("Up", StringComparison.OrdinalIgnoreCase)) key = Keys.Up;
                    else if (clean.Equals("Down", StringComparison.OrdinalIgnoreCase)) key = Keys.Down;
                    else if (clean.Equals("Left", StringComparison.OrdinalIgnoreCase)) key = Keys.Left;
                    else if (clean.Equals("Right", StringComparison.OrdinalIgnoreCase)) key = Keys.Right;
                    else if (clean.Equals("PageUp", StringComparison.OrdinalIgnoreCase) || clean.Equals("PgUp", StringComparison.OrdinalIgnoreCase)) key = Keys.PageUp;
                    else if (clean.Equals("PageDown", StringComparison.OrdinalIgnoreCase) || clean.Equals("PgDn", StringComparison.OrdinalIgnoreCase)) key = Keys.PageDown;
                    else if (clean.Equals("Plus", StringComparison.OrdinalIgnoreCase) || clean.Equals("Add", StringComparison.OrdinalIgnoreCase)) key = Keys.Add;
                    else if (clean.Equals("Minus", StringComparison.OrdinalIgnoreCase) || clean.Equals("Subtract", StringComparison.OrdinalIgnoreCase)) key = Keys.Subtract;
                    else if (Enum.TryParse<Keys>(clean, true, out Keys parsedKey))
                    {
                        key = parsedKey;
                    }
                }
            }

            return key != Keys.None;
        }

        public static string FormatHotkey(uint modifiers, Keys key)
        {
            var list = new List<string>();
            if ((modifiers & MOD_CONTROL) != 0) list.Add("Ctrl");
            if ((modifiers & MOD_ALT) != 0) list.Add("Alt");
            if ((modifiers & MOD_SHIFT) != 0) list.Add("Shift");
            if ((modifiers & MOD_WIN) != 0) list.Add("Win");
            list.Add(key.ToString());
            return string.Join(" + ", list);
        }

        public static string NormalizeDisplay(string input)
        {
            if (TryParseHotkey(input, out uint mod, out Keys key))
            {
                return FormatHotkey(mod, key);
            }
            return input;
        }
    }
}
