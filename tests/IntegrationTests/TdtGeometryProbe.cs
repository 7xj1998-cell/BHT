using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;

public class TdtGeometryProbe
{
    private readonly StringBuilder log = new StringBuilder();
    private int failures;
    private int checks;

    private void Check(string name, bool ok)
    {
        checks++;
        if (!ok) failures++;
        log.AppendLine((ok ? "PASS " : "FAIL ") + name);
    }

    private static Polyline Convert(Curve[] curves, out int pieces)
    {
        // Same probe also runs against the released 1.0.11 assembly to reproduce
        // its single-arc defect. The new public API survives release obfuscation.
        var method = typeof(Tdt91Alignment).GetMethod("CreateReferenceGeometry");
        if (method != null)
        {
            object[] args = { curves, 0 };
            var result = (Polyline)method.Invoke(null, args);
            pieces = (int)args[1];
            return result;
        }
        pieces = 1;
        var legacy = typeof(Tdt91Alignment).GetMethod("ToPolyline", BindingFlags.NonPublic | BindingFlags.Static);
        Curve curve = curves[0];
        double length = curve.GetDistanceAtParameter(curve.EndParam) - curve.GetDistanceAtParameter(curve.StartParam);
        return (Polyline)legacy.Invoke(null, new object[] { curve, length });
    }

    private void ArcCase(string name, Vector3d normal, double start, double end, double radius)
    {
        using (var arc = new Arc(new Point3d(500000, 1200000, 0), normal, radius, start, end))
        {
            int pieces;
            Point3d before = arc.StartPoint;
            using (var result = Convert(new Curve[] { arc }, out pieces))
            {
                Check(name + "-two-vertices", result.NumberOfVertices == 2 && pieces == 1);
                Check(name + "-length", Math.Abs(result.Length - arc.Length) < 1e-7);
                double maxError = 0;
                for (int i = 0; i <= 20; i++)
                {
                    Point3d expected = arc.GetPointAtDist(arc.Length * i / 20.0);
                    Point3d actual = result.GetPointAtDist(result.Length * i / 20.0);
                    maxError = Math.Max(maxError, expected.DistanceTo(actual));
                }
                Check(name + "-positions", maxError < 1e-7);
                Check(name + "-source-unchanged", arc.StartPoint == before && !arc.IsDisposed);
                log.AppendLine("MEASURE " + name + " length-error=" + Math.Abs(result.Length - arc.Length).ToString("R") + " max-position-error=" + maxError.ToString("R"));
            }
        }
    }

    private void ChainCase(bool reverseLine)
    {
        using (var arc = new Arc(Point3d.Origin, Vector3d.ZAxis, 10, 0, Math.PI / 2))
        using (var line = new Line(reverseLine ? new Point3d(-30, 10, 0) : new Point3d(0, 10, 0),
                                   reverseLine ? new Point3d(0, 10, 0) : new Point3d(-30, 10, 0)))
        {
            string name = reverseLine ? "reversed-chain" : "forward-chain";
            int pieces;
            using (var result = Convert(new Curve[] { arc, line }, out pieces))
            {
                Check(name + "-joined", pieces == 2 && result.NumberOfVertices == 3);
                Check(name + "-length", Math.Abs(result.Length - arc.Length - line.Length) < 1e-8);
                Point3d expected = arc.GetPointAtDist(arc.Length / 2);
                double distance = reverseLine ? line.Length + arc.Length / 2 : arc.Length / 2;
                Check(name + "-arc-position", result.GetPointAtDist(distance).DistanceTo(expected) < 1e-8);
            }
        }
    }

