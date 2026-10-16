using System;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Project136Probe
{
    static int count,fail;
    static Database Db{get{return AcApp.DocumentManager.MdiActiveDocument.Database;}}
    static string Dir{get{return Path.GetDirectoryName(typeof(Project136Probe).Assembly.Location);}}
    static void Check(string n,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" CORRECTION132 "+n);}
    static string Rows(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return string.Join("|",new[]{"OBJ","HISTORY","CHANGES","DRAWING","RELEASES","CAD_STATE"}.SelectMany(s=>BhtStore.All(tr,db,s).Select(p=>s+"/"+p.Key+"/"+ObjectRevision.Snapshot(p.Value))));}
    static BhtRecord Read(Database db,string sub,string id){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return BhtStore.Read(tr,db,sub,id);}
    static bool Reject(Action a){try{a();return false;}catch(System.Exception ex){File.AppendAllText(Path.Combine(Dir,"expected-errors.txt"),ex+Environment.NewLine);return true;}}
    [CommandMethod("PROJECT136PROBE")]
    public static void Run(){try{
        var correction=new ObjectCorrection(Db);
        correction.Confirm(correction.Preview("OBJ001",new BhtRecord().Set("ma_hieu","P.103a"),CorrectionKind.Attributes,"Initial correction","QA"),true,false);
        DrawingProject.Register(Db,"DOAN_15 working",false);
        string baseFile=Path.Combine(Dir,"base.bht");ProjectTransfer.Export(Db,baseFile);
        correction.Confirm(correction.Preview("OBJ001",new BhtRecord().Set("ma_hieu","P.104"),CorrectionKind.Attributes,"Update old segment","QA"),false,false);
        string next=Path.Combine(Dir,"next.bht");ProjectTransfer.Export(Db,next);
        string originalName=Db.Filename,originalRows=Rows(Db);object originalModified=AcApp.GetSystemVariable("DBMOD");
        var cadBefore=new LispApi().Call("bht:api-correction-state").Values.ToArray();
        ProjectTransfer.CompareRolling(Db,baseFile);
        Check("active-preview-keeps-filename-and-dirty-state",Db.Filename==originalName&&Convert.ToString(originalModified)==Convert.ToString(AcApp.GetSystemVariable("DBMOD")));
        Check("active-preview-keeps-cad-and-records",originalRows==Rows(Db)&&cadBefore.SequenceEqual(new LispApi().Call("bht:api-correction-state").Values));
        using(var target=new Database(true,true)){
            ProjectTransfer.Import(target,baseFile);DrawingProject.Register(target,"DOAN_01",false);string before=Rows(target);
            string related=Path.Combine(Dir,"related.dwg");target.SaveAs(related,DwgVersion.AC1032);DrawingProject.TrackDrawing(Db,related);
            byte[] relatedBytes=File.ReadAllBytes(related);var status=DrawingProject.RelatedDrawings(Db);
            Check("related-drawing-update-available",status.Count==1&&status[0].Get("state")=="UPDATE_AVAILABLE"&&status[0].Get("updates")=="1");
            Check("related-check-never-writes-file",relatedBytes.SequenceEqual(File.ReadAllBytes(related)));
            var preview=ProjectTransfer.CompareRolling(target,next);
            Check("preview-identifies-update",preview.Count(p=>p.Get("state")=="UPDATE_AVAILABLE")==1&&preview.First(p=>p.Get("object_id")=="OBJ001").Get("decision")=="Upgrade");
            Check("preview-read-only",before==Rows(target));
            ProjectTransfer.ImportRolling(target,next);
            Check("preview-after-sync-up-to-date",ProjectTransfer.CompareRolling(target,next).All(p=>p.Get("state")=="UP_TO_DATE"));
            DrawingProject.Register(target,"Archive",true);before=Rows(target);
            Check("archive-refuses-sync",Reject(()=>ProjectTransfer.ImportRolling(target,next))&&before==Rows(target));
            Check("archive-refuses-write",Reject(()=>{using(var tr=target.TransactionManager.StartTransaction()){BhtStore.Write(tr,target,"META","OTHER",new BhtRecord().Set("test","bad"));tr.Commit();}})&&before==Rows(target));
            string working=Path.Combine(Dir,"working-copy.dwg");DrawingProject.WorkingCopy(target,working);
            Check("working-copy-preserves-archive",File.Exists(working)&&before==Rows(target));
            Check("working-copy-no-overwrite",Reject(()=>DrawingProject.WorkingCopy(target,working)));
            using(var opened=new Database(false,true)){
                opened.ReadDwgFile(working,FileOpenMode.OpenForReadAndAllShare,true,"");opened.CloseInput(true);
                Check("new-drawing-identity",Read(opened,"DRAWING","CURRENT").Get("drawing_id")!=Read(target,"DRAWING","CURRENT").Get("drawing_id"));
                Check("working-copy-keeps-revisions",Read(opened,"OBJ","OBJ001").Get("revision_id")==Read(target,"OBJ","OBJ001").Get("revision_id"));
                string segment=Read(opened,"OBJ","OBJ001").Get("doan");
                string release=DrawingProject.Release(opened,Dir,segment);
                Check("release-R01",release.EndsWith("_R01.bht")&&File.Exists(release)&&Read(opened,"DRAWING","CURRENT").Get("state")=="RELEASED");
                Check("released-refuses-second-write",Reject(()=>DrawingProject.Release(opened,Dir,segment)));
                string nextWork=Path.Combine(Dir,"working-copy-2.dwg");DrawingProject.WorkingCopy(opened,nextWork);
                using(var nextDb=new Database(false,true)){
                    nextDb.ReadDwgFile(nextWork,FileOpenMode.OpenForReadAndAllShare,true,"");nextDb.CloseInput(true);
                    byte[] firstBytes=File.ReadAllBytes(release);string release2=DrawingProject.Release(nextDb,Dir,segment);
                    Check("release-R02-preserves-R01",release2.EndsWith("_R02.bht")&&firstBytes.SequenceEqual(File.ReadAllBytes(release)));
                    using(var tr=nextDb.TransactionManager.StartOpenCloseTransaction())Check("append-only-release-history",BhtStore.All(tr,nextDb,"RELEASES").Count==2);
                }
            }
        }
        var rows=DrawingProject.Segments(Db);Check("segment-counts",rows.Sum(r=>int.Parse(r.Get("objects")))==50&&rows.Sum(r=>int.Parse(r.Get("revised")))==1);
        DrawingProject.Register(Db,"Archive active CAD",true);string final=Rows(Db);
        var lisp=new LispApi().Call("qa132-eval","(bht:rec-write \"META\" \"CONFIG\" (bht:set (bht:rec-read \"META\" \"CONFIG\") \"test\" \"bad\"))");
        Check("lisp-archive-guard",!lisp.Ok&&final==Rows(Db));
        var beforeSymbols=new LispApi().Call("bht:api-correction-state").Values.ToArray();
        var symbols=new LispApi().Call("qa132-eval","(bht:symbol-sync-ex nil T)");
        Check("archive-symbol-sync-read-only",!symbols.Ok&&beforeSymbols.SequenceEqual(new LispApi().Call("bht:api-correction-state").Values));
    }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-ERROR "+ex);}
    AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-TOTAL="+count+" FAIL="+fail+"\n");}
}
