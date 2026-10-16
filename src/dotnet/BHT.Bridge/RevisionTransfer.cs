using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using BHT.Core;

namespace BHT.Bridge
{
    public static partial class ProjectTransfer
    {
        static bool Versioned(IEnumerable<Row> rows) { return rows.Any(r=>r.Sub=="HISTORY"||r.Sub=="OBJ"&&ObjectRevision.Tracked(r.Record)); }
        static string ProjectId(IEnumerable<Row> rows) { var meta=rows.FirstOrDefault(r=>r.Sub=="META"&&r.Key=="CONFIG");return meta==null?"":meta.Record.Get("project_id"); }
        // Portable placement comparison deliberately omits handles, block names and text content.
        // Survey-only merges must not overwrite independently arranged drawing views.
        internal static string PresentationPose(Database db,Transaction tr,string objectId)
        {
            var values=new List<string>();
            var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
            var ms=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead);
            Func<double,string> n=x=>x.ToString("F6",System.Globalization.CultureInfo.InvariantCulture);
            Func<Autodesk.AutoCAD.Geometry.Point3d,string> p=x=>n(x.X)+","+n(x.Y)+","+n(x.Z);
            foreach(ObjectId id in ms) {
                var e=tr.GetObject(id,OpenMode.ForRead) as Entity;
                if(e==null||!new[]{"BHT_KH","BHT_SIGN_ORIGIN","BHT_OFFSET_DIM"}.Any(a=>TagId(e,a)==objectId))continue;
                string pose=e.GetType().Name+"|"+System.Text.RegularExpressions.Regex.Replace(e.Layer,@"^(\$\d+\$)+","")+"|";
                var block=e as BlockReference;var text=e as DBText;var line=e as Line;var poly=e as Polyline;
                if(block!=null)pose+=p(block.Position)+"|"+n(block.Rotation)+"|"+n(block.ScaleFactors.X)+","+n(block.ScaleFactors.Y)+","+n(block.ScaleFactors.Z);
                else if(text!=null)pose+=p(text.Position)+"|"+p(text.AlignmentPoint)+"|"+n(text.Rotation)+"|"+n(text.Height)+"|"+text.HorizontalMode+"|"+text.VerticalMode+"|"+text.TextString;
                else if(line!=null)pose+=p(line.StartPoint)+"|"+p(line.EndPoint);
                else if(poly!=null) { pose+=poly.Closed+"|"+n(poly.Elevation);for(int i=0;i<poly.NumberOfVertices;i++)pose+="|"+p(poly.GetPoint3dAt(i))+","+n(poly.GetBulgeAt(i)); }
                else if(e is AlignedDimension) {
                    var dim=(AlignedDimension)e;pose+=p(dim.XLine1Point)+"|"+p(dim.XLine2Point)+"|"+p(dim.DimLinePoint)+"|"+p(dim.TextPosition)+"|"+dim.DimensionText;
                }
                else throw new InvalidOperationException("Kiểu hình trình bày cần đối chiếu riêng: "+e.GetType().Name);
                values.Add(pose);
            }
            return string.Join("\n",values.OrderBy(v=>v,StringComparer.Ordinal));
        }
        static Dictionary<string,RevisionDecision> RevisionDecisions(List<Row> rows,List<Row> existing)
        {
            var result=new Dictionary<string,RevisionDecision>(StringComparer.OrdinalIgnoreCase);
            var conflicts=new List<ImportConflict>();
            string project=ProjectId(rows),targetProject=ProjectId(existing);
            if(project==""||targetProject!=""&&project!=targetProject||targetProject==""&&existing.Any(r=>r.Sub=="OBJ"))
                conflicts.Add(new ImportConflict("Dự án","PROJECT","Chưa đối chiếu định danh dự án; không tự ghép hồ sơ legacy/công trình khác.","project_id",targetProject,project));
            var incomingHistory=rows.Where(r=>r.Sub=="HISTORY").Select(r=>r.Record).ToArray();
            var targetHistory=existing.Where(r=>r.Sub=="HISTORY").Select(r=>r.Record).ToArray();
            foreach(var row in rows.Where(r=>r.Sub=="OBJ")) {
                var old=existing.FirstOrDefault(r=>r.Sub=="OBJ"&&r.Key==row.Key);
                if(ObjectRevision.Tracked(row.Record)&&existing.Any(r=>r.Sub=="OBJ"&&r.Key!=row.Key&&r.Record.Get("object_guid")==row.Record.Get("object_guid"))) {
                    conflicts.Add(new ImportConflict("Hồ sơ",row.Key,"Một định danh nội bộ có hai mã hiển thị; cần đối chiếu tham chiếu.","object_guid",row.Record.Get("object_guid"),row.Key));continue;
                }
                try {
                    var decision=RevisionMerge.Decide(old==null?null:old.Record,row.Record,targetHistory,incomingHistory);
                    if(decision==RevisionDecision.Legacy) {
                        conflicts.AddRange(ProgressPolicy.RollingConflicts(row.Key,old.Record,row.Record));
                        decision=!ProgressPolicy.Locked(old.Record)&&ProgressPolicy.Locked(row.Record)?RevisionDecision.Upgrade:RevisionDecision.Same;
                    }
                    result[row.Key]=decision;
                } catch(Exception ex) {
                    conflicts.Add(new ImportConflict("Phiên bản",row.Key,ex.Message,"revision_id",old==null?"":old.Record.Get("revision_id"),row.Record.Get("revision_id")));
                    if(old!=null)conflicts.AddRange(ImportConflict.Compare("Hồ sơ",row.Key,"Khác biệt cần đối chiếu",old.Record,row.Record,ProgressPolicy.ProtectedFields));
                }
            }
            if(conflicts.Count>0)throw new ImportConflictException(conflicts);
            return result;
        }
        static Dictionary<string,string> PrepareVersionedRolling(Database source,Transaction st,Database target,Transaction tt,ref List<Row> rows,HashSet<ObjectId> ids,List<Row> existing)
        {
            var decisions=RevisionDecisions(rows,existing);
            var accepted=new HashSet<string>(decisions.Where(p=>p.Value==RevisionDecision.New||p.Value==RevisionDecision.Upgrade).Select(p=>p.Key));
            var replaced=new HashSet<string>(decisions.Where(p=>p.Value==RevisionDecision.Upgrade).Select(p=>p.Key));
            var local=new HashSet<string>(existing.Where(r=>r.Sub=="VIEW_LOCAL").Select(r=>r.Key));
            var conflicts=new List<ImportConflict>();
            var sourcePoints=BhtStore.Points(st,source);var targetPoints=BhtStore.Points(tt,target);
            foreach(var points in new[]{sourcePoints,targetPoints})
                foreach(var duplicate in points.GroupBy(p=>p.IdUpper).Where(g=>g.Count()>1))
                    conflicts.Add(new ImportConflict("Điểm",duplicate.Key,"Trùng mã điểm; chưa nhập.","pt","",""));
            var pointIndex=targetPoints.ToLookup(p=>p.IdUpper);
            foreach(var p in sourcePoints)foreach(var q in pointIndex[p.IdUpper])
                if(Math.Abs(p.X-q.X)>1e-6||Math.Abs(p.Y-q.Y)>1e-6||Math.Abs(p.Z-q.Z)>1e-6)
                    conflicts.Add(new ImportConflict("Điểm",p.Id,"Tọa độ khác; không ghi đè dữ liệu đo gốc.","XYZ",q.X+","+q.Y+","+q.Z,p.X+","+p.Y+","+p.Z));
            var routeNames=new Dictionary<string,string>();
            foreach(var row in rows.Where(r=>r.Sub=="ROUTE")) {
                var old=existing.FirstOrDefault(r=>r.Sub=="ROUTE"&&r.Key==row.Key);
                if(!RouteGeometryMatches(source,st,row.Record)||old!=null&&(!ProgressPolicy.SameRoute(row.Record,old.Record)||!RouteGeometryMatches(target,tt,old.Record)))
                    conflicts.Add(new ImportConflict("Tim",row.Key,"Tim/mốc không khớp; không tự đổi tên tim của hồ sơ có lịch sử.","route_id",old==null?"":old.Key,row.Key));
                routeNames[row.Key]=row.Key;
            }
            foreach(var set in new[]{rows,existing})foreach(var obj in set.Where(r=>r.Sub=="OBJ")) {
                var seg=set.FirstOrDefault(r=>r.Sub=="SEG"&&r.Key==obj.Record.Get("doan"));
                if(seg==null||obj.Record.Get("goi")!=seg.Record.Get("package_id"))
                    conflicts.Add(new ImportConflict("Hồ sơ",obj.Key,"Đoạn/gói không nhất quán.","doan/goi",obj.Record.Get("doan"),obj.Record.Get("goi")));
            }
            foreach(var row in rows.Where(r=>r.Sub!="OBJ"&&r.Sub!="META"&&r.Sub!="ROUTE"&&r.Sub!="CAD_STATE"&&r.Sub!="LOCAL_HISTORY"&&r.Sub!="VIEW_LOCAL")) {
                var old=existing.FirstOrDefault(r=>r.Sub==row.Sub&&r.Key==row.Key);if(old==null)continue;
                if(row.Sub=="SEG")conflicts.AddRange(ImportConflict.Compare("Đoạn",row.Key,"Khác phạm vi/phía/gói/tim; cần đối chiếu",old.Record,row.Record,PackageAliasPolicy.SegmentFields.Concat(new[]{"route_id"})));
                else if(!ObjectRevision.Same(old.Record,row.Record))conflicts.Add(new ImportConflict(row.Sub,row.Key,"Bản ghi phụ trợ khác nhau; chưa nhập.","Dữ liệu",old.Record.ToCanonical(),row.Record.ToCanonical()));
            }
            if(conflicts.Count>0)throw new ImportConflictException(conflicts);
            ValidateConfirmedPoints(target,null);
            ValidateConfirmedPoints(source,null);
            if(accepted.Count==0) { rows.Clear();ids.Clear();return routeNames; }
            foreach(string id in replaced) {
                var next=rows.First(r=>r.Sub=="OBJ"&&r.Key==id).Record;
                if(ObjectRevision.Retired(next))continue;
                string sourcePose=PresentationPose(source,st,id),targetPose=PresentationPose(target,tt,id);
                var old=existing.First(r=>r.Sub=="OBJ"&&r.Key==id).Record;
                if(local.Contains(id)) {
                    if(next.Get("survey_revision")!=old.Get("survey_revision")||next.Get("assignment_revision")!=old.Get("assignment_revision"))
                        conflicts.Add(new ImportConflict("Trình bày LOCAL",id,"Nguồn có thay đổi khảo sát/phân đoạn; cần đối chiếu trước khi cập nhật ký hiệu LOCAL.","revision_scope",old.Get("revision_scope"),next.Get("revision_scope")));
                    continue;
                }
                var child=rows.FirstOrDefault(r=>r.Sub=="HISTORY"&&r.Record.Get("parent_revision")==old.Get("revision_id"));
                var state=rows.FirstOrDefault(r=>r.Sub=="CAD_STATE"&&r.Key==id);
                bool proven=child!=null&&child.Record.Get("before_pose")==targetPose&&state!=null&&state.Record.Get("revision_id")==next.Get("revision_id")&&state.Record.Get("pose")==sourcePose;
                if(old.Get("presentation_pose")!=""&&old.Get("presentation_pose")==targetPose&&next.Get("presentation_pose")==sourcePose)proven=true;
                if(sourcePose!=targetPose&&!proven)
                    conflicts.Add(new ImportConflict("Trình bày",id,"Bố trí hai bản khác nhau; giữ bản đích, đối chiếu trước khi nạp.","Vị trí / góc / tỷ lệ / nhãn","Giữ bố trí hiện hành","Bố trí nguồn khác"));
            }
            if(conflicts.Count>0)throw new ImportConflictException(conflicts);
            var preserved=rows.Where(r=>r.Sub=="OBJ"&&local.Contains(r.Key)&&!ObjectRevision.Retired(r.Record)).Select(r=>r.Key).ToArray();
            var presentationObjects=new HashSet<string>(accepted.Except(preserved));
            replaced.ExceptWith(preserved);
            var neededPoints=new HashSet<string>(rows.Where(r=>r.Sub=="OBJ"&&accepted.Contains(r.Key)).SelectMany(r=>r.Record.GetAll("pt")).Select(s=>s.ToUpperInvariant()));
            var newPoints=new HashSet<string>(neededPoints.Except(targetPoints.Select(p=>p.IdUpper)));
            var newPhotos=new HashSet<string>(rows.Where(r=>r.Sub=="PHOTO"&&!existing.Any(e=>e.Sub=="PHOTO"&&e.Key==r.Key)).Select(r=>r.Key));
            var routeIds=new HashSet<ObjectId>();
            foreach(var row in rows.Where(r=>r.Sub=="ROUTE"&&!existing.Any(e=>e.Sub=="ROUTE"&&e.Key==r.Key)))
                foreach(string key in new[]{"handle","tdt_source_handle"})routeIds.Add(HandleId(source,row.Record.Get(key)));
            ids.RemoveWhere(id=>!routeIds.Contains(id)&&!Owned((Entity)st.GetObject(id,OpenMode.ForRead),presentationObjects,newPoints,newPhotos));
            var bt=(BlockTable)tt.GetObject(target.BlockTableId,OpenMode.ForRead);
            var ms=(BlockTableRecord)tt.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead);
            var erase=new List<Entity>();
            foreach(ObjectId id in ms) {
                var entity=tt.GetObject(id,OpenMode.ForRead) as Entity;
                if(entity==null||!new[]{"BHT_KH","BHT_SIGN_ORIGIN","BHT_OFFSET_DIM"}.Any(tag=>replaced.Contains(TagId(entity,tag))))continue;
                if(((LayerTableRecord)tt.GetObject(entity.LayerId,OpenMode.ForRead)).IsLocked)throw new InvalidOperationException("Ký hiệu nhận phiên bản mới nằm trên lớp khóa; mở khóa lớp trình bày trước khi nạp.");
                erase.Add(entity);
            }
            var acceptedGuids=new HashSet<string>(rows.Where(r=>r.Sub=="OBJ"&&accepted.Contains(r.Key)).Select(r=>r.Record.Get("object_guid")));
            rows=rows.Where(r=>r.Sub=="CAD_STATE"?presentationObjects.Contains(r.Key):r.Sub=="VIEW_LOCAL"||r.Sub=="LOCAL_HISTORY"?false:r.Sub=="OBJ"?accepted.Contains(r.Key):r.Sub=="META"?true:
                (r.Sub!="HISTORY"&&r.Sub!="CHANGES"||acceptedGuids.Contains(r.Record.Get("object_guid")))&&!existing.Any(e=>e.Sub==r.Sub&&e.Key==r.Key)).ToList();
            foreach(var row in rows.Where(r=>r.Sub=="META")) {
                var previous=existing.FirstOrDefault(r=>r.Sub=="META"&&r.Key==row.Key);
                if(previous!=null)row.Record=previous.Record.Clone().Set("project_id",ProjectId(rows));
            }
            // All validation precedes entity removal; the caller owns one rollback transaction.
            foreach(var entity in erase) { entity.UpgradeOpen();entity.Erase(); }
            foreach(var row in rows.Where(r=>r.Sub=="OBJ"&&ObjectRevision.Retired(r.Record)))
                if(BhtStore.Read(tt,target,"VIEW_LOCAL",row.Key)!=null)BhtStore.Delete(tt,target,"VIEW_LOCAL",row.Key);
            return routeNames;
        }
    }
    public static partial class BhtStore
    {
        internal static void WriteMergedRevision(Transaction tr,Database db,string id,BhtRecord next)
        {
            DrawingProject.RequireWritable(tr,db);
            var old=Read(tr,db,"OBJ",id);
            if(!ObjectRevision.Tracked(next)) { Write(tr,db,"OBJ",id,next);return; }
            var history=All(tr,db,"HISTORY").Select(p=>p.Value).ToArray();
            var decision=RevisionMerge.Decide(old,next,history,history);
            if(decision!=RevisionDecision.New&&decision!=RevisionDecision.Upgrade)throw new InvalidOperationException("Không có phiên bản mới được duyệt để ghi.");
            WriteRaw(tr,db,"OBJ",id,next);
        }
    }
}
