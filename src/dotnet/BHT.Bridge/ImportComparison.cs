using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using BHT.Core;

namespace BHT.Bridge
{
    public static partial class ProjectTransfer
    {
        static void CheckRevisionTransfer(List<Row> rows,List<Row> existing,bool surveyOnly,bool rolling)
        {
            if(rolling&&Versioned(rows.Concat(existing))) { RevisionDecisions(rows,existing);return; }
            if(surveyOnly && Versioned(rows.Concat(existing)))
                throw new ImportConflictException(new[]{new ImportConflict("Phiên bản","DỰ ÁN","Hồ sơ có lịch sử hiệu chỉnh. Chỉ nạp đầy đủ vào DWG trống cho tới khi có hợp nhất theo phiên bản.","revision_id","Giữ dữ liệu đích","Chưa nhập")});
        }
        public static event Action<IList<ImportConflict>> ImportConflictsDetected;

        // Inspect the entire merge before changing destination records or presentation.
        static void CheckRollingConflicts(List<Row> rows,List<Row> existing,List<SurveyPoint> sourcePoints,List<SurveyPoint> targetPoints,Dictionary<string,string> reused)
        {
            CheckRevisionTransfer(rows,existing,false,true);
            var conflicts=new List<ImportConflict>();
            var incomingPoints=sourcePoints.ToLookup(p=>p.IdUpper);
            var currentPoints=targetPoints.ToLookup(p=>p.IdUpper);
            foreach(string id in incomingPoints.Select(g=>g.Key).Union(currentPoints.Select(g=>g.Key))) {
                var a=currentPoints[id].ToArray();var b=incomingPoints[id].ToArray();
                if(a.Length>1||b.Length>1) {
                    conflicts.Add(new ImportConflict("Điểm",id,"Trùng mã điểm trong bản vẽ hoặc tệp nạp","Số điểm cùng mã",a.Length.ToString(),b.Length.ToString()));
                    continue;
                }
                if(a.Length==0||b.Length==0)continue;
                double[] av={a[0].X,a[0].Y,a[0].Z},bv={b[0].X,b[0].Y,b[0].Z};
                for(int i=0;i<3;i++)if(Math.Abs(av[i]-bv[i])>1e-6)
                    conflicts.Add(new ImportConflict("Điểm",id,"Cùng mã, khác tọa độ",new[]{"X","Y","Z"}[i],av[i].ToString("R",CultureInfo.InvariantCulture),bv[i].ToString("R",CultureInfo.InvariantCulture)));
            }
            var oldRows=existing.ToDictionary(r=>r.Sub+"/"+r.Key);
            foreach(var set in new[]{existing,rows})foreach(var obj in set.Where(r=>r.Sub=="OBJ")) {
                var seg=set.FirstOrDefault(r=>r.Sub=="SEG"&&r.Key==obj.Record.Get("doan"));
                if(seg!=null&&obj.Record.Get("goi")!=""&&obj.Record.Get("goi")!=seg.Record.Get("package_id"))
                    conflicts.Add(new ImportConflict("Hồ sơ",obj.Key,"Gói trong hồ sơ không khớp bảng đoạn","goi",obj.Record.Get("goi"),seg.Record.Get("package_id")));
            }
            foreach(var row in rows) {
                Row old;if(!oldRows.TryGetValue(row.Sub+"/"+row.Key,out old))continue;
                if(row.Sub=="OBJ") {
                    conflicts.AddRange(ProgressPolicy.RollingConflicts(row.Key,old.Record,row.Record));
                } else if(row.Sub=="SEG") {
                    conflicts.AddRange(ImportConflict.Compare("Đoạn",row.Key,"Khác phạm vi, phía hoặc gói thầu",old.Record,row.Record,PackageAliasPolicy.SegmentFields));
                    bool importingResult=rows.Any(r=>r.Sub=="OBJ"&&ProgressPolicy.Locked(r.Record)&&r.Record.Get("doan")==row.Key&&
                        !existing.Any(e=>e.Sub=="OBJ"&&e.Key==r.Key&&ProgressPolicy.Locked(e.Record)));
                    string bound;
                    if(importingResult&&old.Record.Get("route_id")!=""&&(!reused.TryGetValue(row.Record.Get("route_id"),out bound)||bound!=old.Record.Get("route_id")))conflicts.Add(new ImportConflict("Đoạn",row.Key,"Đích đã gán tim khác cho đoạn nhận kết quả","route_id",old.Record.Get("route_id"),row.Record.Get("route_id")));
                } else if(row.Sub!="META"&&row.Sub!="FIELD_CHAIN"&&row.Sub!="ROUTE"&&old.Record.ToCanonical()!=row.Record.ToCanonical()) {
                    var differences=ImportConflict.Compare(row.Sub,row.Key,"Khác dữ liệu",old.Record,row.Record,old.Record.Pairs.Select(p=>p.Key).Union(row.Record.Pairs.Select(p=>p.Key))).ToArray();
                    conflicts.AddRange(differences);
                    if(differences.Length==0)conflicts.Add(new ImportConflict(row.Sub,row.Key,"Khác thứ tự dữ liệu","Bản ghi",old.Record.ToCanonical(),row.Record.ToCanonical()));
                }
            }
            if(conflicts.Count>0)throw new ImportConflictException(conflicts);
        }
    }
}
