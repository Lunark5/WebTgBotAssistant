using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Types;
using WebTgBotAssistant;
using WebTgBotAssistant.Models;
using WebTgBotAssistant.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error || e.Level == LogEventLevel.Fatal)
        .WriteTo.File("Logs/app-.txt", rollingInterval: RollingInterval.Day))
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
builder.Services.AddHostedService<OpenAiProcessingWorker>();
builder.Services.AddTransient<WebHookInitializer>();
builder.Services.AddScoped<MessageReactions>();

builder.Services.AddSingleton<BotInfoContainer>();
builder.Services.AddSingleton<OpenAiClient>();
builder.Services.AddSingleton<TriggerCache>();
builder.Services.AddSingleton(Channel.CreateBounded<Message>(new BoundedChannelOptions(1000)
{
    FullMode = BoundedChannelFullMode.Wait
}));

var app = builder.Build();

using var scope = app.Services.CreateScope();

var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var options = scope.ServiceProvider.GetRequiredService<AppOptions>();

context.Database.EnsureCreated();

var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
var initializer = scope.ServiceProvider.GetRequiredService<WebHookInitializer>();
var botInfo = scope.ServiceProvider.GetRequiredService<BotInfoContainer>();
var triggerCache = scope.ServiceProvider.GetRequiredService<TriggerCache>();

await initializer.Initialize(botClient);
await triggerCache.RefreshAsync();

var user = await botClient.GetMe();

botInfo.Initialize(user);

app.MapOpenApi();

if (options.EnableSwagger)
{
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapPostEndpoints();

Log.Information("Приложение успешно запущено!");

app.Run();