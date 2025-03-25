using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;

namespace AviaTickets;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        LoadTickets();
    }

    private async void LoadTickets()
    {
        try
        {
            await using var db = new ApplicationDbContext();

            var ticketsFromDb = await db.Tickets.ToListAsync();
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Ticket = new ObservableCollection<Ticket>(ticketsFromDb);
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private async void LoadPurchasedTickets()
    {
        try
        {
            await using var db = new ApplicationDbContext();
        
            var tickets = await db.Purchased_Tickets
                .Include(pt => pt.Ticket)
                .Where(pt => pt.UserId == LoginPage.CurrentUser!.id)
                .ToListAsync();

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                PurchasedTickets = new ObservableCollection<PurchasedTicket>(tickets));
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    private ObservableCollection<Ticket> _ticket = [];

    public ObservableCollection<Ticket> Ticket
    {
        get => _ticket;
        set
        {
            _ticket = value;
            OnPropertyChanged(nameof(Ticket));
        }
    }
    
    private ObservableCollection<PurchasedTicket> _purchasedTickets = [];

    public ObservableCollection<PurchasedTicket> PurchasedTickets
    {
        get => _purchasedTickets;
        set
        {
            if (_purchasedTickets == value) 
                return;
            
            _purchasedTickets = value;
            OnPropertyChanged(nameof(PurchasedTickets));
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void OnMainButtonClick(object? sender, RoutedEventArgs e)
    {
        TicketsList.IsVisible = true;
        PurchasedTicketsList.IsVisible = false;
    }
    
    private void OnMyTicketsButtonClick(object? sender, RoutedEventArgs e)
    {
        // Переключаем видимость списков
        TicketsList.IsVisible = false;
        PurchasedTicketsList.IsVisible = true;

        LoadPurchasedTickets();
    }
    
    private void OnLogoutButtonClick(object? sender, RoutedEventArgs e)
    {
        var loginWindow = new LoginPage();
        loginWindow.Show();
        Close();
    }

    private async void OnBuyTicketButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: Ticket selectedTicket }) return;
            try
            {
                await using var db = new ApplicationDbContext();

                if (selectedTicket.ostatok <= 0)
                {
                    var messageWindow = new MessageWindow("Билетов нет в наличии.");
                    await messageWindow.ShowDialog(this);
                    return;
                }
                
                selectedTicket.ostatok -= 1;
                
                db.Tickets.Update(selectedTicket);

                var purchasedTicket = new PurchasedTicket
                {
                    TicketId = selectedTicket.Id,
                    UserId = LoginPage.CurrentUser!.id,
                    PurchasedDate = DateTime.Now
                };
                db.Purchased_Tickets.Add(purchasedTicket);
                
                await db.SaveChangesAsync();

                var successMessageWindow = new MessageWindow("Билет успешно куплен");
                await successMessageWindow.ShowDialog(this);
                
                LoadTickets();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                var errorMessageWindow = new MessageWindow(exception.Message);
                await errorMessageWindow.ShowDialog(this);
            }
        }
        catch (Exception f)
        {
            Console.WriteLine(f.Message);
        }
    }
    
    private void OnUserSettingsButtonClick(object? sender, RoutedEventArgs e)
    {
        TicketsList.IsVisible = false;
        PurchasedTicketsList.IsVisible = false;

        UserSettingsPanel.IsVisible = true;
        // isFilterVisible = false;
        
        UsernameTextBox.Text = LoginPage.CurrentUser!.username;
        UserEmailTextBox.Text = LoginPage.CurrentUser.email;
        UserPasswordTextBox.Text = LoginPage.CurrentUser.password;
    }

    private void OnUploadPhotoButtonClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    private void OnSaveUserSettingsButtonClick(object? sender, RoutedEventArgs e)
    {
        
    }
}