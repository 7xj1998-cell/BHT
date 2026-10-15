using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
using BHT.Core;

public class CopyProbe
{
    static readonly List<string> Log = new List<string>();
    static List<string> Snapshot(Database db, ObjectId blockId)
    {
        var rows = new List<string>();
        using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            Walk(tr, blockId, rows, 0);
        return rows;
    }
    static void Walk(Transaction tr, ObjectId id, List<string> rows, int depth)
    {
        if (depth > 15) throw new InvalidOperationException("cycle");
        var block = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
        var order = (DrawOrderTable)tr.GetObject(block.DrawOrderTableId, OpenMode.ForRead);
        foreach (ObjectId child in order.GetFullDrawOrder(0))
        {
            var entity = (Entity)tr.GetObject(child, OpenMode.ForRead);
            var color = entity.Color;
            if (color.IsByLayer) color = ((LayerTableRecord)tr.GetObject(entity.LayerId, OpenMode.ForRead)).Color;
            var text = entity as DBText;
            rows.Add(depth + "|" + entity.GetType().Name + "|" + color.ColorMethod + ":" + color.ColorIndex + ":" + color.Red + "," + color.Green + "," + color.Blue + "|" + entity.Visible + "|" + (text == null ? "" : text.TextString));
            var insert = entity as BlockReference;
            if (insert != null)
            {
                Walk(tr, insert.BlockTableRecord, rows, depth + 1);
                foreach (ObjectId aid in insert.AttributeCollection)
                {
                    var attr = (AttributeReference)tr.GetObject(aid, OpenMode.ForRead);
                    rows.Add("ATTR|" + attr.Tag + "|" + attr.TextString + "|" + attr.Color);
                }
            }
        }
    }
    [CommandMethod("BHTCOPYREVIEW")]
    public void Run()
    {
        string folder = Environment.GetEnvironmentVariable("BHT_REVIEW_OUT");
        try
        {
            foreach (string code in new[] {"P.127-80", "R.306-60", "P.127a", "P.127b", "P.127c", "P.127d", "I.434a", "R.415"})
            foreach (bool filled in new[] { true, false })
            using (var source = new Database(true, true))
            {
                var imported = TdtSignLibrary.EnsureBlock(source, code);
                if (!imported.Ok) { Log.Add("IMPORT_FAIL " + code + " " + imported.Error); continue; }
                if (!filled) imported.BlockName = TdtSignLibrary.EnsureOutlineBlock(source, imported.BlockName);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                imported.BlockName = PortableSign.Ensure(source, imported.BlockName);
                Log.Add("PREPARE " + code + " filled=" + filled + " ms=" + watch.ElapsedMilliseconds);
                ObjectId face, reference;
                using (var tr = source.TransactionManager.StartTransaction())
                {
                    var bt = (BlockTable)tr.GetObject(source.BlockTableId, OpenMode.ForRead);
                    face = bt[imported.BlockName];
                    var model = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    var insert = new BlockReference(Point3d.Origin, face);
                    reference = model.AppendEntity(insert); tr.AddNewlyCreatedDBObject(insert, true); tr.Commit();
                }
                var before = Snapshot(source, face);
                File.WriteAllLines(Path.Combine(folder, code + "-source.txt"), before);
                using (var target = new Database(true, true))
                {
                    ObjectId modelId;
                    using (var tr = target.TransactionManager.StartTransaction())
                    {
                        modelId = ((BlockTable)tr.GetObject(target.BlockTableId, OpenMode.ForRead))[BlockTableRecord.ModelSpace];
                        tr.Commit();
                    }
                    var map = new IdMapping();
                    source.WblockCloneObjects(new ObjectIdCollection(new[] {reference}), modelId, map, DuplicateRecordCloning.Ignore, false);
                    var after = Snapshot(target, map[face].Value);
                    File.WriteAllLines(Path.Combine(folder, code + "-target.txt"), after);
                    Log.Add((before.SequenceEqual(after) ? "PASS " : "FAIL ") + code + " filled="+filled+" fresh-drawing-order-colors " + before.Count + "/" + after.Count);
                    string saved = Path.Combine(folder, code + "-" + filled + "-copy.dwg");
                    target.SaveAs(saved, DwgVersion.Current);
                    using(var reopened = new Database(false, true)) {
                        reopened.ReadDwgFile(saved, FileOpenMode.OpenForReadAndAllShare, false, ""); reopened.CloseInput(true);
                        ObjectId rid;
                        using(var rt = reopened.TransactionManager.StartOpenCloseTransaction()) rid=((BlockTable)rt.GetObject(reopened.BlockTableId,OpenMode.ForRead))[imported.BlockName];
                        Log.Add((before.SequenceEqual(Snapshot(reopened,rid)) ? "PASS " : "FAIL ")+code+" filled="+filled+" reopen");
                    }
                }
            }
            Log.Add("TDT_ROOT=" + Tdt91Installation.FindRoot());
            Log.Add("CATALOG=" + BhtSignLibrary.Root);
            CheckLayerCollision(folder);
            CheckProjectTransfer(folder);
        }
        catch (System.Exception ex) { Log.Add("ERROR " + ex); }
        File.WriteAllLines(Path.Combine(folder, "copy-review.txt"), Log);
    }

