using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.Core;
using BHT.Core;
namespace BHT.Bridge
{
    public static partial class ProjectTransfer
    {
        static void ValidateConfirmedPoints(Database db,string segment)
        {
            using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
                var points=BhtStore.Points(tr,db).ToLookup(p=>p.IdUpper);
                foreach(var row in BhtStore.All(tr,db,"OBJ").Where(p=>ProgressPolicy.Locked(p.Value)&&(segment==null||p.Value.Get("doan")==segment))){
                var route=BhtStore.Read(tr,db,"ROUTE",row.Value.Get("route_id"));
                var routeEntity=route==null?ObjectId.Null:HandleId(db,route.Get("handle"));
                var curve=routeEntity.IsNull?null:tr.GetObject(routeEntity,OpenMode.ForRead) as Curve;
                string snapshot=row.Value.Get("progress_route_signature");var parts=snapshot.Split('|');
                if(curve==null||parts.Length!=14||(!string.IsNullOrEmpty(row.Value.Get("station_route_revision"))&&row.Value.Get("station_route_revision")!=route.Get("route_revision")))throw new InvalidOperationException("Tim của "+row.Key+" thiếu hoặc đã đổi sau xác nhận. Mở lại đoạn và tính lại.");
                if(string.Join("|",parts.Skip(1))!=string.Join("|",route.Get("geometry_signature").Split('|').Skip(1)))throw new InvalidOperationException("Thông số tim đã đổi sau xác nhận: "+row.Key+". Mở lại đoạn và tính lại.");
                double length=curve.GetDistanceAtParameter(curve.EndParam),expected;
                if(!double.TryParse(parts[1],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out expected)||Math.Abs(length-expected)>1e-5||curve.Closed!=(parts[2]=="C"))throw new InvalidOperationException("Hình học tim đã đổi sau xác nhận: "+row.Key);
                for(int i=0;i<9;i++){
                    var sample=parts[i+5].Split(',');double x,y;var actual=curve.GetPointAtDist(length*i/(curve.Closed?9.0:8.0));
                    if(sample.Length!=2||!double.TryParse(sample[0],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out x)||!double.TryParse(sample[1],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out y)||Math.Abs(actual.X-x)>1e-5||Math.Abs(actual.Y-y)>1e-5)throw new InvalidOperationException("Hình học tim đã đổi sau xác nhận: "+row.Key);
                }
                var snapshots=row.Value.GetAll("progress_point").ToArray();
                var expectedPoints=row.Value.GetAll("pt").Select(p=>p.ToUpperInvariant()).OrderBy(p=>p).ToArray();
                if(expectedPoints.Length==0||!expectedPoints.SequenceEqual(snapshots.Select(p=>p.Split('|')[0].ToUpperInvariant()).OrderBy(p=>p)))throw new InvalidOperationException("Thiếu dữ liệu xác nhận điểm của "+row.Key+". Mở lại đoạn và xác nhận lại.");
                foreach(string saved in snapshots){
                    var f=saved.Split('|');double x,y,z;
                    if(f.Length!=4||!double.TryParse(f[1],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out x)||!double.TryParse(f[2],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out y)||!double.TryParse(f[3],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out z))throw new InvalidOperationException("Dữ liệu xác nhận điểm không hợp lệ: "+row.Key);
                    var p=points[f[0].ToUpperInvariant()].ToArray();
                    if(p.Length!=1||Math.Abs(p[0].X-x)>1e-6||Math.Abs(p[0].Y-y)>1e-6||Math.Abs(p[0].Z-z)>1e-6)throw new InvalidOperationException("Điểm của "+row.Key+" đã thay đổi sau xác nhận. Mở lại đoạn và tính lại trước khi xuất.");
                }
                }
            }
        }
        static string TagId(Entity e,string app) { var x=BhtStore.XGet(e,app); return x!=null && x.Count>0 ? x[0].ToUpperInvariant() : ""; }
        static bool Owned(Entity e,HashSet<string> objects,HashSet<string> points,HashSet<string> photos)
        {
            return new[]{"BHT_KH","BHT_SIGN_ORIGIN","BHT_OFFSET_DIM"}.Any(a=>objects.Contains(TagId(e,a)))
                || new[]{"BHT_PT","BHT_NHAN"}.Any(a=>points.Contains(TagId(e,a)))
                || new[]{"BHT_ANHPT","BHT_ANHTEN","BHT_ANHRS"}.Any(a=>photos.Contains(TagId(e,a)));
        }
        static HashSet<ObjectId> SelectSegment(Database db,Transaction tr,ref List<Row> rows,string segment)
        {
            var selected=rows.Where(r=>r.Sub=="OBJ" && r.Record.Get("doan")==segment).ToList();
            if(selected.Count==0)throw new InvalidOperationException("Đoạn chưa có hồ sơ để xuất.");
            if(selected.Any(r=>!ProgressPolicy.Locked(r.Record)))throw new InvalidOperationException("Xác nhận các hồ sơ của đoạn trước khi xuất bàn giao.");
            var objects=new HashSet<string>(selected.Select(r=>r.Key));
            var points=new HashSet<string>(selected.SelectMany(r=>r.Record.GetAll("pt")).Select(s=>s.ToUpperInvariant()));
            var photos=new HashSet<string>(selected.SelectMany(r=>r.Record.GetAll("anh")).Select(s=>s.Split('|')[0].ToUpperInvariant()));
            var routes=new HashSet<string>(selected.SelectMany(r=>new[]{r.Record.Get("route_id"),r.Record.Get("sign_heading_route")}).Where(s=>s!=""));
            var seg=rows.FirstOrDefault(r=>r.Sub=="SEG"&&r.Key==segment);if(seg!=null&&seg.Record.Get("route_id")!="")routes.Add(seg.Record.Get("route_id"));
            rows=rows.Where(r=>r.Sub=="META" || r.Sub=="OBJ"&&objects.Contains(r.Key) || r.Sub=="SEG"&&r.Key==segment || r.Sub=="ROUTE"&&routes.Contains(r.Key) || r.Sub=="PHOTO"&&photos.Contains(r.Key)).ToList();
            var ids=Entities(db,tr,rows);
            var geometry=new HashSet<ObjectId>();
            foreach(var r in rows.Where(r=>r.Sub=="ROUTE"))foreach(string k in new[]{"handle","tdt_source_handle"}){var id=HandleId(db,r.Record.Get(k));if(!id.IsNull)geometry.Add(id);}
            foreach(var r in selected){var id=HandleId(db,r.Record.Get("marker_display_handle"));if(!id.IsNull)geometry.Add(id);}
            ids.RemoveWhere(id=>!geometry.Contains(id)&&!Owned((Entity)tr.GetObject(id,OpenMode.ForRead),objects,points,photos));
            // A segment handoff must not inherit a global road handle to omitted geometry.
            foreach(var r in rows.Where(r=>r.Sub=="META")) r.Record=r.Record.Clone().Set("symbol_road_handle","").Set("station_active_route",seg==null?"":seg.Record.Get("route_id"));
            return ids;
        }
        static Dictionary<string,string> PrepareRolling(Database source,Transaction st,Database target,Transaction tt,ref List<Row> rows,HashSet<ObjectId> ids,List<Row> existing)
        {
            var sourcePoints=BhtStore.Points(st,source);var targetPoints=BhtStore.Points(tt,target);
            CheckRollingConflicts(rows,existing,sourcePoints,targetPoints);
            if(sourcePoints.GroupBy(p=>p.IdUpper).Any(g=>g.Count()>1)||targetPoints.GroupBy(p=>p.IdUpper).Any(g=>g.Count()>1))throw new InvalidOperationException("Trùng mã điểm trong bản vẽ; kiểm tra điểm trước khi nạp dữ liệu kế thừa.");
            var targetIndex=targetPoints.ToDictionary(p=>p.IdUpper);var duplicates=new HashSet<string>();
            foreach(var p in sourcePoints){SurveyPoint q;if(targetIndex.TryGetValue(p.IdUpper,out q)){
                if(Math.Abs(p.X-q.X)>1e-6||Math.Abs(p.Y-q.Y)>1e-6||Math.Abs(p.Z-q.Z)>1e-6)throw new InvalidOperationException("Điểm "+p.Id+" trùng mã nhưng khác tọa độ. Chưa nạp dữ liệu.");
                duplicates.Add(p.IdUpper);
            }}
            var skip=new HashSet<string>();var replace=new HashSet<string>();
            foreach(var row in rows.Where(r=>r.Sub=="OBJ")){
                var old=existing.FirstOrDefault(r=>r.Sub=="OBJ"&&r.Key==row.Key);if(old==null)continue;
                if(!ProgressPolicy.SameSurvey(old.Record,row.Record))throw new InvalidOperationException("Hồ sơ "+row.Key+" trùng ID nhưng khác nội dung khảo sát. Chưa nạp; cần đối chiếu hai bản.");
                if(ProgressPolicy.Locked(old.Record)&&ProgressPolicy.Locked(row.Record)&&ProgressPolicy.ResultKey(old.Record)!=ProgressPolicy.ResultKey(row.Record))throw new InvalidOperationException("Hồ sơ "+row.Key+" có hai kết quả đã xác nhận khác nhau. Giữ nguyên bản đích.");
                if(!ProgressPolicy.Locked(old.Record)&&ProgressPolicy.Locked(row.Record)){
                    replace.Add(row.Key);
                    // Preserve destination annotations when adopting a confirmed station result.
                    foreach(var key in old.Record.Pairs.Select(p=>p.Key).Distinct().ToArray())
                        if(!ProgressPolicy.ProtectedFields.Contains(key)&&!key.StartsWith("station_",StringComparison.Ordinal)&&!key.StartsWith("progress_",StringComparison.Ordinal)&&!key.StartsWith("field_",StringComparison.Ordinal)&&!key.StartsWith("kh_",StringComparison.Ordinal)&&!key.StartsWith("sign_heading_",StringComparison.Ordinal)&&!new[]{"gan_doan_pp","doan_ung_vien","nguon_km","marker_display_handle","custom_block"}.Contains(key))row.Record.SetAll(key,old.Record.GetAll(key));
                }else skip.Add(row.Key);
            }
            var neededRoutes=new HashSet<string>(rows.Where(r=>r.Sub=="OBJ"&&!skip.Contains(r.Key)).SelectMany(r=>new[]{r.Record.Get("route_id"),r.Record.Get("sign_heading_route")}));
            var unusedRoutes=rows.Where(r=>r.Sub=="ROUTE"&&!neededRoutes.Contains(r.Key)).ToArray();
            foreach(var row in unusedRoutes)foreach(string k in new[]{"handle","tdt_source_handle"})ids.Remove(HandleId(source,row.Record.Get(k)));
            rows=rows.Where(r=>!unusedRoutes.Contains(r)).ToList();
            var names=new Dictionary<string,string>();var used=new HashSet<string>(existing.Where(r=>r.Sub=="ROUTE").Select(r=>r.Key));
            foreach(var row in rows.Where(r=>r.Sub=="ROUTE")){string name=row.Key;int n=1;while(used.Contains(name))name=row.Key+"_GOI"+(n++);used.Add(name);names.Add(row.Key,name);}
            var removeRows=new HashSet<Row>();
            foreach(var row in rows){
                if(row.Sub=="META"){
                    var destination=BhtStore.Read(tt,target,"META",row.Key)??new BhtRecord();
                    foreach(var p in row.Record.Pairs)if((p.Key=="thu_muc_anh"||p.Key.StartsWith("anh_",StringComparison.Ordinal))&&destination.Get(p.Key)=="")destination.Set(p.Key,p.Value);
                    row.Record=destination;continue;
                }
                if(row.Sub=="FIELD_CHAIN"||row.Sub=="OBJ"&&skip.Contains(row.Key)){removeRows.Add(row);continue;}
                var old=existing.FirstOrDefault(r=>r.Sub==row.Sub&&r.Key==row.Key);if(old==null||row.Sub=="OBJ"||row.Sub=="ROUTE")continue;
                if(row.Sub=="SEG"){
                    if(new[]{"km_start_m","km_end_m","side","package_id"}.Any(k=>old.Record.Get(k)!=row.Record.Get(k)))throw new InvalidOperationException("Bảng đoạn "+row.Key+" khác phạm vi/phía/gói. Đối chiếu bảng trước khi gộp.");
                    bool importingResult=rows.Any(r=>r.Sub=="OBJ"&&!skip.Contains(r.Key)&&ProgressPolicy.Locked(r.Record)&&r.Record.Get("doan")==row.Key);
                    if(importingResult && old.Record.Get("route_id")!="")throw new InvalidOperationException("Đoạn "+row.Key+" đã gắn tim ở đích. Nạp gói vào bản đoạn kế tiếp chưa gắn tim cho đoạn này.");
                    if(!importingResult)removeRows.Add(row);
                }else{
                    if(old.Record.ToCanonical()!=row.Record.ToCanonical())throw new InvalidOperationException("Dữ liệu "+row.Sub+"/"+row.Key+" xung đột; chưa nạp.");
                    removeRows.Add(row);
                }
            }
            foreach(var row in rows.Where(r=>r.Sub=="SEG"&&!removeRows.Contains(r)))
                if(row.Record.Get("route_id")!=""&&!names.ContainsKey(row.Record.Get("route_id")))row.Record=row.Record.Clone().Set("route_id","");
            var existingPhotos=new HashSet<string>(existing.Where(r=>r.Sub=="PHOTO").Select(r=>r.Key));
            ids.RemoveWhere(id=>{
                var e=(Entity)st.GetObject(id,OpenMode.ForRead);
                return skip.Contains(TagId(e,"BHT_KH"))||skip.Contains(TagId(e,"BHT_SIGN_ORIGIN"))||duplicates.Contains(TagId(e,"BHT_PT"))||duplicates.Contains(TagId(e,"BHT_NHAN"))||new[]{"BHT_ANHPT","BHT_ANHTEN","BHT_ANHRS"}.Any(a=>existingPhotos.Contains(TagId(e,a)))||HasTag(e,OffsetDimension.AppName);
            });
            // Replace only presentation belonging to an upgraded, previously unfinished object.
            var bt=(BlockTable)tt.GetObject(target.BlockTableId,OpenMode.ForRead);var ms=(BlockTableRecord)tt.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead);
            foreach(ObjectId id in ms){var e=tt.GetObject(id,OpenMode.ForRead) as Entity;if(e!=null&&(replace.Contains(TagId(e,"BHT_KH"))||replace.Contains(TagId(e,"BHT_SIGN_ORIGIN")))){e.UpgradeOpen();e.Erase();}}
            rows=rows.Where(r=>!removeRows.Contains(r)).ToList();return names;
        }
        public static int ImportRolling(Database target,string path,bool keepCoordinates=false)
        {
            using(var package=ProjectPackage.Open(path))using(var source=new Database(false,true)){
                source.ReadDwgFile(package.DrawingPath,FileOpenMode.OpenForReadAndAllShare,true,"");source.CloseInput(true);
                ValidateConfirmedPoints(source,null);
                return Copy(source,target,false,keepCoordinates,false,true);
            }
        }
        [CommandMethod("BHTNAPTICHLUY")]
        public static void ImportRollingCommand()
        {
            var doc=Application.DocumentManager.MdiActiveDocument;
            var pick=doc.Editor.GetFileNameForOpen(new Autodesk.AutoCAD.EditorInput.PromptOpenFileOptions("\nNạp dữ liệu tích lũy, giữ lý trình đã tính: "){Filter="BHT (*.bht)|*.bht"});
            if(pick.Status!=Autodesk.AutoCAD.EditorInput.PromptStatus.OK)return;
            try{int n;
                try { n=ImportRolling(doc.Database,pick.StringResult); }
                catch(UnitMismatchException ex){
                    doc.Editor.WriteMessage("\n"+ex.Message+" Chỉ giữ nguyên tọa độ nếu hai bản vẽ cùng hệ tọa độ thực tế.");
                    var options=new Autodesk.AutoCAD.EditorInput.PromptKeywordOptions("\nGiữ nguyên tọa độ [Giunguyen/Huy] <Huy>: ");options.Keywords.Add("Giunguyen");options.Keywords.Add("Huy");options.Keywords.Default="Huy";options.AllowNone=true;
                    var answer=doc.Editor.GetKeywords(options);if(answer.Status!=Autodesk.AutoCAD.EditorInput.PromptStatus.OK||answer.StringResult!="Giunguyen")return;
                    n=ImportRolling(doc.Database,pick.StringResult,true);
                }
                doc.Editor.WriteMessage("\nĐã nạp dữ liệu tích lũy: "+n+" đối tượng CAD mới. Kết quả cũ và nhãn lý trình được giữ. Hồ sơ trùng đã có được bỏ qua; xung đột bị chặn. Mở Quản lý xử lý theo đoạn để chọn đoạn tiếp theo.");}
            catch(ImportConflictException ex){
                doc.Editor.WriteMessage("\nBHT: "+ex.Message);
                var show=ImportConflictsDetected;
                if(show!=null)try{show(ex.Conflicts);return;}catch(System.Exception error){doc.Editor.WriteMessage("\nKhông mở được bảng đối chiếu: "+error.Message);}
                foreach(var conflict in ex.Conflicts)doc.Editor.WriteMessage("\n"+conflict);
            }
            catch(System.Exception ex){doc.Editor.WriteMessage("\nBHT: "+ex.Message);}
        }
    }
}
