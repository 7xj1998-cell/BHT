using System;using System.IO;using System.Linq;using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Runtime;using Autodesk.AutoCAD.EditorInput;
using BHT.Bridge;using App=Autodesk.AutoCAD.ApplicationServices.Core.Application;
[assembly:CommandClass(typeof(Selection105Probe))]
public class Selection105Probe {
 static List<string> lines=new List<string>();
 static void Check(string name,bool ok){lines.Add((ok?"PASS ":"FAIL ")+name);}
 static ObjectId[] Selection(){var s=App.DocumentManager.MdiActiveDocument.Editor.SelectImplied();return s.Status==PromptStatus.OK?s.Value.GetObjectIds():new ObjectId[0];}
 static ObjectId Seed(Database db,string key){using(var tr=db.TransactionManager.StartOpenCloseTransaction()){return DisplaySelection.Collect(db,null).First(id=>{var e=tr.GetObject(id,OpenMode.ForRead) as BlockReference;if(e==null)return false;using(var data=e.GetXDataForApplication("BHT_KH")){return data!=null&&data.AsArray().Any(v=>v.TypeCode==1000&&Convert.ToString(v.Value)==key);}});}}
 static void Save(){File.WriteAllLines(Path.Combine(Path.GetDirectoryName(typeof(Selection105Probe).Assembly.Location),"selection105-result.txt"),lines);}
 [CommandMethod("BHTSELECTION105ALL",CommandFlags.UsePickSet|CommandFlags.Redraw)]public static void All(){
  var db=App.DocumentManager.MdiActiveDocument.Database;
  Check("all-selection-survives-command-return",Selection().OrderBy(id=>id.Handle.Value).SequenceEqual(DisplaySelection.Collect(db,null).OrderBy(id=>id.Handle.Value)));
  Check("all-selection-has-components",Selection().Length>=9);
  Check("all-enables-PICKFIRST",Convert.ToInt32(App.GetSystemVariable("PICKFIRST"))==1);
  var seed=Seed(db,"O1");var groups=DisplaySelection.Groups(db,new[]{seed});Check("seed-group-resolved",groups.SequenceEqual(new[]{"BIEN_BAO"}));
  var ed=App.DocumentManager.MdiActiveDocument.Editor;
  App.SetSystemVariable("SELECTSIMILARMODE",130);
  ed.SetImpliedSelection(new ObjectId[0]);
  ed.Command("_.SELECTSIMILAR",SelectionSet.FromObjectIds(new[]{seed}),"");
  Check("native-130-reproduces-missing-other-block",Selection().Contains(seed)&&!Selection().Contains(Seed(db,"O2")));
  var matches=DisplaySelection.Collect(db,groups);using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
   Check("same-group-has-blocks-texts-leaders",matches.Any(id=>tr.GetObject(id,OpenMode.ForRead) is BlockReference)&&matches.Any(id=>tr.GetObject(id,OpenMode.ForRead) is DBText)&&matches.Any(id=>tr.GetObject(id,OpenMode.ForRead) is Curve));
   Check("same-group-excludes-light",!matches.Contains(Seed(db,"L1")));
  }
  App.DocumentManager.MdiActiveDocument.Editor.SetImpliedSelection(new[]{seed});Save();
 }
 [CommandMethod("BHTSELECTION105SIMILAR",CommandFlags.UsePickSet|CommandFlags.Redraw)]public static void Similar(){
  var doc=App.DocumentManager.MdiActiveDocument;var expected=DisplaySelection.Collect(doc.Database,new[]{"BIEN_BAO"});
  Check("group-selection-survives-command-return",Selection().OrderBy(id=>id.Handle.Value).SequenceEqual(expected.OrderBy(id=>id.Handle.Value)));
  doc.Editor.SetImpliedSelection(new[]{Seed(doc.Database,"O1")});Save();
 }
 [CommandMethod("BHTSELECTION105NATIVE",CommandFlags.UsePickSet|CommandFlags.Redraw)]public static void Native(){
  var doc=App.DocumentManager.MdiActiveDocument;var selected=Selection();
  Check("native-layer-mask",Convert.ToInt32(App.GetSystemVariable("SELECTSIMILARMODE"))==2);
  Check("native-selects-different-sign-block-names",selected.Contains(Seed(doc.Database,"O1"))&&selected.Contains(Seed(doc.Database,"O2")));
  using(var tr=doc.Database.TransactionManager.StartOpenCloseTransaction()) {
   Check("native-layer-selection-only-same-entity-type",selected.Length>0&&selected.All(id=>tr.GetObject(id,OpenMode.ForRead) is BlockReference));
   Check("native-excludes-other-layer",selected.All(id=>((Entity)tr.GetObject(id,OpenMode.ForRead)).Layer=="BHT_KYHIEU"));
  }
  Save();
 }
}
