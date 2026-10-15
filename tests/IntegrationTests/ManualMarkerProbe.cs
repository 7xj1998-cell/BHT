using System;
using System.Linq;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using BHT.Bridge;
using BHT.Core;
using App=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class ManualMarkerProbe
{
    static void Check(string name,bool ok){if(!ok)throw new System.Exception(name);App.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nMANUAL-PASS "+name);}
    [CommandMethod("BHTMANUALPROBE")]
    public void Run()
    {
        var db=App.DocumentManager.MdiActiveDocument.Database;
        try { using(var s=new BhtDataService(db,()=>"",false)) {
            int before=s.GetPoints().Count, objects=s.GetObjects().Count;
            Check("invalid-number-rejected",!ManualMarker.Create(db,new Point3d(10,20,0),"COC_TIEU","4","10","").Ok);
            Check("failure-no-orphans",before==s.GetPoints().Count && objects==s.GetObjects().Count);
            var r=ManualMarker.Create(db,new Point3d(10,20,998),"COC_TIEU","40","4","Theo ảnh và vị trí đã biết");Check("create",r.Ok);
            var rec=s.GetObject(r.Message);var pt=s.GetPoint(rec.GetAll("pt")[0]);
            Check("point-xy-plane",pt.X==10 && pt.Y==20 && pt.Z==0 && pt.ZRaw=="");
            Check("provenance",pt.IsManual && rec.Get("position_source")=="MANUAL_CAD" && rec.Get("station_control_eligible")=="0");
            Check("KmH",rec.Get("ly_trinh_km")==Chainage.Format(40400));
            Check("no-field-verification",rec.Get("kiem_tra")!="DA_KIEM_TRA");
            Check("duplicate-rejected",!ManualMarker.Create(db,new Point3d(10,20,0),"COC_TIEU","","","").Ok);
            Check("duplicate-no-orphans",s.GetPoints().Count==before+1 && s.GetObjects().Count==objects+1);
            var blank=ManualMarker.Create(db,new Point3d(30,20,0),"COC_TIEU","","","");Check("unknown-KmH-allowed",blank.Ok && s.GetObject(blank.Message).Get("ly_trinh_m")=="");
            Check("later-edit",s.UpdateObject(r.Message,new BhtRecord().Set("marker_h","5")).Ok && s.GetObject(r.Message).Get("ly_trinh_km")==Chainage.Format(40500));
            Check("edit-preserves-source",s.GetObject(r.Message).Get("position_source")=="MANUAL_CAD");
            var km=ManualMarker.Create(db,new Point3d(50,20,0),"COT_KM","41","","");Check("Km-marker",km.Ok);
            App.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nMANUAL-FINAL-PASS");
        }}catch(System.Exception ex){App.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nMANUAL-FAIL "+ex);}
    }
    [CommandMethod("BHTMANUALREOPEN")]
    public void Reopen()
    {
        try {using(var s=new BhtDataService(App.DocumentManager.MdiActiveDocument.Database,()=>"",false)){
            var rows=s.GetObjects().Values.Where(r=>r.Get("position_source")=="MANUAL_CAD").ToList();
            Check("reopen-three-records",rows.Count==3);Check("reopen-linked-manual-points",rows.All(r=>s.GetPoint(r.GetAll("pt")[0]).IsManual));
            App.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nMANUAL-REOPEN-PASS");
        }}catch(System.Exception ex){App.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nMANUAL-FAIL "+ex);}
    }
    [CommandMethod("BHTMANUALUNDO")]
    public void UndoProbe() { var r=ManualMarker.Create(App.DocumentManager.MdiActiveDocument.Database,new Point3d(999,999,0),"COC_TIEU","","","");Check("undo-setup",r.Ok); }
}
