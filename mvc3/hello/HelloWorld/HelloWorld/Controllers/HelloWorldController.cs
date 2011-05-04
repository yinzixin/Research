using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelloWorld.Controllers
{
    public class HelloWorldController : Controller
    { 

        public string Index()
        {
            return "Hello world";
        }

        public string Hello()
        {
            return "Hello everyone";
        }

        public string Hello2(string name)
        {
            return "Hello to you " + name;
        }
    }
}
