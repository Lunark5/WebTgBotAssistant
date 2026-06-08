using Microsoft.EntityFrameworkCore;
using Serilog;
using WebTgBotAssistant.Entities;
using WebTgBotAssistant.Models;

namespace WebTgBotAssistant;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LeaveMemberReaction> LeaveMemberReactions => Set<LeaveMemberReaction>();
    public DbSet<NewMemberReaction> NewMemberReactions => Set<NewMemberReaction>();
    public DbSet<ChannelMessageReaction> ChannelMessageReactions => Set<ChannelMessageReaction>();
    public DbSet<TextMemberReaction> TextMemberReactions => Set<TextMemberReaction>();

    public DbSet<TextVariable> TextVariables => Set<TextVariable>();

    public async Task AddAsync(MemberReactionType memberReactionType, MemberReaction memberReaction)
    {
        if (IsExists(memberReactionType, memberReaction.Key))
        {
            Log.Warning($"{memberReaction.Key} уже существует");

            return;
        }

        switch (memberReactionType)
        {
            case MemberReactionType.LeaveMember:
            {
                await LeaveMemberReactions.AddAsync(new LeaveMemberReaction(memberReaction));

                break;
            }
            case MemberReactionType.NewMember:
            {
                await NewMemberReactions.AddAsync(new NewMemberReaction(memberReaction));

                break;
            }
            case MemberReactionType.ChannelMessage:
            {
                await ChannelMessageReactions.AddAsync(new ChannelMessageReaction(memberReaction));

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(memberReactionType), memberReactionType, null);
        }

        await SaveChangesAsync();
    }

    public async Task RemoveAsync(MemberReactionType memberReactionType, string key)
    {
        if (!IsExists(memberReactionType, key))
        {
            Log.Warning($"{key} не найден");

            return;
        }

        switch (memberReactionType)
        {
            case MemberReactionType.LeaveMember:
            {
                var entity = LeaveMemberReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    LeaveMemberReactions.Remove(entity);
                }

                break;
            }
            case MemberReactionType.NewMember:
            {
                var entity = NewMemberReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    NewMemberReactions.Remove(entity);
                }

                break;
            }
            case MemberReactionType.ChannelMessage:
            {
                var entity = ChannelMessageReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    ChannelMessageReactions.Remove(entity);
                }

                break;
            }
            case MemberReactionType.TextMember:
            {
                var entity = TextMemberReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    TextMemberReactions.Remove(entity);
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(memberReactionType), memberReactionType, null);
        }

        await SaveChangesAsync();
    }

    public async Task AddTextReactionAsync(TextMemberReaction textMemberReaction)
    {
        if (IsExists(MemberReactionType.TextMember, textMemberReaction.Key))
        {
            Log.Warning($"{textMemberReaction.Key} уже существует");

            return;
        }

        await TextMemberReactions.AddAsync(textMemberReaction);
        await SaveChangesAsync();
    }

    public async Task AddTextVariableAsync(TextVariable textVariable)
    {
        TextVariables.Add(textVariable);

        await SaveChangesAsync();
    }

    public async Task RemoveTextVariableAsync(string id)
    {
        var entity = TextVariables.FirstOrDefault(x => x.Id == Guid.Parse(id));

        if (entity == null) return;

        TextVariables.Remove(entity);

        await SaveChangesAsync();
    }

    public bool IsExists(MemberReactionType memberReactionType, string key)
    {
        switch (memberReactionType)
        {
            case MemberReactionType.LeaveMember:
            {
                var entity = LeaveMemberReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    return true;
                }

                break;
            }
            case MemberReactionType.NewMember:
            {
                var entity = NewMemberReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    return true;
                }

                break;
            }
            case MemberReactionType.ChannelMessage:
            {
                var entity = ChannelMessageReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    return true;
                }

                break;
            }
            case MemberReactionType.TextMember:
            {
                var entity = TextMemberReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    return true;
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(memberReactionType), memberReactionType, null);
        }

        return false;
    }
}