using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using App=Autodesk.AutoCAD.ApplicationServices.Core.Application;
[assembly:CommandClass(typeof(BHT.Bridge.DisplaySelection))]
namespace BHT.Bridge {
 public static class DisplaySelection {
  static string ObjectKey(Entity entity) {
   foreach(string app in new[]{"BHT_KH","BHT_SIGN_ORIGIN"})
    using(var data=entity.GetXDataForApplication(app)) {
     if(data==null)continue;
     var id=data.AsArray().FirstOrDefault(v=>v.TypeCode==1000);
     if(id.Value!=null)return Convert.ToString(id.Value);
    }
   return null;
  }
  public static string[] Groups(Database db,IEnumerable<ObjectId> seeds) {
   var groups=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
   using(var tr=db.TransactionManager.StartOpenCloseTransaction())foreach(var id in seeds) {
    if(id.IsNull||id.IsErased||id.Database!=db)continue;
    var entity=tr.GetObject(id,OpenMode.ForRead) as Entity;
    var key=entity==null?null:ObjectKey(entity);
    var record=key==null?null:BhtStore.Read(tr,db,"OBJ",key);
    if(record!=null&&record.Get("nhom")!="")groups.Add(record.Get("nhom"));
   }
   return groups.ToArray();
  }
  public static ObjectId[] Collect(Database db,IEnumerable<string> groups) {
   var filter=groups==null?null:new HashSet<string>(groups,StringComparer.OrdinalIgnoreCase);
   var result=new List<ObjectId>();
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()) {
    var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
    var model=(BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead);
    foreach(ObjectId id in model) {
     if(id.IsErased)continue;
     var entity=tr.GetObject(id,OpenMode.ForRead) as Entity;
     var key=entity==null?null:ObjectKey(entity);if(key==null)continue;
     if(filter!=null) {
      var record=BhtStore.Read(tr,db,"OBJ",key);
      if(record==null||!filter.Contains(record.Get("nhom")))continue;
     }
     result.Add(id);
    }
   }
   return result.ToArray();
  }
  [CommandMethod("BHTCHONBAOHIEU",CommandFlags.UsePickSet|CommandFlags.Redraw)]
  public static void All(){Select(false);}
  [CommandMethod("BHTCHONTUONGTU",CommandFlags.UsePickSet|CommandFlags.Redraw)]
  public static void Similar(){Select(true);}
  static void Select(bool similar) {
   var doc=App.DocumentManager.MdiActiveDocument;if(doc==null)return;var ed=doc.Editor;
   try {
    string[] groups=null;
    if(similar) {
     var implied=ed.SelectImplied();
     groups=implied.Status==PromptStatus.OK?Groups(doc.Database,implied.Value.GetObjectIds()):new string[0];
     if(groups.Length==0) {
      var chosen=ed.GetEntity("\nChọn một hình/nhãn/đường nối báo hiệu BHT làm mẫu: ");
      if(chosen.Status!=PromptStatus.OK)return;
      groups=Groups(doc.Database,new[]{chosen.ObjectId});
     }
     if(groups.Length==0){ed.WriteMessage("\nĐối tượng chưa liên kết hồ sơ BHT. Chọn một báo hiệu BHT đã có hồ sơ.");return;}
    }
    var ids=Collect(doc.Database,groups);
    if(ids.Length==0){ed.WriteMessage("\nChưa có ký hiệu báo hiệu BHT trong Model. Đặt/cập nhật ký hiệu trước khi chọn.");return;}
    App.SetSystemVariable("PICKFIRST",1);
    ed.SetImpliedSelection(ids);
    ed.WriteMessage("\nĐã chọn "+ids.Length+" thành phần báo hiệu"+(groups==null?"":" thuộc nhóm "+string.Join(", ",groups))+". Hình, nhãn, đường nối và dấu gốc được chọn cùng nhau. COPY mang hình; BHTXUATDUAN chuyển cả hồ sơ.");
   }catch(System.Exception ex){ed.WriteMessage("\nBHT: không chọn được báo hiệu. "+ex.Message);}
  }
  [CommandMethod("BHTSELECTSIMILAR",CommandFlags.UsePickSet|CommandFlags.Redraw)]
  public static void NativeByLayer() {
   var doc=App.DocumentManager.MdiActiveDocument;if(doc==null)return;var ed=doc.Editor;
   try {
    var chosen=ed.SelectImplied();
    if(chosen.Status!=PromptStatus.OK)chosen=ed.GetSelection(new PromptSelectionOptions{MessageForAdding="\nChọn báo hiệu làm mẫu cho SELECTSIMILAR theo lớp: "});
    if(chosen.Status!=PromptStatus.OK)return;
    // Layer=2, block name=128. Different signs deliberately have different names.
    App.SetSystemVariable("SELECTSIMILARMODE",2);
    App.SetSystemVariable("PICKFIRST",1);
    ed.Command("_.SELECTSIMILAR",chosen.Value,"");
    var result=ed.SelectImplied();if(result.Status==PromptStatus.OK)ed.SetImpliedSelection(result.Value);
    ed.WriteMessage("\nSELECTSIMILARMODE=2: so sánh lớp, bỏ tên block. Thiết lập này cũng áp dụng cho SELECTSIMILAR về sau. CAD vẫn so cùng loại đối tượng; dùng BHTCHONTUONGTU để chọn đủ hình, nhãn và đường nối.");
   }catch(System.Exception ex){ed.WriteMessage("\nBHT: không chạy được SELECTSIMILAR. "+ex.Message);}
  }
 }
}
