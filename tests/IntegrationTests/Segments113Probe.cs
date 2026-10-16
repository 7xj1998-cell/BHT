using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using BHT.Core;
using BHT.Bridge;
using BHT.Palette;
using AcApp=Autodesk.AutoCAD.ApplicationServices.Core.Application;

public class Segments113Probe
{
 static int count,fail;
 static string undoBefore;
 static IEnumerable<Control> Children(Control root){foreach(Control c in root.Controls){yield return c;foreach(var child in Children(c))yield return child;}}
 static void Render(Form form,string file){form.StartPosition=FormStartPosition.Manual;form.Location=new System.Drawing.Point(-4000,-4000);form.Show();Application.DoEvents();using(var bitmap=new System.Drawing.Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new System.Drawing.Rectangle(System.Drawing.Point.Empty,bitmap.Size));bitmap.Save(Path.Combine(Dir,file));}form.Close();}
 static string Dir {get{return Path.GetDirectoryName(typeof(Segments113Probe).Assembly.Location);}}
 static void Check(string name,bool ok){count++;if(!ok)fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+(ok?"PASS":"FAIL")+" SEG113 "+name);}
 static Database Open(string path){var db=new Database(false,true);db.ReadDwgFile(path,FileOpenMode.OpenForReadAndAllShare,true,"");db.CloseInput(true);return db;}
 static string Snapshot(Database db,bool excludeSegments){
  using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
   var lines=new List<string>();var root=BhtStore.RootDict(tr,db,false);
   if(root!=null)foreach(DBDictionaryEntry sub in root)if(tr.GetObject(sub.Value,OpenMode.ForRead) is DBDictionary&&(!excludeSegments||sub.Key!="SEG"&&sub.Key!="SEG_ALIAS"))
    lines.AddRange(BhtStore.All(tr,db,sub.Key).Select(p=>sub.Key+"/"+p.Key+"="+p.Value.ToCanonical()));
   lines.AddRange(BhtStore.Points(tr,db).Select(p=>"PT="+p.Id+"|"+p.X.ToString("R")+"|"+p.Y.ToString("R")+"|"+p.Z.ToString("R")));
   var bt=(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
   foreach(ObjectId block in bt){var b=(BlockTableRecord)tr.GetObject(block,OpenMode.ForRead);foreach(ObjectId id in b){
    var e=tr.GetObject(id,OpenMode.ForRead) as Entity;if(e==null)continue;
    string s="CAD="+e.Handle+"|"+b.Name+"|"+e.GetType().FullName+"|"+e.Layer;
    var c=e as Curve;if(c!=null){try{s+="|"+c.StartPoint+"|"+c.EndPoint+"|"+c.GetDistanceAtParameter(c.EndParam);for(int i=0;i<9;i++)s+="|"+c.GetPointAtParameter(c.StartParam+(c.EndParam-c.StartParam)*i/8.0);}catch(Autodesk.AutoCAD.Runtime.Exception ex){s+="|CURVE_SAMPLE_ERROR="+ex.ErrorStatus;}}
    var p=e as Polyline;if(p!=null)for(int i=0;i<p.NumberOfVertices;i++)s+="|"+p.GetPoint3dAt(i)+"|"+p.GetBulgeAt(i);
    var t=e as DBText;if(t!=null)s+="|"+t.TextString+"|"+t.Position+"|"+t.Rotation+"|"+t.Height;
    var m=e as MText;if(m!=null)s+="|"+m.Contents+"|"+m.Location+"|"+m.Rotation+"|"+m.TextHeight;
    var r=e as BlockReference;if(r!=null)s+="|"+r.Position+"|"+r.Rotation+"|"+r.ScaleFactors+"|"+((BlockTableRecord)tr.GetObject(r.BlockTableRecord,OpenMode.ForRead)).Name;
    var pt=e as DBPoint;if(pt!=null)s+="|"+pt.Position;
    using(var x=e.XData)if(x!=null)s+="|"+string.Join(";",x.AsArray().Select(v=>v.TypeCode+":"+v.Value));lines.Add(s);
   }}return string.Join("\n",lines.OrderBy(s=>s,StringComparer.Ordinal));
  }
 }
 static void Fixture(Database db,string id,string package,string route){using(var tr=db.TransactionManager.StartTransaction()){
  BhtStore.Write(tr,db,"SEG",id,new BhtRecord().Set("segment_id",id).Set("package_id",package).Set("package_name","Same package").Set("km_start_m","1000").Set("km_end_m","2000").Set("side","PHAI").Set("route_id",route));tr.Commit();}}
 [CommandMethod("SEGMENTS113PROBE")]
 public static void Run(){try{
  string input=Environment.GetEnvironmentVariable("BHT113_INPUT");
  foreach(var file in Directory.GetFiles(input,"*.bht").OrderBy(s=>s))using(var package=ProjectPackage.Open(file))using(var db=Open(package.DrawingPath)){
   string name=Path.GetFileNameWithoutExtension(file),all=Snapshot(db,false),protectedBefore=Snapshot(db,true);
   int old=SegmentStore.Read(db).Count;bool undo=db.UndoRecording;
   var plan=SegmentDuplicates.Preview(db);Check(name+"-read-only-preview",all==Snapshot(db,false));
   File.WriteAllLines(Path.Combine(Dir,name+"-plan.txt"),plan.Lines);
   if(name=="DOAN15"){
    var type=typeof(BhtPaletteControl).Assembly.GetType("BHT.Palette.SegmentDuplicateReviewForm");using(var form=(Form)Activator.CreateInstance(type,new object[]{plan}))Render(form,"DOAN15-review.png");
   }
   if(plan.Blocked>0){
    string unchanged=Snapshot(db,false);bool rejected=false;
    try{SegmentDuplicates.Apply(db,plan);}catch(InvalidOperationException){rejected=true;}
    Check(name+"-ambiguous-real-references-block-all",name!="DOAN15"&&rejected&&unchanged==Snapshot(db,false)&&SegmentStore.Read(db).Count==old);
    continue;
   }
   Check(name+"-expected-plan",plan.Removable==(old==30?15:0)&&plan.Blocked==0);
   int n=SegmentDuplicates.Apply(db,plan);Check(name+"-fifteen-segments",n==old-15&&SegmentStore.Read(db).Count==15);
   Check(name+"-all-survey-history-RTK-CAD-unchanged",protectedBefore==Snapshot(db,true));
   Check(name+"-undo-mode-restored",undo==db.UndoRecording);
   string after=Snapshot(db,false);Check(name+"-repeated-cleanup-no-op",SegmentDuplicates.Apply(db,SegmentDuplicates.Preview(db))==0&&after==Snapshot(db,false));
   using(var tr=db.TransactionManager.StartOpenCloseTransaction()){
    Check(name+"-journal-count",BhtStore.All(tr,db,"SEG_ALIAS").Count==n);
    if(old==30){Check(name+"-old-confirmed-segment-retained",BhtStore.Read(tr,db,"SEG","DOAN08")!=null&&BhtStore.Read(tr,db,"SEG","DOAN_08")==null);
     Check(name+"-old-locked-objects-retained",BhtStore.All(tr,db,"OBJ").Count(p=>p.Value.Get("doan")=="DOAN08"&&ProgressPolicy.Locked(p.Value))==15);}
   }
   string drawing=Path.Combine(Dir,name+"-15doan.dwg");db.SaveAs(drawing,DwgVersion.AC1032);
   using(var reopened=Open(drawing)){Check(name+"-saved-reopened-identical",after==Snapshot(reopened,false));
    Check(name+"-reopen-cleanup-idempotent",SegmentDuplicates.Apply(reopened,SegmentDuplicates.Preview(reopened))==0);
   }
   if(name=="DOAN15"){
    ProjectPackage.Write(Path.Combine(Dir,"DOAN15-15doan.bht"),drawing);
    using(var repaired=ProjectPackage.Open(Path.Combine(Dir,"DOAN15-15doan.bht")))using(var reopened=Open(repaired.DrawingPath))Check("D15-repacked-verified-identical",after==Snapshot(reopened,false));
    bool blocked=false;try{ProjectTransfer.ImportRolling(db,file,true);}catch(ImportConflictException ex){blocked=ex.Conflicts.Any(c=>c.Field=="segment_id");}
    Check("D15-old-doubled-package-cannot-reintroduce-rows",blocked&&after==Snapshot(db,false));
    using(var target=new Database(true,true)){
     string repaired=Path.Combine(Dir,"DOAN15-15doan.bht");int copied=ProjectTransfer.ImportRolling(target,repaired);
     using(var tr=target.TransactionManager.StartOpenCloseTransaction())Check("D15-repaired-import-retains-all-objects-points-journal",copied>0&&BhtStore.All(tr,target,"OBJ").Count==179&&BhtStore.Points(tr,target).Count==801&&BhtStore.All(tr,target,"SEG").Count==15&&BhtStore.All(tr,target,"SEG_ALIAS").Count==15);
     string imported=Snapshot(target,false);Check("D15-repaired-import-repeat-no-duplicates",ProjectTransfer.ImportRolling(target,repaired)==0&&imported==Snapshot(target,false));
    }
   }
  }
  using(var package=ProjectPackage.Open(Path.Combine(input,"DOAN9.bht")))using(var db=Open(package.DrawingPath)){
   string before=Snapshot(db,false);bool blocked=false;
   try{ProjectTransfer.ImportRolling(db,Path.Combine(input,"DOAN10.bht"),true);}catch(ImportConflictException ex){blocked=ex.Conflicts.Any(c=>c.Field=="segment_id");}
   Check("real-D9-D10-no-doubling",blocked&&SegmentStore.Read(db).Count==15&&before==Snapshot(db,false));
   blocked=false;try{ProjectTransfer.CompareRolling(db,Path.Combine(input,"DOAN10.bht"));}catch(ImportConflictException ex){blocked=ex.Conflicts.Any(c=>c.Field=="segment_id");}
   Check("rolling-preview-and-execution-same-alias-policy",blocked&&before==Snapshot(db,false));
   blocked=false;try{ProjectTransfer.Import(db,Path.Combine(input,"DOAN10.bht"),true,true);}catch(ImportConflictException ex){blocked=ex.Conflicts.Any(c=>c.Field=="segment_id");}
   Check("survey-import-same-alias-policy",blocked&&before==Snapshot(db,false));
  }
  using(var db=new Database(true,true)){
   Fixture(db,"DOAN08","GOI03","R08");Fixture(db,"DOAN_08","GOI_03","");
   var plan=SegmentDuplicates.Preview(db);
   using(var tr=db.TransactionManager.StartTransaction()){
    BhtStore.Write(tr,db,"META","CONFIG",new BhtRecord().Set("edited_after_preview","yes"));tr.Commit();
   }
   string before=Snapshot(db,false);
   bool stale=false;try{SegmentDuplicates.Apply(db,plan);}catch{stale=true;}
   Check("stale-preview-no-delete",stale&&before==Snapshot(db,false)&&SegmentStore.Read(db).Count==2);
  }
  using(var db=new Database(true,true)){
   Fixture(db,"DOAN08","GOI03","");Fixture(db,"DOAN_08","GOI_03","");
   Fixture(db,"DOAN09","GOI03","");Fixture(db,"DOAN_09","GOI_03","");
   var preview=SegmentDuplicates.Preview(db);string before=Snapshot(db,false);bool threw=false;
   // Test the same native transaction contract with a fault after journal and
   // deletion, without throwing across AutoCAD's unmanaged event boundary.
   db.DisableUndoRecording(false);
   try{using(var outer=db.TransactionManager.StartTransaction()){
    Check("cleanup-completed-inside-outer-transaction",SegmentDuplicates.Apply(db,preview)==2&&SegmentStore.Read(db).Count==2);
    throw new InvalidOperationException("Injected caller failure after cleanup before outer commit");
   }}catch(InvalidOperationException){threw=true;}
   Check("caller-failure-restores-records-journal",threw&&before==Snapshot(db,false));
   before=Snapshot(db,false);threw=false;
   try{using(var tr=db.TransactionManager.StartTransaction()){
    BhtStore.Write(tr,db,"SEG_ALIAS","FAULT",new BhtRecord().Set("removed_id","DOAN08"));
    BhtStore.Delete(tr,db,"SEG","DOAN08");
    Check("fault-reached-after-first-deletion",BhtStore.Read(tr,db,"SEG","DOAN08")==null);
    throw new InvalidOperationException("Injected fault between cleanup writes");
   }}catch(InvalidOperationException){threw=true;}
   Check("mid-write-failure-rolls-back-journal-and-segment",threw&&before==Snapshot(db,false));
  }
  using(var db=new Database(true,true)){
   Fixture(db,"DOAN08","GOI03","");Fixture(db,"DOAN_08","GOI_03","");
   using(var tr=db.TransactionManager.StartTransaction()){BhtStore.Write(tr,db,"META","CONFIG",new BhtRecord().Set("progress_active_segment","DOAN08"));tr.Commit();}
   string before=Snapshot(db,true);
   Check("meta-reference-retains-used-code",SegmentDuplicates.Apply(db,SegmentDuplicates.Preview(db))==1&&SegmentStore.Read(db).Single().Get("segment_id")=="DOAN08"&&before==Snapshot(db,true));
   using(var tr=db.TransactionManager.StartTransaction()){
    var journal=BhtStore.All(tr,db,"SEG_ALIAS").Single();bool write=false,delete=false;
    try{BhtStore.Write(tr,db,"SEG_ALIAS",journal.Key,journal.Value.Clone().Set("reason","tampered"));}catch(InvalidOperationException){write=true;}
    try{BhtStore.Delete(tr,db,"SEG_ALIAS",journal.Key);}catch(InvalidOperationException){delete=true;}
    Check("journal-append-only",write&&delete);tr.Commit();
   }
  }
  foreach(string mode in new[]{"both-used","route-conflict","history","release"})using(var db=new Database(true,true)){
   Fixture(db,"DOAN08","GOI03",mode=="route-conflict"?"R08":"");Fixture(db,"DOAN_08","GOI_03",mode=="route-conflict"?"OTHER":"");
   using(var tr=db.TransactionManager.StartTransaction()){
    if(mode=="both-used"){BhtStore.Write(tr,db,"OBJ","A",new BhtRecord().Set("doan","DOAN08"));BhtStore.Write(tr,db,"OBJ","B",new BhtRecord().Set("doan","DOAN_08"));}
    if(mode=="history"||mode=="release")BhtStore.Write(tr,db,mode=="history"?"HISTORY":"RELEASES","OLD",new BhtRecord().Set("snapshot","opaque"));
    tr.Commit();
   }
   string before=Snapshot(db,false);bool blocked=false;
   try{SegmentDuplicates.Apply(db,SegmentDuplicates.Preview(db));}catch(InvalidOperationException){blocked=true;}
   Check(mode+"-blocked-atomically",blocked&&before==Snapshot(db,false));
  }
  using(var db=new Database(true,true)){
   Fixture(db,"DOAN08","GOI03","");string before=Snapshot(db,false);bool blocked=false;
   var rows=new[]{new BhtRecord().Set("segment_id","DOAN_08").Set("package_id","GOI_03").Set("package_name","Same package").Set("km_start_m","1000").Set("km_end_m","2000").Set("side","PHAI").Set("route_id","")};
   try{SegmentStore.Apply(db,rows,"",true);}catch(ImportConflictException){blocked=true;}
   Check("Excel-table-replace-alias-blocked",blocked&&before==Snapshot(db,false));
  }
  using(var db=new Database(true,true))using(var other=new Database(true,true)){
   Fixture(db,"DOAN08","GOI03","");Fixture(db,"DOAN_08","GOI_03","");
   string before=Snapshot(other,false);bool blocked=false;
   try{SegmentDuplicates.Apply(other,SegmentDuplicates.Preview(db));}catch(InvalidOperationException){blocked=true;}
   Check("other-drawing-preview-blocked",blocked&&before==Snapshot(other,false));
  }
  using(var db=new Database(true,true)){
   Fixture(db,"DOAN08","GOI03","");Fixture(db,"DOAN_08","GOI_03","");
   var type=typeof(BhtPaletteControl).Assembly.GetType("BHT.Palette.SegmentDuplicateReviewForm");
   using(var form=(Form)Activator.CreateInstance(type,new object[]{SegmentDuplicates.Preview(db)})){
    form.StartPosition=FormStartPosition.Manual;form.Location=new System.Drawing.Point(-4000,-4000);form.Show();Application.DoEvents();Check("review-form-visible",form.Visible&&form.MinimumSize.Width>=650);
    using(var bitmap=new System.Drawing.Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new System.Drawing.Rectangle(System.Drawing.Point.Empty,bitmap.Size));bitmap.Save(Path.Combine(Dir,"segment-review.png"));}form.Close();
   }
  }
 }catch(System.Exception ex){fail++;AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nSEG113_ERROR "+ex);}
  AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nSEG113_TOTAL="+count+" FAIL="+fail);
 }
 [CommandMethod("SEG113UNDOSEED")]public static void UndoSeed(){var db=AcApp.DocumentManager.MdiActiveDocument.Database;Fixture(db,"DOAN08","GOI03","");Fixture(db,"DOAN_08","GOI_03","");undoBefore=Snapshot(db,false);}
 [CommandMethod("SEG113UNDOAPPLY")]public static void UndoApply(){
  var doc=AcApp.DocumentManager.MdiActiveDocument;int phase=0,ticks=0;
  if(System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToLowerInvariant().Contains("core")){
   Check("command-cleanup",SegmentDuplicates.Apply(doc.Database,SegmentDuplicates.Preview(doc.Database))==1&&SegmentStore.Read(doc.Database).Count==1);return;
  }
  using(var timer=new Timer{Interval=100}){
   timer.Tick+=(s,e)=>{
    if(ticks<4)File.AppendAllText(Path.Combine(Dir,"table-command-trace.txt"),"tick="+ticks+" phase="+phase+" forms="+string.Join(" | ",Application.OpenForms.Cast<Form>().Select(f=>f.GetType().Name+":"+f.Text+":"+string.Join(";",Children(f).Select(c=>c.Text))))+"\r\n");
    if(++ticks>150){timer.Stop();foreach(var form in Application.OpenForms.Cast<Form>().ToArray()){form.DialogResult=DialogResult.Cancel;form.Close();}return;}
    var review=Application.OpenForms.Cast<Form>().FirstOrDefault(f=>f.GetType().Name=="SegmentDuplicateReviewForm");
    if(review!=null&&phase==1){phase=2;Children(review).OfType<Button>().Single(b=>b.Text.StartsWith("Xác nhận gom")).PerformClick();return;}
    var table=Application.OpenForms.Cast<Form>().OfType<SegmentTableForm>().FirstOrDefault();
    if(table==null)return;
    if(phase==0){phase=1;Children(table).OfType<Button>().Single(b=>b.Text.StartsWith("Kiểm tra / gom")).PerformClick();}
    else if(phase==2){phase=3;Check("table-reloads-after-cleanup",Children(table).OfType<DataGridView>().Single().Rows.Count==1);timer.Stop();table.DialogResult=DialogResult.Cancel;table.Close();}
   };
   timer.Start();SegmentTableCommands.Open(doc,false);timer.Stop();
  }
  Check("actual-table-review-confirm-cleanup",phase==3&&SegmentStore.Read(doc.Database).Count==1);
 }
 [CommandMethod("SEG113UNDOVERIFY")]public static void UndoVerify(){Check("one-undo-restores-segments-and-journal",undoBefore==Snapshot(AcApp.DocumentManager.MdiActiveDocument.Database,false));string result="SEG113_UNDO="+(fail==0?"PASS":"FAIL");File.WriteAllText(Path.Combine(Dir,"undo-result-"+System.Diagnostics.Process.GetCurrentProcess().ProcessName+".txt"),result);AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+result);}
}
