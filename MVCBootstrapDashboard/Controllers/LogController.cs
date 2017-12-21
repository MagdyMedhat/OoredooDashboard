using MVCBootstrapDashboard.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Services;
using System.Diagnostics;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace MVCBootstrapDashboard.Controllers
{
    public class LogController : ApiController
    {
        // GET: /Log/

        [WebMethod]
        [System.Web.Mvc.HttpPost]
        public bool LogOperation(int iUserID, string strOperation)
        {
            using (DashboardTabsAndMenusEntities db = new DashboardTabsAndMenusEntities())
            {
                DateTime TimeStamp = DateTime.Now;  

                Log newLog = new Log()
                {
                    UserID = iUserID,
                    Operation = strOperation,
                    Date = TimeStamp
                };

                db.Logs.Add(newLog);

                if (db.SaveChanges() == -1)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
