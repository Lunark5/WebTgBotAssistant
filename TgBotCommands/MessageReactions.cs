using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WebTgBotAssistant.Models;

namespace WebTgBotAssistant;

public class MessageReactions(
    IOptions<AppOptions> appOptions,
    ITelegramBotClient botClient,
    AppDbContext appDbContext,
    OpenAiClient openAiClient)
{
    private long _id;
    private string _username;
    private string _first_name;

    public async Task InitBotInfo()
    {
        var me = await botClient.GetMe();

        _id = me.Id;
        _username = me.Username;
        _first_name = me.FirstName;
    }

    public async Task ReactToNewUser(Message message)
    {
        if (message.NewChatMembers == null || message.NewChatMembers.Length == 0)
        {
            return;
        }

        var memberReaction = await appDbContext.NewMemberReactions
            .OrderBy(r => EF.Functions.Random())
            .FirstOrDefaultAsync();

        if (memberReaction == null)
        {
            return;
        }

        if (memberReaction.IsEmptyReaction())
        {
            return;
        }

        if (!string.IsNullOrEmpty(memberReaction?.StickerId))
        {
            await botClient.SendSticker(message.Chat.Id, memberReaction.StickerId, new()
            {
                MessageId = message.MessageId
            });
        }

        if (!string.IsNullOrEmpty(memberReaction?.Text))
        {
            await botClient.SendMessage(message.Chat.Id, memberReaction.Text, ParseMode.None, new()
            {
                MessageId = message.MessageId
            });
        }
    }

    public async Task ReactToLeaveUser(Message message)
    {
        if (message.LeftChatMember == null)
        {
            return;
        }

        var memberReaction = await appDbContext.LeaveMemberReactions
            .OrderBy(r => EF.Functions.Random())
            .FirstOrDefaultAsync();

        if (memberReaction == null)
        {
            return;
        }

        if (memberReaction.IsEmptyReaction())
        {
            return;
        }

        if (!string.IsNullOrEmpty(memberReaction?.StickerId))
        {
            await botClient.SendSticker(message.Chat.Id, memberReaction.StickerId, new()
            {
                MessageId = message.MessageId
            });
        }

        if (!string.IsNullOrEmpty(memberReaction?.Text))
        {
            await botClient.SendMessage(message.Chat.Id, memberReaction.Text, ParseMode.None, new()
            {
                MessageId = message.MessageId
            });
        }
    }

    public async Task ReactToChannelMessage(Message message)
    {
        if (string.IsNullOrEmpty(appOptions.Value.ChannelReplyUserId))
        {
            return;
        }

        var memberReaction = await appDbContext.ChannelMessageReactions
            .OrderBy(r => EF.Functions.Random())
            .FirstOrDefaultAsync();

        if (memberReaction == null)
        {
            return;
        }

        if (memberReaction.IsEmptyReaction())
        {
            return;
        }

        var replyUserId = long.TryParse(memberReaction.ReplyUserId, out var userId)
            ? userId
            : 0;

        if (message.From != null && message.From.Id != replyUserId)
        {
            return;
        }

        if (!string.IsNullOrEmpty(memberReaction.StickerId))
        {
            await botClient.SendSticker(message.Chat.Id, memberReaction.StickerId, new()
            {
                MessageId = message.MessageId
            });
        }

        if (string.IsNullOrEmpty(memberReaction.Text)) return;

        if (string.IsNullOrEmpty(memberReaction.ReplyMarkupText))
        {
            await botClient.SendMessage(message.Chat.Id, memberReaction.Text, ParseMode.None, new ReplyParameters
            {
                MessageId = message.MessageId,
            });
        }
        else
        {
            if (memberReaction.ReplyMarkupUri != null)
            {
                var inlineKeyboardMarkup = new InlineKeyboardMarkup(
                    [
                        [
                            InlineKeyboardButton.WithUrl(
                                text: memberReaction.ReplyMarkupText,
                                url: memberReaction.ReplyMarkupUri)
                        ]
                    ]
                );

                await botClient.SendMessage(message.Chat.Id, memberReaction.Text, ParseMode.None, new ReplyParameters
                {
                    MessageId = message.MessageId,
                }, inlineKeyboardMarkup);
            }
        }
    }

    public async Task ReactToAppeal(Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            return;
        }

        if (string.IsNullOrEmpty(_username))
        {
            await InitBotInfo();
        }

        var botNames = new List<string>(appOptions.Value.BotAliases) { _username, _first_name };

        if (!botNames.Any(name => message.Text.Contains(name, StringComparison.OrdinalIgnoreCase))
            && message.ReplyToMessage == null
            && message.ReplyToMessage?.From?.Id != _id)
        {
            return;
        }

        SendOpenAiMessage(message);
    }

    private async Task SendOpenAiMessage(Message message)
    {
        try
        {
            var openAiSettings = appOptions.Value.OpenAiSettings;

            if (!openAiSettings.IsEnabled())
            {
                return;
            }

            var openAiText = await openAiClient.SendMessage(message.Text);

            if (!string.IsNullOrEmpty(openAiText))
            {
                await botClient.SendMessage(message.Chat.Id, openAiText, ParseMode.None, new()
                {
                    MessageId = message.MessageId
                });
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Произошла ошибка: {ex.Message}");
        }
    }
}