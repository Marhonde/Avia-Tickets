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
            try
            {
                DotNetEnv.Env.Load();
                
                var server = Environment.GetEnvironmentVariable("DB_SERVER");
                var db = Environment.GetEnvironmentVariable("DB_DATABASE");
                var user = Environment.GetEnvironmentVariable("DB_USERNAME");
                var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
                
                optionsBuilder
                    .EnableSensitiveDataLogging()
                    .UseMySQL($"server={server};database={db};user={user};password={password}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}