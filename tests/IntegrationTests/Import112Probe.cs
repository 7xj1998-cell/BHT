using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using BHT.Palette;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class Import112Probe
{
    static int count, fail, conflicts, picked;
    static string before, mode;
    static bool logBeforePick;
    static string Dir { get { return Path.GetDirectoryName(typeof(Import112Probe).Assembly.Location); } }
    static void Check(string name, bool ok) { count++; if (!ok) fail++; AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + (ok ? "PASS" : "FAIL") + " IMPORT112 " + name); }
    static void Seen(System.Collections.Generic.IList<ImportConflict> items) { conflicts = items.Count; }
    static string Pick()
    {
        picked++;
        logBeforePick = File.Exists(RollingImportLog.PathName) && File.ReadAllText(RollingImportLog.PathName).Contains("chưa chọn tệp") && File.ReadAllText(RollingImportLog.PathName).Contains("Mở bảng chọn");
        if (mode == "cancel") return null;
        if (mode == "exception") throw new IOException("TEST_PICKER_FAILURE");
        if (mode == "missing") return Path.Combine(Dir, "missing.bht");
        return Path.Combine(Environment.GetEnvironmentVariable("BHT111_INPUT"), "DOAN14.bht");
    }
    static void Arm(string value)
    {
        mode=value; conflicts=0; picked=0; logBeforePick=false;
        before=DrawingProject.Fingerprint(AcApp.DocumentManager.MdiActiveDocument.Database);
        ProjectTransfer.ImportConflictsDetected-=Seen;
        ProjectTransfer.ImportConflictsDetected+=Seen;
        ProjectTransfer.RollingFilePicker=Pick;
        AcApp.SetSystemVariable("FILEDIA",1);
    }
    [CommandMethod("IMPORT112ARM")] public static void Start() { Arm("conflict"); }
    [CommandMethod("IMPORT112CANCEL")] public static void Cancel() { Arm("cancel"); }
    [CommandMethod("IMPORT112EXCEPTION")] public static void Exception() { Arm("exception"); }
    [CommandMethod("IMPORT112MISSING")] public static void Missing() { Arm("missing"); }
    [CommandMethod("IMPORT112VERIFY")] public static void Verify()
    {
        Check(mode+"-picker-called-once",picked==1);
        Check(mode+"-logged-before-picker",logBeforePick);
        Check(mode+"-canonical-unchanged",before==DrawingProject.Fingerprint(AcApp.DocumentManager.MdiActiveDocument.Database));
        string log=File.ReadAllText(RollingImportLog.PathName);
        Check(mode+"-correct-result",mode=="cancel"?log.Contains("ĐÃ HỦY")&&conflicts==0:mode=="exception"?log.Contains("TEST_PICKER_FAILURE")&&conflicts==0:mode=="missing"?log.Contains("CHƯA NẠP")&&conflicts==0:conflicts==6&&log.Contains("ĐÃ CHẶN")&&log.Contains("OBJ-000001"));
        Check(mode+"-no-clone",!log.Contains("Sao chép"));
        if(mode=="conflict")Check("file-picker-return-before-package-read",log.IndexOf("Cửa sổ chọn tệp đã đóng",StringComparison.Ordinal)<log.IndexOf("Kiểm tra cấu trúc",StringComparison.Ordinal));
        File.Copy(RollingImportLog.PathName,Path.Combine(Dir,mode+"-rolling.log"),true);
        ProjectTransfer.ImportConflictsDetected-=Seen;ProjectTransfer.RollingFilePicker=null;
    }
    static System.Collections.Generic.IEnumerable<Control> Children(Control c) { foreach(Control x in c.Controls){yield return x;foreach(var child in Children(x))yield return child;} }
    [CommandMethod("IMPORT112UI")] public static void UI()
    {
        try
        {
            var type=typeof(BhtPaletteControl).Assembly.GetType("BHT.Palette.RollingImportFileForm");
            string source=Path.Combine(Environment.GetEnvironmentVariable("BHT111_INPUT"),"DOAN14.bht");
            string hash;using(var sha=System.Security.Cryptography.SHA256.Create())using(var file=File.OpenRead(source))hash=Convert.ToBase64String(sha.ComputeHash(file));
            using(var form=(Form)Activator.CreateInstance(type))
            {
                form.StartPosition=FormStartPosition.Manual;form.Location=new System.Drawing.Point(-4000,-4000);form.Show();Application.DoEvents();
                var input=Children(form).OfType<TextBox>().Single();
                var accept=Children(form).OfType<Button>().Single(b=>b.Text=="Kiểm tra và nạp");
                Check("ui-initial-no-read-no-selection",!accept.Enabled&&type.GetProperty("SelectedPath").GetValue(form,null)==null);
                input.Text=Path.ChangeExtension(source,"dwg");accept.PerformClick();
                Check("ui-rejects-wrong-extension-without-close",form.Visible&&type.GetProperty("SelectedPath").GetValue(form,null)==null);
                input.Text=Path.Combine(Dir,"missing.bht");accept.PerformClick();
                Check("ui-rejects-missing-without-close",form.Visible&&type.GetProperty("SelectedPath").GetValue(form,null)==null);
                input.Text="\""+source+"\"";
                Check("ui-path-change-does-not-accept-or-read",form.Visible&&form.DialogResult!=DialogResult.OK&&type.GetProperty("SelectedPath").GetValue(form,null)==null);
                foreach(var size in new[]{form.MinimumSize,new System.Drawing.Size(1020,420)}){
                    form.Size=size;Application.DoEvents();
                    File.AppendAllText(Path.Combine(Dir,"input-layout.txt"),size+" input="+input.Bounds+" parent="+input.Parent.Bounds+" visible="+input.Visible+" text="+input.Text+"\n");
                    Check("ui-readable-path-"+size.Width,input.Visible&&input.Width>=250&&input.Height>=20&&form.RectangleToScreen(form.ClientRectangle).Contains(input.RectangleToScreen(input.ClientRectangle)));
                    Check("ui-visible-actions-"+size.Width,Children(form).OfType<Button>().All(b=>b.Parent.RectangleToScreen(b.Parent.ClientRectangle).Contains(b.RectangleToScreen(b.ClientRectangle))));
                }
                using(var bitmap=new System.Drawing.Bitmap(form.Width,form.Height)) {
                    form.DrawToBitmap(bitmap,new System.Drawing.Rectangle(0,0,form.Width,form.Height));
                    bitmap.Save(Path.Combine(Dir,"rolling-import-file-112.png"));
                }
                accept.PerformClick();
                Check("ui-explicit-accept-path-only",form.DialogResult==DialogResult.OK&&(string)type.GetProperty("SelectedPath").GetValue(form,null)==source);
            }
            using(var sha=System.Security.Cryptography.SHA256.Create())using(var file=File.OpenRead(source))Check("ui-source-package-bytes-unchanged",hash==Convert.ToBase64String(sha.ComputeHash(file)));
            var policy=typeof(BhtPaletteControl).GetMethod("IsNativeTransferCommand",BindingFlags.Static|BindingFlags.NonPublic);
            Check("native-transfer-recognized",(bool)policy.Invoke(null,new object[]{"bhtnaptichluy"}));
            Check("lisp-command-policy-unchanged",!(bool)policy.Invoke(null,new object[]{"BHTXULYDOAN"}));
        }
        catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nIMPORT112-ERROR "+ex);}
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nIMPORT112-TOTAL="+count+" FAIL="+fail);
    }
}
