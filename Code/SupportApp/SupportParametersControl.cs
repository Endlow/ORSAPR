using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SupportApp
{
    /// <summary>
    /// Пользовательский контрол ввода параметров опоры. Хранит коллекцию
    /// полей ввода, привязанных к <see cref="SupportParameter"/>, и умеет
    /// подсвечивать поле с ошибкой по идентификатору параметра.
    /// </summary>
    public class SupportParametersControl : UserControl
    {
        private readonly Dictionary<SupportParameter, NumericUpDown> _fields =
            new Dictionary<SupportParameter, NumericUpDown>();

        private readonly ToolTip _toolTip = new ToolTip();
        private readonly CheckBox _throughSlot = new CheckBox();

        public event EventHandler ParametersChanged;

        public SupportParametersControl()
        {
            _toolTip.IsBalloon = true;
            _toolTip.ToolTipIcon = ToolTipIcon.Warning;
            _toolTip.ToolTipTitle = "Ошибка валидации";

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                AutoSize = true,
                Padding = new Padding(8, 2, 8, 2)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (int i = 0; i < 7; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            AddParameter(layout, SupportParameter.SupportLength,
                "Длина опоры L, мм (60–150)", 0);
            AddParameter(layout, SupportParameter.SupportWidth,
                "Ширина опоры W, мм (50–100)", 1);
            AddParameter(layout, SupportParameter.SupportHeight,
                "Высота опоры H, мм (100–300)", 2);
            AddParameter(layout, SupportParameter.GrooveLength,
                "Длина паза d, мм (20–100)", 3);
            AddParameter(layout, SupportParameter.GrooveHeight,
                "Высота паза h, мм (20–80)", 4);
            AddParameter(layout, SupportParameter.GrooveDepth,
                "Глубина паза p, мм (20–80)", 5);

            _throughSlot.Text = "Сквозной паз (вырез на всю ширину W)";
            _throughSlot.AutoSize = true;
            _throughSlot.Margin = new Padding(2, 8, 2, 2);
            _throughSlot.CheckedChanged += (sender, e) =>
            {
                _fields[SupportParameter.GrooveDepth].Enabled = !_throughSlot.Checked;
                if (_throughSlot.Checked)
                {
                    ClearError(SupportParameter.GrooveDepth);
                }

                ParametersChanged?.Invoke(this, EventArgs.Empty);
            };

            layout.Controls.Add(_throughSlot, 0, 6);
            layout.SetColumnSpan(_throughSlot, 2);
            Controls.Add(layout);
        }

        private void AddParameter(TableLayoutPanel layout, SupportParameter parameter,
            string labelText, int row)
        {
            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(2, 10, 2, 2)
            };

            var field = new NumericUpDown
            {
                Width = 120,
                DecimalPlaces = 1,
                Minimum = -10000,
                Maximum = 10000,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(2, 6, 2, 2)
            };

            field.ValueChanged += (sender, e) =>
            {
                ClearError(parameter);
                ParametersChanged?.Invoke(this, EventArgs.Empty);
            };

            _fields.Add(parameter, field);
            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(field, 1, row);
        }

        /// <summary>Читает значения из полей ввода и формирует модель параметров.</summary>
        public SupportParameters GetParameters()
        {
            return new SupportParameters
            {
                SupportLength = (double)_fields[SupportParameter.SupportLength].Value,
                SupportWidth = (double)_fields[SupportParameter.SupportWidth].Value,
                SupportHeight = (double)_fields[SupportParameter.SupportHeight].Value,
                GrooveLength = (double)_fields[SupportParameter.GrooveLength].Value,
                GrooveHeight = (double)_fields[SupportParameter.GrooveHeight].Value,
                GrooveDepth = (double)_fields[SupportParameter.GrooveDepth].Value,
                ThroughSlot = _throughSlot.Checked
            };
        }

        /// <summary>Заполняет поля ввода значениями из переданной модели.</summary>
        public void SetParameters(SupportParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            _throughSlot.Checked = parameters.ThroughSlot;
            _fields[SupportParameter.SupportLength].Value = (decimal)parameters.SupportLength;
            _fields[SupportParameter.SupportWidth].Value = (decimal)parameters.SupportWidth;
            _fields[SupportParameter.SupportHeight].Value = (decimal)parameters.SupportHeight;
            _fields[SupportParameter.GrooveLength].Value = (decimal)parameters.GrooveLength;
            _fields[SupportParameter.GrooveHeight].Value = (decimal)parameters.GrooveHeight;
            _fields[SupportParameter.GrooveDepth].Value = (decimal)parameters.GrooveDepth;
        }

        public void SetDefault()
        {
            SetParameters(SupportParameters.Default());
        }

        public void ClearErrors()
        {
            foreach (SupportParameter parameter in _fields.Keys)
            {
                ClearError(parameter);
            }
        }

        /// <summary>
        /// Подсвечивает поле, соответствующее параметру, и показывает
        /// текст ошибки в подсказке при наведении.
        /// </summary>
        public void SetError(SupportParameter parameter, string message)
        {
            NumericUpDown field;
            if (!_fields.TryGetValue(parameter, out field))
            {
                return;
            }

            field.BackColor = Color.MistyRose;
            _toolTip.SetToolTip(field, message);
        }

        /// <summary>Разносит список ошибок валидации по полям контрола.</summary>
        public void ApplyErrors(IReadOnlyList<ValidationError> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            foreach (ValidationError error in errors)
            {
                SetError(error.Parameter, error.Message);
            }
        }

        private void ClearError(SupportParameter parameter)
        {
            NumericUpDown field;
            if (!_fields.TryGetValue(parameter, out field))
            {
                return;
            }

            field.BackColor = SystemColors.Window;
            _toolTip.SetToolTip(field, string.Empty);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _toolTip.Dispose();
            base.OnHandleDestroyed(e);
        }
    }
}
