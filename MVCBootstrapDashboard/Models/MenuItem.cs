using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCBootstrapDashboard.Models
{
    public class MenuItem
    {
        public string JobID { get; set; }
        public int ID { get; set; }
        public string Title { get; set; }
        public int RoleID { get; set; }
        public string PageIcon { get; set; }
        public string URL { get; set; }
        public bool IsDropDown { get; set; }
        public int ParentID { get; set; } // -1 if no Parent
        public int TabID { get; set; } // -1 if no Parent
        public string WarningMessage { get; set; } // This message indicates report status
        public string WarningMessageClass { get; set; } // This message indicates report status
        public bool IsActive { get; set; }
        public string LIClass { get; set; }
        public List<MenuItem> Children { get; set; }
        public JobActivityItem ActivityJob { get; set; }
    }
}