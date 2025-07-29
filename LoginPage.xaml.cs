using System;
using Microsoft.Maui.Controls;

namespace CryptoDCACalculator
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Hide previous error
            ErrorBorder.IsVisible = false;
            
            // Add futuristic loading effect
            LoginButton.Text = "⚡ AUTHENTICATING... ⚡";
            LoginButton.IsEnabled = false;
            
            // Simulate authentication delay for better UX
            await Task.Delay(1000);
            
            if (!string.IsNullOrWhiteSpace(UsernameEntry.Text) && 
                !string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                // Authentication successful
                LoginButton.Text = "✅ ACCESS GRANTED ✅";
                await Task.Delay(500);
                
                // Navigate to main application
                Application.Current.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                // Authentication failed - Show cyber-style error
                LoginErrorLabel.Text = "► AUTHENTICATION FAILED: INVALID CREDENTIALS ◄";
                ErrorBorder.IsVisible = true;
                
                // Reset button
                LoginButton.Text = "⚡ INITIATE ACCESS PROTOCOL ⚡";
                LoginButton.IsEnabled = true;
                
                // Clear password field for security
                PasswordEntry.Text = string.Empty;
                
                // Add subtle shake animation effect (optional)
                await ErrorBorder.TranslateTo(-10, 0, 50);
                await ErrorBorder.TranslateTo(10, 0, 50);
                await ErrorBorder.TranslateTo(-5, 0, 50);
                await ErrorBorder.TranslateTo(5, 0, 50);
                await ErrorBorder.TranslateTo(0, 0, 50);
            }
        }
    }
}