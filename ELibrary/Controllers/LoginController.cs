using ELibrary.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net;
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

        //כדי לשלוח את הקוד לאימייל לאחר שהמשתמש מזין את המייל 
        [HttpPost]
        public JsonResult Forgot_Password(string email)
        {
            try
            {
                // בדיקה אם האימייל קיים בבסיס הנתונים
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        int count = (int)command.ExecuteScalar();

                        if (count == 0)
                        {
                            return Json(new { success = false, message = "Email address not found." });
                        }
                    }
                }

                // יצירת קוד רנדומלי
                Random random = new Random();
                int randomCode = random.Next(100000, 999999); // קוד בן 6 ספרות

                // שמירת הקוד ב-Session
                Session["ResetCode"] = randomCode;
                Session["ResetEmail"] = email;

                // שליחת הקוד למייל
                SendEmail(email, "Reset Your Password", $"Your reset code is: {randomCode}");

                return Json(new { success = true, message = "A reset code has been sent to your email." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Forgot password: {ex.Message}");
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }
        //הוספת פונקציה חדשה שתוודא שהקוד הוזן על ידי המשתמש 
        [HttpPost]
        public JsonResult VerifyResetCode(int enteredCode)
        {
            try
            {
                // בדיקה אם הקוד קיים ב-Session
                if (Session["ResetCode"] != null && Session["ResetEmail"] != null)
                {
                    int correctCode = (int)Session["ResetCode"];
                    if (enteredCode == correctCode)
                    {
                        // הקוד נכון
                        return Json(new { success = true, message = "Code verified successfully." });
                    }
                    else
                    {
                        // הקוד שגוי
                        return Json(new { success = false, message = "Invalid reset code." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Session expired. Please request a new code." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VerifyResetCode: {ex.Message}");
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }
        //יצירת סיסמה חדשה אם הקוד עבר בהצלחה אז נוכל לפנות את המשתמש לעמוד שבו הוא יעשה איפוס סיסמה
        public ActionResult ResetPasswordPage()
        {
            if (Session["ResetEmail"] == null)
            {
                // אם ה-Session לא קיים, חזור לעמוד הראשי
                return RedirectToAction("Forgot_password");
            }

            ViewBag.Email = Session["ResetEmail"].ToString();
            return View();
        }
        //עדכון סיסמה 
        [HttpPost]
        public JsonResult ResetPassword(string newPassword)
        {
            try
            {
                if (Session["ResetEmail"] != null)
                {
                    string email = Session["ResetEmail"].ToString();

                    // עדכון הסיסמה בבסיס הנתונים
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string sqlQuery = "UPDATE Users SET Password = @Password WHERE Email = @Email";
                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Password", newPassword);
                            command.Parameters.AddWithValue("@Email", email);
                            command.ExecuteNonQuery();
                        }
                    }

                    // ניקוי ה-Session
                    Session.Remove("ResetCode");
                    Session.Remove("ResetEmail");

                    return Json(new { success = true, message = "Password has been reset successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Session expired. Please request a new code." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ResetPassword: {ex.Message}");
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }

        //כדי לשלוח את המייל הוספת פונקציה SMTP 
        private void SendEmail(string to, string subject, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("your-email@gmail.com"); // כתובת השולח
                mail.To.Add(to); // כתובת הנמען
                mail.Subject = subject; // נושא המייל
                mail.Body = body; // תוכן המייל

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587); // עדכון שרת ופורט
                client.Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password"); // אימות
                client.EnableSsl = true; // חיבור מאובטח

                client.Send(mail); // שליחת המייל
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw; // ניתן להוסיף טיפול שגיאות נוסף כאן
            }
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