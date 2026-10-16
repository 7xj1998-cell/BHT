using System;using System.IO;using System.Linq;using BHT.Palette;
class CustomBoardStoreTests {
 static int checks;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);checks++;Console.WriteLine("PASS "+name);}
 static void Main(string[] args){
  string path=Path.Combine(args[0],"custom-test.xml");Directory.CreateDirectory(args[0]);
  var a=new CustomBoardTemplate {Code="BHT.TEXT.TEST_A",Name="Biển A",Text="DỪNG LẠI\r\nQUAN SÁT"};
  CustomBoardLibrary.Save(path,a);Check(CustomBoardLibrary.Read(path).Single().Text==a.Text,"Unicode and line breaks persist");
  var b=new CustomBoardTemplate {Code="BHT.TEXT.TEST_B",Name="Biển B",Text="ĐƯỜNG HẸP"};CustomBoardLibrary.Save(path,b);
  a.Text="NỘI DUNG MỚI";CustomBoardLibrary.Save(path,a);
  Check(CustomBoardLibrary.Read(path).Count==2 && CustomBoardLibrary.Read(path).Single(x=>x.Code==b.Code).Text==b.Text,"Edit retains other templates");
  var before=File.ReadAllText(path);a.Text=new string('x',2001);
  try{CustomBoardLibrary.Save(path,a);throw new Exception("accepted invalid");}catch(InvalidDataException){}
  Check(before==File.ReadAllText(path),"Invalid edit leaves library intact");
  a.Text=string.Join("\n",Enumerable.Repeat("line",21));
  try{CustomBoardLibrary.Save(path,a);throw new Exception("accepted 21 lines");}catch(InvalidDataException){}
  Check(before==File.ReadAllText(path),"Line limit enforced");
  File.WriteAllText(path,"<broken>");a.Text="valid";
  try{CustomBoardLibrary.Save(path,a);throw new Exception("overwrote broken library");}catch(System.Xml.XmlException){}
  Check(File.ReadAllText(path)=="<broken>","Corrupt library is not overwritten");
  Console.WriteLine("STORE PASS "+checks);
 }
}
