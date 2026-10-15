using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
using BHT.Core;
using BHT.Palette;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Selection124Probe
{
    static readonly List<string> log = new List<string>();
    static void Check(string name, bool ok) { log.Add((ok ? "PASS " : "FAIL ") + name); File.WriteAllLines(Path.Combine(Path.GetDirectoryName(typeof(Selection124Probe).Assembly.Location),"selection124-result.txt"),log); }
    static void Open(Form f) { f.StartPosition = FormStartPosition.Manual; f.Location = new Point(-3000,-3000); f.Show(); Application.DoEvents(); }
    static IEnumerable<Control> All(Control c) { foreach(Control child in c.Controls) { yield return child; foreach(var nested in All(child)) yield return nested; } }
    [CommandMethod("SELECTION124PROBE")]
    public static void Run()
    {
        string folder = Path.GetDirectoryName(typeof(Selection124Probe).Assembly.Location);
        try
        {
            var db = AcApp.DocumentManager.MdiActiveDocument.Database;
            Check("probe-start",true);
            var fields = SignContentBlocks.Fields("I.449");
            Check("street-two-editable-fields-in-order", fields.Count==2 && fields[0].Key=="HEADER#0" && fields[0].Sample=="ĐƯỜNG" && fields[1].Key=="NAME#0" && fields[1].Sample=="");
            var template = TdtSignLibrary.EnsureBlock(db,"I.449"); Check("native-street-template",template.Ok);
            foreach(string title in new[]{ "KÊNH GIÀ MIẾNG", "Tên đường rất dài giữ tiếng Việt Nguyễn Thị Minh Khai", "AH112" })
            {
                var face = new SignContentFace { Index=0, Code="I.449" }; face.Values["NAME#0"]=title;
                string name=SignContentBlocks.Ensure(db,template.BlockName,"I.449",0,SignContent.Write(new[]{face}));
                Check("legacy-name-default-header-cache",name==SignCorrections.StreetName(db,template.BlockName,"ĐƯỜNG",title));
                foreach(string blockName in new[]{name,TdtSignLibrary.EnsureOutlineBlock(db,name)})
                using(var tr=db.TransactionManager.StartOpenCloseTransaction())
                {
                    var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);var block=(BlockTableRecord)tr.GetObject(bt[blockName],OpenMode.ForRead);
                    var texts=block.Cast<ObjectId>().Select(id=>tr.GetObject(id,OpenMode.ForRead)).OfType<DBText>().ToArray();
                    Check("two-lines-retain-name-and-header",texts.Length==2 && texts.Any(t=>t.TextString==title) && texts.Any(t=>t.TextString=="ĐƯỜNG"));
                    var lower=texts.Single(t=>t.TextString==title); var upper=texts.Single(t=>t.TextString=="ĐƯỜNG");
                    Check("upper-above-lower-with-no-overlap",upper.GeometricExtents.MinPoint.Y>lower.GeometricExtents.MaxPoint.Y);
                    using(var reference=new BlockReference(Point3d.Origin,block.ObjectId))
                    {
                        var b=reference.GeometricExtents;
                        Check("both-lines-fit-compact-plate",b.MaxPoint.X-b.MinPoint.X<=3.21 && texts.All(t=>t.GeometricExtents.MinPoint.X>=b.MinPoint.X && t.GeometricExtents.MaxPoint.X<=b.MaxPoint.X && t.GeometricExtents.MinPoint.Y>.6 && t.GeometricExtents.MaxPoint.Y<1.25));
                    }
                }
                face.Values["HEADER#0"]="ĐƯỜNG TỈNH";
                Check("changed-header-has-new-block",name!=SignContentBlocks.Ensure(db,template.BlockName,"I.449",0,SignContent.Write(new[]{face})));
            }
            List<BhtRecord> rows; BhtRecord obj;
            using(var input=new Database(false,true)) {
                input.ReadDwgFile(Path.Combine(folder,"input.dwg"),FileOpenMode.OpenForReadAndAllShare,false,"");input.CloseInput(true);
                rows=SegmentStore.Read(input);
                using(var tr=input.TransactionManager.StartOpenCloseTransaction()) obj=BhtStore.Read(tr,input,"OBJ","OBJ-000058");
            }
            using(var form=new SegmentPickerForm("OBJ-000058",obj,rows))
            {
                Open(form);var grid=All(form).OfType<DataGridView>().Single();var button=All(form).OfType<Button>().Single(b=>b.Text=="Gán đoạn đã chọn");
                grid.ClearSelection(); Check("no-selection-disables-assignment",!button.Enabled);
                Check("all-15-segments-visible",grid.Rows.Count==15);
                var row=grid.Rows.Cast<DataGridViewRow>().Single(r=>Convert.ToString(r.Cells[0].Value)=="DOAN_10");
                Check("real-mismatch-is-explained",Convert.ToString(row.Cells[7].Value)=="Khác phía đường");
                row.Selected=true;Check("selected-row-enables-assignment",button.Enabled);
                using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));bitmap.Save(Path.Combine(folder,"bang-chon-doan.png"));}
                button.PerformClick(); Check("selection-returns-exact-ID",form.Result.SequenceEqual(new[]{"SELECT","DOAN_10"}));
            }
            using(var form=new SegmentPickerForm("OBJ-000058",obj,rows)) { Open(form);All(form).OfType<Button>().Single(b=>b.Text=="Bỏ gán đoạn").PerformClick();Check("clear-explicit-result",form.Result.SequenceEqual(new[]{"CLEAR"})); }
            using(var form=new SegmentPickerForm("OBJ-000058",obj,rows)) { Open(form);All(form).OfType<Button>().Single(b=>b.Text=="Hủy").PerformClick();Check("cancel-has-no-result",form.Result==null); }
        }
        catch(System.Exception ex) { log.Add("FAIL "+ex); }
        File.WriteAllLines(Path.Combine(folder,"selection124-result.txt"),log);
    }
}
