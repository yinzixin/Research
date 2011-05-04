using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelloWorld.Controllers
{
    public class HelloController : Controller
    {
        //
        // GET: /Hello/

        public ActionResult Index()
        {
            ViewBag.Count = 4;
            return View();
        }
    }
}
