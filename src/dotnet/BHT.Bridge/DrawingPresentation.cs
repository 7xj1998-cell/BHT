using System;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using BHT.Core;

namespace BHT.Bridge
{
    public static class DrawingPresentation
    {
        // Local placement belongs to this DWG and never advances the shared object head.
        public static bool SaveLocal(Database db,string id,string reason,string author)
        {
            bool disabled=!db.UndoRecording;if(disabled)db.DisableUndoRecording(false);
            try { return SaveLocalTransaction(db,id,reason,author); }
            finally { if(disabled)db.DisableUndoRecording(true); }
        }
        static bool SaveLocalTransaction(Database db,string id,string reason,string author)
        {
            if(string.IsNullOrWhiteSpace(reason)||string.IsNullOrWhiteSpace(author))throw new InvalidOperationException("Nhập lý do và người lưu.");
            using(var tr=db.TransactionManager.StartTransaction()) {
                BhtStore.ValidateHistory(tr,db);
                var obj=BhtStore.Read(tr,db,"OBJ",id);
                if(!ObjectRevision.Tracked(obj)||ObjectRevision.Retired(obj)||obj.Get("presentation_pose")=="")throw new InvalidOperationException("Cần hồ sơ đang sử dụng có baseline/revision. Lưu GLOBAL lần đầu trước khi tạo bố trí LOCAL.");
                string pose=ProjectTransfer.PresentationPose(db,tr,id);
                var old=BhtStore.Read(tr,db,"VIEW_LOCAL",id);
                if(old!=null&&old.Get("pose")==pose)return false;
                var next=new BhtRecord().Set("object_id",id).Set("object_guid",obj.Get("object_guid")).Set("project_id",obj.Get("project_id"))
                    .Set("revision_id",Guid.NewGuid().ToString("D")).Set("parent_revision",old==null?"":old.Get("revision_id"))
                    .Set("pose",pose).Set("mode","LOCAL").Set("reason",reason.Trim()).Set("author",author.Trim()).Set("time",DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture));
                BhtStore.Write(tr,db,"LOCAL_HISTORY",next.Get("revision_id"),next);
                BhtStore.Write(tr,db,"VIEW_LOCAL",id,next);tr.Commit();return true;
            }
        }
        public static void UseGlobal(Database db,string id)
        {
            bool disabled=!db.UndoRecording;if(disabled)db.DisableUndoRecording(false);
            try {
            using(var tr=db.TransactionManager.StartTransaction()) {
                BhtStore.ValidateHistory(tr,db);
                var obj=BhtStore.Read(tr,db,"OBJ",id);
                if(obj==null||obj.Get("presentation_pose")==""||ProjectTransfer.PresentationPose(db,tr,id)!=obj.Get("presentation_pose"))
                    throw new InvalidOperationException("Bố trí hiện tại khác GLOBAL. Giữ LOCAL; cần đối chiếu và đưa hình về GLOBAL đã lưu trước khi chuyển.");
                BhtStore.Delete(tr,db,"VIEW_LOCAL",id);tr.Commit();
            }
            } finally { if(disabled)db.DisableUndoRecording(true); }
        }
        internal static void Validate(Transaction tr,Database db)
        {
            var history=BhtStore.All(tr,db,"LOCAL_HISTORY").ToDictionary(p=>p.Key,p=>p.Value,StringComparer.OrdinalIgnoreCase);
            foreach(var pair in BhtStore.All(tr,db,"VIEW_LOCAL")) {
                var obj=BhtStore.Read(tr,db,"OBJ",pair.Key);var value=pair.Value;
                if(obj==null||value.Get("project_id")!=obj.Get("project_id")||value.Get("object_guid")!=obj.Get("object_guid"))throw new InvalidOperationException("Bố trí LOCAL sai định danh đối tượng.");
                string next=value.Get("revision_id");var seen=new System.Collections.Generic.HashSet<string>();
                while(next!="") {
                    BhtRecord entry;
                    if(!seen.Add(next)||!history.TryGetValue(next,out entry)||entry.Get("object_guid")!=obj.Get("object_guid")||entry.Get("project_id")!=obj.Get("project_id"))throw new InvalidOperationException("Thiếu hoặc sai lịch sử bố trí LOCAL.");
                    if(next==value.Get("revision_id")&&!ObjectRevision.Same(value,entry))throw new InvalidOperationException("LOCAL khác lịch sử hiện hành.");
                    next=entry.Get("parent_revision");
                }
                if(seen.Count==0)throw new InvalidOperationException("LOCAL chưa có revision.");
            }
        }
    }
}
