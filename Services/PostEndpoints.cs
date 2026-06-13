using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot.Types;
using WebTgBotAssistant.DTO;
using WebTgBotAssistant.Entities;
using WebTgBotAssistant.Models;
using WebTgBotAssistant.Services;

namespace WebTgBotAssistant;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(ApplicationConstants.WebhookEndpoint, async ([FromBody] Update update,
            IOptions<AppOptions> options,
            MessageReactions messageReactions) =>
        {
            try
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
                        await messageReactions.ReactToMessage(message);
                    }
                }
                else
                {
                    await messageReactions.ReactToMessage(message);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
            }

            return Results.Ok();
        });

        endpoints.MapPost(ApplicationConstants.AddReactionEndpoint,
            async ([FromBody] AddReactionDTO reactionDto,
                string password,
                IOptions<AppOptions> options,
                AppDbContext appDbContext) =>
            {
                if (password != options.Value.ApiPassword)
                {
                    return Results.Unauthorized();
                }
                
                if (string.IsNullOrEmpty(reactionDto.Reaction.Key))
                {
                    return Results.Ok("Ключ пуст");
                }

                if (appDbContext.IsExists(reactionDto.ReactionType, reactionDto.Reaction.Key))
                {
                    return Results.Ok($"Ключ {reactionDto.Reaction.Key} уже существует");
                }

                var newMemberReaction = new MemberReaction()
                {
                    Key = reactionDto.Reaction.Key,
                    StickerId = reactionDto.Reaction.StickerId,
                    ReplyMarkupText = reactionDto.Reaction.ReplyMarkupText,
                    ReplyMarkupUri = reactionDto.Reaction.ReplyMarkupUri,
                    Text = reactionDto.Reaction.Text,
                };

                if (reactionDto.ReactionType == MemberReactionType.ChannelMessage)
                {
                    newMemberReaction.ReplyUserId = options.Value.ChannelReplyUserId;
                }

                await appDbContext.AddAsync(reactionDto.ReactionType, newMemberReaction);

                return Results.Ok($"{reactionDto.Reaction.Key} успешно добавлен");
            });

        endpoints.MapPost(ApplicationConstants.RemoveReactionEndpoint,
            async ([FromBody] RemoveReactionDTO reactionDto,
                string password,
                IOptions<AppOptions> options,
                AppDbContext appDbContext) =>
            {
                if (password != options.Value.ApiPassword)
                {
                    return Results.Unauthorized();
                }
                
                if (string.IsNullOrEmpty(reactionDto.Key))
                {
                    Results.Ok("Ключ пуст");
                }

                if (!appDbContext.IsExists(reactionDto.ReactionType, reactionDto.Key))
                {
                    return Results.Ok($"Ключ {reactionDto.Key} не существует");
                }

                await appDbContext.RemoveAsync(reactionDto.ReactionType, reactionDto.Key);

                return Results.Ok($"{reactionDto.Key} успешно удален");
            });

        endpoints.MapPost(ApplicationConstants.AddTextMemberReaction,
            async ([FromBody] AddTextMemberReactionDTO reactionDto,
                string password,
                IOptions<AppOptions> options,
                AppDbContext appDbContext) =>
            {
                if (password != options.Value.ApiPassword)
                {
                    return Results.Unauthorized();
                }
                
                if (string.IsNullOrEmpty(reactionDto.Key))
                {
                    return Results.Ok("Ключ пуст");
                }

                if (appDbContext.IsExists(MemberReactionType.TextMember, reactionDto.Key))
                {
                    return Results.Ok($"Ключ {reactionDto.Key} уже существует");
                }

                var newMemberReaction = new TextMemberReaction()
                {
                    Key = reactionDto.Key,
                    StickerId = reactionDto.StickerId,
                    Text = reactionDto.Text,
                    CompareType = reactionDto.CompareType,
                    CompareText = reactionDto.CompareText,
                };

                await appDbContext.AddTextReactionAsync(newMemberReaction);

                return Results.Ok($"{reactionDto.Key} успешно добавлен");
            });

        endpoints.MapPost(ApplicationConstants.AddTextVariable,
            async ([FromBody] AddTextrVariableDTO textVarDto,
                string password,
                IOptions<AppOptions> options,
                AppDbContext appDbContext) =>
            {
                if (password != options.Value.ApiPassword)
                {
                    return Results.Unauthorized();
                }
                
                await appDbContext.AddTextVariableAsync(new TextVariable()
                {
                    Group = textVarDto.Group,
                    Value = textVarDto.Value,
                });

                return Results.Ok($"{textVarDto.Group}:{textVarDto.Value} успешно добавлен");
            });

        endpoints.MapPost(ApplicationConstants.RefreshDb,
            async (string password,
                IOptions<AppOptions> options,
                TriggerCache triggerCache) =>
            {
                if (password != options.Value.ApiPassword)
                {
                    return Results.Unauthorized();
                }
                
                await triggerCache.RefreshAsync();

                return Results.Ok($"Успех");
            });
    }
}