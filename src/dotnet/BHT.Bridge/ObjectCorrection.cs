using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Autodesk.AutoCAD.DatabaseServices;
using BHT.Core;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace BHT.Bridge
{
    public sealed class CorrectionPreview
    {
        internal readonly Database Database;
        internal readonly BhtRecord Original, Candidate, Fields;
        internal readonly string Stamp;
        internal readonly CorrectionKind Kind;
        public string ObjectId { get; private set; }
        public string Reason { get; private set; }
        public string Author { get; private set; }
        public bool LegacyBaseline { get { return !ObjectRevision.Tracked(Original); } }
        public bool AssignmentChanged { get { return Original.Get("doan") != Candidate.Get("doan"); } }
        public BhtRecord Before { get { return Original.Clone(); } }
        public BhtRecord After { get { return Candidate.Clone(); } }
        internal CorrectionPreview(Database db, string id, BhtRecord old, BhtRecord next, BhtRecord fields, CorrectionKind kind, string stamp, string reason, string author)
        { Database=db;ObjectId=id;Original=old.Clone();Candidate=next.Clone();Fields=fields.Clone();Kind=kind;Stamp=stamp;Reason=reason;Author=author; }
    }

    public sealed class ObjectCorrection
    {
        readonly Database db;
        public ObjectCorrection(Database database) { db=database; }
        static string Quote(string value) { return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\""; }
        static string LispRecord(BhtRecord r) { return "(" + string.Join(" ",r.Pairs.Select(p=>"("+Quote(p.Key)+" . "+Quote(p.Value)+")")) + ")"; }
        internal static string Stamp(Transaction tr, Database db)
        {
            var r=new BhtRecord();
            foreach(string sub in new[]{"OBJ","SEG","ROUTE","META","HISTORY","CHANGES","CORRECTION","CAD_STATE","VIEW_LOCAL","LOCAL_HISTORY"})
                foreach(var pair in BhtStore.All(tr,db,sub).OrderBy(p=>p.Key))r.Add(sub+"/"+pair.Key,ObjectRevision.Snapshot(pair.Value));
            foreach(var p in BhtStore.Points(tr,db).OrderBy(p=>p.Id))r.Add("POINT",p.Id+"|"+p.X.ToString("R",CultureInfo.InvariantCulture)+"|"+p.Y.ToString("R",CultureInfo.InvariantCulture)+"|"+p.Z.ToString("R",CultureInfo.InvariantCulture));
            return ObjectRevision.Hash(r);
        }
        BhtRecord Evaluate(string id, BhtRecord old, BhtRecord fields, CorrectionKind kind)
        {
            var candidate=ObjectRevision.Patch(old,fields,kind);
            if(kind==CorrectionKind.DisplayGlobal) {
                using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
                    if(BhtStore.Read(tr,db,"VIEW_LOCAL",id)!=null)throw new InvalidOperationException("Đang dùng LOCAL. Bỏ chế độ LOCAL rõ ràng trước khi lưu GLOBAL.");
                    return candidate.Set("presentation_pose",ProjectTransfer.PresentationPose(db,tr,id));
                }
            }
            if(fields.Has("ma_hieu") && candidate.Get("nhom")=="BIEN_BAO" && fields.Get("ma_hieu")!=old.Get("ma_hieu")) {
                string error=SignPresentation.ValidationError(candidate.Get("ma_hieu"));
                if(!string.IsNullOrEmpty(error) || TdtSignLibrary.Find(candidate.Get("ma_hieu"))==null)
                    throw new InvalidOperationException("Mã biển chưa hợp lệ hoặc không có trong thư viện: "+candidate.Get("ma_hieu"));
            }
            using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
                var points=BhtStore.Points(tr,db).ToLookup(p=>p.IdUpper);
                var ids=candidate.GetAll("pt");
                if(ids.Count==0 || ids.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=ids.Count || ids.Any(p=>points[p.ToUpperInvariant()].Count()!=1))
                    throw new InvalidOperationException("Điểm liên kết thiếu hoặc trùng mã.");
                if(kind==CorrectionKind.PointLink) {
                    var added=ids.Except(old.GetAll("pt"),StringComparer.OrdinalIgnoreCase).ToArray();
                    if(BhtStore.All(tr,db,"OBJ").Any(p=>p.Key!=id && p.Value.GetAll("pt").Intersect(added,StringComparer.OrdinalIgnoreCase).Any()))
                        throw new InvalidOperationException("Điểm mới đã thuộc hồ sơ khác; cần đối chiếu liên kết trước.");
                }
            }
            var api=new LispApi(); string version,message;
            if(!api.Probe(out version,out message))throw new InvalidOperationException(message);
            var result=api.Call("bht:api-correction-evaluate",id,LispRecord(candidate),kind.ToString());
            if(!result.Ok)throw new InvalidOperationException(result.Error);
            var evaluated=RecordCodec.Decode(result.Values);
            // Locked CAD layers can silently reject entmod in legacy symbol routines.
            // Reject before writing records rather than leave a stale symbol accepted.
            using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
                foreach(string type in new[]{"INSERT","TEXT","LINE","LWPOLYLINE","DIMENSION"})
                    foreach(string tag in new[]{"BHT_KH","BHT_SIGN_ORIGIN","BHT_NHAN","BHT_OFFSET_DIM"})
                        foreach(var pair in BhtStore.Tagged(tr,db,type,tag).Where(p=>p.Key==id)) {
                            var entity=(Entity)tr.GetObject(pair.Value,OpenMode.ForRead);
                            if(((LayerTableRecord)tr.GetObject(entity.LayerId,OpenMode.ForRead)).IsLocked)
                                throw new InvalidOperationException("Ký hiệu/nhãn của hồ sơ nằm trên lớp khóa. Mở khóa lớp trình bày rồi xem trước lại.");
                        }
            }
            return evaluated;
        }
        public CorrectionPreview Preview(string id, BhtRecord fields, CorrectionKind kind, string reason, string author)
        {
            if(AcadDispatcher.ActiveDocument==null || AcadDispatcher.ActiveDocument.Database!=db)throw new InvalidOperationException("Hiệu chỉnh cần bản vẽ đang hoạt động.");
            if(string.IsNullOrWhiteSpace(reason)||string.IsNullOrWhiteSpace(author))throw new InvalidOperationException("Nhập lý do và người hiệu chỉnh.");
            id=id.ToUpperInvariant();BhtRecord before;string stamp;
            using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
                before=BhtStore.Read(tr,db,"OBJ",id);
                if(before==null)throw new InvalidOperationException("Không tìm thấy hồ sơ.");
                BhtStore.ValidateHistory(tr,db);stamp=Stamp(tr,db);
            }
            var next=Evaluate(id,before,fields,kind);
            if(!ObjectRevision.Differences(before,next).Any())throw new InvalidOperationException("Không có thay đổi; không tạo phiên bản mới.");
            return new CorrectionPreview(db,id,before,next,fields,kind,stamp,reason.Trim(),author.Trim());
        }
        public void Confirm(CorrectionPreview preview, bool acceptLegacyBaseline, bool approveSegmentChange)
        {
            if(preview==null || preview.Database!=db || AcadDispatcher.ActiveDocument==null || AcadDispatcher.ActiveDocument.Database!=db)throw new InvalidOperationException("Bản vẽ đã đổi; xem trước lại.");
            if(preview.LegacyBaseline&&!acceptLegacyBaseline)throw new InvalidOperationException("Cần đồng ý lưu baseline hồ sơ cũ, chưa xác minh nguồn gốc.");
            if(preview.AssignmentChanged&&!approveSegmentChange)throw new InvalidOperationException("Cần duyệt thay đổi phân đoạn.");
            using(var docLock=AcadDispatcher.ActiveDocument.LockDocument()) {
                // Native Lisp block creation cannot safely run inside a managed transaction.
                // One native undo group encloses the committed records AND all CAD edits.
                int undo=Convert.ToInt32(AcApp.GetSystemVariable("UNDOCTL"));
                if(!db.UndoRecording || (undo&1)==0 || (undo&2)!=0)throw new InvalidOperationException("Cần bật Undo ở chế độ All trước khi hiệu chỉnh.");
                using(var read=db.TransactionManager.StartOpenCloseTransaction())
                    if(Stamp(read,db)!=preview.Stamp)throw new InvalidOperationException("Dữ liệu đã đổi sau xem trước. Xem trước lại trước khi xác nhận.");
                var fresh=Evaluate(preview.ObjectId,preview.Original,preview.Fields,preview.Kind);
                if(!ObjectRevision.Same(fresh,preview.Candidate))throw new InvalidOperationException("Kết quả tính đã đổi. Xem trước lại.");
                var api=new LispApi();var before=api.Call("bht:api-correction-state");
                if(!before.Ok)throw new InvalidOperationException(before.Error);
                var editor=AcadDispatcher.ActiveDocument.Editor;
                // AutoCAD opens a group for the current command. End it before the
                // private group so rollback never consumes the preceding user action.
                if((undo&8)!=0)editor.Command("_.UNDO","_End");
                editor.Command("_.UNDO","_Begin");
                bool journalWritten=false;
                try {
                    // A committed journal guarantees the private group is nonempty even
                    // if the following managed transaction aborts before its first write.
                    using(var journal=db.TransactionManager.StartTransaction()) {
                        BhtStore.Write(journal,db,"CORRECTION","IN_FLIGHT",new BhtRecord().Set("object_id",preview.ObjectId)
                            .Set("before",ObjectRevision.Snapshot(preview.Original)).Set("state","IN_PROGRESS"));
                        journal.Commit();
                        journalWritten=true;
                    }
                    using(var tr=db.TransactionManager.StartTransaction()) {
                        BhtStore.CommitCorrection(tr,db,preview);
                        tr.Commit();
                    }
                    if(preview.Kind!=CorrectionKind.DisplayGlobal) {
                        var reply=api.Call("bht:api-correction-refresh",preview.ObjectId);
                        if(!reply.Ok)throw new InvalidOperationException(reply.Error);
                    }
                    using(var journal=db.TransactionManager.StartTransaction()) {
                        var current=BhtStore.Read(journal,db,"OBJ",preview.ObjectId);
                        BhtStore.Write(journal,db,"CAD_STATE",preview.ObjectId,new BhtRecord().Set("revision_id",current.Get("revision_id"))
                            .Set("pose",ProjectTransfer.PresentationPose(db,journal,preview.ObjectId)));
                        BhtStore.Delete(journal,db,"CORRECTION","IN_FLIGHT");journal.Commit();
                    }
                    using(var tr=db.TransactionManager.StartOpenCloseTransaction())BhtStore.ValidateHistory(tr,db);
                } catch(System.Exception error) {
                    editor.Command("_.UNDO","_End");if(journalWritten)editor.Command("_.U");
                    var after=api.Call("bht:api-correction-state");
                    bool restored;
                    using(var read=db.TransactionManager.StartOpenCloseTransaction())restored=Stamp(read,db)==preview.Stamp;
                    if(!restored||!after.Ok||!before.Values.SequenceEqual(after.Values))
                        throw new InvalidOperationException("Không xác minh được phục hồi. Không lưu bản vẽ này; mở lại bản sao trước thao tác. Lỗi: "+error.Message,error);
                    throw new InvalidOperationException("Đã phục hồi đầy đủ dữ liệu và hình CAD. "+error.Message,error);
                }
                editor.Command("_.UNDO","_End");
            }
        }
        public IList<BhtRecord> History(string id)
        {
            using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
                BhtStore.ValidateHistory(tr,db);
                var current=BhtStore.Read(tr,db,"OBJ",id);
                if(!ObjectRevision.Tracked(current))return new List<BhtRecord>();
                var rows=BhtStore.All(tr,db,"HISTORY").ToDictionary(p=>p.Key,p=>p.Value,StringComparer.OrdinalIgnoreCase);
                var result=new List<BhtRecord>();string next=current.Get("revision_id");
                while(next!="") { var r=rows[next];result.Add(r.Clone());next=r.Get("parent_revision"); }
                return result;
            }
        }
    }

    public static partial class BhtStore
    {
        public static void ValidateHistory(Transaction tr, Database db)
        {
            DrawingPresentation.Validate(tr,db);
            if(Read(tr,db,"CORRECTION","IN_FLIGHT")!=null)throw new InvalidOperationException("Có giao dịch hiệu chỉnh chưa hoàn tất. Không xuất/nạp/sửa tiếp; phục hồi bằng Undo hoặc mở bản sao trước thao tác.");
            var entries=All(tr,db,"HISTORY").ToDictionary(p=>p.Key,p=>p.Value,StringComparer.OrdinalIgnoreCase);
            foreach(var pair in entries) {
                var r=ObjectRevision.ReadHistory(pair.Value);
                if(!string.Equals(pair.Key,pair.Value.Get("revision_id"),StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Khóa lịch sử sai.");
                if(ObjectRevision.Tracked(r)) {
                    foreach(string field in new[]{"revision_id","parent_revision","object_guid","project_id","object_id","revision_status","revision_reason","revision_author","revision_time"})
                        if(r.Get(field)!=pair.Value.Get(field))throw new InvalidOperationException("Metadata lịch sử không khớp snapshot: "+field);
                } else if(pair.Value.Get("revision_status")!="LEGACY_UNVERIFIED"||pair.Value.Get("parent_revision")!="")
                    throw new InvalidOperationException("Baseline cũ không hợp lệ.");
            }
            var objects=All(tr,db,"OBJ").Where(p=>ObjectRevision.Tracked(p.Value)).ToArray();
            if(objects.GroupBy(p=>p.Value.Get("object_guid")).Any(g=>g.Count()>1))throw new InvalidOperationException("Trùng định danh nội bộ đối tượng.");
            foreach(var pair in objects) {
                RevisionMerge.Chain(pair.Value,entries);
                var current=pair.Value;var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);string next=current.Get("revision_id");bool first=true;BhtRecord child=null;
                if(current.Get("project_id")==""||current.Get("project_id")!=Meta(tr,db,"project_id",""))throw new InvalidOperationException("Định danh dự án không khớp lịch sử.");
                Guid identity;
                if(!Guid.TryParse(current.Get("object_guid"),out identity)||!Guid.TryParse(current.Get("project_id"),out identity))throw new InvalidOperationException("Định danh nội bộ không hợp lệ.");
                while(next!="") {
                    BhtRecord entry;
                    if(!seen.Add(next)||!entries.TryGetValue(next,out entry)||entry.Get("object_guid")!=current.Get("object_guid")||entry.Get("project_id")!=current.Get("project_id")||entry.Get("object_id")!=pair.Key)
                        throw new InvalidOperationException("Chuỗi lịch sử thiếu, vòng lặp hoặc sai đối tượng: "+pair.Key);
                    var snapshot=ObjectRevision.ReadHistory(entry);
                    if(child!=null)foreach(string counter in new[]{"object_revision","survey_revision","assignment_revision","display_revision"}) {
                        int previous=0,following;
                        if(ObjectRevision.Tracked(snapshot)&&(!int.TryParse(snapshot.Get(counter),out previous)||previous<0))throw new InvalidOperationException("Số phiên bản cha không hợp lệ.");
                        if(!int.TryParse(child.Get(counter),out following)||following<previous||(long)following-previous>1||(counter=="object_revision"&&following!=previous+1))
                            throw new InvalidOperationException("Số phiên bản không khớp quan hệ cha.");
                    }
                    if(first && !ObjectRevision.Same(ObjectRevision.Governed(current),ObjectRevision.Governed(snapshot)))throw new InvalidOperationException("Hồ sơ không khớp phiên bản hiện hành: "+pair.Key);
                    first=false;child=snapshot;next=entry.Get("parent_revision");
                }
            }
        }
        internal static void CommitCorrection(Transaction tr,Database db,CorrectionPreview preview)
        {
            DrawingProject.RequireWritable(tr,db);
            var old=Read(tr,db,"OBJ",preview.ObjectId);
            if(!ObjectRevision.Same(old,preview.Original))throw new InvalidOperationException("Hồ sơ đã đổi.");
            string project=Meta(tr,db,"project_id","");if(project=="")project=Guid.NewGuid().ToString("D");
            string guid=ObjectRevision.Tracked(old)?old.Get("object_guid"):Guid.NewGuid().ToString("D");
            string parent=old.Get("revision_id"),time=DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture);
            if(parent=="") {
                parent=Guid.NewGuid().ToString("D");
                Write(tr,db,"HISTORY",parent,ObjectRevision.History(old,"LEGACY_UNVERIFIED",project,guid,parent,"","Baseline trước hiệu chỉnh; nguồn gốc chưa được xác minh",preview.Author,time));
            }
            var next=ObjectRevision.Next(old,preview.Candidate,project,guid,parent,preview.Reason,preview.Author,time);
            var revision=next.Get("revision_id");
            var savedPose=Read(tr,db,"CAD_STATE",preview.ObjectId);
            string beforePose=preview.Kind==CorrectionKind.DisplayGlobal&&savedPose!=null?savedPose.Get("pose"):ProjectTransfer.PresentationPose(db,tr,preview.ObjectId);
            Write(tr,db,"HISTORY",revision,ObjectRevision.History(next,next.Get("revision_status"),project,guid,revision,parent,preview.Reason,preview.Author,time)
                .Set("before_pose",beforePose));
            MetaSet(tr,db,"project_id",project);
            WriteRaw(tr,db,"OBJ",preview.ObjectId,next);
            Write(tr,db,"CHANGES",revision,new BhtRecord().Set("revision_id",revision).Set("object_guid",guid).Set("object_id",preview.ObjectId)
                .Set("old_segment",old.Get("doan")).Set("new_segment",next.Get("doan")).Set("state","UPDATE_AVAILABLE"));
        }
    }
}
