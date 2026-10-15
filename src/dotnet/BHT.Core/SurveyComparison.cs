using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace BHT.Core
{
    public static class SurveyComparison
    {
        public static bool FieldEqual(BhtRecord current,BhtRecord incoming,string field)
        {
            var a=current.GetAll(field);var b=incoming.GetAll(field);
            if(field!="sign_content")return a.SequenceEqual(b);
            return a.Count==b.Count&&a.Zip(b,SameSignContent).All(equal=>equal);
        }

        public static bool SameSignContent(string a,string b)
        {
            if(string.Equals(a,b,StringComparison.Ordinal))return true;
            string ca,cb;
            return TryCanonical(a,out ca)&&TryCanonical(b,out cb)&&ca==cb;
        }

        public static string DescribeSignContent(string value)
        {
            string canonical;if(!TryCanonical(value,out canonical))return value;
            return string.Join("\r\n",SignContent.Read(canonical).SelectMany(face=>
                new[]{"Mặt "+(face.Index+1)+": "+face.Code}.Concat(face.Values.Select(p=>
                    "  "+(p.Key.StartsWith("LOCATION#",StringComparison.Ordinal)?"Địa danh":p.Key.StartsWith("STATION#",StringComparison.Ordinal)?"Lý trình trên biển":p.Key)+": "+(p.Value==""?"(rỗng)":p.Value)))));
        }

        // Compare the supported content model, never the XML writer's formatting.
        // Unknown schema/data stays an explicit difference rather than being dropped.
        static bool TryCanonical(string value,out string canonical)
        {
            canonical=null;
            if(string.IsNullOrWhiteSpace(value))return false;
            try {
                XElement root;
                using(var reader=XmlReader.Create(new StringReader(value),new XmlReaderSettings {
                    DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=200000
                }))root=XElement.Load(reader);
                if(root.Name!="sign-content"||root.Attributes().Count()!=1||(string)root.Attribute("version")!="1"||!OnlyElements(root,"face"))return false;
                foreach(var face in root.Elements()) {
                    if(face.Attributes().Count()!=2||face.Attribute("index")==null||face.Attribute("code")==null||!OnlyElements(face,"field"))return false;
                    foreach(var field in face.Elements())
                        if(field.Attributes().Count()!=1||field.Attribute("key")==null||field.Nodes().Any(n=>!(n is XText)))return false;
                }
                canonical=SignContent.Write(SignContent.Read(value));
                return true;
            }catch(XmlException){return false;}
            catch(ArgumentException){return false;}
        }

        static bool OnlyElements(XElement element,string childName)
        {
            return element.Nodes().All(n=>n is XElement ? ((XElement)n).Name==childName : n is XText&&string.IsNullOrWhiteSpace(((XText)n).Value));
        }
    }
}
