using System;
using System.Collections.Generic;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Web;

namespace ELibrary.Models
{
    public class Users
    {
        public string Username {  get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class User_Library 
    {
        public string Username { get; set; }
        public string ISBN { get; set; }
        public bool IsBorrowed { get; set; }
        public DateTime TimeBorrowed { get; set; }
    }

    public class WaitingLine 
    {
        public string ISBN { get; set; }
        public string Username { get; set; }
        public int PlaceInLine { get; set; }
    }

    public class User_Profile_info
    {
        public string user { get; set; }
        public List<Reviews> reviews { get; set; }
        public List<Books> personal_books { get; set; }
        public List<WaitingLine> waiting_lists { get; set; }
        public List<User_Library> UserLibrary { get; set; }
    }
}