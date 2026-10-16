using System;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Presentation135Probe
{
    static int count,fail;
    static Database Db {get{return AcApp.DocumentManager.MdiActiveDocument.Database;}}
    static string Dir {get{return Path.GetDirectoryName(typeof(Presentation135Probe).Assembly.Location);}}
    static void Check(string n,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" CORRECTION132 "+n);}
    static BhtRecord Read(Database db,string sub,string id){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return BhtStore.Read(tr,db,sub,id);}
    static string Pose(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var id=BhtStore.Tagged(tr,db,"INSERT","BHT_KH").First(p=>p.Key=="OBJ001").Value;var b=(BlockReference)tr.GetObject(id,OpenMode.ForRead);return b.Position+"|"+b.Rotation+"|"+b.ScaleFactors;}}
    static string Survey(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return string.Join("|",BhtStore.Points(tr,db).Select(p=>p.Id+":"+p.X+","+p.Y+","+p.Z))+string.Join("|",BhtStore.All(tr,db,"ROUTE").Select(p=>p.Value.ToCanonical()));}
    static string Details(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return string.Join("\n",new[]{"TEXT","LWPOLYLINE"}.SelectMany(type=>BhtStore.Tagged(tr,db,type,"BHT_KH").Where(p=>p.Key=="OBJ001").Select(pair=> {
        var e=tr.GetObject(pair.Value,OpenMode.ForRead);var t=e as DBText;if(t!=null)return t.TextString+"|"+t.AlignmentPoint+"|"+t.Position+"|"+t.Rotation;
        var p=(Polyline)e;return string.Join("|",Enumerable.Range(0,p.NumberOfVertices).Select(i=>p.GetPoint3dAt(i).ToString()+":"+p.GetBulgeAt(i))); })).OrderBy(s=>s));}
    static void Move(Database db,double distance){using(var tr=db.TransactionManager.StartTransaction()){var id=BhtStore.Tagged(tr,db,"INSERT","BHT_KH").First(p=>p.Key=="OBJ001").Value;var b=(BlockReference)tr.GetObject(id,OpenMode.ForWrite);b.Position+=new Vector3d(distance,3,0);b.Rotation+=0.25;tr.Commit();}}
    static bool Reject(Action a){try{a();return false;}catch(System.Exception ex){File.AppendAllText(Path.Combine(Dir,"expected-errors.txt"),ex+Environment.NewLine);return true;}}
    [CommandMethod("PRESENTATION135PROBE")]
    public static void Run(){try{
        var svc=new ObjectCorrection(Db);string raw=Survey(Db);var old=Read(Db,"OBJ","OBJ001");
        svc.Confirm(svc.Preview("OBJ001",new BhtRecord(),CorrectionKind.DisplayGlobal,"Initial shared layout","QA"),true,false);
        var first=Read(Db,"OBJ","OBJ001");string baseFile=Path.Combine(Dir,"base.bht");ProjectTransfer.Export(Db,baseFile);
        Check("first-display-baseline",first.Get("display_revision")=="1"&&first.Get("survey_revision")=="0"&&first.Get("assignment_revision")=="0");
        Check("display-keeps-survey",Survey(Db)==raw&&first.Get("ly_trinh_m")==old.Get("ly_trinh_m")&&first.GetAll("pt").SequenceEqual(old.GetAll("pt")));
        Move(Db,10);string moved=Pose(Db);
        using(var tr=Db.TransactionManager.StartTransaction()) {
            foreach(var pr in BhtStore.Tagged(tr,Db,"TEXT","BHT_KH").Where(p=>p.Key=="OBJ001"))((Entity)tr.GetObject(pr.Value,OpenMode.ForWrite)).TransformBy(Matrix3d.Displacement(new Vector3d(4,5,0)));
            foreach(var pr in BhtStore.Tagged(tr,Db,"LWPOLYLINE","BHT_KH").Where(p=>p.Key=="OBJ001")) { var line=(Polyline)tr.GetObject(pr.Value,OpenMode.ForWrite);line.SetPointAt(1,new Point2d(24,35)); }
            tr.Commit();
        }
        var preview=svc.Preview("OBJ001",new BhtRecord(),CorrectionKind.DisplayGlobal,"Move shared symbol","QA");
        Move(Db,2);Check("stale-preview-rejected",Reject(()=>svc.Confirm(preview,false,false)));
        svc.Confirm(svc.Preview("OBJ001",new BhtRecord(),CorrectionKind.DisplayGlobal,"Move shared symbol","QA"),false,false);
        var second=Read(Db,"OBJ","OBJ001");string nextFile=Path.Combine(Dir,"next.bht");ProjectTransfer.Export(Db,nextFile);
        Check("display-only-counter",second.Get("display_revision")=="2"&&second.Get("survey_revision")=="0"&&second.Get("revision_scope")=="DISPLAY");
        Check("save-does-not-reposition",Pose(Db)!=moved&&Survey(Db)==raw);
        var beforeUndo=Read(Db,"OBJ","OBJ001");AcApp.DocumentManager.MdiActiveDocument.Editor.Command("_.U");
        Check("undo-display-revision",Read(Db,"OBJ","OBJ001").Get("revision_id")==first.Get("revision_id"));
        AcApp.DocumentManager.MdiActiveDocument.Editor.Command("_.REDO");Check("redo-display-revision",ObjectRevision.Same(beforeUndo,Read(Db,"OBJ","OBJ001")));
        Check("repeat-save-no-revision",Reject(()=>svc.Preview("OBJ001",new BhtRecord(),CorrectionKind.DisplayGlobal,"Repeat","QA")));
        using(var target=new Database(true,true)){
            ProjectTransfer.Import(target,baseFile);ProjectTransfer.ImportRolling(target,nextFile);
            Check("global-layout-received",Pose(target)==Pose(Db)&&Read(target,"OBJ","OBJ001").Get("revision_id")==second.Get("revision_id"));
            Check("manual-label-and-leader-received",Details(target)==Details(Db));
            Check("global-repeat-no-duplicates",ProjectTransfer.ImportRolling(target,nextFile)==0);
            using(var tr=target.TransactionManager.StartOpenCloseTransaction())Check("global-one-symbol",BhtStore.Tagged(tr,target,"INSERT","BHT_KH").Count(p=>p.Key=="OBJ001")==1);
        }
        using(var target=new Database(true,true)){
            ProjectTransfer.Import(target,baseFile);Move(target,50);string local=Pose(target),record=ObjectRevision.Snapshot(Read(target,"OBJ","OBJ001"));
            Check("save-local",DrawingPresentation.SaveLocal(target,"OBJ001","Local print arrangement","QA"));
            Check("local-no-survey-or-global-revision",record==ObjectRevision.Snapshot(Read(target,"OBJ","OBJ001")));
            Check("repeat-local-no-history",!DrawingPresentation.SaveLocal(target,"OBJ001","Repeat","QA"));
            ProjectTransfer.ImportRolling(target,nextFile);
            Check("local-preserved-on-global",Pose(target)==local&&Read(target,"OBJ","OBJ001").Get("revision_id")==second.Get("revision_id"));
            Check("local-repeat-no-duplicates",ProjectTransfer.ImportRolling(target,nextFile)==0&&Pose(target)==local);
            Check("local-not-exported-as-global",Reject(()=>ProjectTransfer.Export(target,Path.Combine(Dir,"local-must-not-publish.bht"))));
            Check("local-switch-mismatch-blocked",Reject(()=>DrawingPresentation.UseGlobal(target,"OBJ001"))&&Pose(target)==local);
            target.SaveAs(Path.Combine(Dir,"local.dwg"),DwgVersion.AC1032);
        }
        using(var reopened=new Database(false,true)){
            reopened.ReadDwgFile(Path.Combine(Dir,"local.dwg"),FileOpenMode.OpenForReadAndAllShare,true,"");reopened.CloseInput(true);
            Check("local-survives-reopen",Read(reopened,"VIEW_LOCAL","OBJ001")!=null&&Read(reopened,"OBJ","OBJ001").Get("display_revision")=="2");
        }
        using(var target=new Database(true,true)){
            ProjectTransfer.Import(target,baseFile);Move(target,90);string pose=Pose(target);
            Check("unsaved-manual-layout-blocks-overwrite",Reject(()=>ProjectTransfer.ImportRolling(target,nextFile))&&Pose(target)==pose);
        }
    }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-ERROR "+ex);}
    AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-TOTAL="+count+" FAIL="+fail+"\n");}
}
