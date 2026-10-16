using System;using System.IO;using System.Linq;using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Runtime;using BHT.Core;using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Progress125Probe
{
 [LispFunction("BHT125STAGEEXPORT")]public static string StageExport(ResultBuffer args){var db=AcApp.DocumentManager.MdiActiveDocument.Database;Label(db,"A10","Km1+050.00");ProjectTransfer.Export(db,Path.Combine(Path.GetDirectoryName(typeof(Progress125Probe).Assembly.Location),"stage10.bht"));return "OK";}
 [LispFunction("BHT125STAGEIMPORT")]public static string StageImport(ResultBuffer args){ProjectTransfer.ImportRolling(AcApp.DocumentManager.MdiActiveDocument.Database,Path.Combine(Path.GetDirectoryName(typeof(Progress125Probe).Assembly.Location),"stage10.bht"),true);return "OK";}
 [CommandMethod("PROGRESS125STAGELABEL")]public static void StageLabel(){Check("stage10-label-present-in-next-drawing",HasLabel(AcApp.DocumentManager.MdiActiveDocument.Database,"Km1+050.00"));}
 static int count,fail;static void Check(string n,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" PROGRESS125 "+n);}
 static BhtRecord Read(Database db,string id){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return BhtStore.Read(tr,db,"OBJ",id);}
 // Explicitly reopen test fixtures before editing survey content; never depend on a guard loophole.
 static void FixtureContent(Database db,string id,string content){using(var tr=db.TransactionManager.StartTransaction()){var r=Read(db,id);bool locked=ProgressPolicy.Locked(r);BhtStore.Write(tr,db,"OBJ",id,r.Clone().Set("progress_locked","0"));BhtStore.Write(tr,db,"OBJ",id,r.Clone().Set("progress_locked",locked?"1":"0").Set("sign_content",content));tr.Commit();}}
 static int Count(Database db,string kind){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return BhtStore.All(tr,db,kind).Count;}
 static int Points(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return BhtStore.Points(tr,db).Count;}
 static void Label(Database db,string id,string text){using(var tr=db.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);var rt=(RegAppTable)tr.GetObject(db.RegAppTableId,OpenMode.ForRead);if(!rt.Has("BHT_KH")){rt.UpgradeOpen();var a=new RegAppTableRecord{Name="BHT_KH"};rt.Add(a);tr.AddNewlyCreatedDBObject(a,true);}var e=new DBText {TextString=text,Height=1,Position=new Point3d(50,4,0)};ms.AppendEntity(e);tr.AddNewlyCreatedDBObject(e,true);using(var x=new ResultBuffer(new TypedValue(1001,"BHT_KH"),new TypedValue(1000,id),new TypedValue(1000,"NHAN"),new TypedValue(1000,"0"),new TypedValue(1000,"0"),new TypedValue(1000,"0"),new TypedValue(1000,"STATION")))e.XData=x;tr.Commit();}}
 static bool HasLabel(Database db,string text){using(var tr=db.TransactionManager.StartOpenCloseTransaction())return BhtStore.EntitiesOfType(tr,db,"TEXT").Any(id=>((DBText)tr.GetObject(id,OpenMode.ForRead)).TextString==text);}
 [CommandMethod("PROGRESS125PROBE")]public static void Run(){string dir=Path.GetDirectoryName(typeof(Progress125Probe).Assembly.Location);try{
  var source=AcApp.DocumentManager.MdiActiveDocument.Database;Label(source,"A10","Km1+050.00");Label(source,"B11","Km1+150.00");
  string all=Path.Combine(dir,"all.bht"),segment=Path.Combine(dir,"D10.bht");ProjectTransfer.Export(source,all);ProjectTransfer.ExportSelection(source,segment,"D10");
  using(var target=new Database(true,true)){
   ProjectTransfer.ImportRolling(target,all);Check("inherit-all-records-and-points",Count(target,"OBJ")==3&&Points(target)==3);
   Check("inherit-chainage-and-confirmation",Read(target,"A10").Get("ly_trinh_m")=="1050.000"&&ProgressPolicy.Locked(Read(target,"A10")));
   Check("station-label-survives-inheritance",HasLabel(target,"Km1+050.00")&&HasLabel(target,"Km1+150.00"));
   int routes=Count(target,"ROUTE");Check("repeat-import-is-idempotent",ProjectTransfer.ImportRolling(target,all)==0&&Count(target,"OBJ")==3&&Points(target)==3&&Count(target,"ROUTE")==routes);
   bool blocked=false;try{using(var tr=target.TransactionManager.StartTransaction()){BhtStore.Write(tr,target,"OBJ","A10",Read(target,"A10").Clone().Set("ly_trinh_m","999"));tr.Commit();}}catch(InvalidOperationException){blocked=true;}Check("dotnet-blocks-changing-confirmed-result",blocked&&Read(target,"A10").Get("ly_trinh_m")=="1050.000");
   target.SaveAs(Path.Combine(dir,"inherited.dwg"),DwgVersion.AC1032);
  }
  using(var saved=new Database(false,true)){saved.ReadDwgFile(Path.Combine(dir,"inherited.dwg"),FileOpenMode.OpenForReadAndAllShare,true,"");saved.CloseInput(true);Check("saved-dwg-reopens-with-confirmed-station-and-label",ProgressPolicy.Locked(Read(saved,"A10"))&&HasLabel(saved,"Km1+050.00"));}
  using(var target=new Database(true,true)){ProjectTransfer.ImportRolling(target,segment);Check("segment-export-only-selected-record-and-point",Count(target,"OBJ")==1&&Read(target,"B11")==null&&Points(target)==1);Check("segment-export-only-selected-label",HasLabel(target,"Km1+050.00")&&!HasLabel(target,"Km1+150.00"));Check("segment-export-selected-table-and-route",Count(target,"SEG")==1&&Count(target,"ROUTE")==1);}
  using(var target=new Database(true,true)){
   target.Insunits=source.Insunits;
   using(var tr=target.TransactionManager.StartTransaction()){
    var bt=(BlockTable)tr.GetObject(target.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);var line=new Line(Point3d.Origin,new Point3d(0,100,0));ms.AppendEntity(line);tr.AddNewlyCreatedDBObject(line,true);
    BhtStore.Write(tr,target,"ROUTE","R10",new BhtRecord().Set("route_id","R10").Set("handle",line.Handle.ToString()).Set("loai","TIM_DUONG"));BhtStore.MetaSet(tr,target,"station_active_route","R10");BhtStore.MetaSet(tr,target,"symbol_road_handle",line.Handle.ToString());BhtStore.MetaSet(tr,target,"custom_block","LOCAL_BLOCK");tr.Commit();}
   ProjectTransfer.ImportRolling(target,all);
   Check("route-name-collision-renamed-in-imported-object",Read(target,"A10").Get("route_id")=="R10_GOI1");
   using(var tr=target.TransactionManager.StartOpenCloseTransaction())Check("115-layer-uses-final-renamed-route",((Entity)tr.GetObject(CadView.IdFromHandle(target,BhtStore.Read(tr,target,"ROUTE","R10_GOI1").Get("handle")),OpenMode.ForRead)).Layer=="BHT_TUYEN_R10_GOI1");
   using(var tr=target.TransactionManager.StartOpenCloseTransaction())Check("local-active-route-and-imported-segment-preserved",BhtStore.Meta(tr,target,"station_active_route","")=="R10"&&BhtStore.Read(tr,target,"SEG","D10").Get("route_id")=="R10_GOI1"&&BhtStore.Read(tr,target,"ROUTE","R10").Get("loai")=="TIM_DUONG");
   using(var tr=target.TransactionManager.StartOpenCloseTransaction())Check("local-presentation-handles-not-remapped-as-source",BhtStore.Meta(tr,target,"symbol_road_handle","")==BhtStore.Read(tr,target,"ROUTE","R10").Get("handle")&&BhtStore.Meta(tr,target,"custom_block","")=="LOCAL_BLOCK");
   Check("renamed-route-roundtrip-can-export",ProjectTransfer.Export(target,Path.Combine(dir,"renamed.bht"))>0);
  }
  using(var target=new Database(true,true)){
   ProjectTransfer.Import(target,all,false,true);Check("survey-import-clears-confirmation-with-station",!ProgressPolicy.Locked(Read(target,"A10"))&&Read(target,"A10").Get("ly_trinh_m")=="");
   using(var tr=target.TransactionManager.StartTransaction()){BhtStore.Write(tr,target,"OBJ","A10",Read(target,"A10").Clone().Set("ghi_chu","Destination annotation"));tr.Commit();}
   ProjectTransfer.ImportRolling(target,all);Check("inherit-confirmed-result-into-existing-survey",ProgressPolicy.Locked(Read(target,"A10"))&&Read(target,"A10").Get("ly_trinh_m")=="1050.000"&&Points(target)==3);
   Check("destination-annotation-preserved-on-upgrade",Read(target,"A10").Get("ghi_chu")=="Destination annotation");
  }
  using(var target=new Database(true,true)){ProjectTransfer.Import(target,all);using(var tr=target.TransactionManager.StartTransaction()){BhtStore.Write(tr,target,"OBJ","A10",Read(target,"A10").Clone().Set("progress_locked","0"));tr.Commit();}using(var tr=target.TransactionManager.StartTransaction()){BhtStore.Write(tr,target,"OBJ","A10",Read(target,"A10").Clone().Set("ly_trinh_m","999").Set("progress_locked","1"));tr.Commit();}
   bool blocked=false;try{ProjectTransfer.ImportRolling(target,all);}catch(InvalidOperationException){blocked=true;}Check("conflicting-confirmed-results-abort",blocked&&Read(target,"A10").Get("ly_trinh_m")=="999"&&Points(target)==3);}
  using(var target=new Database(true,true)){ProjectTransfer.Import(target,all);using(var tr=target.TransactionManager.StartTransaction()){var p=BhtStore.Points(tr,target).Single(q=>q.Id=="A10-P");var id=target.GetObjectId(false,new Handle(Convert.ToInt64(p.Handle,16)),0);((DBPoint)tr.GetObject(id,OpenMode.ForWrite)).Position=new Point3d(999,999,0);tr.Commit();}bool blocked=false;try{ProjectTransfer.ImportRolling(target,all);}catch(InvalidOperationException){blocked=true;}Check("coordinate-conflict-aborts",blocked&&Points(target)==3);
   blocked=false;try{ProjectTransfer.Export(target,Path.Combine(dir,"stale.bht"));}catch(InvalidOperationException){blocked=true;}Check("moved-confirmed-point-blocks-export",blocked&&!File.Exists(Path.Combine(dir,"stale.bht")));}
  using(var target=new Database(true,true)){ProjectTransfer.ImportRolling(target,all);using(var tr=target.TransactionManager.StartTransaction()){var route=BhtStore.Read(tr,target,"ROUTE","R10");var id=target.GetObjectId(false,new Handle(Convert.ToInt64(route.Get("handle"),16)),0);((Entity)tr.GetObject(id,OpenMode.ForWrite)).TransformBy(Matrix3d.Displacement(new Vector3d(1,0,0)));tr.Commit();}bool blocked=false;try{ProjectTransfer.Export(target,Path.Combine(dir,"moved-route.bht"));}catch(InvalidOperationException){blocked=true;}Check("changed-confirmed-route-blocks-export",blocked&&!File.Exists(Path.Combine(dir,"moved-route.bht")));}
  using(var target=new Database(true,true)){
   string content="<sign-content version=\"1\"><face index=\"0\" code=\"I.419a\"><field key=\"LOCATION#0\">ĐỨC HÒA</field><field key=\"STATION#0\"></field></face></sign-content>";
   FixtureContent(source,"B11",content);
   string equivalent=Path.Combine(dir,"equivalent.bht");ProjectTransfer.Export(source,equivalent);ProjectTransfer.ImportRolling(target,equivalent);
   string variant=content.Replace("<field key=\"STATION#0\"></field>","<field key=\"STATION#0\" />").Normalize(System.Text.NormalizationForm.FormD);
   FixtureContent(target,"B11",variant);
   Check("129-equivalent-content-import-idempotent",ProjectTransfer.ImportRolling(target,equivalent)==0&&Read(target,"B11").Get("sign_content")==variant);
   FixtureContent(target,"B11",content.Replace("HÒA","HOÀ"));
   bool blocked=false;try{ProjectTransfer.ImportRolling(target,equivalent);}catch(ImportConflictException ex){blocked=ex.Conflicts.Any(c=>c.Id=="B11"&&c.Field=="sign_content"&&c.Current.Contains("HOÀ")&&c.Incoming.Contains("HÒA")&&!c.Current.Contains("<field"));}
   Check("129-actual-accent-change-still-reported-readably",blocked);
  }
  using(var target=new Database(true,true)){
   ProjectTransfer.Import(target,all,false,true);
   using(var tr=target.TransactionManager.StartTransaction()){
    BhtStore.Write(tr,target,"OBJ","A10",Read(target,"A10").Clone().Set("ma_hieu","DIFFERENT").SetAll("mat",new[]{"face-one","face-two"}));
    BhtStore.Write(tr,target,"OBJ","B11",Read(target,"B11").Clone().Set("so_tru","99"));
    var seg=BhtStore.Read(tr,target,"SEG","D10");BhtStore.Write(tr,target,"SEG","D10",seg.Clone().Set("km_end_m","8888"));
    var p=BhtStore.Points(tr,target).Single(q=>q.Id=="A10-P");var id=target.GetObjectId(false,new Handle(Convert.ToInt64(p.Handle,16)),0);
    ((DBPoint)tr.GetObject(id,OpenMode.ForWrite)).Position=new Point3d(999,999,0);tr.Commit();
   }
   string before=Read(target,"A10").ToCanonical();int beforeRoutes=Count(target,"ROUTE"),beforeObjects=Count(target,"OBJ");
   ImportConflictException report=null;try{ProjectTransfer.ImportRolling(target,all);}catch(ImportConflictException ex){report=ex;}
   Check("128-all-conflict-categories-collected",report!=null&&new[]{"A10","B11","A10-P","D10"}.All(id=>report.Conflicts.Any(c=>c.Id==id)));
   Check("128-both-coordinate-axes-reported",report!=null&&new[]{"X","Y"}.All(k=>report.Conflicts.Any(c=>c.Id=="A10-P"&&c.Field==k&&c.Current=="999")));
   Check("128-repeated-faces-retained-in-report",report!=null&&report.Conflicts.Any(c=>c.Id=="A10"&&c.Field=="mat"&&c.Current.Contains("face-one")&&c.Current.Contains("face-two")));
   Check("128-full-rejection-preserves-records-and-geometry",before==Read(target,"A10").ToCanonical()&&Count(target,"ROUTE")==beforeRoutes&&Count(target,"OBJ")==beforeObjects&&Points(target)==3);
   Check("128-report-current-incoming-direction",report!=null&&report.Conflicts.Any(c=>c.Id=="A10"&&c.Field=="ma_hieu"&&c.Current=="DIFFERENT"&&c.Incoming!="DIFFERENT"));
  }
 }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nPROGRESS125-ERROR "+ex);}AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nPROGRESS125-DOTNET="+count+" FAIL="+fail);}
}
