using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using BHT.Core;

namespace BHT.Bridge
{
    /// <summary>
    /// Doc / ghi kho du lieu BHT trong DWG DUNG dinh dang Lisp:
    /// Named Object Dictionary "BHT_V02" -> dictionary con (META, DATASET, OBJ, PHOTO, ROUTE, SEG)
    /// -> XRECORD (280 . 1) + cac ma DXF 1 "khoa=gia tri" (xem RecordCodec).
    /// Moi ham chay trong Transaction cua nguoi goi.
    /// </summary>
    public static partial class BhtStore
    {
        public const string Root = "BHT_V02";
        public const string BackupSuffix = "__BHT_CU";

        public static DBDictionary RootDict(Transaction tr, Database db, bool create)
        {
            var nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
            if (nod.Contains(Root)) return (DBDictionary)tr.GetObject(nod.GetAt(Root), OpenMode.ForRead);
            if (!create) return null;
            nod.UpgradeOpen();
            var d = new DBDictionary();
            nod.SetAt(Root, d);
            tr.AddNewlyCreatedDBObject(d, true);
            return d;
        }

        public static DBDictionary SubDict(Transaction tr, Database db, string name, bool create)
        {
            var root = RootDict(tr, db, create);
            if (root == null) return null;
            if (root.Contains(name)) return (DBDictionary)tr.GetObject(root.GetAt(name), OpenMode.ForRead);
            if (!create) return null;
            root.UpgradeOpen();
            var d = new DBDictionary();
            root.SetAt(name, d);
            tr.AddNewlyCreatedDBObject(d, true);
            return d;
        }

        /// <summary>bht:rec-keys: cac khoa theo thu tu dictionary, bo ban sao luu "*__BHT_CU".</summary>
        public static List<string> Keys(Transaction tr, Database db, string sub)
        {
            var o = new List<string>();
            var d = SubDict(tr, db, sub, false);
            if (d == null) return o;
            foreach (DBDictionaryEntry e in d)
                if (!e.Key.EndsWith(BackupSuffix, StringComparison.OrdinalIgnoreCase)) o.Add(e.Key);
            return o;
        }

        public static BhtRecord ReadXrecord(Xrecord xr)
        {
            var strings = new List<string>();
            using (var data = xr.Data)
            if (data != null)
                foreach (TypedValue tv in data)
                    if (tv.TypeCode == 1) strings.Add(Convert.ToString(tv.Value));
            return RecordCodec.Decode(strings);
        }

        /// <summary>bht:rec-read: null neu khong co.</summary>
        public static BhtRecord Read(Transaction tr, Database db, string sub, string key)
        {
            var d = SubDict(tr, db, sub, false);
            if (d == null) return null;
            key = key.ToUpperInvariant();
            if (!d.Contains(key)) return null;
            var xr = tr.GetObject(d.GetAt(key), OpenMode.ForRead) as Xrecord;
            return xr == null ? null : ReadXrecord(xr);
        }

        /// <summary>bht:rec-all theo thu tu khoa.</summary>
        public static List<KeyValuePair<string, BhtRecord>> All(Transaction tr, Database db, string sub)
        {
            var o = new List<KeyValuePair<string, BhtRecord>>();
            var d = SubDict(tr, db, sub, false);
            if (d == null) return o;
            foreach (DBDictionaryEntry e in d)
            {
                if (e.Key.EndsWith(BackupSuffix, StringComparison.OrdinalIgnoreCase)) continue;
                var xr = tr.GetObject(e.Value, OpenMode.ForRead) as Xrecord;
                o.Add(new KeyValuePair<string, BhtRecord>(e.Key, xr == null ? new BhtRecord() : ReadXrecord(xr)));
            }
            return o;
        }

