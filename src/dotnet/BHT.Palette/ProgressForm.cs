using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Application;
[assembly:CommandClass(typeof(BHT.Palette.ProgressFormFunctions))]
namespace BHT.Palette
{
    public sealed class ProgressForm : Form
    {
        readonly DataGridView grid=new DataGridView();
        readonly ComboBox routes=new NoWheelComboBox {DropDownStyle=ComboBoxStyle.DropDownList,Width=220};
        readonly Button calculate=new Button(),finish=new Button(),reopen=new Button(),export=new Button();
        readonly List<BhtRecord> segments,objects;
        public string[] Result {get;private set;}
        public Action<string> ExportRequested;
        public ProgressForm(List<BhtRecord> segments,List<BhtRecord> objects,IEnumerable<string> routeIds)
        {
            this.segments=segments;this.objects=objects;
            Text="Quản lý xử lý theo đoạn";Font=new System.Drawing.Font("Segoe UI",9);AutoScaleMode=AutoScaleMode.Dpi;
            Size=new Size(1150,650);MinimumSize=new Size(900,480);StartPosition=FormStartPosition.CenterParent;
            var panel=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(10),ColumnCount=1,RowCount=4};
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));panel.RowStyles.Add(new RowStyle(SizeType.Percent,100));panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(new Label {AutoSize=true,Dock=DockStyle.Fill,Padding=new Padding(0,0,0,8),Text="Chọn đoạn và tim → Tính lý trình theo đoạn → kiểm tra hồ sơ, nhãn → Xác nhận kết quả đoạn.\nKết quả đã xác nhận được bảo vệ khi xử lý đoạn khác; mở lại đúng đoạn nếu cần sửa.\nXuất dữ liệu tích lũy để chuyển sang bản vẽ tiếp theo. Xuất hồ sơ đoạn chỉ chứa hồ sơ đã xác nhận của đoạn đó.\nPhía đường khác phía so với tim rãnh. Tệp ảnh vẫn là liên kết ngoài gói."});
            grid.Dock=DockStyle.Fill;grid.ReadOnly=true;grid.AllowUserToAddRows=grid.AllowUserToDeleteRows=false;grid.RowHeadersVisible=false;grid.MultiSelect=false;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells;grid.DefaultCellStyle.WrapMode=DataGridViewTriState.True;
            string[] cols={"Đoạn","Gói / tên công trình","Từ Km","Đến Km","Phía đường","Tim đã gán","Hồ sơ","Đã xác nhận"};int[] widths={90,245,110,110,85,145,65,100};
            for(int i=0;i<cols.Length;i++){grid.Columns.Add(cols[i],cols[i]);grid.Columns[i].Width=widths[i];}
            foreach(var seg in segments){double a,b;RouteOptionsForm.TryNumber(seg.Get("km_start_m"),out a);RouteOptionsForm.TryNumber(seg.Get("km_end_m"),out b);var own=objects.Where(r=>r.Get("doan")==seg.Get("segment_id")).ToList();
                grid.Rows.Add(seg.Get("segment_id"),seg.Get("package_id")+" / "+seg.Get("package_name"),Chainage.Format(a),Chainage.Format(b),seg.Get("side"),seg.Get("route_id"),own.Count,own.Count(ProgressPolicy.Locked));}
            panel.Controls.Add(grid);
            var actions=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoSize=true,WrapContents=true};actions.Controls.Add(new Label {Text="Tim tính đoạn:",AutoSize=true});routes.Items.AddRange(routeIds.Cast<object>().ToArray());actions.Controls.Add(routes);
            calculate.Text="Tính lý trình theo đoạn";finish.Text="Xác nhận kết quả đoạn";reopen.Text="Mở lại kết quả đoạn";
            foreach(var b in new[]{calculate,finish,reopen}){b.AutoSize=true;actions.Controls.Add(b);}
            calculate.Click+=(s,e)=>Choose("CALCULATE");
            finish.Click+=(s,e)=>{if(MessageBox.Show(this,"Xác nhận các hồ sơ đang thuộc đoạn? Cần đối chiếu phía đường, nhãn và hồ sơ chưa gán. Thao tác này bảo vệ kết quả đã kiểm tra; không xác nhận số lượng khảo sát ngoài thực địa.","Xác nhận kết quả đoạn",MessageBoxButtons.OKCancel)==DialogResult.OK)Choose("FINISH");};
            reopen.Click+=(s,e)=>{if(MessageBox.Show(this,"Mở lại kết quả đoạn này để chỉnh sửa? Kết quả các đoạn khác vẫn được bảo vệ.","Mở lại kết quả đoạn",MessageBoxButtons.OKCancel)==DialogResult.OK)Choose("REOPEN");};panel.Controls.Add(actions);
            var files=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoSize=true};var all=new Button {Text="Xuất dữ liệu tích lũy (.bht)…",AutoSize=true};export.Text="Xuất hồ sơ đoạn (.bht)…";export.AutoSize=true;var cancel=new Button {Text="Đóng",AutoSize=true,DialogResult=DialogResult.Cancel};
            all.Click+=(s,e)=>{if(ExportRequested!=null)ExportRequested(null);};export.Click+=(s,e)=>{if(ExportRequested!=null&&Selected()!=null)ExportRequested(Selected().Get("segment_id"));};files.Controls.AddRange(new Control[]{all,export,cancel});panel.Controls.Add(files);Controls.Add(panel);CancelButton=cancel;
            grid.SelectionChanged+=(s,e)=>{var seg=Selected();if(seg!=null)routes.SelectedItem=seg.Get("route_id");UpdateActions();};routes.SelectedIndexChanged+=(s,e)=>UpdateActions();Shown+=(s,e)=>{grid.ClearSelection();UpdateActions();};UpdateActions();
        }
        BhtRecord Selected(){return grid.SelectedRows.Count==1?segments.First(s=>s.Get("segment_id")==Convert.ToString(grid.SelectedRows[0].Cells[0].Value)):null;}
        void UpdateActions(){var seg=Selected();var own=seg==null?new List<BhtRecord>():objects.Where(r=>r.Get("doan")==seg.Get("segment_id")).ToList();bool locked=own.Any(ProgressPolicy.Locked);calculate.Enabled=seg!=null&&routes.SelectedIndex>=0&&!locked;finish.Enabled=own.Count>0&&!locked;reopen.Enabled=locked;export.Enabled=own.Count>0&&own.All(ProgressPolicy.Locked);}
        void Choose(string action){var seg=Selected();if(seg==null)return;Result=new[]{action,seg.Get("segment_id"),Convert.ToString(routes.SelectedItem)};DialogResult=DialogResult.OK;Close();}
    }
    public sealed class ProgressFormFunctions
    {
        static ResultBuffer Reply(params string[] v){var r=new ResultBuffer();foreach(string s in v)r.Add(new TypedValue((int)LispDataType.Text,s));return r;}
        [LispFunction("BHTPROGRESSFORM")]
        public ResultBuffer Open(ResultBuffer args)
        {
            if(PaletteCommands.IsCoreConsole())return Reply("CANCEL","","");
            try{var doc=AcApp.DocumentManager.MdiActiveDocument;List<BhtRecord> objects;List<string> routes;
                using(var tr=doc.Database.TransactionManager.StartOpenCloseTransaction()){objects=BhtStore.All(tr,doc.Database,"OBJ").Select(p=>p.Value).ToList();routes=BhtStore.Keys(tr,doc.Database,"ROUTE");}
                using(var form=new ProgressForm(SegmentStore.Read(doc.Database),objects,routes)){
                    form.ExportRequested=segment=>{try{using(var pick=new SaveFileDialog {Filter="BHT (*.bht)|*.bht",FileName=segment==null?"BHT-du-lieu-tich-luy.bht":segment+"-ho-so.bht"}){
                        if(pick.ShowDialog(form)!=DialogResult.OK)return;if(AcApp.DocumentManager.MdiActiveDocument!=doc)throw new InvalidOperationException("Bản vẽ đã đổi; mở lại bảng.");
                        ProjectTransfer.ExportSelection(doc.Database,pick.FileName,segment);MessageBox.Show(form,"Đã xuất "+pick.FileName+"\nGiữ lý trình và nhãn. Tệp ảnh là liên kết ngoài gói.","BHT");
                    }}catch(System.Exception ex){MessageBox.Show(form,ex.Message,"Không xuất được dữ liệu");}};
                    return AcApp.ShowModalDialog(form)==DialogResult.OK?Reply(form.Result):Reply("CANCEL","","");
                }
            }catch(System.Exception ex){AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nBHT: "+ex.Message);return Reply("CANCEL","","");}
        }
        [LispFunction("BHTPROGRESSPREVIEW")]
        public string Preview(ResultBuffer args)
        {
            if(PaletteCommands.IsCoreConsole())return "CANCEL";
            var a=args.AsArray().Where(v=>v.TypeCode==(int)LispDataType.Text).Select(v=>Convert.ToString(v.Value)).ToArray();
            using(var form=new Form {Text=a[0],Size=new Size(1100,650),StartPosition=FormStartPosition.CenterParent}){
                var grid=new DataGridView {Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,RowHeadersVisible=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};foreach(string c in new[]{"ID hồ sơ","Mã biển","Lý trình","Đoạn đang gán","Xử lý / nguyên nhân"})grid.Columns.Add(c,c);foreach(string row in a.Skip(1))grid.Rows.Add(row.Split('\t').Cast<object>().ToArray());
                var buttons=new FlowLayoutPanel {Dock=DockStyle.Bottom,Height=42};var ok=new Button {Text="Áp dụng",DialogResult=DialogResult.OK};var cancel=new Button {Text="Hủy",DialogResult=DialogResult.Cancel};buttons.Controls.AddRange(new Control[]{ok,cancel});form.Controls.Add(grid);form.Controls.Add(buttons);form.CancelButton=cancel;return AcApp.ShowModalDialog(form)==DialogResult.OK?"OK":"CANCEL";
            }
        }
    }
}
