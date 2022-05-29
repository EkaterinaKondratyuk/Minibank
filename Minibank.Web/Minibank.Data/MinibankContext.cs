using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Minibank.Data.Domain.Transactions;
using Minibank.Data.Domain.Users;
using Minibank.Data.Domains.BankAccounts;

namespace Minibank.Data
{
    public class MinibankContext : DbContext
    {
        public DbSet<UserDbModel> Users { get; set; }
        public DbSet<BankAccountDbModel> BankAccounts { get; set; }
        public DbSet<TransactionDbModel> Transactions { get; set; }

        public MinibankContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MinibankContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSnakeCaseNamingConvention()
                .UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }
    }

    public class Factory : IDesignTimeDbContextFactory<MinibankContext>
    {
        public Factory()
        {
        }

        public MinibankContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<MinibankContext>()
                .UseNpgsql()
                .Options;

            return new MinibankContext(options);
        }
    }
}
