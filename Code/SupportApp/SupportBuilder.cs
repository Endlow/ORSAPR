using System;

namespace SupportApp
{
    /// <summary>
    /// Строит модель опоры с пазом по переданным параметрам.
    /// Единственная точка входа для MainForm — метод <see cref="Build"/>.
    /// </summary>
    public class SupportBuilder : IDisposable
    {
        private readonly Kompas3DWrapper _wrapper;

        public SupportBuilder() : this(new Kompas3DWrapper())
        {
        }

        public SupportBuilder(Kompas3DWrapper wrapper)
        {
            _wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
        }

        /// <summary>
        /// Полностью строит модель и сохраняет её по указанному пути.
        /// Возвращает путь к сохранённому файлу.
        /// </summary>
        public string Build(SupportParameters parameters, string filePath)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "Путь к файлу модели не задан.", nameof(filePath));
            }

            _wrapper.ConnectOrStart();
            _wrapper.CreateNewPartDocument();

            BuildBase(parameters);
            BuildSlot(parameters);

            _wrapper.SetIsoView();
            SaveModel(filePath);

            return filePath;
        }

        /// <summary>Строит основание опоры — прямоугольную стойку L × W × H.</summary>
        private void BuildBase(SupportParameters parameters)
        {
            var sketch = _wrapper.CreateSketch(1);
            double halfLength = parameters.SupportLength / 2;
            double halfHeight = parameters.SupportHeight / 2;

            sketch.Document2D.ksLineSeg(-halfLength, -halfHeight, halfLength, -halfHeight, 1);
            sketch.Document2D.ksLineSeg(halfLength, -halfHeight, halfLength, halfHeight, 1);
            sketch.Document2D.ksLineSeg(halfLength, halfHeight, -halfLength, halfHeight, 1);
            sketch.Document2D.ksLineSeg(-halfLength, halfHeight, -halfLength, -halfHeight, 1);
            sketch.Commit();

            _wrapper.BaseExtrude(sketch.Sketch, parameters.SupportWidth);
        }

        /// <summary>Строит паз d × h глубиной p (или на всю ширину W).</summary>
        private void BuildSlot(SupportParameters parameters)
        {
            var sketch = _wrapper.CreateSketch(1);
            double halfLength = parameters.GrooveLength / 2;
            double halfHeight = parameters.GrooveHeight / 2;

            sketch.Document2D.ksLineSeg(-halfLength, -halfHeight, halfLength, -halfHeight, 1);
            sketch.Document2D.ksLineSeg(halfLength, -halfHeight, halfLength, halfHeight, 1);
            sketch.Document2D.ksLineSeg(halfLength, halfHeight, -halfLength, halfHeight, 1);
            sketch.Document2D.ksLineSeg(-halfLength, halfHeight, -halfLength, -halfHeight, 1);
            sketch.Commit();

            double depth = parameters.ThroughSlot
                ? parameters.SupportWidth
                : parameters.GrooveDepth;
            _wrapper.CutExtrude(sketch.Sketch, depth);
        }

        public void SaveModel(string filePath)
        {
            _wrapper.SaveAs(filePath);
        }

        public void CloseModel()
        {
            _wrapper.CloseDocument();
        }

        public void Dispose()
        {
            _wrapper.Dispose();
        }
    }
}
