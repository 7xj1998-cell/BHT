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
            panel.Controls.Add(new Label {AutoSize=true,Dock=DockStyle.Fill,Padding=new Padding(0,0,0,8),Text="Chọn đoạn và tim → Tính lý trình theo đoạn → kiểm tra hồ sơ, nhãn → Xác nhận kết quả đoạn.\nTính/xác nhận bổ sung chỉ xử lý hồ sơ chưa khóa; hồ sơ đã xác nhận giữ nguyên.\nXuất dữ liệu tích lũy để chuyển sang bản vẽ tiếp theo. Xuất hồ sơ đoạn chỉ chứa hồ sơ đã xác nhận của đoạn đó.\nPhía đường khác phía so với tim rãnh. Tệp ảnh vẫn là liên kết ngoài gói."});
            grid.Dock=DockStyle.Fill;grid.ReadOnly=true;grid.AllowUserToAddRows=grid.AllowUserToDeleteRows=false;grid.RowHeadersVisible=false;grid.MultiSelect=false;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells;grid.DefaultCellStyle.WrapMode=DataGridViewTriState.True;
            grid.ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            string[] cols={"Đoạn","Gói / tên công trình","Từ Km","Đến Km","Phía đường","Tim đã gán","Hồ sơ","Đã xác nhận"};int[] widths={90,245,110,110,85,145,65,100};
            for(int i=0;i<cols.Length;i++){grid.Columns.Add(cols[i],cols[i]);grid.Columns[i].Width=widths[i];}
            foreach(var seg in segments){double a,b;RouteOptionsForm.TryNumber(seg.Get("km_start_m"),out a);RouteOptionsForm.TryNumber(seg.Get("km_end_m"),out b);var own=objects.Where(r=>r.Get("doan")==seg.Get("segment_id")).ToList();
                grid.Rows.Add(seg.Get("segment_id"),seg.Get("package_id")+" / "+seg.Get("package_name"),Chainage.Format(a),Chainage.Format(b),seg.Get("side"),seg.Get("route_id"),own.Count,own.Count(ProgressPolicy.Locked));}
            panel.Controls.Add(grid);
            var actions=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoSize=true,WrapContents=true};actions.Controls.Add(new Label {Text="Tim tính đoạn:",AutoSize=true});routes.Items.AddRange(routeIds.Cast<object>().ToArray());actions.Controls.Add(routes);
            calculate.Text="Tính lý trình theo đoạn";finish.Text="Xác nhận kết quả đoạn";reopen.Text="Mở lại kết quả đoạn";
            foreach(var b in new[]{calculate,finish,reopen}){b.AutoSize=true;actions.Controls.Add(b);}
            calculate.Click+=(s,e)=>Choose("CALCULATE");
            finish.Click+=(s,e)=>{if(MessageBox.Show(this,"Xác nhận các hồ sơ chưa khóa đang thuộc đoạn? Hồ sơ đã xác nhận được giữ nguyên. Cần đối chiếu phía đường, nhãn và hồ sơ chưa gán. Thao tác này bảo vệ kết quả đã kiểm tra; không xác nhận số lượng khảo sát ngoài thực địa.","Xác nhận kết quả đoạn",MessageBoxButtons.OKCancel)==DialogResult.OK)Choose("FINISH");};
            reopen.Click+=(s,e)=>{if(MessageBox.Show(this,"Mở lại kết quả đoạn này để chỉnh sửa? Kết quả các đoạn khác vẫn được bảo vệ.","Mở lại kết quả đoạn",MessageBoxButtons.OKCancel)==DialogResult.OK)Choose("REOPEN");};panel.Controls.Add(actions);
            var files=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoSize=true};var all=new Button {Text="Xuất dữ liệu tích lũy (.bht)…",AutoSize=true};export.Text="Xuất hồ sơ đoạn (.bht)…";export.AutoSize=true;var cancel=new Button {Text="Đóng",AutoSize=true,DialogResult=DialogResult.Cancel};
            all.Click+=(s,e)=>{if(ExportRequested!=null)ExportRequested(null);};export.Click+=(s,e)=>{if(ExportRequested!=null&&Selected()!=null)ExportRequested(Selected().Get("segment_id"));};files.Controls.AddRange(new Control[]{all,export,cancel});panel.Controls.Add(files);Controls.Add(panel);CancelButton=cancel;
            grid.SelectionChanged+=(s,e)=>{var seg=Selected();if(seg!=null)routes.SelectedItem=seg.Get("route_id");UpdateActions();};routes.SelectedIndexChanged+=(s,e)=>UpdateActions();Shown+=(s,e)=>{grid.ClearSelection();UpdateActions();};UpdateActions();
        }
        BhtRecord Selected(){return grid.SelectedRows.Count==1?segments.First(s=>s.Get("segment_id")==Convert.ToString(grid.SelectedRows[0].Cells[0].Value)):null;}
        void UpdateActions(){var seg=Selected();var own=seg==null?new List<BhtRecord>():objects.Where(r=>r.Get("doan")==seg.Get("segment_id")).ToList();bool locked=own.Any(ProgressPolicy.Locked);calculate.Enabled=seg!=null&&routes.SelectedIndex>=0&&(!locked||Convert.ToString(routes.SelectedItem)==seg.Get("route_id"));finish.Enabled=own.Any(r=>!ProgressPolicy.Locked(r));reopen.Enabled=locked;export.Enabled=own.Count>0&&own.All(ProgressPolicy.Locked);}
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
            if(a.Length==0)return "CANCEL";
            var doc=AcApp.DocumentManager.MdiActiveDocument;
            string token=a.Length>1&&a[1].StartsWith("@PLAN:",StringComparison.Ordinal)?a[1].Substring(6):"";
            var form=new ProgressPreviewForm(a[0]+" — "+System.IO.Path.GetFileName(doc.Name),a.Skip(a.Length>1&&a[1].StartsWith("@PLAN:",StringComparison.Ordinal)?2:1),token!="");
            form.ApplyRequested=()=>{
                if(AcApp.DocumentManager.MdiActiveDocument!=doc){form.SetResult("Bản vẽ đang mở đã đổi. Chuyển về bản vẽ của bảng này hoặc lập lại phương án.",false);return;}
                string why;if(AcadDispatcher.IsBusy(doc,out why)){form.SetResult(why,false);return;}
                form.SetBusy(true);
                AcadDispatcher.RunLisp("bht:api-progress-preview-apply",new[]{token},reply=>{
                    if(form.IsDisposed)return;
                    form.BeginInvoke((Action)(()=>{if(!form.IsDisposed)form.SetResult(reply.Ok?string.Join("\r\n",reply.Values.ToArray()):reply.Error,true);}));
                });
            };
            Autodesk.AutoCAD.ApplicationServices.DocumentCollectionEventHandler closing=null;
            closing=(s,e)=>{if(e.Document==doc&&!form.IsDisposed)form.Close();};
            AcApp.DocumentManager.DocumentToBeDestroyed+=closing;
            form.FormClosed+=(s,e)=>AcApp.DocumentManager.DocumentToBeDestroyed-=closing;
            AcApp.ShowModelessDialog(form);
            return "PENDING";
        }
    }

    internal sealed class ProgressPreviewForm : Form
    {
        readonly Button apply=new Button {Text="Áp dụng phương án",AutoSize=true};
        readonly Label status=new Label {AutoSize=true,Dock=DockStyle.Fill,Padding=new Padding(4),Text="Bảng chỉ xem trước. Có thể zoom/pan trong CAD. Dữ liệu sẽ được kiểm tra lại khi bấm Áp dụng."};
        public Action ApplyRequested;
        public ProgressPreviewForm(string title,IEnumerable<string> rows,bool canApply)
        {
            Text=title;Size=new Size(1150,680);MinimumSize=new Size(760,420);AutoScaleMode=AutoScaleMode.Dpi;StartPosition=FormStartPosition.CenterScreen;
            var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Padding=new Padding(10)};
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var grid=new DataGridView {Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,RowHeadersVisible=false,
                AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells,ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.AutoSize};
            grid.DefaultCellStyle.WrapMode=DataGridViewTriState.True;
            foreach(string c in new[]{"ID hồ sơ","Mã biển","Lý trình","Đoạn đang gán","Xử lý / nguyên nhân"})grid.Columns.Add(c,c);
            for(int i=0;i<5;i++){grid.Columns[i].MinimumWidth=i==4?240:110;grid.Columns[i].FillWeight=i==4?260:100;}
            foreach(string row in rows)grid.Rows.Add(row.Split('\t').Cast<object>().ToArray());
            var buttons=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoSize=true};
            var close=new Button {Text="Đóng",AutoSize=true};close.Click+=(s,e)=>Close();
            apply.Enabled=canApply;apply.Click+=(s,e)=>{if(ApplyRequested!=null)ApplyRequested();};
            buttons.Controls.Add(apply);buttons.Controls.Add(close);
            layout.Controls.Add(status);layout.Controls.Add(grid);layout.Controls.Add(buttons);Controls.Add(layout);PaletteTheme.ApplyDialog(this);
            grid.BackgroundColor=Color.White;grid.DefaultCellStyle.BackColor=Color.White;grid.DefaultCellStyle.ForeColor=Color.FromArgb(20,55,43);
            grid.DefaultCellStyle.SelectionBackColor=Color.FromArgb(5,107,80);grid.DefaultCellStyle.SelectionForeColor=Color.White;
        }
        public void SetBusy(bool busy){apply.Enabled=!busy;status.Text=busy?"Đang kiểm tra và áp dụng…":status.Text;}
        public void SetResult(string text,bool consumed){status.Text=text;apply.Enabled=!consumed;}
    }
}
