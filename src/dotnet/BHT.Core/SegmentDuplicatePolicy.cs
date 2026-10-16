using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BHT.Core
{
    public sealed class SegmentDuplicateDecision
    {
        public string RemoveId, KeepId, Reason;
        public bool Allowed { get { return RemoveId != ""; } }
    }

    // Spelling alone identifies a candidate, never permission to merge survey data.
    public static class SegmentDuplicatePolicy
    {
        public static string SpellingKey(string id)
        {
            var m=Regex.Match(id??"",@"^DOAN_?([0-9]+)$",RegexOptions.CultureInvariant);
            return m.Success?"DOAN_"+m.Groups[1].Value:id??"";
        }
        public static bool SameMeaning(BhtRecord a,BhtRecord b)
        {
            try {
                return SegmentTable.Metres(a.Get("km_start_m"))==SegmentTable.Metres(b.Get("km_start_m"))
                    &&SegmentTable.Metres(a.Get("km_end_m"))==SegmentTable.Metres(b.Get("km_end_m"))
                    &&SegmentTable.Side(a.Get("side"))==SegmentTable.Side(b.Get("side"))
                    &&a.Get("package_id")!=""&&PackageAliasPolicy.SpellingKey(a.Get("package_id"))==PackageAliasPolicy.SpellingKey(b.Get("package_id"))
                    &&SegmentTable.Text(a.Get("package_name"))!=""&&SegmentTable.Text(a.Get("package_name"))==SegmentTable.Text(b.Get("package_name"));
            }catch{return false;}
        }
        public static IList<SegmentDuplicateDecision> Plan(IEnumerable<BhtRecord> records,ISet<string> referenced)
        {
            var result=new List<SegmentDuplicateDecision>();
            foreach(var group in records.GroupBy(r=>SpellingKey(r.Get("segment_id"))).Where(g=>g.Count()>1)) {
                var rows=group.ToArray();
                var d=new SegmentDuplicateDecision{RemoveId="",KeepId="",Reason=string.Join(" / ",rows.Select(r=>r.Get("segment_id")))};
                if(rows.Length!=2||!SameMeaning(rows[0],rows[1])){d.Reason+=" — khác phạm vi/phía/gói/tên hoặc không đủ bằng chứng; giữ nguyên.";result.Add(d);continue;}
                var used=rows.Where(r=>referenced.Contains(r.Get("segment_id"))).ToArray();
                if(used.Length>1){d.Reason+=" — cả hai mã đang được tham chiếu; cần đối chiếu riêng, giữ nguyên.";result.Add(d);continue;}
                var keep=used.Length==1?used[0]:rows.OrderByDescending(r=>r.Get("route_id")!="").ThenByDescending(r=>r.Get("segment_id")==group.Key).First();
                var remove=rows.First(r=>r!=keep);
                if(remove.Get("route_id")!=""&&remove.Get("route_id")!=keep.Get("route_id")){d.Reason+=" — liên kết tim khác nhau; giữ nguyên.";result.Add(d);continue;}
                d.KeepId=keep.Get("segment_id");d.RemoveId=remove.Get("segment_id");
                d.Reason=d.RemoveId+" → giữ "+d.KeepId+"; cùng phạm vi/phía/tên gói, dòng bỏ không có tham chiếu, không mất liên kết tim.";
                result.Add(d);
            }
            return result;
        }
        public static IList<ImportConflict> NewAliasConflicts(IEnumerable<BhtRecord> incoming,IEnumerable<BhtRecord> current)
        {
            var target=current.ToArray();var keys=new HashSet<string>(target.Select(r=>r.Get("segment_id")),StringComparer.Ordinal);
            var result=new List<ImportConflict>();
            foreach(var row in incoming.Where(r=>!keys.Contains(r.Get("segment_id"))))foreach(var old in target.Where(r=>SpellingKey(r.Get("segment_id"))==SpellingKey(row.Get("segment_id"))))
                result.Add(new ImportConflict("Đoạn",row.Get("segment_id"),"Mã khác cách viết có thể tạo đoạn trùng. Kiểm tra / gom đoạn trùng trên bản sao trước khi nạp.","segment_id",old.Get("segment_id"),row.Get("segment_id")));
            return result;
        }
    }
}
