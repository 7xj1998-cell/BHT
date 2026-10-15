using System;
using System.Windows.Forms;
using System.Linq;

namespace BHT.Palette
{
    public partial class BhtPaletteControl
    {
        private Button _signScaleButton;

        private void RefreshSignScale()
        {
            if (_signScaleButton == null) return;
            _signScaleButton.Enabled = _doc != null && _lispOk && !_lispRunning;
            if (_svc == null) return;
            string symbol = _svc.Meta("sign_scale", _svc.Meta("kh_scale", "1"));
            string label = _svc.Meta("sign_label_scale", symbol);
            _objectTips.SetToolTip(_signScaleButton, "Biển / đèn × " + symbol + "; nhãn × " + label + ". Chọn riêng hai tỷ lệ, áp dụng cho biển báo, bảng chỉ dẫn, bảng quảng cáo và bảng thuộc nhóm Khác/Chưa xác định, đèn tín hiệu và đèn chiếu sáng trong bản vẽ.");
        }

        private void ChooseSignScale()
        {
            if (_lispRunning || !NeedDoc() || !NeedLisp()) return;
            KeepObjectScroll(() => {
                string symbol = _svc.Meta("sign_scale", _svc.Meta("kh_scale", "1"));
                using (var options = new SignScaleForm(symbol, _svc.Meta("sign_label_scale", symbol), _svc.Meta("sign_label_orientation", "PARALLEL")))
                {
                    if (options.ShowDialog(this) != DialogResult.OK) return;
                    CallLisp("bht:api-sign-display", new[] { options.SymbolScale, options.LabelScale, options.LabelOrientation }, "Đổi tỷ lệ và hướng nhãn báo hiệu", null);
                }
            });
        }

        private void ChooseSymbolDistance()
        {
            if(_lispRunning || !NeedDoc() || !NeedLisp()) return;
            // Choosing a layout reference does not reclassify the route used for chainage.
            var records=_svc.Ordered("ROUTE").ToArray();
            var routes=records.Select(p=>p.Key).ToArray();
            var labels=records.Select(p=>p.Key+" — "+SymbolDistanceForm.RouteTypeLabel(p.Value.Get("loai"))).ToArray();
            string selected=_svc.Meta("symbol_center_route","");
            if(!routes.Contains(selected)&&routeList!=null&&routeList.SelectedItems.Count>0)
                selected=routeList.SelectedItems[0].Text;
            string method=_svc.Meta("symbol_layout_method",routes.Length>0?"CENTER":"ROAD");
            bool center=method=="CENTER"&&routes.Length>0,outside=method!="RADIAL";
            string distance=_svc.Meta(center?"symbol_center_distance":outside?"symbol_road_clearance":"symbol_gap",center?"":outside?"1":"2.8");
            using(var form=new SymbolDistanceForm(distance,_svc.Meta("symbol_leader_mode","GAP_KHUC"),_svc.Meta("symbol_road_left",""),_svc.Meta("symbol_road_right",""),outside,routes,selected,center,labels))
                if(form.ShowDialog(this)==DialogResult.OK)
                    if(!string.IsNullOrEmpty(form.ReferenceRoute))
                        CallLisp("bht:api-symbol-center",new[]{form.ReferenceRoute,form.Distance,form.LeaderMode},"Bố trí báo hiệu theo tim "+form.ReferenceRoute,reply=>{
                            if(reply.Ok) foreach(var line in reply.Values) _doc.Editor.WriteMessage("\n"+line);
                        });
                    else if(form.OutsideRoad)
                        CallLisp("bht:api-symbol-road",new[]{form.Distance,form.LeftWidth,form.RightWidth,form.LeaderMode},"Đưa báo hiệu ra ngoài đường: chọn PL và đầu PL trong CAD",reply=>{
                            if(reply.Ok) foreach(var line in reply.Values) _doc.Editor.WriteMessage("\n"+line);
                        });
                    else CallLisp("bht:api-symbol-display",new[]{form.Distance,form.LeaderMode},"Đổi cách đặt tất cả báo hiệu",null);
        }
    }
}
