using System;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
public static class Offset121Probe
{
    static void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nPASS OFFSET121 " + message); }
    [CommandMethod("OFFSET121GEOMETRY")]
    public static void Geometry()
    {
        try
        {
            var db = AcApp.DocumentManager.MdiActiveDocument.Database;
            ObjectId layer;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var table = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                layer = table[OffsetDimension.LayerName];
                ((LayerTableRecord)tr.GetObject(layer, OpenMode.ForWrite)).IsLocked = true; tr.Commit();
            }
            bool rejected = false;
            try { OffsetDimension.Create(db, "OTHER121", "ROAD121", "", new Point3d(1, 3, 0), new Point3d(1, 0, 0), .2); }
            catch (InvalidOperationException) { rejected = true; }
            finally
            {
                using (var tr = db.TransactionManager.StartTransaction())
                { ((LayerTableRecord)tr.GetObject(layer, OpenMode.ForWrite)).IsLocked = false; tr.Commit(); }
            }
            Check(rejected, "locked inspection layer rejects without changing existing dimension");
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var space = (BlockTableRecord)tr.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                var dimensions = space.Cast<ObjectId>().Select(id => tr.GetObject(id, OpenMode.ForRead)).OfType<AlignedDimension>().ToList();
                Check(dimensions.Count == 2, "two actual aligned dimensions saved");
                foreach (var dimension in dimensions)
                {
                    var tags = dimension.GetXDataForApplication(OffsetDimension.AppName).AsArray();
                    bool two = Convert.ToString(tags[1].Value) == "TWO121";
                    Check(Math.Abs(dimension.Measurement - (two ? 14 : 2.262)) < 1e-8, "true geometric Measurement " + dimension.Measurement);
                    Check(string.IsNullOrEmpty(dimension.DimensionText) && dimension.Dimlfac == 1 && dimension.Dimscale == 1 && dimension.Dimrnd == 0, "no fake text scale or rounding");
                    Check(dimension.XLine1Point.IsEqualTo(two ? new Point3d(110, 6, 0) : new Point3d(150, 2.262, 0), new Tolerance(1e-8, 1e-8)), "dimension starts at actual selected survey support in XY");
                    Check(dimension.XLine2Point.IsEqualTo(two ? new Point3d(110, 20, 0) : new Point3d(150, 0, 0), new Tolerance(1e-8, 1e-8)), "dimension ends on correct bound alignment");
                }
            }
            AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nOFFSET121-GEOMETRY-PASS");
        }
        catch (System.Exception ex) { AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nOFFSET121-GEOMETRY-FAIL " + ex); }
    }
}
