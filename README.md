# Crypto DCA Calculator
## Portfolio Evolution Matrix - Professional Documentation

### Application Overview

**Crypto DCA Calculator** is a professional mobile application built with .NET MAUI that helps users analyze and visualize their cryptocurrency Dollar Cost Averaging (DCA) investment strategies. The application features a futuristic cyberpunk design and provides comprehensive portfolio analytics.

---

##  Features

### Core Functionality
- **Multi-Cryptocurrency Support**: Select from popular cryptocurrencies (BTC, ETH, SOL, XRP, ADA, DOT)
- **Flexible DCA Configuration**: Set custom investment amounts and recurring investment days
- **Historical Analysis**: Calculate portfolio performance from any start date to present
- **Real-time Portfolio Metrics**: Track total value, ROI, and individual asset performance

### User Interface
- **Cyberpunk Design**: Futuristic neon-themed interface with glow effects
- **Multi-Tab Dashboard**: Organized view with Overview, Assets, and Timeline tabs
- **Responsive Layout**: Optimized for mobile devices
- **Professional Feedback**: Loading states, success/error messages with animations

### Data Management
- **Local Database**: SQLite storage for price history and configurations
- **Secure Authentication**: Admin-only access with cyberpunk login interface
- **Data Persistence**: Automatic saving of investment configurations

---

##  Authentication

### Login Credentials
- **Username**: `admin`
- **Password**: `admin`

### Security Features
- Secure credential validation
- Automatic password field clearing on failed attempts
- Visual feedback with shake animations
- Session-based access control

---

##  Investment Configuration

### Supported Cryptocurrencies
| Symbol | Name | Base Price (Mock) |
|--------|------|-------------------|
| BTC | Bitcoin | $30,000 |
| ETH | Ethereum | $2,000 |
| SOL | Solana | $50 |
| XRP | Ripple | $0.50 |
| ADA | Cardano | $100 |
| DOT | Polkadot | $100 |

### Investment Parameters
- **Capital Injection**: Monthly investment amount in USD
- **Genesis Date**: Start date for DCA analysis
- **Recurring Days**: Custom days of the month for investments (e.g., 1,15 for bi-monthly)

---

##  Portfolio Analytics

### Overview Tab
- **Total Portfolio Value**: EUR equivalent of current holdings
- **ROI Percentage**: Overall return on investment with color coding
- **Capital Deployed**: Total amount invested to date
- **Current Power**: Present value of all holdings

### Assets Tab
- **Individual Holdings**: Detailed breakdown per cryptocurrency
- **Asset Allocation**: Amount owned and EUR value for each coin
- **Performance Tracking**: Real-time value updates

### Timeline Tab
- **Investment History**: Chronological view of all DCA transactions
- **Monthly Breakdown**: Investment details by month and cryptocurrency
- **ROI per Transaction**: Performance analysis for each investment period

---

##  Technical Specifications

### Platform Support
- **Primary**: Android (net9.0-android)
- **Secondary**: Windows (net9.0-windows10.0.19041.0)
- **Framework**: .NET MAUI 9.0

### Architecture
- **Frontend**: XAML with cyberpunk styling
- **Backend**: C# with async/await patterns
- **Database**: SQLite for local data storage
- **Design Pattern**: MVVM-inspired with clean separation

### Dependencies
- Microsoft.Maui.Controls
- SQLite for data persistence
- Custom cyberpunk UI components

---

##  Installation Guide

### Android Installation
1. **Download APK**: Obtain the signed APK file from build output
2. **Enable Unknown Sources**: Settings → Security → Install unknown apps
3. **Install Application**: Open APK file and confirm installation
4. **Launch**: Find "Crypto DCA Calculator" in app drawer

### Windows Installation
1. **Build Solution**: Use Visual Studio or dotnet CLI
2. **Deploy**: Run from development environment
3. **Package**: Create MSIX package for distribution

---

##  User Guide

### Getting Started
1. **Launch Application**: Open Crypto DCA Calculator
2. **Authenticate**: Login with admin credentials
3. **Configure Investment**: Select cryptocurrencies and set parameters
4. **Execute Analysis**: Run DCA calculation protocol
5. **Review Results**: Explore portfolio analytics across tabs

### Best Practices
- **Regular Updates**: Refresh calculations periodically
- **Diversification**: Select multiple cryptocurrencies for balanced portfolios
- **Long-term Analysis**: Use extended date ranges for meaningful DCA insights
- **Data Backup**: Export results for external analysis

---

##  Configuration Options

### Investment Setup
```
Capital Injection: $100-$10000 (recommended)
Genesis Date: Any historical date
Recurring Days: 1-31 (comma-separated)
```

### Performance Optimization
- Database optimization for faster calculations
- Efficient memory management for large datasets
- Responsive UI updates during processing

---

##  Sample Use Cases

### Basic DCA Strategy
- **Scenario**: Monthly Bitcoin investment
- **Configuration**: $200 monthly, 15th of each month
- **Analysis**: Track performance over 12+ months

### Advanced Multi-Asset Strategy
- **Scenario**: Diversified cryptocurrency portfolio
- **Configuration**: Multiple coins, bi-weekly investments
- **Analysis**: Compare asset performance and optimize allocation

### Historical Analysis
- **Scenario**: "What if" investment scenarios
- **Configuration**: Past start dates with current parameters
- **Analysis**: Evaluate timing impact on portfolio performance

---

##  Known Limitations

- **Mock Pricing**: Uses simulated price data for demonstration
- **Offline Mode**: Requires internet for optimal performance
- **Platform Specific**: Primary optimization for mobile devices

---

##  Future Enhancements

### Planned Features
- Real-time API integration for live cryptocurrency prices
- Advanced charting with technical indicators
- Portfolio export functionality (CSV, PDF)
- Cloud synchronization for multi-device access
- Custom notification system for investment reminders

### Technical Improvements
- Performance optimization for large datasets
- Enhanced security with biometric authentication
- Cross-platform chart rendering
- Advanced analytics with machine learning insights

---

##  Development Information

### Build Requirements
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Android SDK (for mobile deployment)
- MAUI workload installed

### Project Structure
```
CryptoDCACalculator/
├── Models/           # Data models
├── Services/         # Business logic
├── Views/           # XAML pages
├── ViewModels/      # UI logic
└── Resources/       # Assets and styles
```

---

## Demo Video - Watch the application in action here:
https://www.youtube.com/watch?v=iZvFWKYY42I
---
