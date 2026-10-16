using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class Revision134Probe
{
    static int count,fail;
    static Database Db { get { return AcApp.DocumentManager.MdiActiveDocument.Database; } }
    static string Dir { get { return Path.GetDirectoryName(typeof(Revision134Probe).Assembly.Location); } }
    static void Check(string n,bool ok) { count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" CORRECTION132 "+n); }
    static BhtRecord Read(Database db,string sub,string id) { using(var tr=db.TransactionManager.StartOpenCloseTransaction())return BhtStore.Read(tr,db,sub,id); }
    static string Records(Database db) { using(var tr=db.TransactionManager.StartOpenCloseTransaction())return string.Join("\n",new[]{"OBJ","HISTORY","CHANGES","ROUTE","SEG","META"}.SelectMany(s=>BhtStore.All(tr,db,s).OrderBy(p=>p.Key).Select(p=>s+"/"+p.Key+ObjectRevision.Snapshot(p.Value)))); }
    static string Geometry(Database db) {
        using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
            var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead);
            return string.Join("\n",ms.Cast<ObjectId>().Select(id=>{var e=(Entity)tr.GetObject(id,OpenMode.ForRead);var b=e as BlockReference;var p=e as DBPoint;var t=e as DBText;
                return e.Handle+":"+e.GetType().Name+":"+e.Layer+(b==null?"":"|"+b.Name+"|"+b.Position+"|"+b.Rotation+"|"+b.ScaleFactors)+(p==null?"":"|"+p.Position)+(t==null?"":"|"+t.TextString+"|"+t.Position);}).OrderBy(x=>x));
        }
    }
    static bool Reject(Action action) { try { action();return false; } catch(System.Exception ex) { File.AppendAllText(Path.Combine(Dir,"expected-errors.txt"),ex+Environment.NewLine);return true; } }
    static string Snapshot(Database db) { return Records(db)+Geometry(db); }
    [CommandMethod("REVISION134PROBE")]
    public static void Run()
    {
        try {
            var service=new ObjectCorrection(Db);
            service.Confirm(service.Preview("OBJ001",new BhtRecord().Set("ma_hieu","P.103a"),CorrectionKind.Attributes,"REV1","QA"),true,false);
            string rev1=Path.Combine(Dir,"rev1.bht");ProjectTransfer.Export(Db,rev1);
            var first=Read(Db,"OBJ","OBJ001");
            service.Confirm(service.Preview("OBJ001",new BhtRecord().Set("ma_hieu","P.104"),CorrectionKind.Attributes,"REV2","QA"),false,false);
            string rev2=Path.Combine(Dir,"rev2.bht");ProjectTransfer.Export(Db,rev2);
            var second=Read(Db,"OBJ","OBJ001");
            using(var target=new Database(true,true)) {
                ProjectTransfer.Import(target,rev1);
                string others;using(var tr=target.TransactionManager.StartOpenCloseTransaction())others=string.Join("|",BhtStore.All(tr,target,"OBJ").Where(p=>p.Key!="OBJ001").Select(p=>ObjectRevision.Snapshot(p.Value)));
                ProjectTransfer.ImportRolling(target,rev2);
                Check("descendant-upgrade",Read(target,"OBJ","OBJ001").Get("revision_id")==second.Get("revision_id")&&Read(target,"OBJ","OBJ001").Get("ma_hieu")=="P.104");
                using(var tr=target.TransactionManager.StartOpenCloseTransaction()) {
                    BhtStore.ValidateHistory(tr,target);
                    Check("history-union",BhtStore.All(tr,target,"HISTORY").Count==3);
                    Check("49-records-unchanged",others==string.Join("|",BhtStore.All(tr,target,"OBJ").Where(p=>p.Key!="OBJ001").Select(p=>ObjectRevision.Snapshot(p.Value))));
                    Check("no-duplicate-symbol",BhtStore.Tagged(tr,target,"INSERT","BHT_KH").Count(p=>p.Key=="OBJ001")==1);
                }
                string current=Snapshot(target);
                Check("ancestor-ignored",ProjectTransfer.ImportRolling(target,rev1)==0&&Snapshot(target)==current);
                Check("repeat-idempotent",ProjectTransfer.ImportRolling(target,rev2)==0&&Snapshot(target)==current);
                target.SaveAs(Path.Combine(Dir,"merged.dwg"),DwgVersion.AC1032);
            }
            using(var reopen=new Database(false,true)) {
                reopen.ReadDwgFile(Path.Combine(Dir,"merged.dwg"),FileOpenMode.OpenForReadAndAllShare,true,"");reopen.CloseInput(true);
                using(var tr=reopen.TransactionManager.StartOpenCloseTransaction())BhtStore.ValidateHistory(tr,reopen);
                Check("save-reopen-merged-history",Read(reopen,"OBJ","OBJ001").Get("revision_id")==second.Get("revision_id"));
            }
            AcApp.DocumentManager.MdiActiveDocument.Editor.Command("_.U");
            Check("back-to-rev1",Read(Db,"OBJ","OBJ001").Get("revision_id")==first.Get("revision_id"));
            service.Confirm(service.Preview("OBJ001",new BhtRecord().Set("ma_hieu","P.105"),CorrectionKind.Attributes,"FORK","QA"),false,false);
            string fork=Path.Combine(Dir,"fork.bht");ProjectTransfer.Export(Db,fork);
            using(var target=new Database(true,true)) {
                ProjectTransfer.Import(target,rev2);string current=Snapshot(target);
                Check("fork-conflict-no-partial-write",Reject(()=>ProjectTransfer.ImportRolling(target,fork))&&Snapshot(target)==current);
            }
            using(var empty=new Database(true,true)) {
                ProjectTransfer.ImportRolling(empty,rev2);
                Check("new-versioned-project",Read(empty,"OBJ","OBJ001").Get("revision_id")==second.Get("revision_id"));
            }
            using(var moved=new Database(true,true)) {
                ProjectTransfer.Import(moved,rev1);
                using(var tr=moved.TransactionManager.StartTransaction()) {
                    var id=BhtStore.Tagged(tr,moved,"INSERT","BHT_KH").First(p=>p.Key=="OBJ001").Value;
                    var symbol=(BlockReference)tr.GetObject(id,OpenMode.ForWrite);symbol.Position+=new Autodesk.AutoCAD.Geometry.Vector3d(7,11,0);tr.Commit();
                }
                string before=Snapshot(moved);
                Check("manual-layout-conflict-keeps-target",Reject(()=>ProjectTransfer.ImportRolling(moved,rev2))&&before==Snapshot(moved));
            }
            // Throw after the destination has begun changing, exercising the transaction abort.
            using(var broken=new Database(true,true)) {
                ProjectTransfer.Import(broken,rev1);broken.Insunits=UnitsValue.Inches;string before=Snapshot(broken);bool erased=false;
                ObjectErasedEventHandler handler=(sender,args)=> { if(args.DBObject is Entity && args.Erased)erased=true; };
                broken.ObjectErased+=handler;
                bool rejected=Reject(()=>ProjectTransfer.ImportRolling(broken,rev2));broken.ObjectErased-=handler;
                File.WriteAllText(Path.Combine(Dir,"merge-rollback.txt"),"Erased="+erased+" Rejected="+rejected+" Same="+(before==Snapshot(broken)));
                File.WriteAllText(Path.Combine(Dir,"merge-rollback-before.txt"),before);File.WriteAllText(Path.Combine(Dir,"merge-rollback-after.txt"),Snapshot(broken));
                // ObjectErased notifications are deferred until commit in side databases.
                // Unit validation executes after PrepareVersionedRolling has erased the old presentation.
                Check("mid-import-rollback",rejected&&before==Snapshot(broken));
            }
            var retiring=Read(Db,"OBJ","OBJ001");
            service.Confirm(service.Preview("OBJ001",new BhtRecord(),CorrectionKind.Retire,"Ngừng sử dụng sau đối chiếu","QA"),false,false);
            var retired=Read(Db,"OBJ","OBJ001");
            Check("retire-keeps-survey-and-lock",ObjectRevision.Retired(retired)&&retired.Get("progress_locked")=="1"&&retired.Get("ly_trinh_m")==retiring.Get("ly_trinh_m")&&retired.GetAll("pt").SequenceEqual(retiring.GetAll("pt")));
            using(var tr=Db.TransactionManager.StartOpenCloseTransaction())Check("retire-removes-symbol-only",!BhtStore.Tagged(tr,Db,"INSERT","BHT_KH").Any(p=>p.Key=="OBJ001")&&BhtStore.All(tr,Db,"OBJ").Count==50);
            Check("retired-excluded-report",!SignReportRecords.Includes(retired));
            var sync=new LispApi().Call("qa132-eval","(bht:symbol-sync-ex nil T)");
            using(var tr=Db.TransactionManager.StartOpenCloseTransaction())Check("sync-cannot-resurrect",sync.Ok&&!BhtStore.Tagged(tr,Db,"INSERT","BHT_KH").Any(p=>p.Key=="OBJ001"));
            string tombstone=Path.Combine(Dir,"retired.bht");ProjectTransfer.Export(Db,tombstone);
            using(var target=new Database(true,true)) {
                ProjectTransfer.Import(target,fork);ProjectTransfer.ImportRolling(target,tombstone);string before=Snapshot(target);
                Check("old-package-cannot-resurrect",ObjectRevision.Retired(Read(target,"OBJ","OBJ001"))&&ProjectTransfer.ImportRolling(target,fork)==0&&before==Snapshot(target));
            }
        } catch(System.Exception ex) { fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-ERROR "+ex); }
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-TOTAL="+count+" FAIL="+fail+"\n");
    }
}
