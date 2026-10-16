using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Correction132RealProbe
{
    static int count,fail;
    static Database Db { get { return AcApp.DocumentManager.MdiActiveDocument.Database; } }
    static string Dir { get { return Path.GetDirectoryName(typeof(Correction132RealProbe).Assembly.Location); } }
    static void Check(string name,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" CORRECTION132 "+name);}
    static BhtRecord Read(string id){using(var tr=Db.TransactionManager.StartOpenCloseTransaction())return BhtStore.Read(tr,Db,"OBJ",id);}
    static string Rows(Database db,string sub,string except=null){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return string.Join("|",BhtStore.All(tr,db,sub).Where(p=>p.Key!=except).OrderBy(p=>p.Key).Select(p=>p.Key+":"+ObjectRevision.Snapshot(p.Value)));}
    static string Lisp(string value){var r=new LispApi().Call("qa132-eval",value);if(!r.Ok)throw new InvalidOperationException(r.Error);return string.Join("",r.Values);}
    static string Unrelated(string id){var r=new LispApi().Call("qa132-unrelated",id);if(!r.Ok)throw new InvalidOperationException(r.Error);return string.Join("\n",r.Values);}
    static string Snapshot(){var r=new LispApi().Call("bht:api-correction-state");if(!r.Ok)throw new InvalidOperationException(r.Error);return string.Join("\n",r.Values)+string.Join("|",new[]{"OBJ","SEG","ROUTE","META","HISTORY","CHANGES","CORRECTION"}.Select(s=>Rows(Db,s)));}
    [CommandMethod("CORRECTION132REAL")]
    public static void Run(){try{
        string id="OBJ-000044",others=Rows(Db,"OBJ",id),source=Lisp("(list (bht:pt-all) (bht:rec-all \"ROUTE\") (bht:rec-all \"SEG\"))"),before=Snapshot();
        string unrelated=Unrelated(id);
        var old=Read(id);var svc=new ObjectCorrection(Db);
        Check("real-D10-original",old.Get("ma_hieu")=="W.207b"&&ProgressPolicy.Locked(old));
        var plan=svc.Preview(id,new BhtRecord().Set("ma_hieu","W.207c"),CorrectionKind.Attributes,"QA trên bản sao New - đối chiếu mã","QA132");
        Check("real-preview-read-only",before==Snapshot());
        Lisp("(setq qa132-refresh bht:api-correction-refresh)");
        Lisp("(defun bht:api-correction-refresh (id) (qa132-refresh id) (list \"LOI\" \"QA after real CAD update\"))");
        bool failed=false;try{svc.Confirm(plan,true,false);}catch(System.Exception ex){failed=ex.Message.Contains("phục hồi đầy đủ");File.WriteAllText(Path.Combine(Dir,"rollback-result.txt"),ex.ToString());}
        Check("real-fault-full-rollback",failed&&before==Snapshot());
        Lisp("(setq bht:api-correction-refresh qa132-refresh)");
        if(before!=Snapshot())throw new InvalidOperationException("Real rollback mismatch");
        svc.Confirm(plan,true,false);
        Check("real-code-and-face-corrected",Read(id).Get("ma_hieu")=="W.207c"&&Read(id).GetAll("mat").SequenceEqual(new[]{"W.207c"}));
        Check("real-other178-unchanged",others==Rows(Db,"OBJ",id));
        Check("real-unrelated-cad-unchanged",unrelated==Unrelated(id));
        Check("real-801-points-routes-segments-unchanged",source==Lisp("(list (bht:pt-all) (bht:rec-all \"ROUTE\") (bht:rec-all \"SEG\"))"));
        Check("real-baseline-preserves-old",ObjectRevision.Same(old,ObjectRevision.ReadHistory(svc.History(id)[1])));
        Check("real-survey-counter-only",Read(id).Get("survey_revision")=="1"&&Read(id).Get("assignment_revision")=="0"&&Read(id).Get("display_revision")=="0");
        string saved=Path.Combine(Dir,"real-corrected.dwg"),package=Path.Combine(Dir,"real-corrected.bht");Db.SaveAs(saved,DwgVersion.AC1032);ProjectTransfer.Export(Db,package);
        using(var reopened=new Database(false,true)){reopened.ReadDwgFile(saved,FileOpenMode.OpenForReadAndAllShare,true,"");reopened.CloseInput(true);using(var tr=reopened.TransactionManager.StartOpenCloseTransaction())BhtStore.ValidateHistory(tr,reopened);Check("real-save-reopen-records",Rows(Db,"OBJ")==Rows(reopened,"OBJ")&&Rows(Db,"HISTORY")==Rows(reopened,"HISTORY"));}
        using(var imported=new Database(true,true)){ProjectTransfer.Import(imported,package);Check("real-bht-history-roundtrip",Rows(Db,"HISTORY")==Rows(imported,"HISTORY"));}
        File.WriteAllText(Path.Combine(Dir,"record-before.txt"),old.ToCanonical());File.WriteAllText(Path.Combine(Dir,"record-after.txt"),Read(id).ToCanonical());
    }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-ERROR "+ex);}
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-TOTAL="+count+" FAIL="+fail+"\n");
    }
}
