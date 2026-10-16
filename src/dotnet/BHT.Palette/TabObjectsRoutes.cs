using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BHT.Bridge;
using BHT.Core;

namespace BHT.Palette
{
    public partial class BhtPaletteControl
    {
        // ================================================================ D. HO SO DOI TUONG
        private TabPage _tabObjects;
        private ListView _objList;
        private TextBox _oId, _oDesc, _oPoles, _oFaces, _oNote, _oChainage, _oInfo;
        private TextBox _oMarkerKm, _oMarkerH;
        private string _signContent = "", _signLayout = "LEGACY", _signGap = "0.2", _signClearance = "0.6";
        private string _bridgeName = "", _bridgeStation = "", _roadName = "";
        private CheckBox _oMarkerNumber;
        private Button _oSaveChainage, _oClearChainage, _oCalculateChainage;
        private string _storedChainage = "";
        private readonly List<Control> _markerRows = new List<Control>();
        private ComboBox _oCustomBlock;
        private int _placementDirection;
        private string _placementAngle = "0";
        private readonly List<Control> _signRows = new List<Control>();
        private bool _syncingSignFill;
        private readonly ToolTip _objectTips = new ToolTip { AutoPopDelay = 20000 };
        private ComboBox _oGroup, _oCode, _oCodeType, _oSide, _oFaceCodes, _oCond;
        private SignComboFilter _codeFilter, _faceFilter;
        private CheckBox _oChecked, _oAllowShared, _oSignFill;
        private ListBox _oPoints, _oPhotos;
        private Label _oMode, _oSupportHint;
        private bool _oIsNew;
        private TextBox _objSearch;
        private ComboBox _objIssue;
        private Label _objCount;
        private string _objectBaseline;
        private bool _refreshingObjectList;
        private Button _saveObjectButton, _insertObjectButton, _placeObjectButton, _deleteObjectButton;
        private sealed class CodeChoice
        {
            public readonly string Code, Label;
            public CodeChoice(string code, string label) { Code = code; Label = label; }
            public override string ToString() { return Label; }
        }
        private static string ComboCode(ComboBox combo)
        {
            var choice = combo.SelectedItem as CodeChoice;
            return choice != null ? choice.Code : (combo.SelectedItem as string ?? "CHUA_XAC_DINH").Split(' ')[0];
        }

        private TabPage BuildObjectsTab()
        {
            var tp = new TabPage("Hồ\u00A0sơ đối\u00A0tượng") { ToolTipText = "Hồ sơ đối tượng: tạo / sửa hồ sơ từ điểm RTK, ký hiệu, ảnh và lý trình của đối tượng" };
            _tabObjects = tp;
            _objList = new ListView { View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false, Dock = DockStyle.Top, Height = 150 };
            _objList.Columns.Add("ID", 95); _objList.Columns.Add("Nhóm", 90); _objList.Columns.Add("Mã", 55);
            _objList.Columns.Add("Trụ", 35); _objList.Columns.Add("Điểm", 40); _objList.Columns.Add("Ảnh", 40); _objList.Columns.Add("Tình trạng", 80);
            _objList.MouseDoubleClick += (s, e) => {
                if (e.Button != MouseButtons.Left) return;
                var item = _objList.HitTest(e.Location).Item;
                if (item != null) ZoomObject(item.Tag as string);
            };
            _objectTips.SetToolTip(_objList, "Nhấp một lần để sửa hồ sơ; nhấp đúp trên dòng để thu phóng tới vị trí RTK của hồ sơ.");
            _objList.SelectedIndexChanged += (s, e) => {
                if (_refreshingObjectList) return;
                var it = _objList.SelectedItems.Count > 0 ? _objList.SelectedItems[0] : null;
                if (it != null) SelectObjectInList((string)it.Tag);
            };

            var form = _objectForm = new ObjectFormPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _oMode = new Label { AutoSize = false, Dock = DockStyle.Fill, Height = 40, AutoEllipsis = true, ForeColor = Color.DarkBlue, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            _oId = new TextBox { Dock = DockStyle.Fill };
            _oGroup = new NoWheelComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var g in Groups.All) _oGroup.Items.Add(new CodeChoice(g[1], g[2]));
            _oGroup.SelectionChangeCommitted += (s, e) => OnGroupChangedByUser();
            _oGroup.SelectedIndexChanged += (s, e) => UpdateObjectGroupLayout();
            // 5.0: tim khi go (khong dau) tren ma + ten bien; coc tieu / cot Km khong co ma bien -> danh sach rong.
            _oCode = new NoWheelComboBox { Dock = DockStyle.Fill, MaxDropDownItems = 18, DropDownWidth = 430 };
            Func<List<TdtSignEntry>> catalog = () => { try { return TdtSignLibrary.GetCatalog(); } catch { return new List<TdtSignEntry>(); } };
            _codeFilter = new SignComboFilter(_oCode, catalog, () => GroupRules.HasSignCode(EditorGroup()), false);
            _oCode.SelectedIndexChanged += (s, e) => { if (!_codeFilter.Busy) ApplySelectedSignSuggestion(); };
            _oCodeType = new NoWheelComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _oCodeType.Items.AddRange(new object[] { new CodeChoice("CHUA_XAC_DINH", "Chưa xác định"), new CodeChoice("QCVN", "QCVN"), new CodeChoice("NOI_BO", "Nội bộ") });
            _oMarkerKm = new TextBox { Dock = DockStyle.Fill, MaxLength = 5 };
            _oMarkerH = new TextBox { Dock = DockStyle.Fill, MaxLength = 1 };
            _oMarkerNumber = new CheckBox { Text = "Ghi số Km trên ký hiệu", AutoSize = true };
            _oMarkerNumber.CheckedChanged += (s, e) => { _oMarkerKm.Enabled = _oMarkerNumber.Checked; _oMarkerH.Enabled = _oMarkerNumber.Checked && EditorGroup() == "COC_TIEU"; UpdateMarkerStation(); };
            _oMarkerKm.Enabled = _oMarkerH.Enabled = false;
            _oMarkerKm.TextChanged += (s, e) => UpdateMarkerStation();
            _oMarkerH.TextChanged += (s, e) => UpdateMarkerStation();
            _oDesc = new TextBox { Dock = DockStyle.Fill };
            _oPoles = new TextBox { Dock = DockStyle.Fill };
            _oFaces = new TextBox { Dock = DockStyle.Fill };
            _oFaceCodes = new NoWheelComboBox { Dock = DockStyle.Fill, MaxDropDownItems = 18, DropDownWidth = 430 };
            _faceFilter = new SignComboFilter(_oFaceCodes, catalog, () => GroupRules.HasSignCode(EditorGroup()), true);
            _oFaceCodes.TextChanged += (s, e) => { _faceFilter.Remember(); if (!_faceFilter.Busy) AutoFaceCount(); };
            // 5.0: Tinh trang = o chon (Tốt / Bình thường / Hư hỏng), van go tu do duoc de giu gia tri cu.
            _oCond = new NoWheelComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown };
            _oCond.Items.AddRange(ConditionOptions.All);
            _oSide = new NoWheelComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _oSide.Items.AddRange(new object[] { new CodeChoice("CHUA_XAC_DINH", "Chưa xác định"), new CodeChoice("TRAI", "Trái"), new CodeChoice("PHAI", "Phải"), new CodeChoice("HAI_BEN", "Hai bên") });
            _oSignFill = new CheckBox { Text = "Tô nền biển báo", AutoSize = true, Checked = true };
            _oChecked = new CheckBox { Text = "Đã kiểm tra hiện trường", Dock = DockStyle.Fill, Height = 38 };
            _oNote = new TextBox { Dock = DockStyle.Fill };
            _oPoints = new ListBox { Dock = DockStyle.Fill, Height = 60, SelectionMode = SelectionMode.MultiExtended, IntegralHeight = false };
            _oPhotos = new ListBox { Dock = DockStyle.Fill, Height = 45, IntegralHeight = false };
            _oPhotos.DoubleClick += (s, e) => OpenSelectedObjectPhoto();
            var photoPanel = new TableLayoutPanel { Dock = DockStyle.Fill, Height = 82, RowCount = 2, ColumnCount = 1 };
            photoPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            photoPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var photoActions = Flow(); photoActions.Dock = DockStyle.Fill;
            photoActions.Controls.Add(Btn("Xem ở tab Ảnh", (s, e) => OpenSelectedObjectPhoto()));
            photoPanel.Controls.Add(_oPhotos, 0, 0);
            photoPanel.Controls.Add(photoActions, 0, 1);

