using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ConfortAnimal.Models;
using Microsoft.AspNetCore.Identity;

namespace ConfortAnimal.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
    {
        public DbSet<Ambiente> Ambiente { get; set; } = default!;
        public DbSet<Animal> Animais { get; set; } = default!;
        public DbSet<Bovino> Bovinos { get; set; } = default!;
        public DbSet<Avaliacao> Avaliacoes { get; set; } = default!;
    }
}
