using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
namespace BHT.Palette {
 public sealed class CustomBoardTemplate {
  public string Code, Name, Text;
 }
 public static class CustomBoardLibrary {
  public const string Prefix="BHT.TEXT.";
  public static string FilePath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"BHT","custom-boards.xml"); } }
  public static List<CustomBoardTemplate> Read(string path) {
   var result=new List<CustomBoardTemplate>(); if(!File.Exists(path)) return result;
   var doc=new XmlDocument { XmlResolver=null };
   using(var reader=XmlReader.Create(path,new XmlReaderSettings { DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=4000000 })) doc.Load(reader);
   if(doc.DocumentElement==null || doc.DocumentElement.Name!="boards")throw new InvalidDataException("Tệp thư viện biển tùy chỉnh không hợp lệ.");
   foreach(XmlElement e in doc.DocumentElement.SelectNodes("board")){
    var item=new CustomBoardTemplate {Code=e.GetAttribute("code"),Name=e.GetAttribute("name"),Text=e.InnerText};
    Validate(item);
    if(result.Any(x=>x.Code==item.Code))throw new InvalidDataException("Trùng mã biển tùy chỉnh.");
    result.Add(item);
   }
   return result;
  }
  public static void Validate(CustomBoardTemplate item) {
   if(item==null || string.IsNullOrWhiteSpace(item.Code) || !item.Code.StartsWith(Prefix,StringComparison.Ordinal) || item.Code.Length>80
    || string.IsNullOrWhiteSpace(item.Name) || item.Name.Length>80 || string.IsNullOrWhiteSpace(item.Text) || item.Text.Length>2000
    || item.Text.Replace("\r\n","\n").Split('\n').Length>20)throw new InvalidDataException("Nhập tên biển (tối đa 80 ký tự) và nội dung (tối đa 2.000 ký tự / 20 dòng).");
  }
  public static void Save(string path,CustomBoardTemplate item) {
   Validate(item);
   // Serialize reload+replace so two library windows cannot drop one another's templates.
   using(var mutex=new System.Threading.Mutex(false,"Local\\BHTCustomBoardLibrary")){
    bool held=false;
    try {
     try {held=mutex.WaitOne(5000);}catch(System.Threading.AbandonedMutexException){held=true;}
     if(!held)throw new IOException("Thư viện đang được cập nhật. Hãy thử lại.");
     var items=Read(path);items.RemoveAll(x=>x.Code==item.Code);items.Add(item);
     Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
     try {
      using(var writer=XmlWriter.Create(temp,new XmlWriterSettings {Indent=true,NewLineHandling=NewLineHandling.Entitize,Encoding=new System.Text.UTF8Encoding(false)})){
       writer.WriteStartElement("boards");
       foreach(var value in items){writer.WriteStartElement("board");writer.WriteAttributeString("code",value.Code);writer.WriteAttributeString("name",value.Name);writer.WriteString(value.Text);writer.WriteEndElement();}
       writer.WriteEndElement();
      }
      if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
     } finally {if(File.Exists(temp))File.Delete(temp);}
    } finally {if(held)mutex.ReleaseMutex();}
   }
  }
 }
}
