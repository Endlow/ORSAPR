using System;
using System.Drawing;
using System.Windows.Forms;

namespace OporaApp
{
    public class SketchForm : Form
    {
        private readonly OporeParameters _parameters;

        public SketchForm(OporeParameters parameters)
        {
            _parameters = parameters;
            Text = "Эскиз с размерами опоры";
            ClientSize = new Size(760, 560);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Paint += OnPaint;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            double L = _parameters.L, W = _parameters.W, H = _parameters.H;
            double d = _parameters.D, h = _parameters.H1, p = _parameters.P;

            float scale = Math.Min(380f / (float)H, 240f / (float)L);
            float ox = 320, oy = 480;

            var bodyPen = new Pen(Color.Black, 2);
            var groovePen = new Pen(Color.DarkRed, 2);
            var dimPen = new Pen(Color.DimGray, 1);
            var font = new Font("Times New Roman", 11);

            float bodyW = (float)(L * scale);
            float bodyH = (float)(H * scale);
            float depth = _parameters.ThroughSlot
                ? bodyW
                : (float)(p * scale) * 0.55f;
            float grooveH = (float)(h * scale);

            float bx = ox, by = oy - bodyH;

            g.FillRectangle(Brushes.LightSteelBlue, bx, by, bodyW, bodyH);
            g.DrawRectangle(bodyPen, bx, by, bodyW, bodyH);

            float gx = bx + bodyW - depth;
            float gy = by + (bodyH - grooveH) / 2;
            g.FillRectangle(Brushes.White, gx, gy, depth, grooveH);
            g.DrawLine(groovePen, gx, gy, gx + depth, gy);
            g.DrawLine(groovePen, gx + depth, gy, gx + depth, gy + grooveH);
            g.DrawLine(groovePen, gx + depth, gy + grooveH, gx, gy + grooveH);

            DrawDimension(g, dimPen, font, ox, oy + 28, ox + bodyW, oy + 28, "L = " + L.ToString("0.#"));
            DrawDimension(g, dimPen, font, ox - 30, by, ox - 30, oy, "H = " + H.ToString("0.#"));
            if (_parameters.ThroughSlot)
            {
                g.DrawString("паз сквозной (глубина = W = " + W.ToString("0.#") + " мм)",
                    font, Brushes.DarkRed, 40, 48);
            }
            else
            {
                DrawDimension(g, dimPen, font, ox, by - 22, gx, by - 22, "p = " + p.ToString("0.#"));
                g.DrawString("W = " + W.ToString("0.#"), font, Brushes.DimGray, ox + bodyW + 10, by + 8);
            }
            DrawDimension(g, dimPen, font, gx, gy - 12, gx, gy + grooveH + 12, "d = " + d.ToString("0.#"));
            g.DrawString("h = " + h.ToString("0.#"), font, Brushes.DarkRed, gx + depth + 8, gy + grooveH / 2 - 8);
            g.DrawString("M1 = " + OporeParameters.M1.ToString("0.#") + ", M2 = " + OporeParameters.M2.ToString("0.#") + ", M3 = " + OporeParameters.M3.ToString("0.#"),
                font, Brushes.DimGray, 40, 20);
        }

        private static void DrawDimension(Graphics g, Pen pen, Font font,
            float x1, float y1, float x2, float y2, string text)
        {
            g.DrawLine(pen, x1, y1, x2, y2);
            float mx = (x1 + x2) / 2, my = (y1 + y2) / 2;
            g.DrawString(text, font, Brushes.Black, mx - g.MeasureString(text, font).Width / 2,
                Math.Abs(y2 - y1) > Math.Abs(x2 - x1) ? my - 8 : y1 + (y2 > y1 ? 4 : -22));
        }
    }
}
