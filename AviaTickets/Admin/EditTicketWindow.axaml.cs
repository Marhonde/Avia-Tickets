using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AviaTickets;

public partial class EditTicketWindow : Window
{
    private readonly Ticket _ticket;
    
    public EditTicketWindow(Ticket ticket)
    {
        _ticket = ticket;
        InitializeComponent();
        
        Console.WriteLine($"Редактирование билета: {ticket.Title}");
        
        TitleTextBox.Text = ticket.Title;
        CityFromTextBox.Text = ticket.City_From;
        CityToTextBox.Text = ticket.City_To;
        
        DateUletDatePicker.SelectedDate = ticket.Date_Ulet;
        DateUletTimePicker.SelectedTime = ticket.Date_Ulet.TimeOfDay;
        
        DatePriletDatePicker.SelectedDate = ticket.Date_Prilet;
        DatePriletTimePicker.SelectedTime = ticket.Date_Prilet.TimeOfDay;
        
        TimeFlyTextBox.SelectedTime = ticket.Time_Fly;
        OstatokTextBox.Text = ticket.ostatok.ToString();
        DescriptionTextBox.Text = ticket.Description;
        
        Console.WriteLine("The fields are filled in");
    }

    private async void OnSaveButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                string.IsNullOrWhiteSpace(CityFromTextBox.Text) ||
                string.IsNullOrWhiteSpace(CityToTextBox.Text) ||
                DateUletDatePicker.SelectedDate == null ||
                DateUletTimePicker.SelectedTime == null ||
                DatePriletDatePicker.SelectedDate == null ||
                DatePriletTimePicker.SelectedTime == null ||
                string.IsNullOrWhiteSpace(TimeFlyTextBox.SelectedTime.ToString()) ||
                string.IsNullOrWhiteSpace(OstatokTextBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                var messageWindow = new MessageWindow("Заполните все поля");
                await messageWindow.ShowDialog(this);
                return;
            }
            
            var dateUlet = (DateUletDatePicker.SelectedDate.Value.Date +
                            DateUletTimePicker.SelectedTime.Value).ToUniversalTime();
            
            var datePrilet = (DatePriletDatePicker.SelectedDate.Value.Date +
                              DatePriletTimePicker.SelectedTime.Value).ToUniversalTime();

            dateUlet = dateUlet.AddHours(7);
            datePrilet = datePrilet.AddHours(7);
            
            var timeFly = TimeFlyTextBox.SelectedTime!.Value;
            
            if (!int.TryParse(OstatokTextBox.Text, out var ostatok))
            {
                var messageWindow = new MessageWindow("Некорректный формат остатка билетов. Введите целое число.");
                await messageWindow.ShowDialog(this);
                return;
            }

            _ticket.Title = TitleTextBox.Text;
            _ticket.City_From = CityFromTextBox.Text;
            _ticket.City_To = CityToTextBox.Text;
            
            _ticket.Date_Ulet = dateUlet;
            _ticket.Date_Prilet = datePrilet;
            _ticket.Time_Fly = timeFly;
            _ticket.ostatok = ostatok;
            _ticket.Description = DescriptionTextBox.Text;

            await using (var db = new ApplicationDbContext())
            {
                db.Tickets.Update(_ticket);
                await db.SaveChangesAsync();
            }

            var messageSuccess = new MessageWindow("Билет успешно обновлен.");
            await messageSuccess.ShowDialog(this);
            
            Close(true);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
}