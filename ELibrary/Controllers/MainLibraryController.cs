using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELibrary.Controllers
{
    public class MainLibraryController : Controller
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["deafultConnectionString_R"].ConnectionString;

        // GET: MainLibrary
        public ActionResult HomePage()
        {
            return View();
        }

        public ActionResult InfoBook() 
        {
            return View(); 
        }
    }
}