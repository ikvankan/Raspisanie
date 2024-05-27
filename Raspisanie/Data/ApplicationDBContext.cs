using Microsoft.EntityFrameworkCore;
using Raspisanie.Models;

namespace Raspisanie.Data
{
    
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Auditoria> Auditoria { get; set; }
        public DbSet<Teacher> Teacher { get; set; }
        public DbSet<Predmet> Predmet { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<Placement> Placement { get; set; }
        public DbSet<TGUser> TGUser { get; set; }
    }
}
