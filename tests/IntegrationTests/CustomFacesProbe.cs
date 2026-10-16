using System;using System.IO;using System.Linq;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Runtime;using BHT.Bridge;using BHT.Core;
public class CustomFacesProbe{
 static string Dir{get{return Path.GetDirectoryName(typeof(CustomFacesProbe).Assembly.Location);}}
 [CommandMethod("CUSTOMFACECONTENT")]public void Content(){
  var a=new SignContentFace{Index=1,Code="BHT.TEXT.SAMPLE"};a.Values["custom_text"]="DỪNG LẠI\r\nQUAN SÁT";a.Values["custom_name"]="Dừng lại quan sát";
  File.WriteAllText(Path.Combine(Dir,"custom-content.xml"),SignContent.Write(new[]{a}).Replace("\r","&#13;").Replace("\n","&#10;"),new System.Text.UTF8Encoding(false));
 }
 [CommandMethod("CUSTOMFACECHECK")]public void Check(){
  var db=Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database;
  using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
   var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
   var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead);
   var insert=ms.Cast<ObjectId>().Select(id=>tr.GetObject(id,OpenMode.ForRead)).OfType<BlockReference>().Single(b=>((BlockTableRecord)tr.GetObject(b.BlockTableRecord,OpenMode.ForRead)).Name.StartsWith("BHT_ASSEMBLY_"));
   var block=(BlockTableRecord)tr.GetObject(insert.BlockTableRecord,OpenMode.ForRead);
   var refs=block.Cast<ObjectId>().Select(id=>tr.GetObject(id,OpenMode.ForRead)).OfType<BlockReference>().ToArray();
   if(refs.Length!=2)throw new System.Exception("Expected two faces");
   var custom=refs.Single(r=>((BlockTableRecord)tr.GetObject(r.BlockTableRecord,OpenMode.ForRead)).Name.Contains("CUSTOM_V0661"));
   var stop=refs.Single(r=>r!=custom);
   if(stop.GeometricExtents.MinPoint.Y<=custom.GeometricExtents.MaxPoint.Y)throw new System.Exception("STOP must be above custom");
   if(Math.Abs((custom.GeometricExtents.MaxPoint.X-custom.GeometricExtents.MinPoint.X)-1.8)>1e-7)throw new System.Exception("Wrong custom scale");
   var face=(BlockTableRecord)tr.GetObject(custom.BlockTableRecord,OpenMode.ForRead);
   var texts=face.Cast<ObjectId>().Select(id=>tr.GetObject(id,OpenMode.ForRead)).OfType<DBText>().ToArray();
   if(texts.Length!=2 || !texts.Any(t=>t.TextString=="DỪNG LẠI")||!texts.Any(t=>t.TextString=="QUAN SÁT"))throw new System.Exception("Text lost");
   foreach(var t in texts){
    var style=(TextStyleTableRecord)tr.GetObject(t.TextStyleId,OpenMode.ForRead);
    if(!style.FileName.EndsWith("giaothong1.ttf",StringComparison.OrdinalIgnoreCase) || t.WidthFactor!=1)throw new System.Exception("Wrong font or stretched text");
    if(t.GeometricExtents.MinPoint.X<-.81 || t.GeometricExtents.MaxPoint.X>.81)throw new System.Exception("Text clips frame");
   }
  }
  // Same code twice must build independent content.
  var first=new SignContentFace{Index=0,Code="BHT.TEXT.DUP"};first.Values["custom_text"]="A";
  var second=new SignContentFace{Index=1,Code=first.Code};second.Values["custom_text"]="B";
  string xml=SignContent.Write(new[]{first,second});
  if(CustomSignPlate.Ensure(db,first.Code,0,xml)==CustomSignPlate.Ensure(db,first.Code,1,xml))throw new System.Exception("Duplicate-code faces share text");
  Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCUSTOM-FACES-PASS");
 }
}
