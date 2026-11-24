using System;

namespace SupportApp
{
    /// <summary>
    /// Единый тип ошибки построения модели. Используется и слоем WinForms,
    /// и обёрткой над API КОМПАС-3D, чтобы все ошибки отображались одинаково.
    /// </summary>
    public class BuildException : Exception
    {
        public BuildException(string message) : base(message)
        {
        }

        public BuildException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
