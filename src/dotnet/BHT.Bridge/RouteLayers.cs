using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using BHT.Core;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly:CommandClass(typeof(BHT.Bridge.RouteLayers))]
namespace BHT.Bridge
{
    public static class RouteLayers
    {
        public static string Name(string routeId)
        {
            if(!ObjectLogic.ValidId(routeId))throw new InvalidOperationException("Mã tuyến không hợp lệ để đặt layer: "+routeId);
            return "BHT_TUYEN_"+routeId;
        }
        static Entity Reference(Transaction tr,Database db,BhtRecord row)
        {
            var id=CadView.IdFromHandle(db,row.Get("handle"));
            var entity=id.IsNull?null:tr.GetObject(id,OpenMode.ForRead) as Entity;
            // Legacy packages also contain native Line/Arc references. Only change layers
            // on known native curves; custom TDT entities must retain their own layers.
            string native=entity==null?"":entity.GetRXClass().Name;
            if(!new[]{"AcDbPolyline","AcDb2dPolyline","AcDb3dPolyline","AcDbLine","AcDbArc","AcDbCircle","AcDbEllipse","AcDbSpline"}.Contains(native))
                throw new InvalidOperationException("Tim thiếu hoặc không phải đường tham chiếu AutoCAD thường: "+row.Get("route_id"));
            var owner=tr.GetObject(entity.OwnerId,OpenMode.ForRead) as BlockTableRecord;
            if(owner==null||!owner.IsLayout||owner.IsFromExternalReference)
                throw new InvalidOperationException("Tim nằm trong block/xref; chưa đổi layer.");
            return entity;
        }
        static bool SameStyle(LayerTableRecord a,LayerTableRecord b)
        {
            return a.Color.Equals(b.Color)&&a.LinetypeObjectId==b.LinetypeObjectId&&a.LineWeight==b.LineWeight
                &&a.Transparency.Equals(b.Transparency)&&a.IsPlottable==b.IsPlottable&&a.PlotStyleNameId==b.PlotStyleNameId
                &&a.IsOff==b.IsOff&&a.IsFrozen==b.IsFrozen&&a.IsLocked==b.IsLocked
                &&a.ViewportVisibilityDefault==b.ViewportVisibilityDefault;
        }
        static bool ViewportState(Transaction tr,Database db,LayerTableRecord layer)
        {
            if(layer.HasOverrides)return true;
            var blocks=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
            foreach(ObjectId id in blocks){var space=(BlockTableRecord)tr.GetObject(id,OpenMode.ForRead);
                if(!space.IsLayout||space.Name==BlockTableRecord.ModelSpace)continue;
                foreach(ObjectId child in space){var viewport=tr.GetObject(child,OpenMode.ForRead) as Viewport;
                    if(viewport!=null&&viewport.GetFrozenLayers().Contains(layer.ObjectId))return true;}}
            return false;
        }
        static void CheckTarget(Transaction tr,Database db,Entity entity,string name)
        {
            var layers=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);
            if(entity.Layer==name)return;
            var source=(LayerTableRecord)tr.GetObject(entity.LayerId,OpenMode.ForRead);
            if(source.IsLocked)throw new InvalidOperationException("Layer tim đang khóa: "+source.Name+". Mở khóa layer trình bày rồi thử lại; không mở khóa hồ sơ.");
            if(ViewportState(tr,db,source))throw new InvalidOperationException("Layer "+source.Name+" có thiết lập riêng trong viewport. Giữ nguyên để đối chiếu bố cục trước khi đổi layer.");
            if(layers.Has(name)){
                var target=(LayerTableRecord)tr.GetObject(layers[name],OpenMode.ForRead);
                if(target.IsDependent||ViewportState(tr,db,target)||!SameStyle(source,target))
                    throw new InvalidOperationException("Layer "+name+" đã có thuộc tính khác layer tim hiện tại. Đối chiếu màu, nét, in và trạng thái layer trước; chưa thay đổi dữ liệu.");
            }
        }
        internal static void ApplyOne(Transaction tr,Database db,string key,BhtRecord row)
        {
            var entity=Reference(tr,db,row);string name=Name(key);
            CheckTarget(tr,db,entity,name);if(entity.Layer==name)return;
            var layers=(LayerTable)tr.GetObject(db.LayerTableId,OpenMode.ForRead);
            if(!layers.Has(name)){
                var source=(LayerTableRecord)tr.GetObject(entity.LayerId,OpenMode.ForRead);
                var layer=new LayerTableRecord {Name=name};
                layers.UpgradeOpen();layers.Add(layer);tr.AddNewlyCreatedDBObject(layer,true);
                layer.Color=source.Color;layer.LinetypeObjectId=source.LinetypeObjectId;
                layer.LineWeight=source.LineWeight;layer.Transparency=source.Transparency;
                layer.IsPlottable=source.IsPlottable;if(!db.PlotStyleMode)layer.PlotStyleNameId=source.PlotStyleNameId;
                layer.IsOff=source.IsOff;layer.IsFrozen=source.IsFrozen;layer.Description="BHT - tim "+key;
                layer.ViewportVisibilityDefault=source.ViewportVisibilityDefault;
            }
            entity.UpgradeOpen();entity.LayerId=layers[name];
        }
        static List<KeyValuePair<string,BhtRecord>> Plan(Transaction tr,Database db)
        {
            DrawingProject.RequireWritable(tr,db);
            var rows=BhtStore.All(tr,db,"ROUTE");var seen=new HashSet<ObjectId>();
            foreach(var row in rows){var entity=Reference(tr,db,row.Value);
                if(!seen.Add(entity.ObjectId))throw new InvalidOperationException("Nhiều tuyến cùng tham chiếu một đường. Đối chiếu liên kết trước khi đặt layer.");
                CheckTarget(tr,db,entity,Name(row.Key));}
            return rows;
        }
        public static string[] Preview(Database db)
        {
            using(var tr=db.TransactionManager.StartOpenCloseTransaction())return Plan(tr,db).Select(p=>
                p.Key+": "+Reference(tr,db,p.Value).Layer+" -> "+Name(p.Key)).ToArray();
        }
        public static int Apply(Database db)
        {
            bool disabled=!db.UndoRecording;if(disabled)db.DisableUndoRecording(false);
            try{using(var tr=db.TransactionManager.StartTransaction()){
                var rows=Plan(tr,db);int count=0;
                foreach(var row in rows)if(Reference(tr,db,row.Value).Layer!=Name(row.Key)){
                    ApplyOne(tr,db,row.Key,row.Value);count++;}
                tr.Commit();return count;
            }}finally{if(disabled)db.DisableUndoRecording(true);}
        }
        [LispFunction("BHTROUTEWRITE")]
        public static string WriteLisp(ResultBuffer args)
        {
            try{
                var values=args.AsArray().Where(v=>v.TypeCode==5005).Select(v=>Convert.ToString(v.Value)).ToArray();
                if(values.Length<2)throw new InvalidOperationException("Thiếu mã hoặc dữ liệu tuyến.");
                var db=AcApp.DocumentManager.MdiActiveDocument.Database;bool disabled=!db.UndoRecording;
                if(disabled)db.DisableUndoRecording(false);
                try{using(var tr=db.TransactionManager.StartTransaction()){
                    var row=RecordCodec.Decode(values.Skip(1));
                    BhtStore.Write(tr,db,"ROUTE",values[0],row);
                    ApplyOne(tr,db,values[0].ToUpperInvariant(),row);
                    tr.Commit();return "OK";
                }}finally{if(disabled)db.DisableUndoRecording(true);}
            }catch(System.Exception ex){return "LOI|"+ex.Message;}
        }
    }
}
