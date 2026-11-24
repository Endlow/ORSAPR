using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SupportApp
{
    /// <summary>
    /// Главное окно приложения. Отвечает за компоновку, валидацию
    /// и вызов строителя модели; геометрию строит <see cref="SupportBuilder"/>.
    /// </summary>
    public class MainForm : Form
    {
        private readonly SupportParametersControl _parametersControl =
            new SupportParametersControl();

        private readonly SupportBuilder _builder = new SupportBuilder();
        private readonly Label _statusLabel = new Label();

        public MainForm()
        {
            Text = "Опора с пазом — КОМПАС-3D";
            ClientSize = new Size(560, 440);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10f);

            var parametersGroup = new GroupBox
            {
                Text = "Параметры опоры",
                Location = new Point(16, 12),
                Size = new Size(528, 270)
            };
            _parametersControl.Dock = DockStyle.Fill;
            parametersGroup.Controls.Add(_parametersControl);

            var resetButton = new Button
            {
                Text = "Сбросить",
                Location = new Point(16, 292),
                Size = new Size(120, 32)
            };
            resetButton.Click += (sender, e) => ResetParameters();

            var sketchButton = new Button
            {
                Text = "Показать эскиз",
                Location = new Point(144, 292),
                Size = new Size(150, 32)
            };
            sketchButton.Click += (sender, e) => ShowSketch();

            var buildButton = new Button
            {
                Text = "Построить модель",
                Location = new Point(304, 292),
                Size = new Size(240, 32),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            buildButton.Click += OnBuildModelClick;

            _statusLabel.Location = new Point(16, 336);
            _statusLabel.Size = new Size(528, 80);
            _statusLabel.ForeColor = Color.Firebrick;

            Controls.AddRange(new Control[]
            {
                parametersGroup, resetButton, sketchButton, buildButton, _statusLabel
            });

            _parametersControl.ParametersChanged += (sender, e) =>
                _statusLabel.Text = string.Empty;

            ResetParameters();
        }

        private void ResetParameters()
        {
            _parametersControl.SetDefault();
            _statusLabel.Text = string.Empty;
        }

        private void ShowSketch()
        {
            using (var sketch = new SketchForm(_parametersControl.GetParameters()))
            {
                sketch.ShowDialog(this);
            }
        }

        /// <summary>
        /// Обработчик кнопки «Построить модель»: читает параметры,
        /// проверяет их и только при отсутствии ошибок вызывает строителя.
        /// </summary>
        private void OnBuildModelClick(object sender, EventArgs e)
        {
            _parametersControl.ClearErrors();

            SupportParameters parameters = _parametersControl.GetParameters();
            if (ValidateParameters(parameters).Count > 0)
            {
                return;
            }

            BuildModel(parameters);
        }

        /// <summary>
        /// Проверяет параметры и при наличии ошибок отображает их на контроле
        /// и в едином окне ошибки. Возвращает список найденных ошибок.
        /// </summary>
        private IReadOnlyList<ValidationError> ValidateParameters(SupportParameters parameters)
        {
            var errors = parameters.Validate();

            if (errors.Count > 0)
            {
                _parametersControl.ApplyErrors(errors);
                ShowError("Проверьте параметры:\n\n" +
                    string.Join("\n\n", errors.Select(x => x.Message)));
                _statusLabel.Text = "Проверьте значения параметров.";
            }

            return errors;
        }

        private void BuildModel(SupportParameters parameters)
        {
            _statusLabel.Text = "Запуск КОМПАС-3D и построение модели…";
            Cursor = Cursors.WaitCursor;

            try
            {
                string directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ОРСАПР");
                Directory.CreateDirectory(directory);

                string kind = parameters.ThroughSlot ? "сквозной" : "глухой";
                string path = Path.Combine(directory,
                    $"Опора_{parameters.SupportLength:0}x" +
                    $"{parameters.SupportWidth:0}x" +
                    $"{parameters.SupportHeight:0}_{kind}.m3d");

                _builder.Build(parameters, path);
                _statusLabel.Text = "Модель построена: " + path;

                MessageBox.Show(this,
                    "Модель опоры с пазом построена и сохранена:\n" + path,
                    "Построение завершено",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError("Не удалось построить модель:\n" + ex.Message);
                _statusLabel.Text = "Ошибка: " + ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Единое окно ошибки для валидации и для ошибок построения.
        /// Используется одним и тем же способом во всех сценариях.
        /// </summary>
        private void ShowError(string message)
        {
            MessageBox.Show(this, message, "Ошибка построения",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _builder.Dispose();
            base.OnFormClosed(e);
        }
    }
}
