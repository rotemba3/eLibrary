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
        public int ISBN { get; set; }
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
    }
}