using System.Collections.Generic;

namespace OporaApp
{
    public class ValidationError
    {
        public string FieldName { get; }
        public string Message { get; }

        public ValidationError(string fieldName, string message)
        {
            FieldName = fieldName;
            Message = message;
        }
    }

    public class OporeParameters
    {
        public const double M1 = 10;
        public const double M2 = 20;
        public const double M3 = 10;

        public double L { get; set; } = 80;
        public double W { get; set; } = 60;
        public double H { get; set; } = 120;
        public double D { get; set; } = 40;
        public double H1 { get; set; } = 50;
        public double P { get; set; } = 30;

        public bool ThroughSlot { get; set; }

        public static (double Min, double Max) LRange => (60, 150);
        public static (double Min, double Max) WRange => (50, 100);
        public static (double Min, double Max) HRange => (100, 300);
        public static (double Min, double Max) DRange => (20, 100);
        public static (double Min, double Max) H1Range => (20, 80);
        public static (double Min, double Max) PRange => (20, 80);

        public static OporeParameters Default() => new OporeParameters();

        public List<ValidationError> Validate()
        {
            var errors = new List<ValidationError>();

            CheckRange(errors, nameof(L), "длины опоры L", L, LRange);
            CheckRange(errors, nameof(W), "ширины опоры W", W, WRange);
            CheckRange(errors, nameof(H), "высоты опоры H", H, HRange);
            CheckRange(errors, nameof(D), "длины паза d", D, DRange);
            CheckRange(errors, nameof(H1), "высоты паза h", H1, H1Range);
            if (!ThroughSlot)
            {
                CheckRange(errors, nameof(P), "глубины паза p", P, PRange);
            }

            if (!errors.Exists(e => e.FieldName == nameof(L)) && D > L - 2 * M1)
            {
                errors.Add(new ValidationError(nameof(D),
                    "Длина паза d = " + D.ToString("0.##") + " мм не должна выходить за боковые грани опоры: " +
                    "d ≤ L − 2·M₁ = " + (L - 2 * M1).ToString("0.##") + " мм. " +
                    "Уменьшите d или увеличьте L минимум до " + (D + 2 * M1).ToString("0.##") + " мм."));
            }

            if (!errors.Exists(e => e.FieldName == nameof(H)) &&
                !errors.Exists(e => e.FieldName == nameof(H1)) && H1 > H - 2 * M2)
            {
                errors.Add(new ValidationError(nameof(H1),
                    "Высота паза h = " + H1.ToString("0.##") + " мм не должна выходить за верхнюю и нижнюю грани опоры: " +
                    "h ≤ H − 2·M₂ = " + (H - 2 * M2).ToString("0.##") + " мм. " +
                    "Уменьшите h или увеличьте H минимум до " + (H1 + 2 * M2).ToString("0.##") + " мм."));
            }

            if (!ThroughSlot && !errors.Exists(e => e.FieldName == nameof(W)) &&
                !errors.Exists(e => e.FieldName == nameof(P)) && P > W - M3)
            {
                errors.Add(new ValidationError(nameof(P),
                    "Глубина паза p = " + P.ToString("0.##") + " мм не должна выходить за заднюю грань опоры: " +
                    "p ≤ W − M₃ = " + (W - M3).ToString("0.##") + " мм. " +
                    "Уменьшите p или увеличьте W минимум до " + (P + M3).ToString("0.##") + " мм."));
            }

            return errors;
        }

        private static void CheckRange(List<ValidationError> errors, string field,
            string displayName, double value, (double Min, double Max) range)
        {
            if (value < range.Min || value > range.Max)
            {
                errors.Add(new ValidationError(field,
                    "Значение " + displayName + " = " + value.ToString("0.##") + " мм выходит за допустимый диапазон [" +
                    range.Min.ToString("0.##") + "; " + range.Max.ToString("0.##") + "] мм."));
            }
        }
    }
}
