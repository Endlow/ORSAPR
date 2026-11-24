using System;
using Kompas6API5;

namespace SupportApp
{
    /// <summary>
    /// Обёртка над API КОМПАС-3D: создаёт документ, эскизы и операции
    /// выдавливания/вырезания. Все ошибки оборачиваются в единый
    /// <see cref="BuildException"/>.
    /// </summary>
    public class Kompas3DWrapper : IDisposable
    {
        private readonly KompasLauncher _launcher;
        private ksDocument3D _document3D;
        private ksPart _part;

        public Kompas3DWrapper() : this(new KompasLauncher())
        {
        }

        public Kompas3DWrapper(KompasLauncher launcher)
        {
            _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        }

        public void ConnectOrStart()
        {
            _launcher.ConnectOrStart();
        }

        public void CreateNewPartDocument()
        {
            EnsureConnected();
            _document3D = (ksDocument3D)_launcher.Kompas.Document3D();
            if (!_document3D.Create(false, true))
            {
                throw new BuildException(
                    "Не удалось создать документ-деталь в КОМПАС-3D.");
            }
            _part = (ksPart)_document3D.GetPart(-1);
        }

        public SketchSession CreateSketch(int planeType)
        {
            EnsureConnected();
            ksEntity sketch = (ksEntity)_part.NewEntity(5);
            ksSketchDefinition sketchDef =
                (ksSketchDefinition)sketch.GetDefinition();
            ksEntity plane =
                (ksEntity)_part.GetDefaultEntity((short)planeType);
            sketchDef.SetPlane(plane);
            sketch.Create();
            ksDocument2D doc2d = (ksDocument2D)sketchDef.BeginEdit();
            return new SketchSession(sketch, sketchDef, doc2d);
        }

        public void BaseExtrude(ksEntity sketch, double depth)
        {
            EnsureConnected();
            ksEntity boss = (ksEntity)_part.NewEntity(24);
            ksBaseExtrusionDefinition bossDef =
                (ksBaseExtrusionDefinition)boss.GetDefinition();
            bossDef.SetThinParam(false, (short)0, 0, 0);
            bossDef.SetSideParam(true, (short)0, depth, 0);
            bossDef.SetSketch(sketch);
            if (!boss.Create())
            {
                throw new BuildException(
                    "Операция выдавливания не выполнена.");
            }
            _part.RebuildModel();
        }

        public void CutExtrude(ksEntity sketch, double depth)
        {
            EnsureConnected();
            ksEntity cut = (ksEntity)_part.NewEntity(26);
            ksCutExtrusionDefinition cutDef =
                (ksCutExtrusionDefinition)cut.GetDefinition();
            cutDef.SetThinParam(false, (short)0, 0, 0);
            cutDef.SetSideParam(false, (short)0, depth, 0);
            cutDef.SetSketch(sketch);
            if (!cut.Create())
            {
                throw new BuildException(
                    "Операция вырезания не выполнена.");
            }
            _part.RebuildModel();
        }

        public void SetIsoView()
        {
            SetViewByName(new[] { "Изометрия XYZ", "Изометрия", "Isometric XYZ" });
        }

        public void SaveAs(string path)
        {
            EnsureConnected();
            try
            {
                _document3D.SaveAs(path);
            }
            catch (Exception ex)
            {
                throw new BuildException(
                    "Не удалось сохранить файл модели по указанному пути.\n" +
                    "Проверьте, что папка назначения существует и доступна для записи.",
                    ex);
            }

            if (!System.IO.File.Exists(path))
            {
                throw new BuildException(
                    "Не удалось сохранить файл модели по указанному пути.\n" +
                    "Проверьте, что папка назначения существует и доступна для записи.");
            }
        }

        public void CloseDocument()
        {
            if (_document3D != null)
            {
                try
                {
                    _document3D.close();
                }
                catch (Exception)
                {
                    // Документ уже мог быть закрыт пользователем в КОМПАС-3D.
                }

                _document3D = null;
                _part = null;
            }
        }

        public void Dispose()
        {
            CloseDocument();
            _launcher.Dispose();
        }

        private void EnsureConnected()
        {
            if (!_launcher.IsConnected)
            {
                throw new BuildException(
                    "Нет подключения к КОМПАС-3D. Сначала выполните ConnectOrStart().");
            }
        }

        private void SetViewByName(string[] names)
        {
            EnsureConnected();
            var views = (ksViewProjectionCollection)
                _document3D.GetViewProjectionCollection();
            foreach (string name in names)
            {
                ksViewProjection view =
                    (ksViewProjection)views.GetByName(name);
                if (view != null)
                {
                    view.SetCurrent();
                    return;
                }
            }
        }
    }

    public class SketchSession
    {
        public ksEntity Sketch { get; }

        public ksSketchDefinition Definition { get; }

        public ksDocument2D Document2D { get; }

        public SketchSession(ksEntity sketch, ksSketchDefinition definition,
            ksDocument2D document2D)
        {
            Sketch = sketch;
            Definition = definition;
            Document2D = document2D;
        }

        public void Commit()
        {
            Definition.EndEdit();
        }
    }
}
