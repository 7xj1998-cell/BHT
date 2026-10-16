using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BHT.Core;

namespace BHT.Palette
{
    internal sealed class ImportConflictForm : Form
    {
        readonly IList<ImportConflict> conflicts;
        readonly DataGridView grid=new DataGridView();
        readonly TextBox search=new TextBox();
        readonly TextBox detail=new TextBox();
        readonly Label count=new Label();
        public ImportConflictForm(IList<ImportConflict> items)
        {
            conflicts=items;
            Text="BHT "+BhtVersion.Version+" - Đối chiếu dữ liệu nạp";
            Size=new Size(1120,760);MinimumSize=new Size(820,620);
            StartPosition=FormStartPosition.CenterParent;AutoScaleMode=AutoScaleMode.Dpi;
            var layout=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(12),ColumnCount=1,RowCount=6};
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute,110));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var note=new Label {AutoSize=true,Dock=DockStyle.Fill,Text="Chưa nạp do có xung đột; bản vẽ giữ nguyên. Cùng mã OBJ có thể là hai hồ sơ khác nhau.\r\nChọn dòng để đọc đủ hai giá trị và đối chiếu điểm gốc. Có thể zoom/pan CAD khi bảng mở."};
            var filter=new TableLayoutPanel {Dock=DockStyle.Fill,AutoSize=true,ColumnCount=2};
            filter.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            filter.Controls.Add(new Label {Text="Tìm mã / nội dung:",AutoSize=true,Anchor=AnchorStyles.Left},0,0);
            search.Dock=DockStyle.Fill;filter.Controls.Add(search,1,0);
            count.AutoSize=true;count.Dock=DockStyle.Fill;
            grid.Dock=DockStyle.Fill;grid.ReadOnly=true;grid.AllowUserToAddRows=false;grid.AllowUserToDeleteRows=false;
            grid.RowHeadersVisible=false;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;grid.MultiSelect=false;
            grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;grid.AllowUserToOrderColumns=true;
            grid.ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.ColumnHeadersDefaultCellStyle.WrapMode=DataGridViewTriState.True;
            grid.ColumnHeadersDefaultCellStyle.Padding=new Padding(5);
            foreach(string title in new[]{"Loại","Mã đối tượng","Nội dung khác biệt","Trường dữ liệu","Bản vẽ hiện tại","Tệp BHT nạp vào"})grid.Columns.Add(title,title);
            grid.Columns[0].FillWeight=55;grid.Columns[1].FillWeight=85;grid.Columns[2].FillWeight=130;
            detail.Dock=DockStyle.Fill;detail.Multiline=true;detail.ReadOnly=true;detail.ScrollBars=ScrollBars.Vertical;
            var buttons=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoSize=true,FlowDirection=FlowDirection.RightToLeft};
            var close=new Button {Text="Đóng",AutoSize=true,DialogResult=DialogResult.Cancel};
            close.Click+=(s,e)=>Close();
            var copy=new Button {Text="Sao chép toàn bộ đối chiếu",AutoSize=true};
            copy.Click+=(s,e)=>{try{Clipboard.SetText(string.Join("\r\n\r\n",conflicts.Select(c=>c.ToString())));}catch(Exception ex){MessageBox.Show(this,ex.Message,"Chưa sao chép được");}};
            buttons.Controls.Add(close);buttons.Controls.Add(copy);CancelButton=close;
            layout.Controls.Add(note,0,0);layout.Controls.Add(filter,0,1);layout.Controls.Add(count,0,2);
            layout.Controls.Add(grid,0,3);layout.Controls.Add(detail,0,4);layout.Controls.Add(buttons,0,5);Controls.Add(layout);
            search.TextChanged+=(s,e)=>Fill();grid.SelectionChanged+=(s,e)=>ShowDetail();
            PaletteTheme.ApplyTableDialog(this);
            grid.BackgroundColor=Color.White;
            grid.DefaultCellStyle.ForeColor=Color.FromArgb(24,45,39);
            grid.DefaultCellStyle.BackColor=Color.White;
            grid.DefaultCellStyle.SelectionBackColor=Color.FromArgb(6,95,70);
            grid.DefaultCellStyle.SelectionForeColor=Color.White;
            grid.DefaultCellStyle.WrapMode=DataGridViewTriState.True;
            grid.AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.DisplayedCells;
            Fill();
        }
        static string FieldName(string field)
        {
            string[] keys={"nhom","ma_hieu","mat","pt","so_tru","so_mat","marker_km","marker_h","sign_content","phia_duong","anh","km_start_m","km_end_m","side","package_id","route_id","ly_trinh_m","ly_trinh_km","offset_m","doan","goi"};
            string[] labels={"Nhóm báo hiệu","Mã hiệu","Mặt biển","Điểm chân trụ","Số trụ","Số mặt","Km cọc tiêu","Hm cọc tiêu","Nội dung biển","Phía đường","Ảnh khảo sát","Lý trình đầu (m)","Lý trình cuối (m)","Phía tuyến","Gói thầu","Tim tuyến","Lý trình (m)","Lý trình Km","Khoảng cách đến tim (m)","Đoạn","Gói thầu"};
            int i=Array.IndexOf(keys,field);return i<0?field:labels[i]+" ("+field+")";
        }
        void Fill()
        {
            string term=search.Text.Trim();grid.Rows.Clear();
            foreach(var c in conflicts.Where(c=>term==""||(c.ToString()+" "+FieldName(c.Field)).IndexOf(term,StringComparison.CurrentCultureIgnoreCase)>=0))
                grid.Rows[grid.Rows.Add(c.Kind,c.Id,c.Reason,FieldName(c.Field),c.Current,c.Incoming)].Tag=c;
            count.Text=conflicts.Select(c=>c.Kind+"/"+c.Id).Distinct().Count()+" mục xung đột; "+conflicts.Count+" khác biệt. Đang hiển thị "+grid.Rows.Count+" dòng.";
            if(grid.Rows.Count>0)grid.Rows[0].Selected=true;ShowDetail();
        }
        void ShowDetail()
        {
            var c=grid.SelectedRows.Count==0?null:grid.SelectedRows[0].Tag as ImportConflict;
            detail.Text=c==null?"":c.Kind+" / "+c.Id+" - "+FieldName(c.Field)+"\r\nBẢN VẼ HIỆN TẠI:\r\n"+c.Current+"\r\nTỆP BHT NẠP VÀO:\r\n"+c.Incoming;
        }
    }
}
