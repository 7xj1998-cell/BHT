using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BHT.Core;

namespace BHT.Palette
{
    public sealed class DrawingProjectForm : Form
    {
        public readonly TextBox NameInput = new TextBox();
        public readonly FlowLayoutPanel Actions = new FlowLayoutPanel();
        public readonly DataGridView Grid = new DataGridView();
        public readonly Label StatusText = new Label();
        readonly TextBox detail = new TextBox();

        public DrawingProjectForm(string drawingName)
        {
            Text = "BHT — Quản lý bình đồ và đồng bộ";
            Size = new Size(1180, 760); MinimumSize = new Size(820, 600);
            AutoScaleMode = AutoScaleMode.Dpi; StartPosition = FormStartPosition.CenterParent;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(14) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            top.Controls.Add(new Label { Text = "Tên bình đồ", AutoSize = true, Padding = new Padding(0, 6, 8, 0) });
            NameInput.Width = 380; NameInput.Text = drawingName; top.Controls.Add(NameInput);
            Actions.Dock = DockStyle.Fill; Actions.AutoSize = true; Actions.WrapContents = true;
            Actions.Padding = new Padding(0, 4, 0, 8);
            Grid.Dock = DockStyle.Fill; Grid.ReadOnly = true; Grid.AllowUserToAddRows = false;
            Grid.AllowUserToDeleteRows = false; Grid.RowHeadersVisible = false;
            Grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            Grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; Grid.MultiSelect = false;
            Grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            detail.Dock = DockStyle.Fill; detail.Multiline = true; detail.ReadOnly = true;
            detail.ScrollBars = ScrollBars.Vertical;
            StatusText.AutoSize = true; StatusText.Dock = DockStyle.Fill; StatusText.Padding = new Padding(0, 8, 0, 2);
            StatusText.Text = "Chọn dòng để đọc chi tiết. Có thể zoom/pan trong CAD khi bảng mở. Xem trước không ghi DWG.";
            layout.Controls.Add(top, 0, 0); layout.Controls.Add(Actions, 0, 1); layout.Controls.Add(Grid, 0, 2);
            layout.Controls.Add(detail, 0, 3); layout.Controls.Add(StatusText, 0, 4); Controls.Add(layout);
            Grid.SelectionChanged += (s, e) => ShowDetail();
            PaletteTheme.ApplyTableDialog(this);
        }

        public void SetRows(string[] titles, IList<BhtRecord> rows, string[] fields)
        {
            Grid.SuspendLayout();
            try {
                Grid.Rows.Clear(); Grid.Columns.Clear();
                for (int i = 0; i < titles.Length; i++) {
                    int index = Grid.Columns.Add(fields[i], titles[i]);
                    var column = Grid.Columns[index]; column.MinimumWidth = 95;
                    if (fields[i] == "state" || fields[i] == "drawing_state" || fields[i] == "path" || fields[i] == "reason" || fields[i] == "detail") {
                        column.MinimumWidth = 180; column.FillWeight = 170;
                    }
                }
                foreach (var row in rows) Grid.Rows[Grid.Rows.Add(fields.Select(f => (object)row.Get(f)).ToArray())].Tag = row;
            } finally { Grid.ResumeLayout(); }
            ShowDetail();
        }

        void ShowDetail()
        {
            if (Grid.SelectedRows.Count == 0) { detail.Text = ""; return; }
            var row = Grid.SelectedRows[0];
            detail.Text = string.Join(Environment.NewLine, Grid.Columns.Cast<DataGridViewColumn>()
                .Select(c => c.HeaderText + ": " + Convert.ToString(row.Cells[c.Index].Value)));
        }
    }
}
