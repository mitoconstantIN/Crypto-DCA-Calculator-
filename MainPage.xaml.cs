using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using CryptoDCACalculator.Models;
using CryptoDCACalculator.Services;
using Microsoft.Maui.Graphics;

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

    // Dynamic Table View Model for any number of cryptos
    public class DynamicTableRow
    {
        public string Date { get; set; } = string.Empty;
        public string InvestedAmount { get; set; } = string.Empty;
        public Dictionary<string, string> CryptoAmounts { get; set; } = new Dictionary<string, string>();
        public string ValueToday { get; set; } = string.Empty;
        public string ROI { get; set; } = string.Empty;
        public Color ROIColor { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private bool _dbInitialized = false;
        
        // Tab management
        private Button _activeTab;
        private Border _activeTabBorder;
        private StackLayout _activeContent;

        // Chart data
        private List<DcaResultRow> _chartData = new List<DcaResultRow>();
        private List<string> _chartCryptos = new List<string>();

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

        #region Tab Management

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

        private void OnTableTabClicked(object sender, EventArgs e)
        {
            SwitchTab(TableTab, TableTabBorder, TableContent);
        }

        private void OnChartTabClicked(object sender, EventArgs e)
        {
            SwitchTab(ChartTab, ChartTabBorder, ChartContent);
        }

        private void SwitchTab(Button newTab, Border newTabBorder, StackLayout newContent)
        {
            // Deactivate current tab
            _activeTabBorder.BackgroundColor = Colors.Transparent;
            _activeTab.TextColor = Color.FromArgb("#666666");
            _activeContent.IsVisible = false;

            // Activate new tab
            newTabBorder.BackgroundColor = Color.FromArgb("#7C3AED");
            newTab.TextColor = Colors.White;
            newContent.IsVisible = true;

            // Update active references
            _activeTab = newTab;
            _activeTabBorder = newTabBorder;
            _activeContent = newContent;
        }

        #endregion

        #region Chart Handlers

        private void OnPortfolioChartClicked(object sender, EventArgs e)
        {
            PortfolioChartBtn.BackgroundColor = Color.FromArgb("#7C3AED");
            PortfolioChartBtn.TextColor = Colors.White;
            ROIChartBtn.BackgroundColor = Colors.Transparent;
            ROIChartBtn.TextColor = Color.FromArgb("#666666");
            
            UpdateChartDisplay("PORTFOLIO");
        }

        private void OnROIChartClicked(object sender, EventArgs e)
        {
            ROIChartBtn.BackgroundColor = Color.FromArgb("#7C3AED");
            ROIChartBtn.TextColor = Colors.White;
            PortfolioChartBtn.BackgroundColor = Colors.Transparent;
            PortfolioChartBtn.TextColor = Color.FromArgb("#666666");
            
            UpdateChartDisplay("ROI");
        }

        private void UpdateChartDisplay(string chartType)
        {
            if (chartType == "PORTFOLIO")
            {
                DrawSimplePortfolioChart();
            }
            else
            {
                DrawSimpleROIChart();
            }
        }

        private void DrawSimplePortfolioChart()
        {
            ChartGrid.Children.Clear();
            ChartGrid.RowDefinitions.Clear();
            ChartGrid.ColumnDefinitions.Clear();

            if (!_chartData.Any()) 
            {
                var noDataLabel = new Label
                {
                    Text = "📈 NO DATA - RUN CALCULATION FIRST 📈",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#00D4FF"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                ChartGrid.Children.Add(noDataLabel);
                return;
            }

            // Group data by month
            var monthlyData = _chartData
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalValue = g.Sum(r => r.ValueToday),
                    TotalInvested = g.Sum(r => r.InvestedAmount)
                })
                .ToList();

            if (monthlyData.Count < 2) return;

            // Create grid with bars
            var maxValue = Math.Max(monthlyData.Max(d => d.TotalValue), monthlyData.Max(d => d.TotalInvested));
            var barCount = Math.Min(monthlyData.Count, 12); // Show max 12 bars
            var recentData = monthlyData.TakeLast(barCount).ToList();

            // Setup grid
            for (int i = 0; i < barCount; i++)
            {
                ChartGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            ChartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            ChartGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Draw bars
            for (int i = 0; i < recentData.Count; i++)
            {
                var data = recentData[i];
                
                // Container for bars
                var barContainer = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    VerticalOptions = LayoutOptions.End,
                    Spacing = 2,
                    Margin = new Thickness(2)
                };

                // Current value bar (cyan)
                var valueHeight = (double)(data.TotalValue / maxValue) * 200;
                var valueBar = new BoxView
                {
                    Color = Color.FromArgb("#00D4FF"),
                    HeightRequest = Math.Max(valueHeight, 5),
                    WidthRequest = 15,
                    HorizontalOptions = LayoutOptions.Center
                };

                // Invested bar (purple)
                var investedHeight = (double)(data.TotalInvested / maxValue) * 200;
                var investedBar = new BoxView
                {
                    Color = Color.FromArgb("#7C3AED"),
                    HeightRequest = Math.Max(investedHeight, 5),
                    WidthRequest = 10,
                    HorizontalOptions = LayoutOptions.Center
                };

                // Stack bars side by side
                var barsStack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 2
                };
                barsStack.Children.Add(investedBar);
                barsStack.Children.Add(valueBar);

                barContainer.Children.Add(barsStack);

                // Date label
                var dateLabel = new Label
                {
                    Text = data.Date.ToString("MMM"),
                    FontSize = 10,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center
                };

                Grid.SetColumn(barContainer, i);
                Grid.SetRow(barContainer, 0);
                ChartGrid.Children.Add(barContainer);

                Grid.SetColumn(dateLabel, i);
                Grid.SetRow(dateLabel, 1);
                ChartGrid.Children.Add(dateLabel);
            }

            // Add legend at top
            var legendStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 20,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var cyanLegend = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 5
            };
            cyanLegend.Children.Add(new BoxView { Color = Color.FromArgb("#00D4FF"), WidthRequest = 15, HeightRequest = 15 });
            cyanLegend.Children.Add(new Label { Text = "Current Value", TextColor = Color.FromArgb("#00D4FF"), FontSize = 12 });

            var purpleLegend = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 5
            };
            purpleLegend.Children.Add(new BoxView { Color = Color.FromArgb("#7C3AED"), WidthRequest = 15, HeightRequest = 15 });
            purpleLegend.Children.Add(new Label { Text = "Invested", TextColor = Color.FromArgb("#7C3AED"), FontSize = 12 });

            legendStack.Children.Add(purpleLegend);
            legendStack.Children.Add(cyanLegend);

            // Add legend spanning all columns
            Grid.SetColumn(legendStack, 0);
            Grid.SetColumnSpan(legendStack, barCount);
            Grid.SetRow(legendStack, 0);
            Grid.SetRowSpan(legendStack, 1);
            ChartGrid.Children.Add(legendStack);
        }

        private void DrawSimpleROIChart()
        {
            ChartGrid.Children.Clear();
            ChartGrid.RowDefinitions.Clear();
            ChartGrid.ColumnDefinitions.Clear();

            if (!_chartData.Any()) 
            {
                var noDataLabel = new Label
                {
                    Text = "📊 NO DATA - RUN CALCULATION FIRST 📊",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#00D4FF"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                ChartGrid.Children.Add(noDataLabel);
                return;
            }

            // Calculate monthly ROI
            var monthlyROI = _chartData
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g =>
                {
                    var totalValue = g.Sum(r => r.ValueToday);
                    var totalInvested = g.Sum(r => r.InvestedAmount);
                    var roi = totalInvested > 0 ? ((totalValue - totalInvested) / totalInvested) * 100 : 0;
                    return new
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        ROI = roi
                    };
                })
                .ToList();

            var barCount = Math.Min(monthlyROI.Count, 12);
            var recentData = monthlyROI.TakeLast(barCount).ToList();

            // Setup grid
            for (int i = 0; i < barCount; i++)
            {
                ChartGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            ChartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            ChartGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var maxAbsROI = recentData.Max(d => Math.Abs((double)d.ROI));
            if (maxAbsROI == 0) maxAbsROI = 10; // Prevent division by zero

            // Draw ROI bars
            for (int i = 0; i < recentData.Count; i++)
            {
                var data = recentData[i];
                var roi = (double)data.ROI;
                
                var barHeight = Math.Abs(roi) / maxAbsROI * 200;
                var barColor = roi >= 0 ? Color.FromArgb("#50C878") : Color.FromArgb("#FF6B6B");

                var roiBar = new BoxView
                {
                    Color = barColor,
                    HeightRequest = Math.Max(barHeight, 5),
                    WidthRequest = 20,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.End
                };

                // ROI value label
                var roiLabel = new Label
                {
                    Text = $"{roi:F1}%",
                    FontSize = 9,
                    TextColor = barColor,
                    HorizontalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold
                };

                var barStack = new StackLayout
                {
                    Children = { roiLabel, roiBar },
                    VerticalOptions = LayoutOptions.End,
                    Spacing = 2
                };

                // Date label
                var dateLabel = new Label
                {
                    Text = data.Date.ToString("MMM"),
                    FontSize = 10,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center
                };

                Grid.SetColumn(barStack, i);
                Grid.SetRow(barStack, 0);
                ChartGrid.Children.Add(barStack);

                Grid.SetColumn(dateLabel, i);
                Grid.SetRow(dateLabel, 1);
                ChartGrid.Children.Add(dateLabel);
            }

            // Add zero line and legend
            var zeroLine = new BoxView
            {
                Color = Colors.White,
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };

            Grid.SetColumn(zeroLine, 0);
            Grid.SetColumnSpan(zeroLine, barCount);
            Grid.SetRow(zeroLine, 0);
            ChartGrid.Children.Add(zeroLine);
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
            UpdateTableTab(monthlyResults, selectedCryptos);
            UpdateChartTab(monthlyResults, selectedCryptos);

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

            PortfolioValueLabel.Text = $"€{totalPortfolioEur:F2}";
            PortfolioROILabel.Text = $"{overallRoi:F2}%";
            
            if (overallRoi >= 0)
            {
                PortfolioROILabel.TextColor = Color.FromArgb("#00D4FF");
            }
            else
            {
                PortfolioROILabel.TextColor = Color.FromArgb("#FF6B6B");
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

        private void UpdateTableTab(List<DcaResultRow> results, List<string> selectedCryptos)
        {
            CreateDynamicTableHeader(selectedCryptos);

            var monthlyGroups = results
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g =>
                {
                    var monthResults = g.ToList();
                    var totalInvested = monthResults.Sum(r => r.InvestedAmount);
                    var totalValue = monthResults.Sum(r => r.ValueToday);
                    var overallROI = totalInvested > 0 ? (totalValue - totalInvested) / totalInvested : 0;

                    var cryptoAmounts = new Dictionary<string, string>();
                    foreach (var crypto in selectedCryptos)
                    {
                        var amount = monthResults.Where(r => r.Coin == crypto).Sum(r => r.CryptoAmount);
                        cryptoAmounts[crypto] = amount > 0 ? $"{amount:F4}" : "-";
                    }

                    return new DynamicTableRow
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        InvestedAmount = $"${totalInvested:F0}",
                        CryptoAmounts = cryptoAmounts,
                        ValueToday = $"${totalValue:F2}",
                        ROI = $"{(overallROI * 100):F1}%",
                        ROIColor = overallROI >= 0 ? Color.FromArgb("#00D4FF") : Color.FromArgb("#FF6B6B")
                    };
                })
                .ToList();

            var displayData = monthlyGroups.Select(row => new
            {
                Date = row.Date,
                InvestedAmount = row.InvestedAmount,
                CryptoData = string.Join(" | ", selectedCryptos.Select(crypto => 
                    $"{crypto}: {(row.CryptoAmounts.ContainsKey(crypto) ? row.CryptoAmounts[crypto] : "-")}")),
                ValueToday = row.ValueToday,
                ROI = row.ROI,
                ROIColor = row.ROIColor
            }).ToList();

            TableCollectionView.ItemsSource = displayData;
        }

        private void CreateDynamicTableHeader(List<string> selectedCryptos)
        {
            TableHeaderContainer.Children.Clear();
            
            var totalColumns = 4 + selectedCryptos.Count;
            var columnWidth = Math.Max(60, 400 / totalColumns);

            TableHeaderContainer.Children.Add(new Label
            {
                Text = "DATE", FontSize = 10, FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#00D4FF"), HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center, WidthRequest = columnWidth
            });

            TableHeaderContainer.Children.Add(new Label
            {
                Text = "INVESTED", FontSize = 10, FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#00D4FF"), HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center, WidthRequest = columnWidth
            });

            foreach (var crypto in selectedCryptos)
            {
                TableHeaderContainer.Children.Add(new Label
                {
                    Text = crypto, FontSize = 10, FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#00D4FF"), HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center, WidthRequest = columnWidth
                });
            }

            TableHeaderContainer.Children.Add(new Label
            {
                Text = "VALUE", FontSize = 10, FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#00D4FF"), HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center, WidthRequest = columnWidth
            });

            TableHeaderContainer.Children.Add(new Label
            {
                Text = "ROI", FontSize = 10, FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#00D4FF"), HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center, WidthRequest = columnWidth
            });
        }

        private void UpdateChartTab(List<DcaResultRow> results, List<string> selectedCryptos)
        {
            _chartData = results;
            _chartCryptos = selectedCryptos;
            
            UpdateChartLegend(selectedCryptos);
            UpdateChartDisplay("PORTFOLIO");
        }

        private void UpdateChartLegend(List<string> selectedCryptos)
        {
            LegendItems.Children.Clear();
            
            var colors = new[] { "#00D4FF", "#7C3AED", "#FF6B6B", "#50C878", "#FFD700", "#FF8C00" };
            
            for (int i = 0; i < selectedCryptos.Count; i++)
            {
                var legendItem = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center
                };
                
                var colorBox = new BoxView
                {
                    Color = Color.FromArgb(colors[i % colors.Length]),
                    WidthRequest = 15,
                    HeightRequest = 15,
                    VerticalOptions = LayoutOptions.Center
                };
                
                var label = new Label
                {
                    Text = $"◊ {selectedCryptos[i]}",
                    FontSize = 12,
                    TextColor = Color.FromArgb(colors[i % colors.Length]),
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center
                };
                
                legendItem.Children.Add(colorBox);
                legendItem.Children.Add(label);
                LegendItems.Children.Add(legendItem);
            }
        }
    }
}