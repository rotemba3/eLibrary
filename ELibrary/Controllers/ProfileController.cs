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
            var model = new User_Profile_info();
            HttpCookie userCookie = Request.Cookies["Username"];
            if (userCookie != null)
            {
                model = new User_Profile_info()
                {
                    user = userCookie.Value,
                    reviews = GetReviews(userCookie.Value),
                    personal_books = GetPersonalBooks(userCookie.Value),
                    waiting_lists = GetWaitingLists(userCookie.Value),
                    UserLibrary = GetUserLibrary(userCookie.Value)
                };
            }
            return View(model);
        }

        public ActionResult AdminProfile() { return View(); }

        public ActionResult AddBook(Books book)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "INSERT INTO Book (ISBN, Title, Authors, Price, PriceDecrease, Cover, Publisher, PublisherYear, Genre, IsBuyOnly, Derip) VALUES (@ISBN, @Title, @Authors, @Price, @PriceDecrease, @Cover, @Publisher, @PublisherYear, @Genre, @IsBuyOnly, @Derip)";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@ISBN", book.ISBN);
                    commend.Parameters.AddWithValue("@Title", book.Title);
                    commend.Parameters.AddWithValue("@Authors", book.Authors);
                    commend.Parameters.AddWithValue("@Price", book.Price);
                    commend.Parameters.AddWithValue("@PriceDecrease", book.PriceDecrease);
                    commend.Parameters.AddWithValue("@Cover", book.Cover);
                    commend.Parameters.AddWithValue("@Publisher", book.Publisher);
                    commend.Parameters.AddWithValue("@PublisherYear", book.PublishYear);
                    commend.Parameters.AddWithValue("@Genre", book.Genre);
                    commend.Parameters.AddWithValue("@IsBuyOnly", book.IsBuyOnly);
                    commend.Parameters.AddWithValue("@Derip", book.Desrip);
                    //בדיקה אם הערך של היום תקין
                    if (book.PublishYear < new DateTime(1753, 1, 1) || book.PublishYear > DateTime.Now)
                    {
                        throw new Exception("Invalid Publish Date. The date must be between 1753 and today.");
                    }

                    commend.ExecuteNonQuery();
                    TempData["SuccessMessage"] = "The book was successfully added!";
                }
                connection.Close();
            }
            return View("AdminProfile");
        }

        public List<Reviews> GetReviews(string currentUser)
        {
            List<Reviews> Reviews_list = new List<Reviews>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                System.Diagnostics.Debug.WriteLine("Connection String: " + connection.Database); //check connection to db
                connection.Open();
                string sqlQuery = "SELECT * FROM Reviews WHERE Username = @Username";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@Username", currentUser);
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
            return Reviews_list;
        }

        public List<Books> GetPersonalBooks(string currentUser) 
        {
            List<Books> Personal_books = new List<Books>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                System.Diagnostics.Debug.WriteLine("Connection String: " + connection.Database); //check connection to db
                connection.Open();
                string sqlQuery = "SELECT Book.ISBN, Book.Title, Book.Authors, Book.Price, Book.PriceDecrease,Book.Cover, Book.Publisher, Book.PublishYear, Book.Genre, Book.IsBuyOnly, Book.Desrip FROM Book INNER JOIN User_Library ON User_Library.ISBN = Book.ISBN WHERE User_Library.Username = @Username";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@Username", currentUser);
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

        public List<WaitingLine> GetWaitingLists(string currentUser) 
        {
            List<WaitingLine> waitingLines = new List<WaitingLine>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                System.Diagnostics.Debug.WriteLine("Connection String: " + connection.Database); //check connection to db
                connection.Open();
                string sqlQuery = "SELECT * FROM WaitingLine WHERE Username = @Username";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@Username", currentUser);
                    SqlDataReader reader = commend.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            WaitingLine line = new WaitingLine
                            {
                                ISBN = reader.GetString(0),
                                Username = reader.GetString(1),
                                PlaceInLine = reader.GetInt32(2)
                            };
                            waitingLines.Add(line);
                        }
                        catch (Exception ex) { Console.WriteLine($"Error reading data: {ex.Message}"); }
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (waitingLines == null || waitingLines.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("waitingLines is null or empty.");
            }
            System.Diagnostics.Debug.WriteLine("All lines Count: " + waitingLines.Count);
            return waitingLines;
        }

        public List<User_Library> GetUserLibrary(string currentUser) 
        {
            List<User_Library> user_Libraries = new List<User_Library>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                System.Diagnostics.Debug.WriteLine("Connection String: " + connection.Database); //check connection to db
                connection.Open();
                string sqlQuery = "SELECT * FROM User_Library WHERE Username = @Username";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@Username", currentUser);
                    SqlDataReader reader = commend.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            User_Library book = new User_Library
                            {
                                Username = reader.GetString(0),
                                ISBN = reader.GetString(1),
                                IsBorrowed = reader.GetBoolean(2),
                                TimeBorrowed = reader.GetDateTime(3)
                            };
                            user_Libraries.Add(book);
                        }
                        catch (Exception ex) { Console.WriteLine($"Error reading data: {ex.Message}"); }
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (user_Libraries == null || user_Libraries.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("user_Libraries is null or empty.");
            }
            System.Diagnostics.Debug.WriteLine("All books Count: " + user_Libraries.Count);
            return user_Libraries;
        }

        public ActionResult RemoveBook(string ISBN) //Remove book from db
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "DELETE FROM Book WHERE ISBN = @ISBN";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@ISBN", ISBN);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return RedirectToAction("AdminProfile");
        }

        [HttpPost]
        public ActionResult RemoveBookFromUser(string ISBN) //when user wants to remove book from his library
        {
            string userCookie = Request.Cookies["Username"].Value;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "DELETE FROM User_Library WHERE ISBN = @ISBN AND Username = @Username ";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@ISBN", ISBN);
                    command.Parameters.AddWithValue("@Username", userCookie);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return RedirectToAction("UserProfile");
        }
    }
}