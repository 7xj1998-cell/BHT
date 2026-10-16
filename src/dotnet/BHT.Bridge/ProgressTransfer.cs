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
        static bool TryFiniteNumber(string value,out double number)
        {
            return double.TryParse(value,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out number)&&!double.IsNaN(number)&&!double.IsInfinity(number);
        }
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
                if(row.Value.Get("progress_route_state")!=""&&row.Value.Get("progress_route_state")!=ProgressPolicy.RouteState(route))throw new InvalidOperationException("Mốc hoặc thông số tim đã đổi sau xác nhận: "+row.Key+". Đối chiếu trước khi xuất/nạp.");
                if(string.Join("|",parts.Skip(1))!=string.Join("|",route.Get("geometry_signature").Split('|').Skip(1)))throw new InvalidOperationException("Thông số tim đã đổi sau xác nhận: "+row.Key+". Mở lại đoạn và tính lại.");
                double length=curve.GetDistanceAtParameter(curve.EndParam),expected;
                if(!TryFiniteNumber(parts[1],out expected)||Math.Abs(length-expected)>1e-5||curve.Closed!=(parts[2]=="C"))throw new InvalidOperationException("Hình học tim đã đổi sau xác nhận: "+row.Key);
                for(int i=0;i<9;i++){
                    var sample=parts[i+5].Split(',');double x,y;var actual=curve.GetPointAtDist(length*i/(curve.Closed?9.0:8.0));
                    if(sample.Length!=2||!TryFiniteNumber(sample[0],out x)||!TryFiniteNumber(sample[1],out y)||Math.Abs(actual.X-x)>1e-5||Math.Abs(actual.Y-y)>1e-5)throw new InvalidOperationException("Hình học tim đã đổi sau xác nhận: "+row.Key);
                }
                var snapshots=row.Value.GetAll("progress_point").ToArray();
                var expectedPoints=row.Value.GetAll("pt").Select(p=>p.ToUpperInvariant()).OrderBy(p=>p).ToArray();
                if(expectedPoints.Length==0||!expectedPoints.SequenceEqual(snapshots.Select(p=>p.Split('|')[0].ToUpperInvariant()).OrderBy(p=>p)))throw new InvalidOperationException("Thiếu dữ liệu xác nhận điểm của "+row.Key+". Mở lại đoạn và xác nhận lại.");
                foreach(string saved in snapshots){
                    var f=saved.Split('|');double x,y,z;
                    if(f.Length!=4||!TryFiniteNumber(f[1],out x)||!TryFiniteNumber(f[2],out y)||!TryFiniteNumber(f[3],out z))throw new InvalidOperationException("Dữ liệu xác nhận điểm không hợp lệ: "+row.Key);
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
            rows=rows.Where(r=>r.Sub=="META" || (r.Sub=="OBJ"||r.Sub=="CAD_STATE")&&objects.Contains(r.Key) || (r.Sub=="HISTORY"||r.Sub=="CHANGES")&&objects.Contains(r.Record.Get("object_id")) || r.Sub=="SEG"&&r.Key==segment || r.Sub=="ROUTE"&&routes.Contains(r.Key) || r.Sub=="PHOTO"&&photos.Contains(r.Key)).ToList();
            var ids=Entities(db,tr,rows);
            var geometry=new HashSet<ObjectId>();
            foreach(var r in rows.Where(r=>r.Sub=="ROUTE"))foreach(string k in new[]{"handle","tdt_source_handle"}){var id=HandleId(db,r.Record.Get(k));if(!id.IsNull)geometry.Add(id);}
            foreach(var r in selected){var id=HandleId(db,r.Record.Get("marker_display_handle"));if(!id.IsNull)geometry.Add(id);}
            ids.RemoveWhere(id=>!geometry.Contains(id)&&!Owned((Entity)tr.GetObject(id,OpenMode.ForRead),objects,points,photos));
            // A segment handoff must not inherit a global road handle to omitted geometry.
            foreach(var r in rows.Where(r=>r.Sub=="META")) r.Record=r.Record.Clone().Set("symbol_road_handle","").Set("station_active_route",seg==null?"":seg.Record.Get("route_id"));
            return ids;
        }
        static bool RouteGeometryMatches(Database db,Transaction tr,BhtRecord route)
        {
            var parts=route.Get("geometry_signature").Split('|');var id=HandleId(db,route.Get("handle"));
            var curve=id.IsNull?null:tr.GetObject(id,OpenMode.ForRead) as Curve;
            double expected,length;
            if(curve==null||parts.Length!=14||!TryFiniteNumber(parts[1],out expected)||double.IsNaN(expected)||double.IsInfinity(expected))return false;
            length=curve.GetDistanceAtParameter(curve.EndParam);
            if(Math.Abs(length-expected)>1e-5||curve.Closed!=(parts[2]=="C"))return false;
            for(int i=0;i<9;i++){
                var sample=parts[i+5].Split(',');double x,y;var actual=curve.GetPointAtDist(length*i/(curve.Closed?9.0:8.0));
                if(sample.Length!=2||!TryFiniteNumber(sample[0],out x)||!TryFiniteNumber(sample[1],out y)||double.IsNaN(x)||double.IsInfinity(x)||double.IsNaN(y)||double.IsInfinity(y)||Math.Abs(actual.X-x)>1e-5||Math.Abs(actual.Y-y)>1e-5)return false;
            }
            return true;
        }
        static Dictionary<string,string> RollingRouteBindings(Database source,Transaction st,Database target,Transaction tt,List<Row> rows,List<Row> existing)
        {
            var candidates=new List<KeyValuePair<string,string>>();
            foreach(var seg in rows.Where(r=>r.Sub=="SEG")) {
                var old=existing.FirstOrDefault(r=>r.Sub=="SEG"&&r.Key==seg.Key);
                if(old==null||old.Record.Get("route_id")==""||seg.Record.Get("route_id")=="")continue;
                var a=rows.FirstOrDefault(r=>r.Sub=="ROUTE"&&r.Key==seg.Record.Get("route_id"));
                var b=existing.FirstOrDefault(r=>r.Sub=="ROUTE"&&r.Key==old.Record.Get("route_id"));
                if(a!=null&&b!=null&&ProgressPolicy.SameRoute(a.Record,b.Record)&&RouteGeometryMatches(source,st,a.Record)&&RouteGeometryMatches(target,tt,b.Record))candidates.Add(new KeyValuePair<string,string>(a.Key,b.Key));
            }
            return candidates.GroupBy(p=>p.Key).Where(g=>g.Select(p=>p.Value).Distinct().Count()==1).ToDictionary(g=>g.Key,g=>g.First().Value);
        }
        static Dictionary<string,string> PrepareRolling(Database source,Transaction st,Database target,Transaction tt,ref List<Row> rows,HashSet<ObjectId> ids,List<Row> existing)
        {
            if(Versioned(rows.Concat(existing)))return PrepareVersionedRolling(source,st,target,tt,ref rows,ids,existing);
            var sourcePoints=BhtStore.Points(st,source);var targetPoints=BhtStore.Points(tt,target);
            var aliases=PackageAliasPolicy.VerifiedAliases(rows.Where(r=>r.Sub=="SEG").Select(r=>r.Record.Clone().Set("segment_id",r.Key)),existing.Where(r=>r.Sub=="SEG").Select(r=>r.Record.Clone().Set("segment_id",r.Key)));
            // Normalize only detached incoming records, to the destination spelling.
            // The same normalized input is used by preview and execution.
            foreach(var row in rows.Where(r=>r.Sub=="OBJ"||r.Sub=="SEG")) {
                string field=row.Sub=="OBJ"?"goi":"package_id",mapped;
                if(!aliases.TryGetValue(row.Record.Get(field),out mapped))continue;
                if(row.Sub=="OBJ"&&!rows.Any(s=>s.Sub=="SEG"&&s.Key==row.Record.Get("doan")&&
                    PackageAliasPolicy.SpellingKey(s.Record.Get("package_id"))==PackageAliasPolicy.SpellingKey(mapped)))
                    throw new ImportConflictException(new[]{new ImportConflict("Hồ sơ",row.Key,"Gói không khớp bảng phân đoạn","goi",row.Record.Get(field),mapped)});
                row.Record=row.Record.Clone().Set(field,mapped);
            }
            var reused=RollingRouteBindings(source,st,target,tt,rows,existing);
            CheckRollingConflicts(rows,existing,sourcePoints,targetPoints,reused);
            ValidateConfirmedPoints(target,null);
            if(sourcePoints.GroupBy(p=>p.IdUpper).Any(g=>g.Count()>1)||targetPoints.GroupBy(p=>p.IdUpper).Any(g=>g.Count()>1))throw new InvalidOperationException("Trùng mã điểm trong bản vẽ; kiểm tra điểm trước khi nạp dữ liệu kế thừa.");
            var targetIndex=targetPoints.ToDictionary(p=>p.IdUpper);var duplicates=new HashSet<string>();
            foreach(var p in sourcePoints){SurveyPoint q;if(targetIndex.TryGetValue(p.IdUpper,out q)){
                if(Math.Abs(p.X-q.X)>1e-6||Math.Abs(p.Y-q.Y)>1e-6||Math.Abs(p.Z-q.Z)>1e-6)throw new InvalidOperationException("Điểm "+p.Id+" trùng mã nhưng khác tọa độ. Chưa nạp dữ liệu.");
                duplicates.Add(p.IdUpper);
            }}
            var skip=new HashSet<string>();var replace=new HashSet<string>();
            foreach(var row in rows.Where(r=>r.Sub=="OBJ")){
                var old=existing.FirstOrDefault(r=>r.Sub=="OBJ"&&r.Key==row.Key);if(old==null)continue;
                var conflicts=ProgressPolicy.RollingConflicts(row.Key,old.Record,row.Record).ToArray();
                if(conflicts.Length>0)throw new ImportConflictException(conflicts);
                if(!ProgressPolicy.Locked(old.Record)&&ProgressPolicy.Locked(row.Record)){
                    replace.Add(row.Key);
                    // Preserve destination annotations when adopting a confirmed station result.
                    foreach(var key in old.Record.Pairs.Select(p=>p.Key).Distinct().ToArray())
                        if(!ProgressPolicy.ProtectedFields.Contains(key)&&!key.StartsWith("station_",StringComparison.Ordinal)&&!key.StartsWith("progress_",StringComparison.Ordinal)&&!key.StartsWith("field_",StringComparison.Ordinal)&&!key.StartsWith("kh_",StringComparison.Ordinal)&&!key.StartsWith("sign_heading_",StringComparison.Ordinal)&&!new[]{"gan_doan_pp","doan_ung_vien","nguon_km","marker_display_handle","custom_block"}.Contains(key))row.Record.SetAll(key,old.Record.GetAll(key));
                }else skip.Add(row.Key);
            }
            var neededRoutes=new HashSet<string>(rows.Where(r=>r.Sub=="OBJ"&&!skip.Contains(r.Key)).SelectMany(r=>new[]{r.Record.Get("route_id"),r.Record.Get("sign_heading_route")}));
            var unusedRoutes=rows.Where(r=>r.Sub=="ROUTE"&&(!neededRoutes.Contains(r.Key)||reused.ContainsKey(r.Key))).ToArray();
            foreach(var row in unusedRoutes)foreach(string k in new[]{"handle","tdt_source_handle"})ids.Remove(HandleId(source,row.Record.Get(k)));
            rows=rows.Where(r=>!unusedRoutes.Contains(r)).ToList();
            var names=new Dictionary<string,string>(reused);var used=new HashSet<string>(existing.Where(r=>r.Sub=="ROUTE").Select(r=>r.Key));
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
                    if(PackageAliasPolicy.SegmentFields.Any(k=>!SurveyComparison.FieldEqual(old.Record,row.Record,k)))throw new InvalidOperationException("Bảng đoạn "+row.Key+" khác phạm vi/phía/gói. Đối chiếu bảng trước khi gộp.");
                    bool importingResult=rows.Any(r=>r.Sub=="OBJ"&&!skip.Contains(r.Key)&&ProgressPolicy.Locked(r.Record)&&r.Record.Get("doan")==row.Key);
                    string bound;
                    if(importingResult && old.Record.Get("route_id")!=""&&(!reused.TryGetValue(row.Record.Get("route_id"),out bound)||bound!=old.Record.Get("route_id")))throw new InvalidOperationException("Đoạn "+row.Key+" đã gắn tim khác ở đích. Đối chiếu tim trước khi nạp.");
                    if(!importingResult||old.Record.Get("route_id")!="")removeRows.Add(row);
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
            return ImportRolling(target,path,keepCoordinates,null);
        }
        public static int ImportRolling(Database target,string path,bool keepCoordinates,Action<string> report)
        {
            TransferStep(report,"Kiểm tra cấu trúc và SHA256 tệp BHT");
            using(var package=ProjectPackage.Open(path))using(var source=new Database(false,true)){
                TransferStep(report,"Đọc DWG trong tệp BHT");
                source.ReadDwgFile(package.DrawingPath,FileOpenMode.OpenForReadAndAllShare,true,"");source.CloseInput(true);
                TransferStep(report,"Kiểm tra điểm/tim đã xác nhận trong nguồn");
                ValidateConfirmedPoints(source,null);
                return Copy(source,target,false,keepCoordinates,false,true,null,report);
            }
        }
        // Installed by the GUI plugin. Core Console and FILEDIA=0 keep the scriptable prompt.
        public static Func<string> RollingFilePicker { get; set; }

        [CommandMethod("BHTNAPTICHLUY")]
        public static void ImportRollingCommand()
        {
            var doc=Application.DocumentManager.MdiActiveDocument;
            if(doc==null)return;
            RollingImportLog.Begin(doc.Name);
            string path;
            try {
                var picker=RollingFilePicker;
                if(picker!=null && Convert.ToInt32(Application.GetSystemVariable("FILEDIA"))!=0) {
                    RollingImportLog.Note("Mở bảng chọn hồ sơ kế thừa BHT");
                    path=picker();
                } else {
                    RollingImportLog.Note("Chọn tệp bằng dòng lệnh/AutoCAD");
                    var pick=doc.Editor.GetFileNameForOpen(new Autodesk.AutoCAD.EditorInput.PromptOpenFileOptions("\nNạp dữ liệu tích lũy, giữ lý trình đã tính: "){Filter="BHT (*.bht)|*.bht"});
                    path=pick.Status==Autodesk.AutoCAD.EditorInput.PromptStatus.OK?pick.StringResult:null;
                }
                if(string.IsNullOrWhiteSpace(path)){RollingImportLog.Note("ĐÃ HỦY — chưa đọc hay nạp dữ liệu");return;}
                RollingImportLog.Note("Cửa sổ chọn tệp đã đóng. Nguồn: "+path);
                if(Application.DocumentManager.MdiActiveDocument!=doc)throw new InvalidOperationException("Đã chuyển bản vẽ trong lúc chọn tệp. Chưa nạp dữ liệu.");
            }catch(System.Exception ex){RollingImportLog.Note("LỖI CHỌN TỆP — "+ex);doc.Editor.WriteMessage("\nBHT: "+ex.Message);return;}
            if(RollingImportLog.PathName!=null)doc.Editor.WriteMessage("\nNhật ký nạp BHT: "+RollingImportLog.PathName);
            Action<string> report=step=>{
                RollingImportLog.Note(step);
                doc.Editor.WriteMessage("\nBHT: "+step);
            };
            try{int n;
                try { n=ImportRolling(doc.Database,path,false,report); }
                catch(UnitMismatchException ex){
                    doc.Editor.WriteMessage("\n"+ex.Message+" Chỉ giữ nguyên tọa độ nếu hai bản vẽ cùng hệ tọa độ thực tế.");
                    var options=new Autodesk.AutoCAD.EditorInput.PromptKeywordOptions("\nGiữ nguyên tọa độ [Giunguyen/Huy] <Huy>: ");options.Keywords.Add("Giunguyen");options.Keywords.Add("Huy");options.Keywords.Default="Huy";options.AllowNone=true;
                    var answer=doc.Editor.GetKeywords(options);if(answer.Status!=Autodesk.AutoCAD.EditorInput.PromptStatus.OK||answer.StringResult!="Giunguyen"){RollingImportLog.Note("ĐÃ HỦY — không duyệt khác đơn vị");return;}
                    n=ImportRolling(doc.Database,path,true,report);
                }
                doc.Editor.WriteMessage("\nĐã nạp dữ liệu tích lũy: "+n+" đối tượng CAD mới. Kết quả cũ và nhãn lý trình được giữ. Hồ sơ trùng đã có được bỏ qua; xung đột bị chặn. Mở Quản lý xử lý theo đoạn để chọn đoạn tiếp theo.");}
            catch(ImportConflictException ex){
                TransferStep(report,"ĐÃ CHẶN — "+ex.Message);
                foreach(var conflict in ex.Conflicts)RollingImportLog.Note(conflict.ToString());
                doc.Editor.WriteMessage("\nBHT: "+ex.Message);
                var show=ImportConflictsDetected;
                if(show!=null)try{RollingImportLog.Note("Yêu cầu hiển thị bảng đối chiếu");show(ex.Conflicts);return;}catch(System.Exception error){RollingImportLog.Note("LỖI BẢNG ĐỐI CHIẾU — "+error);doc.Editor.WriteMessage("\nKhông mở được bảng đối chiếu: "+error.Message);}
                foreach(var conflict in ex.Conflicts)doc.Editor.WriteMessage("\n"+conflict);
            }
            catch(System.Exception ex){TransferStep(report,"CHƯA NẠP — "+ex.Message);RollingImportLog.Note(ex.ToString());}
            finally{RollingImportLog.Note("Kết thúc xử lý lệnh nạp");}
        }
    }
}
