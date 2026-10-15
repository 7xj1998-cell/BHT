using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

public static class Caption118Probe
{
    [CommandMethod("CAPTION118API")]
    public static void Api()
    {
        var api = new LispApi();
        var refresh = api.Call("bht:api-station-label-refresh", "OTHER118");
        var render = api.Call("bht:api-symbol-sync", "MULTI118");
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCAPTION118-API-" +
            (refresh.Ok && render.Ok ? "PASS" : "FAIL " + refresh.Error + "; " + render.Error));
    }

    [CommandMethod("CAPTION118LAYOUT")]
    public static void Layout()
    {
        var db = AcApp.DocumentManager.MdiActiveDocument.Database;
        using (var tr = db.TransactionManager.StartTransaction())
        {
            var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var paper = (BlockTableRecord)tr.GetObject(table[BlockTableRecord.PaperSpace], OpenMode.ForWrite);
            var text = new DBText { TextString = "layout caption", Height = 1, Position = Point3d.Origin };
            paper.AppendEntity(text); tr.AddNewlyCreatedDBObject(text, true);
            if (!((RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead)).Has("BHT_KH")) throw new InvalidOperationException("Missing tag");
            text.XData = new ResultBuffer(new TypedValue(1001, "BHT_KH"), new TypedValue(1000, "LAYOUT118"), new TypedValue(1000, "NHAN"));
            var cover = new Solid(Point3d.Origin, new Point3d(100, 0, 0), new Point3d(0, 100, 0), new Point3d(100, 100, 0));
            paper.AppendEntity(cover); tr.AddNewlyCreatedDBObject(cover, true);
            tr.Commit();
        }
        SignCaptionDrawOrder.Apply(db);
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCAPTION118-LAYOUT-" + (Order(null) > 0 ? "PASS" : "FAIL"));
    }

    [LispFunction("CAPTION118ORDER")]
    public static int Order(ResultBuffer args)
    {
        int count = 0;
        var db = AcApp.DocumentManager.MdiActiveDocument.Database;
        using (var tr = db.TransactionManager.StartTransaction())
        {
            var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId owner in table)
            {
                var space = (BlockTableRecord)tr.GetObject(owner, OpenMode.ForRead);
                if (!space.IsLayout) continue;
                var labels = new HashSet<ObjectId>(space.Cast<ObjectId>().Where(id =>
                {
                    var text = tr.GetObject(id, OpenMode.ForRead) as DBText;
                    if (text == null) return false;
                    using (var x = text.GetXDataForApplication("BHT_KH"))
                        return x != null && x.AsArray().Any(v => v.TypeCode == 1000 && Convert.ToString(v.Value) == "NHAN");
                }));
                if (labels.Count == 0) continue;
                var order = ((DrawOrderTable)tr.GetObject(space.DrawOrderTableId, OpenMode.ForRead)).GetFullDrawOrder(0).Cast<ObjectId>().ToList();
                if (!order.Skip(order.Count - labels.Count).All(labels.Contains)) return -1;
                count += labels.Count;
            }
        }
        return count;
    }
}
