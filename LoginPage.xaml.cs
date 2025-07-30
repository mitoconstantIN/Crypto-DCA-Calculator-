using System;
using Microsoft.Maui.Controls;

namespace CryptoDCACalculator
{
    public partial class LoginPage : ContentPage
    {
        // Credențiale hardcodate pentru securitate
        private const string ADMIN_USERNAME = "admin";
        private const string ADMIN_PASSWORD = "admin";

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
            
            // Verifică credențialele specifice
            if (!string.IsNullOrWhiteSpace(UsernameEntry.Text) && 
                !string.IsNullOrWhiteSpace(PasswordEntry.Text) &&
                UsernameEntry.Text.Trim().ToLower() == ADMIN_USERNAME &&
                PasswordEntry.Text == ADMIN_PASSWORD)
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
                if (string.IsNullOrWhiteSpace(UsernameEntry.Text) || 
                    string.IsNullOrWhiteSpace(PasswordEntry.Text))
                {
                    LoginErrorLabel.Text = "► AUTHENTICATION FAILED: PLEASE ENTER CREDENTIALS ◄";
                }
                else
                {
                    LoginErrorLabel.Text = "► AUTHENTICATION FAILED: INVALID CREDENTIALS ◄";
                }
                
                ErrorBorder.IsVisible = true;
                
                // Reset button
                LoginButton.Text = "⚡ INITIATE ACCESS PROTOCOL ⚡";
                LoginButton.IsEnabled = true;
                
                // Clear password field for security
                PasswordEntry.Text = string.Empty;
                
                // Add subtle shake animation effect
                await ErrorBorder.TranslateTo(-10, 0, 50);
                await ErrorBorder.TranslateTo(10, 0, 50);
                await ErrorBorder.TranslateTo(-5, 0, 50);
                await ErrorBorder.TranslateTo(5, 0, 50);
                await ErrorBorder.TranslateTo(0, 0, 50);
            }
        }
    }
}