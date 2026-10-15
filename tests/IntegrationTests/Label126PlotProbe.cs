using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.PlottingServices;
using BHT.Bridge;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
public class Label126PlotProbe
{
    [CommandMethod("BHTPLOT126")]
    public static void Plot()
    {
        var doc = AcApp.DocumentManager.MdiActiveDocument;
        using (var tr = doc.Database.TransactionManager.StartTransaction())
        {
            var manager = LayoutManager.Current;
            var layout = (Layout)tr.GetObject(manager.GetLayoutId(manager.CurrentLayout), OpenMode.ForRead);
            using (var settings = new PlotSettings(layout.ModelType))
            {
                settings.CopyFrom(layout);
                var validator = PlotSettingsValidator.Current;
                validator.SetPlotConfigurationName(settings, "DWG To PDF.pc3", "ISO_full_bleed_A4_(297.00_x_210.00_MM)");
                validator.RefreshLists(settings);
                                var bounds = new Extents3d(); bool found = false;
                var bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                string oid = Environment.GetEnvironmentVariable("BHT_LABEL126_OID") ?? "OBJ-000004";
                foreach (ObjectId id in ms) {
                    var entity = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    if (entity == null) continue;
                    using (var data = entity.GetXDataForApplication("BHT_KH")) {
                        if (data == null) continue;
                        var vals = data.AsArray();
                        if (vals.Length < 2 || !string.Equals(vals[1].Value as string, oid, StringComparison.OrdinalIgnoreCase)) continue;
                        try { bounds.AddExtents(entity.GeometricExtents); found = true; } catch { }
                    }
                }
                if (!found) throw new InvalidOperationException("No symbol for " + oid);
                double pad = Math.Max(bounds.MaxPoint.X-bounds.MinPoint.X,bounds.MaxPoint.Y-bounds.MinPoint.Y)*0.12;
                validator.SetPlotWindowArea(settings, new Extents2d(bounds.MinPoint.X-pad,bounds.MinPoint.Y-pad,bounds.MaxPoint.X+pad,bounds.MaxPoint.Y+pad));
                validator.SetPlotType(settings, Autodesk.AutoCAD.DatabaseServices.PlotType.Window);
                validator.SetUseStandardScale(settings, true); validator.SetStdScaleType(settings, StdScaleType.ScaleToFit);
                validator.SetPlotCentered(settings, true); settings.PlotPlotStyles = false;
                var info = new PlotInfo { Layout = layout.ObjectId, OverrideSettings = settings };
                var infoValidator = new PlotInfoValidator { MediaMatchingPolicy = MatchingPolicy.MatchEnabled }; infoValidator.Validate(info);
                using (var engine = PlotFactory.CreatePublishEngine())
                {
                    string pdf = Environment.GetEnvironmentVariable("BHT_QA_PDF") ?? Path.Combine(Path.GetTempPath(), "BHT-sign0610.pdf");
                    engine.BeginPlot(null, null); engine.BeginDocument(info, doc.Name, null, 1, true, pdf);
                    var page = new PlotPageInfo(); engine.BeginPage(page, info, true, null); engine.BeginGenerateGraphics(null); engine.EndGenerateGraphics(null); engine.EndPage(null); engine.EndDocument(null); engine.EndPlot(null);
                }
            }
            tr.Commit();
        }
    }
}
