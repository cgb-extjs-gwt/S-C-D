using System.Web.Http;
using System.Web.Mvc;

namespace Gdc.Scd.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return File("~/index.html", "text/html");
        }
    }
}