using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ELibrary.Models
{
    public class Books
    {
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public double Price { get; set; }
        public int PriceDecrease { get; set; }
        public string Cover { get; set; }
        public string Publisher { get; set; }
        public DateTime PublishYear { get; set; }
        public string Genre { get; set; }
        public bool IsBuyOnly { get; set; }
        public string Desrip { get; set; }
        public int ReviewsCount { get; set; } // מספר הביקורות מחושב מתוך Reviews


        // Override Equals to compare ISBN
        public override bool Equals(object obj)
        {
            if (obj is Books other)
            {
                return this.ISBN == other.ISBN;
            }
            return false;
        }

        // Override GetHashCode to align with Equals
        public override int GetHashCode()
        {
            return ISBN.GetHashCode();
        }
    }

    public class Reviews
    {
        public string ISBN { get; set; }
        public string Username { get; set; }
        public int Stars { get; set; }
        public string Info { get; set; }
    }

    public class Book_and_Reviews 
    {
        public Books book { get; set; }
        public List<Reviews> reviews_list { get; set; }
    }
}