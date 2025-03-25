using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AviaTickets;

public partial class AddTicketWindow : Window
{
    public AddTicketWindow()
    {
        InitializeComponent();
    }

    private async void OnAddButtonClick(object? sender, RoutedEventArgs e)
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
                var messageWindow = new MessageWindow("Заполни все поля, слышь");
                await messageWindow.ShowDialog(this);
                return;
            }

            var dateUlet = (DateUletDatePicker.SelectedDate.Value.Date +
                            DateUletTimePicker.SelectedTime.Value).ToUniversalTime();
            
            var datePrilet = (DatePriletDatePicker.SelectedDate.Value.Date +
                              DatePriletTimePicker.SelectedTime.Value).ToUniversalTime();

            dateUlet = dateUlet.AddHours(7);
            datePrilet = datePrilet.AddHours(7);
            
            if (!TimeSpan.TryParseExact(TimeFlyTextBox.SelectedTime.ToString(), @"hh\:mm\:ss", CultureInfo.InvariantCulture, out var timeFly))
            {
                var messageWindow = new MessageWindow("Некорректный формат длительности полета. Используйте формат hh:mm.");
                await messageWindow.ShowDialog(this);
                return;
            }

            if (!int.TryParse(OstatokTextBox.Text, out var ostatok))
            {
                var messageWindow = new MessageWindow("Некорректный формат остатка билетов. Введите целое число.");
                await messageWindow.ShowDialog(this);
                return;
            }

            var newTicket = new Ticket
            {
                Title = TitleTextBox.Text,
                Create_Date = DateTime.Now,
                City_From = CityFromTextBox.Text,
                City_To = CityToTextBox.Text,
                Date_Ulet = dateUlet,
                Date_Prilet = datePrilet,
                Time_Fly = timeFly,
                ostatok = ostatok,
                Description = DescriptionTextBox.Text
            };

            await using var db = new ApplicationDbContext();
            
            db.Tickets.Add(newTicket);
            await db.SaveChangesAsync();

            var messageSuccess = new MessageWindow("Билет успешно добавлен");
            await messageSuccess.ShowDialog(this);
            Close(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}