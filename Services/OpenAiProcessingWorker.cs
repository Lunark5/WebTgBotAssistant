using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace WebTgBotAssistant.Models;

public class OpenAiProcessingWorker(
    Channel<Message> appealChannel,
    IOptions<AppOptions> appOptions,
    ITelegramBotClient botClient,
    OpenAiClient openAiClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in appealChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await SendOpenAiMessage(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при обработке обращения из очереди: {ExMessage}", ex.Message);
            }
        }
    }

    private async Task SendOpenAiMessage(Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            return;
        }

        var openAiSettings = appOptions.Value.OpenAiSettings ??
                             throw new ArgumentNullException("appOptions.Value.OpenAiSettings");

        if (!openAiSettings.IsEnabled())
        {
            return;
        }

        var openAiText = await openAiClient.SendMessage(message.Text);

        if (string.IsNullOrEmpty(openAiText)) return;

        await botClient.SendMessage(message.Chat.Id, openAiText, ParseMode.None, new ReplyParameters
        {
            MessageId = message.MessageId
        });
    }
}