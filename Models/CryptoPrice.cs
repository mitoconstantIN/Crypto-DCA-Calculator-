using SQLite;
using System;

namespace CryptoDCACalculator.Models
{
    public class CryptoPrice 
    {
        public string Coin { get; set; }
        public DateTime Date { get; set; }  
        public decimal Price { get; set; }
    }
} 