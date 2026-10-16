using System;
using System.Linq;
using BHT.Core;
namespace BHT.CoreTests
{
    static class RevisionMerge134Tests
    {
        public static void Run(Action<string,string,bool> check)
        {
            Action<string,bool> ok=(n,v)=>check("M134-"+n,n,v);
            Action<string,Action> rejects=(n,a)=>{bool failed=false;try{a();}catch(Exception){failed=true;}ok(n,failed);};
            string p=Guid.NewGuid().ToString(),g=Guid.NewGuid().ToString(),b=Guid.NewGuid().ToString();
            var old=new BhtRecord().Set("object_id","OBJ001").Set("progress_locked","1").Set("ma_hieu","P.102");
            var baseline=ObjectRevision.History(old,"LEGACY_UNVERIFIED",p,g,b,"","baseline","QA","t");
            var r1=ObjectRevision.Next(old,old.Clone().Set("ma_hieu","P.103a"),p,g,b,"first","QA","2099");
            var h1=ObjectRevision.History(r1,r1.Get("revision_status"),p,g,r1.Get("revision_id"),b,"first","QA","2099");
            var r2=ObjectRevision.Next(r1,r1.Clone().Set("ma_hieu","P.104"),p,g,r1.Get("revision_id"),"second","QA","1900");
            var h2=ObjectRevision.History(r2,r2.Get("revision_status"),p,g,r2.Get("revision_id"),r1.Get("revision_id"),"second","QA","1900");
            var one=new[]{baseline,h1};var two=new[]{baseline,h1,h2};
            ok("new",RevisionMerge.Decide(null,r1,new BhtRecord[0],one)==RevisionDecision.New);
            ok("same",RevisionMerge.Decide(r1,r1,one,one)==RevisionDecision.Same);
            ok("old-keeps-new",RevisionMerge.Decide(r2,r1,two,one)==RevisionDecision.KeepCurrent);
            ok("descendant-upgrades-not-clock",RevisionMerge.Decide(r1,r2,one,two)==RevisionDecision.Upgrade);
            var fork=ObjectRevision.Next(r1,r1.Clone().Set("ma_hieu","P.105"),p,g,r1.Get("revision_id"),"fork","QA","2100");
            var hf=ObjectRevision.History(fork,fork.Get("revision_status"),p,g,fork.Get("revision_id"),r1.Get("revision_id"),"fork","QA","2100");
            rejects("fork",()=>RevisionMerge.Decide(r2,fork,two,new[]{baseline,h1,hf}));
            rejects("missing-parent",()=>RevisionMerge.Decide(r1,r2,one,new[]{h2}));
            rejects("payload-tamper",()=>RevisionMerge.Decide(r1,r2.Clone().Set("ma_hieu","P.999"),one,two));
            rejects("same-id-history-tamper",()=>RevisionMerge.Decide(r1,r1,one,new[]{baseline,h1.Clone().Set("revision_reason","changed")}));
            rejects("legacy-not-assumed-same",()=>RevisionMerge.Decide(old,r1,new BhtRecord[0],one));
            rejects("legacy-never-overwrites",()=>RevisionMerge.Decide(r2,old,two,new BhtRecord[0]));
            rejects("duplicate-revision-id",()=>RevisionMerge.Index(new[]{h1,h1.Clone().Set("revision_reason","bad")}));
            ok("display-local-does-not-change-survey",RevisionMerge.Decide(r1.Clone().Set("kh_free_x","9"),r1,one,one)==RevisionDecision.Same);
            rejects("different-project",()=>RevisionMerge.Decide(r1.Clone().Set("project_id",Guid.NewGuid().ToString()),r1,one,one));
            rejects("different-guid-same-code",()=>RevisionMerge.Decide(r1.Clone().Set("object_guid",Guid.NewGuid().ToString()),r1,one,one));
            var retired=ObjectRevision.Next(r2,ObjectRevision.Patch(r2,new BhtRecord(),CorrectionKind.Retire),p,g,r2.Get("revision_id"),"retired","QA","1901");
            var ht=ObjectRevision.History(retired,"RETIRED",p,g,retired.Get("revision_id"),r2.Get("revision_id"),"retired","QA","1901");
            ok("tombstone-keeps-lock",ObjectRevision.Retired(retired)&&ProgressPolicy.Locked(retired));
            ok("tombstone-no-resurrection",RevisionMerge.Decide(retired,r1,two.Concat(new[]{ht}),one)==RevisionDecision.KeepCurrent);
            rejects("ordinary-revive-blocked",()=>ObjectRevision.Patch(retired,new BhtRecord(),CorrectionKind.Attributes));
            ok("tombstone-report-excluded",!SignReportRecords.Includes(retired));
        }
    }
}
