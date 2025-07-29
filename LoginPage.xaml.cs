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
    if (!string.IsNullOrWhiteSpace(UsernameEntry.Text) && !string.IsNullOrWhiteSpace(PasswordEntry.Text))
    {
        // Set Shell as root, after navigate to MainPage
        Application.Current.MainPage = new AppShell();
        await Shell.Current.GoToAsync("//MainPage");
    }
    else
    {
        LoginErrorLabel.Text = "Please enter username and password.";
        LoginErrorLabel.IsVisible = true;
    }
}
    }
} 