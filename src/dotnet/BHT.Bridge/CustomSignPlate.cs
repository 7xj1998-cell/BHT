using System;using System.IO;using System.Linq;using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;using Autodesk.AutoCAD.Colors;using Autodesk.AutoCAD.Runtime;using BHT.Core;
[assembly:CommandClass(typeof(BHT.Bridge.CustomSignPlate))]
namespace BHT.Bridge {
 public static class CustomSignPlate {
  public static bool IsCustom(string code){return (code??"").StartsWith("BHT.TEXT.",StringComparison.OrdinalIgnoreCase);}
  public static string FontPath(){
   for(var dir=new DirectoryInfo(Path.GetDirectoryName(typeof(CustomSignPlate).Assembly.Location));dir!=null;dir=dir.Parent)
    foreach(var p in new[]{Path.Combine(dir.FullName,"Fonts","giaothong1.ttf"),Path.Combine(dir.FullName,"fonts_local","giaothong1.ttf")})if(File.Exists(p))return p;
   throw new InvalidOperationException("Thiếu phông Giaothong1. Cài lại bộ thư viện BHT đầy đủ.");
  }
  [LispFunction("BHTCUSTOMPLATE")] public static string Build(ResultBuffer args){
   var v=args.AsArray();return Ensure(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database,Convert.ToString(v[0].Value),Convert.ToInt32(v[1].Value),Convert.ToString(v[2].Value));
  }
  public static string Ensure(Database db,string code,int index,string content){
   var values=SignContent.ForFace(content,index,code);string text;
   if(!values.TryGetValue("custom_text",out text) || string.IsNullOrWhiteSpace(text))throw new InvalidOperationException("Thiếu nội dung mặt biển tùy chỉnh "+(index+1)+". Mở thư viện để nhập lại.");
   var lines=text.Replace("\r\n","\n").Replace("\r","\n").Split('\n');
   if(lines.Length>20)throw new ArgumentException("Biển tùy chỉnh tối đa 20 dòng.");
   string fontPath=FontPath(),name="BHT_SIGN_CUSTOM_V0661_"+SignLayout.Hash(text);
   using(var tr=db.TransactionManager.StartTransaction()){
    var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);if(bt.Has(name))return name;
    var st=(TextStyleTable)tr.GetObject(db.TextStyleTableId,OpenMode.ForRead);ObjectId style;
    if(st.Has("BHT_QCVN_GIAOTHONG1_V0661"))style=st["BHT_QCVN_GIAOTHONG1_V0661"];
    else{st.UpgradeOpen();var ts=new TextStyleTableRecord{Name="BHT_QCVN_GIAOTHONG1_V0661",FileName=fontPath,XScale=1,TextSize=0};style=st.Add(ts);tr.AddNewlyCreatedDBObject(ts,true);}
    bt.UpgradeOpen();var block=new BlockTableRecord{Name=name};bt.Add(block);tr.AddNewlyCreatedDBObject(block,true);
    // Longest side is 1.8 units, matching supplementary QCVN face normalization.
    double h=Math.Min(1.8,Math.Max(.65,.32*lines.Length+.26)),w=1.8,bottom=.6;
    var border=new Polyline{Closed=true,Color=Color.FromRgb(0,118,190)};
    foreach(var q in new[]{new Point2d(-w/2,bottom),new Point2d(w/2,bottom),new Point2d(w/2,bottom+h),new Point2d(-w/2,bottom+h)})border.AddVertexAt(border.NumberOfVertices,q,0,0,0);
    block.AppendEntity(border);tr.AddNewlyCreatedDBObject(border,true);
    var hatch=new Hatch{Color=Color.FromRgb(0,118,190)};block.AppendEntity(hatch);tr.AddNewlyCreatedDBObject(hatch,true);hatch.SetHatchPattern(HatchPatternType.PreDefined,"SOLID");hatch.AppendLoop(HatchLoopTypes.External,new ObjectIdCollection(new[]{border.ObjectId}));hatch.EvaluateHatch(true);
    var frame=new Polyline{Closed=true,ColorIndex=7,ConstantWidth=.012};
    foreach(var q in new[]{new Point2d(-.86,bottom+.04),new Point2d(.86,bottom+.04),new Point2d(.86,bottom+h-.04),new Point2d(-.86,bottom+h-.04)})frame.AddVertexAt(frame.NumberOfVertices,q,0,0,0);
    block.AppendEntity(frame);tr.AddNewlyCreatedDBObject(frame,true);
    double size=Math.Min(.22,(h-.18)/(lines.Length*1.45));
    foreach(string line in lines.Where(l=>l.Length>0))using(var probe=new DBText{TextString=line,Height=size,TextStyleId=style,WidthFactor=1}){
     var ext=probe.GeometricExtents;double width=ext.MaxPoint.X-ext.MinPoint.X;if(width>1.58)size*=1.58/width;
    }
    for(int i=0;i<lines.Length;i++){
     if(lines[i].Length==0)continue;
     var t=new DBText{TextString=lines[i],Height=size,WidthFactor=1,TextStyleId=style,ColorIndex=7,HorizontalMode=TextHorizontalMode.TextCenter,VerticalMode=TextVerticalMode.TextVerticalMid,AlignmentPoint=new Point3d(0,bottom+h/2+((lines.Length-1)/2.0-i)*size*1.45,0)};
     block.AppendEntity(t);tr.AddNewlyCreatedDBObject(t,true);t.AdjustAlignment(db);
    }
    var pole=new Line(Point3d.Origin,new Point3d(0,bottom,0)){ColorIndex=7};block.AppendEntity(pole);tr.AddNewlyCreatedDBObject(pole,true);
    var foot=new Circle(Point3d.Origin,Vector3d.ZAxis,.06){ColorIndex=7};block.AppendEntity(foot);tr.AddNewlyCreatedDBObject(foot,true);
    ((DrawOrderTable)tr.GetObject(block.DrawOrderTableId,OpenMode.ForWrite)).MoveToBottom(new ObjectIdCollection(new[]{hatch.ObjectId}));
    tr.Commit();return name;
   }
  }
 }
}
