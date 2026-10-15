using System;using System.IO;using System.Linq;using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Runtime;using BHT.Bridge;
using App=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class SampleProject104Probe {
 [CommandMethod("BHTSAMPLEPROJECT104PROBE")] public void Run(){
  var lines=new List<string>();Action<string,bool> check=(n,ok)=>lines.Add((ok?"PASS ":"FAIL ")+n);
  string folder=Environment.GetEnvironmentVariable("BHT_PROJECT104_OUTPUT");Directory.CreateDirectory(folder);
  try{var source=App.DocumentManager.MdiActiveDocument.Database;string export=Path.Combine(folder,"BHT-du-an-"+Guid.NewGuid().ToString("N")+".dwg");int total=ProjectTransfer.Export(source,export);
   using(var target=new Database(true,true)){target.Insunits=source.Insunits;check("sample-import-entity-count",ProjectTransfer.Import(target,export)==total);
    using(var a=source.TransactionManager.StartOpenCloseTransaction())using(var b=target.TransactionManager.StartOpenCloseTransaction()){
     var points=BhtStore.Points(a,source).OrderBy(p=>p.Id).ToList();var copied=BhtStore.Points(b,target).OrderBy(p=>p.Id).ToList();
     check("sample-all-points-and-original-coordinates",points.Count==copied.Count && points.Zip(copied,(x,y)=>x.Id==y.Id && x.X==y.X && x.Y==y.Y && x.Z==y.Z && x.Description==y.Description).All(x=>x));
     var root=BhtStore.RootDict(a,source,false);
     foreach(DBDictionaryEntry sub in root){if(!(a.GetObject(sub.Value,OpenMode.ForRead) is DBDictionary))continue;
      var rows=BhtStore.All(a,source,sub.Key);var received=BhtStore.All(b,target,sub.Key);
      check("sample-records-"+sub.Key,rows.Count==received.Count && rows.All(row=>{
       var r=BhtStore.Read(b,target,sub.Key,row.Key);if(r==null)return false;
       return row.Value.Pairs.Where(p=>p.Key!="handle" && p.Key!="tdt_source_handle" && p.Key!="custom_block" && !p.Key.StartsWith("kh_block_")).SequenceEqual(r.Pairs.Where(p=>p.Key!="handle" && p.Key!="tdt_source_handle" && p.Key!="custom_block" && !p.Key.StartsWith("kh_block_")));}));
     }
    }
    string imported=Path.Combine(folder,"BHT-nap-du-an-"+Guid.NewGuid().ToString("N")+".dwg");target.SaveAs(imported,DwgVersion.AC1032);
    using(var reopen=new Database(false,true)){reopen.ReadDwgFile(imported,FileOpenMode.OpenForReadAndAllShare,true,"");reopen.CloseInput(true);using(var tr=reopen.TransactionManager.StartOpenCloseTransaction())check("sample-import-save-reopen",BhtStore.Keys(tr,reopen,"OBJ").Count==177 && BhtStore.Points(tr,reopen).Count==801);}
    lines.Add("DWG="+imported);lines.Add("EXPORT="+export);
   }
  }catch(System.Exception ex){lines.Add("FAIL exception "+ex);}
  File.WriteAllLines(Path.Combine(folder,"project-result.txt"),lines);
 }
}
