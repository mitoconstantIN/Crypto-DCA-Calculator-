using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CryptoDCACalculator.Models;

namespace CryptoDCACalculator.Services
{
    public class DcaDays
    {
        [PrimaryKey]
        public int Id { get; set; } = 1;
        public string DaysCsv { get; set; }
    }

    public class DatabaseService
    {
        private SQLiteAsyncConnection _db;
        public async Task InitAsync(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<CryptoPrice>();
            await _db.CreateTableAsync<DcaDays>();
        }

        public Task<int> SavePriceAsync(CryptoPrice price) => _db.InsertOrReplaceAsync(price);
        public Task<CryptoPrice> GetPriceAsync(string coin, DateTime date) => _db.Table<CryptoPrice>().Where(x => x.Coin == coin && x.Date == date).FirstOrDefaultAsync();
        public Task<List<CryptoPrice>> GetPricesAsync(string coin) => _db.Table<CryptoPrice>().Where(x => x.Coin == coin).ToListAsync();

        public Task<int> SaveDcaDaysAsync(List<int> days)
        {
            var csv = string.Join(",", days);
            return _db.InsertOrReplaceAsync(new DcaDays { DaysCsv = csv });
        }
        public async Task<List<int>> GetDcaDaysAsync()
        {
            var d = await _db.Table<DcaDays>().FirstOrDefaultAsync();
            if (d == null || string.IsNullOrEmpty(d.DaysCsv)) return new List<int>();
            return d.DaysCsv.Split(',').Select(int.Parse).ToList();
        }
    }
} 