using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using BHT.Palette;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class Incremental131Probe
{
    static int count,fail;
    static string Dir { get { return Path.GetDirectoryName(typeof(Incremental131Probe).Assembly.Location); } }
    static void Check(string name,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" INCREMENTAL131 "+name);}
    static BhtRecord Read(Database db,string sub,string key){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return BhtStore.Read(tr,db,sub,key);}
    static string Snapshot(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return string.Join("\n",new[]{"OBJ","SEG","ROUTE","META"}.SelectMany(s=>BhtStore.All(tr,db,s).OrderBy(p=>p.Key).Select(p=>s+"/"+p.Key+"="+p.Value.ToCanonical())))+"\n"+string.Join("\n",BhtStore.Points(tr,db).OrderBy(p=>p.Id).Select(p=>p.Id+":"+p.X+","+p.Y+","+p.Z));}
    static IEnumerable<Control> All(Control control){foreach(Control c in control.Controls){yield return c;foreach(var child in All(c))yield return child;}}
    static Button Button(Form form,string text){return All(form).OfType<Button>().Single(b=>b.Text==text);}
    static void Write(Database db,string sub,string key,BhtRecord r){using(var tr=db.TransactionManager.StartTransaction()){BhtStore.Write(tr,db,sub,key,r);tr.Commit();}}
    static ObjectId Label(Database db){using(var tr=db.TransactionManager.StartTransaction()){
        var apps=(RegAppTable)tr.GetObject(db.RegAppTableId,OpenMode.ForRead);if(!apps.Has("BHT_KH")){apps.UpgradeOpen();var app=new RegAppTableRecord{Name="BHT_KH"};apps.Add(app);tr.AddNewlyCreatedDBObject(app,true);}
        var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
        var label=new DBText{TextString="manual label",Position=new Point3d(7,8,0),Height=1};var id=ms.AppendEntity(label);tr.AddNewlyCreatedDBObject(label,true);
        using(var tag=new ResultBuffer(new TypedValue(1001,"BHT_KH"),new TypedValue(1000,"OBJ001"),new TypedValue(1000,"NHAN"),new TypedValue(1000,"TAY")))label.XData=tag;tr.Commit();return id;
    }}
    static void RenameFixture(Database db){using(var tr=db.TransactionManager.StartTransaction()){
        var objects=BhtStore.All(tr,db,"OBJ").Where(p=>p.Value.Get("doan")=="DOAN_08").ToArray();
        foreach(var p in objects)BhtStore.Write(tr,db,"OBJ",p.Key,p.Value.Clone().Set("progress_locked","0"));
        BhtStore.Write(tr,db,"SEG","DOAN_08",BhtStore.Read(tr,db,"SEG","DOAN_08").Clone().Set("package_id","GOI_01"));
        foreach(var p in objects)BhtStore.Write(tr,db,"OBJ",p.Key,p.Value.Clone().Set("goi","GOI_01"));tr.Commit();
    }}
    static void Ui(){
        var seg=new BhtRecord().Set("segment_id","DOAN_08").Set("route_id","R08").Set("km_start_m","1000").Set("km_end_m","1100");
        var locked=Enumerable.Range(1,25).Select(i=>new BhtRecord().Set("object_id","OBJ"+i).Set("doan","DOAN_08").Set("progress_locked","1")).ToList();
        foreach(bool pending in new[]{false,true}){
            var objects=locked.Select(r=>r.Clone()).ToList();if(pending)objects.Add(new BhtRecord().Set("object_id","OBJ026").Set("doan","DOAN_08"));
            using(var form=new ProgressForm(new List<BhtRecord>{seg},objects,new[]{"R08","R11"})){
                form.Location=new System.Drawing.Point(-3000,-3000);form.StartPosition=FormStartPosition.Manual;form.Show();Application.DoEvents();
                var grid=All(form).OfType<DataGridView>().Single();grid.Rows[0].Selected=true;Application.DoEvents();
                Check("ui-incremental-calculate-"+pending,Button(form,"Tính lý trình theo đoạn").Enabled);
                Check("ui-finish-only-pending-"+pending,Button(form,"Xác nhận kết quả đoạn").Enabled==pending);
                Check("ui-export-only-fully-confirmed-"+pending,Button(form,"Xuất hồ sơ đoạn (.bht)…").Enabled==!pending);
                All(form).OfType<ComboBox>().Single().SelectedItem="R11";Application.DoEvents();
                Check("ui-reject-different-route-"+pending,!Button(form,"Tính lý trình theo đoạn").Enabled);
            }
        }
    }
    [CommandMethod("INCREMENTAL131PROBE")]
    public static void Run(){try{
        var source=AcApp.DocumentManager.MdiActiveDocument.Database;
        string original=Snapshot(source),path=Path.Combine(Dir,"confirmed.bht");ProjectTransfer.Export(source,path);
        Check("export-keeps-source-canonical",original==Snapshot(source));
        source.SaveAs(Path.Combine(Dir,"confirmed.dwg"),DwgVersion.AC1032);
        using(var saved=new Database(false,true)){saved.ReadDwgFile(Path.Combine(Dir,"confirmed.dwg"),FileOpenMode.OpenForReadAndAllShare,true,"");saved.CloseInput(true);Check("save-reopen-canonical",original==Snapshot(saved));}
        var validSnapshot=Read(source,"OBJ","OBJ001");
        foreach(int coordinate in new[]{1,2,3}){
            var malformed=validSnapshot.GetAll("progress_point").First().Split('|');malformed[coordinate]="NaN";
            Write(source,"OBJ","OBJ001",validSnapshot.Clone().SetAll("progress_point",new[]{string.Join("|",malformed)}));
            string invalidPath=Path.Combine(Dir,"invalid-point-"+coordinate+".bht");bool blocked=false;
            try{ProjectTransfer.Export(source,invalidPath);}catch(InvalidOperationException){blocked=true;}
            Write(source,"OBJ","OBJ001",validSnapshot);
            Check("nonfinite-confirmed-coordinate-rejected-"+coordinate,blocked&&!File.Exists(invalidPath)&&original==Snapshot(source));
        }
        foreach(string field in new[]{"ma_hieu","doan","goi","so_mat","sign_content","station_point_method"}){
            bool blocked=false;try{Write(source,"obj","obj001",Read(source,"OBJ","OBJ001").Clone().Set(field,"BAD"));}catch(InvalidOperationException){blocked=true;}
            Check("dotnet-case-insensitive-guard-"+field,blocked&&original==Snapshot(source));
        }
        using(var target=new Database(true,true)){
            target.DisableUndoRecording(false);
            ProjectTransfer.ImportRolling(target,path);RenameFixture(target);string before=Snapshot(target);
            Check("verified-package-alias-repeat",ProjectTransfer.ImportRolling(target,path)==0&&before==Snapshot(target));
            var pending=Read(source,"OBJ","OTHER11").Clone().Set("object_id","NEW031").Set("doan","DOAN_08").Set("goi","GOI01");
            Write(source,"OBJ","NEW031",pending);string next=Path.Combine(Dir,"additional.bht");ProjectTransfer.Export(source,next);
            ProjectTransfer.ImportRolling(target,next);
            Check("new-object-package-agrees-with-seg",Read(target,"OBJ","NEW031").Get("goi")==Read(target,"SEG","DOAN_08").Get("package_id")&&Read(target,"OBJ","NEW031").Get("goi")=="GOI_01");
            before=Snapshot(target);Check("repeat-addition-is-idempotent",ProjectTransfer.ImportRolling(target,next)==0&&before==Snapshot(target));
            // A deliberate fault after a record write must abort the DWG transaction.
            try{using(var tr=target.TransactionManager.StartTransaction()){BhtStore.Write(tr,target,"OBJ","NEW031",pending.Clone().Set("ma_hieu","BAD"));throw new InvalidOperationException("QA fault");}}catch(InvalidOperationException){}
            string after=Snapshot(target);if(before!=after){File.WriteAllText(Path.Combine(Dir,"rollback-before.txt"),before);File.WriteAllText(Path.Combine(Dir,"rollback-after.txt"),after);}
            Check("dotnet-write-fault-rolls-back",before==after);
            var upgraded=Read(source,"OBJ","OBJ028").Clone().Set("object_id","NEWCONF");
            Write(source,"OBJ","NEWCONF",upgraded);string confirmed=Path.Combine(Dir,"new-confirmed.bht");ProjectTransfer.Export(source,confirmed);
            var old=Read(target,"OBJ","OBJ001").ToCanonical();ProjectTransfer.ImportRolling(target,confirmed);
            Check("confirmed-addition-reuses-bound-route",ProgressPolicy.Locked(Read(target,"OBJ","NEWCONF"))&&Read(target,"OBJ","NEWCONF").Get("route_id")==Read(target,"SEG","DOAN_08").Get("route_id"));
            Check("confirmed-addition-keeps-existing-locked-record",old==Read(target,"OBJ","OBJ001").ToCanonical());
            Check("confirmed-addition-reexports",ProjectTransfer.Export(target,Path.Combine(Dir,"reexport.bht"))>0);
        }
        using(var target=new Database(true,true)){
            ProjectTransfer.Import(target,path,false,true);var seg=Read(target,"SEG","DOAN_08");
            Write(target,"SEG","DOAN_08",seg.Clone().Set("package_id","GOI_01").Set("km_end_m","9999"));string before=Snapshot(target);bool blocked=false;
            try{ProjectTransfer.ImportRolling(target,path);}catch(ImportConflictException ex){blocked=ex.Conflicts.Any(c=>c.Field=="km_end_m")&&ex.Conflicts.Any(c=>c.Field=="package_id");}
            Check("package-change-with-other-scope-blocked-atomically",blocked&&before==Snapshot(target));
        }
        using(var target=new Database(true,true)){
            ProjectTransfer.Import(target,path,false,true);var label=Label(target);string before=Snapshot(target);bool recording=target.UndoRecording;
            target.Insunits=source.Insunits==UnitsValue.Feet?UnitsValue.Meters:UnitsValue.Feet;
            bool blocked=false;try{ProjectTransfer.ImportRolling(target,path);}catch(ProjectTransfer.UnitMismatchException){blocked=true;}
            using(var tr=target.TransactionManager.StartOpenCloseTransaction()){
                var text=tr.GetObject(label,OpenMode.ForRead,false) as DBText;
                Check("import-failure-after-erasure-restores-cad",blocked&&!label.IsErased&&text!=null&&text.TextString=="manual label"&&text.Position==new Point3d(7,8,0));
            }
            Check("import-failure-restores-records-and-undo-mode",blocked&&before==Snapshot(target)&&target.UndoRecording==recording);
        }
        Ui();
    }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nINCREMENTAL131-ERROR "+ex);}
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nINCREMENTAL131-DOTNET="+count+" FAIL="+fail);
    }
}
