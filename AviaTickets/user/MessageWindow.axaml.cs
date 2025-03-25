using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Mysqlx.Session;

namespace AviaTickets;

public partial class MessageWindow : Window
{
    public MessageWindow()
    {
        InitializeComponent();
    }

    public MessageWindow(string message) : this()
    {
        MessageText.Text = message;
    }

    private void OnOkButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}