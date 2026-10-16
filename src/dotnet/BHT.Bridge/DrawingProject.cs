using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using Autodesk.AutoCAD.DatabaseServices;
using BHT.Core;

namespace BHT.Bridge
{
    public static class DrawingProject
    {
        public static string State(Transaction tr,Database db)
        { var r=BhtStore.Read(tr,db,"DRAWING","CURRENT");return r==null?"WORKING":r.Get("state"); }
        internal static void RequireWritable(Transaction tr,Database db)
        { if(new[]{"RELEASED","READ_ONLY_ARCHIVE"}.Contains(State(tr,db)))throw new InvalidOperationException("Bình đồ đã phát hành/lưu trữ. Tạo bản làm việc mới trước khi thay đổi."); }
        public static string Fingerprint(Database db)
        {
            using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
                var r=new BhtRecord().Set("records",ObjectCorrection.Stamp(tr,db)).Set("state",State(tr,db));
                foreach(var obj in BhtStore.All(tr,db,"OBJ").OrderBy(p=>p.Key))r.Add(obj.Key,ProjectTransfer.PresentationPose(db,tr,obj.Key));
                return ObjectRevision.Hash(r);
            }
        }
        public static void Register(Database db,string name,bool archive)
        {
            if(string.IsNullOrWhiteSpace(name))throw new InvalidOperationException("Nhập tên bình đồ.");
            bool disabled=!db.UndoRecording;if(disabled)db.DisableUndoRecording(false);
            try { using(var tr=db.TransactionManager.StartTransaction()) {
                RequireWritable(tr,db);BhtStore.ValidateHistory(tr,db);
                var project=BhtStore.Meta(tr,db,"project_id","");
                if(project=="")throw new InvalidOperationException("Chưa xác lập dự án. Lưu baseline GLOBAL hoặc hiệu chỉnh đã duyệt trước.");
                var prior=BhtStore.Read(tr,db,"DRAWING","CURRENT");
                var next=prior==null?new BhtRecord().Set("drawing_id",Guid.NewGuid().ToString("D")):prior.Clone();
                next.Set("project_id",project).Set("name",name.Trim()).Set("state",archive?"READ_ONLY_ARCHIVE":"WORKING");
                BhtStore.WriteDrawing(tr,db,next);tr.Commit();
            }} finally {if(disabled)db.DisableUndoRecording(true);}
        }
        public static IList<BhtRecord> Segments(Database db)
        {
            using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
                var objects=BhtStore.All(tr,db,"OBJ");
                return BhtStore.All(tr,db,"SEG").Select(s=> {
                    var items=objects.Where(o=>o.Value.Get("doan")==s.Key).Select(o=>o.Value).ToArray();
                    return new BhtRecord().Set("segment",s.Key).Set("package",s.Value.Get("package_id")).Set("objects",items.Length.ToString())
                        .Set("confirmed",items.Count(ProgressPolicy.Locked).ToString()).Set("revised",items.Count(ObjectRevision.Tracked).ToString())
                        .Set("retired",items.Count(ObjectRevision.Retired).ToString()).Set("drawing_state",State(tr,db));
                }).ToList();
            }
        }
        public static void TrackDrawing(Database db,string path)
        {
            path=Path.GetFullPath(path);
            using(var other=new Database(false,true)) {
                other.ReadDwgFile(path,FileOpenMode.OpenForReadAndAllShare,true,"");other.CloseInput(true);
                using(var read=other.TransactionManager.StartOpenCloseTransaction())using(var tr=db.TransactionManager.StartTransaction()) {
                    RequireWritable(tr,db);BhtStore.ValidateHistory(read,other);
                    var identity=BhtStore.Read(read,other,"DRAWING","CURRENT");
                    if(identity==null||identity.Get("project_id")!=BhtStore.Meta(tr,db,"project_id",""))throw new InvalidOperationException("Bình đồ cần được đăng ký cùng dự án trước khi liên kết.");
                    var local=BhtStore.Read(tr,db,"DRAWING","CURRENT");
                    if(local!=null&&local.Get("drawing_id")==identity.Get("drawing_id"))throw new InvalidOperationException("Cùng danh tính bình đồ. Dùng Tạo bản làm việc mới để tách bản sao.");
                    BhtStore.Write(tr,db,"DRAWING_VIEWS",identity.Get("drawing_id"),new BhtRecord().Set("drawing_id",identity.Get("drawing_id")).Set("path",path).Set("name",identity.Get("name")));
                    tr.Commit();
                }
            }
        }
        public static IList<BhtRecord> RelatedDrawings(Database db)
        {
            var result=new List<BhtRecord>();List<BhtRecord> views;
            using(var tr=db.TransactionManager.StartOpenCloseTransaction())views=BhtStore.All(tr,db,"DRAWING_VIEWS").Select(p=>p.Value.Clone()).ToList();
            if(views.Count==0)return result;
            string temp=Path.Combine(Path.GetTempPath(),"BHT-views-"+Guid.NewGuid().ToString("N")+".bht");
            try {
                ProjectTransfer.Export(db,temp);
                foreach(var view in views) {
                    try {
                        using(var other=new Database(false,true)) {
                            other.ReadDwgFile(view.Get("path"),FileOpenMode.OpenForReadAndAllShare,true,"");other.CloseInput(true);
                            string state;
                            using(var tr=other.TransactionManager.StartOpenCloseTransaction()) {
                                var identity=BhtStore.Read(tr,other,"DRAWING","CURRENT");
                                if(identity==null||identity.Get("drawing_id")!=view.Get("drawing_id"))throw new InvalidOperationException("Danh tính tệp đã đổi; liên kết lại.");
                                state=State(tr,other);
                            }
                            if(state=="WORKING") {
                                var changes=ProjectTransfer.CompareRolling(other,temp);int count=changes.Count(r=>r.Get("state")=="UPDATE_AVAILABLE");
                                state=count>0?"UPDATE_AVAILABLE":"UP_TO_DATE";view.Set("updates",count.ToString());
                            }
                            view.Set("state",state).Set("detail",state=="RELEASED"||state=="READ_ONLY_ARCHIVE"?"Giữ bản lịch sử; tạo bản làm việc mới nếu cần cập nhật.":"Mở bình đồ và xem trước gói để đồng bộ.");
                        }
                    } catch(Exception ex) {view.Set("state","CONFLICT").Set("detail",ex.Message);}
                    result.Add(view);
                }
                return result;
            } finally {if(File.Exists(temp))File.Delete(temp);}
        }
        public static string Release(Database db,string folder,string segment)
        {
            if(!Directory.Exists(folder))throw new DirectoryNotFoundException(folder);
            if(string.IsNullOrEmpty(segment)||segment.IndexOfAny(Path.GetInvalidFileNameChars())>=0)throw new InvalidOperationException("Chọn mã đoạn hợp lệ.");
            using(var read=db.TransactionManager.StartOpenCloseTransaction()) {
                RequireWritable(read,db);
                if(BhtStore.Read(read,db,"DRAWING","CURRENT")==null)throw new InvalidOperationException("Đăng ký bình đồ trước khi phát hành.");
            }
            string final=null,temp=Path.Combine(folder,"BHT-release-"+Guid.NewGuid().ToString("N")+".bht");bool created=false;
            bool disabled=!db.UndoRecording;if(disabled)db.DisableUndoRecording(false);
            try {
                ProjectTransfer.ExportSelection(db,temp,segment);
                int index=1;do { final=Path.Combine(folder,segment+"_R"+index.ToString("D2",CultureInfo.InvariantCulture)+".bht");index++; }while(File.Exists(final));
                File.Move(temp,final);created=true;
                using(var tr=db.TransactionManager.StartTransaction()) {
                    RequireWritable(tr,db);var drawing=BhtStore.Read(tr,db,"DRAWING","CURRENT");
                    string release=Guid.NewGuid().ToString("D"),hash;
                    using(var sha=SHA256.Create())using(var stream=File.OpenRead(final))hash=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","");
                    var entry=new BhtRecord().Set("release_id",release).Set("segment",segment).Set("drawing_id",drawing.Get("drawing_id"))
                        .Set("file",final).Set("sha256",hash).Set("time",DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture));
                    foreach(var pair in BhtStore.All(tr,db,"OBJ").Where(p=>p.Value.Get("doan")==segment))entry.Add("object",pair.Key+"|"+pair.Value.Get("revision_id")+"|"+ObjectRevision.Hash(pair.Value));
                    BhtStore.Write(tr,db,"RELEASES",release,entry);
                    BhtStore.WriteDrawing(tr,db,drawing.Clone().Set("state","RELEASED").Set("last_release",release));tr.Commit();
                }
                return final;
            } catch {if(created&&File.Exists(final))File.Delete(final);throw;}
            finally {if(File.Exists(temp))File.Delete(temp);if(disabled)db.DisableUndoRecording(true);}
        }
        public static void WorkingCopy(Database source,string path)
        {
            if(File.Exists(path))throw new IOException("Không ghi đè bản vẽ đã có.");
            if(!Path.GetExtension(path).Equals(".dwg",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Chọn tệp DWG mới.");
            using(var copy=Snapshot(source)) {
                copy.DisableUndoRecording(false);
                using(var tr=copy.TransactionManager.StartTransaction()) {
                    BhtStore.ValidateHistory(tr,copy);var previous=BhtStore.Read(tr,copy,"DRAWING","CURRENT");
                    if(previous==null)throw new InvalidOperationException("Chưa đăng ký bình đồ.");
                    BhtStore.WriteDrawing(tr,copy,previous.Clone().Set("parent_drawing",previous.Get("drawing_id")).Set("drawing_id",Guid.NewGuid().ToString("D")).Set("state","WORKING").Set("name",Path.GetFileNameWithoutExtension(path)));
                    tr.Commit();
                }
                // Save to a private path then move without overwrite; a competing file wins safely.
                string temp=path+"."+Guid.NewGuid().ToString("N")+".dwg";
                try {copy.SaveAs(temp,DwgVersion.AC1032);File.Move(temp,path);}finally{if(File.Exists(temp))File.Delete(temp);}
            }
        }
        internal static Database Snapshot(Database source)
        {
            string temp=Path.Combine(Path.GetTempPath(),"BHT-preview-"+Guid.NewGuid().ToString("N")+".dwg");
            var copy=new Database(false,true);
            try {
                source.SaveAs(temp,false,DwgVersion.AC1032,source.SecurityParameters);
                copy.ReadDwgFile(temp,FileOpenMode.OpenForReadAndAllShare,true,"");copy.CloseInput(true);return copy;
            } catch {copy.Dispose();throw;}
            finally {if(File.Exists(temp))File.Delete(temp);}
        }
    }
    public static partial class BhtStore
    {
        internal static void WriteDrawing(Transaction tr,Database db,BhtRecord record) { WriteRaw(tr,db,"DRAWING","CURRENT",record); }
    }
    public static partial class ProjectTransfer
    {
        public static IList<BhtRecord> CompareRolling(Database target,string path)
        {
            // A DWG snapshot preserves NOD and layouts; Wblock alone omits project dictionaries.
            using(var trial=DrawingProject.Snapshot(target))ImportRolling(trial,path);
            using(var package=Path.GetExtension(path).Equals(".bht",StringComparison.OrdinalIgnoreCase)?ProjectPackage.Open(path):null)
            using(var source=new Database(false,true)) {
                source.ReadDwgFile(package==null?path:package.DrawingPath,FileOpenMode.OpenForReadAndAllShare,true,"");source.CloseInput(true);
                using(var st=source.TransactionManager.StartOpenCloseTransaction())using(var tt=target.TransactionManager.StartOpenCloseTransaction()) {
                    var incoming=Records(source,st);var existing=Records(target,tt);
                    var decisions=Versioned(incoming.Concat(existing))?RevisionDecisions(incoming,existing):incoming.Where(r=>r.Sub=="OBJ").ToDictionary(r=>r.Key,r=>existing.Any(e=>e.Sub=="OBJ"&&e.Key==r.Key&&ProgressPolicy.Locked(e.Record))?RevisionDecision.Same:RevisionDecision.Upgrade);
                    return incoming.Where(r=>r.Sub=="OBJ").Select(r=>new BhtRecord().Set("object_id",r.Key).Set("segment",r.Record.Get("doan")).Set("revision_id",r.Record.Get("revision_id"))
                        .Set("state",decisions[r.Key]==RevisionDecision.New||decisions[r.Key]==RevisionDecision.Upgrade?"UPDATE_AVAILABLE":"UP_TO_DATE").Set("decision",decisions[r.Key].ToString())).ToList();
                }
            }
        }
    }
}
