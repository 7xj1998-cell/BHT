using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BHT.Core;

namespace BHT.Palette
{
    internal sealed class ImportedMarkersForm : Form
    {
        public readonly string Snapshot;
        private readonly DataGridView markers = new DataGridView();
        private readonly Button use = new Button();
        public string ManualId, ManualValue;
        public ImportedMarkersForm(IList<string> values)
        {
            Snapshot=values.Count>0?values[0]:"";
            Text="Mốc lý trình đã nhập"; Size=new Size(1140,640); MinimumSize=new Size(900,480);
            Font=new Font("Segoe UI",9f);AutoScaleMode=AutoScaleMode.Dpi;StartPosition=FormStartPosition.CenterParent;
            var panel=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(10),ColumnCount=1,RowCount=3};
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));panel.RowStyles.Add(new RowStyle(SizeType.Percent,100));panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));Controls.Add(panel);
            panel.Controls.Add(new Label{AutoSize=true,MaximumSize=new Size(1080,0),Text="Cột Km và cọc tiêu được xếp chung theo lý trình tăng. Kiểm tra chúng thuộc cùng một đường trước khi dùng.\nCột Km là mốc KmN+000; cọc H là KmN+H×100. Mốc thiếu số hoặc vị trí vẫn hiện để quản lý, chưa dùng tính.\nMốc thủ công cần xác nhận từng dòng. Xác nhận lưu ngay; sửa loại mốc, vị trí hoặc Km/H phải xác nhận lại.",Padding=new Padding(0,0,0,8)});
            markers.Dock=DockStyle.Fill;markers.ReadOnly=true;markers.AllowUserToAddRows=false;markers.AllowUserToDeleteRows=false;markers.AllowUserToOrderColumns=false;
            markers.RowHeadersVisible=false;markers.SelectionMode=DataGridViewSelectionMode.FullRowSelect;markers.MultiSelect=false;
            markers.AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells;markers.DefaultCellStyle.WrapMode=DataGridViewTriState.True;
            foreach(var c in new[]{"Hồ sơ cọc","Lý trình","X / Y","Từ mốc trước","Kiểm tra đoạn","Loại mốc","Nguồn"})markers.Columns.Add(c,c);
            markers.Columns[0].Width=110;markers.Columns[1].Width=110;markers.Columns[2].Width=160;markers.Columns[3].Width=100;
            markers.Columns[4].AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill;markers.Columns[4].MinimumWidth=200;
            markers.Columns[5].Width=75;markers.Columns[6].Width=85;
            markers.Columns[5].DisplayIndex=1;markers.Columns[6].DisplayIndex=3;
            foreach(DataGridViewColumn column in markers.Columns)column.SortMode=DataGridViewColumnSortMode.NotSortable;
            double px=0,py=0;bool previous=false;int eligible=0;
            var ordered=values.Skip(1).OrderBy(line=>{double n;var f=line.Split('|');return f.Length>1&&double.TryParse(f[1],NumberStyles.Float,CultureInfo.InvariantCulture,out n)&&!double.IsNaN(n)?n:double.MaxValue;}).ThenBy(line=>line.Split('|')[0],StringComparer.Ordinal);
            foreach(var line in ordered)
            {
                var f=line.Split('|');double station,x,y;
                if((f.Length!=5&&f.Length!=7) || !double.TryParse(f[1],NumberStyles.Float,CultureInfo.InvariantCulture,out station) || !double.TryParse(f[2],NumberStyles.Float,CultureInfo.InvariantCulture,out x) || !double.TryParse(f[3],NumberStyles.Float,CultureInfo.InvariantCulture,out y))throw new InvalidOperationException("Dữ liệu xem trước cọc không hợp lệ.");
                bool manualOn=f[4].StartsWith("[MANUAL_ON]"),manualOff=f[4].StartsWith("[MANUAL_OFF]");
                bool incomplete=f[4].StartsWith("[INCOMPLETE]")||double.IsNaN(station)||double.IsNaN(x)||double.IsNaN(y);
                bool usable=!manualOff&&!incomplete;
                string status=f[4].Replace("[MANUAL_ON] ","THỦ CÔNG — đã xác nhận làm mốc. ").Replace("[MANUAL_OFF] ","THỦ CÔNG — ").Replace("[INCOMPLETE] ","CHƯA DÙNG — ");
                int i=markers.Rows.Add(f[0],double.IsNaN(station)?"Chưa có Km/H":Chainage.Format(station),double.IsNaN(x)||double.IsNaN(y)?"Thiếu vị trí":x.ToString("0.000",CultureInfo.InvariantCulture)+" / "+y.ToString("0.000",CultureInfo.InvariantCulture),previous&&usable?Math.Sqrt((x-px)*(x-px)+(y-py)*(y-py)).ToString("0.00")+" m":"—",status,f.Length==7&&f[5]=="COT_KM"?"Cột Km":"Cọc tiêu",manualOn||manualOff?"Thủ công":"Điểm đo");
                markers.Rows[i].Tag=incomplete?"INCOMPLETE":manualOn?"ON":manualOff?"OFF":"RTK";
                if(f[4]!="Nội suy"&&f[4]!="Cọc đầu")markers.Rows[i].DefaultCellStyle.BackColor=Color.LemonChiffon;
                if(manualOff)markers.Rows[i].DefaultCellStyle.BackColor=Color.AliceBlue;
                if(usable){px=x;py=y;previous=true;eligible++;}
            }
            panel.Controls.Add(markers);
            var actions=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,FlowDirection=FlowDirection.RightToLeft};
            var cancel=new Button{Text="Hủy",AutoSize=true,DialogResult=DialogResult.Cancel};
            use.Text="Dùng các mốc này";use.AutoSize=true;use.Enabled=eligible>=2;use.DialogResult=DialogResult.OK;
            var confirm=new Button{Text="Xác nhận dùng mốc",AutoSize=true,Enabled=false};
            Action selection=()=>{var row=markers.CurrentRow;string tag=row==null?"":Convert.ToString(row.Tag);confirm.Enabled=tag=="ON"||tag=="OFF";confirm.Text=tag=="ON"?"Ngừng dùng mốc thủ công":"Xác nhận dùng mốc";};
            markers.CurrentCellChanged+=(s,e)=>selection();selection();
            confirm.Click+=(s,e)=>{var row=markers.CurrentRow;if(row==null)return;bool enabled=Convert.ToString(row.Tag)=="ON";
                string id=Convert.ToString(row.Cells[0].Value);
                if(!enabled&&MessageBox.Show(this,"Dùng "+id+" ("+row.Cells[1].Value+") làm mốc nội suy theo cọc?\nChỉ xác nhận khi đã kiểm tra vị trí và số Km/H. Nguồn vẫn là thủ công; kết quả sẽ được đánh dấu tham khảo.","Xác nhận mốc thủ công",MessageBoxButtons.OKCancel,MessageBoxIcon.Question)!=DialogResult.OK)return;
                ManualId=id;ManualValue=enabled?"0":"1";DialogResult=DialogResult.Retry;Close();};
            actions.Controls.Add(cancel);actions.Controls.Add(use);actions.Controls.Add(confirm);actions.Controls.Add(new Label{AutoSize=true,Padding=new Padding(0,6,0,0),Text=eligible+" / "+markers.Rows.Count+" mốc đủ dữ liệu; xem cảnh báo"});
            panel.Controls.Add(actions);AcceptButton=use;CancelButton=cancel;
        }
    }
}
