using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Services;
using MVCBootstrapDashboard.Models;
using System.Data.SqlClient;

namespace MVCBootstrapDashboard.Controllers
{
    public class MenusController : ApiController
    {
        Dictionary<string, JobActivityItem> JobActivities;

        //
        // GET: /Menus/

        [WebMethod]
        [System.Web.Mvc.HttpGet]
        public List<MenuItem> GetMenus(int p_iTabID, int iRoleID)
        {
            List<MenuItem> lstMenuItems = new List<MenuItem>();

            //try
            //{
            //    JobActivities = GetJobActivities();
            //}
            //catch
            //{
            //    JobActivities = new Dictionary<string, JobActivityItem>();
            //}

            JobActivities = new Dictionary<string, JobActivityItem>();

            using (DashboardTabsAndMenusEntities db = new DashboardTabsAndMenusEntities())
            {
                List<Menu> lstMenu = db.Menus.Where(U => U.TabID == p_iTabID && U.RoleID >= iRoleID && U.IsActive == true).OrderBy(u => u.Ordering).ThenBy(u => u.ID).ToList();

                // Get All Root Menus
                foreach (Menu item in lstMenu)
                {
                    // If the Role of user is not Admin, and Role of tab is Managment
                    if ((iRoleID > 2 && iRoleID != 5) && item.RoleID == 5)
                        continue;

                    if (item.ParentID == -1)
                    {
                        MenuItem newItem = new MenuItem();

                        newItem.ID = item.ID;
                        newItem.Title = item.Title;
                        newItem.RoleID = item.RoleID;
                        newItem.ParentID = item.ParentID;
                        newItem.TabID = item.TabID;
                        newItem.IsActive = item.IsActive;
                        newItem.JobID = item.JobID;

                        if (newItem.JobID != null)
                        {
                            SetMessage(ref newItem);
                        }


                        // No need to this Line cause there is no direct reports, all the reports are under menues
                        //if (newItem.JobID != null)
                        //    newItem.ActivityJob = JobActivities[newItem.JobID];

                        if (item.URL != "#" && item.URL[0] != '/') // old Report
                            newItem.URL = item.URL;
                        else // new Report Or Dropdown Menu
                            newItem.URL = item.URL;
    
                        newItem.PageIcon = item.PageIcon;
                        newItem.IsDropDown = item.IsDropDown;
                        if (item.IsDropDown)
                            newItem.LIClass = "mm-dropdown ng-scope mm-dropdown-root";
                        else
                            newItem.LIClass = "";

                        GetAllChildren(lstMenu, ref newItem, iRoleID);

                        lstMenuItems.Add(newItem);
                    }
                }
            }
            // ToDo: we need to:
            //  1. Get the Warning Message from the DB depend on report status
            return lstMenuItems;
        }

        private void GetAllChildren(List<Menu> lstMenu, ref MenuItem newItem, int iRoleID)
        {
            if (newItem.IsDropDown == false)
                return;

            newItem.Children = new List<MenuItem>();

            int iParentID = newItem.ID;

            List<Menu> lstChildren = lstMenu.Where(U => U.ParentID == iParentID && U.RoleID >= iRoleID && U.IsActive == true).OrderBy(u => u.Ordering).ThenBy(u => u.ID).ToList();

            foreach (Menu item in lstChildren)
            {
                // If the Role of user is not Admin, and Role of tab is Managment
                if ((iRoleID > 2 && iRoleID != 5) && item.RoleID == 5)
                    continue;

                MenuItem newChildItem = new MenuItem();

                newChildItem.ID = item.ID;
                newChildItem.Title = item.Title;
                newChildItem.RoleID = item.RoleID;
                newChildItem.ParentID = item.ParentID;
                newChildItem.TabID = item.TabID;
                newChildItem.IsActive = item.IsActive;
                newChildItem.JobID = item.JobID;

                if (newChildItem.JobID != null)
                {
                    SetMessage(ref newChildItem);
                }

                if (item.URL[0] != '/') // old Report
                    newChildItem.URL = item.URL;
                else // new Report
                    newChildItem.URL = string.Format("{0}://{1}{2}", Request.RequestUri.Scheme, Request.Headers.Host, item.URL);

                newChildItem.PageIcon = item.PageIcon;
                newChildItem.IsDropDown = item.IsDropDown;

                //GetAllChildren(lstMenu, ref newChildItem);

                newItem.Children.Add(newChildItem);
            }
        }