        /// <summary>
        /// bht:rec-write (ghi AN TOAN): tao XRECORD moi TRUOC; doi ten ban cu thanh KEY__BHT_CU;
        /// gan ban moi; xoa ban cu. Them vao do ca buoc nam trong 1 Transaction:
        /// loi o bat ky buoc nao -> nguoi goi Abort -> ban cu giu nguyen.
        /// </summary>
        public static ObjectId Write(Transaction tr, Database db, string sub, string key, BhtRecord rec)
        {
            sub=sub.ToUpperInvariant();key=key.ToUpperInvariant();
            DrawingProject.RequireWritable(tr,db);
            if(sub=="DRAWING")throw new InvalidOperationException("Dùng quản lý bình đồ để đổi trạng thái.");
            var stored = Read(tr, db, sub, key);
            if ((sub == "HISTORY" || sub == "CHANGES" || sub == "LOCAL_HISTORY" || sub == "RELEASES" || sub == "SEG_ALIAS") && stored != null && !ObjectRevision.Same(stored, rec))
                throw new InvalidOperationException("Lịch sử chỉ được ghi thêm.");
            if (sub == "META" && stored != null && stored.Get("project_id") != "" && stored.Get("project_id") != rec.Get("project_id"))
                throw new InvalidOperationException("Không được thay định danh dự án đã có lịch sử.");
            if (sub == "OBJ" && !ObjectRevision.OrdinaryWriteAllowed(stored, rec))
                throw new InvalidOperationException("Hồ sơ có lịch sử. Dùng Hiệu chỉnh hồ sơ đã xác nhận.");
            if (sub == "ROUTE" || sub == "SEG") {
                var previous = Read(tr, db, sub, key);
                if (previous != null && previous.ToCanonical() != rec.ToCanonical() && All(tr,db,"OBJ").Any(p=>ProgressPolicy.Locked(p.Value)&&p.Value.Get(sub=="ROUTE"?"route_id":"doan")==key))
                    throw new InvalidOperationException("Tim/đoạn " + key + " có kết quả đã xác nhận. Mở lại đoạn trước khi sửa.");
            }
            if (sub == "OBJ") {
                var previous = Read(tr, db, sub, key);
                if (BHT.Core.ProgressPolicy.Locked(previous) && BHT.Core.ProgressPolicy.Changed(previous, rec))
                    throw new InvalidOperationException("Hồ sơ " + key + " đã xác nhận đoạn " + previous.Get("doan") + ". Mở lại đoạn trong Quản lý xử lý theo đoạn trước khi đổi vị trí hoặc lý trình.");
            }
            return WriteRaw(tr, db, sub, key, rec);
        }

        private static ObjectId WriteRaw(Transaction tr, Database db, string sub, string key, BhtRecord rec)
        {
            key = key.ToUpperInvariant();
            string bak = key + BackupSuffix;
            var d = SubDict(tr, db, sub, true);
            d.UpgradeOpen();
            if (d.Contains(bak))
            {
                var bid = d.GetAt(bak);
                d.Remove(bak);
                var bo = tr.GetObject(bid, OpenMode.ForWrite);
                if (!bo.IsErased) bo.Erase();
            }
            var xr = new Xrecord();
            xr.XlateReferences = false;
            xr.MergeStyle = DuplicateRecordCloning.Ignore; // (280 . 1) nhu Lisp
            using (var rb = new ResultBuffer()) {
                foreach (var s in RecordCodec.Encode(rec)) rb.Add(new TypedValue(1, s));
                xr.Data = rb;
            }
            ObjectId oldId = ObjectId.Null;
            if (d.Contains(key)) { oldId = d.GetAt(key); d.SetName(key, bak); }
            var nid = d.SetAt(key, xr);
            tr.AddNewlyCreatedDBObject(xr, true);
            if (!oldId.IsNull)
            {
                d.Remove(bak);
                var old = tr.GetObject(oldId, OpenMode.ForWrite);
                old.Erase();
            }
            return nid;
        }

        public static bool Delete(Transaction tr, Database db, string sub, string key)
        {
            sub=sub.ToUpperInvariant();key=key.ToUpperInvariant();
            DrawingProject.RequireWritable(tr,db);
            if(sub=="DRAWING"||sub=="RELEASES")throw new InvalidOperationException("Không được xóa trạng thái bình đồ/lịch sử phát hành.");
            if (sub == "HISTORY" || sub == "CHANGES" || sub == "LOCAL_HISTORY" || sub == "SEG_ALIAS" || sub == "META" && (Read(tr,db,sub,key) ?? new BhtRecord()).Get("project_id") != "" || sub == "OBJ" && ObjectRevision.Tracked(Read(tr,db,sub,key)))
                throw new InvalidOperationException("Không được xóa lịch sử hoặc định danh dự án.");
            if (sub == "OBJ" && ProgressPolicy.Locked(Read(tr,db,sub,key)) || (sub == "ROUTE" || sub == "SEG") && All(tr,db,"OBJ").Any(p=>ProgressPolicy.Locked(p.Value)&&p.Value.Get(sub=="ROUTE"?"route_id":"doan")==key))
                throw new InvalidOperationException("Dữ liệu đã được xác nhận. Mở lại kết quả đoạn trước khi xóa.");
            var d = SubDict(tr, db, sub, false);
            key = key.ToUpperInvariant();
            if (d == null || !d.Contains(key)) return false;
            d.UpgradeOpen();
            var id = d.GetAt(key);
            d.Remove(key);
            tr.GetObject(id, OpenMode.ForWrite).Erase();
            return true;
        }

        public static string Meta(Transaction tr, Database db, string key, string def)
        {
            var r = Read(tr, db, "META", "CONFIG");
            string v = r == null ? "" : r.Get(key);
            return v == "" ? def : v;
        }

        public static void MetaSet(Transaction tr, Database db, string key, string value)
        {
            var r = Read(tr, db, "META", "CONFIG") ?? new BhtRecord();
            Write(tr, db, "META", "CONFIG", r.Set(key, value));
        }

