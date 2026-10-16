using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using BHT.Core;
using BHT.Bridge;
namespace BHT.Palette
{
    public partial class BhtPaletteControl
    {
        void OpenDrawingProject()
        {
            var doc=AcadDispatcher.ActiveDocument;if(doc==null)return;
            var form=new DrawingProjectForm(Path.GetFileNameWithoutExtension(doc.Name));
            var name=form.NameInput;var actions=form.Actions;var grid=form.Grid;var status=form.StatusText;
            string source=null,hash=null,destinationStamp=null;
            Func<string,string> fileHash=path=> {using(var sha=System.Security.Cryptography.SHA256.Create())using(var stream=File.OpenRead(path))return Convert.ToBase64String(sha.ComputeHash(stream));};
            Action<string[],System.Collections.Generic.IList<BhtRecord>,string[]> show=form.SetRows;
            Action refresh=()=>show(new[]{"Đoạn","Gói","Hồ sơ","Xác nhận","Có revision","Ngừng dùng","Trạng thái"},DrawingProject.Segments(doc.Database),new[]{"segment","package","objects","confirmed","revised","retired","drawing_state"});
            Action<string,Func<OpResult>> run=(title,operation)=>AcadDispatcher.RunInCommandContext(title,()=> {
                if(AcadDispatcher.ActiveDocument!=doc)return OpResult.Fail("Đã chuyển bản vẽ.");return operation();
            },r=> {if(!form.IsDisposed)status.Text=r.Message;});
            Action<string,Action> button=(text,action)=> {var b=new Button { Text=text,AutoSize=true };b.Click+=(s,e)=>{try{action();}catch(Exception ex){status.Text=ex.Message;}};actions.Controls.Add(b);};
            button("Đăng ký / cập nhật tên",()=>run("Đăng ký bình đồ",()=>{DrawingProject.Register(doc.Database,name.Text,false);refresh();return OpResult.Success("Đã đăng ký bình đồ làm việc.");}));
            button("Danh sách đoạn",()=>{refresh();source=hash=null;});
            button("Liên kết bình đồ cùng dự án…",()=> {
                using(var pick=new OpenFileDialog { Filter="DWG (*.dwg)|*.dwg" })if(pick.ShowDialog(form)==DialogResult.OK) {
                    string path=pick.FileName;run("Liên kết bình đồ",()=>{DrawingProject.TrackDrawing(doc.Database,path);return OpResult.Success("Đã liên kết. Tệp được chọn chỉ được đọc.");});
                }
            });
            button("Kiểm tra bình đồ liên quan",()=>run("Kiểm tra cập nhật",()=> {
                show(new[]{"Tên bình đồ","Tệp DWG","Trạng thái","Cập nhật","Chi tiết"},DrawingProject.RelatedDrawings(doc.Database),new[]{"name","path","state","updates","detail"});source=hash=null;return OpResult.Success("Đã kiểm tra các DWG đã lưu; không ghi tệp liên quan.");
            }));
            button("Xem trước gói cập nhật…",()=> {
                using(var pick=new OpenFileDialog { Filter="BHT (*.bht)|*.bht|DWG (*.dwg)|*.dwg" })if(pick.ShowDialog(form)==DialogResult.OK) {
                    string path=pick.FileName;source=hash=null;
                    run("Đối chiếu bản cập nhật",()=> {
                        string beforeHash=fileHash(path),beforeStamp=DrawingProject.Fingerprint(doc.Database);
                        System.Collections.Generic.IList<BhtRecord> rows;
                        try {rows=ProjectTransfer.CompareRolling(doc.Database,path);}
                        catch(ImportConflictException ex) {
                            show(new[]{"OBJ","Trạng thái","Lý do","Trường","Hiện tại","Nguồn"},ex.Conflicts.Select(c=>new BhtRecord().Set("id",c.Id).Set("state","CONFLICT").Set("reason",c.Reason).Set("field",c.Field).Set("current",c.Current).Set("incoming",c.Incoming)).ToList(),new[]{"id","state","reason","field","current","incoming"});
                            return OpResult.Fail(ex.Message);
                        }
                        show(new[]{"OBJ","Đoạn","Trạng thái","Quyết định","Revision nguồn"},rows,new[]{"object_id","segment","state","decision","revision_id"});
                        if(beforeHash!=fileHash(path)||beforeStamp!=DrawingProject.Fingerprint(doc.Database))throw new InvalidOperationException("Nguồn/đích đổi trong lúc xem trước; xem lại.");
                        hash=beforeHash;destinationStamp=beforeStamp;source=path;
                        return OpResult.Success(rows.Count(r=>r.Get("state")=="UPDATE_AVAILABLE")+" hồ sơ có bản cập nhật. Đối chiếu trước khi bấm Đồng bộ.");
                    });
                }
            });
            button("Đồng bộ gói đã xem trước",()=> {
                if(source==null)throw new InvalidOperationException("Xem trước gói trước khi đồng bộ.");
                if(MessageBox.Show(form,"Đồng bộ gói đã chọn vào bình đồ đang mở?","Xác nhận",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
                string path=source,expected=hash;
                run("Đồng bộ bình đồ",()=> {
                    if(fileHash(path)!=expected||DrawingProject.Fingerprint(doc.Database)!=destinationStamp)throw new InvalidOperationException("Nguồn hoặc bình đồ đã đổi. Xem trước lại.");
                    int n=ProjectTransfer.ImportRolling(doc.Database,path);refresh();source=hash=null;return OpResult.Success("Đã đồng bộ "+n+" thực thể CAD; kiểm tra và lưu DWG.");
                });
            });
            button("Phát hành đoạn R01/R02…",()=> {
                if(grid.SelectedRows.Count==0)throw new InvalidOperationException("Mở Danh sách đoạn rồi chọn đoạn.");
                var row=(BhtRecord)grid.SelectedRows[0].Tag;string segment=row.Get("segment");
                if(!row.Has("confirmed"))throw new InvalidOperationException("Chọn từ Danh sách đoạn.");
                using(var pick=new FolderBrowserDialog())if(pick.ShowDialog(form)==DialogResult.OK) {
                    string folder=pick.SelectedPath;
                    if(MessageBox.Show(form,"Phát hành "+segment+" và chuyển bình đồ sang RELEASED?","Xác nhận phát hành",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
                    run("Phát hành đoạn",()=>{string path=DrawingProject.Release(doc.Database,folder,segment);refresh();return OpResult.Success("Đã phát hành: "+path+". Lưu DWG để giữ trạng thái.");});
                }
            });
            button("Tạo bản DWG làm việc mới…",()=> {
                using(var pick=new SaveFileDialog { Filter="DWG (*.dwg)|*.dwg",OverwritePrompt=true })if(pick.ShowDialog(form)==DialogResult.OK) {
                    string path=pick.FileName;run("Tạo bản làm việc",()=>{DrawingProject.WorkingCopy(doc.Database,path);return OpResult.Success("Đã tạo "+path+". Mở tệp mới để tiếp tục; bản cũ giữ nguyên.");});
                }
            });
            button("Đánh dấu lưu trữ",()=> {
                if(MessageBox.Show(form,"Giữ bình đồ này làm lịch sử chỉ đọc trong BHT?","Lưu trữ",MessageBoxButtons.YesNo)==DialogResult.Yes)
                    run("Lưu trữ bình đồ",()=>{DrawingProject.Register(doc.Database,name.Text,true);refresh();return OpResult.Success("READ_ONLY_ARCHIVE. Lưu DWG để giữ trạng thái.");});
            });
            refresh();PaletteTheme.ApplyTableDialog(form);Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessDialog(form);
        }
    }
}
