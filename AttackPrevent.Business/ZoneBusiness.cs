using AttackPrevent.Access;
using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AttackPrevent.Business
{
    public class ZoneBusiness
    {
        public static List<ZoneEntity> GetAllList()
        {
            return ZoneAccess.GetAllList();
        }

        public static List<SelectListItem> GetZoneSelectList()
        {
            List<SelectListItem> zonelist = new List<SelectListItem>() {
                new SelectListItem() {Value="111",Text="ent.comm100.com"},
                new SelectListItem() {Value="222",Text="hosted.comm100.com"},
                new SelectListItem() {Value="333",Text="app.comm100.com"},
            };

            //List<SelectListItem> zonelist = new List<SelectListItem>();
            //List<ZoneEntity> alllist = GetAllList();

            //alllist.ForEach(item => {
            //    zonelist.Add(new SelectListItem() { Value=item.ZoneId,Text=item.ZoneName});
            //});


            return zonelist;
        }
    }
}
