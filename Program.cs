using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using Telegram.Bot;
using WebTgBotAssistant;
using WebTgBotAssistant.Models;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Host.UseSerilog();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.Configure<AppOptions>(
    builder.Configuration.GetSection("AppOptions")
);
builder.Services.AddHttpClient("TgBot")
    .AddTypedClient<ITelegramBotClient>((httpClient, serviceProvider) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<AppOptions>>().Value;

        var token = options.TgToken;

        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("AppOptions.TgToken не найден");
        }

        return new TelegramBotClient(token, httpClient);
    });

builder.Services.AddTransient<MessageReactions>();
builder.Services.AddTransient<WebHookInitializer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    context.Database.EnsureCreated();
}

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<WebHookInitializer>();
    var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

    await initializer.Initialize(botClient);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); 
}

app.UseHttpsRedirection();
app.MapPostEndpoints();

Log.Information("Приложение успешно запущено!");

app.Run();