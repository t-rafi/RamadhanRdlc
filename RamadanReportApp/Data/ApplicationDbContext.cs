using Microsoft.EntityFrameworkCore;

namespace RamadanReportApp.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Models.RamadanSchedule> RamadanSchedule { get; set; }
    }
}
