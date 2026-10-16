using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BHT.Bridge;
using BHT.Core;

namespace BHT.Palette
{
    public partial class BhtPaletteControl
    {
        // ================================================================ A. TONG QUAN
        private TextBox _ovText;
        private Label _workflowHint;

        private TabPage BuildOverviewTab()
        {
            var tp = new TabPage("Tổng quan") { ToolTipText = "Tổng quan bản vẽ và chuyển đến bước tiếp theo" };
            _ovText = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Top, Height = 300, Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.None };
            var body = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var workflow = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Padding = new Padding(6) };
            workflow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 6; i++) workflow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var title = new Label { Text = "BHT 1.0 · Bàn làm việc", AutoSize = true, Font = new Font("Segoe UI Semibold", 14f), Margin = new Padding(3, 4, 3, 8) };
            workflow.Controls.Add(title);
            _workflowHint = new Label { Text = "Mở bản vẽ để bắt đầu.", AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(3, 0, 3, 10) };
            workflow.Controls.Add(_workflowHint);
            var next = Btn("Tiếp tục công việc", (s,e)=>ContinueWorkflow());
            next.Dock=DockStyle.Top; next.MinimumSize=new Size(0,38); workflow.Controls.Add(next);
            var f = Flow();
            f.Controls.Add(Btn("Làm mới", (s, e) => { if (NeedDoc()) { RefreshAll(); ProbeLisp(); } }));
            f.Dock = DockStyle.Bottom;
            body.Controls.Add(_ovText);
            body.Controls.Add(workflow);
            tp.Controls.Add(body);
            tp.Controls.Add(f);
            return tp;
        }

        private Control WorkflowRow(string heading, string left, Action a, string right, Action b)
        {
            var row = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 2, Margin = new Padding(0, 0, 0, 7) };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            row.RowStyles.Add(new RowStyle(SizeType.AutoSize)); row.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            var label = new Label { Text = heading, AutoSize = true, Margin = new Padding(3, 3, 3, 4) };
            row.Controls.Add(label, 0, 0); row.SetColumnSpan(label, 2);
            var first = Btn(left, (s,e)=>{if(NeedDoc())a();}); first.Dock = DockStyle.Fill;
            var second = Btn(right, (s,e)=>{if(NeedDoc())b();}); second.Dock = DockStyle.Fill;
            // Allow Vietnamese labels to wrap at narrow dock widths.
            first.MinimumSize = second.MinimumSize = new Size(0, 42);
            row.Controls.Add(first, 0, 1); row.Controls.Add(second, 1, 1);
            return row;
        }

        private void CheckDrawing()
        {
            CallLisp("bht:api-check", new string[0], "Kiểm tra", r => {
                if (!r.Ok) return;
                RefreshOverview();
                ShowReportDialog("Kiểm tra bản vẽ", CheckReportText.Format(r.Values));
            });
        }

        private string BuildOverviewText()
        {
            if (_svc == null) return "";
            var o = _svc.GetOverview();
            int withoutStation = _svc.GetObjects().Values.Count(r=>!HasObjectStation(r));
            _workflowHint.Text = o.RtkPoints == 0 ? "Bắt đầu: nhập điểm khảo sát hoặc ảnh hiện trường." :
                o.ObjectRecords == 0 ? "Bước tiếp theo: chọn điểm để tạo hồ sơ báo hiệu." :
                o.ObjectsWithoutPoints > 0 ? "Cần kiểm tra: " + o.ObjectsWithoutPoints + " hồ sơ chưa có điểm vị trí hợp lệ." :
                withoutStation > 0 ? "Có " + withoutStation + " hồ sơ chưa có lý trình. Xem mốc đã nhập trước khi tính." :
                "Đã có " + o.ObjectRecords + " hồ sơ. Kiểm tra bản vẽ trước khi xuất hồ sơ.";
            var sb = new StringBuilder();
            int manualCount = _svc.GetPoints().Count(p=>p.IsManual);
            sb.Append("ĐIỂM KHẢO SÁT RTK (dữ liệu gốc): ").Append(o.RtkPoints-manualCount).Append("\r\n");
            sb.Append("ĐIỂM BỔ SUNG THỦ CÔNG: ").Append(manualCount).Append(" (cần xác nhận riêng nếu dùng làm mốc)\r\n");
            if (o.LegacyV01Points > 0) sb.Append("  + điểm BHT 0.1 chưa có ID: ").Append(o.LegacyV01Points).Append("\r\n");
            sb.Append("HỒ SƠ ĐỐI TƯỢNG (đơn vị quản lý): ").Append(o.ObjectRecords).Append("\r\n");
            sb.Append("  (1 điểm RTK ≠ 1 biển; 1 hồ sơ có thể gồm nhiều điểm, nhiều ảnh)\r\n");
            sb.Append("ẢNH TIMEMARK: ").Append(o.Photos).Append(" | GPS hợp lệ ").Append(o.PhotosGpsValid)
              .Append(" | GPS 0,0/không có ").Append(o.PhotosGpsInvalid).Append("\r\n");
            sb.Append("  đã gắn hồ sơ ").Append(o.PhotosLinked).Append(" | chưa gắn ").Append(o.PhotosUnlinked)
              .Append(" | có đề xuất chờ duyệt ").Append(o.PhotosSuggested).Append(" | ký hiệu ảnh ").Append(o.PhotoMarkers).Append("\r\n");
            sb.Append("TUYẾN THAM CHIẾU: ").Append(o.Routes).Append(" | ĐOẠN/GÓI: ").Append(o.Segments).Append("\r\n");
            sb.Append("TRÌNH BÀY: nhãn điểm ").Append(o.Labels).Append(" | ký hiệu đối tượng ").Append(o.Symbols).Append("\r\n");
            sb.Append("LISP: ").Append(_lispMsg).Append("\r\n");
            sb.Append("PLUGIN: BHT.Palette ").Append(BhtVersion.FileVersion).Append("\r\n");
            if (o.RtkPoints == 0 && o.ObjectRecords == 0 && o.Photos == 0 && o.Routes == 0 && o.Segments == 0)
                sb.Append("\r\nBản vẽ chưa có dữ liệu BHT. Bắt đầu bằng Nhập CSV RTK hoặc Nhập KMZ.\r\n");
            if (o.Warnings.Count > 0)
            {
                sb.Append("\r\nCẢNH BÁO:\r\n");
                foreach (var w in o.Warnings) sb.Append("  - ").Append(w).Append("\r\n");
            }
            return sb.ToString();
        }

        private void RefreshOverview() { if (_ovText != null) _ovText.Text = BuildOverviewText(); }

        private void ContinueWorkflow()
        {
            if (!NeedDoc()) return;
            var o = _svc.GetOverview();
            if (o.RtkPoints == 0) { _tabs.SelectedTab = _tabPoints; Status("Bản vẽ chưa có điểm RTK - chọn Nhập CSV RTK trong Điểm vị trí."); return; }
            if (o.ObjectRecords == 0) { _tabs.SelectedTab = _tabPoints; Status("Chọn điểm RTK để tạo hồ sơ đối tượng."); return; }
            if (o.ObjectsWithoutPoints > 0) { _tabs.SelectedTab = _tabObjects; Status("Mở hồ sơ chưa có điểm hợp lệ để bổ sung vị trí."); return; }
            if (_svc.GetObjects().Values.Any(r=>!HasObjectStation(r)))
            { _tabs.SelectedIndex = 4; Status("Xem cột Km và cọc tiêu đã nhập trước khi tính lý trình."); return; }
            _tabs.SelectedTab = _tabObjects;
            Status("Tiếp tục tại danh sách hồ sơ đối tượng.");
        }

        // ================================================================ B. DIEM KHAO SAT
        private TabPage _tabPoints;
        private TextBox _ptSearch, _ptDetail;
        private ComboBox _ptClass;
        private ListView _ptList;
        private Label _ptCount;

        private TabPage BuildPointsTab()
        {
            var tp = new TabPage("Điểm vị trí") { ToolTipText = "Điểm RTK và điểm bổ sung thủ công: tìm, thu phóng, xem nguồn vị trí, tạo hồ sơ từ điểm" };
            _tabPoints = tp;
            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 28, ColumnCount = 3 };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _ptSearch = new TextBox { Dock = DockStyle.Fill };
            _ptSearch.TextChanged += (s, e) => QueueSearch();
            _ptClass = new NoWheelComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _ptClass.Items.Add("(mọi loại)");
            foreach (var g in Groups.All) _ptClass.Items.Add(g[1]);
            _ptClass.SelectedIndex = 0;
            _ptClass.SelectedIndexChanged += (s, e) => FillPointList();
            _ptCount = new Label { AutoSize = true, Padding = new Padding(0, 6, 0, 0) };
            top.Controls.Add(_ptSearch, 0, 0); top.Controls.Add(_ptClass, 1, 0); top.Controls.Add(_ptCount, 2, 0);

            _ptList = new ListView { View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = true, Dock = DockStyle.Fill };
            _ptList.Columns.Add("Tên", 70); _ptList.Columns.Add("Mô tả gốc", 150); _ptList.Columns.Add("Loại gợi ý", 90);
            _ptList.Columns.Add("Hồ sơ", 90); _ptList.Columns.Add("ID", 120);
            _ptList.SelectedIndexChanged += (s, e) =>
            {
                if (_suppressSel) return;
                ShowPointDetail();
                HighlightSelectedPointInCad();
            };
            _ptList.DoubleClick += (s, e) => ZoomSelectedPoint();

            _ptDetail = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Bottom, Height = 190, Font = new Font("Consolas", 8.5f) };
            var f = Flow(); f.Dock = DockStyle.Bottom;
            f.Controls.Add(Btn("Nhập CSV RTK", (s, e) => SendCmd("BHTNHAP")));
            f.Controls.Add(Btn("Thu phóng", (s, e) => ZoomSelectedPoint()));
            f.Controls.Add(Btn("Thông tin (BHTINFO)", (s, e) => PointInfoFromLisp()));
            f.Controls.Add(Btn("Ảnh liên quan", (s, e) => PhotosForSelectedPoint()));
            f.Controls.Add(Btn("Tạo hồ sơ từ điểm", (s, e) => NewObjectFromPoints(SelectedListPointIds())));
            f.Controls.Add(_rtkUpdateLabelsButton = Btn("Cập nhật nhãn RTK", (s, e) => UpdateRtkLabels()));
            f.Controls.Add(_rtkScaleButton = Btn("Tỷ lệ ký hiệu và nhãn RTK…", (s, e) => ChooseRtkScale()));
            f.Controls.Add(_rtkRotateButton = Btn("Gom / xoay nhãn…", (s, e) => RotateRtkLabels()));
            _objectTips.SetToolTip(_rtkRotateButton, "Gom / xoay nhãn RTK và Timemark theo UCS hoặc hướng A → B. Giữ điểm đo; cập nhật nhãn vẫn giữ hướng đã chọn.");
            _objectTips.SetToolTip(_rtkUpdateLabelsButton, "Chọn điểm để sắp lại nhãn của các điểm đó; không chọn điểm sẽ cập nhật tất cả. Nhãn dời tay được giữ. Không đổi cỡ X hoặc cỡ chữ.");
            _objectTips.SetToolTip(_rtkScaleButton, "Chọn riêng cỡ dấu X và cỡ nhãn RTK. Đổi cỡ và giữ vị trí nhãn; cập nhật/sắp nhãn bằng nút riêng.");
            tp.Controls.Add(_ptList);
            tp.Controls.Add(top);
            tp.Controls.Add(f);
            tp.Controls.Add(_ptDetail);
            return tp;
        }

        private List<SurveyPoint> _points = new List<SurveyPoint>();
        private Dictionary<string, List<string>> _owners = new Dictionary<string, List<string>>();

        private void RefreshPoints()
        {
            UpdateRtkActionAvailability();
            _points = _svc.GetPoints().OrderBy(p => p.IdUpper, StringComparer.Ordinal).ToList();
            _owners = ObjectLogic.OwnerMap(_svc.GetObjects());
            FillPointList();
        }

        private void FillPointList()
        {
            if (_ptList == null) return;
            string cls = _ptClass.SelectedIndex > 0 ? (string)_ptClass.SelectedItem : null;
            var l = TextSearch.Filter(_points, _ptSearch.Text, cls);
            var selected = new HashSet<string>(SelectedListPointIds(), StringComparer.Ordinal);
            var topItem = _ptList.IsHandleCreated ? _ptList.TopItem : null;
            string topId = topItem != null ? ((SurveyPoint)topItem.Tag).IdUpper : null;
            int topIndex = topItem != null ? topItem.Index : 0;
            bool suppress = _suppressSel; _suppressSel = true;
            _ptList.BeginUpdate();
            try
            {
                _ptList.Items.Clear();
                foreach (var p in l)
                {
                    List<string> ow;
                    var it = new ListViewItem(new[] { p.Name, p.Description, p.Class, _owners.TryGetValue(p.IdUpper, out ow) ? string.Join(",", ow.ToArray()) : "", p.Id });
                    it.Tag = p;
                    _ptList.Items.Add(it);
                    it.Selected = selected.Contains(p.IdUpper);
                }
            }
            finally { _ptList.EndUpdate(); _suppressSel = suppress; }
            if (_ptList.IsHandleCreated && _ptList.Items.Count > 0)
                _ptList.TopItem = _ptList.Items.Cast<ListViewItem>().FirstOrDefault(item => ((SurveyPoint)item.Tag).IdUpper == topId) ?? _ptList.Items[Math.Min(topIndex, _ptList.Items.Count - 1)];
            _ptCount.Text = l.Count + "/" + _points.Count;
            ShowPointDetail();
        }

        private List<string> SelectedListPointIds()
        {
            return _ptList.SelectedItems.Cast<ListViewItem>().Select(i => ((SurveyPoint)i.Tag).IdUpper).ToList();
        }

        private SurveyPoint SelectedPoint()
        {
            return _ptList.SelectedItems.Count > 0 ? (SurveyPoint)_ptList.SelectedItems[0].Tag : null;
        }

        private void SelectPointInList(string surveyId)
        {
            string u = (surveyId ?? "").ToUpperInvariant();
            if (_ptSearch.Text != "" || _ptClass.SelectedIndex > 0) { _ptSearch.Text = ""; _ptClass.SelectedIndex = 0; }
            foreach (ListViewItem it in _ptList.Items)
            {
                bool m = ((SurveyPoint)it.Tag).IdUpper == u;
                it.Selected = m;
                if (m) it.EnsureVisible();
            }
            ShowPointDetail();
        }

        private void ShowPointDetail()
        {
            var p = SelectedPoint();
            if (p == null) { _ptDetail.Text = ""; return; }
            var sb = new StringBuilder();
            if(p.IsManual)sb.Append("BỔ SUNG THỦ CÔNG TRÊN CAD — không phải điểm đo RTK.\r\nCao độ chưa biết; Z=0 chỉ là mặt phẳng vẽ.\r\n");
            sb.Append("Tên: ").Append(p.Name).Append("   ID điểm: ").Append(p.Id).Append("\r\n");
            sb.Append("Mô tả gốc: [").Append(p.Description).Append("]\r\n");
            sb.Append("X (Đông) = ").Append(LispFormat.Fnum(p.X, 3)).Append("   Y (Bắc) = ").Append(LispFormat.Fnum(p.Y, 3)).Append("   Z = ").Append(LispFormat.Fnum(p.Z, 3)).Append("\r\n");
            sb.Append("Chuỗi gốc N/E/Z: ").Append(p.NRaw).Append(" / ").Append(p.ERaw).Append(" / ").Append(p.ZRaw).Append("\r\n");
            sb.Append("Nhóm gợi ý (từ mô tả, chưa xác nhận): ").Append(Groups.Label(p.Class)).Append(" (").Append(p.Class).Append(")\r\n");
            sb.Append("Dataset ").Append(p.Dataset).Append(", dòng ").Append(p.Row).Append(", file ").Append(p.SourceFile).Append(", không gian ").Append(p.Space).Append("\r\n");
            List<string> ow;
            sb.Append("Hồ sơ: ").Append(_owners.TryGetValue(p.IdUpper, out ow) ? string.Join(", ", ow.ToArray()) : "(chưa thuộc hồ sơ nào)").Append("\r\n");
            var r = PhotoLogic.ParseRadius(_svc.Meta("ghep_r", "10"));
            double rad = r;
            var near = Geo.PhotosNear(p.X, p.Y, _svc.GetPhotos(), rad);
            sb.Append("Ảnh chụp trong ").Append(rad).Append(" m (chỉ gợi ý): ");
            sb.Append(near.Count == 0 ? "(không)" : string.Join(", ", near.Take(10).Select(n => n.Item + " " + LispFormat.Fnum(n.Distance, 1) + "m").ToArray()));
            _ptDetail.Text = sb.ToString();
        }

        private void ZoomSelectedPoint()
        {
            var p = SelectedPoint();
            if (p == null) { Status("Chọn 1 điểm."); return; }
            HighlightAndZoom(p.Handle, p.X, p.Y, 30.0);
        }

        private void HighlightSelectedPointInCad()
        {
            var p = SelectedPoint();
            if (p == null || _doc == null) return;
            string why;
            if (AcadDispatcher.IsBusy(_doc, out why)) return;
            try
            {
                using (_doc.LockDocument())
                {
                    var id = CadView.IdFromHandle(_doc.Database, p.Handle);
                    if (id.IsNull) return;
                    _suppressSel = true;
                    _suppressUntil = DateTime.Now.AddMilliseconds(800);
                    try { _doc.Editor.SetImpliedSelection(new[] { id }); }
                    finally { _suppressSel = false; }
                }
            }
            catch (Exception ex) { StatusError("Chọn điểm trên CAD: " + ex.Message); }
        }

        private void PointInfoFromLisp()
        {
            var p = SelectedPoint();
            if (p == null) { Status("Chọn 1 điểm."); return; }
            CallLisp("bht:api-info-point", new[] { p.Id }, "Thông tin " + p.Name, r =>
            {
                if (r.Ok) _ptDetail.Text = string.Join("\r\n", r.Values.ToArray());
            });
        }

        private void PhotosForSelectedPoint()
        {
            var p = SelectedPoint();
            if (p == null) { Status("Chọn 1 điểm."); return; }
            _photoFilterPoint = p;
            _tabs.SelectedTab = _tabPhotos;
            FillPhotoList();
        }
    }
}
