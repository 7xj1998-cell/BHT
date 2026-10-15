using System;
using System.Globalization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(BHT.Bridge.OffsetDimension))]

namespace BHT.Bridge
{
    public static class OffsetDimension
    {
        public const string LayerName = "BHT_DIM_KIEM_TRA";
        public const string AppName = "BHT_OFFSET_DIM";

        // Real XY geometry and a unit scale of 1; never use a text override to
        // make the dimension agree with a saved offset.
        public static ObjectId Create(Database db, string objectId, string routeId, string pointId,
            Point3d source, Point3d projection, double size)
        {
            if (string.IsNullOrWhiteSpace(objectId) || string.IsNullOrWhiteSpace(routeId)) throw new ArgumentException("Thiếu hồ sơ hoặc tim tuyến.");
            double[] values = { source.X, source.Y, projection.X, projection.Y, size };
            foreach (double value in values) if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentException("Tọa độ DIM không hợp lệ.");
            if (size <= 0 || size > 1000) throw new ArgumentException("Cỡ chữ DIM phải lớn hơn 0 và không quá 1000.");
            source = new Point3d(source.X, source.Y, 0); projection = new Point3d(projection.X, projection.Y, 0);
            var delta = projection - source;
            if (delta.Length < 1e-9) throw new ArgumentException("Điểm nằm trên tim, khoảng cách bằng 0; không tạo DIM có hai đầu trùng nhau.");
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                ObjectId layer;
                if (layers.Has(LayerName))
                {
                    layer = layers[LayerName];
                    var record = (LayerTableRecord)tr.GetObject(layer, OpenMode.ForRead);
                    if (record.IsLocked || record.IsOff || record.IsFrozen) throw new InvalidOperationException("Lớp BHT_DIM_KIEM_TRA đang khóa/tắt/đóng băng; mở lớp để DIM.");
                }
                else
                {
                    layers.UpgradeOpen();
                    var record = new LayerTableRecord { Name = LayerName, Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 2) };
                    layer = layers.Add(record); tr.AddNewlyCreatedDBObject(record, true);
                }
                var apps = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
                if (!apps.Has(AppName)) { apps.UpgradeOpen(); var app = new RegAppTableRecord { Name = AppName }; apps.Add(app); tr.AddNewlyCreatedDBObject(app, true); }
                var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var space = (BlockTableRecord)tr.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                AlignedDimension dimension = null;
                foreach (ObjectId id in space)
                {
                    var candidate = tr.GetObject(id, OpenMode.ForRead) as AlignedDimension;
                    if (candidate == null) continue;
                    using (var data = candidate.GetXDataForApplication(AppName))
                    {
                        if (data == null) continue;
                        var tags = data.AsArray();
                        if (tags.Length >= 3 && string.Equals(Convert.ToString(tags[1].Value), objectId, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(Convert.ToString(tags[2].Value), routeId, StringComparison.OrdinalIgnoreCase))
                        { dimension = candidate; dimension.UpgradeOpen(); break; }
                    }
                }
                var midpoint = source + delta * .5;
                var normal = new Vector3d(-delta.Y, delta.X, 0).GetNormal();
                var location = midpoint + normal * (size * 3);
                if (dimension == null)
                {
                    space.UpgradeOpen();
                    dimension = new AlignedDimension(source, projection, location, "", db.Dimstyle);
                    space.AppendEntity(dimension); tr.AddNewlyCreatedDBObject(dimension, true);
                }
                dimension.XLine1Point = source; dimension.XLine2Point = projection; dimension.DimLinePoint = location;
                dimension.DimensionText = ""; dimension.LayerId = layer; dimension.ColorIndex = 256;
                dimension.Dimtxt = size; dimension.Dimasz = size * .4; dimension.Dimgap = size * .2;
                dimension.Dimexo = size * .15; dimension.Dimexe = size * .3;
                dimension.Dimscale = 1; dimension.Dimlfac = 1; dimension.Dimdec = 3; dimension.Dimzin = 0;
                dimension.Dimrnd = 0; dimension.Dimlunit = 2; dimension.Dimalt = false; dimension.Dimtol = false; dimension.Dimlim = false;
                dimension.Dimpost = "<> m"; dimension.Dimtad = 1;
                dimension.Dimse1 = false; dimension.Dimse2 = false;
                dimension.Dimsd1 = false; dimension.Dimsd2 = false;
                dimension.Dimclrd = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByLayer, 256);
                dimension.Dimclre = dimension.Dimclrd; dimension.Dimclrt = dimension.Dimclrd;
                using (var data = new ResultBuffer(new TypedValue(1001, AppName), new TypedValue(1000, objectId),
                    new TypedValue(1000, routeId), new TypedValue(1000, pointId ?? ""))) dimension.XData = data;
                dimension.RecomputeDimensionBlock(true);
                ObjectId result = dimension.ObjectId; tr.Commit(); return result;
            }
        }

        [LispFunction("BHTOFFSETDIM")]
        public static string FromLisp(ResultBuffer args)
        {
            try
            {
                var a = args == null ? new TypedValue[0] : args.AsArray();
                if (a.Length != 8) return "ERR|Cần hồ sơ, tim, điểm trụ, 4 tọa độ và cỡ chữ.";
                var db = AcApp.DocumentManager.MdiActiveDocument.Database;
                ObjectId id = Create(db, Convert.ToString(a[0].Value), Convert.ToString(a[1].Value), Convert.ToString(a[2].Value),
                    new Point3d(Convert.ToDouble(a[3].Value, CultureInfo.InvariantCulture), Convert.ToDouble(a[4].Value, CultureInfo.InvariantCulture), 0),
                    new Point3d(Convert.ToDouble(a[5].Value, CultureInfo.InvariantCulture), Convert.ToDouble(a[6].Value, CultureInfo.InvariantCulture), 0),
                    Convert.ToDouble(a[7].Value, CultureInfo.InvariantCulture));
                return "OK|" + id.Handle;
            }
            catch (System.Exception ex) { return "ERR|" + ex.Message; }
        }
    }
}
