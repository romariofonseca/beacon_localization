// Data/DatabaseContext.cs
using Microsoft.EntityFrameworkCore;

public class DatabaseContext : DbContext
{
    public DbSet<Device> Devices { get; set; }
    public DbSet<Gateways> Gateways { get; set; }
    public DbSet<Map> Map { get; set; }

    public DbSet<RegisteredDevice> RegisteredDevice { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapear a entidade 'Device' para a tabela 'dispositivos'
        modelBuilder.Entity<Device>().ToTable("dispositivos");
        modelBuilder.Entity<Gateways>().ToTable("gateways");
        modelBuilder.Entity<Map>().ToTable("mapas");
        modelBuilder.Entity<RegisteredDevice>().ToTable("dispositivos_cadastrados");

    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=C:\\Users\\Sup\\Desktop\\VirtualBorder\\dispositivos.db");


    }
}
