using ELibrary.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELibrary.Controllers
{
    public class HomeController : Controller
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionString_R"].ConnectionString;
        // GET: Home
        public ActionResult Index()
        {
            ViewBag.Title = "Index";
            return View();
        }
        public ActionResult HomePage()
        {
            List<Books> book_list = new List<Books>(); //שליפת ספרים מומלצים במבצע
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                System.Diagnostics.Debug.WriteLine("Connection String: " + connection.Database); //check connection to db
                connection.Open();
                string sqlQuery = "SELECT * FROM Book WHERE PriceDecrease > 0";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    SqlDataReader reader = commend.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            Books book = new Books
                            {
                                ISBN = reader.GetString(0),
                                Title = reader.GetString(1),
                                Authors = reader.GetString(2),
                                Price = reader.GetDouble(3),
                                PriceDecrease = (!reader.IsDBNull(4) ? reader.GetInt32(4) : 0),
                                Cover = reader.GetString(5),
                                Publisher = reader.GetString(6),
                                PublishYear = reader.GetDateTime(7),
                                Genre = reader.GetString(8),
                                IsBuyOnly = reader.GetBoolean(9),
                                Desrip = (!reader.IsDBNull(10) ? reader.GetString(10) : string.Empty),
                                AgeLimit = (!reader.IsDBNull(11) ? reader.GetInt32(11) : 0),
                                BorrowPrice = (!reader.IsDBNull(12) ? reader.GetDouble(12) : 0)
                            };
                            book_list.Add(book);
                        }
                        catch (Exception ex) { Console.WriteLine($"Error reading data: {ex.Message}"); }
                    }
                    reader.Close();
                }
                connection.Close();
                System.Diagnostics.Debug.WriteLine("All Books Count: " + book_list.Count);

                //הצגת 6 ספרים מומלצים במבצע כל פעם
                Random random = new Random(); 
                List<Books> Feature_books = book_list.OrderBy(x => random.Next()).Take(6).ToList();

                return View(Feature_books);
            }
        }

        [HttpGet]
        public JsonResult SearchAutoComplete(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            List<object> foundBooks = new List<object>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT Title, Authors, Cover, ISBN FROM Book WHERE Title LIKE @Query OR Authors LIKE @Query\r\n";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Query", $"%{query}%"); // חיפוש בטוח מ-SQL Injection
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        foundBooks.Add(new
                        {
                            Title = reader.GetString(0),
                            Authors = reader.GetString(1),
                            Cover = reader.GetString(2),
                            ISBN = reader.GetString(3) 
                        });
                    }
                }
            }
            return Json(foundBooks.Take(10), JsonRequestBehavior.AllowGet); // הגבלת התוצאות ל-10
        }

    }
}