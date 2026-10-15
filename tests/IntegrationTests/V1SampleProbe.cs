using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
using BHT.Core;
using BHT.Palette;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class V1SampleProbe
{
 static List<string> lines=new List<string>();
 static void Measure(string name, Action action) {
  var times=new List<double>();
  for(int i=0;i<5;i++){var sw=Stopwatch.StartNew();action();sw.Stop();times.Add(sw.Elapsed.TotalMilliseconds);}
  lines.Add(name+"_ms="+string.Join(",",times.Select(x=>x.ToString("F3",System.Globalization.CultureInfo.InvariantCulture))));
  lines.Add(name+"_median_ms="+times.OrderBy(x=>x).ElementAt(2).ToString("F3",System.Globalization.CultureInfo.InvariantCulture));
 }
 [CommandMethod("BHTV1SAMPLE")]
 public void Run(){
  string folder=Environment.GetEnvironmentVariable("BHT_V1_QA");Directory.CreateDirectory(folder);
  try {
   File.AppendAllText(Path.Combine(folder,"trace.txt"),"start\n"); var doc=AcApp.DocumentManager.MdiActiveDocument;
   using(var svc=new BhtDataService(doc.Database,()=>Path.GetDirectoryName(doc.Name),true)){
    lines.Add("version="+BhtVersion.Version);
    Measure("overview",()=>svc.GetOverview());
    File.WriteAllLines(Path.Combine(folder,"sample-perf.txt"),lines); var pts=svc.GetPoints();var objs=svc.GetObjects();
    lines.Add("points="+pts.Count+" objects="+objs.Count+" photos="+svc.GetPhotos().Count);
    var overview=svc.GetOverview();
    using(var tr=doc.Database.TransactionManager.StartTransaction()){
     if(overview.LegacyV01Points!=BhtStore.LegacyPointCount(tr,doc.Database) || overview.Symbols!=BhtStore.Tagged(tr,doc.Database,"INSERT","BHT_KH").Count ||
        overview.PhotoMarkers!=BhtStore.Tagged(tr,doc.Database,"INSERT","BHT_ANHPT").Count || overview.Labels!=BhtStore.Tagged(tr,doc.Database,"TEXT","BHT_NHAN").Count)
      throw new System.Exception("overview counts differ from reference scans");
     tr.Commit();lines.Add("PASS overview counts match reference scans");
    }
    foreach(var g in objs.Values.GroupBy(x=>x.Get(ObjFields.Group)))lines.Add("group="+g.Key+":"+g.Count());
    Measure("lookup_all_points",()=>{foreach(var p in pts)svc.GetPoint(p.Id);});
    Measure("objects_read",()=>svc.GetObjects());
    File.WriteAllLines(Path.Combine(folder,"sample-perf.txt"),lines); using(var palette=new BhtPaletteControl()){
     typeof(BhtPaletteControl).GetField("_svc",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(palette,svc);
     Measure("palette_refresh",()=>palette.RefreshAll());
     File.WriteAllLines(Path.Combine(folder,"sample-perf.txt"),lines); using(var form=new Form()){form.Width=520;form.Height=900;form.Controls.Add(palette);form.Show();Application.DoEvents();
      using(var bitmap=new System.Drawing.Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,form.ClientRectangle);bitmap.Save(Path.Combine(folder,"dashboard.png"));}
      form.ClientSize=new System.Drawing.Size(340,600);Application.DoEvents();
      using(var bitmap=new System.Drawing.Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,form.ClientRectangle);bitmap.Save(Path.Combine(folder,"dashboard-narrow.png"));}
      form.Controls.Remove(palette);form.Close();
     }
    }
   }
   lines.Add("PASS sample read and refresh");
  }catch(System.Exception ex){lines.Add("FAIL "+ex);}
  File.WriteAllLines(Path.Combine(folder,"sample-perf.txt"),lines);
 }
}

