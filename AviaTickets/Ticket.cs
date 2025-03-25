using System;

namespace AviaTickets;

public class Ticket
{
    public int Id { get; set; }
    
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public DateTime Create_Date { get; set; }
    
    public DateTime Date_Ulet { get; set; }
    
    public DateTime Date_Prilet { get; set; }
    
    public string City_From { get; set; }
    
    public string City_To { get; set; }
    
    public TimeSpan Time_Fly { get; set; }
    
    public int ostatok { get; set; }
}