using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return File("~/Content/index.html", "text/html");
        }
    }
}
