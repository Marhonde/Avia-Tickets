using System;
using Microsoft.EntityFrameworkCore;

namespace AviaTickets
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        
        public DbSet<Ticket> Tickets { get; set; }
        
        public DbSet<PurchasedTicket> Purchased_Tickets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            DotNetEnv.Env.Load();
            
            var server = Environment.GetEnvironmentVariable("SERVER");
            var db = Environment.GetEnvironmentVariable("DB");
            var user = Environment.GetEnvironmentVariable("USER");
            var password = Environment.GetEnvironmentVariable("PASSWORD");
            
            optionsBuilder.UseMySQL($"server={server};database={db};user={user};password={password}");
        }
    }
}