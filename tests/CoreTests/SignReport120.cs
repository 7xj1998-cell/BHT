using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using BHT.Core;

namespace BHT.CoreTests
{
    public static class SignReport120
    {
        private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static BhtRecord Record(string code, params string[] faces)
        {
            return new BhtRecord().Add("nhom", "BIEN_BAO").Add("ma_hieu", code).Add("so_tru", "2")
                .Add("ly_trinh_m", "48530.05").Add("ly_trinh_km", "Km48+530.05").Add("route_id", "TIMRANHD8")
                .Add("offset_m", "1.080").Add("doan", "DOAN08").Add("goi", "GOI03").SetAll("mat", faces);
        }
        private static List<SignReportRow> Build(params BhtRecord[] records)
        {
            return SignReportRecords.Build(records.Select((r, i) => new KeyValuePair<string, BhtRecord>("OBJ-" + i, r)), "Đoạn 8", code =>
                code.StartsWith("W.239a") ? new SignReportRow { SignGroup = "Biển nguy hiểm", Description = "Đường cáp điện ở phía trên" }
                : code.StartsWith("S.509a") ? new SignReportRow { SignGroup = "Biển phụ", Description = "Chiều cao an toàn" } : null);
        }
        private static string Cell(XElement row, string column)
        {
            var cell = row.Elements(Ns + "c").Single(c => (string)c.Attribute("r") == column + (string)row.Attribute("r"));
            return cell.Value;
        }
        public static void Run(Action<string, string, bool> check)
        {
            var street = new SignContentFace { Index = 0, Code = "I.449" }; street.Values["NAME#0"] = "KÊNH GIÀ MIẾNG";
            check("C124a", "legacy street name exports default upper line", Build(Record("I.449").Set("sign_content", SignContent.Write(new[] { street })))[0].FaceContent == "ĐƯỜNG\nKÊNH GIÀ MIẾNG");
            street.Values["HEADER#0"] = "ĐƯỜNG TỈNH";
            check("C124b", "street header and name export in face order", Build(Record("I.449").Set("sign_content", SignContent.Write(new[] { street })))[0].FaceContent == "ĐƯỜNG TỈNH\nKÊNH GIÀ MIẾNG");
            var original = Record("W.239a", "W.239a", "S.509a@5").Add("so_mat", "2").Add("mo_ta", "Cáp điện cao thế");
            var content = new SignContentFace { Index = 1, Code = "S.509a@5" };
            content.Values["DESC_1#0"] = "CHIỀU CAO"; content.Values["DESC_2#0"] = "AN TOÀN";
            original.Add("sign_content", SignContent.Write(new[] { content }));
            string before = original.ToCanonical();
            var rows = Build(original);
            check("C120a", "W239a and auxiliary S509a export independently with parameter in content", rows.Count == 2 && rows[0].Code == "W.239a" && rows[1].Code == "S.509a");
            check("C120b", "auxiliary uses its own catalog group title and five metre content", rows[0].Description == "Cáp điện cao thế" && rows[1].Description == "Chiều cao an toàn" && rows[1].SignGroup == "Biển phụ" && rows[1].FaceContent.Contains("5 m") && rows[1].FaceContent.Contains("AN TOÀN"));
            check("C120c", "both faces retain station route offset segment package and object ID", rows.All(r => r.Chainage == "Km48+530.05" && r.Route == "TIMRANHD8" && r.Offset == "1.080" && r.Segment == "DOAN08" && r.Package == "GOI03" && r.ObjectId == "OBJ-0"));
            check("C120d", "two faces one installation without multiplying poles", SignReportWorkbook.InstallationCount(rows) == 1 && rows[0].PoleCount == "2" && rows[1].PoleCount == "" && rows[0].FaceCount == "2" && rows[1].FaceCount == "" && rows[0].FacePosition == "1/2" && rows[1].FacePosition == "2/2");
            check("C120e", "export is read-only", original.ToCanonical() == before);
            var duplicates = Build(Record("W.239a", "W.239a", "W.239a", "S.509a@6"));
            check("C120f", "identical physical plates are not deduplicated", duplicates.Count == 3 && duplicates.Count(r => r.Code == "W.239a") == 2);
            var legacy = Build(Record("P.127").Add("so_mat", "2").Add("mo_ta", "Tốc độ 50"));
            check("C120g", "legacy face count follows CAD fallback and speed description", legacy.Count == 2 && legacy.All(r => r.Code == "P.127-50" && r.FaceContent == "50 km/h"));
            var listFirst = Build(Record("W.239a", "S.509a@6"));
            check("C120h", "explicit face list remains authoritative as on CAD", listFirst.Count == 1 && listFirst[0].Code == "S.509a" && listFirst[0].FaceContent == "6 m");
            var unknown = Build(Record("", "UNKNOWN-A", "UNKNOWN-B"));
            check("C120i", "missing catalog never drops faces", unknown.Count == 2 && unknown[1].Code == "UNKNOWN-B");
            var custom = new SignContentFace { Index = 1, Code = "BHT.TEXT.X" };
            custom.Values["custom_name"] = "Biển địa danh"; custom.Values["custom_text"] = "ĐỊA PHẬN XÃ ĐỨC HÒA";
            var customRows = Build(Record("W.239a", "W.239a", "BHT.TEXT.X").Add("sign_content", SignContent.Write(new[] { custom })));
            check("C120j", "custom auxiliary keeps its Unicode name and content without raw XML", customRows[1].Description == "Biển địa danh" && customRows[1].FaceContent == "ĐỊA PHẬN XÃ ĐỨC HÒA" && !customRows.Any(r => r.Description.Contains("<sign-content")));
            var malformed = Build(Record("W.239a", "W.239a", "S.509a").Add("sign_content", "<broken>"));
            check("C120k", "invalid content retains all faces and flags review", malformed.Count == 2 && malformed.All(r => r.Note.Contains("không đọc được")) && !malformed.Any(r => r.Description.Contains("<")));
            var stale = new SignContentFace { Index = 0, Code = "S.509a@5" }; stale.Values["NAME#0"] = "WRONG FACE";
            check("C120l", "content cannot leak across face index", !Build(Record("W.239a", "W.239a", "S.509a@5").Add("sign_content", SignContent.Write(new[] { stale }))).Any(r => r.FaceContent.Contains("WRONG FACE")));
            check("C120m", "empty and uncoded installations retained without invented codes", Build(Record("")).Count == 1 && Build(Record(""))[0].Code == "" && SignReportWorkbook.InstallationCount(new List<SignReportRow>()) == 0);
            var overrideFace = new SignContentFace { Index = 1, Code = "S.509a@5" };
            overrideFace.Values["DESC_3#0"] = "4.75 m";
            check("C120t", "saved editable clearance overrides default as on actual CAD face", Build(Record("W.239a", "W.239a", "S.509a@5").Add("sign_content", SignContent.Write(new[] { overrideFace })))[1].FaceContent == "4.75 m");

            string path = Path.Combine(Path.GetTempPath(), "bht120-" + Guid.NewGuid().ToString("N") + ".xlsx");
            try
            {
                var all = Build(original, Record("W.239a", "W.239a", "W.239a", "S.509a@6"), Record("S.509a", "S.509a"));
                SignReportWorkbook.Write(path, "Đoạn 8", all);
                using (var file = File.OpenRead(path))
                using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                {
                    XDocument detail, summary;
                    using (var stream = zip.GetEntry("xl/worksheets/sheet2.xml").Open()) detail = XDocument.Load(stream);
                    using (var stream = zip.GetEntry("xl/worksheets/sheet1.xml").Open()) summary = XDocument.Load(stream);
                    var data = detail.Descendants(Ns + "row").Where(r => (int)r.Attribute("r") >= 5).ToList();
                    var totals = summary.Descendants(Ns + "row").Where(r => (int)r.Attribute("r") >= 6).ToList();
                    check("C120n", "real workbook counts every primary auxiliary and duplicate face", data.Count == 6 && totals.Sum(r => int.Parse(Cell(r, "E"), CultureInfo.InvariantCulture)) == 6 && Cell(totals.Single(r => Cell(r, "B") == "W.239a" && Cell(r, "C") != "Cáp điện cao thế"), "E") == "2");
                    check("C120o", "workbook auxiliary parameters do not merge five and six metre signs", totals.Any(r => Cell(r, "B") == "S.509a" && Cell(r, "F").Contains("5 m")) && totals.Any(r => Cell(r, "B") == "S.509a" && Cell(r, "F") == "6 m"));
                    check("C120p", "workbook installation pole and face totals reconcile", data.Sum(r => Cell(r, "P") == "" ? 0 : int.Parse(Cell(r, "P"))) == 6 && data.Sum(r => Cell(r, "Q") == "" ? 0 : int.Parse(Cell(r, "Q"))) == 6 && detail.Root.Value.Contains("3 hồ sơ/cụm biển; 6 mặt biển"));
                    check("C120u", "pole and face totals are numeric Excel cells so SUM works", data[0].Elements(Ns + "c").Single(c => (string)c.Attribute("r") == "P5").Element(Ns + "v").Value == "2" && data[0].Elements(Ns + "c").Single(c => (string)c.Attribute("r") == "Q5").Element(Ns + "v").Value == "2");
                    check("C120q", "all auxiliary detail fields and filter columns present", Cell(data[1], "F") == "S.509a" && Cell(data[1], "T") == "OBJ-0" && Cell(data[1], "U") == "2/2" && Cell(data[1], "V").Contains("5 m") && (string)detail.Root.Element(Ns + "autoFilter").Attribute("ref") == "A4:V10");
                    check("C120r", "offset meaning is included in workbook", detail.Root.Value.Contains("Lệch tim tuyến (m)") && detail.Root.Value.Contains("trụ gần tim nhất") && detail.Root.Value.Contains("Không lấy vị trí ký hiệu trình bày"));
                }
                SignReportWorkbook.Write(path, "Rỗng", new List<SignReportRow>());
                using (var file = File.OpenRead(path))
                using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                using (var stream = zip.GetEntry("xl/worksheets/sheet2.xml").Open())
                    check("C120s", "empty export atomically replaces existing workbook", XDocument.Load(stream).Root.Value.Contains("0 hồ sơ/cụm biển; 0 mặt biển"));
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }
    }
}
