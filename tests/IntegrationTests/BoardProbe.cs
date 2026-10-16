using System;using System.IO;using System.Linq;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Runtime;using Autodesk.AutoCAD.Geometry;using BHT.Bridge;using BHT.Core;
[assembly: CommandClass(typeof(BoardProbe))]
public class BoardProbe {
 [CommandMethod("BOARDPROBE")]public void Run(){
 var doc=Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
 var db=doc.Database;string title="DỪNG LẠI\r\nQUAN SÁT";
 string name=SignCorrections.NamedBoard(db,title,"Standard");
 string single=SignCorrections.NamedBoard(db,"DỪNG LẠI QUAN SÁT","Standard");
 if(name==single)throw new System.Exception("Cache collision");
 using(var tr=db.TransactionManager.StartTransaction()){
 var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
 var block=(BlockTableRecord)tr.GetObject(bt[name],OpenMode.ForRead);
 var texts=block.Cast<ObjectId>().Select(id=>tr.GetObject(id,OpenMode.ForRead)).OfType<DBText>().OrderByDescending(t=>t.AlignmentPoint.Y).ToArray();
 if(texts.Length!=2 || texts[0].TextString!="DỪNG LẠI" || texts[1].TextString!="QUAN SÁT")throw new System.Exception("Manual line break lost");
 var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
 var br=new BlockReference(Point3d.Origin,block.ObjectId);ms.AppendEntity(br);tr.AddNewlyCreatedDBObject(br,true);
 var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForWrite);
 var record=new Xrecord();record.Data=new ResultBuffer(new TypedValue(1,title));nod.SetAt("BOARD_QA",record);tr.AddNewlyCreatedDBObject(record,true);
 tr.Commit();
 }
 doc.Editor.WriteMessage("\nBOARD-CREATE-PASS");
 }
 [CommandMethod("BOARDREOPEN")]public void Reopen(){
 var db=Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database;
 using(var tr=db.TransactionManager.StartTransaction()){
 var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead);
 var record=(Xrecord)tr.GetObject(nod.GetAt("BOARD_QA"),OpenMode.ForRead);
 if((string)record.Data.AsArray()[0].Value!="DỪNG LẠI\r\nQUAN SÁT")throw new System.Exception("Stored newline lost");
 var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
 var block=bt.Cast<ObjectId>().Select(id=>tr.GetObject(id,OpenMode.ForRead)).OfType<BlockTableRecord>().First(b=>b.Name.StartsWith("BHT_BOARD_V0656_"));
 if(block.Cast<ObjectId>().Select(id=>tr.GetObject(id,OpenMode.ForRead)).OfType<DBText>().Count()!=2)throw new System.Exception("Saved board lost");
 }
 Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nBOARD-REOPEN-PASS");
 }
}
