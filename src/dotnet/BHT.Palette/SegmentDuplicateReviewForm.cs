using System;
using System.Drawing;
using System.Windows.Forms;
using BHT.Bridge;

namespace BHT.Palette
{
    internal sealed class SegmentDuplicateReviewForm:Form
    {
        public SegmentDuplicateReviewForm(SegmentDuplicatePreview preview)
        {
            Text="BHT — Đối chiếu đoạn trùng";Size=new Size(940,650);MinimumSize=new Size(680,480);
            AutoScaleMode=AutoScaleMode.Dpi;StartPosition=FormStartPosition.CenterParent;
            var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Padding=new Padding(12)};
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label{Dock=DockStyle.Fill,AutoSize=true,Text="Có thể gom: "+preview.Removable+" dòng. Cần đối chiếu riêng: "+preview.Blocked+" cặp.\r\nChỉ bỏ dòng không được tham chiếu. Giữ nguyên mã hồ sơ đã khóa, RTK, lý trình, tim và ký hiệu.\r\nBản ghi bị bỏ được giữ đầy đủ trong nhật ký DWG; có thể Undo trước khi lưu.",Padding=new Padding(0,0,0,10)},0,0);
            layout.Controls.Add(new TextBox{Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,WordWrap=true,Text=string.Join("\r\n\r\n————————————————————————\r\n\r\n",preview.Lines)},0,1);
            var actions=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,FlowDirection=FlowDirection.RightToLeft};
            var cancel=new Button{Text="Hủy",DialogResult=DialogResult.Cancel,AutoSize=true};
            var apply=new Button{Text="Xác nhận gom "+preview.Removable+" dòng",DialogResult=DialogResult.OK,AutoSize=true,Enabled=preview.Removable>0&&preview.Blocked==0};
            actions.Controls.Add(cancel);actions.Controls.Add(apply);layout.Controls.Add(actions,0,2);
            Controls.Add(layout);CancelButton=cancel;PaletteTheme.ApplyTableDialog(this);
        }
    }
}
