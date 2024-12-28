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
        [HttpPost]
        public ActionResult Verify(Login acc)
        {
            // בדיקה אם שם המשתמש או הסיסמה ריקים
            if (string.IsNullOrEmpty(acc.Name) || string.IsNullOrEmpty(acc.Password))
            {
                ViewBag.Message = "Username and password are required.";
                return View("index"); // חזרה לדף ההתחברות
            }

            // בדיקה אם שם המשתמש מכיל רק מספרים
            if (System.Text.RegularExpressions.Regex.IsMatch(acc.Name, @"^\d+$"))
            {
                ViewBag.Message = "Username cannot contain only numbers. It must include letters and numbers.";
                return View("index"); // חזרה לדף ההתחברות
            }

            // בדיקה אם הסיסמה באורך 8 לפחות וכוללת אותיות ומספרים
            if (!System.Text.RegularExpressions.Regex.IsMatch(acc.Password, @"^(?=.*[A-Za-z])(?=.*\d).{8,}$"))
            {
                ViewBag.Message = "Password must be at least 8 characters long and include both letters and numbers.";
                return View("index"); // חזרה לדף ההתחברות
            }

            // אם כל הבדיקות עברו, לבצע אימות שם משתמש וסיסמה
            if (acc.Name == "admin" && acc.Password == "1234")
            {
                ViewBag.Message = "Login successful!";
                return View("Success"); // מעבר לדף הצלחה
            }
            else
            {
                ViewBag.Message = "Invalid username or password.";
                return View("index"); // חזרה לדף ההתחברות עם הודעת שגיאה
            }
        }
        // פעולה לדף ההרשמה
        public ActionResult Sign_Up()
        {
            ViewBag.Title = "Sign Up";
            return View(); // מציג את דף ההרשמה
        }
        // פעולה עבור דף ההרשמה (SignUp)
        [HttpPost]
        public ActionResult sign_up(string username, string password, string email, string role)
        {
            // בדיקות בסיסיות
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "All fields are required.";
                return View(); // חזרה לדף ההרשמה עם הודעת שגיאה
            }

            // לוגיקה לרישום משתמש חדש
            // לדוגמה: שמירת המשתמש בבסיס נתונים (כאן תצטרכי להוסיף קוד מתאים)

            // הפניה לעמוד ההתחברות אחרי הרשמה מוצלחת
            return RedirectToAction("index");
        }
        public ActionResult Forgot_password()
        {
            ViewBag.Title = "forgot_password";
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