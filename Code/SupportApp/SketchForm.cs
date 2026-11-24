using System;
using System.Drawing;
using System.Windows.Forms;

namespace SupportApp
{
    public class SketchForm : Form
    {
        private readonly SupportParameters _parameters;

        public SketchForm(SupportParameters parameters)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

            Text = "Эскиз с размерами опоры";
            ClientSize = new Size(720, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Paint += OnPaint;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            double length = _parameters.SupportLength;
            double width = _parameters.SupportWidth;
            double height = _parameters.SupportHeight;
            double grooveLength = _parameters.GrooveLength;
            double grooveHeight = _parameters.GrooveHeight;
            double grooveDepth = _parameters.GrooveDepth;

            float scale = Math.Min(380f / (float)height, 240f / (float)length);
            float ox = 320;
            float oy = 480;

            using (var bodyPen = new Pen(Color.Black, 2))
            using (var groovePen = new Pen(Color.DarkRed, 2))
            using (var dimPen = new Pen(Color.DimGray, 1))
            using (var font = new Font("Times New Roman", 11))
            {
                float bodyWidth = (float)(length * scale);
                float bodyHeight = (float)(height * scale);
                float depth = _parameters.ThroughSlot
                    ? bodyWidth
                    : (float)(grooveDepth * scale) * 0.55f;
                float grooveH = (float)(grooveHeight * scale);

                float bx = ox;
                float by = oy - bodyHeight;

                g.FillRectangle(Brushes.LightSteelBlue, bx, by, bodyWidth, bodyHeight);
                g.DrawRectangle(bodyPen, bx, by, bodyWidth, bodyHeight);

                float gx = bx + bodyWidth - depth;
                float gy = by + (bodyHeight - grooveH) / 2;
                g.FillRectangle(Brushes.White, gx, gy, depth, grooveH);
                g.DrawLine(groovePen, gx, gy, gx + depth, gy);
                g.DrawLine(groovePen, gx + depth, gy, gx + depth, gy + grooveH);
                g.DrawLine(groovePen, gx + depth, gy + grooveH, gx, gy + grooveH);

                DrawDimension(g, dimPen, font, ox, oy + 28, ox + bodyWidth, oy + 28,
                    "L = " + length.ToString("0.#"));
                DrawDimension(g, dimPen, font, ox - 30, by, ox - 30, oy,
                    "H = " + height.ToString("0.#"));

                if (_parameters.ThroughSlot)
                {
                    g.DrawString("паз сквозной (глубина = W = " +
                        width.ToString("0.#") + " мм)",
                        font, Brushes.DarkRed, 40, 48);
                }
                else
                {
                    DrawDimension(g, dimPen, font, ox, by - 22, gx, by - 22,
                        "p = " + grooveDepth.ToString("0.#"));
                    g.DrawString("W = " + width.ToString("0.#"),
                        font, Brushes.DimGray, ox + bodyWidth + 10, by + 8);
                }

                DrawDimension(g, dimPen, font, gx, gy - 12, gx, gy + grooveH + 12,
                    "d = " + grooveLength.ToString("0.#"));
                g.DrawString("h = " + grooveHeight.ToString("0.#"),
                    font, Brushes.DarkRed, gx + depth + 8, gy + grooveH / 2 - 8);
                g.DrawString("M1 = " + SupportParameters.GrooveEndMargin.ToString("0.#") +
                    ", M2 = " + SupportParameters.GrooveVerticalMargin.ToString("0.#") +
                    ", M3 = " + SupportParameters.GrooveBackMargin.ToString("0.#"),
                    font, Brushes.DimGray, 40, 20);
            }
        }

        private static void DrawDimension(Graphics g, Pen pen, Font font,
            float x1, float y1, float x2, float y2, string text)
        {
            g.DrawLine(pen, x1, y1, x2, y2);
            float mx = (x1 + x2) / 2;
            float my = (y1 + y2) / 2;
            g.DrawString(text, font, Brushes.Black,
                mx - g.MeasureString(text, font).Width / 2,
                Math.Abs(y2 - y1) > Math.Abs(x2 - x1) ? my - 8 : y1 + (y2 > y1 ? 4 : -22));
        }
    }
}
