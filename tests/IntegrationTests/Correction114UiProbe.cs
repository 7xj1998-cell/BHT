using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using BHT.Palette;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class Correction114UiProbe
{
    static int count,fail;
    static void Check(string name,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" CORRECTION132 "+name);}
    static T Field<T>(ObjectCorrectionForm form,string name){return (T)typeof(ObjectCorrectionForm).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(form);}
    static void SetPlan(ObjectCorrectionForm form,CorrectionPreview plan){typeof(ObjectCorrectionForm).GetField("plan",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(form,plan);typeof(ObjectCorrectionForm).GetMethod("UpdateConfirmation",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,null);}
    [CommandMethod("CORRECTION114UI")]
    public static void Run(){try{
        var doc=AcApp.DocumentManager.MdiActiveDocument;BhtRecord old;
        using(var tr=doc.Database.TransactionManager.StartOpenCloseTransaction())old=BhtStore.Read(tr,doc.Database,"OBJ","OBJ001");
        var service=new ObjectCorrection(doc.Database);
        using(var form=new ObjectCorrectionForm(doc,"OBJ001",old)){
            form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-4000,-4000);form.Show();Application.DoEvents();
            var confirm=Field<Button>(form,"confirm");var baseline=Field<CheckBox>(form,"baseline");var assignment=Field<CheckBox>(form,"assignment");var status=Field<Label>(form,"status");
            Check("no-preview-no-confirm-or-irrelevant-approvals",!confirm.Enabled&&!baseline.Visible&&!assignment.Visible);
            var plan=service.Preview("OBJ001",new BhtRecord().Set("mo_ta","QA correction"),CorrectionKind.Attributes,"QA","QA");
            baseline.Visible=plan.LegacyBaseline;assignment.Visible=plan.AssignmentChanged;SetPlan(form,plan);
            Check("legacy-consent-needed-with-explanation",!confirm.Enabled&&status.Text.Contains("bản gốc"));
            baseline.Checked=true;Check("legacy-consent-enables-confirm",confirm.Enabled);
            baseline.Checked=false;Check("revoking-consent-disables-confirm",!confirm.Enabled);
            baseline.Checked=true;
            Field<TextBox>(form,"reason").Text="Changed reason";
            Check("editing-invalidates-preview-and-clears-approvals",!confirm.Enabled&&!baseline.Checked&&!baseline.Visible&&Field<DataGridView>(form,"diff").Rows.Count==0&&status.Text.Contains("Xem trước"));
            plan=service.Preview("OBJ001",new BhtRecord().SetAll("pt",new[]{"NEW-RTK"}).Set("doan","DOAN_02"),CorrectionKind.PointLink,"QA","QA");
            baseline.Visible=plan.LegacyBaseline;assignment.Visible=plan.AssignmentChanged;SetPlan(form,plan);
            baseline.Checked=true;Check("assignment-needs-independent-consent",!confirm.Enabled&&status.Text.Contains("chuyển đoạn"));
            assignment.Checked=true;Check("both-consents-enable-confirm",confirm.Enabled);
            assignment.Checked=false;Check("revoking-assignment-consent-disables-confirm",!confirm.Enabled);
            using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));bitmap.Save(Path.Combine(Path.GetDirectoryName(typeof(Correction114UiProbe).Assembly.Location),"correction-conditions.png"));}
            using(var tr=doc.Database.TransactionManager.StartOpenCloseTransaction())Check("ui-review-does-not-write-record-or-history",old.ToCanonical()==BhtStore.Read(tr,doc.Database,"OBJ","OBJ001").ToCanonical()&&BhtStore.All(tr,doc.Database,"HISTORY").Count==0);
        }
    }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-ERROR "+ex);}
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-TOTAL="+count+" FAIL="+fail+"\n");
    }
}
