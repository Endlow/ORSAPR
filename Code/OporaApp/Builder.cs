using Kompas6API5;

namespace OporaApp
{
    public class Builder
    {
        private readonly Kompas3DWrapper _wrapper;

        public Builder(Kompas3DWrapper wrapper)
        {
            _wrapper = wrapper;
        }

        public string Build(OporeParameters p, string filePath)
        {
            _wrapper.ConnectOrStart();
            _wrapper.CreateNewPartDocument();

            var baseSketch = _wrapper.CreateSketch(1);
            double halfL = p.L / 2, halfH = p.H / 2;
            baseSketch.Document2D.ksLineSeg(-halfL, -halfH, halfL, -halfH, 1);
            baseSketch.Document2D.ksLineSeg(halfL, -halfH, halfL, halfH, 1);
            baseSketch.Document2D.ksLineSeg(halfL, halfH, -halfL, halfH, 1);
            baseSketch.Document2D.ksLineSeg(-halfL, halfH, -halfL, -halfH, 1);
            baseSketch.Commit();

            _wrapper.BaseExtrude(baseSketch.Sketch, p.W);

            var slotSketch = _wrapper.CreateSketch(1);
            double halfD = p.D / 2, halfH1 = p.H1 / 2;
            slotSketch.Document2D.ksLineSeg(-halfD, -halfH1, halfD, -halfH1, 1);
            slotSketch.Document2D.ksLineSeg(halfD, -halfH1, halfD, halfH1, 1);
            slotSketch.Document2D.ksLineSeg(halfD, halfH1, -halfD, halfH1, 1);
            slotSketch.Document2D.ksLineSeg(-halfD, halfH1, -halfD, -halfH1, 1);
            slotSketch.Commit();

            double cutDepth = p.ThroughSlot ? p.W : p.P;
            _wrapper.CutExtrude(slotSketch.Sketch, cutDepth);

            _wrapper.SetIsoView();
            _wrapper.SaveAs(filePath);
            return filePath;
        }
    }
}