    static void CheckLayerCollision(string folder)
    {
        using(var source = new Database(true,true))
        using(var target = new Database(true,true))
        {
            ObjectId face;
            using(var tr=source.TransactionManager.StartTransaction()) {
                var layers=(LayerTable)tr.GetObject(source.LayerTableId,OpenMode.ForWrite);
                var layer=new LayerTableRecord { Name="COPY_CONFLICT", Color=Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci,1) };
                layers.Add(layer);tr.AddNewlyCreatedDBObject(layer,true);
                var bt=(BlockTable)tr.GetObject(source.BlockTableId,OpenMode.ForWrite);
                var block=new BlockTableRecord {Name="BHT_TEST_CONFLICT"};face=bt.Add(block);tr.AddNewlyCreatedDBObject(block,true);
                var line=new Line(Point3d.Origin,new Point3d(1,1,0)){LayerId=layer.ObjectId,ColorIndex=256};block.AppendEntity(line);tr.AddNewlyCreatedDBObject(line,true);tr.Commit();
            }
            string name=PortableSign.Ensure(source,"BHT_TEST_CONFLICT");
            using(var tr=source.TransactionManager.StartOpenCloseTransaction()) face=((BlockTable)tr.GetObject(source.BlockTableId,OpenMode.ForRead))[name];
            using(var tr=target.TransactionManager.StartTransaction()) {
                var layers=(LayerTable)tr.GetObject(target.LayerTableId,OpenMode.ForWrite);
                var layer=new LayerTableRecord {Name="COPY_CONFLICT",Color=Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci,7)};
                layers.Add(layer);tr.AddNewlyCreatedDBObject(layer,true);tr.Commit();
            }
            var map=new IdMapping();source.WblockCloneObjects(new ObjectIdCollection(new[]{face}),target.BlockTableId,map,DuplicateRecordCloning.Ignore,false);
            Log.Add((Snapshot(source,face).SequenceEqual(Snapshot(target,map[face].Value))?"PASS ":"FAIL ")+"destination-layer-white-preserves-red");
        }
    }

    static void CheckProjectTransfer(string folder)
    {
        using(var source=new Database(true,true))
        using(var target=new Database(true,true))
        {
            var imported=TdtSignLibrary.EnsureBlock(source,"P.127-80");ObjectId face;
            using(var tr=source.TransactionManager.StartTransaction()) {
                var bt=(BlockTable)tr.GetObject(source.BlockTableId,OpenMode.ForRead);face=bt[imported.BlockName];
                var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
                var br=new BlockReference(Point3d.Origin,face);ms.AppendEntity(br);tr.AddNewlyCreatedDBObject(br,true);
                var apps=(RegAppTable)tr.GetObject(source.RegAppTableId,OpenMode.ForWrite);var app=new RegAppTableRecord{Name="BHT_KH"};apps.Add(app);tr.AddNewlyCreatedDBObject(app,true);
                br.XData=new ResultBuffer(new TypedValue(1001,"BHT_KH"),new TypedValue(1000,"COPY_TEST"));
                BhtStore.Write(tr,source,"OBJ","COPY_TEST",new BhtRecord().Set("nhom","BIEN_BAO"));tr.Commit();
            }
            var before=Snapshot(source,face);string file=Path.Combine(folder,"project-"+Guid.NewGuid().ToString("N")+".dwg");
            ProjectTransfer.Export(source,file);ProjectTransfer.Import(target,file);
            ObjectId importedFace;
            using(var tr=target.TransactionManager.StartOpenCloseTransaction()) {
                var bt=(BlockTable)tr.GetObject(target.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead);
                importedFace=((BlockReference)tr.GetObject(ms.Cast<ObjectId>().First(),OpenMode.ForRead)).BlockTableRecord;
            }
            Log.Add((before.SequenceEqual(Snapshot(target,importedFace))?"PASS ":"FAIL ")+"project-export-import-order-colors");
        }
    }
}
