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
    public class ProfileController : Controller
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionString_R"].ConnectionString;

        // GET: Profile
        public ActionResult UserProfile()
        {
            return View();
        }

        public ActionResult AdminProfile() { return View(); }

        public ActionResult AddBook(Books book) 
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "INSERT INTO Book (ISBN, Title, Authors, Price, PriceDecrease, Cover, Publisher, PublishYear, Genre, IsBuyOnly, Desrip) VALUES (@ISBN, @Title, @Authors, @Price, @PriceDecrease, @Cover, @Publisher, @PublishYear, @Genre, @IsBuyOnly, @Desrip)";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@ISBN", book.ISBN);
                    commend.Parameters.AddWithValue("@Title", book.Title);
                    commend.Parameters.AddWithValue("@Authors", book.Authors);
                    commend.Parameters.AddWithValue("@Price", book.Price);
                    commend.Parameters.AddWithValue("@PriceDecrease", book.PriceDecrease);
                    commend.Parameters.AddWithValue("@Cover", book.Cover);
                    commend.Parameters.AddWithValue("@Publisher", book.Publisher);
                    commend.Parameters.AddWithValue("@PublishYear", book.PublishYear);
                    commend.Parameters.AddWithValue("@Genre", book.Genre);
                    commend.Parameters.AddWithValue("@IsBuyOnly", book.IsBuyOnly);
                    commend.Parameters.AddWithValue("@Desrip", book.Desrip);

                    commend.ExecuteNonQuery();
                }
                connection.Close();
            }
            return View();
        }
    }
}