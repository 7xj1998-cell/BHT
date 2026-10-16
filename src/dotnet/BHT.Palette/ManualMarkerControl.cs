using System;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.EditorInput;
using BHT.Bridge;
using BHT.Core;

namespace BHT.Palette
{
    internal sealed class ManualMarkerForm : Form
    {
        private readonly ComboBox group = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        private readonly TextBox km = new TextBox { Dock = DockStyle.Fill, MaxLength = 5 };
        private readonly TextBox h = new TextBox { Dock = DockStyle.Fill, MaxLength = 1 };
        private readonly TextBox note = new TextBox { Dock = DockStyle.Fill, MaxLength = 500 };
        private readonly Label error = new Label { AutoSize = true, ForeColor = Color.Firebrick, MaximumSize = new Size(430, 0) };
        public string Group { get { return group.SelectedIndex == 0 ? "COC_TIEU" : "COT_KM"; } }
        public string Km { get { return km.Text.Trim(); } }
        public string H { get { return Group == "COC_TIEU" ? h.Text.Trim() : ""; } }
        public string Note { get { return note.Text.Trim(); } }
        public ManualMarkerForm()
        {
            Text = "Bổ sung cọc tại vị trí trên CAD"; Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Dpi; AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink;
            FormBorderStyle = FormBorderStyle.FixedDialog; MinimizeBox = MaximizeBox = false; StartPosition = FormStartPosition.CenterParent;
            var layout = new TableLayoutPanel { AutoSize = true, Padding = new Padding(12), ColumnCount = 2, Dock = DockStyle.Fill };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
            var info = new Label { Text = "Chọn vị trí đã biết trên bản vẽ. Nguồn: bổ sung thủ công.\nChưa dùng làm mốc; có thể xác nhận tại Mốc lý trình đã nhập. Chưa biết cao độ.\nKm/H chưa biết: để trống; có thể bổ sung sau trong hồ sơ.", AutoSize = true, MaximumSize = new Size(435,0), Margin = new Padding(0,0,0,12) };
            layout.Controls.Add(info); layout.SetColumnSpan(info,2);
            group.Items.AddRange(new object[]{"Cọc tiêu", "Cột Km"}); group.SelectedIndex = 0;
            group.SelectedIndexChanged += (s,e) => { h.Enabled = Group == "COC_TIEU"; error.Text = ""; };
            km.TextChanged += (s,e) => error.Text = "";
            h.TextChanged += (s,e) => error.Text = "";
            foreach(var pair in new[]{Tuple.Create("Loại cọc",(Control)group),Tuple.Create("Số Km",(Control)km),Tuple.Create("H (0–9)",(Control)h),Tuple.Create("Căn cứ vị trí",(Control)note)})
            { layout.Controls.Add(new Label{Text=pair.Item1,AutoSize=true,Margin=new Padding(0,6,4,4)});layout.Controls.Add(pair.Item2); }
            layout.Controls.Add(error); layout.SetColumnSpan(error,2);
            var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            var ok = new Button { Text = "Chọn vị trí trên CAD…", AutoSize = true };
            ok.Click += (s,e) => { double value; if((Km!=""||H!="")&&!MarkerStation.TryMetres(Group,Km,H,out value)) {error.Text="Nhập Km nguyên 0–99999 và H 0–9, hoặc để trống cả hai.";return;} DialogResult=DialogResult.OK; };
            var cancel = new Button { Text="Hủy",AutoSize=true,DialogResult=DialogResult.Cancel };
            actions.Controls.Add(ok); actions.Controls.Add(cancel); layout.Controls.Add(actions); layout.SetColumnSpan(actions,2);
            Controls.Add(layout); AcceptButton=ok;CancelButton=cancel;
        }
    }

    public partial class BhtPaletteControl
    {
        private void CreateManualMarker()
        {
            if (!NeedDoc() || !ConfirmDiscardObjectEdits()) return;
            var expected = _doc;
            string group, km, h, note;
            using (var form = new ManualMarkerForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK || _doc != expected) return;
                group=form.Group;km=form.Km;h=form.H;note=form.Note;
            }
            Status("Chọn vị trí cọc trên CAD hoặc nhập tọa độ; Esc để hủy.");
            AcadDispatcher.RunInCommandContext("Thêm cọc thủ công", () =>
            {
                if (AcadDispatcher.ActiveDocument != expected) return OpResult.Fail("Bản vẽ đang mở đã đổi.");
                if (Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("CVPORT")) == 1)
                    return OpResult.Fail("Chuyển sang Model hoặc vào khung nhìn Model trước khi chọn cọc.");
                try { Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); } catch { }
                var picked=expected.Editor.GetPoint(new PromptPointOptions("\nVị trí cọc bổ sung (chọn hoặc nhập tọa độ, Esc để hủy): "));
                if(picked.Status!=PromptStatus.OK)return OpResult.Fail("Đã hủy; không tạo điểm hay hồ sơ.");
                // Editor.GetPoint supplies UCS coordinates; storage is always WCS XY.
                var wcs=picked.Value.TransformBy(expected.Editor.CurrentUserCoordinateSystem);
                return ManualMarker.Create(expected.Database,wcs,group,km,h,note);
            }, result => UI(() =>
            {
                if (_doc != expected) return;
                if(!result.Ok){StatusError(result.Message);return;}
                MarkObjectEditorClean(); RefreshAll(false); SelectObjectInList(result.Message);
                if(_lispOk) CallLisp("bht:api-symbol-sync",new[]{result.Message},"Vẽ cọc bổ sung",r=>{
                    if(r.Ok)Status("Đã bổ sung "+result.Message+". Vị trí thủ công; chưa xác nhận làm mốc lý trình.");
                    else StatusWarn("Đã lưu cọc "+result.Message+", chưa vẽ được ký hiệu. Bấm Vẽ / cập nhật ký hiệu để thử lại. "+r.ToString());
                });
                else StatusWarn("Đã lưu cọc "+result.Message+". Nạp Lisp BHT rồi bấm Vẽ / cập nhật ký hiệu để vẽ ký hiệu.");
            }));
        }
    }
}
