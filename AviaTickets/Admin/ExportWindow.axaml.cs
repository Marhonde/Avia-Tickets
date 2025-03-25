using System;
using System.Linq;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace AviaTickets;

public partial class ExportWindow : Window
{
    public ExportWindow()
    {
        InitializeComponent();
    }

    private async void OnExportAllTicketsClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ExportTicketsAsync("Все билеты", includeSold: true, includeUnsold: true);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
    
    private async void OnExportUnsoldTicketsClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ExportTicketsAsync("Непроданные билеты", includeSold: false, includeUnsold: true);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
    
    private async void OnExportSoldTicketsClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ExportTicketsAsync("Проданные билеты", includeSold: true, includeUnsold: false);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }

    private async Task ExportTicketsAsync(string fileNamePrefix, bool includeSold, bool includeUnsold)
    {
        try
        {
            await using var db = new ApplicationDbContext();

            var tickets = await db.Tickets.ToListAsync();
            var purchasedTickets = await db.Purchased_Tickets.Include(pt => pt.Ticket).ToListAsync();

            using var workbook = new XLWorkbook();
            
            var worksheet = workbook.Worksheets.Add("Билеты");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Название";
            worksheet.Cell(1, 3).Value = "Город отправления";
            worksheet.Cell(1, 4).Value = "Город назначения";
            worksheet.Cell(1, 5).Value = "Дата вылета";
            worksheet.Cell(1, 6).Value = "Дата прилета";
            worksheet.Cell(1, 7).Value = "Остаток";
            worksheet.Cell(1, 8).Value = "Количество проданных";
            worksheet.Cell(1, 9).Value = "Статус";

            var row = 2;

            foreach (var ticket in tickets)
            {
                var isSold = purchasedTickets.Any(pt => pt.TicketId == ticket.Id);
                var soldCount = purchasedTickets.Count(pt => pt.TicketId == ticket.Id);

                if ((!includeSold || !isSold) && (!includeUnsold || isSold))
                    continue;
                
                worksheet.Cell(row, 1).Value = ticket.Id;
                worksheet.Cell(row, 2).Value = ticket.Title;
                worksheet.Cell(row, 3).Value = ticket.City_From;
                worksheet.Cell(row, 4).Value = ticket.City_To;
                worksheet.Cell(row, 5).Value = ticket.Date_Ulet.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cell(row, 6).Value = ticket.Date_Prilet.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cell(row, 7).Value = ticket.ostatok;
                worksheet.Cell(row, 8).Value = soldCount;
                worksheet.Cell(row, 9).Value = isSold ? "Продан" : "Не продан";
                row++;
            }
            
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filters.Add(new FileDialogFilter { Name = "Excel Files", Extensions = { "xlsx" } });
            saveFileDialog.InitialFileName = $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            var result = await saveFileDialog.ShowAsync(this);
            if (result != null)
                workbook.SaveAs(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}