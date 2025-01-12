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
//using Microsoft.Extensions.Options;
//using Microsoft.Extensions.DependencyInjection;
using System.Net.Configuration;

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
                            // מחיקת קוקי ישן (אם קיים)
                            if (Request.Cookies["Username"] != null)
                            {
                                HttpCookie oldCookie = new HttpCookie("Username");
                                oldCookie.Expires = DateTime.Now.AddDays(-1); // פג תוקף מיידי
                                Response.Cookies.Add(oldCookie);
                            }

                            // יצירת קוקי חדש
                            HttpCookie userCookie = new HttpCookie("Username");
                            userCookie.Value = usernameString; // שם המשתמש
                            userCookie.HttpOnly = true; // מונע גישה מ-JavaScript
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
                    Console.WriteLine($"Cookie Username (raw): {userCookie.Value}");
                    Console.WriteLine($"Cookie Username (trimmed): {username}");

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
        //זה הלמטה שכחתי סיסמה 
        public ActionResult Forgot_password()
        {
            ViewBag.Title = "Forgot_password";
            return View();
        }

        private void SendEmail(string to, string subject, string body)
        {

            try
            {
                // Retrieve SMTP settings from web.config
                var smtpSection = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;

                if (smtpSection == null)
                    throw new Exception("SMTP settings are missing in the web.config file.");

                using (var client = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port))
                {
                    client.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                    client.EnableSsl = smtpSection.Network.EnableSsl;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpSection.From),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true // Set to true if sending HTML content
                    };

                    mailMessage.To.Add(to);

                    client.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Log exception or handle as needed
                throw new Exception("An error occurred while sending the email: " + ex.Message);
            }
        }
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
                Console.WriteLine($"Error in Forgot_password: {ex.Message}");
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }
        public ActionResult VerifyCode()
        {
            return View("VerifyCode");
        }
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

        public ActionResult Resetpasswordview()
        {
            return View("~/Views/Login/Reset_password.cshtml");
        }

        public ActionResult Register()
        {
            return View("sign_up");
        }

        public ActionResult ForgotPassword() { return View("Forgot_password"); }


    }
}