using System;using System.IO;using System.Linq;using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;using BHT.Core;using App=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Project116Probe {
 static string Folder {get{return Environment.GetEnvironmentVariable("BHT_PROJECT116_OUT");}}
 static void Check(string name,bool ok){File.AppendAllText(Path.Combine(Folder,"result.txt"),(ok?"PASS ":"FAIL ")+name+Environment.NewLine);}
 static int Count(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);return ((BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead)).Cast<ObjectId>().Count();}}
 static void AddLine(Database db){using(var tr=db.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);var e=new Line(Point3d.Origin,new Point3d(1,2,0));ms.AppendEntity(e);tr.AddNewlyCreatedDBObject(e,true);tr.Commit();}}
 static void Compare(Database source,Database target){
  using(var st=source.TransactionManager.StartOpenCloseTransaction())using(var tt=target.TransactionManager.StartOpenCloseTransaction()){
   var sp=BhtStore.Points(st,source).OrderBy(p=>p.Id).ToArray();var tp=BhtStore.Points(tt,target).OrderBy(p=>p.Id).ToArray();
   Check("all-survey-coordinates",sp.Length==tp.Length && sp.Zip(tp,(a,b)=>a.Id==b.Id && a.X==b.X && a.Y==b.Y && a.Z==b.Z).All(b=>b));
   foreach(var sub in new[]{"OBJ","DATASET","PHOTO","FIELD_CHAIN","ROUTE"}){
    var a=BhtStore.All(st,source,sub);var b=BhtStore.All(tt,target,sub);
    Check("record-count-"+sub,a.Count==b.Count);
    Check("record-content-"+sub,a.All(r=>{
     var dest=b.FirstOrDefault(x=>x.Key==r.Key).Value;
     if(dest==null)return false;
     Func<BhtRecord,string> stable=record=>string.Join("\n",record.Pairs.Where(p=>p.Key!="handle" && p.Key!="tdt_source_handle" && p.Key!="marker_display_handle" && p.Key!="custom_block" && !p.Key.StartsWith("kh_block_")).Select(p=>p.Key+"="+p.Value));
     return stable(r.Value)==stable(dest);
    }));
   }
   foreach(var r in BhtStore.All(tt,target,"ROUTE")){
    var id=target.GetObjectId(false,new Handle(Convert.ToInt64(r.Value.Get("handle"),16)),0);
    Check("route-geometry-remapped-"+r.Key,trCurve(tt,id));
   }
  }
 }
 static bool trCurve(Transaction tr,ObjectId id){return tr.GetObject(id,OpenMode.ForRead) is Curve;}
 [CommandMethod("BHTPROJECT116PROBE")]public void Run(){
  Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"result.txt"),"");
  try{
   string input=Environment.GetEnvironmentVariable("BHT_PROJECT116_INPUT"),package=Path.Combine(Folder,"filebaohieu.bht");
   using(var source=new Database(false,true)){
    source.ReadDwgFile(input,FileOpenMode.OpenForReadAndAllShare,true,"");source.CloseInput(true);
    int total=ProjectTransfer.Export(source,package);Check("export-bht",total>0 && File.Exists(package));
    using(var verify=ProjectPackage.Open(package))Check("verified-envelope",File.Exists(verify.DrawingPath));
    using(var target=new Database(true,true)){
     target.Insunits=UnitsValue.Undefined;
     using(var tr=target.TransactionManager.StartTransaction()){BhtStore.MetaSet(tr,target,"test","configuration-only");tr.Commit();}
     Check("empty-target-adopts-source-units",ProjectTransfer.Import(target,package)==total && target.Insunits==source.Insunits);
     Compare(source,target);
     string saved=Path.Combine(Folder,"imported.dwg");target.SaveAs(saved,DwgVersion.AC1032);
     int before=Count(target);bool rejected=false;try{ProjectTransfer.Import(target,package);}catch(InvalidOperationException){rejected=true;}
     Check("existing-project-no-mutation",rejected && Count(target)==before);
     using(var reopened=new Database(false,true)){reopened.ReadDwgFile(saved,FileOpenMode.OpenForReadAndAllShare,true,"");reopened.CloseInput(true);Compare(source,reopened);}
    }
    using(var target=new Database(true,true)){
     target.Insunits=UnitsValue.Meters;AddLine(target);bool rejected=false;
     try{ProjectTransfer.Import(target,package);}catch(ProjectTransfer.UnitMismatchException){rejected=true;}
     Check("populated-unit-mismatch-no-mutation",rejected && Count(target)==1 && target.Insunits==UnitsValue.Meters);
     Check("explicit-keep-coordinates",ProjectTransfer.Import(target,package,true)==total && Count(target)==total+1 && target.Insunits==UnitsValue.Meters);
     Compare(source,target);
    }
    using(var target=new Database(true,true)){Check("legacy-DWG-import",ProjectTransfer.Import(target,input)==total);}
   }
   using(var empty=new Database(true,true)){
    using(var tr=empty.TransactionManager.StartTransaction()){BhtStore.MetaSet(tr,empty,"test","only");tr.Commit();}
    bool rejected=false;string path=Path.Combine(Folder,"empty.bht");try{ProjectTransfer.Export(empty,path);}catch(InvalidOperationException){rejected=true;}
    Check("config-only-export-rejected",rejected && !File.Exists(path));
   }
  }catch(System.Exception ex){File.AppendAllText(Path.Combine(Folder,"result.txt"),"FAIL exception "+ex+Environment.NewLine);}
 }
}
