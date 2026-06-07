using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot.Types;
using WebTgBotAssistant.DTO;
using WebTgBotAssistant.Entities;
using WebTgBotAssistant.Models;

namespace WebTgBotAssistant;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(ApplicationConstants.WebhookEndpoint, async ([FromBody] Update update, 
            IOptions<AppOptions> options, 
            MessageReactions messageReactions) =>
            {
                if (update.Message == null)
                {
                    return Results.Ok();
                }

                var message = update.Message;

                if (options.Value.AllowedChatIds.Count != 0)
                {
                    if (options.Value.AllowedChatIds.Contains(message.Chat.Id))
                    {
                        await ReactToMessage(message, messageReactions);
                    }
                }
                else
                {
                    await ReactToMessage(message, messageReactions);
                }

                return Results.Ok();
            });

        endpoints.MapPost(ApplicationConstants.AddReactionEndpoint,
            async ([FromBody] AddReactionDTO reactionDTO,
                IOptions<AppOptions> options,
                AppDbContext appDbContext) =>
            {
                if (string.IsNullOrEmpty(reactionDTO.Reaction.Key))
                {
                    return Results.BadRequest("Ключ пуст");
                }

                if (appDbContext.IsExists(reactionDTO.ReactionType, reactionDTO.Reaction.Key))
                {
                    return Results.BadRequest($"Ключ {reactionDTO.Reaction.Key} уже существует");
                }

                var newMemberReaction = new MemberReaction()
                {
                    Key = reactionDTO.Reaction.Key,
                    StickerId = reactionDTO.Reaction.StickerId,
                    ReplyMarkupText = reactionDTO.Reaction.ReplyMarkupText,
                    ReplyMarkupUri = reactionDTO.Reaction.ReplyMarkupUri,
                    ReplyUserId = options.Value.ChannelReplyUserId,
                    Text = reactionDTO.Reaction.Text,
                };

                await appDbContext.AddAsync(reactionDTO.ReactionType, newMemberReaction);

                return Results.Ok($"{reactionDTO.Reaction.Key} успешно добавлен");
            });

        endpoints.MapPost(ApplicationConstants.RemoveReactionEndpoint,
            async ([FromBody] RemoveReactionDTO reactionDTO,
                AppDbContext appDbContext) =>
            {
                if (string.IsNullOrEmpty(reactionDTO.Key))
                {
                    Results.BadRequest("Ключ пуст");
                }

                if (!appDbContext.IsExists(reactionDTO.ReactionType, reactionDTO.Key))
                {
                    return Results.BadRequest($"Ключ {reactionDTO.Key} не существует");
                }

                await appDbContext.RemoveAsync(reactionDTO.ReactionType, reactionDTO.Key);

                return Results.Ok($"{reactionDTO.Key} успешно удален");
            });
    }

    private static async Task ReactToMessage(Message message, MessageReactions messageReactions)
    {
        Log.Information($"Новое сообщение: {message.From?.Username}:{message.Text}");

        try
        {
            await messageReactions.ReactToNewUser(message);
            await messageReactions.ReactToLeaveUser(message);
            await messageReactions.ReactToChannelMessage(message);
            await messageReactions.ReactToAppeal(message);
        }
        catch (Exception ex)
        {
            Log.Warning($"Произошла ошибка в обработке сообщения: {ex.Message}");
        }
    }
}