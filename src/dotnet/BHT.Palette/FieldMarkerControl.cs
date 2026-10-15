using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Font = System.Drawing.Font;
using FlowDirection = System.Windows.Forms.FlowDirection;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using BHT.Bridge;
using BHT.Core;

namespace BHT.Palette
{
    internal sealed class FieldMarkerChoice
    {
        public string Id, Label;
        public double Station, X, Y;
        public override string ToString() { return Label; }
    }

    // The right-hand list is an explicit user-approved order, never an inferred alignment.
    internal sealed class FieldChainForm : Form
    {
        private readonly ListBox available = new ListBox(), ordered = new ListBox();
        private readonly TextBox chainId = new TextBox();
        private readonly NumericUpDown offset = new NumericUpDown(), span = new NumericUpDown(), tolerance = new NumericUpDown();
        private readonly TextBox review = new TextBox();
        private readonly bool pairMode;
        public string ChainId { get { return chainId.Text.Trim().ToUpperInvariant(); } }
        public string[] MarkerIds { get { return ordered.Items.Cast<FieldMarkerChoice>().Select(x => x.Id).ToArray(); } }
        public string MaxOffset { get { return offset.Value.ToString(CultureInfo.InvariantCulture); } }
        public string MaxSpan { get { return span.Value.ToString(CultureInfo.InvariantCulture); } }
        public string RatioTolerance { get { return (tolerance.Value / 100m).ToString(CultureInfo.InvariantCulture); } }
        private static decimal Number(BhtRecord rec, string key, decimal fallback)
        { decimal n; return rec != null && decimal.TryParse(rec.Get(key), NumberStyles.Float, CultureInfo.InvariantCulture, out n) ? n : fallback; }
        public FieldChainForm(List<FieldMarkerChoice> markers, BhtRecord existing, bool pair)
        {
            pairMode = pair;
            Text = pair ? "Chọn hai mốc theo chiều Km tăng" : "Nhóm cọc theo đường / nhánh";
            Size = new Size(940, 670); MinimumSize = new Size(780, 570); StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f); AutoScaleMode = AutoScaleMode.Dpi;
            var main = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 1, RowCount = 6 };
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize)); main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 130)); main.RowStyles.Add(new RowStyle(SizeType.AutoSize)); Controls.Add(main);
            main.Controls.Add(new Label { AutoSize = true, MaximumSize = new Size(860, 0), Text = "Dùng cột Km hoặc cọc tiêu có số và điểm đo, hoặc mốc thủ công đã xác nhận. Thêm cọc cùng đường/nhánh vào danh sách bên phải, xếp theo thứ tự thực địa và chiều Km tăng. Kiểm tra các mốc nằm giữa trước khi chọn cặp riêng. Mốc khai báo theo tim tuyến không tham gia." });
            var naming = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            naming.Controls.Add(new Label { Text = "Tên nhóm", AutoSize = true, Padding = new Padding(0, 5, 0, 0) });
            chainId.Width = 240; chainId.Text = existing == null ? "COC1" : existing.Get("chain_id"); chainId.ReadOnly = existing != null;
            naming.Controls.Add(chainId); naming.Visible = !pair; main.Controls.Add(naming);
            var lists = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 };
            lists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); lists.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95)); lists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            lists.RowStyles.Add(new RowStyle(SizeType.AutoSize)); lists.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lists.Controls.Add(new Label { Text = "Mốc có thể chọn", AutoSize = true }, 0, 0); lists.Controls.Add(new Label { Text = pair ? "Mốc đầu → mốc cuối" : "Thứ tự mốc trong nhóm", AutoSize = true }, 2, 0);
            available.Dock = ordered.Dock = DockStyle.Fill; available.HorizontalScrollbar = ordered.HorizontalScrollbar = true;
            available.SelectionMode = SelectionMode.MultiExtended; lists.Controls.Add(available, 0, 1); lists.Controls.Add(ordered, 2, 1);
            foreach (var m in markers.OrderBy(x => x.Station).ThenBy(x => x.Id)) available.Items.Add(m);
            if (existing != null) foreach (var id in existing.GetAll("marker")) { var item = markers.FirstOrDefault(x => x.Id == id); ordered.Items.Add(item ?? new FieldMarkerChoice { Id = id, Label = id + " — THIẾU DỮ LIỆU", Station = double.NaN }); }
            var moves = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
            Action<string, EventHandler> button = (text, click) => { var b = new Button { Text = text, Width = 85, Height = 32 }; b.Click += click; moves.Controls.Add(b); };
            button("Thêm →", (s,e) => { foreach (FieldMarkerChoice m in available.SelectedItems) if (!ordered.Items.Cast<FieldMarkerChoice>().Any(x => x.Id == m.Id) && (!pairMode || ordered.Items.Count < 2)) ordered.Items.Add(m); Review(); });
            button("Bỏ cọc", (s,e) => { if (ordered.SelectedIndex >= 0) ordered.Items.RemoveAt(ordered.SelectedIndex); Review(); });
            button("Lên", (s,e) => MoveMarker(-1)); button("Xuống", (s,e) => MoveMarker(1)); lists.Controls.Add(moves, 1, 1); main.Controls.Add(lists);
            var settings = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            Configure(settings, "Phạm vi lệch (m)", offset, Number(existing, "max_offset", 100), 10000);
            Configure(settings, "Cảnh báo đoạn dài (m)", span, Number(existing, "max_span", 500), 100000);
            Configure(settings, "Cảnh báo hệ số (%)", tolerance, Number(existing, "ratio_tol", .03m) * 100m, 99);
            offset.Enabled = span.Enabled = tolerance.Enabled = !pair; main.Controls.Add(settings);
            review.Dock = DockStyle.Fill; review.Multiline = true; review.ReadOnly = true; review.ScrollBars = ScrollBars.Vertical; main.Controls.Add(review);
            foreach (var n in new[] {offset,span,tolerance}) n.ValueChanged += (s,e) => Review();
            var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Width = 85, Height = 32 };
            var save = new Button { Text = pair ? "Tính từ 2 cọc" : "Lưu và dùng nhóm", AutoSize = true, Height = 32 };
            save.Click += (s,e) => {
                if (ordered.Items.Count < 2 || (pairMode && ordered.Items.Count != 2) || ordered.Items.Cast<FieldMarkerChoice>().Any(x => double.IsNaN(x.Station))) { MessageBox.Show(this,"Chọn cọc đủ Km/H và vị trí; cọc thủ công cần xác nhận trước."); return; }
                if (!pair && (ChainId.Length == 0 || ChainId.IndexOfAny(new[] {' ', ',', ';', '|', '='}) >= 0)) { MessageBox.Show(this,"ID chuỗi không chứa dấu cách hoặc dấu phân cách."); return; }
                DialogResult = DialogResult.OK; Close();
            };
            actions.Controls.Add(cancel); actions.Controls.Add(save); main.Controls.Add(actions); CancelButton = cancel; AcceptButton = save; Review();
        }
        private void Configure(FlowLayoutPanel p, string label, NumericUpDown n, decimal value, decimal max)
        { p.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0,5,0,0) }); n.Minimum = .01m; n.Maximum = max; n.DecimalPlaces = 2; n.Value = Math.Max(n.Minimum,Math.Min(max,value)); n.Width=75; p.Controls.Add(n); }
        private void MoveMarker(int delta) { int i=ordered.SelectedIndex,j=i+delta;if(i<0||j<0||j>=ordered.Items.Count)return;var m=ordered.Items[i];ordered.Items.RemoveAt(i);ordered.Items.Insert(j,m);ordered.SelectedIndex=j;Review(); }
        private void Review()
        {
            var lines = new List<string>(); var m=ordered.Items.Cast<FieldMarkerChoice>().ToList();
            for(int i=1;i<m.Count;i++) { var a=m[i-1];var b=m[i];double d=Math.Sqrt(Math.Pow(b.X-a.X,2)+Math.Pow(b.Y-a.Y,2));double ds=b.Station-a.Station;
                string status=double.IsNaN(ds)||d<.000001||ds<=.001 ? "LỖI: kiểm tra cọc/thứ tự" : d>(double)span.Value || Math.Abs(ds/d-1)>(double)tolerance.Value/100 ? "CẦN KIỂM TRA" : "Nội suy";
                lines.Add(a.Id+" → "+b.Id+": dài "+d.ToString("0.00")+" m / chênh lý trình "+ds.ToString("0.00")+" m — "+status); }
            review.Text=lines.Count==0 ? "Chọn ít nhất hai cọc. Các đoạn thiếu cọc, đường cong hoặc sai số Km/H cần đối chiếu thực địa. Đoạn lỗi không làm dừng các đoạn độc lập khác." : string.Join("\r\n",lines.ToArray());
        }
    }

    public partial class BhtPaletteControl
    {
        private ComboBox fieldChains;
        private Label fieldMode;
        private List<FieldMarkerChoice> FieldChoices()
        {
            var points = _svc.GetPoints().GroupBy(p => p.IdUpper).ToDictionary(g=>g.Key,g=>g.First());
            var result = new List<FieldMarkerChoice>();
            foreach(var pair in _svc.GetObjects()) { var r=pair.Value;double metres;
                if(!MarkerStation.TryMetres(r.Get("nhom"),r.Get("marker_km"),r.Get("marker_h"),out metres))continue;
                var ids=r.GetAll("pt").ToList(); if(ids.Count==0 || ids.Any(id=>!points.ContainsKey(id.ToUpperInvariant())))continue;
                var pp=ids.Select(id=>points[id.ToUpperInvariant()]).ToList();
                bool manual=r.Get("position_source")=="MANUAL_CAD" || r.Get("station_control_eligible")=="0" || pp.Any(p=>p.IsManual);
                if(manual&&!MarkerStation.ManualControlApproved(r,pp))continue;
                result.Add(new FieldMarkerChoice {Id=pair.Key,Station=metres,X=pp.Average(p=>p.X),Y=pp.Average(p=>p.Y),Label=pair.Key+" | "+(r.Get("nhom")=="COT_KM"?"Cột Km":"Cọc tiêu")+" | "+Chainage.Format(metres)+" | "+string.Join(", ",ids.ToArray())+(manual?" | THỦ CÔNG đã xác nhận":"")});
            } return result;
        }
        private void ReviewImportedMarkers(Action after)
        {
            if(!NeedDoc())return;var expected=_doc;
            CallLisp("bht:api-field-input-preview",new string[0],"Đọc cọc đã nhập",r=>{
                if(!r.Ok||_doc!=expected)return;
                using(var form=new ImportedMarkersForm(r.Values)) {
                    var decision=form.ShowDialog(this);if(_doc!=expected)return;
                    if(decision==DialogResult.Retry) {
                        CallLisp("bht:api-field-manual-control",new[]{form.ManualId,form.ManualValue},"Cập nhật mốc thủ công",saved=>{if(saved.Ok&&_doc==expected)ReviewImportedMarkers(after);});return;
                    }
                    if(decision!=DialogResult.OK)return;
                    CallLisp("bht:api-field-input-use",new[]{form.Snapshot},"Dùng cọc đã nhập",saved=>{if(saved.Ok&&_doc==expected&&after!=null)after();});
                }
            });
        }
        private bool NeedsImportedMarkerReview(string oid)
        {
            if(_svc==null)return false;
            var rec=_svc.GetObject(oid);if(rec==null)return false;
            if(rec.Get("field_pair_a")!=""||rec.Get("field_chain_id")!=""||_svc.Meta("field_active_chain","")!="")return false;
            using(var tr=_doc.Database.TransactionManager.StartOpenCloseTransaction())return BhtStore.Keys(tr,_doc.Database,"ROUTE").Count==0;
        }
        internal static bool NeedsBatchMarkerReview(string method, string activeChain, bool hasRoutes)
        {
            return method == "FIELD_MARKERS" ? string.IsNullOrEmpty(activeChain) : !hasRoutes;
        }

        private void CalculateAllStations()
        {
            if (!NeedDoc() || !NeedLisp()) return;
            bool hasRoutes;
            using (var tr = _doc.Database.TransactionManager.StartOpenCloseTransaction())
                hasRoutes = BhtStore.Keys(tr, _doc.Database, "ROUTE").Count > 0;
            if (NeedsBatchMarkerReview(_svc.Meta("station_method", "ROUTE"), _svc.Meta("field_active_chain", ""), hasRoutes))
                ReviewImportedMarkers(() => SendCmd("BHTLYTRINH"));
            else SendCmd("BHTLYTRINH");
        }

        private Control BuildFieldData()
        {
            var panel=RouteSection("LÝ TRÌNH TOÀN BỘ HỒ SƠ", "Tính theo phương pháp đang hiển thị bên dưới. Mốc thực địa gồm cả cột Km và cọc tiêu.");
            fieldMode=new Label {AutoSize=true,MaximumSize=new Size(360,0)};panel.Controls.Add(fieldMode);
            panel.Controls.Add(Btn("Mốc lý trình đã nhập…",(s,e)=>ReviewImportedMarkers(null)));
            panel.Controls.Add(Btn("Tính lý trình tất cả hồ sơ", (s,e) => CalculateAllStations()));
            panel.Controls.Add(Btn("Xoay các biển vuông góc theo cọc",(s,e)=>CallLisp("bht:api-field-heading",new[]{"ALL"},"Hướng biển theo cọc",null)));
            var advanced=new FlowLayoutPanel{AutoSize=true,Width=365,FlowDirection=FlowDirection.TopDown,WrapContents=false,Visible=false};
            var toggle=Btn("Nâng cao: nhóm đường / nhánh ▸",(s,e)=>{advanced.Visible=!advanced.Visible;});panel.Controls.Add(toggle);
            advanced.Controls.Add(new Label{Text="Dùng khi có nhiều đường/nhánh. Nhóm/cặp riêng đã gán cho hồ sơ vẫn được giữ khi đổi danh sách mặc định.",AutoSize=true,MaximumSize=new Size(355,0)});
            fieldChains=new NoWheelComboBox {Width=355,DropDownStyle=ComboBoxStyle.DropDownList};advanced.Controls.Add(fieldChains);
            var row=new FlowLayoutPanel {AutoSize=true,Width=355};
            row.Controls.Add(Btn("Tạo nhóm…",(s,e)=>EditFieldChain(false)));row.Controls.Add(Btn("Sửa nhóm…",(s,e)=>EditFieldChain(true)));
            row.Controls.Add(Btn("Xóa nhóm",(s,e)=> {if(fieldChains.SelectedItem==null)return;string id=Convert.ToString(fieldChains.SelectedItem);if(MessageBox.Show(this,"Xóa khai báo "+id+"? Giữ cọc và RTK.","BHT",MessageBoxButtons.OKCancel)!=DialogResult.OK)return;CallLisp("bht:api-field-delete",new[]{id},"Xóa nhóm cọc",null);}));advanced.Controls.Add(row);
            advanced.Controls.Add(Btn("Dùng nhóm đang chọn",(s,e)=> {if(fieldChains.SelectedItem==null){StatusWarn("Chọn nhóm cọc trước.");return;}CallLisp("bht:api-field-mode",new[]{"FIELD_MARKERS",Convert.ToString(fieldChains.SelectedItem)},"Dùng nhóm cọc",null);}));
            advanced.Controls.Add(Btn("Gán hồ sơ đang chọn vào nhóm",(s,e)=>UseFieldChainForObject()));
            advanced.Controls.Add(Btn("Chuyển về tính theo tim tuyến",(s,e)=>CallLisp("bht:api-field-mode",new[]{"ROUTE",""},"Dùng tim tuyến",null)));
            panel.Controls.Add(advanced);return panel;
        }
        private void RefreshFieldData()
        {
            if(fieldChains==null)return;var previous=fieldChains.SelectedItem;fieldChains.Items.Clear();if(_doc==null){fieldMode.Text="Chưa có bản vẽ.";return;}
            using(var tr=_doc.Database.TransactionManager.StartOpenCloseTransaction()) {
                string active=BhtStore.Meta(tr,_doc.Database,"field_active_chain","");string mode=BhtStore.Meta(tr,_doc.Database,"station_method","ROUTE");
                foreach(var row in BhtStore.All(tr,_doc.Database,"FIELD_CHAIN"))if(row.Key!="BHT_IMPORTED_MARKERS")fieldChains.Items.Add(row.Key);
                if(fieldChains.Items.Contains(active))fieldChains.SelectedItem=active;
                else if(previous!=null&&fieldChains.Items.Contains(previous))fieldChains.SelectedItem=previous;
                string route=BhtStore.Meta(tr,_doc.Database,"station_active_route","");
                fieldMode.Text=mode=="FIELD_MARKERS" ? "Đang tính: cột Km và cọc tiêu. Nguồn mặc định: "+(active=="BHT_IMPORTED_MARKERS"?"cọc đã nhập":active==""?"chưa chọn (dùng cặp riêng)":active) : "Đang tính: tim tuyến và mốc tuyến."+(route==""?"":" Tim: "+route+".");
            }
        }
        private void EditFieldChain(bool edit)
        {
            if(!NeedDoc())return;var doc=_doc;BhtRecord rec=null;
            if(edit) {if(fieldChains.SelectedItem==null){StatusWarn("Chọn chuỗi cần sửa.");return;}using(var tr=doc.Database.TransactionManager.StartOpenCloseTransaction())rec=BhtStore.Read(tr,doc.Database,"FIELD_CHAIN",Convert.ToString(fieldChains.SelectedItem));}
            using(var form=new FieldChainForm(FieldChoices(),rec,false)) {
                if(form.ShowDialog(this)!=DialogResult.OK || _doc!=doc)return;
                if(!edit)using(var tr=doc.Database.TransactionManager.StartOpenCloseTransaction())if(BhtStore.Read(tr,doc.Database,"FIELD_CHAIN",form.ChainId)!=null){StatusWarn("ID chuỗi đã tồn tại. Chọn Sửa chuỗi.");return;}
                CallLisp("bht:api-field-save",new[]{form.ChainId,form.ChainId,string.Join(" ",form.MarkerIds),form.MaxOffset,form.MaxSpan,form.RatioTolerance},"Lưu chuỗi cọc",null);
            }
        }
        private void ChooseFieldPair()
        {
            if(!NeedDoc()||_oIsNew||string.IsNullOrEmpty(_oId.Text)){StatusWarn("Lưu hồ sơ trước khi chọn cọc.");return;}
            var doc=_doc;string oid=_oId.Text;
            using(var form=new FieldChainForm(FieldChoices(),null,true)) {
                if(form.ShowDialog(this)!=DialogResult.OK||_doc!=doc)return;
                var ids=form.MarkerIds;CallLisp("bht:api-field-pair",new[]{oid,ids[0],ids[1]},"Tính từ hai cọc",r=>{if(r.Ok)ShowReportDialog("Lý trình theo cọc",string.Join("\r\n",r.Values.ToArray()));});
            }
        }
        private void UseFieldChainForObject()
        {
            if(_oIsNew||string.IsNullOrEmpty(_oId.Text)){StatusWarn("Chọn hồ sơ đã lưu trước.");return;}
            if(fieldChains.SelectedItem==null){StatusWarn("Chọn nhóm cọc trước.");return;}
            string oid=_oId.Text,cid=Convert.ToString(fieldChains.SelectedItem);
            CallLisp("bht:api-field-mode",new[]{"FIELD_MARKERS",cid},"Dùng nhóm cọc",mode=>{
                if(mode.Ok)CallLisp("bht:api-field-auto",new[]{oid},"Tính theo nhóm đang chọn",r=>{if(r.Ok)ShowReportDialog("Lý trình theo nhóm",string.Join("\r\n",r.Values.ToArray()));});
            });
        }
    }
}
