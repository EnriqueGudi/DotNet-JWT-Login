using Microsoft.EntityFrameworkCore;
using peluqueria.Models;

namespace peluqueria.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UsersModel> Usuarios { get; set; }
    }
}
