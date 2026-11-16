using InventarioProductos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Data;

public static class SembradoUsuarios
{
    public static async Task EjecutarAsync(InventarioContext context)
    {
        if (await context.Usuarios.AnyAsync())
            return;

        // Los roles los crea createDB.sql; aquí solo se buscan por nombre.
        var roles = await context.Roles.ToDictionaryAsync(r => r.Nombre, r => r.Id);

        context.Usuarios.AddRange(
            new Usuario
            {
                Nombres = "Administrador",
                Apellidos = "Sistema",
                NombreUsuario = "admin",
                Email = "admin@inventario.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*"),
                IdRol = roles["Administrador"],
                Estado = true
            },
            new Usuario
            {
                Nombres = "Operador",
                Apellidos = "Sistema",
                NombreUsuario = "operador",
                Email = "operador@inventario.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operador123*"),
                IdRol = roles["Operador"],
                Estado = true
            });

        await context.SaveChangesAsync();
    }
}