    [CommandMethod("BHTGEOMETRYREGRESSION")]
    public void Run()
    {
        try
        {
            ArcCase("semicircle", Vector3d.ZAxis, 0, Math.PI, 10);
            if (Environment.GetEnvironmentVariable("BHT_GEOMETRY_LEGACY") != "1")
            {
                ArcCase("clockwise", -Vector3d.ZAxis, 0.3, 4.5, 25);
                ArcCase("wrapped-angle", Vector3d.ZAxis, 5.5, 0.5, 12);
                ArcCase("tiny-arc", Vector3d.ZAxis, 0, Math.PI, 0.01);
                ArcCase("large-radius", Vector3d.ZAxis, 0, Math.PI / 2, 5000);
                ChainCase(false);
                ChainCase(true);
                int pieces;
                using (var line = new Line(new Point3d(10, 20, 0), new Point3d(40, 60, 0)))
                using (var result = Convert(new Curve[] { line }, out pieces))
                    Check("single-line", result.NumberOfVertices == 2 && Math.Abs(result.Length - 50) < 1e-9);
                using (var source = new Polyline())
                {
                    source.AddVertexAt(0, Point2d.Origin, 1, 0, 0);
                    source.AddVertexAt(1, new Point2d(20, 0), 0, 0, 0);
                    using (var result = Convert(new Curve[] { source }, out pieces))
                    {
                        Check("polyline-bulge-preserved", result.GetBulgeAt(0) == 1 && Math.Abs(result.Length - source.Length) < 1e-9);
                        result.SetPointAt(0, new Point2d(-1, 0));
                        Check("independent-owned-reference", source.GetPoint2dAt(0) == Point2d.Origin);
                    }
                }
                using (var ellipse = new Ellipse(Point3d.Origin, Vector3d.ZAxis, new Vector3d(20, 0, 0), 0.5, 0, Math.PI))
                using (var result = Convert(new Curve[] { ellipse }, out pieces))
                    Check("ellipse-fallback-endpoints", result.StartPoint.DistanceTo(ellipse.StartPoint) < 1e-8 && result.EndPoint.DistanceTo(ellipse.EndPoint) < 1e-8);
                Check("empty-input", Convert(new Curve[0], out pieces) == null && pieces == 0);
                SaveReopen();
                ReferenceOwnership();
            }
        }
        catch (System.Exception ex) { failures++; log.AppendLine("ERROR " + ex); }
        log.AppendLine("CHECKS=" + checks);
        log.AppendLine("FAILURES=" + failures);
        File.WriteAllText(Environment.GetEnvironmentVariable("BHT_GEOMETRY_REPORT"), log.ToString(), new UTF8Encoding(false));
    }

    private void SaveReopen()
    {
        string path = Path.ChangeExtension(Environment.GetEnvironmentVariable("BHT_GEOMETRY_REPORT"), ".dwg");
        using (var db = new Database(true, true))
        using (var arc = new Arc(Point3d.Origin, Vector3d.ZAxis, 10, 0, Math.PI))
        {
            int pieces;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var space = (BlockTableRecord)tr.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                var route = Convert(new Curve[] { arc }, out pieces);
                space.AppendEntity(route); tr.AddNewlyCreatedDBObject(route, true);
                tr.Commit();
            }
            db.SaveAs(path, DwgVersion.Current);
        }
        using (var db = new Database(false, true))
        {
            db.ReadDwgFile(path, FileOpenMode.OpenForReadAndAllShare, false, ""); db.CloseInput(true);
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var space = (BlockTableRecord)tr.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                var route = (Polyline)tr.GetObject(space.Cast<ObjectId>().Single(), OpenMode.ForRead);
                Check("save-reopen-exact-arc", route.NumberOfVertices == 2 && Math.Abs(route.Length - 10 * Math.PI) < 1e-8 && Math.Abs(route.GetBulgeAt(0) - 1) < 1e-9);
            }
        }
    }

    private void ReferenceOwnership()
    {
        using (var db = new Database(true, true))
        using (var tr = db.TransactionManager.StartTransaction())
        {
            var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForWrite);
            var layer = new LayerTableRecord { Name = Tdt91Alignment.LayerName };
            layers.Add(layer); tr.AddNewlyCreatedDBObject(layer, true);
            var apps = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForWrite);
            var app = new RegAppTableRecord { Name = Tdt91Alignment.AppName };
            apps.Add(app); tr.AddNewlyCreatedDBObject(app, true);
            var blocks = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var model = (BlockTableRecord)tr.GetObject(blocks[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            var route = new Polyline();
            route.AddVertexAt(0, Point2d.Origin, 0, 0, 0); route.AddVertexAt(1, new Point2d(10, 0), 0, 0, 0);
            model.AppendEntity(route); tr.AddNewlyCreatedDBObject(route, true);
            route.Layer = Tdt91Alignment.LayerName;
            var method = typeof(Tdt91Alignment).GetMethod("CanUpdateReference");
            Check("ownership-api", method != null);
            if (method == null) return;
            Func<string, bool> allowed = source => (bool)method.Invoke(null, new object[] { route, source });
            Check("same-layer-is-not-ownership", !allowed("A1"));
            using (var data = new ResultBuffer(new TypedValue(1001, Tdt91Alignment.AppName), new TypedValue(1000, "A1"))) route.XData = data;
            Check("other-source-reference-rejected", !allowed("B2"));
            Check("matching-source-reference-accepted", allowed("a1"));
            Check("ownership-check-keeps-geometry", route.StartPoint == Point3d.Origin && route.Length == 10);
        }
    }
}
