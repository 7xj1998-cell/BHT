using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BHT.Core;

namespace BHT.Palette
{
    internal static class CheckReportText
    {
        public static string Format(IList<string> values)
        {
            int errors, warnings;
            if (values.Count < 2 || !values[0].StartsWith("loi=") || !values[1].StartsWith("canh_bao=") ||
                !int.TryParse(values[0].Substring(4), out errors) || !int.TryParse(values[1].Substring(9), out warnings))
                return string.Join("\r\n", values.ToArray());
            var details = values.Skip(2).ToList();
            var problems = details.Where(IsProblem).ToList();
            var text = new StringBuilder("KẾT QUẢ KIỂM TRA BẢN VẼ\r\n");
            text.AppendFormat("Lỗi cần xử lý: {0}\r\nMục cần kiểm tra: {1}\r\n", errors, warnings);
            text.Append("Báo cáo chỉ kiểm tra dữ liệu; chưa sửa hoặc xóa đối tượng trong bản vẽ.\r\n");
            text.Append(errors == 0 && warnings == 0 ? "Chưa phát hiện lỗi trong các mục đã kiểm tra.\r\n" : "Đọc các mục dưới đây, xử lý rồi bấm Kiểm tra bản vẽ để xem lại.\r\n");
            text.Append("\r\nVIỆC CẦN LÀM\r\n");
            if (problems.Count == 0) text.Append("Không có mục cần xử lý.\r\n");
            for (int i = 0; i < problems.Count; i++)
                text.Append(i + 1).Append(". ").Append(problems[i].Replace("CẢNH BÁO: ", "CẦN KIỂM TRA: ")).Append("\r\n\r\n");
            text.Append("\r\nSỐ LIỆU ĐÃ KIỂM TRA\r\n");
            foreach (var line in details.Where(s => !IsProblem(s)))
                text.Append(line.Replace(" | ", "\r\n  ")).Append("\r\n");
            return text.ToString();
        }

        private static bool IsProblem(string line)
        {
            return line.StartsWith("LỖI: ") || line.StartsWith("CẢNH BÁO: ");
        }
    }

    internal sealed class ReportDialog : Form
    {
        private readonly TextBox body;
        public ReportDialog(string title, string content, Action<string> copied, Action<string> failed)
        {
            Text = "BHT " + BhtVersion.Version + " — " + (title ?? "Báo cáo");
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(900, 680); MinimumSize = new Size(620, 420);
            ShowIcon = false; AutoScaleMode = AutoScaleMode.Dpi;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12) };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Multiline = true,
                WordWrap = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 10f),
                Text = content ?? "", BorderStyle = BorderStyle.None };
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 0) };
            var close = new Button { Text = "Đóng báo cáo", AutoSize = true, DialogResult = DialogResult.Cancel };
            close.Click += (s, e) => Close();
            var copy = new Button { Text = "Sao chép báo cáo", AutoSize = true };
            copy.Click += (s, e) => {
                try { if (body.TextLength > 0) Clipboard.SetText(body.Text); if (copied != null) copied("Đã sao chép toàn bộ báo cáo."); }
                catch (Exception ex) { if (failed != null) failed("Chưa sao chép được báo cáo. Thử lại. Chi tiết: " + ex.Message); }
            };
            buttons.Controls.Add(close); buttons.Controls.Add(copy);
            layout.Controls.Add(body, 0, 0); layout.Controls.Add(buttons, 0, 1); Controls.Add(layout);
            CancelButton = close;
            PaletteTheme.ApplyDialog(this);
            Shown += (s, e) => { body.Select(0, 0); body.ScrollToCaret(); };
        }
    }
}
