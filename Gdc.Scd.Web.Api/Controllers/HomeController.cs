using Microsoft.AspNetCore.Mvc;

namespace Gdc.Scd.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return File("~/index.html", "text/html");
        }
    }
}