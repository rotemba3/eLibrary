using ELibrary.Models;
using Microsoft.SqlServer.Server;
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

        //בדיקה אם אדמין והעברה לדף המותאם 
        public ActionResult User_Profile()
        {
            var model = new User_Profile_info();
            HttpCookie userCookie = Request.Cookies["Username"];
            if (userCookie != null)
            {
                string username = userCookie.Value;

                // בדיקת האם המשתמש הוא אדמין
                bool isAdmin = IsUserAdmin(username);

                if (isAdmin)
                {
                    // הפניה לעמוד האדמין
                    return RedirectToAction("AdminProfile", "Profile");
                }
                else
                {
                    // אם הוא משתמש רגיל, טעני את המידע הרגיל
                    model = new User_Profile_info()
                    {
                        user = username,
                        reviews = GetReviews(username),
                        personal_books = GetPersonalBooks(username),
                        waiting_lists = GetWaitingLists(username),
                        UserLibrary = GetUserLibrary(username)
                    };
                }
            }
            else
            {
                // אם אין קוקי, הפניה לעמוד התחברות
                return RedirectToAction("NoUserProfile", "Profile");
            }

            return RedirectToAction("UserProfile", "Profile");
        }

        //בדיקה לאדמין 
        private bool IsUserAdmin(string username)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT IsAdmin FROM Users WHERE Username = @Username";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    object result = command.ExecuteScalar();
                    return result != null && Convert.ToBoolean(result);
                }
            }
        }
        
        public ActionResult AdminProfile() { return View(); }

        public ActionResult AddBook(Books book)
        {
            //checking values
            var allowedGenres = new List<string> { "fantasy", "sci-fi", "romance", "non-fiction", "horror" };
            if (!allowedGenres.Contains(book.Genre.ToLower()))
            {
                TempData["ErrorMessage5"] = "Invalid Genre. Please select one of the following: Fantasy, Sci-fi, Romance, Non-Fiction, Horror.";
                return View("AdminProfile");
            }

            if (string.IsNullOrEmpty(book.ISBN) || book.ISBN.Length != 13)
            {
                TempData["ErrorMessage5"] = "ISBN must be exactly 13 characters long.";
                return View("AdminProfile");
            }

            if (book.Price <= 0)
            {
                TempData["ErrorMessage5"] = "Price must be a positive number (float).";
                return View("AdminProfile");
            }

            if (book.PriceDecrease < 0 || book.PriceDecrease > 100)
            {
                TempData["ErrorMessage5"] = "PriceDecrease must be an integer between 0 and 100.";
                return View("AdminProfile");
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "INSERT INTO Book (ISBN, Title, Authors, Price, PriceDecrease, Cover, Publisher, PublisherYear, Genre, IsBuyOnly, Desrip, AgeLimit, BorrowPrice) VALUES (@ISBN, @Title, @Authors, @Price, @PriceDecrease, @Cover, @Publisher, @PublisherYear, @Genre, @IsBuyOnly, @Desrip, @AgeLimit, @BorrowPrice)";
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
                    commend.Parameters.AddWithValue("@Desrip", book.Desrip);
                    commend.Parameters.AddWithValue("@AgeLimit", book.AgeLimit);
                    commend.Parameters.AddWithValue("@BorrowPrice", book.BorrowPrice);
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
                string sqlQuery = "SELECT Book.ISBN, Book.Title, Book.Authors, Book.Price, Book.PriceDecrease,Book.Cover, Book.Publisher, Book.PublishYear, Book.Genre, Book.IsBuyOnly, Book.Desrip, book.AgeLimit, book.BorrowPrice FROM Book INNER JOIN User_Library ON User_Library.ISBN = Book.ISBN WHERE User_Library.Username = @Username";
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
                                Desrip = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                                AgeLimit = (!reader.IsDBNull(11) ? reader.GetInt32(11) : 0),
                                BorrowPrice = (!reader.IsDBNull(12) ? reader.GetDouble(12) : 0)
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

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "The book was successfully removed!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Book not found or could not be removed.";
                    }
                }
                connection.Close();
            }
            return RedirectToAction("AdminProfile");
        }

        [HttpPost]
        public ActionResult UpdatePrice(string bookId, decimal newPrice) //update price
        {
            if (newPrice <= 0)
            {
                TempData["ErrorMessage5"] = "Price must be a positive number (float).";
                return View("AdminProfile");
            }

            if (string.IsNullOrWhiteSpace(bookId) || newPrice <= 0)
            {
                TempData["ErrorMessage1"] = "Invalid book ID or price.";
                return RedirectToAction("AdminProfile");
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE Book SET Price = @NewPrice WHERE ISBN = @BookId";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@BookId", bookId.Trim());
                    command.Parameters.AddWithValue("@NewPrice", newPrice);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage1"] = "The book's price was successfully updated!";
                    }
                    else
                    {
                        TempData["ErrorMessage1"] = "Book not found. Price could not be updated.";
                    }
                }
                connection.Close();
            }

            return RedirectToAction("AdminProfile");
        }

        [HttpPost]
        public ActionResult AddPromotion(string bookId, int discount) //add Sale
        {
            if (string.IsNullOrWhiteSpace(bookId) || discount < 0 || discount > 100)
            {
                TempData["ErrorMessage1"] = "Invalid book ID or discount value.";
                return RedirectToAction("AdminProfile");
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE Book SET PriceDecrease = @Discount WHERE ISBN = @BookId";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@BookId", bookId.Trim());
                    command.Parameters.AddWithValue("@Discount", discount);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage1"] = "Promotion successfully added!";
                    }
                    else
                    {
                        TempData["ErrorMessage1"] = "Book not found. Promotion could not be added.";
                    }
                }
                connection.Close();
            }

            return RedirectToAction("AdminProfile");
        }

        [HttpPost]
        public ActionResult SetPurchaseOnly(string bookId) //change book to price only
        {
            if (string.IsNullOrWhiteSpace(bookId))
            {
                TempData["ErrorMessage3"] = "Invalid book ID.";
                return RedirectToAction("AdminProfile");
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE Book SET IsBuyOnly = 1 WHERE ISBN = @BookId";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@BookId", bookId.Trim());

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage3"] = "The book was successfully marked as purchase only!";
                    }
                    else
                    {
                        TempData["ErrorMessage3"] = "Book not found. Could not update the status.";
                    }
                }
                connection.Close();
            }

            return RedirectToAction("AdminProfile");
        }

        [HttpPost]
        public ActionResult ManageUsers(string username) //user to admin
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["ErrorMessage2"] = "Invalid user ID.";
                return RedirectToAction("AdminProfile");
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE Users SET IsAdmin = 1 WHERE Username = @Username";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Username", username.Trim());

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage2"] = "The user was successfully promoted to admin!";
                    }
                    else
                    {
                        TempData["ErrorMessage2"] = "User not found. Could not update the role.";
                    }
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

        public ActionResult NoUserProfile() { return View(); }

        [HttpPost]
        public ActionResult DownloadBook(string BookFormat)
        {
            // Logic to serve the requested file based on the selected format
            string filePath = "";

            // Logic to determine the file path based on format
            switch (BookFormat.ToUpper())
            {
                case "PDF":
                    filePath = Server.MapPath("~/Formats/BookFormat.pdf"); // Adjust the path as needed
                    break;
                case "FB2":
                    filePath = Server.MapPath("~/Formats/BookFormat.fb2"); // Adjust the path as needed
                    break;
                case "MOBI":
                    filePath = Server.MapPath("~/Formats/BookFormat.mobi"); // Adjust the path as needed
                    break;
                case "EPUB":
                    filePath = Server.MapPath("~/Formats/BookFormat.epub"); // Adjust the path as needed
                    break;
                default:
                    return new HttpStatusCodeResult(400, "Invalid format selected");
            }

            // Check if the file exists
            if (System.IO.File.Exists(filePath))
            {
                return File(filePath, "application/octet-stream", $"book.{BookFormat.ToLower()}");
            }
            else
            {
                return new HttpStatusCodeResult(404, "File not found in the selected format");
            }
        }

    }
}