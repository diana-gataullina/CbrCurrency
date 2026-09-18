using CbrCurrency;
using CbrCurrency.Data;
using CbrCurrency.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Настройка БД
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка HttpClient и сервисов
builder.Services.AddHttpClient<ICbrClient, CbrClient>();
builder.Services.AddScoped<CurrencySyncService>();

// Регистрация фоновой задачи
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Автоматическое создание таблиц при старте
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

host.Run();