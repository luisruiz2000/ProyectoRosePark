using Microsoft.EntityFrameworkCore;
using RosePark.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext service
builder.Services.AddDbContext<RoseParkDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Distributed Memory Cache and Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración de la sesión
    options.Cookie.HttpOnly = true; // Mejora la seguridad
    options.Cookie.IsEssential = true; // Hace que la cookie sea esencial
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar el uso de sesiones
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

/* 
dotnet aspnet-codegenerator controller -name HabitacionController -m Habitacione -dc NombreDelDbContext --relativeFolderPath Controladores --useDefaultLayout --referenceScriptLibraries
 */