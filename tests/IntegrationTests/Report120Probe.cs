using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

public static class Report120Probe
{
    private static int checks;
    private static void Check(bool ok, string message)
    {
        if (!ok) throw new InvalidOperationException(message);
        checks++; AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nPASS REPORT120 " + message);
    }
    private static List<SignReportRow> Rows(Database db)
    {
        using (var service = new BhtDataService(db, () => "", false))
            return SignReportRecords.Build(service.GetObjects(), "BHT 1.0.30", code =>
            {
                var item = TdtSignLibrary.Find(code);
                return item == null ? null : new SignReportRow { SignGroup = item.Group, Description = item.Description };
            });
    }
    private static string Snapshot(Database db)
    {
        using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            return string.Join("|", BhtStore.All(tr, db, "OBJ").Select(p => p.Key + p.Value.ToCanonical()).ToArray());
    }
    private static void VerifyWorkbook(string path, IList<SignReportRow> rows)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        using (var file = File.OpenRead(path))
        using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
        {
            XDocument detail, summary;
            using (var s = zip.GetEntry("xl/worksheets/sheet1.xml").Open()) summary = XDocument.Load(s);
            using (var s = zip.GetEntry("xl/worksheets/sheet2.xml").Open()) detail = XDocument.Load(s);
            var data = detail.Descendants(ns + "row").Where(r => (int)r.Attribute("r") >= 5).ToList();
            Check(data.Count == rows.Count, "workbook detail agrees with all faces");
            int total = summary.Descendants(ns + "row").Where(r => (int)r.Attribute("r") >= 6)
                .Sum(r => int.Parse(r.Elements(ns + "c").Single(c => ((string)c.Attribute("r")).StartsWith("E")).Element(ns + "v").Value));
            Check(total == rows.Count, "summary quantity reconciles with detail");
            Check(rows.All(r => r.Code == "" || data.Any(d => d.Elements(ns + "c").Any(c => ((string)c.Attribute("r")).StartsWith("F") && c.Value == r.Code))), "every saved face code is present in workbook");
        }
    }
    [CommandMethod("REPORT120")]
    public static void Run()
    {
        try
        {
            string dir = Environment.GetEnvironmentVariable("BHT_REPORT_OUTPUT"); Directory.CreateDirectory(dir);
            using (var db = new Database(true, true))
            {
                var warning = new BhtRecord().Add("nhom", "BIEN_BAO").Add("ma_hieu", "W.239a").Add("so_mat", "2").Add("so_tru", "1")
                    .Add("mat", "W.239a").Add("mat", "S.509a@5").Add("ly_trinh_km", "Km48+530.05").Add("ly_trinh_m", "48530.05")
                    .Add("route_id", "TIMRANHD8").Add("offset_m", "1.080").Add("doan", "DOAN08").Add("goi", "GOI03");
                using (var tr = db.TransactionManager.StartTransaction()) { BhtStore.Write(tr, db, "OBJ", "W239-S509", warning); tr.Commit(); }
                string before = Snapshot(db); var rows = Rows(db);
                Check(rows.Count == 2 && SignReportWorkbook.InstallationCount(rows) == 1, "XRecord installation expands into both primary and auxiliary faces");
                Check(rows[1].Code == "S.509a" && rows[1].SignGroup == "Biển phụ" && rows[1].FaceContent == "5 m", "real catalog resolves auxiliary with metre parameter");
                Check(rows.All(r => r.Route == "TIMRANHD8" && r.Chainage == "Km48+530.05" && r.Offset == "1.080"), "station and offset retained for both faces");
                string path = Path.Combine(dir, "BHT-1.0.20-W239a-S509a-5m.xlsx");
                SignReportWorkbook.Write(path, "Mẫu W.239a + S.509a (5 m)", rows); VerifyWorkbook(path, rows);
                Check(before == Snapshot(db), "export never alters XRecord data");
            }
            string inputs = Environment.GetEnvironmentVariable("BHT_REPORT_INPUTS");
            if (!string.IsNullOrEmpty(inputs))
            foreach (string input in inputs.Split('|'))
            {
                using (var db = new Database(false, true))
                {
                    db.ReadDwgFile(input, FileOpenMode.OpenForReadAndAllShare, false, ""); db.CloseInput(true);
                    string before = Snapshot(db); var rows = Rows(db);
                    string name = Path.GetFileNameWithoutExtension(input);
                    string path = Path.Combine(dir, "BHT-1.0.20-" + name + ".xlsx");
                    SignReportWorkbook.Write(path, name, rows); VerifyWorkbook(path, rows);
                    Check(before == Snapshot(db), "read-only input DWG objects unchanged");
                    File.WriteAllLines(path + ".txt", new[] { "INPUT=" + input, "INSTALLATIONS=" + SignReportWorkbook.InstallationCount(rows), "FACES=" + rows.Count,
                        "AUXILIARY=" + rows.Count(r => r.Code.StartsWith("S.", StringComparison.OrdinalIgnoreCase)),
                        "W239=" + string.Join("; ", rows.Where(r => r.Code.StartsWith("W.239", StringComparison.OrdinalIgnoreCase) || r.Code.StartsWith("S.509", StringComparison.OrdinalIgnoreCase)).Select(r => r.ObjectId + " " + r.Code + " " + r.Chainage + " " + r.FaceContent).ToArray()) }, new UTF8Encoding(true));
                    AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nREPORT120-INPUT " + name + " INSTALLATIONS=" + SignReportWorkbook.InstallationCount(rows) + " FACES=" + rows.Count);
                }
            }
            AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nREPORT120-PASS CHECKS=" + checks);
        }
        catch (System.Exception ex) { AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nREPORT120-FAIL " + ex); }
    }
}
