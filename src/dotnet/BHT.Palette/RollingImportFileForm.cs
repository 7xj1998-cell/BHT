using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BHT.Bridge;
using BHT.Core;

namespace BHT.Palette
{
    internal sealed class RollingImportFileForm : Form
    {
        readonly TextBox path = new TextBox();
        readonly Label status = new Label();
        readonly Button accept = new Button();
        public string SelectedPath { get; private set; }

        public RollingImportFileForm()
        {
            Text = "BHT " + BhtVersion.Version + " — Nạp dữ liệu kế thừa";
            Size = new Size(780, 320); MinimumSize = new Size(650, 320);
            StartPosition = FormStartPosition.CenterParent; AutoScaleMode = AutoScaleMode.Dpi;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label { AutoSize = true, Dock = DockStyle.Fill,
                Text = "Chọn tệp .bht đã xuất từ đoạn trước, rồi bấm Kiểm tra và nạp.\r\nBHT sẽ đối chiếu trước khi ghi; hồ sơ trùng mã khác nội dung sẽ bị chặn.", Margin = new Padding(3, 3, 3, 14) }, 0, 0);
            var row = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            path.Dock = DockStyle.Fill; path.Margin = new Padding(3, 6, 8, 3);
            var browse = new Button { Text = "Chọn tệp…", AutoSize = true, MinimumSize = new Size(100, 36) };
            browse.Click += (s, e) => Browse(); row.Controls.Add(path, 0, 0); row.Controls.Add(browse, 1, 0);
            layout.Controls.Add(row, 0, 1);
            status.AutoSize = true; status.Dock = DockStyle.Fill; status.Padding = new Padding(3, 12, 3, 6);
            status.Text = "Có thể dán đường dẫn tệp trực tiếp vào ô trên. Chọn tệp chưa làm thay đổi bản vẽ.";
            layout.Controls.Add(status, 0, 2);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            var cancel = new Button { Text = "Hủy", AutoSize = true, MinimumSize = new Size(100, 38), DialogResult = DialogResult.Cancel };
            accept.Text = "Kiểm tra và nạp"; accept.AutoSize = true; accept.MinimumSize = new Size(155, 38); accept.Enabled = false;
            accept.Click += (s, e) => AcceptPath();
            buttons.Controls.Add(cancel); buttons.Controls.Add(accept); layout.Controls.Add(buttons, 0, 3);
            Controls.Add(layout); AcceptButton = accept; CancelButton = cancel;
            path.TextChanged += (s, e) => { accept.Enabled = !string.IsNullOrWhiteSpace(path.Text); };
            PaletteTheme.ApplyTableDialog(this);
        }

        // The older Windows picker omits the shell preview pane. It only returns a
        // filename: no DWG/package reads occur while either file dialog is open.
        void Browse()
        {
            try
            {
                RollingImportLog.Note("Mở cửa sổ chọn tệp Windows (không xem trước)");
                using (var pick = new OpenFileDialog { Title = "Chọn hồ sơ kế thừa BHT", Filter = "Hồ sơ BHT (*.bht)|*.bht",
                    AutoUpgradeEnabled = false, CheckFileExists = true, Multiselect = false, RestoreDirectory = true })
                {
                    if (pick.ShowDialog(this) != DialogResult.OK) { RollingImportLog.Note("Đóng chọn tệp Windows — hủy lựa chọn"); return; }
                    path.Text = pick.FileName;
                    RollingImportLog.Note("Đóng chọn tệp Windows. Tệp đã chọn: " + path.Text);
                    status.Text = "Đã chọn tệp. Bấm Kiểm tra và nạp để bắt đầu đối chiếu. Bản vẽ chưa thay đổi.";
                }
            }
            catch (Exception ex) { RollingImportLog.Note("LỖI CỬA SỔ CHỌN TỆP — " + ex); status.Text = "Chưa chọn được tệp: " + ex.Message + " Có thể dán đường dẫn trực tiếp."; }
        }

        void AcceptPath()
        {
            string why, selected;
            if (!TryPath(path.Text, out selected, out why)) { status.Text = why; return; }
            SelectedPath = selected; RollingImportLog.Note("Đã duyệt đường dẫn; đóng bảng chọn trước khi đọc BHT: " + selected);
            DialogResult = DialogResult.OK; Close();
        }

        internal static bool TryPath(string input, out string selected, out string error)
        {
            selected = null; error = "";
            try
            {
                string value = (input ?? "").Trim().Trim('"');
                if (value.Length == 0 || !System.IO.Path.IsPathRooted(value)) { error = "Dán đường dẫn đầy đủ tới tệp .bht hoặc bấm Chọn tệp."; return false; }
                value = System.IO.Path.GetFullPath(value);
                if (!string.Equals(System.IO.Path.GetExtension(value), ".bht", StringComparison.OrdinalIgnoreCase)) { error = "Chọn tệp hồ sơ có đuôi .bht."; return false; }
                if (!File.Exists(value)) { error = "Không tìm thấy tệp. Kiểm tra lại đường dẫn."; return false; }
                selected = value; return true;
            }
            catch (Exception ex) { error = "Đường dẫn không hợp lệ: " + ex.Message; return false; }
        }
    }
}
