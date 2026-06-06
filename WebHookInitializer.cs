using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using WebTgBotAssistant.Models;

namespace WebTgBotAssistant;

public class WebHookInitializer(IOptions<AppOptions> options)
{
    public async Task Initialize(ITelegramBotClient  botClient)
    {
        if (!await IsWebhookExists(botClient))
        {
            var endpointUrl = $"{options.Value.WebhookUrl}{ApplicationConstants.WebhookEndpoint}";

            await botClient.SetWebhook(endpointUrl);
        }
    }

    public async Task<bool> IsWebhookExists(ITelegramBotClient  botClient)
    {
        try
        {
            var info = await botClient.GetWebhookInfo();
            var infoWebhookUrl = info.Url.Replace(ApplicationConstants.WebhookEndpoint, string.Empty);

            if (info.LastErrorMessage != null) Log.Warning(info.LastErrorMessage);

            if (!string.IsNullOrEmpty(info.Url)
                && infoWebhookUrl == options.Value.WebhookUrl) return true;

            if (infoWebhookUrl != options.Value.WebhookUrl)
            {
                Log.Warning($"Current webhook url: {infoWebhookUrl}, app webhook url: {options.Value.WebhookUrl}");
            }

            await botClient.DeleteWebhook();

            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);

            return false;
        }
    }
}