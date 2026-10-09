using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Gui
{
    public class uc_filterSlider : ThemedUserControl
    {
        public CheckBox chk_filter;
        public Panel pnl_color;
        public PictureBox pictureBox1;
        public ColorSlider trackBar1;
        public Label lbl_value;

        public uc_filterSlider()
        {
            this.DoubleBuffered = true;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Size = new Size(350, 73);
            this.Margin = Padding.Empty;

            var st = Settings_json.Get();

            chk_filter = new CheckBox
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(14, 6),
                Text = "Screen Filter",
                Cursor = Cursors.Hand,
                Checked = st.ScreenFilter_Active
            };

            pnl_color = new Panel
            {
                Size = new Size(15, 15),
                Location = new Point(chk_filter.Right + 8, chk_filter.Top + 2),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = st.ScreenFilter_Color,
                Cursor = Cursors.Hand
            };
            var tt = new ToolTip();
            tt.SetToolTip(pnl_color, "Filter Color (Click to change)");

            pnl_color.Click += (s, e) =>
            {
                var settings = Settings_json.Get();
                using (var cd = new ColorDialog())
                {
                    cd.Color = settings.ScreenFilter_Color;
                    cd.FullOpen = true;
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        settings.ScreenFilter_Color = cd.Color;
                        settings.SaveTo_JsonFile();
                        pnl_color.BackColor = cd.Color;
                        ScreenFilterManager.UpdateColor(cd.Color);
                    }
                }
            };

            pictureBox1 = new PictureBox
            {
                Location = new Point(18, 30),
                Size = new Size(20, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };
            tt.SetToolTip(pictureBox1, "Toggle Screen Filter");
            pictureBox1.Click += (s, e) =>
            {
                chk_filter.Checked = !chk_filter.Checked;
            };

            trackBar1 = new ColorSlider
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(45, 29),
                Size = new Size(250, 23),
                Minimum = 0,
                Maximum = 100,
                SmallChange = 5,
                LargeChange = 0,
                ShowDivisionsText = false,
                ShowSmallScale = false,
                BorderRoundRectSize = new Size(8, 8),
                Enabled = st.ScreenFilter_Active
            };

            lbl_value = new Label
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(300, 26),
                Text = st.ScreenFilter_Opacity + "%",
                TextAlign = ContentAlignment.TopCenter
            };

            this.Controls.Add(chk_filter);
            this.Controls.Add(pnl_color);
            this.Controls.Add(pictureBox1);
            this.Controls.Add(trackBar1);
            this.Controls.Add(lbl_value);

            chk_filter.CheckedChanged += (s, e) =>
            {
                var settings = Settings_json.Get();
                settings.ScreenFilter_Active = chk_filter.Checked;
                settings.SaveTo_JsonFile();

                trackBar1.Enabled = chk_filter.Checked;
                trackBar1.Invalidate();

                if (chk_filter.Checked)
                {
                    ScreenFilterManager.ApplyFilter();
                }
                else
                {
                    ScreenFilterManager.CloseAll();
                }
            };

            trackBar1.Scroll += TrackBar1_Scroll;
            trackBar1.MouseUp += TrackBar1_MouseUp;
            trackBar1.MouseWheel += TrackBar1_MouseWheel;

            this.Layout += (s, e) =>
            {
                if (chk_filter != null && pnl_color != null)
                {
                    pnl_color.Location = new Point(chk_filter.Right + 8, chk_filter.Top + 2);
                }
            };
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            int val = (int)trackBar1.Value;
            lbl_value.Text = val + "%";
            ScreenFilterManager.UpdateOpacity(val);
        }

        private void TrackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            int val = (int)trackBar1.Value;
            var st = Settings_json.Get();
            st.ScreenFilter_Opacity = val;
            st.SaveTo_JsonFile();
        }

        private void TrackBar1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!trackBar1.Enabled) return;
            int current = (int)trackBar1.Value;
            int step = e.Delta > 0 ? 5 : -5;
            int newVal = (int)MathFn.Clamp(current + step, 0, 100);
            trackBar1.Value = newVal;
            lbl_value.Text = newVal + "%";
            ScreenFilterManager.UpdateOpacity(newVal);

            var st = Settings_json.Get();
            st.ScreenFilter_Opacity = newVal;
            st.SaveTo_JsonFile();
        }

        public void SetGUIColors(Color backColor, Color textColor, Color borderColor, Settings settings_forTheme)
        {
            this.BackColor = backColor;
            this.FrameColor = borderColor;
            chk_filter.ForeColor = textColor;
            chk_filter.BackColor = backColor;
            lbl_value.ForeColor = textColor;

            trackBar1 = ColorSliderFn.setStyle_win10_trackbarColor_v2(trackBar1, settings_forTheme);
            trackBar1.BackColor = backColor;

            pictureBox1.Image?.Dispose();
            pictureBox1.Image = CreateMoonIcon(textColor);

            if (pnl_color != null)
            {
                pnl_color.BackColor = Settings_json.Get().ScreenFilter_Color;
            }
        }

        public void UpdatePreviewColor(Color color)
        {
            if (pnl_color != null)
                pnl_color.BackColor = color;
        }

        public void UpdateValue(int opacity)
        {
            int safe = (int)MathFn.Clamp(opacity, 0, 100);
            trackBar1.Value = safe;
            lbl_value.Text = safe + "%";
        }

        private static Bitmap CreateMoonIcon(Color color)
        {
            var bmp = new Bitmap(20, 20);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(color))
                {
                    using (var path = new GraphicsPath())
                    {
                        path.AddArc(2, 2, 16, 16, 75, 220);
                        path.AddArc(5, 2, 12, 16, 295, -170);
                        path.CloseFigure();
                        g.FillPath(brush, path);
                    }
                }
            }
            return bmp;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                pictureBox1.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
