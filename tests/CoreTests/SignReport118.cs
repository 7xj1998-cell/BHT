using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using BHT.Core;

namespace BHT.CoreTests
{
    public static class SignReport118
    {
        public static void Run(Action<string, string, bool> check)
        {
            var records = new List<KeyValuePair<string, BhtRecord>>();
            foreach (string group in new[] { "BIEN_BAO", "BANG_CHI_DAN", "BANG_QC", "KHAC", "CHUA_XAC_DINH", "COC_TIEU", "COT_KM", "DEN_CS", "DEN_TH", "CONG_TRINH" })
                records.Add(new KeyValuePair<string, BhtRecord>(group, new BhtRecord().Add("nhom", group)));
            var board = records.First(p => p.Key == "KHAC").Value;
            board.Set("mo_ta", "ĐỊA PHẬN XÃ ĐỨC HÒA"); board.Set("ly_trinh_km", "Km48+525.03"); board.Set("ly_trinh_m", "48525.03");
            board.Set("so_tru", "2"); board.Set("phia_tuyen", "PHAI"); board.Set("route_id", "TDT");
            var warning = records[0].Value;
            warning.Set("ma_hieu", "W.239a"); warning.Set("ly_trinh_m", "48530.05"); warning.Set("so_mat", "2");
            warning.Add("mat", "W.239a").Add("mat", "S.509a");
            var rows = SignReportRecords.Build(records, "Đoạn 8", code => new SignReportRow { SignGroup = "Biển nguy hiểm", Description = "Đường cáp điện ở phía trên" });
            check("C118a", "include all five board groups and both faces; exclude markers lights structures", rows.Count == 6 && rows.Select(r => r.ObjectId).Distinct().Count() == 5);
            check("C118b", "named board retains title station poles side route and empty code", rows[0].ObjectId == "KHAC" && rows[0].Description == "ĐỊA PHẬN XÃ ĐỨC HÒA" && rows[0].Chainage == "Km48+525.03" && rows[0].PoleCount == "2" && rows[0].Side == "Phải" && rows[0].Route == "TDT" && rows[0].Code == "");
            check("C118c", "multi face installation exports both plates with shared ID", rows[1].Code == "W.239a" && rows[1].FaceCount == "2" && rows[1].SignGroup == "Biển nguy hiểm" && rows[1].Description == "Đường cáp điện ở phía trên" && rows[2].Code == "S.509a" && rows[2].ObjectId == rows[1].ObjectId);
            check("C118d", "unknown boards retained without invented station", rows.Last().Chainage == "" && rows.Any(r => r.SignGroup == "Chưa xác định"));
            string path = Path.Combine(Path.GetTempPath(), "bht118-" + Guid.NewGuid().ToString("N") + ".xlsx");
            try
            {
                // Two uncoded boards with the same name in different groups must not merge.
                rows.Add(new SignReportRow { Number = 7, SignGroup = "Bảng quảng cáo", Description = rows[0].Description });
                SignReportWorkbook.Write(path, "Đoạn 8", rows);
                using (var file = File.OpenRead(path))
                using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                {
                    XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                    XDocument detail, summary;
                    using (var stream = zip.GetEntry("xl/worksheets/sheet2.xml").Open()) detail = XDocument.Load(stream);
                    using (var stream = zip.GetEntry("xl/worksheets/sheet1.xml").Open()) summary = XDocument.Load(stream);
                    var data = detail.Descendants(ns + "row").Where(r => (int)r.Attribute("r") >= 5).ToList();
                    check("C118e", "XLSX detail includes board station and all faces", data.Count == 7 && data[0].Value.Contains("Km48+525.03") && data[0].Value.Contains("ĐỊA PHẬN XÃ ĐỨC HÒA"));
                    var totals = summary.Descendants(ns + "row").Where(r => (int)r.Attribute("r") >= 6).ToList();
                    check("C118f", "summary reconciles with detail and separates board groups", totals.Count == 7 && totals.Sum(r => int.Parse(r.Elements(ns + "c").Single(c => ((string)c.Attribute("r")).StartsWith("E")).Element(ns + "v").Value)) == data.Count);
                }
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }
    }
}
