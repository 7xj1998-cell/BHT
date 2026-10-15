using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;
[assembly: CommandClass(typeof(BHT.Bridge.DisplayBounds))]
namespace BHT.Bridge
{
    public static class DisplayBounds
    {
        // WCS extents of the display and its labels; excludes leaders and survey origins.
        // Fail closed when any component has no usable extents.
        [LispFunction("BHTDISPLAYBOUNDS")]
        public static ResultBuffer Bounds(ResultBuffer args)
        {
            if (args == null) return null;
            var db = App.DocumentManager.MdiActiveDocument.Database;
            Extents3d? total = null;
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (var value in args.AsArray())
                {
                    if (!(value.Value is ObjectId)) continue;
                    var id = (ObjectId)value.Value;
                    if (id.IsNull || id.IsErased || id.Database != db) return null;
                    var entity = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    if (entity == null) return null;
                    Extents3d bounds;
                    try { bounds = entity.GeometricExtents; }
                    catch (Autodesk.AutoCAD.Runtime.Exception) { return null; }
                    if (total.HasValue) { var merged = total.Value; merged.AddExtents(bounds); total = merged; }
                    else total = bounds;
                }
            }
            if (!total.HasValue) return null;
            var b = total.Value;
            foreach (double n in new[] { b.MinPoint.X, b.MinPoint.Y, b.MaxPoint.X, b.MaxPoint.Y })
                if (double.IsNaN(n) || double.IsInfinity(n)) return null;
            return new ResultBuffer(new TypedValue((int)LispDataType.Double, b.MinPoint.X),
                new TypedValue((int)LispDataType.Double, b.MinPoint.Y),
                new TypedValue((int)LispDataType.Double, b.MaxPoint.X),
                new TypedValue((int)LispDataType.Double, b.MaxPoint.Y));
        }
    }
}
