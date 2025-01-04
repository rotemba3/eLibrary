using ELibrary.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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

        public ActionResult GetReviews(Users currentUser)
        {
            List<Reviews> Reviews_list = new List<Reviews>();
            List<Books> Books_list = new List<Books>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                System.Diagnostics.Debug.WriteLine("Connection String: " + connection.Database); //check connection to db
                connection.Open();
                string sqlQuery = "SELECT * FROM Reviews WHERE Username = @Username";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@Username", currentUser.Username);
                    SqlDataReader reader = commend.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            Reviews review = new Reviews
                            {
                                ISBN = reader.GetString(0),
                                Username = reader.GetString(1),
                                Stars = reader.GetInt32(2),
                                Info = reader.GetString(3)
                            };
                            Reviews_list.Add(review);
                            //to get a book according to user
                            string sqlQuery_2 = "SELECT * FROM Books WHERE ISBN = @ISBN";
                            using (SqlCommand commend2 = new SqlCommand(sqlQuery_2, connection))
                            {
                                commend2.Parameters.AddWithValue("@ISBN", review.ISBN);
                                SqlDataReader reader2 = commend2.ExecuteReader();
                                while (reader.Read())
                                {
                                    Books book = new Books
                                    {
                                        ISBN = reader2.GetString(0),
                                        Title = reader2.GetString(1),
                                        Authors = reader2.GetString(2),
                                        Price = reader2.GetDouble(3),
                                        PriceDecrease = reader2.IsDBNull(4) ? 0 : reader2.GetInt32(4),
                                        Cover = reader2.GetString(5),
                                        Publisher = reader2.GetString(6),
                                        PublishYear = reader2.GetDateTime(7),
                                        Genre = reader2.GetString(8),
                                        IsBuyOnly = reader2.GetBoolean(9),
                                        Desrip = reader2.IsDBNull(10) ? string.Empty : reader2.GetString(10)
                                    };
                                    Books_list.Add(book);
                                }
                                reader2.Close();
                            }
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

            var model = new User_Profile_info()
            {
                user = currentUser,
                reviews = Reviews_list,
                books = Books_list,
                waiting_lists = null,
                profile_library = null
            };
            return PartialView(model);
        }

        public List<Books> GetPersonalBooks(Users currentUser) 
        {
            List<Books> Personal_books = new List<Books>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                System.Diagnostics.Debug.WriteLine("Connection String: " + connection.Database); //check connection to db
                connection.Open();
                string sqlQuery = "SELECT * FROM User_Library, Book WHERE Username = @Username AND User_Library.ISBN = Book.ISBN";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@Username", currentUser.Username);
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
                                PriceDecrease = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                Cover = reader.GetString(5),
                                Publisher = reader.GetString(6),
                                PublishYear = reader.GetDateTime(7),
                                Genre = reader.GetString(8),
                                IsBuyOnly = reader.GetBoolean(9),
                                Desrip = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
                            };
                            Personal_books.Add(book);
                        }
                        catch (Exception ex) { Console.WriteLine($"Error reading data: {ex.Message}"); }
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (Personal_books == null || Personal_books.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Personal_books is null or empty.");
            }
            System.Diagnostics.Debug.WriteLine("All Books Count: " + Personal_books.Count);
            return Personal_books;
        } 
    }
}