using System;using System.IO;using System.Linq;using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;using App=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Project104Probe {
 static int Count(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction()){var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);return ((BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead)).Cast<ObjectId>().Count();}}
 [CommandMethod("BHTPROJECT104PROBE")] public void Run(){var lines=new List<string>();Action<string,bool> check=(n,ok)=>lines.Add((ok?"PASS ":"FAIL ")+n);
  var source=App.DocumentManager.MdiActiveDocument.Database;string folder=Path.Combine(Path.GetDirectoryName(typeof(Project104Probe).Assembly.Location),"project104");Directory.CreateDirectory(folder);string exported=Path.Combine(folder,"Project-"+Guid.NewGuid().ToString("N")+".dwg");
  try{
   using(var tr=source.TransactionManager.StartTransaction()){
    var bt=(BlockTable)tr.GetObject(source.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
    var reference=new Polyline();reference.AddVertexAt(0,new Point2d(0,10),0,0,0);reference.AddVertexAt(1,new Point2d(100,10),0,0,0);
    ms.AppendEntity(reference);tr.AddNewlyCreatedDBObject(reference,true);
    var marker=BhtStore.Read(tr,source,"OBJ","O2");marker.Set("marker_display_handle",reference.Handle.ToString());BhtStore.Write(tr,source,"OBJ","O2",marker);
    var meta=BhtStore.Read(tr,source,"META","CONFIG");meta.Set("symbol_road_handle",reference.Handle.ToString());BhtStore.Write(tr,source,"META","CONFIG",meta);tr.Commit();
   }
   int total=ProjectTransfer.Export(source,exported);check("export-excludes-unrelated-line",total==Count(source)-1);
   using(var target=new Database(true,true)){target.Insunits=source.Insunits;
    using(var tr=target.TransactionManager.StartTransaction()){var bt=(BlockTable)tr.GetObject(target.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);ms.AppendEntity(new Line(new Point3d(300,300,0),new Point3d(310,300,0)));tr.AddNewlyCreatedDBObject(tr.GetObject(ms.Cast<ObjectId>().Last(),OpenMode.ForWrite),true);tr.Commit();}
    check("import-preserves-unrelated-geometry",ProjectTransfer.Import(target,exported)==total && Count(target)==total+1);
    using(var tr=target.TransactionManager.StartOpenCloseTransaction()){
     var o=BhtStore.Read(tr,target,"OBJ","O1");check("object-point-route-links",o!=null && o.Get("pt")=="P1" && o.Get("route_id")=="R1");
     check("points-coordinates-preserved",BhtStore.Points(tr,target).Count==2 && BhtStore.Points(tr,target).Any(p=>p.Id=="P1" && p.X==20 && p.Y==30));
     var route=BhtStore.Read(tr,target,"ROUTE","R1");ObjectId rid=target.GetObjectId(false,new Handle(Convert.ToInt64(route.Get("handle"),16)),0);check("route-handle-remapped",tr.GetObject(rid,OpenMode.ForRead) is Polyline);
     check("field-chain-kept",BhtStore.Read(tr,target,"FIELD_CHAIN","C1").GetAll("marker").SequenceEqual(new[]{"O1","O2"}));
     check("photo-link-kept",BhtStore.Read(tr,target,"PHOTO","A1").Get("goc")=="C:/photos");
     string markerHandle=BhtStore.Read(tr,target,"OBJ","O2").Get("marker_display_handle");
     var markerRef=(Polyline)tr.GetObject(target.GetObjectId(false,new Handle(Convert.ToInt64(markerHandle,16)),0),OpenMode.ForRead);
     check("marker-display-reference-cloned-remapped",markerRef.NumberOfVertices==2 && markerRef.GetPoint2dAt(0).IsEqualTo(new Point2d(0,10)) && markerRef.GetPoint2dAt(1).IsEqualTo(new Point2d(100,10)));
     check("road-and-marker-share-remapped-reference",BhtStore.Read(tr,target,"META","CONFIG").Get("symbol_road_handle")==markerHandle);
    }
    int before=Count(target);bool rejected=false;try{ProjectTransfer.Import(target,exported);}catch(InvalidOperationException){rejected=true;}check("existing-BHT-rejected-without-mutation",rejected && Count(target)==before);
    string saved=Path.Combine(folder,"Imported-"+Guid.NewGuid().ToString("N")+".dwg");target.SaveAs(saved,DwgVersion.AC1032);
    using(var reopen=new Database(false,true)){reopen.ReadDwgFile(saved,FileOpenMode.OpenForReadAndAllShare,true,"");reopen.CloseInput(true);using(var tr=reopen.TransactionManager.StartOpenCloseTransaction()){check("save-reopen-keeps-records-points",BhtStore.Keys(tr,reopen,"OBJ").Count==2 && BhtStore.Points(tr,reopen).Count==2);}}
   }
   bool overwrite=false;try{ProjectTransfer.Export(source,exported);}catch(IOException){overwrite=true;}check("export-no-overwrite",overwrite);
  }catch(System.Exception ex){lines.Add("FAIL exception "+ex);}
  File.WriteAllLines(Path.Combine(folder,"result.txt"),lines);
 }
}
