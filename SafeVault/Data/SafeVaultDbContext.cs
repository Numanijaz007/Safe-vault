using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SafeVault.Models;

namespace SafeVault.Data
{
    public class SafeVaultDbContext : IdentityDbContext<VaultUser>
    {
        public SafeVaultDbContext(DbContextOptions<SafeVaultDbContext> options) : base(options) { }

        public DbSet<VaultItem> VaultItems { get; set; } = null!;
    }
}
