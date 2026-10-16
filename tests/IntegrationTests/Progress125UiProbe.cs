using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Drawing;using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Runtime;using BHT.Core;using BHT.Bridge;using BHT.Palette;
public class Progress125UiProbe
{
 static int count;static void Check(string n,bool ok){if(!ok)throw new System.Exception(n);count++;}
 static IEnumerable<Control> All(Control c){foreach(Control d in c.Controls){yield return d;foreach(var x in All(d))yield return x;}}
 static Button Button(Form f,string text){return All(f).OfType<Button>().Single(b=>b.Text==text);}
 static void Show(Form f){f.StartPosition=FormStartPosition.Manual;f.Location=new Point(-3000,-3000);f.Show();Application.DoEvents();}
 [CommandMethod("PROGRESS125UI")]public void Run(){string dir=Path.GetDirectoryName(typeof(Progress125UiProbe).Assembly.Location);try{
  List<BhtRecord> segs,objects;List<string> routes;using(var db=new Database(false,true)){db.ReadDwgFile(Environment.GetEnvironmentVariable("BHT_PROGRESS_REAL"),FileOpenMode.OpenForReadAndAllShare,false,"");db.CloseInput(true);segs=SegmentStore.Read(db);using(var tr=db.TransactionManager.StartOpenCloseTransaction()){objects=BhtStore.All(tr,db,"OBJ").Select(p=>p.Value).ToList();routes=BhtStore.Keys(tr,db,"ROUTE");}}
  using(var form=new ProgressForm(segs,objects,routes)){Show(form);var grid=All(form).OfType<DataGridView>().Single();var combo=All(form).OfType<ComboBox>().Single();Check("all15",grid.Rows.Count==15);Check("no-selection-disabled",!Button(form,"Tính lý trình theo đoạn").Enabled);grid.Rows.Cast<DataGridViewRow>().Single(r=>Convert.ToString(r.Cells[0].Value)=="DOAN_10").Selected=true;combo.SelectedItem="TIMRANHD10";Check("selected-route-enables-calculate",Button(form,"Tính lý trình theo đoạn").Enabled);Check("unconfirmed-segment-cannot-export",!Button(form,"Xuất hồ sơ đoạn (.bht)…").Enabled);
   using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));bitmap.Save(Path.Combine(dir,"xu-ly-theo-doan.png"));}
   Button(form,"Tính lý trình theo đoạn").PerformClick();Check("exact-selection-result",form.Result.SequenceEqual(new[]{"CALCULATE","DOAN_10","TIMRANHD10"}));}
  foreach(var r in objects.Where(r=>r.Get("doan")=="DOAN_10"))r.Set("progress_locked","1");
  using(var form=new ProgressForm(segs,objects,routes)){Show(form);var grid=All(form).OfType<DataGridView>().Single();grid.Rows.Cast<DataGridViewRow>().Single(r=>Convert.ToString(r.Cells[0].Value)=="DOAN_10").Selected=true;All(form).OfType<ComboBox>().Single().SelectedItem=segs.Single(r=>r.Get("segment_id")=="DOAN_10").Get("route_id");Check("confirmed-allows-incremental-only-on-bound-route",Button(form,"Tính lý trình theo đoạn").Enabled==(All(form).OfType<ComboBox>().Single().SelectedIndex>=0));Check("confirmed-enables-reopen-and-export",Button(form,"Mở lại kết quả đoạn").Enabled&&Button(form,"Xuất hồ sơ đoạn (.bht)…").Enabled);string selected="";form.ExportRequested=s=>selected=s;Button(form,"Xuất hồ sơ đoạn (.bht)…").PerformClick();Check("export-selects-only-current-segment",selected=="DOAN_10");Button(form,"Xuất dữ liệu tích lũy (.bht)…").PerformClick();Check("cumulative-export-all",selected==null);Button(form,"Đóng").PerformClick();Check("close-without-data-action",form.Result==null);}
  File.WriteAllText(Path.Combine(dir,"progress-ui.txt"),"PASS PROGRESS125 UI "+count);
 }catch(System.Exception ex){File.WriteAllText(Path.Combine(dir,"progress-ui.txt"),"FAIL "+ex);}}
}
