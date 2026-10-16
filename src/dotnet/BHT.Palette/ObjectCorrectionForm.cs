using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BHT.Core;
using BHT.Bridge;
using Autodesk.AutoCAD.ApplicationServices;

namespace BHT.Palette
{
    public sealed class ObjectCorrectionForm : Form
    {
        readonly Document document;
        readonly string id;
        readonly BhtRecord original;
        readonly ObjectCorrection service;
        readonly ComboBox kind = new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill };
        readonly TextBox reason = new TextBox { Dock=DockStyle.Fill };
        readonly TextBox author = new TextBox { Dock=DockStyle.Fill,Text=Environment.UserName };
        readonly DataGridView edit = Grid(), diff = Grid();
        readonly CheckBox baseline = new CheckBox { AutoSize=true,Text="Đồng ý lưu bản gốc cũ; nguồn gốc bản cũ chưa được xác minh" };
        readonly CheckBox assignment = new CheckBox { AutoSize=true,Text="Duyệt chuyển sang đoạn hiển thị trong bảng đối chiếu" };
        readonly Button preview = new Button { Text="Xem trước / tính riêng",AutoSize=true };
        readonly Button confirm = new Button { Text="Xác nhận hiệu chỉnh",AutoSize=true,Enabled=false };
        readonly Label status = new Label { AutoSize=true,MaximumSize=new Size(850,0) };
        CorrectionPreview plan;
        bool busy;
        public event Action Saved;
        static DataGridView Grid()
        {
            var grid=new DataGridView { Dock=DockStyle.Fill,AllowUserToAddRows=false,AllowUserToDeleteRows=false,RowHeadersVisible=false,
                AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells,
                ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.AutoSize,EnableHeadersVisualStyles=false };
            // Header and row metrics must follow the inherited font, including CAD DPI scaling.
            grid.ColumnHeadersDefaultCellStyle=new DataGridViewCellStyle { BackColor=Color.FromArgb(209,250,229),ForeColor=Color.FromArgb(6,95,70),
                SelectionBackColor=Color.FromArgb(209,250,229),SelectionForeColor=Color.FromArgb(6,95,70),
                Padding=new Padding(6,5,6,5),WrapMode=DataGridViewTriState.True,Alignment=DataGridViewContentAlignment.MiddleLeft };
            grid.DefaultCellStyle.Padding=new Padding(5,3,5,3);
            grid.DefaultCellStyle.WrapMode=DataGridViewTriState.True;
            return grid;
        }
        public ObjectCorrectionForm(Document doc,string objectId,BhtRecord record)
        {
            document=doc;id=objectId;original=record.Clone();service=new ObjectCorrection(doc.Database);
            Text="Hiệu chỉnh hồ sơ đã xác nhận - "+id;Size=new Size(920,760);MinimumSize=new Size(690,610);StartPosition=FormStartPosition.CenterScreen;
            var layout=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=2,RowCount=10,Padding=new Padding(12) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,135));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            for(int i=0;i<10;i++)layout.RowStyles.Add(new RowStyle(i==4||i==5?SizeType.Percent:SizeType.AutoSize,i==4?45:i==5?55:0));
            foreach(var input in new Control[]{kind,reason,author})input.Margin=new Padding(3,4,3,4);
            var note=new Label { AutoSize=true,MaximumSize=new Size(850,0),Text="Chỉ hiệu chỉnh hồ sơ "+id+". Giữ khóa và lưu bản trước khi sửa. Dữ liệu đo gốc được giữ nguyên. Có thể tiếp tục thao tác CAD khi cửa sổ này mở." };
            layout.Controls.Add(note,0,0);layout.SetColumnSpan(note,2);
            kind.Items.AddRange(new object[]{"Thuộc tính khảo sát","Liên kết điểm RTK","Tính lại lý trình riêng","Phân đoạn / gói thầu","Ngừng sử dụng (giữ lịch sử và điểm RTK)","Lưu thay đổi trình bày GLOBAL"});
            layout.Controls.Add(new Label { Text="Loại hiệu chỉnh",AutoSize=true },0,1);layout.Controls.Add(kind,1,1);
            layout.Controls.Add(new Label { Text="Lý do",AutoSize=true },0,2);layout.Controls.Add(reason,1,2);
            layout.Controls.Add(new Label { Text="Người hiệu chỉnh",AutoSize=true },0,3);layout.Controls.Add(author,1,3);
            edit.Columns.Add("field","Nội dung");edit.Columns.Add("value","Giá trị đề nghị");edit.Columns[0].ReadOnly=true;
            diff.ReadOnly=true;diff.Columns.Add("field","Trường thay đổi");diff.Columns.Add("before","Trước");diff.Columns.Add("after","Sau");
            layout.Controls.Add(edit,0,4);layout.SetColumnSpan(edit,2);layout.Controls.Add(diff,0,5);layout.SetColumnSpan(diff,2);
            layout.Controls.Add(baseline,0,6);layout.SetColumnSpan(baseline,2);layout.Controls.Add(assignment,0,7);layout.SetColumnSpan(assignment,2);
            layout.Controls.Add(status,0,8);layout.SetColumnSpan(status,2);
            var buttons=new FlowLayoutPanel { AutoSize=true,Dock=DockStyle.Fill };buttons.Controls.Add(preview);buttons.Controls.Add(confirm);
            var close=new Button { Text="Đóng",AutoSize=true };close.Click+=(s,e)=>Close();buttons.Controls.Add(close);
            layout.Controls.Add(buttons,0,9);layout.SetColumnSpan(buttons,2);Controls.Add(layout);
            kind.SelectedIndexChanged+=(s,e)=>LoadFields();reason.TextChanged+=(s,e)=>InvalidatePlan();author.TextChanged+=(s,e)=>InvalidatePlan();edit.CellValueChanged+=(s,e)=>InvalidatePlan();
            preview.Click+=(s,e)=>Preview();confirm.Click+=(s,e)=>Confirm();kind.SelectedIndex=0;
            baseline.CheckedChanged+=(s,e)=>UpdateConfirmation();assignment.CheckedChanged+=(s,e)=>UpdateConfirmation();
            PaletteTheme.ApplyDialog(this);
            layout.SizeChanged+=(s,e)=> {
                int width=Math.Max(200,layout.ClientSize.Width-layout.Padding.Horizontal-6);
                note.MaximumSize=status.MaximumSize=new Size(width,0);
                baseline.MaximumSize=assignment.MaximumSize=new Size(width,0);
            };
            foreach(var grid in new[]{edit,diff}) {
                grid.BackgroundColor=Color.White;grid.ForeColor=Color.FromArgb(24,45,39);
                grid.DefaultCellStyle.ForeColor=Color.FromArgb(24,45,39);grid.DefaultCellStyle.BackColor=Color.White;
                grid.DefaultCellStyle.SelectionBackColor=Color.FromArgb(6,95,70);grid.DefaultCellStyle.SelectionForeColor=Color.White;
                grid.DefaultCellStyle.WrapMode=DataGridViewTriState.True;
            }
        }
        void InvalidatePlan() {
            plan=null;confirm.Enabled=false;diff.Rows.Clear();baseline.Checked=assignment.Checked=false;
            baseline.Visible=assignment.Visible=false;
            status.Text="Chưa có đối chiếu hợp lệ. Sửa giá trị đề nghị, nhập lý do rồi bấm Xem trước / tính riêng.";
        }
        void UpdateConfirmation() {
            confirm.Enabled=!busy&&plan!=null&&(!plan.LegacyBaseline||baseline.Checked)&&(!plan.AssignmentChanged||assignment.Checked);
            if(plan==null||busy)return;
            status.Text=plan.LegacyBaseline&&!baseline.Checked?"Cần đồng ý lưu bản gốc cũ chưa được xác minh trước khi xác nhận.":
                plan.AssignmentChanged&&!assignment.Checked?"Cần kiểm tra đoạn trước/sau và duyệt chuyển đoạn trước khi xác nhận.":
                "Đã có đối chiếu. Kiểm tra toàn bộ khác biệt rồi bấm Xác nhận hiệu chỉnh nếu đồng ý.";
        }
        void Add(string field,string title) { int i=edit.Rows.Add(title,field=="pt"?string.Join(";",original.GetAll(field)):original.Get(field));edit.Rows[i].Tag=field; }
        void LoadFields()
        {
            InvalidatePlan();edit.Rows.Clear();
            if(kind.SelectedIndex==0) { Add("ma_hieu","Mã biển");Add("mo_ta","Mô tả");Add("so_tru","Số trụ");Add("so_mat","Số mặt");Add("tinh_trang","Tình trạng");Add("ghi_chu","Ghi chú"); }
            if(kind.SelectedIndex==1)Add("pt","Mã điểm RTK (phân cách bằng dấu ;)");
            if(kind.SelectedIndex==1||kind.SelectedIndex==3)Add("doan","Mã đoạn (gói lấy theo bảng đoạn)");
            status.Text=kind.SelectedIndex==2?"Tính lại bằng tim và mốc của đoạn hiện hành; không nhập đè lý trình thủ công.":"Sửa thông tin đề nghị, nhập lý do rồi bấm Xem trước.";
            if(kind.SelectedIndex==5)status.Text="Hoàn thành MOVE/ROTATE/chỉnh nhãn và đường dẫn trên CAD, nhập lý do rồi xem trước. Chỉ tăng phiên bản trình bày.";
        }
        bool Active() { if(AcadDispatcher.ActiveDocument==document)return true;status.Text="Đã chuyển bản vẽ. Quay lại bản vẽ gốc hoặc đóng cửa sổ này.";return false; }
        void Preview()
        {
            if(busy||!Active())return;edit.EndEdit();var fields=new BhtRecord();var selected=(CorrectionKind)kind.SelectedIndex;
            foreach(DataGridViewRow row in edit.Rows) {
                string key=(string)row.Tag,value=Convert.ToString(row.Cells[1].Value).Trim();
                var values=key=="pt"?value.Split(new[]{';',',',' '},StringSplitOptions.RemoveEmptyEntries).Select(x=>x.ToUpperInvariant()).ToArray():new[]{key=="doan"?value.ToUpperInvariant():value};
                if(!original.GetAll(key).SequenceEqual(values))fields.SetAll(key,values);
            }
            InvalidatePlan();Busy(true);
            AcadDispatcher.RunInCommandContext("Xem trước hiệu chỉnh",()=> { if(!Active())return OpResult.Fail("Bản vẽ đã đổi.");plan=service.Preview(id,fields,selected,reason.Text,author.Text);
                if(!ObjectRevision.Same(ObjectRevision.Governed(plan.Before),ObjectRevision.Governed(original))) { plan=null;return OpResult.Fail("Hồ sơ đã đổi sau khi mở cửa sổ. Đóng rồi mở lại hiệu chỉnh."); }
                return OpResult.Success("Xem trước thành công."); },r=> {
                if(IsDisposed)return;Busy(false);status.Text=r.Message;if(!r.Ok||plan==null)return;
                foreach(string key in ObjectRevision.Differences(plan.Before,plan.After))diff.Rows.Add(key,string.Join("; ",plan.Before.GetAll(key)),string.Join("; ",plan.After.GetAll(key)));
                baseline.Visible=plan.LegacyBaseline;assignment.Visible=plan.AssignmentChanged;UpdateConfirmation();
            });
        }
        void Confirm()
        {
            if(busy||!Active()||plan==null)return;
            var accepted=plan;bool legacy=baseline.Checked,segment=assignment.Checked;
            if(accepted.LegacyBaseline&&!legacy || accepted.AssignmentChanged&&!segment) { status.Text="Đánh dấu các mục cần duyệt trước khi xác nhận.";return; }
            confirm.Enabled=false;Busy(true);
            AcadDispatcher.RunInCommandContext("Xác nhận hiệu chỉnh",()=> { service.Confirm(accepted,legacy,segment);return OpResult.Success("Đã lưu phiên bản mới và cập nhật ký hiệu hiện có."); },r=> {
                if(IsDisposed)return;Busy(false);status.Text=r.Message;
                if(r.Ok) { var saved=Saved;if(saved!=null)saved();MessageBox.Show(this,r.Message,Text);Close(); }
                else { plan=null;confirm.Enabled=false; }
            });
        }
        void Busy(bool value) { busy=value;preview.Enabled=kind.Enabled=edit.Enabled=reason.Enabled=author.Enabled=!value; }
        public static Form HistoryWindow(IList<BhtRecord> history,string id)
        {
            var form=new Form { Text="Lịch sử hồ sơ - "+id,Size=new Size(920,650),MinimumSize=new Size(650,450) };
            var grid=Grid();grid.ReadOnly=true;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;grid.MultiSelect=false;
            foreach(string title in new[]{"Trạng thái","Thời điểm UTC","Người sửa","Lý do","Revision"})grid.Columns.Add(title,title);
            var detail=new TextBox { Dock=DockStyle.Bottom,Height=280,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Both,WordWrap=false };
            foreach(var r in history)grid.Rows[grid.Rows.Add(r.Get("revision_status"),r.Get("revision_time"),r.Get("revision_author"),r.Get("revision_reason"),r.Get("revision_id"))].Tag=r;
            grid.SelectionChanged+=(s,e)=> { if(grid.SelectedRows.Count>0)detail.Text=ObjectRevision.ReadHistory((BhtRecord)grid.SelectedRows[0].Tag).ToCanonical().Replace("\n","\r\n"); };
            form.Controls.Add(grid);form.Controls.Add(detail);PaletteTheme.ApplyDialog(form);
            grid.DefaultCellStyle.ForeColor=Color.FromArgb(24,45,39);grid.DefaultCellStyle.BackColor=Color.White;return form;
        }
    }
}
