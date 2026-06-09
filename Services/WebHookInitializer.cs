using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using WebTgBotAssistant.Models;

namespace WebTgBotAssistant;

public class WebHookInitializer(IOptions<AppOptions> options)
{
    public async Task Initialize(ITelegramBotClient botClient)
    {
        var endpointUrl = $"{options.Value.WebhookUrl}{ApplicationConstants.WebhookEndpoint}";
        var publicCertPath = Path.Combine(AppContext.BaseDirectory, "public.pem");

        if (File.Exists(publicCertPath))
        {
            await using var stream = File.OpenRead(publicCertPath);

            var telegramCert = new InputFileStream(stream, "public.pem");

            await botClient.SetWebhook(
                url: endpointUrl,
                certificate: telegramCert,
                dropPendingUpdates: true
            );

            return;
        }

        await botClient.SetWebhook(endpointUrl);
    }
}