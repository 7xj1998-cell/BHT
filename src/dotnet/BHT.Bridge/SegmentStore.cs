using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.Core;
using BHT.Core;

[assembly: CommandClass(typeof(BHT.Bridge.SegmentStore))]
namespace BHT.Bridge
{
    public static class SegmentStore
    {
        public static List<BhtRecord> Read(Database db)
        {
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
                return BhtStore.All(tr, db, "SEG").Select(p => p.Value.Clone().Set("segment_id", p.Key)).ToList();
        }
        public static void Apply(Database db, IEnumerable<BhtRecord> source, string path, bool replace)
        {
            var rows = SegmentTable.Validate(source);
            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (var row in rows)
                    if (row.Get("route_id") != "" && BhtStore.Read(tr, db, "ROUTE", row.Get("route_id")) == null)
                        throw new InvalidOperationException("Đoạn " + row.Get("segment_id") + ": chưa có tim " + row.Get("route_id") + ". Khai báo hoặc chọn lại tim trước khi áp dụng.");
                var old = BhtStore.All(tr, db, "SEG").ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);
                var aliases=SegmentDuplicatePolicy.NewAliasConflicts(rows,old.Select(p=>p.Value.Clone().Set("segment_id",p.Key)));
                if(aliases.Count>0)throw new ImportConflictException(aliases);
                var keys = new HashSet<string>(rows.Select(r => r.Get("segment_id")), StringComparer.OrdinalIgnoreCase);
                foreach (var row in rows) BhtStore.Write(tr, db, "SEG", row.Get("segment_id"), row);
                if (replace) foreach (var key in BhtStore.Keys(tr, db, "SEG")) if (!keys.Contains(key)) BhtStore.Delete(tr, db, "SEG", key);
                // Preserve explicit decisions only when the segment's meaning did not change.
                foreach (var obj in BhtStore.All(tr, db, "OBJ"))
                {
                    var r = obj.Value;
                    BhtRecord previous; var next = BhtStore.Read(tr, db, "SEG", r.Get("doan"));
                    bool unchanged = old.TryGetValue(r.Get("doan"), out previous) && next != null && new[] { "route_id", "km_start_m", "km_end_m", "side", "package_id" }.All(k => previous.Get(k) == next.Get(k));
                    if (ProgressPolicy.Locked(r)) {
                        if (!unchanged) throw new InvalidOperationException("Đoạn " + r.Get("doan") + " đã có kết quả duyệt. Mở lại đoạn trước khi đổi tim/phạm vi.");
                        continue;
                    }
                    if (unchanged && r.Get("gan_doan_pp") == "THU_CONG") continue;
                    if (!unchanged && previous != null && previous.Get("route_id") != "" && r.Get("nguon_km") != "Số Km/H đã nhập")
                    {
                        foreach (string field in new[] { "ly_trinh_m", "ly_trinh_km", "offset_m", "phia_tuyen", "loai_tuyen", "station_route_revision" }) r.Set(field, "");
                        r.Set("trang_thai_km", "CAN_CAP_NHAT").Set("station_status", "STALE").Set("nguon_km", "Tim hoặc phạm vi đoạn đã đổi; cần phân lại đoạn.");
                    }
                    foreach (string field in new[] { "doan", "goi", "doan_ung_vien" }) r.Set(field, "");
                    r.Set("gan_doan_pp", "CAN_PHAN_LAI");
                    BhtStore.Write(tr, db, "OBJ", obj.Key, r);
                }
                BhtStore.MetaSet(tr, db, "goi_thau_nguon", path ?? "");
                BhtStore.MetaSet(tr, db, "segment_format", "BHTSEG/1");
                tr.Commit();
            }
        }
        [LispFunction("BHTSEGLOAD")]
        public static string LoadLisp(ResultBuffer args)
        {
            try
            {
                var a = args.AsArray(); string path = Convert.ToString(a[0].Value);
                var rows = System.IO.Path.GetExtension(path).Equals(".bhtseg", StringComparison.OrdinalIgnoreCase) ? SegmentTable.Read(path) : SegmentTable.ReadLegacy(path);
                Apply(Application.DocumentManager.MdiActiveDocument.Database, rows, path, a.Length < 2 || Convert.ToInt32(a[1].Value) != 0);
                return "OK|" + rows.Count;
            }
            catch (System.Exception ex) { return "LOI|" + ex.Message; }
        }
    }
}
