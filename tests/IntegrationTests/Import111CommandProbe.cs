using System;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class Import111CommandProbe
{
    static string before;static int conflicts;
    static void Seen(System.Collections.Generic.IList<ImportConflict> items){conflicts=items.Count;}
    [CommandMethod("IMPORT111ARM")] public static void Arm(){
        before=DrawingProject.Fingerprint(AcApp.DocumentManager.MdiActiveDocument.Database);
        ProjectTransfer.ImportConflictsDetected+=Seen;
    }
    [CommandMethod("IMPORT111VERIFY")] public static void Verify(){
        var doc=AcApp.DocumentManager.MdiActiveDocument;
        bool ok=conflicts==6&&before==DrawingProject.Fingerprint(doc.Database);
        ProjectTransfer.ImportConflictsDetected-=Seen;
        doc.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" IMPORT111 public-command-with-runtime-conflict-is-atomic");
        string log=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BHT","Logs","rolling-import-last.log");
        bool logged=File.Exists(log)&&File.ReadAllText(log).Contains("OBJ-000001")&&File.ReadAllText(log).Contains("ĐÃ CHẶN");
        doc.Editor.WriteMessage("\n"+(logged?"PASS":"FAIL")+" IMPORT111 public-command-diagnostic-log");
        if(File.Exists(log))File.Copy(log,Path.Combine(Path.GetDirectoryName(typeof(Import111CommandProbe).Assembly.Location),"rolling-import-last.log"),true);
        doc.Editor.WriteMessage("\nIMPORT111-TOTAL=2 FAIL="+((ok?0:1)+(logged?0:1)));
    }
}
