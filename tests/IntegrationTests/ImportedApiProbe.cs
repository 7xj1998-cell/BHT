using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
public class ImportedApiProbe {
 [CommandMethod("BHTIMPORTEDAPIPROBE")] public void Run() {
  var api=new LispApi();var preview=api.Call("bht:api-field-input-preview");
  bool ok=preview.Ok&&preview.Values.Count>=3&&preview.Values[0].Length>1000;
  if(ok)ok=api.Call("bht:api-field-input-use",preview.Values[0]).Ok;
  using(var service=new BhtDataService(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database,()=>"",false)) {
   var rec=service.GetObject("HAND");var points=new System.Collections.Generic.List<BHT.Core.SurveyPoint>();foreach(var id in rec.GetAll("pt"))points.Add(service.GetPoint(id));
   ok=ok&&BHT.Core.MarkerStation.ManualControlApproved(rec,points);
   points[0].X+=1;ok=ok&&!BHT.Core.MarkerStation.ManualControlApproved(rec,points);
  }
  Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nINPUT-API-"+(ok?"PASS":"FAIL"));
 }
}
