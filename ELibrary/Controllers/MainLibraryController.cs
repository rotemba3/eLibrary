using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ELibrary.Models;

namespace ELibrary.Controllers
{
    public class MainLibraryController : Controller
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["deafultConnectionString_R"].ConnectionString;

        // GET: MainLibrary
        public ActionResult Library()
        {
            return View();
        }

        public ActionResult InfoBook() 
        {
            Books books_list = new Books();
            return View(books_list); 
        }

        public ActionResult SingleBook() 
        {
            return View();
        }

        public ActionResult CheckOut() { return View(); }
    }
}