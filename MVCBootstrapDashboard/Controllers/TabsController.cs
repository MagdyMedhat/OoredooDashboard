using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.Http;
using MVCBootstrapDashboard.Models;

namespace MVCBootstrapDashboard.Controllers
{
    public class TabsController : ApiController 
    {
        // GET: /Tabs/
        [WebMethod]
        [System.Web.Mvc.HttpGetAttribute]
        public List<TabItem> GetMainTabs(int iRoleID)
        {
            List<TabItem> lstTabItems = new List<TabItem>();
            using (DashboardTabsAndMenusEntities db = new DashboardTabsAndMenusEntities())
            {
                List<Tab> lstTab = db.Tabs.Where(U => U.RoleID >= iRoleID && U.IsActive == true).ToList();

                foreach (Tab item in lstTab)
                {
                    // If the Role of user is not Admin, and Role of tab is Managment
                    if ((iRoleID > 2 && iRoleID != 5) && item.RoleID == 5)
                        continue;

                    TabItem newItem = new TabItem();
                    newItem.ID = item.ID;
                    newItem.Title = item.Title;
                    newItem.RoleID = item.RoleID;
                    newItem.IsActive = item.IsActive;

                    lstTabItems.Add(newItem);
                }
            }
            return lstTabItems;
        }
    }
}
