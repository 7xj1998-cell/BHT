using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using BHT.Bridge;
using BHT.Core;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class Layers115Probe
{
    static int count,fail;static string undo;
    static Database Db {get{return AcApp.DocumentManager.MdiActiveDocument.Database;}}
    static string Dir {get{return Path.GetDirectoryName(typeof(Layers115Probe).Assembly.Location);}}
    static void Check(string name,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" CORRECTION132 L115-"+name);}
    static string Records(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
        var root=BhtStore.RootDict(tr,db,false);var lines=new List<string>();
        if(root!=null)foreach(DBDictionaryEntry sub in root)lines.AddRange(BhtStore.All(tr,db,sub.Key).Select(p=>sub.Key+"/"+p.Key+"="+p.Value.ToCanonical()));
        lines.AddRange(BhtStore.Points(tr,db).Select(p=>p.Id+"="+p.X.ToString("R")+","+p.Y.ToString("R")+","+p.Z.ToString("R")));
        return string.Join("\n",lines.OrderBy(s=>s));}}
    static string Geometry(Database db,bool layers){using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
        var lines=new List<string>();var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
        foreach(ObjectId bid in bt){var b=(BlockTableRecord)tr.GetObject(bid,OpenMode.ForRead);
            foreach(ObjectId id in b){var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;
                string s=e.Handle+"|"+e.GetType().Name+"|"+e.Color+"|"+e.Linetype+"|"+e.LineWeight+"|"+e.Transparency;
                if(layers)s+="|"+e.Layer;
                var c=e as Curve;if(c!=null)try{s+="|"+c.StartPoint+"|"+c.EndPoint+"|"+c.GetDistanceAtParameter(c.EndParam);}catch{}
                var p=e as Polyline;if(p!=null)for(int i=0;i<p.NumberOfVertices;i++)s+="|"+p.GetPoint3dAt(i)+"|"+p.GetBulgeAt(i)+"|"+p.GetStartWidthAt(i)+"|"+p.GetEndWidthAt(i);
                var r=e as BlockReference;if(r!=null)s+="|"+r.Position+"|"+r.ScaleFactors+"|"+r.Rotation;
                var t=e as DBText;if(t!=null)s+="|"+t.TextString+"|"+t.Position+"|"+t.Height+"|"+t.Rotation;
                using(var x=e.XData)if(x!=null)s+="|"+string.Join(";",x.AsArray().Select(v=>v.TypeCode+":"+v.Value));lines.Add(s);
        }}return string.Join("\n",lines.OrderBy(s=>s));}}
    static void SetLayer(Database db,string name,short color){using(var tr=db.TransactionManager.StartTransaction()){
        var table=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);
        if(!table.Has(name)){table.UpgradeOpen();var l=new LayerTableRecord{Name=name};table.Add(l);tr.AddNewlyCreatedDBObject(l,true);l.Color=Color.FromColorIndex(ColorMethod.ByAci,color);}
        foreach(var r in BhtStore.All(tr,db,"ROUTE")){var e=(Entity)tr.GetObject(CadView.IdFromHandle(db,r.Value.Get("handle")),OpenMode.ForWrite);e.Layer=name;}
        tr.Commit();}}
    static bool Rejected(Action action){try{action();return false;}catch(InvalidOperationException ex){File.AppendAllText(Path.Combine(Dir,"expected-errors.txt"),ex.Message+Environment.NewLine);return true;}}
    static Database Open(string file){var db=new Database(false,true);db.ReadDwgFile(file,FileOpenMode.OpenForReadAndAllShare,true,"");db.CloseInput(true);return db;}
    [CommandMethod("LAYERS115PROBE")]
    public static void Run(){try{
        string original=Records(Db),geom=Geometry(Db,false);
        SetLayer(Db,"$0$BHT_TUYEN_TDT",7);string before=Geometry(Db,true);
        var preview=RouteLayers.Preview(Db);Check("preview-read-only",before==Geometry(Db,true)&&original==Records(Db));
        Check("legacy-migrate-count",RouteLayers.Apply(Db)==1);
        Check("geometry-records-unchanged",geom==Geometry(Db,false)&&original==Records(Db));
        string after=Geometry(Db,true);Check("repeat-no-op",RouteLayers.Apply(Db)==0&&after==Geometry(Db,true));
        using(var tr=Db.TransactionManager.StartOpenCloseTransaction())Check("layer-final-route-name",BhtStore.All(tr,Db,"ROUTE").All(p=>((Entity)tr.GetObject(CadView.IdFromHandle(Db,p.Value.Get("handle")),OpenMode.ForRead)).Layer==RouteLayers.Name(p.Key)));
        SetLayer(Db,"$0$DIFFERENT",1);before=Geometry(Db,true);
        Check("style-conflict-aborts-all",Rejected(()=>RouteLayers.Apply(Db))&&before==Geometry(Db,true)&&original==Records(Db));
        using(var tr=Db.TransactionManager.StartTransaction()){var lt=(LayerTable)tr.GetObject(Db.LayerTableId,OpenMode.ForRead);var layer=(LayerTableRecord)tr.GetObject(lt["$0$DIFFERENT"],OpenMode.ForWrite);layer.Color=Color.FromColorIndex(ColorMethod.ByAci,7);layer.IsLocked=true;tr.Commit();}
        before=Geometry(Db,true);Check("locked-source-blocks-without-unlocking",Rejected(()=>RouteLayers.Apply(Db))&&before==Geometry(Db,true));
        using(var tr=Db.TransactionManager.StartTransaction()){var lt=(LayerTable)tr.GetObject(Db.LayerTableId,OpenMode.ForRead);((LayerTableRecord)tr.GetObject(lt["$0$DIFFERENT"],OpenMode.ForWrite)).IsLocked=false;tr.Commit();}
        ObjectId viewId;
        using(var tr=Db.TransactionManager.StartTransaction()){
            var bt=(BlockTable)tr.GetObject(Db.BlockTableId,OpenMode.ForRead);
            var paper=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.PaperSpace],OpenMode.ForWrite);
            var view=new Viewport();viewId=paper.AppendEntity(view);tr.AddNewlyCreatedDBObject(view,true);
            var lt=(LayerTable)tr.GetObject(Db.LayerTableId,OpenMode.ForRead);
            view.FreezeLayersInViewport(new ObjectIdCollection(new[]{lt["$0$DIFFERENT"]}).GetEnumerator());tr.Commit();}
        before=Geometry(Db,true);
        Check("viewport-freeze-preserved-and-migration-blocked",Rejected(()=>RouteLayers.Apply(Db))&&before==Geometry(Db,true)&&original==Records(Db));
        using(var tr=Db.TransactionManager.StartTransaction()){tr.GetObject(viewId,OpenMode.ForWrite).Erase();tr.Commit();}
        RouteLayers.Apply(Db);
        string save=Path.Combine(Dir,"synthetic.dwg");Db.SaveAs(save,DwgVersion.AC1032);
        using(var reopened=Open(save))Check("save-reopen-identical",Records(Db)==Records(reopened)&&Geometry(Db,true)==Geometry(reopened,true));
        using(var legacy=new Database(true,true)){
            using(var tr=legacy.TransactionManager.StartTransaction()){
                var bt=(BlockTable)tr.GetObject(legacy.BlockTableId,OpenMode.ForRead);
                var space=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
                var line=new Line(Point3d.Origin,new Point3d(10,20,0));space.AppendEntity(line);tr.AddNewlyCreatedDBObject(line,true);
                BhtStore.Write(tr,legacy,"ROUTE","OLD_LINE",new BhtRecord().Set("route_id","OLD_LINE").Set("handle",line.Handle.ToString()));tr.Commit();}
            string nativeBefore=Geometry(legacy,false)+Records(legacy);
            Check("legacy-native-line-layer-only",RouteLayers.Apply(legacy)==1&&nativeBefore==Geometry(legacy,false)+Records(legacy));
        }
        var input=Environment.GetEnvironmentVariable("BHT115_INPUT");
        using(var package=ProjectPackage.Open(input))using(var real=Open(package.DrawingPath)){
            original=Records(real);geom=Geometry(real,false);int routes;
            using(var tr=real.TransactionManager.StartOpenCloseTransaction())routes=BhtStore.All(tr,real,"ROUTE").Count;
            File.WriteAllLines(Path.Combine(Dir,"real-layer-preview.txt"),RouteLayers.Preview(real));
            Check("real-normalize-all-routes",RouteLayers.Apply(real)==routes);
            Check("real-all-records-RTK-geometry-xdata-unchanged",original==Records(real)&&geom==Geometry(real,false));
            Check("real-repeat-idempotent",RouteLayers.Apply(real)==0);
            real.SaveAs(Path.Combine(Dir,"DOAN15-layers.dwg"),DwgVersion.AC1032);
            using(var reopened=Open(Path.Combine(Dir,"DOAN15-layers.dwg")))Check("real-save-reopen",Records(real)==Records(reopened)&&Geometry(real,true)==Geometry(reopened,true));
            string output=Path.Combine(Dir,"DOAN15-layers.bht");ProjectTransfer.Export(real,output);
            Check("export-keeps-source",Records(real)==original&&Geometry(real,false)==geom);
            using(var target=new Database(true,true)){
                ProjectTransfer.Import(target,output);
                using(var tr=target.TransactionManager.StartOpenCloseTransaction())Check("import-route-layers-no-mangled-name",BhtStore.All(tr,target,"ROUTE").All(p=>((Entity)tr.GetObject(CadView.IdFromHandle(target,p.Value.Get("handle")),OpenMode.ForRead)).Layer==RouteLayers.Name(p.Key)));
                string imported=Records(target)+Geometry(target,true);Check("repeat-rolling-no-duplicate",ProjectTransfer.ImportRolling(target,output)==0&&imported==Records(target)+Geometry(target,true));
            }
            using(var target=new Database(true,true)){
                using(var tr=target.TransactionManager.StartTransaction()){var lt=(LayerTable)tr.GetObject(target.LayerTableId,OpenMode.ForWrite);var l=new LayerTableRecord{Name="BHT_TUYEN_DOAN_11"};lt.Add(l);tr.AddNewlyCreatedDBObject(l,true);l.Color=Color.FromColorIndex(ColorMethod.ByAci,1);tr.Commit();}
                string empty=Records(target)+Geometry(target,true);
                Check("import-layer-conflict-rolls-back-records-and-cad",Rejected(()=>ProjectTransfer.Import(target,output))&&empty==Records(target)+Geometry(target,true));
            }
        }
    }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-ERROR "+ex);}
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-TOTAL="+count+" FAIL="+fail+"\n");
    }
    [CommandMethod("L115SEED")]public static void Seed(){SetLayer(Db,"$0$BHT_TUYEN_TDT",7);undo=Records(Db)+Geometry(Db,true);}
    [CommandMethod("L115APPLY")]public static void Apply(){RouteLayers.Apply(Db);}
    [CommandMethod("L115UNDO")]public static void Undo(){AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(undo==Records(Db)+Geometry(Db,true)?"PASS":"FAIL")+" LAYERS115 single-undo");}
}
