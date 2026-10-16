using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using BHT.Palette;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Correction132UiProbe
{
    static int count,fail;
    static void Check(string name,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" CORRECTION132 "+name);}
    static IEnumerable<Control> Children(Control root){foreach(Control c in root.Controls){yield return c;foreach(var sub in Children(c))yield return sub;}}
    [CommandMethod("CORRECTION132UI")]
    public static void Run(){try{
        var doc=AcApp.DocumentManager.MdiActiveDocument;
        var rec=new BhtRecord().Set("object_id","OBJ001").Set("progress_locked","1").Set("ma_hieu","P.102").Set("so_tru","1").Set("so_mat","1");
        using(var form=new ObjectCorrectionForm(doc,"OBJ001",rec)) {
            form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-3000,-3000);form.Show();Application.DoEvents();
            Check("ui-confirm-disabled-before-preview",!Children(form).OfType<Button>().Single(b=>b.Text=="Xác nhận hiệu chỉnh").Enabled);
            var kind=Children(form).OfType<ComboBox>().Single();
            foreach(var input in Children(form).Where(c=>c is TextBox || c is ComboBox))Check("ui-input-layout-"+input.GetType().Name,input.Visible&&input.Width>200&&input.Height>=20);
            Check("ui-kind-current-text",kind.Text=="Thuộc tính khảo sát");
            Check("ui-original-four-plus-retire-and-display",kind.Items.Count==6&&kind.Items[4].ToString().Contains("Ngừng sử dụng")&&kind.Items[5].ToString().Contains("GLOBAL"));
            foreach(var size in new[]{new Size(690,610),new Size(920,760)}) {
                form.Size=size;Application.DoEvents();
                foreach(var b in Children(form).OfType<Button>())Check("ui-visible-"+size.Width+"-"+b.Text,form.ClientRectangle.Contains(form.PointToClient(b.PointToScreen(new Point(b.Width-1,b.Height-1)))));
                foreach(var grid in Children(form).OfType<DataGridView>())Check("ui-grid-height-"+size.Width,grid.Height>=70);
            }
            var edit=Children(form).OfType<DataGridView>().Single(g=>!g.ReadOnly);
            kind.SelectedIndex=1;Application.DoEvents();Check("ui-rtk-has-point-and-segment",edit.Rows.Count==2);
            kind.SelectedIndex=2;Application.DoEvents();Check("ui-no-manual-station-overwrite",edit.Rows.Count==0);
            kind.SelectedIndex=3;Application.DoEvents();Check("ui-assignment-one-explicit-segment",edit.Rows.Count==1);
            kind.SelectedIndex=4;Application.DoEvents();Check("ui-retire-no-raw-point-input",edit.Rows.Count==0);
            kind.SelectedIndex=5;Application.DoEvents();Check("ui-display-no-survey-input",edit.Rows.Count==0);
            kind.SelectedIndex=0;Application.DoEvents();
            using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));bitmap.Save(Path.Combine(Path.GetDirectoryName(typeof(Correction132UiProbe).Assembly.Location),"correction-ui.png"));}
        }
    }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-ERROR "+ex);}
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-TOTAL="+count+" FAIL="+fail+"\n");
    }
}
