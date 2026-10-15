using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(BHT.Bridge.SignFillService))]
namespace BHT.Bridge
{
    public static class SignFillService
    {
        [LispFunction("BHTSIGNFILLALL")]
        public static int FromLisp(ResultBuffer args)
        {
            string value = args == null ? "" : Convert.ToString(args.AsArray()[0].Value);
            if (value != "0" && value != "1") throw new ArgumentException("Giá trị tô nền không hợp lệ.");
            return Apply(AcApp.DocumentManager.MdiActiveDocument.Database, value == "1");
        }

        public static int Apply(Database db, bool filled)
        {
            // One scan and one prototype per distinct block, without recalculating
            // stationing, placement, labels, leaders or survey points.
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var eligible = new HashSet<string>(BhtStore.All(tr, db, "OBJ")
                    .Where(p => new[] { "BIEN_BAO", "BANG_CHI_DAN", "BANG_QC", "KHAC", "CHUA_XAC_DINH" }.Contains(p.Value.Get("nhom")))
                    .Select(p => p.Key), StringComparer.OrdinalIgnoreCase);
                var changes = new Dictionary<ObjectId, ObjectId>();
                var refs = new List<BlockReference>();
                var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (var pair in BhtStore.Tagged(tr, db, "INSERT", "BHT_KH"))
                {
                    if (!eligible.Contains(pair.Key)) continue;
                    var br = tr.GetObject(pair.Value, OpenMode.ForRead) as BlockReference;
                    if (br == null) continue;
                    refs.Add(br);
                    if (changes.ContainsKey(br.BlockTableRecord)) continue;
                    string current = ((BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead)).Name;
                    string baseName = System.Text.RegularExpressions.Regex.Replace(current, @"_NOFILL_V\d+$", "");
                    if (!table.Has(baseName)) throw new InvalidOperationException("Thiếu block có nền: " + baseName + ". Cập nhật ký hiệu trước.");
                    string target = filled ? baseName : TdtSignLibrary.EnsureOutlineBlock(db, baseName);
                    changes.Add(br.BlockTableRecord, table[target]);
                }
                int count = 0;
                foreach (var br in refs)
                {
                    ObjectId next = changes[br.BlockTableRecord];
                    if (br.BlockTableRecord == next) continue;
                    br.UpgradeOpen(); br.BlockTableRecord = next; br.RecordGraphicsModified(true); count++;
                }
                if (BhtStore.Meta(tr, db, "sign_fill_all", "1") != (filled ? "1" : "0"))
                    BhtStore.MetaSet(tr, db, "sign_fill_all", filled ? "1" : "0");
                tr.Commit();
                return count;
            }
        }
    }
}
