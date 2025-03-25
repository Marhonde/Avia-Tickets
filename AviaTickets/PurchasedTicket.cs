using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviaTickets;

public class PurchasedTicket
{
    public int Id { get; set; }
    
    [Column("id_ticket")]
    public int TicketId { get; set; }
    
    [Column("id_user")]
    public int UserId { get; set; }
    
    [Column("purchased_date")]
    public DateTime PurchasedDate { get; set; }
    
    public Ticket? Ticket { get; set; }
    
    public User? User { get; set; }
    
    [NotMapped]
    public string? Username { get; set; }
}