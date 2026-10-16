using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BHT.Core
{
    public enum RevisionDecision { New, Same, KeepCurrent, Upgrade, Legacy }

    public static class RevisionMerge
    {
        public static IDictionary<string,BhtRecord> Index(IEnumerable<BhtRecord> entries)
        {
            var result=new Dictionary<string,BhtRecord>(StringComparer.OrdinalIgnoreCase);
            foreach(var entry in entries) {
                string id=entry.Get("revision_id");Guid parsed;
                if(!Guid.TryParse(id,out parsed))throw new InvalidDataException("Revision ID không hợp lệ.");
                BhtRecord existing;
                if(result.TryGetValue(id,out existing)) {
                    if(!ObjectRevision.Same(existing,entry))throw new InvalidDataException("Cùng revision ID nhưng khác lịch sử.");
                } else result.Add(id,entry);
            }
            return result;
        }
        public static HashSet<string> Chain(BhtRecord current,IDictionary<string,BhtRecord> entries)
        {
            var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if(!ObjectRevision.Tracked(current))throw new InvalidDataException("Hồ sơ chưa có định danh phiên bản.");
            Guid parsed;
            if(!Guid.TryParse(current.Get("project_id"),out parsed)||!Guid.TryParse(current.Get("object_guid"),out parsed))
                throw new InvalidDataException("Định danh dự án/đối tượng không hợp lệ.");
            string next=current.Get("revision_id");BhtRecord child=null;
            while(next!="") {
                BhtRecord entry;
                if(!seen.Add(next)||!entries.TryGetValue(next,out entry))throw new InvalidDataException("Thiếu revision cha hoặc lịch sử có vòng lặp.");
                foreach(string field in new[]{"project_id","object_guid","object_id"})
                    if(entry.Get(field)!=current.Get(field))throw new InvalidDataException("Lịch sử thuộc đối tượng/dự án khác.");
                var snapshot=ObjectRevision.ReadHistory(entry);
                if(child==null&&!ObjectRevision.Same(ObjectRevision.Governed(current),ObjectRevision.Governed(snapshot)))
                    throw new InvalidDataException("Dữ liệu hiện hành khác snapshot revision.");
                if(ObjectRevision.Tracked(snapshot)) {
                    if(entry.Get("parent_revision")=="")throw new InvalidDataException("Revision hiệu chỉnh thiếu baseline cha.");
                    foreach(string field in new[]{"project_id","object_guid","object_id","revision_id","parent_revision","revision_status","revision_reason","revision_time","revision_author"})
                        if(entry.Get(field)!=snapshot.Get(field))throw new InvalidDataException("Metadata lịch sử khác snapshot.");
                } else if(entry.Get("revision_status")!="LEGACY_UNVERIFIED"||entry.Get("parent_revision")!="")
                    throw new InvalidDataException("Baseline cũ không hợp lệ.");
                if(child!=null)foreach(string counter in new[]{"object_revision","survey_revision","assignment_revision","display_revision"}) {
                    int a=0,b;
                    if(ObjectRevision.Tracked(snapshot)&&(!int.TryParse(snapshot.Get(counter),out a)||a<0))throw new InvalidDataException("Số phiên bản cha không hợp lệ.");
                    if(!int.TryParse(child.Get(counter),out b)||b<a||(long)b-a>1||(counter=="object_revision"&&(long)b-a!=1))
                        throw new InvalidDataException("Số phiên bản không khớp quan hệ cha.");
                }
                child=snapshot;next=entry.Get("parent_revision");
            }
            return seen;
        }
        public static RevisionDecision Decide(BhtRecord current,BhtRecord incoming,IEnumerable<BhtRecord> currentHistory,IEnumerable<BhtRecord> incomingHistory)
        {
            if(incoming==null)throw new ArgumentNullException("incoming");
            if(!ObjectRevision.Tracked(current)&&!ObjectRevision.Tracked(incoming))return current==null?RevisionDecision.New:RevisionDecision.Legacy;
            var source=Index(incomingHistory);var target=Index(currentHistory);
            if(!ObjectRevision.Tracked(incoming)||current!=null&&!ObjectRevision.Tracked(current))
                throw new InvalidDataException("Hồ sơ legacy chưa được đối chiếu định danh dự án/đối tượng; chưa nhập.");
            var ancestors=Chain(incoming,source);
            if(current==null)return RevisionDecision.New;
            foreach(string key in new[]{"project_id","object_guid","object_id"})
                if(current.Get(key)!=incoming.Get(key))throw new InvalidDataException("Trùng mã hiển thị nhưng khác định danh hoặc mã nội bộ chưa được đối chiếu.");
            var existing=Chain(current,target);
            foreach(string id in ancestors.Intersect(existing,StringComparer.OrdinalIgnoreCase))
                if(!ObjectRevision.Same(source[id],target[id]))throw new InvalidDataException("Cùng revision ID nhưng khác nội dung; không hợp nhất.");
            if(current.Get("revision_id")==incoming.Get("revision_id"))return RevisionDecision.Same;
            if(existing.Contains(incoming.Get("revision_id")))return RevisionDecision.KeepCurrent;
            if(ancestors.Contains(current.Get("revision_id")))return RevisionDecision.Upgrade;
            throw new InvalidDataException("Hai nhánh hiệu chỉnh độc lập; đối chiếu trước khi hợp nhất. Không chọn theo thời gian.");
        }
    }
}
