using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BHT.Core;
namespace BHT.CoreTests
{
    public static class Segment119
    {
        static bool Reject(Action action) { try { action(); return false; } catch { return true; } }
        public static void Run(Action<string, string, bool> check)
        {
            string dir = Path.Combine(Path.GetTempPath(), "BHTSegmentTest-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(dir);
            try
            {
                string excel = Path.Combine(dir, "template.xlsx"), native = Path.Combine(dir, "table.bhtseg");
                SegmentTable.WriteTemplate(excel);
                var import = SegmentTable.ReadExcel(excel).Single(); var row = import.Rows.Single().Set("route_id", "TIM_RANH_1").Set("note", "Đoạn\tA\nRãnh = kiểm tra");
                check("SEG119-01", "template XLSX roundtrip actual headers and station values", row.Get("km_start_m") == "40000" && row.Get("km_end_m") == "41000" && row.Get("side") == "HAI_BEN");
                SegmentTable.Write(native, new[] { row });
                var reopened = SegmentTable.Read(native).Single();
                check("SEG119-02", "native Unicode and route retained; no TSV delimiters", reopened.Get("route_id") == "TIM_RANH_1" && reopened.Get("note") == row.Get("note") && reopened.Get("package_name") == "Tên gói thầu");
                byte[] before = File.ReadAllBytes(native);
                check("SEG119-03", "invalid overwrite leaves complete original", Reject(() => SegmentTable.Write(native, new[] { row.Clone().Set("km_end_m", "0") })) && before.SequenceEqual(File.ReadAllBytes(native)));
                check("SEG119-04", "duplicate IDs rejected", Reject(() => SegmentTable.Validate(new[] { row, row })));
                check("SEG119-05", "decimal comma and fractional Km", SegmentTable.Metres("Km48+469,10") == 48469.1 && SegmentTable.Metres("48469,10") == 48469.1);
                foreach (string s in new[] { "NaN", "Infinity", "-1", "Km48+1000", "48.469.10", "=1+1" }) check("SEG119-06-" + s, "invalid chainage rejected", Reject(() => SegmentTable.Metres(s)));
                check("SEG119-07", "invalid side never becomes both sides", Reject(() => SegmentTable.Validate(new[] { row.Clone().Set("side", "") })));
                using (var f = new FileStream(native, FileMode.Open, FileAccess.ReadWrite)) using (var zip = new ZipArchive(f, ZipArchiveMode.Update))
                {
                    XElement data; using (var s = zip.GetEntry("segments.xml").Open()) data = XElement.Load(s);
                    data.Descendants("Field").First(e => (string)e.Attribute("name") == "km_end_m").Value = "42000";
                    zip.GetEntry("segments.xml").Delete(); using (var s = zip.CreateEntry("segments.xml").Open()) data.Save(s);
                }
                check("SEG119-08", "native tampering rejected", Reject(() => SegmentTable.Read(native)));
                // Exercise the supplied real layout if available without embedding personal workbook data.
                string actual = Environment.GetEnvironmentVariable("BHT_SEGMENT_XLSX");
                if (!string.IsNullOrEmpty(actual))
                {
                    var t = SegmentTable.ReadExcel(actual).Single();
                    check("SEG119-09", "real workbook 15 segments three merged package groups", t.Rows.Count == 15 && t.Rows.Select(r => r.Get("package_id")).Distinct().Count() == 3);
                    check("SEG119-10", "split Km cells and side parsed correctly", t.Rows[0].Get("km_start_m") == "38622" && t.Rows[0].Get("km_end_m") == "38697" && t.Rows[14].Get("km_end_m") == "52618" && t.Rows[2].Get("side") == "PHAI");
                    check("SEG119-11", "merged group labels and generated IDs disclosed", t.Rows[4].Get("package_name") == t.Rows[0].Get("package_name") && t.Rows[5].Get("package_name") != t.Rows[4].Get("package_name") && t.Notes[0].Contains("GOI_01"));
                    SegmentTable.Write(native, t.Rows); check("SEG119-12", "real workbook converts to native and reopens", SegmentTable.Read(native).Count == 15);
                }
                // A malformed row must reject the entire import, including preceding valid rows.
                using (var f = new FileStream(excel, FileMode.Open, FileAccess.ReadWrite)) using (var zip = new ZipArchive(f, ZipArchiveMode.Update))
                {
                    XNamespace s = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"; XElement x;
                    using (var input = zip.GetEntry("xl/worksheets/sheet1.xml").Open()) x = XElement.Load(input);
                    var bad = new XElement(x.Descendants(s + "row").Last()); bad.SetAttributeValue("r", "3");
                    foreach (var c in bad.Elements(s + "c")) { string address = (string)c.Attribute("r"); c.SetAttributeValue("r", address.Replace("2", "3")); }
                    bad.Elements(s + "c").First().Descendants(s + "t").Single().Value = "DOAN_02";
                    bad.Elements(s + "c").First(c => (string)c.Attribute("r") == "C3").Descendants(s + "t").Single().Value = "BAD";
                    x.Element(s + "sheetData").Add(bad); zip.GetEntry("xl/worksheets/sheet1.xml").Delete(); using (var output = zip.CreateEntry("xl/worksheets/sheet1.xml").Open()) x.Save(output);
                }
                check("SEG119-13", "malformed Excel row rejects full workbook with row location", Reject(() => SegmentTable.ReadExcel(excel)));
            }
            finally { Directory.Delete(dir, true); }
        }
    }
}
