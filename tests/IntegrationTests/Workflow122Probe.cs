using System;using System.IO;using System.Linq;using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Runtime;
using BHT.Core;using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public static class Workflow122Probe {
 static int count,fail;
 static void Check(string name,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" WORKFLOW122 "+name);}
 static ObjectId Route(Database db,string id){using(var tr=db.TransactionManager.StartTransaction()){
  var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
  var line=new Polyline();line.AddVertexAt(0,new Point2d(0,0),0,0,0);line.AddVertexAt(1,new Point2d(1000,0),0,0,0);ms.AppendEntity(line);tr.AddNewlyCreatedDBObject(line,true);
  BhtStore.Write(tr,db,"ROUTE",id,new BhtRecord().Set("route_id",id).Set("handle",line.Handle.ToString()).Set("loai","TIM_RANH").Set("geometry_signature",line.Handle+"|1000|O|0|1|fixture"));
  tr.Commit();return line.ObjectId;}}
 static void Tag(Database db,Transaction tr,Entity e,string name,params string[] values){var apps=(RegAppTable)tr.GetObject(db.RegAppTableId,OpenMode.ForRead);if(!apps.Has(name)){apps.UpgradeOpen();var a=new RegAppTableRecord{Name=name};apps.Add(a);tr.AddNewlyCreatedDBObject(a,true);}
  using(var data=new ResultBuffer()){data.Add(new TypedValue(1001,name));foreach(string v in values)data.Add(new TypedValue(1000,v));e.XData=data;}}
 [CommandMethod("WORKFLOW122REALROUTE")]public static void RealRoute(){try{
  var db=AcApp.DocumentManager.MdiActiveDocument.Database;
  using(var source=new Database(false,true)){source.ReadDwgFile(Environment.GetEnvironmentVariable("BHT_WORKFLOW_REFERENCE"),FileOpenMode.OpenForReadAndAllShare,true,"");source.CloseInput(true);
   using(var st=source.TransactionManager.StartOpenCloseTransaction())using(var tt=db.TransactionManager.StartTransaction()){
    var rec=BhtStore.Read(st,source,"ROUTE","TIMRANHD8").Clone();var ent=source.GetObjectId(false,new Handle(Convert.ToInt64(rec.Get("handle"),16)),0);
    var bt=(BlockTable)tt.GetObject(db.BlockTableId,OpenMode.ForRead);var map=new IdMapping();source.WblockCloneObjects(new ObjectIdCollection(new[]{ent}),bt[BlockTableRecord.ModelSpace],map,DuplicateRecordCloning.MangleName,false);
    string handle=map[ent].Value.Handle.ToString(),signature=rec.Get("geometry_signature");rec.Set("handle",handle).Set("geometry_signature",handle+signature.Substring(signature.IndexOf('|'))).Set("loai","TIM_RANH");
    // Keep the supplied, confirmed controls for this geometry; native proxy scanning is a separate check.
    BhtStore.Write(tt,db,"ROUTE","TIMRANHD8",rec);tt.Commit();
   }}
  ProjectTransfer.Import(db,Environment.GetEnvironmentVariable("BHT_WORKFLOW_PACKAGE"),true,true);
  AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nWORKFLOW122-REAL-READY");
 }catch(System.Exception ex){AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nWORKFLOW122-ERROR "+ex);}}
 [CommandMethod("WORKFLOW122PROBE")]public static void Run(){try{
  string folder=Path.GetDirectoryName(typeof(Workflow122Probe).Assembly.Location),path=Path.Combine(folder,"source122.bht");
  using(var source=new Database(true,true)){
   var sourceRoad=Route(source,"OLD122");using(var tr=source.TransactionManager.StartTransaction()){
    BhtStore.Write(tr,source,"OBJ","SIGN122",new BhtRecord().Set("nhom","BIEN_BAO").Set("ma_hieu","W.239a").SetAll("mat",new[]{"W.239a","S.509a@DESC_3=5m"}).Set("route_id","OLD122").Set("ly_trinh_m","42000").Set("ly_trinh_km","Km42+000.00").Set("doan","OLDSEG").Set("offset_m","9.9").Set("station_source","FIELD_MARKERS"));
    BhtStore.Write(tr,source,"OBJ","MARK122",new BhtRecord().Set("nhom","COC_TIEU").Set("marker_km","42").Set("marker_h","1"));
    BhtStore.Write(tr,source,"SEG","OLDSEG",new BhtRecord().Set("segment_id","OLDSEG").Set("package_id","GOI122").Set("route_id","OLD122").Set("km_start_m","42000").Set("km_end_m","43000").Set("side","TRAI").Set("commune","Xã thử"));
    BhtStore.Write(tr,source,"META","CONFIG",new BhtRecord().Set("station_method","FIELD_MARKERS").Set("thu_muc_anh","F:/BHT_ANH").Set("sign_scale","3").Set("symbol_road_handle",sourceRoad.Handle.ToString()));
    var bt=(BlockTable)tr.GetObject(source.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
    var p=new DBPoint(new Point3d(100,2,5));ms.AppendEntity(p);tr.AddNewlyCreatedDBObject(p,true);Tag(source,tr,p,"BHT_PT","P122");
    var label=new DBText{TextString="Km42+000.00",Position=new Point3d(100,2,0),Height=1};ms.AppendEntity(label);tr.AddNewlyCreatedDBObject(label,true);Tag(source,tr,label,"BHT_KH","SIGN122","NHAN","0","0","0","STATION");
    tr.Commit();}ProjectTransfer.Export(source,path);
  }
  using(var target=new Database(true,true)){
   var dest=Route(target,"DEST122");ProjectTransfer.Import(target,path,true,true);
   using(var tr=target.TransactionManager.StartOpenCloseTransaction()){
    var rows=BhtStore.All(tr,target,"ROUTE");var rec=BhtStore.Read(tr,target,"OBJ","SIGN122");
    Check("survey-import-preserves-destination-route-only",rows.Count==1&&rows[0].Key=="DEST122"&&BhtStore.Read(tr,target,"ROUTE","OLD122")==null);
    Check("source-station-cleared-and-auditable",rec.Get("ly_trinh_m")==""&&rec.Get("offset_m")==""&&rec.Get("import_source_ly_trinh_m")=="42000");
    Check("auxiliary-faces-preserved",rec.GetAll("mat").Count==2&&rec.GetAll("mat")[1].Contains("5m"));
    Check("segment-table-retained-without-old-route-or-mode",BhtStore.Read(tr,target,"SEG","OLDSEG").Get("route_id")==""&&BhtStore.Read(tr,target,"SEG","OLDSEG").Get("import_source_route_id")=="OLD122"&&BhtStore.Meta(tr,target,"station_method","")!="FIELD_MARKERS");
    Check("segment-range-package-side-and-Unicode-retained",BhtStore.Read(tr,target,"SEG","OLDSEG").Get("km_start_m")=="42000"&&BhtStore.Read(tr,target,"SEG","OLDSEG").Get("package_id")=="GOI122"&&BhtStore.Read(tr,target,"SEG","OLDSEG").Get("side")=="TRAI"&&BhtStore.Read(tr,target,"SEG","OLDSEG").Get("commune")=="Xã thử");
    Check("imported-markers-not-station-controls",BhtStore.Read(tr,target,"OBJ","MARK122").Get("station_control_eligible")=="0");
    Check("observed-marker-number-retained",BhtStore.Read(tr,target,"OBJ","MARK122").Get("ly_trinh_m")=="42100.000");
    var bt=(BlockTable)tr.GetObject(target.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead);
    Check("old-station-caption-not-cloned",!ms.Cast<ObjectId>().Any(id=>tr.GetObject(id,OpenMode.ForRead) is DBText));
    Check("survey-import-does-not-clone-source-road-from-display-settings",ms.Cast<ObjectId>().Count(id=>tr.GetObject(id,OpenMode.ForRead) is Curve)==1);
    Check("survey-point-coordinate-unchanged",ms.Cast<ObjectId>().Select(id=>tr.GetObject(id,OpenMode.ForRead)).OfType<DBPoint>().Single().Position==new Point3d(100,2,5));
    Check("photo-folder-and-symbol-scale-survive-without-old-method",BhtStore.Meta(tr,target,"thu_muc_anh","")=="F:/BHT_ANH"&&BhtStore.Meta(tr,target,"sign_scale","")=="3"&&BhtStore.Meta(tr,target,"station_method","")!="FIELD_MARKERS");
   }
   bool rejected=false;try{ProjectTransfer.Import(target,path,true,true);}catch(InvalidOperationException){rejected=true;}Check("second-survey-import-rejected-without-ID-overwrite",rejected);
  }
  using(var target=new Database(true,true)){
   var destinationRoad=Route(target,"DEST_SEG122");using(var tr=target.TransactionManager.StartTransaction()){
    BhtStore.Write(tr,target,"SEG","OLDSEG",new BhtRecord().Set("route_id","DEST_SEG122").Set("package_id","DESTPACKAGE"));
    BhtStore.MetaSet(tr,target,"station_method","ROUTE");BhtStore.MetaSet(tr,target,"thu_muc_anh","D:/DEST_PHOTOS");BhtStore.MetaSet(tr,target,"symbol_road_handle",destinationRoad.Handle.ToString());BhtStore.MetaSet(tr,target,"custom_block","DEST_BLOCK");tr.Commit();}
   ProjectTransfer.Import(target,path,true,true);using(var tr=target.TransactionManager.StartOpenCloseTransaction()){
    Check("existing-destination-segment-and-photo-folder-not-overwritten",BhtStore.Read(tr,target,"SEG","OLDSEG").Get("route_id")=="DEST_SEG122"&&BhtStore.Read(tr,target,"SEG","OLDSEG").Get("package_id")=="DESTPACKAGE"&&BhtStore.Meta(tr,target,"thu_muc_anh","")=="D:/DEST_PHOTOS");
    Check("destination-road-and-custom-block-settings-not-remapped-as-source",BhtStore.Meta(tr,target,"symbol_road_handle","")==destinationRoad.Handle.ToString()&&BhtStore.Meta(tr,target,"custom_block","")=="DEST_BLOCK");}}
  using(var target=new Database(true,true)){ProjectTransfer.Import(target,path);using(var tr=target.TransactionManager.StartOpenCloseTransaction()){
   var rec=BhtStore.Read(tr,target,"ROUTE","OLD122");Check("full-project-import-remaps-geometry-signature",rec.Get("geometry_signature").StartsWith(rec.Get("handle")+"|"));
   Check("full-project-export-import-retains-segment-route-binding",BhtStore.Read(tr,target,"SEG","OLDSEG").Get("route_id")=="OLD122");
   Check("export-summary-discloses-segment-count",ProjectTransfer.ContentSummary(target).Contains("1 đoạn/gói thầu"));}}
  // The scanner must not treat a BHT chainage caption or numbered-marker block as a new control.
  using(var db=new Database(true,true)){
   var id=Route(db,"SCAN122");using(var tr=db.TransactionManager.StartTransaction()){
    var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
    foreach(string text in new[]{"Km50+140","Km42+000"}){var label=new DBText{TextString=text,Position=new Point3d(text.Contains("50")?0:100,0,0),Height=1};ms.AppendEntity(label);tr.AddNewlyCreatedDBObject(label,true);if(text.Contains("42"))Tag(db,tr,label,"BHT_KH","SIGN122","NHAN");}tr.Commit();}
   var scanned=Tdt91Stakes.Scan(db,id.Handle.ToString(),100);Check("scan-excludes-BHT-caption-retains-real-Km-text",scanned.Count==1&&scanned[0].Station==50140);
  }
  string real=Environment.GetEnvironmentVariable("BHT_WORKFLOW_PACKAGE");if(!string.IsNullOrEmpty(real))using(var target=new Database(true,true)){
   Route(target,"DEST_REAL122");ProjectTransfer.Import(target,real,true,true);using(var tr=target.TransactionManager.StartOpenCloseTransaction()){
    var rows=BhtStore.All(tr,target,"OBJ");Check("real-filebaohieu-imports-179-records",rows.Count==179);
    Check("real-filebaohieu-has-no-segment-table",BhtStore.All(tr,target,"ROUTE").Count==1&&BhtStore.All(tr,target,"SEG").Count==0&&BhtStore.All(tr,target,"FIELD_CHAIN").Count==0);
    Check("real-signs-wait-for-destination-chainage",rows.Where(r=>r.Value.Get("nhom")!="COC_TIEU"&&r.Value.Get("nhom")!="COT_KM").All(r=>r.Value.Get("ly_trinh_m")==""&&r.Value.Get("route_id")==""));
   }}
  string segments=Environment.GetEnvironmentVariable("BHT_WORKFLOW_SEGMENTS"),complete=Environment.GetEnvironmentVariable("BHT_WORKFLOW_COMPLETE");
  if(!string.IsNullOrEmpty(real)&&!string.IsNullOrEmpty(segments)&&!string.IsNullOrEmpty(complete))using(var source=new Database(true,true)){
   ProjectTransfer.Import(source,real);var rows=SegmentTable.Read(segments);SegmentStore.Apply(source,rows,segments,true);ProjectTransfer.Export(source,complete);
   using(var target=new Database(true,true)){ProjectTransfer.Import(target,complete);var roundtrip=SegmentStore.Read(target);
    Check("real-15-segment-export-import-lossless",rows.Count==15&&rows.OrderBy(r=>r.Get("segment_id")).Select(r=>r.ToCanonical()).SequenceEqual(roundtrip.OrderBy(r=>r.Get("segment_id")).Select(r=>r.ToCanonical())));}
   using(var target=new Database(true,true)){Route(target,"DEST_COMPLETED122");ProjectTransfer.Import(target,complete,true,true);
    Check("completed-package-survey-import-retains-15-segments",SegmentStore.Read(target).Count==15);}
  }
 }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nWORKFLOW122-ERROR "+ex);}
 AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nWORKFLOW122-DOTNET-CHECKS="+count+" FAIL="+fail);}
}
