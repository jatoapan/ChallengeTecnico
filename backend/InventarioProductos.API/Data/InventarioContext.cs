using InventarioProductos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Data;

public class InventarioContext : DbContext
{
    public InventarioContext(DbContextOptions<InventarioContext> options) : base(options) { }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<Inventario> Inventarios => Set<Inventario>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("Rol");
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.Property(x => x.NombreUsuario).HasColumnName("usuario");
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.Rol).WithMany(r => r.Usuarios).HasForeignKey(x => x.IdRol);
        });

        modelBuilder.Entity<Proveedor>(e =>
        {
            e.ToTable("Proveedor");
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("Producto");
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Lote>(e =>
        {
            e.ToTable("Lote");
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Producto).WithMany(p => p.Lotes).HasForeignKey(x => x.IdProducto);
            e.HasOne(x => x.Proveedor).WithMany(p => p.Lotes).HasForeignKey(x => x.IdProveedor);
        });

        modelBuilder.Entity<Inventario>(e =>
        {
            e.ToTable("Inventario");
            e.Property(x => x.Costo).HasPrecision(10, 2);
            e.Property(x => x.Precio).HasPrecision(10, 2);
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("GETDATE()");
            e.Property(x => x.FechaActualizacion).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Lote).WithOne(l => l.Inventario).HasForeignKey<Inventario>(x => x.IdLote);
        });

        modelBuilder.Entity<Auditoria>(e =>
        {
            e.ToTable("Auditoria");
            e.Property(x => x.Fecha).HasDefaultValueSql("GETDATE()");
        });
    }
}