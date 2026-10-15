using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace BHT.Palette
{
    public sealed class SignScaleForm : Form
    {
        private readonly ComboBox symbol = new NoWheelComboBox { Dock = DockStyle.Fill };
        private readonly ComboBox label = new NoWheelComboBox { Dock = DockStyle.Fill };
        private readonly ComboBox orientation = new NoWheelComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label error = new Label { AutoSize = true, ForeColor = Color.Firebrick };
        private readonly Label preview = new Label { Dock = DockStyle.Fill, AutoSize = true };
        public string SymbolScale { get; private set; }
        public string LabelScale { get; private set; }
        public string LabelOrientation { get; private set; }

        public SignScaleForm(string symbolScale, string labelScale) : this(symbolScale, labelScale, "PARALLEL") { }

        public SignScaleForm(string symbolScale, string labelScale, string labelOrientation)
        {
            Text = "Tỷ lệ biển, đèn và nhãn";
            Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(480, 390);
            MinimumSize = new Size(440, 300);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            var content = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var form = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(12) };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (var combo in new[] { symbol, label })
            {
                combo.Items.AddRange(new object[] { "0.5", "0.75", "1", "1.5", "2", "3", "5", "10" });
                combo.TextChanged += (s, e) => UpdatePreview();
            }
            symbol.Text = symbolScale; label.Text = labelScale;
            form.Controls.Add(new Label { Text = "Tỷ lệ biển / đèn", AutoSize = true, Padding = new Padding(0, 5, 0, 0) }); form.Controls.Add(symbol);
            form.Controls.Add(new Label { Text = "Tỷ lệ nhãn mã biển", AutoSize = true, Padding = new Padding(0, 5, 0, 0) }); form.Controls.Add(label);
            orientation.Items.AddRange(new object[] { "Song song đường nối biển–trụ", "Vuông góc đường nối biển–trụ" });
            orientation.SelectedIndex = labelOrientation == "PERPENDICULAR" ? 1 : 0;
            form.Controls.Add(new Label { Text = "Hướng nhãn tự động", AutoSize = true, Padding = new Padding(0, 5, 0, 0) }); form.Controls.Add(orientation);
            form.Controls.Add(preview); form.SetColumnSpan(preview, 2);
            var help = new Label { AutoSize = true, MaximumSize = new Size(435, 0), Text = "Chọn hoặc nhập tỷ lệ từ 0,01 đến 100. Mức 1 là kích thước chuẩn; hình biển/đèn và nhãn thay đổi độc lập.\r\n\r\nHướng nhãn lấy theo đoạn đường nối gần biển nhất. Biển hai trụ: cụm mã biển và lý trình đặt giữa hai đường nối. Nhãn đã dịch chuyển thủ công giữ vị trí và hướng.\r\n\r\nÁp dụng cho biển, bảng và đèn trong bản vẽ và khi chèn mới. Giữ vị trí, hướng biển, điểm RTK và lý trình." };
            form.Controls.Add(help); form.SetColumnSpan(help, 2);
            form.Controls.Add(error); form.SetColumnSpan(error, 2);
            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(8) };
            var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, AutoSize = true };
            var apply = new Button { Text = "Áp dụng", AutoSize = true };
            var reset = new Button { Text = "Về chuẩn 1:1", AutoSize = true };
            reset.Click += (s, e) => { symbol.Text = label.Text = "1"; error.Text = ""; };
            apply.Click += (s, e) => AcceptScale();
            buttons.Controls.Add(cancel); buttons.Controls.Add(apply); buttons.Controls.Add(reset);
            content.Controls.Add(form);
            AcceptButton = apply; CancelButton = cancel; Controls.Add(content); Controls.Add(buttons); UpdatePreview();
        }

        public static bool TryScale(string text, out double value)
        {
            return double.TryParse((text ?? "").Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out value) && value >= 0.01 && value <= 100 && Math.Round(value, 3) == value;
        }

        private void UpdatePreview()
        {
            double s, l;
            preview.Text = TryScale(symbol.Text, out s) && TryScale(label.Text, out l)
                ? "Biển / đèn × " + s.ToString("0.###", CultureInfo.InvariantCulture) + "    |    Nhãn × " + l.ToString("0.###", CultureInfo.InvariantCulture)
                : "Nhập tỷ lệ hợp lệ, tối đa 3 chữ số thập phân.";
        }

        private void AcceptScale()
        {
            double s, l;
            if (!TryScale(symbol.Text, out s)) { error.Text = "Tỷ lệ biển / đèn: từ 0,01 đến 100, tối đa 3 số lẻ."; symbol.Focus(); return; }
            if (!TryScale(label.Text, out l)) { error.Text = "Tỷ lệ nhãn: từ 0,01 đến 100, tối đa 3 số lẻ."; label.Focus(); return; }
            SymbolScale = s.ToString("0.###", CultureInfo.InvariantCulture);
            LabelScale = l.ToString("0.###", CultureInfo.InvariantCulture);
            LabelOrientation = orientation.SelectedIndex == 1 ? "PERPENDICULAR" : "PARALLEL";
            DialogResult = DialogResult.OK; Close();
        }
    }
}
