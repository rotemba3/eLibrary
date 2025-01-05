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
        public JsonResult Verify_Login()
        {
            using (var reader = new System.IO.StreamReader(Request.InputStream))
            {
                var json = reader.ReadToEnd();
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Login>(json);

                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                {
                    return Json(new { success = false, message = "Username or password cannot be empty." });
                }

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT Username FROM Users WHERE Username = @Username AND Password = @Password";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", model.Username);
                        command.Parameters.AddWithValue("@Password", model.Password);
                        var Username = command.ExecuteScalar(); // השאילתה מחזירה את שם המשתמש
                        string usernameString = Username as string;
                        if (usernameString != null)
                        {
                            // שמירת שם המשתמש בקוקי
                            HttpCookie userCookie = new HttpCookie("Username");
                            userCookie.Value = usernameString; // שם המשתמש
                            userCookie.HttpOnly = true; // מונע גישה מ-JavaScript
                            userCookie.Secure = true; // שימוש רק ב-HTTPS
                            userCookie.Expires = DateTime.Now.AddDays(7); // תוקף לשבוע
                            Response.Cookies.Add(userCookie);

                            return Json(new { success = true, username = usernameString });
                        }
                        else
                        {
                            Console.WriteLine("Invalid username or password.");
                            return Json(new { success = false, message = "Invalid username or password." });
                        }
                    }
                }
            }
        }

        //בודקת האם המשתמש האם ה COOKIE של המשתמש קיים ואז אפשר לזהות אותו
        [HttpGet] //verify Credentials
        public JsonResult IsUserLoggedIn()
        {
            try
            {
                // בדיקה אם קיים קוקי בשם "Username"
                HttpCookie userCookie = Request.Cookies["Username"];
                if (userCookie != null)
                {
                    string username = userCookie.Value;

                    // בדיקת שם המשתמש בבסיס הנתונים
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string sqlQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Username", username);

                            // בדיקה אם שם המשתמש קיים בבסיס הנתונים
                            object result = command.ExecuteScalar();
                            int count = result != null ? Convert.ToInt32(result) : 0;

                            if (count > 0)
                            {
                                return Json(new { isLoggedIn = true, username = username }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // טיפול בשגיאות
                Console.WriteLine($"Error in IsUserLoggedIn: {ex.Message}");

                // החזרת הודעת שגיאה למשתמש עם סטטוס מתאים
                Response.StatusCode = 500; // סטטוס שגיאה פנימית
                return Json(new { isLoggedIn = false, error = "An error occurred while checking login status." }, JsonRequestBehavior.AllowGet);
            }

            // אם הקוקי לא קיים או אם שם המשתמש אינו נמצא בבסיס הנתונים
            return Json(new { isLoggedIn = false }, JsonRequestBehavior.AllowGet);
        }

        //כדי לבצע יציאה מחק את הקוקי גם כן מהשרת
        public ActionResult Logout()
        {
            try
            {
                // מחיקת הקוקי מהלקוח
                if (Request.Cookies["Username"] != null)
                {
                    HttpCookie cookie = new HttpCookie("Username");
                    cookie.Expires = DateTime.Now.AddDays(-1); // תוקף לפוג תוקף
                    Response.Cookies.Add(cookie);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Logout: {ex.Message}");
                return Json(new { success = false, message = "Error during logout." });
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
            return View("Forgot_password");
        }
    }
}