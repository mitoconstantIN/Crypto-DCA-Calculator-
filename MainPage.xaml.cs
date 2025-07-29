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
    // View Models for the tabs
    public class CoinSummary
    {
        public string CoinName { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string ValueEur { get; set; } = string.Empty;
    }

    public class HistoryItem
    {
        public string Date { get; set; } = string.Empty;
        public string Coin { get; set; } = string.Empty;
        public string InvestmentInfo { get; set; } = string.Empty;
        public string CryptoAmount { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string ROI { get; set; } = string.Empty;
        public Color ROIColor { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private bool _dbInitialized = false;
        
        // Tab management - Updated for futuristic UI
        private Button _activeTab;
        private Border _activeTabBorder;
        private StackLayout _activeContent;

        public MainPage()
        {
            InitializeComponent();
            _dbService = Application.Current.Handler?.MauiContext?.Services.GetService(typeof(DatabaseService)) as DatabaseService;
            
            // Initialize crypto list
            var cryptos = new List<string> { "BTC", "ETH", "SOL", "XRP", "ADA", "DOT" };
            CryptoListView.ItemsSource = cryptos;
            
            // Set initial active tab
            _activeTab = OverviewTab;
            _activeTabBorder = OverviewTabBorder;
            _activeContent = OverviewContent;
        }

        private async Task EnsureDbInitializedAsync()
        {
            if (_dbInitialized) return;
            var dbPath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "crypto_dca.db3");
            await _dbService.InitAsync(dbPath);
            _dbInitialized = true;
        }

        #region Tab Management - Futuristic Style

        private void OnOverviewTabClicked(object sender, EventArgs e)
        {
            SwitchTab(OverviewTab, OverviewTabBorder, OverviewContent);
        }

        private void OnCoinsTabClicked(object sender, EventArgs e)
        {
            SwitchTab(CoinsTab, CoinsTabBorder, CoinsContent);
        }

        private void OnHistoryTabClicked(object sender, EventArgs e)
        {
            SwitchTab(HistoryTab, HistoryTabBorder, HistoryContent);
        }

        private void SwitchTab(Button newTab, Border newTabBorder, StackLayout newContent)
        {
            // Deactivate current tab - Cyber style
            _activeTabBorder.BackgroundColor = Colors.Transparent;
            _activeTab.TextColor = Color.FromArgb("#666666");
            _activeContent.IsVisible = false;

            // Activate new tab - Neon glow effect
            newTabBorder.BackgroundColor = Color.FromArgb("#7C3AED");
            newTab.TextColor = Colors.White;
            newContent.IsVisible = true;

            // Update active references
            _activeTab = newTab;
            _activeTabBorder = newTabBorder;
            _activeContent = newContent;
        }

        #endregion

        private class DcaResultRow
        {
            public DateTime Date { get; set; }
            public string Coin { get; set; } = string.Empty;
            public decimal InvestedAmount { get; set; }
            public decimal CryptoAmount { get; set; }
            public decimal ValueToday { get; set; }
            public decimal ROI { get; set; }
        }

        private async void OnCalculateClicked(object sender, EventArgs e)
        {
            await EnsureDbInitializedAsync();
            ErrorLabel.IsVisible = false;

            var selectedCryptos = CryptoListView.SelectedItems?.Cast<string>().ToList() ?? new List<string>();
            if (selectedCryptos.Count == 0)
            {
                ErrorLabel.Text = "► PLEASE SELECT DIGITAL ASSETS ◄";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (!decimal.TryParse(AmountEntry.Text, out decimal monthlyAmount) || monthlyAmount <= 0)
            {
                ErrorLabel.Text = "► INVALID CAPITAL INJECTION AMOUNT ◄";
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
                ErrorLabel.Text = "► INVALID EXECUTION DAYS PROTOCOL ◄";
                ErrorLabel.IsVisible = true;
                return;
            }

            await _dbService.SaveDcaDaysAsync(dayList);

            var today = DateTime.Today;
            var monthlyResults = new List<DcaResultRow>();
            var rand = new Random(42);

            // Calculate DCA results
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

            // Update UI with results
            UpdateOverviewTab(monthlyResults);
            UpdateCoinsTab(monthlyResults);
            UpdateHistoryTab(monthlyResults);

            // Show results
            ResultsFrame.IsVisible = true;
        }

        private void UpdateOverviewTab(List<DcaResultRow> results)
        {
            const decimal UsdToEur = 0.92m;
            
            var totalInvested = results.Sum(r => r.InvestedAmount);
            var totalCurrentValue = results.Sum(r => r.ValueToday);
            var totalPortfolioEur = totalCurrentValue * UsdToEur;
            var overallRoi = totalInvested > 0 ? ((totalCurrentValue - totalInvested) / totalInvested) * 100 : 0;

            // Update portfolio summary - Futuristic style
            PortfolioValueLabel.Text = $"€{totalPortfolioEur:F2}";
            PortfolioROILabel.Text = $"{overallRoi:F2}%";
            
            // Color coding for ROI - Cyber theme
            if (overallRoi >= 0)
            {
                PortfolioROILabel.TextColor = Color.FromArgb("#00D4FF"); // Cyan for positive
            }
            else
            {
                PortfolioROILabel.TextColor = Color.FromArgb("#FF6B6B"); // Red for negative
            }

            TotalInvestedLabel.Text = $"${totalInvested:F0}";
            CurrentValueLabel.Text = $"${totalCurrentValue:F2}";
        }

        private void UpdateCoinsTab(List<DcaResultRow> results)
        {
            const decimal UsdToEur = 0.92m;
            
            var coinSummaries = results.GroupBy(r => r.Coin)
                .Select(g => new CoinSummary
                {
                    CoinName = $"◊ {g.Key}",
                    Amount = $"{g.Sum(r => r.CryptoAmount):F6} {g.Key}",
                    ValueEur = $"€{(g.Sum(r => r.ValueToday) * UsdToEur):F2}"
                })
                .ToList();

            CoinsCollectionView.ItemsSource = coinSummaries;
        }

        private void UpdateHistoryTab(List<DcaResultRow> results)
        {
            var historyItems = results.OrderByDescending(r => r.Date)
                .Select(r => new HistoryItem
                {
                    Date = r.Date.ToString("MMM yyyy"),
                    Coin = $"◊ {r.Coin}",
                    InvestmentInfo = $"Capital: ${r.InvestedAmount:F0}",
                    CryptoAmount = $"{r.CryptoAmount:F6} {r.Coin}",
                    CurrentValue = $"${r.ValueToday:F2}",
                    ROI = $"{(r.ROI * 100):F2}%",
                    ROIColor = r.ROI >= 0 ? Color.FromArgb("#00D4FF") : Color.FromArgb("#FF6B6B")
                })
                .ToList();

            HistoryCollectionView.ItemsSource = historyItems;
        }
    }
}