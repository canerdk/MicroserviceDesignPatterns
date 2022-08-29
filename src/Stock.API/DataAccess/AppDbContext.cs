using Microsoft.EntityFrameworkCore;
using Stock.API.Models;

namespace Stock.API.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Models.Stock> Stocks { get; set; }
    }
}
