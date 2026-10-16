using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BHT.Core
{
    public static class PackageAliasPolicy
    {
        public static readonly string[] SegmentFields = { "km_start_m", "km_end_m", "side", "package_id" };

        // Accept only the documented GOI01/GOI_01 spelling. Numbers and arbitrary
        // punctuation are not evidence that two contractual packages are equal.
        public static string SpellingKey(string id)
        {
            var match=Regex.Match(id??"",@"^GOI_?([0-9]+)$",RegexOptions.CultureInvariant);
            return match.Success?"GOI_"+match.Groups[1].Value:id;
        }

        public static Dictionary<string,string> VerifiedAliases(IEnumerable<BhtRecord> incoming,IEnumerable<BhtRecord> current)
        {
            var source=incoming.ToArray();var target=current.ToArray();var result=new Dictionary<string,string>();
            foreach(var group in source.GroupBy(s=>s.Get("package_id"))) {
                string from=group.Key;
                var candidates=target.Select(s=>s.Get("package_id")).Distinct().Where(p=>p!=from&&SpellingKey(p)==SpellingKey(from)).ToArray();
                if(candidates.Length!=1||target.Any(s=>s.Get("package_id")==from)||source.Any(s=>s.Get("package_id")==candidates[0]))continue;
                string to=candidates[0];var destination=target.Where(s=>s.Get("package_id")==to).ToArray();
                if(!group.Select(s=>s.Get("segment_id")).OrderBy(s=>s).SequenceEqual(destination.Select(s=>s.Get("segment_id")).OrderBy(s=>s)))continue;
                if(group.All(s=>destination.Any(t=>t.Get("segment_id")==s.Get("segment_id")&&
                    s.Get("package_name")!=""&&new[]{"package_name","km_start_m","km_end_m","side"}.All(k=>s.Get(k)==t.Get(k)))))result.Add(from,to);
            }
            return result;
        }
    }
}