            _oChainage = new TextBox { Dock = DockStyle.Fill };
            var chainPanel = new ResponsiveFlowPanel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 3, 0, 8) };
            chainPanel.Controls.Add(_oChainage);
            var saveChainage = _oSaveChainage = Btn("Lưu lý trình đã nhập", (s, e) => SaveManualChainage());
            var clearChainage = _oClearChainage = Btn("Xóa lý trình đã nhập", (s, e) => ClearManualChainage());
            var calculateChainage = _oCalculateChainage = Btn("Tính lý trình hồ sơ này", (s, e) => CalculateSelectedChainage());
            chainPanel.Controls.Add(saveChainage);
            chainPanel.Controls.Add(clearChainage);
            chainPanel.Controls.Add(calculateChainage);
            chainPanel.Controls.Add(Btn("Chọn 2 mốc…", (s,e) => ChooseFieldPair()));
            chainPanel.Controls.Add(Btn("Dùng mốc trong danh sách…", (s,e) => { if(_oIsNew || string.IsNullOrEmpty(_oId.Text)){StatusWarn("Lưu hồ sơ trước.");return;} string oid=_oId.Text; ReviewImportedMarkers(()=>CallLisp("bht:api-field-auto",new[]{oid},"Tính theo cọc đã nhập",r=>{if(r.Ok)ShowReportDialog("Kết quả lý trình",string.Join("\r\n",r.Values.ToArray()));})); }));
            _oAllowShared = new CheckBox { Text = "Cho phép dùng chung điểm RTK", AutoSize = true };
            _objectTips.SetToolTip(_oAllowShared, "Mặc định mỗi điểm RTK chỉ thuộc một hồ sơ. Bật khi cần tạo hồ sơ khác dùng cùng điểm (ví dụ biển báo và cọc tiêu cùng vị trí). BHT vẫn hỏi xác nhận; hồ sơ cũ không bị ghi đè.");
            _oInfo = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Height = 65, Font = new Font("Consolas", 8.5f) };
            Action<string, Control> row = (t, c) => { form.Controls.Add(new Label { Text = t, AutoSize = true, Padding = new Padding(0, 5, 0, 0) }); form.Controls.Add(c); };
            Action<string> section = title => {
                var label = new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Padding = new Padding(0, 10, 0, 4) };
                form.Controls.Add(label); form.SetColumnSpan(label, 2);
            };
            Action<string, Control> signRow = (title, control) => {
                var label = new Label { Text = title, AutoSize = true, Padding = new Padding(0, 5, 0, 0) };
                _signRows.Add(label); _signRows.Add(control); form.Controls.Add(label); form.Controls.Add(control);
            };
            form.Controls.Add(_oMode); form.SetColumnSpan(_oMode, 2);
            section("1. Hồ sơ");
            row("ID", _oId); row("Nhóm", _oGroup);
            _objectNameLabel = new Label { Text = "Mô tả", AutoSize = true, Padding = new Padding(0, 5, 0, 0) };
            form.Controls.Add(_objectNameLabel); form.Controls.Add(_oDesc);
            row("Tình trạng", _oCond); row("Phía đường", _oSide); row("", _oChecked); row("Ghi chú", _oNote);
            section("2. Ký hiệu trên CAD");
            _lightLabel = new Label { Text="Loại đèn", AutoSize=true };
            _lightButton = Btn("Chọn loại đèn…", (s,e)=>ChooseLight());
            form.Controls.Add(_lightLabel); form.Controls.Add(_lightButton);
            _lightButton.AutoSize=false; _lightButton.Dock=DockStyle.Fill; _lightButton.Height=52;
            _oCustomBlock = new NoWheelComboBox { Dock = DockStyle.Fill };
            _oCustomBlock.TextChanged += (s,e)=>RefreshLightChoice();
            _oCustomBlock.DropDown += (s, e) => RefreshCustomBlocks();
            var customPanel = Flow(); customPanel.Dock = DockStyle.Fill;
            _oCustomBlock.Width = 170; _oCustomBlock.Dock = DockStyle.None;
            customPanel.Controls.Add(_oCustomBlock);
            customPanel.Controls.Add(Btn("Chọn block CAD", (s, e) => PickCustomBlock()));
            customPanel.Controls.Add(Btn("Nạp DWG…", (s,e)=>ImportCustomDwg()));
            customPanel.Controls.Add(Btn("Bỏ gán", (s, e) => _oCustomBlock.Text = ""));
            row("Block tùy chỉnh", customPanel);
            signRow("Mã biển", _oCode); signRow("Loại mã", _oCodeType);

            foreach (var item in new[] { new { Label = "", Control = (Control)_oMarkerNumber }, new { Label = "Số Km", Control = (Control)_oMarkerKm }, new { Label = "H (cọc tiêu)", Control = (Control)_oMarkerH } })
            {
                var label = new Label { Text = item.Label, AutoSize = true }; form.Controls.Add(label); form.Controls.Add(item.Control); _markerRows.Add(label); _markerRows.Add(item.Control);
                label.Visible = item.Control.Visible = false;
            }
            _objectTips.SetToolTip(_oMarkerH, "H là số hectomet: 9 và Km 39 hiển thị H9/39. Cọc Km chỉ dùng ô Số Km.");
            row("Số trụ/chân", _oPoles); signRow("Số mặt biển", _oFaces); signRow("Các mặt biển", _oFaceCodes);
            var tip = _objectTips;
            tip.SetToolTip(_oCode, "Mã biển chính (quyết định ký hiệu/nhãn). Gõ mã hoặc tên, có dấu hay không dấu: vd 'di cham', '245a'. Cọc tiêu / Cột Km: để trống.");
            tip.SetToolTip(_oFaceCodes, "Mã từng mặt trên cùng trụ, cách nhau dấu ';'. Hai tấm cùng mã vẫn nhập hai lần. Số mặt biển tự đếm theo danh sách.");
            tip.SetToolTip(_oFaces, "Tự đếm theo các mặt đã chọn; một mã biển là một mặt. Thêm hoặc bỏ mặt trong Chọn biển để thay đổi số lượng.");
            tip.SetToolTip(_oPoles, "Số trụ/chân đỡ (1 trụ gắn 2 biển: Số trụ = 1, Số mặt = 2). Với 2 chân, gắn 2 điểm RTK đo chân để mỗi điểm có đường nối riêng vào ký hiệu.");
            section("3. Vị trí và ảnh hiện trường");
            row("Điểm vị trí", _oPoints);
            var pointActions = Flow();
            pointActions.Controls.Add(Btn("Gắn điểm có sẵn…", (s, e) => PickSurveyPoints(AddPointIds)));
            pointActions.Controls.Add(Btn("Gỡ điểm chọn", (s, e) => RemoveListPoints()));
            form.Controls.Add(pointActions); form.SetColumnSpan(pointActions, 2);
            _oSupportHint = new Label { AutoSize = true, Dock = DockStyle.Fill, MaximumSize = new Size(360, 0), ForeColor = Color.Firebrick };
            form.Controls.Add(_oSupportHint); form.SetColumnSpan(_oSupportHint, 2);
            row("Ảnh", photoPanel);
            section("4. Lý trình");
            form.Controls.Add(chainPanel); form.SetColumnSpan(chainPanel, 2);
            row("Thông tin", _oInfo);
            form.Controls.Add(_oAllowShared); form.SetColumnSpan(_oAllowShared, 2);

            var f1 = new TableLayoutPanel { Dock = DockStyle.Top, Height = 180, ColumnCount = 2, RowCount = 5, Padding = new Padding(2) };
            for (int column = 0; column < 2; column++) f1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int rowIndex = 0; rowIndex < 4; rowIndex++) f1.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            f1.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            var actions = new[] {
                Btn("Tạo từ RTK…", (s, e) => PickSurveyPoints(NewObjectFromPoints)),
                Btn("Thêm cọc…", (s, e) => CreateManualMarker()),
                Btn("Chọn biển…", (s, e) => OpenSignPicker()),
                _saveObjectButton = Btn("Lưu hồ sơ", (s, e) => SaveObject()),
                _placeObjectButton = Btn("Đặt ký hiệu…", (s, e) => PlaceCurrentObject()),
                _insertObjectButton = Btn("Vẽ / cập nhật ký hiệu", (s, e) => SymbolForCurrent()),
                _signScaleButton = Btn("Tỷ lệ biển / nhãn…", (s, e) => ChooseSignScale()) };
            for (int index = 0; index < actions.Length; index++) { actions[index].AutoSize = false; actions[index].Dock = DockStyle.Fill; f1.Controls.Add(actions[index], index % 2, index / 2); }
            f1.SetColumnSpan(_signScaleButton, 2);
            _objectTips.SetToolTip(actions[0], "Chọn điểm RTK trên CAD để tạo hồ sơ đối tượng.");
            _objectTips.SetToolTip(actions[1], "Tạo cọc tại vị trí chọn hoặc nhập trên CAD; ghi nguồn thủ công; xác nhận riêng tại Mốc lý trình đã nhập nếu dùng làm mốc.");
            _objectTips.SetToolTip(_placeObjectButton, "Lưu thay đổi rồi chọn vị trí, đường dẫn và hướng ký hiệu trên CAD. G = tâm X gốc; T = vị trí tùy ý.");
            _objectTips.SetToolTip(_insertObjectButton, "Lưu thay đổi rồi chèn ký hiệu còn thiếu hoặc cập nhật hình và nhãn của hồ sơ đang chọn.");
            _oSignFill.Text = "Tô nền tất cả biển"; _oSignFill.Margin = new Padding(4, 7, 2, 2);
            _objectTips.SetToolTip(_oSignFill, "Áp dụng cho biển báo, bảng chỉ dẫn, quảng cáo, Khác và Chưa xác định BHT đã chèn trong bản vẽ này, kể cả block tùy chỉnh. Bật: nền màu; tắt: bỏ nền, giữ hatch biểu tượng để in trắng đen. Không đổi Hatch của công trình khác.");
            _oSignFill.CheckedChanged += (s, e) => ChangeSignFill();
            f1.Controls.Add(_oSignFill, 0, 4); f1.SetColumnSpan(_oSignFill, 2);
            var f2 = Flow(); f2.Dock = DockStyle.Bottom;
            _deleteObjectButton=Btn("Xóa hồ sơ…",(s,e)=>DeleteCurrentObject());
            _objectTips.SetToolTip(_deleteObjectButton,"Xóa hồ sơ đang chọn cùng ký hiệu/nhãn/đường nối; giữ điểm RTK và ảnh gốc. Có xác nhận trước khi xóa.");
            f2.Controls.Add(_deleteObjectButton);
            f2.Controls.Add(Btn("Hiệu chỉnh hồ sơ đã xác nhận…", (s,e)=>OpenObjectCorrection(false)));
            f2.Controls.Add(Btn("Xem lịch sử…", (s,e)=>OpenObjectCorrection(true)));
            f2.Controls.Add(Btn("Bố trí GLOBAL / LOCAL…", (s,e)=>OpenPresentationMode()));
            f2.Controls.Add(Btn("Thông tin hồ sơ", (s, e) =>
            {
                if (_oIsNew || _oId.Text == "") return;
                CallLisp("bht:api-info-object", new[] { _oId.Text }, "Thông tin " + _oId.Text, r => { if (r.Ok) _oInfo.Text = string.Join("\r\n", r.Values.ToArray()); });
            }));
            var filter = new TableLayoutPanel { Dock = DockStyle.Top, Height = 60, ColumnCount = 3 };
            filter.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filter.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _objSearch = new TextBox { Dock = DockStyle.Fill };
            _objectTips.SetToolTip(_objSearch, "Tìm hồ sơ theo ID, nhóm, mã biển, mô tả hoặc tình trạng; hỗ trợ tìm không dấu.");
            _objSearch.TextChanged += (s, e) => QueueSearch();
            _objCount = new Label { AutoSize = true, Padding = new Padding(4, 5, 4, 0) };
            filter.Controls.Add(new Label { Text = "Tìm:", AutoSize = true, Padding = new Padding(2, 5, 0, 0) }, 0, 0);
            filter.Controls.Add(_objSearch, 1, 0); filter.Controls.Add(_objCount, 2, 0);
            _objIssue = new NoWheelComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _objIssue.Items.AddRange(new object[] { "Tất cả hồ sơ", "Chưa có lý trình", "Chưa có mã biển", "Thiếu điểm vị trí / chân trụ", "Lý trình cần kiểm tra" });
            _objIssue.SelectedIndex = 0;
            _objIssue.SelectedIndexChanged += (s,e)=>{if(_svc!=null)RefreshObjects();};
            filter.Controls.Add(_objIssue, 0, 1); filter.SetColumnSpan(_objIssue, 3);
            _objList.Dock = DockStyle.Fill;
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, Size = new Size(420, 650), Panel1MinSize = 60, Panel2MinSize = 350, SplitterDistance = 150 };
            split.Panel1.Controls.Add(_objList); split.Panel1.Controls.Add(filter);
            split.Panel2.Controls.Add(form); split.Panel2.Controls.Add(f1); split.Panel2.Controls.Add(f2);
            tp.Controls.Add(split);
            form.EnableResponsiveLayout();
            ClearObjectEditor();
            foreach (var control in new Control[] { _oId, _oDesc, _oPoles, _oFaces, _oNote, _oCode, _oCodeType, _oGroup, _oFaceCodes, _oCond, _oSide, _oCustomBlock, _oMarkerKm, _oMarkerH })
                control.TextChanged += (s, e) => UpdateObjectEditorState();
            _oMarkerNumber.CheckedChanged += (s, e) => UpdateObjectEditorState();
            _oChecked.CheckedChanged += (s, e) => UpdateObjectEditorState();
            _oAllowShared.CheckedChanged += (s, e) => UpdateObjectEditorState();
            _oChainage.TextChanged += (s, e) => UpdateObjectEditorState();
            _objectTips.SetToolTip(_saveObjectButton, "Ghi hồ sơ vào bản vẽ. Dùng Vẽ / cập nhật ký hiệu để thay đổi hình CAD.");
            return tp;
        }

        private void RefreshObjects()
        {
            RefreshSignFill();
            string keep = _oIsNew ? null : _oId.Text;
            string topId = _objList.IsHandleCreated && _objList.TopItem != null ? _objList.TopItem.Tag as string : null;
            int topIndex = _objList.IsHandleCreated && _objList.TopItem != null ? _objList.TopItem.Index : 0;
            bool preserveEdits = IsObjectEditorDirty();
            var objs = _svc.GetObjects();
            var pointIds = _objIssue.SelectedIndex == 3 ? new HashSet<string>(_svc.GetPoints().Select(p=>p.IdUpper), StringComparer.OrdinalIgnoreCase) : null;
            string query = _objSearch.Text;
            _refreshingObjectList = true;
            _objList.BeginUpdate();
            try
            {
            _objList.Items.Clear();
            foreach (var kv in objs.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                var r = kv.Value;
                if (!MatchesObjectIssue(r, _objIssue.SelectedIndex, pointIds)) continue;
                if (!TextSearch.Match(new SurveyPoint { Id = kv.Key, Name = Groups.Label(r.Get(ObjFields.Group)), Description = r.Get(ObjFields.Code) + " " + r.Get(ObjFields.Desc) + " " + r.Get(ObjFields.Condition) }, query)) continue;
                var it = new ListViewItem(new[] { kv.Key, Groups.Label(r.Get(ObjFields.Group)), r.Get(ObjFields.Code), r.Get(ObjFields.PoleCount),
                    r.GetAll(ObjFields.Point).Count.ToString(), r.GetAll(ObjFields.Photo).Count.ToString(), ObjectRevision.Retired(r)?"NGỪNG SỬ DỤNG":r.Get(ObjFields.Condition) });
                it.Tag = kv.Key;
                _objList.Items.Add(it);
                it.Selected = kv.Key == keep;
            }
            _objCount.Text = _objList.Items.Count + " / " + objs.Count;
            }
            finally
            {
                _objList.EndUpdate();
                if (_objList.IsHandleCreated && _objList.Items.Count > 0)
                {
                    var top = _objList.Items.Cast<ListViewItem>().FirstOrDefault(item => (string)item.Tag == topId);
                    _objList.TopItem = top ?? _objList.Items[Math.Min(topIndex, _objList.Items.Count - 1)];
                }
                _refreshingObjectList = false;
            }
            if (!preserveEdits && !string.IsNullOrEmpty(keep))
            {
                if (objs.ContainsKey(keep)) LoadObject(keep);
                else ClearObjectEditor();
            }
        }

        private static bool HasObjectStation(BhtRecord record)
        {
            double metres;
            return !string.IsNullOrWhiteSpace(record.Get(ObjFields.ChainageM)) ||
                MarkerStation.TryMetres(record.Get(ObjFields.Group), record.Get(ObjFields.MarkerKm), record.Get(ObjFields.MarkerH), out metres);
        }

        private static bool MatchesObjectIssue(BhtRecord record, int issue, HashSet<string> pointIds)
        {
            switch(issue)
            {
                case 1: return !HasObjectStation(record);
                case 2: return (record.Get(ObjFields.Group)=="BIEN_BAO" || record.Get(ObjFields.Group)=="BANG_CHI_DAN") &&
                    string.IsNullOrWhiteSpace(record.Get(ObjFields.Code)) && string.IsNullOrWhiteSpace(record.Get(ObjFields.CustomBlock));
                case 3:
                    var ids=record.GetAll(ObjFields.Point); int poles;
                    return ids.Count==0 || ids.Any(id=>!pointIds.Contains(id)) ||
                        (int.TryParse(record.Get(ObjFields.PoleCount),out poles) && poles>ids.Count);
                case 4: return record.Get(ObjFields.StationStatus)=="NEEDS_REVIEW" || record.Get(ObjFields.KmState)=="CAN_KIEM_TRA" || !string.IsNullOrEmpty(record.Get("field_last_error"));
                default: return true;
            }
        }

        private void SelectObjectInList(string oid)
        {
            if (_svc == null) return;
            if (string.Equals(_oId.Text, oid, StringComparison.OrdinalIgnoreCase) && IsObjectEditorDirty()) return;
            if (!ConfirmDiscardObjectEdits())
            {
                _refreshingObjectList = true;
                try { foreach (ListViewItem item in _objList.Items) item.Selected = (string)item.Tag == _oId.Text; }
                finally { _refreshingObjectList = false; }
                return;
            }
            _refreshingObjectList = true;
            try
            {
            foreach (ListViewItem it in _objList.Items)
            {
                bool m = string.Equals((string)it.Tag, oid, StringComparison.OrdinalIgnoreCase);
                it.Selected = m; if (m) it.EnsureVisible();
            }
            }
            finally { _refreshingObjectList = false; }
            LoadObject(oid);
        }

        private string ObjectEditorSignature()
        {
            return EditorFields().ToCanonical() + "\nid=" + _oId.Text + "\nchainage=" + _oChainage.Text + "\npoints=" + string.Join("|", EditorPointIds().ToArray()) + "\nshared=" + _oAllowShared.Checked;
        }

        private bool IsObjectEditorDirty()
        {
            return _objectBaseline != null && _objectBaseline != ObjectEditorSignature();
        }

        private void MarkObjectEditorClean()
        {
            AutoFaceCount();
            _objectBaseline = ObjectEditorSignature();
            UpdateObjectEditorState();
        }

        private void UpdateObjectEditorState()
        {
            AutoFaceCount();
            if (_objectBaseline == null) return;
            bool dirty = IsObjectEditorDirty();
            _oMode.Text = (_oIsNew ? "HỒ SƠ MỚI" : "HỒ SƠ " + _oId.Text) + (dirty ? " • Chưa lưu thay đổi" : (_oIsNew ? " • Chọn điểm RTK để bắt đầu" : " • Đã lưu"));
            _insertObjectButton.Enabled = !_oIsNew && _oId.Text != "";
            _placeObjectButton.Enabled = _insertObjectButton.Enabled && EditorGroup() != "COC_TIEU";
            if(_deleteObjectButton != null) _deleteObjectButton.Enabled = !_oIsNew && _oId.Text != "" && _doc != null && _lispOk && !_lispRunning;
            int poles = 0;
            bool multiple = new[] { "BIEN_BAO", "BANG_QC", "KHAC", "CHUA_XAC_DINH" }.Contains(EditorFields().Get(ObjFields.Group)) && int.TryParse(_oPoles.Text.Trim(), out poles) && poles > 1;
            if (_oSupportHint != null)
            {
                int linked = EditorPointIds().Distinct(StringComparer.OrdinalIgnoreCase).Count();
                _oSupportHint.Text = multiple && linked < poles ? "Thiếu " + (poles - linked) + " điểm chân RTK. Bấm Thêm điểm CAD để nối từng chân vào biển." : "";
                _oSupportHint.Visible = _oSupportHint.Text != "";
            }
        }

        private bool ConfirmDiscardObjectEdits()
        {
            return !IsObjectEditorDirty() || MessageBox.Show(this,
                "Hồ sơ đang có thay đổi chưa lưu. Bỏ các thay đổi để chuyển hồ sơ?\nChọn Không để quay lại và bấm Lưu hồ sơ.",
                "BHT — thay đổi chưa lưu", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private void ClearObjectEditor()
        {
            ChangeObjectEditor(ClearObjectEditorFields);
        }

        private void ClearObjectEditorFields()
        {
            CloseSignPicker();
            _oIsNew = true;
            _oMode.Text = "HỒ SƠ MỚI (chưa lưu)";
            _oAllowShared.Enabled = true;
            _objectTips.SetToolTip(_oMode, "Chọn điểm và kiểm tra nhóm đối tượng trước khi lưu hồ sơ.");
            _objectTips.SetToolTip(_oAllowShared, "Mặc định mỗi điểm RTK chỉ thuộc một hồ sơ. Bật để tạo thêm hồ sơ dùng cùng điểm; BHT hỏi xác nhận và không ghi đè hồ sơ cũ.");
            _signContent = ""; _signLayout = "LEGACY"; _signGap = "0.2"; _signClearance = "0.6";
            _bridgeName = _bridgeStation = _roadName = _oMarkerKm.Text = _oMarkerH.Text = ""; _oMarkerNumber.Checked = false;
            _oId.Text = ""; _oId.ReadOnly = false; _oCustomBlock.Text = "";
            _oGroup.SelectedIndex = Groups.All.Length - 1;
            _oCode.SelectedIndex = -1; _oCode.Text = ""; _oCodeType.SelectedIndex = 0; _oDesc.Text = ""; _oPoles.Text = ""; _oFaces.Text = ""; _oFaceCodes.Text = "";
            _oCond.Text = ""; _oSide.SelectedIndex = 0;  _oChecked.Checked = false; _oNote.Text = "";
            _oPoints.Items.Clear(); _oPhotos.Items.Clear();
            _oPhotos.Items.Add("(chưa gắn ảnh — bấm Nhập KMZ hoặc sang tab Ảnh)");
            _storedChainage = ""; _oChainage.Text = ""; UpdateMarkerStation(); _oInfo.Text = ""; _oAllowShared.Checked = false;
            MarkObjectEditorClean();
        }

        private static void SelectCombo(ComboBox c, string value)
        {
            for (int i = 0; i < c.Items.Count; i++)
            {
                var choice = c.Items[i] as CodeChoice;
                if ((choice != null ? choice.Code : ((string)c.Items[i]).Split(' ')[0]) == value) { c.SelectedIndex = i; return; }
            }
            if (!string.IsNullOrEmpty(value)) { c.Items.Add(value); c.SelectedIndex = c.Items.Count - 1; }
        }

        private void ApplySelectedSignSuggestion()
        {
            var sign = _oCode.SelectedItem as TdtSignEntry;
            if (sign == null) return;
            SelectCombo(_oGroup, "BIEN_BAO");
            SelectCombo(_oCodeType, "QCVN");
            if (string.IsNullOrWhiteSpace(_oDesc.Text) || _oDesc.Text == "bb" || _oDesc.Text.StartsWith("bb.", StringComparison.OrdinalIgnoreCase))
                _oDesc.Text = sign.Description;
        }

        private string EditorSignCode()
        {
            var sign = _oCode.SelectedItem as TdtSignEntry;
            if (sign != null) return sign.Code;
            // danh sach da loc lai -> chi con chu "W.245a — Đi chậm": lay phan ma
            string t = _oCode.Text.Trim();
            int i = t.IndexOf(" — ", StringComparison.Ordinal);
            return i > 0 ? t.Substring(0, i).Trim() : t;
        }

        private string EditorGroup()
        {
            return ComboCode(_oGroup);
        }

        /// <summary>5.0: Coc tieu / Cot Km khong co ma bien - bo ma bien (neu co) khi nguoi dung chon nhom nay.</summary>
        private void OnGroupChangedByUser()
        {
            if (GroupRules.HasSignCode(EditorGroup())) return;
            if (_oCode.Text.Trim() != "")
            {
                _oCode.SelectedIndex = -1; _oCode.Text = "";
                SelectCombo(_oCodeType, "CHUA_XAC_DINH");
                Status("Nhóm " + Groups.Label(EditorGroup()) + " không có mã biển - đã để trống Mã hiệu.");
            }
            _oCode.DroppedDown = false; _oCode.Items.Clear();
        }

        private void AutoFaceCount()
        {
            _oFaces.ReadOnly = EditorGroup() == "BIEN_BAO";
            if (!_oFaces.ReadOnly) return;
            string next = SignSearch.FaceCount(EditorSignCode(), _oFaceCodes.Text);
            if (next != _oFaces.Text.Trim()) _oFaces.Text = next;
        }

        private void LoadObject(string oid)
        {
            ChangeObjectEditor(() => LoadObjectFields(oid));
        }

        private void LoadObjectFields(string oid)
        {
            if (_svc == null) return;
            var r = _svc.GetObject(oid);
            if (r == null) { Status("Không có hồ sơ " + oid); return; }
            if (_oIsNew || _oId.Text != oid) CloseSignPicker();
            _oIsNew = false;
            _oMode.Text = "SỬA HỒ SƠ " + oid.ToUpperInvariant();
            _oAllowShared.Enabled = false;
            _oId.Text = oid.ToUpperInvariant(); _oId.ReadOnly = true; _oCustomBlock.Text = r.Get(ObjFields.CustomBlock);
            SelectCombo(_oGroup, r.Get(ObjFields.Group));
            _oCode.Text = r.Get(ObjFields.Code); SelectCombo(_oCodeType, r.Get(ObjFields.CodeType) == "" ? "CHUA_XAC_DINH" : r.Get(ObjFields.CodeType));
            _signContent = r.Get(ObjFields.SignContent); _signLayout = r.Get(ObjFields.SignLayout) == "" ? "LEGACY" : r.Get(ObjFields.SignLayout);
            _signGap = r.Get(ObjFields.SignGap) == "" ? "0.2" : r.Get(ObjFields.SignGap); _signClearance = r.Get(ObjFields.SignClearance) == "" ? "0.6" : r.Get(ObjFields.SignClearance);
            _bridgeName = r.Get(ObjFields.BridgeName); _bridgeStation = r.Get(ObjFields.SignChainage); _roadName = r.Get(ObjFields.RoadName);
            _oMarkerKm.Text = r.Get(ObjFields.MarkerKm); _oMarkerH.Text = r.Get(ObjFields.MarkerH); _oMarkerNumber.Checked = _oMarkerKm.Text != "";
            _oDesc.Text = r.Get(ObjFields.Desc); _oPoles.Text = r.Get(ObjFields.PoleCount); _oFaces.Text = r.Get(ObjFields.FaceCount);
            _oFaceCodes.Items.Clear(); _oFaceCodes.Text = string.Join("; ", r.GetAll(ObjFields.Face).ToArray());
            _oCode.Items.Clear(); _oCode.Text = r.Get(ObjFields.Code);
            _oCond.Text = ConditionOptions.Display(r.Get(ObjFields.Condition)); SelectCombo(_oSide, r.Get(ObjFields.RoadSide) == "" ? "CHUA_XAC_DINH" : r.Get(ObjFields.RoadSide));

            _oChecked.Checked = r.Get(ObjFields.CheckState) == "DA_KIEM_TRA"; _oNote.Text = r.Get(ObjFields.Note);
            _oPoints.Items.Clear();
            var idx = _svc.PointIndex();
            foreach (var p in r.GetAll(ObjFields.Point))
            {
                SurveyPoint sp;
                _oPoints.Items.Add(idx.TryGetValue(p.ToUpperInvariant(), out sp) ? p + " | " + sp.Name + " | " + sp.Description : p + " | (KHÔNG CÒN ĐIỂM)");
            }
            _oPhotos.Items.Clear();
            foreach (var a in r.GetAll(ObjFields.Photo)) _oPhotos.Items.Add(a);
            if (_oPhotos.Items.Count == 0) _oPhotos.Items.Add("(chưa gắn ảnh — sang tab Ảnh để chọn và xác nhận gắn)");
            _storedChainage = r.Get(ObjFields.ChainageKm); _oChainage.Text = _storedChainage; UpdateMarkerStation();
            var km = new StringBuilder();
            if (ProgressPolicy.Locked(r)) km.Append("ĐÃ XÁC NHẬN — dùng Hiệu chỉnh hồ sơ đã xác nhận để sửa riêng đối tượng.\r\n");
            if (r.Get(ObjFields.ChainageKm) != "") km.Append(r.Get(ObjFields.ChainageKm)).Append(r.Get("station_source")=="FIELD_MARKERS" ? " (theo cọc)" : " offset "+r.Get(ObjFields.OffsetM)+" m "+r.Get(ObjFields.RouteSide));
            else km.Append("(chưa tính lý trình - ").Append(r.Get(ObjFields.KmState)).Append(")");
            km.Append("\r\nPhía đường đã nhập: ").Append(r.Get("phia_duong"));
            if (r.Get("gan_doan_pp") == "KHAC_PHIA_DUONG") km.Append(" | Chưa gán: khác phía đường của đoạn");
            km.Append("\r\nĐoạn/gói: ").Append(r.Get(ObjFields.Segment)).Append(" / ").Append(r.Get(ObjFields.Package))
              .Append("\r\nTạo ").Append(r.Get(ObjFields.CreatedAt)).Append(" | sửa ").Append(r.Get(ObjFields.ModifiedAt));
            if (r.Get("station_source") == "FIELD_MARKERS") {
                km.Append("\r\nNguồn: mốc thực địa ").Append(r.Get("field_used_a")).Append(" → ").Append(r.Get("field_used_b"))
                  .Append("\r\nLệch đường nối cọc: ").Append(r.Get("field_offset_m")).Append(" m | ").Append(r.Get("station_status"))
                  .Append("\r\n").Append(r.Get("nguon_km"));
            }
            if(r.Get("field_last_error")!="")km.Append("\r\nChưa tính lại: ").Append(r.Get("field_last_error"));
            if(r.Get("position_source")=="MANUAL_CAD" || r.GetAll("pt").Any(id=>idx.ContainsKey(id.ToUpperInvariant()) && idx[id.ToUpperInvariant()].IsManual))
                km.Insert(0,"Vị trí: bổ sung thủ công trên CAD; chưa biết cao độ. Xem trạng thái mốc tại Mốc lý trình đã nhập.\r\n");
            _oInfo.Text = km.ToString();
            AutoFaceCount();
            MarkObjectEditorClean();
        }

        private void OpenSelectedObjectPhoto()
        {
            var link = _oPhotos.SelectedItem as string;
            if (string.IsNullOrEmpty(link) || link.StartsWith("(", StringComparison.Ordinal))
            {
                _tabs.SelectedTab = _tabPhotos;
                Status(_photos.Count == 0 ? "Chưa có dữ liệu ảnh — bấm Nhập KMZ." : "Chọn ảnh, chọn hồ sơ rồi bấm Xác nhận gắn.");
                return;
            }
            string photoId = ObjectLogic.PhotoIdOfLink(link);
            _tabs.SelectedTab = _tabPhotos;
            SelectPhoto(photoId);
        }

        private void UpdateMarkerStation()
        {
            if (_oChainage == null || _oSaveChainage == null || _oMarkerNumber == null) return;
            double metres;
            bool derived = _oMarkerNumber.Checked && MarkerStation.TryMetres(EditorGroup(), _oMarkerKm.Text.Trim(), _oMarkerH.Text.Trim(), out metres);
            _oMarkerH.Enabled = _oMarkerNumber.Checked && EditorGroup() == "COC_TIEU";
            bool wasDerived = _oChainage.ReadOnly;
            _oChainage.ReadOnly = derived;
            _oSaveChainage.Enabled = _oClearChainage.Enabled = _oCalculateChainage.Enabled = !derived;
            if (derived) { MarkerStation.TryMetres(EditorGroup(), _oMarkerKm.Text.Trim(), _oMarkerH.Text.Trim(), out metres); _oChainage.Text = Chainage.Format(metres); }
            else if (wasDerived) _oChainage.Text = _storedChainage;
            _objectTips.SetToolTip(_oChainage, derived ? "Lưu hồ sơ là đủ: lý trình lấy từ số Km/H. Trong chế độ thực địa, mở Mốc lý trình đã nhập để kiểm tra và dùng các mốc; không cần BHTMOCKM." : "Tính theo phương pháp đang chọn trong tab Tuyến, hoặc chọn trực tiếp hai mốc (cột Km hoặc cọc tiêu).");
        }

        private void CalculateSelectedChainage()
        {
            if (_oIsNew || string.IsNullOrEmpty(_oId.Text)) { StatusWarn("Lưu hồ sơ trước khi tính lý trình."); return; }
            string id = _oId.Text;
            if(NeedsImportedMarkerReview(id)){ReviewImportedMarkers(()=>CalculateSelectedChainage());return;}
            CallLisp("bht:api-object-station", new[] { id }, "Tính lý trình " + id, r => {
                if (!r.Ok) return;
                ShowReportDialog("Kết quả lý trình " + id, string.Join("\r\n", r.Values.ToArray()));
            });
        }

        private void SaveManualChainage()
        {
            if (_oIsNew || string.IsNullOrEmpty(_oId.Text)) { Status("Lưu hồ sơ trước khi nhập lý trình."); return; }
            if (!NeedDoc()) return;
            if (_oChainage.ReadOnly) { SaveObject(); return; }
            double metres;
            if (!Chainage.TryParse(_oChainage.Text, out metres))
            {
                StatusWarn("Lý trình không hợp lệ. Nhập dạng Km12+345.67, 12+345.67 hoặc số mét 12345.67.");
                return;
            }
            var fields = new BhtRecord()
                .Add(ObjFields.ChainageM, LispFormat.Fnum(metres, 3))
                .Add(ObjFields.ChainageKm, Chainage.Format(metres))
                .Add(ObjFields.KmState, "NHAP_TAY")
                .Add(ObjFields.KmSource, "Nhập thủ công từ Palette")
                .Add("station_source", "MANUAL").Add("field_last_error", "").Add(ObjFields.StationStatus, "MANUAL").Add(ObjFields.StationRouteRevision, "")
                .Add(ObjFields.RouteId, "").Add(ObjFields.OffsetM, "").Add(ObjFields.RouteSide, "")
                .Add(ObjFields.Segment, "").Add(ObjFields.Package, "")
                .Add(ObjFields.SegMethod, "CHUA_PHAN_DOAN").Add(ObjFields.SegCandidates, "");
            string id = _oId.Text;
            var result = AcadDispatcher.RunWrite(_doc, "Ghi lý trình tay", db => _svc.UpdateObject(id, fields));
            if (result.Ok) Status("Đã ghi " + Chainage.Format(metres) + " cho " + id + "."); else StatusError(result.ToString());
            if (!result.Ok) return;
            RefreshAll();
            if (_lispOk) CallLisp("bht:api-station-label-refresh", new[] { id }, "Cập nhật lý trình " + id, null);
        }

        private void ClearManualChainage()
        {
            if (_oIsNew || string.IsNullOrEmpty(_oId.Text) || !NeedDoc()) return;
            string id = _oId.Text;
            var fields = new BhtRecord()
                .Add(ObjFields.RouteId, "").Add(ObjFields.ChainageM, "").Add(ObjFields.ChainageKm, "")
                .Add(ObjFields.OffsetM, "").Add(ObjFields.RouteSide, "")
                .Add("station_source", "").Add("field_last_error", "").Add(ObjFields.KmState, "CHUA_TINH").Add(ObjFields.KmSource, "")
                .Add(ObjFields.StationStatus, "NO_ROUTE").Add(ObjFields.StationRouteRevision, "")
                .Add(ObjFields.Segment, "").Add(ObjFields.Package, "")
                .Add(ObjFields.SegMethod, "CHUA_PHAN_DOAN").Add(ObjFields.SegCandidates, "");
            var result = AcadDispatcher.RunWrite(_doc, "Xóa lý trình", db => _svc.UpdateObject(id, fields));
            if (result.Ok) Status("Đã xóa lý trình của " + id + "."); else StatusError(result.ToString());
            if (result.Ok)
            {
                RefreshAll();
                if (_lispOk) CallLisp("bht:api-station-label-refresh", new[] { id }, "Cập nhật lý trình " + id, null);
            }
        }

        private List<string> EditorPointIds()
        {
            return _oPoints.Items.Cast<string>().Select(s => s.Split('|')[0].Trim().ToUpperInvariant()).ToList();
        }

        /// <summary>
        /// Tao ho so tu diem: doc ten / mo ta / toa do, goi y nhom TU MO TA (co can cu), kiem tra diem da thuoc ho so khac.
        /// Mac dinh KHONG tao ho so moi khi diem da thuoc ho so khac.
        /// </summary>
        private void NewObjectFromPoints(List<string> ids)
        {
            if (!NeedDoc()) return;
            if (ids.Count == 0) { Status("Chưa chọn điểm RTK nào (chọn POINT/nhãn BHT trong CAD hoặc trong thẻ Điểm RTK)."); return; }
            _tabs.SelectedTab = _tabObjects;
            List<string> hits, same;
            ObjectLogic.Overlap(ids, _svc.GetObjects(), out hits, out same);
            if (hits.Count > 0)
            {
                string msg = "Điểm đã chọn ĐÃ THUỘC hồ sơ: " + string.Join(", ", hits.ToArray()) + "."
                    + (same.Count > 0 ? "\nBộ điểm TRÙNG KHỚP hồ sơ " + string.Join(", ", same.ToArray()) + " - có thể hồ sơ đã được tạo." : "")
                    + "\n\nYes = mở hồ sơ " + hits[0] + " để xem / sửa / thêm điểm\nNo = soạn hồ sơ MỚI dùng chung điểm (khi Lưu sẽ hỏi xác nhận lần nữa)\nCancel = hủy (mặc định)";
                var ans = MessageBox.Show(this, msg, "BHT - điểm đã thuộc hồ sơ khác", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                if (ans == DialogResult.Yes) { SelectObjectInList(hits[0]); return; }
                if (ans != DialogResult.No) { Status("Đã hủy - không tạo hồ sơ."); return; }
            }
            if (!ConfirmDiscardObjectEdits()) return;
            ChangeObjectEditor(() => InitializeObjectFromPoints(ids, hits), true);
        }

        private void InitializeObjectFromPoints(List<string> ids, List<string> hits)
        {
            ClearObjectEditor();
            _oId.Text = _svc.NextObjectId();
            _oAllowShared.Checked = hits.Count > 0;
            var idx = _svc.PointIndex();
            var pts = new List<SurveyPoint>();
            foreach (var id in ids)
            {
                SurveyPoint sp;
                if (idx.TryGetValue(id, out sp)) { pts.Add(sp); _oPoints.Items.Add(sp.Id + " | " + sp.Name + " | " + sp.Description); }
                else _oPoints.Items.Add(id + " | (KHÔNG CÒN ĐIỂM)");
            }
            string basis;
            SelectCombo(_oGroup, Groups.Suggest(pts, out basis));
            if (pts.Count == 1) _oDesc.Text = pts[0].Description;
            if(pts.Count>0 && pts.All(p=>TextSearch.Fold(p.Description).StartsWith("tru.matbb"))) { SelectCombo(_oGroup,"BIEN_BAO"); _oCond.Text="Mất mặt biển, còn trụ"; _oFaces.Text="0"; }
            _oMode.Text = "HỒ SƠ MỚI — " + ids.Count + " điểm";
            _objectTips.SetToolTip(_oMode, "Gợi ý: " + basis + ". Kiểm tra nhóm rồi bấm Lưu.");
            _objectTips.SetToolTip(_oAllowShared, "Gợi ý phân nhóm: " + basis + ". Kiểm tra nhóm trước khi lưu.\nMặc định mỗi điểm RTK thuộc một hồ sơ. Bật để tạo thêm hồ sơ dùng cùng điểm; BHT sẽ hỏi xác nhận và giữ hồ sơ cũ.");
            Status("Soạn hồ sơ mới. Nhóm chỉ là GỢI Ý từ mô tả gốc - hãy xác nhận.");
            UpdateObjectEditorState();
        }

        private void ApplyPickedSign(SignPickerForm picker)
        {
            ChangeObjectEditor(() => {
                _signContent = picker.SelectedContent; _signLayout = picker.SelectedLayout; _signGap = picker.SelectedGap; _signClearance = picker.SelectedClearance;
                if (_signLayout != "LEGACY") _oPoles.Text = SignLayoutSpec.Find(_signLayout).Posts.ToString();
                _bridgeName = picker.SelectedBridgeName; _bridgeStation = picker.SelectedBridgeStation; _roadName = picker.SelectedBridgeRoad;
                var sign = picker.SelectedSign;
                SelectCombo(_oGroup, "BIEN_BAO");
                _oCode.Items.Clear(); _oCode.SelectedIndex = -1; _oCode.Text = picker.SelectedCode;

                if (picker.SelectedFaces != null)
                {
                    _oFaceCodes.Items.Clear(); _oFaceCodes.Text = string.Join("; ", picker.SelectedFaces.ToArray());
                    _oFaces.Text = picker.SelectedFaces.Count.ToString();
                }
                else { _oFaceCodes.Items.Clear(); _oFaceCodes.Text = picker.SelectedCode; }
                AutoFaceCount();
                SelectCombo(_oCodeType, CustomSignPlate.IsCustom(picker.SelectedCode) ? "NOI_BO" : "QCVN");
                string previous = _oDesc.Text.Trim();
                bool catalogDescription = TdtSignLibrary.GetCatalog().Any(x => x.Description == previous);
                if (previous == "" || catalogDescription) _oDesc.Text = sign.Description;
                else if (!previous.Contains(sign.Description) && !string.IsNullOrWhiteSpace(sign.Description))
                    _oDesc.Text = sign.Description + " | " + previous;
            });
            Status("Đã chọn " + picker.SelectedCode + " — " + picker.SelectedSign.Description + ". Bấm Lưu để ghi hồ sơ.");
        }

        private bool CustomBlockExists(string name)
        {
            using (var tr = _doc.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var table = (Autodesk.AutoCAD.DatabaseServices.BlockTable)tr.GetObject(_doc.Database.BlockTableId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                if (!table.Has(name)) return false;
                var block = (Autodesk.AutoCAD.DatabaseServices.BlockTableRecord)tr.GetObject(table[name], Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                return !block.IsLayout && !block.IsAnonymous && !block.IsFromExternalReference;
            }
        }

        private void RefreshCustomBlocks()
        {
            if (!NeedDoc()) return;
            string text = _oCustomBlock.Text;
            using (var tr = _doc.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var table = (Autodesk.AutoCAD.DatabaseServices.BlockTable)tr.GetObject(_doc.Database.BlockTableId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                _oCustomBlock.Items.Clear();
                foreach (Autodesk.AutoCAD.DatabaseServices.ObjectId id in table)
                {
                    var block = (Autodesk.AutoCAD.DatabaseServices.BlockTableRecord)tr.GetObject(id, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                    if (!block.IsLayout && !block.IsAnonymous && !block.IsFromExternalReference) _oCustomBlock.Items.Add(block.Name);
                }
            }
            _oCustomBlock.Text = text;
        }

        private void PickCustomBlock()
        {
            if (!NeedDoc()) return;
            var document = _doc;
            AcadDispatcher.RunInCommandContext("Chọn block tùy chỉnh", () =>
            {
                var prompt = new Autodesk.AutoCAD.EditorInput.PromptEntityOptions("\nChọn block dùng cho hồ sơ này: ");
                prompt.SetRejectMessage("\nHãy chọn một block.");
                prompt.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.BlockReference), true);
                var result = document.Editor.GetEntity(prompt);
                if (result.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) return OpResult.Fail("Đã hủy chọn.");
                using (var tr = document.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var block = (Autodesk.AutoCAD.DatabaseServices.BlockReference)tr.GetObject(result.ObjectId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                    var definition = (Autodesk.AutoCAD.DatabaseServices.BlockTableRecord)tr.GetObject(
                        block.IsDynamicBlock ? block.DynamicBlockTableRecord : block.BlockTableRecord, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                    if (definition.IsFromExternalReference) return OpResult.Fail("Không dùng Xref làm block hồ sơ.");
                    return OpResult.Success(definition.Name);
                }
            }, result =>
            {
                if (result.Ok && _doc == document) { _oCustomBlock.Text = result.Message; Status("Đã chọn block " + result.Message + ". Bấm Lưu để gán riêng cho hồ sơ."); }
                else StatusResult(result);
            });
        }

        private BhtRecord EditorFields()
        {
            var f = new BhtRecord()
                .Add(ObjFields.Group, EditorGroup())
                .Add(ObjFields.SignContent, _signContent).Add(ObjFields.SignLayout, _signLayout).Add(ObjFields.SignGap, _signGap).Add(ObjFields.SignClearance, _signClearance)
                .Add(ObjFields.BridgeName, _bridgeName.Trim().Normalize())
                .Add(ObjFields.SignChainage, _bridgeStation.Trim().Normalize())
                .Add(ObjFields.RoadName, _roadName.Trim().Normalize())
                .Add(ObjFields.MarkerKm, _oMarkerNumber.Checked ? _oMarkerKm.Text.Trim() : "")
                .Add(ObjFields.MarkerH, _oMarkerNumber.Checked ? _oMarkerH.Text.Trim() : "")
                .Add(ObjFields.CustomBlock, _oCustomBlock.Text.Trim())
                .Add(ObjFields.Code, SignPresentation.ResolveCode(EditorSignCode(), _oDesc.Text))
                .Add(ObjFields.CodeType, ComboCode(_oCodeType))
                .Add(ObjFields.Desc, _oDesc.Text)
                .Add(ObjFields.PoleCount, _oPoles.Text.Trim())
                .Add(ObjFields.FaceCount, _oFaces.Text.Trim())
                .Add(ObjFields.Condition, _oCond.Text.Trim())
                .Add(ObjFields.RoadSide, ComboCode(_oSide))
                .Add(ObjFields.CheckState, _oChecked.Checked ? "DA_KIEM_TRA" : "CHUA_KIEM_TRA")
                .Add(ObjFields.Note, _oNote.Text);
            foreach (var m in SignSearch.SplitCodes(_oFaceCodes.Text)) f.Add(ObjFields.Face, m);
            return f;
        }

        private static bool IsCount(string s) { int n; return s == "" || (int.TryParse(s, out n) && n >= 0 && n <= 999); }

        private bool SaveObject(bool updateSymbol = false)
        {
            if (!NeedDoc()) return false;
            if(updateSymbol && (EditorGroup()=="DEN_CS" || EditorGroup()=="DEN_TH") && string.IsNullOrWhiteSpace(_oCustomBlock.Text))
            { StatusWarn("Chọn loại đèn trước khi chèn ký hiệu."); return false; }
            if((EditorGroup()=="DEN_CS" || EditorGroup()=="DEN_TH") && _oCustomBlock.Text.StartsWith("BHT_LIGHT_",StringComparison.OrdinalIgnoreCase)
                && !LightModels.ForGroup(EditorGroup()).Any(m=>_oCustomBlock.Text.IndexOf("BHT_LIGHT_"+m[0]+"_",StringComparison.OrdinalIgnoreCase)>=0))
            { StatusWarn("Mẫu đèn không thuộc nhóm đã chọn. Bấm Chọn loại đèn để chọn lại."); return false; }
            AutoFaceCount();
            if (!IsCount(_oPoles.Text.Trim()) || !IsCount(_oFaces.Text.Trim())) { StatusWarn("Số trụ / số mặt phải là số nguyên ≥ 0 (hoặc để trống)."); return false; }
            int faceCount;
            var faceCodes = SignSearch.SplitCodes(_oFaceCodes.Text);
            if (EditorGroup() == "BIEN_BAO" && (faceCodes.Count > 20 || (int.TryParse(_oFaces.Text, out faceCount) && faceCount > 20)))
            { StatusWarn("Một cụm ký hiệu hỗ trợ tối đa 20 mặt biển."); return false; }
            if (EditorGroup() == "BIEN_BAO" && faceCodes.Count > 0)
            {
                _oCode.Text = faceCodes[0];
                if (!int.TryParse(_oFaces.Text, out faceCount) || faceCount < faceCodes.Count) _oFaces.Text = faceCodes.Count.ToString();
            }
            if (_oMarkerNumber.Checked && (EditorGroup() == "COC_TIEU" || EditorGroup() == "COT_KM"))
            {
                int km, hm;
                if (!int.TryParse(_oMarkerKm.Text, out km) || km < 0 || km > 99999 || (EditorGroup() == "COC_TIEU" && (!int.TryParse(_oMarkerH.Text, out hm) || hm < 0 || hm > 9)))
                { StatusWarn("Nhập số Km nguyên từ 0 đến 99999; cọc tiêu cần H từ 0 đến 9."); return false; }
                _oMarkerKm.Text = km.ToString();
            }
            if (_bridgeStation.Trim() != "") { double chainage; if (!Chainage.TryParse(_bridgeStation, out chainage)) { StatusWarn("Lý trình trên biển không hợp lệ; ví dụ Km252+831."); return false; } }
            if (EditorGroup() == "BIEN_BAO" && _signLayout != "LEGACY") {
                double gap, height;
                if (!double.TryParse(_signGap, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out gap) || !double.TryParse(_signClearance, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out height)) { StatusWarn("Thông số CAP1 không hợp lệ."); return false; }
                int count = Math.Max(faceCodes.Count, int.TryParse(_oFaces.Text, out faceCount) ? faceCount : 1);
                string error = SignLayoutSpec.Validate(_signLayout, count, gap, height); if (error != "") { StatusWarn(error); return false; }
                _oPoles.Text = SignLayoutSpec.Find(_signLayout).Posts.ToString();
            }
            var f = EditorFields();
            if (!_oChainage.ReadOnly && _oChainage.Text.Trim() != _storedChainage)
            {
                double metres = 0;
                bool hasStation = _oChainage.Text.Trim() != "";
                if (hasStation && !Chainage.TryParse(_oChainage.Text, out metres))
                { StatusWarn("Lý trình không hợp lệ. Nhập Km12+345.67 hoặc số mét 12345.67."); _oChainage.Focus(); return false; }
                f.Set(ObjFields.ChainageM, hasStation ? LispFormat.Fnum(metres, 3) : "")
                 .Set(ObjFields.ChainageKm, hasStation ? Chainage.Format(metres) : "")
                 .Set(ObjFields.KmState, hasStation ? "NHAP_TAY" : "CHUA_TINH")
                 .Set(ObjFields.KmSource, hasStation ? "Nhập thủ công từ Palette" : "")
                 .Set(ObjFields.StationStatus, hasStation ? "MANUAL" : "NO_ROUTE")
                 .Set(ObjFields.StationRouteRevision, "").Set(ObjFields.RouteId, "")
                 .Set(ObjFields.OffsetM, "").Set(ObjFields.RouteSide, "")
                 .Set(ObjFields.Segment, "").Set(ObjFields.Package, "")
                 .Set(ObjFields.SegMethod, "CHUA_PHAN_DOAN").Set(ObjFields.SegCandidates, "");
            }
            if (f.Get(ObjFields.CustomBlock) != "" && !CustomBlockExists(f.Get(ObjFields.CustomBlock)))
            { StatusWarn("Block tùy chỉnh không tồn tại trong bản vẽ hiện tại."); return false; }
            if (!_oIsNew && !ConfirmNoDuplicate(f, false)) return false;
            if (_oIsNew)
            {
                var ids = EditorPointIds();
                if (ids.Count == 0) { StatusWarn("Hồ sơ mới cần ít nhất 1 điểm RTK."); return false; }
                List<string> hits, same;
                ObjectLogic.Overlap(ids, _svc.GetObjects(), out hits, out same);
                bool share = false;
                if (hits.Count > 0)
                {
                    if (!_oAllowShared.Checked) { StatusWarn("Điểm đã thuộc hồ sơ " + string.Join(", ", hits.ToArray()) + " - KHÔNG tạo (đánh dấu 'Cho phép dùng chung' nếu thật sự cần)."); return false; }
                    if (MessageBox.Show(this, "Tạo hồ sơ MỚI dùng chung điểm với " + string.Join(", ", hits.ToArray()) + "?" +
                        (same.Count > 0 ? "\nCHÚ Ý: bộ điểm trùng khớp hồ sơ " + string.Join(", ", same.ToArray()) + "." : ""),
                        "BHT - xác nhận dùng chung điểm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    { Status("Đã hủy - không tạo hồ sơ."); return false; }
                    share = true;
                }
                if (!ConfirmNoDuplicate(f, share)) return false;
                string id = _oId.Text.Trim().ToUpperInvariant();
                var r = AcadDispatcher.RunWrite(_doc, "Tạo hồ sơ", db => _svc.CreateObject(id, f, ids, share));
                if (r.Ok) Status("Đã tạo hồ sơ " + r.Message + "."); else StatusError(r.ToString());
                if (!r.Ok) return false;
                MarkObjectEditorClean();
                RefreshAll(false);
                SelectObjectInList(r.Message);
                if (_lispOk && updateSymbol)
                    CallLisp("bht:api-symbol-sync", new[] { r.Message }, "Chèn ký hiệu " + r.Message, null);
            }
            else
            {
                string id = _oId.Text;
                var r = AcadDispatcher.RunWrite(_doc, "Cập nhật hồ sơ", db => _svc.UpdateObject(id, f));
                StatusResult(r);
                if (!r.Ok) return false;
                MarkObjectEditorClean();
                RefreshAll(false);
                if (_lispOk && updateSymbol) CallLisp("bht:api-symbol-sync", new[] { id }, "Cập nhật ký hiệu " + id, null); // chi cap nhat / tao theo object_id
            }
            return true;
        }

        /// <summary>
        /// 5.0: Coc tieu / Cot Km - canh bao trung (cung diem RTK, cach &lt;= nguong meta trung_kc_m mac dinh 0.5 m,
        /// Cot Km trung gia tri Km) voi ho so CUNG nhom. Chi doc, khong dich/sua diem RTK. true = tiep tuc luu.
        /// </summary>
        private bool ConfirmNoDuplicate(BhtRecord f, bool sharedConfirmed)
        {
            string g = f.Get(ObjFields.Group);
            if (!DuplicateCheck.Applies(g)) return true;
            List<DuplicateHit> hits;
            double tol;
            try
            {
                tol = DuplicateCheck.ParseTolerance(_svc.Meta(DuplicateCheck.MetaKey, "0.5"));
                string km = _oIsNew ? "" : (_svc.GetObject(_oId.Text) ?? new BhtRecord()).Get(ObjFields.ChainageKm);
                hits = DuplicateCheck.Find(_oId.Text, g, EditorPointIds(), km, _svc.GetObjects(), _svc.PointIndex(), tol, sharedConfirmed);
            }
            catch (Exception ex) { StatusWarn("Không kiểm tra được trùng: " + ex.Message); return true; }
            if (hits.Count == 0) return true;
            string msg = Groups.Label(Groups.Code(g)) + " " + (_oIsNew ? "mới" : _oId.Text) + " có thể TRÙNG với:\n  "
                + string.Join("\n  ", hits.Take(10).Select(h => h.ToString()).ToArray())
                + (hits.Count > 10 ? "\n  ... (+" + (hits.Count - 10) + ")" : "")
                + "\n\nNgưỡng khoảng cách: " + LispFormat.Fnum(tol, 2) + " m."
                + "\nĐiểm RTK KHÔNG bị di chuyển hay sửa.\n\nYes = vẫn lưu    No = hủy (mặc định)";
            if (MessageBox.Show(this, msg, "BHT - cảnh báo trùng " + Groups.Label(Groups.Code(g)), MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes) return true;
            Status("Đã hủy lưu - trùng " + hits[0].Id + ".");
            return false;
        }

        private void PlaceCurrentObject()
        {
            if (_oIsNew || _oId.Text == "") { Status("Lưu hồ sơ trước khi đặt ký hiệu tự do."); return; }
            if (!NeedDoc() || !NeedLisp()) return;
            if (EditorGroup() == "COC_TIEU")
            {
                Status("Cọc được đặt tự động tại điểm đo. Chọn tim trong Bố trí tất cả báo hiệu để cập nhật hướng cọc.");
                return;
            }
            var placementRecord = _svc.GetObject(_oId.Text);
            string leaderMode = placementRecord == null ? "" : placementRecord.Get("kh_leader_mode");
            if (leaderMode == "") leaderMode = _svc.Meta("symbol_leader_mode", "GAP_KHUC");
            int modeIndex = leaderMode == "THANG" ? 0 : leaderMode == "WAYPOINT" ? 2 : 1;
            using (var options = new PlacementOptionsForm(Groups.Label(EditorGroup()) + " " + _oId.Text, modeIndex, _placementDirection, _placementAngle))
            {
                if (options.ShowDialog(this) != DialogResult.OK) return;
                if (!SaveObject(false)) return;
                _placementDirection = options.DirectionIndex; _placementAngle = options.Degrees;
                CallLisp("bht:api-sign-place", new[] { _oId.Text, options.Mode, options.Direction, options.Degrees }, "Đặt ký hiệu " + _oId.Text, null);
            }
        }
        private void SymbolForCurrent()
        {
            if (_oIsNew || _oId.Text == "") { Status("Lưu hồ sơ trước khi chèn ký hiệu."); return; }
            if (NeedLisp()) SaveObject(true);
        }

        private void AddPointIds(List<string> ids)
        {
            if (ids.Count == 0) { Status("Chọn điểm RTK trong CAD trước."); return; }
            if (_oIsNew)
            {
                foreach (var id in ids) if (!EditorPointIds().Contains(id)) _oPoints.Items.Add(id);
                UpdateObjectEditorState();
                return;
            }
            if (IsObjectEditorDirty() && !SaveObject(false)) return;
            string oid = _oId.Text;
            List<string> hits, same;
            ObjectLogic.Overlap(ids, _svc.GetObjects(), out hits, out same);
            hits.Remove(oid);
            if (hits.Count > 0 && MessageBox.Show(this, "Điểm đã thuộc hồ sơ khác: " + string.Join(", ", hits.ToArray()) + ". Vẫn thêm vào " + oid + "?",
                "BHT", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            StatusResult(AcadDispatcher.RunWrite(_doc, "Thêm điểm", db => _svc.AddPoints(oid, ids)));
            RefreshAll();
        }

        private void RemoveListPoints()
        {
            var sel = _oPoints.SelectedItems.Cast<string>().Select(s => s.Split('|')[0].Trim()).ToList();
            if (sel.Count == 0) { Status("Chọn điểm trong danh sách Điểm RTK của hồ sơ."); return; }
            if (_oIsNew) { foreach (var s in _oPoints.SelectedItems.Cast<string>().ToList()) _oPoints.Items.Remove(s); UpdateObjectEditorState(); return; }
            if (IsObjectEditorDirty() && !SaveObject(false)) return;
            string oid = _oId.Text;
            if (MessageBox.Show(this, "Gỡ " + sel.Count + " điểm khỏi " + oid + "? (điểm RTK giữ nguyên)", "BHT", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            StatusResult(AcadDispatcher.RunWrite(_doc, "Gỡ điểm", db => _svc.RemovePoints(oid, sel)));
            RefreshAll();
        }

        private void ZoomObject(string oid)
        {
            if (_svc == null || string.IsNullOrEmpty(oid)) return;
            var rec = _svc.GetObject(oid);
            double x, y, z;
            if (rec == null || !ObjectLogic.Position(rec, _svc.PointIndex(), out x, out y, out z)) { Status("Hồ sơ chưa có điểm RTK hợp lệ."); return; }
            HighlightAndZoom(null, x, y, 40.0);
        }

        // ================================================================ E. TUYEN & BAO CAO
        private TabPage BuildRoutesTab()
        {
            var tp = new TabPage("Tuyến & báo cáo") { ToolTipText = "Quản lý tuyến, mốc lý trình và báo cáo" };
            var f = new ResponsiveFlowPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(7) };
            var transfer = RouteSection("1. NẠP BÁO HIỆU VÀO BẢN ĐOẠN", "Mở DWG đoạn có tim TDT, nạp báo hiệu từ file tổng hợp. Kết quả Km cũ được giữ đối chiếu và chờ tính theo tim đích.");
            AddRouteCommandButton(transfer, "BHTNAPBAOHIEU", "Nạp khảo sát mới (tính lại lý trình)…", 365);
            f.Controls.Add(transfer);
            var progress = RouteSection("QUẢN LÝ XỬ LÝ THEO ĐOẠN", "Kế thừa hồ sơ và lý trình đã tính. Chọn đoạn/tim, tính lý trình, xác nhận rồi xuất dữ liệu tích lũy sang bản vẽ tiếp theo.");
            AddRouteCommandButton(progress, "BHTNAPTICHLUY", "Nạp dữ liệu kế thừa (.bht)…", 365);
            AddRouteCommandButton(progress, "BHTXULYDOAN", "Xử lý theo đoạn / xuất dữ liệu…", 365);
            progress.Controls.Add(Btn("Quản lý bình đồ / đồng bộ / phát hành…",(s,e)=>OpenDrawingProject()));
            f.Controls.Add(progress);
            var geometry = RouteSection("2. CHỌN TIM CỦA ĐOẠN", "Chọn loại Tim đường, Tim rãnh hoặc Tuyến khác trong hộp nạp tuyến. Sau đó xác định điểm đầu và chiều tăng lý trình.");
            var addRoute = new FlowLayoutPanel { AutoSize = true, Width = 365 };
            var source = new NoWheelComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
            source.Items.AddRange(new object[] { "Polyline thường", "Tuyến TDT 9.1" }); source.SelectedIndex = 0;
            addRoute.Controls.Add(source);
            addRoute.Controls.Add(Btn("Nạp tuyến…", (s, e) => SendCmd(source.SelectedIndex == 1 ? "BHTTUYENTDT" : "BHTTUYEN")));
            geometry.Controls.Add(addRoute);
            geometry.Controls.Add(BuildRouteData());

            AddRouteCommandButton(geometry, "BHTROUTESTART", "Chọn điểm đầu và chiều tuyến", 365);
            AddRouteCommandButton(geometry, "BHTROUTEREVERSE", "Đảo chiều tuyến", 365);
            f.Controls.Add(geometry);

            var station = RouteSection("3. XÁC NHẬN MỐC KM CỦA TIM", "Kiểm tra ít nhất hai mốc bao quanh phạm vi cần tính. Tim TDT không lấy số Km/H của cọc tiêu nhập từ hồ sơ làm mốc.");
            _tdtRouteTools = RouteSection("CỌC TDT", "Đọc cọc gốc và xác nhận mốc. Cần profile TDT 9.1; nhãn lý trình do BHT vẽ không phải mốc.");
            // The containing station section already supplies the shared inset.
            _tdtRouteTools.Padding = Padding.Empty;
            _tdtRouteTools.Margin = new Padding(0, 8, 0, 0);
            AddRouteCommandButton(_tdtRouteTools, "BHTDOCCOCTDT", "Đọc / kiểm tra cọc TDT", 365);
            _tdtRouteTools.Visible = false;
            station.Controls.Add(_tdtRouteTools);
            AddRouteCommandButton(station, "BHTMOCKM", "Thêm mốc bổ sung / điểm gãy Km", 365);
            AddRouteCommandButton(station, "BHTDSMOC", "Bảng mốc: xem vị trí / xóa mốc…", 365);
            f.Controls.Add(station);

            var actions = RouteSection("4. TÍNH LẠI VÀ KIỂM TRA", "Tính theo tim đang chọn, xem số hồ sơ tính được trước khi áp dụng. Dùng DIM để kiểm tra lệch tim tại trụ gần nhất.");
            AddRouteCommandButton(actions, "BHTTINHTHEOTUYEN", "Tính lại theo tim đang chọn…", 365);
            var routeDiag = Btn("Kiểm tra tim tuyến", (s, e) => CallLisp("bht:api-route-diag", new[] { routeList.SelectedItems.Count == 0 ? "" : routeList.SelectedItems[0].Text }, "Chẩn đoán tuyến", r =>
            {
                if (r.Ok) ShowReportDialog("Kết quả kiểm tra tim tuyến", string.Join("\r\n", r.Values.ToArray()));
            }));
            routeDiag.Width = 365; routeDiag.TextAlign = ContentAlignment.MiddleLeft; actions.Controls.Add(routeDiag);
            AddRouteCommandButton(actions, "BHTDIMTIM", "DIM từ điểm trụ gần nhất tới tim", 365);
            f.Controls.Add(actions);

            var presentation = RouteSection("5. TRÌNH BÀY BẢN VẼ", "Sau khi kiểm tra lý trình, chỉnh hướng biển và sắp xếp nhãn.");
            AddRouteCommandButton(presentation, "BHTHUONGBIEN", "Xoay biển vuông góc với hướng…", 365);
            AddRouteCommandButton(presentation, "BHTSAPNHAN", "Sắp xếp nhãn trên bản vẽ", 365);
            AddRouteCommandButton(presentation, "BHTMAUNHAN", "Áp dụng mẫu nhãn báo hiệu…", 365);
            f.Controls.Add(presentation);
            var reports = RouteSection("6. KIỂM TRA VÀ XUẤT HỒ SƠ", "Xuất dữ liệu đã lưu. Tính lại và kiểm tra các hồ sơ chưa xác định trước khi xuất.");
            var export = Btn("Xuất báo cáo biển báo Excel", (s, e) => ExportSignReport());
            export.Width = 365; export.TextAlign = ContentAlignment.MiddleLeft; reports.Controls.Add(export);
            var check = Btn("Kiểm tra bản vẽ", (s,e) => CheckDrawing());
            check.Width = 365; check.TextAlign = ContentAlignment.MiddleLeft; reports.Controls.Add(check);
            AddRouteCommandButton(reports, "BHTXUAT", "Xuất dữ liệu kỹ thuật CSV", 365);
            AddRouteCommandButton(reports, "BHTXUATDUAN", "Xuất dự án BHT (.bht)…", 365);
            f.Controls.Add(reports);

            var alternatives = RouteSection("DỰ ÁN NGUYÊN BẢN / TÍNH THEO CỌC", "Nạp nguyên dự án giữ cả tim và lý trình nguồn. Tính theo cọc thực địa dùng khi không có tim; đây là phương pháp riêng.");
            AddRouteCommandButton(alternatives, "BHTNAPDUAN", "Nạp nguyên dự án BHT (.bht / DWG cũ)…", 365);
            AddRouteCommandButton(alternatives, "BHTKM2COC", "Kiểm tra 2 cọc Km (không có tuyến)", 365);
            alternatives.Controls.Add(BuildFieldData());
            AddOptionalRouteSection(f, "Phương pháp theo cọc / nạp nguyên dự án", alternatives);

            var packages = RouteSection("ĐOẠN / GÓI THẦU", "Tùy chọn cho công trình có phân chia đoạn hoặc gói thầu. Công trình không phân chia có thể bỏ qua; vẫn tính lý trình và xuất báo cáo bình thường.");
            var loadSegments = Btn("Nạp Excel / bảng BHT (.bhtseg)…", (s, e) => OpenSegments(true));
            loadSegments.Width = 365; packages.Controls.Add(loadSegments);
            var editSegments = Btn("Xem đoạn / gán tim đường, tim rãnh…", (s, e) => OpenSegments(false));
            editSegments.Width = 365; packages.Controls.Add(editSegments);
            var template = Btn("Lưu bảng Excel mẫu…", (s, e) => SavePackageTemplate());
            template.Width = 365; packages.Controls.Add(template);
            AddRouteCommandButton(packages, "BHTPHANDOAN", "Gán đoạn / gói theo tim của đoạn", 365);
            AddRouteCommandButton(packages, "BHTGANDOAN", "Chọn đoạn / gói cho hồ sơ…", 365);
            AddOptionalRouteSection(f, "Phân chia đoạn / gói thầu (tùy chọn)", packages);

            var maintenance = RouteSection("MẪU BIỂN & HIỂN THỊ", "Cỡ dấu X và nhãn nằm ở Điểm vị trí. Ghép ảnh và thư mục ảnh nằm ở Ảnh hiện trường. Nội dung biển được chỉnh trong thư viện.");
            var upgrade = Btn("Cập nhật mẫu biển đã vẽ", (s, e) => CallLisp("bht:api-symbol-upgrade", new string[0], "Cập nhật mẫu biển", r =>
            {
                if (r.Ok) ShowReportDialog("Cập nhật mẫu biển", string.Join("\r\n", r.Values.ToArray()));
            }));
            upgrade.Width = 365; maintenance.Controls.Add(upgrade);
            var distance = Btn("Bố trí tất cả báo hiệu…", (s,e) => ChooseSymbolDistance());
            distance.Width=365; maintenance.Controls.Add(distance);
            AddRouteCommandButton(maintenance, "BHTCHONBAOHIEU", "Chọn tất cả báo hiệu trên CAD", 365);
            AddRouteCommandButton(maintenance, "BHTCHONTUONGTU", "Chọn báo hiệu cùng nhóm…", 365);
            AddRouteCommandButton(maintenance, "BHTSELECTSIMILAR", "SELECTSIMILAR theo lớp…", 365);
            _objectTips.SetToolTip(upgrade, "Áp dụng phông và bỏ điểm dựng hình thừa cho các biển BHT đã vẽ. Giữ vị trí, góc và tỷ lệ; không tự chạy khi mở bản vẽ.");
            AddRouteCommandButton(maintenance, "BHTTHUTUVE", "Sửa thứ tự hiển thị", 365);
            AddRouteCommandButton(maintenance, "BHTBLOCK", "Gán block mặc định cho nhóm…", 365);
            AddOptionalRouteSection(f, "Công cụ bổ sung", maintenance);
            tp.Controls.Add(f);
            UpdateRouteTools();
            return tp;
        }

        private Control _tdtRouteTools;
        private readonly List<Button> _selectedRouteActions = new List<Button>();

        private void UpdateRouteTools()
        {
            if(refreshingRoutes)return;
            bool selected = routeList != null && routeList.SelectedItems.Count == 1;
            foreach (var button in _selectedRouteActions) button.Enabled = selected;
            if (_tdtRouteTools == null) return;
            _tdtRouteTools.Visible = routeList != null && routeList.SelectedItems.Count == 1
                && routeList.SelectedItems[0].SubItems.Count>2
                && routeList.SelectedItems[0].SubItems[2].Text.IndexOf("TDT", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void AddOptionalRouteSection(Control parent, string title, Control section)
        {
            section.Visible = false;
            var toggle = new CheckBox { Text = title, AutoSize = true, Margin = new Padding(3, 10, 3, 3) };
            toggle.CheckedChanged += (s, e) =>
            {
                parent.SuspendLayout();
                try { section.Visible = toggle.Checked; }
                finally { parent.ResumeLayout(true); }
                // Refit after VisibleChanged has completed, including the scrollbar.
                // Otherwise a section expanded at the bottom keeps its collapsed bounds.
                Action reveal = () =>
                {
                    if (parent.IsDisposed || section.IsDisposed) return;
                    section.PerformLayout(); parent.PerformLayout();
                    var page = parent as ResponsiveFlowPanel;
                    if (toggle.Checked && page != null) page.RevealControl(section);
                    parent.Invalidate(true);
                };
                reveal();
                if (parent.IsHandleCreated) parent.BeginInvoke(reveal);
            };
            parent.Controls.Add(toggle); parent.Controls.Add(section);
        }

        private void SavePackageTemplate()
        {
            using (var dialog = new SaveFileDialog { Title = "Lưu bảng phân chia đoạn / gói thầu mẫu", Filter = "Excel (*.xlsx)|*.xlsx", DefaultExt = "xlsx", AddExtension = true, FileName = "BHT_GOI_THAU.xlsx" })
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                try {
                    SegmentTable.WriteTemplate(dialog.FileName);
                    ShowReportDialog("Bảng gói thầu mẫu", "Đã lưu: " + dialog.FileName + "\r\n\r\nSửa dữ liệu bằng Excel rồi nạp trực tiếp .xlsx. BHT cho xem trước và chuyển thành .bhtseg.\r\nKM_START_M / KM_END_M: mét hoặc Km40+500. SIDE: TRAI, PHAI, HAI_BEN.\r\nROUTE_ID: mã tim đã khai báo; để trống nếu dùng lý trình hiện có. Có thể gán tim trong bảng BHT sau khi nạp.");
                } catch (Exception ex) { StatusError("Không lưu được bảng mẫu: " + ex.Message); }
            }
        }

        private FlowLayoutPanel RouteSection(string title, string description)
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Width = 380,
                Padding = new Padding(10), Margin = new Padding(0, 8, 0, 10), Tag = "route-section", BackColor = PaletteTheme.GreenLight
            };
            panel.Controls.Add(new Label { Text = title, AutoSize = true, ForeColor = PaletteTheme.GreenDark, Font = new Font("Segoe UI Semibold", 10f) });
            panel.Controls.Add(new Label { Text = description, AutoSize = true, MaximumSize = new Size(360, 0), ForeColor = Color.FromArgb(65, 80, 88), Margin = new Padding(3, 4, 3, 10) });
            return panel;
        }

        private void AddRouteCommandButton(Control parent, string command, string text, int width)
        {
            string cmd = command;
            bool requiresRoute = new[] { "BHTROUTESTART", "BHTROUTEREVERSE", "BHTDOCCOCTDT", "BHTMOCKM", "BHTDSMOC", "BHTTINHTHEOTUYEN" }.Contains(cmd);
            var button = Btn(text, (s, e) => { if (requiresRoute) SelectedRouteCommand(cmd); else SendCmd(cmd); });
            if (requiresRoute) { _selectedRouteActions.Add(button); button.Enabled = false; }
            button.Width = width;
            button.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(button);
        }

        private void ExportSignReport()
        {
            if (!NeedDoc()) return;
            string project = Path.GetFileNameWithoutExtension(SafeName(_doc));
            var rows = SignReportRecords.Build(_svc.GetObjects(), project, code =>
            {
                var sign = TdtSignLibrary.Find(code);
                return sign == null ? null : new SignReportRow { SignGroup = sign.Group, Description = sign.Description };
            });
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Xuất báo cáo biển báo BHT";
                dialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                dialog.DefaultExt = "xlsx";
                dialog.AddExtension = true;
                dialog.FileName = "BHT_BAO_CAO_BIEN_BAO_" + (project == "" ? "BAN_VE" : project) + ".xlsx";
                string folder = CurrentDwgPrefix();
                if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder)) dialog.InitialDirectory = folder;
                if (dialog.ShowDialog() != DialogResult.OK) return;
                try
                {
                    SignReportWorkbook.Write(dialog.FileName, project, rows);
                    Status("Đã xuất " + rows.Count + " mặt biển / " + SignReportWorkbook.InstallationCount(rows) + " hồ sơ: " + dialog.FileName);
                    ShowReportDialog("Xuất báo cáo biển báo",
                        "Đã tạo tệp Excel UTF-8/Unicode:\r\n" + dialog.FileName
                        + "\r\n\r\nSố hồ sơ/cụm biển: " + SignReportWorkbook.InstallationCount(rows) + "; số mặt biển (gồm biển phụ): " + rows.Count
                        + "\r\nTrang Tổng hợp: biển báo, bảng chỉ dẫn, bảng quảng cáo, biển khác và chưa xác định; đếm từng mặt theo nhóm, mã, tên, nội dung và tình trạng."
                        + "\r\nTrang Danh sách biển: mỗi mặt một dòng, giữ chung ID hồ sơ, lý trình và tim của cụm; trụ và tổng số mặt chỉ ghi ở dòng đầu để tránh cộng trùng."
                        + "\r\nLệch tim tuyến (Offset) là khoảng cách mặt bằng từ vị trí hồ sơ tới tim dùng để tính lý trình, không phải vị trí ký hiệu trình bày.");
                }
                catch (Exception ex) { StatusError("Lỗi xuất Excel: " + ex.Message); }
            }
        }

        private static double ParseDouble(string value)
        {
            double number;
            return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out number)
                ? number : double.MaxValue;
        }

        private static string DisplaySide(string value)
        {
            switch ((value ?? "").ToUpperInvariant())
            {
                case "TRAI": return "Trái";
                case "PHAI": return "Phải";
                case "HAI_BEN": return "Hai bên";
                case "TREN_TUYEN": return "Trên tuyến";
                default: return "Chưa xác định";
            }
        }
    }
}
