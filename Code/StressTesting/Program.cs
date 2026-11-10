using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using OporaApp;

namespace StressTesting
{
    internal static class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        private static double UsedRamGb()
        {
            var status = new MEMORYSTATUSEX();
            status.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            GlobalMemoryStatusEx(ref status);
            return (status.ullTotalPhys - status.ullAvailPhys) / 1073741824.0;
        }

        private static double KompasCpuSeconds()
        {
            double total = 0;
            foreach (Process process in Process.GetProcessesByName("KOMPAS"))
            {
                try
                {
                    total += process.TotalProcessorTime.TotalSeconds;
                }
                catch (Exception)
                {
                }
                finally
                {
                    process.Dispose();
                }
            }
            return total;
        }

        [STAThread]
        private static void Main(string[] args)
        {
            int totalBuilds = args.Length > 0 ? int.Parse(args[0]) : 60;
            var builder = new Builder(new Kompas3DWrapper());
            var parameters = OporeParameters.Default();
            string savePath = Path.Combine(Path.GetTempPath(), "OporaStress.m3d");
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

            using (var writer = new StreamWriter(logPath, false))
            {
                double prevCpuSeconds = 0;
                var stopwatch = new Stopwatch();

                for (int i = 0; i < totalBuilds; i++)
                {
                    stopwatch.Restart();
                    builder.Build(parameters, savePath);
                    stopwatch.Stop();

                    double wallSeconds = stopwatch.Elapsed.TotalSeconds;
                    double cpuSeconds = KompasCpuSeconds();
                    double cpuDelta = cpuSeconds - prevCpuSeconds;
                    double cpuPercent = i == 0 ? 0 : cpuDelta / wallSeconds * 100.0;
                    double ramGb = UsedRamGb();

                    writer.WriteLine(string.Format("{0}\t{1:hh\\:mm\\:ss}\t{2:F3}\t{3:F1}",
                        i + 1, stopwatch.Elapsed, ramGb, cpuPercent));
                    writer.Flush();

                    prevCpuSeconds = cpuSeconds;
                    Console.WriteLine("{0}\t{1:hh\\:mm\\:ss}\t{2:F3}\t{3:F1}",
                        i + 1, stopwatch.Elapsed, ramGb, cpuPercent);
                }
            }

            Console.WriteLine("Done: " + logPath);
        }
    }
}
