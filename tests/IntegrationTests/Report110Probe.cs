using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using BHT.Palette;
using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Report110Probe
{
    static int count,fail;
    static void Check(string name,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" CORRECTION132 "+name);}
    static IEnumerable<Control> Children(Control root){foreach(Control c in root.Controls){yield return c;foreach(var sub in Children(c))yield return sub;}}
    static Form Create(string name,params object[] args){return (Form)Activator.CreateInstance(typeof(BhtPaletteControl).Assembly.GetType("BHT.Palette."+name),args);}
    static void Layout(Form f,string prefix){
        f.StartPosition=FormStartPosition.Manual;f.Location=new Point(-3000,-3000);f.Show();Application.DoEvents();
        Check(prefix+"-modeless",!f.Modal);
        foreach(var size in new[]{new Size(800,500),new Size(1150,680)}){
            f.Size=size;Application.DoEvents();
            foreach(var b in Children(f).OfType<Button>())Check(prefix+"-button-visible-"+size.Width+"-"+b.Text,f.ClientRectangle.Contains(f.PointToClient(b.PointToScreen(new Point(b.Width-1,b.Height-1)))));
        }
    }
    [CommandMethod("REPORT110PROBE")]
    public static void Run(){try{
        var lisp=new LispApi();lisp.Call("bht:api-messages","CLEAR");
        var eval=lisp.Call("qa110-eval","(progn (setq qa110-n 0) (repeat 180 (bht:screen-message-add (strcat \"COC-\" (itoa (setq qa110-n (1+ qa110-n)))))))");
        var messages=lisp.Call("bht:api-messages","DRAIN");
        Check("bridge-report-all-180",eval.Ok&&messages.Ok&&messages.Values.Count==180&&messages.Values.First()=="COC-1"&&messages.Values.Last()=="COC-180");
        var invalid=lisp.Call("bht:api-progress-preview-apply","QA110-invalid-token");
        Check("preview-api-registered-rejects-invalid-token",!invalid.Ok&&!invalid.Error.Contains("eInvalidInput"));
        string body=string.Join("\r\n",messages.Values.Select(s=>s+" | Km42+848.45 | Cọc có nội dung cần kiểm tra với cảnh báo rất dài để kiểm tra dòng cuối."));
        using(var f=Create("ReportDialog","Report110",body,null,null)){
            Layout(f,"report");var text=Children(f).OfType<TextBox>().Single();
            Check("report-full-text-tail",text.Text.Contains("COC-1 |")&&text.Text.Contains("COC-180 |"));
            Check("report-readonly-scrollable",text.ReadOnly&&text.ScrollBars==ScrollBars.Both&&text.ForeColor.GetBrightness()<0.3&&text.BackColor.GetBrightness()>0.8);
            var wrap=Children(f).OfType<CheckBox>().Single();wrap.Checked=false;Check("report-horizontal-scroll-mode",!text.WordWrap);
            using(var b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(0,0,f.Width,f.Height));b.Save(Path.Combine(Path.GetDirectoryName(typeof(Report110Probe).Assembly.Location),"report110.png"));}
        }
        string[] rows={"OBJ001\tW.239a\tKm42+848.45\tDOAN_13\tGiữ kết quả đã xác nhận; kiểm tra nhãn và phía đường trước khi áp dụng phương án bổ sung."};
        using(var f=Create("ProgressPreviewForm","Preview110",rows,true)){
            Layout(f,"preview");var grid=Children(f).OfType<DataGridView>().Single();
            Check("preview-wrap-and-header",grid.DefaultCellStyle.WrapMode==DataGridViewTriState.True&&grid.ColumnHeadersHeightSizeMode==DataGridViewColumnHeadersHeightSizeMode.AutoSize);
            Check("preview-full-reason",Convert.ToString(grid.Rows[0].Cells[4].Value)==rows[0].Split('\t')[4]&&grid.DefaultCellStyle.ForeColor.GetBrightness()<0.3);
            f.GetType().GetMethod("SetResult").Invoke(f,new object[]{"Dữ liệu đã đổi; lập lại phương án.",true});
            Check("preview-consumed-cannot-reapply",!Children(f).OfType<Button>().Single(b=>b.Text=="Áp dụng phương án").Enabled);
            using(var b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(0,0,f.Width,f.Height));b.Save(Path.Combine(Path.GetDirectoryName(typeof(Report110Probe).Assembly.Location),"preview110.png"));}
        }
        using(var f=Create("ProgressPreviewForm","Empty",new string[0],false))Check("empty-preview-cannot-apply",!Children(f).OfType<Button>().Single(b=>b.Text=="Áp dụng phương án").Enabled);
    }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-ERROR "+ex);}
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-TOTAL="+count+" FAIL="+fail+"\n");
    }
}
