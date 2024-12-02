using Microsoft.EntityFrameworkCore;
using ServerCentral.Models;

namespace ServerCentral.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MonitoringData> MonitoringData { get; set; }
    }
}
