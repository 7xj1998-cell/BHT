using System;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
public static class Segment119Probe
{
    static void Check(bool test, string message) { if (!test) throw new InvalidOperationException(message); }
    static string Snapshot(Database db) { using (var tr = db.TransactionManager.StartOpenCloseTransaction()) return string.Join("|", new[] { "SEG", "OBJ", "META" }.SelectMany(s => BhtStore.All(tr, db, s).Select(p => s + p.Key + p.Value.ToCanonical())).ToArray()); }
    [CommandMethod("SEG119STORE")]
    public static void Run()
    {
        var doc = AcApp.DocumentManager.MdiActiveDocument; var db = doc.Database;
        string dir = Path.Combine(Path.GetTempPath(), "BHTSegProbe-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(dir);
        try
        {
            doc.Editor.WriteMessage("\nSEG119-STORE-START");
            var rows = SegmentStore.Read(db); string before = Snapshot(db); bool rejected = false;
            var broken = rows.Select(r => r.Clone()).ToList(); broken[1].Set("route_id", "MISSING");
            try { SegmentStore.Apply(db, broken, "bad", true); } catch { rejected = true; }
            Check(rejected && before == Snapshot(db), "invalid route changed database");
            doc.Editor.WriteMessage("\nSEG119-STORE-VALIDATED");
            string file = Path.Combine(dir, "segments.bhtseg"); SegmentTable.Write(file, rows);
            SegmentStore.Apply(db, SegmentTable.Read(file), file, true);
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                var tie = BhtStore.Read(tr, db, "OBJ", "TIE");
                Check(tie.Get("gan_doan_pp") == "THU_CONG" && tie.Get("doan") == "DITCH-A", "unchanged manual decision lost");
                Check(BhtStore.Read(tr, db, "OBJ", "DITCH-SIGN").Get("doan") == "", "old automatic assignment retained");
            }
            var changed = rows.Select(r => r.Clone()).ToList(); changed.Single(r => r.Get("segment_id") == "DITCH-A").Set("route_id", "ROAD");
            SegmentStore.Apply(db, changed, file, true);
            using (var tr = db.TransactionManager.StartOpenCloseTransaction()) Check(BhtStore.Read(tr, db, "OBJ", "TIE").Get("doan") == "", "changed manual binding retained");
            SegmentStore.Apply(db, rows, file, true);
            doc.Editor.WriteMessage("\nSEG119-STORE-APPLIED");
            string project = Path.Combine(dir, "transfer.bht"); ProjectTransfer.Export(db, project);
            doc.Editor.WriteMessage("\nSEG119-STORE-EXPORTED");
            using (var target = new Database(true, true))
            {
                ProjectTransfer.Import(target, project);
                var transferred = SegmentStore.Read(target);
                Check(transferred.Single(r => r.Get("segment_id") == "DITCH-A").Get("route_id") == "DITCH-NEW", "project transfer lost route link");
                using (var tr = target.TransactionManager.StartOpenCloseTransaction()) Check(BhtStore.Read(tr, target, "ROUTE", "DITCH-NEW") != null, "project transfer lost route");
            }
            doc.Editor.WriteMessage("\nSEG119-STORE-PASS atomic apply, manual invalidation, native roundtrip, project transfer");
        }
        catch (System.Exception ex) { doc.Editor.WriteMessage("\nFAIL SEG119 STORE " + ex); }
        finally { Directory.Delete(dir, true); }
    }
}
