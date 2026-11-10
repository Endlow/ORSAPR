using System;
using Kompas6API5;
using System.Runtime.InteropServices;

namespace OporaApp
{
    public class Kompas3DWrapper
    {
        private KompasObject _kompas;
        private ksDocument3D _document3d;
        private ksPart _part;

        public bool Connected { get; private set; }

        public void ConnectOrStart()
        {
            var type = Type.GetTypeFromProgID("KOMPAS.Application.5");
            if (type == null)
            {
                throw new InvalidOperationException(
                    "КОМПАС-3D не установлен или COM-сервер не зарегистрирован.");
            }

            object instance = Activator.CreateInstance(type);
            _kompas = (KompasObject)instance;
            _kompas.Visible = true;
            Connected = true;
        }

        public void CreateNewPartDocument()
        {
            EnsureConnected();
            _document3d = (ksDocument3D)_kompas.Document3D();
            if (!_document3d.Create(false, true))
            {
                throw new InvalidOperationException(
                    "Не удалось создать документ-деталь в КОМПАС-3D.");
            }
            _part = (ksPart)_document3d.GetPart(-1);
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
            bool created = boss.Create();
            if (!created)
            {
                throw new InvalidOperationException(
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
            bool created = cut.Create();
            if (!created)
            {
                throw new InvalidOperationException(
                    "Операция вырезания не выполнена.");
            }
            _part.RebuildModel();
        }

        public void SetIsoView()
        {
            SetViewByName(new[] { "Изометрия XYZ", "Изометрия", "Isometric XYZ" });
        }

        public void SetFrontView()
        {
            SetViewByName(new[] { "Спереди", "Front" });
        }

        private void SetViewByName(string[] names)
        {
            EnsureConnected();
            var views = (ksViewProjectionCollection)
                _document3d.GetViewProjectionCollection();
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

        public void SaveAs(string path)
        {
            EnsureConnected();
            _document3d.SaveAs(path);
        }

        public void CloseDocument()
        {
            if (_document3d != null)
            {
                _document3d.close();
                _document3d = null;
                _part = null;
            }
        }

        private void EnsureConnected()
        {
            if (!Connected)
            {
                throw new InvalidOperationException(
                    "Нет подключения к КОМПАС-3D. Сначала выполните ConnectOrStart().");
            }
        }
    }

    public class SketchSession
    {
        public ksEntity Sketch { get; }
        public ksSketchDefinition Definition { get; }
        public ksDocument2D Document2D { get; }

        public SketchSession(ksEntity sketch, ksSketchDefinition definition,
            ksDocument2D document2d)
        {
            Sketch = sketch;
            Definition = definition;
            Document2D = document2d;
        }

        public void Commit()
        {
            Definition.EndEdit();
        }
    }
}
