using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BHT.Core
{
    public enum CorrectionKind { Attributes, PointLink, Station, Assignment, Retire, DisplayGlobal }

    // Snapshots preserve repeated keys, order, newlines and Unicode without delimiters.
    public static class ObjectRevision
    {
        static readonly string[] AttributeFields = { "ma_hieu", "so_tru", "so_mat", "mo_ta", "tinh_trang", "ghi_chu" };
        static readonly string[] DisplayFields = { "custom_block", "sign_layout", "sign_gap", "sign_clearance", "sign_fill", "sign_heading_mode", "sign_heading_route", "sign_heading_angle", "marker_display_handle", "marker_display_direction", "marker_display_rotation" };
        public static bool Tracked(BhtRecord r) { return r != null && r.Get("revision_id") != ""; }
        public static bool Retired(BhtRecord r) { return r != null && r.Get("lifecycle") == "RETIRED"; }
        public static bool DisplayField(string key) { return key.StartsWith("kh_", StringComparison.Ordinal) || DisplayFields.Contains(key); }
        public static string Snapshot(BhtRecord r)
        {
            using (var stream = new MemoryStream()) {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
                    writer.Write(1); writer.Write(r.Count);
                    foreach (var pair in r.Pairs) { writer.Write(pair.Key); writer.Write(pair.Value); }
                }
                return Convert.ToBase64String(stream.ToArray());
            }
        }
        public static BhtRecord Restore(string value)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(value)))
            using (var reader = new BinaryReader(stream, Encoding.UTF8)) {
                if (reader.ReadInt32() != 1) throw new InvalidDataException("Unknown snapshot version.");
                int count = reader.ReadInt32();
                if (count < 0 || count > 100000) throw new InvalidDataException("Invalid snapshot size.");
                var r = new BhtRecord();
                for (int i = 0; i < count; i++) r.Add(reader.ReadString(), reader.ReadString());
                if (stream.Position != stream.Length) throw new InvalidDataException("Trailing snapshot data.");
                return r;
            }
        }
        public static string Hash(BhtRecord r)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Convert.FromBase64String(Snapshot(r)))).Replace("-", "");
        }
        public static BhtRecord Governed(BhtRecord r)
        {
            return new BhtRecord(r.Pairs.Where(p => !DisplayField(p.Key) && p.Key != "sua_luc"));
        }
        public static bool Same(BhtRecord a, BhtRecord b) { return a != null && b != null && Snapshot(a) == Snapshot(b); }
        public static bool OrdinaryWriteAllowed(BhtRecord old, BhtRecord next)
        {
            if (Retired(old)) return Same(old,next);
            return !Tracked(old) || next != null && Same(Governed(old), Governed(next));
        }
        public static BhtRecord Patch(BhtRecord before, BhtRecord fields, CorrectionKind kind)
        {
            if (!ProgressPolicy.Locked(before)) throw new InvalidOperationException("Chỉ hiệu chỉnh hồ sơ đã xác nhận.");
            if (Retired(before)) throw new InvalidOperationException("Hồ sơ đã ngừng sử dụng; giữ lịch sử, chưa hỗ trợ phục hồi hoạt động.");
            var allowed = kind == CorrectionKind.Attributes ? AttributeFields : kind == CorrectionKind.PointLink ? new[] { "pt", "doan" } : kind == CorrectionKind.Assignment ? new[] { "doan" } : new string[0];
            if (fields.Pairs.Any(p => !allowed.Contains(p.Key))) throw new InvalidOperationException("Trường sửa không thuộc loại hiệu chỉnh đã chọn.");
            var r = before.Clone();
            if (kind == CorrectionKind.Retire) r.Set("lifecycle", "RETIRED");
            foreach (string key in fields.Pairs.Select(p => p.Key).Distinct()) {
                if(key!="pt" && key!="mat" && fields.GetAll(key).Count!=1)throw new InvalidOperationException("Trường đơn có nhiều giá trị: "+key);
                if(key=="pt"||key=="mat")r.SetAll(key, fields.GetAll(key));else r.Set(key,fields.Get(key));
            }
            // A changed code must not leave an old face/content overriding the new symbol.
            if (kind == CorrectionKind.Attributes && fields.Has("ma_hieu") && fields.Get("ma_hieu") != before.Get("ma_hieu")) {
                if (before.GetAll("mat").Count > 1 || before.Get("sign_content") != "" || before.Get("custom_block") != "")
                    throw new InvalidOperationException("Biển nhiều mặt/nội dung tùy chỉnh cần quy trình hiệu chỉnh riêng; chưa hỗ trợ đổi mã tại đây.");
                if (before.GetAll("mat").Count == 1) r.SetAll("mat", new[] { fields.Get("ma_hieu") });
            }
            foreach (string key in new[] { "so_tru", "so_mat" }) {
                int n;
                if (fields.Has(key) && (!int.TryParse(fields.Get(key), out n) || n < (key=="so_tru"?0:1) || n > (key=="so_tru"?100:20)))
                    throw new InvalidOperationException("Số trụ từ 0 đến 100; số mặt từ 1 đến 20.");
            }
            if (fields.Has("ma_hieu") && string.IsNullOrWhiteSpace(fields.Get("ma_hieu"))) throw new InvalidOperationException("Mã biển không được để trống.");
            return r;
        }
        public static IEnumerable<string> Differences(BhtRecord a, BhtRecord b)
        {
            return a.Pairs.Select(p => p.Key).Union(b.Pairs.Select(p => p.Key)).Where(k => !a.GetAll(k).SequenceEqual(b.GetAll(k)));
        }
        static int Number(BhtRecord r, string key)
        {
            int n;
            if (!int.TryParse(r.Get(key), NumberStyles.None, CultureInfo.InvariantCulture, out n) || n < 0 || n == int.MaxValue) throw new InvalidDataException("Invalid revision counter: " + key);
            return n;
        }
        public static BhtRecord History(BhtRecord snapshot, string status, string project, string guid, string revision, string parent, string reason, string author, string time)
        {
            return new BhtRecord().Set("schema", "1").Set("project_id", project).Set("object_guid", guid).Set("object_id", snapshot.Get("object_id"))
                .Set("revision_id", revision).Set("parent_revision", parent).Set("revision_status", status).Set("revision_reason", reason).Set("revision_author", author)
                .Set("revision_time", time).Set("snapshot", Snapshot(snapshot)).Set("snapshot_sha256", Hash(snapshot));
        }
        public static BhtRecord ReadHistory(BhtRecord entry)
        {
            if (entry == null || entry.Get("schema") != "1") throw new InvalidDataException("Thiếu lịch sử phiên bản.");
            var r = Restore(entry.Get("snapshot"));
            if (Hash(r) != entry.Get("snapshot_sha256")) throw new InvalidDataException("Lịch sử không khớp mã kiểm tra.");
            return r;
        }
        public static BhtRecord Next(BhtRecord before, BhtRecord candidate, string project, string guid, string parent, string reason, string author, string time)
        {
            if (string.IsNullOrWhiteSpace(reason) || string.IsNullOrWhiteSpace(author)) throw new InvalidOperationException("Nhập lý do và người hiệu chỉnh.");
            var changed = Differences(before, candidate).ToArray();
            if (changed.Length == 0) throw new InvalidOperationException("Không có thay đổi để xác nhận.");
            bool assignment = changed.Any(k => new[] { "doan", "goi", "gan_doan_pp", "doan_ung_vien" }.Contains(k));
            bool display = changed.Any(k=>k=="presentation_pose");
            bool survey = changed.Any(k => k!="presentation_pose" && !DisplayField(k) && !new[] { "doan", "goi", "gan_doan_pp", "doan_ung_vien", "progress_context", "progress_route_state", "progress_route_signature", "progress_point" }.Contains(k));
            var r = candidate.Clone();
            r.Set("project_id", project).Set("object_guid", guid).Set("revision_id", Guid.NewGuid().ToString("D")).Set("parent_revision", parent)
                .Set("revision_status", Retired(r) ? "RETIRED" : "CONFIRMED_CORRECTION").Set("revision_reason", reason.Trim()).Set("revision_author", author.Trim()).Set("revision_time", time)
                .Set("revision_scope", survey ? (assignment ? "SURVEY_ASSIGNMENT" : "SURVEY") : display ? "DISPLAY" : "ASSIGNMENT");
            foreach (var key in new[] { "object_revision", "survey_revision", "display_revision", "assignment_revision" }) {
                int n = Tracked(before) ? Number(before, key) : 0;
                bool add = key == "object_revision" || key == "survey_revision" && survey || key == "assignment_revision" && assignment || key=="display_revision"&&display;
                r.Set(key, (n + (add ? 1 : 0)).ToString(CultureInfo.InvariantCulture));
            }
            return r;
        }
    }
}
