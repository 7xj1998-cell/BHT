using System;
using System.Linq;
using Autodesk.AutoCAD.Runtime;
using BHT.Bridge;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

public static class SignDisplay127Probe
{
    [CommandMethod("BHTSIGNDISPLAY127")]
    public static void Run()
    {
        var doc = AcApp.DocumentManager.MdiActiveDocument;
        int failures = 0, checks = 0;
        Action<string, bool> check = (name, ok) => {
            checks++; if (!ok) failures++;
            doc.Editor.WriteMessage("\n" + (ok ? "PASS " : "FAIL ") + "DISPLAY127 " + name);
        };
        using (var service = new BhtDataService(doc.Database, () => "", false))
        {
            var before = service.GetObjects().ToDictionary(p => p.Key, p => p.Value.Pairs.ToArray());
            var api = new LispApi();
            foreach (string mode in new[] { "PARALLEL", "PERPENDICULAR" })
            {
                // Same Application.Invoke boundary used by the palette's Apply action.
                var reply = api.Call("bht:api-sign-display", "2", "0.75", mode);
                check("invoke-" + mode, reply.Ok);
                if (!reply.Ok) doc.Editor.WriteMessage("\nDISPLAY127-ERROR " + reply.Error);
                check("saved-" + mode, service.Meta("sign_label_orientation", "") == mode
                    && service.Meta("sign_scale", "") == "2.000" && service.Meta("sign_label_scale", "") == "0.750");
            }
            var invalid = api.Call("bht:api-sign-display", "2", "0.75", "BAD");
            check("invalid-mode-rejected-by-lisp", !invalid.Ok && invalid.Error.Contains("Hướng nhãn"));
            check("rejection-keeps-settings", service.Meta("sign_label_orientation", "") == "PERPENDICULAR");
            check("legacy-api-still-invokable", api.Call("bht:api-sign-scale", "1", "1").Ok);
            check("legacy-keeps-orientation", service.Meta("sign_label_orientation", "") == "PERPENDICULAR");
            var after = service.GetObjects();
            check("survey-records-unchanged", before.Count > 0 && after.Count == before.Count
                && before.All(p => after.ContainsKey(p.Key) && p.Value.SequenceEqual(after[p.Key].Pairs)));
        }
        doc.Editor.WriteMessage("\nDISPLAY127-CHECKS=" + checks + " FAIL=" + failures);
    }
}
