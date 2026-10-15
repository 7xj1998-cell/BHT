using System;
using System.Collections.Generic;
using System.Linq;

namespace BHT.Core
{
    public sealed class ImportConflict
    {
        public string Kind { get; private set; }
        public string Id { get; private set; }
        public string Reason { get; private set; }
        public string Field { get; private set; }
        public string Current { get; private set; }
        public string Incoming { get; private set; }
        public ImportConflict(string kind, string id, string reason, string field, string current, string incoming)
        { Kind=kind; Id=id; Reason=reason; Field=field; Current=current; Incoming=incoming; }

        public static IEnumerable<ImportConflict> Compare(string kind, string id, string reason, BhtRecord current, BhtRecord incoming, IEnumerable<string> fields)
        {
            foreach (string field in fields.Distinct())
                if (!SurveyComparison.FieldEqual(current,incoming,field))
                    yield return new ImportConflict(kind,id,reason,field,Values(current,field),Values(incoming,field));
        }
        static string Values(BhtRecord record,string field)
        {
            var values=record.GetAll(field).ToArray();
            if(field=="sign_content")values=values.Select(SurveyComparison.DescribeSignContent).ToArray();
            return values.Length==0 ? "(không có)" : string.Join("\r\n",values.Select((s,i)=>(values.Length>1 ? (i+1)+". " : "")+(s=="" ? "(rỗng)" : s)));
        }
        public override string ToString() { return Kind+" / "+Id+" | "+Reason+" | "+Field+" | Bản vẽ: "+Current+" | Tệp nạp: "+Incoming; }
    }

    public sealed class ImportConflictException : InvalidOperationException
    {
        public IList<ImportConflict> Conflicts { get; private set; }
        public ImportConflictException(IList<ImportConflict> conflicts)
            : base("Chưa nạp dữ liệu: "+conflicts.Select(c=>c.Kind+"/"+c.Id).Distinct().Count()+" mục xung đột, "+conflicts.Count+" nội dung cần đối chiếu. Bản vẽ đích được giữ nguyên.")
        { Conflicts=new List<ImportConflict>(conflicts).AsReadOnly(); }
    }
}
