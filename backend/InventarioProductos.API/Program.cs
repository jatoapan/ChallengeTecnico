using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using InventarioProductos.API.Common;
using InventarioProductos.API.Data;
using Microsoft.EntityFrameworkCore;
using InventarioProductos.API.Services;
using Microsoft.OpenApi;
using Serilog;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((contexto, configuracion) =>
    configuracion.ReadFrom.Configuration(contexto.Configuration));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inventario de Productos API",
        Version = "v1",
        Description = "CRUD de productos con ofertas por proveedor y lote. Autenticación con JWT."
    });

    // Comentarios /// de los controladores y DTOs
    var archivoXml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, archivoXml));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega el token que devuelve el login."
    });

    options.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = new List<string>()
    });
});

builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PermitirAngular", politica =>
    {
        var origenes = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? Array.Empty<string>();
        politica.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddDbContext<InventarioContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IResumenService, ResumenService>();
builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// Singleton: la cuenta de intentos debe sobrevivir entre peticiones.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IRegistroIntentosLogin, RegistroIntentosLoginMemoria>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

// Errores globales: el manejador traduce cada excepción a ProblemDetails (400, 404, 429 o 500).
builder.Services.AddExceptionHandler<ManejadorErroresGlobal>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Emisor"],
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Clave"]!)),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async contexto =>
            {
                contexto.HandleResponse();
                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await contexto.Response.WriteAsJsonAsync(ProblemaHttp.Crear(contexto.HttpContext, 401,
                    "No autenticado", "Debes iniciar sesión para acceder a este recurso."));
            },
            OnForbidden = async contexto =>
            {
                contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                await contexto.Response.WriteAsJsonAsync(ProblemaHttp.Crear(contexto.HttpContext, 403,
                    "Acceso denegado", "No tienes permisos para realizar esta acción."));
            }
        };
    });

// Políticas por acción: los controladores piden una acción, aquí se decide qué roles la tienen.
builder.Services.AddAuthorization(opciones =>
{
    // Denegar por defecto: todo endpoint exige sesión salvo los marcados con [AllowAnonymous].
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    opciones.AddPolicy(Politicas.GestionarProductos, politica =>
        politica.RequireRole("Administrador"));

    opciones.AddPolicy(Politicas.AjustarInventario, politica =>
        politica.RequireRole("Operador", "Administrador"));

    opciones.AddPolicy(Politicas.VerAuditoria, politica =>
        politica.RequireRole("Administrador"));

    opciones.AddPolicy(Politicas.GestionarProveedores, politica =>
        politica.RequireRole("Administrador"));

    opciones.AddPolicy(Politicas.GestionarUsuarios, politica =>
        politica.RequireRole("Administrador"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InventarioContext>();
    await SembradoUsuarios.EjecutarAsync(context);
}

app.UseSerilogRequestLogging();

// Middleware oficial de errores: llama a ManejadorErroresGlobal cuando algo falla más adelante en el pipeline.
app.UseExceptionHandler();

app.UseCors("PermitirAngular");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
