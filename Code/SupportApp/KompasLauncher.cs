using System;
using Kompas6API5;

namespace SupportApp
{
    /// <summary>
    /// Запускает и удерживает COM-объект КОМПАС-3D. Отвечает только за
    /// подключение к приложению и не содержит геометрических операций.
    /// </summary>
    public class KompasLauncher : IDisposable
    {
        private KompasObject _kompas;

        public KompasObject Kompas => _kompas;

        public bool IsConnected => _kompas != null;

        public void ConnectOrStart()
        {
            if (IsConnected)
            {
                return;
            }

            var type = Type.GetTypeFromProgID("KOMPAS.Application.5");
            if (type == null)
            {
                throw new BuildException(
                    "КОМПАС-3D не установлен или COM-сервер не зарегистрирован.");
            }

            try
            {
                _kompas = (KompasObject)Activator.CreateInstance(type);
                _kompas.Visible = true;
            }
            catch (Exception ex)
            {
                _kompas = null;
                throw new BuildException("Не удалось запустить КОМПАС-3D.", ex);
            }
        }

        public void Disconnect()
        {
            _kompas = null;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
