using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ExamenParcial.Data;
using ExamenParcial.Services;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
// --- servicios ---
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
// habilitar sesiones (puedes configurar timeout aquí)
builder.Services.AddDistributedMemoryCache(); // si no usas Redis aún
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configurar Redis
try
{
    var redisHost = builder.Configuration["Redis:Host"];
    var redisPort = builder.Configuration.GetValue<int>("Redis:Port");
    var redisUser = builder.Configuration["Redis:User"];
    var redisPassword = builder.Configuration["Redis:Password"];

    var configurationOptions = new ConfigurationOptions
    {
        EndPoints = { { redisHost!, redisPort } },
        User = redisUser,
        Password = redisPassword,
        AbortOnConnectFail = false, 
        ConnectTimeout = 5000
    };

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConfigurationOptions = configurationOptions;
    });

    builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
        ConnectionMultiplexer.Connect(configurationOptions));
}
catch (Exception ex)
{
    // Si hay error con Redis, usar cache en memoria como fallback
    builder.Services.AddMemoryCache();
    Console.WriteLine($"Redis no disponible, usando cache en memoria: {ex.Message}");
}

// Registrar configuración de Redis
builder.Services.Configure<RedisConfiguration>(builder.Configuration.GetSection("Redis"));

// Registrar servicios
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CursoCacheService>();
builder.Services.AddScoped<UltimoCursoService>();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ExamenParcial.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.IsEssential = true;
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseSession(); // <-- Agrega esta línea

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();