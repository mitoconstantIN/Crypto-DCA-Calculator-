using SQLite;
using System;

namespace CryptoDCACalculator.Models
{
    public class CryptoPrice
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Coin { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }
} 