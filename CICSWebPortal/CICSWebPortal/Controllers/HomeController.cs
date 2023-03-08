using CICSWebPortal.Infrastructure;
using CICSWebPortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CICSWebPortal.Controllers
{
    [CustomAuthorize]
    public class HomeController : Controller
    {
        private IDataService DataContext;

        public HomeController():this(MainContainer.DataService())
        {

        }

        public HomeController(IDataService DataContext)
        {
            this.DataContext = DataContext;
        }

        //Get Dashboard
        public ActionResult Index()
        {
            var roleId = Session["RoleId"];
            var userId = Session["UserId"];
            return View(DataContext.GetDashBoardSummary(Convert.ToInt32(roleId),Convert.ToInt32(userId)));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}