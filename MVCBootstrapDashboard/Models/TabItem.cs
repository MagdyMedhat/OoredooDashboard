using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCBootstrapDashboard.Models
{
    public class TabItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public int RoleID { get; set; }
        //public bool IsDropDown { get; set; }
        //public int ParentID { get; set; } // -1 if no Parent
        //public string WarningMessage { get; set; } // This message indicates report status
        public bool IsActive { get; set; }
    }
}