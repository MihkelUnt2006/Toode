using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public class ToodedContext : DbContext
{
    public DbSet<Toode> Toodetabel => Set<Toode>();
    public DbSet<Kategooria> Kategooriatabel => Set<Kategooria>();
    public DbSet<Kasutaja> Kasutajad => Set<Kasutaja>();
    public DbSet<Ost> Ostud => Set<Ost>();
    public DbSet<OstuRida> OstuRead => Set<OstuRida>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={Path.Combine(Kaustad.Juur, "tooded.db")}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kategooria>().ToTable("Kategooriatabel");
        modelBuilder.Entity<Toode>().ToTable("Toodetabel");
        modelBuilder.Entity<Ost>().ToTable("Ostud");
        modelBuilder.Entity<OstuRida>().ToTable("OstuRead");

        // Ostu read kustuvad koos ostuga
        modelBuilder.Entity<Ost>()
            .HasMany(o => o.Read).WithOne(r => r.Ost)
            .HasForeignKey(r => r.OstId).OnDelete(DeleteBehavior.Cascade);

        // Toote kustutamisel jääb ostuajalugu alles (ToodeId muutub NULL-iks)
        modelBuilder.Entity<OstuRida>()
            .HasOne(r => r.Toode).WithMany()
            .HasForeignKey(r => r.ToodeId).OnDelete(DeleteBehavior.SetNull);

        // Kasutajat, kellel on oste, ei saa kustutada
        modelBuilder.Entity<Ost>()
            .HasOne(o => o.Kasutaja).WithMany()
            .HasForeignKey(o => o.KasutajaId).OnDelete(DeleteBehavior.Restrict);

        // Algandmed: ilma kategooriateta ei saa toodet lisada
        modelBuilder.Entity<Kategooria>().HasData(
            new Kategooria { Id = 1, Kategooria_nimetus = "Joogid" },
            new Kategooria { Id = 2, Kategooria_nimetus = "Puu- ja köögiviljad" },
            new Kategooria { Id = 3, Kategooria_nimetus = "Maiustused" });
    }
}
