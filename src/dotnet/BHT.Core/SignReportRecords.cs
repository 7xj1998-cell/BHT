using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BHT.Core
{
    public static class SignReportRecords
    {
        public static bool Includes(BhtRecord record)
        {
            if (ObjectRevision.Retired(record)) return false;
            string group = record.Get(ObjFields.Group).Trim().ToUpperInvariant();
            return new[] { "BIEN_BAO", "BANG_CHI_DAN", "BANG_QC", "KHAC", "CHUA_XAC_DINH" }.Contains(group);
        }

        // One row per physical face, in the same order and with the same legacy
        // count fallback as bht:kh-sign-codes. Repeated codes are separate plates.
        public static List<SignReportRow> Build(IEnumerable<KeyValuePair<string, BhtRecord>> source,
            string project, Func<string, SignReportRow> catalog)
        {
            var rows = new List<SignReportRow>();
            foreach (var pair in source.Where(p => Includes(p.Value))
                .OrderBy(p => Station(p.Value.Get(ObjFields.ChainageM)))
                .ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            {
                var r = pair.Value;
                string group = r.Get(ObjFields.Group).Trim().ToUpperInvariant();
                var codes = r.GetAll(ObjFields.Face).Select(c => c.Trim()).Where(c => c != "").ToList();
                if (codes.Count == 0) codes.Add(SignPresentation.ResolveCode(r.Get(ObjFields.Code).Trim(), r.Get(ObjFields.Desc)));
                int count;
                if (int.TryParse(r.Get(ObjFields.FaceCount), out count))
                    while (codes.Count < Math.Min(20, count)) codes.Add(codes[0]);
                string rawContent = r.Get(ObjFields.SignContent), contentError = "";
                var content = new List<SignContentFace>();
                if (rawContent.TrimStart().StartsWith("<", StringComparison.Ordinal))
                {
                    try { content = SignContent.Read(rawContent); }
                    catch (Exception ex)
                    {
                        if (!(ex is ArgumentException) && !(ex is System.Xml.XmlException)) throw;
                        contentError = "Nội dung mặt biển không đọc được; kiểm tra lại hồ sơ.";
                    }
                }
                for (int index = 0; index < codes.Count; index++)
                {
                    string code = codes[index];
                    var sign = catalog == null || string.IsNullOrWhiteSpace(code) ? null : catalog(code);
                    var face = content.FirstOrDefault(f => f.Index == index && f.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                    string title = index == 0 ? r.Get(ObjFields.Desc) : "";
                    string customName;
                    if (string.IsNullOrWhiteSpace(title) && face != null && face.Values.TryGetValue("custom_name", out customName)) title = customName;
                    if (string.IsNullOrWhiteSpace(title) && index == 0 && !rawContent.TrimStart().StartsWith("<", StringComparison.Ordinal)) title = rawContent;
                    if (string.IsNullOrWhiteSpace(title) && sign != null) title = sign.Description;
                    if (string.IsNullOrWhiteSpace(title)) title = Groups.Label(group);
                    rows.Add(new SignReportRow
                    {
                        Number = rows.Count + 1, Project = project, ObjectId = pair.Key,
                        Segment = r.Get(ObjFields.Segment), Package = r.Get(ObjFields.Package),
                        SignGroup = group == "BIEN_BAO" && sign != null && !string.IsNullOrWhiteSpace(sign.SignGroup)
                            ? sign.SignGroup : Groups.Label(group),
                        Code = SignPresentation.BaseCode(code), Description = title,
                        Side = Side(r.Get(ObjFields.RoadSide) == "" ? r.Get(ObjFields.RouteSide) : r.Get(ObjFields.RoadSide)),
                        Chainage = r.Get(ObjFields.ChainageKm), Route = r.Get(ObjFields.RouteId), Offset = r.Get(ObjFields.OffsetM),
                        StationSource = r.Get(ObjFields.KmSource),
                        StationStatus = r.Get(ObjFields.StationStatus) == "" ? r.Get(ObjFields.KmState) : r.Get(ObjFields.StationStatus),
                        RouteRevision = r.Get(ObjFields.StationRouteRevision), Condition = r.Get(ObjFields.Condition),
                        // Installation totals appear once so summing columns cannot multiply poles.
                        PoleCount = index == 0 ? r.Get(ObjFields.PoleCount) : "",
                        FaceCount = index == 0 ? codes.Count.ToString(CultureInfo.InvariantCulture) : "",
                        FacePosition = (index + 1).ToString(CultureInfo.InvariantCulture) + "/" + codes.Count.ToString(CultureInfo.InvariantCulture),
                        FaceContent = FaceText(code, face),
                        Checked = r.Get(ObjFields.CheckState) == "DA_KIEM_TRA" ? "Đã kiểm tra" : "Chưa kiểm tra",
                        Note = r.Get(ObjFields.Note) + (contentError == "" ? "" : "\n" + contentError)
                    });
                }
            }
            return rows;
        }

        private static string FaceText(string code, SignContentFace face)
        {
            var values = new List<string>();
            string metres = SignPresentation.MetreValue(code);
            if (metres == "") metres = SignPresentation.MetreDefault(code);
            // S.509a retains the legacy editable DESC_3 field. Its saved text
            // overrides the code/default when CAD creates the actual face.
            if (face != null && SignPresentation.BaseCode(code).Equals("S.509a", StringComparison.OrdinalIgnoreCase))
            {
                var saved = face.Values.FirstOrDefault(p => p.Key.Equals("DESC_3#0", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(saved.Value)) metres = "";
            }
            if (metres != "") values.Add(metres + " m");
            string weight = SignPresentation.WeightValue(code);
            if (weight != "") values.Add(weight + " t");
            var speed = SignPresentation.Speed(code, "");
            if (speed.HasValue) values.Add(speed.Value.ToString(CultureInfo.InvariantCulture) + " km/h");
            string hours = SignPresentation.ZoneTime(code);
            if (hours != "") values.Add(hours);
            if (face != null && SignPresentation.BaseCode(code).Equals("I.449", StringComparison.OrdinalIgnoreCase)
                && face.Values.ContainsKey("NAME#0") && !face.Values.ContainsKey("HEADER#0")) values.Add("ĐƯỜNG");
            if (face != null) values.AddRange(face.Values.Where(p => p.Key != "custom_name")
                .OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => p.Value).Where(v => !string.IsNullOrWhiteSpace(v)));
            return string.Join("\n", values.Distinct(StringComparer.Ordinal).ToArray());
        }

        private static double Station(string text)
        {
            double value;
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                && !double.IsNaN(value) && !double.IsInfinity(value) ? value : double.MaxValue;
        }

        private static string Side(string text)
        {
            switch (text.ToUpperInvariant())
            {
                case "TRAI": return "Trái";
                case "PHAI": return "Phải";
                case "HAI_BEN": return "Hai bên";
                case "TREN_TUYEN": return "Trên tuyến";
                default: return "Chưa xác định";
            }
        }
    }
}
