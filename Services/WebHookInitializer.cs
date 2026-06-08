using Microsoft.Extensions.Options;
using Telegram.Bot;
using WebTgBotAssistant.Models;

namespace WebTgBotAssistant;

public class WebHookInitializer(IOptions<AppOptions> options)
{
    public async Task Initialize(ITelegramBotClient botClient)
    {
        var endpointUrl = $"{options.Value.WebhookUrl}{ApplicationConstants.WebhookEndpoint}";

        await botClient.SetWebhook(endpointUrl);
    }
}