        private Dictionary<string, JobActivityItem> GetJobActivities()
        {
            Dictionary<string, JobActivityItem> allJobs = new Dictionary<string, JobActivityItem>();

            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = "User Id=sa;Password=sa;Data Source=10.123.105.193";
                con.Open();

                SqlCommand command = con.CreateCommand();
                string sql = @"use RAReporting EXEC msdb..sp_help_job";

                command.CommandText = sql;

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    JobActivityItem newItem = new JobActivityItem();

                    string jobID = newItem.name = reader.GetGuid(0).ToString().ToUpper();

                    newItem.name = reader.GetString(2);
                    newItem.enabled = Convert.ToBoolean(reader.GetByte(3));
                    newItem.last_ran_outcome = (RanOutcome)reader.GetInt32(21);


                    string date = reader.GetInt32(19).ToString(); //last_ran_date
                    string time = reader.GetInt32(20).ToString(); //last_ran_time

                    if (date == "0")
                    {
                        newItem.last_ran_datetime = null;
                    }
                    else
                    {
                        date = date.PadLeft(8, '0');
                        time = time.PadLeft(6, '0');
                        newItem.last_ran_datetime = DateTime.ParseExact(date + time, "yyyyMMddHHmmss", null);
                    }


                    date = reader.GetInt32(22).ToString(); //next_ran_date
                    time = reader.GetInt32(23).ToString(); //next_ran_time

                    if (date == "0")
                    {
                        newItem.next_ran_datetime = null;
                    }
                    else
                    {
                        date = date.PadLeft(8, '0');
                        time = time.PadLeft(6, '0');
                        newItem.next_ran_datetime = DateTime.ParseExact(date + time, "yyyyMMddHHmmss", null);
                    }

                    // for more info, please visit: https://clinthuijbers.wordpress.com/2011/06/29/check-current_execution_status-on-sql-agent-job-waitfor/
                    int current_execution_status = reader.GetInt32(25);
                    if (current_execution_status == 4)
                        newItem.isExecuting = false;
                    else
                        newItem.isExecuting = true;

                    allJobs.Add(jobID, newItem);
                }
            }

            return allJobs;
        }

        private void SetMessage(ref MenuItem menue)
        {
            menue.ActivityJob = JobActivities[menue.JobID];
            if (menue.ActivityJob.isExecuting == true)
            {
                //menue.WarningMessage = "Executing";
                menue.WarningMessageClass = "label label-info fa fa-refresh";
                //menue.WarningMessageClass = "fa fa-refresh";
            }
            else
            {
                if (menue.ActivityJob.next_ran_datetime != null && menue.ActivityJob.next_ran_datetime.Value.DayOfYear == DateTime.Now.DayOfYear &&
                    menue.ActivityJob.next_ran_datetime.Value.Ticks > DateTime.Now.Ticks)
                {
                    //menue.WarningMessage = "Pending";
                    menue.WarningMessageClass = "label label-warning fa fa-circle-o";
                    //menue.WarningMessageClass = "fa fa-circle-o";
                }
                else
                {
                    if (menue.ActivityJob.last_ran_outcome == RanOutcome.Succeeded)
                    {
                        //menue.WarningMessage = "Succeeded";
                        menue.WarningMessageClass = "label label-success fa fa-check-circle";
                        //menue.WarningMessageClass = "fa fa-check-circle";
                    }
                    else if (menue.ActivityJob.last_ran_outcome == RanOutcome.Error_Failed || menue.ActivityJob.last_ran_outcome == RanOutcome.Cancelled || menue.ActivityJob.last_ran_outcome == RanOutcome.Status_Unknown)
                    {
                        //menue.WarningMessage = "Failed";
                        menue.WarningMessageClass = "label label-danger fa fa-times-circle";
                        //menue.WarningMessageClass = "fa fa-times-circle";
                    }
                    else if (menue.ActivityJob.last_ran_outcome == RanOutcome.In_Progress || menue.ActivityJob.last_ran_outcome == RanOutcome.Retry)
                    {
                        //menue.WarningMessage = "Executing";
                        menue.WarningMessageClass = "label label-info fa fa-refresh";
                        //menue.WarningMessageClass = "fa fa-refresh";
                    }
                    else // Unknown
                    {
                        //menue.WarningMessage = "Unknown";
                        menue.WarningMessageClass = "label label-info fa fa-question-circle";
                        //menue.WarningMessageClass = "fa fa-refresh"; 
                    }
                }
            }
        }
    }
}
