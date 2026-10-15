using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using BHT.Core;

namespace BHT.Bridge
{
    internal static class SignLettering
    {
        internal static ObjectId Style(Transaction tr, Database db)
        {
            const string name = "BHT_SIGN_GIAOTHONG1_V101";
            var table = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);
            if (table.Has(name)) return table[name];
            table.UpgradeOpen();
            var style = new TextStyleTableRecord { Name = name, FileName = CustomSignPlate.FontPath(), XScale = 1, TextSize = 0 };
            ObjectId id = table.Add(style); tr.AddNewlyCreatedDBObject(style, true); return id;
        }

        internal static void Prepare(Transaction tr, ObjectId id, HashSet<ObjectId> seen)
        {
            if (!seen.Add(id)) return;
            var block = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
            foreach (ObjectId child in block)
            {
                var e = tr.GetObject(child, OpenMode.ForRead) as Entity;
                // Construction points belong to the private sign face only. Model-space
                // RTK points, support circles and origin blocks are not traversed here.
                if (e is DBPoint) { e.UpgradeOpen(); e.Erase(); continue; }
                var nested = e as BlockReference;
                if (nested != null)
                {
                    Prepare(tr, nested.BlockTableRecord, seen);
                    foreach (ObjectId a in nested.AttributeCollection) Text(tr, (Entity)tr.GetObject(a, OpenMode.ForRead));
                }
                Text(tr, e);
            }
        }

        private static void Text(Transaction tr, Entity entity)
        {
            var text = entity as DBText; var note = entity as MText;
            if (text == null && note == null) return;
            ObjectId style = Style(tr, entity.Database);
            var old = (TextStyleTableRecord)tr.GetObject(text != null ? text.TextStyleId : note.TextStyleId, OpenMode.ForRead);
            // Keep symbol-encoded fonts; they are pictures, not lettering.
            string file = Path.GetFileNameWithoutExtension(old.FileName ?? "").ToLowerInvariant();
            if (file.Contains("symbol") || file.Contains("wingding") || file == "tdt") return;
            bool legacy = file.StartsWith("vn") && !file.Contains("update");
            if (text != null)
            {
                if (text.TextStyleId == style) return;
                double width = 0;
                try { var b = text.GeometricExtents; width = b.MaxPoint.X - b.MinPoint.X; } catch { }
                text.UpgradeOpen(); text.TextStyleId = style;
                text.TextString = (legacy ? Tcvn3.Decode(text.TextString) : text.TextString).Normalize(NormalizationForm.FormC);
                text.AdjustAlignment(entity.Database);
                // Preserve available space without increasing letter height or stretching it.
                try { var b = text.GeometricExtents; double next = b.MaxPoint.X - b.MinPoint.X;
                    if (width > 1e-8 && next > width * 1.001) { text.WidthFactor *= width / next; text.AdjustAlignment(entity.Database); }
                } catch { }
            }
            else
            {
                string content = Regex.Replace(note.Contents, @"\\[fF][^;]*;", "");
                if (note.TextStyleId == style && content == note.Contents) return;
                double width = note.ActualWidth, height = note.ActualHeight;
                note.UpgradeOpen(); note.TextStyleId = style;
                note.Contents = (legacy ? Tcvn3.Decode(content) : content).Normalize(NormalizationForm.FormC);
                double ratio = 1;
                if (width > 1e-8 && note.ActualWidth > width) ratio = Math.Min(ratio, width / note.ActualWidth);
                if (height > 1e-8 && note.ActualHeight > height) ratio = Math.Min(ratio, height / note.ActualHeight);
                if (ratio < .999) note.TextHeight *= ratio;
            }
        }
    }
}
