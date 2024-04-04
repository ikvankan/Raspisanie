namespace Raspisanie.Data
{
    public class ApplicationDBContext
    {
        public class ApplicationDbContext : IdentityDbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
            {

            }
            public DbSet<ItemType> ItemType { get; set; }
            public DbSet<Punkt> Punkt { get; set; }
            public DbSet<Item> Item { get; set; }
            public DbSet<ApplicationUser> ApplicationUser { get; set; }
            public DbSet<OrderHeader> OrderHeader { get; set; }
            public DbSet<OrderDetail> OrderDetail { get; set; }
        }
    }
}
