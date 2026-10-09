using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Gui
{
    public class Form_HotkeyConfig : Form
    {
        private TextBox _txtUp;
        private TextBox _txtDown;
        private ComboBox _cmbStep;
        private CheckBox _chkAllScreens;
        private CheckBox _chkEnabled;
        private Label _lblStatus;

        public Form_HotkeyConfig()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Configure Brightness Hotkeys";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(420, 390);
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);

            var titleLabel = new Label
            {
                Text = "Global Brightness Hotkeys",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            var subtitleLabel = new Label
            {
                Text = "Press keys in the boxes below to set shortcuts, or choose a preset.",
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(20, 42),
                Size = new Size(380, 20)
            };
            this.Controls.Add(subtitleLabel);

            // Enabled CheckBox
            _chkEnabled = new CheckBox
            {
                Text = "Enable Global Hotkeys",
                ForeColor = Color.White,
                Location = new Point(20, 70),
                AutoSize = true,
                Checked = true
            };
            this.Controls.Add(_chkEnabled);

            // Hotkey UP
            var lblUp = new Label
            {
                Text = "Increase Brightness Shortcut:",
                ForeColor = Color.White,
                Location = new Point(20, 105),
                AutoSize = true
            };
            this.Controls.Add(lblUp);

            _txtUp = new TextBox
            {
                Location = new Point(20, 128),
                Size = new Size(380, 26),
                BackColor = Color.FromArgb(48, 48, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ReadOnly = true
            };
            SetupHotkeyCapture(_txtUp);
            this.Controls.Add(_txtUp);

            // Hotkey DOWN
            var lblDown = new Label
            {
                Text = "Decrease Brightness Shortcut:",
                ForeColor = Color.White,
                Location = new Point(20, 165),
                AutoSize = true
            };
            this.Controls.Add(lblDown);

            _txtDown = new TextBox
            {
                Location = new Point(20, 188),
                Size = new Size(380, 26),
                BackColor = Color.FromArgb(48, 48, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ReadOnly = true
            };
            SetupHotkeyCapture(_txtDown);
            this.Controls.Add(_txtDown);

            // Presets
            var lblPresets = new Label
            {
                Text = "Presets:",
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(20, 226),
                AutoSize = true
            };
            this.Controls.Add(lblPresets);

            var pnlPresets = new FlowLayoutPanel
            {
                Location = new Point(75, 222),
                Size = new Size(330, 32),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            AddPresetButton(pnlPresets, "Ctrl+Alt", "Control+Alt+Up", "Control+Alt+Down");
            AddPresetButton(pnlPresets, "Win+Alt", "Win+Alt+Up", "Win+Alt+Down");
            AddPresetButton(pnlPresets, "Ctrl+Shift", "Control+Shift+Up", "Control+Shift+Down");
            AddPresetButton(pnlPresets, "Alt+PgUp/Dn", "Alt+PageUp", "Alt+PageDown");
            this.Controls.Add(pnlPresets);

            // Step size
            var lblStep = new Label
            {
                Text = "Step Size:",
                ForeColor = Color.White,
                Location = new Point(20, 266),
                AutoSize = true
            };
            this.Controls.Add(lblStep);

            _cmbStep = new ComboBox
            {
                Location = new Point(90, 263),
                Size = new Size(80, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(48, 48, 48),
                ForeColor = Color.White
            };
            _cmbStep.Items.AddRange(new object[] { "1%", "2%", "5%", "10%", "15%", "20%" });
            _cmbStep.SelectedIndex = 2; // 5%
            this.Controls.Add(_cmbStep);

            // All screens checkbox
            _chkAllScreens = new CheckBox
            {
                Text = "Hotkeys Change All Monitors (instead of monitor under cursor)",
                ForeColor = Color.White,
                Location = new Point(20, 300),
                AutoSize = true
            };
            this.Controls.Add(_chkAllScreens);

            // Status label
            _lblStatus = new Label
            {
                ForeColor = Color.FromArgb(255, 128, 128),
                Location = new Point(20, 326),
                Size = new Size(380, 20),
                Text = ""
            };
            this.Controls.Add(_lblStatus);

            // Buttons
            var btnSave = new Button
            {
                Text = "Save & Apply",
                DialogResult = DialogResult.OK,
                Location = new Point(200, 350),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(310, 350),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void AddPresetButton(FlowLayoutPanel panel, string text, string up, string down)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 5, 0)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btn.Click += (s, e) =>
            {
                _txtUp.Text = BrightnessHotkeyManager.NormalizeDisplay(up);
                _txtDown.Text = BrightnessHotkeyManager.NormalizeDisplay(down);
                _txtUp.Tag = up;
                _txtDown.Tag = down;
            };
            panel.Controls.Add(btn);
        }

        private void SetupHotkeyCapture(TextBox txt)
        {
            txt.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;

                if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
                    return;

                if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey ||
                    e.KeyCode == Keys.Menu || e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
                    return;

                var parts = new List<string>();
                if (e.Control) parts.Add("Ctrl");
                if (e.Alt) parts.Add("Alt");
                if (e.Shift) parts.Add("Shift");

                // Win key check via async state
                bool winDown = (Win32GetAsyncKeyState(0x5B) < 0) || (Win32GetAsyncKeyState(0x5C) < 0);
                if (winDown && !parts.Contains("Win")) parts.Add("Win");

                parts.Add(e.KeyCode.ToString());
                string combo = string.Join(" + ", parts);

                txt.Text = combo;
                txt.Tag = combo;
            };
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetAsyncKeyState")]
        private static extern short Win32GetAsyncKeyState(int vKey);

        private void LoadCurrentSettings()
        {
            var st = Settings_json.Get();
            _chkEnabled.Checked = st.Hotkey_Brightness_Enabled;
            _txtUp.Text = BrightnessHotkeyManager.NormalizeDisplay(st.Hotkey_BrightnessUp);
            _txtUp.Tag = st.Hotkey_BrightnessUp;
            _txtDown.Text = BrightnessHotkeyManager.NormalizeDisplay(st.Hotkey_BrightnessDown);
            _txtDown.Tag = st.Hotkey_BrightnessDown;
            _chkAllScreens.Checked = st.Hotkey_ChangesAllScreens;

            int step = st.Hotkey_BrightnessStep;
            string stepStr = step + "%";
            int idx = _cmbStep.FindStringExact(stepStr);
            if (idx >= 0) _cmbStep.SelectedIndex = idx;
            else _cmbStep.SelectedItem = "5%";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string upStr = _txtUp.Text;
            string downStr = _txtDown.Text;

            if (!BrightnessHotkeyManager.TryParseHotkey(upStr, out _, out _))
            {
                _lblStatus.Text = "Invalid shortcut for Brightness Up.";
                this.DialogResult = DialogResult.None;
                return;
            }

            if (!BrightnessHotkeyManager.TryParseHotkey(downStr, out _, out _))
            {
                _lblStatus.Text = "Invalid shortcut for Brightness Down.";
                this.DialogResult = DialogResult.None;
                return;
            }

            int step = 5;
            if (_cmbStep.SelectedItem != null)
            {
                string sel = _cmbStep.SelectedItem.ToString().Replace("%", "");
                int.TryParse(sel, out step);
            }

            Settings_json.Update(st =>
            {
                st.Hotkey_Brightness_Enabled = _chkEnabled.Checked;
                st.Hotkey_BrightnessUp = upStr;
                st.Hotkey_BrightnessDown = downStr;
                st.Hotkey_BrightnessStep = Math.Max(1, step);
                st.Hotkey_ChangesAllScreens = _chkAllScreens.Checked;
            });

            BrightnessHotkeyManager.RegisterHotkeys();

            if (BrightnessHotkeyManager.LastError != null)
            {
                MessageBox.Show(BrightnessHotkeyManager.LastError, "Hotkey Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.Close();
        }
    }
}
