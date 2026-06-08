using Telegram.Bot;
using Telegram.Bot.Types;

namespace WebTgBotAssistant;

public class BotInfoContainer
{
    public User Info { get; private set; } = null!;

    public void Initialize(User info)
    {
        Info = info ?? throw new ArgumentNullException(nameof(info));
    }
}