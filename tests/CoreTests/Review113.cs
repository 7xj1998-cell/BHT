using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using BHT.Core;

namespace BHT.CoreTests
{
    public static class Review113
    {
        public static void Run(Action<string, string, bool> check)
        {
            double metres;
            check("C113a", "reject overflowing manual station", !Chainage.TryParse("1e100", out metres));
            check("C113b", "formatter does not throw on extreme input", Chainage.Format(double.MaxValue) == "");
            check("C113c", "reject NaN and infinity station", !Chainage.TryParse("NaN", out metres) && !Chainage.TryParse("Infinity", out metres));
            check("C113d", "representable station boundary", Chainage.TryParse("2147483647000", out metres) && Chainage.Format(metres) == "Km2147483647+000.00" && !Chainage.TryParse("2147483648000", out metres));
            var marks = new List<RouteControlPoint> { new RouteControlPoint(0, 1000), new RouteControlPoint(100, 1100) };
            check("C113e", "reject NaN projection distance", !RouteModelLogic.ToRouteDistance(double.NaN, 100, 0, 1, false).HasValue);
            check("C113f", "reject NaN station query", RouteModelLogic.Station(marks, double.NaN, 0).Station == null);
            check("C113g", "reject infinite extrapolation", RouteModelLogic.Station(marks, 200, double.PositiveInfinity).Station == null);
            marks.Add(new RouteControlPoint(100, 1200));
            check("C113h", "reject duplicate controls before extrapolation", RouteModelLogic.Station(marks, 150, 100).Status == "INVALID_CONTROL");
            marks = new List<RouteControlPoint> { new RouteControlPoint(0, double.NaN) };
            check("C113i", "reject invalid control values", RouteModelLogic.Station(marks, 0, 0).Status == "INVALID_CONTROL");
            marks = new List<RouteControlPoint> { new RouteControlPoint(0, 1000), new RouteControlPoint(100, 1100, 1200), new RouteControlPoint(200, 1300) };
            check("C113j", "retain deliberate station break", RouteModelLogic.Station(marks, 100, 0).Station == 1100 && RouteModelLogic.Station(marks, 150, 0).Station == 1250);
            string directory = Path.Combine(Path.GetTempPath(), "BHT-report113-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "report.xlsx");
            try
            {
                var rows = new List<SignReportRow> { new SignReportRow { Note = "Tiếng Việt\u0001_x0041_\r\n\U0001F6A6 & < >" } };
                SignReportWorkbook.Write(path, "Test", rows);
                using (var file = File.OpenRead(path))
                using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                {
                    bool allValid = true;
                    foreach (var entry in zip.Entries.Where(e => e.FullName.EndsWith(".xml")))
                        try { using (var stream = entry.Open()) XDocument.Load(stream); } catch { allValid = false; }
                    check("C113k", "control characters produce valid workbook XML", allValid);
                    string xml;
                    using (var reader = new StreamReader(zip.GetEntry("xl/worksheets/sheet2.xml").Open())) xml = reader.ReadToEnd();
                    check("C113l", "preserve controls literal escapes and emoji", xml.Contains("_x0001__x005F_x0041__x000D_\n\U0001F6A6") && xml.Contains("Tiếng Việt"));
                }
                byte[] before = File.ReadAllBytes(path);
                bool rejected = false;
                try { SignReportWorkbook.Write(path, "Bad", new List<SignReportRow> { null }); } catch (ArgumentException) { rejected = true; }
                check("C113m", "failed export preserves existing workbook", rejected && before.SequenceEqual(File.ReadAllBytes(path)));
                rejected = false;
                using (var locked = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                    try { SignReportWorkbook.Write(path, "Locked", rows); } catch (IOException) { rejected = true; }
                check("C113n", "locked export preserves existing workbook", rejected && before.SequenceEqual(File.ReadAllBytes(path)));
                check("C113o", "failed export removes temporary file", Directory.GetFiles(directory).Length == 1);
                rows[0].Note = "Đã thay báo cáo";
                SignReportWorkbook.Write(path, "Updated", rows);
                check("C113p", "completed export replaces existing workbook", !before.SequenceEqual(File.ReadAllBytes(path)));
            }
            finally { Directory.Delete(directory, true); }
        }
    }
}
