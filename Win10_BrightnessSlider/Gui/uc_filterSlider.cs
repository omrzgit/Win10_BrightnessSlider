using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Gui
{
    public class uc_filterSlider : ThemedUserControl
    {
        public PictureBox pictureBox1;
        public Label lbl_Name;
        public Panel pnl_color;
        public Label lbl_value;
        public ColorSlider trackBar1;

        public uc_filterSlider()
        {
            this.DoubleBuffered = true;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Size = new Size(350, 73);
            this.Margin = Padding.Empty;

            lbl_Name = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(14, 7),
                Text = "Screen Filter",
                TextAlign = ContentAlignment.TopCenter
            };

            pnl_color = new Panel
            {
                Size = new Size(14, 14),
                Location = new Point(102, 8),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Settings_json.Get().ScreenFilter_Color,
                Cursor = Cursors.Hand
            };
            var tt = new ToolTip();
            tt.SetToolTip(pnl_color, "Filter Color (Click to change)");
            pnl_color.Click += (s, e) =>
            {
                var st = Settings_json.Get();
                using (var cd = new ColorDialog())
                {
                    cd.Color = st.ScreenFilter_Color;
                    cd.FullOpen = true;
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        st.ScreenFilter_Color = cd.Color;
                        st.SaveTo_JsonFile();
                        pnl_color.BackColor = cd.Color;
                        ScreenFilterManager.UpdateColor(cd.Color);
                    }
                }
            };

            pictureBox1 = new PictureBox
            {
                Location = new Point(18, 30),
                Size = new Size(20, 20),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            trackBar1 = new ColorSlider
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(45, 29),
                Minimum = 0,
                Maximum = 100,
                SmallChange = 5,
                LargeChange = 0,
                ShowDivisionsText = false,
                ShowSmallScale = false,
                BorderRoundRectSize = new Size(8, 8)
            };

            lbl_value = new Label
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(300, 26),
                Text = "00%",
                TextAlign = ContentAlignment.TopCenter
            };

            this.Controls.Add(lbl_Name);
            this.Controls.Add(pnl_color);
            this.Controls.Add(pictureBox1);
            this.Controls.Add(trackBar1);
            this.Controls.Add(lbl_value);

            trackBar1.Scroll += TrackBar1_Scroll;
            trackBar1.MouseUp += TrackBar1_MouseUp;
            trackBar1.MouseWheel += TrackBar1_MouseWheel;
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
            lbl_Name.ForeColor = textColor;
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
                        path.AddArc(2, 2, 16, 16, 70, 240);
                        path.AddArc(6, 2, 13, 16, 310, -200);
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
