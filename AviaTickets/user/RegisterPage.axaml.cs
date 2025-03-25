using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;

namespace AviaTickets;

public partial class RegisterPage : Window
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var login = LoginTextBox.Text;
            var password = PasswordTextBox.Text;
            var email = EmailTextBox.Text;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                var messageBox = new MessageWindow("Не все данные введены");
                await messageBox.ShowDialog(this);
                return;
            }
        
            await using var db = new ApplicationDbContext();
        
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.username == login);

            if (existingUser != null)
            {
                var messageWindow = new MessageWindow("Пользователь с таким логином уже существует.");
                await messageWindow.ShowDialog(this);
            }
            else
            {
                db.Users.Add(new User { username = login, password = password, email = email });
                await db.SaveChangesAsync();

                var messageWindow = new MessageWindow("Пользователь успешно добавлен.");
                await messageWindow.ShowDialog(this);

                var loginWindow = new LoginPage();
                await loginWindow.ShowDialog(this);
                Close();
            }
        }
        catch (Exception f)
        {
            //throw; TODO handle exception
        }
    }

    private void GoBack(object? sender, RoutedEventArgs e)
    {
        var loginPage = new LoginPage();
        loginPage.Show();
        Close();
    }
}