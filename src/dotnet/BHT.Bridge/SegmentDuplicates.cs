using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using BHT.Core;

namespace BHT.Bridge
{
    public sealed class SegmentDuplicatePreview
    {
        internal Database Database;
        internal string Stamp;
        internal List<SegmentDuplicateDecision> Decisions;
        internal string[] Details;
        public string[] Lines { get { return (string[])Details.Clone(); } }
        public int Removable { get { return Decisions.Count(d=>d.Allowed); } }
        public int Blocked { get { return Decisions.Count(d=>!d.Allowed); } }
    }
    public static class SegmentDuplicates
    {
        static string Describe(BhtRecord row)
        {
            string side=row.Get("side"),route=row.Get("route_id");
            return "Mã đoạn: "+row.Get("segment_id")+"\r\nGói: "+row.Get("package_id")+" — "+row.Get("package_name")
                +"\r\nLý trình: "+row.Get("km_start_m")+" m → "+row.Get("km_end_m")+" m; phía: "+(side=="TRAI"?"Trái":side=="PHAI"?"Phải":side=="HAI_BEN"?"Hai bên":side)
                +"\r\nTim: "+(route==""?"(chưa gắn)":route)+"\r\nGhi chú: "+row.Get("note");
        }
        static Dictionary<string,List<KeyValuePair<string,BhtRecord>>> Records(Transaction tr,Database db)
        {
            var result=new Dictionary<string,List<KeyValuePair<string,BhtRecord>>>();
            var root=BhtStore.RootDict(tr,db,false);if(root==null)return result;
            foreach(DBDictionaryEntry sub in root){
                if(!(tr.GetObject(sub.Value,OpenMode.ForRead) is DBDictionary))throw new InvalidOperationException("Kho BHT có mục không đúng cấu trúc: "+sub.Key+". Giữ nguyên dữ liệu để kiểm tra riêng.");
                result[sub.Key]=BhtStore.All(tr,db,sub.Key);
            }
            return result;
        }
        static string Stamp(Dictionary<string,List<KeyValuePair<string,BhtRecord>>> records)
        {
            var r=new BhtRecord();foreach(var sub in records.OrderBy(p=>p.Key))foreach(var row in sub.Value.OrderBy(p=>p.Key))r.Add(sub.Key+"/"+row.Key,ObjectRevision.Snapshot(row.Value));return ObjectRevision.Hash(r);
        }
        static SegmentDuplicatePreview Preview(Transaction tr,Database db)
        {
            DrawingProject.RequireWritable(tr,db);
            var records=Records(tr,db);var segments=records.ContainsKey("SEG")?records["SEG"].Select(p=>p.Value.Clone().Set("segment_id",p.Key)).ToArray():new BhtRecord[0];
            var ids=new HashSet<string>(segments.Select(s=>s.Get("segment_id")),StringComparer.Ordinal);
            var used=new HashSet<string>(StringComparer.Ordinal);
            // Ambiguous references always preserve the row. ROUTE identity alone is
            // not a segment reference (DOAN08 may use the route named DOAN_08).
            var nonReferences=new HashSet<string>(new[]{"route_id","route_name","sign_heading_route","station_active_route","import_source_route_id","name"});
            foreach(var sub in records.Where(p=>p.Key!="SEG"&&p.Key!="SEG_ALIAS"))foreach(var row in sub.Value)foreach(var field in row.Value.Pairs)
                if(!nonReferences.Contains(field.Key))foreach(string value in field.Value.Split(new[]{'|',';',',','\t','\r','\n'},StringSplitOptions.RemoveEmptyEntries))if(ids.Contains(value.Trim()))used.Add(value.Trim());
            // Opaque historical snapshots can contain old segment references. A
            // drawing with revision history requires a separate audited migration.
            bool historical=new[]{"HISTORY","LOCAL_HISTORY","CHANGES","RELEASES"}.Any(s=>records.ContainsKey(s)&&records[s].Count>0);
            if(historical)used.UnionWith(ids);
            var decisions=SegmentDuplicatePolicy.Plan(segments,used).ToList();
            var details=decisions.Select(d=>d.Reason+(d.Allowed?"\r\n\r\nDÒNG GIỮ:\r\n"+Describe(segments.Single(s=>s.Get("segment_id")==d.KeepId))+"\r\n\r\nDÒNG GOM VÀO NHẬT KÝ:\r\n"+Describe(segments.Single(s=>s.Get("segment_id")==d.RemoveId)):"")).ToArray();
            return new SegmentDuplicatePreview{Database=db,Stamp=Stamp(records),Decisions=decisions,Details=details};
        }
        public static SegmentDuplicatePreview Preview(Database db)
        { using(var tr=db.TransactionManager.StartOpenCloseTransaction())return Preview(tr,db); }

        // Only redundant, unreferenced SEG rows may be removed. Never rewrites an
        // OBJ, survey result, route or CAD entity, and never disables a guard.
        public static int Apply(Database db,SegmentDuplicatePreview preview)
        {
            if(preview==null||preview.Database!=db)throw new InvalidOperationException("Phương án thuộc bản vẽ khác. Kiểm tra lại trước khi gom.");
            bool disabled=!db.UndoRecording;if(disabled)db.DisableUndoRecording(false);
            try {using(var tr=db.TransactionManager.StartTransaction()){
                var fresh=Preview(tr,db);
                if(fresh.Stamp!=preview.Stamp)throw new InvalidOperationException("Dữ liệu đã đổi sau xem trước. Kiểm tra lại trước khi gom.");
                if(fresh.Blocked>0)throw new InvalidOperationException("Có cặp chưa đủ bằng chứng hoặc cả hai mã đang dùng. Giữ nguyên toàn bộ bảng; cần đối chiếu riêng.");
                int n=0;
                foreach(var d in fresh.Decisions.Where(d=>d.Allowed)){
                    var removed=BhtStore.Read(tr,db,"SEG",d.RemoveId);var kept=BhtStore.Read(tr,db,"SEG",d.KeepId);
                    var journal=new BhtRecord().Set("format","BHT_SEG_ALIAS/1").Set("removed_id",d.RemoveId).Set("kept_id",d.KeepId)
                        .Set("removed_record",ObjectRevision.Snapshot(removed)).Set("kept_record",ObjectRevision.Snapshot(kept)).Set("reason",d.Reason);
                    string key=ObjectRevision.Hash(journal);
                    if(BhtStore.Read(tr,db,"SEG_ALIAS",key)==null)BhtStore.Write(tr,db,"SEG_ALIAS",key,journal);
                    if(!BhtStore.Delete(tr,db,"SEG",d.RemoveId))throw new InvalidOperationException("Đoạn cần gom không còn tồn tại. Chưa hoàn tất giao dịch.");
                    n++;
                }
                tr.Commit();return n;
            }}finally{if(disabled)db.DisableUndoRecording(true);}
        }
    }
}
