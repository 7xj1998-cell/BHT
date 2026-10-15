using System;using System.IO;using System.Linq;using System.Drawing;using System.Reflection;using System.Windows.Forms;
using BHT.Palette;using Autodesk.AutoCAD.Runtime;
public class Route123UiProbe
{
 static int count;static readonly BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
 static object Field(object target,string name){return target.GetType().GetField(name,Hidden).GetValue(target);}
 static void Call(object target,string name){target.GetType().GetMethod(name,Hidden).Invoke(target,null);}
 static void Check(string name,bool ok){if(!ok)throw new System.Exception(name);count++;}
 static void Open(Form form){form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-3000,-3000);form.Show();Application.DoEvents();}
 static void Capture(Form form,string file){using(var b=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(b,new Rectangle(0,0,form.Width,form.Height));b.Save(file);}}
 [CommandMethod("ROUTE123UIPROBE")]public static void Run(){string folder=Path.Combine(Path.GetDirectoryName(typeof(Route123UiProbe).Assembly.Location),"route-ui");Directory.CreateDirectory(folder);try{
  foreach(string type in new[]{"TIM_DUONG","TIM_RANH","KHAC"})using(var form=new RouteOptionsForm("Nạp tim TDT","TIMRANHDOAN10",type,"20,5","0")){
   Open(form);Call(form,"AcceptOptions");Check("chosen-type-and-comma-"+type,form.DialogResult==DialogResult.OK&&form.Result[1]==type&&form.Result[2]=="20.5"&&form.Result[3]=="0");}
  using(var form=new RouteOptionsForm("Sửa thông tin tim tuyến","TIMRANHDOAN10","TIM_DUONG","20.000","0.000")){
   Open(form);((ComboBox)Field(form,"routeType")).SelectedIndex=1;Capture(form,Path.Combine(folder,"sua-tim.png"));Call(form,"AcceptOptions");Check("edit-chooses-ditch-with-zero-extrapolation",form.Result[1]=="TIM_RANH"&&form.Result[3]=="0");}
  foreach(string bad in new[]{"", "20,5,0", "NaN", "Infinity", "0", "-2"})using(var form=new RouteOptionsForm("Nạp tim TDT","R","TIM_RANH",bad,"0")){
   Open(form);Call(form,"AcceptOptions");Check("invalid-offset-stays-open-"+bad,form.DialogResult==DialogResult.None&&form.Result==null&&((Label)Field(form,"error")).Text.Contains("lệch"));form.Close();}
  using(var form=new RouteOptionsForm("Nạp tim TDT","BAD ID","TIM_RANH","20","0")){Open(form);Call(form,"AcceptOptions");Check("invalid-id-stays-open",form.Result==null&&((Label)Field(form,"error")).Text.StartsWith("ID"));form.Close();}
  using(var form=new RouteOptionsForm("Nạp tim TDT","R","TIM_RANH","20","-1")){Open(form);Call(form,"AcceptOptions");Check("negative-extrapolation-stays-open",form.Result==null&&((Label)Field(form,"error")).Text.StartsWith("Ngoại"));form.Close();}
  var rows=new[]{"0|100|50240|50240|TDT_STAKES 433B6:X91|", "1|0|50140|50140|TDT_STAKES 433B6:X4|", "2|80.145|50220.15|50220.15|TDT_STAKES 433B6:X70|", "3|80|50220|50230|TDT_STAKES 433B6:X58|Điểm gãy", "4|bad|x|y|Nguồn cũ|Thiếu số", "-1|150|50290|50290|Hồ sơ COC1|Sửa tại hồ sơ"};
  using(var form=new RouteMarksForm("TIMRANHDOAN10",rows)){
   Open(form);var grid=(DataGridView)Field(form,"grid");var remove=(Button)Field(form,"remove");var view=(Button)Field(form,"view");
   Check("rows-sorted-by-distance-with-invalid-last",Convert.ToString(grid.Rows[0].Cells[1].Value)=="0.000"&&Convert.ToString(grid.Rows[5].Cells[6].Value).Contains("không hợp lệ"));
   Check("station-equation-displays-both-values",Convert.ToString(grid.Rows[1].Cells[2].Value)!=Convert.ToString(grid.Rows[1].Cells[3].Value));
   Check("abnormal-interval-flagged",grid.Rows.Cast<DataGridViewRow>().Any(r=>Convert.ToString(r.Cells[6].Value).Contains("CẦN KIỂM TRA")));
   grid.ClearSelection();Check("no-selection-disables-actions",!remove.Enabled&&!view.Enabled);
   grid.Rows[0].Selected=true;grid.Rows[2].Selected=true;Check("multi-selection-enables-only-delete",remove.Enabled&&!view.Enabled);
   grid.ClearSelection();grid.Rows[4].Selected=true;Check("automatic-control-cannot-be-deleted",!remove.Enabled&&view.Enabled);
   grid.ClearSelection();grid.Rows[5].Selected=true;Check("damaged-control-can-be-removed-not-located",remove.Enabled&&!view.Enabled);
   grid.ClearSelection();grid.Rows[2].Selected=true;Capture(form,Path.Combine(folder,"bang-moc.png"));view.PerformClick();Check("view-uses-distance-of-selected-sorted-row",form.Result[0]=="VIEW"&&form.Result[1]=="80.145");}
  using(var form=new RouteMarksForm("EMPTY",new string[0])){Open(form);Check("empty-table-actions-disabled",!((Button)Field(form,"remove")).Enabled&&!((Button)Field(form,"view")).Enabled);form.Close();}
  using(var form=new RouteMarksForm("TIMRANHDOAN10",rows)){form.Size=form.MinimumSize;Open(form);Capture(form,Path.Combine(folder,"bang-moc-minimum.png"));foreach(var b in ((Button)Field(form,"view")).Parent.Controls.OfType<Button>())Check("minimum-button-visible-"+b.Text,form.ClientRectangle.Contains(form.PointToClient(b.PointToScreen(new Point(b.Width-1,b.Height-1)))));form.Close();}
  File.WriteAllText(Path.Combine(folder,"result.txt"),"PASS ROUTE123 UI: "+count+" checks");
 }catch(System.Exception ex){File.WriteAllText(Path.Combine(folder,"result.txt"),"FAIL ROUTE123 UI: "+ex);}}
}
