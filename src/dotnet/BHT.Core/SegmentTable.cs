using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace BHT.Core
{
    public sealed class SegmentImport
    {
        public string Sheet = "";
        public List<BhtRecord> Rows = new List<BhtRecord>();
        public List<string> Notes = new List<string>();
    }

    // Portable table only. Drawing geometry stays in the .bht project / DWG.
    public static class SegmentTable
    {
        public const int MaxRows = 10000;
        const int MaxXml = 16 * 1024 * 1024;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        static readonly XNamespace S = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        static readonly string[] Columns = { "segment_id", "package_id", "km_start_m", "km_end_m", "side", "route_id", "package_name", "note" };

        public static string Text(string value) { return (value ?? "").Trim().Normalize(NormalizationForm.FormC); }
        public static string Key(string value)
        {
            var b = new StringBuilder();
            foreach (char c in Text(value).ToLowerInvariant().Normalize(NormalizationForm.FormD))
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c)) b.Append(c == 'đ' ? 'd' : c);
            return b.ToString();
        }
        public static bool ValidId(string value) { return Regex.IsMatch(value ?? "", @"^[A-Z0-9][A-Z0-9_.-]{0,79}$"); }
        public static double Metres(string value)
        {
            string t = Text(value).Replace(" ", "").Replace(",", ".");
            var km = Regex.Match(t, @"^(?:km)?(\d+)\+(\d+(?:\.\d+)?)$", RegexOptions.IgnoreCase);
            double result;
            if (km.Success)
            {
                double tail = double.Parse(km.Groups[2].Value, Inv);
                if (tail >= 1000) throw new InvalidDataException("Phần mét sau dấu + phải nhỏ hơn 1000: " + value);
                result = double.Parse(km.Groups[1].Value, Inv) * 1000 + tail;
            }
            else if (!double.TryParse(t, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, Inv, out result))
                throw new InvalidDataException("Lý trình không hợp lệ: " + value);
            if (double.IsNaN(result) || double.IsInfinity(result) || result < 0 || result > 100000000) throw new InvalidDataException("Lý trình ngoài giới hạn: " + value);
            return result;
        }
        public static string Side(string value)
        {
            string k = Key(value);
            if (k == "trai" || k == "left") return "TRAI";
            if (k == "phai" || k == "right") return "PHAI";
            if (k == "haiben" || k == "both") return "HAI_BEN";
            throw new InvalidDataException("Phía phải là TRAI, PHAI hoặc HAI_BEN: " + value);
        }
        public static List<BhtRecord> Validate(IEnumerable<BhtRecord> source)
        {
            var rows = source.Select(r => r == null ? null : r.Clone()).ToList();
            if (rows.Count == 0 || rows.Count > MaxRows) throw new InvalidDataException("Bảng phải có từ 1 đến " + MaxRows + " đoạn.");
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                string where = "Dòng " + (i + 1) + (r != null && r.Get("source_row") != "" ? " (Excel " + r.Get("source_sheet") + ", hàng " + r.Get("source_row") + ")" : "");
                try
                {
                    if (r == null || r.Count > 128) throw new InvalidDataException("Bản ghi rỗng hoặc quá nhiều trường.");
                    if (r.Pairs.GroupBy(p => p.Key).Any(g => g.Count() > 1)) throw new InvalidDataException("Trùng trường dữ liệu.");
                    foreach (var p in r.Pairs.ToArray())
                    {
                        if (!Regex.IsMatch(p.Key, @"^[a-z][a-z0-9_]{0,63}$") || p.Value.Length > 4096) throw new InvalidDataException("Trường dữ liệu không hợp lệ.");
                        r.Set(p.Key, Text(p.Value));
                    }
                    foreach (string k in new[] { "segment_id", "package_id", "route_id" }) r.Set(k, r.Get(k).ToUpperInvariant());
                    if (!ValidId(r.Get("segment_id")) || !ids.Add(r.Get("segment_id"))) throw new InvalidDataException("Mã đoạn sai hoặc trùng: " + r.Get("segment_id"));
                    foreach (string k in new[] { "package_id", "route_id" }) if (r.Get(k) != "" && !ValidId(r.Get(k))) throw new InvalidDataException("Mã " + k + " không hợp lệ.");
                    double a = Metres(r.Get("km_start_m")), b = Metres(r.Get("km_end_m"));
                    if (b <= a) throw new InvalidDataException("Lý trình cuối phải lớn hơn lý trình đầu.");
                    r.Set("km_start_m", a.ToString("0.#########", Inv)).Set("km_end_m", b.ToString("0.#########", Inv));
                    r.Set("side", Side(r.Get("side")));
                }
                catch (Exception ex) { throw new InvalidDataException(where + ": " + ex.Message, ex); }
            }
            return rows;
        }

        static byte[] Bytes(XElement xml) { using (var m = new MemoryStream()) { xml.Save(m); return m.ToArray(); } }
        static string Hash(byte[] data) { using (var h = SHA256.Create()) return BitConverter.ToString(h.ComputeHash(data)).Replace("-", ""); }
        static XElement Xml(Stream stream, int limit)
        {
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = limit })) return XElement.Load(reader);
        }
        static XElement Part(ZipArchive zip, string path)
        {
            var e = zip.GetEntry(path);
            if (e == null || e.Length > MaxXml) throw new InvalidDataException("Thiếu hoặc quá lớn: " + path);
            using (var input = e.Open()) return Xml(input, MaxXml);
        }
        static void Add(ZipArchive zip, string path, byte[] data) { using (var s = zip.CreateEntry(path, CompressionLevel.Optimal).Open()) s.Write(data, 0, data.Length); }
        static void Atomic(string path, Action<string> write)
        {
            string dest = Path.GetFullPath(path), temp = dest + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try { write(temp); if (File.Exists(dest)) File.Replace(temp, dest, null); else File.Move(temp, dest); }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
        public static void Write(string path, IEnumerable<BhtRecord> source)
        {
            var rows = Validate(source);
            byte[] data = Bytes(new XElement("Segments", rows.Select(r => new XElement("Segment", r.Pairs.Select(p => new XElement("Field", new XAttribute("name", p.Key), p.Value))))));
            byte[] manifest = Bytes(new XElement("BHTSegments", new XAttribute("format", "1"), new XAttribute("count", rows.Count), new XAttribute("bytes", data.Length), new XAttribute("sha256", Hash(data))));
            Atomic(path, temp => { using (var f = File.Create(temp)) using (var zip = new ZipArchive(f, ZipArchiveMode.Create)) { Add(zip, "manifest.xml", manifest); Add(zip, "segments.xml", data); } Read(temp); });
        }
        public static List<BhtRecord> Read(string path)
        {
            using (var f = File.OpenRead(path)) using (var zip = new ZipArchive(f, ZipArchiveMode.Read))
            {
                if (zip.Entries.Count != 2 || zip.Entries.Count(e => e.FullName == "manifest.xml") != 1 || zip.Entries.Count(e => e.FullName == "segments.xml") != 1) throw new InvalidDataException("Không đúng cấu trúc .bhtseg.");
                var m = Part(zip, "manifest.xml");
                if (m.Name != "BHTSegments" || (string)m.Attribute("format") != "1") throw new InvalidDataException("Phiên bản .bhtseg chưa được hỗ trợ.");
                var entry = zip.GetEntry("segments.xml");
                if (entry.Length > MaxXml || entry.Length.ToString(Inv) != (string)m.Attribute("bytes")) throw new InvalidDataException("Sai dung lượng dữ liệu .bhtseg.");
                byte[] data;
                using (var input = entry.Open()) using (var output = new MemoryStream()) { input.CopyTo(output); data = output.ToArray(); }
                if (Hash(data) != (string)m.Attribute("sha256")) throw new InvalidDataException("Tệp .bhtseg bị hỏng: SHA256 không khớp.");
                XElement xml; using (var input = new MemoryStream(data)) xml = Xml(input, MaxXml);
                if (xml.Name != "Segments" || xml.Elements().Any(e => e.Name != "Segment")) throw new InvalidDataException("Sai cấu trúc đoạn.");
                var rows = xml.Elements("Segment").Select(e => new BhtRecord(e.Elements().Select(p => {
                    if (p.Name != "Field" || p.HasElements) throw new InvalidDataException("Sai cấu trúc trường.");
                    return new KeyValuePair<string, string>((string)p.Attribute("name") ?? "", p.Value);
                }))).ToList();
                if (rows.Count.ToString(Inv) != (string)m.Attribute("count")) throw new InvalidDataException("Sai số đoạn .bhtseg.");
                return Validate(rows);
            }
        }

        static string Header(string value)
        {
            switch (Key(value))
            {
                case "segmentid": case "madoan": case "doan": return "segment_id";
                case "packageid": case "magoi": case "goithau": return "package_id";
                case "kmstartm": case "lytrinhdau": case "kmdau": return "km_start_m";
                case "kmendm": case "lytrinhcuoi": case "kmcuoi": return "km_end_m";
                case "side": case "phia": case "vitri": return "side";
                case "routeid": case "matuyen": case "timtuyen": return "route_id";
                case "packagename": case "tengoi": return "package_name";
                case "note": case "ghichu": return "note";
                default: return "";
            }
        }
        sealed class Sheet
        {
            public string Name;
            public SortedDictionary<int, Dictionary<int, string>> Rows = new SortedDictionary<int, Dictionary<int, string>>();
            public string Cell(int row, int col) { Dictionary<int, string> cells; string value; return Rows.TryGetValue(row, out cells) && cells.TryGetValue(col, out value) ? value : ""; }
        }
        static int Col(string cell)
        {
            int n = 0; foreach (char c in cell.ToUpperInvariant()) { if (c < 'A' || c > 'Z') break; n = checked(n * 26 + c - 'A' + 1); } return n;
        }
        static string Strings(XElement cell) { return string.Concat(cell.Descendants(S + "t").Select(t => t.Value)); }
        static Sheet LoadSheet(ZipArchive zip, string name, string path, List<string> shared)
        {
            var xml = Part(zip, path); var sheet = new Sheet { Name = name };
            int cellCount = 0;
            foreach (var row in xml.Descendants(S + "sheetData").Elements(S + "row"))
            {
                int n; if (!int.TryParse((string)row.Attribute("r"), out n) || n < 1 || n > 100000) throw new InvalidDataException("Số hàng Excel không hợp lệ.");
                var cells = new Dictionary<int, string>(); sheet.Rows.Add(n, cells);
                foreach (var cell in row.Elements(S + "c"))
                {
                    if (++cellCount > 200000) throw new InvalidDataException("Trang tính vượt 200.000 ô; chỉ nạp bảng phân đoạn.");
                    string t = (string)cell.Attribute("t"), v = (string)cell.Element(S + "v") ?? "";
                    if (t == "s") { int idx; if (!int.TryParse(v, out idx) || idx < 0 || idx >= shared.Count) throw new InvalidDataException("Chuỗi Excel không hợp lệ."); v = shared[idx]; }
                    else if (t == "inlineStr") v = Strings(cell);
                    else if (t == "e") v = "#LOI_EXCEL:" + v;
                    if (cell.Element(S + "f") != null && cell.Element(S + "v") == null) v = "#CONG_THUC_CHUA_TINH";
                    int c = Col((string)cell.Attribute("r") ?? ""); if (c < 1 || c > 16384) throw new InvalidDataException("Cột Excel không hợp lệ.");
                    cells.Add(c, Text(v));
                }
            }
            // Expand only the package name column. Never fill missing stations from merged cells.
            foreach (var merge in xml.Descendants(S + "mergeCell"))
            {
                var match = Regex.Match((string)merge.Attribute("ref") ?? "", @"^([A-Z]+)(\d+):([A-Z]+)(\d+)$");
                if (!match.Success || match.Groups[1].Value != "M" || match.Groups[3].Value != "M") continue;
                int a = int.Parse(match.Groups[2].Value, Inv), b = int.Parse(match.Groups[4].Value, Inv);
                if (b < a || b - a > MaxRows) throw new InvalidDataException("Vùng gộp gói thầu quá lớn.");
                string value = sheet.Cell(a, 13);
                for (int r = a + 1; r <= b; r++) if (sheet.Rows.ContainsKey(r) && sheet.Cell(r, 13) == "") sheet.Rows[r][13] = value;
            }
            return sheet;
        }
        static SegmentImport Parse(Sheet sheet)
        {
            var result = new SegmentImport { Sheet = sheet.Name };
            var header = new Dictionary<int, string>(); int headRow = 0; bool split = false;
            foreach (var row in sheet.Rows.Where(r => r.Key <= 50))
            {
                var h = row.Value.Select(c => new { c.Key, Field = Header(c.Value) }).Where(c => c.Field != "").ToList();
                if (h.Any(c => c.Field == "segment_id") && h.Any(c => c.Field == "km_start_m") && h.Any(c => c.Field == "km_end_m"))
                {
                    if (h.GroupBy(c => c.Field).Any(g => g.Count() > 1)) throw new InvalidDataException(sheet.Name + ": trùng tên cột.");
                    header = h.ToDictionary(c => c.Key, c => c.Field); headRow = row.Key; break;
                }
                if (Key(sheet.Cell(row.Key, 1)) == "doan" && Key(sheet.Cell(row.Key, 2)) == "lytrinh" && Key(sheet.Cell(row.Key, 10)) == "vitri") { split = true; headRow = row.Key; break; }
            }
            if (headRow == 0) return null;
            var packages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in sheet.Rows.Where(r => r.Key > headRow))
            {
                if (row.Value.Values.All(string.IsNullOrWhiteSpace)) continue;
                var rec = new BhtRecord().Add("source_sheet", sheet.Name).Add("source_row", row.Key.ToString(Inv));
                if (split)
                {
                    string id = sheet.Cell(row.Key, 1), start = sheet.Cell(row.Key, 2) + sheet.Cell(row.Key, 3), end = sheet.Cell(row.Key, 5) + sheet.Cell(row.Key, 6);
                    if (id == "" && start == "" && end == "") continue; // totals / comments outside the table
                    if (id == "" || !Regex.IsMatch(id, @"^\d+$")) throw new InvalidDataException(sheet.Name + ", hàng " + row.Key + ": thiếu hoặc sai số đoạn.");
                    string label = sheet.Cell(row.Key, 13), package;
                    if (label == "") throw new InvalidDataException(sheet.Name + ", hàng " + row.Key + ": thiếu tên nhóm gói thầu ở cột M.");
                    if (!packages.TryGetValue(label, out package)) { package = "GOI_" + (packages.Count + 1).ToString("00", Inv); packages.Add(label, package); }
                    rec.Add("segment_id", "DOAN_" + id.PadLeft(2, '0')).Add("package_id", package).Add("package_name", label)
                        .Add("km_start_m", start).Add("km_end_m", end).Add("side", sheet.Cell(row.Key, 10))
                        .Add("commune", sheet.Cell(row.Key, 11)).Add("survey_traverse", sheet.Cell(row.Key, 14));
                }
                else
                {
                    foreach (var h in header) rec.Add(h.Value, sheet.Cell(row.Key, h.Key));
                    if (header.All(h => sheet.Cell(row.Key, h.Key) == "")) continue;
                }
                result.Rows.Add(rec);
            }
            result.Rows = Validate(result.Rows);
            if (split) result.Notes.Add("Bảng không có mã gói: BHT tạo " + string.Join(", ", packages.Select(p => p.Value + " = " + p.Key).ToArray()) + ". Kiểm tra hoặc sửa trước khi lưu.");
            result.Notes.Add("Giữ nguyên lý trình đầu/cuối của Excel; không suy ngược từ cột chiều dài.");
            return result;
        }
        public static List<SegmentImport> ReadExcel(string path)
        {
            using (var file = File.OpenRead(path)) using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                if (zip.Entries.Count > 5000) throw new InvalidDataException("Workbook quá nhiều thành phần.");
                var shared = zip.GetEntry("xl/sharedStrings.xml") == null ? new List<string>() : Part(zip, "xl/sharedStrings.xml").Elements(S + "si").Select(Strings).ToList();
                var book = Part(zip, "xl/workbook.xml"); var rel = Part(zip, "xl/_rels/workbook.xml.rels");
                XNamespace r = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
                var results = new List<SegmentImport>();
                foreach (var sheet in book.Descendants(S + "sheet"))
                {
                    string id = (string)sheet.Attribute(r + "id");
                    var link = rel.Elements().FirstOrDefault(e => (string)e.Attribute("Id") == id);
                    if (link == null || (string)link.Attribute("TargetMode") == "External") throw new InvalidDataException("Liên kết trang tính không hợp lệ.");
                    string target = (string)link.Attribute("Target") ?? "";
                    string part = new Uri(new Uri("http://bht/xl/workbook.xml"), target).AbsolutePath.TrimStart('/');
                    var parsed = Parse(LoadSheet(zip, (string)sheet.Attribute("name") ?? "", part, shared));
                    if (parsed != null) results.Add(parsed);
                }
                if (results.Count == 0) throw new InvalidDataException("Không nhận diện được bảng. Dùng mẫu XLSX của BHT với cột SEGMENT_ID, KM_START_M, KM_END_M, SIDE; hoặc bảng Đoạn / Lý trình / Vị trí hiện có.");
                return results;
            }
        }
        // Migration only; never used for new saves.
        public static List<BhtRecord> ReadLegacy(string path)
        {
            var lines = File.ReadAllLines(path, new UTF8Encoding(false, true));
            if (lines.Length < 2 || lines.Length > MaxRows + 1) throw new InvalidDataException("Bảng TSV cũ rỗng hoặc quá lớn.");
            var header = lines[0].TrimStart('\uFEFF').Split('\t').Select(h => Text(h).ToLowerInvariant()).ToArray();
            if (!new[] { "segment_id", "km_start_m", "km_end_m", "side" }.All(header.Contains) || header.Distinct().Count() != header.Length) throw new InvalidDataException("Bảng TSV cũ thiếu hoặc trùng cột.");
            var rows = new List<BhtRecord>();
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var cells = lines[i].Split('\t');
                if (cells.Length != header.Length) throw new InvalidDataException("TSV hàng " + (i + 1) + ": sai số cột.");
                rows.Add(new BhtRecord(header.Select((h, j) => new KeyValuePair<string, string>(h, cells[j]))).Set("source_row", (i + 1).ToString(Inv)));
            }
            return Validate(rows);
        }
        public static void WriteTemplate(string path)
        {
            Atomic(path, temp => {
                using (var f = File.Create(temp)) using (var zip = new ZipArchive(f, ZipArchiveMode.Create))
                {
                    XNamespace ct = "http://schemas.openxmlformats.org/package/2006/content-types", pr = "http://schemas.openxmlformats.org/package/2006/relationships", r = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
                    Add(zip, "[Content_Types].xml", Bytes(new XElement(ct + "Types", new XElement(ct + "Default", new XAttribute("Extension", "rels"), new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")), new XElement(ct + "Default", new XAttribute("Extension", "xml"), new XAttribute("ContentType", "application/xml")), new XElement(ct + "Override", new XAttribute("PartName", "/xl/workbook.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")), new XElement(ct + "Override", new XAttribute("PartName", "/xl/worksheets/sheet1.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")))));
                    Add(zip, "_rels/.rels", Bytes(new XElement(pr + "Relationships", new XElement(pr + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", r.NamespaceName + "/officeDocument"), new XAttribute("Target", "xl/workbook.xml")))));
                    Add(zip, "xl/workbook.xml", Bytes(new XElement(S + "workbook", new XAttribute(XNamespace.Xmlns + "r", r), new XElement(S + "sheets", new XElement(S + "sheet", new XAttribute("name", "Doan goi thau"), new XAttribute("sheetId", "1"), new XAttribute(r + "id", "rId1"))))));
                    Add(zip, "xl/_rels/workbook.xml.rels", Bytes(new XElement(pr + "Relationships", new XElement(pr + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", r.NamespaceName + "/worksheet"), new XAttribute("Target", "worksheets/sheet1.xml")))));
                    var values = new[] { Columns.Select(c => c.ToUpperInvariant()).ToArray(), new[] { "DOAN_01", "GOI_01", "Km40+000", "Km41+000", "HAI_BEN", "", "Tên gói thầu", "Chọn tim đã khai báo trong BHT" } };
                    Add(zip, "xl/worksheets/sheet1.xml", Bytes(new XElement(S + "worksheet", new XElement(S + "cols", new XElement(S + "col", new XAttribute("min", 1), new XAttribute("max", 8), new XAttribute("width", 24), new XAttribute("customWidth", 1))), new XElement(S + "sheetData", values.Select((row, i) => new XElement(S + "row", new XAttribute("r", i + 1), row.Select((v, j) => new XElement(S + "c", new XAttribute("r", ((char)('A' + j)).ToString() + (i + 1)), new XAttribute("t", "inlineStr"), new XElement(S + "is", new XElement(S + "t", v))))))))));
                }
            });
        }
    }
}
