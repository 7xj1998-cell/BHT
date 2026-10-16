using System;
using System.Collections.Generic;
using System.Linq;
using BHT.Core;
namespace BHT.CoreTests
{
 public static class Segment113Tests
 {
  public static void Run(Action<string,string,bool> check){
   var a=new BhtRecord().Set("segment_id","DOAN08").Set("package_id","GOI03").Set("km_start_m","48469").Set("km_end_m","50290").Set("side","PHAI").Set("package_name","KCN Hựu Thạnh").Set("route_id","R08");
   var b=a.Clone().Set("segment_id","DOAN_08").Set("package_id","GOI_03").Set("route_id","");
   Func<BhtRecord[],string[],IList<SegmentDuplicateDecision>> plan=(rows,used)=>SegmentDuplicatePolicy.Plan(rows,new HashSet<string>(used));
   check("S113-equivalent","verified spelling candidates have same meaning",SegmentDuplicatePolicy.SameMeaning(a,b));
   check("S113-preserve-used","keep referenced old segment without rewriting objects",plan(new[]{a,b},new[]{"DOAN08"}).Single().RemoveId=="DOAN_08");
   check("S113-both-used","both referenced must block",!plan(new[]{a,b},new[]{"DOAN08","DOAN_08"}).Single().Allowed);
   foreach(var field in new[]{"km_start_m","km_end_m","side","package_id","package_name"})check("S113-different-"+field,"different meaning must block",!plan(new[]{a,b.Clone().Set(field,"DIFFERENT")},new string[0]).Single().Allowed);
   check("S113-route","different nonempty route cannot be discarded",!plan(new[]{a,b.Clone().Set("route_id","R99")},new[]{"DOAN08"}).Single().Allowed);
   check("S113-zero","do not guess leading zero aliases",SegmentDuplicatePolicy.SpellingKey("DOAN8")!=SegmentDuplicatePolicy.SpellingKey("DOAN_08"));
   check("S113-unrelated","arbitrary names stay distinct",plan(new[]{a,b.Clone().Set("segment_id","SEGMENT_08")},new string[0]).Count==0);
   check("S113-idempotent","single surviving row has no cleanup",plan(new[]{a},new string[0]).Count==0);
   check("S113-import","preflight blocks new alternate segment key",SegmentDuplicatePolicy.NewAliasConflicts(new[]{b},new[]{a}).Count==1);
   check("S113-repeat","same keys remain importable",SegmentDuplicatePolicy.NewAliasConflicts(new[]{a,b},new[]{a,b}).Count==0);
   check("S113-numeric","numeric spelling is compared safely",SegmentDuplicatePolicy.SameMeaning(a,b.Clone().Set("km_start_m","Km48+469")));
  }
 }
}
