using System;using System.Drawing;using System.Globalization;using System.Windows.Forms;
namespace BHT.Palette {
 public sealed class SymbolDistanceForm:Form {
  public string Distance {get;private set;}
  public string LeaderMode {get;private set;}
  public bool OutsideRoad {get;private set;}
  public string LeftWidth {get;private set;}
  public string RightWidth {get;private set;}
  public string ReferenceRoute {get;private set;}
  private sealed class RouteItem {
   public string Id,Label;
   public override string ToString(){return Label;}
  }
  public static string RouteTypeLabel(string type){
   switch((type??"").Trim().ToUpperInvariant()){
    case "TIM_DUONG":return "Tim đường";
    case "TIM_RANH":return "Tim rãnh";
    case "KHAC":return "Khác";
    case "":return "Chưa phân loại";
    default:return type;
   }
  }
  public SymbolDistanceForm(string current):this(current,"GAP_KHUC"){}
  public SymbolDistanceForm(string current,string mode):this(current,mode,"","",false){}
  public SymbolDistanceForm(string current,string mode,string left,string right,bool outside):this(current,mode,left,right,outside,new string[0],"",false){}
  public SymbolDistanceForm(string current,string mode,string left,string right,bool outside,string[] routes,string selectedRoute,bool byCenter,string[] routeLabels=null){
   Text="Bố trí tất cả báo hiệu";Font=new Font("Segoe UI",9f);AutoScaleMode=AutoScaleMode.Dpi;
   ClientSize=new Size(480,510);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=MinimizeBox=false;StartPosition=FormStartPosition.CenterParent;
   var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(12),ColumnCount=1};
   var center=new RadioButton{Text=routes.Length>0?"Cách tim: chọn tuyến đã nạp":"Cách tim: chọn tuyến đã nạp (chưa có tuyến)",AutoSize=true,Name="centerReference",Enabled=routes.Length>0};
   var route=new NoWheelComboBox{Name="referenceRoute",DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill};
   for(int i=0;i<routes.Length;i++)route.Items.Add(new RouteItem{Id=routes[i],Label=routeLabels!=null&&i<routeLabels.Length?routeLabels[i]:routes[i]});
   route.SelectedIndex=routes.Length>0?Math.Max(0,Array.IndexOf(routes,selectedRoute)):-1;
   var road=new RadioButton{Text="Đưa ra ngoài đường theo PL tham chiếu",AutoSize=true,Name="outsideRoad"};
   var radial=new RadioButton{Text="Đổi khoảng cách từ điểm đo theo hướng đang đặt",AutoSize=true};
   var widths=new TableLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,ColumnCount=2};
   widths.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));widths.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
   var li=new TextBox{Name="leftWidth",Text=left,Dock=DockStyle.Fill};var ri=new TextBox{Name="rightWidth",Text=right,Dock=DockStyle.Fill};
   widths.Controls.Add(new Label{Text="PL → mép trái",AutoSize=true},0,0);widths.Controls.Add(new Label{Text="PL → mép phải",AutoSize=true},1,0);
   widths.Controls.Add(li,0,1);widths.Controls.Add(ri,1,1);
   var label=new Label{AutoSize=true,MaximumSize=new Size(450,0)};
   var input=new TextBox{Name="distanceInput",Text=current,Dock=DockStyle.Fill};
   var modeLabel=new Label{Text="Kiểu đường nối",AutoSize=true};
   var leader=new NoWheelComboBox{Name="leaderMode",DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill};
   leader.Items.AddRange(new object[]{"Đường thẳng","Gấp khúc tránh mặt biển"});leader.SelectedIndex=mode=="THANG"?0:1;
   var note=new Label{AutoSize=true,MaximumSize=new Size(450,0)};
   Action refresh=()=>{widths.Visible=road.Checked;route.Visible=center.Checked&&routes.Length>0;widths.Enabled=road.Checked;route.Enabled=center.Checked;label.Text=center.Checked?"Khoảng cách tối thiểu từ tim đến hình biển và nhãn":road.Checked?"Khoảng lùi từ mép đường đến hình biển và nhãn":"Khoảng cách từ điểm đo đến gốc ký hiệu";
    note.Text=(center.Checked?"Chọn tuyến dùng làm tim bố trí biển, kể cả khi loại đang là Khác. Đơn vị bản vẽ. Giữ loại tuyến, mốc, lý trình, điểm đo và tỷ lệ biển. Hướng biển theo tiếp tuyến tại từng điểm.":road.Checked?"Đơn vị bản vẽ. Sau khi xác nhận, chọn PL và đầu PL trong CAD. Trái/phải theo chiều đầu → cuối. Giữ điểm đo, lý trình và tỷ lệ biển. Hướng biển theo tiếp tuyến PL tại từng điểm.":"Đơn vị bản vẽ. Giữ hướng dịch chuyển, tỷ lệ biển và điểm đo. Hướng biển theo tuyến của hồ sơ; thiếu tuyến thì giữ góc cũ.")
      +"\nCọc tiêu giữ chân tại điểm đo, vuông góc tuyến; không áp dụng khoảng lùi hoặc đường nối. Cọc thiếu tuyến giữ góc cũ và được báo riêng. Cột Km giữ nguyên.";};
   center.CheckedChanged+=(s,e)=>refresh();road.CheckedChanged+=(s,e)=>refresh();radial.CheckedChanged+=(s,e)=>refresh();
   var error=new Label{AutoSize=true,ForeColor=Color.Firebrick};
   var row=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,FlowDirection=FlowDirection.RightToLeft};
   var cancel=new Button{Text="Hủy",AutoSize=true,DialogResult=DialogResult.Cancel};var apply=new Button{Text="Áp dụng tất cả",AutoSize=true};
   apply.Click+=(s,e)=>{double n,l=0,r=0;if(!Number(input.Text,out n)){error.Text="Nhập khoảng cách từ 0 đến 100000.";input.Focus();return;}
    if(center.Checked&&route.SelectedIndex<0){error.Text="Chọn tim tuyến đã nạp.";return;}
    if(road.Checked&&(!Number(li.Text,out l)||!Number(ri.Text,out r))){error.Text="Nhập bề rộng hai bên từ 0 đến 100000.";return;}
    Distance=n.ToString("0.########",CultureInfo.InvariantCulture);LeaderMode=leader.SelectedIndex==0?"THANG":"GAP_KHUC";OutsideRoad=road.Checked;ReferenceRoute=center.Checked?((RouteItem)route.SelectedItem).Id:"";LeftWidth=l.ToString("0.########",CultureInfo.InvariantCulture);RightWidth=r.ToString("0.########",CultureInfo.InvariantCulture);DialogResult=DialogResult.OK;Close();};
   row.Controls.Add(cancel);row.Controls.Add(apply);layout.Controls.Add(center);layout.Controls.Add(route);layout.Controls.Add(road);layout.Controls.Add(radial);layout.Controls.Add(widths);layout.Controls.Add(label);layout.Controls.Add(input);layout.Controls.Add(modeLabel);layout.Controls.Add(leader);layout.Controls.Add(note);layout.Controls.Add(error);layout.Controls.Add(row);Controls.Add(layout);AcceptButton=apply;CancelButton=cancel;
   center.Checked=byCenter&&routes.Length>0;road.Checked=!center.Checked&&outside;radial.Checked=!center.Checked&&!road.Checked;refresh();
  }
  static bool Number(string value,out double n){return double.TryParse(value.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out n)&&!double.IsNaN(n)&&!double.IsInfinity(n)&&n>=0&&n<=100000;}
 }
}
