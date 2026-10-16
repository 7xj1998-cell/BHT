using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BHT.Bridge;
using BHT.Core;
namespace BHT.Palette {
 public sealed partial class SignPickerForm {
  private readonly Dictionary<string,CustomBoardTemplate> customBoards=new Dictionary<string,CustomBoardTemplate>(StringComparer.OrdinalIgnoreCase);
  private bool customLibraryReadable=true;
  private string customLibraryPath=CustomBoardLibrary.FilePath;
  private Control customFaceActions;
  private bool IsCustomSelected {get{return SelectedSign!=null && customBoards.ContainsKey(SelectedSign.Code);}}
  private void LoadCustomBoards() {
   AddCustomCard(new CustomBoardTemplate {Code=CustomBoardLibrary.Prefix+"SAMPLE",Name="Bảng chữ tùy chỉnh",Text="DỪNG LẠI\r\nQUAN SÁT"});
   try {foreach(var item in CustomBoardLibrary.Read(customLibraryPath))AddCustomCard(item);}
   catch(Exception e){customLibraryReadable=false;MessageBox.Show(this,"Không đọc được thư viện biển tùy chỉnh: "+e.Message+"\nTệp gốc được giữ nguyên.","BHT");}
  }
  public void ConfigureCustomBoard(string group,string code,string text) {
   if((group!="KHAC" && group!="BANG_QC" && group!="CHUA_XAC_DINH") || string.IsNullOrWhiteSpace(text))return;
   string key=(code??"").StartsWith(CustomBoardLibrary.Prefix,StringComparison.OrdinalIgnoreCase)?code:CustomBoardLibrary.Prefix+"CURRENT";
   CustomBoardTemplate template;customBoards.TryGetValue(key,out template);
   AddCustomCard(new CustomBoardTemplate {Code=key,Name=template==null?"Bảng trong hồ sơ hiện tại":template.Name,Text=text});
   groups.SelectedIndex=7;Filter();Choose(cards.Single(c=>((TdtSignEntry)c.Tag).Code==key));
  }
  private void AddCustomCard(CustomBoardTemplate item) {
   customBoards[item.Code]=item;
   var old=cards.FirstOrDefault(c=>((TdtSignEntry)c.Tag).Code==item.Code);
   if(old!=null){
    var oldPicture=old.Controls.OfType<PictureBox>().FirstOrDefault();
    if(oldPicture!=null && oldPicture.Image!=null){var previous=oldPicture.Image;if(preview.Image==previous)preview.Image=null;oldPicture.Image=null;images.Remove(previous);previous.Dispose();}
    cards.Remove(old);filteredCards.Remove(old);grid.Controls.Remove(old);old.Dispose();}
   var sign=new TdtSignEntry {Code=item.Code,Description=item.Name,Group="Biển tùy chỉnh",Provider="BHT-CUSTOM",HasVector=true};
   entries[item.Code]=sign;
   var card=new Panel {Width=160,Height=175,BorderStyle=BorderStyle.FixedSingle,Margin=new Padding(5),Tag=sign};
   var picture=new PictureBox {Dock=DockStyle.Top,Height=108,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.White};
   var label=new Label {Dock=DockStyle.Fill,TextAlign=ContentAlignment.TopCenter,Text=item.Name,Padding=new Padding(3)};
   card.Controls.Add(label);card.Controls.Add(picture);cards.Add(card);
   foreach(Control control in new Control[]{card,picture,label}){control.Click+=(s,e)=>Choose(card);control.DoubleClick+=(s,e)=>{Choose(card);if(multi.Checked)AddFace();else EditCustomTemplate(false);};}
  }
  private Image CustomBoardPreview(string code) {
   CustomBoardTemplate item;if(!customBoards.TryGetValue(code,out item))return null;
   var lines=item.Text.Replace("\r\n","\n").Replace("\r","\n").Split('\n');
   float plateHeight=(float)Math.Min(1.8,Math.Max(.65,.32*lines.Length+.26));
   float pixels=320f/1.8f;
   var image=new Bitmap(320,(int)Math.Round(plateHeight*pixels));
   using(var g=Graphics.FromImage(image))
   using(var format=new StringFormat {Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center,FormatFlags=StringFormatFlags.NoWrap}){
    g.Clear(Color.FromArgb(0,118,190));g.DrawRectangle(Pens.White,7,7,305,image.Height-15);
    float size=(float)Math.Min(.22,(plateHeight-.18)/(lines.Length*1.45))*pixels;
    foreach(var line in lines)using(var font=new Font(CustomFont.Families[0],size,FontStyle.Regular,GraphicsUnit.Pixel)){
     float width=g.MeasureString(line,font).Width;if(width>1.58*pixels)size*=1.58f*pixels/width;
    }
    using(var font=new Font(CustomFont.Families[0],size,FontStyle.Regular,GraphicsUnit.Pixel))
     for(int i=0;i<lines.Length;i++)g.DrawString(lines[i],font,Brushes.White,new PointF(160,image.Height/2f+(i-(lines.Length-1)/2f)*size*1.45f),format);
   }
   return image;
  }
  private void EditCustomTemplate(bool create) {
   if(!customLibraryReadable){MessageBox.Show(this,"Cần khắc phục lỗi đọc thư viện trước khi lưu mẫu.","BHT");return;}
   CustomBoardTemplate current=null;if(!create && IsCustomSelected)current=customBoards[SelectedSign.Code];
   if(!create && current==null){validation.Text="Chọn biển trong nhóm Biển tùy chỉnh để sửa.";return;}
   using(var dialog=new Form {Text=create?"Thêm biển tùy chỉnh":"Sửa biển tùy chỉnh",Width=550,Height=440,MinimumSize=new Size(480,400),StartPosition=FormStartPosition.CenterParent}){
    var form=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,Padding=new Padding(12),RowCount=6};
    form.RowStyles.Add(new RowStyle(SizeType.Absolute,24));form.RowStyles.Add(new RowStyle(SizeType.Absolute,32));form.RowStyles.Add(new RowStyle(SizeType.Absolute,26));form.RowStyles.Add(new RowStyle(SizeType.Percent,100));form.RowStyles.Add(new RowStyle(SizeType.Absolute,50));form.RowStyles.Add(new RowStyle(SizeType.Absolute,38));
    var name=new TextBox {Dock=DockStyle.Fill,MaxLength=80,Text=current==null?"":current.Name};
    var text=new TextBox {Dock=DockStyle.Fill,Multiline=true,AcceptsReturn=true,ScrollBars=ScrollBars.Vertical,MaxLength=2000,Text=current==null?"":current.Text};
    var error=new Label {Dock=DockStyle.Fill,ForeColor=Color.Firebrick};
    var buttons=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft};
    var save=new Button {Text="Lưu mẫu",AutoSize=true};var cancel=new Button {Text="Hủy",DialogResult=DialogResult.Cancel};
    CustomBoardTemplate saved=null;
    save.Click+=(s,e)=>{
     var item=new CustomBoardTemplate {Code=current==null || current.Code.EndsWith(".CURRENT")?CustomBoardLibrary.Prefix+Guid.NewGuid().ToString("N"):current.Code,Name=name.Text.Trim(),Text=text.Text.Trim()};
     try {CustomBoardLibrary.Save(customLibraryPath,item);saved=item;dialog.DialogResult=DialogResult.OK;dialog.Close();}
     catch(Exception ex){error.Text=ex.Message;}
    };
    buttons.Controls.Add(save);buttons.Controls.Add(cancel);
    form.Controls.Add(new Label {Text="Tên trong thư viện",AutoSize=true});form.Controls.Add(name);
    form.Controls.Add(new Label {Text="Nội dung trên biển — mỗi dòng nhập là một dòng",AutoSize=true});form.Controls.Add(text);form.Controls.Add(error);form.Controls.Add(buttons);
    dialog.Controls.Add(form);dialog.CancelButton=cancel;
    if(dialog.ShowDialog(this)!=DialogResult.OK)return;
    AddCustomCard(saved);
    if(!multi.Checked)CaptureCustomFace(0,saved.Code,true);
    else if(faces.SelectedIndex>=0 && Convert.ToString(faces.SelectedItem)==saved.Code)CaptureCustomFace(faces.SelectedIndex,saved.Code,true);
    search.Text="";groups.SelectedIndex=7;Filter();Choose(cards.Single(c=>((TdtSignEntry)c.Tag).Code==saved.Code));
   }
  }
  private static System.Drawing.Text.PrivateFontCollection customFont;
  private static System.Drawing.Text.PrivateFontCollection CustomFont {
   get {if(customFont==null){var collection=new System.Drawing.Text.PrivateFontCollection();collection.AddFontFile(CustomSignPlate.FontPath());customFont=collection;}return customFont;}
  }
  private void CaptureCustomFace(int index,string code,bool replace) {
   CustomBoardTemplate item;if(!customBoards.TryGetValue(code,out item))return;
   var all=SignContent.Read(SelectedContent);var face=all.FirstOrDefault(x=>x.Index==index);
   if(!replace && face!=null && face.Code.Equals(code,StringComparison.OrdinalIgnoreCase) && face.Values.ContainsKey("custom_text"))return;
   all.RemoveAll(x=>x.Index==index);
   face=new SignContentFace {Index=index,Code=code};face.Values["custom_text"]=item.Text;face.Values["custom_name"]=item.Name;
   all.Add(face);SelectedContent=SignContent.Write(all);
  }
  private void CaptureCustomFaces(){
   var codes=SelectedFaces??new List<string>{SelectedCode};
   for(int i=0;i<codes.Count;i++)CaptureCustomFace(i,codes[i],false);
   var all=SignContent.Read(SelectedContent);all.RemoveAll(x=>x.Index>=codes.Count || !x.Code.Equals(codes[x.Index],StringComparison.OrdinalIgnoreCase));SelectedContent=SignContent.Write(all);
  }
  private void RestoreCustomFaces(){
   if(string.IsNullOrEmpty(SelectedContent))return;
   string chosen=SelectedSign==null?initialCode:SelectedSign.Code;
   foreach(var face in SignContent.Read(SelectedContent)){
    string text,name;if(!CustomSignPlate.IsCustom(face.Code) || !face.Values.TryGetValue("custom_text",out text))continue;
    face.Values.TryGetValue("custom_name",out name);
    AddCustomCard(new CustomBoardTemplate{Code=face.Code,Name=string.IsNullOrEmpty(name)?"Biển tùy chỉnh trong hồ sơ":name,Text=text});
   }
   Filter();var selected=cards.FirstOrDefault(c=>((TdtSignEntry)c.Tag).Code.Equals(chosen,StringComparison.OrdinalIgnoreCase));if(selected!=null)Choose(selected);
  }
  private void UseCustomFacePreview(int index){
   if(index<0)return;string code=Convert.ToString(faces.Items[index]);
   var values=SignContent.ForFace(SelectedContent,index,code);string text;CustomBoardTemplate item;
   if(!values.TryGetValue("custom_text",out text)||!customBoards.TryGetValue(code,out item))return;
   string name;values.TryGetValue("custom_name",out name);
   AddCustomCard(new CustomBoardTemplate{Code=code,Name=string.IsNullOrEmpty(name)?item.Name:name,Text=text});
   Filter();
  }
 }
}
