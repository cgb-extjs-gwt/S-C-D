using System.Web.Mvc;

namespace Gdc.Scd.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return File("~/Content/index.html", "text/html");
        }

        [HttpPost]
        public ActionResult TestAjax(FormCollection form)
        {
            return JavaScript("<script>alert(\"some message\")</script>");
        }
    }
}