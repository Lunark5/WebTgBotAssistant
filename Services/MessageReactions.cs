using System.Text.RegularExpressions;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WebTgBotAssistant.Models;
using WebTgBotAssistant.Services;

namespace WebTgBotAssistant;

public class MessageReactions(
    IOptions<AppOptions> appOptions,
    ITelegramBotClient botClient,
    TriggerCache triggerCache,
    BotInfoContainer botInfo,
    Channel<Message> appealChannel)
{
    private static readonly Regex VariableRegex = new(@"\{\{(.+?)\}\}", RegexOptions.Compiled);

    public async Task ReactToMessage(Message message)
    {
        Log.Information("Новое сообщение: {FromUsername}:{MessageText}", message.From?.Username, message.Text);

        try
        {
            await ReactToNewUser(message);
            await ReactToLeaveUser(message);
            await ReactToChannelMessage(message);
            await ReactToTriggers(message);

            _ = Task.Run(async () =>
            {
                try
                {
                    await ReactToAppeal(message);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Произошла ошибка при обращении к ИИ: {ExMessage}", ex.Message);
                }
            });
        }
        catch (Exception ex)
        {
            Log.Warning("Произошла ошибка в обработке сообщения: {ExMessage}", ex.Message);
        }
    }

    private async Task ReactToNewUser(Message message)
    {
        if (message.NewChatMembers == null || message.NewChatMembers.Length == 0)
        {
            return;
        }

        if (!triggerCache.NewMemberReactions.Any())
        {
            return;
        }

        var randomIndex = Random.Shared.Next(triggerCache.NewMemberReactions.Count);
        var randomMemberReaction = triggerCache.NewMemberReactions[randomIndex];

        if (randomMemberReaction.IsEmptyReaction())
        {
            return;
        }

        if (!string.IsNullOrEmpty(randomMemberReaction.StickerId))
            await ReactWithSticker(randomMemberReaction.StickerId, message.Chat.Id, message.MessageId);

        if (!string.IsNullOrEmpty(randomMemberReaction.Text))
            await ReactWithText(randomMemberReaction.Text, message.Chat.Id, message.MessageId);
    }

    private async Task ReactToLeaveUser(Message message)
    {
        if (message.LeftChatMember == null)
        {
            return;
        }

        if (!triggerCache.LeaveMemberReactions.Any())
        {
            return;
        }

        var randomIndex = Random.Shared.Next(triggerCache.LeaveMemberReactions.Count);
        var randomMemberReaction = triggerCache.LeaveMemberReactions[randomIndex];

        if (randomMemberReaction.IsEmptyReaction())
        {
            return;
        }

        if (!string.IsNullOrEmpty(randomMemberReaction.StickerId))
            await ReactWithSticker(randomMemberReaction.StickerId, message.Chat.Id, message.MessageId);

        if (!string.IsNullOrEmpty(randomMemberReaction.Text))
            await ReactWithText(randomMemberReaction.Text, message.Chat.Id, message.MessageId);
    }

    private async Task ReactToChannelMessage(Message message)
    {
        if (string.IsNullOrEmpty(appOptions.Value.ChannelReplyUserId))
        {
            return;
        }

        if (!triggerCache.ChannelMessageReactions.Any())
        {
            return;
        }

        var randomIndex = Random.Shared.Next(triggerCache.ChannelMessageReactions.Count);
        var randomMemberReaction = triggerCache.ChannelMessageReactions[randomIndex];

        if (randomMemberReaction.IsEmptyReaction())
        {
            return;
        }

        var replyUserId = long.TryParse(randomMemberReaction.ReplyUserId, out var userId)
            ? userId
            : 0;

        if (message.From != null && message.From.Id != replyUserId)
        {
            return;
        }

        if (!string.IsNullOrEmpty(randomMemberReaction.StickerId))
            await ReactWithSticker(randomMemberReaction.StickerId, message.Chat.Id, message.MessageId);

        if (!string.IsNullOrEmpty(randomMemberReaction.Text))
            await ReactWithText(randomMemberReaction.Text, message.Chat.Id, message.MessageId,
                randomMemberReaction.ReplyMarkupText, randomMemberReaction.ReplyMarkupUri);
    }

    private async Task ReactToTriggers(Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            return;
        }

        if (string.IsNullOrEmpty(message.Text)) return;

        var reactions = triggerCache.TextMemberReactions;
        var text = message.Text;

        foreach (var reaction in reactions)
        {
            if (string.IsNullOrEmpty(reaction.CompareText)) continue;

            var isMatch = reaction.CompareType switch
            {
                CompareType.Contains => text.Contains(reaction.CompareText, StringComparison.OrdinalIgnoreCase),
                CompareType.StartsWith => text.StartsWith(reaction.CompareText, StringComparison.OrdinalIgnoreCase),
                CompareType.EndsWith => text.EndsWith(reaction.CompareText, StringComparison.OrdinalIgnoreCase),
                _ => false
            };

            if (!isMatch) continue;

            if (!string.IsNullOrEmpty(reaction.StickerId))
                await ReactWithSticker(reaction.StickerId, message.Chat.Id, message.MessageId);

            if (!string.IsNullOrEmpty(reaction.Text))
                await ReactWithText(reaction.Text, message.Chat.Id, message.MessageId);
        }
    }

    private async Task ReactToAppeal(Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            return;
        }

        if (botInfo.Info.Username == null)
        {
            return;
        }

        var botNames = new List<string>(appOptions.Value.BotAliases) { botInfo.Info.Username, botInfo.Info.FirstName };

        var isBotMentioned = botNames.Any(name => message.Text.Contains(name, StringComparison.OrdinalIgnoreCase));
        var isReplyToBot = message.ReplyToMessage != null && message.ReplyToMessage?.From?.Id == botInfo.Info.Id;

        if (!isBotMentioned && !isReplyToBot)
        {
            return;
        }

        await appealChannel.Writer.WriteAsync(message);
    }

    private async Task ReactWithSticker(string stickerId, long chatId, int messageId)
    {
        if (string.IsNullOrEmpty(stickerId))
        {
            return;
        }

        await botClient.SendSticker(chatId, stickerId, new()
        {
            MessageId = messageId
        });
    }

    private async Task ReactWithText(string text, long chatId, int messageId,
        string replyMarkupText = "", string replyMarkupUri = "")
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (string.IsNullOrEmpty(replyMarkupText) || string.IsNullOrEmpty(replyMarkupUri))
        {
            await botClient.SendMessage(chatId, ReplaceVars(text), ParseMode.None, new()
            {
                MessageId = messageId
            });
        }
        else
        {
            await botClient.SendMessage(chatId, ReplaceVars(text), ParseMode.None, new()
            {
                MessageId = messageId
            }, new InlineKeyboardMarkup(
                [
                    [
                        InlineKeyboardButton.WithUrl(
                            text: replyMarkupText,
                            url: replyMarkupUri)
                    ]
                ]
            ));
        }
    }

    private string ReplaceVars(string templateText)
    {
        if (string.IsNullOrEmpty(templateText) || !triggerCache.TextVariables.Any()) return templateText;

        return VariableRegex.Replace(templateText, match =>
        {
            var varName = match.Groups[1].Value.Trim().ToLower();

            var customVariables = triggerCache.TextVariables
                .Where(variable => variable.Group.Equals(varName, StringComparison.OrdinalIgnoreCase));

            if (!customVariables.Any()) return templateText;

            var randomIndex = Random.Shared.Next(triggerCache.TextVariables.Count);
            var randomMemberReaction = triggerCache.TextVariables[randomIndex];

            return !string.IsNullOrEmpty(randomMemberReaction.Value)
                ? randomMemberReaction.Value
                : templateText;
        });
    }
}