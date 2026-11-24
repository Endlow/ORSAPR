using System.Collections.Generic;

namespace SupportApp
{
    /// <summary>
    /// Идентификатор параметра опоры. Используется для привязки ошибки
    /// валидации к конкретному полю ввода в <see cref="SupportParametersControl"/>.
    /// </summary>
    public enum SupportParameter
    {
        SupportLength,
        SupportWidth,
        SupportHeight,
        GrooveLength,
        GrooveHeight,
        GrooveDepth
    }

    /// <summary>
    /// Ошибка валидации одного параметра. Содержит идентификатор параметра,
    /// по которому форма находит нужное поле ввода, и готовый текст сообщения.
    /// </summary>
    public class ValidationError
    {
        public SupportParameter Parameter { get; }

        public string Message { get; }

        public ValidationError(SupportParameter parameter, string message)
        {
            Parameter = parameter;
            Message = message;
        }
    }

    /// <summary>
    /// Параметры опоры с пазом и правила их проверки.
    /// Ограничения диапазонов и зависимости между параметрами,
    /// заданные в п. 3.1.1 ТЗ, собраны в методе <see cref="Validate"/>.
    /// </summary>
    public class SupportParameters
    {
        /// <summary>Минимальный отступ от торцов паза до боковых граней опоры, мм (M1).</summary>
        public const double GrooveEndMargin = 10;

        /// <summary>Минимальный отступ от паза до верхней и нижней граней опоры, мм (M2).</summary>
        public const double GrooveVerticalMargin = 20;

        /// <summary>Минимальная остаточная стенка за пазом, мм (M3).</summary>
        public const double GrooveBackMargin = 10;

        /// <summary>Длина опоры L, мм.</summary>
        public double SupportLength { get; set; } = 80;

        /// <summary>Ширина опоры W, мм.</summary>
        public double SupportWidth { get; set; } = 60;

        /// <summary>Высота опоры H, мм.</summary>
        public double SupportHeight { get; set; } = 120;

        /// <summary>Длина паза d, мм.</summary>
        public double GrooveLength { get; set; } = 40;

        /// <summary>Высота паза h, мм.</summary>
        public double GrooveHeight { get; set; } = 50;

        /// <summary>Глубина паза p, мм.</summary>
        public double GrooveDepth { get; set; } = 30;

        /// <summary>Признак сквозного паза: паз вырезается на всю ширину W.</summary>
        public bool ThroughSlot { get; set; }

        public static (double Min, double Max) SupportLengthRange => (60, 150);

        public static (double Min, double Max) SupportWidthRange => (50, 100);

        public static (double Min, double Max) SupportHeightRange => (100, 300);

        public static (double Min, double Max) GrooveLengthRange => (20, 100);

        public static (double Min, double Max) GrooveHeightRange => (20, 80);

        public static (double Min, double Max) GrooveDepthRange => (20, 80);

        public static SupportParameters Default() => new SupportParameters();

        /// <summary>
        /// Проверяет диапазоны параметров и зависимости между ними.
        /// Возвращает список ошибок; каждая ошибка привязана к параметру
        /// через <see cref="ValidationError.Parameter"/>.
        /// </summary>
        public IReadOnlyList<ValidationError> Validate()
        {
            var errors = new List<ValidationError>();

            CheckRange(errors, SupportParameter.SupportLength, SupportLength, SupportLengthRange);
            CheckRange(errors, SupportParameter.SupportWidth, SupportWidth, SupportWidthRange);
            CheckRange(errors, SupportParameter.SupportHeight, SupportHeight, SupportHeightRange);
            CheckRange(errors, SupportParameter.GrooveLength, GrooveLength, GrooveLengthRange);
            CheckRange(errors, SupportParameter.GrooveHeight, GrooveHeight, GrooveHeightRange);

            if (!ThroughSlot)
            {
                CheckRange(errors, SupportParameter.GrooveDepth, GrooveDepth, GrooveDepthRange);
            }

            // Зависимости параметров (п. 3.1.1 ТЗ):
            // d <= L - 2*M1; h <= H - 2*M2; p <= W - M3.
            if (!HasError(errors, SupportParameter.SupportLength) &&
                !HasError(errors, SupportParameter.GrooveLength) &&
                GrooveLength > SupportLength - 2 * GrooveEndMargin)
            {
                errors.Add(new ValidationError(SupportParameter.GrooveLength,
                    "Длина паза d = " + GrooveLength.ToString("0.##") +
                    " мм не должна выходить за боковые грани опоры: " +
                    "d ≤ L − 2·M₁ = " + (SupportLength - 2 * GrooveEndMargin).ToString("0.##") +
                    " мм. Уменьшите d или увеличьте L минимум до " +
                    (GrooveLength + 2 * GrooveEndMargin).ToString("0.##") + " мм."));
            }

            if (!HasError(errors, SupportParameter.SupportHeight) &&
                !HasError(errors, SupportParameter.GrooveHeight) &&
                GrooveHeight > SupportHeight - 2 * GrooveVerticalMargin)
            {
                errors.Add(new ValidationError(SupportParameter.GrooveHeight,
                    "Высота паза h = " + GrooveHeight.ToString("0.##") +
                    " мм не должна выходить за верхнюю и нижнюю грани опоры: " +
                    "h ≤ H − 2·M₂ = " + (SupportHeight - 2 * GrooveVerticalMargin).ToString("0.##") +
                    " мм. Уменьшите h или увеличьте H минимум до " +
                    (GrooveHeight + 2 * GrooveVerticalMargin).ToString("0.##") + " мм."));
            }

            if (!ThroughSlot &&
                !HasError(errors, SupportParameter.SupportWidth) &&
                !HasError(errors, SupportParameter.GrooveDepth) &&
                GrooveDepth > SupportWidth - GrooveBackMargin)
            {
                errors.Add(new ValidationError(SupportParameter.GrooveDepth,
                    "Глубина паза p = " + GrooveDepth.ToString("0.##") +
                    " мм не должна выходить за заднюю грань опоры: " +
                    "p ≤ W − M₃ = " + (SupportWidth - GrooveBackMargin).ToString("0.##") +
                    " мм. Уменьшите p или увеличьте W минимум до " +
                    (GrooveDepth + GrooveBackMargin).ToString("0.##") + " мм."));
            }

            return errors;
        }

        private static void CheckRange(List<ValidationError> errors,
            SupportParameter parameter, double value,
            (double Min, double Max) range)
        {
            if (value < range.Min || value > range.Max)
            {
                errors.Add(new ValidationError(parameter,
                    "Значение " + GetDisplayName(parameter) + " = " +
                    value.ToString("0.##") + " мм выходит за допустимый диапазон [" +
                    range.Min.ToString("0.##") + "; " + range.Max.ToString("0.##") + "] мм."));
            }
        }

        private static bool HasError(List<ValidationError> errors, SupportParameter parameter)
        {
            return errors.Exists(e => e.Parameter == parameter);
        }

        private static string GetDisplayName(SupportParameter parameter)
        {
            switch (parameter)
            {
                case SupportParameter.SupportLength:
                    return "длины опоры L";
                case SupportParameter.SupportWidth:
                    return "ширины опоры W";
                case SupportParameter.SupportHeight:
                    return "высоты опоры H";
                case SupportParameter.GrooveLength:
                    return "длины паза d";
                case SupportParameter.GrooveHeight:
                    return "высоты паза h";
                case SupportParameter.GrooveDepth:
                    return "глубины паза p";
                default:
                    return parameter.ToString();
            }
        }
    }
}
