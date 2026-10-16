using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BHT.Palette;
using BHT.Bridge;
using BHT.Core;

using Autodesk.AutoCAD.Runtime;

public class PaletteLayoutProbe
{
    static int checks;
    static System.Exception uiError;
    static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    static object Field(object target, string name) { return target.GetType().GetField(name, Hidden).GetValue(target); }
    static void Set(object target, string name, object value) { target.GetType().GetField(name, Hidden).SetValue(target, value); }
    static object Call(object target, string name, params object[] args) { return target.GetType().GetMethods(Hidden).Single(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(target, args); }
    static void Check(string name, bool valid) { if (!valid) throw new System.Exception(name); checks++; Console.WriteLine("PASS " + name); }
    static System.Collections.Generic.IEnumerable<Control> Children(Control root)
    {
        foreach (Control child in root.Controls) { yield return child; foreach (var nested in Children(child)) yield return nested; }
    }

    [CommandMethod("BHTPALETTELAYOUTPROBE")] public static void Run()
    {
        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException+=(s,e)=>{uiError=e.Exception;foreach(var form in Application.OpenForms.Cast<Form>().ToArray())form.Close();};
        string output = Path.Combine(Path.GetDirectoryName(typeof(PaletteLayoutProbe).Assembly.Location), "ui-preview");
        checks = 0;
        try { Test(output); if(uiError!=null)throw uiError; File.WriteAllText(Path.Combine(output, "result.txt"), "PASS palette layout and editor workflow (" + checks + " checks)"); }
        catch (System.Exception ex) { Directory.CreateDirectory(output); File.WriteAllText(Path.Combine(output, "result.txt"), "FAIL " + (uiError ?? ex)); }
    }

