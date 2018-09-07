using Gdc.Scd.Web.Server.Models;
using Gdc.Scd.Web.Server.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class TestController : Controller
    {
        private readonly IMessageService _service;

        public TestController(IMessageService service)
        {
            _service = service;
        }

        // GET: Test
        public ActionResult Index()
        {
            var message = _service.GetMessage();
            var model = new Message() { Mess = message };
            return View(model);
        }
    }
}