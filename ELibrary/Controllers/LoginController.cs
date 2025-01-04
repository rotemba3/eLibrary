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
    public class LoginController : Controller
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionString_R"].ConnectionString;

        // GET: Login
        public ActionResult Login()
        {
            ViewBag.Title = "Login";
            return View();
        }

        [HttpPost] //verify Credentials
        public ActionResult Verify_Login(string username, string password)
        {
            Users user = null;
            using (SqlConnection connection = new SqlConnection(ConnectionString)) //getting the user from the database
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM User WHERE Username = @Username AND Password = @Password";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@Username", username);
                    commend.Parameters.AddWithValue("@Password", password);
                    SqlDataReader reader = commend.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            user = new Users()
                            {
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                Email = reader.GetString(3),
                                IsAdmin = reader.GetBoolean(4)
                            };
                        }
                        catch (Exception ex) { Console.WriteLine($"Error reading data: {ex.Message}"); }
                    }
                    reader.Close();
                }
                connection.Close();
            }
            // בדיקה אם שם המשתמש או הסיסמה ריקים
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Username and password are required.";
                return View(); // חזרה לדף ההתחברות
            }

            // בדיקה אם שם המשתמש מכיל רק מספרים
            if (System.Text.RegularExpressions.Regex.IsMatch(username, @"^\d+$"))
            {
                ViewBag.Message = "Username cannot contain only numbers. It must include letters and numbers.";
                return View("Login"); // חזרה לדף ההתחברות
            }

            // בדיקה אם הסיסמה באורך 8 לפחות וכוללת אותיות ומספרים
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*\d).{8,}$"))
            {
                ViewBag.Message = "Password must be at least 8 characters long and include both letters and numbers.";
                return View("Login"); // חזרה לדף ההתחברות
            }

            // אם כל הבדיקות עברו, לבצע אימות שם משתמש וסיסמה
            if (user != null)
            {
                ViewBag.Message = "Login successful!";
                return View("HomePage"); // מעבר לדף הצלחה
            }
            else
            {
                ViewBag.Message = "Invalid username or password.";
                return View("Login"); // חזרה לדף ההתחברות עם הודעת שגיאה
            }
        }

        //register user to db
        [HttpPost]
        public ActionResult Sign_Up_click(string username, string password, string email, string IsAdmin)
        {
            // בדיקות בסיסיות
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "All fields are required.";
                return View(); // חזרה לדף ההרשמה עם הודעת שגיאה
            }

            // לוגיקה לרישום משתמש חדש
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sqlQuery = "INSERT INTO Users (Username, Password, Email, IsAdmin) VALUES (@Username, @Password, @Email, @IsAdmin)";
                using (SqlCommand commend = new SqlCommand(sqlQuery, connection))
                {
                    commend.Parameters.AddWithValue("@Username", username);
                    commend.Parameters.AddWithValue("@Password", password);
                    commend.Parameters.AddWithValue("@Email", email);
                    if (IsAdmin == "Admin")
                        commend.Parameters.AddWithValue("@IsAdmin", true);
                    else
                        commend.Parameters.AddWithValue("@IsAdmin", false);
                    commend.ExecuteNonQuery();
                }
                connection.Close();
            }

            // הפניה לעמוד ההתחברות אחרי הרשמה מוצלחת
            return RedirectToAction("Login");
        }

        public ActionResult Sign_Up() //register to site 
        {
            ViewBag.Title = "Sign_Up";
            return View();
        }

        public ActionResult Forgot_password()
        {
            ViewBag.Title = "Forgot_password";
            return View();
        }

        public ActionResult Register()
        {
            return View("sign_up");
        }

        public ActionResult ForgotPassword()
        {
            return View("forgot_password");
        }
    }
}