using System;
using System.Linq;
using BHT.Core;
namespace BHT.CoreTests
{
    static class Revision132Tests
    {
        public static void Run(Action<string,string,bool> check)
        {
            Action<string,bool> ok=(name,value)=>check("R132-"+name,name,value);
            Action<string,Action> rejects=(name,act)=>{bool blocked=false;try{act();}catch(Exception){blocked=true;}ok(name,blocked);};
            var old=new BhtRecord().Set("object_id","OBJ001").Set("progress_locked","1").Set("ma_hieu","P.102").Set("doan","DOAN_01").Set("ly_trinh_m","1006.000").Set("pt","P1");
            string snapshot=ObjectRevision.Snapshot(old);
            var next=ObjectRevision.Patch(old,new BhtRecord().Set("ma_hieu","P.103a"),CorrectionKind.Attributes);
            var revision=ObjectRevision.Next(old,next,"project","guid","baseline","Sửa sau đối chiếu ảnh","QA","2026-10-15T00:00:00Z");
            ok("isolated-code-correction",next.Get("ma_hieu")=="P.103a" && next.Get("ly_trinh_m")==old.Get("ly_trinh_m") && ObjectRevision.Snapshot(old)==snapshot);
            ok("survey-only-counter",revision.Get("survey_revision")=="1"&&revision.Get("assignment_revision")=="0"&&revision.Get("display_revision")=="0");
            ok("stable-parent-identity",revision.Get("parent_revision")=="baseline"&&revision.Get("object_guid")=="guid"&&revision.Get("object_id")=="OBJ001");
            var second=ObjectRevision.Next(revision,revision.Clone().Set("doan","DOAN_02"),"project","guid",revision.Get("revision_id"),"Chuyển đoạn","QA","1900-01-01");
            ok("assignment-separate-survey",second.Get("survey_revision")=="1"&&second.Get("assignment_revision")=="1"&&second.Get("object_revision")=="2");
            ok("parent-not-timestamp",second.Get("parent_revision")==revision.Get("revision_id"));
            foreach(string field in new[]{"object_guid","revision_id","progress_locked","doan","ly_trinh_m","pt","sign_content","mat"})rejects("patch-forbidden-"+field,()=>ObjectRevision.Patch(old,new BhtRecord().Set(field,"bad"),CorrectionKind.Attributes));
            rejects("too-many-faces",()=>ObjectRevision.Patch(old,new BhtRecord().Set("so_mat","21"),CorrectionKind.Attributes));
            rejects("unlocked-not-correction",()=>ObjectRevision.Patch(old.Clone().Set("progress_locked","0"),new BhtRecord(),CorrectionKind.Attributes));
            rejects("multiface-code-rejected",()=>ObjectRevision.Patch(old.Clone().Add("mat","P.102").Add("mat","S.509"),new BhtRecord().Set("ma_hieu","P.103a"),CorrectionKind.Attributes));
            rejects("missing-reason",()=>ObjectRevision.Next(old,next,"p","o","r"," ","QA","t"));
            rejects("no-op",()=>ObjectRevision.Next(old,old,"p","o","r","why","QA","t"));
            rejects("invalid-counter",()=>ObjectRevision.Next(revision.Clone().Set("survey_revision","NaN"),next,"p","o","r","why","QA","t"));
            var special=new BhtRecord().Add("k","a\nb=c").Add("k","Việt Nam = \"x\"; \\t").Add("empty","");
            ok("lossless-snapshot",ObjectRevision.Same(special,ObjectRevision.Restore(ObjectRevision.Snapshot(special))));
            ok("ambiguous-lines-no-collision",ObjectRevision.Hash(new BhtRecord().Add("a","x\nb=y"))!=ObjectRevision.Hash(new BhtRecord().Add("a","x").Add("b","y")));
            var entry=ObjectRevision.History(old,"LEGACY_UNVERIFIED","p","o","r","","why","QA","t");
            ok("legacy-baseline-exact",ObjectRevision.Same(old,ObjectRevision.ReadHistory(entry))&&entry.Get("revision_status")=="LEGACY_UNVERIFIED");
            rejects("history-tampering",()=>ObjectRevision.ReadHistory(entry.Clone().Set("snapshot",ObjectRevision.Snapshot(next))));
            ok("display-edit-no-survey-write",ObjectRevision.OrdinaryWriteAllowed(revision,revision.Clone().Set("kh_free_x","50").Set("sua_luc","later")));
            foreach(string field in new[]{"ma_hieu","pt","doan","progress_locked","revision_id","ghi_chu","unknown_field"})
                ok("tracked-guard-"+field,!ObjectRevision.OrdinaryWriteAllowed(revision,revision.Clone().Set(field,"bad")));
            ok("tracked-deletion-blocked",!ObjectRevision.OrdinaryWriteAllowed(revision,null));
        }
    }
}
