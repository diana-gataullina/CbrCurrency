using Microsoft.EntityFrameworkCore;

namespace CbrCurrency.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // уникальность валют по идентификатору ЦБ
        modelBuilder.Entity<Currency>()
            .HasIndex(c => c.CbrId)
            .IsUnique();

        // Одна и та же валюта не может иметь два разных курса на одну дату
        modelBuilder.Entity<ExchangeRate>()
            .HasIndex(e => new { e.CurrencyId, e.Date })
            .IsUnique();
            
        base.OnModelCreating(modelBuilder);
    }
}