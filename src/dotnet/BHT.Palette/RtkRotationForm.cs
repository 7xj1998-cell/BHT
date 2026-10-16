using System.Drawing;
using System.Windows.Forms;

namespace BHT.Palette
{
    public sealed class RtkRotationForm : Form
    {
        public string Mode { get; private set; }
        public RtkRotationForm()
        {
            Text = "Gom / xoay nhãn RTK và Timemark";
            Font = new Font("Segoe UI", 9f); AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(410, 180); FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false; StartPosition = FormStartPosition.CenterParent;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "Gom các dòng gần điểm rồi xoay cả cụm RTK và Timemark. Giữ tọa độ gốc.", AutoSize = true, MaximumSize = new Size(380, 0) });
            var ucs = new Button { Text = "Theo UCS hiện tại", Dock = DockStyle.Fill, Height = 34 };
            var picked = new Button { Text = "Chọn hướng A → B trên CAD…", Dock = DockStyle.Fill, Height = 34 };
            ucs.Click += (s,e) => { Mode = "UCS"; DialogResult = DialogResult.OK; Close(); };
            picked.Click += (s,e) => { Mode = "PICK"; DialogResult = DialogResult.OK; Close(); };
            var cancel = new Button { Text = "Hủy", AutoSize = true, Anchor = AnchorStyles.Right, DialogResult = DialogResult.Cancel };
            layout.Controls.Add(ucs); layout.Controls.Add(picked); layout.Controls.Add(cancel);
            Controls.Add(layout); CancelButton = cancel;
        }
    }
}
