using System;
using System.IO;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
public class CantileverLightBuilder {
 static void Poly(BlockTableRecord m, Transaction t, double[] xy, bool closed, short color, double width) {
  var p=new Polyline(); for(int i=0;i<xy.Length;i+=2)p.AddVertexAt(i/2,new Point2d(xy[i],xy[i+1]),0,0,0);
  p.Closed=closed;p.ColorIndex=color;p.ConstantWidth=width;m.AppendEntity(p);t.AddNewlyCreatedDBObject(p,true);
 }
 [CommandMethod("BHTCANTILEVERBUILD")] public void Run() {
  string folder=Environment.GetEnvironmentVariable("BHT_LIGHT_BUILD_DEST");
  foreach(int side in new[]{-1,1}) using(var db=new Database(true,true)){
   using(var t=db.TransactionManager.StartTransaction()){
    var m=(BlockTableRecord)t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),OpenMode.ForWrite);
    // Side elevation, foot at origin. Arm and pole are one continuous outline.
    Poly(m,t,new double[]{-.07,0,-.07,2.75,.08*side,2.93,2.3*side,2.93,2.3*side,2.79,.12*side,2.79,.07,2.7,.07,0},true,0,0);
    Poly(m,t,new double[]{-.23,0,.23,0},false,0,0);
    double x=2.02*side;
    Poly(m,t,new double[]{x-.28,2.79,x+.28,2.79,x+.28,1.33,x-.28,1.33},true,0,0);
    for(int i=0;i<3;i++){
     double y=2.51-i*.46;var p=new Polyline(2);
     p.AddVertexAt(0,new Point2d(x-.08,y),1,0,0);p.AddVertexAt(1,new Point2d(x+.08,y),1,0,0);
     p.Closed=true;p.ConstantWidth=.16;p.ColorIndex=(short)(i+1);m.AppendEntity(p);t.AddNewlyCreatedDBObject(p,true);
    }
    t.Commit();
   }
   db.Insbase=Point3d.Origin;db.Insunits=UnitsValue.Undefined;
   db.SaveAs(Path.Combine(folder,side<0?"DEN_TH_CAN_VUON_TRAI.dwg":"DEN_TH_CAN_VUON_PHAI.dwg"),DwgVersion.AC1027);
  }
 }
}
