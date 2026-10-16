using System;
using System.IO;
using System.Text;
using BHT.Core;

namespace BHT.Bridge
{
    // Diagnostic output only. It never reads or writes drawing data.
    public static class RollingImportLog
    {
        static readonly object Sync = new object();
        public static string PathName { get; private set; }

        public static void Begin(string drawing)
        {
            lock (Sync)
            {
                PathName = null;
                try
                {
                    string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BHT", "Logs");
                    Directory.CreateDirectory(dir);
                    string path = Path.Combine(dir, "rolling-import-last.log");
                    File.WriteAllText(path, "BHT " + BhtVersion.Version + "\r\nĐích: " + drawing + "\r\nBridge: " + typeof(RollingImportLog).Assembly.Location + "\r\n", Encoding.UTF8);
                    PathName = path;
                    Note("Bắt đầu lệnh BHTNAPTICHLUY — chưa chọn tệp");
                }
                catch { }
            }
        }

        public static void Note(string message)
        {
            lock (Sync)
            {
                if (PathName == null) return;
                try { File.AppendAllText(PathName, DateTime.Now.ToString("O") + " " + message + "\r\n", Encoding.UTF8); }
                catch { }
            }
        }
    }
}
