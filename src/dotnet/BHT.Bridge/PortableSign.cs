using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(BHT.Bridge.PortableSign))]

namespace BHT.Bridge
{
    // Clipboard cloning can discard SORTENTSTABLE. Store the same order in the
    // actual entity sequence of private definitions so another CAD needs no BHT.
    public static class PortableSign
    {
        internal static void StoreOrder(Transaction tr, ObjectId blockId, HashSet<ObjectId> seen)
        {
            if (!seen.Add(blockId)) return;
            var block = (BlockTableRecord)tr.GetObject(blockId, OpenMode.ForRead);
            var draw = (DrawOrderTable)tr.GetObject(block.DrawOrderTableId, OpenMode.ForRead);
            var ordered = draw.GetFullDrawOrder(0).Cast<ObjectId>().ToArray();
            foreach (ObjectId id in ordered)
            {
                var nested = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                if (nested != null) StoreOrder(tr, nested.BlockTableRecord, seen);
            }
            if (block.Cast<ObjectId>().SequenceEqual(ordered)) return;
            foreach (ObjectId id in ordered)
            {
                var map = new IdMapping();
                block.Database.DeepCloneObjects(new ObjectIdCollection(new[] { id }), blockId, map, false);
                var hatch = tr.GetObject(map[id].Value, OpenMode.ForWrite) as Hatch;
                if (hatch != null && hatch.Associative) hatch.Associative = false;
            }
            foreach (ObjectId id in ordered) tr.GetObject(id, OpenMode.ForWrite).Erase();
        }

        public static string Ensure(Database db, string sourceName)
        {
            if (sourceName.StartsWith("BHT_PORTABLE_V110_", StringComparison.Ordinal)) return sourceName;
            string name = "BHT_PORTABLE_V110_" + SignLayout.Hash(sourceName);
            string baseName = System.Text.RegularExpressions.Regex.Replace(sourceName, @"_NOFILL_V\d+$", "");
            if (baseName != sourceName) name = Ensure(db, baseName) + sourceName.Substring(baseName.Length);
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (!table.Has(sourceName)) throw new ArgumentException("Không tìm thấy block biển: " + sourceName);
                if (table.Has(name)) return name;
                Copy(tr, db, table[sourceName], name, new Dictionary<ObjectId, ObjectId>(), new HashSet<ObjectId>());
                tr.Commit();
                return name;
            }
        }

        private static ObjectId Copy(Transaction tr, Database db, ObjectId sourceId, string name,
            Dictionary<ObjectId, ObjectId> copies, HashSet<ObjectId> path)
        {
            if (path.Contains(sourceId)) throw new InvalidOperationException("Block biển có tham chiếu vòng.");
            ObjectId existing;
            if (copies.TryGetValue(sourceId, out existing)) return existing;
            path.Add(sourceId);
            var source = (BlockTableRecord)tr.GetObject(sourceId, OpenMode.ForRead);
            if (source.IsFromExternalReference) throw new InvalidOperationException("Bind Xref trước khi tạo block biển để sao chép.");
            var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
            var target = new BlockTableRecord { Name = name, Origin = source.Origin, Units = source.Units };
            ObjectId targetId = table.Add(target); tr.AddNewlyCreatedDBObject(target, true);
            copies.Add(sourceId, targetId);
            var order = (DrawOrderTable)tr.GetObject(source.DrawOrderTableId, OpenMode.ForRead);
            foreach (ObjectId id in order.GetFullDrawOrder(0))
            {
                var entity = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (entity == null) continue;
                var nested = entity as BlockReference;
                ObjectId nestedId = ObjectId.Null;
                if (nested != null)
                    nestedId = Copy(tr, db, nested.BlockTableRecord, name + "_" + copies.Count,
                        copies, path);
                // Deep clone one entity at a time, retaining attributes and hatch
                // geometry while making append order independent of draw order.
                var map = new IdMapping();
                db.DeepCloneObjects(new ObjectIdCollection(new[] { id }), targetId, map, false);
                var clone = (Entity)tr.GetObject(map[id].Value, OpenMode.ForWrite);
                if (clone.Color.IsByLayer)
                    clone.Color = ((LayerTableRecord)tr.GetObject(entity.LayerId, OpenMode.ForRead)).Color;
                // Associative boundaries are not part of this private display block.
                var hatch = clone as Hatch;
                if (hatch != null && hatch.Associative) hatch.Associative = false;
                if (nested != null)
                {
                    var reference = (BlockReference)clone;
                    reference.BlockTableRecord = nestedId;
                    foreach (ObjectId aid in reference.AttributeCollection)
                    {
                        var attribute = (AttributeReference)tr.GetObject(aid, OpenMode.ForWrite);
                        if (attribute.Color.IsByLayer)
                            attribute.Color = ((LayerTableRecord)tr.GetObject(attribute.LayerId, OpenMode.ForRead)).Color;
                    }
                }
            }
            path.Remove(sourceId);
            return targetId;
        }

        [LispFunction("BHTSIGNPORTABLE")]
        public static string Build(ResultBuffer args)
        {
            if (args == null || args.AsArray().Length != 1) throw new ArgumentException("Cần tên block biển.");
            return Ensure(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database,
                Convert.ToString(args.AsArray()[0].Value));
        }

        [CommandMethod("BHTSUASAOCHEP", CommandFlags.UsePickSet)]
        public static void RepairSelected()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var selection = doc.Editor.GetSelection(new Autodesk.AutoCAD.EditorInput.SelectionFilter(
                new[] { new TypedValue((int)DxfCode.Start, "INSERT") }));
            if (selection.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) return;
            int count = 0;
            try
            {
                using (var tr = doc.Database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in selection.Value.GetObjectIds())
                    {
                        var insert = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                        string source = ((BlockTableRecord)tr.GetObject(insert.BlockTableRecord, OpenMode.ForRead)).Name;
                        if (!source.StartsWith("BHT_", StringComparison.OrdinalIgnoreCase) || insert.IsDynamicBlock) continue;
                        string portable = Ensure(doc.Database, source);
                        var table = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                        insert.UpgradeOpen(); insert.BlockTableRecord = table[portable]; count++;
                    }
                    tr.Commit();
                }
                doc.Editor.Regen();
                doc.Editor.WriteMessage("\nĐã chuẩn bị " + count + " block BHT để sao chép. Thực hiện trong DWG nguồn khi biển đang hiển thị đúng, rồi copy sang DWG đích.");
            }
            catch (System.Exception ex) { doc.Editor.WriteMessage("\nKhông chuẩn bị được block để sao chép: " + ex.Message); }
        }
    }
}