        // ------------------------------------------------------------------ XData

        /// <summary>bht:xget: moi gia tri sau ten ung dung (Lisp dung ma 1000).</summary>
        public static List<string> XGet(DBObject o, string app)
        {
            var rb = o.GetXDataForApplication(app);
            if (rb == null) return null;
            var l = new List<string>();
            bool first = true;
            foreach (TypedValue tv in rb)
            {
                if (first && tv.TypeCode == 1001) { first = false; continue; }
                first = false;
                l.Add(Convert.ToString(tv.Value, System.Globalization.CultureInfo.InvariantCulture));
            }
            rb.Dispose();
            return l;
        }

        /// <summary>Duyet moi thuc the trong Model + moi Layout (giong ssget "_X"), loc theo ten lop DXF.</summary>
        public static IEnumerable<ObjectId> EntitiesOfType(Transaction tr, Database db, string dxfName)
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId btrId in bt)
            {
                var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                if (!btr.IsLayout) continue;
                foreach (ObjectId id in btr)
                    if (id.ObjectClass.DxfName == dxfName) yield return id;
            }
        }

        public static string SpaceName(Transaction tr, Entity e)
        {
            var btr = (BlockTableRecord)tr.GetObject(e.OwnerId, OpenMode.ForRead);
            if (btr.Name.Equals(BlockTableRecord.ModelSpace, StringComparison.OrdinalIgnoreCase)) return "Model";
            if (!btr.LayoutId.IsNull) { var lay = (Layout)tr.GetObject(btr.LayoutId, OpenMode.ForRead); return lay.LayoutName; }
            return btr.Name;
        }

        /// <summary>Moi diem RTK v0.2+ (POINT co XData BHT_PT &gt;= 10 chuoi) - giong bht:pt-all.</summary>
        public static List<SurveyPoint> Points(Transaction tr, Database db)
        {
            var o = new List<SurveyPoint>();
            foreach (var id in EntitiesOfType(tr, db, "POINT"))
            {
                var pt = tr.GetObject(id, OpenMode.ForRead) as DBPoint;
                if (pt == null) continue;
                var x = XGet(pt, "BHT_PT");
                if (x == null) continue;
                Point3d p = pt.Position;
                var sp = SurveyPoint.FromXData(x, p.X, p.Y, p.Z);
                if (sp == null) continue;
                sp.Handle = pt.Handle.ToString();
                sp.Space = SpaceName(tr, pt);
                o.Add(sp);
            }
            return o;
        }

        public static void CountOverviewEntities(Transaction tr, Database db, Overview overview)
        {
            var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId blockId in table)
            {
                var block = (BlockTableRecord)tr.GetObject(blockId, OpenMode.ForRead);
                if (!block.IsLayout) continue;
                foreach (ObjectId id in block)
                {
                    string type = id.ObjectClass.DxfName;
                    if (type != "POINT" && type != "INSERT" && type != "TEXT") continue;
                    var entity = tr.GetObject(id, OpenMode.ForRead);
                    if (type == "POINT")
                    {
                        if (XGet(entity,"BHT_PT") == null && XGet(entity,"BHT_RTK") != null) overview.LegacyV01Points++;
                    }
                    else if (type == "INSERT")
                    {
                        var photo = XGet(entity,"BHT_ANHPT"); var sign = XGet(entity,"BHT_KH");
                        if (photo != null && photo.Count > 0) overview.PhotoMarkers++;
                        if (sign != null && sign.Count > 0) overview.Symbols++;
                    }
                    else
                    {
                        var label = XGet(entity,"BHT_NHAN");
                        if (label != null && label.Count > 0) overview.Labels++;
                    }
                }
            }
        }

        /// <summary>So POINT v0.1 (BHT_RTK nhung chua co BHT_PT).</summary>
        public static int LegacyPointCount(Transaction tr, Database db)
        {
            int n = 0;
            foreach (var id in EntitiesOfType(tr, db, "POINT"))
            {
                var pt = tr.GetObject(id, OpenMode.ForRead);
                if (XGet(pt, "BHT_PT") == null && XGet(pt, "BHT_RTK") != null) n++;
            }
            return n;
        }

        /// <summary>(khoa XData dau tien HOA) -> ObjectId cho thuc the mang app (giong bht:tagged-pairs nkey=1).</summary>
        public static List<KeyValuePair<string, ObjectId>> Tagged(Transaction tr, Database db, string dxfName, string app)
        {
            var o = new List<KeyValuePair<string, ObjectId>>();
            foreach (var id in EntitiesOfType(tr, db, dxfName))
            {
                var ob = tr.GetObject(id, OpenMode.ForRead);
                var x = XGet(ob, app);
                if (x != null && x.Count >= 1) o.Add(new KeyValuePair<string, ObjectId>(x[0].ToUpperInvariant(), id));
            }
            return o;
        }
    }
}
