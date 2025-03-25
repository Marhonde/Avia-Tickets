using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;

namespace AviaTickets;

public partial class AdminPage : Window, INotifyPropertyChanged
{
    public AdminPage()
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
            Console.WriteLine("0:" + e.Message);
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
            _purchasedTickets = value;
            OnPropertyChanged(nameof(PurchasedTickets));
        }
    }
    
    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void LoadPurchasedTickets()
    {
        try
        {
            await using var db = new ApplicationDbContext();
        
            var tickets = await db.Purchased_Tickets
                .AsNoTracking()
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .Where(t => t.UserId == LoginPage.CurrentUser!.id)
                .Select(t => new PurchasedTicket
                {
                    Id = t.Id,
                    TicketId = t.TicketId,
                    Ticket = t.Ticket,
                    UserId = t.UserId,
                    PurchasedDate = t.PurchasedDate,
                    username = t.User.username
                })
                .ToListAsync();
        
            Console.WriteLine($"Загружено купленных билетов: {tickets.Count}");
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                PurchasedTickets = new ObservableCollection<PurchasedTicket>(tickets);
            });
        }
        catch (Exception e)
        {
            Console.WriteLine("1: " + e.Message);
        }
    }

    private void OnMainButtonClick(object? sender, RoutedEventArgs e)
    {
        TicketsList.IsVisible = true;
        PurchasedTicketsList.IsVisible = false;
    }
    
    private void OnMyTicketButtonClick(object? sender, RoutedEventArgs e)
    {
        TicketsList.IsVisible = false;
        PurchasedTicketsList.IsVisible = true;
        
        LoadPurchasedTickets();
    }
    
    private async void OnAddTicketButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var addTicketWindow = new AddTicketWindow();
        
            await addTicketWindow.ShowDialog(this);
            LoadTickets();
        }
        catch (Exception f)
        {
            Console.WriteLine(f.Message);
        }
    }
    
    private void OnLogoutButtonClick(object? sender, RoutedEventArgs e)
    {
        var loginPage = new LoginPage();
        
        loginPage.Show();
        Close();
    }

    private async void OnEditTicketButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: Ticket selectedTicket })
            {
                Console.WriteLine("Не удалось получить выбранный билет.");
                return;
            }
        
            var editTicketWindow = new EditTicketWindow(selectedTicket);
        
            await editTicketWindow.ShowDialog(this);
        
            LoadTickets();
        }
        catch (Exception f)
        {
            Console.WriteLine(f.Message);
        }
    }
    
    private async void OnDeleteTicketButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: Ticket selectedTicket })
                return;

            await using (var db = new ApplicationDbContext())
            {
                var ticket = await db.Tickets.FindAsync(selectedTicket.Id);

                if (ticket != null)
                {
                    var relatedPurchases = db.Purchased_Tickets.Where(t => t.TicketId == ticket.Id);
                    db.Purchased_Tickets.Remove(relatedPurchases.FirstOrDefault());
                    
                    db.Tickets.Remove(selectedTicket);
                    await db.SaveChangesAsync();
                }
            }
            
            LoadTickets();
        }
        catch (Exception f)
        {
            Console.WriteLine("2: " + f.Message);
        }
    }
}