using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
using BHT.Core;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(BHT.Palette.SegmentTableCommands))]
namespace BHT.Palette
{
    public class SegmentTableCommands
    {
        [CommandMethod("BHTNAPDOAN")]
        public static void ImportCommand() { Open(AcApp.DocumentManager.MdiActiveDocument, true); }
        [CommandMethod("BHTTIMDOAN")]
        public static void EditCommand() { Open(AcApp.DocumentManager.MdiActiveDocument, false); }

        public static void Open(Document doc, bool import)
        {
            if (doc == null || PaletteCommands.IsCoreConsole()) return;
            try
            {
                List<BhtRecord> rows; string path = "", note = "";
                if (import)
                {
                    using (var pick = new OpenFileDialog { Title = "Nạp đoạn / gói thầu", Filter = "BHT / Excel (*.bhtseg;*.xlsx)|*.bhtseg;*.xlsx|Bảng BHT (*.bhtseg)|*.bhtseg|Excel (*.xlsx)|*.xlsx|TSV cũ — chỉ chuyển đổi (*.tsv)|*.tsv" })
                    {
                        if (pick.ShowDialog() != DialogResult.OK) return;
                        path = pick.FileName;
                    }
                    string ext = Path.GetExtension(path).ToLowerInvariant();
                    if (ext == ".bhtseg") rows = SegmentTable.Read(path);
                    else if (ext == ".xlsx")
                    {
                        var tables = SegmentTable.ReadExcel(path);
                        var table = tables[0];
                        if (tables.Count > 1)
                        {
                            using (var choice = new Form { Text = "Chọn trang tính phân đoạn", Width = 500, Height = 170, StartPosition = FormStartPosition.CenterParent })
                            {
                                var combo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
                                combo.Items.AddRange(tables.Select(t => (object)t.Sheet).ToArray()); combo.SelectedIndex = 0;
                                var ok = new Button { Text = "Xem dữ liệu", Dock = DockStyle.Bottom, DialogResult = DialogResult.OK };
                                choice.Controls.Add(combo); choice.Controls.Add(ok); choice.AcceptButton = ok;
                                if (choice.ShowDialog() != DialogResult.OK) return;
                                table = tables[combo.SelectedIndex];
                            }
                        }
                        rows = table.Rows; note = "Trang tính: " + table.Sheet + ". " + string.Join("\r\n", table.Notes.ToArray());
                    }
                    else if (ext == ".tsv") { rows = SegmentTable.ReadLegacy(path); note = "Chuyển đổi bảng TSV cũ sang .bhtseg; không tiếp tục lưu TSV."; }
                    else throw new InvalidDataException("Chọn .xlsx hoặc .bhtseg.");
                }
                else rows = SegmentStore.Read(doc.Database);
                if (rows.Count == 0) { MessageBox.Show("Chưa có đoạn. Chọn Nạp Excel / bảng BHT trước.", "BHT"); return; }
                Dictionary<string, BhtRecord> routes;
                using (var tr = doc.Database.TransactionManager.StartOpenCloseTransaction()) routes = BhtStore.All(tr, doc.Database, "ROUTE").ToDictionary(p => p.Key, p => p.Value);
                using (var form = new SegmentTableForm(rows, routes, path, note, import))
                {
                    if (form.ShowDialog() != DialogResult.OK) return;
                    if (AcApp.DocumentManager.MdiActiveDocument != doc) throw new InvalidOperationException("Bản vẽ đã thay đổi; chưa áp dụng bảng.");
                    string filePath = form.NativePath;
                    if (filePath != "") SegmentTable.Write(filePath, form.Result);
                    // This method is entered either from a normal command or RunInCommandContext.
                    using (doc.LockDocument()) SegmentStore.Apply(doc.Database, form.Result, filePath != "" ? filePath : path, true);
                    doc.Editor.WriteMessage("\nBHT: đã áp dụng " + form.Result.Count + " đoạn" + (filePath == "" ? "." : " và lưu " + filePath + ".") + " Chạy BHTPHANDOAN để tính và gán lại hồ sơ.");
                    MessageBox.Show("Đã áp dụng " + form.Result.Count + " đoạn.\r\n" + (filePath == "" ? "" : "Đã lưu: " + filePath + "\r\n") + "Chọn Gán đoạn / gói theo tim của đoạn để tính lại.\r\nBảng và liên kết tim được lưu trong DWG khi anh lưu bản vẽ.", "BHT");
                }
            }
            catch (System.Exception ex) { MessageBox.Show(ex.Message + "\r\nBảng đoạn trong bản vẽ chưa được thay đổi nếu thao tác áp dụng thất bại.", "BHT — chưa hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }

    public sealed class SegmentTableForm : Form
    {
        readonly DataGridView grid = new DataGridView();
        readonly Dictionary<string, BhtRecord> routes;
        readonly string source;
        readonly bool converting;
        readonly string[] fields = { "segment_id", "package_id", "km_start_m", "km_end_m", "side", "route_id", "package_name", "note" };
        public List<BhtRecord> Result;
        public string NativePath = "";

        public SegmentTableForm(List<BhtRecord> rows, Dictionary<string, BhtRecord> routes, string path, string notes, bool import)
        {
            this.routes = routes; source = path;
            converting = import && !Path.GetExtension(path).Equals(".bhtseg", StringComparison.OrdinalIgnoreCase);
            Text = "BHT — Đoạn / gói thầu và tim tham chiếu"; Width = 1180; Height = 650; MinimumSize = new Size(760, 460);
            Font = new Font("Segoe UI", 9f); StartPosition = FormStartPosition.CenterParent;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, Padding = new Padding(10) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label { AutoSize = true, MaximumSize = new Size(1080, 0), Text = "Mỗi đoạn dùng một tim đã khai báo: tim đường, tim rãnh hoặc tuyến khác. Để trống = dùng lý trình hiện có. Phía trái/phải theo chiều tăng lý trình của tim. Các đoạn cùng mốc biên được báo để chọn thủ công." }, 0, 0);
            grid.Dock = DockStyle.Fill; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false; grid.RowHeadersVisible = false; grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = true; grid.BackgroundColor = Color.White;
            string[] labels = { "Mã đoạn", "Mã gói", "Lý trình đầu (m / Km)", "Lý trình cuối (m / Km)", "Phía", "Tim của đoạn", "Tên gói", "Ghi chú" };
            for (int i = 0; i < fields.Length; i++)
            {
                DataGridViewColumn col;
                if (fields[i] == "side")
                {
                    var c = new DataGridViewComboBoxColumn(); c.Items.AddRange("HAI_BEN", "TRAI", "PHAI"); col = c;
                }
                else if (fields[i] == "route_id")
                {
                    var c = new DataGridViewComboBoxColumn { DisplayMember = "Label", ValueMember = "Id", DataSource = RouteChoices(rows, routes) }; col = c;
                }
                else col = new DataGridViewTextBoxColumn();
                col.Name = fields[i]; col.HeaderText = labels[i]; col.Width = i == 5 ? 230 : (i >= 6 ? 220 : 130); col.SortMode = DataGridViewColumnSortMode.NotSortable; grid.Columns.Add(col);
            }
            foreach (var rec in rows) { int i = grid.Rows.Add(fields.Select(f => (object)rec.Get(f)).ToArray()); grid.Rows[i].Tag = rec.Clone(); }
            grid.DataError += (s, e) => { e.ThrowException = false; grid.Rows[e.RowIndex].ErrorText = "Giá trị không hợp lệ. Chọn lại phía hoặc tim."; };
            layout.Controls.Add(grid, 0, 1);
            var info = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Text = rows.Count + " đoạn. " + notes + "\r\nTim chưa có trong bản vẽ sẽ hiện [THIẾU TIM]; cần khai báo hoặc chọn lại. Phân tự động chọn tim phù hợp gần nhất; hai tim lệch nhau không quá 1 cm cần chọn đoạn thủ công. Khi áp dụng, gán tự động cũ được xóa để phân lại; gán tay giữ nếu phạm vi và tim không đổi." };
            layout.Controls.Add(info, 0, 2);
            var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
            var applyRoute = new ComboBox { Width = 270, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Label", ValueMember = "Id", DataSource = RouteChoices(new List<BhtRecord>(), routes) };
            actions.Controls.Add(applyRoute);
            var bind = new Button { Text = "Gán tim cho các dòng chọn", AutoSize = true };
            bind.Click += (s, e) => { grid.EndEdit(); foreach (DataGridViewRow row in grid.SelectedRows) row.Cells["route_id"].Value = Convert.ToString(applyRoute.SelectedValue); };
            actions.Controls.Add(bind);
            var save = new Button { Text = converting ? "Chuyển sang .bhtseg và nạp…" : "Lưu .bhtseg và áp dụng…", AutoSize = true };
            save.Click += (s, e) => Complete(true); actions.Controls.Add(save);
            if (!converting) { var apply = new Button { Text = "Áp dụng vào bản vẽ", AutoSize = true }; apply.Click += (s, e) => Complete(false); actions.Controls.Add(apply); }
            var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, AutoSize = true }; actions.Controls.Add(cancel); CancelButton = cancel;
            layout.Controls.Add(actions, 0, 3); Controls.Add(layout);
        }
        public sealed class RouteChoice { public string Id { get; set; } public string Label { get; set; } }
        static List<RouteChoice> RouteChoices(List<BhtRecord> rows, Dictionary<string, BhtRecord> routes)
        {
            var result = new List<RouteChoice> { new RouteChoice { Id = "", Label = "(Dùng lý trình hiện có)" } };
            result.AddRange(routes.Select(p => new RouteChoice { Id = p.Key, Label = p.Key + " — " + SymbolDistanceForm.RouteTypeLabel(p.Value.Get("loai")) }));
            foreach (string id in rows.Select(r => r.Get("route_id")).Where(id => id != "" && !routes.ContainsKey(id)).Distinct()) result.Add(new RouteChoice { Id = id, Label = "[THIẾU TIM] " + id });
            return result;
        }
        void Complete(bool save)
        {
            try
            {
                if (!grid.EndEdit()) throw new InvalidDataException("Hoàn tất ô đang sửa trước khi áp dụng.");
                var rows = new List<BhtRecord>();
                foreach (DataGridViewRow row in grid.Rows)
                {
                    var r = ((BhtRecord)row.Tag).Clone(); foreach (string field in fields) r.Set(field, Convert.ToString(row.Cells[field].Value)); rows.Add(r);
                }
                Result = SegmentTable.Validate(rows);
                foreach (var r in Result) if (r.Get("route_id") != "" && !routes.ContainsKey(r.Get("route_id"))) throw new InvalidDataException("Đoạn " + r.Get("segment_id") + ": chưa có tim " + r.Get("route_id") + ". Chọn lại tim trong bảng.");
                NativePath = "";
                if (save)
                {
                    using (var dialog = new SaveFileDialog { Title = "Lưu bảng riêng của BHT", Filter = "Bảng đoạn BHT (*.bhtseg)|*.bhtseg", DefaultExt = "bhtseg", AddExtension = true, FileName = source == "" ? "BHT_GOI_THAU.bhtseg" : Path.GetFileNameWithoutExtension(source) + ".bhtseg", InitialDirectory = source == "" ? "" : Path.GetDirectoryName(source) })
                    { if (dialog.ShowDialog() != DialogResult.OK) return; NativePath = dialog.FileName; }
                    if (!Path.GetExtension(NativePath).Equals(".bhtseg", StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Đuôi tệp phải là .bhtseg.");
                }
                DialogResult = DialogResult.OK; Close();
            }
            catch (System.Exception ex) { MessageBox.Show(this, ex.Message, "BHT — kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }

    public partial class BhtPaletteControl
    {
        private void OpenSegments(bool import)
        {
            if (!NeedDoc()) return; var doc = _doc;
            AcadDispatcher.RunInCommandContext("Bảng đoạn / gói thầu", () => { if (_doc != doc) return OpResult.Fail("Bản vẽ đã đổi."); SegmentTableCommands.Open(doc, import); return OpResult.Success("Đã đóng bảng đoạn."); }, r => { StatusResult(r); RefreshAll(false); });
        }
    }
}
