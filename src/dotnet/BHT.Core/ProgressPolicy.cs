using System;
using System.Linq;
namespace BHT.Core
{
    public static class ProgressPolicy
    {
        public static bool Locked(BhtRecord r) { return r != null && r.Get("progress_locked") == "1"; }
        public static readonly string[] ProtectedFields = { "nhom", "ma_hieu", "mat", "pt", "so_tru", "marker_km", "marker_h", "phia_duong", "route_id", "ly_trinh_m", "ly_trinh_km", "offset_m", "phia_tuyen", "loai_tuyen", "station_route_revision", "trang_thai_km", "station_status", "station_source", "doan", "goi", "station_point_id", "station_point_e", "station_point_n", "station_projection_e", "station_projection_n" };
        public static bool Changed(BhtRecord a, BhtRecord b) { return ProtectedFields.Any(k => !a.GetAll(k).SequenceEqual(b.GetAll(k))); }
        public static string ResultKey(BhtRecord r) { return string.Join("\n", ProtectedFields.Where(k => k != "route_id").Select(k => k + "=" + string.Join("|",r.GetAll(k)))); }
        public static readonly string[] SurveyFields = {"nhom","ma_hieu","mat","pt","so_tru","so_mat","marker_km","marker_h","sign_content","phia_duong","anh"};
        public static bool SameSurvey(BhtRecord a,BhtRecord b) { return SurveyFields.All(k=>SurveyComparison.FieldEqual(a,b,k)); }
    }
}
