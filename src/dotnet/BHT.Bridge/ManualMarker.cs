using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using BHT.Core;

namespace BHT.Bridge
{
    public static class ManualMarker
    {
        // One transaction owns the point AND its record. Cancel/failure leaves neither.
        public static OpResult Create(Database db, Point3d position, string group, string km, string h, string note)
        {
            double station;
            if (group != "COC_TIEU" && group != "COT_KM") return OpResult.Fail("Chỉ bổ sung cọc tiêu hoặc cột Km.");
            km = (km ?? "").Trim(); h = (h ?? "").Trim();
            if (group == "COT_KM") h = "";
            if ((km != "" || h != "") && !MarkerStation.TryMetres(group, km, h, out station))
                return OpResult.Fail("Nhập Km nguyên 0–99999 và H 0–9; chưa biết thì để trống cả hai.");
            if (double.IsNaN(position.X) || double.IsInfinity(position.X) || double.IsNaN(position.Y) || double.IsInfinity(position.Y))
                return OpResult.Fail("Tọa độ không hợp lệ.");
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var points = BhtStore.Points(tr, db);
                var objects = new Dictionary<string, BhtRecord>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in BhtStore.All(tr, db, "OBJ")) objects[pair.Key] = pair.Value;
                // Prevent an accidental double click or recreation over an existing marker.
                foreach (var rec in objects.Values.Where(r => r.Get(ObjFields.Group) == group))
                {
                    var ids = rec.GetAll(ObjFields.Point);
                    if (points.Any(p => ids.Contains(p.IdUpper) && Math.Sqrt(Math.Pow(p.X-position.X,2)+Math.Pow(p.Y-position.Y,2)) <= 0.001))
                        return OpResult.Fail("Đã có cọc cùng loại tại vị trí này: " + rec.Get(ObjFields.Id) + ". Mở hồ sơ để cập nhật ký hiệu.");
                }
                string id = ObjectLogic.NextId(objects.Keys, BhtStore.Meta(tr, db, "obj_seq", "0"));
                string pid = "MANUAL-M-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
                string now = BhtTime.Now();
                var fields = new BhtRecord().Set(ObjFields.Group, group).Set(ObjFields.MarkerKm, km).Set(ObjFields.MarkerH, h)
                    .Set(ObjFields.Desc, group == "COC_TIEU" ? "Cọc tiêu bổ sung thủ công" : "Cột Km bổ sung thủ công")
                    .Set(ObjFields.Note, note ?? "").Set(ObjFields.PoleCount, "1").Set(ObjFields.FaceCount, "0");
                BhtRecord record;
                var why = ObjectLogic.BuildNew(id, fields, new List<string>{pid}, false,
                    new HashSet<string>{pid}, objects, now, out record);
                if (why != null) return OpResult.Fail(why);
                record.Set("position_source", "MANUAL_CAD").Set("position_z_status", "UNKNOWN")
                    .Set("station_control_eligible", "0");
                var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                if (!layers.Has("BHT_MANUAL"))
                {
                    layers.UpgradeOpen();
                    var layer = new LayerTableRecord { Name = "BHT_MANUAL", Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 6) };
                    layers.Add(layer); tr.AddNewlyCreatedDBObject(layer, true);
                }
                var apps = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
                if (!apps.Has("BHT_PT")) { apps.UpgradeOpen(); var app = new RegAppTableRecord { Name = "BHT_PT" }; apps.Add(app); tr.AddNewlyCreatedDBObject(app, true); }
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var model = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                // XY only: zero is a drawing plane, never a claimed surveyed elevation.
                var point = new DBPoint(new Point3d(position.X, position.Y, 0)) { Layer = "BHT_MANUAL" };
                model.AppendEntity(point); tr.AddNewlyCreatedDBObject(point, true);
                var values = new List<TypedValue> { new TypedValue(1001, "BHT_PT") };
                foreach (var value in new[] {pid, "MANUAL_CAD", "0", id, position.Y.ToString("R",CultureInfo.InvariantCulture),
                    position.X.ToString("R",CultureInfo.InvariantCulture), "", group, "MANUAL_CAD", now, fields.Get(ObjFields.Desc)})
                    values.Add(new TypedValue(1000, value));
                using (var rb = new ResultBuffer(values.ToArray())) point.XData = rb;
                BhtStore.Write(tr, db, "OBJ", id, record);
                BhtStore.MetaSet(tr, db, "obj_seq", ObjectLogic.SeqNum(id).ToString(CultureInfo.InvariantCulture));
                tr.Commit();
                return OpResult.Success(id);
            }
        }
    }
}
