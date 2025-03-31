using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
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
                FilteredTickets = new ObservableCollection<Ticket>(ticketsFromDb);
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
        IsFilterVisible = true;
        UserSettingsPanel.IsVisible = false;
    }
    
    private void OnMyTicketsButtonClick(object? sender, RoutedEventArgs e)
    {
        // Переключаем видимость списков
        TicketsList.IsVisible = false;
        PurchasedTicketsList.IsVisible = true;
        UserSettingsPanel.IsVisible = false;

        LoadPurchasedTickets();
        IsFilterVisible = false;
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

    private ObservableCollection<Ticket> _filteredTickets = [];

    public ObservableCollection<Ticket> FilteredTickets
    {
        get => _filteredTickets;
        set
        {
            _filteredTickets = value;
            OnPropertyChanged(nameof(FilteredTickets));
        }
    }
    
    private bool _isFilterVisible = true;

    public bool IsFilterVisible
    {
        get => _isFilterVisible;
        set
        {
            if (_isFilterVisible == value) 
                return;
            
            _isFilterVisible = value;
            OnPropertyChanged(nameof(IsFilterVisible));
        }
    }
    
    private void OnUserSettingsButtonClick(object? sender, RoutedEventArgs e)
    {
        TicketsList.IsVisible = false;
        PurchasedTicketsList.IsVisible = false;

        UserSettingsPanel.IsVisible = true;
        IsFilterVisible = false;
        
        UsernameTextBox.Text = LoginPage.CurrentUser!.username;
        UserEmailTextBox.Text = LoginPage.CurrentUser.email;
        UserPasswordTextBox.Text = LoginPage.CurrentUser.password;

        if (!string.IsNullOrEmpty(LoginPage.CurrentUser.photo))
        {
            var projectRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
            var absolutePath = Path.Combine(projectRoot, LoginPage.CurrentUser.photo);
            
            UpdateUserPhoto(absolutePath);
        }
        else
        {
            UserPhotoImage.Source = null;
        }
    }

    [Obsolete("Obsolete")]
    private async void OnUploadPhotoButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filters.Add(new FileDialogFilter { Name = "Images", Extensions = { "png", "jpg", "jpeg" } });
            fileDialog.AllowMultiple = false;

            var result = await fileDialog.ShowAsync(this);
            if (result is not { Length: > 0 })
                return;
        
            var filePath = result[0];
            
            var projectRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
            var photosDirectory = Path.Combine(projectRoot, "PhotoUser");
            Console.WriteLine($"Directory for photos: {photosDirectory}");

            if (!Directory.Exists(photosDirectory))
            {
                Directory.CreateDirectory(photosDirectory);
                Console.WriteLine($"Directory created: {photosDirectory}");
            }
            else
            {
                Console.WriteLine($"Directory already exists: {photosDirectory}");
            }
            
            var fileName = Guid.NewGuid() + Path.GetExtension(filePath);
            var destinationPath = Path.Combine(photosDirectory, fileName);

            try
            {
                File.Copy(filePath, destinationPath, true);
                Console.WriteLine($"Error when copying a file: {destinationPath}");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            
            var relativePath = Path.Combine("PhotoUser", fileName);
            
            await using var db = new ApplicationDbContext();
            
            var user = db.Users.FirstOrDefault(u => u.username == LoginPage.CurrentUser!.username);
            if (user == null)
                return;
        
            user.photo = relativePath;
            db.Users.Update(user);
                
            await db.SaveChangesAsync();
                
            LoginPage.CurrentUser!.photo = relativePath;

            var mw = new MessageWindow("Фотография успешно добавлена!");
            await mw.ShowDialog(this);
            UpdateUserPhoto(destinationPath);
        }
        catch (Exception f)
        {
            Console.WriteLine(f.Message);
        }
    }

    private void UpdateUserPhoto(string photoPath)
    {
        if (File.Exists(photoPath))
        {
            var bitmap = new Avalonia.Media.Imaging.Bitmap(photoPath);
            UserPhotoImage.Source = bitmap;
        }
        else
        {
            UserPhotoImage.Source = null;
            Console.WriteLine($"File not found: {photoPath}");
        }
    }
    
    private async void OnSaveUserSettingsButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await using var db = new ApplicationDbContext();
            
            var user = await db.Users.FirstOrDefaultAsync(u => u.id == LoginPage.CurrentUser!.id);
            if (user == null)
                return;
            
            if (UsernameTextBox.Text != null)
                user.username = UsernameTextBox.Text;
                
            if (UserEmailTextBox.Text != null)
                user.email = UserEmailTextBox.Text;
                
            if (UserPasswordTextBox.Text != null)
                user.password = UserPasswordTextBox.Text;
                
            db.Users.Update(user);
            await db.SaveChangesAsync();
                
            LoginPage.CurrentUser!.username = user.username;
            LoginPage.CurrentUser.email = user.email;
            LoginPage.CurrentUser.password = user.password;

            var mw = new MessageWindow("Изменения успешно сохранены!");
            await mw.ShowDialog(this);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            
            var errorMw = new MessageWindow("Произошла ошибка при сохранении изменений");
            await errorMw.ShowDialog(this);
        }
    }

    private void OnFilterDateTextBlockTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TextBlock textBlock)
            FlyoutBase.ShowAttachedFlyout(textBlock);
    }

    private void OnFilterDateChanged(object? sender, DatePickerSelectedValueChangedEventArgs e)
    {
        FilterDateTextBlock.Text = FilterDatePicker.SelectedDate.HasValue
            ? FilterDatePicker.SelectedDate.Value.ToString("dd.MM.yyyy") : "Выберите дату";

        ApplyFilters();
    }

    private void OnResetFilterClick(object? sender, RoutedEventArgs e)
    {
        SearchTextBox.Text = string.Empty;
        FilterDatePicker.SelectedDate = null;
        FilterDateTextBlock.Text = "Выберите дату";
        ApplyFilters();
    }
    
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }
    
    private void ApplyFilters()
    {
        var filtered = _ticket.AsEnumerable();

        // фильтр по названию
        if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            filtered = filtered.Where(t => t.Title.Contains(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase));
        
        // фильтр по дате вылета
        if (FilterDatePicker.SelectedDate.HasValue)
            filtered = filtered.Where(t => t.Date_Ulet.Date == FilterDatePicker.SelectedDate.Value.Date);
        
        FilteredTickets = new ObservableCollection<Ticket>(filtered);
    }
}