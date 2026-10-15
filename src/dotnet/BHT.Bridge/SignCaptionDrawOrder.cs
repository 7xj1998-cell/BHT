using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(BHT.Bridge.SignCaptionDrawOrder))]
namespace BHT.Bridge
{
    public static class SignCaptionDrawOrder
    {
        // Sortents is safe from acedInvoke, unlike the DRAWORDER command.
        // Work in each owning layout; never alter a block definition or an xref.
        [LispFunction("BHTLABELFRONT")]
        public static int FromLisp(ResultBuffer args)
        {
            return Apply(AcApp.DocumentManager.MdiActiveDocument.Database);
        }

        public static int Apply(Database db)
        {
            int count = 0;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var table = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId owner in table)
                {
                    var space = (BlockTableRecord)tr.GetObject(owner, OpenMode.ForRead);
                    if (!space.IsLayout || space.IsFromExternalReference) continue;
                    var captions = new ObjectIdCollection();
                    foreach (ObjectId id in space)
                    {
                        var text = tr.GetObject(id, OpenMode.ForRead) as DBText;
                        if (text == null) continue;
                        using (var data = text.GetXDataForApplication("BHT_KH"))
                        {
                            if (data == null) continue;
                            var values = new List<string>();
                            foreach (TypedValue value in data)
                                if (value.TypeCode == 1000) values.Add(Convert.ToString(value.Value));
                            if (values.Count >= 2 && values[1] == "NHAN") captions.Add(id);
                        }
                    }
                    if (captions.Count == 0) continue;
                    var order = (DrawOrderTable)tr.GetObject(space.DrawOrderTableId, OpenMode.ForWrite);
                    order.MoveToTop(captions);
                    count += captions.Count;
                }
                tr.Commit();
            }
            return count;
        }
    }
}
