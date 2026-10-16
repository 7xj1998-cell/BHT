using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

[assembly: Autodesk.AutoCAD.Runtime.CommandClass(typeof(BHT.Bridge.CustomDwgBlock))]
namespace BHT.Bridge
{
    public static class CustomDwgBlock
    {
        [Autodesk.AutoCAD.Runtime.LispFunction("BHTCURRENTLIGHT")]
        public static string CurrentLight(ResultBuffer args)
        {
            if(args==null || args.AsArray().Length!=1) return "";
            string old=Convert.ToString(args.AsArray()[0].Value);
            foreach(string group in new[]{"DEN_TH","DEN_CS"})
                foreach(var model in BHT.Core.LightModels.ForGroup(group))
                    if(old.IndexOf("BHT_LIGHT_"+model[0]+"_",StringComparison.OrdinalIgnoreCase)>=0)
                    {
                        string path=Path.Combine(LightFolder(),model[0]+".dwg");
                        return File.Exists(path)?Import(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database,path):old;
                    }
            return old;
        }
        [Autodesk.AutoCAD.Runtime.LispFunction("BHTLIGHTFACTOR")]
        public static double LightFactor(ResultBuffer args)
        {
            if(args==null || args.AsArray().Length!=1) return 0;
            string name=Convert.ToString(args.AsArray()[0].Value);
            var db=Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database;
            using(var tr=db.TransactionManager.StartOpenCloseTransaction())
            {
                var table=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
                if(!table.Has(name))return 0;
                using(var instance=new BlockReference(Autodesk.AutoCAD.Geometry.Point3d.Origin,table[name]))
                {
                    var ext=instance.GeometricExtents;
                    double size=Math.Max(ext.MaxPoint.X-ext.MinPoint.X,ext.MaxPoint.Y-ext.MinPoint.Y);
                    return size>1e-8 && !double.IsInfinity(size) ? 2.4/size : 0;
                }
            }
        }
        public static string LightFolder()
        {
            for(var dir=new DirectoryInfo(Path.GetDirectoryName(typeof(CustomDwgBlock).Assembly.Location));dir!=null;dir=dir.Parent)
                foreach(string path in new[] {Path.Combine(dir.FullName,"LightLibrary"),Path.Combine(dir.FullName,"assets","light-blocks")})
                    if(Directory.Exists(path)) return path;
            return "";
        }
        // Import the model at INSBASE. Content-based names keep existing definitions intact.
        public static string Import(Database target,string path)
        {
            string hash;
            using(var sha=SHA256.Create()) using(var file=File.OpenRead(path))
                hash=BitConverter.ToString(sha.ComputeHash(file)).Replace("-","").Substring(0,12);
            string code=Path.GetFileNameWithoutExtension(path).ToUpperInvariant();
            string[] lights={"DEN_CS_DON_MB","DEN_CS_DOI_MB","DEN_CS_TRANG_TRI_MB","DEN_CS_TRAI_MD","DEN_CS_PHAI_MD","DEN_TIN_HIEU_3_MAU","DEN_CANH_BAO_VANG","DEN_TH_CAN_VUON_TRAI","DEN_TH_CAN_VUON_PHAI"};
            string name=Array.IndexOf(lights,code)>=0 ? "BHT_LIGHT_"+code+"_V104_"+hash : "BHT_DWG_"+hash;
            using(var tr=target.TransactionManager.StartOpenCloseTransaction())
            {
                var table=(BlockTable)tr.GetObject(target.BlockTableId,OpenMode.ForRead);
                if(table.Has(name)) return name;
            }
            using(var source=new Database(false,true))
            {
                source.ReadDwgFile(path,FileOpenMode.OpenForReadAndAllShare,true,"");source.CloseInput(true);
                if(Array.IndexOf(lights,code)>=0) NormalizeLight(source);
                target.Insert(name,source,false);
            }
            return name;
        }

        // Flatten only the supplied light model in memory. Never save its source DWG.
        // Layer 0 inherits the BHT insert layer; explicit lamp colors are preserved.
        private static void FlattenLight(Entity entity, List<Entity> output, int depth, short inheritedColor, ObjectId zeroLayer)
        {
            if(depth>20) throw new InvalidOperationException("Block đèn lồng quá nhiều cấp.");
            short color=(short)entity.ColorIndex;
            if(color==0 || color==256) color=inheritedColor;
            var block=entity as BlockReference;
            if(block!=null)
            {
                var children=new DBObjectCollection();
                block.Explode(children);
                foreach(DBObject child in children)
                    using(child) { var e=child as Entity; if(e!=null) FlattenLight(e,output,depth+1,color,zeroLayer); }
                return;
            }
            if(entity is DBPoint) return;
            if(!(entity is Curve) && !(entity is Hatch))
                throw new InvalidOperationException("Mẫu đèn chứa đối tượng chưa hỗ trợ: "+entity.GetType().Name);
            var copy=(Entity)entity.Clone();
            copy.LayerId=zeroLayer; copy.ColorIndex=color;
            output.Add(copy);
        }
        private static void NormalizeLight(Database source)
        {
            var flattened=new List<Entity>();
            try
            {
                using(var tr=source.TransactionManager.StartTransaction())
                {
                    var table=(BlockTable)tr.GetObject(source.BlockTableId,OpenMode.ForRead);
                    var layers=(LayerTable)tr.GetObject(source.LayerTableId,OpenMode.ForRead);
                    var model=(BlockTableRecord)tr.GetObject(table[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
                    var old=new List<ObjectId>(); foreach(ObjectId id in model) old.Add(id);
                    foreach(ObjectId id in old) FlattenLight((Entity)tr.GetObject(id,OpenMode.ForRead),flattened,0,7,layers["0"]);
                    foreach(ObjectId id in old) tr.GetObject(id,OpenMode.ForWrite).Erase();
                    foreach(var e in flattened) {model.AppendEntity(e);tr.AddNewlyCreatedDBObject(e,true);}
                    tr.Commit();
                }
            }
            finally { foreach(var e in flattened) if(e.Database==null) e.Dispose(); }
        }
    }
}
