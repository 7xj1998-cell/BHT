using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class Import111Probe
{
    static int count,fail;
    static string Dir {get{return Path.GetDirectoryName(typeof(Import111Probe).Assembly.Location);}}
    static void Check(string name,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" IMPORT111 "+name);}
    static string Snapshot(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
        var lines=new List<string>();var root=BhtStore.RootDict(tr,db,false);
        if(root!=null)foreach(DBDictionaryEntry entry in root)if(tr.GetObject(entry.Value,OpenMode.ForRead) is DBDictionary)
            lines.AddRange(BhtStore.All(tr,db,entry.Key).OrderBy(p=>p.Key).Select(p=>entry.Key+"/"+p.Key+"="+p.Value.ToCanonical()));
        lines.AddRange(BhtStore.Points(tr,db).OrderBy(p=>p.Id).Select(p=>"PT "+p.Id+"|"+p.X.ToString("R")+"|"+p.Y.ToString("R")+"|"+p.Z.ToString("R")));
        var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
        foreach(ObjectId b in bt){var block=(BlockTableRecord)tr.GetObject(b,OpenMode.ForRead);foreach(ObjectId id in block){
            var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;
            string s="CAD "+e.Handle+"|"+block.Name+"|"+e.GetType().Name+"|"+e.Layer;
            if(e is Curve){var c=(Curve)e;s+="|"+c.StartPoint+"|"+c.EndPoint+"|"+c.GetDistanceAtParameter(c.EndParam);}
            if(e is Polyline){var p=(Polyline)e;for(int i=0;i<p.NumberOfVertices;i++)s+="|"+p.GetPoint3dAt(i)+","+p.GetBulgeAt(i);}
            if(e is DBText){var t=(DBText)e;s+="|"+t.TextString+"|"+t.Position+"|"+t.Rotation+"|"+t.Height;}
            if(e is BlockReference){var r=(BlockReference)e;s+="|"+r.Position+"|"+r.Rotation+"|"+r.ScaleFactors+"|"+r.BlockTableRecord;}
            using(var x=e.XData)if(x!=null)s+="|"+string.Join(";",x.AsArray().Select(v=>v.TypeCode+":"+v.Value));lines.Add(s);
        }}
        lines.Add("UNITS "+db.Insunits);return string.Join("\n",lines.OrderBy(s=>s,StringComparer.Ordinal));
    }}
    [CommandMethod("IMPORT111PROBE")]public static void Run(){try{
        string input=Environment.GetEnvironmentVariable("BHT111_INPUT");if(string.IsNullOrEmpty(input))throw new InvalidOperationException("BHT111_INPUT must point to copied fixtures");
        foreach(var path in Directory.GetFiles(input,"*.bht"))using(var package=ProjectPackage.Open(path))using(var db=new Database(false,true)){
            db.ReadDwgFile(package.DrawingPath,FileOpenMode.OpenForReadAndAllShare,true,"");db.CloseInput(true);
            using(var tr=db.TransactionManager.StartOpenCloseTransaction()){Check(Path.GetFileName(path)+"-read-179-objects",BhtStore.All(tr,db,"OBJ").Count==179);Check(Path.GetFileName(path)+"-read-801-points",BhtStore.Points(tr,db).Count==801);}
        }
        string source=Path.Combine(input,"DOAN14.bht");
        using(var target=new Database(false,true)){
            target.ReadDwgFile(Directory.GetFiles(input,"*.dwg").Single(),FileOpenMode.OpenForReadAndAllShare,true,"");target.CloseInput(true);
            string before=Snapshot(target);bool undo=target.UndoRecording;var steps=new List<string>();
            for(int i=0;i<2;i++){
                bool blocked=false;var watch=System.Diagnostics.Stopwatch.StartNew();
                try{ProjectTransfer.ImportRolling(target,source,true,steps.Add);}catch(ImportConflictException ex){
                    blocked=ex.Conflicts.Count==6&&ex.Conflicts.All(c=>c.Id=="OBJ-000001")&&ex.Conflicts.Any(c=>c.Field=="nhom"&&c.Current=="DEN_TH"&&c.Incoming=="BIEN_BAO")&&ex.Conflicts.Any(c=>c.Field=="pt"&&c.Current=="BOT19-R-000144"&&c.Incoming=="BOT1-R-000041");
                    File.WriteAllLines(Path.Combine(Dir,"D14-D15-conflicts.txt"),ex.Conflicts.Select(c=>c.ToString()));
                }
                watch.Stop();File.AppendAllText(Path.Combine(Dir,"import-timing.txt"),"conflict "+i+": "+watch.ElapsedMilliseconds+" ms\n");
                Check("D14-D15-blocks-different-object-"+i,blocked);
                Check("D14-D15-canonical-unchanged-"+i,before==Snapshot(target));
                Check("D14-D15-undo-mode-restored-"+i,undo==target.UndoRecording);
                Check("D14-D15-bounded-preflight-"+i,watch.Elapsed.TotalSeconds<30);
            }
            Check("phase-reports-stop-before-clone",steps.Any(s=>s.Contains("Đối chiếu"))&&!steps.Any(s=>s.StartsWith("Sao chép")));
        }
        using(var target=new Database(true,true)){
            var steps=new List<string>();var watch=System.Diagnostics.Stopwatch.StartNew();int n=ProjectTransfer.ImportRolling(target,source,false,steps.Add);watch.Stop();
            File.AppendAllText(Path.Combine(Dir,"import-timing.txt"),"empty import: "+watch.ElapsedMilliseconds+" ms / "+n+" entities\n");
            Check("empty-import-success",n>0&&steps.Last()=="Hoàn tất giao dịch");
            using(var tr=target.TransactionManager.StartOpenCloseTransaction()){Check("empty-import-179-objects",BhtStore.All(tr,target,"OBJ").Count==179);Check("empty-import-801-points",BhtStore.Points(tr,target).Count==801);}
            string before=Snapshot(target);Check("repeated-import-no-duplicates",ProjectTransfer.ImportRolling(target,source)==0&&before==Snapshot(target));
            target.SaveAs(Path.Combine(Dir,"D14-imported.dwg"),DwgVersion.AC1032);
        }
        using(var target=new Database(false,true)){
            target.ReadDwgFile(Path.Combine(Dir,"D14-imported.dwg"),FileOpenMode.OpenForReadAndAllShare,true,"");target.CloseInput(true);
            string before=Snapshot(target);Check("saved-reopened-import-no-change",ProjectTransfer.ImportRolling(target,source)==0&&before==Snapshot(target));
        }
    }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nIMPORT111-ERROR "+ex);}
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nIMPORT111-TOTAL="+count+" FAIL="+fail);
    }
}
