using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace OporaApp
{
    public class MainForm : Form
    {
        private readonly NumericUpDown _lField = CreateField();
        private readonly NumericUpDown _wField = CreateField();
        private readonly NumericUpDown _hField = CreateField();
        private readonly NumericUpDown _dField = CreateField();
        private readonly NumericUpDown _h1Field = CreateField();
        private readonly NumericUpDown _pField = CreateField();

        private readonly CheckBox _throughSlot = new CheckBox();
        private readonly Label _status = new Label();
        private readonly ToolTip _toolTip = new ToolTip();

        public MainForm()
        {
            Text = "Опора с пазом — КОМПАС-3D";
            ClientSize = new Size(460, 690);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10f);

            AddParameter("Длина опоры L, мм (60–150)", _lField, 20, 0);
            AddParameter("Ширина опоры W, мм (50–100)", _wField, 20, 50);
            AddParameter("Высота опоры H, мм (100–300)", _hField, 20, 100);
            AddParameter("Длина паза d, мм (20–100)", _dField, 20, 150);
            AddParameter("Высота паза h, мм (20–80)", _h1Field, 20, 200);
            AddParameter("Глубина паза p, мм (20–80)", _pField, 20, 250);

            _throughSlot.Text = "Сквозной паз (вырез на всю ширину W)";
            _throughSlot.Location = new Point(20, 304);
            _throughSlot.Size = new Size(412, 24);
            _throughSlot.CheckedChanged += (sender, e) => _pField.Enabled = !_throughSlot.Checked;

            var resetButton = new Button
            {
                Text = "Сбросить до значений по умолчанию",
                Location = new Point(20, 334),
                Size = new Size(200, 32)
            };
            resetButton.Click += (sender, e) => LoadDefaults();

            var sketchButton = new Button
            {
                Text = "Показать эскиз с размерами",
                Location = new Point(232, 334),
                Size = new Size(200, 32)
            };
            sketchButton.Click += (sender, e) => new SketchForm(ReadParameters()).ShowDialog(this);

            var buildButton = new Button
            {
                Text = "Спроектировать",
                Location = new Point(20, 380),
                Size = new Size(412, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            buildButton.Click += OnBuild;

            _status.Location = new Point(20, 428);
            _status.Size = new Size(412, 60);
            _status.ForeColor = Color.Firebrick;

            Controls.AddRange(new Control[]
            {
                _lField, _wField, _hField, _dField, _h1Field, _pField,
                _throughSlot, resetButton, sketchButton, buildButton, _status
            });

            LoadDefaults();
        }

        private static NumericUpDown CreateField()
        {
            return new NumericUpDown
            {
                Size = new Size(120, 27),
                DecimalPlaces = 1,
                Minimum = -10000,
                Maximum = 10000
            };
        }

        private void AddParameter(string label, NumericUpDown field, int x, int y)
        {
            var caption = new Label
            {
                Text = label,
                Location = new Point(x, y),
                Size = new Size(410, 24)
            };
            field.Location = new Point(x, y + 24);
            Controls.Add(caption);
            Controls.Add(field);
        }

        private OporeParameters ReadParameters()
        {
            return new OporeParameters
            {
                L = (double)_lField.Value,
                W = (double)_wField.Value,
                H = (double)_hField.Value,
                D = (double)_dField.Value,
                H1 = (double)_h1Field.Value,
                P = (double)_pField.Value,
                ThroughSlot = _throughSlot.Checked
            };
        }

        private void LoadDefaults()
        {
            var p = OporeParameters.Default();
            _lField.Value = (decimal)p.L;
            _wField.Value = (decimal)p.W;
            _hField.Value = (decimal)p.H;
            _dField.Value = (decimal)p.D;
            _h1Field.Value = (decimal)p.H1;
            _pField.Value = (decimal)p.P;
            _throughSlot.Checked = false;
            _pField.Enabled = true;
            _status.Text = string.Empty;
            ClearHighlights();
        }

        private void ClearHighlights()
        {
            foreach (var field in new[] { _lField, _wField, _hField, _dField, _h1Field, _pField })
            {
                field.BackColor = SystemColors.Window;
                _toolTip.SetToolTip(field, string.Empty);
            }
        }

        private void OnBuild(object sender, EventArgs e)
        {
            ClearHighlights();
            var parameters = ReadParameters();
            var errors = parameters.Validate();

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    var field = FieldByName(error.FieldName);
                    if (field != null)
                    {
                        field.BackColor = Color.MistyRose;
                        _toolTip.SetToolTip(field, error.Message);
                    }
                }

                _status.Text = "Проверьте параметры: " + errors[0].Message;
                MessageBox.Show(this,
                    string.Join(Environment.NewLine + Environment.NewLine,
                        errors.Select(x => x.Message)),
                    "Ошибка валидации параметров",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _status.Text = "Запуск КОМПАС-3D и построение модели…";
            Cursor = Cursors.WaitCursor;

            try
            {
                var wrapper = new Kompas3DWrapper();
                var builder = new Builder(wrapper);
                string kind = parameters.ThroughSlot ? "сквозной" : "глухой";
                string directory = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ОРСАПР");
                System.IO.Directory.CreateDirectory(directory);
                string path = builder.Build(parameters,
                    System.IO.Path.Combine(directory,
                        $"Опора_{parameters.L:0}x{parameters.W:0}x{parameters.H:0}_{kind}.m3d"));
                _status.Text = "Модель построена: " + path;
                MessageBox.Show(this,
                    "Модель опоры с пазом построена и сохранена:\n" + path,
                    "Построение завершено",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _status.Text = "Ошибка: " + ex.Message;
                MessageBox.Show(this,
                    "Не удалось построить модель:\n" + ex.Message,
                    "Ошибка построения",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private NumericUpDown FieldByName(string fieldName)
        {
            if (fieldName == "L") return _lField;
            if (fieldName == "W") return _wField;
            if (fieldName == "H") return _hField;
            if (fieldName == "D") return _dField;
            if (fieldName == "H1") return _h1Field;
            if (fieldName == "P") return _pField;
            return null;
        }
    }
}
