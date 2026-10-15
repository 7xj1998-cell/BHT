using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Font = System.Drawing.Font;
using FlowDirection = System.Windows.Forms.FlowDirection;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(BHT.Palette.RouteFormFunctions))]
namespace BHT.Palette
{
    public sealed class RouteOptionsForm : Form
    {
        private readonly TextBox routeId = new TextBox(), maxOffset = new TextBox(), extrapolation = new TextBox();
        private readonly ComboBox routeType = new NoWheelComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label error = new Label { AutoSize = true, ForeColor = Color.Firebrick };
        public string[] Result { get; private set; }
        public RouteOptionsForm(string title, string id, string type, string offset, string extrap)
        {
            Text = title; Font = new Font("Segoe UI", 9f); AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(550, 320); MinimumSize = new Size(510, 350); StartPosition = FormStartPosition.CenterParent;
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 7 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 205)); panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            routeId.Text = id; maxOffset.Text = offset; extrapolation.Text = extrap;
            routeType.Items.AddRange(new object[] { "Tim đường", "Tim rãnh", "Tuyến khác" });
            routeType.SelectedIndex = Array.IndexOf(new[] { "TIM_DUONG", "TIM_RANH", "KHAC" }, type);
            string[] labels = { "ID tuyến", "Loại tim", "Phạm vi lệch tim tối đa (m)", "Ngoại suy ngoài mốc tối đa (m)" };
            Control[] fields = { routeId, routeType, maxOffset, extrapolation };
            for (int i = 0; i < fields.Length; i++) { panel.Controls.Add(new Label { Text = labels[i], AutoSize = true, Padding = new Padding(0, 5, 0, 0) }, 0, i); fields[i].Dock = DockStyle.Top; panel.Controls.Add(fields[i], 1, i); }
            var note = new Label { AutoSize = true, Dock = DockStyle.Fill, Text = "Chọn loại tim theo công trình đang tính. Phạm vi lệch giới hạn đối tượng được chiếu lên tim; ngoại suy 0 = chỉ tính trong phạm vi mốc. Chấp nhận dấu phẩy hoặc dấu chấm thập phân." };
            panel.Controls.Add(note, 0, 4); panel.SetColumnSpan(note, 2);
            panel.Controls.Add(error, 0, 5); panel.SetColumnSpan(error, 2);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            var cancel = new Button { Text = "Hủy", AutoSize = true, DialogResult = DialogResult.Cancel };
            var save = new Button { Text = "Áp dụng", AutoSize = true }; save.Click += (s, e) => AcceptOptions();
            actions.Controls.Add(cancel); actions.Controls.Add(save); panel.Controls.Add(actions, 0, 6); panel.SetColumnSpan(actions, 2);
            Controls.Add(panel); AcceptButton = save; CancelButton = cancel;
        }
        public static bool TryNumber(string text, out double value)
        {
            return double.TryParse((text ?? "").Trim().Replace(',', '.'), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out value) && !double.IsNaN(value) && !double.IsInfinity(value);
        }
        private void AcceptOptions()
        {
            string id = routeId.Text.Trim().ToUpperInvariant(); double offset, extrap;
            if (!Regex.IsMatch(id, @"^[A-Z0-9_.-]+$")) { error.Text = "ID chỉ gồm A–Z, 0–9, gạch dưới, gạch ngang và dấu chấm."; routeId.Focus(); return; }
            if (routeType.SelectedIndex < 0) { error.Text = "Chọn loại tim."; routeType.Focus(); return; }
            if (!TryNumber(maxOffset.Text, out offset) || offset <= 0) { error.Text = "Phạm vi lệch tim phải là số lớn hơn 0."; maxOffset.Focus(); return; }
            if (!TryNumber(extrapolation.Text, out extrap) || extrap < 0) { error.Text = "Ngoại suy phải là số từ 0 trở lên."; extrapolation.Focus(); return; }
            Result = new[] { id, new[] { "TIM_DUONG", "TIM_RANH", "KHAC" }[routeType.SelectedIndex], offset.ToString("R", CultureInfo.InvariantCulture), extrap.ToString("R", CultureInfo.InvariantCulture) };
            DialogResult = DialogResult.OK; Close();
        }
    }

    public sealed class RouteMarksForm : Form
    {
        private sealed class Mark
        {
            public int Index; public double Distance, Back, Ahead; public string Source, Note; public bool Valid;
        }
        private readonly DataGridView grid = new DataGridView();
        private readonly Button remove = new Button(), view = new Button();
        public string[] Result { get; private set; }
        public RouteMarksForm(string route, IEnumerable<string> records)
        {
            Text = "Mốc trên tim — " + route; Font = new Font("Segoe UI", 9f); AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(1120, 620); MinimumSize = new Size(760, 420); StartPosition = FormStartPosition.CenterParent;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(10) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label { AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 0, 0, 8), Text = "Chọn dòng để xem vị trí; Ctrl/Shift để chọn nhiều mốc cần xóa. Mốc được xếp theo khoảng cách dọc tim.\nXóa chỉ bỏ mốc BHT đã khai báo, giữ cọc TDT gốc. Mốc tự động từ hồ sơ cọc được quản lý tại hồ sơ. Sau khi xóa, tính lại lý trình." });
            grid.Dock = DockStyle.Fill; grid.ReadOnly = true; grid.AllowUserToAddRows = grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = true; grid.BackgroundColor = Color.White;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells; grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            foreach (string label in new[] { "STT", "Dọc tim (m)", "Lý trình sau", "Lý trình trước", "Hệ số đoạn trước", "Nguồn", "Ghi chú / kiểm tra" }) grid.Columns.Add(label, label);
            int[] widths = { 50, 95, 120, 120, 110, 200, 240 };
            for (int i = 0; i < widths.Length; i++) { grid.Columns[i].Width = widths[i]; grid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable; }
            grid.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; grid.Columns[6].MinimumWidth = 180;
            var marks = new List<Mark>();
            foreach (string record in records)
            {
                string[] f = record.Split('|'); int index;
                if (f.Length < 2 || !int.TryParse(f[0], out index)) throw new InvalidOperationException("Dòng mốc thiếu mã lựa chọn.");
                var m = new Mark { Index = index, Source = f.Length > 4 ? f[4] : "", Note = f.Length > 5 ? string.Join("|", f.Skip(5)) : "" };
                m.Valid = f.Length >= 6 && RouteOptionsForm.TryNumber(f[1], out m.Distance) && RouteOptionsForm.TryNumber(f[2], out m.Back) && RouteOptionsForm.TryNumber(f[3], out m.Ahead) && m.Distance >= 0;
                marks.Add(m);
            }
            Mark previous = null; int ordinal = 0;
            foreach (Mark m in marks.OrderBy(m => m.Valid ? m.Distance : double.MaxValue).ThenBy(m => m.Index))
            {
                string status = m.Note, ratio = ""; bool warn = !m.Valid;
                if (!m.Valid) status = "Dữ liệu mốc không hợp lệ. " + status;
                else
                {
                    if (m.Back != m.Ahead) status = "Điểm gãy Km. " + status;
                    if (previous != null)
                    {
                        double dd = m.Distance - previous.Distance, ds = m.Back - previous.Ahead;
                        if (dd <= 1e-9) { status = "Trùng vị trí dọc tim. " + status; warn = true; }
                        else { double r = ds / dd; ratio = r.ToString("0.00000", CultureInfo.InvariantCulture); if (r <= 0 || Math.Abs(r - 1) > .03) { status = "CẦN KIỂM TRA hệ số đoạn. " + status; warn = true; } }
                    }
                    previous = m;
                }
                int row = grid.Rows.Add(++ordinal, m.Valid ? m.Distance.ToString("0.000", CultureInfo.InvariantCulture) : "—", m.Valid ? Chainage.Format(m.Back) : "—", m.Valid ? Chainage.Format(m.Ahead) : "—", ratio, (m.Index < 0 ? "Tự động: " : "") + m.Source, status);
                grid.Rows[row].Tag = m; if (warn) grid.Rows[row].DefaultCellStyle.BackColor = Color.LemonChiffon;
            }
            layout.Controls.Add(grid);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            view.Text = "Xem vị trí trên CAD"; view.AutoSize = true; view.Click += (s, e) => ViewSelected();
            remove.Text = "Xóa mốc đã chọn…"; remove.AutoSize = true; remove.Click += (s, e) => {
                var selected = Selected().ToArray(); if (selected.Length == 0 || selected.Any(m => m.Index < 0)) return;
                if (MessageBox.Show(this, "Xóa " + selected.Length + " mốc BHT của tim " + route + "?\nHồ sơ liên quan cần tính lại. Có thể dùng Undo để khôi phục.", "Xóa mốc đã chọn", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
                Result = new[] { "DELETE" }.Concat(selected.Select(m => m.Index.ToString(CultureInfo.InvariantCulture))).ToArray(); DialogResult = DialogResult.OK; Close();
            };
            var close = new Button { Text = "Đóng", AutoSize = true, DialogResult = DialogResult.Cancel };
            actions.Controls.Add(view); actions.Controls.Add(remove); actions.Controls.Add(close); actions.Controls.Add(new Label { Text = marks.Count + " mốc", AutoSize = true, Padding = new Padding(8, 6, 0, 0) });
            layout.Controls.Add(actions); Controls.Add(layout); CancelButton = close;
            grid.SelectionChanged += (s, e) => SelectionChanged(); grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) ViewSelected(); };
            grid.ClearSelection(); SelectionChanged();
        }
        private IEnumerable<Mark> Selected() { return grid.SelectedRows.Cast<DataGridViewRow>().Select(r => (Mark)r.Tag); }
        private void SelectionChanged() { var rows = Selected().ToArray(); remove.Enabled = rows.Length > 0 && rows.All(m => m.Index >= 0); view.Enabled = rows.Length == 1 && rows[0].Valid; }
        private void ViewSelected()
        {
            var rows = Selected().ToArray(); if (rows.Length != 1 || !rows[0].Valid) return;
            Result = new[] { "VIEW", rows[0].Distance.ToString("R", CultureInfo.InvariantCulture) }; DialogResult = DialogResult.OK; Close();
        }
    }

    public sealed class RouteFormFunctions
    {
        private static ResultBuffer Reply(IEnumerable<string> values) { var r = new ResultBuffer(); foreach (string s in values) r.Add(new TypedValue((int)LispDataType.Text, s)); return r; }
        [LispFunction("BHTROUTEOPTIONS")]
        public ResultBuffer Options(ResultBuffer args)
        {
            if (PaletteCommands.IsCoreConsole()) return Reply(new[] { "SKIP" });
            try { var a = args.AsArray().Select(v => Convert.ToString(v.Value)).ToArray();
                if (a.Length != 5) return Reply(new[] { "LOI", "Thiếu thông số tuyến." });
                using (var form = new RouteOptionsForm(a[0], a[1], a[2], a[3], a[4])) return Reply(AcApp.ShowModalDialog(form) == DialogResult.OK ? new[] { "OK" }.Concat(form.Result) : new[] { "CANCEL" });
            } catch (System.Exception ex) { return Reply(new[] { "LOI", ex.Message }); }
        }
        [LispFunction("BHTROUTEMARKS")]
        public ResultBuffer Marks(ResultBuffer args)
        {
            if (PaletteCommands.IsCoreConsole()) return Reply(new[] { "SKIP" });
            try { var a = args.AsArray().Select(v => Convert.ToString(v.Value)).ToArray();
                if (a.Length == 0) return Reply(new[] { "LOI", "Chưa chọn tim." });
                using (var form = new RouteMarksForm(a[0], a.Skip(1))) return Reply(AcApp.ShowModalDialog(form) == DialogResult.OK ? form.Result : new[] { "CANCEL" });
            } catch (System.Exception ex) { return Reply(new[] { "LOI", ex.Message }); }
        }
    }
}
