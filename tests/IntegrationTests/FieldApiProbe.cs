using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
public class FieldApiProbe {
 [CommandMethod("BHTFIELDAPIPROBE")] public void Run() { var r=new LispApi().Call("bht:api-field-auto",new[]{"SIGN"}); Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nFIELD-API-"+(r.Ok?"OK":r.Error)); }
}
