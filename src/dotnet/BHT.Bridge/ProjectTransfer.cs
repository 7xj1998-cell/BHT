using System;using System.IO;using System.Linq;using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.Core;using BHT.Core;
[assembly:CommandClass(typeof(BHT.Bridge.ProjectTransfer))]
namespace BHT.Bridge {
 public static partial class ProjectTransfer {
  sealed class Row {public string Sub,Key;public BhtRecord Record;}
  static List<Row> Records(Database db,Transaction tr){
   var result=new List<Row>();var root=BhtStore.RootDict(tr,db,false);
   if(root==null)return result;
   foreach(DBDictionaryEntry sub in root){
    if(!(tr.GetObject(sub.Value,OpenMode.ForRead) is DBDictionary))continue;
    foreach(var row in BhtStore.All(tr,db,sub.Key))result.Add(new Row{Sub=sub.Key,Key=row.Key,Record=row.Value});
   }
   return result;
  }
  static bool Tagged(Entity e){using(var data=e.XData){return data!=null && data.AsArray().Any(v=>v.TypeCode==1001 && Convert.ToString(v.Value).StartsWith("BHT_",StringComparison.Ordinal));}}
  static ObjectId HandleId(Database db,string value){long n;ObjectId id;
   return long.TryParse(value,System.Globalization.NumberStyles.HexNumber,System.Globalization.CultureInfo.InvariantCulture,out n) && db.TryGetObjectId(new Handle(n),out id) && !id.IsErased ? id : ObjectId.Null;}
  static bool StationCaption(Entity e,List<Row> rows){using(var data=e.GetXDataForApplication("BHT_KH")){
   if(data==null)return false;var text=data.AsArray().Where(v=>v.TypeCode==1000).Select(v=>Convert.ToString(v.Value)).ToArray();
   if(text.Length>=6 && text[1]=="NHAN" && text[5]=="STATION")return true;
   var label=e as DBText;double station;
   return text.Length>=2 && text.Length<6 && text[1]=="NHAN" && label!=null && Tdt91Stakes.TryStation(label.TextString,out station)
    && rows.Any(r=>r.Sub=="OBJ" && r.Key==text[0] && r.Record.Get("nhom")!="COC_TIEU" && r.Record.Get("nhom")!="COT_KM");}}
  static bool HasTag(Entity e,string name){using(var data=e.GetXDataForApplication(name))return data!=null;}
  static HashSet<ObjectId> Entities(Database db,Transaction tr,List<Row> rows,bool surveyOnly=false){
   var result=new HashSet<ObjectId>();var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
   var model=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead);
   foreach(ObjectId id in model){var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e!=null && Tagged(e)){
    if(surveyOnly && (StationCaption(e,rows) || HasTag(e,Tdt91Alignment.AppName) || HasTag(e,OffsetDimension.AppName)))continue;
    result.Add(id);}}
   foreach(ObjectId blockId in bt){var space=(BlockTableRecord)tr.GetObject(blockId,OpenMode.ForRead);
    if(!space.IsLayout || blockId==bt[BlockTableRecord.ModelSpace])continue;
    foreach(ObjectId id in space){var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e!=null && Tagged(e))
      throw new InvalidOperationException("Có dữ liệu BHT trong Layout. Chuyển các đối tượng BHT về Model trước khi xuất dự án.");}}
   foreach(var row in rows.Where(r=>r.Sub=="ROUTE"))foreach(string key in new[]{"handle","tdt_source_handle"}){
    string value=row.Record.Get(key);if(value=="")continue;var id=HandleId(db,value);
    if(id.IsNull){if(key=="handle")throw new InvalidOperationException("Tuyến "+row.Key+" không còn hình học. Chọn lại tuyến trước khi xuất.");continue;}
    result.Add(id);
   }
   foreach(var row in rows.Where(r=>r.Sub=="OBJ" || (!surveyOnly && r.Sub=="META"))){
    string key=row.Sub=="OBJ"?"marker_display_handle":"symbol_road_handle";
    var id=HandleId(db,row.Record.Get(key));
    if(!id.IsNull && tr.GetObject(id,OpenMode.ForRead) is Curve)result.Add(id);
   }
   return result;
  }
  static string MappedHandle(Database source,string value,IdMapping map){
   var id=HandleId(source,value);if(id.IsNull)return "";
   try{var pair=map[id];return pair.Value.IsNull?"":pair.Value.Handle.ToString();}catch{return "";}
  }
  static string MappedBlock(Database source,Transaction tr,string name,IdMapping map){
   var bt=(BlockTable)tr.GetObject(source.BlockTableId,OpenMode.ForRead);if(!bt.Has(name))return name;
   try{var pair=map[bt[name]];if(!pair.Value.IsNull)using(var tt=pair.Value.Database.TransactionManager.StartOpenCloseTransaction())return ((BlockTableRecord)tt.GetObject(pair.Value,OpenMode.ForRead)).Name;}catch{}
   return name;
  }
  static bool HasGeometry(Database db,Transaction tr){
   var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
   foreach(ObjectId id in bt){var space=(BlockTableRecord)tr.GetObject(id,OpenMode.ForRead);if(space.IsLayout && space.Cast<ObjectId>().Any())return true;}
   return false;
  }
  public sealed class UnitMismatchException:InvalidOperationException {
   public UnitMismatchException(UnitsValue source,UnitsValue target):base("Đơn vị nguồn: "+source+"; bản vẽ đích: "+target+". BHT không tự đổi tỷ lệ tọa độ."){}
  }
  static readonly string[] DerivedKeys={"route_id","ly_trinh_m","ly_trinh_km","offset_m","phia_tuyen","loai_tuyen","station_route_revision",
   "field_chain_id","field_pair_a","field_pair_b","field_last_error","doan","goi","doan_ung_vien","station_point_id","station_point_method",
   "station_point_e","station_point_n","station_projection_e","station_projection_n","field_method","field_sources_signature"};
  static void ResetStation(BhtRecord record){
   record.Set("progress_locked","0").Set("progress_state","CHUA_TINH").Set("progress_confirmed_at","").Set("progress_route_signature","").Set("progress_context","").Set("progress_route_state","").SetAll("progress_point",new string[0]);
   foreach(string key in new[]{"route_id","ly_trinh_m","ly_trinh_km","doan","goi"})record.Set("import_source_"+key,record.Get(key));
   foreach(string key in DerivedKeys)record.Set(key,"");
   record.Set("station_control_eligible","0").Set("station_source","CHUA_TINH").Set("station_status","NO_ROUTE")
    .Set("trang_thai_km","CHUA_TINH").Set("nguon_km","Chờ tính theo tim bản vẽ đích; số liệu nguồn giữ để đối chiếu.").Set("gan_doan_pp","CHUA_PHAN_DOAN");
   if(record.Get("sign_heading_route")!="")record.Set("sign_heading_mode","").Set("sign_heading_route","").Set("sign_heading_angle","");
   MarkerStation.Apply(record);
  }
  static void TransferStep(Action<string> report,string text){if(report!=null)try{report(text);}catch{}}
  static int Copy(Database source,Database target,bool allowExisting,bool keepCoordinates=false,bool surveyOnly=false,bool rolling=false,string segment=null,Action<string> report=null){
   // Side databases default to disabled undo; native transaction abort then does
   // not restore dictionary edits. Enable recording before starting the write.
   bool disabled=!target.UndoRecording;
   if(disabled)target.DisableUndoRecording(false);
   try{return CopyTransaction(source,target,allowExisting,keepCoordinates,surveyOnly,rolling,segment,report);}
   finally{if(disabled)target.DisableUndoRecording(true);}
  }
  static int CopyTransaction(Database source,Database target,bool allowExisting,bool keepCoordinates,bool surveyOnly,bool rolling,string segment,Action<string> report){
   using(var st=source.TransactionManager.StartOpenCloseTransaction())
   using(var tt=target.TransactionManager.StartTransaction()){
    DrawingProject.RequireWritable(tt,target);
    TransferStep(report,"Kiểm tra bản ghi và lịch sử nguồn/đích");
    var rows=Records(source,st);
    rows.RemoveAll(r=>r.Sub=="DRAWING"||r.Sub=="DRAWING_VIEWS");
    if(rows.Any(r=>r.Sub=="VIEW_LOCAL"))throw new InvalidOperationException("Nguồn có bố trí LOCAL. Lưu DWG riêng; xuất gói đồng bộ từ bình đồ GLOBAL để không phát tán vị trí riêng.");
    BhtStore.ValidateHistory(st,source);
    BhtStore.ValidateHistory(tt,target);
    HashSet<ObjectId> selectedIds=null;
    if(segment!=null) selectedIds=SelectSegment(source,st,ref rows,segment);
    if(surveyOnly)rows=rows.Where(r=>r.Sub!="ROUTE" && r.Sub!="FIELD_CHAIN").ToList();
    TransferStep(report,"Đọc thực thể CAD cần nạp");
    var ids=selectedIds??Entities(source,st,rows,surveyOnly);
    var retired=new HashSet<string>(rows.Where(r=>r.Sub=="OBJ"&&ObjectRevision.Retired(r.Record)).Select(r=>r.Key));
    ids.RemoveWhere(id=>new[]{"BHT_KH","BHT_SIGN_ORIGIN","BHT_OFFSET_DIM"}.Any(tag=>retired.Contains(TagId((Entity)st.GetObject(id,OpenMode.ForRead),tag))));
    if(surveyOnly && !rows.Any(r=>r.Sub=="OBJ"))throw new InvalidOperationException("Tệp không có hồ sơ báo hiệu BHT để nạp.");
    if(!rows.Any(r=>r.Sub!="META") && ids.Count==0)throw new InvalidOperationException("Tệp không có dữ liệu dự án BHT; chỉ có cấu hình hoặc hình CAD thông thường.");
    // Loading BHT initializes META even in a drawing with no project data.
    // Configuration alone must not prevent the first import.
    var existing=Records(target,tt);
    existing.RemoveAll(r=>r.Sub=="DRAWING"||r.Sub=="DRAWING_VIEWS");
    if(rolling||surveyOnly){
     var segmentAliases=SegmentDuplicatePolicy.NewAliasConflicts(rows.Where(r=>r.Sub=="SEG").Select(r=>r.Record.Clone().Set("segment_id",r.Key)),existing.Where(r=>r.Sub=="SEG").Select(r=>r.Record.Clone().Set("segment_id",r.Key)));
     if(segmentAliases.Count>0)throw new ImportConflictException(segmentAliases);
    }
    CheckRevisionTransfer(rows,existing,surveyOnly,rolling);
    Dictionary<string,string> routeNames=null;
    if(rolling) {
     TransferStep(report,"Đối chiếu mã hồ sơ, điểm RTK, tim và phân đoạn");
     routeNames=PrepareRolling(source,st,target,tt,ref rows,ids,existing);
     if(!rows.Any(r=>r.Sub=="OBJ")){TransferStep(report,"Không có thay đổi; giữ nguyên bản vẽ");return 0;}
    }
    if(surveyOnly && (existing.Any(r=>r.Sub!="META" && r.Sub!="ROUTE" && r.Sub!="SEG") || rows.Any(r=>r.Sub!="SEG" && r.Sub!="META" && existing.Any(x=>x.Sub==r.Sub && x.Key==r.Key))))
      throw new InvalidOperationException("Bản vẽ đích đã có dữ liệu khảo sát BHT. Dùng bản đoạn chưa nạp báo hiệu để tránh trùng ID; giữ tim và bảng đoạn hiện có.");
    if(!rolling && !surveyOnly && !allowExisting && (existing.Any(r=>r.Sub!="META") || Entities(target,tt,new List<Row>()).Count>0))
      throw new InvalidOperationException("DWG đích đã có dữ liệu BHT. Mở DWG chưa có BHT để nạp; dữ liệu hiện tại được giữ nguyên.");
    bool adoptUnits=!HasGeometry(target,tt);
    if(source.Insunits!=target.Insunits && !adoptUnits && !keepCoordinates)throw new UnitMismatchException(source.Insunits,target.Insunits);
    var table=(BlockTable)tt.GetObject(target.BlockTableId,OpenMode.ForRead);
    var map=new IdMapping();
    TransferStep(report,"Sao chép "+ids.Count+" thực thể CAD; giữ nguyên tọa độ");
    if(ids.Count>0)source.WblockCloneObjects(new ObjectIdCollection(ids.ToArray()),table[BlockTableRecord.ModelSpace],map,DuplicateRecordCloning.MangleName,false);
    TransferStep(report,"Khôi phục thứ tự hiển thị ký hiệu");
    var sourceTable=(BlockTable)st.GetObject(source.BlockTableId,OpenMode.ForRead);
    BhtSignLibrary.RestoreDrawOrder(st,tt,sourceTable[BlockTableRecord.ModelSpace],table[BlockTableRecord.ModelSpace],map,new HashSet<ObjectId>());
    int written=0;
    foreach(var row in rows.OrderBy(r=>r.Sub=="OBJ"?1:0)){
     if(written++%100==0)TransferStep(report,"Ghi hồ sơ trong giao dịch: "+written+" / "+rows.Count);
     if(surveyOnly && row.Sub=="SEG" && existing.Any(x=>x.Sub=="SEG" && x.Key==row.Key))continue;
     var record=row.Record.Clone();
     if(rolling) {
      foreach(string k in new[]{"route_id","sign_heading_route"}) { string renamed; if(routeNames.TryGetValue(record.Get(k),out renamed))record.Set(k,renamed); }
      if(row.Sub=="OBJ"&&!ObjectRevision.Tracked(record))record.Set("station_control_eligible","0");
     }
     if(row.Sub=="ROUTE"){
      foreach(string key in new[]{"handle","tdt_source_handle"})if(record.Get(key)!="")record.Set(key,MappedHandle(source,record.Get(key),map));
      string signature=record.Get("geometry_signature");int split=signature.IndexOf('|');
      // Old packages can already contain the handle from an earlier export.
      // Rebind only identity; sampled geometry remains unchanged and still detects edits.
      if(split>=0)record.Set("geometry_signature",record.Get("handle")+signature.Substring(split));
      var routeId=HandleId(target,record.Get("handle"));
      if(!routeId.IsNull && record.Get("tdt_source_handle")!=""){
       var routeEntity=tt.GetObject(routeId,OpenMode.ForWrite) as Entity;
       if(routeEntity!=null)using(var oldData=routeEntity.GetXDataForApplication(Tdt91Alignment.AppName))if(oldData!=null){
        var values=oldData.AsArray();for(int i=0;i<values.Length;i++)if(values[i].TypeCode==1000){values[i]=new TypedValue(1000,record.Get("tdt_source_handle"));break;}
        using(var data=new ResultBuffer(values))routeEntity.XData=data;
       }
      }
     }
     if(surveyOnly && row.Sub=="OBJ")ResetStation(record);
     if(surveyOnly && row.Sub=="SEG")record.Set("import_source_route_id",record.Get("route_id")).Set("route_id","");
     if(surveyOnly && row.Sub=="META"){
      var destination=BhtStore.Read(tt,target,row.Sub,row.Key)??new BhtRecord();
      foreach(var pair in record.Pairs)if((pair.Key=="thu_muc_anh" || pair.Key.StartsWith("anh_",StringComparison.Ordinal) ||
       pair.Key.StartsWith("nhan_",StringComparison.Ordinal) || pair.Key.StartsWith("pt_",StringComparison.Ordinal) || pair.Key.StartsWith("sign_",StringComparison.Ordinal)) && destination.Get(pair.Key)=="")destination.Set(pair.Key,pair.Value);
      record=destination;
     }
     if(row.Sub=="OBJ" && record.Get("marker_display_handle")!="")record.Set("marker_display_handle",MappedHandle(source,record.Get("marker_display_handle"),map));
     if(!surveyOnly && !rolling && row.Sub=="META" && record.Get("symbol_road_handle")!="")record.Set("symbol_road_handle",MappedHandle(source,record.Get("symbol_road_handle"),map));
     foreach(var pair in record.Pairs.ToArray())if(!((surveyOnly || rolling) && row.Sub=="META") && (pair.Key=="custom_block" || pair.Key.StartsWith("kh_block_",StringComparison.Ordinal)))
      record.Set(pair.Key,MappedBlock(source,st,pair.Value,map));
     if(rolling&&row.Sub=="OBJ"&&ObjectRevision.Tracked(record))BhtStore.WriteMergedRevision(tt,target,row.Key,record);
     else BhtStore.Write(tt,target,row.Sub,row.Sub=="ROUTE" && rolling ? routeNames[row.Key] : row.Key,record);
     // Keep clone isolation for blocks/styles; only route references get semantic layers.
     if(row.Sub=="ROUTE")RouteLayers.ApplyOne(tt,target,rolling?routeNames[row.Key]:row.Key,record);
    }
    if(adoptUnits)target.Insunits=source.Insunits;
    TransferStep(report,"Kiểm tra lịch sử trước khi hoàn tất giao dịch");
    BhtStore.ValidateHistory(tt,target);
    tt.Commit();TransferStep(report,"Hoàn tất giao dịch");return ids.Count;
   }
  }
  public static int Export(Database source,string path){return ExportSelection(source,path,null);}
  public static int ExportSelection(Database source,string path,string segment){
   ValidateConfirmedPoints(source,segment);
   if(File.Exists(path))throw new IOException("Tệp đã tồn tại. Chọn tên khác để giữ bản đã xuất.");
   string full=Path.GetFullPath(path),temp=full+"."+Guid.NewGuid().ToString("N")+".tmp.dwg";
   try{using(var target=new Database(true,true)){
    target.Insunits=source.Insunits;target.Insbase=source.Insbase;
    int count=Copy(source,target,false,false,false,false,segment);target.SaveAs(temp,DwgVersion.AC1032);
    if(Path.GetExtension(full).Equals(".bht",StringComparison.OrdinalIgnoreCase))ProjectPackage.Write(full,temp);
    else if(Path.GetExtension(full).Equals(".dwg",StringComparison.OrdinalIgnoreCase))File.Move(temp,full);
    else throw new InvalidOperationException("Lưu dự án với phần mở rộng .bht.");
    return count;
   }}finally{if(File.Exists(temp))File.Delete(temp);}
  }
  public static int Import(Database target,string path){return Import(target,path,false);}
  public static int Import(Database target,string path,bool keepCoordinates){return Import(target,path,keepCoordinates,false);}
  public static int Import(Database target,string path,bool keepCoordinates,bool surveyOnly){
   string extension=Path.GetExtension(path);
   if(!extension.Equals(".bht",StringComparison.OrdinalIgnoreCase) && !extension.Equals(".dwg",StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Chọn tệp dự án .bht hoặc DWG BHT cũ.");
   using(var package=extension.Equals(".bht",StringComparison.OrdinalIgnoreCase)?ProjectPackage.Open(path):null)
   using(var source=new Database(false,true)){source.ReadDwgFile(package==null?path:package.DrawingPath,FileOpenMode.OpenForReadAndAllShare,true,"");source.CloseInput(true);
    return Copy(source,target,false,keepCoordinates,surveyOnly);
   }
  }
  [LispFunction("BHTPROJECTEXPORT")]public static string ExportLisp(ResultBuffer args){try{return "OK|"+Export(Application.DocumentManager.MdiActiveDocument.Database,Convert.ToString(args.AsArray()[0].Value));}catch(System.Exception ex){return "LOI|"+ex.Message;}}
  [LispFunction("BHTPROJECTIMPORT")]public static string ImportLisp(ResultBuffer args){try{return "OK|"+Import(Application.DocumentManager.MdiActiveDocument.Database,Convert.ToString(args.AsArray()[0].Value));}catch(System.Exception ex){return "LOI|"+ex.Message;}}
  [LispFunction("BHTSURVEYIMPORT")]public static string SurveyImportLisp(ResultBuffer args){try{return "OK|"+Import(Application.DocumentManager.MdiActiveDocument.Database,Convert.ToString(args.AsArray()[0].Value),false,true);}catch(System.Exception ex){return "LOI|"+ex.Message;}}
  [CommandMethod("BHTXUATDUAN")]public static void ExportCommand(){Run(false);}
  [CommandMethod("BHTNAPDUAN")]public static void ImportCommand(){Run(true);}
  [CommandMethod("BHTNAPBAOHIEU")]public static void ImportSurveyCommand(){Run(true,true);}
  public static string ContentSummary(Database db){using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
   int objects=BhtStore.All(tr,db,"OBJ").Count,routes=BhtStore.All(tr,db,"ROUTE").Count,segments=BhtStore.All(tr,db,"SEG").Count;
   return objects+" hồ sơ báo hiệu; "+routes+" tim tuyến; "+segments+" đoạn/gói thầu."+(segments==0?" Chưa có bảng phân chia đoạn trong dự án.":"");}}
  static void Run(bool import,bool surveyOnly=false){var doc=Application.DocumentManager.MdiActiveDocument;if(doc==null)return;
   var result=import?doc.Editor.GetFileNameForOpen(new Autodesk.AutoCAD.EditorInput.PromptOpenFileOptions("\nChọn dự án BHT: "){Filter="Dự án BHT (*.bht)|*.bht|Dự án DWG BHT cũ (*.dwg)|*.dwg"}):
    doc.Editor.GetFileNameForSave(new Autodesk.AutoCAD.EditorInput.PromptSaveFileOptions("\nLưu dự án BHT: "){Filter="Dự án BHT (*.bht)|*.bht"});
   if(result.Status!=Autodesk.AutoCAD.EditorInput.PromptStatus.OK)return;
   try{int n;string path=result.StringResult,sourceSummary="";
    if(!import && !Path.GetExtension(path).Equals(".bht",StringComparison.OrdinalIgnoreCase))path=Path.ChangeExtension(path,".bht");
    var previousUnits=doc.Database.Insunits;
    if(import)using(var package=Path.GetExtension(path).Equals(".bht",StringComparison.OrdinalIgnoreCase)?ProjectPackage.Open(path):null)
     using(var source=new Database(false,true)){source.ReadDwgFile(package==null?path:package.DrawingPath,FileOpenMode.OpenForReadAndAllShare,true,"");source.CloseInput(true);sourceSummary=ContentSummary(source);}
    try{n=import?Import(doc.Database,path,false,surveyOnly):Export(doc.Database,path);}
    catch(UnitMismatchException ex){
     doc.Editor.WriteMessage("\n"+ex.Message+" Chỉ giữ nguyên tọa độ khi hai bản vẽ dùng cùng hệ tọa độ thực tế.");
     var options=new Autodesk.AutoCAD.EditorInput.PromptKeywordOptions("\nNạp giữ nguyên tọa độ, không đổi đơn vị bản vẽ đích [Giunguyen/Huy] <Huy>: ");
     options.Keywords.Add("Giunguyen");options.Keywords.Add("Huy");options.Keywords.Default="Huy";options.AllowNone=true;
     var choice=doc.Editor.GetKeywords(options);
     if(choice.Status!=Autodesk.AutoCAD.EditorInput.PromptStatus.OK || choice.StringResult!="Giunguyen")return;
     n=Import(doc.Database,path,true,surveyOnly);
    }
    doc.Editor.WriteMessage("\nĐã "+(import?"nạp":"xuất")+" dự án BHT: "+n+" đối tượng CAD. Giữ nguyên tọa độ và liên kết ảnh; tệp ảnh gốc vẫn nằm ngoài gói dự án.\nTệp: "+path);
    doc.Editor.WriteMessage("\n"+(import?"Dữ liệu trong tệp nguồn: "+sourceSummary:"Dữ liệu đã xuất: "+ContentSummary(doc.Database)));
    if(import && previousUnits!=doc.Database.Insunits)doc.Editor.WriteMessage("\nBản vẽ trống đã nhận đơn vị nguồn: "+doc.Database.Insunits+".");
    if(surveyOnly)doc.Editor.WriteMessage("\nĐã nạp báo hiệu và bảng đoạn để tính theo tim đích. Không nạp tim/mốc nguồn; các đoạn mới chưa gán tim. Lý trình cũ chỉ giữ đối chiếu. Chọn tim đoạn → đọc/xác nhận ít nhất 2 mốc → tính lại theo tim đang chọn → gán đoạn theo tim.");
   }catch(System.Exception ex){doc.Editor.WriteMessage("\nBHT: "+ex.Message);}
  }
 }
}
