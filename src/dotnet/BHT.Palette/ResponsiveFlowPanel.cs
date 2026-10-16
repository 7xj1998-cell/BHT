using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BHT.Palette
{
    // A vertical page keeps its width; only its height grows with wrapped text.
    internal sealed class ResponsiveFlowPanel : FlowLayoutPanel
    {
        private bool sizing;
        private readonly HashSet<FlowLayoutPanel> observed = new HashSet<FlowLayoutPanel>();
        internal ResponsiveFlowPanel()
        {
            FlowDirection = FlowDirection.TopDown;
            WrapContents = false;
            DoubleBuffered = true;
        }

        // CAD returns focus after a command/dialog; that must not reposition the page.
        private bool explicitScroll;
        protected override Point ScrollToControl(Control activeControl) { return explicitScroll ? base.ScrollToControl(activeControl) : AutoScrollPosition; }
        internal void RevealControl(Control control)
        {
            explicitScroll=true;
            try { ScrollControlIntoView(control); }
            finally { explicitScroll=false; }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            bool handled=base.ProcessDialogKey(keyData);
            if((keyData & Keys.KeyCode)==Keys.Tab) {
                Control focused=this;
                while(focused.HasChildren) {
                    Control next=null;
                    foreach(Control child in focused.Controls)if(child.ContainsFocus){next=child;break;}
                    if(next==null)break;focused=next;
                }
                if(focused!=this)RevealControl(focused);
            }
            return handled;
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (sizing) return;
            sizing = true;
            try
            {
                // Reserve the scrollbar gutter even while it is temporarily hidden.
                int width = Math.Max(80, ClientSize.Width - Padding.Horizontal
                    - (AutoScroll && !VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0));
                foreach (Control child in Controls) Fit(child, Math.Max(40, width - child.Margin.Horizontal));
                base.OnLayout(e);
            }
            finally { sizing = false; }
        }

        private static int NaturalWidth(Control control)
        {
            var combo = control as ComboBox;
            if (combo != null) {
                int width = 170;
                foreach (var item in combo.Items) width = Math.Max(width, TextRenderer.MeasureText(combo.GetItemText(item),combo.Font).Width + 34);
                return width;
            }
            return TextRenderer.MeasureText(control.Text, control.Font, Size.Empty,
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix).Width + 28;
        }

        private void Fit(Control control, int width)
        {
            var panel = control as FlowLayoutPanel;
            if (panel != null)
            {
                if (observed.Add(panel)) panel.Layout += (sender, args) => { if (!sizing) PerformLayout(); };
                panel.AutoSize = false;
                panel.MaximumSize = new Size(width, 0);
                panel.MinimumSize = new Size(width, 0);
                panel.Width = width;
                int inner = Math.Max(40, width - panel.Padding.Horizontal);
                bool vertical = panel.FlowDirection == FlowDirection.TopDown;
                foreach (Control child in panel.Controls)
                {
                    int available = Math.Max(40, inner - child.Margin.Horizontal);
                    // Action rows wrap naturally; vertical sections fill the available width.
                    Fit(child, vertical ? available : Math.Min(available, NaturalWidth(child)));
                }
                panel.PerformLayout();
                panel.Height = panel.GetPreferredSize(new Size(width, 0)).Height;
                return;
            }
            control.MaximumSize = control is Label ? new Size(width, 0) : Size.Empty;
            control.Width = width;
            if (control is ListView)
            {
                control.MinimumSize = new Size(0, 175);
                control.Height = 175;
            }
            var button = control as Button;
            var check = control as CheckBox;
            if (button != null || check != null)
            {
                control.AutoSize = false;
                var text = TextRenderer.MeasureText(control.Text, control.Font,
                    new Size(Math.Max(20, width - (check != null ? 26 : 18)), int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                control.Height = Math.Max(32, text.Height + 14);
                if (button != null) {
                    button.TextAlign = ContentAlignment.MiddleLeft;
                    button.Padding = new Padding(8, 0, 6, 0);
                }
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate(true);
        }
    }
}
