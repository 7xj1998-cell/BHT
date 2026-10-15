using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using BHT.Bridge;
public class Import128Audit
{
    [CommandMethod("BHTIMPORT128AUDIT")]
    public static void Run()
    {
        string dir=Environment.GetEnvironmentVariable("BHT_IMPORT_AUDIT_DIR");
        var paths=File.ReadAllLines(Path.Combine(dir,"inputs.txt"));
        var report=new XElement("Audit");
        try {
            for(int i=0;i<paths.Length;i++)using(var package=ProjectPackage.Open(paths[i]))using(var db=new Database(false,true)) {
                db.ReadDwgFile(package.DrawingPath,FileOpenMode.OpenForReadAndAllShare,true,"");db.CloseInput(true);
                var item=new XElement("File",new XAttribute("index",i),new XAttribute("path",paths[i]));report.Add(item);
                using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
                    foreach(string kind in new[]{"OBJ","SEG","ROUTE"})
                        item.Add(new XElement(kind,BhtStore.All(tr,db,kind).Select(r=>new XElement("Record",new XAttribute("id",r.Key),r.Value.Pairs.Select(p=>new XElement("Field",new XAttribute("name",p.Key),p.Value))))));
                    item.Add(new XElement("Points",new XAttribute("count",BhtStore.Points(tr,db).Count)));
                }
                for(int j=0;j<paths.Length;j++)if(i!=j) {
                    // Each pair gets a fresh in-memory destination; user files are never saved.
                    using(var target=new Database(false,true)) {
                        target.ReadDwgFile(package.DrawingPath,FileOpenMode.OpenForReadAndAllShare,true,"");target.CloseInput(true);
                        var attempt=new XElement("Import",new XAttribute("source",j));item.Add(attempt);
                        try { attempt.Add(new XAttribute("created",ProjectTransfer.ImportRolling(target,paths[j],true))); }
                        catch(ImportConflictException ex) { attempt.Add(new XAttribute("status","CONFLICT"),ex.Conflicts.Select(c=>new XElement("Difference",new XAttribute("kind",c.Kind),new XAttribute("id",c.Id),new XAttribute("field",c.Field),new XAttribute("reason",c.Reason),new XElement("Current",c.Current),new XElement("Incoming",c.Incoming)))); }
                        catch(System.Exception ex) { attempt.Add(new XAttribute("status","ERROR"),new XElement("Error",ex.ToString())); }
                    }
                }
            }
        }catch(System.Exception ex){report.Add(new XElement("Error",ex.ToString()));}
        report.Save(Path.Combine(dir,"comparison.xml"));
    }
}
