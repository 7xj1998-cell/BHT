using System;
using System.Globalization;
namespace BHT.Core
{
    public static class MarkerStation
    {
        public static bool ManualControlApproved(BhtRecord rec, System.Collections.Generic.IList<SurveyPoint> points)
        {
            if(rec.Get("field_manual_approved")!="1" || rec.Get("nhom")!=rec.Get("field_manual_group") || rec.Get("marker_km")!=rec.Get("field_manual_km") || rec.Get("marker_h")!=rec.Get("field_manual_h"))return false;
            var ids=rec.GetAll("pt");if(ids.Count==0||ids.Count!=points.Count||string.Join(";",ids.ToArray())!=rec.Get("field_manual_points"))return false;
            double x,y;if(!double.TryParse(rec.Get("field_manual_x"),NumberStyles.Float,CultureInfo.InvariantCulture,out x)||!double.TryParse(rec.Get("field_manual_y"),NumberStyles.Float,CultureInfo.InvariantCulture,out y))return false;
            double sx=0,sy=0;foreach(var p in points){sx+=p.X;sy+=p.Y;}return Math.Abs(x-sx/points.Count)<=1e-7 && Math.Abs(y-sy/points.Count)<=1e-7;
        }
        public const string Source = "Số Km/H đã nhập";
        public static bool TryMetres(string group, string km, string h, out double metres)
        {
            metres = 0; int k, n = 0;
            if (group != "COC_TIEU" && group != "COT_KM") return false;
            if (!int.TryParse(km, NumberStyles.None, CultureInfo.InvariantCulture, out k) || k < 0 || k > 99999) return false;
            if (group == "COC_TIEU" && (!int.TryParse(h, NumberStyles.None, CultureInfo.InvariantCulture, out n) || n < 0 || n > 9)) return false;
            metres = k * 1000.0 + n * 100.0; return true;
        }
        public static void Apply(BhtRecord record)
        {
            double metres;
            if (TryMetres(record.Get(ObjFields.Group), record.Get(ObjFields.MarkerKm), record.Get(ObjFields.MarkerH), out metres))
            {
                bool changed = record.Get(ObjFields.ChainageKm) != Chainage.Format(metres);
                record.Set(ObjFields.ChainageM, LispFormat.Fnum(metres, 3)).Set(ObjFields.ChainageKm, Chainage.Format(metres))
                      .Set(ObjFields.KmState, "NHAP_TAY").Set(ObjFields.KmSource, Source).Set(ObjFields.StationStatus, "MANUAL").Set(ObjFields.StationRouteRevision, "");
                if (changed) ClearSegment(record);
            }
            else if (record.Get(ObjFields.KmSource) == Source)
            {
                record.Set(ObjFields.ChainageM, "").Set(ObjFields.ChainageKm, "").Set(ObjFields.KmState, "CHUA_TINH").Set(ObjFields.KmSource, "")
                      .Set(ObjFields.StationStatus, "NO_ROUTE").Set(ObjFields.StationRouteRevision, "");
                ClearSegment(record);
            }
        }
        private static void ClearSegment(BhtRecord record)
        {
            record.Set(ObjFields.Segment, "").Set(ObjFields.Package, "").Set(ObjFields.SegMethod, "CHUA_PHAN_DOAN").Set(ObjFields.SegCandidates, "");
        }
    }
}
