using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Palette;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class CorrectionGrid133Probe
{
    static int count,fail;
    static void Check(string name,bool ok) { count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" CORRECTION132 "+name); }
    static IEnumerable<Control> Children(Control root) { foreach(Control c in root.Controls) { yield return c;foreach(var sub in Children(c))yield return sub; } }
    static void CheckGrid(DataGridView grid,string label)
    {
        foreach(DataGridViewColumn column in grid.Columns) {
            var style=column.HeaderCell.InheritedStyle;
            int width=Math.Max(20,column.Width-style.Padding.Horizontal-18);
            int textHeight=TextRenderer.MeasureText(column.HeaderText,style.Font,new Size(width,int.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.NoPrefix).Height;
            Check(label+"-header-"+column.Index,grid.ColumnHeadersHeight>=textHeight+style.Padding.Vertical+4);
        }
        foreach(DataGridViewRow row in grid.Rows)
            Check(label+"-row-"+row.Index,row.Height>=row.GetPreferredHeight(row.Index,DataGridViewAutoSizeRowMode.AllCells,true));
    }
    [CommandMethod("CORRECTIONGRID133")]
    public static void Run()
    {
        try {
            var record=new BhtRecord().Set("object_id","OBJ001").Set("progress_locked","1").Set("ma_hieu","W.207b").Set("so_tru","1").Set("so_mat","1")
                .Set("mo_ta","Giao nhau với đường không ưu tiên; đối chiếu dữ liệu khảo sát đã xác nhận")
                .Set("ghi_chu","Dòng một có dấu tiếng Việt: chuyển hướng.\nDòng hai: kiểm tra chiều cao và khoảng cách.");
            foreach(float scale in new[]{1f,1.25f,1.5f,2f}) {
                using(var form=new ObjectCorrectionForm(AcApp.DocumentManager.MdiActiveDocument,"OBJ001",record)) {
                    form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-4000,-4000);form.Show();
                    using(var font=new Font("Segoe UI",9f*scale)) {
                        form.Font=font;
                        foreach(var grid in Children(form).OfType<DataGridView>()) {
                            grid.Font=font;
                            if(grid.ReadOnly)grid.Rows.Add("Mô tả có nhiều dòng cần kiểm tra",record.Get("mo_ta"),record.Get("ghi_chu"));
                        }
                        foreach(var size in new[]{new Size(690,610),new Size(920,760)}) {
                            form.Size=size;Application.DoEvents();
                            int n=0;foreach(var grid in Children(form).OfType<DataGridView>())CheckGrid(grid,scale+"-"+size.Width+"-"+(n++));
                        }
                        if(scale==1.5f)using(var bitmap=new Bitmap(form.Width,form.Height)) {
                            form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));
                            bitmap.Save(Path.Combine(Path.GetDirectoryName(typeof(CorrectionGrid133Probe).Assembly.Location),"correction-grid-150.png"));
                        }
                    }
                }
            }
            using(var form=ObjectCorrectionForm.HistoryWindow(new List<BhtRecord>(),"OBJ001")) {
                form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-4000,-4000);form.Show();
                form.Size=new Size(650,450);Application.DoEvents();CheckGrid(Children(form).OfType<DataGridView>().Single(),"history");
            }
        } catch(System.Exception ex) { fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-ERROR "+ex); }
        AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCORRECTION132-TOTAL="+count+" FAIL="+fail+"\n");
    }
}
