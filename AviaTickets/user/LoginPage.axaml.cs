using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;

namespace AviaTickets;

public partial class LoginPage : Window
{
    public LoginPage()
    {
        InitializeComponent();
    }
    
    public static User? CurrentUser { get; set; }

    private async void OnLoginButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var login = LoginTextBox.Text;
            var password = PasswordTextBox.Text;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage.Text = "Логин и пароль не могут быть пустыми";
                return;
            }

            await using var db = new ApplicationDbContext();
        
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.username == login);

            CurrentUser = existingUser;
            
            if (existingUser != null && existingUser.password == password)
            {
                if (existingUser.role == 1)
                {
                    var messageWindowAdmin = new MessageWindow("Админ сервера зашел, всем встать");
                    
                    await messageWindowAdmin.ShowDialog(this);
                    var adminPage = new AdminPage();
                    adminPage.Show();
                }
                else
                {
                    var messageWindow = new MessageWindow($"Добро пожаловать, {login}!");
                    
                    await messageWindow.ShowDialog(this);
                    
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                }

                Close();
            }
            else
            {
                var messageWindow = new MessageWindow("Неверный логин или пароль.");
                await messageWindow.ShowDialog(this);
            }
        }
        catch (Exception f)
        {
            Console.WriteLine(f.Message);
        }
    }

    private void OnRegisterButtonClick(object? sender, RoutedEventArgs e)
    {
        var registerWindow = new RegisterPage();
        registerWindow.Show();
        Close();
    }
}