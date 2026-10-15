using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BHT.Palette
{
    internal static class PaletteTheme
    {
        // Green workspace with pale input surfaces.
        internal static readonly Color GreenDark = Color.FromArgb(0x06, 0x5F, 0x46);   // #065F46
        internal static readonly Color GreenLight = Color.FromArgb(0xD1, 0xFA, 0xE5);  // #D1FAE5
        private static readonly Color Canvas = Color.FromArgb(0x04, 0x78, 0x57);
        private static readonly Color GreenDeep = Color.FromArgb(0x06, 0x4E, 0x3B);    // #064E3B hover / vien
        private static readonly Color GreenPressed = Color.FromArgb(0x02, 0x2C, 0x22); // #022C22 nhan nut
        private static readonly Color InputBack = Color.FromArgb(0xEC, 0xFD, 0xF5);
        private static readonly Color ErrorBack = Color.FromArgb(0xFE, 0xE2, 0xE2);
        private static readonly Color ErrorFore = Color.FromArgb(0x99, 0x1B, 0x1B);
        private static readonly Color WarnBack = Color.FromArgb(0xFE, 0xF3, 0xC7);
        private static readonly Color WarnFore = Color.FromArgb(0x92, 0x40, 0x0E);

        public static void Apply(Control root, TabControl tabs, Label document, Label status)
        {
            root.BackColor = Canvas;
            root.ForeColor = GreenLight;
            StyleChildren(root);
            // Dat sau StyleChildren de tieu de va vung trang thai khong bi to lai nhu nhan thuong.
            document.BackColor = GreenDark;
            document.ForeColor = GreenLight;
            document.Font = new Font("Segoe UI Semibold", 9f);
            status.BackColor = GreenLight;
            status.ForeColor = GreenDark;
            tabs.Alignment = TabAlignment.Top;
            tabs.Multiline = true;
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            using (var g = root.CreateGraphics()) UpdateTabMetrics(tabs, g.DpiY);
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.Padding = new Point(0, 0);
            tabs.ShowToolTips = true;
            tabs.DrawItem += DrawTab;
        }

        public static void ApplyDialog(Form dialog)
        {
            dialog.BackColor = Canvas;
            dialog.ForeColor = GreenLight;
            dialog.Font = new Font("Segoe UI", 9f);
            StyleChildren(dialog);
        }

        internal const int TabStripWidth = 64;
        internal const int TabHeight = 36;

        internal static void UpdateTabMetrics(TabControl tabs, float dpi)
        {
            float scale = Math.Max(1f, dpi / 96f);
            int minimum = (int)Math.Ceiling(TabStripWidth * scale);
            int width = Math.Max(minimum, (tabs.ClientSize.Width - 12) / Math.Max(1, tabs.TabCount));
            var size = new Size(width, (int)Math.Ceiling(TabHeight * scale));
            if (tabs.ItemSize != size) tabs.ItemSize = size;
        }

        internal static Color ErrorBackColor { get { return ErrorBack; } }
        internal static Color ErrorForeColor { get { return ErrorFore; } }
        internal static Color WarnBackColor { get { return WarnBack; } }
        internal static Color WarnForeColor { get { return WarnFore; } }

        public static Color StatusBack(string text)
        {
            string value = (text ?? "").ToUpperInvariant();
            if (value.Contains("LỖI") || value.Contains("KHÔNG CÙNG PHIÊN BẢN")) return ErrorBack;
            if (value.Contains("CẢNH BÁO") || value.Contains("ĐÃ BỊ HỦY")) return WarnBack;
            return GreenLight;
        }

        public static Color StatusFore(string text)
        {
            string value = (text ?? "").ToUpperInvariant();
            if (value.Contains("LỖI") || value.Contains("KHÔNG CÙNG PHIÊN BẢN")) return ErrorFore;
            if (value.Contains("CẢNH BÁO") || value.Contains("ĐÃ BỊ HỦY")) return WarnFore;
            return GreenDark;
        }

        private static void StyleChildren(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TabPage)
                {
                    c.BackColor = Canvas;
                    c.ForeColor = GreenLight;
                }
                else if (c is Button)
                {
                    var b = (Button)c;
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderColor = Color.FromArgb(79, 161, 136);
                    b.FlatAppearance.BorderSize = 1;
                    b.FlatAppearance.MouseOverBackColor = GreenDeep;
                    b.FlatAppearance.MouseDownBackColor = GreenPressed;
                    b.BackColor = GreenDark;
                    b.ForeColor = GreenLight;
                    b.UseVisualStyleBackColor = false;
                    if (b.Text == "Lưu hồ sơ" || b.Text == "Tính lý trình hồ sơ này" || b.Text == "Tính lý trình tất cả hồ sơ")
                    {
                        b.BackColor = GreenLight; b.ForeColor = GreenDark;
                        b.Font = new Font("Segoe UI Semibold", 9f);
                        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(167, 243, 208);
                        b.FlatAppearance.MouseDownBackColor = Color.FromArgb(110, 231, 183);
                    }
                    Color normalBack = b.BackColor, normalFore = b.ForeColor;
                    EventHandler enabledStyle = (s, e) => {
                        b.BackColor = b.Enabled ? normalBack : Color.FromArgb(203, 213, 209);
                        b.ForeColor = b.Enabled ? normalFore : Color.FromArgb(83, 99, 92);
                    };
                    b.EnabledChanged += enabledStyle;
                    enabledStyle(b, EventArgs.Empty);
                }
                else if (c is ListView)
                {
                    c.BackColor = Color.White;
                    c.ForeColor = GreenDark;
                    ((ListView)c).BorderStyle = BorderStyle.FixedSingle;
                    StyleListView((ListView)c);
                }
                else if (c is TextBoxBase && c != parent)
                {
                    var t = (TextBoxBase)c;
                    t.BackColor = t.ReadOnly ? Canvas : InputBack;
                    t.ForeColor = t.ReadOnly ? GreenLight : GreenDark;
                }
                else if (c is ComboBox)
                {
                    var cb = (ComboBox)c;
                    cb.FlatStyle = FlatStyle.Flat;
                    cb.BackColor = InputBack;
                    cb.ForeColor = GreenDark;
                }
                else if (c is ListBox)
                {
                    c.BackColor = GreenLight;
                    c.ForeColor = GreenDark;
                    StyleListBox((ListBox)c);
                }
                else if (c is Label)
                {
                    // Nhan tren nen xanh: chu sang; nhan canh bao (do) doi sang do nhat de van doc duoc.
                    if (c.ForeColor == Color.DarkRed || c.ForeColor == Color.Red) c.ForeColor = Color.MistyRose;
                    else c.ForeColor = GreenLight;
                }
                else if (c is CheckBox || c is RadioButton)
                {
                    c.ForeColor = GreenLight;
                    c.BackColor = Canvas;
                }
                else if (c is PictureBox)
                {
                    c.BackColor = GreenDark;
                }
                else if (c is FlowLayoutPanel || c is TableLayoutPanel || c is Panel)
                {
                    c.BackColor = Convert.ToString(c.Tag) == "route-section" ? Color.FromArgb(5, 107, 80) : parent.BackColor;
                    c.ForeColor = GreenLight;
                }
                StyleChildren(c);
            }
        }

        // Tieu de cot va dong dang chon cua ListView (Details) ve bang mau xanh thay cho xam/xanh duong he thong.
        private static void StyleListView(ListView lv)
        {
            if (lv.OwnerDraw || lv.View != View.Details) return;
            lv.OwnerDraw = true;
            lv.DrawColumnHeader += (s, e) =>
            {
                using (var back = new SolidBrush(GreenDark)) e.Graphics.FillRectangle(back, e.Bounds);
                using (var edge = new Pen(Canvas))
                    e.Graphics.DrawLine(edge, e.Bounds.Right - 1, e.Bounds.Top + 2, e.Bounds.Right - 1, e.Bounds.Bottom - 3);
                var r = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, Math.Max(0, e.Bounds.Width - 6), e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, e.Header == null ? "" : e.Header.Text, lv.Font, r, GreenLight,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            };
            lv.DrawItem += (s, e) => { e.DrawDefault = false; };
            lv.DrawSubItem += (s, e) =>
            {
                if (e.Item == null || !e.Item.Selected) { e.DrawDefault = true; return; }
                using (var back = new SolidBrush(GreenDark)) e.Graphics.FillRectangle(back, e.Bounds);
                var r = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, Math.Max(0, e.Bounds.Width - 6), e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, e.SubItem == null ? "" : e.SubItem.Text, lv.Font, r, GreenLight,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            };
            // ListView OwnerDraw + FullRowSelect co the chi ve lai cot dau khi doi lua chon: ve lai ca dong.
            lv.ItemSelectionChanged += (s, e) => { try { if (e.Item != null) lv.Invalidate(e.Item.Bounds); } catch { } };
        }

        // Dong dang chon cua ListBox: nen #065F46, chu #D1FAE5.
        private static void StyleListBox(ListBox lb)
        {
            if (lb.DrawMode != DrawMode.Normal) return;
            lb.DrawMode = DrawMode.OwnerDrawFixed;
            lb.ItemHeight = Math.Max(lb.Font.Height + 2, 15);
            lb.DrawItem += (s, e) =>
            {
                if (e.Index < 0 || e.Index >= lb.Items.Count)
                {
                    using (var bg = new SolidBrush(GreenLight)) e.Graphics.FillRectangle(bg, e.Bounds);
                    return;
                }
                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                using (var back = new SolidBrush(selected ? GreenDark : GreenLight)) e.Graphics.FillRectangle(back, e.Bounds);
                var r = new Rectangle(e.Bounds.X + 2, e.Bounds.Y, Math.Max(0, e.Bounds.Width - 3), e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, lb.GetItemText(lb.Items[e.Index]), lb.Font, r, selected ? GreenLight : GreenDark,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            };
        }

        private static void DrawTab(object sender, DrawItemEventArgs e)
        {
            var tabs = (TabControl)sender;
            PaintTab(e.Graphics, e.Bounds, e.Index == tabs.SelectedIndex, tabs.TabPages[e.Index].Text);
        }

        /// <summary>
        /// Compact horizontal navigation preserves the full width of the form.
        /// </summary>
        internal static void PaintTab(Graphics g, Rectangle r, bool active, string text)
        {
            using (var back = new SolidBrush(active ? GreenLight : GreenDark)) g.FillRectangle(back, r);
            if (active)
                using (var bar = new SolidBrush(GreenDark)) g.FillRectangle(bar, r.Left, r.Bottom - 3, r.Width, 3);
            using (var sep = new Pen(Canvas, 1f))
                g.DrawLine(sep, r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);

            Rectangle textRect = new Rectangle(r.Left + (active ? 6 : 3), r.Top + 1, Math.Max(0, r.Width - (active ? 8 : 5)), Math.Max(0, r.Height - 2));
            using (var font = new Font("Segoe UI Semibold", 9f))
                TextRenderer.DrawText(g, BalanceLines(g, text ?? "", font, textRect.Width), font, textRect,
                    active ? GreenDark : GreenLight,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak
                    | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        /// <summary>Ten the dai hon o chu: ngat thanh 2 dong can bang tai dau cach thuong (dau cach cung U+00A0 giu cac tu di lien,
        /// vd "Hồ\u00A0sơ đối\u00A0tượng" -> "Hồ sơ" / "đối tượng").</summary>
        private static string BalanceLines(Graphics g, string text, Font font, int width)
        {
            if (text.IndexOf(' ') < 0 || TextRenderer.MeasureText(g, text, font, Size.Empty, TextFormatFlags.NoPrefix).Width <= width) return text;
            string best = text;
            int bestWidth = int.MaxValue;
            for (int i = text.IndexOf(' '); i >= 0; i = text.IndexOf(' ', i + 1))
            {
                string a = text.Substring(0, i), b = text.Substring(i + 1);
                int w = Math.Max(TextRenderer.MeasureText(g, a, font, Size.Empty, TextFormatFlags.NoPrefix).Width,
                                 TextRenderer.MeasureText(g, b, font, Size.Empty, TextFormatFlags.NoPrefix).Width);
                if (w < bestWidth) { bestWidth = w; best = a + "\n" + b; }
            }
            return best;
        }

        /// <summary>
        /// Paint the navigation and page border consistently with the palette.
        /// </summary>
        internal sealed class ThemedTabControl : TabControl
        {
            public ThemedTabControl()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                         | ControlStyles.ResizeRedraw, true);
            }

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                base.OnSelectedIndexChanged(e);
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                UpdateTabMetrics(this, e.Graphics.DpiY);
                e.Graphics.Clear(Canvas);
                for (int i = 0; i < TabCount; i++)
                {
                    Rectangle r;
                    try { r = GetTabRect(i); } catch { continue; }
                    if (!e.ClipRectangle.IntersectsWith(r)) continue;
                    string[] labels = { "Chung", "Điểm", "Hồ sơ", "Ảnh", "Tuyến" };
                    PaintTab(e.Graphics, r, i == SelectedIndex, i < labels.Length ? labels[i] : TabPages[i].Text);
                }
                Rectangle page = DisplayRectangle;
                page.Inflate(1, 1);
                using (var pen = new Pen(GreenDark)) e.Graphics.DrawRectangle(pen, page.X, page.Y, page.Width - 1, page.Height - 1);
            }
        }
    }
}
