using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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

        public ActionResult GetBooks() //getting the list of books from the library 
        {
            List<Books> LibraryList = new List<Books>();
            using (SqlConnection connection = new SqlConnection(ConnectionString)) 
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM Book";

                using (SqlCommand commend = new SqlCommand(sqlQuery, connection)) 
                {
                    SqlDataReader reader = commend.ExecuteReader();
                    while (reader.Read()) 
                    {
                        Books book = new Books
                        {
                            ISBN = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Authors = reader.GetString(2),
                            Price = reader.GetDouble(3),
                            PriceDecrease = reader.GetInt32(4),
                            Cover = reader.GetString(5),
                            Publisher = reader.GetString(6),
                            PublishYear = reader.GetDateTime(7),
                            Genre = reader.GetString(8),
                            IsBuyOnly = reader.GetBoolean(9),
                            Desrip = reader.GetString(10)
                        };
                        LibraryList.Add(book);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return View(LibraryList);
        }

        public ActionResult SingleBook() 
        {
            return View();
        }

        public ActionResult CheckOut() { return View(); }
    }
}