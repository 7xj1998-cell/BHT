using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Palette;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class Tables111Probe
{
    static int count,fail;
    static string Dir { get { return Path.GetDirectoryName(typeof(Tables111Probe).Assembly.Location); } }
    static void Check(string name,bool ok) { count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" TABLES111 "+name); }
    static IEnumerable<Control> Children(Control root) { foreach(Control c in root.Controls){yield return c;foreach(var child in Children(c))yield return child;} }
    static double Light(Color c) { return new[]{c.R,c.G,c.B}.Select(v=>{double d=v/255.0;return d<=0.04045?d/12.92:Math.Pow((d+0.055)/1.055,2.4);}).Select((v,i)=>v*new[]{0.2126,0.7152,0.0722}[i]).Sum(); }
    static double Contrast(Color a,Color b) { double x=Light(a),y=Light(b);return (Math.Max(x,y)+0.05)/(Math.Min(x,y)+0.05); }
    static void GridCheck(Form form,string label)
    {
        foreach(var grid in Children(form).OfType<DataGridView>()) {
            Check(label+"-font",grid.Font.SizeInPoints>=10);
            Check(label+"-headers-wrap",grid.ColumnHeadersHeightSizeMode==DataGridViewColumnHeadersHeightSizeMode.AutoSize&&grid.ColumnHeadersDefaultCellStyle.WrapMode==DataGridViewTriState.True);
            foreach(DataGridViewColumn col in grid.Columns) {
                var style=col.HeaderCell.InheritedStyle;int w=Math.Max(20,col.Width-style.Padding.Horizontal-18);
                int h=TextRenderer.MeasureText(col.HeaderText,style.Font,new Size(w,int.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.NoPrefix).Height;
                Check(label+"-header-"+col.Index,grid.ColumnHeadersHeight>=h+style.Padding.Vertical+2);
            }
            if(grid.Rows.Count>0) {
                var s=grid.Rows[0].Cells[0].InheritedStyle;
                Check(label+"-contrast",Contrast(s.ForeColor,s.BackColor)>=4.5);
                Check(label+"-selection",Contrast(s.SelectionForeColor,s.SelectionBackColor)>=4.5);
                Check(label+"-alternate",Contrast(s.ForeColor,grid.AlternatingRowsDefaultCellStyle.BackColor.IsEmpty?s.BackColor:grid.AlternatingRowsDefaultCellStyle.BackColor)>=4.5);
            }
        }
        var buttons=Children(form).OfType<Button>().Where(b=>b.Visible).ToArray();
        Check(label+"-buttons-visible",buttons.All(b=>b.Parent.RectangleToScreen(b.Parent.ClientRectangle).Contains(b.RectangleToScreen(b.ClientRectangle))));
    }
    static void Render(Form form,string label)
    {
        form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-4000,-4000);form.Show();
        foreach(float scale in new[]{1f,1.25f,1.5f,2f}) {
            using(var font=new Font("Segoe UI",10.5f*scale)) {
                form.Font=font;
                foreach(var size in new[]{form.MinimumSize,new Size(1320,850)}) {
                    form.Size=size;Application.DoEvents();GridCheck(form,label+"-"+scale+"-"+size.Width);
                    if(scale==1&&size.Width==1320)using(var bitmap=new Bitmap(form.Width,form.Height)) {
                        form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));bitmap.Save(Path.Combine(Dir,label+".png"));
                    }
                }
            }
        }
        form.Close();
    }
    [CommandMethod("TABLES111PROBE")] public static void Run()
    {
        try {
            var rows=Enumerable.Range(1,30).Select(i=>new BhtRecord().Set("segment_id",i<16?"DOAN"+i.ToString("D2"):"DOAN_"+(i-15).ToString("D2"))
                .Set("package_id","GOI_01").Set("km_start_m","40000").Set("km_end_m","41000").Set("side","TRAI").Set("route_id","TIM_RANH_DOAN_14")
                .Set("package_name","Đường tỉnh 830 — Gói thầu số 01").Set("note","Giữ nguyên mã đoạn; kiểm tra phạm vi trước khi gộp.")).ToList();
            string original=string.Join("\n",rows.Select(r=>r.ToCanonical()));
            using(var form=new SegmentTableForm(rows,new Dictionary<string,BhtRecord>{{"TIM_RANH_DOAN_14",new BhtRecord().Set("loai","TIM_RANH")}},"","",false))Render(form,"segments");
            Check("opening-segments-preserves-records",original==string.Join("\n",rows.Select(r=>r.ToCanonical())));
            var projectType=typeof(BhtPaletteControl).Assembly.GetType("BHT.Palette.DrawingProjectForm");
            Check("project-window-exists",projectType!=null);
            if(projectType!=null)using(var form=(Form)Activator.CreateInstance(projectType,new object[]{"Đoạn 15 — bản kiểm tra"})) {
                var actions=(FlowLayoutPanel)projectType.GetField("Actions").GetValue(form);
                foreach(var text in new[]{"Đăng ký / cập nhật tên","Danh sách đoạn","Liên kết bình đồ cùng dự án…","Kiểm tra bình đồ liên quan","Xem trước gói cập nhật…","Đồng bộ gói đã xem trước","Phát hành đoạn R01/R02…","Tạo bản DWG làm việc mới…","Đánh dấu lưu trữ"})actions.Controls.Add(new Button { Text=text,AutoSize=true });
                projectType.GetMethod("SetRows").Invoke(form,new object[]{new[]{"Đoạn","Gói","Hồ sơ","Xác nhận","Có revision","Ngừng dùng","Trạng thái"},rows.Select((r,i)=>new BhtRecord().Set("segment",r.Get("segment_id")).Set("package","GOI_01").Set("objects","25").Set("confirmed","25").Set("revised","1").Set("retired","0").Set("drawing_state",i%2==0?"UPDATE_AVAILABLE":"READ_ONLY_ARCHIVE")).ToList(),new[]{"segment","package","objects","confirmed","revised","retired","drawing_state"}});
                Render(form,"drawing-project");
            }
            var conflicts=new List<ImportConflict>{new ImportConflict("Hồ sơ","OBJ-000001","Cùng mã nhưng khác hồ sơ khảo sát","nhom","DEN_TH","BIEN_BAO"),new ImportConflict("Hồ sơ","OBJ-000001","Liên kết điểm khác nhau","pt","BOT19-R-000144","BOT1-R-000041")};
            var conflictType=typeof(BhtPaletteControl).Assembly.GetType("BHT.Palette.ImportConflictForm");
            using(var form=(Form)Activator.CreateInstance(conflictType,new object[]{conflicts}))Render(form,"import-conflicts");
            var many=Enumerable.Range(1,1000).Select(i=>new ImportConflict("Hồ sơ","OBJ"+i,"Dữ liệu cần đối chiếu","ghi_chu",new string('A',200),new string('B',200))).ToList();
            var timer=System.Diagnostics.Stopwatch.StartNew();
            using(var form=(Form)Activator.CreateInstance(conflictType,new object[]{many}))Check("large-report-1000-rows",Children(form).OfType<DataGridView>().Single().Rows.Count==1000);
            timer.Stop();File.WriteAllText(Path.Combine(Dir,"table-performance.txt"),"1000 conflicts constructor milliseconds="+timer.ElapsedMilliseconds);
            Check("large-report-bounded",timer.Elapsed.TotalSeconds<20);
        } catch(System.Exception ex) {fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nTABLES111-ERROR "+ex);}
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nTABLES111-TOTAL="+count+" FAIL="+fail);
    }
}
