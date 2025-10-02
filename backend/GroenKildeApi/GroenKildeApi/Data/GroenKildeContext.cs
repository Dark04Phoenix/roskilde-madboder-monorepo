using Microsoft.EntityFrameworkCore;
using GroenKildeApi.Models;

namespace GroenKildeApi.Data
{
    public class GroenKildeContext : DbContext
    {
        public GroenKildeContext(DbContextOptions<GroenKildeContext> options) : base(options) { }

        public DbSet<FoodBooth> FoodBooths { get; set; }
    }
}
