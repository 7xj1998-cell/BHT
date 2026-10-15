using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
using BHT.Core;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(BHT.Palette.SegmentPickerFunctions))]
namespace BHT.Palette
{
    public sealed class SegmentPickerForm : Form
    {
        private readonly DataGridView grid = new DataGridView();
        private readonly Button select = new Button { Text = "Gán đoạn đã chọn", AutoSize = true, Enabled = false };
        public string[] Result { get; private set; }
        public SegmentPickerForm(string id, BhtRecord record, IEnumerable<BhtRecord> segments)
        {
            Text = "Chọn đoạn / gói cho hồ sơ " + id;
            Font = new System.Drawing.Font("Segoe UI", 9f); AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(1180, 600); MinimumSize = new Size(840, 420); StartPosition = FormStartPosition.CenterParent;
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 1, RowCount = 3 };
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(new Label { AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 0, 0, 10), Text =
                "Hồ sơ: " + id + " | " + record.Get("ma_hieu") + " | " + record.Get("ly_trinh_km") + " | Tim: " + record.Get("route_id") +
                "\nPhía đường đã nhập: " + record.Get("phia_duong") + " | Phía so với tim: " + record.Get("phia_tuyen") + " (" + record.Get("loai_tuyen") + ")" +
                "\nĐang gán: " + record.Get("doan") + " / " + record.Get("goi") +
                ". Chọn một dòng để gán thủ công. Bảng hiển thị mọi đoạn, kể cả ngoài phạm vi; đối chiếu lý trình và phía đường trước khi chọn." });
            grid.Dock = DockStyle.Fill; grid.ReadOnly = true; grid.AllowUserToAddRows = grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false; grid.MultiSelect = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.BackgroundColor = Color.White; grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells; grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            string[] labels = { "Mã đoạn", "Mã gói", "Tên gói / công trình", "Từ lý trình", "Đến lý trình", "Phía đường", "Tim của đoạn", "Đối chiếu hồ sơ" };
            int[] widths = { 95, 85, 220, 115, 115, 90, 120, 190 };
            for (int i = 0; i < labels.Length; i++) { grid.Columns.Add(labels[i], labels[i]); grid.Columns[i].Width = widths[i]; }
            foreach (var seg in segments)
            {
                double a, b, station; bool startValid = RouteOptionsForm.TryNumber(seg.Get("km_start_m"), out a);
                bool range = RouteOptionsForm.TryNumber(seg.Get("km_end_m"), out b) && startValid;
                string note = "Cần tính lý trình", route = seg.Get("route_id"), side = record.Get("phia_duong");
                if (side != "TRAI" && side != "PHAI") side = record.Get("loai_tuyen") == "TIM_DUONG" ? record.Get("phia_tuyen") : "";
                if (route != "" && route != record.Get("route_id")) note = "Dùng tim khác; tính khi gán";
                else if (range && RouteOptionsForm.TryNumber(record.Get("ly_trinh_m"), out station))
                    note = station < a - 1e-6 || station > b + 1e-6 ? "Ngoài khoảng lý trình" :
                        (side == "TRAI" || side == "PHAI") && seg.Get("side") != "HAI_BEN" && seg.Get("side") != side ? "Khác phía đường" :
                        side == "" && seg.Get("side") != "HAI_BEN" ? "Trong khoảng; chưa rõ phía đường" : "Trong khoảng / đúng phía";
                int row = grid.Rows.Add(seg.Get("segment_id"), seg.Get("package_id"), seg.Get("package_name"), range ? Chainage.Format(a) : "?", range ? Chainage.Format(b) : "?",
                    seg.Get("side") == "TRAI" ? "Trái" : seg.Get("side") == "PHAI" ? "Phải" : "Hai bên", route == "" ? "Theo tim hồ sơ" : route, note);
                grid.Rows[row].Tag = seg.Get("segment_id");
                if (seg.Get("segment_id") == record.Get("doan")) grid.Rows[row].DefaultCellStyle.BackColor = Color.LightCyan;
            }
            panel.Controls.Add(grid);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            select.Click += (s, e) => Choose();
            var clear = new Button { Text = "Bỏ gán đoạn", AutoSize = true }; clear.Click += (s, e) => { Result = new[] { "CLEAR" }; DialogResult = DialogResult.OK; Close(); };
            var cancel = new Button { Text = "Hủy", AutoSize = true, DialogResult = DialogResult.Cancel };
            actions.Controls.AddRange(new Control[] { select, clear, cancel }); panel.Controls.Add(actions); Controls.Add(panel);
            AcceptButton = select; CancelButton = cancel;
            grid.SelectionChanged += (s, e) => select.Enabled = grid.SelectedRows.Count == 1;
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Choose(); };
            grid.ClearSelection();
            Shown += (s, e) => grid.ClearSelection();
        }
        private void Choose() { if (grid.SelectedRows.Count != 1) return; Result = new[] { "SELECT", (string)grid.SelectedRows[0].Tag }; DialogResult = DialogResult.OK; Close(); }
    }
    public sealed class SegmentPickerFunctions
    {
        private static ResultBuffer Reply(params string[] values) { var r = new ResultBuffer(); foreach (string s in values) r.Add(new TypedValue((int)LispDataType.Text, s)); return r; }
        [LispFunction("BHTSEGMENTPICK")]
        public ResultBuffer Pick(ResultBuffer args)
        {
            if (PaletteCommands.IsCoreConsole()) return Reply("CANCEL");
            try
            {
                var doc = AcApp.DocumentManager.MdiActiveDocument; string id = Convert.ToString(args.AsArray()[0].Value); BhtRecord rec; List<BhtRecord> rows;
                using (var tr = doc.Database.TransactionManager.StartOpenCloseTransaction()) rec = BhtStore.Read(tr, doc.Database, "OBJ", id);
                if (rec == null) return Reply("LOI", "Hồ sơ không còn tồn tại.");
                rows = SegmentStore.Read(doc.Database);
                if (rows.Count == 0) return Reply("LOI", "Chưa có bảng đoạn. Nạp Excel / bảng BHT trước.");
                string before = string.Join("\n", rows.Select(r => r.ToCanonical()));
                using (var form = new SegmentPickerForm(id, rec, rows))
                {
                    if (AcApp.ShowModalDialog(form) != DialogResult.OK) return Reply("CANCEL");
                    if (AcApp.DocumentManager.MdiActiveDocument != doc || before != string.Join("\n", SegmentStore.Read(doc.Database).Select(r => r.ToCanonical())))
                        return Reply("LOI", "Bản vẽ hoặc bảng đoạn đã đổi. Mở lại bảng chọn.");
                    return Reply(form.Result);
                }
            }
            catch (System.Exception ex) { return Reply("LOI", ex.Message); }
        }
    }
}
