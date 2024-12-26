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
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionString_R"].ConnectionString;

        // GET: MainLibrary
        public ActionResult Library()
        {
            var libraryBooks = GetBooks();
            return View();
        }

        public ActionResult GetBooks() //getting the list of books from the library 
        {
            List<Books> LibraryList = new List<Books>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                //System.Diagnostics.Debug.WriteLine("All Books Count: " + LibraryList.Count); //check size of list
                System.Diagnostics.Debug.WriteLine("Connection String: " + connection.Database); //check connection to db
                connection.Open();
                string sqlQuery = "SELECT * FROM Book";
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
                                Desrip = (!reader.IsDBNull(10) ? reader.GetString(10) : string.Empty)
                            };
                            LibraryList.Add(book);
                        }
                        catch (Exception ex) { Console.WriteLine($"Error reading data: {ex.Message}"); }
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (LibraryList == null || LibraryList.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("LibraryList is null or empty.");
            }
            System.Diagnostics.Debug.WriteLine("All Books Count: " + LibraryList.Count);
            return View(LibraryList);
        }

        public ActionResult GetReviews() 
        {
            List<Reviews> Reviews_list = new List<Reviews>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                //System.Diagnostics.Debug.WriteLine("All Books Count: " + LibraryList.Count); //check size of list
                System.Diagnostics.Debug.WriteLine("Connection String: " + connection.Database); //check connection to db
                connection.Open();
                string sqlQuery = "SELECT * FROM Reviews";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    SqlDataReader reader = commend.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            Reviews book = new Reviews
                            {
                                ISBN = reader.GetString(0),
                                Username = reader.GetString(1),
                                Stars = reader.GetInt32(2),
                                Info = reader.GetString(3)
                            };
                            Reviews_list.Add(book);
                        }
                        catch (Exception ex) { Console.WriteLine($"Error reading data: {ex.Message}"); }
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (Reviews_list == null || Reviews_list.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Reviews list  is null or empty.");
            }
            System.Diagnostics.Debug.WriteLine("All Reviews Count: " + Reviews_list.Count);
            return View(Reviews_list);
        }

        public ActionResult SingleBook() 
        {
            return View(GetReviews());
        }

        public ActionResult CheckOut() { return View(); }
    }
}