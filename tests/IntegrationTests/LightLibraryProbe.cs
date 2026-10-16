using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class LightLibraryProbe
{
    static string Hash(string path){using(var sha=SHA256.Create())using(var f=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(f));}
    [CommandMethod("BHTLIGHTPROBE")] public void Run()
    {
        var report=new List<string>();int checks=0;
        Action<string,bool> check=(name,ok)=>{report.Add((ok ? "PASS ":"FAIL ")+name);checks++;};
        try
        {
            var db=AcApp.DocumentManager.MdiActiveDocument.Database;
            var folder=CustomDwgBlock.LightFolder();var files=Directory.GetFiles(folder,"*.dwg");
            check("nine-native-light-blocks",files.Length==9);
            foreach(var file in files)
            {
                string before=Hash(file),name=CustomDwgBlock.Import(db,file);
                check(Path.GetFileName(file)+" imported-once",name==CustomDwgBlock.Import(db,file));
                check(Path.GetFileName(file)+" source-unchanged",before==Hash(file));
                using(var tr=db.TransactionManager.StartOpenCloseTransaction())
                {
                    var table=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
                    var block=(BlockTableRecord)tr.GetObject(table[name],OpenMode.ForRead);
                    using(var rb=new ResultBuffer(new TypedValue(5005,name)))
                    using(var br=new BlockReference(Point3d.Origin,table[name])){
                        var ext=br.GeometricExtents;double size=Math.Max(ext.MaxPoint.X-ext.MinPoint.X,ext.MaxPoint.Y-ext.MinPoint.Y);
                        check(Path.GetFileName(file)+" normalized-footprint",Math.Abs(size*CustomDwgBlock.LightFactor(rb)-2.4)<1e-8);
                    }
                    check(Path.GetFileName(file)+" anchor-at-survey-origin",block.Origin.IsEqualTo(Point3d.Origin));
                    check(Path.GetFileName(file)+" native-2D-no-proxy",block.Cast<ObjectId>().All(id=>tr.GetObject(id,OpenMode.ForRead) is Curve || tr.GetObject(id,OpenMode.ForRead) is Hatch));
                    if(file.EndsWith("DEN_TIN_HIEU_3_MAU.dwg",StringComparison.OrdinalIgnoreCase) || file.Contains("CAN_VUON"))
                        check("traffic-three-distinct-filled-colors",new[]{1,2,3}.All(c=>block.Cast<ObjectId>().Select(id=>tr.GetObject(id,OpenMode.ForRead)).OfType<Entity>().Any(e=>e.ColorIndex==c && (e is Hatch || (e is Polyline && ((Polyline)e).Closed && ((Polyline)e).ConstantWidth>0)))));
                    check(Path.GetFileName(file)+" inherits-common-insert-layer",block.Cast<ObjectId>().Select(id=>tr.GetObject(id,OpenMode.ForRead)).OfType<Entity>().All(e=>e.Layer=="0"));
                    if(file.Contains("CAN_VUON")) {
                        using(var bmp=new System.Drawing.Bitmap(480,480))
                        using(var g=System.Drawing.Graphics.FromImage(bmp)){
                            g.Clear(System.Drawing.Color.FromArgb(32,40,46));
                            g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            foreach(ObjectId id in block){
                                var p=tr.GetObject(id,OpenMode.ForRead) as Polyline;if(p==null)continue;
                                var color=p.ColorIndex==1?System.Drawing.Color.Red:p.ColorIndex==2?System.Drawing.Color.Yellow:p.ColorIndex==3?System.Drawing.Color.Lime:System.Drawing.Color.White;
                                using(var pen=new System.Drawing.Pen(color,2))
                                using(var brush=new System.Drawing.SolidBrush(color)){
                                    if(p.NumberOfVertices==2 && p.Closed && p.ConstantWidth>0){
                                        var a=p.GetPoint2dAt(0);var b=p.GetPoint2dAt(1);double cx=(a.X+b.X)/2,cy=a.Y,r=Math.Abs(a.X-b.X)/2+p.ConstantWidth/2;
                                        g.FillEllipse(brush,(float)(240+(cx-r)*85),(float)(420-(cy+r)*85),(float)(2*r*85),(float)(2*r*85));
                                    }else{
                                        var pts=Enumerable.Range(0,p.NumberOfVertices).Select(i=>new System.Drawing.PointF((float)(240+p.GetPoint2dAt(i).X*85),(float)(420-p.GetPoint2dAt(i).Y*85))).ToArray();
                                        if(p.Closed)g.DrawPolygon(pen,pts);else g.DrawLines(pen,pts);
                                    }
                                }
                            }
                            bmp.Save(Path.Combine(Path.GetDirectoryName(typeof(LightLibraryProbe).Assembly.Location),Path.GetFileNameWithoutExtension(file)+".png"));
                        }
                    }
                    if(file.EndsWith("DEN_CS_TRANG_TRI_MB.dwg",StringComparison.OrdinalIgnoreCase))
                        // Decorative fixture legitimately contains native solid hatches.
                        check("decorative-hatch-retained",block.Cast<ObjectId>().Any(id=>tr.GetObject(id,OpenMode.ForRead) is Hatch));
                }
            }
        }
        catch(System.Exception ex){check("exception "+ex,false);}
        File.WriteAllLines(Path.Combine(Path.GetDirectoryName(typeof(LightLibraryProbe).Assembly.Location),"light-probe.txt"),report);
    }
}
