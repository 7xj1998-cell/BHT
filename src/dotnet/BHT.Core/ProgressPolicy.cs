using System;
using System.Linq;
using System.Collections.Generic;
namespace BHT.Core
{
    public static class ProgressPolicy
    {
        public static bool Locked(BhtRecord r) { return r != null && r.Get("progress_locked") == "1"; }
        public static readonly string[] ProtectedFields = { "nhom", "ma_hieu", "mat", "pt", "so_tru", "so_mat", "sign_content", "marker_km", "marker_h", "phia_duong", "route_id", "ly_trinh_m", "ly_trinh_km", "offset_m", "phia_tuyen", "loai_tuyen", "station_route_revision", "trang_thai_km", "station_status", "station_source", "doan", "goi", "station_point_id", "station_point_method", "station_point_e", "station_point_n", "station_projection_e", "station_projection_n" };
        public static bool Changed(BhtRecord a, BhtRecord b) { return ProtectedFields.Any(k => !a.GetAll(k).SequenceEqual(b.GetAll(k))); }
        public static readonly string[] ResultFields = ProtectedFields.Except(new[]{"nhom","ma_hieu","mat","pt","so_tru","so_mat","marker_km","marker_h","sign_content","phia_duong","route_id"}).ToArray();
        public static string ResultKey(BhtRecord r) { var result=new BhtRecord();foreach(string k in ResultFields)result.SetAll(k,r.GetAll(k));return result.ToCanonical(); }
        public static readonly string[] SurveyFields = {"nhom","ma_hieu","mat","pt","so_tru","so_mat","marker_km","marker_h","sign_content","phia_duong","anh"};
        public static bool SameSurvey(BhtRecord a,BhtRecord b) { return SurveyFields.All(k=>SurveyComparison.FieldEqual(a,b,k)); }
        public static string RouteState(BhtRecord route)
        {
            var state=new BhtRecord();
            foreach(string key in new[]{"loai","start_dist","direction","chieu","max_offset","ngoai_suy_m","route_revision","station_control_mode","station_source"})state.Add(key,route.Get(key));
            state.SetAll("moc",route.GetAll("moc"));return string.Join("\n",RecordCodec.Encode(state));
        }
        public static bool SameRoute(BhtRecord a,BhtRecord b)
        {
            if(a==null||b==null)return false;
            var sa=a.Get("geometry_signature").Split('|');var sb=b.Get("geometry_signature").Split('|');
            return sa.Length==14&&sb.Length==14&&sa.Skip(1).SequenceEqual(sb.Skip(1))&&
                new[]{"loai","start_dist","direction","chieu","max_offset","ngoai_suy_m","route_revision","moc","station_control_mode","station_source"}.All(k=>SurveyComparison.FieldEqual(a,b,k));
        }
        public static IEnumerable<ImportConflict> RollingConflicts(string id,BhtRecord current,BhtRecord incoming)
        {
            return ImportConflict.Compare("Hồ sơ",id,"Khác nội dung khảo sát",current,incoming,SurveyFields)
                .Concat(Locked(current)&&Locked(incoming)
                    ? ImportConflict.Compare("Hồ sơ",id,"Khác kết quả đã xác nhận",current,incoming,ResultFields)
                    : Enumerable.Empty<ImportConflict>());
        }
    }
}
