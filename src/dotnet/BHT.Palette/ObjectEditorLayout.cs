using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using BHT.Core;

namespace BHT.Palette
{
    public partial class BhtPaletteControl
    {
        private ObjectFormPanel _objectForm;
        private Label _objectNameLabel;
        private int _objectLayoutDepth;

        private sealed class ObjectFormPanel : TableLayoutPanel
        {
            internal bool Updating;
            private readonly Dictionary<Control, int> spans = new Dictionary<Control, int>();
            private bool arranging;
            private bool? stacked;
            internal ObjectFormPanel() { DoubleBuffered = true; Padding = new Padding(8); }
            internal void EnableResponsiveLayout()
            {
                foreach (Control child in Controls) spans[child] = GetColumnSpan(child);
                PerformLayout();
            }
            protected override void OnLayout(LayoutEventArgs e)
            {
                if (arranging) return;
                arranging = true;
                try
                {
                    bool narrow = ClientSize.Width < 310;
                    if (spans.Count > 0 && stacked != narrow)
                    {
                        stacked = narrow;
                        SuspendLayout();
                        foreach (var item in spans)
                        {
                            SetColumnSpan(item.Key, narrow ? 2 : item.Value);
                            var label = item.Key as Label;
                            if (label != null) label.Margin = new Padding(3, narrow ? 6 : 3, 3, 3);
                        }
                        ResumeLayout(false);
                    }
                    int width = Math.Max(60, ClientSize.Width - Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 6);
                    foreach (Control child in Controls)
                    {
                        if (child is Label || child is CheckBox)
                        {
                            int available = GetColumnSpan(child) == 2 ? width : (GetColumn(child) == 0 ? 86 : Math.Max(50, width - 92));
                            child.MaximumSize = new Size(available, 0);
                            if (child is CheckBox) {
                                child.AutoSize = false;
                                child.Height = Math.Max(32, TextRenderer.MeasureText(child.Text, child.Font,
                                    new Size(Math.Max(20, available - 25), int.MaxValue), TextFormatFlags.WordBreak).Height + 8);
                                child.Width = available;
                            }
                        }
                    }
                    base.OnLayout(e);
                }
                finally { arranging = false; }
            }
            protected override void OnScroll(ScrollEventArgs se)
            {
                base.OnScroll(se);
                // Redraw child text after WinForms scrolls the backing surface.
                Invalidate(true);
            }
            protected override Point ScrollToControl(Control activeControl)
            {
                // Rebinding must not scroll to the field that had focus before CAD selection.
                return Updating ? DisplayRectangle.Location : base.ScrollToControl(activeControl);
            }
        }

        private void ChangeObjectEditor(Action change, bool startAtTop = false)
        {
            if (_objectForm == null) { change(); return; }
            bool outer = _objectLayoutDepth++ == 0;
            Point scroll = _objectForm.AutoScrollPosition;
            bool updating = _objectForm.Updating;
            if (outer) { _objectForm.Updating = true; _objectForm.SuspendLayout(); }
            try { change(); }
            finally
            {
                _objectLayoutDepth--;
                if (outer)
                {
                    _objectForm.ResumeLayout(true);
                    if (startAtTop) _oGroup.Focus();
                    _objectForm.AutoScrollPosition = startAtTop ? Point.Empty : new Point(-scroll.X, -scroll.Y);
                    _objectForm.Updating = updating;
                }
            }
        }

        private void KeepObjectScroll(Action action)
        {
            Point scroll = _objectForm.AutoScrollPosition;
            bool updating = _objectForm.Updating;
            _objectForm.Updating = true;
            try { action(); }
            finally
            {
                _objectForm.AutoScrollPosition = new Point(-scroll.X, -scroll.Y);
                _objectForm.Updating = updating;
            }
        }

        private void UpdateObjectGroupLayout()
        {
            ChangeObjectEditor(() => {
                string group = EditorGroup();
                if(_lightButton!=null) _lightButton.Visible=_lightLabel.Visible=group=="DEN" || group=="DEN_CS" || group=="DEN_TH";
                foreach (var control in _signRows) control.Visible = GroupRules.HasSignCode(group);
                foreach (var control in _markerRows) control.Visible = group == "COC_TIEU" || group == "COT_KM";
                if (_placeObjectButton != null)
                {
                    _placeObjectButton.Text = group == "COC_TIEU" ? "Cọc đặt tự động" : "Đặt ký hiệu…";
                    _placeObjectButton.Enabled = group != "COC_TIEU" && !_oIsNew && _oId.Text != "";
                    _objectTips.SetToolTip(_placeObjectButton, group == "COC_TIEU"
                        ? "Cọc tự đặt tại điểm khảo sát. Chọn tim trong Bố trí tất cả báo hiệu để cập nhật hướng; dùng Vẽ / cập nhật ký hiệu để cập nhật cọc."
                        : "Lưu thay đổi rồi chọn vị trí, đường dẫn và hướng ký hiệu trên CAD. G = tâm X gốc; T = vị trí tùy ý.");
                }
                bool namedBoard = group == "BANG_QC" || group == "KHAC";
                if (_objectNameLabel != null) _objectNameLabel.Text = namedBoard ? "Nội dung bảng" : "Mô tả";
                _oDesc.Multiline = namedBoard;
                _oDesc.AcceptsReturn = namedBoard;
                _oDesc.ScrollBars = namedBoard ? ScrollBars.Vertical : ScrollBars.None;
                _oDesc.Height = namedBoard ? 76 : 24;
                _objectTips.SetToolTip(_oDesc, namedBoard ? "Nhập tên hiển thị trên bảng chữ nhật nền xanh, chữ trắng. Block tùy chỉnh được ưu tiên nếu đã gán." : "Mô tả đối tượng khảo sát.");
                UpdateMarkerStation();
            });
        }
    }
}
