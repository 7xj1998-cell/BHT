using System;
using System.Windows.Forms;
using BHT.Core;
using BHT.Bridge;
namespace BHT.Palette
{
    public partial class BhtPaletteControl
    {
        void OpenPresentationMode()
        {
            var doc=AcadDispatcher.ActiveDocument;string id=_oId.Text;
            if(doc==null||_oIsNew||id=="")return;
            var form=new Form { Text="Bố trí ký hiệu - "+id,Width=600,Height=310,MinimumSize=new System.Drawing.Size(500,310) };
            var flow=new FlowLayoutPanel { Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,Padding=new Padding(12),AutoScroll=true,WrapContents=false };
            var note=new Label { AutoSize=true,MaximumSize=new System.Drawing.Size(540,0),Text="GLOBAL là bố trí chung. Lưu GLOBAL tại Hiệu chỉnh hồ sơ. LOCAL giữ bố trí trong DWG này khi nhận cập nhật GLOBAL; chưa nhận thay đổi khảo sát có ảnh hưởng hình LOCAL." };
            var reason=new TextBox { Width=520 };
            var status=new Label { AutoSize=true,MaximumSize=new System.Drawing.Size(540,0) };
            flow.Controls.Add(note);flow.Controls.Add(new Label { Text="Lý do lưu bố trí riêng",AutoSize=true });flow.Controls.Add(reason);
            foreach(bool local in new[]{true,false}) {
                bool save=local;
                var button=new Button { AutoSize=true,Text=save?"Lưu bố trí hiện tại thành LOCAL":"Chuyển về GLOBAL đã khớp" };
                button.Click+=(s,e)=>AcadDispatcher.RunInCommandContext("Lưu bố trí",()=> {
                    if(AcadDispatcher.ActiveDocument!=doc)return OpResult.Fail("Đã chuyển bản vẽ.");
                    if(save)DrawingPresentation.SaveLocal(doc.Database,id,reason.Text,Environment.UserName);else DrawingPresentation.UseGlobal(doc.Database,id);
                    return OpResult.Success(save?"Đã lưu LOCAL. RTK và lý trình giữ nguyên.":"Đã dùng GLOBAL.");
                },r=> { if(!form.IsDisposed)status.Text=r.Message; });
                flow.Controls.Add(button);
            }
            flow.Controls.Add(status);form.Controls.Add(flow);PaletteTheme.ApplyDialog(form);
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessDialog(form);
        }
        void OpenObjectCorrection(bool history)
        {
            try {
                if(_svc==null||_oIsNew||_oId.Text=="")return;
                var doc=AcadDispatcher.ActiveDocument;var record=_svc.GetObject(_oId.Text);
                if(doc==null||record==null)return;
                if(history) {
                    var rows=new ObjectCorrection(doc.Database).History(_oId.Text);
                    if(rows.Count==0) { MessageBox.Show("Hồ sơ chưa có lịch sử hiệu chỉnh. Bản gốc được lưu khi xác nhận hiệu chỉnh lần đầu.","Lịch sử");return; }
                    Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessDialog(ObjectCorrectionForm.HistoryWindow(rows,_oId.Text));return;
                }
                if(!ProgressPolicy.Locked(record)) { MessageBox.Show("Hồ sơ chưa khóa; dùng Lưu hồ sơ để chỉnh sửa.","Hiệu chỉnh");return; }
                var form=new ObjectCorrectionForm(doc,_oId.Text,record);
                form.Saved+=()=> { if(AcadDispatcher.ActiveDocument==doc) { RefreshObjects();LoadObjectFields(_oId.Text); } };
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessDialog(form);
            } catch(Exception ex) { MessageBox.Show(ex.Message,"Hiệu chỉnh hồ sơ"); }
        }
    }
}
