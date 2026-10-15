using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
public class V1CacheProbe {
 static BhtDataService live;
 [LispFunction("BHTV1CACHEVALUE")] public static string CachedValue(ResultBuffer args){
  var db=Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database;
  if(live==null)live=new BhtDataService(db,()=>"",true);
  BhtRecord record;return live.GetObjects().TryGetValue("UNDO-QA",out record)?record.Get(ObjFields.Desc):"";
 }
 [LispFunction("BHTV1CACHECLOSE")] public static string CloseCache(ResultBuffer args){if(live!=null){live.Dispose();live=null;}return "OK";}
 static List<string> report=new List<string>();
 static void Check(string name,bool value){if(!value)throw new System.Exception(name);report.Add("PASS "+name);}
 static void Record(Database db,string value,bool commit,BhtDataService svc){
  using(var tr=db.TransactionManager.StartTransaction()){
   var rec=new BhtRecord();rec.Set(ObjFields.Id,"QA");rec.Set(ObjFields.Desc,value);
   BhtStore.Write(tr,db,"OBJ","QA",rec);
   if(svc!=null)Check("read-uncommitted",svc.GetObjects()["QA"].Get(ObjFields.Desc)==value);
   if(commit)tr.Commit();else tr.Abort();
  }
 }
 [CommandMethod("BHTV1CACHE")]public void Run(){
  try{
   using(var db=new Database(true,true))using(var svc=new BhtDataService(db,()=>"",true)){
    db.DisableUndoRecording(false); Check("empty-cache",svc.GetObjects().Count==0);Record(db,"first",true,null);
    Check("external-append",svc.GetObjects()["QA"].Get(ObjFields.Desc)=="first");
    var records=svc.GetObjects();records["QA"].Set(ObjFields.Desc,"polluted");records.Clear();
    Check("record-mutation-isolated",svc.GetObjects()["QA"].Get(ObjFields.Desc)=="first");
    Record(db,"second",true,null);Check("external-replacement",svc.GetObjects()["QA"].Get(ObjFields.Desc)=="second");
    Record(db,"aborted",false,svc); report.Add("rollback-value="+svc.GetObjects()["QA"].Get(ObjFields.Desc)); Check("rollback-not-cached",svc.GetObjects()["QA"].Get(ObjFields.Desc)=="second");
    using(var other=new Database(true,true))using(var os=new BhtDataService(other,()=>"",true)){
     Record(other,"other",true,null);Check("database-isolation",os.GetObjects()["QA"].Get(ObjFields.Desc)=="other"&&svc.GetObjects()["QA"].Get(ObjFields.Desc)=="second");
    }
    ObjectId pointId;
    using(var tr=db.TransactionManager.StartTransaction()){
     var reg=(RegAppTable)tr.GetObject(db.RegAppTableId,OpenMode.ForWrite);
     var app=new RegAppTableRecord{Name="BHT_PT"};reg.Add(app);tr.AddNewlyCreatedDBObject(app,true);
     var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
     var model=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
     var p=new DBPoint(new Point3d(12,34,0));pointId=model.AppendEntity(p);tr.AddNewlyCreatedDBObject(p,true);
     var values=new List<TypedValue>{new TypedValue(1001,"BHT_PT")};
     foreach(var v in new[]{"QA-P","QA","1","point","34","12","0","COC_TIEU","RTK","today"})values.Add(new TypedValue(1000,v));
     using(var rb=new ResultBuffer(values.ToArray()))p.XData=rb;
     tr.Commit();
    }
    var point=svc.GetPoint("qa-p");Check("point-index-case",point!=null&&point.X==12);
    point.X=999;svc.GetPoints().Clear();Check("point-mutation-isolated",svc.GetPoint("QA-P").X==12);
    Check("handle-index",svc.GetPointByHandle(point.Handle).Id=="QA-P");
    var ov=svc.GetOverview();ov.Warnings.Add("pollution");Check("overview-isolated",!svc.GetOverview().Warnings.Contains("pollution"));
    using(var tr=db.TransactionManager.StartTransaction()){((DBPoint)tr.GetObject(pointId,OpenMode.ForWrite)).Position=new Point3d(56,34,0);tr.Commit();}
    Check("point-move-invalidates",svc.GetPoint("QA-P").X==56);
    using(var tr=db.TransactionManager.StartTransaction()){tr.GetObject(pointId,OpenMode.ForWrite).Erase();tr.Commit();}
    Check("erase-invalidates",svc.GetPoint("QA-P")==null&&svc.GetOverview().RtkPoints==0);
    using(var tr=db.TransactionManager.StartTransaction()){tr.GetObject(pointId,OpenMode.ForWrite,true).Erase(false);tr.Commit();}
    Check("unerase-invalidates",svc.GetPoint("QA-P").X==56&&svc.GetOverview().RtkPoints==1);
   }
  }catch(System.Exception ex){report.Add("FAIL "+ex);}
  File.WriteAllLines(Path.Combine(Path.GetDirectoryName(typeof(V1CacheProbe).Assembly.Location),"cache-test.txt"),report);
 }
}

