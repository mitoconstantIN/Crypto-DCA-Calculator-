using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using CryptoDCACalculator.Models;
using CryptoDCACalculator.Services;

namespace CryptoDCACalculator
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private bool _dbInitialized = false;

        public MainPage()
        {
            InitializeComponent();
            _dbService = Application.Current.Handler.MauiContext.Services.GetService(typeof(DatabaseService)) as DatabaseService;
        }

        private async Task EnsureDbInitializedAsync()
        {
            if (_dbInitialized) return;
            var dbPath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "crypto_dca.db3");
            await _dbService.InitAsync(dbPath);
            _dbInitialized = true;
        }

        private class DcaResultRow
        {
            public DateTime Date { get; set; }
            public string Coin { get; set; }
            public decimal InvestedAmount { get; set; }
            public decimal CryptoAmount { get; set; }
            public decimal ValueToday { get; set; }
            public decimal ROI { get; set; }
        }

        // Mock price data: date -> coin -> price
        private Dictionary<string, Dictionary<DateTime, decimal>> GetMockPriceHistory(List<string> coins, DateTime start, DateTime end)
        {
            var dict = new Dictionary<string, Dictionary<DateTime, decimal>>();
            var rand = new Random(42);
            foreach (var coin in coins)
            {
                var prices = new Dictionary<DateTime, decimal>();
                decimal basePrice = coin switch { "BTC" => 30000, "ETH" => 2000, "SOL" => 50, "XRP" => 0.5m, _ => 100 };
                for (var dt = start.Date; dt <= end.Date; dt = dt.AddDays(1))
                {
                    // Simulate price with some random walk
                    basePrice *= (decimal)(1 + (rand.NextDouble() - 0.5) * 0.01);
                    prices[dt] = Math.Round(basePrice, 2);
                }
                dict[coin] = prices;
            }
            return dict;
        }

        private class TableRow
        {
            public string Date { get; set; }
            public string Coin { get; set; }
            public string InvestedUsd { get; set; }
            public string CryptoAmount { get; set; }
            public string ValueTodayUsdEur { get; set; }
            public string RoiCurrency { get; set; }
        }

        private async void OnCalculateClicked(object sender, EventArgs e)
        {
            await EnsureDbInitializedAsync();
            ErrorLabel.IsVisible = false;

            var selectedCryptos = CryptoListView.SelectedItems?.Cast<string>().ToList() ?? new List<string>();
            if (selectedCryptos.Count == 0)
            {
                ErrorLabel.Text = "Please select at least one cryptocurrency.";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (!decimal.TryParse(AmountEntry.Text, out decimal monthlyAmount) || monthlyAmount <= 0)
            {
                ErrorLabel.Text = "Please enter a valid monthly amount.";
                ErrorLabel.IsVisible = true;
                return;
            }

            var startDate = StartDatePicker.Date;
            var daysText = DaysEntry.Text ?? string.Empty;
            var dayList = daysText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(s => s.Trim())
                                  .Where(s => int.TryParse(s, out int _))
                                  .Select(int.Parse)
                                  .Where(d => d >= 1 && d <= 31)
                                  .Distinct()
                                  .ToList();
            if (dayList.Count == 0)
            {
                ErrorLabel.Text = "Please enter at least one valid recurring day (1-31).";
                ErrorLabel.IsVisible = true;
                return;
            }

            await _dbService.SaveDcaDaysAsync(dayList);

            var today = DateTime.Today;
            var monthlyResults = new List<DcaResultRow>();
            var rand = new Random(42);

            foreach (var coin in selectedCryptos)
            {
                decimal basePrice = coin switch { "BTC" => 30000, "ETH" => 2000, "SOL" => 50, "XRP" => 0.5m, _ => 100 };
                var dt = new DateTime(startDate.Year, startDate.Month, 1);

                while (dt <= today)
                {
                    decimal monthlyInvested = 0;
                    decimal monthlyCrypto = 0;

                    foreach (var day in dayList)
                    {
                        DateTime investDate;
                        try { investDate = new DateTime(dt.Year, dt.Month, day); }
                        catch { continue; }

                        if (investDate < startDate || investDate > today) continue;

                        var dbPrice = await _dbService.GetPriceAsync(coin, investDate);
                        decimal price;

                        if (dbPrice != null)
                        {
                            price = dbPrice.Price;
                        }
                        else
                        {
                            basePrice *= (decimal)(1 + (rand.NextDouble() - 0.5) * 0.01);
                            price = Math.Round(basePrice, 2);
                            await _dbService.SavePriceAsync(new CryptoPrice { Coin = coin, Date = investDate, Price = price });
                        }

                        monthlyInvested += monthlyAmount;
                        monthlyCrypto += monthlyAmount / price;
                    }

                    if (monthlyInvested > 0)
                    {
                        var todayPriceObj = await _dbService.GetPriceAsync(coin, today);
                        decimal todayPrice;

                        if (todayPriceObj != null)
                        {
                            todayPrice = todayPriceObj.Price;
                        }
                        else
                        {
                            basePrice *= (decimal)(1 + (rand.NextDouble() - 0.5) * 0.01);
                            todayPrice = Math.Round(basePrice, 2);
                            await _dbService.SavePriceAsync(new CryptoPrice { Coin = coin, Date = today, Price = todayPrice });
                        }

                        var valueToday = monthlyCrypto * todayPrice;
                        var roi = (valueToday - monthlyInvested) / monthlyInvested;

                        monthlyResults.Add(new DcaResultRow
                        {
                            Date = dt,
                            Coin = coin,
                            InvestedAmount = monthlyInvested,
                            CryptoAmount = monthlyCrypto,
                            ValueToday = Math.Round(valueToday, 2),
                            ROI = roi
                        });
                    }

                    dt = dt.AddMonths(1);
                }
            }

            // Convert to final table format
            const decimal UsdToEur = 0.92m;
            var finalResults = new List<TableRow>();

            // 1. Add TOTAL rows for each coin (in EUR)
            var coinGroups = monthlyResults.GroupBy(r => r.Coin);
            decimal totalPortfolioEur = 0;
            foreach (var group in coinGroups)
            {
                var coin = group.Key;
                var totalCrypto = group.Sum(r => r.CryptoAmount);
                var totalUsd = group.Sum(r => r.ValueToday);
                var totalEur = Math.Round(totalUsd * UsdToEur, 2);
                totalPortfolioEur += totalEur;

                finalResults.Add(new TableRow
                {
                    Date = "TOTAL",
                    Coin = coin,
                    InvestedUsd = "0",
                    CryptoAmount = totalCrypto.ToString("F7"),
                    ValueTodayUsdEur = totalEur.ToString("F2"),
                    RoiCurrency = "EUR"
                });
            }

            // 2. Add PORTFOLIO total row
            finalResults.Add(new TableRow
            {
                Date = "TOTAL",
                Coin = "PORTFOLIO",
                InvestedUsd = "0",
                CryptoAmount = "0.0",
                ValueTodayUsdEur = totalPortfolioEur.ToString("F2"),
                RoiCurrency = "EUR"
            });

            // 3. Add overall TOTAL row (total invested vs total current value)
            var totalInvested = monthlyResults.Sum(r => r.InvestedAmount);
            var totalCurrentValue = monthlyResults.Sum(r => r.ValueToday);
            var overallRoi = totalInvested > 0 ? ((totalCurrentValue - totalInvested) / totalInvested) * 100 : 0;

            finalResults.Add(new TableRow
            {
                Date = "TOTAL",
                Coin = "TOTAL",
                InvestedUsd = ((int)totalInvested).ToString(),
                CryptoAmount = "0.0",
                ValueTodayUsdEur = totalCurrentValue.ToString("F2"),
                RoiCurrency = overallRoi.ToString("F2") + "%"
            });

            // 4. Add monthly data rows
            foreach (var result in monthlyResults.OrderBy(r => r.Date))
            {
                var roi = result.ROI * 100;
                finalResults.Add(new TableRow
                {
                    Date = result.Date.ToString("MM/yyyy"),
                    Coin = result.Coin,
                    InvestedUsd = ((int)result.InvestedAmount).ToString(),
                    CryptoAmount = result.CryptoAmount.ToString("F4"),
                    ValueTodayUsdEur = result.ValueToday.ToString("F2"),
                    RoiCurrency = roi.ToString("F2") + "%"
                });
            }

            ResultsTable.ItemsSource = finalResults;
            ResultsTable.IsVisible = true;
        }
    }
}