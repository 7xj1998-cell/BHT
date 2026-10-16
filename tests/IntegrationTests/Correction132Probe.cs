using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class Correction132Probe
{
    static int count,fail;
    static string Dir { get { return Path.GetDirectoryName(typeof(Correction132Probe).Assembly.Location); } }
    static Database Db { get { return AcApp.DocumentManager.MdiActiveDocument.Database; } }
    static void Check(string name,bool value) { count++;if(!value)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(value?"PASS":"FAIL")+" CORRECTION132 "+name); }
    static bool Rejected(Action act) { try { act();return false; } catch(System.Exception ex) { File.AppendAllText(Path.Combine(Dir,"expected-errors.txt"),ex.Message+Environment.NewLine);return true; } }
    static string Lisp(string expression) { var r=new LispApi().Call("qa132-eval",expression);if(!r.Ok)throw new InvalidOperationException(r.Error);return string.Join("",r.Values); }
    static BhtRecord Read(Database db,string sub,string id) { using(var tr=db.TransactionManager.StartOpenCloseTransaction())return BhtStore.Read(tr,db,sub,id); }
    static string Records(Database db,string sub,string except=null) { using(var tr=db.TransactionManager.StartOpenCloseTransaction())return string.Join("|",BhtStore.All(tr,db,sub).Where(p=>p.Key!=except).OrderBy(p=>p.Key).Select(p=>p.Key+":"+ObjectRevision.Snapshot(p.Value))); }
    static string Source(Database db) { using(var tr=db.TransactionManager.StartOpenCloseTransaction())return Records(db,"SEG")+Records(db,"ROUTE")+string.Join("|",BhtStore.Points(tr,db).OrderBy(p=>p.Id).Select(p=>p.Id+":"+p.X.ToString("R")+","+p.Y.ToString("R")+","+p.Z.ToString("R"))); }
    static string Entities(Database db)
    {
        using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
            var lines=new List<string>();
            foreach(string type in new[]{"INSERT","TEXT","LWPOLYLINE","LINE","POINT"})foreach(var id in BhtStore.EntitiesOfType(tr,db,type)) {
                var entity=(Entity)tr.GetObject(id,OpenMode.ForRead);string s=entity.Handle+":"+entity.GetType().Name+":"+entity.Layer;
                var block=entity as BlockReference;if(block!=null)s+="|"+block.Name+"|"+block.Position+"|"+block.Rotation+"|"+block.ScaleFactors;
                var text=entity as DBText;if(text!=null)s+="|"+text.TextString+"|"+text.Position+"|"+text.AlignmentPoint+"|"+text.Rotation+"|"+text.Height;
                var point=entity as DBPoint;if(point!=null)s+="|"+point.Position;
                var curve=entity as Curve;if(curve!=null) { s+="|"+curve.StartPoint+"|"+curve.EndPoint;var pl=curve as Polyline;if(pl!=null)for(int i=0;i<pl.NumberOfVertices;i++)s+="|"+pl.GetPoint3dAt(i)+"|"+pl.GetBulgeAt(i); }
                using(var x=entity.XData)if(x!=null)s+="|"+string.Join(";",x.AsArray().Select(v=>v.TypeCode+":"+v.Value));lines.Add(s);
            }
            return string.Join("\n",lines.OrderBy(x=>x));
        }
    }
    static string Snapshot(Database db) { return string.Join("\n",new[]{"OBJ","SEG","ROUTE","META","HISTORY","CHANGES"}.Select(s=>s+Records(db,s)))+Entities(db); }
    static void Write(string sub,string id,BhtRecord record) { using(var tr=Db.TransactionManager.StartTransaction()){BhtStore.Write(tr,Db,sub,id,record);tr.Commit();} }
    static string Transform() { using(var tr=Db.TransactionManager.StartOpenCloseTransaction()) { var id=BhtStore.Tagged(tr,Db,"INSERT","BHT_KH").First(p=>p.Key=="OBJ001").Value;var b=(BlockReference)tr.GetObject(id,OpenMode.ForRead);return b.Position+"|"+b.Rotation+"|"+b.ScaleFactors; } }
    [CommandMethod("CORRECTION132PROBE")]
    public static void Run()
    {
        try {
            var service=new ObjectCorrection(Db);string original=Snapshot(Db),others=Records(Db,"OBJ","OBJ001"),source=Source(Db),transform=Transform();
            var old=Read(Db,"OBJ","OBJ001");
            AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-UNDOCTL="+AcApp.GetSystemVariable("UNDOCTL")+" RECORDING="+Db.UndoRecording);
            Check("50-confirmed-fixture",Enumerable.Range(1,50).All(i=>ProgressPolicy.Locked(Read(Db,"OBJ","OBJ"+i.ToString("D3")))));
            string legacy=Path.Combine(Dir,"legacy.bht");ProjectTransfer.Export(Db,legacy);
            var plan=service.Preview("OBJ001",new BhtRecord().Set("ma_hieu","P.103a"),CorrectionKind.Attributes,"Đối chiếu ảnh","QA132");
            Check("preview-cancel-no-write",original==Snapshot(Db));
            Check("baseline-approval-required",Rejected(()=>service.Confirm(plan,false,false))&&original==Snapshot(Db));
            // Actual Lisp CAD edits followed by an injected error: managed transaction must undo all of them.
            Lisp("(setq qa132-refresh bht:api-correction-refresh)");
            Lisp("(defun bht:api-correction-refresh (id) (qa132-refresh id) (list \"LOI\" \"QA failure after CAD/history/record writes\"))");
            bool injected=false;
            try{service.Confirm(plan,true,false);}catch(System.Exception ex){injected=ex.Message.Contains("QA failure after CAD/history/record writes")&&ex.Message.Contains("phục hồi đầy đủ");}
            Check("fault-after-cad-rejected",injected);
            string rolled=Snapshot(Db);File.WriteAllText(Path.Combine(Dir,"rollback-before.txt"),original);File.WriteAllText(Path.Combine(Dir,"rollback-after.txt"),rolled);
            Check("atomic-cad-record-history-rollback",original==rolled);
            Lisp("(setq bht:api-correction-refresh qa132-refresh)");
            if(original!=rolled)throw new InvalidOperationException("Atomicity failed; stop further correction tests.");
            service.Confirm(plan,true,false);
            var now=Read(Db,"OBJ","OBJ001");var history=service.History("OBJ001");
            Check("code-corrected-survey-revision",now.Get("ma_hieu")=="P.103a"&&now.Get("survey_revision")=="1"&&now.Get("display_revision")=="0"&&now.Get("assignment_revision")=="0");
            Check("49-unchanged-locked",others==Records(Db,"OBJ","OBJ001"));
            Check("original-rtk-route-segment-unchanged",source==Source(Db));
            Check("baseline-keeps-exact-old-record",history.Count==2&&history[1].Get("revision_status")=="LEGACY_UNVERIFIED"&&ObjectRevision.Same(ObjectRevision.ReadHistory(history[1]),old));
            Check("explicit-parent-chain",now.Get("parent_revision")==history[1].Get("revision_id")&&history[0].Get("parent_revision")==history[1].Get("revision_id"));
            Check("manual-transform-retained",transform==Transform());
            Check("symbol-matches-corrected-code",Lisp("(= (cdr (assoc 2 (entget qa132-symbol))) (bht:kh-block (bht:obj-read \"OBJ001\")))")=="T");
            string confirmed=Snapshot(Db);
            AcApp.DocumentManager.MdiActiveDocument.Editor.Command("_.U");
            Check("one-undo-restores-entire-correction",original==Snapshot(Db));
            AcApp.DocumentManager.MdiActiveDocument.Editor.Command("_.REDO");
            Check("redo-restores-data-history-and-cad",confirmed==Snapshot(Db));
            Check("repeat-confirm-no-duplicate",Rejected(()=>service.Confirm(plan,true,false))&&confirmed==Snapshot(Db));
            Check("no-op-no-history",Rejected(()=>service.Preview("OBJ001",new BhtRecord().Set("ma_hieu","P.103a"),CorrectionKind.Attributes,"repeat","QA"))&&confirmed==Snapshot(Db));
            foreach(string key in new[]{"progress_locked","revision_id","pt","doan","ma_hieu","ghi_chu"}) {
                Check("net-guard-"+key,Rejected(()=>Write("OBJ","OBJ001",now.Clone().Set(key,"BAD")))&&confirmed==Snapshot(Db));
                string r=Lisp("(vl-catch-all-error-p (vl-catch-all-apply 'bht:rec-write (list \"OBJ\" \"OBJ001\" (bht:set (bht:obj-read \"OBJ001\") \""+key+"\" \"BAD\"))))");
                Check("lisp-guard-"+key,r=="T"&&confirmed==Snapshot(Db));
            }
            string hk=history[0].Get("revision_id");
            Check("history-immutable-net",Rejected(()=>Write("HISTORY",hk,history[0].Clone().Set("revision_reason","tamper")))&&confirmed==Snapshot(Db));
            Check("history-immutable-lisp",Lisp("(vl-catch-all-error-p (vl-catch-all-apply 'bht:rec-delete (list \"HISTORY\" \""+hk+"\")))")=="T"&&confirmed==Snapshot(Db));
            Check("segment-reopen-refused",Lisp("(vl-catch-all-error-p (vl-catch-all-apply 'bht:progress-reopen (list \"DOAN_01\")))")=="T"&&confirmed==Snapshot(Db));
            Check("invalid-code-rejected",Rejected(()=>service.Preview("OBJ001",new BhtRecord().Set("ma_hieu","INVALID-132"),CorrectionKind.Attributes,"invalid","QA"))&&confirmed==Snapshot(Db));
            Lisp("(setq qa132-layer (tblobjname \"LAYER\" (cdr (assoc 8 (entget qa132-symbol)))))");
            Lisp("(setq qa132-layer-before (entget qa132-layer))");
            Lisp("(entmod (bht:dxf-put (entget qa132-layer) 70 (logior 4 (cdr (assoc 70 (entget qa132-layer))))))");
            Check("locked-symbol-layer-rejected",Rejected(()=>service.Preview("OBJ001",new BhtRecord().Set("ghi_chu","locked layer"),CorrectionKind.Attributes,"layer","QA")));
            Lisp("(entmod qa132-layer-before)");
            Check("same-station-no-spurious-revision",Rejected(()=>service.Preview("OBJ001",new BhtRecord(),CorrectionKind.Station,"no-op","QA"))&&confirmed==Snapshot(Db));
            var relink=service.Preview("OBJ001",new BhtRecord().Set("pt","NEW-RTK").Set("doan","DOAN_02"),CorrectionKind.PointLink,"Sửa liên kết RTK","QA132");
            Check("rtk-preview-recalculates",relink.After.Get("ly_trinh_m")=="1080.000"&&relink.After.GetAll("pt").SequenceEqual(new[]{"NEW-RTK"})&&relink.After.Get("station_point_method")=="REPRESENTATIVE"&&relink.After.Get("station_point_e")=="80.000000"&&relink.After.Get("offset_m")=="3.000"&&relink.After.Get("phia_tuyen")!=now.Get("phia_tuyen"));
            Check("segment-change-needs-approval",Rejected(()=>service.Confirm(relink,false,false))&&confirmed==Snapshot(Db));
            service.Confirm(relink,false,true);now=Read(Db,"OBJ","OBJ001");
            Check("point-assignment-counters",now.Get("survey_revision")=="2"&&now.Get("assignment_revision")=="1"&&now.Get("goi")=="GOI_02"&&now.Get("doan")=="DOAN_02");
            Check("relink-preserves-raw-data-and-others",source==Source(Db)&&others==Records(Db,"OBJ","OBJ001"));
            Check("relink-keeps-previous-results",service.History("OBJ001").Count==3&&ObjectRevision.ReadHistory(service.History("OBJ001")[1]).Get("ly_trinh_m")==old.Get("ly_trinh_m"));
            string before=Snapshot(Db);
            Check("out-of-range-rejected",Rejected(()=>service.Preview("OBJ001",new BhtRecord().Set("pt","OUT-RTK"),CorrectionKind.PointLink,"outside","QA"))&&before==Snapshot(Db));
            Check("shared-point-rejected",Rejected(()=>service.Preview("OBJ001",new BhtRecord().Set("pt","OBJ002-P"),CorrectionKind.PointLink,"shared","QA"))&&before==Snapshot(Db));
            var stale=service.Preview("OBJ001",new BhtRecord().Set("ghi_chu","test"),CorrectionKind.Attributes,"stale","QA");
            Lisp("(bht:meta-set \"qa132-stale\" \"changed\")");before=Snapshot(Db);
            Check("stale-preview-rejected",Rejected(()=>service.Confirm(stale,false,false))&&before==Snapshot(Db));
            Check("old-rolling-import-refused",Rejected(()=>ProjectTransfer.ImportRolling(Db,legacy))&&before==Snapshot(Db));
            string full=Path.Combine(Dir,"corrected.bht");ProjectTransfer.Export(Db,full);
            using(var target=new Database(true,true)) {
                ProjectTransfer.Import(target,full);
                Check("full-import-preserves-history",Records(Db,"HISTORY")==Records(target,"HISTORY")&&ObjectRevision.Same(ObjectRevision.Governed(Read(Db,"OBJ","OBJ001")),ObjectRevision.Governed(Read(target,"OBJ","OBJ001"))));
                string saved=Path.Combine(Dir,"imported.dwg");target.SaveAs(saved,DwgVersion.AC1032);
            }
            using(var saved=new Database(false,true)) { saved.ReadDwgFile(Path.Combine(Dir,"imported.dwg"),FileOpenMode.OpenForReadAndAllShare,true,"");saved.CloseInput(true);using(var tr=saved.TransactionManager.StartOpenCloseTransaction())BhtStore.ValidateHistory(tr,saved);Check("dwg-reopen-history",Records(Db,"HISTORY")==Records(saved,"HISTORY")); }
            string segment=Path.Combine(Dir,"segment02.bht");ProjectTransfer.ExportSelection(Db,segment,"DOAN_02");
            using(var target=new Database(true,true)) { ProjectTransfer.Import(target,segment);Check("segment-export-keeps-lineage",Records(Db,"HISTORY")==Records(target,"HISTORY")&&Read(target,"OBJ","OBJ002")==null); }
            Check("export-source-unchanged",before==Snapshot(Db));
            var assignmentOnly=service.Preview("OBJ001",new BhtRecord().Set("doan","DOAN_01"),CorrectionKind.Assignment,"Chuyển đoạn phù hợp phạm vi","QA");
            service.Confirm(assignmentOnly,false,true);
            Check("assignment-only-does-not-increment-survey",Read(Db,"OBJ","OBJ001").Get("survey_revision")=="2"&&Read(Db,"OBJ","OBJ001").Get("assignment_revision")=="2");
            var multi=service.Preview("OBJ002",new BhtRecord().Add("pt","ALT-A").Add("pt","ALT-B"),CorrectionKind.PointLink,"Hai điểm đo","QA");
            service.Confirm(multi,true,false);
            var supports=service.Preview("OBJ002",new BhtRecord().Set("so_tru","2"),CorrectionKind.Attributes,"Hai trụ","QA");
            Check("pole-count-recalculates-dependent-result",supports.Before.Get("ly_trinh_m")=="1030.000"&&supports.After.Get("ly_trinh_m")=="1040.000"&&supports.After.Get("station_point_id")=="ALT-B");
            service.Confirm(supports,false,false);
            Check("pole-count-correction-history",service.History("OBJ002").Count==3&&Read(Db,"OBJ","OBJ002").Get("survey_revision")=="2");
            Db.SaveAs(Path.Combine(Dir,"confirmed.dwg"),DwgVersion.AC1032);
            File.WriteAllText(Path.Combine(Dir,"current-record.txt"),Read(Db,"OBJ","OBJ001").ToCanonical());
        } catch(System.Exception ex) { fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-ERROR "+ex); }
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-TOTAL="+count+" FAIL="+fail+"\n");
    }
}
