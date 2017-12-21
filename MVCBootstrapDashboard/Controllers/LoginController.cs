using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Services;
using MVCBootstrapDashboard.Models;

namespace MVCBootstrapDashboard.Controllers
{
    public class LoginController : ApiController
    {
        //
        // GET: /Login/

        [WebMethod]
        [System.Web.Mvc.HttpPost]
        public UserItem ISLogin(string username, string password)
        {
            using (DashboardTabsAndMenusEntities db = new DashboardTabsAndMenusEntities())
            {
                User user = db.Users.Include("Role").Where(U => U.Username == username && U.Password == password).SingleOrDefault();

                if (user == null)
                    return null;

                UserItem newUser = new UserItem()
                {
                    ID = user.ID,
                    Name = user.Name,
                    Username = user.Username,
                    Password = user.Password,
                    RoleID = user.RoleID
                };

                LogController LC = new LogController();
                LC.LogOperation(user.ID, "Login");

                return newUser;
            }
        }

    }
}
