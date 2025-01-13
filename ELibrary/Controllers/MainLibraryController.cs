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
                                Desrip = (!reader.IsDBNull(10) ? reader.GetString(10) : string.Empty),
                                AgeLimit = (!reader.IsDBNull(11) ? reader.GetInt32(11) : 0),
                                BorrowPrice = (!reader.IsDBNull(12) ? reader.GetDouble(12) : 0)
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

        public ActionResult SingleBook(string ISBN)
        {
            Books selectedBook = getBookFromDB(ISBN); //using function that get book with ISBN
            using (SqlConnection connection = new SqlConnection(ConnectionString)) //opening connection
            {
                if (selectedBook == null)
                {
                    return HttpNotFound(); // Handle cases where the book isn't found.
                }

                List<Reviews> Reviews_list = new List<Reviews>();
                using (SqlConnection connection_2 = new SqlConnection(ConnectionString))
                {
                    System.Diagnostics.Debug.WriteLine("Connection String: " + connection_2.Database); //check connection to db
                    connection_2.Open();
                    string sqlQuery_2 = "SELECT * FROM Reviews WHERE ISBN = @ISBN";
                    using (SqlCommand commend = new SqlCommand(sqlQuery_2, connection_2))
                    {
                        commend.Parameters.AddWithValue("@ISBN", ISBN);
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
                    connection_2.Close();
                }
                if (Reviews_list == null || Reviews_list.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Reviews list  is null or empty.");
                }
                System.Diagnostics.Debug.WriteLine("All Reviews Count: " + Reviews_list.Count);

                var model = new Book_and_Reviews
                {
                    book = selectedBook,
                    reviews_list = Reviews_list
                };
                return PartialView(model);
            }
        }

        public ActionResult CheckOut() 
        {
            var cartList = Session["CartItems"] as List<Books> ?? new List<Books>();
            var BorrowCartList = Session["BorrowItems"] as List<Books> ?? new List<Books>();
            var totalPrice = CalculateCartTotal(cartList, BorrowCartList);
            ViewBag.TotalPrice = totalPrice;
            return View(); 
        }

        // סינון לפי הנתונים של מנוע החיפוש
        [HttpGet]
        public JsonResult FilterBooks(string query, string filterBy, string genre)
        {
            var books = GetAllBooks();
            // חיפוש לפי הקלט של המשתמש
            if (!string.IsNullOrEmpty(query))
            {
                books = books.Where(book =>
                     book.Title.ToLower().Contains(query.ToLower()) ||
                     book.Authors.ToLower().Contains(query.ToLower())
                ).ToList();
            }

            // סינון לפי הפילטרים שנבחרו
            if (filterBy == "price")
            {
                books = books.OrderByDescending(book => book.Price).ToList();
            }
            else if (filterBy == "year")
            {
                books = books.OrderByDescending(book => book.PublishYear).ToList();
            }
            else if (filterBy == "genre" && !string.IsNullOrEmpty(genre))
            {
                books = books.Where(book => book.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Json(books, JsonRequestBehavior.AllowGet);
        }

        // פונקציה שנותנת את הספרים
        public List<Books> GetAllBooks()
        {
            List<Books> allBooks = new List<Books>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = @"
            SELECT b.ISBN, b.Title, b.Authors, b.Price, b.PriceDecrease, b.Cover, 
                   b.Publisher, b.PublishYear, b.Genre, b.IsBuyOnly, b.Desrip, b.AgeLimit, b.BorrowPrice,
                   (SELECT COUNT(*) FROM Reviews r WHERE r.ISBN = b.ISBN) AS ReviewsCount
            FROM Book b";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        allBooks.Add(new Books
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
                            BorrowPrice = (!reader.IsDBNull(12) ? reader.GetDouble(12) : 0),
                            ReviewsCount = reader.GetInt32(13) // מספר הביקורות מחושב דינמית
                        });
                    }
                }
            }
            return allBooks;
        }

        [HttpPost]
        public ActionResult AddToCart(string ISBN)
        {
            // Retrieve the cart from Session or initialize a new list
            var cartList = Session["CartItems"] as List<Books> ?? new List<Books>();

            if (FindBookFromUserDB(ISBN) != null) 
            {
                // Book is already in the library, set an error message and return to the Library page
                TempData["ErrorMessage"] = "The book is already in your library and cannot be added to the cart.";
                return RedirectToAction("Library");
            }

            Books book = getBookFromDB(ISBN);

            // Add the ISBN to the cart
            if (!cartList.Contains(book)) { cartList.Add(book); }

            // Save the cart back to Session
            Session["CartItems"] = cartList;

            // Redirect back to the Library page (or wherever the user was)
            return RedirectToAction("Library");
        }

        [HttpPost]
        public ActionResult AddToBorrowCart(string ISBN)
        {
            // Retrieve the cart from Session or initialize a new list
            var BorrowCartList = Session["BorrowItems"] as List<Books> ?? new List<Books>();

            Books book = getBookFromDB(ISBN);

            if (FindBookFromUserDB(ISBN) != null)
            {
                // Book is already in the library, set an error message and return to the Library page
                TempData["ErrorMessage"] = "The book is already in your library and cannot be added to the cart.";
                return RedirectToAction("Library");
            }

            // Add the ISBN to the cart
            if (!BorrowCartList.Contains(book)) { BorrowCartList.Add(book); }

            // Save the cart back to Session
            Session["BorrowItems"] = BorrowCartList;

            // Redirect back to the Library page (or wherever the user was)
            return RedirectToAction("Library");
        }

        public Books getBookFromDB(string ISBN)
        {
            Books book = null;
            using (SqlConnection connection = new SqlConnection(ConnectionString)) //opening connection to db
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM Book WHERE ISBN = @ISBN"; //using specific ISBN
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@ISBN", ISBN);
                    SqlDataReader reader = commend.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            book = new Books
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
                        }
                        catch (Exception ex) { Console.WriteLine($"Error reading data: {ex.Message}"); }
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return book;
        }

        [HttpPost]
        public ActionResult RemoveFromCart(string ISBN)
        {
            var cartList = Session["CartItems"] as List<Books> ?? new List<Books>();
            Books book = getBookFromDB(ISBN);
            cartList.Remove(book); //this will call for Equels in books model
            Session["CartItems"] = cartList;
            return RedirectToAction("CheckOut");
        }

        [HttpPost]
        public ActionResult RemoveFromBorrowCart(string ISBN)
        {
            var BorrowCartList = Session["BorrowItems"] as List<Books> ?? new List<Books>();
            Books book = getBookFromDB(ISBN);
            BorrowCartList.Remove(book);
            Session["BorrowItems"] = BorrowCartList;
            return RedirectToAction("CheckOut");
        }

        [HttpPost]
        public ActionResult checkout_books()
        {
            var cartItems = Session["CartItems"] as List<ELibrary.Models.Books> ?? new List<ELibrary.Models.Books>();
            var borrowItems = Session["BorrowItems"] as List<ELibrary.Models.Books> ?? new List<ELibrary.Models.Books>();
            HttpCookie userCookie = Request.Cookies["Username"];

            if (cartItems != null)
            {
                foreach (var book in cartItems)
                {
                    AddBookToUser(book.ISBN, userCookie.Value, false);
                }
                cartItems.Clear();
            }
            if (borrowItems != null)
            {
                foreach (var book in borrowItems)
                {
                    BorrowBook(book.ISBN, userCookie.Value);
                }
                borrowItems.Clear();
            }
            return RedirectToAction("CheckOut");
        }

        public void BorrowBook (string ISBN, string username) //to check book availability 
        {
            int checkCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT COUNT(*) FROM User_Library WHERE ISBN = @ISBN";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@ISBN", ISBN);
                    object result = command.ExecuteScalar();
                    checkCount = (result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                }
                connection.Close();
            }
            if (checkCount < 3 && AmountUserBorrow(username) == false) { AddBookToUser(ISBN, username, true); }
            else { AddUserToWaiting(ISBN, username); }
        }

        public void AddBookToUser(string ISBN, string username, bool IsBorrow) //to add books to user library
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "INSERT INTO User_Library (Username, ISBN, IsBorrowed, TimeBorrowed) VALUES (@Username, @ISBN, @IsBorrowed, @TimeBorrowed)";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@Username", username);
                    commend.Parameters.AddWithValue("@ISBN", ISBN);
                    commend.Parameters.AddWithValue("@IsBorrowed", IsBorrow);
                    commend.Parameters.AddWithValue("@TimeBorrowed", DateTime.Now);

                    commend.ExecuteNonQuery();//need to add catch for books that already in db
                    TempData["SuccessMessage"] = "The book was successfully added!";
                }
                connection.Close();
            }
        }

        public void AddUserToWaiting(string ISBN, string username) //to add user to the book waiting line
        {
            int maxPlaceInLine = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT MAX(PlaceInLine) AS MaxPlaceInLine FROM WaitingLine WHERE ISBN = @ISBN";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@ISBN", ISBN);

                    object result = command.ExecuteScalar();
                    maxPlaceInLine = (result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                }
                connection.Close();
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "INSERT INTO WaitingLine (ISBN, Username, PlaceInLine) VALUES (@ISBN, @Username, @PlaceInLine)";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@ISBN", ISBN);
                    commend.Parameters.AddWithValue("@Username", username);
                    commend.Parameters.AddWithValue("@PlaceInLine", maxPlaceInLine);

                    commend.ExecuteNonQuery();
                    TempData["SuccessMessage"] = "The book was successfully added!";
                }
                connection.Close();
            }
        }

        public bool AmountUserBorrow(string username) //to check that user borrow max 3 books
        {
            int count = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT COUNT(*) FROM User_Library WHERE Username = @Username AND IsBorrowed = 1";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    object result = command.ExecuteScalar();
                    count = (result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                }
            }
            if (count < 3) { return false; }
            return true;
        }

        public double CalculateCartTotal(IEnumerable<Books> cartItems, IEnumerable<Books> BorrowItems)
        {
            double total = 0;
            foreach (Books book in cartItems) 
            {
                var tmp = book.Price - (book.Price * (book.PriceDecrease * 0.01));
                if (tmp < 0) { tmp = 0; }
                if (book.PriceDecrease > 0) { total += tmp; }
                else { total += book.Price; }
            }
            foreach (Books book in BorrowItems)
            {
                var tmp = book.BorrowPrice - (book.BorrowPrice * (book.PriceDecrease * 0.01));
                if (tmp < 0) { tmp = 0; }
                if (book.PriceDecrease > 0) { total += tmp; }
                else { total += book.BorrowPrice; }
            }
            return total;
        }

        public ActionResult Payment() { return View(); }

        public ActionResult NewReview(string ISBN)
        {
            var book = getBookFromDB(ISBN);  // Assuming you have a method to fetch a book by ISBN
            var model = new Book_and_Reviews
            {
                book = book  // Passing the book to the view
            };
            return View(model);  // Passing the model to the view
        }

        [HttpPost]
        public ActionResult AddReview(string ISBN, string Stars, string Info)
        {
            // Ensure that the cookie exists for the username
            HttpCookie userCookie = Request.Cookies["Username"];
            if (userCookie == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to post a review.";
                return RedirectToAction("SingleBook", "MainLibrary", new { ISBN });
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "INSERT INTO Reviews (ISBN, Username, Stars, Info) VALUES (@ISBN, @Username, @Stars, @Info)";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@ISBN", ISBN);
                    command.Parameters.AddWithValue("@Username", userCookie.Value); // assuming cookie holds username
                    command.Parameters.AddWithValue("@Stars", Stars);
                    command.Parameters.AddWithValue("@Info", Info);

                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            TempData["SuccessMessage"] = "Review submitted successfully!";
            // Redirect back to the SingleBook page after submitting a review
            return RedirectToAction("SingleBook", "MainLibrary", new { ISBN });
        }

        public User_Library FindBookFromUserDB(string ISBN) //to check if the book is in the user library
        {
            HttpCookie userCookie = Request.Cookies["Username"];
            User_Library user_Library = null;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM User_Library WHERE Username = @Username AND ISBN = @ISBN";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@Username", userCookie.Value);
                    commend.Parameters.AddWithValue("@ISBN", ISBN);
                    SqlDataReader reader = commend.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            user_Library = new User_Library
                            {
                                Username = reader.GetString(0),
                                ISBN = reader.GetString(1),
                                IsBorrowed = reader.GetBoolean(2),
                                TimeBorrowed = reader.GetDateTime(3)
                            };
                        }
                        catch (Exception ex) { Console.WriteLine($"Error reading data: {ex.Message}"); }
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return user_Library;
        }

    }
}