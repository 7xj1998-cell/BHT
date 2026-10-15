using System;using System.IO;using System.IO.Compression;using System.Linq;using System.Text;using BHT.Core;
namespace BHT.CoreTests {
 public static class ProjectPackageTests {
  static void Rewrite(string source,string target,Func<string,string> changeManifest,bool corrupt,bool extra){
   using(var input=File.OpenRead(source))using(var from=new ZipArchive(input,ZipArchiveMode.Read))
   using(var output=File.Create(target))using(var to=new ZipArchive(output,ZipArchiveMode.Create)){
    foreach(var entry in from.Entries)using(var src=entry.Open())using(var dst=to.CreateEntry(entry.FullName).Open()){
     if(entry.FullName=="manifest.xml")using(var reader=new StreamReader(src))using(var writer=new StreamWriter(dst))writer.Write(changeManifest(reader.ReadToEnd()));
     else if(corrupt){var bytes=new byte[entry.Length];int n=src.Read(bytes,0,bytes.Length);bytes[0]^=1;dst.Write(bytes,0,n);}else src.CopyTo(dst);
    }
    if(extra)to.CreateEntry("../escape.dwg");
   }
  }
  public static void Run(Action<string,string,bool> check){
   string dir=Path.Combine(Path.GetTempPath(),"BHT-format-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
   try{
    string drawing=Path.Combine(dir,"drawing.dwg"),package=Path.Combine(dir,"Du an & cot moc.bht");
    byte[] bytes=Encoding.UTF8.GetBytes("geometry test payload");File.WriteAllBytes(drawing,bytes);ProjectPackage.Write(package,drawing);
    string extracted;
    using(var read=ProjectPackage.Open(package)){extracted=read.DrawingPath;check("PKG01","roundtrip exact bytes",File.ReadAllBytes(extracted).SequenceEqual(bytes));}
    check("PKG02","temporary payload removed",!File.Exists(extracted));
    bool rejected=false;try{ProjectPackage.Write(package,drawing);}catch(IOException){rejected=true;}
    check("PKG03","existing export preserved",rejected);
    foreach(string kind in new[]{"hash","version","size","extra","dtd","truncated"}){
     string invalid=Path.Combine(dir,kind+".bht");
     if(kind=="truncated")File.WriteAllBytes(invalid,File.ReadAllBytes(package).Take(20).ToArray());
     else Rewrite(package,invalid,s=>kind=="version"?s.Replace("format=\"1\"","format=\"999\""):kind=="size"?s.Replace("bytes=\""+bytes.Length+"\"","bytes=\"536870913\""):kind=="dtd"?"<!DOCTYPE BHTProject [<!ENTITY x SYSTEM 'file:///not-read'>]><BHTProject format=\"1\">&x;</BHTProject>":s,kind=="hash",kind=="extra");
     rejected=false;try{using(ProjectPackage.Open(invalid)){} }catch(InvalidDataException){rejected=true;}catch(System.Xml.XmlException){rejected=true;}
     check("PKG-"+kind,"invalid package rejected",rejected);
    }
   }finally{Directory.Delete(dir,true);}
  }
 }
}
