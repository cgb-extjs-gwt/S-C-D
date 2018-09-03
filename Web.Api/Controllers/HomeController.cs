using System.Web.Mvc;

namespace Gdc.Scd.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return File("~/wwwroot/index.html", "text/html");
        }
    }
}