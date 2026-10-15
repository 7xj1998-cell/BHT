using System; using System.IO; using System.Linq; using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices; using Autodesk.AutoCAD.Geometry; using Autodesk.AutoCAD.Runtime;
using BHT.Bridge; using BHT.Core;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Presentation101Probe
{
    static List<string> report = new List<string>();
    static void Check(bool ok, string text) { report.Add((ok ? "PASS " : "FAIL ") + text); }
    static List<Entity> Face(Transaction tr, ObjectId id) {
        var result = new List<Entity>();
        foreach(ObjectId c in (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead)) {
            var e = tr.GetObject(c, OpenMode.ForRead) as Entity; if(e == null) continue; result.Add(e);
            var br = e as BlockReference; if(br != null) result.AddRange(Face(tr, br.BlockTableRecord));
        } return result;
    }
    [CommandMethod("BHT101SMOKE")]
    public static void Run() {
        string folder=Environment.GetEnvironmentVariable("BHT101_QA");
        try {
            var db=AcApp.DocumentManager.MdiActiveDocument.Database;
            string[] codes={"IE.472a@500","W.207a","R.415a","I.449"}; ObjectId first=ObjectId.Null;
            for(int i=0;i<codes.Length;i++) {
                var r=TdtSignLibrary.EnsureBlock(db,codes[i]); Check(r.Ok,codes[i]+" imported "+r.Error);
                using(var tr=db.TransactionManager.StartTransaction()) {
                    var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);var face=Face(tr,bt[r.BlockName]);
                    Check(!face.Any(e=>e is DBPoint),codes[i]+" no construction point");
                    if(i==0) {
                        var text=face.OfType<MText>().ToList(); Check(text.Any(t=>t.Text.Contains("500")),"toll retains edited distance 500");
                        Check(text.All(t=>Path.GetFileName(((TextStyleTableRecord)tr.GetObject(t.TextStyleId,OpenMode.ForRead)).FileName).Equals("giaothong1.ttf",StringComparison.OrdinalIgnoreCase)),"toll captions and distance use same font");
                    }
                    var model=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
                    var apps=(RegAppTable)tr.GetObject(db.RegAppTableId,OpenMode.ForRead);if(!apps.Has("BHT_KH")){apps.UpgradeOpen();var app=new RegAppTableRecord{Name="BHT_KH"};apps.Add(app);tr.AddNewlyCreatedDBObject(app,true);}
                    var br=new BlockReference(new Point3d(i*6,0,0),bt[r.BlockName]);model.AppendEntity(br);tr.AddNewlyCreatedDBObject(br,true);
                    br.XData=new ResultBuffer(new TypedValue(1001,"BHT_KH"),new TypedValue(1000,"TEST"+i));
                    BhtStore.Write(tr,db,"OBJ","TEST"+i,new BhtRecord().Set("nhom","BIEN_BAO"));
                    if(i==0)first=br.ObjectId;
                    tr.Commit();
                }
            }
            Check(SignFillService.Apply(db,false)==4,"first toggle changes four signs");
            Check(SignFillService.Apply(db,false)==0,"repeated same setting changes nothing");
            Check(SignFillService.Apply(db,true)==4,"filled variants restored");
            using(var tr=db.TransactionManager.StartTransaction()) {
                var layers=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead); layers.UpgradeOpen();
                var locked=new LayerTableRecord{Name="QA_LOCK"};ObjectId id=layers.Add(locked);tr.AddNewlyCreatedDBObject(locked,true);
                ((Entity)tr.GetObject(first,OpenMode.ForWrite)).LayerId=id;locked.IsLocked=true;tr.Commit();
            }
            bool failed=false; try {SignFillService.Apply(db,false);}catch(Autodesk.AutoCAD.Runtime.Exception){failed=true;}
            Check(failed,"locked layer rejects batch");
            using(var tr=db.TransactionManager.StartTransaction()) {
                Check(BhtStore.Meta(tr,db,"sign_fill_all","")=="1","failed batch preserves meta");
                Check(BhtStore.Tagged(tr,db,"INSERT","BHT_KH").All(p=>!((BlockReference)tr.GetObject(p.Value,OpenMode.ForRead)).Name.Contains("_NOFILL_")),"failed batch rolls back all references");
                var layers=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);((LayerTableRecord)tr.GetObject(layers["QA_LOCK"],OpenMode.ForWrite)).IsLocked=false;tr.Commit();
            }
            string path=Path.Combine(folder,"BIEN_MAU_1.0.1.dwg"); db.Pdmode=3;db.Pdsize=.2;db.SaveAs(path,DwgVersion.AC1032);
            using(var reopened=new Database(false,true)) {
                reopened.ReadDwgFile(path,FileOpenMode.OpenForReadAndAllShare,false,"");reopened.CloseInput(true);
                Check(SignFillService.Apply(reopened,false)==4,"reopened drawing toggles off");
                Check(SignFillService.Apply(reopened,true)==4,"reopened drawing toggles on");
            }
        } catch(System.Exception ex) {Check(false,ex.ToString());}
        File.WriteAllLines(Path.Combine(folder,"presentation.txt"),report,System.Text.Encoding.UTF8);
    }
}