    static void TestConflicts128(string output)
    {
        var items=new System.Collections.Generic.List<ImportConflict> {
            new ImportConflict("Hồ sơ","OBJ-000116","Khác nội dung khảo sát","mat","1. W.207a\r\n2. S.509a","1. W.207b\r\n2. S.509b"),
            new ImportConflict("Điểm","P-002","Cùng mã, khác tọa độ","X","123.456789","123.456790"),
            new ImportConflict("Đoạn","D10","Khác phạm vi","km_end_m","51634.24","51634.25")};
        var type=typeof(BhtPaletteControl).Assembly.GetType("BHT.Palette.ImportConflictForm");
        using(var form=(Form)Activator.CreateInstance(type,new object[]{items})) {
            form.Show();Application.DoEvents();
            var grid=(DataGridView)Field(form,"grid");var search=(TextBox)Field(form,"search");var detail=(TextBox)Field(form,"detail");
            Check("128-conflict-table-all-rows",grid.Rows.Count==3&&grid.Columns.Count==6);
            Check("128-conflict-table-readonly",grid.ReadOnly&&!grid.AllowUserToAddRows&&!grid.AllowUserToDeleteRows);
            Check("128-conflict-detail-all-faces",detail.Text.Contains("S.509a")&&detail.Text.Contains("S.509b"));
            search.Text="OBJ-000116";Application.DoEvents();Check("128-conflict-id-filter",grid.Rows.Count==1);
            search.Text="not-found";Application.DoEvents();Check("128-conflict-empty-filter-clears-detail",grid.Rows.Count==0&&detail.Text=="");
            search.Clear();Application.DoEvents();Check("128-conflict-clear-restores-all",grid.Rows.Count==3);
            foreach(var size in new[]{new Size(760,520),new Size(1120,720)}) {
                form.Size=size;Application.DoEvents();
                foreach(var button in Children(form).OfType<Button>())Check("128-conflict-button-visible-"+size.Width+button.Text,form.ClientRectangle.Contains(form.PointToClient(button.PointToScreen(new Point(button.Width-1,button.Height-1)))));
            }
            using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));bitmap.Save(Path.Combine(output,"import-conflicts-128.png"));}
            Children(form).OfType<Button>().Single(b=>b.Text=="Đóng").PerformClick();Check("128-conflict-close",!form.Visible);
        }
    }

    static void TestReadableReport(string output)
    {
        var assembly=typeof(BhtPaletteControl).Assembly;
        var formatter=assembly.GetType("BHT.Palette.CheckReportText").GetMethod("Format");
        var values=new System.Collections.Generic.List<string>{"loi=1","canh_bao=2","ĐIỂM VỊ TRÍ","LỖI: điểm BOT2-R-000027 đã thay đổi vị trí so với tọa độ lúc nhập. Đối chiếu số liệu gốc trước khi dùng điểm này.","Điểm đo RTK: 790 | Điểm bổ sung thủ công: 5","CẢNH BÁO: OBJ-000015 chưa đủ điểm vị trí cho 2 trụ. Mở hồ sơ, gắn thêm điểm chân trụ để vẽ đúng các đường nối.","KÝ HIỆU ẢNH","Ảnh có ký hiệu trên bản vẽ: 18 | Ký hiệu không tìm thấy hồ sơ ảnh: 0","CẢNH BÁO: 1 ảnh có GPS hợp lệ nhưng chưa có ký hiệu trên bản vẽ. Mở tab Ảnh hiện trường → Cập nhật ký hiệu ảnh."};
        string formatted=(string)formatter.Invoke(null,new object[]{values});
        Check("report-human-summary",formatted.Contains("Lỗi cần xử lý: 1")&&formatted.Contains("Mục cần kiểm tra: 2")&&!formatted.Contains("loi="));
        Check("report-actions-before-statistics",formatted.IndexOf("BOT2-R-000027")<formatted.IndexOf("SỐ LIỆU ĐÃ KIỂM TRA"));
        Check("report-keeps-IDs-and-instructions",formatted.Contains("OBJ-000015")&&formatted.Contains("Cập nhật ký hiệu ảnh"));
        Check("report-separates-long-statistics",!formatted.Contains(" | "));
        string clean=(string)formatter.Invoke(null,new object[]{new System.Collections.Generic.List<string>{"loi=0","canh_bao=0","Điểm đo RTK: 0"}});
        Check("report-clean-result-no-false-guarantee",clean.Contains("Chưa phát hiện lỗi trong các mục đã kiểm tra."));
        var dialogType=assembly.GetType("BHT.Palette.ReportDialog");
        using(var dialog=(Form)Activator.CreateInstance(dialogType,new object[]{"Kiểm tra bản vẽ",formatted,null,null})){
            dialog.StartPosition=FormStartPosition.Manual;dialog.Location=new Point(-3000,-3000);dialog.Show();Application.DoEvents();
            var body=(TextBox)Field(dialog,"body");
            Check("report-opens-at-beginning",body.SelectionStart==0&&body.GetCharIndexFromPosition(new Point(2,2))==0);
            Check("report-wraps-with-optional-horizontal-scroll",body.ReadOnly&&body.WordWrap&&body.ScrollBars==ScrollBars.Both);
            var wrap=Children(dialog).OfType<CheckBox>().Single();
            wrap.Checked=false;Check("report-can-disable-wrap",!body.WordWrap);wrap.Checked=true;
            foreach(var size in new[]{new Size(620,420),new Size(900,680)}){
                dialog.Size=size;Application.DoEvents();
                foreach(var b in Children(dialog).OfType<Button>())Check("report-buttons-visible-"+size.Width+"-"+b.Text,dialog.ClientRectangle.Contains(dialog.PointToClient(b.PointToScreen(new Point(b.Width-1,b.Height-1)))));
            }
            Check("report-copy-keeps-complete-content",body.Text.Replace("\r","")==formatted.Replace("\r",""));
            using(var bitmap=new Bitmap(dialog.Width,dialog.Height)){dialog.DrawToBitmap(bitmap,new Rectangle(0,0,dialog.Width,dialog.Height));bitmap.Save(Path.Combine(output,"readable-check-report.png"));}
            Children(dialog).OfType<Button>().Single(b=>b.Text=="Đóng báo cáo").PerformClick();Check("report-close-button",dialog.IsDisposed||!dialog.Visible);
        }
    }

    static void TestV101(string output)
    {
        var type = typeof(BhtPaletteControl).Assembly.GetType("BHT.Palette.PaletteTheme");
        var metrics = type.GetMethod("UpdateTabMetrics",BindingFlags.NonPublic|BindingFlags.Static);
        using (var tabs=new TabControl()) foreach(float dpi in new[]{96f,120f,144f,192f}) {
            metrics.Invoke(null,new object[]{tabs,dpi});
            Check("tabs-readable-height-"+dpi,tabs.ItemSize.Width>=64*dpi/96 && tabs.ItemSize.Height>=36*dpi/96);
        }
        using(var palette=new BhtPaletteControl()) using(var form=new Form {ClientSize=new Size(560,900)}) {
            form.Controls.Add(palette); form.Show();Application.DoEvents();
            var tabs=(TabControl)Field(palette,"_tabs");tabs.SelectedIndex=4;Application.DoEvents();
            var routes=(ListView)Field(palette,"routeList");var tools=(Control)Field(palette,"_tdtRouteTools");
            Check("green-workspace",palette.BackColor==Color.FromArgb(4,120,87));
            Check("tdt-hidden-without-route",!tools.Visible);
            var recalculate=Children(tabs.TabPages[4]).OfType<Button>().Single(b=>b.Text=="Tính lại theo tim đang chọn…");
            Check("122-selected-route-required-for-recalculation",!recalculate.Enabled);
            Check("122-recalculation-after-control-step",recalculate.Parent.Controls.OfType<Label>().First().Text.StartsWith("4."));
            var sections=((FlowLayoutPanel)tabs.TabPages[4].Controls[0]).Controls.OfType<FlowLayoutPanel>().Select(p=>p.Controls.OfType<Label>().First().Text).Where(t=>t.Length>2&&char.IsDigit(t[0])).ToArray();
            Check("122-engineering-workflow-order",sections.Length==6&&sections.Select((s,i)=>s.StartsWith((i+1)+".")).All(x=>x));
            Check("107-route-type-column-visible",routes.Columns[1].Text=="Loại");
            var pl=new ListViewItem(new[]{"PL_TEST","Khác","Polyline","100","Sẵn sàng"});routes.Items.Add(pl);pl.Selected=true;Application.DoEvents();
            Check("tdt-hidden-for-polyline",!tools.Visible);
            Check("122-selected-route-enables-recalculation",recalculate.Enabled);
            pl.Selected=false;var tdt=new ListViewItem(new[]{"TDT_TEST","Tim đường","TDTSolution 9.1","200","Sẵn sàng"});routes.Items.Add(tdt);tdt.Selected=true;Application.DoEvents();
            Check("tdt-visible-for-selected-tdt",tools.Visible);
            var routePage=(FlowLayoutPanel)tabs.TabPages[4].Controls[0];
            routePage.AutoScrollPosition=new Point(0,650);Application.DoEvents();
            int previousScroll=routePage.AutoScrollPosition.Y;
            Set(palette,"refreshingRoutes",true);routes.Items.Clear();Application.DoEvents();
            Check("128-refresh-does-not-collapse-tdt-panel",tools.Visible&&routePage.AutoScrollPosition.Y==previousScroll);
            routes.Items.Add(pl);routes.Items.Add(tdt);tdt.Selected=true;
            Set(palette,"refreshingRoutes",false);Call(palette,"UpdateRouteTools");Application.DoEvents();
            Check("128-restored-route-keeps-scroll",routePage.AutoScrollPosition.Y==previousScroll);
            routes.Focus();Application.DoEvents();
            Check("128-focus-return-does-not-scroll-page",routePage.AutoScrollPosition.Y==previousScroll);
            Call(routePage,"RevealControl",routes);Application.DoEvents();
            Check("128-explicit-navigation-still-scrolls",routePage.AutoScrollPosition.Y!=previousScroll);
            var readStakes=Children(tools).OfType<Button>().Single();
            var station=tools.Parent;
            var stationButtons=station.Controls.OfType<Button>().ToArray();
            foreach(int width in new[]{340,400,480,560,400}) {
                form.ClientSize=new Size(width,900); Application.DoEvents();
                tabs.TabPages[4].Controls[0].PerformLayout(); Application.DoEvents();
                var bounds=station.RectangleToClient(readStakes.RectangleToScreen(readStakes.ClientRectangle));
                foreach(var button in stationButtons) {
                    var other=station.RectangleToClient(button.RectangleToScreen(button.ClientRectangle));
                    Check("127-tdt-left-aligned-"+width+button.Text,bounds.Left==other.Left);
                    Check("127-tdt-right-aligned-"+width+button.Text,bounds.Right==other.Right);
                }
                Check("127-tdt-no-horizontal-scroll-"+width,!((FlowLayoutPanel)tabs.TabPages[4].Controls[0]).HorizontalScroll.Visible);
                using(var bitmap=new Bitmap(station.Width,station.Height)) {
                    station.DrawToBitmap(bitmap,station.ClientRectangle);
                    bitmap.Save(Path.Combine(output,"tdt-actions-127-"+width+".png"));
                }
            }
            tdt.Selected=false;pl.Selected=true;Application.DoEvents();
            Check("tdt-hidden-after-switch",!tools.Visible);
            Check("tdt-status-removed",!Children(palette).OfType<Label>().Any(l=>l.Text.Contains("bản thường: sẵn sàng")));
            Check("no-duplicate-photo-folder",Children(palette).OfType<Button>().Count(b=>b.Text.Contains("thư mục ảnh"))==1);
            Check("no-duplicate-point-style",!Children(tabs.TabPages[4]).OfType<Button>().Any(b=>b.Text.Contains("Dấu X")));
            using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,form.ClientRectangle);bitmap.Save(Path.Combine(output,"routes-101.png"));}
            tabs.SelectedIndex=0;Application.DoEvents();
            using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,form.ClientRectangle);bitmap.Save(Path.Combine(output,"dashboard-101.png"));}
            form.Close();
        }
    }

    static void TestV102(string output)
    {
        using(var palette = new BhtPaletteControl()) using(var host = new Form())
        {
            host.Controls.Add(palette); host.ClientSize = new Size(480,850); host.Show(); Application.DoEvents();
            var tabs = (TabControl)Field(palette,"_tabs");
            var status = (Label)Field(palette,"_status");
            Check("102-no-status-footer",status.Parent == null && !status.Visible);
            tabs.SelectedIndex=4; Application.DoEvents();
            var page = (FlowLayoutPanel)tabs.TabPages[4].Controls[0];
            foreach(var check in Children(page).OfType<CheckBox>()) check.Checked=true;
            Children(page).OfType<Button>().Single(b=>b.Text.StartsWith("Nâng cao:")).PerformClick();
            var source = Children(page).OfType<ComboBox>().Single(c=>c.Items.Contains("Tuyến TDT 9.1"));
            source.SelectedIndex=1; source.Focus();
            var wheel = Message.Create(source.Handle,0x020A,new IntPtr(120<<16),IntPtr.Zero);
            source.GetType().GetMethod("WndProc",Hidden).Invoke(source,new object[]{wheel});
            Check("102-wheel-preserves-source",source.SelectedIndex==1);
            foreach(int width in new[]{400,480,560,400,560})
            {
                host.ClientSize=new Size(width,850); Application.DoEvents(); page.PerformLayout(); Application.DoEvents();
                Check("102-no-route-horizontal-scroll-"+width,!page.HorizontalScroll.Visible);
                Check("102-source-readable-"+width,source.Width>=150);
                var list=(ListView)Field(palette,"routeList");
                Check("102-route-list-visible-"+width,list.Height>=170);
                foreach(var button in Children(page).OfType<Button>().Where(b=>b.Visible))
                {
                    Check("102-left-action-"+width+button.Text,button.TextAlign==ContentAlignment.MiddleLeft);
                    var measured=TextRenderer.MeasureText(button.Text,button.Font,new Size(Math.Max(20,button.Width-18),int.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.NoPrefix);
                    Check("102-action-fits-"+width+button.Text,button.Height>=measured.Height+8 && button.Right<=button.Parent.ClientSize.Width);
                }
                foreach(var panel in Children(page).OfType<FlowLayoutPanel>().Where(p=>p.Visible)) {
                    var items=panel.Controls.Cast<Control>().Where(c=>c.Visible).ToArray();
                    for(int i=0;i<items.Length;i++) {
                        Check("102-section-contains-"+width+items[i].Text,items[i].Bottom<=panel.ClientSize.Height);
                        for(int j=i+1;j<items.Length;j++) Check("102-no-overlap-"+width+items[i].Text+items[j].Text,!items[i].Bounds.IntersectsWith(items[j].Bounds));
                    }
                }
                page.AutoScrollPosition=Point.Empty; Application.DoEvents();
                using(var bmp=new Bitmap(host.Width,host.Height)){host.DrawToBitmap(bmp,host.ClientRectangle);bmp.Save(Path.Combine(output,"routes-102-"+width+".png"));}
            }
            var advanced=Children(page).OfType<Button>().Single(b=>b.Text.StartsWith("Nâng cao:"));
            int expanded=advanced.Parent.Height;
            advanced.PerformClick(); Application.DoEvents();
            Check("102-collapse-updates-page",advanced.Parent.Height<expanded);
            advanced.PerformClick(); Application.DoEvents();
            Check("102-expand-updates-page",advanced.Parent.Height==expanded);
            tabs.SelectedIndex=2; Application.DoEvents();
            foreach(int width in new[]{340,480,560})
            {
                host.ClientSize=new Size(width,850); Application.DoEvents();
                var form=(TableLayoutPanel)Field(palette,"_objectForm");
                var chain=(Control)Field(palette,"_oChainage");
                var shared=(CheckBox)Field(palette,"_oAllowShared");
                form.ScrollControlIntoView(shared); Application.DoEvents();
                var buttons=chain.Parent.Controls.OfType<Button>().ToArray();
                Check("102-chain-actions-all-present-"+width,buttons.Length==5);
                for(int i=0;i<buttons.Length;i++)
                {
                    Check("102-chain-full-width-"+width+i,buttons[i].Width>=150);
                    if(i>0) Check("102-chain-no-overlap-"+width+i,buttons[i].Top>=buttons[i-1].Bottom);
                }
                Check("102-shared-checkbox-height-"+width,shared.Height>=TextRenderer.MeasureText(shared.Text,shared.Font,new Size(shared.Width-25,int.MaxValue),TextFormatFlags.WordBreak).Height);
                using(var bmp=new Bitmap(host.Width,host.Height)){host.DrawToBitmap(bmp,host.ClientRectangle);bmp.Save(Path.Combine(output,"station-102-"+width+".png"));}
                form.AutoScrollPosition=Point.Empty; Application.DoEvents();
                using(var bmp=new Bitmap(host.Width,host.Height)){host.DrawToBitmap(bmp,host.ClientRectangle);bmp.Save(Path.Combine(output,"editor-102-"+width+".png"));}
            }
        }
    }

    static void TestV103(string output)
    {
        using(var palette=new BhtPaletteControl()) using(var host=new Form {ClientSize=new Size(560,900)})
        {
            host.Controls.Add(palette);host.Show();Application.DoEvents();
            var tabs=(TabControl)Field(palette,"_tabs");
            Check("111-navigation-above-content",tabs.Alignment==TabAlignment.Top);
            Check("111-no-right-rail",tabs.DisplayRectangle.Width>=tabs.ClientSize.Width-12);
            Check("111-overview-one-next-action",Children(tabs.TabPages[0]).OfType<Button>().Count(b=>b.Text!="Làm mới")==1);
            var all=Children(palette).OfType<Button>().ToArray();
            foreach(string text in new[]{"Tính lý trình tất cả hồ sơ","Kiểm tra bản vẽ","Nhập KMZ","Nhập CSV RTK","Thống kê ảnh"})
                Check("103-single-action-"+text,all.Count(b=>b.Text==text)==1);
            Check("103-no-second-station-action",!all.Any(b=>b.Text=="Tính lý trình và khoảng cách đến tim"));
            Check("103-single-station-distinct",all.Count(b=>b.Text=="Tính lý trình hồ sơ này")==1);
            Check("103-import-points-in-points",Children(tabs.TabPages[1]).OfType<Button>().Any(b=>b.Text=="Nhập CSV RTK"));
            Check("103-import-photos-in-photos",Children(tabs.TabPages[3]).OfType<Button>().Any(b=>b.Text=="Nhập KMZ"));
            var routeMethod=typeof(BhtPaletteControl).GetMethod("NeedsBatchMarkerReview",BindingFlags.Static|BindingFlags.NonPublic);
            foreach(var method in new[]{"ROUTE","FIELD_MARKERS"}) foreach(bool hasRoutes in new[]{false,true}) foreach(var chain in new[]{"","BHT_IMPORTED_MARKERS","ROAD_A"})
            {
                bool review=(bool)routeMethod.Invoke(null,new object[]{method,chain,hasRoutes});
                Check("103-method-"+method+"-"+hasRoutes+"-"+chain,review==(method=="FIELD_MARKERS"?chain=="":!hasRoutes));
            }
            File.WriteAllLines(Path.Combine(output,"button-inventory.txt"),tabs.TabPages.Cast<TabPage>().SelectMany(tab=>Children(tab).OfType<Button>().Select(b=>tab.Text.Replace("\u00a0"," ")+" | "+b.Text)).ToArray());
            tabs.SelectedIndex=4;Application.DoEvents();
            var routePage=tabs.TabPages[4];
            var routePanel=routePage.Controls.OfType<FlowLayoutPanel>().Single();
            foreach(int height in new[]{340,560})
            {
                host.ClientSize=new Size(340,height);Application.DoEvents();
                foreach(string title in new[]{"Công cụ bổ sung","Phân chia đoạn / gói thầu (tùy chọn)"})
                {
                    var toggle=Children(routePage).OfType<CheckBox>().Single(c=>c.Text==title);
                    toggle.Checked=false;Application.DoEvents();routePanel.ScrollControlIntoView(toggle);Application.DoEvents();
                    toggle.Checked=true;Application.DoEvents();
                    int index=routePanel.Controls.IndexOf(toggle);var section=routePanel.Controls[index+1];
                    Check("105-expansion-visible-without-tab-change-"+height+title,section.Visible&&section.Height>100);
                    var first=Children(section).OfType<Button>().First();
                    Check("105-expanded-action-in-viewport-"+height+title,routePanel.RectangleToScreen(routePanel.ClientRectangle).IntersectsWith(first.RectangleToScreen(first.ClientRectangle)));
                    var actions=Children(section).OfType<Button>().ToArray();
                    for(int i=1;i<actions.Length;i++)Check("105-expanded-actions-no-overlap-"+height+title+i,actions[i].Top>=actions[i-1].Bottom);
                    if(title=="Công cụ bổ sung")using(var bmp=new Bitmap(palette.Width,palette.Height)){palette.DrawToBitmap(bmp,palette.ClientRectangle);bmp.Save(Path.Combine(output,"routes-expanded-105-"+height+".png"));}
                    toggle.Checked=false;Application.DoEvents();Check("105-collapse-without-tab-change-"+height+title,!section.Visible);
                }
            }
            host.ClientSize=new Size(560,900);Application.DoEvents();
            var selectionPolicy=typeof(BhtPaletteControl).GetMethod("IsSelectionCommand",BindingFlags.Static|BindingFlags.NonPublic);
            foreach(string selectionCommand in new[]{"BHTCHONBAOHIEU","bhtchontuongtu","BHTSELECTSIMILAR"})
                Check("105-selection-finishes-without-Lisp-drain-"+selectionCommand,(bool)selectionPolicy.Invoke(null,new object[]{selectionCommand}));
            Check("105-write-command-still-refreshes",!(bool)selectionPolicy.Invoke(null,new object[]{"BHTKYHIEU"}));
            using(var bmp=new Bitmap(palette.Width,palette.Height)){palette.DrawToBitmap(bmp,palette.ClientRectangle);bmp.Save(Path.Combine(output,"routes-103.png"));}
            tabs.SelectedIndex=0;Application.DoEvents();
            using(var bmp=new Bitmap(palette.Width,palette.Height)){palette.DrawToBitmap(bmp,palette.ClientRectangle);bmp.Save(Path.Combine(output,"overview-103.png"));}
        }
    }

    static void TestSegment119(string output)
    {
        var rows = new System.Collections.Generic.List<BhtRecord> {
            new BhtRecord().Add("segment_id","D01").Add("package_id","G01").Add("km_start_m","40000").Add("km_end_m","41000").Add("side","PHAI").Add("route_id","").Add("package_name","Gói thầu tiếng Việt"),
            new BhtRecord().Add("segment_id","D02").Add("package_id","G01").Add("km_start_m","41000").Add("km_end_m","42000").Add("side","TRAI").Add("route_id","MISSING") };
        var routes = new System.Collections.Generic.Dictionary<string,BhtRecord> { {"RANH", new BhtRecord().Add("loai","TIM_RANH")}, {"ROAD",new BhtRecord().Add("loai","TIM_DUONG")} };
        using(var review = new SegmentTableForm(rows,routes,"","Kiểm tra giao diện đoạn / gói thầu",false))
        {
            review.StartPosition=FormStartPosition.Manual;review.Location=new Point(-3000,-3000);review.Show();Application.DoEvents();
            var grid=Children(review).OfType<DataGridView>().Single();
            Check("119-segment-count",grid.Rows.Count==2);
            Check("119-route-label-type",Convert.ToString(grid.Rows[1].Cells["route_id"].FormattedValue).Contains("THIẾU TIM"));
            Check("119-edit-snapshot-preserves-original",rows[0].Get("route_id")=="");
            var combo=Children(review).OfType<ComboBox>().Single();combo.SelectedValue="RANH";
            grid.ClearSelection();grid.Rows[0].Selected=true;grid.Rows[1].Selected=true;
            Children(review).OfType<Button>().Single(b=>b.Text=="Gán tim cho các dòng chọn").PerformClick();Application.DoEvents();
            Check("119-batch-route-selection",grid.Rows.Cast<DataGridViewRow>().All(r=>Convert.ToString(r.Cells["route_id"].Value)=="RANH"));
            Check("119-tim-ranh-visible",Convert.ToString(grid.Rows[0].Cells["route_id"].FormattedValue).Contains("Tim rãnh"));
            using(var bmp=new Bitmap(review.Width,review.Height)){review.DrawToBitmap(bmp,review.ClientRectangle);bmp.Save(Path.Combine(output,"segments119.png"));}
            Children(review).OfType<Button>().Single(b=>b.Text=="Áp dụng vào bản vẽ").PerformClick();Application.DoEvents();
            Check("119-result-independent-and-validated",review.DialogResult==DialogResult.OK&&review.Result.Count==2&&review.Result[0].Get("route_id")=="RANH"&&rows[0].Get("route_id")=="");
        }
        using(var review = new SegmentTableForm(rows,routes,"table.xlsx","",true))
        {
            Check("119-convert-requires-native-file",!Children(review).OfType<Button>().Any(b=>b.Text=="Áp dụng vào bản vẽ")&&Children(review).OfType<Button>().Any(b=>b.Text.Contains("Chuyển sang .bhtseg")));
        }
    }
    static void Test(string output)
    {
        Directory.CreateDirectory(output);
        TestConflicts128(output);
        TestSegment119(output);
        TestV103(output);
        TestV102(output);
        TestV101(output);
        TestReadableReport(output);
        TestFieldForms(output);
        Check("I428-petrol-name-restored",TdtSignLibrary.Find("I.428").Description=="Cửa hàng xăng dầu");
        Check("I428-name-search",TdtSignLibrary.Search("cua hang xang dau",100).Any(s=>s.Code=="I.428"));
        var inputType=typeof(BhtPaletteControl).Assembly.GetType("BHT.Palette.ImportedMarkersForm");
        var inputValues=new System.Collections.Generic.List<string>{"SNAP","A|40000|0|0|Cọc đầu","B|40100|100|0|Nội suy","C|40200|110|0|CẦN ĐỐI CHIẾU: Khoảng cách không phù hợp chênh lệch Km/H."};
        using(var review=(Form)Activator.CreateInstance(inputType,new object[]{inputValues})) {
            review.StartPosition=FormStartPosition.Manual;review.Location=new Point(-3000,-3000);review.Show();Application.DoEvents();
            var grid=(DataGridView)Field(review,"markers");
            Check("input-all-markers-already-listed",grid.Rows.Count==3);
            Check("input-readonly-keeps-source",grid.ReadOnly&&!grid.AllowUserToAddRows);
            Check("input-distance-visible",Convert.ToString(grid.Rows[1].Cells[3].Value).Contains("100"));
            Check("input-warning-highlight",grid.Rows[2].DefaultCellStyle.BackColor==Color.LemonChiffon);
            Check("input-no-manual-chain-name",!Children(review).OfType<TextBox>().Any());
            using(var bitmap=new Bitmap(review.Width,review.Height)){review.DrawToBitmap(bitmap,new Rectangle(0,0,review.Width,review.Height));bitmap.Save(Path.Combine(output,"imported-markers.png"));}
            Children(review).OfType<Button>().Single(b=>b.Text=="Hủy").PerformClick();Check("input-review-cancel",review.DialogResult==DialogResult.Cancel);
        }
        using(var review=(Form)Activator.CreateInstance(inputType,new object[]{new System.Collections.Generic.List<string>{""}}))
            Check("input-insufficient-disabled",!((Button)Field(review,"use")).Enabled);
        var manualValues=new System.Collections.Generic.List<string>{"SNAP","A|40000|0|0|Cọc đầu","B|40100|100|0|[MANUAL_OFF] Chưa dùng làm mốc","C|40200|200|0|[MANUAL_ON] Nội suy","D|NaN|300|0|[MANUAL_OFF] Thiếu Km/H"};
        using(var review=(Form)Activator.CreateInstance(inputType,new object[]{manualValues})){
            review.StartPosition=FormStartPosition.Manual;review.Location=new Point(-3000,-3000);review.Show();Application.DoEvents();
            var grid=(DataGridView)Field(review,"markers");
            Check("manual-markers-included-and-sorted",grid.Rows.Count==4 && Convert.ToString(grid.Rows[1].Cells[0].Value)=="B");
            Check("distance-skips-unapproved-marker",Convert.ToString(grid.Rows[2].Cells[3].Value).Contains("200"));
            grid.CurrentCell=grid.Rows[1].Cells[0];Application.DoEvents();
            Check("manual-off-can-confirm",Children(review).OfType<Button>().Single(b=>b.Text=="Xác nhận dùng mốc").Enabled);
            grid.CurrentCell=grid.Rows[3].Cells[0];Application.DoEvents();
            Check("manual-incomplete-cannot-confirm",!Children(review).OfType<Button>().Single(b=>b.Text=="Xác nhận dùng mốc").Enabled);
            grid.CurrentCell=grid.Rows[2].Cells[0];Application.DoEvents();
            using(var bitmap=new Bitmap(review.Width,review.Height)){review.DrawToBitmap(bitmap,new Rectangle(0,0,review.Width,review.Height));bitmap.Save(Path.Combine(output,"manual-marker-controls.png"));}
            Children(review).OfType<Button>().Single(b=>b.Text=="Ngừng dùng mốc thủ công").PerformClick();
            Check("manual-revoke-is-explicit-action",review.DialogResult==DialogResult.Retry && (string)inputType.GetField("ManualId").GetValue(review)=="C" && (string)inputType.GetField("ManualValue").GetValue(review)=="0");
        }
        var mixedValues=new System.Collections.Generic.List<string>{"SNAP","H9|39900|0|0|Cọc đầu|COC_TIEU|RTK","KM|40000|100|0|Nội suy|COT_KM|RTK","H1|40100|200|0|Nội suy|COC_TIEU|RTK","EMPTY|NaN|300|0|[INCOMPLETE] Thiếu số Km|COT_KM|RTK","HAND|41000|1000|0|[MANUAL_OFF] Chưa dùng làm mốc|COT_KM|MANUAL_CAD"};
        using(var review=(Form)Activator.CreateInstance(inputType,new object[]{mixedValues})){
            review.StartPosition=FormStartPosition.Manual;review.Location=new Point(-3000,-3000);review.Show();Application.DoEvents();
            var grid=(DataGridView)Field(review,"markers");
            Check("mixed-Km-between-H-markers",Convert.ToString(grid.Rows[1].Cells[0].Value)=="KM"&&Convert.ToString(grid.Rows[1].Cells[5].Value)=="Cột Km");
            Check("mixed-source-measured",Convert.ToString(grid.Rows[1].Cells[6].Value)=="Điểm đo");
            Check("mixed-source-manual",Convert.ToString(grid.Rows[3].Cells[6].Value)=="Thủ công");
            Check("mixed-incomplete-visible-at-bottom",Convert.ToString(grid.Rows[4].Cells[0].Value)=="EMPTY"&&Convert.ToString(grid.Rows[4].Tag)=="INCOMPLETE");
            grid.CurrentCell=grid.Rows[3].Cells[0];Application.DoEvents();
            Check("manual-Km-can-confirm",Children(review).OfType<Button>().Single(b=>b.Text=="Xác nhận dùng mốc").Enabled);
            grid.CurrentCell=grid.Rows[4].Cells[0];Application.DoEvents();
            Check("measured-incomplete-cannot-confirm",!Children(review).OfType<Button>().Single(b=>b.Text=="Xác nhận dùng mốc").Enabled);
            Check("incomplete-not-counted",Children(review).OfType<Label>().Any(l=>l.Text.StartsWith("3 / 5 mốc")));
            foreach(var size in new[]{new Size(900,480),new Size(1140,640)}){
                review.Size=size;Application.DoEvents();
                foreach(var button in Children(review).OfType<Button>())Check("mixed-actions-visible-"+size.Width+"-"+button.Text,review.ClientRectangle.Contains(review.PointToClient(button.PointToScreen(new Point(button.Width-1,button.Height-1)))));
            }
            using(var bitmap=new Bitmap(review.Width,review.Height)){review.DrawToBitmap(bitmap,new Rectangle(0,0,review.Width,review.Height));bitmap.Save(Path.Combine(output,"mixed-Km-markers.png"));}
            review.Close();
        }
        var manualType=typeof(BhtPaletteControl).Assembly.GetType("BHT.Palette.ManualMarkerForm");
        using(var manual=(Form)Activator.CreateInstance(manualType)) {
            manual.StartPosition=FormStartPosition.Manual;manual.Location=new Point(-3000,-3000);manual.Show();Application.DoEvents();
            var km=(TextBox)Field(manual,"km");var h=(TextBox)Field(manual,"h");
            km.Text="40";h.Text="X";
            Children(manual).OfType<Button>().Single(b=>b.Text=="Chọn vị trí trên CAD…").PerformClick();
            Check("manual-invalid-H-keeps-dialog",manual.DialogResult!=DialogResult.OK && ((Label)Field(manual,"error")).Text!="");
            h.Text="4";((ComboBox)Field(manual,"group")).SelectedIndex=1;Check("manual-km-h-disabled",!h.Enabled);
            ((ComboBox)Field(manual,"group")).SelectedIndex=0;
            foreach(var button in Children(manual).OfType<Button>())Check("manual-action-visible-"+button.Text,manual.ClientRectangle.Contains(manual.PointToClient(button.PointToScreen(new Point(button.Width-1,button.Height-1)))));
            using(var bitmap=new Bitmap(manual.Width,manual.Height)){manual.DrawToBitmap(bitmap,new Rectangle(0,0,manual.Width,manual.Height));bitmap.Save(Path.Combine(output,"manual-marker.png"));}
            Children(manual).OfType<Button>().Single(b=>b.Text=="Hủy").PerformClick();Check("manual-cancel",manual.DialogResult==DialogResult.Cancel);
        }
        using(var manual=(Form)Activator.CreateInstance(manualType)) {
            manual.StartPosition=FormStartPosition.Manual;manual.Location=new Point(-3000,-3000);manual.Show();Application.DoEvents();
            Children(manual).OfType<Button>().Single(b=>b.Text=="Chọn vị trí trên CAD…").PerformClick();
            Check("manual-unknown-KmH-accepted",manual.DialogResult==DialogResult.OK);
        }
        using(var placement=new PlacementOptionsForm("QA",0,1,"0")) {
            Check("placement-route-option-uses-perpendicular-mode",placement.Direction=="ROUTE_PERP");
            Check("placement-route-label-is-explicit",((ComboBox)Field(placement,"direction")).Text=="Vuông góc với tuyến");
        }
        foreach(var mode in new[]{new[]{"0","Đường dẫn thẳng","DIRECT"},new[]{"1","Gấp khúc tự động","ELBOW"},new[]{"2","Chọn điểm trung gian","WAYPOINT"}})
        using(var placement=new PlacementOptionsForm("QA",int.Parse(mode[0]),0,"0")) {
            Check("105-placement-label-and-mode-match-"+mode[2],((ComboBox)Field(placement,"mode")).Text==mode[1]&&placement.Mode==mode[2]);
        }
        using(var alias=new SignPickerForm("P.124A-2","","")){
            Check("P124-legacy-selection-opens-right-variant",alias.SelectedSign!=null && alias.SelectedSign.Code=="P.124a2");
            ((TextBox)Field(alias,"search")).Text="P.124B-1";Call(alias,"Filter");
            var filtered=(System.Collections.Generic.List<Panel>)Field(alias,"filteredCards");
            Check("P124-search-old-code-finds-canonical",filtered.Count>0 && filtered.Any(c=>((TdtSignEntry)c.Tag).Code=="P.124b1"));
        }
        using(var mixed=new SignPickerForm("R.122","","R.122")){
            var all=(System.Collections.Generic.List<Panel>)Field(mixed,"cards");
            Call(mixed,"Choose",all.Single(c=>((TdtSignEntry)c.Tag).Code=="BHT.TEXT.SAMPLE"));Call(mixed,"AddFace");
            Check("custom-add-retains-STOP-first",((ListBox)Field(mixed,"faces")).Items.Cast<string>().SequenceEqual(new[]{"R.122","BHT.TEXT.SAMPLE"}));
            Check("custom-per-face-content-stored",SignContent.ForFace(mixed.SelectedContent,1,"BHT.TEXT.SAMPLE").ContainsKey("custom_text"));
            Call(mixed,"MoveFace",-1);
            Check("custom-move-preserves-text-index",SignContent.ForFace(mixed.SelectedContent,0,"BHT.TEXT.SAMPLE").ContainsKey("custom_text"));
            Call(mixed,"MoveFace",1);Call(mixed,"AcceptSign");
            Check("custom-mixed-selection-accepted",mixed.DialogResult==DialogResult.OK && mixed.SelectedFaces.Count==2 && mixed.SelectedCode=="R.122");
            using(var restored=new SignPickerForm("R.122","","R.122;BHT.TEXT.SAMPLE")){
                restored.ConfigurePresentation(mixed.SelectedContent,"LEGACY","0.2","0.6");Call(restored,"AcceptSign");
                Check("custom-mixed-reopen-keeps-content",restored.DialogResult==DialogResult.OK && restored.SelectedContent==mixed.SelectedContent);
            }
        }
        using(var custom=new SignPickerForm()){
            custom.ConfigureCustomBoard("KHAC","","DỪNG LẠI\r\nQUAN SÁT");
            Check("legacy-board-selects-custom-library",custom.SelectedSign!=null && custom.SelectedSign.Provider=="BHT-CUSTOM");
            Call(custom,"AcceptSign");
            Check("custom-content-keeps-line-breaks",SignContent.ForFace(custom.SelectedContent,0,custom.SelectedCode)["custom_text"].Replace("\r\n","\n")=="DỪNG LẠI\nQUAN SÁT" && custom.SelectedFaces==null);
        }
        using(var picker=new SignPickerForm("R.415a","","R.415a; R.415b")) {
            picker.StartPosition=FormStartPosition.Manual;picker.Location=new Point(-3000,-3000);picker.Show(); Application.DoEvents();
            Check("library-BHT-title",picker.Text=="Thư viện biển báo BHT");
            var allCards=(System.Collections.Generic.List<Panel>)Field(picker,"cards");
            Check("P124-picker-hides-six-source-aliases",!allCards.Any(c=>SignPickerAliases.IsAlias(((TdtSignEntry)c.Tag).Code)));
            Check("P124-picker-keeps-four-direction-vehicle-variants",new[]{"P.124a1","P.124a2","P.124b1","P.124b2"}.All(code=>allCards.Any(c=>((TdtSignEntry)c.Tag).Code==code)));
            Check("P124-old-codes-still-resolve",new[]{"P.124A","P.124A-1","P.124A-2","P.124B","P.124B-1","P.124B-2"}.All(code=>TdtSignLibrary.Find(code)!=null));
            Check("picker-retains-complete-catalog-with-bounded-page",allCards.Count>=461 && ((FlowLayoutPanel)Field(picker,"grid")).Controls.Count<=60);
            Check("picker-keeps-only-current-page-images",((System.Collections.Generic.List<Image>)Field(picker,"images")).Count<=61);
            Call(picker,"Filter");
            var reached=new System.Collections.Generic.HashSet<string>();
            int pages=(allCards.Count+59)/60;
            for(int page=0;page<pages;page++) {
                Set(picker,"pageIndex",page);Call(picker,"ShowPage");
                foreach(var card in ((FlowLayoutPanel)Field(picker,"grid")).Controls.Cast<Control>()) {
                    reached.Add(((TdtSignEntry)card.Tag).Code);
                    var visibleGrid=(FlowLayoutPanel)Field(picker,"grid");
                    visibleGrid.ScrollControlIntoView(card);
                    // Native scroll raises Scroll; reproduce that event after programmatic scrolling.
                    typeof(ScrollableControl).GetMethod("OnScroll",Hidden).Invoke(visibleGrid,new object[]{new ScrollEventArgs(ScrollEventType.ThumbPosition,visibleGrid.VerticalScroll.Value)});
                    Application.DoEvents();
                    Check("every-scrolled-card-has-image-before-click",card.Controls.OfType<PictureBox>().Single().Image!=null);
                }
            }
            Check("all-pages-reach-all-catalog-codes-without-loss",reached.Count==allCards.Count);
            Check("browsing-pages-keeps-images-bounded",((System.Collections.Generic.List<Image>)Field(picker,"images")).Count<=61);
            var place=typeof(SignPickerForm).GetMethod("IsPlace",BindingFlags.Static|BindingFlags.NonPublic);
            Check("place-fields-across-sign-groups",new[] {"DESTN_L","DESTINATION1","START","END","LOCATION","PAGODA_Vi","NAMEPART1","ĐỊAĐIỂM1"}.All(tag=>(bool)place.Invoke(null,new object[]{tag})));
            var groups=(ComboBox)Field(picker,"groups"); Check("supplementary-filter-present",groups.Items.Count==8 && groups.Items.Cast<string>().Contains("Biển phụ"));
            groups.SelectedIndex=7; Application.DoEvents();
            var customCards=((FlowLayoutPanel)Field(picker,"grid")).Controls.Cast<Control>().ToList();
            Check("custom-filter-shows-only-custom",customCards.Count>0 && customCards.All(c=>((TdtSignEntry)c.Tag).Provider=="BHT-CUSTOM"));
            Call(picker,"Choose",(Panel)customCards[0]);
            Check("custom-allows-multiple-faces",((CheckBox)Field(picker,"multi")).Enabled);
            Check("custom-preview-before-click",customCards.All(c=>c.Controls.OfType<PictureBox>().Single().Image!=null));
            using(var bitmap=new Bitmap(picker.Width,picker.Height)){picker.DrawToBitmap(bitmap,picker.ClientRectangle);bitmap.Save(Path.Combine(output,"library-custom.png"));}
            groups.SelectedIndex=6; Application.DoEvents();
            var supplementaryGrid=(FlowLayoutPanel)Field(picker,"grid");
            var supplementaryCards=supplementaryGrid.Controls.Cast<Control>().Where(c=>c.Visible && c.Tag is TdtSignEntry).Select(c=>(TdtSignEntry)c.Tag).ToList();
            Check("supplementary-filter-has-all-S-codes",supplementaryCards.Count>0 && supplementaryCards.All(s=>s.Code.StartsWith("S.",StringComparison.OrdinalIgnoreCase)) && supplementaryCards.Count==TdtSignLibrary.GetCatalog().Count(s=>s.Code.StartsWith("S.",StringComparison.OrdinalIgnoreCase)));
            Check("supplementary-filter-opens-at-first-row",supplementaryGrid.AutoScrollPosition.Y==0);
            using(var bitmap=new Bitmap(picker.Width,picker.Height)) {picker.DrawToBitmap(bitmap,picker.ClientRectangle);bitmap.Save(Path.Combine(output,"library-supplementary.png"));}
            groups.SelectedIndex=5; Application.DoEvents();
            var grid=(FlowLayoutPanel)Field(picker,"grid");
            var visible=grid.Controls.Cast<Control>().Where(c=>c.Visible).Select(c=>(TdtSignEntry)c.Tag).ToList();
            Check("highway-filter-shows-highway",visible.Count>0 && visible.All(e=>TextSearch.Fold(e.Group).Contains("cao toc")) && visible.All(e=>!e.Code.StartsWith("S.")));
            groups.SelectedIndex=0;
            Call(picker,"Choose",allCards.First(c=>((TdtSignEntry)c.Tag).Code=="R.415a"));
            ((CheckBox)Field(picker,"multi")).Checked=true;
            var content=new SignContentFace {Index=0,Code="R.415a"}; content.Values["demo"]="Mặt đầu";
            picker.ConfigurePresentation(SignContent.Write(new[] {content}),"CAP1_3","0.3","0.8");
            ((ListBox)Field(picker,"faces")).SelectedIndex=0;Call(picker,"MoveFace",1);
            Check("moving-face-retains-content-index",SignContent.Read(picker.SelectedContent).Single().Index==1);
            Call(picker,"ReindexContent",0,-1,true);
            Check("removing-face-renumbers-content",SignContent.Read(picker.SelectedContent).Single().Index==0);
            picker.ConfigurePresentation("","CAP1_3","0.3","0.8");
            using(var finish=new Timer {Interval=80}) {
                finish.Tick+=(sender,args)=> {
                    var dialog=Application.OpenForms.Cast<Form>().FirstOrDefault(d=>d.Text=="Nội dung biển"); if(dialog==null) return; finish.Stop();
                    Check("saved-CAP1-opens-advanced-layout",Children(dialog).OfType<CheckBox>().Single(c=>c.Text.Contains("khung/trụ")).Checked);
                    using(var bitmap=new Bitmap(dialog.Width,dialog.Height)) {dialog.DrawToBitmap(bitmap,dialog.ClientRectangle);bitmap.Save(Path.Combine(output,"cap1-editor.png"));}
                    Children(dialog).OfType<Button>().Single(b=>b.Text=="Áp dụng").PerformClick();
                };
                finish.Start();Call(picker,"EditPresentation");finish.Stop();
            }
            Check("CAP1-editor-applies-parameters",picker.SelectedLayout=="CAP1_3" && picker.SelectedGap=="0.3" && picker.SelectedClearance=="0.8");
            using(var bitmap=new Bitmap(picker.Width,picker.Height)) {picker.DrawToBitmap(bitmap,picker.ClientRectangle);bitmap.Save(Path.Combine(output,"library-highway.png"));}
            picker.Close();
        }
        foreach(string code in new[] {"P.127-80","DP.134-60","R.306-40","P.117@4.5","S.505a@8","R.E9b@22:00-05:00"}) {
            using(var picker=new SignPickerForm(code,"","")) {
                picker.StartPosition=FormStartPosition.Manual;picker.Location=new Point(-3000,-3000);picker.Show();Application.DoEvents();
                Check("legacy-inputs-absent-from-sidebar",!Children(picker).Contains((Control)Field(picker,"speed")) && !Children(picker).Contains((Control)Field(picker,"metres")));
                using(var finish=new Timer {Interval=80}) {
                    finish.Tick+=(sender,args)=> {
                        var dialog=Application.OpenForms.Cast<Form>().FirstOrDefault(d=>d.Text=="Nội dung biển");if(dialog==null)return;finish.Stop();
                        var table=Children(dialog).OfType<DataGridView>().Single();
                        Check("unified-editor-has-legacy-field-"+code,table.Rows.Count>0);
                        if(code=="P.127-80") {
                            table.Rows[0].Cells[2].Value="800";Children(dialog).OfType<Button>().Single(b=>b.Text=="Áp dụng").PerformClick();
                            Check("unified-speed-rejects-invalid",dialog.Visible);
                            table.Rows[0].Cells[2].Value="90";
                            using(var bitmap=new Bitmap(dialog.Width,dialog.Height)){dialog.DrawToBitmap(bitmap,dialog.ClientRectangle);bitmap.Save(Path.Combine(output,"unified-content.png"));}
                        }
                        Children(dialog).OfType<Button>().Single(b=>b.Text=="Áp dụng").PerformClick();
                        if(dialog.DialogResult!=DialogResult.OK) {using(var bitmap=new Bitmap(dialog.Width,dialog.Height)){dialog.DrawToBitmap(bitmap,dialog.ClientRectangle);bitmap.Save(Path.Combine(output,"failed-editor.png"));} string problem="Enabled="+Children(dialog).OfType<Button>().Single(b=>b.Text=="Áp dụng").Enabled+" Visible="+Children(dialog).OfType<Button>().Single(b=>b.Text=="Áp dụng").Visible+" | "+string.Join(" | ",Children(dialog).OfType<Label>().Select(l=>l.Text));dialog.Close();throw new System.Exception("Unified editor could not apply "+code+": "+problem);}
                    };finish.Start();Call(picker,"EditPresentation");finish.Stop();
                }
                Call(picker,"AcceptSign");Check("unified-editor-persists-"+code,picker.SelectedCode==(code=="P.127-80" ? "P.127-90" : code));
            }
        }
        using(var picker=new SignPickerForm("P.127-80","","P.127-80; P.127-40")) {
            picker.StartPosition=FormStartPosition.Manual;picker.Location=new Point(-3000,-3000);picker.Show();Application.DoEvents();
            using(var finish=new Timer {Interval=80}) {
                finish.Tick+=(sender,args)=> {
                    var dialog=Application.OpenForms.Cast<Form>().FirstOrDefault(d=>d.Text=="Nội dung biển");if(dialog==null)return;finish.Stop();
                    var table=Children(dialog).OfType<DataGridView>().Single();Check("two-speed-faces-have-distinct-rows",table.Rows.Count==2);
                    table.Rows[0].Cells[2].Value="90";table.Rows[1].Cells[2].Value="50";
                    Children(dialog).OfType<Button>().Single(b=>b.Text=="Áp dụng").PerformClick();
                };finish.Start();Call(picker,"EditPresentation");finish.Stop();
            }
            Call(picker,"AcceptSign");Check("unified-editor-keeps-per-face-speed",picker.SelectedFaces.SequenceEqual(new[] {"P.127-90","P.127-50"}));
        }
        using(var picker=new SignPickerForm("IE.456A-1","","IE.456A-1; IE.456A-1")) {
            picker.ConfigurePresentation("","CAP1_9","0.2","0.6"); picker.StartPosition=FormStartPosition.Manual;picker.Location=new Point(-3000,-3000);picker.Show(); Application.DoEvents();
            Call(picker,"AcceptSign"); Check("place-name-required-before-accept",picker.DialogResult!=DialogResult.OK && ((Label)Field(picker,"validation")).Text.Contains("nội dung thực tế"));
            using(var finish=new Timer {Interval=80}) {
                finish.Tick+=(sender,args)=> {
                    var dialog=Application.OpenForms.Cast<Form>().FirstOrDefault(d=>d.Text=="Nội dung biển"); if(dialog==null) return; finish.Stop();
                    var table=Children(dialog).OfType<DataGridView>().Single();
                    Check("place-sample-separated-from-record",Convert.ToString(table.Rows[0].Cells[2].Value)=="" && Convert.ToString(table.Rows[0].Cells[3].Value)!="");
                    foreach(DataGridViewRow row in table.Rows) if(Convert.ToString(row.Cells[2].Value)=="") row.Cells[2].Value=Convert.ToString(row.Cells[0].Value).StartsWith("1 / ") ? "HÀ NỘI" : "ĐÀ NẴNG";
                    using(var bitmap=new Bitmap(dialog.Width,dialog.Height)) {dialog.DrawToBitmap(bitmap,dialog.ClientRectangle);bitmap.Save(Path.Combine(output,"cap1-highway-content.png"));}
                    Children(dialog).OfType<Button>().Single(b=>b.Text=="Áp dụng").PerformClick();
                }; finish.Start(); Call(picker,"EditPresentation"); finish.Stop();
            }
            Call(picker,"AcceptSign"); var content=SignContent.Read(picker.SelectedContent);
            Check("two-identical-highway-faces-keep-different-place-names",picker.DialogResult==DialogResult.OK && content.Count==2 && content[0].Values.ContainsValue("HÀ NỘI") && content[1].Values.ContainsValue("ĐÀ NẴNG"));
        }
        using(var picker=new SignPickerForm("I.441a","","")) {
            picker.StartPosition=FormStartPosition.Manual;picker.Location=new Point(-3000,-3000);picker.Show();Application.DoEvents();
            using(var finish=new Timer {Interval=80}) {
                finish.Tick+=(sender,args)=> {
                    var dialog=Application.OpenForms.Cast<Form>().FirstOrDefault(d=>d.Text=="Nội dung biển");if(dialog==null)return;finish.Stop();
                    Check("default-content-editor-hides-advanced-layout",!Children(dialog).OfType<CheckBox>().Single(c=>c.Text.Contains("khung/trụ")).Checked);
                    var table=Children(dialog).OfType<DataGridView>().Single();var apply=Children(dialog).OfType<Button>().Single(b=>b.Text=="Áp dụng");
                    Check("numeric-CAD-text-shown-with-unit",table.Rows.Count==1 && Convert.ToString(table.Rows[0].Cells[1].Value).Contains("(m)") && Convert.ToString(table.Rows[0].Cells[3].Value)=="500 m");
                    table.Rows[0].Cells[2].Value="2 km";apply.PerformClick();Check("numeric-editor-rejects-wrong-unit",dialog.DialogResult!=DialogResult.OK && Children(dialog).OfType<Label>().Any(l=>l.Text.Contains("Đơn vị phải")));
                    table.Rows[0].Cells[2].Value="350,5";
                    using(var bitmap=new Bitmap(dialog.Width,dialog.Height)){dialog.DrawToBitmap(bitmap,dialog.ClientRectangle);bitmap.Save(Path.Combine(output,"parameter-editor.png"));}
                    apply.PerformClick();
                };finish.Start();Call(picker,"EditPresentation");finish.Stop();
            }
            Check("numeric-editor-saves-decimal-and-native-unit",SignContent.ForFace(picker.SelectedContent,0,"I.441a").ContainsValue("350.5 m"));
            Call(picker,"AcceptSign");Check("numeric-edited-sign-can-be-selected",picker.DialogResult==DialogResult.OK);
        }
        using(var picker=new SignPickerForm("P.127a","","")) {
            picker.StartPosition=FormStartPosition.Manual;picker.Location=new Point(-3000,-3000);picker.Show();Application.DoEvents();
            using(var finish=new Timer {Interval=80}) {
                finish.Tick+=(sender,args)=> {
                    var dialog=Application.OpenForms.Cast<Form>().FirstOrDefault(d=>d.Text=="Nội dung biển");if(dialog==null)return;finish.Stop();
                    var table=Children(dialog).OfType<DataGridView>().Single();var first=table.Rows.Cast<DataGridViewRow>().Single(r=>Convert.ToString(r.Cells[1].Value).Contains("Time1"));var last=table.Rows.Cast<DataGridViewRow>().Single(r=>Convert.ToString(r.Cells[1].Value).Contains("Time2"));var apply=Children(dialog).OfType<Button>().Single(b=>b.Text=="Áp dụng");
                    first.Cells[2].Value="24:00";apply.PerformClick();Check("hour-editor-rejects-invalid-hour",dialog.DialogResult!=DialogResult.OK);
                    first.Cells[2].Value="22:15";last.Cells[2].Value="5:30";apply.PerformClick();
                };finish.Start();Call(picker,"EditPresentation");finish.Stop();
            }
            var values=SignContent.ForFace(picker.SelectedContent,0,"P.127a");Check("hour-editor-keeps-two-overnight-values",values.ContainsValue("22:15") && values.ContainsValue("05:30"));
        }
        using (var editor = new BhtPaletteControl())
        using (var host = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-3000, -3000), ClientSize = new Size(480, 850) })
        {
            host.Controls.Add(editor);
            host.Show();
            var tabs = (TabControl)Field(editor, "_tabs");
            var issueMethod=typeof(BhtPaletteControl).GetMethod("MatchesObjectIssue",BindingFlags.Static|BindingFlags.NonPublic);
            var validIds=new System.Collections.Generic.HashSet<string>(new[]{"P1"},StringComparer.OrdinalIgnoreCase);
            Func<BhtRecord,int,bool> issue=(record,kind)=>(bool)issueMethod.Invoke(null,new object[]{record,kind,validIds});
            Check("review-filter-no-station",issue(new BhtRecord(),1));
            Check("review-filter-marker-number-is-station",!issue(new BhtRecord().Add(ObjFields.Group,"COT_KM").Add(ObjFields.MarkerKm,"40"),1));
            Check("review-filter-missing-sign-code",issue(new BhtRecord().Add(ObjFields.Group,"BIEN_BAO"),2));
            Check("review-filter-custom-block-is-valid",!issue(new BhtRecord().Add(ObjFields.Group,"BIEN_BAO").Add(ObjFields.CustomBlock,"CUSTOM"),2));
            Check("review-filter-missing-second-foot",issue(new BhtRecord().Add(ObjFields.Point,"P1").Add(ObjFields.PoleCount,"2"),3));
            Check("review-filter-stale-station",issue(new BhtRecord().Add(ObjFields.StationStatus,"NEEDS_REVIEW"),4));
            Check("bulk-heading-action-present", Children(editor).OfType<Button>().Any(b => b.Text.Contains("Xoay biển vuông góc")));
            Check("workflow-tab-order", tabs.TabPages[2] == Field(editor, "_tabObjects") && tabs.TabPages[3] == Field(editor, "_tabPhotos"));
            tabs.SelectedTab = (TabPage)Field(editor, "_tabObjects");
            Application.DoEvents();
            Check("empty-editor-clean", !(bool)Call(editor, "IsObjectEditorDirty"));
            Check("no-external-custom-board-button",!Children(editor).OfType<Button>().Any(b=>b.Text.Contains("Nhập bảng chữ")));
            using(var custom=new SignPickerForm()){
                custom.ConfigureCustomBoard("KHAC","","DỪNG LẠI\r\nQUAN SÁT");
                Call(custom,"AcceptSign");Call(editor,"ApplyPickedSign",custom);
                var record=(BhtRecord)Call(editor,"EditorFields");
                Check("custom-selection-applies-native-board",record.Get(ObjFields.Group)=="BIEN_BAO" && SignContent.ForFace(record.Get(ObjFields.SignContent),0,record.Get(ObjFields.Code))["custom_text"].Replace("\r\n","\n")=="DỪNG LẠI\nQUAN SÁT" && record.Get(ObjFields.CodeType)=="NOI_BO");
                Check("custom-selection-clears-stale-sign-settings",record.Get(ObjFields.SignLayout)=="LEGACY" && record.Get(ObjFields.SignContent)!="" && record.Get(ObjFields.CustomBlock)=="");
            }
            Call(editor,"ClearObjectEditor");
            ((TextBox)Field(editor, "_oDesc")).Text = "Biển báo đang sửa";
            Check("editing-detects-unsaved-changes", (bool)Call(editor, "IsObjectEditorDirty"));
            Check("unsaved-label-visible", ((Label)Field(editor, "_oMode")).Text.Contains("Chưa lưu"));
            Call(editor, "MarkObjectEditorClean");
            Check("accepted-state-clean", !(bool)Call(editor, "IsObjectEditorDirty"));
            ((TextBox)Field(editor, "_oId")).Text = "OBJ-000001";
            Check("id-edit-detected", (bool)Call(editor, "IsObjectEditorDirty"));
            Call(editor, "MarkObjectEditorClean");
            ((TextBox)Field(editor, "_oChainage")).Text = "Km12+345";
            Check("manual-station-edit-detected", (bool)Call(editor, "IsObjectEditorDirty"));
            Check("new-object-placement-disabled", !((Button)Field(editor, "_placeObjectButton")).Enabled);
            Check("new-object-delete-disabled", !((Button)Field(editor, "_deleteObjectButton")).Enabled);
            var parameter = typeof(BhtPaletteControl).GetMethod("SaveObject", Hidden).GetParameters()[0];
            Check("save-does-not-sync-symbol-by-default", (bool)parameter.DefaultValue == false);
            var draft = (Action)Call(editor, "CaptureObjectDraft");
            Set(editor,"_signLayout","CAP1_9"); Set(editor,"_signGap","0.7");
            var presentationDraft=(Action)Call(editor,"CaptureObjectDraft");
            Call(editor,"ClearObjectEditor"); presentationDraft();
            Check("draft-retains-CAP1",(string)Field(editor,"_signLayout")=="CAP1_9" && ((BhtRecord)Call(editor,"EditorFields")).Get(ObjFields.SignGap)=="0.7");
            Call(editor, "ClearObjectEditor"); draft();
            Check("draft-restores-manual-station-and-id", ((TextBox)Field(editor, "_oChainage")).Text == "Km12+345" && ((TextBox)Field(editor, "_oId")).Text == "OBJ-000001");
            Check("restored-draft-still-unsaved", (bool)Call(editor, "IsObjectEditorDirty"));
            Call(editor, "ClearObjectEditor");
            var document = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            using (var tr = document.Database.TransactionManager.StartTransaction())
            {
                BhtStore.Write(tr, document.Database, "OBJ", "UI-001", new BhtRecord().Add(ObjFields.Group, "BIEN_BAO").Add(ObjFields.Desc, "Biển tốc độ").Add(ObjFields.Code, "P.127-80"));
                BhtStore.Write(tr, document.Database, "OBJ", "UI-002", new BhtRecord().Add(ObjFields.Group, "COC_TIEU").Add(ObjFields.Desc, "Cọc tiêu"));
                tr.Commit();
            }
            editor.BindTo(document);
            Call(editor, "LoadObject", "UI-001");
            var automaticFaces=(TextBox)Field(editor,"_oFaces");
            Check("single-sign-face-count-automatic-readonly",automaticFaces.ReadOnly && automaticFaces.Text=="1" && !(bool)Call(editor,"IsObjectEditorDirty"));
            var faceCodes=(ComboBox)Field(editor,"_oFaceCodes");faceCodes.Text="P.127-80; P.127-80";
            Check("same-code-two-physical-faces-counted",automaticFaces.Text=="2");
            faceCodes.Text="P.127-80";
            Check("remove-face-updates-count",automaticFaces.Text=="1");
            faceCodes.Text="";((ComboBox)Field(editor,"_oCode")).Text="R.306-40";
            Check("one-main-code-without-face-list-counted",automaticFaces.Text=="1");
            faceCodes.Text="P.127-80; P.127-80";
            using(var picker=new SignPickerForm("W.207a","",""))
            {
                Call(picker,"AcceptSign"); Call(editor,"ApplyPickedSign",picker);
                Check("single-pick-replaces-stale-multiple-faces",automaticFaces.Text=="1" && faceCodes.Text=="W.207a" && ((ComboBox)Field(editor,"_oCode")).Text=="W.207a");
            }
            Call(editor,"LoadObject","UI-001");
            var library=(SignPickerForm)Call(editor,"CreateSignPicker");
            library.StartPosition=FormStartPosition.Manual;library.Location=new Point(-3000,-3000);library.Show(host);Application.DoEvents();
            Check("picker-modeless-single-instance",!library.Modal && ReferenceEquals(library,Call(editor,"CreateSignPicker")));
            document.Editor.Command("_.ZOOM","_E");Application.DoEvents();editor.RefreshAll();
            Check("CAD-command-with-picker-keeps-editing-target",library.Visible && ReferenceEquals(library,Field(editor,"_signPicker")) && ((TextBox)Field(editor,"_oId")).Text=="UI-001");
            Call(library,"AcceptSign");Application.DoEvents();
            Check("modeless-choice-applies-to-current-draft",Field(editor,"_signPicker")==null && ((ComboBox)Field(editor,"_oCode")).Text=="P.127-80");
            var abandoned=(SignPickerForm)Call(editor,"CreateSignPicker");abandoned.StartPosition=FormStartPosition.Manual;abandoned.Location=new Point(-3000,-3000);abandoned.Show(host);
            Call(editor,"LoadObject","UI-002");
            var markerPlace=(Button)Field(editor,"_placeObjectButton");
            Check("marker-placement-auto-disabled",!markerPlace.Enabled && markerPlace.Text=="Cọc đặt tự động");
            Check("disabled-marker-distinct-background",markerPlace.BackColor==Color.FromArgb(203,213,209));
            ((TextBox)Field(editor,"_oNote")).Text="Marker edit";
            Check("marker-edit-cannot-enable-free-placement",!markerPlace.Enabled);
            Check("switch-object-closes-picker-without-applying",abandoned.IsDisposed && Field(editor,"_signPicker")==null && ((TextBox)Field(editor,"_oId")).Text=="UI-002");
            Call(editor,"LoadObject","UI-001");
            Check("sign-placement-reenabled",markerPlace.Enabled && markerPlace.Text=="Đặt ký hiệu…" && markerPlace.BackColor!=Color.FromArgb(203,213,209));
            var detached=(SignPickerForm)Call(editor,"CreateSignPicker");detached.StartPosition=FormStartPosition.Manual;detached.Location=new Point(-3000,-3000);detached.Show(host);
            editor.Unbind(false);
            Check("unbind-closes-modeless-picker",detached.IsDisposed && Field(editor,"_signPicker")==null);
            editor.BindTo(document);Call(editor,"LoadObject","UI-001");
            ((TextBox)Field(editor, "_oNote")).Text = "Nội dung chưa lưu";
            editor.RefreshAll();
            Check("refresh-preserves-unsaved-note", ((TextBox)Field(editor, "_oNote")).Text == "Nội dung chưa lưu" && (bool)Call(editor, "IsObjectEditorDirty"));
            ((TextBox)Field(editor, "_objSearch")).Text = "toc do";
            ((TabControl)Field(editor,"_tabs")).SelectedIndex=2;
            var searchUntil=DateTime.UtcNow.AddMilliseconds(250);
            while(DateTime.UtcNow<searchUntil){Application.DoEvents();System.Threading.Thread.Sleep(10);}
            var list = (ListView)Field(editor, "_objList");
            Check("accent-insensitive-object-search", list.Items.Count == 1 && (string)list.Items[0].Tag == "UI-001");
            Check("filter-preserves-editor", ((TextBox)Field(editor, "_oNote")).Text == "Nội dung chưa lưu");
            Check("group-label-keeps-stored-code", ((ComboBox)Field(editor, "_oGroup")).Text == "Biển báo" && ((BhtRecord)Call(editor, "EditorFields")).Get(ObjFields.Group) == "BIEN_BAO");
            editor.BindTo(null);
            Check("unbound-editor-cleared", ((TextBox)Field(editor, "_oNote")).Text == "");
            editor.BindTo(document);
            Check("drawing-draft-restored", ((TextBox)Field(editor, "_oNote")).Text == "Nội dung chưa lưu" && ((TextBox)Field(editor, "_oId")).Text == "UI-001" && (bool)Call(editor, "IsObjectEditorDirty"));
            Check("redundant-object-zoom-button-removed", !Children((TabPage)Field(editor, "_tabObjects")).OfType<Button>().Any(b => b.Text == "Thu phóng"));
            var supportHint = (Label)Field(editor, "_oSupportHint");
            ((ComboBox)Field(editor, "_oGroup")).SelectedIndex = 4;
            ((TextBox)Field(editor, "_oPoles")).Text = "2";
            var objectPoints = (ListBox)Field(editor, "_oPoints"); objectPoints.Items.Clear(); objectPoints.Items.Add("FOOT-1 | Chân 1"); Call(editor, "UpdateObjectEditorState");
            Check("two-supports-one-point-shows-missing-foot", supportHint.Visible && supportHint.Text.Contains("Thiếu 1 điểm chân RTK"));
            objectPoints.Items.Add("FOOT-2 | Chân 2"); Call(editor, "UpdateObjectEditorState");
            Check("second-measured-point-clears-foot-warning", !supportHint.Visible && supportHint.Text == "");
            ((TextBox)Field(editor, "_oPoles")).Text = "1"; objectPoints.Items.RemoveAt(1); Call(editor, "UpdateObjectEditorState");
            Check("one-support-one-point-needs-no-warning", !supportHint.Visible);
            Call(editor, "LoadObject", "UI-001");
            ((TextBox)Field(editor, "_oNote")).Text = "Nội dung chưa lưu";
            ((TextBox)Field(editor, "_objSearch")).Text = "";
            searchUntil=DateTime.UtcNow.AddMilliseconds(250);
            while(DateTime.UtcNow<searchUntil){Application.DoEvents();System.Threading.Thread.Sleep(10);}
            var status = (Label)Field(editor, "_status");
            var itemBounds = list.Items[1].Bounds;
            var mouse = typeof(Control).GetMethod("OnMouseDoubleClick", Hidden);
            // The clicked row can differ from the dirty editor after a cancelled selection change.
            status.Text = "Before double click";
            mouse.Invoke(list, new object[] { new MouseEventArgs(MouseButtons.Left, 2, itemBounds.Left + 5, itemBounds.Top + 5, 0) });
            Check("double-click-zooms-clicked-record", status.Text == "Hồ sơ chưa có điểm RTK hợp lệ.");
            Check("double-click-preserves-unsaved-editor", ((TextBox)Field(editor, "_oId")).Text == "UI-001" && ((TextBox)Field(editor, "_oNote")).Text == "Nội dung chưa lưu" && (bool)Call(editor, "IsObjectEditorDirty"));
            status.Text = "Blank area";
            mouse.Invoke(list, new object[] { new MouseEventArgs(MouseButtons.Left, 2, 5, list.ClientSize.Height - 5, 0) });
            Check("double-click-blank-area-does-nothing", status.Text == "Blank area");
            mouse.Invoke(list, new object[] { new MouseEventArgs(MouseButtons.Right, 2, itemBounds.Left + 5, itemBounds.Top + 5, 0) });
            Check("right-double-click-does-nothing", status.Text == "Blank area");
            var fill = (CheckBox)Field(editor, "_oSignFill");
            var api = new LispApi();
            Check("fill-API-on-confirmed", api.Call("bht:api-sign-fill", "1").Ok);
            Set(editor, "_lispOk", true);
            Call(editor, "RefreshSignFill");
            Check("fill-reads-confirmed-on", fill.Checked);
            int generation = (int)Field(editor, "_lispGeneration");
            // Delay the completion boundary while refresh reads the old drawing state.
            Set(editor, "_pendingSignFill", (bool?)false); Set(editor, "_lispRunning", true);
            editor.RefreshAll(); editor.RefreshAll();
            Check("fill-refresh-does-not-recheck-pending-off", !fill.Checked && !fill.Enabled);
            fill.Checked = true;
            Check("fill-second-change-rejected-while-pending", !fill.Checked);
            var off = api.Call("bht:api-sign-fill", "0");
            Check("fill-API-off-confirmed", off.Ok);
            Set(editor, "_lispRunning", false);
            Call(editor, "CompleteSignFill", document, generation, off);
            editor.RefreshAll();
            Check("fill-off-persists-after-completion-and-refresh", !fill.Checked && fill.Enabled && Field(editor, "_pendingSignFill") == null);
            Set(editor, "_pendingSignFill", (bool?)true); Set(editor, "_lispRunning", true);
            editor.RefreshAll();
            Check("fill-refresh-keeps-pending-on", fill.Checked && !fill.Enabled);
            Set(editor, "_lispRunning", false);
            Call(editor, "CompleteSignFill", document, generation, LispReply.FromStrings(new[] { "LOI", "Rejected fill operation" }));
            Check("fill-failure-restores-confirmed-off-and-reports", !fill.Checked && fill.Enabled && status.Text.Contains("Rejected fill operation"));
            Set(editor, "_pendingSignFill", (bool?)true); Set(editor, "_lispRunning", true);
            Call(editor, "CompleteSignFill", document, generation - 1, off);
            Check("fill-old-completion-ignored", (bool?)Field(editor, "_pendingSignFill") == true && (bool)Field(editor, "_lispRunning"));
            editor.Unbind();
            Check("fill-unbind-clears-pending-and-disables", Field(editor, "_pendingSignFill") == null && !fill.Enabled);
            editor.BindTo(document);
            Set(editor, "_lispOk", true);
            Call(editor, "RefreshSignScale");
            var scaleButton = (Button)Field(editor, "_signScaleButton");
            Check("scale-button-enabled-with-ready-drawing", scaleButton.Enabled);
            Check("fill-API-on-after-off-confirmed", api.Call("bht:api-sign-fill", "1").Ok);
            Call(editor, "RefreshSignFill");
            Check("fill-rebind-reads-drawing-and-enables", fill.Checked && fill.Enabled);
            fill.Checked = false;
            Check("fill-busy-rejection-keeps-confirmed-state", fill.Checked && !((bool?)Field(editor, "_pendingSignFill")).HasValue && status.Text.Contains("Chưa đổi tô nền"));
            Set(editor, "_lispRunning", true); Call(editor, "UpdateSignFillAvailability");
            Check("fill-disabled-during-other-Lisp-operation", !fill.Enabled);
            Check("scale-disabled-during-other-Lisp-operation", !scaleButton.Enabled);
            Set(editor, "_lispRunning", false); Call(editor, "UpdateSignFillAvailability");
            var rtkScale = (Button)Field(editor, "_rtkScaleButton");
            var rtkUpdate = (Button)Field(editor, "_rtkUpdateLabelsButton");
            var rtkRotate = (Button)Field(editor, "_rtkRotateButton");
            Check("RTK-rotation-action-enabled",rtkRotate.Enabled && rtkRotate.Text=="Gom / xoay nhãn…");
            Check("RTK-independent-actions-enabled", rtkScale.Enabled && rtkUpdate.Enabled && rtkScale.Text == "Tỷ lệ ký hiệu và nhãn RTK…" && rtkUpdate.Text == "Cập nhật nhãn RTK");
            Set(editor, "_lispRunning", true); Call(editor, "UpdateSignFillAvailability");
            Check("RTK-actions-disabled-while-busy", !rtkScale.Enabled && !rtkUpdate.Enabled);
            Check("RTK-rotation-disabled-while-busy",!rtkRotate.Enabled);
            Set(editor, "_lispRunning", false); Call(editor, "UpdateSignFillAvailability");
            Check("RTK-scale-registered-through-acedInvoke", api.Call("bht:api-rtk-scale", "2", "0.75").Ok);
            Check("RTK-rotation-registered-through-acedInvoke",api.Call("bht:api-label-rotate","UCS").Ok);
            tabs.SelectedTab = (TabPage)Field(editor, "_tabPoints");
            var points = Enumerable.Range(0, 70).Select(n => new SurveyPoint { Id = "UI-RTK-" + n.ToString("000"), Name = n.ToString(), Description = "Điểm thử", Class = "CHUA_XAC_DINH" }).ToList();
            Set(editor, "_points", points); Call(editor, "FillPointList"); Application.DoEvents();
            var pointList = (ListView)Field(editor, "_ptList");
            Set(editor, "_suppressSel", true); pointList.Items[35].Selected = true; pointList.Items[36].Selected = true; Set(editor, "_suppressSel", false);
            pointList.TopItem = pointList.Items[33];
            string pointTop = ((SurveyPoint)pointList.TopItem.Tag).Id;
            Call(editor, "FillPointList"); Application.DoEvents();
            Check("RTK-refresh-keeps-multiple-selection", pointList.SelectedItems.Count == 2 && pointList.SelectedItems.Cast<ListViewItem>().All(i => ((SurveyPoint)i.Tag).Id == "UI-RTK-035" || ((SurveyPoint)i.Tag).Id == "UI-RTK-036"));
            Check("RTK-refresh-keeps-list-top", ((SurveyPoint)pointList.TopItem.Tag).Id == pointTop);
            ((TextBox)Field(editor, "_ptSearch")).Text = "UI-RTK-0"; Application.DoEvents();
            Check("RTK-filter-keeps-visible-selection-and-top", pointList.SelectedItems.Count == 2 && ((SurveyPoint)pointList.TopItem.Tag).Id == pointTop);
            foreach (var size in new[] { new Size(340, 600), new Size(340, 850), new Size(480, 850) })
            {
                host.ClientSize = size; Application.DoEvents();
                foreach (var button in new[] { rtkScale, rtkUpdate, rtkRotate })
                    Check("RTK-action-visible-" + size.Width + "-" + size.Height + "-" + button.Text, editor.ClientRectangle.Contains(editor.PointToClient(button.PointToScreen(new Point(button.Width - 1, button.Height - 1)))));
                using (var bitmap = new Bitmap(editor.Width, editor.Height)) { editor.DrawToBitmap(bitmap, editor.ClientRectangle); bitmap.Save(Path.Combine(output, "RTK-" + size.Width + "-" + size.Height + ".png")); }
            }
            editor.Unbind(); Check("RTK-actions-disabled-without-drawing", !rtkScale.Enabled && !rtkUpdate.Enabled);
            Check("RTK-rotation-disabled-without-drawing",!rtkRotate.Enabled);
            editor.BindTo(document); Set(editor, "_lispOk", true); Call(editor, "UpdateSignFillAvailability");
            tabs.SelectedTab = (TabPage)Field(editor, "_tabObjects"); host.ClientSize = new Size(480, 850); Application.DoEvents();
            using (var tr = document.Database.TransactionManager.StartTransaction())
            {
                for (int n = 0; n < 70; n++)
                    BhtStore.Write(tr, document.Database, "OBJ", "UI-L" + n.ToString("000"), new BhtRecord().Add(ObjFields.Group, "BIEN_BAO").Add(ObjFields.Code, "P.127-80").Add(ObjFields.Desc, "Biển tốc độ " + n));
                tr.Commit();
            }
            Call(editor, "MarkObjectEditorClean"); Call(editor, "RefreshObjects");
            Call(editor, "SelectObjectInList", "UI-L035");
            var deleteButton=(Button)Field(editor,"_deleteObjectButton");
            Check("saved-object-delete-enabled",deleteButton.Enabled);
            Set(editor,"_lispRunning",true);Call(editor,"UpdateObjectEditorState");
            Check("running-command-delete-disabled",!deleteButton.Enabled);
            Set(editor,"_lispRunning",false);Call(editor,"UpdateObjectEditorState");
            Check("delete-enabled-after-command",deleteButton.Enabled);
            Check("delete-button-outside-scroll-form",deleteButton.Parent!=Field(editor,"_objectForm"));
            list.TopItem = list.Items.Cast<ListViewItem>().Single(item => (string)item.Tag == "UI-L033");
            string topBeforeSave = list.TopItem.Tag as string;
            ((TextBox)Field(editor, "_oNote")).Text = "Lưu ở giữa danh sách";
            using (var tr = document.Database.TransactionManager.StartTransaction())
            {
                BhtStore.Write(tr, document.Database, "OBJ", "UI-L035", new BhtRecord().Add(ObjFields.Group, "BIEN_BAO").Add(ObjFields.Code, "P.127-80").Add(ObjFields.Desc, "Biển tốc độ 35").Add(ObjFields.Note, "Lưu ở giữa danh sách"));
                tr.Commit();
            }
            Call(editor, "MarkObjectEditorClean"); editor.RefreshAll();
            Application.DoEvents();
            Check("saved-record-refresh-preserves-object-list-top", (string)list.TopItem.Tag == topBeforeSave);
            editor.RefreshAll(); Application.DoEvents();
            Check("refresh-preserves-object-list-top", (string)list.TopItem.Tag == topBeforeSave);
            ((TextBox)Field(editor, "_objSearch")).Text = "UI-L0"; Application.DoEvents();
            Check("filter-preserves-top-record-when-visible", (string)list.TopItem.Tag == topBeforeSave);
            ((TextBox)Field(editor, "_objSearch")).Text = "";
            foreach (var size in new[] { new Size(340, 600), new Size(340, 850), new Size(480, 850) })
            {
                int width = size.Width;
                host.ClientSize = size;
                Application.DoEvents();
                Call(editor, "ClearObjectEditor");
                var form = (TableLayoutPanel)Field(editor, "_objectForm");
                var info = (TextBox)Field(editor, "_oInfo");
                info.Focus(); form.ScrollControlIntoView(info);
                Application.DoEvents();
                Call(editor, "NewObjectFromPoints", new System.Collections.Generic.List<string> { "UI-NEW-POINT" });
                Application.DoEvents();
                Check("new-record-at-top-" + width + "-" + size.Height, form.AutoScrollPosition.Y == 0 && ((ComboBox)Field(editor, "_oGroup")).Focused);
                var group = (ComboBox)Field(editor, "_oGroup");
                group.SelectedIndex = 4;
                ((TextBox)Field(editor, "_oDesc")).Text = "Bảng quảng cáo cửa hàng";
                Application.DoEvents();
                Check("advertising-name-label-" + width, ((Label)Field(editor, "_objectNameLabel")).Text == "Nội dung bảng");
                Check("group-change-stays-at-top-" + width, form.AutoScrollPosition.Y == 0);
                Call(editor, "MarkObjectEditorClean");
                ((TextBox)Field(editor, "_oNote")).Focus();
                int beforeEdit = form.AutoScrollPosition.Y;
                ((TextBox)Field(editor, "_oNote")).Text = "Ghi chú chưa lưu";
                Application.DoEvents();
                Check("dirty-status-does-not-jump-" + width, form.AutoScrollPosition.Y == beforeEdit);
                                group.SelectedIndex=group.Items.Cast<object>().Select((item,index)=>new{item,index}).Single(x=>x.item.ToString()=="Đèn tín hiệu").index;
                Check("signal-group-code-"+width,((BhtRecord)Call(editor,"EditorFields")).Get(ObjFields.Group)=="DEN_TH");
                Check("signal-picker-visible-"+width,((Button)Field(editor,"_lightButton")).Visible);
                Check("signal-types-have-shipped-dwg-"+width,LightModels.ForGroup("DEN_TH").Length==4 && LightModels.ForGroup("DEN_TH").All(m=>File.Exists(Path.Combine(CustomDwgBlock.LightFolder(),m[0]+".dwg"))));
                group.SelectedIndex=group.Items.Cast<object>().Select((item,index)=>new{item,index}).Single(x=>x.item.ToString()=="Đèn chiếu sáng").index;
                Check("lighting-group-code-"+width,((BhtRecord)Call(editor,"EditorFields")).Get(ObjFields.Group)=="DEN_CS");
                Check("lighting-types-have-shipped-dwg-"+width,LightModels.ForGroup("DEN_CS").Length==5 && LightModels.ForGroup("DEN_CS").All(m=>File.Exists(Path.Combine(CustomDwgBlock.LightFolder(),m[0]+".dwg"))));
                group.SelectedIndex=group.Items.Cast<object>().Select((item,index)=>new{item,index}).Single(x=>x.item.ToString()=="Khác").index;
                Check("other-hides-light-picker-"+width,!((Button)Field(editor,"_lightButton")).Visible);
                Check("other-name-label-" + width, ((Label)Field(editor, "_objectNameLabel")).Text == "Nội dung bảng");
                Call(editor, "MarkObjectEditorClean");
                Call(editor, "LoadObject", "UI-001");
                form.ScrollControlIntoView(info);
                Application.DoEvents();
                int beforeRefresh = form.AutoScrollPosition.Y;
                editor.RefreshAll(); Application.DoEvents();
                Check("clean-refresh-keeps-scroll-" + width, form.AutoScrollPosition.Y == beforeRefresh);
                using (var picker = new SignPickerForm("S.509a@4.75", "", "W.239a; S.509a@4.75"))
                {
                    typeof(SignPickerForm).GetMethod("AcceptSign", Hidden).Invoke(picker, null);
                    Check("picker-confirmed-" + width, picker.SelectedCode == "W.239a");
                    int beforePicker = form.AutoScrollPosition.Y;
                    Call(editor, "KeepObjectScroll", (Action)(() => {
                        ((ComboBox)Field(editor, "_oGroup")).Focus();
                        Call(editor, "ApplyPickedSign", picker);
                        Application.DoEvents();
                    }));
                    Application.DoEvents();
                    Check("picker-return-keeps-form-scroll-" + width, form.AutoScrollPosition.Y == beforePicker);
                    Check("picker-return-keeps-face-codes-" + width, ((ComboBox)Field(editor, "_oFaceCodes")).Text.Contains("S.509a@4.75"));
                }
                int beforeScale = form.AutoScrollPosition.Y;
                string draftBeforeScale = ((TextBox)Field(editor, "_oNote")).Text;
                bool dirtyBeforeScale = (bool)Call(editor, "IsObjectEditorDirty");
                using (var closeScale = new Timer { Interval = 50 })
                {
                    closeScale.Tick += (s, e) => {
                        var dialog = Application.OpenForms.Cast<Form>().OfType<SignScaleForm>().FirstOrDefault();
                        if (dialog == null) return;
                        closeScale.Stop(); dialog.DialogResult = DialogResult.Cancel; dialog.Close();
                    };
                    closeScale.Start(); Call(editor, "ChooseSignScale"); closeScale.Stop();
                }
                Application.DoEvents();
                Check("scale-dialog-keeps-draft-and-scroll-" + width, form.AutoScrollPosition.Y == beforeScale && ((TextBox)Field(editor, "_oNote")).Text == draftBeforeScale && (bool)Call(editor, "IsObjectEditorDirty") == dirtyBeforeScale);
                Call(editor, "ClearObjectEditor");
                Call(editor, "NewObjectFromPoints", new System.Collections.Generic.List<string> { "UI-NEW-POINT" });
                group.SelectedIndex = 4;
                ((TextBox)Field(editor, "_oDesc")).Text = "Cửa hàng vật liệu xây dựng";
                Application.DoEvents();
                foreach (string name in new[] { "_saveObjectButton", "_placeObjectButton", "_insertObjectButton", "_signScaleButton" })
                {
                    var button = (Button)Field(editor, name);
                    Check("action-within-panel-" + width + "-" + name, button.Right <= button.Parent.ClientSize.Width && button.Width > 90 && button.Height >= 30);
                }
                var actionGrid = (TableLayoutPanel)scaleButton.Parent;
                Check("grouped-symbol-actions-" + width, actionGrid.GetRow(scaleButton) == 3 && actionGrid.GetRow((Button)Field(editor, "_placeObjectButton")) == 2 && actionGrid.GetRow((Button)Field(editor, "_insertObjectButton")) == 2);
                Check("point-actions-next-to-points-" + width, Children(form).OfType<Button>().Any(b => b.Text == "Gắn điểm có sẵn…") && Children(form).OfType<Button>().Any(b => b.Text == "Gỡ điểm chọn"));
                Check("manual-marker-action-"+width, Children(editor).OfType<Button>().Any(b=>b.Text=="Thêm cọc…"));
                Call(editor, "MarkObjectEditorClean"); Call(editor, "SelectObjectInList", "UI-001");
                form.AutoScrollPosition = Point.Empty; Application.DoEvents();
                using (var bitmap = new Bitmap(editor.Width, editor.Height))
                {
                    editor.DrawToBitmap(bitmap, editor.ClientRectangle);
                    bitmap.Save(Path.Combine(output, "objects-" + width + "-" + size.Height + ".png"));
                }
            }
            tabs.SelectedTab=tabs.TabPages.Cast<TabPage>().Single(p=>p.Text=="Tuyến & báo cáo");
            host.ClientSize=new Size(480,850);Application.DoEvents();
            var advancedButton=Children(editor).OfType<Button>().Single(b=>b.Text=="Nâng cao: nhóm đường / nhánh ▸");
            var createGroup=Children(editor).OfType<Button>().Single(b=>b.Text=="Tạo nhóm…");
            var alternative=Children(editor).OfType<CheckBox>().Single(c=>c.Text=="Phương pháp theo cọc / nạp nguyên dự án");
            Check("122-field-method-hidden-from-route-workflow",!Children(editor).OfType<Button>().Single(b=>b.Text=="Mốc lý trình đã nhập…").Visible);
            alternative.Checked=true;Application.DoEvents();
            Check("input-review-main-action-visible",Children(editor).OfType<Button>().Single(b=>b.Text=="Mốc lý trình đã nhập…").Visible);
            Check("input-advanced-hidden-by-default",!createGroup.Visible);
            using(var bitmap=new Bitmap(editor.Width,editor.Height)){editor.DrawToBitmap(bitmap,editor.ClientRectangle);bitmap.Save(Path.Combine(output,"imported-main.png"));}
            advancedButton.PerformClick();Application.DoEvents();Check("input-advanced-opens",createGroup.Visible);
            advancedButton.PerformClick();Application.DoEvents();Check("input-advanced-closes",!createGroup.Visible);
        }
        foreach(string value in new[]{"4,5","0","-1","100001","NaN","Infinity",""})
        using(var distance = new SymbolDistanceForm("5"))
        {
            distance.StartPosition=FormStartPosition.Manual;distance.Location=new Point(-3000,-3000);distance.Show();Application.DoEvents();
            var input=Children(distance).OfType<TextBox>().Single(t=>t.Name=="distanceInput");input.Text=value;
            foreach(var button in Children(distance).OfType<Button>())
                Check("distance-dialog-fits-"+value+button.Text,distance.ClientRectangle.Contains(distance.PointToClient(button.PointToScreen(new Point(button.Width-1,button.Height-1)))));
            if(value=="4,5")using(var bitmap=new Bitmap(distance.Width,distance.Height)){distance.DrawToBitmap(bitmap,distance.ClientRectangle);bitmap.Save(Path.Combine(output,"symbol-distance.png"));}
            Children(distance).OfType<Button>().Single(b=>b.Text=="Áp dụng tất cả").PerformClick();
            bool valid=value=="4,5"||value=="0";
            Check("distance-dialog-validation-"+value,valid?distance.DialogResult==DialogResult.OK&&distance.Distance==(value=="0"?"0":"4.5"):distance.DialogResult==DialogResult.None&&distance.Distance==null);
            if(!valid){Children(distance).OfType<Button>().Single(b=>b.Text=="Hủy").PerformClick();Check("distance-dialog-cancel-"+value,distance.DialogResult==DialogResult.Cancel&&distance.Distance==null);}
        }
        foreach(string mode in new[]{"THANG","GAP_KHUC"})
        using(var distance=new SymbolDistanceForm("3",mode))
        {
            distance.StartPosition=FormStartPosition.Manual;distance.Location=new Point(-3000,-3000);distance.Show();Application.DoEvents();
            Check("105-leader-mode-restored-"+mode,Children(distance).OfType<ComboBox>().Single(c=>c.Name=="leaderMode").SelectedIndex==(mode=="THANG"?0:1));
            Children(distance).OfType<Button>().Single(b=>b.Text=="Áp dụng tất cả").PerformClick();
            Check("105-leader-mode-accepted-"+mode,distance.LeaderMode==mode&&distance.Distance=="3");
        }
        foreach(string width in new[]{"8,5","0","","-1","NaN","Infinity","100001"})
        using(var distance=new SymbolDistanceForm("1","THANG",width,"5",true))
        {
            distance.StartPosition=FormStartPosition.Manual;distance.Location=new Point(-3000,-3000);distance.Show();Application.DoEvents();
            Check("106-road-selected-"+width,Children(distance).OfType<RadioButton>().Single(r=>r.Name=="outsideRoad").Checked);
            foreach(var control in Children(distance).Where(c=>c.Visible&&(c is Button||c is TextBox||c is Label||c is ComboBox||c is RadioButton)))
                Check("106-road-dialog-fits-"+width+control.Name+control.Text,distance.ClientRectangle.Contains(distance.PointToClient(control.PointToScreen(new Point(control.Width-1,control.Height-1)))));
            if(width=="8,5")using(var bitmap=new Bitmap(distance.Width,distance.Height)){distance.DrawToBitmap(bitmap,distance.ClientRectangle);bitmap.Save(Path.Combine(output,"road-placement.png"));}
            Children(distance).OfType<Button>().Single(b=>b.Text=="Áp dụng tất cả").PerformClick();
            bool valid=width=="8,5"||width=="0";
            Check("106-road-width-validation-"+width,valid?distance.DialogResult==DialogResult.OK&&distance.OutsideRoad&&distance.LeftWidth==(width=="0"?"0":"8.5")&&distance.RightWidth=="5":distance.DialogResult==DialogResult.None&&distance.Distance==null);
        }
        double parsed;
        using(var distance=new SymbolDistanceForm("12,5","THANG","","",true,new[]{"TIM_A","TIM_B"},"TIM_B",true))
        {
            distance.StartPosition=FormStartPosition.Manual;distance.Location=new Point(-3000,-3000);distance.Show();Application.DoEvents();
            Check("106-center-route-restored",Children(distance).OfType<ComboBox>().Single(c=>c.Name=="referenceRoute").Text=="TIM_B");
            Check("106-center-widths-disabled",!Children(distance).OfType<TextBox>().Single(t=>t.Name=="leftWidth").Enabled);
            using(var bitmap=new Bitmap(distance.Width,distance.Height)){distance.DrawToBitmap(bitmap,distance.ClientRectangle);bitmap.Save(Path.Combine(output,"center-placement.png"));}
            Children(distance).OfType<Button>().Single(b=>b.Text=="Áp dụng tất cả").PerformClick();
            Check("106-center-accepts-without-road-widths",distance.DialogResult==DialogResult.OK&&distance.ReferenceRoute=="TIM_B"&&distance.Distance=="12.5"&&!distance.OutsideRoad);
        }
        foreach(string type in new[]{"KHAC","TIM_DUONG","TIM_RANH",""})
        using(var distance=new SymbolDistanceForm("12,5","THANG","","",true,new[]{"TUYEN1"},"TUYEN1",true,new[]{"TUYEN1 — "+SymbolDistanceForm.RouteTypeLabel(type)}))
        {
            distance.StartPosition=FormStartPosition.Manual;distance.Location=new Point(-3000,-3000);distance.Show();Application.DoEvents();
            Check("107-loaded-reference-listed-"+type,Children(distance).OfType<ComboBox>().Single(c=>c.Name=="referenceRoute").Text.Contains(SymbolDistanceForm.RouteTypeLabel(type)));
            Check("107-loaded-reference-enabled-"+type,Children(distance).OfType<RadioButton>().Single(r=>r.Name=="centerReference").Checked);
            foreach(var control in Children(distance).Where(c=>c.Visible&&(c is Button||c is TextBox||c is Label||c is ComboBox||c is RadioButton)))
                Check("107-center-dialog-fits-"+type+control.Name+control.Text,distance.ClientRectangle.Contains(distance.PointToClient(control.PointToScreen(new Point(control.Width-1,control.Height-1)))));
            if(type=="KHAC")using(var bitmap=new Bitmap(distance.Width,distance.Height)){distance.DrawToBitmap(bitmap,distance.ClientRectangle);bitmap.Save(Path.Combine(output,"center-loaded-route.png"));}
            Children(distance).OfType<Button>().Single(b=>b.Text=="Áp dụng tất cả").PerformClick();
            Check("107-reference-returns-ID-without-display-label-"+type,distance.DialogResult==DialogResult.OK&&distance.ReferenceRoute=="TUYEN1"&&distance.Distance=="12.5");
        }
        using (var scale = new RtkScaleForm("2", "0,75"))
        {
            scale.StartPosition = FormStartPosition.Manual; scale.Location = new Point(-3000, -3000);
            scale.Show(); Application.DoEvents(); Call(scale, "AcceptScale");
            Check("RTK-dialog-independent-comma-values", scale.DialogResult == DialogResult.OK && scale.SymbolScale == "2" && scale.LabelScale == "0.75");
        }
        foreach(string mode in new[]{"UCS","PICK","CANCEL"})
        using(var rotate = new RtkRotationForm())
        {
            rotate.StartPosition=FormStartPosition.Manual;rotate.Location=new Point(-3000,-3000);rotate.Show();Application.DoEvents();
            var actions=Children(rotate).OfType<Button>().ToArray();
            foreach(var button in actions) Check("RTK-rotation-dialog-fits-"+mode+button.Text,rotate.ClientRectangle.Contains(rotate.PointToClient(button.PointToScreen(new Point(button.Width-1,button.Height-1)))));
            if(mode=="UCS") using(var bitmap=new Bitmap(rotate.Width,rotate.Height)){rotate.DrawToBitmap(bitmap,rotate.ClientRectangle);bitmap.Save(Path.Combine(output,"RTK-rotation.png"));}
            actions.Single(b=>mode=="UCS"?b.Text=="Theo UCS hiện tại":mode=="PICK"?b.Text.StartsWith("Chọn hướng"):b.Text=="Hủy").PerformClick();
            Check("RTK-rotation-dialog-choice-"+mode,mode=="CANCEL"?rotate.Mode==null&&rotate.DialogResult==DialogResult.Cancel:rotate.Mode==mode&&rotate.DialogResult==DialogResult.OK);
        }
        using (var scale = new RtkScaleForm("1", "1"))
        {
            scale.StartPosition = FormStartPosition.Manual; scale.Location = new Point(-3000, -3000);
            scale.Show(); Application.DoEvents();
            ((ComboBox)Field(scale, "label")).Text = "0"; Call(scale, "AcceptScale");
            Check("RTK-dialog-invalid-stays-open", scale.DialogResult == DialogResult.None && ((Label)Field(scale, "error")).Text.Length > 0 && scale.LabelScale == null);
            Children(scale).OfType<Button>().Single(b => b.Text == "Về chuẩn 1:1").PerformClick(); Application.DoEvents();
            Check("RTK-dialog-reset-both-values", ((ComboBox)Field(scale, "symbol")).Text == "1" && ((ComboBox)Field(scale, "label")).Text == "1");
            foreach (var button in Children(scale).OfType<Button>())
                Check("RTK-dialog-visible-" + button.Text, scale.ClientRectangle.Contains(scale.PointToClient(button.PointToScreen(new Point(button.Width - 1, button.Height - 1)))));
            using (var bitmap = new Bitmap(scale.Width, scale.Height)) { scale.DrawToBitmap(bitmap, scale.ClientRectangle); bitmap.Save(Path.Combine(output, "RTK-scale.png")); }
            Children(scale).OfType<Button>().Single(b => b.Text == "Hủy").PerformClick();
            Check("RTK-dialog-cancel-no-values", scale.DialogResult == DialogResult.Cancel && scale.SymbolScale == null && scale.LabelScale == null);
        }
        foreach (string valid in new[] { "0.01", "0,75", "1", "2.125", "100" })
            Check("scale-input-valid-" + valid, SignScaleForm.TryScale(valid, out parsed));
        foreach (string invalid in new[] { "", "0", "-1", "0.001", "101", "1.0001", "NaN", "Infinity", "1e2", "1:100" })
            Check("scale-input-invalid-" + invalid, !SignScaleForm.TryScale(invalid, out parsed));
        using (var scale = new SignScaleForm("2", "0.75"))
        {
            scale.StartPosition = FormStartPosition.Manual; scale.Location = new Point(-3000, -3000);
            scale.Show(); Application.DoEvents();
            Call(scale, "AcceptScale");
            Check("scale-dialog-independent-normalized-values", scale.DialogResult == DialogResult.OK && scale.SymbolScale == "2" && scale.LabelScale == "0.75");
        }
        using (var scale = new SignScaleForm("1", "1"))
        {
            scale.StartPosition = FormStartPosition.Manual; scale.Location = new Point(-3000, -3000);
            scale.Show(); Application.DoEvents();
            ((ComboBox)Field(scale, "label")).Text = "0"; Call(scale, "AcceptScale");
            Check("scale-dialog-invalid-stays-open", scale.DialogResult == DialogResult.None && ((Label)Field(scale, "error")).Text.Length > 0 && scale.LabelScale == null);
            Children(scale).OfType<Button>().Single(b => b.Text == "Về chuẩn 1:1").PerformClick();
            Check("scale-dialog-reset-both-controls", ((ComboBox)Field(scale, "symbol")).Text == "1" && ((ComboBox)Field(scale, "label")).Text == "1");
            foreach (var button in Children(scale).OfType<Button>())
                Check("scale-dialog-visible-" + button.Text, scale.ClientRectangle.Contains(scale.PointToClient(button.PointToScreen(new Point(button.Width - 1, button.Height - 1)))));
            using (var bitmap = new Bitmap(scale.Width, scale.Height)) { scale.DrawToBitmap(bitmap, new Rectangle(Point.Empty, scale.Size)); bitmap.Save(Path.Combine(output, "sign-scale.png")); }
            Children(scale).OfType<Button>().Single(b => b.Text == "Hủy").PerformClick();
            Check("scale-dialog-cancel-no-values", scale.DialogResult == DialogResult.Cancel && scale.SymbolScale == null && scale.LabelScale == null);
        }
        foreach (float factor in new[] { 1f, 1.25f, 1.5f, 2f })
        using (var scale = new SignScaleForm("2", "3", "PERPENDICULAR"))
        {
            scale.StartPosition = FormStartPosition.Manual; scale.Location = new Point(-5000, -5000);
            scale.Show(); scale.Scale(new SizeF(factor, factor)); Application.DoEvents();
            scale.ClientSize = new Size((int)(440 * factor), (int)(270 * factor)); Application.DoEvents();
            foreach (var button in Children(scale).OfType<Button>())
            {
                var bounds = scale.RectangleToClient(button.RectangleToScreen(button.ClientRectangle));
                Check("scale-action-visible-DPI-" + factor + "-" + button.Text, button.Visible && scale.ClientRectangle.Contains(bounds));
            }
            Check("scale-orientation-restored-" + factor, ((ComboBox)Field(scale, "orientation")).SelectedIndex == 1);
            Children(scale).OfType<Button>().Single(b => b.Text == "Áp dụng").PerformClick();
            Check("scale-orientation-accepted-" + factor, scale.DialogResult == DialogResult.OK && scale.LabelOrientation == "PERPENDICULAR");
        }
    }
    static void TestFieldForms(string output)
    {
        var asm=typeof(BhtPaletteControl).Assembly;
        var markerType=asm.GetType("BHT.Palette.FieldMarkerChoice",true);
        var formType=asm.GetType("BHT.Palette.FieldChainForm",true);
        var list=(System.Collections.IList)Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(markerType));
        for(int i=0;i<3;i++) {
            var m=Activator.CreateInstance(markerType,true);
            markerType.GetField("Id").SetValue(m,"COC"+i);markerType.GetField("Label").SetValue(m,"COC"+i+" | H"+i+"/40");
            markerType.GetField("Station").SetValue(m,40000.0+100*i);markerType.GetField("X").SetValue(m,100.0*i);markerType.GetField("Y").SetValue(m,0.0);list.Add(m);
        }
        foreach(bool pair in new[]{false,true}) using(var form=(Form)Activator.CreateInstance(formType,new object[]{list,null,pair})) {
            form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-3000,-3000);form.Show();Application.DoEvents();
            var available=(ListBox)Field(form,"available");for(int i=0;i<3;i++)available.SetSelected(i,true);
            Children(form).OfType<Button>().Single(b=>b.Text=="Thêm →").PerformClick();
            var ordered=(ListBox)Field(form,"ordered");
            Check("field-selected-count-"+pair,ordered.Items.Count==(pair?2:3));
            Check("field-review-shows-span",((TextBox)Field(form,"review")).Text.Contains("100"));
            ordered.SelectedIndex=1;Children(form).OfType<Button>().Single(b=>b.Text=="Lên").PerformClick();
            Check("field-order-warning-"+pair,((TextBox)Field(form,"review")).Text.Contains("LỖI"));
            Children(form).OfType<Button>().Single(b=>b.Text=="Xuống").PerformClick();
            Check("field-order-corrected-"+pair,!((TextBox)Field(form,"review")).Text.Contains("LỖI"));
            using(var bmp=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bmp,form.ClientRectangle);bmp.Save(Path.Combine(output,pair?"field-pair.png":"field-chain.png"));}
            foreach(var button in Children(form).OfType<Button>())Check("field-button-visible-"+pair+button.Text,form.ClientRectangle.Contains(form.PointToClient(button.PointToScreen(new Point(button.Width-1,button.Height-1)))));
            Children(form).OfType<Button>().Single(b=>b.Text=="Hủy").PerformClick();Check("field-cancel-"+pair,form.DialogResult==DialogResult.Cancel);
        }
    }

}

